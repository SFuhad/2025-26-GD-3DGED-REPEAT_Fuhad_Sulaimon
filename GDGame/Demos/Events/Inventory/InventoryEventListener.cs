#nullable enable
using GDEngine.Core.Components;
using GDEngine.Core.Services;
using System;
using System.Collections.Generic;

namespace GDGame.Demos
{
    // sits on the player object and listens for InventoryEvents, keeps a running count per item id
    // this is just a bare-bones version, no saving/loading, just an in-memory dictionary for now
    public sealed class InventoryEventListener : Component
    {
        #region Fields
        private IDisposable? _sub;
        private readonly Dictionary<string, int> _items = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Lifecycle Methods
        protected override void Awake()
        {
            if (EngineContext.Instance == null)
                throw new NullReferenceException(nameof(EngineContext));

            // subscribe on the bus - every InventoryEvent in the whole scene comes through here,
            // that's why we filter by e.Player below
            _sub = EngineContext.Instance.Events.Subscribe<InventoryEvent>(OnInventoryEvent);
        }

        protected override void OnDestroy()
        {
            // gotta unsubscribe or we leak the handler, learned this one the hard way
            _sub?.Dispose();
            _sub = null;
        }
        #endregion

        #region Methods
        private void OnInventoryEvent(InventoryEvent e)
        {
            // events for other players don't concern us
            if (e.Player != GameObject)
                return;

            if (e.IsAdd)
            {
                if (!_items.ContainsKey(e.ItemId))
                    _items[e.ItemId] = 0;

                _items[e.ItemId] += e.Quantity;
                System.Diagnostics.Debug.WriteLine($"[Inventory] +{e.Quantity} {e.ItemId} (total={_items[e.ItemId]})");
            }
            else
            {
                if (!_items.TryGetValue(e.ItemId, out var count))
                    return; // don't have it, nothing to remove

                var newCount = Math.Max(0, count - e.Quantity);
                if (newCount == 0) _items.Remove(e.ItemId); // clean up so the dict doesn't fill with zeros
                else _items[e.ItemId] = newCount;

                System.Diagnostics.Debug.WriteLine($"[Inventory] -{e.Quantity} {e.ItemId} (total={newCount})");
            }
        }
        #endregion
    }
}
