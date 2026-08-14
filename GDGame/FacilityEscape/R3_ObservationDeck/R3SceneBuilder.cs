using GDEngine.Core.Collections;
using GDEngine.Core.Entities;
using GDEngine.Core.Managers;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // stub for this checkpoint - the orbit camera controller + trigger-based camera
    // switching come in the next pass.
    public static class R3SceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material material)
        {
            return StubZoneBuilder.Build(sceneManager, FacilityAppData.R3_SCENE_NAME,
                FacilityAppData.R3_DISPLAY_NAME, device, sounds, textures, font, material);
        }
    }
}
