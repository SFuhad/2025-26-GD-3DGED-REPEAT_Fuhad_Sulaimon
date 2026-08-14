using System.Collections.Generic;
using GDEngine.Core.Components;

namespace GDGame.FacilityEscape
{
    // TriggerEvent fires every single frame while two colliders are still overlapping -
    // there's no "OnTriggerEnter"/"OnTriggerExit" anywhere in the engine. So if you want
    // "walked onto the plate" to fire ONCE, not 60 times a second, you need something like
    // this tracking who's currently inside.
    //
    // Rule #1 for anything reacting to TriggerEvent in this project: always filter by
    // reference identity (evt.TriggerBody == _myRigidBody), never by name/tag, and always
    // debounce through something like this before actually reacting.
    public sealed class TriggerDebouncer
    {
        private readonly HashSet<RigidBody> _inside = new HashSet<RigidBody>();

        // returns true only on the false->true edge (first time this body shows up)
        public bool TryEnter(RigidBody body)
        {
            return _inside.Add(body);
        }

        public bool IsInside(RigidBody body)
        {
            return _inside.Contains(body);
        }

        // call this if you ever need "exit" behaviour - since there's no real exit event,
        // about the only reliable way to detect it is "stopped receiving TriggerEvents for
        // this body for a frame or two", which isn't implemented here - not needed for R1.
        public void ForceExit(RigidBody body)
        {
            _inside.Remove(body);
        }
    }
}
