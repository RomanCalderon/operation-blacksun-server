using UnityEngine;
using InventorySystem;
using InventorySystem.Presets;
using InventorySystem.Slots;

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
    [SerializeField]
    private Inventory m_inventory = null;
    [SerializeField]
    private Preset m_inventoryPreset = null;
    public PlayerItemDatabase PlayerItemDatabase { get; private set; } = null;

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

    public void RemoveItem ( string slotId, int transferMode )
    {
        m_inventory.RemoveItem ( slotId, transferMode );
    }

    public void OnValidate ()
    {
        m_inventory.OnValidate ();
    }
}
