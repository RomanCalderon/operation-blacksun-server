using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InventorySystem.Slots;
using InventorySystem.PlayerItems;

#region Models

public enum Weapons
{
    Primary,
    Secondary
}

#endregion

[RequireComponent ( typeof ( Player ) )]
public class WeaponsController : MonoBehaviour
{
    #region Constants

    private const string PRIMARY_WEAPON_ID = "primary-weapon";
    private const string SECONDARY_WEAPON_ID = "secondary-weapon";
    private const string PRIMARY_PREFIX = "primary";
    private const string SECONDARY_PREFIX = "secondary";
    private const string BARREL_ATTACHMENT_ID = "barrel";
    private const string SIGHT_ATTACHMENT_ID = "sight";
    private const string MAGAZINE_ATTACHMENT_ID = "magazine";
    private const string STOCK_ATTACHMENT_ID = "stock";

    #endregion

    #region Delegates

    public delegate void WeaponSwitchHandler ();
    public static WeaponSwitchHandler OnSwitchWeapon;

    #endregion

    #region Members

    private Player m_player = null;
    private InventoryManager m_inventoryManager = null;

    private int m_activeWeaponIndex = 0;

    public Weapons ActiveWeaponSlot { get; private set; } = Weapons.Primary;
    public Weapons InactiveWeaponSlot { get => ActiveWeaponSlot == Weapons.Primary ? Weapons.Secondary : Weapons.Primary; }
    public WeaponInstance ActiveWeapon
    {
        get
        {
            return ActiveWeaponSlot == Weapons.Primary ? m_primaryEquipped : m_secondaryEquipped;
        }
    }

    public WeaponInstance InactiveWeapon
    {
        get
        {
            return ActiveWeaponSlot == Weapons.Primary ? m_secondaryEquipped : m_primaryEquipped;
        }
    }

    public bool CanUseWeapons
    {
        get
        {
            return !m_player.IsDead && ActiveWeapon != null;
        }
    }

    public bool CanShootWeapon
    {
        get
        {
            return CanUseWeapons && ActiveWeapon.BulletCount > 0;
        }
    }

    [Space]
    [SerializeField]
    private List<WeaponInstance> m_primaryWeapons = new List<WeaponInstance> ();
    private WeaponInstance m_primaryEquipped = null;
    [SerializeField]
    private List<WeaponInstance> m_secondaryWeapons = new List<WeaponInstance> ();
    private WeaponInstance m_secondaryEquipped = null;

    #endregion

    #region Initializations

    private void OnEnable ()
    {
        m_inventoryManager.OnInventoryInitialized += UpdateAttachments;
        m_inventoryManager.OnWeaponSlotsUpdated += WeaponSlotsUpdated;
    }

    private void OnDisable ()
    {
        m_inventoryManager.OnInventoryInitialized -= UpdateAttachments;
        m_inventoryManager.OnWeaponSlotsUpdated -= WeaponSlotsUpdated;
    }

    private void Awake ()
    {
        m_player = GetComponent<Player> ();
        m_inventoryManager = m_player.InventoryManager;
    }

    #endregion

    #region Accessors

    public bool HasWeapon ( Weapons weapon )
    {
        return weapon == Weapons.Primary ? m_primaryEquipped != null : m_secondaryEquipped != null;
    }

    #endregion

    #region Shooting

    public void Shoot ( Vector3 lookDirection )
    {
        if ( ActiveWeapon != null )
        {
            ActiveWeapon.Shoot ( lookDirection );
        }
    }

    #endregion

    #region Weapon switching

    public void ActivateWeapon ( int weapon )
    {
        m_activeWeaponIndex = weapon;
        ActiveWeaponSlot = ( Weapons ) weapon;
        EnableWeapon ( ActiveWeaponSlot );
    }

    #endregion

    #region Weapon Reloading

    public void ReloadWeapon ()
    {
        if ( ActiveWeapon != null )
        {
            ActiveWeapon.Reload ();
        }
    }

    public void CancelWeaponReload ()
    {
        if ( ActiveWeapon != null )
        {
            ActiveWeapon.CancelReload ();
        }
    }

    #endregion

    #region Weapon and Attachment Equipping

