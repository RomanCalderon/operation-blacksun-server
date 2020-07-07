using System;
using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    [Serializable]
    public class StockSlot : AttachmentSlot
    {
        #region Constructors

        public StockSlot ( string id )
        {
            Id = id;
            PlayerItem = null;
        }

        public StockSlot ( string id, Stock stock )
        {
            Id = id;
            PlayerItem = stock;
        }

        #endregion

        /// <summary>
        /// Inserts <paramref name="stock"/> into this Slot.
        /// </summary>
        /// <param name="stock">The Stock to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem stock )
        {
            if ( !IsValidPlayerItem ( stock ) )
            {
                return new InsertionResult ( stock, InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( stock );
            }
            return new InsertionResult ( stock, InsertionResult.Results.SLOT_FULL );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem is Stock;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Attachment Slot (Stock) - {PlayerItem.Name}";
            }
            else
            {
                return "Attachment Slot (Stock) - Empty";
            }
        }
    }
}
