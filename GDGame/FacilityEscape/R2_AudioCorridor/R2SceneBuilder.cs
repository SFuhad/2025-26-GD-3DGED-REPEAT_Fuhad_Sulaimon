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
    // R2 - walk down the corridor between two looping spatial emitters (alarm left, radio
    // chatter right - panning should be obvious as you pass between them), step on the SFX
    // pad, then activate the console at the far end to switch music and complete the zone.
    public static class R2SceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material litMaterial)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(
                sceneManager, FacilityAppData.R2_SCENE_NAME, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, litMaterial,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            FacilitySceneFactory.BuildPlayerAndCamera(scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            BuildEmitterMarker(scene, device, litMaterial, textures.Get("crate1"),
                new Vector3(-6, 1.5f, -3), "laser_gun_salve", 0.5f);
            BuildEmitterMarker(scene, device, litMaterial, textures.Get("mona lisa"),
                new Vector3(6, 1.5f, -3), "SFX_UI_Click_Designed_Pop_Negative_Close_1", 0.35f);

            var statusText = new UIText(font, "CONSOLE OFFLINE", new Vector2(20, 20));
            statusText.FallbackColor = Color.OrangeRed;
            var statusGO = new GameObject("R2_StatusText");
            statusGO.AddComponent(statusText);
            scene.Add(statusGO);

            var annotationGO = new GameObject("R2_Annotation");
            var annotation = new UIText(font,
                "AudioSystem + EventBus - Spatial 3D sources + EventBus.Publish\n" +
                "Walk between the alarm (left) and radio chatter (right). Step on the pad\n" +
                "for a one-shot SFX, then activate the console to switch music.",
                new Vector2(20, 50));
            annotation.FallbackColor = Color.LightGray;
            annotationGO.AddComponent(annotation);
            scene.Add(annotationGO);

            BuildSfxPad(scene, device, litMaterial, textures.Get("checkerboard"), new Vector3(0, 0.25f, 3));

            var console = BuildConsole(scene, device, litMaterial, textures.Get("button_rectangle_10"), statusText);

            BuildReturnToHubPortal(scene, device, litMaterial, textures.Get("mainmenu_monkey"), sceneManager);

            var gameState = scene.GetSystem<GameStateSystem>();
            gameState.ConfigureConditions(
                GameConditions.FromPredicate("console activated", () => console.IsActivated),
                null);
            gameState.StateChanged += (oldState, newState) =>
            {
                if (newState == GameOutcomeState.Won)
                    EngineContext.Instance.Events.Publish(new ZoneCompletedEvent(FacilityAppData.ZONE_ID_R2));
            };

            return scene;
        }

        private static void BuildEmitterMarker(Scene scene, GraphicsDevice device, Material material,
            Texture2D texture, Vector3 position, string clipId, float volume)
        {
            var go = new GameObject("Emitter_" + clipId);
            go.Transform.TranslateTo(position);
            go.Transform.ScaleTo(new Vector3(1, 1, 1));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var emitter = go.AddComponent<LoopingSpatialEmitter>();
            emitter.ClipId = clipId;
            emitter.Volume = volume;
            emitter.RepeatIntervalSeconds = 2.5f;

            scene.Add(go);
        }

        private static void BuildSfxPad(Scene scene, GraphicsDevice device, Material material,
            Texture2D texture, Vector3 position)
        {
            var go = new GameObject("R2_SfxPad");
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

            var pad = go.AddComponent<SfxTriggerPad>();
            pad.ClipId = "SFX_UI_Click_Designed_Pop_Generic_1";
            pad.Volume = 0.6f;

            scene.Add(go);
        }

        private static ConsoleActivator BuildConsole(Scene scene, GraphicsDevice device,
            Material material, Texture2D texture, UIText statusText)
        {
            var go = new GameObject("R2_Console");
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

            var activator = go.AddComponent<ConsoleActivator>();
            activator.StatusText = statusText;

            scene.Add(go);
            return activator;
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
