using System;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;
using InventorySystem.Slots.Results;

namespace InventorySystem.Slots
{
    #region Slot

    [Serializable]
    public class Slot
    {
        [HideInInspector]
        public string Name = "Slot";
        public string Id { get; protected set; }

        public virtual PlayerItem PlayerItem
        {
            get;
            protected set;
        }
        public virtual bool IsStackable
        {
            get;
            protected set;
        }
        public int StackSize
        {
            get;
            protected set;
        }


        #region Constructors

        public Slot ()
        {
            Id = "unassigned";
        }

        public Slot ( string id )
        {
            Id = id;
            PlayerItem = null;
            IsStackable = false;
            StackSize = 0;
        }

        public Slot ( string id, PlayerItem playerItem )
        {
            Id = id;
            if ( playerItem != null )
            {
                IsStackable = playerItem.StackLimit > 1;
            }
            StackSize = 0;
            Insert ( playerItem );
        }

        public Slot ( string id, PlayerItem playerItem, int quantity )
        {
            Id = id;
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
                        return new InsertionResult ( this, overflow );
                    }
                    // Return a slot full SlotInsertionResult
                    return new InsertionResult ( playerItem, InsertionResult.Results.SLOT_FULL );
                }
                else // Successful insertion
                {
                    // Increase the slot stack volume by the new quantity
                    StackSize += quantity;

                    // Return a success SlotInsertionResult
                    return new InsertionResult ( this, InsertionResult.Results.SUCCESS );
                }
            }
            return new InsertionResult ( playerItem, InsertionResult.Results.SLOT_FULL );
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
            return ( PlayerItem.Equals ( playerItem ) ); // Check if there's room and compare ids
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

            // Weapon slot
            if ( this is WeaponSlot )
            {
                return playerItem is Weapon;
            }
            // Attachment slot
            if ( this is AttachmentSlot )
            {
                return playerItem is Attachment;
            }
            // Barrel slot
            if ( this is BarrelSlot )
            {
                return playerItem is Barrel;
            }
            // Sight slot
            if ( this is SightSlot )
            {
                return playerItem is Sight;
            }
            // Magazine slot
            if ( this is MagazineSlot )
            {
                return playerItem is Magazine;
            }
            // Stock slot
            if ( this is StockSlot )
            {
                return playerItem is Stock;
            }
            // Slot
            return !( playerItem is Weapon );
        }

        /// <summary>
        /// Resets the slot to empty. Sets StackSize to 0.
        /// </summary>
        public void Clear ()
        {
            PlayerItem = null;
            //IsStackable = false;
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
                return $"Slot [{Id}] - {PlayerItem.Name} - {( IsStackable ? $"[{StackSize}/{PlayerItem.StackLimit}]" : "Not Stackable" )}";
            }
            else
            {
                return $"Slot [{Id}] - Empty";
            }
        }

        public override bool Equals ( object obj )
        {
            return obj is Slot instance && PlayerItem.Equals ( instance.PlayerItem );
        }

        #endregion

        #endregion
    }

    #endregion

    #region WeaponSlots

    /// <summary>
    /// Weapon and attachment slots.
    /// </summary>
    [Serializable]
    public class WeaponSlots
    {
        #region Members

        public string Id;
        public WeaponSlot WeaponSlot;
        public BarrelSlot BarrelSlot;
        public SightSlot SightSlot;
        public MagazineSlot MagazineSlot;
        public StockSlot StockSlot;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs new WeaponSlots with provided slot IDs.
        /// </summary>
        /// <param name="weaponSlotId"></param>
        /// <param name="barrelSlotId"></param>
        /// <param name="sightSlotId"></param>
        /// <param name="magazineSlotId"></param>
        /// <param name="stockSlotId"></param>
        public WeaponSlots ( string weaponSlotId, string barrelSlotId, string sightSlotId, string magazineSlotId, string stockSlotId )
        {
            Id = weaponSlotId;
            WeaponSlot = new WeaponSlot ( weaponSlotId );
            BarrelSlot = new BarrelSlot ( barrelSlotId );
            SightSlot = new SightSlot ( sightSlotId );
            MagazineSlot = new MagazineSlot ( magazineSlotId );
            StockSlot = new StockSlot ( stockSlotId );
        }

        public WeaponSlots ( string weaponSlotId, string barrelSlotId, string sightSlotId, string magazineSlotId, string stockSlotId,
            Weapon weapon, Barrel barrel, Sight sight, Magazine magazine, Stock stock )
        {
            Id = weaponSlotId;
            WeaponSlot = new WeaponSlot ( weaponSlotId, weapon );
            BarrelSlot = new BarrelSlot ( barrelSlotId, barrel );
            SightSlot = new SightSlot ( sightSlotId, sight );
            MagazineSlot = new MagazineSlot ( magazineSlotId, magazine );
            StockSlot = new StockSlot ( stockSlotId, stock );
        }

        public WeaponSlots ( WeaponSlots other )
        {
            if ( other.WeaponSlot != null && !other.WeaponSlot.IsEmpty () ) // Weapon
            {
                WeaponSlot = new WeaponSlot ( other.WeaponSlot.Id, other.WeaponSlot.PlayerItem as Weapon );
            }
            if ( other.BarrelSlot != null && !other.BarrelSlot.IsEmpty () ) // Barrel
            {
                BarrelSlot = new BarrelSlot ( other.BarrelSlot.Id, other.BarrelSlot.PlayerItem as Barrel );
            }
            if ( other.SightSlot != null && !other.SightSlot.IsEmpty () ) // Sight
            {
                SightSlot = new SightSlot ( other.SightSlot.Id, other.SightSlot.PlayerItem as Sight );
            }
            if ( other.MagazineSlot != null && !other.MagazineSlot.IsEmpty () ) // Magazine
            {
                MagazineSlot = new MagazineSlot ( other.MagazineSlot.Id, other.MagazineSlot.PlayerItem as Magazine );
            }
            if ( other.StockSlot != null && !other.StockSlot.IsEmpty () ) // Stock
            {
                StockSlot = new StockSlot ( other.StockSlot.Id, other.StockSlot.PlayerItem as Stock );
            }
        }

        #endregion

        #region Contains

        public bool ContainsWeapon ()
        {
            return !WeaponSlot.IsEmpty ();
        }

        public bool ContainsBarrel ()
        {
            return !BarrelSlot.IsEmpty ();
        }

        public bool ContainsSight ()
        {
            return !SightSlot.IsEmpty ();
        }

        public bool ContainsMagazine ()
        {
            return !MagazineSlot.IsEmpty ();
        }

        public bool ContainsStock ()
        {
            return !StockSlot.IsEmpty ();
        }

        public bool ContainsSlot ( string slotId )
        {
            if ( WeaponSlot.Id == slotId )
            {
                return true;
            }
            if ( BarrelSlot.Id == slotId )
            {
                return true;
            }
            if ( SightSlot.Id == slotId )
            {
                return true;
            }
            if ( MagazineSlot.Id == slotId )
            {
                return true;
            }
            if ( StockSlot.Id == slotId )
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Assignments

        public void AssignContents ( Weapon weapon, Barrel barrel, Sight sight, Magazine magazine, Stock stock )
        {
            WeaponSlot.Insert ( weapon );
            BarrelSlot.Insert ( barrel );
            SightSlot.Insert ( sight );
            MagazineSlot.Insert ( magazine );
            StockSlot.Insert ( stock );
        }

        public void AssignContents ( WeaponSlots other )
        {
            if ( other == null )
            {
                return;
            }
            Clear ();

            if ( other.WeaponSlot != null && !other.WeaponSlot.IsEmpty () )
            {
                WeaponSlot.Insert ( other.WeaponSlot.PlayerItem );
            }
            if ( other.BarrelSlot != null && !other.BarrelSlot.IsEmpty () )
            {
                BarrelSlot.Insert ( other.BarrelSlot.PlayerItem );
            }
            if ( other.SightSlot != null && !other.SightSlot.IsEmpty () )
            {
                SightSlot.Insert ( other.SightSlot.PlayerItem );
            }
            if ( other.MagazineSlot != null && !other.MagazineSlot.IsEmpty () )
            {
                MagazineSlot.Insert ( other.MagazineSlot.PlayerItem );
            }
            if ( other.StockSlot != null && !other.StockSlot.IsEmpty () )
            {
                StockSlot.Insert ( other.StockSlot.PlayerItem );
            }
        }

        #endregion

        #region Accessors

        public Slot GetSlot ( string slotId )
        {
            if ( WeaponSlot.Id == slotId )
            {
                return WeaponSlot;
            }
            if ( BarrelSlot.Id == slotId )
            {
                return BarrelSlot;
            }
            if ( SightSlot.Id == slotId )
            {
                return SightSlot;
            }
            if ( MagazineSlot.Id == slotId )
            {
                return MagazineSlot;
            }
            if ( StockSlot.Id == slotId )
            {
                return StockSlot;
            }
            return null;
        }

        public List<Slot> GetSlotsWithItem ( string playerItemId )
        {
            List<Slot> slots = new List<Slot> ();

            if ( WeaponSlot.PlayerItem && WeaponSlot.PlayerItem.Id == playerItemId )
            {
                slots.Add ( WeaponSlot );
            }
            if ( BarrelSlot.PlayerItem && BarrelSlot.PlayerItem.Id == playerItemId )
            {
                slots.Add ( BarrelSlot );
            }
            if ( SightSlot.PlayerItem && SightSlot.PlayerItem.Id == playerItemId )
            {
                slots.Add ( SightSlot );
            }
            if ( MagazineSlot.PlayerItem && MagazineSlot.PlayerItem.Id == playerItemId )
            {
                slots.Add ( MagazineSlot );
            }
            if ( StockSlot.PlayerItem && StockSlot.PlayerItem.Id == playerItemId )
            {
                slots.Add ( StockSlot );
            }

            return slots;
        }

        public int GetItemCount ( string playerItemId )
        {
            int count = 0;

            if ( WeaponSlot.PlayerItem && WeaponSlot.PlayerItem.Id == playerItemId )
            {
                count++;
            }
            if ( BarrelSlot.PlayerItem && BarrelSlot.PlayerItem.Id == playerItemId )
            {
                count++;
            }
            if ( SightSlot.PlayerItem && SightSlot.PlayerItem.Id == playerItemId )
            {
                count++;
            }
            if ( MagazineSlot.PlayerItem && MagazineSlot.PlayerItem.Id == playerItemId )
            {
                count++;
            }
            if ( StockSlot.PlayerItem && StockSlot.PlayerItem.Id == playerItemId )
            {
                count++;
            }

            return count;
        }

        #endregion

        /// <summary>
        /// Sends current WeaponSlot data to the client.
        /// </summary>
        public void Apply ( int playerId )
        {
            // WeaponSlot
            if ( WeaponSlot.IsEmpty () )
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, WeaponSlot.Id );
            }
            else
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, WeaponSlot.Id, WeaponSlot.PlayerItem.Id, 1 );
            }

            // BarrelSlot
            if ( BarrelSlot.IsEmpty () )
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, BarrelSlot.Id );
            }
            else
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, BarrelSlot.Id, BarrelSlot.PlayerItem.Id, 1 );
            }

            // SightSlot
            if ( SightSlot.IsEmpty () )
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, SightSlot.Id );
            }
            else
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, SightSlot.Id, SightSlot.PlayerItem.Id, 1 );
            }

            // MagazineSlot
            if ( MagazineSlot.IsEmpty () )
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, MagazineSlot.Id );
            }
            else
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, MagazineSlot.Id, MagazineSlot.PlayerItem.Id, 1 );
            }

            // StockSlot
            if ( StockSlot.IsEmpty () )
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, StockSlot.Id );
            }
            else
            {
                ServerSend.PlayerUpdateInventorySlot ( playerId, StockSlot.Id, StockSlot.PlayerItem.Id, 1 );
            }
        }

        public RemovalResult [] Clear ()
        {
            List<RemovalResult> removalResults = new List<RemovalResult> ();

            if ( !WeaponSlot.IsEmpty () )
            {
                removalResults.Add ( new RemovalResult ( WeaponSlot, WeaponSlot.PlayerItem, WeaponSlot.StackSize, RemovalResult.Results.SUCCESS ) );
            }
            if ( !BarrelSlot.IsEmpty () )
            {
                removalResults.Add ( new RemovalResult ( BarrelSlot, BarrelSlot.PlayerItem, BarrelSlot.StackSize, RemovalResult.Results.SUCCESS ) );
            }
            if ( !SightSlot.IsEmpty () )
            {
                removalResults.Add ( new RemovalResult ( SightSlot, SightSlot.PlayerItem, SightSlot.StackSize, RemovalResult.Results.SUCCESS ) );
            }
            if ( !MagazineSlot.IsEmpty () )
            {
                removalResults.Add ( new RemovalResult ( MagazineSlot, MagazineSlot.PlayerItem, MagazineSlot.StackSize, RemovalResult.Results.SUCCESS ) );
            }
            if ( !StockSlot.IsEmpty () )
            {
                removalResults.Add ( new RemovalResult ( StockSlot, StockSlot.PlayerItem, StockSlot.StackSize, RemovalResult.Results.SUCCESS ) );
            }

            WeaponSlot.Clear ();
            BarrelSlot.Clear ();
            SightSlot.Clear ();
            MagazineSlot.Clear ();
            StockSlot.Clear ();

            return removalResults.ToArray ();
        }
    }

    #endregion
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

        public Slot Slot { get; private set; }
        public PlayerItem Contents { get; private set; }
        public Results Result { get; private set; }
        public int OverflowAmount { get; private set; }


        public InsertionResult ( Results result = Results.INSERTION_FAILED )
        {
            Slot = null;
            Contents = null;
            Result = result;
            OverflowAmount = 0;
        }

        public InsertionResult ( Slot slot, Results result = Results.SUCCESS )
        {
            Slot = slot;
            Contents = slot.PlayerItem;
            Result = result;
            OverflowAmount = 0;
        }

        public InsertionResult ( PlayerItem playerItem, Results result = Results.INSERTION_FAILED )
        {
            Contents = playerItem;
            Result = result;
            OverflowAmount = 0;
        }

        public InsertionResult ( Slot slot, int overflowAmount )
        {
            Slot = slot;
            Contents = slot.PlayerItem;
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
