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
using GDEngine.Core.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // the fully-worked zone for this checkpoint. Push crates onto the pressure plate,
    // it opens the vault gate and the zone completes. Scanner on the player highlights
    // whatever it's looking at. Everything else (R2/R3/R5/R6) follows this same shape.
    public static class R1SceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material litMaterial)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(
                sceneManager, FacilityAppData.R1_SCENE_NAME, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, litMaterial,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            var player = FacilitySceneFactory.BuildPlayerAndCamera(
                scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            BuildCrate(scene, device, litMaterial, textures.Get("crate1"), new Vector3(-5, 3, -3));
            BuildCrate(scene, device, litMaterial, textures.Get("crate1"), new Vector3(5, 3, -3));
            BuildCrate(scene, device, litMaterial, textures.Get("crate1"), new Vector3(0, 3, 3));

            var statusText = new UIText(font, "AWAITING WEIGHT", new Vector2(20, 20));
            statusText.FallbackColor = Color.OrangeRed;
            var statusGO = new GameObject("R1_StatusText");
            statusGO.AddComponent(statusText);
            scene.Add(statusGO);

            var annotationGO = new GameObject("R1_Annotation");
            var annotation = new UIText(font,
                "PhysicsSystem - RigidBody + TriggerVolume + Raycast\n" +
                "Push the crates onto the pressure plate. The scanner (center label)\n" +
                "highlights whatever the player is currently looking at.",
                new Vector2(20, 50));
            annotation.FallbackColor = Color.LightGray;
            annotationGO.AddComponent(annotation);
            scene.Add(annotationGO);

            var exitDoor = BuildExitDoor(scene, device, litMaterial, textures.Get("button_rectangle_10"));

            var plate = BuildPressurePlate(scene, device, litMaterial, textures.Get("checkerboard"),
                exitDoor, statusText);

            BuildScanner(player, font, device);

            BuildReturnToHubPortal(scene, device, litMaterial, textures.Get("mainmenu_monkey"), sceneManager);

            // zone completion - this exact shape (predicate + StateChanged -> Publish) is what
            // every other zone reuses, only the predicate and zone id change
            var gameState = scene.GetSystem<GameStateSystem>();
            gameState.ConfigureConditions(
                GameConditions.FromPredicate("plate activated", () => plate.IsActivated),
                null);
            gameState.StateChanged += (oldState, newState) =>
            {
                if (newState == GameOutcomeState.Won)
                    EngineContext.Instance.Events.Publish(new ZoneCompletedEvent(FacilityAppData.ZONE_ID_R1));
            };

            return scene;
        }

        private static void BuildCrate(Scene scene, GraphicsDevice device, Material material,
            Texture2D texture, Vector3 position)
        {
            var go = new GameObject("Crate");
            go.Transform.TranslateTo(position);
            go.Transform.ScaleTo(new Vector3(1.5f, 1.5f, 1.5f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(1.5f, 1.5f, 1.5f);

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Dynamic;
            rigidBody.Mass = 1.0f;

            scene.Add(go);
        }

        private static GameObject BuildExitDoor(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture)
        {
            var go = new GameObject("R1_VaultGate");
            go.Transform.TranslateTo(new Vector3(0, 1.5f, -9.5f));
            go.Transform.ScaleTo(new Vector3(4, 3, 1));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(4, 3, 1);

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            scene.Add(go);
            return go;
        }

        private static PressurePlate BuildPressurePlate(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture, GameObject exitDoor, UIText statusText)
        {
            var go = new GameObject("R1_PressurePlate");
            go.Transform.TranslateTo(new Vector3(0, 0.25f, -8f));
            go.Transform.ScaleTo(new Vector3(4, 0.5f, 4));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(4, 0.5f, 4);
            collider.IsTrigger = true;

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var plate = go.AddComponent<PressurePlate>();
            plate.ExitDoor = exitDoor;
            plate.StatusText = statusText;

            scene.Add(go);
            return plate;
        }

        private static void BuildScanner(GameObject player, SpriteFont font, GraphicsDevice device)
        {
            var label = new UIText(font);
            label.PositionProvider = () => device.Viewport.GetCenter() + new Vector2(0, 40);
            label.Anchor = TextAnchor.Center;
            label.FallbackColor = Color.Cyan;

            var labelGO = new GameObject("R1_ScannerLabel");
            labelGO.AddComponent(label);
            player.Scene.Add(labelGO);

            var scanner = player.AddComponent<ScannerRaycaster>();
            scanner.Label = label;
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
