using System;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // Observer #1. Lives once in the Hub scene, doesn't care which zone published the
    // ZoneCompletedEvent - just marks it done in the shared ZoneProgressState, which the
    // hub's HUD overlay reads from every frame via TextProvider.
    public sealed class ZoneProgressController : Component
    {
        private IDisposable _sub;

        protected override void Start()
        {
            _sub = EngineContext.Instance.Events.On<ZoneCompletedEvent>()
                .WithPriorityPreset(EventPriority.Gameplay)
                .Do(e => ZoneProgressState.MarkComplete(e.ZoneId));
        }
    }
}
