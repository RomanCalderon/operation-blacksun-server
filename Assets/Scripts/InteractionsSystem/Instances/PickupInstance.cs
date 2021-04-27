using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

[RequireComponent ( typeof ( BoxCollider ) )]
[RequireComponent ( typeof ( Rigidbody ) )]
public class PickupInstance : Interactable
{
    public PlayerItem PlayerItem { get => m_playerItem; }

    private const float m_rigidbodyMass = 20f;
    private PlayerItem m_playerItem = null;
    private int m_quantity = 1;
    private Transform m_container = null;
    private BoxCollider m_boxCollider = null;
    private Rigidbody m_rigidbody = null;

    private void Awake ()
    {
        m_boxCollider = GetComponent<BoxCollider> ();
        m_rigidbody = GetComponent<Rigidbody> ();
        m_container = transform.GetChild ( 0 );
    }

    public void Initialize ( PlayerItem playerItem, int quantity = 1, bool isInteractable = true, string accessKey = null )
    {
        base.Initialize ( isInteractable, accessKey );

        if ( playerItem == null )
        {
            Debug.LogWarning ( "playerItem is null." );
            return;
        }
        m_playerItem = playerItem;
        m_quantity = quantity;
        m_rigidbody.mass = m_rigidbodyMass;
        m_rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // Instantiate PlayerItem object/model into container

        SetColliderBounds ( gameObject );
    }

    #region Interactable

    public override void StartHover ()
    {
        // Display Interactable access UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartHover" );
    }

    public override void StartInteract ( int clientId, string accessKey = null )
    {
        base.StartInteract ( clientId, accessKey );
        if ( !IsInteracting )
        {
            return;
        }

        // Display Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartInteract" );
    }

    public override void StartInteract ( int clientId, string [] accessKeys = null )
    {
        base.StartInteract ( clientId, accessKeys );
        if ( !IsInteracting )
        {
            return;
        }

        // Display Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartInteract" );
    }

    protected override void OnInteract ()
    {
        // Add PlayerItem to Inventory
        Debug.Log ( $"Interactable [{m_playerItem}] - OnInteract" );

        Server.clients [ ClientId ].player.InventoryManager.Inventory.AddToBackpack ( m_playerItem, m_quantity );

        // Destroy this PickupInstance
        Destroy ( gameObject );
    }

    public override void StopInteract ()
    {
        base.StopInteract ();

        // Hide Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StopInteract" );
    }

    public override void EndHover ()
    {
        // Hide Interactable UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StopHover" );
    }

    #endregion

    #region Util

    protected void SetColliderBounds ( GameObject assetModel )
    {
        Vector3 pos = assetModel.transform.localPosition;
        Quaternion rot = assetModel.transform.localRotation;
        Vector3 scale = assetModel.transform.localScale;

        // Need to clear out transforms while encapsulating bounds
        assetModel.transform.localPosition = Vector3.zero;
        assetModel.transform.localRotation = Quaternion.identity;
        assetModel.transform.localScale = Vector3.one;

        // Start with root object's bounds
        Bounds bounds = new Bounds ( Vector3.zero, Vector3.zero );
        if ( assetModel.transform.TryGetComponent ( out Renderer mainRenderer ) )
        {
            // New Bounds() will include 0,0,0 which you may not want to Encapsulate
            // because the vertices of the mesh may be way off the model's origin,
            // so instead start with the first renderer bounds and Encapsulate from there
            bounds = mainRenderer.bounds;
        }

        Transform [] descendants = assetModel.GetComponentsInChildren<Transform> ();
        foreach ( Transform desc in descendants )
        {
            if ( desc.TryGetComponent ( out Renderer childRenderer ) )
            {
                if ( bounds.extents == Vector3.zero )
                    bounds = childRenderer.bounds;
                bounds.Encapsulate ( childRenderer.bounds );
            }
        }

        m_boxCollider.center = bounds.center - assetModel.transform.position;
        m_boxCollider.size = bounds.size;

        // Restore transforms
        assetModel.transform.localPosition = pos;
        assetModel.transform.localRotation = rot;
        assetModel.transform.localScale = scale;
    }

    #endregion
}
