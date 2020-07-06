using System;
using UnityEngine;
using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    [Serializable]
    public class Slot
    {
        [HideInInspector]
        public string Name = "Slot";

        public virtual PlayerItem PlayerItem
        {
            get;
            protected set;
        }
        protected virtual bool IsStackable
        {
            get;
            set;
        }
        protected int StackSize
        {
            get;
            set;
        }


        #region Constructors

        public Slot ()
        {
            PlayerItem = null;
            IsStackable = false;
            StackSize = 0;
        }

        public Slot ( PlayerItem playerItem )
        {
            if ( playerItem != null )
            {
                IsStackable = playerItem.StackLimit > 1;
            }
            StackSize = 0;
            Insert ( playerItem );
        }

        public Slot ( PlayerItem playerItem, int quantity )
        {
            if ( playerItem != null )
            {
                IsStackable = playerItem.StackLimit > 1;
            }
            StackSize = 0;
            Insert ( playerItem, quantity );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts one <paramref name="playerItem"/> into this Slot.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to be inserted into the Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public virtual InsertionResult Insert ( PlayerItem playerItem )
        {
            return Insert ( playerItem, 1 );
        }

        /// <summary>
        /// Inserts <paramref name="quantity"/> amount of <paramref name="playerItem"/> into this Slot.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to be inserted into the Slot.</param>
        /// <returns>Returns a SlotInsertionResult.</returns>
        public virtual InsertionResult Insert ( PlayerItem playerItem, int quantity )
        {
            if ( !IsValidPlayerItem ( playerItem ) ) // Check if playerItem type is valid
            {
                // Return an error SlotInsertionResult
                return new InsertionResult ( playerItem, InsertionResult.Results.INVALID_TYPE );
            }
            if ( quantity <= 0 ) // Check if quantity value is valid
            {
                // Return an error SlotInsertionResult
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            if ( IsAvailable ( playerItem ) ) // The Slot is available
            {
                if ( PlayerItem == null ) // If the slot is empty
                {
                    PlayerItem = playerItem;
                    IsStackable = playerItem.StackLimit > 1;
                }

                if ( quantity + StackSize > playerItem.StackLimit ) // Check if slot is full (overflow)
                {
                    if ( IsStackable ) // If playerItem is stackable
                    {
                        // Calculate overflow amount
                        int availableStackSpace = playerItem.StackLimit - StackSize;
                        int overflow = quantity - availableStackSpace;

                        // Fill slot stack completely
                        StackSize += availableStackSpace;

                        // Return an overflow SlotInsertionResult
                        return new InsertionResult ( playerItem, overflow );
                    }
                    // Return a slot full SlotInsertionResult
                    return new InsertionResult ( playerItem, InsertionResult.Results.SLOT_FULL );
                }
                else // Successful insertion
                {
                    // Check if this slot is empty
                    if ( PlayerItem == null )
                    {
                        PlayerItem = playerItem;
                    }

                    // Increase the slot stack volume by the new quantity
                    StackSize += quantity;

                    // Return a success SlotInsertionResult
                    return new InsertionResult ( playerItem, InsertionResult.Results.SUCCESS );
                }
            }
            return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
        }

        /// <summary>
        /// Removes one PlayerItem from the slot.
        /// </summary>
        /// <returns>RemovalResult containing PlayerItem removal information.</returns>
        public virtual RemovalResult Remove ()
        {
            return Remove ( 1 );
        }

        /// <summary>
        /// Removes <paramref name="removeAmount"/> PlayerItems from the stack.
        /// </summary>
        /// <param name="removeAmount"></param>
        /// <returns>RemovalResult containing PlayerItem removal information.</returns>
        public virtual RemovalResult Remove ( int removeAmount )
        {
            if ( PlayerItem == null ) // Empty slot
            {
                return new RemovalResult ( this, null );
            }
            // Reduce StackSize by removeAmount
            removeAmount = Mathf.Clamp ( removeAmount, 0, StackSize );
            PlayerItem contents = PlayerItem;
            StackSize -= removeAmount;
            if ( StackSize == 0 ) // Clear the slot if its stack size is 0
            {
                Clear ();
            }
            return new RemovalResult ( this, contents, removeAmount, RemovalResult.Results.SUCCESS );
        }

        /// <summary>
        /// Removes all PlayerItem stored in this Slot.
        /// Clears the slot afterwards.
        /// </summary>
        /// <returns>RemovalResult containing PlayerItem removal information.</returns>
        public virtual RemovalResult RemoveAll ()
        {
            if ( PlayerItem == null ) // Empty slot
            {
                return new RemovalResult ( this, null );
            }
            // Remove all
            int removalAmount = StackSize;
            PlayerItem contents = PlayerItem;
            // Clear the slot
            Clear ();
            return new RemovalResult ( this, contents, removalAmount, RemovalResult.Results.SUCCESS );
        }

        /// <summary>
        /// Removes half of the PlayerItems from the stack.
        /// </summary>
        /// <returns>RemovalResult containing PlayerItem removal information.</returns>
        public virtual RemovalResult RemoveHalf ()
        {
            if ( PlayerItem == null ) // Empty slot
            {
                return new RemovalResult ( this, null );
            }
            // Reduce StackSize by removeAmount
            int removeAmount = Mathf.CeilToInt ( StackSize / 2f );
            PlayerItem contents = PlayerItem;
            StackSize -= removeAmount;
            if ( StackSize == 0 ) // Clear the slot if its stack size is 0
            {
                Clear ();
            }
            return new RemovalResult ( this, contents, removeAmount, RemovalResult.Results.SUCCESS );
        }

        /// <summary>
        /// Indicates whether this Slot can allow <paramref name="playerItem"/>.
        /// </summary>
        /// <param name="playerItem"></param>
        /// <returns>True if this Slot does not contain a PlayerItem.
        /// Otherwise, compares <paramref name="playerItem"/>'s id
        /// to this Slot's PlayerItem id.</returns>
        public bool IsAvailable ( PlayerItem playerItem )
        {
            if ( playerItem == null ) // If playerItem is null
            {
                Debug.Assert ( playerItem != null, "IsSlotAvailable() -> playerItem is null" );
                return false;
            }
            if ( PlayerItem == null ) // If the Slot is empty
            {
                return true;
            }
            return ( !IsFull () && PlayerItem.Equals ( playerItem ) ); // Check if there's room and compare ids
        }

        /// <summary>
        /// Checks if this Slot is full.
        /// </summary>
        /// <returns>False if PlayerItem is null. Otherwise, checks if StackSize has reached the stack limit.</returns>
        public bool IsFull ()
        {
            if ( PlayerItem == null )
            {
                return false;
            }
            return StackSize >= PlayerItem.StackLimit;
        }

        /// <summary>
        /// Is this slot empty?
        /// </summary>
        /// <returns>True is there is no PlayerItem inserted into this slot.</returns>
        public bool IsEmpty ()
        {
            return PlayerItem == null;
        }

        protected virtual bool IsValidPlayerItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return false;
            }
            return playerItem.GetType () != typeof ( Weapon );
        }

        /// <summary>
        /// Resets the slot to empty. Sets StackSize to 0.
        /// </summary>
        public void Clear ()
        {
            PlayerItem = null;
            IsStackable = false;
            StackSize = 0;
        }

        public virtual void OnValidate ()
        {
            Name = ToString ();
        }

        #region Overrides

        public override string ToString ()
        {
            if ( PlayerItem != null )
            {
                return $"Slot - {PlayerItem.Name} - {( IsStackable ? $"[{StackSize}/{PlayerItem.StackLimit}]" : "Not Stackable" )}";
            }
            else
            {
                return "Slot - Empty";
            }
        }

        #endregion

        #endregion
    }
}

