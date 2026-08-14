using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.Demos.Controllers
{
    // same idea as SineTranslationController but you can plug in an easing function
    // so the motion doesn't feel so robotic/linear. bit over-engineered for the demo tbh
    // but our lecturer wanted to see we understood easing curves
    public class EasableSineTranslationController : Component
    {
        #region Fields
        // radians/sec, 2PI = one full cycle per second
        public float _angularSpeed = MathHelper.TwoPi * 0.5f;

        // how far it swings from the start position
        public float _maxDistance = 1f;

        // which way it moves (gets normalized in Awake)
        public Vector3 _direction = Vector3.UnitY;

        // shove the cycle forward/back if you don't want it starting at 0
        public float _phaseRadians = 0f;

        // default is EaseInOutSine but you can swap for EaseOutCubic etc, see Ease.cs
        public Func<float, float> _timeCurve = Ease.EaseInOutSine;

        private Vector3 _originalLocalPosition;
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Methods
        // takes the raw angle we've been accumulating and squishes it through the easing curve
        private float ComputeEasedAngle(float rawAngleInRadians)
        {
            // how many full cycles have we done, keep just the leftover fraction (0-1)
            float cycles = rawAngleInRadians / MathHelper.TwoPi;
            float t = cycles - MathF.Floor(cycles);

            // run it through the easing function (if we have one)
            float te = _timeCurve != null ? _timeCurve(t) : t;

            // back to radians so Sin() can use it
            float angleEased = te * MathHelper.TwoPi;
            return angleEased;
        }
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            if (Transform == null)
                throw new ArgumentNullException(nameof(Transform));

            // remember where we started, everything oscillates around this point
            _originalLocalPosition = Transform.LocalPosition;

            if (_direction != Vector3.Zero)
                _direction.Normalize();
        }

        protected override void Update(float deltaTime)
        {
            // using real time here (not scaled deltaTime) so pausing doesn't mess up the phase
            float angleRaw = (float)Time.RealtimeSinceStartupSecs * _angularSpeed + _phaseRadians;

            float angle = ComputeEasedAngle(angleRaw);

            // -1 to 1
            float distance = MathF.Sin(angle);

            Transform?.TranslateTo(_originalLocalPosition + _direction * distance * _maxDistance);
        }
        #endregion
    }
}
