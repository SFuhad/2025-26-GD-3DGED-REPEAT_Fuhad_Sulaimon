using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.Demos.Controllers
{
    // moves a gameobject along a 1D animation curve, kind of a hacky demo but it works
    public class AnimationCurveController : Component
    {
        #region Fields
        private Vector3 _direction = Vector3.UnitY;
        private AnimationCurve _curve;

        // cached so we don't recompute every frame
        private float _totalElapsedTimeSecs;
        private Vector3 _originalLocalPosition;

        public AnimationCurveController(AnimationCurve curve)
        {
            _curve = curve;
        }
        #endregion

        public Vector3 Direction { get => _direction; set => _direction = value; }
        public AnimationCurve Curve { get => _curve; set => _curve = value; }

        protected override void Update(float deltaTime)
        {
            if (Curve == null)
                throw new ArgumentNullException(nameof(Curve));

            _totalElapsedTimeSecs += Time.UnscaledDeltaTimeSecs;

           // 2 = smoothing passes, just eyeballed a value that looked ok
           var delta = _curve.Evaluate(_totalElapsedTimeSecs, 2);
           Transform?.TranslateTo(_originalLocalPosition + delta * _direction);
        }

        protected override void Awake()
        {
            if (Transform == null)
                throw new ArgumentNullException(nameof(Transform));

            // save starting position so the curve offsets from here, not from world origin
            _originalLocalPosition = Transform.LocalPosition;

            // normalize direction otherwise the curve output gets scaled weirdly
            _direction.Normalize();
        }
    }
}
