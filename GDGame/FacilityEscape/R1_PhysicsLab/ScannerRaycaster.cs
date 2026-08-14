using GDEngine.Core.Components;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Systems;
using GDEngine.Core.Utilities;

namespace GDGame.FacilityEscape
{
    // player-mounted "scanner" - raycasts from the center of the screen every frame and
    // writes whatever it's looking at into a UIText label. Modeled directly on
    // GDGame/Demos/Controllers/Interaction/InteractionComponent.cs's use of
    // PhysicsSystem.RaycastFromScreen, minus the "delete on click" part.
    public sealed class ScannerRaycaster : Component
    {
        public UIText Label { get; set; }

        private PhysicsSystem _physics;
        private string _currentTarget = string.Empty;

        protected override void Start()
        {
            _physics = GameObject.Scene.GetSystem<PhysicsSystem>();

            if (Label != null)
                Label.TextProvider = () => _currentTarget;
        }

        protected override void Update(float deltaTime)
        {
            var scene = GameObject.Scene;
            var camera = scene?.ActiveCamera;

            if (_physics == null || camera == null)
            {
                _currentTarget = string.Empty;
                return;
            }

            var viewport = camera.GetViewport(scene.Context.GraphicsDevice);
            var center = viewport.GetCenter();

            if (_physics.RaycastFromScreen(camera, center.X, center.Y, 25f, LayerMask.All, out var hit, false))
                _currentTarget = hit.Body?.GameObject?.Name ?? string.Empty;
            else
                _currentTarget = string.Empty;
        }
    }
}
