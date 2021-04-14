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
        private readonly Player player;
        private readonly InventoryManager inventoryManager;

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


        #region Constructors/Destructor

        public Inventory ( InventoryManager manager, Player player )
        {
            this.player = player;
            inventoryManager = manager;

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

            SendInitializedInventory ();
            OnValidate ();
        }

        private Inventory ( InventoryManager manager, Player player, Preset preset )
        {
            if ( manager == null || player == null || preset == null )
            {
                return;
            }
            this.player = player;
            inventoryManager = manager;

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
            }

            // Primary weapon
            m_primaryWeaponSlots = new WeaponSlots ( "primary-weapon", "primary-barrel", "primary-sight", "primary-magazine", "primary-stock",
                preset.PrimaryWeapon.Weapon, preset.PrimaryWeapon.Barrel, preset.PrimaryWeapon.Sight, preset.PrimaryWeapon.Magazine, preset.PrimaryWeapon.Stock );

            // Secondary weapon
            m_secondaryWeaponSlots = new WeaponSlots ( "secondary-weapon", "secondary-barrel", "secondary-sight", "secondary-magazine", "secondary-stock",
                preset.SecondaryWeapon.Weapon, preset.SecondaryWeapon.Barrel, preset.SecondaryWeapon.Sight, preset.SecondaryWeapon.Magazine, preset.SecondaryWeapon.Stock );

            SendInitializedInventory ();
            OnValidate ();
        }

        public static Inventory Create ( InventoryManager manager, Player player, Preset preset, Action<WeaponSlots> actionCallback )
        {
            Inventory result = new Inventory ( manager, player, preset );
            result.Initialized ( actionCallback );
            return result;
        }

        public virtual void Initialized ( Action<WeaponSlots> actionCallback )
        {
            actionCallback?.Invoke ( m_primaryWeaponSlots );
            actionCallback?.Invoke ( m_secondaryWeaponSlots );
        }

        #endregion

        #region Server Sends

        /// <summary>
        /// Sends via TCP the entire inventory (slots) to this clients' player.
        /// </summary>
        private void SendInitializedInventory ()
        {
            // Rig slots
            for ( int i = 0; i < m_rigSlots.Length; i++ )
            {
                if ( !m_rigSlots [ i ].IsEmpty () )
                {
                    string slotId = m_rigSlots [ i ].Id;
                    string playerItemId = m_rigSlots [ i ].PlayerItem.Id;
                    int quantity = m_rigSlots [ i ].StackSize;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, quantity );
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
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, quantity );
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
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.BarrelSlot != null )
                {
                    if ( m_primaryWeaponSlots.BarrelSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.BarrelSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.BarrelSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.SightSlot != null )
                {
                    if ( m_primaryWeaponSlots.SightSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.SightSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.SightSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.MagazineSlot != null )
                {
                    if ( m_primaryWeaponSlots.MagazineSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.MagazineSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.MagazineSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_primaryWeaponSlots.StockSlot != null )
                {
                    if ( m_primaryWeaponSlots.StockSlot.PlayerItem != null )
                    {
                        string slotId = m_primaryWeaponSlots.StockSlot.Id;
                        string playerItemId = m_primaryWeaponSlots.StockSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
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
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.BarrelSlot != null )
                {
                    if ( m_secondaryWeaponSlots.BarrelSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.BarrelSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.BarrelSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.SightSlot != null )
                {
                    if ( m_secondaryWeaponSlots.SightSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.SightSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.SightSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.MagazineSlot != null )
                {
                    if ( m_secondaryWeaponSlots.MagazineSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.MagazineSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.MagazineSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
                if ( m_secondaryWeaponSlots.StockSlot != null )
                {
                    if ( m_secondaryWeaponSlots.StockSlot.PlayerItem != null )
                    {
                        string slotId = m_secondaryWeaponSlots.StockSlot.Id;
                        string playerItemId = m_secondaryWeaponSlots.StockSlot.PlayerItem.Id;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                    }
                }
            }
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

            // Weapon swap
            if ( fromSlot is WeaponSlot && toSlot is WeaponSlot )
            {
                SwapWeapons ();
                return;
            }

            // Sight swap
            if ( fromSlot is SightSlot fromSightSlot && toSlot is SightSlot toSightSlot )
            {
                TransferSight ( fromSightSlot, toSightSlot );
                return;
            }

            // Magazine swap
            if ( fromSlot.PlayerItem is Magazine && toSlot is MagazineSlot magazineSlot )
            {
                TransferMagazine ( fromSlot, magazineSlot );
                return;
            }

            PlayerItem fromSlotItem = fromSlot.PlayerItem;
            if ( fromSlotItem != null )
            {
                // Removal
                RemovalResult removalResult = fromSlot.RemoveAll ();
                if ( removalResult.Result != RemovalResult.Results.SUCCESS )
                {
                    Debug.LogError ( $"RemoveAll() error - RemovalResult [{removalResult.Result}]" );
                    return;
                }

                // Insertion
                int transferQuantity = removalResult.RemoveAmount;
                InsertionResult insertionResult = toSlot.Insert ( fromSlotItem, transferQuantity );
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, fromSlotItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, 0 ); // fromSlot
                        break;
                    case InsertionResult.Results.SLOT_FULL: // Swap
                        fromSlot.Insert ( toSlot.PlayerItem, toSlot.StackSize );
                        toSlot.Clear ();
                        toSlot.Insert ( fromSlotItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItem.Id, fromSlot.StackSize ); // fromSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, toSlot.PlayerItem.Id, toSlot.StackSize ); // toSlot
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        fromSlot.Insert ( fromSlotItem, insertionResult.OverflowAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, fromSlotItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItem.Id, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        Debug.Log ( "Insertion failed" );
                        Debug.Log ( fromSlot.Insert ( fromSlotItem, removalResult.RemoveAmount ) );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItem.Id, fromSlot.StackSize ); // fromSlot
                        return;
                    case InsertionResult.Results.INVALID_TYPE:
                        Debug.Log ( "Invalid type" );
                        fromSlot.Insert ( fromSlotItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItem.Id, fromSlot.StackSize ); // fromSlot
                        return;
                    default:
                        Debug.LogError ( $"Unknown InsertionResult [{insertionResult.Result}]" );
                        break;
                }
                CheckWeaponSlotsUpdated ( removalResult.Origin );
                CheckWeaponSlotsUpdated ( insertionResult.Slot );
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
                RemovalResult removalResult = fromSlot.Remove (); // Removal
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

                InsertionResult insertionResult = toSlot.Insert ( playerItem ); // Insertion
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        string fromSlotPlayerItemId = fromSlot.PlayerItem ? fromSlot.PlayerItem.Id : string.Empty;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, toSlot.PlayerItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotPlayerItemId, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.SLOT_FULL:
                        Debug.Log ( "Slot full" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        fromSlot.Insert ( playerItem, insertionResult.OverflowAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, playerItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, playerItem.Id, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
                        return;
                    case InsertionResult.Results.INVALID_TYPE:
                        Debug.Log ( "Invalid type" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
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
                RemovalResult removalResult = fromSlot.RemoveHalf (); // Removal
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

                int transferQuantity = removalResult.RemoveAmount;
                InsertionResult insertionResult = toSlot.Insert ( playerItem, transferQuantity ); // Insertion
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        string fromSlotPlayerItemId = fromSlot.PlayerItem ? fromSlot.PlayerItem.Id : string.Empty;
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, toSlot.PlayerItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotPlayerItemId, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.SLOT_FULL:
                        Debug.Log ( "Slot full" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        Debug.Log ( $"Overflow - OverflowAmount [{insertionResult.OverflowAmount}]" );
                        fromSlot.Insert ( playerItem, insertionResult.OverflowAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, playerItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, playerItem.Id, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        Debug.Log ( "Insertion failed" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
                        return;
                    case InsertionResult.Results.INVALID_TYPE:
                        Debug.Log ( "Invalid type" );
                        fromSlot.Insert ( playerItem, removalResult.RemoveAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
                        return;
                    default:
                        Debug.LogError ( $"Unknown InsertionResult [{insertionResult.Result}]" );
                        return;
                }
            }
            OnValidate ();
        }

        /// <summary>
        /// Swaps the contents of <paramref name="fromSlot"/> and <paramref name="toSlot"/>, including their respective AttachmentSlots.
        /// </summary>
        /// <param name="fromSlot"></param>
        /// <param name="toSlot"></param>
        private void SwapWeapons ()
        {
            if ( m_primaryWeaponSlots == null || m_secondaryWeaponSlots == null )
            {
                Debug.Assert ( m_primaryWeaponSlots != null );
                Debug.Assert ( m_secondaryWeaponSlots != null );
                return;
            }

            // Move primary to empty secondary
            if ( !m_secondaryWeaponSlots.ContainsWeapon () )
            {
                m_secondaryWeaponSlots.AssignContents ( m_primaryWeaponSlots );
                m_primaryWeaponSlots.Clear ();
            }
            // Move secondary to empty primary
            else if ( !m_primaryWeaponSlots.ContainsWeapon () )
            {
                m_primaryWeaponSlots.AssignContents ( m_secondaryWeaponSlots );
                m_secondaryWeaponSlots.Clear ();
            }
            else // Swap primary and secondary
            {
                WeaponSlots tempSlots = new WeaponSlots ( m_primaryWeaponSlots );
                m_primaryWeaponSlots.AssignContents ( m_secondaryWeaponSlots );
                m_secondaryWeaponSlots.AssignContents ( tempSlots );
            }

            // Apply changes
            m_primaryWeaponSlots.Apply ( player.Id );
            m_secondaryWeaponSlots.Apply ( player.Id );
            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
            OnValidate ();
        }

        private void TransferSight ( SightSlot fromSlot, SightSlot toSlot )
        {
            if ( fromSlot == null || toSlot == null )
            {
                Debug.Assert ( fromSlot != null );
                Debug.Assert ( toSlot != null );
                return;
            }

            // Check if toSlot is associated with a weapon (Is primary WeaponSlot empty)
            if ( toSlot.Id.Contains ( "primary" ) )
            {
                if ( !m_primaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, toSlot );
                    OnValidate ();
                    return;
                }
            }
            // Check if toSlot is associated with a weapon (Is secondary WeaponSlot empty)
            if ( toSlot.Id.Contains ( "secondary" ) )
            {
                if ( !m_secondaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, toSlot );
                    OnValidate ();
                    return;
                }
            }

            // TODO: Check weapon/sight compatibility

            // Swap slot contents
            SwapItems ( fromSlot, toSlot );

            if ( fromSlot.Id.Contains ( "primary" ) || toSlot.Id.Contains ( "primary" ) )
            {
                Debug.Log ( "update primary weapon slot - sight changed" );
                inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
            }
            if ( fromSlot.Id.Contains ( "secondary" ) || toSlot.Id.Contains ( "secondary" ) )
            {
                Debug.Log ( "update secondary weapon slot - sight changed" );
                inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
            }
        }

        private void TransferMagazine ( Slot fromSlot, MagazineSlot magazineSlot )
        {
            if ( fromSlot == null || magazineSlot == null )
            {
                Debug.Assert ( fromSlot != null );
                Debug.Assert ( magazineSlot != null );
                return;
            }

            // Check if magazineSlot is associated with a weapon (Is weapon slot empty)
            if ( magazineSlot.Id.Contains ( "primary" ) )
            {
                if ( !m_primaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, magazineSlot );
                    OnValidate ();
                    return;
                }
            }
            if ( magazineSlot.Id.Contains ( "secondary" ) )
            {
                if ( !m_secondaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, magazineSlot );
                    OnValidate ();
                    return;
                }
            }

            // Get magazine caliber
            Magazine magazine = fromSlot.PlayerItem as Magazine;
            Ammunition.Calibers magazineCaliber = magazine.CompatibleAmmoCaliber;

            // Get magazine slot caliber
            Ammunition.Calibers magazineSlotCaliber = Ammunition.Calibers.AAC;
            if ( magazineSlot.Id.Contains ( "primary" ) )
            {
                magazineSlotCaliber = ( m_primaryWeaponSlots.WeaponSlot.PlayerItem as Weapon ).Caliber;
            }
            else if ( magazineSlot.Id.Contains ( "secondary" ) )
            {
                magazineSlotCaliber = ( m_secondaryWeaponSlots.WeaponSlot.PlayerItem as Weapon ).Caliber;
            }

            // Check for caliber compatibility
            if ( magazineCaliber == magazineSlotCaliber )
            {
                if ( magazineSlot.IsEmpty () ) // Empty magazine slot
                {
                    magazineSlot.Insert ( magazine );
                    fromSlot.Clear ();

                    ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, 1 ); // fromSlot
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, magazineSlot.Id, magazine.Id, 1 ); // magazineSlot
                    CheckWeaponSlotsUpdated ( magazineSlot );
                }
                else // Swap magazines
                {
                    SwapItems ( fromSlot, magazineSlot );
                }
            }
            else // Incompatible magazine
            {
                CancelSwap ( fromSlot, magazineSlot );
            }
            OnValidate ();
        }

        private void SwapItems ( Slot slotA, Slot slotB )
        {
            // Swaps both PlayerItems
            if ( !slotA.IsEmpty () && !slotB.IsEmpty () )
            {
                PlayerItem tempItem = slotB.PlayerItem;
                int tempSize = slotB.StackSize;
                slotB.Clear ();
                slotB.Insert ( slotA.PlayerItem, slotA.StackSize );
                slotA.Clear ();
                slotA.Insert ( tempItem, tempSize );
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotA.Id, slotA.PlayerItem.Id, slotA.StackSize ); // slotA
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotB.Id, slotB.PlayerItem.Id, slotB.StackSize ); // slotB
            }
            // Swaps slotA PlayerItem to empty slotB
            else if ( !slotA.IsEmpty () && slotB.IsEmpty () )
            {
                slotB.Insert ( slotA.PlayerItem, slotA.StackSize );
                slotA.Clear ();
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotA.Id, 0 ); // slotA
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotB.Id, slotB.PlayerItem.Id, slotB.StackSize ); // slotB
            }
            // Swaps slotB PlayerItem to empty slotA
            else if ( slotA.IsEmpty () && !slotB.IsEmpty () )
            {
                slotA.Insert ( slotB.PlayerItem, slotB.StackSize );
                slotB.Clear ();
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotA.Id, slotA.PlayerItem.Id, slotA.StackSize ); // slotA
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotB.Id, 0 ); // slotB
            }
            else
            {
                // Swapped two empty slots (invalid)
                Debug.LogError ( "Invalid swap ( slotA and slotB are empty)" );
            }
        }

        private void CancelSwap ( Slot fromSlot, Slot toSlot )
        {
            // From slot
            string fromSlotItemId = fromSlot.IsEmpty () ? string.Empty : fromSlot.PlayerItem.Id;
            ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItemId, 1 );

            // Magazine slot
            string magazineSlotItemId = toSlot.IsEmpty () ? string.Empty : toSlot.PlayerItem.Id;
            ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, magazineSlotItemId, 1 );
        }

        /// <summary>
        /// Invokes InventoryManager.OnWeaponSlotsUpdated() with the associated WeaponSlots based on updated slot <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot which was updated.</param>
        private void CheckWeaponSlotsUpdated ( Slot slot )
        {
            if ( slot is WeaponSlot || slot is AttachmentSlot )
            {
                Debug.Log ( $"CheckWeaponSlotsUpdated() - slot.Id={slot.Id}" );
                WeaponSlots weaponSlotsUpdated = slot.Id.Contains ( "primary" ) ? m_primaryWeaponSlots :
                    ( slot.Id.Contains ( "secondary" ) ? m_secondaryWeaponSlots : null );
                if ( weaponSlotsUpdated == null )
                {
                    Debug.LogError ( "weaponSlotsUpdated == null" );
                    return;
                }
                inventoryManager.OnWeaponSlotsUpdated.Invoke ( weaponSlotsUpdated );
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

        public WeaponSlot GetWeaponSlot ( string slotId )
        {
            // Search primary weapon slots
            if ( m_primaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_primaryWeaponSlots.GetSlot ( slotId ) as WeaponSlot;
            }

            // Search secondary weapon slots
            if ( m_secondaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_secondaryWeaponSlots.GetSlot ( slotId ) as WeaponSlot;
            }

            return null;
        }

        public int GetItemCount ( string playerItemId )
        {
            int count = 0;

            // Rig slots
            List<Slot> rigSlots = m_rigSlots.Where ( s => s.PlayerItem && s.PlayerItem.Id == playerItemId ).ToList ();
            foreach ( Slot slot in rigSlots )
            {
                count += slot.StackSize;
            }
            // Backpack slots
            List<Slot> backpackSlots = m_backpackSlots.Where ( s => s.PlayerItem && s.PlayerItem.Id == playerItemId ).ToList ();
            foreach ( Slot slot in backpackSlots )
            {
                count += slot.StackSize;
            }

            // Weapon slots
            count += m_primaryWeaponSlots.GetItemCount ( playerItemId );
            count += m_secondaryWeaponSlots.GetItemCount ( playerItemId );

            return count;
        }

        /// <summary>
        /// Reduces <paramref name="playerItemId"/> by <paramref name="reductionAmount"/> from the inventory.
        /// </summary>
        public void ReduceItem ( string playerItemId, int reductionAmount )
        {
            if ( string.IsNullOrEmpty ( playerItemId ) || reductionAmount <= 0 )
            {
                return;
            }

            // Keep track of amount of items reduced from slots
            int itemsReduced = 0;

            // Get all slots that contain PlayerItem with matching IDs
            List<Slot> slots = new List<Slot> ();
            slots.AddRange ( m_rigSlots.Where ( s => s.PlayerItem && s.PlayerItem.Id == playerItemId ) );
            slots.AddRange ( m_backpackSlots.Where ( s => s.PlayerItem && s.PlayerItem.Id == playerItemId ) );
            slots.AddRange ( m_primaryWeaponSlots.GetSlotsWithItem ( playerItemId ) );
            slots.AddRange ( m_secondaryWeaponSlots.GetSlotsWithItem ( playerItemId ) );

            // Sort the slots by stack size
            slots.Sort ( delegate ( Slot a, Slot b )
             {
                 if ( a.PlayerItem == null && b.PlayerItem == null ) return 0;
                 else if ( a.PlayerItem == null ) return -1;
                 else if ( b.PlayerItem == null ) return 1;
                 else return a.StackSize.CompareTo ( b.StackSize );
             } );

            // Create a queue
            Queue<Slot> slotQueue = new Queue<Slot> ( slots );
            List<Slot> affectedSlots = new List<Slot> ();

            while ( itemsReduced < reductionAmount )
            {
                if ( slotQueue.Count () == 0 )
                {
                    break;
                }
                Slot currentSlot = slotQueue.Dequeue ();
                int amountToRemove = reductionAmount - itemsReduced;
                RemovalResult result = currentSlot.Remove ( amountToRemove );
                if ( result.Result == RemovalResult.Results.SUCCESS )
                {
                    itemsReduced += result.RemoveAmount;
                    affectedSlots.Add ( currentSlot );
                }
            }

            // ServerSend the affected slots
            foreach ( Slot slot in affectedSlots )
            {
                // ID of slot's PlayerItem (empty if null)
                string itemId = slot.PlayerItem == null ? string.Empty : playerItemId;
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slot.Id, itemId, slot.StackSize );
            }
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
                slot = m_backpackSlots.FirstOrDefault ( s => s.IsAvailable ( playerItem ) && !s.IsFull () );

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
    }
}