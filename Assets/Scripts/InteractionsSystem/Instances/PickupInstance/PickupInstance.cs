using System;
using UnityEngine;
using InventorySystem.PlayerItems;
using PlayerItemExporter;

[RequireComponent ( typeof ( BoxCollider ) )]
[RequireComponent ( typeof ( Rigidbody ) )]
[RequireComponent ( typeof ( NetworkedRigidbody ) )]
public class PickupInstance : Interactable
{
    private const float RIGIDBODY_MASS = 20f;

    private PlayerItem m_playerItem = null;
    private int m_quantity = 1;
    private Action m_pickupCallback = null;
    private BoxCollider m_boxCollider = null;
    private Rigidbody m_rigidbody = null;
    private NetworkedRigidbody m_networkedRigidbody = null;

    private void Awake ()
    {
        m_boxCollider = GetComponent<BoxCollider> ();
        m_rigidbody = GetComponent<Rigidbody> ();
        m_networkedRigidbody = GetComponent<NetworkedRigidbody> ();
    }

    public void Initialize ( PickupInstanceConfig config, Action pickupCallback )
    {
        base.Initialize (
            ( int ) config.InteractionType,
            config.IsInteractable,
            config.PlayerItem.Name,
            config.ContextColor,
            config.InteractTime,
            config.AccessKey );

        if ( config.PlayerItem == null )
        {
            Debug.LogWarning ( "playerItem is null." );
            return;
        }
        // Initialize PlayerItem data
        m_playerItem = config.PlayerItem;
        m_pickupCallback = pickupCallback;
        m_quantity = config.Quantity;

        // Initialize instance Bounds
        BoundsData data = PlayerItemBoundsData.Instance.GetBoundsData ( config.PlayerItem.Id );
        SetColliderBounds ( data );

        // Initialize Rigidbody
        m_rigidbody.mass = RIGIDBODY_MASS;
        m_rigidbody.isKinematic = data.Size == Vector3.zero;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Initialize NetworkedRigidbody
        m_networkedRigidbody.Initialize ( InstanceId );
    }

    #region Interactable

    protected override void OnInteract ()
    {
        // Invoke pickup callback
        m_pickupCallback?.Invoke ();

        // Add PlayerItem to Inventory
        Server.clients [ ClientId ].player.InventoryManager.Inventory.AddToBackpack ( m_playerItem, m_quantity );

        // Destroy this instance
        Destroy ( gameObject );
    }

    #endregion

    #region Util

    protected void SetColliderBounds ( BoundsData data )
    {
        if ( m_playerItem == null )
        {
            return;
        }

        m_boxCollider.center = data.Center/* - transform.position*/;
        m_boxCollider.size = data.Size;
    }

    #endregion
}
