using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookOriginController : MonoBehaviour
{
    private const float CROUCH_SPEED = 4f;
    private const float CROUCH_TARGET_HEIGHT = -0.5f;

    public Vector3 ShootOrigin
    {
        get
        {
            return m_localShootOrigin + transform.position;
        }
    }
    private Vector3 m_localShootOrigin;

    public Vector3 LookDirection { get => m_lookDirection; }
    private Vector3 m_lookDirection;

    private Vector3 m_headPositionTarget;
    private Vector3 m_headOffset;

    public void Initialize ()
    {
        m_headOffset = new Vector3 ( 0, 0.75f, 0 ); // client head offset must match this value
        m_localShootOrigin = m_headOffset;
    }

    public void ProcessInput( ClientInputState state )
    {
        m_lookDirection = state.LookDirection;
        UpdateMovementChanges ( state.Crouch, Time.fixedDeltaTime );
    }

    public void UpdateMovementChanges ( bool crouch, float deltaTime )
    {
        if ( crouch ) // Crouch
        {
            m_headPositionTarget = new Vector3 ( 0, CROUCH_TARGET_HEIGHT, 0 );
        }
        else // Standing
        {
            m_headPositionTarget = Vector3.zero;
        }

        // Update head positioning
        m_localShootOrigin = Vector3.MoveTowards ( m_localShootOrigin, m_headPositionTarget + m_headOffset, deltaTime * CROUCH_SPEED );
    }
}
