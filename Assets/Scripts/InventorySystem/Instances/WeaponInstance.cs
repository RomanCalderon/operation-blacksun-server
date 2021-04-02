using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;


public class WeaponInstance : PlayerItemInstance
{
    private Player m_player = null;

    public Barrel Barrel { get; private set; } = null;
    public Magazine Magazine { get; private set; } = null;
    public Sight Sight { get; private set; } = null;
    public Stock Stock { get; private set; } = null;

    public int BulletCount { get; private set; } = 0;


    [Header ( "Reloading" )]
    [SerializeField]
    private float m_partialReloadTime = 0.5f;
    [SerializeField]
    private float m_fullReloadTime = 0.8f;
    private bool m_isReloading = false;
    private bool m_isFullReload = false;
    private Coroutine m_reloadCoroutine = null;

    private float m_fireCooldown = 0f;
    

    public void Initialize ( Player player )
    {
        m_player = player;

        UpdateAttachments ();
    }

    private void OnDisable ()
    {
        // Cancel reload if this gun gets disabled
        CancelReload ();
    }

    // Update is called once per frame
    void Update ()
    {
        if ( m_fireCooldown > 0f )
        {
            m_fireCooldown -= Time.deltaTime;
        }
        else
        {
            m_fireCooldown = 0f;
        }
    }

    #region Attachment Equipping/Unequipping

    public void UpdateAttachments ()
    {
        // TODO
        //m_attachmentsController.UpdateAttachment ( Barrel );
        //m_attachmentsController.UpdateAttachment ( Magazine );
        //m_attachmentsController.UpdateAttachment ( Sight );
        //m_attachmentsController.UpdateAttachment ( Stock );
    }

    /// <summary>
    /// Equips a Barrel to this WeaponInstance.
    /// </summary>
    /// <param name="barrel">The Barrel to equip.</param>
    public void EquipAttachment ( Barrel barrel )
    {
        Barrel = barrel;

        // Update attachment instance
        //m_attachmentsController.UpdateAttachment ( Barrel );
    }

    /// <summary>
    /// Equips a Magazine to this WeaponInstance.
    /// </summary>
    /// <param name="magazine">The Magazine to equip.</param>
    public void EquipAttachment ( Magazine magazine )
    {
        Magazine = magazine;

        // Update attachment instance
        //m_attachmentsController.UpdateAttachment ( Magazine );

        if ( Magazine == null )
        {
            CancelReload ();
        }
        else if ( BulletCount == 0 )
        {
            string ammoId = m_player.InventoryManager.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
            int inventoryAmmoCount = m_player.InventoryManager.GetItemCount ( ammoId );
            BulletCount = Mathf.Min ( Magazine.AmmoCapacity, inventoryAmmoCount );
        }
    }

    public void EquipAttachment ( Sight sight )
    {
        Sight = sight;

        // Update attachment instance
        //m_attachmentsController.UpdateAttachment ( Sight );
    }

    /// <summary>
    /// Equips a Stock to this WeaponInstance.
    /// </summary>
    /// <param name="stock">The Stock to equip.</param>
    public void EquipAttachment ( Stock stock )
    {
        Stock = stock;

        // Update attachment instance
        //m_attachmentsController.UpdateAttachment ( Stock );
    }

    #endregion

    #region Shooting

    /// <summary>
    /// Fires a single round in a specified direction.
    /// Invoked by WeaponsController.
    /// </summary>
    /// <param name="direction">The direction the weapon is fired.</param>
    public void Shoot ()
    {
        if ( m_isReloading && !m_isFullReload )
        {
            CancelReload ();
            return;
        }
        if ( Magazine == null )
        {
            return;
        }
        if ( m_fireCooldown > 0f )
        {
            return;
        }

        // Shoot the weapon if it has ammo
        if ( BulletCount > 0 )
        {
            Debug.Log ( "Shoot gun" );

            // Reset fireCooldown
            m_fireCooldown = ( PlayerItem as Weapon ).FireRate;

            // Subtract one bullet from the magazine
            BulletCount--;

            // TODO: Perform gunshot
            float damage = ( PlayerItem as Weapon ).BaseDamage;
        }
    }

    #endregion

    #region Reloading

    /// <summary>
    /// Invoked by WeaponsController.
    /// </summary>
    public void Reload ()
    {
        if ( m_isReloading ) // Already reloading
        {
            return;
        }
        if ( Magazine == null ) // No magazine
        {
            return;
        }
        if ( BulletCount == Magazine.AmmoCapacity ) // Full ammo capacity
        {
            return;
        }
        string ammoId = m_player.InventoryManager.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
        int inventoryAmmoCount = m_player.InventoryManager.GetItemCount ( ammoId );
        if ( inventoryAmmoCount == 0 ) // Out of compatible ammo in inventory
        {
            return;
        }

        // Set reloading flag
        m_isReloading = true;

        // If a full reload is required
        m_isFullReload = BulletCount == 0;

        if ( m_isFullReload )
        {
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_fullReloadTime ) );
        }
        else
        {
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_partialReloadTime ) );
        }
    }

    private IEnumerator ReloadCoroutine ( float reloadTime )
    {
        yield return new WaitForSeconds ( reloadTime );

        FinishReload ();
    }

    private void FinishReload ()
    {
        if ( !m_isReloading )
        {
            return;
        }

        string ammoId = m_player.InventoryManager.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
        int inventoryAmmoCount = m_player.InventoryManager.GetItemCount ( ammoId );
        int shotsFired = Magazine.AmmoCapacity - BulletCount;
        int refillAmount = Mathf.Min ( shotsFired, inventoryAmmoCount );
        BulletCount += refillAmount;

        // Reset reload flags
        m_isReloading = false;
        m_isFullReload = false;
    }

    private void CancelReload ()
    {
        // Stop the reload animation
        //WeaponsController.OnSetTrigger?.Invoke ( "CancelReload" );

        // Stop the reload coroutine
        if ( m_reloadCoroutine != null )
        {
            StopCoroutine ( m_reloadCoroutine );
            m_reloadCoroutine = null;
        }

        // Reset reload flags
        m_isReloading = false;
        m_isFullReload = false;
    }

    #endregion

}
