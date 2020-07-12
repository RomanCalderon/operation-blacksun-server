using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Presets;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;
using InventorySystem.PlayerItems;

namespace InventorySystem
{
    [Serializable]
    public class Inventory
    {
        private readonly Player m_player;

        // Rig
        [SerializeField]
        private Slot [] m_rigSlots;
        // Backpack
        [SerializeField]
        private Slot [] m_backpackSlots;
        // Primary weapon
        [SerializeField]
        private WeaponSlots m_primaryWeaponSlots;
        // Secondary weapon
        [SerializeField]
        private WeaponSlots m_secondaryWeaponSlots;


        #region Constructors

        public Inventory ( Player player )
        {
            m_player = player;

            // Initialize Rig slots
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                m_rigSlots [ i ] = new Slot ( "rig-" + i );
            }

            // Initialize Backpack slots
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                m_backpackSlots [ i ] = new Slot ( "backpack-" + i );
            }

            m_primaryWeaponSlots = new WeaponSlots ( "primary-weapon", "primary-barrel", "primary-sight", "primary-magazine", "primary-stock" );
            m_secondaryWeaponSlots = new WeaponSlots ( "secondary-weapon", "secondary-barrel", "secondary-sight", "secondary-magazine", "secondary-stock" );

            OnValidate ();
        }

        public Inventory ( Player player, Preset preset )
        {
            if ( preset == null )
            {
                return;
            }

            m_player = player;

            // Initialize Rig slots
            m_rigSlots = new Slot [ Constants.INVENTORY_RIG_SIZE ];
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( i < preset.RigItems.Length )
                {
                    m_rigSlots [ i ] = new Slot ( "rig-" + i, preset.RigItems [ i ].PlayerItem, preset.RigItems [ i ].Quantity );
                }
                else
                {
                    m_rigSlots [ i ] = new Slot ( "rig-" + i );
                }
            }
            // Initialize Backpack slots
            m_backpackSlots = new Slot [ Constants.INVENTORY_BACKPACK_SIZE ];
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                m_backpackSlots [ i ] = new Slot ( "backpack-" + i );
            }
            // Add PlayerItems to the backpack
            foreach ( PlayerItemPreset playerItemPreset in preset.BackpackItems )
            {
                AddToBackpackAny ( playerItemPreset.PlayerItem, playerItemPreset.Quantity );
                //Debug.Log ( AddToBackpackAny ( playerItemPreset.PlayerItem, playerItemPreset.Quantity ) );
            }

            // Primary weapon
            m_primaryWeaponSlots = new WeaponSlots ( "primary-weapon", "primary-barrel", "primary-sight", "primary-magazine", "primary-stock",
                preset.PrimaryWeapon.Weapon, preset.PrimaryWeapon.Barrel, preset.PrimaryWeapon.Sight, preset.PrimaryWeapon.Magazine, preset.PrimaryWeapon.Stock );

            // Secondary weapon
            m_secondaryWeaponSlots = new WeaponSlots ( "secondary-weapon", "secondary-barrel", "secondary-sight", "secondary-magazine", "secondary-stock",
                preset.SecondaryWeapon.Weapon, preset.SecondaryWeapon.Barrel, preset.SecondaryWeapon.Sight, preset.SecondaryWeapon.Magazine, preset.SecondaryWeapon.Stock );

            OnValidate ();
        }

        #endregion

        #region ServerSends

        /// <summary>
        /// Sends via TCP the entire inventory (slots) to this clients' player.
        /// </summary>
        public void SendInitializedInventory ()
        {
            // Rig slots
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( !m_rigSlots [ i ].IsEmpty () )
                {
                    string slotId = m_rigSlots [ i ].Id;
                    string playerItemId = m_rigSlots [ i ].PlayerItem.Id;
                    int quantity = m_rigSlots [ i ].StackSize;
                    ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, quantity );
                }
            }
            // Backpack slots
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                if ( !m_backpackSlots [ i ].IsEmpty () )
                {
                    string slotId = m_backpackSlots [ i ].Id;
                    string playerItemId = m_backpackSlots [ i ].PlayerItem.Id;
                    int quantity = m_backpackSlots [ i ].StackSize;
                    ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, quantity );
                }
            }
            // Primary weapon slots
            if ( m_primaryWeaponSlots != null )
            {
                if ( m_primaryWeaponSlots.WeaponSlot != null )
                {
                    if ( m_primaryWeaponSlots.WeaponSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.WeaponSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.WeaponSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.BarrelSlot != null )
                {
                    if ( m_primaryWeaponSlots.BarrelSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.BarrelSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.BarrelSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.SightSlot != null )
                {
                    if ( m_primaryWeaponSlots.SightSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.SightSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.SightSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.MagazineSlot != null )
                {
                    if ( m_primaryWeaponSlots.MagazineSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.MagazineSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.MagazineSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.StockSlot != null )
                {
                    if ( m_primaryWeaponSlots.StockSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.StockSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.StockSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
            }
            // Secondary weapon slots
            if ( m_secondaryWeaponSlots != null )
            {
                if ( m_secondaryWeaponSlots.WeaponSlot != null )
                {
                    if ( m_secondaryWeaponSlots.WeaponSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.WeaponSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.WeaponSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.BarrelSlot != null )
                {
                    if ( m_secondaryWeaponSlots.BarrelSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.BarrelSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.BarrelSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.SightSlot != null )
                {
                    if ( m_secondaryWeaponSlots.SightSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.SightSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.SightSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.MagazineSlot != null )
                {
                    if ( m_secondaryWeaponSlots.MagazineSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.MagazineSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.MagazineSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.StockSlot != null )
                {
                    if ( m_secondaryWeaponSlots.StockSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.StockSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.StockSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, slotId, playerItemId, 1 );
                    }
                }
            }
        }

        #endregion

        #region Slot access

        /// <summary>
        /// Get the slot with ID <paramref name="slotId"/>.
        /// </summary>
        /// <param name="slotId">The ID of the slot.</param>
        /// <returns>The Slot with ID <paramref name="slotId"/>.</returns>
        public Slot GetSlot ( string slotId )
        {
            // Search rig
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( m_rigSlots [ i ].Id == slotId )
                {
                    return m_rigSlots [ i ];
                }
            }

            // Search backpack
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                if ( m_backpackSlots [ i ].Id == slotId )
                {
                    return m_backpackSlots [ i ];
                }
            }

            // Search primary weapon slots
            if ( m_primaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_primaryWeaponSlots.GetSlot ( slotId );
            }

            // Search secondary weapon slots
            if ( m_secondaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_secondaryWeaponSlots.GetSlot ( slotId );
            }

            return null;
        }

        public void TransferContentsAll ( string fromSlotId, string toSlotId )
        {
            if ( string.IsNullOrEmpty ( fromSlotId ) || string.IsNullOrEmpty ( toSlotId ) )
            {
                Debug.Assert ( !string.IsNullOrEmpty ( fromSlotId ), "fromSlotId is null or empty." );
                Debug.Assert ( !string.IsNullOrEmpty ( toSlotId ), "toSlotId is null or empty." );
                return;
            }
            Slot fromSlot = GetSlot ( fromSlotId );
            Slot toSlot = GetSlot ( toSlotId );
            if ( fromSlot == null || toSlot == null )
            {
                Debug.Assert ( fromSlot != null, "fromSlot is null." );
                Debug.Assert ( toSlot != null, "toSlot is null." );
                return;
            }

            PlayerItem playerItem = fromSlot.PlayerItem;
            int quantity = fromSlot.StackSize;
            if ( playerItem != null )
            {
                // Remove the PlayerItem from the fromSlot
                RemovalResult removalResult = fromSlot.RemoveAll ();
                switch ( removalResult.Result )
                {
                    case RemovalResult.Results.SUCCESS:
                        break;
                    case RemovalResult.Results.SLOT_EMPTY:
                        Debug.LogError ( "fromSlot is empty." );
                        return;
                    default:
                        Debug.LogError ( $"Unknown RemovalResult [{removalResult.Result}]" );
                        return;
                }

                InsertionResult insertionResult = toSlot.Insert ( playerItem, quantity );
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, toSlot.Id, playerItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, fromSlot.Id, string.Empty, 0 ); // fromSlot
                        break;
                    case InsertionResult.Results.SLOT_FULL:
                        Debug.Log ( "Slot full" );
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        Debug.Log ( "Overflow" );
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        Debug.Log ( "Insertion failed" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize );
                        return;
                    case InsertionResult.Results.INVALID_TYPE:
                        Debug.Log ( "Invalid type" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize );
                        return;
                    default:
                        break;
                }
            }
            OnValidate ();
        }

        public void TransferContentsSingle ( string fromSlotId, string toSlotId )
        {
            if ( string.IsNullOrEmpty ( fromSlotId ) || string.IsNullOrEmpty ( toSlotId ) )
            {
                Debug.Assert ( !string.IsNullOrEmpty ( fromSlotId ), "fromSlotId is null or empty." );
                Debug.Assert ( !string.IsNullOrEmpty ( toSlotId ), "toSlotId is null or empty." );
                return;
            }
            Slot fromSlot = GetSlot ( fromSlotId );
            Slot toSlot = GetSlot ( toSlotId );
            if ( fromSlot == null || toSlot == null )
            {
                Debug.Assert ( fromSlot != null, "fromSlot is null." );
                Debug.Assert ( toSlot != null, "toSlot is null." );
                return;
            }

            PlayerItem playerItem = fromSlot.PlayerItem;
            if ( playerItem != null )
            {
                // Remove the PlayerItem from the fromSlot
                RemovalResult removalResult = fromSlot.Remove ();
                switch ( removalResult.Result )
                {
                    case RemovalResult.Results.SUCCESS:
                        break;
                    case RemovalResult.Results.SLOT_EMPTY:
                        Debug.LogError ( "fromSlot is empty." );
                        return;
                    default:
                        Debug.LogError ( $"Unknown RemovalResult [{removalResult.Result}]" );
                        return;
                }

                // Insert the PlayerItem into the toSlot
                InsertionResult insertionResult = toSlot.Insert ( playerItem );
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        string fromSlotPlayerItemId = fromSlot.PlayerItem ? fromSlot.PlayerItem.Id : string.Empty;
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, toSlot.Id, toSlot.PlayerItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, fromSlot.Id, fromSlotPlayerItemId, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.SLOT_FULL:
                        Debug.Log ( "Slot full" );
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        Debug.Log ( "Overflow" );
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        Debug.Log ( "Insertion failed" );
                        ServerSend.PlayerUpdateInventorySlot ( m_player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize );
                        return;
                    case InsertionResult.Results.INVALID_TYPE:
                        Debug.Log ( "Invalid type" );
                        return;
                    default:
                        Debug.LogError ( $"Unknown InsertionResult [{insertionResult.Result}]" );
                        return;
                }
            }
            OnValidate ();
        }

        public void TransferContentsHalf ( string fromSlotId, string toSlotId )
        {
            throw new NotImplementedException ( "TransferContentsHalf() not implemented" );

            if ( string.IsNullOrEmpty ( fromSlotId ) || string.IsNullOrEmpty ( toSlotId ) )
            {
                Debug.Assert ( !string.IsNullOrEmpty ( fromSlotId ), "fromSlotId is null or empty." );
                Debug.Assert ( !string.IsNullOrEmpty ( toSlotId ), "toSlotId is null or empty." );
                return;
            }
            Slot fromSlot = GetSlot ( fromSlotId );
            Slot toSlot = GetSlot ( toSlotId );
            if ( fromSlot == null || toSlot == null )
            {
                Debug.Assert ( fromSlot != null, "fromSlot is null." );
                Debug.Assert ( toSlot != null, "toSlot is null." );
                return;
            }

            PlayerItem playerItem = fromSlot.PlayerItem;
            int quantity = Mathf.CeilToInt ( fromSlot.StackSize / 2f );
            if ( playerItem != null && quantity > 0 )
            {
                InsertionResult insertionResult = toSlot.Insert ( playerItem, quantity );
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        break;
                    case InsertionResult.Results.SLOT_FULL:
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        break;
                    case InsertionResult.Results.INVALID_TYPE:
                        break;
                    default:
                        break;
                }
            }
            OnValidate ();
        }

        #region Rig

        /// <summary>
        /// Gets all occupied rig slot ids.
        /// </summary>
        /// <returns></returns>
        public List<string> GetOccupiedSlotsRig ()
        {
            List<string> ids = new List<string> ();

            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( !m_rigSlots [ i ].IsEmpty () )
                {
                    ids.Add ( m_rigSlots [ i ].Id );
                }
            }
            return ids;
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a rig slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The rig slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToRig ( PlayerItem playerItem, int quantity = 1 )
        {
            if ( playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = m_rigSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

            return slot.Insert ( playerItem, quantity );
        }

        /// <summary>
        /// Get the slot at <paramref name="index"/> in the rig.
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>The Slot at <paramref name="index"/> in the rig.</returns>
        public Slot GetFromRig ( int index )
        {
            index = Mathf.Clamp ( index, 0, m_rigSlots.Length - 1 );
            return m_rigSlots [ index ];
        }

        #endregion

        #region Backpack

        /// <summary>
        /// Gets all occupied backpack slot ids.
        /// </summary>
        /// <returns></returns>
        public List<string> GetOccupiedSlotsBackpack ()
        {
            List<string> ids = new List<string> ();

            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                if ( !m_backpackSlots [ i ].IsEmpty () )
                {
                    ids.Add ( m_backpackSlots [ i ].Id );
                }
            }
            return ids;
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into any available backpack slot or slots.
        /// </summary>
        /// <param name="playerItem">The PlayerItem being added to the backpack.</param>
        /// <param name="quantity">The amount of items being added.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpackAny ( PlayerItem playerItem, int quantity )
        {
            if ( playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = null;
            InsertionResult insertionResult = null;
            do
            {
                slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

                if ( slot != null )
                {
                    // Insert
                    insertionResult = slot.Insert ( playerItem, quantity );

                    // Update quantity
                    quantity = insertionResult.OverflowAmount;

                }
            } while ( slot != null && insertionResult.Result == InsertionResult.Results.OVERFLOW );

            return insertionResult;
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a backpack slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The backpack slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int quantity = 1 )
        {
            if ( playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }
            Slot slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) );

            if ( slot == null ) // No slots available
            {
                return new InsertionResult ( InsertionResult.Results.INSERTION_FAILED );
            }
            return slot.Insert ( playerItem, quantity );
        }

        /// <summary>
        /// Get the slot at <paramref name="index"/> in the backpack.
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>The Slot at <paramref name="index"/> in the backpack.</returns>
        public Slot GetFromBackpack ( int index )
        {
            index = Mathf.Clamp ( index, 0, m_backpackSlots.Length - 1 );
            return m_backpackSlots [ index ];
        }

        #endregion

        #endregion

        /// <summary>
        /// Updates the inspector when a change to the inventory is made.
        /// </summary>
        public void OnValidate ()
        {
            // Rig slots
            foreach ( Slot slot in m_rigSlots )
            {
                if ( slot != null )
                {
                    slot.OnValidate ();
                }
            }

            // Backpack slots
            foreach ( Slot slot in m_backpackSlots )
            {
                if ( slot != null )
                {
                    slot.OnValidate ();
                }
            }

            // Primary weapon and attachment slots
            if ( m_primaryWeaponSlots != null )
            {
                if ( m_primaryWeaponSlots.WeaponSlot != null )
                {
                    m_primaryWeaponSlots.WeaponSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.BarrelSlot != null )
                {
                    m_primaryWeaponSlots.BarrelSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.SightSlot != null )
                {
                    m_primaryWeaponSlots.SightSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.MagazineSlot != null )
                {
                    m_primaryWeaponSlots.MagazineSlot.OnValidate ();
                }
                if ( m_primaryWeaponSlots.StockSlot != null )
                {
                    m_primaryWeaponSlots.StockSlot.OnValidate ();
                }
            }

            // Secondary weapon and attachment slots
            if ( m_secondaryWeaponSlots != null )
            {
                if ( m_secondaryWeaponSlots.WeaponSlot != null )
                {
                    m_secondaryWeaponSlots.WeaponSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.BarrelSlot != null )
                {
                    m_secondaryWeaponSlots.BarrelSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.SightSlot != null )
                {
                    m_secondaryWeaponSlots.SightSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.MagazineSlot != null )
                {
                    m_secondaryWeaponSlots.MagazineSlot.OnValidate ();
                }
                if ( m_secondaryWeaponSlots.StockSlot != null )
                {
                    m_secondaryWeaponSlots.StockSlot.OnValidate ();
                }
            }
        }

        #region Models

        /// <summary>
        /// Weapon and attachment slots.
        /// </summary>
        [Serializable]
        public class WeaponSlots
        {
            public WeaponSlot WeaponSlot;
            public BarrelSlot BarrelSlot;
            public SightSlot SightSlot;
            public MagazineSlot MagazineSlot;
            public StockSlot StockSlot;

            public WeaponSlots ( string weaponSlotId, string barrelSlotId, string sightSlotId, string magazineSlotId, string stockSlotId )
            {
                WeaponSlot = new WeaponSlot ( weaponSlotId );
                BarrelSlot = new BarrelSlot ( barrelSlotId );
                SightSlot = new SightSlot ( sightSlotId );
                MagazineSlot = new MagazineSlot ( magazineSlotId );
                StockSlot = new StockSlot ( stockSlotId );
            }

            public WeaponSlots ( string weaponSlotId, string barrelSlotId, string sightSlotId, string magazineSlotId, string stockSlotId,
                Weapon weapon, Barrel barrel, Sight sight, Magazine magazine, Stock stock )
            {
                WeaponSlot = new WeaponSlot ( weaponSlotId, weapon );
                BarrelSlot = new BarrelSlot ( barrelSlotId, barrel );
                SightSlot = new SightSlot ( sightSlotId, sight );
                MagazineSlot = new MagazineSlot ( magazineSlotId, magazine );
                StockSlot = new StockSlot ( stockSlotId, stock );
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
        }

        #endregion
    }
}
