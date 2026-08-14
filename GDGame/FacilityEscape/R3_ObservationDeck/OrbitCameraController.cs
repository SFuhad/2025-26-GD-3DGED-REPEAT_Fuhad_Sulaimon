using System;
using GDEngine.Core.Components;
using Microsoft.Xna.Framework;

namespace GDGame.FacilityEscape
{
    // orbits a fixed point at a constant radius/height, always looking at it. Not in the
    // engine anywhere - ThirdPersonController follows a moving target, it doesn't orbit a
    // fixed point, which is what "orbit around a central terminal" actually needs.
    public sealed class OrbitCameraController : Component
    {
        public Vector3 TargetPoint { get; set; }
        public float Radius { get; set; } = 8f;
        public float Height { get; set; } = 4f;
        public float SpeedRadPerSec { get; set; } = 0.4f;

        private float _angle;

        protected override void Update(float deltaTime)
        {
            _angle += SpeedRadPerSec * deltaTime;

            var position = TargetPoint
                + new Vector3(MathF.Cos(_angle), 0f, MathF.Sin(_angle)) * Radius
                + Vector3.Up * Height;

            Transform.TranslateTo(position);

            Vector3 forward = Vector3.Normalize(TargetPoint - position);
            Matrix worldMatrix = Matrix.CreateWorld(position, forward, Vector3.Up);
            Transform.RotateToWorld(Quaternion.CreateFromRotationMatrix(worldMatrix));
        }
    }
}
