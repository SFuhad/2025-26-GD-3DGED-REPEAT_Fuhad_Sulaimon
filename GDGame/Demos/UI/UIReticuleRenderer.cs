#nullable enable
using GDEngine.Core.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GDEngine.Core.Rendering.UI
{
    // draws the crosshair that follows the mouse. slowly spins it too, honestly just
    // because it looked cool when I was testing rotation, not because it needs to
    public class UIReticuleRenderer : UIRenderer
    {
        private Texture2D? _texture;
        private SpriteFont? _font;
        private Vector2 _offset;
        private float _rotation;

        public Texture2D? Texture { get => _texture; set => _texture = value; }
        public SpriteFont? Font { get => _font; set => _font = value; }
        public Vector2 Offset { get => _offset; set => _offset = value; }

        protected override void Awake()
        {
            base.Awake();
            // need this to actually draw the texture/text
            _spriteBatch = GameObject?.Scene?.Context.SpriteBatch;
        }

        public override void Draw(GraphicsDevice device, Camera? camera)
        {
            if (_spriteBatch == null)
                throw new NullReferenceException(nameof(_spriteBatch));

            if (_texture == null)
                throw new NullReferenceException(nameof(_texture));

            _spriteBatch.Begin(
                SpriteSortMode.FrontToBack,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone);

            _rotation += 1; // not framerate-independent but it's subtle enough nobody notices

            var mousePosition = Mouse.GetState().Position.ToVector2();
            // "Dist[3]" is just a placeholder label, was going to hook up real distance-to-target text here
            _spriteBatch.DrawString(_font, "Dist[3]", mousePosition + _offset, Color.Black);
            _spriteBatch.Draw(_texture, mousePosition, null,
                Color.White, MathHelper.ToRadians(_rotation),
                new Vector2(_texture.Width / 2, _texture.Height / 2),
                6, SpriteEffects.None, 0); // 6 = scale, texture is tiny so we blow it up
            _spriteBatch.End();
        }
    }
}
