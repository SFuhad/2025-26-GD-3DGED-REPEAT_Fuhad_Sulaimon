using System;
using GDEngine.Core.Components;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDGame.FacilityEscape
{
    // Built from scratch instead of the engine's FirstPersonCapsuleController +
    // MouseYawPitchController combo - that pair kept feeling inverted no matter how the two
    // were wired together, and it was hard to be sure why without being able to inspect the
    // engine's own forward/right convention live. This one computes forward/right directly
    // from a yaw angle I track myself, verified by hand against Vector3.Forward/Right at
    // yaw=0, so there's no ambiguity: W is forward, A is left, S is back, D is right, always.
    //
    // IMPORTANT: the camera is NOT parented to this body's Transform, on purpose. This body is
    // a Dynamic RigidBody, and Dynamic bodies get their Transform's rotation overwritten from
    // the physics simulation every single step - collisions/friction (bumping a wall, a crate,
    // a door) impart spin that has nothing to do with where the player is looking. If the
    // camera were a child, its rendered rotation is computed relative to THIS body's rotation,
    // so every physics nudge would silently drag the camera's facing away from where it was
    // told to look - a bug that compounds with every bump instead of being a fixed offset
    // (this is what "camera keeps breaking" turned out to be). The camera is a plain root
    // object with its own independent rotation, manually kept in the same position as this
    // body each frame - see SyncCameraPosition.
    //
    // Space jumps once, then needs JUMP_COOLDOWN_SECONDS before it'll fire again, regardless
    // of whether you've landed in the meantime.
    public sealed class SimpleFirstPersonController : Component
    {
        public Transform CameraTransform { get; set; }
        public bool IsGrounded => _isGrounded;

        private const float MOVE_SPEED = 8f;
        private const float ACCELERATION = 50f;
        private const float GROUND_FRICTION = 10f;
        private const float JUMP_IMPULSE = 7f;
        private const float JUMP_COOLDOWN_SECONDS = 2f;
        private const float GROUND_CHECK_DISTANCE = 0.25f;
        private const float EYE_HEIGHT = 1.6f;

        // radians per pixel of raw mouse delta - deliberately NOT scaled by deltaTime (the
        // original MouseYawPitchController did that, which makes sensitivity depend on
        // framerate since mouse delta is already a per-frame sample, not a rate)
        private const float MOUSE_SENSITIVITY = 0.0025f;
        private const float MAX_PITCH_DEGREES = 89f;

        private const float CAPSULE_RADIUS = 0.5f;
        private const float CAPSULE_HEIGHT = 1.8f;

        private PhysicsSystem _physicsSystem;
        private RigidBody _rigidBody;

        private MouseState _oldMouseState;
        private KeyboardState _oldKeyboardState;

        private float _yaw;
        private float _pitch;
        private bool _isGrounded;
        private float _jumpCooldownRemaining;

        protected override void Awake()
        {
            EnsurePhysicsComponents();
        }

        protected override void Start()
        {
            _oldMouseState = Mouse.GetState();
            _oldKeyboardState = Keyboard.GetState();
        }

        protected override void Update(float deltaTime)
        {
            var currentKeyboard = Keyboard.GetState();

            UpdateLook();
            UpdateGrounded();
            UpdateMovement(deltaTime, currentKeyboard);
            UpdateJump(deltaTime, currentKeyboard);
            FreezeBodyRotation();
            SyncCameraPosition();

            _oldKeyboardState = currentKeyboard;
        }

        private void EnsurePhysicsComponents()
        {
            _physicsSystem = GameObject.Scene.GetSystem<PhysicsSystem>();

            var capsule = GameObject.AddComponent<CapsuleCollider>();
            capsule.Radius = CAPSULE_RADIUS;
            capsule.Height = CAPSULE_HEIGHT;
            capsule.Center = new Vector3(0f, CAPSULE_HEIGHT * 0.5f - CAPSULE_RADIUS, 0f);
            capsule.IsTrigger = false;

            _rigidBody = GameObject.AddComponent<RigidBody>();
            _rigidBody.BodyType = BodyType.Dynamic;
            _rigidBody.UseGravity = true;
            _rigidBody.LinearDamping = 0f;
            _rigidBody.AngularDamping = 0f;
        }

        private void UpdateLook()
        {
            var newMouseState = Mouse.GetState();
            float dX = newMouseState.X - _oldMouseState.X;
            float dY = newMouseState.Y - _oldMouseState.Y;

            _yaw += dX * MOUSE_SENSITIVITY;
            _pitch -= dY * MOUSE_SENSITIVITY;

            float maxPitchRad = MathHelper.ToRadians(MAX_PITCH_DEGREES);
            _pitch = MathHelper.Clamp(_pitch, -maxPitchRad, maxPitchRad);

            // camera has no parent, so RotateToWorld sets its rotation directly with nothing
            // else able to perturb it afterward - see the class comment for why that matters
            if (CameraTransform != null)
                CameraTransform.RotateToWorld(Quaternion.CreateFromYawPitchRoll(_yaw, _pitch, 0f));

            // recenter the cursor every frame so it never runs out of window to move across -
            // without this, the cursor drifts to the edge of the screen and stops generating
            // any further delta in that direction, which caps how far you can turn and makes
            // the camera feel like it stops responding to the mouse.
            var viewport = GameObject.Scene.Context.GraphicsDevice.Viewport;
            int centerX = viewport.Width / 2;
            int centerY = viewport.Height / 2;
            Mouse.SetPosition(centerX, centerY);
            _oldMouseState = Mouse.GetState();
        }

        private void UpdateGrounded()
        {
            _isGrounded = false;

            if (_physicsSystem == null || Transform == null)
                return;

            Vector3 origin = Transform.Position + Vector3.Up * 0.05f;
            if (_physicsSystem.Raycast(origin, -Vector3.Up, GROUND_CHECK_DISTANCE, out _))
                _isGrounded = true;
        }

        private void UpdateMovement(float deltaTime, KeyboardState currentKeyboard)
        {
            if (_rigidBody == null)
                return;

            // yaw-derived basis, derived from actually expanding
            // Quaternion.CreateFromYawPitchRoll(yaw,0,0) applied to Vector3.Forward/Right by
            // hand (not just checked at yaw=0, which can't catch a sign error - sin(0)=0 either
            // way). This matches the camera's real rotation at every yaw, not just the start.
            Vector3 forward = new Vector3(-MathF.Sin(_yaw), 0f, -MathF.Cos(_yaw));
            Vector3 right = new Vector3(MathF.Cos(_yaw), 0f, -MathF.Sin(_yaw));

            Vector3 inputDirection = Vector3.Zero;
            if (currentKeyboard.IsKeyDown(Keys.W)) inputDirection += forward;
            if (currentKeyboard.IsKeyDown(Keys.S)) inputDirection -= forward;
            if (currentKeyboard.IsKeyDown(Keys.D)) inputDirection += right;
            if (currentKeyboard.IsKeyDown(Keys.A)) inputDirection -= right;

            bool hasInput = inputDirection.LengthSquared() > 0.0001f;
            if (hasInput)
                inputDirection.Normalize();

            Vector3 velocity = _rigidBody.LinearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.X, 0f, velocity.Z);
            Vector3 desiredHorizontalVelocity = hasInput ? inputDirection * MOVE_SPEED : Vector3.Zero;

            if (hasInput)
            {
                Vector3 velocityDelta = desiredHorizontalVelocity - horizontalVelocity;
                float maxChange = ACCELERATION * deltaTime;
                float deltaLength = velocityDelta.Length();

                if (deltaLength > maxChange && deltaLength > 0f)
                    velocityDelta *= maxChange / deltaLength;

                horizontalVelocity += velocityDelta;
            }
            else if (_isGrounded && GROUND_FRICTION > 0f)
            {
                float frictionFactor = MathHelper.Clamp(1f - GROUND_FRICTION * deltaTime, 0f, 1f);
                horizontalVelocity *= frictionFactor;

                if (horizontalVelocity.LengthSquared() < 0.0001f)
                    horizontalVelocity = Vector3.Zero;
            }

            velocity.X = horizontalVelocity.X;
            velocity.Z = horizontalVelocity.Z;
            _rigidBody.LinearVelocity = velocity;
        }

        private void UpdateJump(float deltaTime, KeyboardState currentKeyboard)
        {
            if (_jumpCooldownRemaining > 0f)
                _jumpCooldownRemaining -= deltaTime;

            bool jumpPressed = currentKeyboard.IsKeyDown(Keys.Space) && _oldKeyboardState.IsKeyUp(Keys.Space);

            if (jumpPressed && _isGrounded && _jumpCooldownRemaining <= 0f)
            {
                var velocity = _rigidBody.LinearVelocity;
                velocity.Y = JUMP_IMPULSE;
                _rigidBody.LinearVelocity = velocity;

                _jumpCooldownRemaining = JUMP_COOLDOWN_SECONDS;
            }
        }

        // stop the capsule from picking up spin/tip from collisions - it doesn't need any
        // rotation at all anymore (movement uses the tracked _yaw, not Transform.Forward/Right,
        // and the camera no longer depends on this body's rotation either), so there's nothing
        // to gain from letting physics rotate it and a stray spin could only cause weirdness
        // (e.g. the capsule's ground-contact geometry shifting under it).
        private void FreezeBodyRotation()
        {
            if (_rigidBody != null)
                _rigidBody.AngularVelocity = Vector3.Zero;
        }

        // camera is a root object (see class comment) - manually follow the body's position
        // every frame instead of relying on parent/child transform inheritance
        private void SyncCameraPosition()
        {
            if (CameraTransform == null || Transform == null)
                return;

            CameraTransform.TranslateTo(Transform.Position + Vector3.Up * EYE_HEIGHT);
        }
    }
}
