using GDEngine.Core.Components;

namespace GDGame.Demos.Components
{
    // WIP - haven't gotten around to actually making the wisp do anything yet
    public class WispController : Component
    {
        public int x; // placeholder, gonna rename these once I figure out what I actually need
        public bool y;

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            // TODO: wisp movement/flicker logic goes here
        }
    }
}
