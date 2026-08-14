using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.Demos.Controllers
{
    // makes an object bob back and forth along one axis using a sine wave
    // used this for the floating pickup items, but works for anything
    public class SineTranslationController : Component
    {
        #region Fields
        // radians/sec - bigger number = faster back and forth
        public float _angularSpeed = 1f;

        // how far it swings from where it started
        public float _maxDistance = 1f;

        // which axis it moves along, gets normalized in Awake so don't worry about the magnitude
        public Vector3 _direction = Vector3.UnitY;

        private Vector3 _originalLocalPosition;
        #endregion

        #region Lifecycle Methods
        protected override void Update(float deltaTime)
        {
            // note: using RealtimeSinceStartup here instead of deltaTime, keeps the phase
            // consistent even if the game gets paused/unpaused
            float phase = (float)(Time.RealtimeSinceStartupSecs * _angularSpeed);
            float distance = MathF.Sin(phase); // -1 to 1

            Transform?.TranslateTo(_originalLocalPosition + _direction * distance * _maxDistance);
        }

        protected override void Awake()
        {
            if (Transform == null)
                throw new ArgumentNullException(nameof(Transform));

            // this is the "center" the object oscillates around
            _originalLocalPosition = Transform.LocalPosition;

            if (_direction != Vector3.Zero)
                _direction.Normalize();
        }
        #endregion
    }
}
