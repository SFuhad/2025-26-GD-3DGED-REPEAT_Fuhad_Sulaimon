using System.Collections.Generic;
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Gameplay;
using GDEngine.Core.Managers;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // R3 - three floor pads near spawn switch between the player's own first-person camera,
    // an orbit camera circling the central terminal, and a fixed security-camera-style view -
    // all trigger-based, no key presses. Zone completes once all 3 have been visited.
    public static class R3SceneBuilder
    {
        private static readonly Vector3 TerminalPosition = new Vector3(0, 1f, -6f);

        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material litMaterial)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(
                sceneManager, FacilityAppData.R3_SCENE_NAME, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, litMaterial,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            FacilitySceneFactory.BuildPlayerAndCamera(scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            BuildTerminal(scene, device, litMaterial, textures.Get("mona lisa"));
            BuildOrbitCamera(scene);
            BuildFixedCamera(scene);

            var annotationGO = new GameObject("R3_Annotation");
            var annotation = new UIText(font,
                "CameraSystem - FirstPerson / Orbit / Cinematic modes\n" +
                "Step on the three floor pads to cycle camera modes - first person (left),\n" +
                "orbit (middle), fixed security cam (right). No key presses involved.",
                new Vector2(20, 50));
            annotation.FallbackColor = Color.LightGray;
            annotationGO.AddComponent(annotation);
            scene.Add(annotationGO);

            var visitedModes = new HashSet<string>();

            BuildCameraPad(scene, device, litMaterial, textures.Get("crate1"),
                new Vector3(-6, 0.1f, 3), AppData.CAMERA_NAME_FIRST_PERSON, () => visitedModes.Add("fps"));
            BuildCameraPad(scene, device, litMaterial, textures.Get("tree4"),
                new Vector3(0, 0.1f, 3), "R3_OrbitCamera", () => visitedModes.Add("orbit"));
            BuildCameraPad(scene, device, litMaterial, textures.Get("button_rectangle_10"),
                new Vector3(6, 0.1f, 3), "R3_FixedCamera", () => visitedModes.Add("fixed"));

            BuildReturnToHubPortal(scene, device, litMaterial, textures.Get("mainmenu_monkey"), sceneManager);

            var gameState = scene.GetSystem<GameStateSystem>();
            gameState.ConfigureConditions(
                GameConditions.FromPredicate("all camera modes visited", () => visitedModes.Count >= 3),
                null);
            gameState.StateChanged += (oldState, newState) =>
            {
                if (newState == GameOutcomeState.Won)
                    EngineContext.Instance.Events.Publish(new ZoneCompletedEvent(FacilityAppData.ZONE_ID_R3));
            };

            return scene;
        }

        private static void BuildTerminal(Scene scene, GraphicsDevice device, Material material, Texture2D texture)
        {
            var go = new GameObject("R3_Terminal");
            go.Transform.TranslateTo(TerminalPosition);
            go.Transform.ScaleTo(new Vector3(1.5f, 2f, 1.5f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(1.5f, 2f, 1.5f);

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            scene.Add(go);
        }

        private static void BuildOrbitCamera(Scene scene)
        {
            var go = new GameObject("R3_OrbitCamera");
            var camera = go.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(70f);

            var orbit = go.AddComponent<OrbitCameraController>();
            orbit.TargetPoint = TerminalPosition;
            orbit.Radius = 6f;
            orbit.Height = 4f;
            orbit.SpeedRadPerSec = 0.4f;

            scene.Add(go);
        }

        private static void BuildFixedCamera(Scene scene)
        {
            var position = new Vector3(9f, 4f, 9f);

            var go = new GameObject("R3_FixedCamera");
            go.Transform.TranslateTo(position);

            var camera = go.AddComponent<Camera>();
            camera.FieldOfView = MathHelper.ToRadians(60f);

            scene.Add(go);

            // aim it at the terminal once - genuinely static, no controller, closest reading
            // of "fixed security-camera style"
            Vector3 forward = Vector3.Normalize(TerminalPosition - position);
            Matrix worldMatrix = Matrix.CreateWorld(position, forward, Vector3.Up);
            go.Transform.RotateToWorld(Quaternion.CreateFromRotationMatrix(worldMatrix));
        }

        private static void BuildCameraPad(Scene scene, GraphicsDevice device, Material material,
            Texture2D texture, Vector3 position, string targetCameraName, System.Action onActivated)
        {
            var go = new GameObject("R3_CameraPad_" + targetCameraName);
            go.Transform.TranslateTo(position);
            go.Transform.ScaleTo(new Vector3(3, 0.5f, 3));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(3, 0.5f, 3);
            collider.IsTrigger = true;

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var trigger = go.AddComponent<CameraZoneTrigger>();
            trigger.TargetCameraName = targetCameraName;
            trigger.OnActivated = onActivated;

            scene.Add(go);
        }

        private static void BuildReturnToHubPortal(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture, SceneManager sceneManager)
        {
            var go = new GameObject("ReturnToHub");
            go.Transform.TranslateTo(new Vector3(0, 1.5f, 9f));
            go.Transform.ScaleTo(new Vector3(3, 3, 1));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(3, 3, 1);
            collider.IsTrigger = true;

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var portal = go.AddComponent<ZonePortalTrigger>();
            portal.SceneManager = sceneManager;
            portal.TargetSceneName = FacilityAppData.HUB_SCENE_NAME;

            scene.Add(go);
        }
    }
}
