#nullable enable
using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.Demos.Controllers
{
    public class PositionRotationController : Component
    {
        #region Fields
        private AnimationCurve3D? _positionCurve;
        private AnimationCurve3D? _rotationCurve;

        private float totalElapsedTimeSecs;
        private Quaternion _originalLocalRotation;
        private Vector3 _oldYawPitchRoll;

        #endregion
        #region Properties
        public AnimationCurve3D? PositionCurve { get => _positionCurve; set => _positionCurve = value; }
        public AnimationCurve3D? RotationCurve { get => _rotationCurve; set => _rotationCurve = value; }

        #endregion
        protected override void Update(float deltaTime)
        {
            if (Transform == null || _positionCurve == null || _rotationCurve == null)
                return;

            totalElapsedTimeSecs += deltaTime;

            // just snap straight to whatever position the curve says for this point in time
            Transform.TranslateTo(_positionCurve.Evaluate(totalElapsedTimeSecs));

            // spent AGES debugging gimbal lock here - turns out you can't just set rotation directly
            // from the curve, you have to diff it against last frame and apply the delta instead
            var nextYawPitchRoll = _rotationCurve.Evaluate(totalElapsedTimeSecs);

            var deltaYawPitchRoll = nextYawPitchRoll - _oldYawPitchRoll;
            if (deltaYawPitchRoll.LengthSquared() > 0)
                Transform.RotateEulerBy(deltaYawPitchRoll, true);

            _oldYawPitchRoll = nextYawPitchRoll;
        }

        protected override void Awake()
        {
            if (Transform == null)
                throw new NullReferenceException(nameof(Transform));

            _originalLocalRotation = Transform.LocalRotation;
            _oldYawPitchRoll = Vector3.Zero;
        }
    }
}
