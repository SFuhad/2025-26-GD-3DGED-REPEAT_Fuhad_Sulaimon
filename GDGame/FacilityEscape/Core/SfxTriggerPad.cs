using System;
using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // generic "step on this and it plays a sound once" pad - reusable anywhere a zone needs
    // a player action to trigger an SFX via the EventBus. Same debounce shape as PressurePlate.
    public sealed class SfxTriggerPad : Component
    {
        public string ClipId { get; set; }
        public float Volume { get; set; } = 0.6f;

        private RigidBody _myBody;
        private readonly TriggerDebouncer _debounce = new TriggerDebouncer();
        private IDisposable _sub;

        protected override void Start()
        {
            _myBody = GameObject.GetComponent<RigidBody>();
            _sub = EngineContext.Instance.Events.On<TriggerEvent>().Do(OnTrigger);
        }

        private void OnTrigger(TriggerEvent e)
        {
            if (e.TriggerBody != _myBody) return;
            if (!_debounce.TryEnter(e.OtherBody)) return;

            EngineContext.Instance.Events.Publish(new PlaySfxEvent(ClipId, Volume, false, null));
        }
    }
}
