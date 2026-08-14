using GDEngine.Core.Components;
using Microsoft.Xna.Framework;

namespace GDGame.FacilityEscape
{
    // if the player ends up below FALL_THRESHOLD_Y (should only happen by tunneling through a
    // collision seam onto the VoidCatcher floor way down at y=-100, see BuildBoxRoom), teleport
    // back to spawn instead of leaving them stuck down there.
    public sealed class RespawnOnFall : Component
    {
        public Vector3 SpawnPosition { get; set; }

        private const float FALL_THRESHOLD_Y = -20f;

        protected override void Update(float deltaTime)
        {
            if (Transform.Position.Y < FALL_THRESHOLD_Y)
                Respawn();
        }

        private void Respawn()
        {
            var rigidBody = GameObject.GetComponent<RigidBody>();

            if (rigidBody != null)
            {
                rigidBody.LinearVelocity = Vector3.Zero;
                rigidBody.AngularVelocity = Vector3.Zero;
            }

            Transform.TranslateTo(SpawnPosition);

            if (rigidBody != null)
            {
                // RigidBody has no direct "teleport" API - a Dynamic body's Transform is
                // overwritten FROM the physics simulation every step (Physics -> Transform,
                // one-way), so just moving the Transform gets silently undone next frame.
                // Toggling BodyType forces the engine to tear down and recreate the physics
                // body from the Transform's current position, which is the only way to
                // actually move a Dynamic body from outside the physics step.
                rigidBody.BodyType = BodyType.Kinematic;
                rigidBody.BodyType = BodyType.Dynamic;
            }
        }
    }
}
