using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // PlaySfxEvent has no loop flag (only PlayMusicEvent loops), so this simulates a looping
    // 3D emitter by re-publishing a one-shot spatial SFX on a timer from its own position.
    // Spatial panning comes from AudioSystem syncing its listener to the active camera each
    // frame and applying 3D attenuation based on this Transform - see PlaySfxEvent(spatial:true).
    public sealed class LoopingSpatialEmitter : Component
    {
        public string ClipId { get; set; }
        public float Volume { get; set; } = 1f;
        public float RepeatIntervalSeconds { get; set; } = 2f;

        private float _elapsed;

        protected override void Update(float deltaTime)
        {
            _elapsed += deltaTime;
            if (_elapsed < RepeatIntervalSeconds)
                return;

            _elapsed = 0f;
            EngineContext.Instance.Events.Publish(new PlaySfxEvent(ClipId, Volume, true, Transform));
        }
    }
}
