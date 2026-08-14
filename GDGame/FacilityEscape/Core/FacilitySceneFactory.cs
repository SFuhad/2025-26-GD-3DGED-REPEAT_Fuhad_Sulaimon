using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Components.Controllers.Physics;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Impulses;
using GDEngine.Core.Input.Data;
using GDEngine.Core.Input.Devices;
using GDEngine.Core.Managers;
using GDEngine.Core.Orchestration;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.Base;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDGame.FacilityEscape
{
    // this is the one place that does what Main.cs's InitializeSystems()/InitializeCameras()
    // used to do for the single demo scene - except now it's a method we can call 6 times
    // (once per hub/zone) instead of copy-pasting ~80 lines per scene. Every Scene in this
    // engine owns its own full set of systems (nothing is shared engine-wide except the
    // EventBus/EngineContext), so this factory is genuinely load-bearing - get it wrong here
    // and every zone breaks the same way.
    public static class FacilitySceneFactory
    {
        public static Scene BuildStandardScene(SceneManager sceneManager, string name,
            GraphicsDevice device, ContentDictionary<SoundEffect> sounds, bool physicsDebug = false)
        {
            var scene = new Scene(EngineContext.Instance, name);
            sceneManager.AddScene(name, scene);

            var physics = scene.AddSystem(new PhysicsSystem());
            physics.Gravity = AppData.GRAVITY;

            if (physicsDebug)
                scene.AddSystem(new PhysicsDebugSystem());

            scene.Add(new EventSystem(EngineContext.Instance.Events));
            scene.Add(BuildInputSystem());
            scene.Add(new CameraSystem(device, -100));
            scene.Add(new RenderSystem(-100));
            scene.Add(new UIRenderSystem(-100));

            var audio = new AudioSystem(sounds);
            // the G1 fix - without this every scene's AudioSystem would react to the same
            // PlaySfxEvent/PlayMusicEvent at once since they all share one EventBus
            audio.IsActiveScene = () => sceneManager.ActiveScene == scene;
            scene.Add(audio);

            scene.Add(BuildOrchestrationSystem());
            scene.Add(new ImpulseSystem(EngineContext.Instance.Impulses));
            scene.AddSystem(new UIEventSystem());
            scene.AddSystem(new GameStateSystem());

            return scene;
        }

        private static InputSystem BuildInputSystem()
        {
            // same bindings Main.cs uses for the legacy demo scene, just extracted so every
            // zone gets the same feel instead of drifting apart
            var bindings = InputBindings.Default;
            bindings.MouseSensitivity = 0.12f;
            bindings.DebounceMs = 60;
            bindings.EnableKeyRepeat = true;
            bindings.KeyRepeatMs = 300;

            var inputSystem = new InputSystem();
            inputSystem.Add(new GDKeyboardInput(bindings));
            inputSystem.Add(new GDMouseInput(bindings));
            inputSystem.Add(new GDGamepadInput(PlayerIndex.One, AppData.GAMEPAD_P1_NAME));
            return inputSystem;
        }

        private static OrchestrationSystem BuildOrchestrationSystem()
        {
            var orchestrationSystem = new OrchestrationSystem();
            orchestrationSystem.Configure(options =>
            {
                options.Time = Orchestrator.OrchestrationTime.Unscaled;
                options.LocalScale = 1;
                options.Paused = false;
            });
            return orchestrationSystem;
        }

        // first-person capsule (physics/movement) + child camera (look), same recipe as
        // Main.InitializeCameras()'s "First-person capsule + camera" block
        public static GameObject BuildPlayerAndCamera(Scene scene, Vector3 spawnPosition)
        {
            var parentGO = new GameObject(AppData.PLAYER_NAME);
            parentGO.Layer = LayerMask.IgnoreRaycast;
            parentGO.Transform.TranslateTo(spawnPosition);

            // NOT FirstPersonCapsuleController/MouseYawPitchController - that stock combo kept
            // feeling inverted no matter how it was wired (see SimpleFirstPersonController's
            // header comment for why). This one owns movement + look together with an explicit,
            // hand-verified W/A/S/D mapping and a cooldown-gated jump.
            var controller = parentGO.AddComponent<SimpleFirstPersonController>();

            parentGO.AddComponent<FootstepController>();

            var respawn = parentGO.AddComponent<RespawnOnFall>();
            respawn.SpawnPosition = spawnPosition;

            // deliberately NOT parented to parentGO - see SimpleFirstPersonController's class
            // comment for why the camera has to stay independent of the physics body's Transform.
            // SimpleFirstPersonController.SyncCameraPosition keeps it following every frame;
            // this initial position just avoids a one-frame pop at (0,0,0) before that first runs.
            var cameraGO = new GameObject(AppData.CAMERA_NAME_FIRST_PERSON);
            cameraGO.Transform.TranslateTo(spawnPosition + Vector3.Up * 1.6f);
            var camera = cameraGO.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(80.0f);

            controller.CameraTransform = cameraGO.Transform;

            // order matters: SimpleFirstPersonController.Awake() (which needs the scene's
            // PhysicsSystem to already exist) only runs once the GameObject is added to the
            // scene, so BuildStandardScene must be called before this
            scene.Add(parentGO);
            scene.Add(cameraGO);
            scene.ActiveCamera = camera;

            return parentGO;
        }

        // floor + 4 walls, all just scaled cubes with static box colliders - reuses whatever
        // texture/material is passed in, no new assets needed.
        //
        // walls/floor deliberately OVERLAP each other at every seam (instead of just touching
        // edge-to-edge) - colliders that only "kiss" at a boundary leave a seam narrow/fast
        // enough movement can slip through with no contact ever being generated there. That's
        // what let the player walk through the floor/wall joins and corners.
        public static void BuildBoxRoom(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture, Vector3 roomSize)
        {
            float halfX = roomSize.X / 2f;
            float halfZ = roomSize.Z / 2f;
            const float wallThickness = 1f;
            const float overlap = 1f; // how far walls reach into the floor and into each other at corners

            BuildBoxPart(scene, device, material, texture, "Floor",
                new Vector3(0, -0.5f, 0), new Vector3(roomSize.X, 1f, roomSize.Z));

            // reach down into the floor by `overlap`, and reach past the room edges by
            // `overlap` on each side so adjacent walls overlap instead of meeting at a corner seam
            float wallHeight = roomSize.Y + overlap;
            float wallCenterY = (roomSize.Y - overlap) / 2f;
            float wallSpanX = roomSize.X + overlap * 2f;
            float wallSpanZ = roomSize.Z + overlap * 2f;

            BuildBoxPart(scene, device, material, texture, "Wall_North",
                new Vector3(0, wallCenterY, -halfZ), new Vector3(wallSpanX, wallHeight, wallThickness));

            BuildBoxPart(scene, device, material, texture, "Wall_South",
                new Vector3(0, wallCenterY, halfZ), new Vector3(wallSpanX, wallHeight, wallThickness));

            BuildBoxPart(scene, device, material, texture, "Wall_East",
                new Vector3(halfX, wallCenterY, 0), new Vector3(wallThickness, wallHeight, wallSpanZ));

            BuildBoxPart(scene, device, material, texture, "Wall_West",
                new Vector3(-halfX, wallCenterY, 0), new Vector3(wallThickness, wallHeight, wallSpanZ));

            // safety net: if anything ever does tunnel out (a fast-moving crate, a physics
            // glitch), it free-falls under gravity forever with nothing to stop it, and enough
            // frames of unbounded acceleration eventually overflows to Infinity/NaN and crashes
            // the whole simulation (exactly the BepuPhysics crash we hit). A big catch-floor far
            // below every room means a leak is just a visible fall-through-the-world bug, not a crash.
            BuildBoxPart(scene, device, material, texture, "VoidCatcher",
                new Vector3(0, -100f, 0), new Vector3(2000f, 2f, 2000f));
        }

        private static void BuildBoxPart(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture, string name, Vector3 position, Vector3 size)
        {
            var go = new GameObject(name);
            go.Transform.TranslateTo(position);
            go.Transform.ScaleTo(size);   // CreateCubeTexturedLit is a unit cube, so ScaleTo == full world size

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = size;

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;
            go.IsStatic = true;
            go.Layer = LayerMask.Ground;

            scene.Add(go);
        }
    }
}
