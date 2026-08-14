using System;
using GDEngine.Core.Audio;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using GDEngine.Core.Utilities;
using Microsoft.Xna.Framework.Input;

namespace GDGame.Demos.Controllers
{
    // when the player left clicks and we're looking at something, delete it
    // basically just a proof-of-concept for the raycasting + click stuff, not final game logic
    public class InteractionComponent : Component
    {
        private Scene _scene;
        private PhysicsSystem _physicsSystem;

        private float _maxDistance = 100;
        private LayerMask _hitMask = LayerMask.All;
        private bool _hitTriggers = false;
        private MouseState _currentMouseState;
        private MouseState _oldMouseState;

        public float MaxDistance { get => _maxDistance; set => _maxDistance = value; }
        public LayerMask HitMask { get => _hitMask; set => _hitMask = value; }
        public bool HitTriggers { get => _hitTriggers; set => _hitTriggers = value; }

        protected override void Start()
        {
            if (GameObject == null)
                throw new NullReferenceException(nameof(GameObject));

            _scene = GameObject.Scene
                        ?? throw new NullReferenceException(nameof(GameObject.Scene));

            _physicsSystem = _scene.GetSystem<PhysicsSystem>()
                            ?? throw new InvalidOperationException(
                                // yeah the error message name is wrong, copy pasted this from another class, should fix later
                                "UIPickerInfoRenderer requires a PhysicsSystem in the Scene.");
        }
        protected override void Update(float deltaTime)
        {
            _currentMouseState = Mouse.GetState();

            if (GameObject == null)
                return;

            var scene = GameObject.Scene;
            if (scene == null)
                return;

            // grab the active camera fresh each frame in case it switched (first/third person etc)
            var camera = scene.ActiveCamera ?? null;
            if (camera == null)
                return;

            var device = scene.Context.GraphicsDevice;
            var viewport = camera.GetViewport(device);

            // reticle is dead center of the screen, same spot the crosshair UI uses
            var center = viewport.GetCenter();

            // shoot a ray out from the center of the screen and see what we hit
            RaycastHit hitInfo;
            if (_physicsSystem.RaycastFromScreen(
                    camera,
                    center.X,
                    center.Y,
                    MaxDistance,
                    HitMask,
                    out hitInfo,
                    HitTriggers))
            {
                System.Diagnostics.Debug.WriteLine($"looking at: {hitInfo.Body.GameObject.Name}");

                // only fire on the actual click, not every frame the button happens to be held
                if (_currentMouseState.LeftButton == ButtonState.Pressed
                    && _oldMouseState.LeftButton == ButtonState.Released)
                {
                    // little click sfx so it feels like something happened
                    EngineContext.Instance.Events.Publish(new PlaySfxEvent("SFX_UI_Click_Designed_Pop_Generic_1", 1, false, null));

                    _scene.Remove(hitInfo.Body.GameObject);
                }
            }

            _oldMouseState = _currentMouseState;
        }
    }
}
