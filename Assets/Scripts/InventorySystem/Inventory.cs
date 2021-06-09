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
        #region Members

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

        #endregion

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
                throw new NullReferenceException ( $"manager, player or preset is null." );
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
                AddToBackpack ( playerItemPreset.PlayerItem, playerItemPreset.Quantity );
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
                string slotId = m_rigSlots [ i ].Id;
                if ( !m_rigSlots [ i ].IsEmpty () )
                {
                    string playerItemId = m_rigSlots [ i ].PlayerItem.Id;
                    int quantity = m_rigSlots [ i ].StackSize;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, quantity );
                }
                else
                {
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
            }
            // Backpack slots
            for ( int i = 0; i < m_backpackSlots.Length; i++ )
            {
                string slotId = m_backpackSlots [ i ].Id;
                if ( !m_backpackSlots [ i ].IsEmpty () )
                {
                    string playerItemId = m_backpackSlots [ i ].PlayerItem.Id;
                    int quantity = m_backpackSlots [ i ].StackSize;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, quantity );
                }
                else
                {
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
            }
            // Primary weapon slots
            if ( m_primaryWeaponSlots != null )
            {
                if ( !m_primaryWeaponSlots.WeaponSlot.IsEmpty () )
                {
                    string slotId = m_primaryWeaponSlots.WeaponSlot.Id;
                    string playerItemId = m_primaryWeaponSlots.WeaponSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_primaryWeaponSlots.WeaponSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_primaryWeaponSlots.BarrelSlot.IsEmpty () )
                {
                    string slotId = m_primaryWeaponSlots.BarrelSlot.Id;
                    string playerItemId = m_primaryWeaponSlots.BarrelSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_primaryWeaponSlots.BarrelSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_primaryWeaponSlots.SightSlot.IsEmpty () )
                {
                    string slotId = m_primaryWeaponSlots.SightSlot.Id;
                    string playerItemId = m_primaryWeaponSlots.SightSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_primaryWeaponSlots.SightSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_primaryWeaponSlots.MagazineSlot.IsEmpty () )
                {
                    string slotId = m_primaryWeaponSlots.MagazineSlot.Id;
                    string playerItemId = m_primaryWeaponSlots.MagazineSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_primaryWeaponSlots.MagazineSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_primaryWeaponSlots.StockSlot.IsEmpty () )
                {
                    string slotId = m_primaryWeaponSlots.StockSlot.Id;
                    string playerItemId = m_primaryWeaponSlots.StockSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_primaryWeaponSlots.StockSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
            }
            // Secondary weapon slots
            if ( m_secondaryWeaponSlots != null )
            {
                if ( !m_secondaryWeaponSlots.WeaponSlot.IsEmpty () )
                {
                    string slotId = m_secondaryWeaponSlots.WeaponSlot.Id;
                    string playerItemId = m_secondaryWeaponSlots.WeaponSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_secondaryWeaponSlots.WeaponSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_secondaryWeaponSlots.BarrelSlot.IsEmpty () )
                {
                    string slotId = m_secondaryWeaponSlots.BarrelSlot.Id;
                    string playerItemId = m_secondaryWeaponSlots.BarrelSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_secondaryWeaponSlots.BarrelSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_secondaryWeaponSlots.SightSlot.IsEmpty () )
                {
                    string slotId = m_secondaryWeaponSlots.SightSlot.Id;
                    string playerItemId = m_secondaryWeaponSlots.SightSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_secondaryWeaponSlots.SightSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_secondaryWeaponSlots.MagazineSlot.IsEmpty () )
                {
                    string slotId = m_secondaryWeaponSlots.MagazineSlot.Id;
                    string playerItemId = m_secondaryWeaponSlots.MagazineSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_secondaryWeaponSlots.MagazineSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
                }
                if ( !m_secondaryWeaponSlots.StockSlot.IsEmpty () )
                {
                    string slotId = m_secondaryWeaponSlots.StockSlot.Id;
                    string playerItemId = m_secondaryWeaponSlots.StockSlot.PlayerItem.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId, playerItemId, 1 );
                }
                else
                {
                    string slotId = m_secondaryWeaponSlots.StockSlot.Id;
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slotId );
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

            // Attachment transfer
            if ( fromSlot is AttachmentSlot || toSlot is AttachmentSlot )
            {
                TransferAttachments ( fromSlot, toSlot );
                return;
            }

            PlayerItem fromSlotItem = fromSlot.PlayerItem;
            if ( fromSlotItem != null )
            {
                // Removal
                RemovalResult removalResult = fromSlot.RemoveAll ();
                if ( removalResult.Result != RemovalResult.Results.SUCCESS )
                {
                    Debug.LogError ( $"fromSlot.RemoveAll() error - RemovalResult [{removalResult.Result}]" );
                    return;
                }

                // Insertion
                InsertionResult insertionResult = toSlot.Insert ( fromSlotItem, removalResult.RemoveAmount );
                switch ( insertionResult.Result )
                {
                    case InsertionResult.Results.SUCCESS:
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, fromSlotItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id ); // fromSlot
                        break;
                    case InsertionResult.Results.SLOT_FULL: // Swap
                        if ( fromSlot is BarrelSlot || fromSlot is SightSlot || fromSlot is MagazineSlot || fromSlot is StockSlot )
                        {
                            fromSlot.Insert ( removalResult.Contents );
                            TransferContentsAll ( toSlot.Id, fromSlot.Id );
                        }
                        else
                        {
                            fromSlot.Insert ( toSlot.PlayerItem, toSlot.StackSize );
                            toSlot.Clear ();
                            toSlot.Insert ( fromSlotItem, removalResult.RemoveAmount );
                            ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, toSlot.PlayerItem.Id, toSlot.StackSize ); // toSlot
                            ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlot.PlayerItem.Id, fromSlot.StackSize ); // fromSlot
                        }
                        break;
                    case InsertionResult.Results.OVERFLOW:
                        fromSlot.Insert ( fromSlotItem, insertionResult.OverflowAmount );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, fromSlotItem.Id, toSlot.StackSize ); // toSlot
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItem.Id, fromSlot.StackSize ); // fromSlot
                        break;
                    case InsertionResult.Results.INSERTION_FAILED:
                        Debug.Log ( fromSlot.Insert ( fromSlotItem, removalResult.RemoveAmount ) );
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, fromSlot.Id, fromSlotItem.Id, fromSlot.StackSize ); // fromSlot
                        return;
                    case InsertionResult.Results.INVALID_TYPE:
                        fromSlot.Insert ( fromSlotItem, removalResult.RemoveAmount );
                        if ( toSlot.IsEmpty () )
                            ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id ); // toSlot
                        else
                            ServerSend.PlayerUpdateInventorySlot ( player.Id, toSlot.Id, toSlot.PlayerItem.Id, toSlot.StackSize ); // toSlot

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

                InsertionResult insertionResult = toSlot.Insert ( playerItem, removalResult.RemoveAmount ); // Insertion
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

        private void TransferAttachments ( Slot fromSlot, Slot toSlot )
        {
            // Check for 1-to-1 attachment swapping (AttachmentSlot to AttachmentSlot)
            if ( fromSlot is BarrelSlot fromBarrelSlot && toSlot is BarrelSlot toBarrelSlot )
            {
                SwapAttachments<Barrel> ( fromBarrelSlot, toBarrelSlot );
            }
            else if ( fromSlot is SightSlot fromSightSlot && toSlot is SightSlot toSightSlot )
            {
                SwapAttachments<Sight> ( fromSightSlot, toSightSlot );
            }
            else if ( fromSlot is MagazineSlot fromMagazineSlot && toSlot is MagazineSlot toMagazineSlot )
            {
                SwapAttachments<Magazine> ( fromMagazineSlot, toMagazineSlot );
            }
            else if ( fromSlot is StockSlot fromStockSlot && toSlot is StockSlot toStockSlot )
            {
                SwapAttachments<Stock> ( fromStockSlot, toStockSlot );
            }
            else
            {
                // At this point, either fromSlot or toSlot is not an AttachmentSlot.
                // Determine which slot is an AttachmentSlot, perform the proper
                // compatibility check, then transfer the attachment.

                // From non-AttachmentSlot to AttachmentSlot
                if ( !( fromSlot is AttachmentSlot ) && toSlot is AttachmentSlot toAttachmentSlot )
                {
                    switch ( toAttachmentSlot )
                    {
                        case BarrelSlot barrelSlot:
                            TransferAttachment<Barrel> ( fromSlot, barrelSlot );
                            break;
                        case SightSlot sightSlot:
                            TransferAttachment<Sight> ( fromSlot, sightSlot );
                            break;
                        case MagazineSlot magazineSlot:
                            TransferAttachment<Magazine> ( fromSlot, magazineSlot );
                            break;
                        case StockSlot stockSlot:
                            TransferAttachment<Stock> ( fromSlot, stockSlot );
                            break;
                        default:
                            throw new NotImplementedException ( $"Unknown AttachmentSlot [{toAttachmentSlot}]" );
                    }
                }
                // From AttachmentSlot to non-AttachmentSlot
                else if ( fromSlot is AttachmentSlot fromAttachmentSlot && !( toSlot is AttachmentSlot ) )
                {
                    switch ( fromAttachmentSlot )
                    {
                        case BarrelSlot barrelSlot:
                            TransferAttachment<Barrel> ( barrelSlot, toSlot );
                            break;
                        case SightSlot sightSlot:
                            TransferAttachment<Sight> ( sightSlot, toSlot );
                            break;
                        case MagazineSlot magazineSlot:
                            TransferAttachment<Magazine> ( magazineSlot, toSlot );
                            break;
                        case StockSlot stockSlot:
                            TransferAttachment<Stock> ( stockSlot, toSlot );
                            break;
                        default:
                            throw new NotImplementedException ( $"Unknown AttachmentSlot [{fromAttachmentSlot}]" );
                    }
                }
            }
        }

        private void SwapAttachments<T> ( AttachmentSlot fromSlot, AttachmentSlot toSlot ) where T : Attachment
        {
            if ( fromSlot == null || toSlot == null )
            {
                Debug.Assert ( fromSlot != null );
                Debug.Assert ( toSlot != null );
                return;
            }

            // Check if toSlot is associated with a weapon
            if ( toSlot.Id.Contains ( "primary" ) )
            {
                // Missing weapon check
                if ( !m_primaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, toSlot );
                    OnValidate ();
                    return;
                }
                // Primary weapon-attachment compatibility check
                if ( !m_primaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( fromSlot.PlayerItem as T ) )
                {
                    CancelSwap ( fromSlot, toSlot );
                    OnValidate ();
                    return;
                }
                // Secondary weapon-attachment compatibility check
                else if ( !m_secondaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( toSlot.PlayerItem as T ) )
                {
                    // Check if inventory can hold the attachment
                    InsertionResult backpackResult = AddToBackpack ( toSlot.PlayerItem );
                    toSlot.Clear ();
                    if ( backpackResult.Result != InsertionResult.Results.SUCCESS )
                    {
                        // Drop attachment
                        inventoryManager.DropItem ( backpackResult.Contents );
                    }
                }
            }
            // Check if toSlot is associated with a weapon
            else if ( toSlot.Id.Contains ( "secondary" ) )
            {
                // Missing weapon check
                if ( !m_secondaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, toSlot );
                    OnValidate ();
                    return;
                }
                // Secondary weapon-attachment compatibility check
                if ( !m_secondaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( fromSlot.PlayerItem as T ) )
                {
                    CancelSwap ( fromSlot, toSlot );
                    OnValidate ();
                    return;
                }
                // Primary weapon-attachment compatibility check
                else if ( !m_primaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( toSlot.PlayerItem as T ) )
                {
                    // Check if inventory can hold the attachment
                    InsertionResult backpackResult = AddToBackpack ( toSlot.PlayerItem );
                    toSlot.Clear ();
                    if ( backpackResult.Result != InsertionResult.Results.SUCCESS )
                    {
                        // Drop attachment
                        inventoryManager.DropItem ( backpackResult.Contents );
                    }
                }
            }

            // Swap slot contents
            SwapItems ( fromSlot, toSlot );

            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
            OnValidate ();
        }

        private void TransferAttachment<T> ( Slot fromSlot, AttachmentSlot toAttachmentSlot ) where T : Attachment
        {
            if ( fromSlot.IsEmpty () )
            {
                CancelSwap ( fromSlot, toAttachmentSlot );
                OnValidate ();
                return;
            }

            // Check if toAttachmentSlot is associated with a weapon
            if ( toAttachmentSlot.Id.Contains ( "primary" ) )
            {
                // Missing weapon check
                if ( !m_primaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, toAttachmentSlot );
                    OnValidate ();
                    return;
                }
                // Weapon-Attachment compatibility check
                if ( !m_primaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( fromSlot.PlayerItem as T ) )
                {
                    CancelSwap ( fromSlot, toAttachmentSlot );
                    OnValidate ();
                    return;
                }
            }
            // Check if toAttachmentSlot is associated with a weapon
            else if ( toAttachmentSlot.Id.Contains ( "secondary" ) )
            {
                // Missing weapon check
                if ( !m_secondaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromSlot, toAttachmentSlot );
                    OnValidate ();
                    return;
                }
                // Weapon-Attachment compatibility check
                if ( !m_secondaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( fromSlot.PlayerItem as T ) )
                {
                    CancelSwap ( fromSlot, toAttachmentSlot );
                    OnValidate ();
                    return;
                }
            }

            // Swap slot contents
            SwapItems ( fromSlot, toAttachmentSlot );

            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
            OnValidate ();
        }

        private void TransferAttachment<T> ( AttachmentSlot fromAttachmentSlot, Slot toSlot ) where T : Attachment
        {
            if ( fromAttachmentSlot.IsEmpty () )
            {
                CancelSwap ( fromAttachmentSlot, toSlot );
                OnValidate ();
                return;
            }

            // Check if fromAttachmentSlot is associated with a weapon
            if ( fromAttachmentSlot.Id.Contains ( "primary" ) )
            {
                // Missing weapon check
                if ( !m_primaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromAttachmentSlot, toSlot );
                    OnValidate ();
                    return;
                }
                // Weapon-Attachment compatibility check
                if ( !toSlot.IsEmpty () )
                {
                    if ( !m_primaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( toSlot.PlayerItem as T ) )
                    {
                        CancelSwap ( fromAttachmentSlot, toSlot );
                        OnValidate ();
                        return;
                    }
                }
            }
            // Check if fromAttachmentSlot is associated with a weapon
            else if ( fromAttachmentSlot.Id.Contains ( "secondary" ) )
            {
                // Missing weapon check
                if ( !m_secondaryWeaponSlots.ContainsWeapon () )
                {
                    CancelSwap ( fromAttachmentSlot, toSlot );
                    OnValidate ();
                    return;
                }
                // Weapon-Attachment compatibility check
                if ( !toSlot.IsEmpty () )
                {
                    if ( !m_secondaryWeaponSlots.WeaponSlot.Weapon.IsCompatibleAttachment ( toSlot.PlayerItem as T ) )
                    {
                        CancelSwap ( fromAttachmentSlot, toSlot );
                        OnValidate ();
                        return;
                    }
                }
            }

            SwapItems ( fromAttachmentSlot, toSlot );

            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
            inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
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
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotA.Id, slotA.PlayerItem.Id, slotA.StackSize ); // slotA - insert
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotB.Id, slotB.PlayerItem.Id, slotB.StackSize ); // slotB - insert
            }
            // Swaps slotA PlayerItem to empty slotB
            else if ( !slotA.IsEmpty () && slotB.IsEmpty () )
            {
                slotB.Insert ( slotA.PlayerItem, slotA.StackSize );
                slotA.Clear ();
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotA.Id ); // slotA - clear
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotB.Id, slotB.PlayerItem.Id, slotB.StackSize ); // slotB - insert
            }
            // Swaps slotB PlayerItem to empty slotA
            else if ( slotA.IsEmpty () && !slotB.IsEmpty () )
            {
                slotA.Insert ( slotB.PlayerItem, slotB.StackSize );
                slotB.Clear ();
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotA.Id, slotA.PlayerItem.Id, slotA.StackSize ); // slotA - insert
                ServerSend.PlayerUpdateInventorySlot ( player.Id, slotB.Id ); // slotB - clear
            }
            else
            {
                // Swapped two empty slots (invalid)
                Debug.LogError ( $"Invalid swap ( slotA [{slotA}] and slotB [{slotB}] are empty)" );
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

        public T [] GetFromInventory<T> ( bool includeWeaponSlots = true ) where T : PlayerItem
        {
            List<T> results = new List<T> ();
            results.AddRange ( GetFromRig<T> () );
            results.AddRange ( GetFromBackpack<T> () );
            if ( includeWeaponSlots )
            {
                results.AddRange ( GetFromWeaponSlots<T> ( Weapons.Primary ) );
                results.AddRange ( GetFromWeaponSlots<T> ( Weapons.Secondary ) );
            }
            return results.ToArray ();
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

            // Keep track of amount of items reduced from slots
            int itemsReduced = 0;

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

        #region Item Removal

        public RemovalResult [] RemoveItem ( string slotId, int transferMode )
        {
            if ( string.IsNullOrEmpty ( slotId ) )
            {
                return null;
            }

            Slot removalSlot = GetSlot ( slotId );
            RemovalResult removalResult = null;

            // Weapon removal (includes attachments)
            if ( removalSlot is WeaponSlot weaponSlot )
            {
                return RemoveWeapon ( weaponSlot );
            }
            else if ( removalSlot is AttachmentSlot attachmentSlot )
            {
                return new RemovalResult [] { RemoveAttachment ( attachmentSlot ) };
            }

            switch ( transferMode )
            {
                case 0: // Remove ALL
                    removalResult = removalSlot.RemoveAll ();
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, removalSlot.Id );
                    break;
                case 1: // Remove ONE
                    removalResult = removalSlot.Remove ();
                    if ( removalSlot.IsEmpty () )
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, removalSlot.Id );
                    else
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, removalSlot.Id, removalSlot.PlayerItem.Id, removalSlot.StackSize );
                    break;
                case 2: // Remove HALF
                    removalResult = removalSlot.RemoveHalf ();
                    if ( removalSlot.IsEmpty () )
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, removalSlot.Id );
                    else
                        ServerSend.PlayerUpdateInventorySlot ( player.Id, removalSlot.Id, removalSlot.PlayerItem.Id, removalSlot.StackSize );
                    break;
                default:
                    break;
            }
            OnValidate ();
            return new RemovalResult [] { removalResult };
        }

        public RemovalResult RemoveItem ( PlayerItem playerItem )
        {
            if ( playerItem == null )
            {
                return null;
            }

            List<string> slotIds = new List<string> ();
            slotIds.AddRange ( GetOccupiedSlotsBackpack () );
            slotIds.AddRange ( GetOccupiedSlotsRig () );
            foreach ( string slotId in slotIds )
            {
                Slot slot = GetSlot ( slotId );
                if ( slot != null && slot.PlayerItem == playerItem )
                {
                    return RemoveItem ( slotId, 0 ) [ 0 ];
                }
            }
            return null;
        }

        /// <summary>
        /// Removes a specified weapon and all of its attachments
        /// from the player's inventory.
        /// </summary>
        /// <param name="weaponSlots"></param>
        private RemovalResult [] RemoveWeapon ( WeaponSlot weaponSlot )
        {
            RemovalResult [] removalResults = null;
            if ( weaponSlot.Id.Contains ( "primary" ) )
            {
                // Clear all slots
                removalResults = m_primaryWeaponSlots.Clear ();
                // Apply changes
                m_primaryWeaponSlots.Apply ( player.Id );
                inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
            }
            else if ( weaponSlot.Id.Contains ( "secondary" ) )
            {
                // Clear all slots
                removalResults = m_secondaryWeaponSlots.Clear ();
                // Apply changes
                m_secondaryWeaponSlots.Apply ( player.Id );
                inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
            }
            OnValidate ();
            return removalResults;
        }

        private RemovalResult RemoveAttachment ( AttachmentSlot attachmentSlot )
        {
            RemovalResult removalResult = attachmentSlot.Remove ();
            ServerSend.PlayerUpdateInventorySlot ( player.Id, attachmentSlot.Id );
            return removalResult;
        }

        #endregion

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

        public T [] GetFromRig<T> () where T : PlayerItem
        {
            List<T> results = new List<T> ();
            foreach ( Slot slot in m_rigSlots )
            {
                if ( !slot.IsEmpty () && slot.PlayerItem is T )
                {
                    results.Add ( slot.PlayerItem as T );
                }
            }
            return results.ToArray ();
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
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int quantity = 1 )
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
                    ServerSend.PlayerUpdateInventorySlot ( player.Id, slot.Id, playerItem.Id, slot.StackSize );

                    // Update quantity
                    quantity = insertionResult.OverflowAmount;

                }
            } while ( slot != null && insertionResult.Result == InsertionResult.Results.OVERFLOW );

            OnValidate ();
            return insertionResult;
        }

        /// <summary>
        /// Add <paramref name="quantity"/> amount of <paramref name="playerItem"/> into slot <paramref name="slotIndex"/>.
        /// </summary>
        /// <param name="playerItem">The PlayerItem to add to a backpack slot.</param>
        /// <param name="quantity">The amount of items to add.</param>
        /// <param name="slotIndex">The backpack slot index.</param>
        /// <returns>An InsertionResult for this operation.</returns>
        public InsertionResult AddToBackpack ( PlayerItem playerItem, int backpackSlotIndex, int quantity = 1 )
        {
            if ( playerItem == null || quantity <= 0 )
            {
                return new InsertionResult ( playerItem, InsertionResult.Results.INSERTION_FAILED );
            }

            Slot slot = m_backpackSlots [ backpackSlotIndex ];
            InsertionResult result;

            if ( slot == null ) // No slots available
            {
                return new InsertionResult ( InsertionResult.Results.INSERTION_FAILED );
            }
            result = slot.Insert ( playerItem, quantity );
            ServerSend.PlayerUpdateInventorySlot ( player.Id, slot.Id, playerItem.Id, slot.StackSize );
            OnValidate ();
            return result;
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

        public T [] GetFromBackpack<T> () where T : PlayerItem
        {
            List<T> results = new List<T> ();
            foreach ( Slot slot in m_backpackSlots )
            {
                if ( !slot.IsEmpty () && slot.PlayerItem is T )
                {
                    results.Add ( slot.PlayerItem as T );
                }
            }
            return results.ToArray ();
        }

        #endregion

        #region Weapons

        public void EquipWeapon ( Weapon newWeapon, Weapons activeWeaponSlot, out Weapons targetWeaponSlot )
        {
            // Calculate which weapon slot to equip this weapon to
            // Assign target slot to active slot by default
            targetWeaponSlot = activeWeaponSlot;
            Weapons otherWeaponSlot = activeWeaponSlot == Weapons.Primary ? Weapons.Secondary : Weapons.Primary;
            // Get references to current and other weapon(not currently active)
            WeaponSlots activeWeaponSlots = activeWeaponSlot == Weapons.Primary ? m_primaryWeaponSlots : m_secondaryWeaponSlots;
            WeaponSlots otherWeaponSlots = activeWeaponSlot == Weapons.Primary ? m_secondaryWeaponSlots : m_primaryWeaponSlots;
            if ( activeWeaponSlots.ContainsWeapon () && !otherWeaponSlots.ContainsWeapon () )
            {
                // Set target slot to other weapon(not in use)
                targetWeaponSlot = otherWeaponSlot;
            }

            RemovalResult [] removalResults = null;
            Weapon removedWeapon = null;
            Barrel barrel = null;
            Sight sight = null;
            Magazine magazine = null;
            Stock stock = null;
            switch ( targetWeaponSlot )
            {
                case Weapons.Primary:
                    if ( m_primaryWeaponSlots.ContainsWeapon () )
                    {
                        // Remove current weapon and attachments
                        removalResults = RemoveWeapon ( m_primaryWeaponSlots.WeaponSlot );

                        // Get removed attachments
                        barrel = GetRemovedPlayerItem<Barrel> ( removalResults );
                        sight = GetRemovedPlayerItem<Sight> ( removalResults );
                        magazine = GetRemovedPlayerItem<Magazine> ( removalResults );
                        stock = GetRemovedPlayerItem<Stock> ( removalResults );

                        GetCompatibleWeaponAttachments ( newWeapon, ref barrel, ref sight, ref magazine, ref stock );

                        // Drop removed weapon
                        removedWeapon = GetRemovedPlayerItem<Weapon> ( removalResults );
                        inventoryManager.DropItem ( removedWeapon );
                    }

                    // Assign compatible attachments to new weapon
                    m_primaryWeaponSlots.AssignContents ( newWeapon, barrel, sight, magazine, stock );
                    m_primaryWeaponSlots.Apply ( player.Id );
                    inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_primaryWeaponSlots );
                    break;
                case Weapons.Secondary:
                    if ( m_secondaryWeaponSlots.ContainsWeapon () )
                    {
                        // Remove current weapon and attachments
                        removalResults = RemoveWeapon ( m_secondaryWeaponSlots.WeaponSlot );

                        // Get removed attachments
                        barrel = GetRemovedPlayerItem<Barrel> ( removalResults );
                        sight = GetRemovedPlayerItem<Sight> ( removalResults );
                        magazine = GetRemovedPlayerItem<Magazine> ( removalResults );
                        stock = GetRemovedPlayerItem<Stock> ( removalResults );

                        GetCompatibleWeaponAttachments ( newWeapon, ref barrel, ref sight, ref magazine, ref stock );

                        // Drop removed weapon
                        removedWeapon = GetRemovedPlayerItem<Weapon> ( removalResults );
                        inventoryManager.DropItem ( removedWeapon );
                    }

                    // Assign compatible attachments to new weapon
                    m_secondaryWeaponSlots.AssignContents ( newWeapon, barrel, sight, magazine, stock );
                    m_secondaryWeaponSlots.Apply ( player.Id );
                    inventoryManager.OnWeaponSlotsUpdated.Invoke ( m_secondaryWeaponSlots );
                    break;
                default:
                    break;
            }
        }

        public WeaponSlots GetWeaponSlots ( Weapons weapon )
        {
            return weapon == Weapons.Primary ? m_primaryWeaponSlots : m_secondaryWeaponSlots;
        }


        public T GetWeaponSlot<T> ( string slotId ) where T : Slot
        {
            // Search primary weapon slots
            if ( m_primaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_primaryWeaponSlots.GetSlot ( slotId ) as T;
            }

            // Search secondary weapon slots
            if ( m_secondaryWeaponSlots.ContainsSlot ( slotId ) )
            {
                return m_secondaryWeaponSlots.GetSlot ( slotId ) as T;
            }

            return null;
        }

        private T [] GetFromWeaponSlots<T> ( Weapons weapon ) where T : PlayerItem
        {
            return weapon switch
            {
                Weapons.Primary => m_primaryWeaponSlots.GetItem<T> (),
                Weapons.Secondary => m_secondaryWeaponSlots.GetItem<T> (),
                _ => null
            };
        }

        #region Weapon Equip Util

        /// <summary>
        /// Retrieves a PlayerItem from a RemovalResult array.
        /// </summary>
        /// <typeparam name="T">The target PlayerItem type to retrieve.</typeparam>
        /// <param name="removalResults">An array of RemovalResult type.</param>
        /// <returns></returns>
        private T GetRemovedPlayerItem<T> ( RemovalResult [] removalResults )
        {
            if ( removalResults == null )
            {
                return default;
            }
            foreach ( RemovalResult removalResult in removalResults )
            {
                if ( removalResult.Contents == null )
                {
                    continue;
                }
                else if ( removalResult.Contents is T match )
                {
                    return match;
                }
            }
            return default;
        }

        private void GetCompatibleWeaponAttachments ( Weapon newWeapon, ref Barrel barrel, ref Sight sight, ref Magazine magazine, ref Stock stock )
        {
            // Check attachment compatibility
            // Assign null on failed matches
            if ( !newWeapon.IsCompatibleAttachment ( barrel ) )
            {
                // Drop incompatible barrel
                inventoryManager.DropItem ( barrel );
                barrel = null;
            }
            if ( !newWeapon.IsCompatibleAttachment ( sight ) )
            {
                // Drop incompatible sight
                inventoryManager.DropItem ( sight );
                sight = null;
            }
            if ( !newWeapon.IsCompatibleAttachment ( magazine ) )
            {
                // Drop incompatible magazine
                inventoryManager.DropItem ( magazine );
                magazine = null;
            }
            if ( !newWeapon.IsCompatibleAttachment ( stock ) )
            {
                // Drop incompatible stock
                inventoryManager.DropItem ( stock );
                stock = null;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Util

        /// <summary>
        /// Invokes InventoryManager.OnWeaponSlotsUpdated() with the associated WeaponSlots based on updated slot <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot which was updated.</param>
        private void CheckWeaponSlotsUpdated ( Slot slot )
        {
            if ( slot is WeaponSlot || slot is AttachmentSlot )
            {
                WeaponSlots weaponSlotsUpdated = slot.Id.Contains ( "primary" ) ? m_primaryWeaponSlots :
                    ( slot.Id.Contains ( "secondary" ) ? m_secondaryWeaponSlots : null );
                if ( weaponSlotsUpdated != null )
                {
                    inventoryManager.OnWeaponSlotsUpdated.Invoke ( weaponSlotsUpdated );
                }
            }
        }

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

        #endregion
    }
}