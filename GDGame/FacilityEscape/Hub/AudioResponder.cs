using System;
using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // Observer #2. Same subject (ZoneCompletedEvent) as ZoneProgressController, different
    // reaction - plays a little confirmation "ding" whenever any zone completes, regardless
    // of which one. This is the second leg of the Subject=EventBus / Observers=these-two-classes
    // pattern - same event type, two independent listeners doing unrelated things with it.
    public sealed class AudioResponder : Component
    {
        private IDisposable _sub;

        protected override void Start()
        {
            var events = EngineContext.Instance.Events;

            _sub = events.On<ZoneCompletedEvent>()
                .WithPriorityPreset(EventPriority.UI)
                .Do(e => events.Publish(new PlaySfxEvent(
                    "SFX_UI_Click_Designed_Pop_Mallet_Open_1", 1f, false, null)));
        }
    }
}
