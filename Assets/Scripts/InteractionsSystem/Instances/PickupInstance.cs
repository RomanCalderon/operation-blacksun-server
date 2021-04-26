using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

[RequireComponent ( typeof ( BoxCollider ) )]
public class PickupInstance : Interactable
{
    public PlayerItem PlayerItem { get => m_playerItem; }

    private PlayerItem m_playerItem = null;
    private BoxCollider m_boxCollider = null;

    private void Awake ()
    {
        m_boxCollider = GetComponent<BoxCollider> ();
    }

    private void OnEnable ()
    {
        SetColliderBounds ( gameObject );
    }

    public void Initialize ( PlayerItem playerItem )
    {
        if ( playerItem == null )
        {
            Debug.LogWarning ( "playerItem is null." );
            return;
        }
        m_playerItem = playerItem;
    }

    #region Interactable

    public override void StartHover ()
    {
        // Display Interactable access UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartHover" );
    }

    public override void StartInteract ( string accessKey )
    {
        base.StartInteract ( accessKey );

        // Display Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartInteract" );
    }

    public override void StartInteract ( string [] accessKeys )
    {
        base.StartInteract ( accessKeys );

        // Display Interactable interaction UI
        Debug.Log ( $"Interactable [{m_playerItem}] - StartInteract" );
    }

    protected override void OnInteract ()
    {
        // Add PlayerItem to Inventory

        Debug.Log ( $"Interactable [{m_playerItem}] - OnInteract" );
        throw new System.NotImplementedException ();
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
