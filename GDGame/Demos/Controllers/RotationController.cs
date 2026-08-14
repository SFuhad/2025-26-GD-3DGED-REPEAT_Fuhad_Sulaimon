using GDEngine.Core.Components;
using GDEngine.Core.Entities;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.Demos.Controllers
{
    // spins the gameobject around an axis, like a turntable by default (spins around Y)
    // pretty much the simplest controller in this folder, everything else builds on this idea
    public sealed class RotationController : Component
    {
        #region Static Fields
        // anything smaller than this and it's basically not moving, don't bother doing the math
        private static readonly float ROTATION_THRESHOLD = 1E-8f;
        #endregion

        #region Fields
        // which way it spins, defaults to straight up (Vector3.Up)
        public Vector3 _rotationAxisNormalized = Vector3.Up;

        // how fast, in radians/sec. negative = spins the other way
        public float _rotationSpeedInRadiansPerSecond = MathF.PI / 2f; // roughly 90 degrees a second
        #endregion

        #region Lifecycle Methods
        protected override void Update(float deltaTime)
        {
            // don't bother if the speed is basically zero
            if (MathF.Abs(_rotationSpeedInRadiansPerSecond) <= ROTATION_THRESHOLD)
                return;

            // angle for this frame = speed * time (basic physics formula from class)
            float angle = _rotationSpeedInRadiansPerSecond * deltaTime;

            Quaternion delta = Quaternion.CreateFromAxisAngle(_rotationAxisNormalized, angle);

            Transform?.RotateBy(delta);
        }

        protected override void Awake()
        {
            if (Transform == null)
                throw new ArgumentNullException(nameof(Transform));

            // has to be normalized or the rotation speed won't match what you set - learned this the hard way
            _rotationAxisNormalized.Normalize();
        }
        #endregion
    }
}
