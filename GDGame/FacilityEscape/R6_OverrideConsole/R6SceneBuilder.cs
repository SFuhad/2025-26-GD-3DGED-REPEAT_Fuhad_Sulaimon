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
    // R6 - walk into the console to publish an override event, flip the lockdown status
    // (red -> green) and stop any playing music, then the zone completes.
    public static class R6SceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material litMaterial)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(
                sceneManager, FacilityAppData.R6_SCENE_NAME, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, litMaterial,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            FacilitySceneFactory.BuildPlayerAndCamera(scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            var statusText = new UIText(font, "LOCKDOWN ACTIVE", new Vector2(20, 20));
            statusText.FallbackColor = Color.Red;
            var statusGO = new GameObject("R6_StatusText");
            statusGO.AddComponent(statusText);
            scene.Add(statusGO);

            var annotationGO = new GameObject("R6_Annotation");
            var annotation = new UIText(font,
                "EventBus + GameStateSystem - Custom events + fluent On<T>()\n" +
                "Walk into the override console to publish events and change game state.\n" +
                "Two event types, three subscriptions across Gameplay/UI/Telemetry priorities.",
                new Vector2(20, 50));
            annotation.FallbackColor = Color.LightGray;
            annotationGO.AddComponent(annotation);
            scene.Add(annotationGO);

            var console = BuildConsole(scene, device, litMaterial, textures.Get("button_rectangle_10"), statusText);

            BuildReturnToHubPortal(scene, device, litMaterial, textures.Get("mainmenu_monkey"), sceneManager);

            var gameState = scene.GetSystem<GameStateSystem>();
            gameState.ConfigureConditions(
                GameConditions.FromPredicate("override activated", () => console.IsOverridden),
                null);
            gameState.StateChanged += (oldState, newState) =>
            {
                if (newState == GameOutcomeState.Won)
                    EngineContext.Instance.Events.Publish(new ZoneCompletedEvent(FacilityAppData.ZONE_ID_R6));
            };

            return scene;
        }

        private static OverrideConsoleController BuildConsole(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture, UIText statusText)
        {
            var go = new GameObject("R6_Console");
            go.Transform.TranslateTo(new Vector3(0, 1.5f, -9f));
            go.Transform.ScaleTo(new Vector3(2, 3, 1));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(2, 3, 1);
            collider.IsTrigger = true;

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var controller = go.AddComponent<OverrideConsoleController>();
            controller.StatusText = statusText;

            scene.Add(go);
            return controller;
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
