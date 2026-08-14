using System;
using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;

namespace GDGame.FacilityEscape
{
    // walking into the console publishes OverrideActivatedEvent (Gameplay priority), which
    // flips the lockdown state and publishes LockdownStateChangedEvent - subscribed twice more,
    // once at UI priority (flips the status text) and once at Telemetry priority (just logs,
    // deliberately inert - demonstrates "must never affect gameplay ordering" literally).
    public sealed class OverrideConsoleController : Component
    {
        public bool IsOverridden { get; private set; }
        public UIText StatusText { get; set; }

        private RigidBody _myBody;
        private readonly TriggerDebouncer _debounce = new TriggerDebouncer();
        private IDisposable _triggerSub;
        private IDisposable _overrideSub;
        private IDisposable _uiSub;
        private IDisposable _telemetrySub;

        protected override void Start()
        {
            _myBody = GameObject.GetComponent<RigidBody>();
            var events = EngineContext.Instance.Events;

            _triggerSub = events.On<TriggerEvent>().Do(OnTrigger);

            _overrideSub = events.On<OverrideActivatedEvent>()
                .WithPriorityPreset(EventPriority.Gameplay)
                .Do(HandleOverrideActivated);

            _uiSub = events.On<LockdownStateChangedEvent>()
                .WithPriorityPreset(EventPriority.UI)
                .Do(HandleLockdownUiFlash);

            _telemetrySub = events.On<LockdownStateChangedEvent>()
                .WithPriorityPreset(EventPriority.Telemetry)
                .Do(e => System.Console.WriteLine($"[Telemetry] lockdown override = {e.IsOverridden}"));
        }

        private void OnTrigger(TriggerEvent e)
        {
            if (e.TriggerBody != _myBody) return;
            if (e.OtherBody?.GameObject?.Name != AppData.PLAYER_NAME) return;
            if (IsOverridden) return;
            if (!_debounce.TryEnter(e.OtherBody)) return;

            EngineContext.Instance.Events.Publish(new OverrideActivatedEvent("R6_Console"));
        }

        private void HandleOverrideActivated(OverrideActivatedEvent e)
        {
            IsOverridden = true;
            var events = EngineContext.Instance.Events;
            events.Publish(new LockdownStateChangedEvent(true));
            events.Publish(new StopMusicEvent(2f)); // the "music stop" visible consequence
        }

        private void HandleLockdownUiFlash(LockdownStateChangedEvent e)
        {
            if (StatusText != null)
                StatusText.FallbackColor = e.IsOverridden ? Color.LimeGreen : Color.Red;
        }
    }
}
