using GDEngine.Core.Audio;
using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Events;
using GDEngine.Core.Factories;
using GDEngine.Core.Gameplay;
using GDEngine.Core.Managers;
using GDEngine.Core.Rendering;
using GDEngine.Core.Rendering.UI;
using GDEngine.Core.Services;
using GDEngine.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // R5 - a live HUD (position/speed/locks remaining), a slider that actually scales gravity
    // (watch the dynamic crate fall faster/slower), and a button that mutes/restores the
    // ambient hum - all through real UIText/UISlider/UIButton components, no raw SpriteBatch.
    public static class R5SceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material litMaterial)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(
                sceneManager, FacilityAppData.R5_SCENE_NAME, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, litMaterial,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            var player = FacilitySceneFactory.BuildPlayerAndCamera(scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            var dynamicCrate = BuildDynamicCrate(scene, device, litMaterial, textures.Get("crate1"));

            var ambientGO = new GameObject("R5_AmbientEmitter");
            ambientGO.Transform.TranslateTo(new Vector3(0, 1.5f, -8f));
            var ambientEmitter = ambientGO.AddComponent<LoopingSpatialEmitter>();
            ambientEmitter.ClipId = "secret_door";
            ambientEmitter.Volume = 0.3f;
            ambientEmitter.RepeatIntervalSeconds = 3f;
            scene.Add(ambientGO);

            var annotationGO = new GameObject("R5_Annotation");
            var annotation = new UIText(font,
                "UIRenderSystem + InputSystem - UIText / UIButton / UISlider\n" +
                "HUD (top-left) reflects live engine state. Slider scales gravity - watch\n" +
                "the crate. Button mutes/restores the ambient hum.",
                new Vector2(20, 50));
            annotation.FallbackColor = Color.LightGray;
            annotationGO.AddComponent(annotation);
            scene.Add(annotationGO);

            bool hasInteracted = false;

            BuildHud(scene, font, player);
            BuildGravitySlider(scene, textures, font, () => { hasInteracted = true; });
            BuildMuteButton(scene, textures, font, () => { hasInteracted = true; });

            BuildReturnToHubPortal(scene, device, litMaterial, textures.Get("mainmenu_monkey"), sceneManager);

            var gameState = scene.GetSystem<GameStateSystem>();
            gameState.ConfigureConditions(
                GameConditions.FromPredicate("player used a control", () => hasInteracted),
                null);
            gameState.StateChanged += (oldState, newState) =>
            {
                if (newState == GameOutcomeState.Won)
                    EngineContext.Instance.Events.Publish(new ZoneCompletedEvent(FacilityAppData.ZONE_ID_R5));
            };

            return scene;
        }

        private static GameObject BuildDynamicCrate(Scene scene, GraphicsDevice device, Material material, Texture2D texture)
        {
            var go = new GameObject("R5_GravityDemoCrate");
            go.Transform.TranslateTo(new Vector3(4, 6, -4));
            go.Transform.ScaleTo(new Vector3(1.2f, 1.2f, 1.2f));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            go.AddComponent(meshFilter);

            var meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = texture;

            var collider = go.AddComponent<BoxCollider>();
            collider.Size = new Vector3(1.2f, 1.2f, 1.2f);

            var rigidBody = go.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Dynamic;
            rigidBody.Mass = 1f;

            scene.Add(go);
            return go;
        }

        private static void BuildHud(Scene scene, SpriteFont font, GameObject player)
        {
            var hudGO = new GameObject("R5_LiveHud");

            var positionText = hudGO.AddComponent<UIText>();
            positionText.Font = font;
            positionText.PositionProvider = () => new Vector2(20, 90);
            positionText.FallbackColor = Color.Cyan;
            positionText.LayerDepth = UILayer.HUD;

            var velocityText = hudGO.AddComponent<UIText>();
            velocityText.Font = font;
            velocityText.PositionProvider = () => new Vector2(20, 114);
            velocityText.FallbackColor = Color.Cyan;
            velocityText.LayerDepth = UILayer.HUD;

            var locksText = hudGO.AddComponent<UIText>();
            locksText.Font = font;
            locksText.PositionProvider = () => new Vector2(20, 138);
            locksText.FallbackColor = Color.Cyan;
            locksText.LayerDepth = UILayer.HUD;

            var hud = hudGO.AddComponent<LiveHudController>();
            hud.Player = player;
            hud.PositionText = positionText;
            hud.VelocityText = velocityText;
            hud.LocksText = locksText;

            scene.Add(hudGO);
        }

        private static void BuildGravitySlider(Scene scene, ContentDictionary<Texture2D> textures,
            SpriteFont font, System.Action onInteracted)
        {
            var physicsSystem = scene.GetSystem<PhysicsSystem>();
            Vector2 rowPos = new Vector2(20, 170);
            Vector2 itemSize = new Vector2(220, 30);

            var sliderGO = new GameObject("R5_GravitySlider");

            var track = sliderGO.AddComponent<UITexture>();
            track.Texture = textures.Get("Free Flat Hyphen Icon");
            track.Position = rowPos + new Vector2(120, 0);
            track.Size = new Vector2(itemSize.X - 120, itemSize.Y * 0.4f);
            track.Tint = Color.White;
            track.LayerDepth = UILayer.Menu;

            var slider = sliderGO.AddComponent<UISlider>();
            slider.TargetGraphic = track;
            slider.AutoSizeFromTargetGraphic = false;
            slider.Position = track.Position;
            slider.Size = track.Size;
            slider.MinValue = 0.1f;
            slider.MaxValue = 2f;
            slider.WholeNumbers = false;
            slider.Value = 1f;

            var handleGO = new GameObject("R5_GravitySliderHandle");
            handleGO.Transform.SetParent(sliderGO.Transform);
            var handle = handleGO.AddComponent<UITexture>();
            handle.Texture = textures.Get("Free Flat Toggle Thumb Centre Icon");
            handle.Size = new Vector2(20, 20);
            handle.Tint = Color.White;
            handle.LayerDepth = UILayer.MenuFront;
            slider.HandleGraphic = handle;

            var label = sliderGO.AddComponent<UIText>();
            label.Font = font;
            label.TextProvider = () => $"Gravity: {slider.Value:0.00}x";
            label.PositionProvider = () => rowPos + new Vector2(0, itemSize.Y * 0.5f);
            label.Anchor = TextAnchor.Left;
            label.FallbackColor = Color.White;
            label.LayerDepth = UILayer.HUD;

            slider.ValueChanged += v =>
            {
                if (physicsSystem != null)
                    physicsSystem.Gravity = AppData.GRAVITY * v;
                onInteracted();
            };

            scene.Add(sliderGO);
            scene.Add(handleGO);
        }

        private static void BuildMuteButton(Scene scene, ContentDictionary<Texture2D> textures,
            SpriteFont font, System.Action onInteracted)
        {
            Vector2 rowPos = new Vector2(20, 210);
            Vector2 itemSize = new Vector2(160, 32);
            bool muted = false;

            var buttonGO = new GameObject("R5_MuteButton");

            var graphic = buttonGO.AddComponent<UITexture>();
            graphic.Texture = textures.Get("button_rectangle_10");
            graphic.Position = rowPos;
            graphic.Size = itemSize;
            graphic.Tint = Color.White;
            graphic.LayerDepth = UILayer.Menu;

            var button = buttonGO.AddComponent<UIButton>();
            button.TargetGraphic = graphic;
            button.AutoSizeFromTargetGraphic = false;
            button.Position = rowPos;
            button.Size = itemSize;
            button.NormalColor = Color.White;
            button.HighlightedColor = Color.LightGray;
            button.PressedColor = Color.Gray;

            var label = buttonGO.AddComponent<UIText>();
            label.Font = font;
            label.TextProvider = () => muted ? "Unmute" : "Mute";
            label.PositionProvider = () => button.Position + button.Size * 0.5f;
            label.Anchor = TextAnchor.Center;
            label.FallbackColor = Color.Black;
            label.LayerDepth = UILayer.MenuFront;

            button.Clicked += () =>
            {
                muted = !muted;
                EngineContext.Instance.Events.Publish(
                    new FadeChannelEvent(AudioMixer.AudioChannel.Master, muted ? 0f : 1f, 0.5f));
                onInteracted();
            };

            scene.Add(buttonGO);
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
