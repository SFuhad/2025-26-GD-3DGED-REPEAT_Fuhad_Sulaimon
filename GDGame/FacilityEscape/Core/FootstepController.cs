using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // plays a quiet "step" sound on a timer while the player is grounded and actually moving.
    // no dedicated footstep asset exists in Content, so this reuses the softest UI click SFX
    // as a placeholder - kept deliberately quiet, this is meant to be barely-there texture,
    // not a "sound effect" you consciously notice.
    public sealed class FootstepController : Component
    {
        private const float STEP_INTERVAL_SECONDS = 0.4f;
        private const float MOVE_SPEED_THRESHOLD = 0.5f;
        private const float STEP_VOLUME = 0.12f;

        private RigidBody _rigidBody;
        private SimpleFirstPersonController _controller;
        private float _sinceLastStep;

        protected override void Start()
        {
            _rigidBody = GameObject.GetComponent<RigidBody>();
            _controller = GameObject.GetComponent<SimpleFirstPersonController>();
        }

        protected override void Update(float deltaTime)
        {
            if (_rigidBody == null || _controller == null || !_controller.IsGrounded)
            {
                _sinceLastStep = 0f;
                return;
            }

            var horizontalVelocity = _rigidBody.LinearVelocity;
            horizontalVelocity.Y = 0f;

            if (horizontalVelocity.LengthSquared() < MOVE_SPEED_THRESHOLD * MOVE_SPEED_THRESHOLD)
            {
                _sinceLastStep = 0f;
                return;
            }

            _sinceLastStep += deltaTime;
            if (_sinceLastStep < STEP_INTERVAL_SECONDS)
                return;

            _sinceLastStep = 0f;
            EngineContext.Instance.Events.Publish(new PlaySfxEvent(
                "SFX_UI_Click_Designed_Pop_Movement_Open_1", STEP_VOLUME, false, null));
        }
    }
}
