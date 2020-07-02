using InventorySystem.PlayerItems;

namespace InventorySystem.Slots
{
    public class AttachmentSlot : Slot
    {
        public override PlayerItem PlayerItem
        {
            get;
            protected set;
        }

        protected override bool IsStackable
        {
            get;
            set;
        }

        #region Constructors

        public AttachmentSlot ()
        {
            PlayerItem = null;
            IsStackable = false;
            StackSize = 0;
        }

        public AttachmentSlot ( Attachment attachment )
        {
            PlayerItem = attachment;
            IsStackable = false;
            StackSize = 0;
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
                return new InsertionResult ( InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( attachment );
            }
            return new InsertionResult ( InsertionResult.Results.SLOT_FULL );
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
