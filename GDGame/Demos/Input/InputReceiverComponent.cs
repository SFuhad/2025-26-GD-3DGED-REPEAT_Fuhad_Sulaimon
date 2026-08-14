using GDEngine.Core.Components;
using GDEngine.Core.Input.Data;
using GDEngine.Core.Input.Devices;
using GDEngine.Core.Systems;
using System.Diagnostics;
using System.Linq;

namespace GDGame.Demos
{
    // just a scratch component to test that InputSystem actually routes events to receivers -
    // spams the debug output whenever something happens on any device. not meant to ship,
    // more of a "does my input pipeline even work" sanity check
    public class InputReceiverComponent : Component, IInputReceiver
    {
        #region Static Fields
        // small deadzone so idle analog sticks/mouse jitter doesn't spam the console
        static readonly float INPUT_DEADZONE = 0.001f;
        #endregion

        #region Fields
        private InputSystem _input; // keep a ref around so we can unsubscribe later
        #endregion

        #region Methods
        // notes to self on how this whole thing works, for when I forget in 2 weeks:
        // step 1 - implement IInputReceiver on the class (done, see class def above)
        // step 2 - in Start(), find the InputSystem for this scene and register with it
        protected override void Start()
        {
            var scene = GameObject?.Scene;
            if (scene == null)
            {
                Debug.WriteLine("[DemoInputReceiverComponent] No scene found. Add one before using this demo.");
                return;
            }

            // step 3 - InputSystem should already be added to the scene (via InputSystem.CreateDefault())
            // before this component runs, otherwise we bail out below
            _input = scene.Systems.FirstOrDefault(s => s is InputSystem) as InputSystem;
            if (_input == null)
            {
                Debug.WriteLine("[DemoInputReceiverComponent] No InputSystem found in Scene. Add one before using this demo.");
                return;
            }

            _input.Add(this);
            Debug.WriteLine("[DemoInputReceiverComponent] Subscribed to InputSystem.");
        }

        // step 4 - and obviously unsubscribe when we're destroyed, otherwise dangling refs
        protected override void OnDestroy()
        {
            if (_input != null)
            {
                _input.Remove(this);
                Debug.WriteLine("[DemoInputReceiverComponent] Unsubscribed from InputSystem.");
            }
        }

        // called every frame there's axis movement - stuff like MoveX/MoveY (WASD/stick) or LookX/LookY (mouse/arrows)
        public void OnAxis(InputAction action, float value)
        {
            if (value > -INPUT_DEADZONE && value < INPUT_DEADZONE)
                return;

            Debug.WriteLine($"[Input AXIS] {action} = {value:0.###}");

            switch (action)
            {
                case InputAction.ScrollWheelDelta:
                    // could use this for camera zoom later
                    Debug.WriteLine($"[Input AXIS] {action} = {value:0.###}");
                    break;

                case InputAction.ScrollWheelValue:
                    // absolute scroll value, not delta - didn't end up needing this but leaving it in
                    Debug.WriteLine($"[Input AXIS] {action} = {value:0.###}");
                    break;

                    // MoveX/MoveY/LookX/LookY etc just fall through to the generic log above
            }
        }

        // fires once when a button first goes down. isFirstPress is false on OS key-repeat events
        public void OnButtonPressed(InputAction action, bool isFirstPress)
        {
            // if key-repeat spam becomes annoying just uncomment this:
            //if (!isFirstPress) return;

            Debug.WriteLine($"[Input DOWN] {action} (first:{isFirstPress})");
        }

        // fires once on release
        public void OnButtonReleased(InputAction action)
        {
            Debug.WriteLine($"[Input UP]   {action}");
        }
        #endregion

        #region Housekeeping Methods
        public override string ToString()
        {
            return "DemoInputReceiverComponent (logs device-agnostic input via InputSystem)";
        }
        #endregion
    }
}
