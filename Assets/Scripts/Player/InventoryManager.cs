using UnityEngine;
using InventorySystem;
using InventorySystem.Presets;
using InventorySystem.Slots;
using InventorySystem.Slots.Results;

public class InventoryManager : MonoBehaviour
{
    #region Constants

    private const string PLAYER_ITEM_DATABASE = "PlayerItemDatabase";

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

    [SerializeField]
    private Vector3 m_itemDropPositionOffset = Vector3.forward;

    #endregion

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

    #region Transfers

    public void TransferContents ( string fromSlot, string toSlot, int transferMode )
    {
        switch ( transferMode )
        {
            case 0: // Transfer ALL
                m_inventory.TransferContentsAll ( fromSlot, toSlot );
                break;
            case 1: // Transfer ONE
                m_inventory.TransferContentsSingle ( fromSlot, toSlot );
                break;
            case 2: // Transfer HALF
                m_inventory.TransferContentsHalf ( fromSlot, toSlot );
                break;
            default:
                break;
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

    public void DropItem ( string slotId, int transferMode )
    {
        if ( string.IsNullOrEmpty ( slotId ) )
        {
            return;
        }

        // Remove item(s) from inventory
        RemovalResult [] removalResults = m_inventory.RemoveItem ( slotId, transferMode );

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
                    // FIXME: Position item spawner
                    Vector3 itemSpawnPosition = new Vector3 ( 0, 5, 0 );
                    ItemSpawnerManager.Instance.SpawnItem ( removalResult.Contents, removalResult.RemoveAmount, itemSpawnPosition );
                }
            }
        }
        else
        {
            Debug.Log ( "removalResults array is null." );
        }
    }

    public void OnValidate ()
    {
        m_inventory.OnValidate ();
    }
}
