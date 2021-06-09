using System.Collections;
using UnityEngine;
using InventorySystem.PlayerItems;


public class WeaponInstance : PlayerItemInstance
{
    private const string PLAYER_TAG = "Player";
    private const string PLAYER_LAYER = "Player";
    private const string IGNORE_RAYCAST_LAYER = "Ignore Raycast";

    private Player m_player = null;

    public Weapon Weapon { get => PlayerItem as Weapon; }
    public Barrel Barrel { get; private set; } = null;
    public Magazine Magazine { get; private set; } = null;
    public Sight Sight { get; private set; } = null;
    public Stock Stock { get; private set; } = null;

    // Ammo
    public int BulletCount { get; private set; } = 0;
    private string m_ammoId;

    [Header ( "Reloading" )]
    [SerializeField]
    private float m_partialReloadTime = 0.5f;
    [SerializeField]
    private float m_fullReloadTime = 0.8f;
    private bool m_isReloading = false;
    private bool m_isFullReload = false;
    private Coroutine m_reloadCoroutine = null;

    [Header ( "Raycast Mask" ), SerializeField]
    private LayerMask m_playerLayerMask;
    private int m_ignoreRaycastLayer, m_playerLayer;

    [Header ( "Audio" )]
    [SerializeField]
    private float m_audioMinDistance = 8f, m_audioMaxDistance = 400f;
    private string m_shootStandardAudioClipName;
    private string m_shootSuppressedAudioClipName;


    public void Initialize ( Player player )
    {
        m_player = player;

        m_ammoId = m_player.InventoryManager.PlayerItemDatabase.GetAmmoByCaliber ( ( PlayerItem as Weapon ).Caliber );

        m_ignoreRaycastLayer = LayerMask.NameToLayer ( IGNORE_RAYCAST_LAYER );
        m_playerLayer = LayerMask.NameToLayer ( PLAYER_LAYER );

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

        // Shoot the weapon if it has ammo
        if ( BulletCount > 0 )
        {
            // Subtract one bullet from the magazine
            BulletCount--;

            // Raycasting
            Vector3 shootOrigin = m_player.LookOriginController.ShootOrigin;
            Ray shootRay = new Ray ( shootOrigin, lookDirection );
            float bulletDamage = ( PlayerItem as Weapon ).BaseDamage;
            bool hitPlayer = false, killedPlayer = false;

            // Change player layer to Ignore 
            m_player.gameObject.layer = m_ignoreRaycastLayer;

            // Shoot Raycast
            if ( Physics.Raycast ( shootRay, out RaycastHit hit, 1000f ) )
            {
                if ( hit.collider.CompareTag ( PLAYER_TAG ) )
                {
                    Player player = hit.collider.GetComponent<Player> ();

                    // Check if player component is null
                    if ( player != null )
                    {
                        hitPlayer = true;
                        player.TakeDamage ( bulletDamage, out killedPlayer );

                        if ( killedPlayer )
                        {
                            Debug.Log ( $"{m_player.Username} [{PlayerItem.Id}] {player.Username}" );
                        }
                    }
                }

                // Projectile hit
                ShootableObjectsManager.Instance.ProjectileHit ( hit, bulletDamage );
            }

            //float dist = Vector3.Distance ( shootRay.origin, hit.point );
            //Debug.DrawRay ( shootOrigin, lookDirection * dist, Color.blue, 1f );

            // Change layermask back to original
            m_player.gameObject.layer = m_playerLayer;

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

        int inventoryAmmoCount = m_player.InventoryManager.GetItemCount ( m_ammoId );
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
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_fullReloadTime, inventoryAmmoCount ) );
        }
        else
        {
            m_reloadCoroutine = StartCoroutine ( ReloadCoroutine ( m_partialReloadTime, inventoryAmmoCount ) );
        }
    }

    private IEnumerator ReloadCoroutine ( float reloadTime, int inventoryAmmoCount )
    {
        yield return new WaitForSeconds ( reloadTime );

        FinishReload ( inventoryAmmoCount );
    }

    private void FinishReload ( int inventoryAmmoCount )
    {
        if ( !m_isReloading )
        {
            return;
        }

        int shotsFired = Magazine.AmmoCapacity - BulletCount;
        int refillAmount = Mathf.Min ( shotsFired, inventoryAmmoCount );
        BulletCount += refillAmount;

        // Reduce ammo from inventory
        m_player.InventoryManager.ReduceItem ( m_ammoId, refillAmount );

        // Reset reload flags
        m_isReloading = false;
        m_isFullReload = false;
    }

    public void CancelReload ()
    {
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
