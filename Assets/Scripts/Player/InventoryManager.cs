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

    #region Weapon

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
    }

    #endregion

    public int GetItemCount ( string playerItemId )
    {
        return m_inventory.GetItemCount ( playerItemId );
    }

    public void ReduceItem ( string playerItemId, int reductionAmount )
    {
        m_inventory.ReduceItem ( playerItemId, reductionAmount );
    }

    public void DropItem ( string slotId, int transferMode, out RemovalResult [] removalResults )
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
                    ItemSpawnerManager.Instance.SpawnItem ( removalResult.Contents, removalResult.RemoveAmount, GetItemDropPosition () );
                }
            }
        }
        else
        {
            Debug.Log ( "removalResults array is null." );
        }
    }

    public void DropItem ( PlayerItem playerItem )
    {
        DropItem ( playerItem, 1 );
    }

    public void DropItem ( PlayerItem playerItem, int quantity )
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
