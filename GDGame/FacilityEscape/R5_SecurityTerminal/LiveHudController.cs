using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering;

namespace GDGame.FacilityEscape
{
    // wires 3 UIText elements to live-updating values: player position, player speed, and
    // how many zones are still locked. Canonical UIText.TextProvider HUD pattern (same shape
    // as GDEngine's own UIPickerInfo) - set the provider once in Start(), the values it reads
    // update on their own every frame since Player/RigidBody are live references.
    public sealed class LiveHudController : Component
    {
        public GameObject Player { get; set; }
        public UIText PositionText { get; set; }
        public UIText VelocityText { get; set; }
        public UIText LocksText { get; set; }

        protected override void Start()
        {
            if (Player == null)
                return;

            var rigidBody = Player.GetComponent<RigidBody>();

            if (PositionText != null)
                PositionText.TextProvider = () => $"Position: {Player.Transform.Position:F1}";

            if (VelocityText != null)
                VelocityText.TextProvider = () => $"Speed: {(rigidBody != null ? rigidBody.LinearVelocity.Length() : 0f):F2} m/s";

            if (LocksText != null)
                LocksText.TextProvider = () => $"Locks remaining: {ZoneProgressState.RemainingCount}";
        }
    }
}