    public bool IsCompatibleAttachment ( Weapons targetWeapon, Attachment attachment )
    {
        return targetWeapon switch
        {
            Weapons.Primary => m_primaryEquipped != null && m_primaryEquipped.Weapon.IsCompatibleAttachment ( attachment ),
            Weapons.Secondary => m_secondaryEquipped != null && m_secondaryEquipped.Weapon.IsCompatibleAttachment ( attachment ),
            _ => false,
        };
    }

    private void UpdateAttachments ()
    {
        if ( m_primaryEquipped != null )
        {
            m_primaryEquipped.UpdateAttachments ();
        }
        if ( m_secondaryEquipped != null )
        {
            m_secondaryEquipped.UpdateAttachments ();
        }
    }

    private void WeaponSlotsUpdated ( WeaponSlots weaponSlots )
    {
        if ( weaponSlots == null )
        {
            return;
        }

        // Check if WeaponSlot is empty
        if ( weaponSlots.WeaponSlot.IsEmpty () )
        {
            switch ( weaponSlots.Id )
            {
                case PRIMARY_WEAPON_ID:
                    ClearWeapon ( Weapons.Primary );
                    return;
                case SECONDARY_WEAPON_ID:
                    ClearWeapon ( Weapons.Secondary );
                    return;
                default:
                    Debug.LogError ( $"Unknown WeaponSlots Id [{weaponSlots.Id}]" );
                    return;
            }
        }

        WeaponSlot weaponSlot = weaponSlots.WeaponSlot;
        BarrelSlot barrelSlot = weaponSlots.BarrelSlot;
        SightSlot sightSlot = weaponSlots.SightSlot;
        MagazineSlot magazineSlot = weaponSlots.MagazineSlot;
        StockSlot stockSlot = weaponSlots.StockSlot;

        if ( weaponSlots.Id == PRIMARY_WEAPON_ID )
        {
            // Check if primary weapon changed
            if ( m_primaryEquipped == null )
            {
                EquipWeapon ( weaponSlot );
            }
            else if ( !weaponSlot.PlayerItem.Equals ( m_primaryEquipped.PlayerItem ) )
            {
                EquipWeapon ( weaponSlot );
            }

            // Check if any attachments changed
            if ( !barrelSlot.Equals ( m_primaryEquipped.Barrel ) )
            {
                EquipAttachment ( barrelSlot );
            }
            if ( !sightSlot.Equals ( m_primaryEquipped.Sight ) )
            {
                EquipAttachment ( sightSlot );
            }
            if ( !magazineSlot.Equals ( m_primaryEquipped.Magazine ) )
            {
                EquipAttachment ( magazineSlot );
            }
            if ( !stockSlot.Equals ( m_primaryEquipped.Stock ) )
            {
                EquipAttachment ( stockSlot );
            }
        }
        else if ( weaponSlots.Id == SECONDARY_WEAPON_ID )
        {
            // Check if secondary weapon changed
            if ( m_secondaryEquipped == null )
            {
                EquipWeapon ( weaponSlot );
            }
            else if ( !weaponSlot.PlayerItem.Equals ( m_secondaryEquipped.PlayerItem ) )
            {
                EquipWeapon ( weaponSlot );
            }

            // Check if any attachments changed
            if ( !barrelSlot.Equals ( m_secondaryEquipped.Barrel ) )
            {
                EquipAttachment ( barrelSlot );
            }
            if ( !sightSlot.Equals ( m_secondaryEquipped.Sight ) )
            {
                EquipAttachment ( sightSlot );
            }
            if ( !magazineSlot.Equals ( m_secondaryEquipped.Magazine ) )
            {
                EquipAttachment ( magazineSlot );
            }
            if ( !stockSlot.Equals ( m_secondaryEquipped.Stock ) )
            {
                EquipAttachment ( stockSlot );
            }
        }
    }

