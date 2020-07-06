using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    public class MagazineSlot : AttachmentSlot
    {
        #region Constructors

        public MagazineSlot ()
        {
            PlayerItem = null;
        }

        public MagazineSlot ( Magazine magazine )
        {
            PlayerItem = magazine;
        }

        #endregion

        /// <summary>
        /// Inserts <paramref name="magazine"/> into this Slot.
        /// </summary>
        /// <param name="magazine">The Magazine to be inserted into this Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public override InsertionResult Insert ( PlayerItem magazine )
        {
            if ( !IsValidPlayerItem ( magazine ) )
            {
                return new InsertionResult ( magazine, InsertionResult.Results.INVALID_TYPE );
            }
            if ( IsEmpty () )
            {
                return base.Insert ( magazine );
            }
            return new InsertionResult ( magazine, InsertionResult.Results.SLOT_FULL );
        }

        protected override bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem is Magazine;
        }

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Attachment Slot (Magazine) - {PlayerItem.Name}";
            }
            else
            {
                return "Attachment Slot (Magazine) - Empty";
            }
        }
    }
}
