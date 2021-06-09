using System.Collections.Generic;
using UnityEngine;
using InventorySystem;
using InventorySystem.Presets;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;
using InventorySystem.PlayerItems;

public class InventoryManager : MonoBehaviour
{
    #region Constants

    private const string PLAYER_ITEM_DATABASE = "PlayerItemDatabase";
    private const float ITEM_DROP_MAX_DISTANCE = 1f;
    private const float ITEM_DROP_HEIGHT_OFFSET = 0.5f;

    #endregion

    #region Delegates

    public delegate void InventoryHandler ();
    public InventoryHandler OnInventoryInitialized;

    public delegate void WeaponSlotHandler ( WeaponSlots weaponSlots );
    public WeaponSlotHandler OnWeaponSlotsUpdated;

    #endregion

    #region Members

    private Player m_player = null;
    public Inventory Inventory { get { return m_inventory; } }
    public PlayerItemDatabase PlayerItemDatabase { get; private set; } = null;
    [SerializeField]
    private Inventory m_inventory = null;
    [SerializeField]
    private Preset m_inventoryPreset = null;
    [Space]
    [SerializeField]
    private WeaponsController m_weaponsController = null;
    private bool m_initialWeaponsEquipped = false;

    #endregion

    #region Initializations

    private void Awake ()
    {
        m_player = GetComponent<Player> ();
        Debug.Assert ( m_player != null, "m_player is null." );
        PlayerItemDatabase = Resources.Load<PlayerItemDatabase> ( PLAYER_ITEM_DATABASE );
        Debug.Assert ( PlayerItemDatabase != null, "PlayerItemDatabase is null." );
    }

    public void InitializeInventory ()
    {
        m_inventory = Inventory.Create ( this, m_player, m_inventoryPreset, OnWeaponSlotsUpdated.Invoke );
        OnInventoryInitialized?.Invoke ();
    }

    #endregion

    #region Transfers

    public void TransferContents ( string fromSlotId, string toSlotId, int transferMode )
    {
        if ( fromSlotId.Contains ( "primary" ) || fromSlotId.Contains ( "secondary" ) ||
            toSlotId.Contains ( "primary" ) || toSlotId.Contains ( "secondary" ) )
        {
            transferMode = 0;
        }

        switch ( transferMode )
        {
            case 0: // Transfer ALL
                m_inventory.TransferContentsAll ( fromSlotId, toSlotId );
                break;
            case 1: // Transfer ONE
                m_inventory.TransferContentsSingle ( fromSlotId, toSlotId );
                break;
            case 2: // Transfer HALF
                m_inventory.TransferContentsHalf ( fromSlotId, toSlotId );
                break;
            default:
                break;
        }
    }

    #endregion

    #region Weapon/Attachment Equipping

    public void EquipWeapon ( Weapon weapon )
    {
        if ( !m_initialWeaponsEquipped )
        {
            m_initialWeaponsEquipped = m_weaponsController.HasWeapon ( Weapons.Primary ) && m_weaponsController.HasWeapon ( Weapons.Secondary );
        }
        m_inventory.EquipWeapon ( weapon, m_weaponsController.ActiveWeaponSlot, out Weapons targetWeaponSlot );
        if ( m_initialWeaponsEquipped )
        {
            m_weaponsController.ActivateWeapon ( ( int ) targetWeaponSlot );
        }

        // Find attachments in inventory to equip to the weapon
        List<Attachment> [] allAttachments = new List<Attachment> []
        {
            new List<Attachment> ( m_inventory.GetFromInventory<Barrel> ( false ) ),
            new List<Attachment> ( m_inventory.GetFromInventory<Sight> ( false ) ),
            new List<Attachment> ( m_inventory.GetFromInventory<Magazine> ( false ) ),
            new List<Attachment> ( m_inventory.GetFromInventory<Stock> ( false ) )
        };

        // For each attachment, equip the highest-rarity, compatible attachment
        for ( int i = 0; i < allAttachments.Length; i++ )
        {
            foreach ( Attachment attachment in allAttachments [ i ] )
            {
                if ( EquipAttachmentToWeapon ( targetWeaponSlot, attachment ) )
                {
                    // Remove attachment from inventory
                    RemovalResult result = m_inventory.RemoveItem ( attachment );
                    if ( result.Result != RemovalResult.Results.SUCCESS )
                    {
                        throw new System.Exception ( $"Error removing attachment [{attachment}] from inventory. RemovalResult = [{result.Result}]" );
                    }
                    break;
                }
            }
        }
    }

    public void EquipAttachment ( Attachment attachment )
    {
        /// 1. If there are any compatible weapons:
        ///     - Prioritize the active weapon, fallback to other weapon.
        ///     - Else, try to add attachment to inventory.
        ///     - Else, drop attachment.
        /// 2. If a compatible weapon is found:
        ///     - Check if the attachment slot is empty:
        ///         - If so, equip the attachment.
        ///         - Else, determine if attachment should replace the current-equipped attachment.
        ///             - If so, drop current-equipped attachment, equip new attachment.
        ///             - Else, if other weapon is compatible and hasn't been checked:
        ///                 - Repeat step 2 for other weapon.
        ///             - Else, try to add attachment to inventory.
        ///             - Else, drop attachment.

        // Equip to active weapon
        if ( !EquipAttachmentToWeapon ( m_weaponsController.ActiveWeaponSlot, attachment ) )
        {
            // Equip to inactive weapon
            if ( !EquipAttachmentToWeapon ( m_weaponsController.InactiveWeaponSlot, attachment ) )
            {
                // Try to add attachment to inventory
                if ( !AddToInventory ( attachment ) )
                {
                    // Drop attachment
                    Debug.Log ( $"Failed to equip/add [{attachment}]. Dropping item." );
                    DropItem ( attachment );
                }
            }
        }
    }