    /// <summary>
    /// Invoked when a WeaponSlot has been updated in the Inventory.
    /// </summary>
    /// <param name="slotId"></param>
    private void EquipWeapon ( Slot weaponSlot )
    {
        if ( weaponSlot == null )
        {
            Debug.LogWarning ( "weaponSlot is null." );
            return;
        }

        if ( weaponSlot.Id == PRIMARY_WEAPON_ID )
        {
            // No weapon
            if ( weaponSlot.IsEmpty () )
            {
                ClearWeapon ( Weapons.Primary );
                return;
            }

            WeaponInstance weapon = m_primaryWeapons.FirstOrDefault ( w => w.PlayerItem.Id == weaponSlot.PlayerItem.Id );
            if ( weapon != null )
            {
                ClearWeapon ( Weapons.Primary );
                m_primaryEquipped = weapon;
                m_primaryEquipped.SetActive ( true );
                m_primaryEquipped.Initialize ( m_player );
            }
            else
            {
                Debug.LogError ( $"Primary weapon with PlayerItem [{weaponSlot.PlayerItem}] does not exist in the list." );
            }
        }
        else if ( weaponSlot.Id == SECONDARY_WEAPON_ID )
        {
            // No weapon
            if ( weaponSlot.PlayerItem == null )
            {
                ClearWeapon ( Weapons.Secondary );
                return;
            }

            WeaponInstance weapon = m_secondaryWeapons.FirstOrDefault ( w => w.PlayerItem.Id == weaponSlot.PlayerItem.Id );
            if ( weapon != null )
            {
                ClearWeapon ( Weapons.Secondary );
                m_secondaryEquipped = weapon;
                m_secondaryEquipped.SetActive ( true );
                m_secondaryEquipped.Initialize ( m_player );
            }
            else
            {
                Debug.LogError ( $"Secondary weapon with PlayerItem [{weaponSlot.PlayerItem}] does not exist in the list." );
            }
        }
    }

    /// <summary>
    /// Invoked when an AttachmentSlot has been updated in the Inventory.
    /// </summary>
    /// <param name="slotId"></param>
    private void EquipAttachment ( Slot attachmentSlot )
    {
        if ( attachmentSlot == null )
        {
            Debug.LogWarning ( "attachmentSlot is null." );
            return;
        }

        if ( attachmentSlot.Id.Contains ( PRIMARY_PREFIX ) ) // Primary weapon attachment
        {
            if ( m_primaryEquipped == null )
            {
                return;
            }
            string slotId = attachmentSlot.Id;

            // Equip the attachment
            if ( slotId.Contains ( BARREL_ATTACHMENT_ID ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Barrel ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( MAGAZINE_ATTACHMENT_ID ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Magazine ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( SIGHT_ATTACHMENT_ID ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Sight ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( STOCK_ATTACHMENT_ID ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Stock ) attachmentSlot.PlayerItem );
            }
        }
        else if ( attachmentSlot.Id.Contains ( SECONDARY_PREFIX ) ) // Secondary weapon attachment
        {
            if ( m_secondaryEquipped == null )
            {
                return;
            }
            string slotId = attachmentSlot.Id;

            // Equip the attachment
            if ( slotId.Contains ( BARREL_ATTACHMENT_ID ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Barrel ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( MAGAZINE_ATTACHMENT_ID ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Magazine ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( SIGHT_ATTACHMENT_ID ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Sight ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( STOCK_ATTACHMENT_ID ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Stock ) attachmentSlot.PlayerItem );
            }
        }
    }

    #endregion

    #region Util

    private void EnableWeapon ( Weapons weapon )
    {
        // Disable both weapons
        DisableWeapons ();

        // Enable weapon
        switch ( weapon )
        {
            case Weapons.Primary:
                if ( m_primaryEquipped != null )
                {
                    m_primaryEquipped.gameObject.SetActive ( true );
                }
                break;
            case Weapons.Secondary:
                if ( m_secondaryEquipped != null )
                {
                    m_secondaryEquipped.gameObject.SetActive ( true );
                }
                break;
            default:
                break;
        }
    }

    private void DisableWeapons ()
    {
        if ( m_primaryEquipped != null )
        {
            m_primaryEquipped.gameObject.SetActive ( false );
        }
        if ( m_secondaryEquipped != null )
        {
            m_secondaryEquipped.gameObject.SetActive ( false );
        }
    }

    private void ClearWeapon ( Weapons weaponType )
    {
        switch ( weaponType )
        {
            case Weapons.Primary:
                foreach ( WeaponInstance weapon in m_primaryWeapons )
                {
                    weapon.gameObject.SetActive ( false );
                }
                m_primaryEquipped = null;
                break;
            case Weapons.Secondary:
                foreach ( WeaponInstance weapon in m_secondaryWeapons )
                {
                    weapon.gameObject.SetActive ( false );
                }
                m_secondaryEquipped = null;
                break;
            default:
                break;
        }
    }

    private void ClearAllWeapons ()
    {
        ClearWeapon ( Weapons.Primary );
        ClearWeapon ( Weapons.Secondary );
    }

    #endregion
}
