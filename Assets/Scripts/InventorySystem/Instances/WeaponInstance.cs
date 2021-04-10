using System.Collections;
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

    [Header ( "Raycast Mask" ), SerializeField]
    private LayerMask m_playerLayerMask;

    [Header ( "Audio" )]
    [SerializeField]
    private float m_audioMinDistance = 1f, m_audioMaxDistance = 300f;
    private string m_shootStandardAudioClipName;
    private string m_shootSuppressedAudioClipName;


    public void Initialize ( Player player )
    {
        m_player = player;

        m_shootStandardAudioClipName = GameAssets.Instance.GetAudioClip ( "gunshot-" + PlayerItem.Id + "-standard" );
        m_shootSuppressedAudioClipName = GameAssets.Instance.GetAudioClip ( "gunshot-" + PlayerItem.Id + "-suppressed" );
    }

    private void OnEnable ()
    {
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

    /// <summary>
    /// Equips all currently-assigned attachments on the WeaponInstance
    /// and their respective initializations.
    /// </summary>
    public void UpdateAttachments ()
    {
        EquipAttachment ( Barrel );
        EquipAttachment ( Magazine );
        EquipAttachment ( Sight );
        EquipAttachment ( Stock );
    }

    /// <summary>
    /// Equips a Barrel to this WeaponInstance.
    /// </summary>
    /// <param name="barrel">The Barrel to equip.</param>
    public void EquipAttachment ( Barrel barrel )
    {
        Barrel = barrel;
    }

    /// <summary>
    /// Equips a Magazine to this WeaponInstance.
    /// </summary>
    /// <param name="magazine">The Magazine to equip.</param>
    public void EquipAttachment ( Magazine magazine )
    {
        Magazine = magazine;

        if ( Magazine == null )
        {
            CancelReload ();
        }
        else if ( BulletCount == 0 )
        {
            string playerItemId = m_player.InventoryManager.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );
            int inventoryAmmoCount = m_player.InventoryManager.GetItemCount ( playerItemId );
            BulletCount = Mathf.Min ( Magazine.AmmoCapacity, inventoryAmmoCount );
        }
    }

    public void EquipAttachment ( Sight sight )
    {
        Sight = sight;
    }

    /// <summary>
    /// Equips a Stock to this WeaponInstance.
    /// </summary>
    /// <param name="stock">The Stock to equip.</param>
    public void EquipAttachment ( Stock stock )
    {
        Stock = stock;
    }

    #endregion

    #region Shooting

    /// <summary>
    /// Fires a single round in a specified direction.
    /// Invoked by WeaponsController.
    /// </summary>
    /// <param name="direction">The direction the weapon is fired.</param>
    public void Shoot ( Vector3 lookDirection )
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
            // Reset fireCooldown
            m_fireCooldown = ( PlayerItem as Weapon ).FireRate;

            // Subtract one bullet from the magazine
            BulletCount--;

            // Raycasting
            Vector3 shootOrigin = m_player.LookOriginController.ShootOrigin;
            Ray shootRay = new Ray ( shootOrigin, lookDirection );
            float bulletDamage = ( PlayerItem as Weapon ).BaseDamage;
            bool hitPlayer = false, killedPlayer = false;

            // Shoot Raycast - LayerMask:Player
            if ( Physics.Raycast ( shootRay, out RaycastHit hit, 1000f, m_playerLayerMask ) )
            {
                Player player = hit.collider.GetComponent<Player> ();
                if ( player != null )
                {
                    hitPlayer = true;
                    player.TakeDamage ( bulletDamage, out killedPlayer );
                    Debug.Log ( $"{m_player.Username} shot {player.Username}" );
                }
            }

            // DEBUG
            //if ( Physics.Raycast ( shootRay, out RaycastHit debugHit, 1000f ) )
            //{
            //    float dist = Vector3.Distance ( shootRay.origin, debugHit.point );
            //    Debug.DrawRay ( shootOrigin, lookDirection * dist, Color.blue, 1f );
            //}

            // Shoot SFX
            ServerSend.PlayAudioClip ( m_player.Id, m_shootStandardAudioClipName, 1f, transform.position, m_audioMinDistance, m_audioMaxDistance );

            // Hitmarker feedback
            if ( hitPlayer )
            {
                int hitmarkerType = killedPlayer ? 1 : 0;
                ServerSend.Hitmarker ( m_player.Id, hitmarkerType );
            }

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
