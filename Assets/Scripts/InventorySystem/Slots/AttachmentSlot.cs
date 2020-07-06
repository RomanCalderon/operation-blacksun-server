using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    public class AttachmentSlot : Slot
    {
        #region Constructors

        public AttachmentSlot ()
        {
            PlayerItem = null;
        }

        public AttachmentSlot ( Attachment attachment )
        {
            PlayerItem = attachment;
        }

        #endregion

        /// <summary>
        /// Inserts <paramref name="attachment"/> into this Slot.
        /// </summary>
        /// <param name="attachment">The Attachment to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem attachment )
        {
            if ( !IsValidPlayerItem ( attachment ) )
            {
                return new InsertionResult ( attachment, InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( attachment );
            }
            return new InsertionResult ( attachment, InsertionResult.Results.SLOT_FULL );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem is Attachment;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Attachment Slot - {PlayerItem.Name}";
            }
            else
            {
                return "Attachment Slot - Empty";
            }
        }
    }
}