namespace InventorySystem.Slots.Results
{
    public class InsertionResult
    {
        public enum Results
        {
            SUCCESS,
            SLOT_FULL,
            OVERFLOW,
            INSERTION_FAILED,
            INVALID_TYPE
        }

        public PlayerItem Contents { get; private set; }
        public Results Result { get; private set; }
        public int OverflowAmount { get; private set; }

        public InsertionResult ( PlayerItem contents, Results result = Results.SUCCESS )
        {
            Contents = contents;
            Result = result;
            OverflowAmount = 0;
        }

        public InsertionResult ( PlayerItem contents, int overflowAmount )
        {
            Contents = contents;
            OverflowAmount = overflowAmount;
            Result = Results.OVERFLOW;
        }

        public override string ToString ()
        {
            if ( Result == Results.OVERFLOW )
            {
                return $"InsertionResult [{Result}] Contents [{Contents}] OverflowAmount [{OverflowAmount}]";
            }
            return $"InsertionResult [{Result}] Contents [{Contents}]";
        }
    }

    public class RemovalResult
    {
        public enum Results
        {
            SUCCESS,
            SLOT_EMPTY
        }

        /// <summary>
        /// The result of the removal.
        /// </summary>
        public Results Result { get; private set; }
        /// <summary>
        /// The Slot that had its contents removed from.
        /// </summary>
        public Slot Origin { get; private set; }
        /// <summary>
        /// The PlayerItem stored in the slot.
        /// </summary>
        public PlayerItem Contents { get; private set; }
        /// <summary>
        /// The amount of PlayerItems removed from the slot.
        /// </summary>
        public int RemoveAmount { get; private set; }

        public RemovalResult ( Slot slot, PlayerItem playerItem )
        {
            Origin = slot;
            Contents = playerItem;
            Result = ( playerItem == null ) ? Results.SLOT_EMPTY : Results.SUCCESS;
        }

        public RemovalResult ( Slot removalSlot, PlayerItem playerItem, int removalAmount, Results result )
        {
            Origin = removalSlot;
            Contents = playerItem;
            RemoveAmount = removalAmount;
            Result = result;
        }

        public override string ToString ()
        {
            if ( Result == Results.SUCCESS )
            {
                return $"RemovalResult [{Result}] Removed [{Contents.Name}] RemoveAmount [{RemoveAmount}]";
            }
            return $"RemovalResult [{Result}]";
        }
    }
}
