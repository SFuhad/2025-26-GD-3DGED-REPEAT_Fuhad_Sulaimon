using System.Collections.Generic;
using GDEngine.Core.Components;
using GDEngine.Core.Enums;
using GDEngine.Core.Systems;

namespace GDGame.Demos.Systems
{
    // WIP - was going to have this frustum-cull enemies that are off screen for a perf boost,
    // ran out of time so right now it just grabs transforms and does nothing with them
    public class EnemyCullingSystem : SystemBase
    {
        public EnemyCullingSystem(int order = 0)
            : base(FrameLifecycle.LateUpdate, order)
        {
        }

        protected override void OnAdded()
        {
            // TODO: this should probably filter by an "Enemy" tag/component instead of grabbing
            // every Transform in the scene, but that component doesn't exist yet
            var enemyComponents = new List<Component>();
            for (int i = 0; i < Scene.GameObjects.Count; i++)
            {
                var enemyComponent = Scene.GameObjects[i].GetComponent<Transform>();
                if (enemyComponent != null)
                    enemyComponents.Add(enemyComponent);
            }

            base.OnAdded();
        }
    }
}
