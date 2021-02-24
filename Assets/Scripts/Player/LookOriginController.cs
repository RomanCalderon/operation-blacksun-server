using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookOriginController : MonoBehaviour
{
    private const float CROUCH_SPEED = 4f;
    private const float CROUCH_TARGET_HEIGHT = -0.5f;

    public Vector3 ShootOrigin { get; private set; }

    private Vector3 m_headPositionTarget;
    private Vector3 m_headOffset;

    public void Initialize ()
    {
        m_headOffset = new Vector3 ( 0, 0.75f, 0 ); // This offset must match the client head offset value
        ShootOrigin = transform.position + m_headOffset;
    }

    public void ProcessInput( ClientInputState state )
    {
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
        ShootOrigin = Vector3.MoveTowards ( ShootOrigin, m_headPositionTarget + m_headOffset, deltaTime * CROUCH_SPEED );
    }
}
