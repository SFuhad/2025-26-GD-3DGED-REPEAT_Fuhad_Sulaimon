using System;
using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;

namespace GDGame.FacilityEscape
{
    // walk-in trigger for R2's console. On first activation: publishes ConsoleActivatedEvent
    // (the "named scene event" the music switch requirement asks for), switches the music
    // track, and flips the status text green - same shape as R1's PressurePlate.
    public sealed class ConsoleActivator : Component
    {
        public bool IsActivated { get; private set; }
        public UIText StatusText { get; set; }

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
            if (IsActivated) return;
            if (!_debounce.TryEnter(e.OtherBody)) return;

            IsActivated = true;

            var events = EngineContext.Instance.Events;
            events.Publish(new ConsoleActivatedEvent());
            // no dedicated second music track exists in Content, so this just swaps back to
            // the same clip the hub uses for ambience, at a level that actually reads as
            // "music" rather than background hum - reused placeholder, same as the hub's.
            events.Publish(new PlayMusicEvent("secret_door", 0.3f, 2f));

            if (StatusText != null)
                StatusText.FallbackColor = Color.LimeGreen;
        }
    }
}
