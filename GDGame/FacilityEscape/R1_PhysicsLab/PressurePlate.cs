using System;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;

namespace GDGame.FacilityEscape
{
    // sits under a trigger collider on the floor. First rigidbody that lands on it (any of
    // the 3 crates) opens the exit door and flips the status text green - this is the
    // "trigger volume changes scene state" requirement for R1.
    public sealed class PressurePlate : Component
    {
        public GameObject ExitDoor { get; set; }
        public UIText StatusText { get; set; }

        public bool IsActivated { get; private set; }

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
            if (e.TriggerBody != _myBody) return;   // Rule #1 - identity filter
            if (IsActivated) return;                 // already open, don't re-slide the door
            if (!_debounce.TryEnter(e.OtherBody)) return;

            IsActivated = true;

            if (ExitDoor != null)
                ExitDoor.Transform.TranslateBy(Vector3.Up * 5f, true);

            if (StatusText != null)
                StatusText.FallbackColor = Color.LimeGreen;
        }
    }
}
