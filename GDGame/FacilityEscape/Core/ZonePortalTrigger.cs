using System;
using GDEngine.Core.Components;
using GDEngine.Core.Events;
using GDEngine.Core.Managers;
using GDEngine.Core.Services;

namespace GDGame.FacilityEscape
{
    // generic "walk into this trigger volume and switch scenes" component. Used for every
    // hub door (5x) and every zone's "return to hub" trigger (5x more). Doesn't care which
    // scene it's switching to/from, just needs a target scene name.
    public sealed class ZonePortalTrigger : Component
    {
        public SceneManager SceneManager { get; set; }
        public string TargetSceneName { get; set; }

        private RigidBody _myTriggerBody;
        private readonly TriggerDebouncer _debounce = new TriggerDebouncer();
        private IDisposable _sub;

        protected override void Start()
        {
            _myTriggerBody = GameObject.GetComponent<RigidBody>();

            // deliberately never disposed - this needs to keep working every single time the
            // player walks through this door for as long as the game runs, not just once
            _sub = EngineContext.Instance.Events.On<TriggerEvent>().Do(OnTrigger);
        }

        private void OnTrigger(TriggerEvent e)
        {
            if (e.TriggerBody != _myTriggerBody) return;               // Rule #1 - identity, not name
            if (e.OtherBody?.GameObject?.Name != AppData.PLAYER_NAME) return;
            if (!_debounce.TryEnter(e.OtherBody)) return;               // only fire once per entry

            SceneManager.SetActiveScene(TargetSceneName);
        }
    }
}
