using GDEngine.Core.Audio;
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Managers;
using GDEngine.Core.Rendering;
using GDEngine.Core.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // the control room - 5 doors, a HUD overlay showing what's locked/complete, and the two
    // Observer components (ZoneProgressController/AudioResponder) that make the cross-zone
    // "completing a zone updates the hub" behaviour work.
    public static class HubSceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material wallMaterial)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(
                sceneManager, FacilityAppData.HUB_SCENE_NAME, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, wallMaterial,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            FacilitySceneFactory.BuildPlayerAndCamera(scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            // 5 doors in a row along the north wall, each a different texture so they're
            // at least visually distinguishable from each other (no dedicated "door" art exists)
            BuildDoor(scene, device, wallMaterial, textures.Get("crate1"),
                sceneManager, FacilityAppData.R1_SCENE_NAME, new Vector3(-10, 1.5f, -10));
            BuildDoor(scene, device, wallMaterial, textures.Get("tree4"),
                sceneManager, FacilityAppData.R2_SCENE_NAME, new Vector3(-5, 1.5f, -10));
            BuildDoor(scene, device, wallMaterial, textures.Get("mona lisa"),
                sceneManager, FacilityAppData.R3_SCENE_NAME, new Vector3(0, 1.5f, -10));
            BuildDoor(scene, device, wallMaterial, textures.Get("button_rectangle_10"),
                sceneManager, FacilityAppData.R5_SCENE_NAME, new Vector3(5, 1.5f, -10));
            BuildDoor(scene, device, wallMaterial, textures.Get("Crosshair_21"),
                sceneManager, FacilityAppData.R6_SCENE_NAME, new Vector3(10, 1.5f, -10));

            BuildObservers(scene);
            BuildHudOverlay(scene, font);

            // ambient "facility hum" - no dedicated ambient loop asset exists, so this is a
            // placeholder reuse of a one-shot door-creak sound. It's not a smooth drone, so
            // looping it at any real volume reads as a repeating random noise rather than
            // ambience - kept very quiet on purpose until there's an actual ambient loop asset.
            EngineContext.Instance.Events.Publish(new PlayMusicEvent("secret_door", 0.05f, 2f));

            return scene;
        }

        private static void BuildDoor(Scene scene, GraphicsDevice device, Material material,
            Texture2D texture, SceneManager sceneManager, string targetSceneName, Vector3 position)
        {
            var go = new GameObject("Door_" + targetSceneName);
            go.Transform.TranslateTo(position);
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
            portal.TargetSceneName = targetSceneName;

            scene.Add(go);
        }

        private static void BuildObservers(Scene scene)
        {
            var observersGO = new GameObject("Observers");
            observersGO.AddComponent<ZoneProgressController>();
            observersGO.AddComponent<AudioResponder>();
            scene.Add(observersGO);
        }

        private static void BuildHudOverlay(Scene scene, SpriteFont font)
        {
            var hudGO = new GameObject("HubHUD");

            var title = new UIText(font, "FACILITY ESCAPE - CONTROL ROOM", new Vector2(20, 20));
            title.FallbackColor = Color.White;
            hudGO.AddComponent(title);

            // the architecture annotation the brief asks every zone to have
            var annotation = new UIText(font,
                "SceneManager swaps one active Scene at a time; EventBus is shared across all of them.\n" +
                "Walk into a door to enter that zone. Completing a zone publishes ZoneCompletedEvent,\n" +
                "which this Hub listens for (ZoneProgressController + AudioResponder) and reflects below.",
                new Vector2(20, 50));
            annotation.FallbackColor = Color.LightGray;
            hudGO.AddComponent(annotation);

            float y = 130;
            foreach (var zoneId in ZoneProgressState.AllZoneIds)
            {
                // capture per-iteration locals - `y` is a manually-incremented loop variable,
                // not the foreach variable, so it needs its own copy or every line's
                // TextProvider closure ends up reading whatever `y` is after the loop finishes
                string capturedZoneId = zoneId;
                float capturedY = y;
                string displayName = DisplayNameFor(capturedZoneId);

                var line = hudGO.AddComponent<UIText>();
                line.Font = font;
                line.TextProvider = () =>
                    $"{capturedZoneId} - {displayName}: " +
                    (ZoneProgressState.IsComplete(capturedZoneId) ? "COMPLETE" : "LOCKED");
                line.PositionProvider = () => new Vector2(20, capturedY);
                line.ColorProvider = () =>
                    ZoneProgressState.IsComplete(capturedZoneId) ? Color.LimeGreen : Color.OrangeRed;

                y += 24;
            }

            scene.Add(hudGO);
        }

        private static string DisplayNameFor(string zoneId)
        {
            if (zoneId == FacilityAppData.ZONE_ID_R1) return FacilityAppData.R1_DISPLAY_NAME;
            if (zoneId == FacilityAppData.ZONE_ID_R2) return FacilityAppData.R2_DISPLAY_NAME;
            if (zoneId == FacilityAppData.ZONE_ID_R3) return FacilityAppData.R3_DISPLAY_NAME;
            if (zoneId == FacilityAppData.ZONE_ID_R5) return FacilityAppData.R5_DISPLAY_NAME;
            if (zoneId == FacilityAppData.ZONE_ID_R6) return FacilityAppData.R6_DISPLAY_NAME;
            return zoneId;
        }
    }
}