    private bool EquipAttachmentToWeapon ( Weapons weapon, Attachment attachment )
    {
        if ( attachment == null )
        {
            Debug.LogError ( "attachment is null." );
            return false;
        }
        if ( !m_weaponsController.IsCompatibleAttachment ( weapon, attachment ) )
        {
            return false;
        }

        // Compatible weapon found
        // Check if AttachmentSlot is empty
        WeaponSlots weaponSlots = m_inventory.GetWeaponSlots ( weapon );
        AttachmentSlot attachmentSlot = attachment switch
        {
            Barrel _ => weaponSlots.BarrelSlot,
            Sight _ => weaponSlots.SightSlot,
            Magazine _ => weaponSlots.MagazineSlot,
            Stock _ => weaponSlots.StockSlot,
            _ => throw new System.NotImplementedException ( $"InventoryManager::attachment [{attachment}] is null or not supported." ),
        };

        if ( attachmentSlot.IsEmpty () )
        {
            // Equip attachment to empty attachment slot
            InsertionResult result = attachmentSlot.Insert ( attachment );
            Debug.Log ( $"Equip attachment [{attachment}] result [{result.Contents}]" );

            OnWeaponSlotsUpdated?.Invoke ( weaponSlots );
            weaponSlots.Apply ( m_player.Id );
            return true;
        }
        // Determine if attachment should replace the current-equipped attachment, based on Rarity
        // or if the attachment is a Sight
        else if ( attachment.Rarity >= attachmentSlot.PlayerItem.Rarity ||
                  attachmentSlot is SightSlot )
        {
            // Drop current-equipped attachment
            DropFromInventory ( attachmentSlot.Id, 0, out _ );

            // Equip attachment to empty attachment slot
            InsertionResult result = attachmentSlot.Insert ( attachment );
            Debug.Log ( $"Equip attachment [{attachment}] result [{result.Contents}]" );

            OnWeaponSlotsUpdated?.Invoke ( weaponSlots );
            weaponSlots.Apply ( m_player.Id );
            return true;
        }

        return false;
    }

    #endregion

    public bool AddToInventory ( PlayerItem playerItem )
    {
        // Add to backpack
        InsertionResult backpackResult = m_inventory.AddToBackpack ( playerItem );
        if ( backpackResult.Result == InsertionResult.Results.SUCCESS )
        {
            Debug.Log ( $"Successfully added [{playerItem}] to backpack." );
            return true;
        }
        else
        {
            // Add to rig
            InsertionResult rigResult = m_inventory.AddToRig ( playerItem );
            if ( rigResult.Result == InsertionResult.Results.SUCCESS )
            {
                Debug.Log ( $"Successfully added [{playerItem}] to rig." );
                return true;
            }
        }
        return false;
    }

    public int GetItemCount ( string playerItemId )
    {
        return m_inventory.GetItemCount ( playerItemId );
    }

    public void ReduceItem ( string playerItemId, int reductionAmount )
    {
        m_inventory.ReduceItem ( playerItemId, reductionAmount );
    }

    public void DropFromInventory ( string slotId, int transferMode, out RemovalResult [] removalResults )
    {
        removalResults = null;
        if ( string.IsNullOrEmpty ( slotId ) )
        {
            return;
        }

        // Remove item(s) from inventory
        removalResults = m_inventory.RemoveItem ( slotId, transferMode );

        // Spawn item(s)
        if ( removalResults != null )
        {
            foreach ( RemovalResult removalResult in removalResults )
            {
                if ( removalResult == null )
                {
                    Debug.LogError ( "RemovalResult is null." );
                }
                else if ( removalResult.Contents != null )
                {
                    Debug.Log ( $"Drop item [{removalResult.Contents}] - Quantity [{removalResult.RemoveAmount}]" );
                    DropItem ( removalResult.Contents, removalResult.RemoveAmount );
                }
            }
        }
        else
        {
            Debug.LogError ( "removalResults array is null." );
        }
    }

    public void DropItem ( PlayerItem playerItem, int quantity = 1 )
    {
        if ( playerItem == null )
        {
            return;
        }

        // Spawn PlayerItem
        ItemSpawnerManager.Instance.SpawnItem ( playerItem, quantity, GetItemDropPosition () );
    }

    #region Util

    private Vector3 GetItemDropPosition ()
    {
        Vector3 origin = transform.position + Vector3.up * ITEM_DROP_HEIGHT_OFFSET;
        Ray ray = new Ray ( origin, transform.forward );

        if ( Physics.Raycast ( ray, out RaycastHit hit, ITEM_DROP_MAX_DISTANCE ) )
        {
            return hit.point - transform.forward * 0.1f;
        }
        return origin + transform.forward;
    }

    public void OnValidate ()
    {
        m_inventory.OnValidate ();
    }

    #endregion
}
