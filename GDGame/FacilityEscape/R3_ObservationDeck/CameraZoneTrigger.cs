using System;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // walk-in trigger that swaps the active camera by name - same shape as ZonePortalTrigger,
    // just calls Scene.SetActiveCamera instead of SceneManager.SetActiveScene. This is how R3
    // switches camera modes "by walking into collision triggers, not key presses."
    public sealed class CameraZoneTrigger : Component
    {
        public string TargetCameraName { get; set; }
        public Action OnActivated { get; set; }

        private RigidBody _myTriggerBody;
        private readonly TriggerDebouncer _debounce = new TriggerDebouncer();
        private IDisposable _sub;

        protected override void Start()
        {
            _myTriggerBody = GameObject.GetComponent<RigidBody>();
            _sub = EngineContext.Instance.Events.On<TriggerEvent>().Do(OnTrigger);
        }

        private void OnTrigger(TriggerEvent e)
        {
            if (e.TriggerBody != _myTriggerBody) return;
            if (e.OtherBody?.GameObject?.Name != AppData.PLAYER_NAME) return;
            if (!_debounce.TryEnter(e.OtherBody)) return;

            GameObject.Scene.SetActiveCamera(TargetCameraName);
            OnActivated?.Invoke();
        }
    }
}
