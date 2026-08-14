using GDEngine.Core.Collections;
using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using GDEngine.Core.Factories;
using GDEngine.Core.Managers;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // shared shape for the 4 zones that are still placeholders this pass (R2/R3/R5/R6):
    // room + player + a way back to the hub + a "not built yet" sign. Each zone's own
    // SceneBuilder just calls this with its own name/id - once a zone gets real content,
    // its SceneBuilder stops calling this and gets its own R1SceneBuilder-style build.
    public static class StubZoneBuilder
    {
        public static Scene Build(SceneManager sceneManager, string sceneName, string displayName,
            GraphicsDevice device, ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material material)
        {
            var scene = FacilitySceneFactory.BuildStandardScene(sceneManager, sceneName, device, sounds);

            FacilitySceneFactory.BuildBoxRoom(scene, device, material,
                textures.Get("checkerboard"), FacilityAppData.ROOM_SIZE);

            FacilitySceneFactory.BuildPlayerAndCamera(scene, FacilityAppData.DEFAULT_SPAWN_POSITION);

            var signGO = new GameObject("StubSign");
            var sign = new UIText(font, displayName + " - under construction (next pass)", new Vector2(20, 20));
            sign.FallbackColor = Color.Yellow;
            signGO.AddComponent(sign);
            scene.Add(signGO);

            var portalGO = new GameObject("ReturnToHub");
            portalGO.Transform.TranslateTo(new Vector3(0, 1.5f, 9f));
            portalGO.Transform.ScaleTo(new Vector3(3, 3, 1));

            var meshFilter = MeshFilterFactory.CreateCubeTexturedLit(device);
            portalGO.AddComponent(meshFilter);

            var meshRenderer = portalGO.AddComponent<MeshRenderer>();
            meshRenderer.Material = material;
            meshRenderer.Overrides.MainTexture = textures.Get("mainmenu_monkey");

            var collider = portalGO.AddComponent<BoxCollider>();
            collider.Size = new Vector3(3, 3, 1);
            collider.IsTrigger = true;

            var rigidBody = portalGO.AddComponent<RigidBody>();
            rigidBody.BodyType = BodyType.Static;

            var portal = portalGO.AddComponent<ZonePortalTrigger>();
            portal.SceneManager = sceneManager;
            portal.TargetSceneName = FacilityAppData.HUB_SCENE_NAME;

            scene.Add(portalGO);

            return scene;
        }
    }
}
