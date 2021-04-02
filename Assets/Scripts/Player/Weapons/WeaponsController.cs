using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using InventorySystem.Slots;
using InventorySystem.PlayerItems;

[RequireComponent ( typeof ( Player ) )]
public class WeaponsController : MonoBehaviour
{
    #region Models

    public enum Weapons
    {
        Primary,
        Secondary
    }

    #endregion

    #region Constants

    private const string PRIMARY_WEAPON_ID = "primary-weapon";
    private const string SECONDARY_WEAPON_ID = "secondary-weapon";
    private const string PRIMARY_ATTACHMENT_ID = "primary";
    private const string SECONDARY_ATTACHMENT_ID = "secondary";
    private const string BARREL_ATTACHMENT_ID = "barrel";
    private const string SIGHT_ATTACHMENT_ID = "sight";
    private const string MAGAZINE_ATTACHMENT_ID = "magazine";
    private const string STOCK_ATTACHMENT_ID = "stock";

    #endregion

    public delegate void WeaponSwitchHandler ();
    public static WeaponSwitchHandler OnSwitchWeapon;

    private Player m_player = null;
    private InventoryManager m_inventoryManager = null;

    private int m_activeHotbarIndex = 0;

    public Weapons ActiveWeaponSlot { get; private set; } = Weapons.Primary;
    public WeaponInstance ActiveWeapon
    {
        get
        {
            return ActiveWeaponSlot == Weapons.Primary ? m_primaryEquipped : m_secondaryEquipped;
        }
    }

    [Space]
    [SerializeField]
    private List<WeaponInstance> m_primaryWeapons = new List<WeaponInstance> ();
    private WeaponInstance m_primaryEquipped = null;
    [SerializeField]
    private List<WeaponInstance> m_secondaryWeapons = new List<WeaponInstance> ();
    private WeaponInstance m_secondaryEquipped = null;

    // Shooting
    private bool m_didShoot = false;

    private void OnEnable ()
    {
        m_inventoryManager.OnWeaponSlotsUpdated += WeaponSlotsUpdated;
    }

    private void OnDisable ()
    {
        m_inventoryManager.OnWeaponSlotsUpdated -= WeaponSlotsUpdated;
    }

    private void Awake ()
    {
        m_player = GetComponent<Player> ();
        m_inventoryManager = m_player.InventoryManager;
    }

    // Update is called once per frame
    void Update ()
    {
        /*
        // Weapon switching - Primary
        if ( ActiveWeaponSlot != Weapons.Primary )
        {
            ActivateWeapon ( Weapons.Primary );
        }
        // Weapon switching - Secondary
        if ( ActiveWeaponSlot != Weapons.Secondary )
        {
            ActivateWeapon ( Weapons.Secondary );
        }

        // Reloading
        if ( Input.GetKeyDown ( KeyCode.R ) )
        {
            if ( ActiveWeapon != null )
            {
                ActiveWeapon.Reload ();
            }
        }
        */
    }

    public void Shoot ( bool shootInput )
    {
        if ( shootInput == false )
        {
            m_didShoot = false;
            return;
        }

        if ( ActiveWeapon != null )
        {
            switch ( ( ActiveWeapon.PlayerItem as Weapon ).FireMode )
            {
                case Weapon.FireModes.SemiAuto:
                    if ( !m_didShoot )
                    {
                        ActiveWeapon.Shoot ();
                        m_didShoot = true;
                    }
                    break;
                case Weapon.FireModes.FullAuto:
                    ActiveWeapon.Shoot ();
                    break;
                default:
                    break;
            }
        }
    }

    #region Weapon switching

    private void ActivateWeapon ( Weapons weapon )
    {
        ActiveWeaponSlot = weapon;
        m_activeHotbarIndex = ( int ) weapon;
    }

    #endregion

    #region Weapon and Attachment Equipping

    private void WeaponSlotsUpdated ( WeaponSlots weaponSlots )
    {
        if ( weaponSlots == null )
        {
            return;
        }
        Debug.Log ( $"WeaponSlotsUpdated ( {weaponSlots.WeaponSlot} )" );

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

        // Check if primary weapon changed
        if ( weaponSlots.Id == PRIMARY_WEAPON_ID )
        {
            if ( m_primaryEquipped == null )
            {
                Debug.Log ( "adding weapon to empty primary slot" );
                EquipWeapon ( weaponSlot );
            }
            else if ( !weaponSlot.PlayerItem.Equals ( m_primaryEquipped.PlayerItem ) )
            {
                Debug.Log ( "primary weapon changed" );
                EquipWeapon ( weaponSlot );
            }
        }
        else if ( weaponSlots.Id == SECONDARY_WEAPON_ID )
        {
            if ( m_secondaryEquipped == null )
            {
                Debug.Log ( "adding weapon to empty secondary slot" );
                EquipWeapon ( weaponSlot );
            }
            else if ( !weaponSlot.PlayerItem.Equals ( m_secondaryEquipped.PlayerItem ) )
            {
                Debug.Log ( "secondary weapon changed" );
                EquipWeapon ( weaponSlot );
            }
        }

        //if ( weaponSlots.WeaponSlot.Id.Contains ( PRIMARY_ATTACHMENT_ID ) )
        //{
        //    Debug.Log ( $"Updated attachment [{weaponSlots.WeaponSlot}]" );
        //    EquipWeapon ( weaponSlots.WeaponSlot );
        //    return;
        //}
    }

    /// <summary>
    /// Invoked when a WeaponSlot has been updated in the Inventory.
    /// </summary>
    /// <param name="slotId"></param>
    private void EquipWeapon ( Slot weaponSlot )
    {
        Debug.Log ( $"EquipWeapon({weaponSlot})" );
        if ( weaponSlot == null )
        {
            Debug.LogWarning ( "weaponSlot is null." );
            return;
        }

        if ( weaponSlot.Id == PRIMARY_WEAPON_ID )
        {
            // No weapon
            if ( weaponSlot.PlayerItem == null )
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

        if ( attachmentSlot.Id.Contains ( "primary" ) ) // Primary weapon attachment
        {
            if ( m_primaryEquipped == null )
            {
                return;
            }
            string slotId = attachmentSlot.Id;

            // Equip the attachment
            if ( slotId.Contains ( "barrel" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Barrel ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( "magazine" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Magazine ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( "sight" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Sight ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( "stock" ) )
            {
                m_primaryEquipped.EquipAttachment ( ( Stock ) attachmentSlot.PlayerItem );
            }
        }
        else if ( attachmentSlot.Id.Contains ( "secondary" ) ) // Secondary weapon attachment
        {
            if ( m_secondaryEquipped == null )
            {
                return;
            }
            string slotId = attachmentSlot.Id;

            // Equip the attachment
            if ( slotId.Contains ( "barrel" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Barrel ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( "magazine" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Magazine ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( "sight" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Sight ) attachmentSlot.PlayerItem );
            }
            else if ( slotId.Contains ( "stock" ) )
            {
                m_secondaryEquipped.EquipAttachment ( ( Stock ) attachmentSlot.PlayerItem );
            }
        }
    }

    #endregion

    #region Util

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

    private void ResetWeaponsAll ()
    {
        ClearWeapon ( Weapons.Primary );
        ClearWeapon ( Weapons.Secondary );
    }

    #endregion
}
