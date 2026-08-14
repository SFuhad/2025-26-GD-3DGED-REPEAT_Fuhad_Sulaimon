using GDEngine.Core.Components;
using GDEngine.Core.Timing;
using Microsoft.Xna.Framework;
using System;

namespace GDGame.Demos.Controllers
{
    // makes stuff pulse/breathe by scaling it up and down with a sine wave
    // mainly just made this to test if ScaleTo() worked, kept it as a demo
    public class SineScaleController : Component
    {
        private Vector3 _originalLocalScale;

        protected override void Update(float deltaTime)
        {
            float phase = (float)(Time.RealtimeSinceStartupSecs * 10); // 10 = speed, just picked something that looked good
            float scale = MathF.Sin(phase); // between -1 and 1
            Transform?.ScaleTo(_originalLocalScale
                + scale * new Vector3(0.25f, 1.25f, 0.25f) * 0.1f); // taller pulse on Y than X/Z, looks more "alive"
        }

        protected override void Awake()
        {
            if (Transform == null)
                throw new NullReferenceException(nameof(Transform));

            _originalLocalScale = Transform.LocalScale;
        }
    }
}
