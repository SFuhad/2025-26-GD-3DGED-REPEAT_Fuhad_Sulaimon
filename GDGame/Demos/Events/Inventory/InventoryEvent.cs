#nullable enable
using GDEngine.Core.Entities;
using System;

namespace GDGame.Demos
{
    // fired whenever the player picks up or drops/uses an item
    // publish one of these on the EventBus and whoever's listening (UI, save system etc) reacts to it
    public sealed class InventoryEvent
    {
        #region Properties
        public GameObject Player { get; }
        public string ItemId { get; }
        public bool IsAdd { get; }
        public int Quantity { get; }
        public GameObject? Source { get; }
        #endregion

        #region Constructors
        public InventoryEvent(GameObject player, string itemId, bool isAdd, int quantity = 1, GameObject? source = null)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            ItemId = itemId ?? throw new ArgumentNullException(nameof(itemId));
            IsAdd = isAdd;
            Quantity = Math.Max(1, quantity);
            Source = source;
        }
        #endregion
    }
}
