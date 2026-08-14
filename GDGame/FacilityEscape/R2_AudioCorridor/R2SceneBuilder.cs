using GDEngine.Core.Collections;
using GDEngine.Core.Entities;
using GDEngine.Core.Managers;
using GDEngine.Core.Rendering;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.FacilityEscape
{
    // stub for this checkpoint - spatial audio emitters + music-switch console come in
    // the next pass, following the same shape R1SceneBuilder already proved out.
    public static class R2SceneBuilder
    {
        public static Scene Build(SceneManager sceneManager, GraphicsDevice device,
            ContentDictionary<SoundEffect> sounds, ContentDictionary<Texture2D> textures,
            SpriteFont font, Material material)
        {
            return StubZoneBuilder.Build(sceneManager, FacilityAppData.R2_SCENE_NAME,
                FacilityAppData.R2_DISPLAY_NAME, device, sounds, textures, font, material);
        }
    }
}
