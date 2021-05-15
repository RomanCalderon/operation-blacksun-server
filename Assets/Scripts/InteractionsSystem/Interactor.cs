using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    private const float CHECK_RADIUS = 2f;
    private const float CHECK_ANGLE = 15f;

    public bool CanInteract { get; set; } = true;

    [SerializeField]
    private Player m_player = null;
    [SerializeField]
    private LookOriginController m_lookOriginController = null;
    [SerializeField]
    private LayerMask m_interactableMask;

    private IInteractable m_target = null;
    private bool m_lastInput = false;


    // Start is called before the first frame update
    void Start ()
    {
        Debug.Assert ( m_player != null, "m_player is null." );
        Debug.Assert ( m_lookOriginController != null, "m_lookOriginController is null." );
    }

    // Update is called once per frame
    private void FixedUpdate ()
    {
        LookForTarget ();
    }

    public void ProcessClientInput ( int clientId, ClientInputState clientInputState )
    {
        if ( m_player.Id != clientId || m_target == null )
        {
            return;
        }

        if ( clientInputState.Interact != m_lastInput )
        {
            if ( clientInputState.Interact )
            {
                m_target.StartInteract ( m_player.Id, null as string );
            }
            else
            {
                m_target.StopInteract ();
            }
        }
        m_lastInput = clientInputState.Interact;
    }

    private void LookForTarget ()
    {
        // Check if the player can interact
        if ( !CanInteract )
        {
            if ( m_target != null )
            {
                m_target.StopInteract ();
                m_target = null;
            }
            return;
        }

        IInteractable target = GetTargetInteractable ();
        if ( IsNewTarget ( target ) )
        {
            if ( m_target != null )
            {
                m_target.StopHover ();
                m_target = null;
            }
            m_target = target;
            if ( m_target != null )
            {
                m_target.StartHover ();
            }
        }
    }

    #region Util

    private bool IsNewTarget ( IInteractable target )
    {
        if ( m_target == null && target != null )
        {
            return true;
        }
        if ( m_target == target )
        {
            return false;
        }
        return true;
    }

    private IInteractable GetTargetInteractable ()
    {
        Collider [] allInteractables = Physics.OverlapSphere ( transform.position, CHECK_RADIUS, m_interactableMask );
        Vector3 lookDirection = m_lookOriginController.GunDirection;
        Vector3 headPosition = m_lookOriginController.ShootOrigin;
        IInteractable target = null;
        float closest = CHECK_ANGLE;

        // Debug
        Debug.DrawRay ( headPosition, lookDirection * CHECK_RADIUS, Color.blue, Time.fixedDeltaTime );

        // Find closest Interactable
        foreach ( Collider collider in allInteractables )
        {
            float viewDistance = Vector3.Angle ( lookDirection, collider.transform.position - headPosition );
            if ( viewDistance < closest )
            {
                IInteractable interactable = collider.GetComponent<IInteractable> ();
                if ( interactable == null )
                {
                    Debug.LogError ( $"Collider ({collider.name}) with layer [Interactable] is missing an Interactable component." );
                    continue;
                }
                closest = viewDistance;
                target = interactable;
            }
        }
        return target;
    }

    #endregion
}
