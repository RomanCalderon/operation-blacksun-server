using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( Player ) )]
[RequireComponent ( typeof ( CharacterMotor ) )]
[RequireComponent ( typeof ( CharacterController ) )]
public class PlayerMovementController : MonoBehaviour
{
    public enum MovementStates
    {
        NONE,
        WALKING,
        RUNNING,
        CROUCHING
    }

    private Player m_player = null;
    private CharacterMotor m_motor = null;
    private CharacterController m_controller = null;

    // Movement
    [SerializeField]
    private float m_walkSpeed = 4.5f;
    [SerializeField]
    private float m_runSpeed = 8f;
    [SerializeField]
    private float m_crouchSpeed = 2f;
    [SerializeField]
    private float m_slideSpeed = 5f;
    private MovementStates m_currentMovementState = MovementStates.NONE;

    // Crouching
    private float m_crouchSmoothTime = 2.5f;
    private float m_crouchCurrVelocity;
    private float m_height;

    // Sliding
    private bool m_isSliding = false;
    private float m_minSlideThreshold = 7f;
    private Vector3 m_initialSlideVelocity; // Initial direction of slide
    private float m_slideTimer = 0f;
    private float m_slideTimerMax = 1.5f; // Slide duration in seconds
    private Vector3 [] m_slopeCheckOffsets = new Vector3 [ 8 ];
    private Vector3 [] m_slopeHitChecks = new Vector3 [ 8 ];

    // Gizmos
    [SerializeField]
    private bool m_drawGizmos = false;


    private void Awake ()
    {
        m_player = GetComponent<Player> ();
        m_motor = GetComponent<CharacterMotor> ();
        m_controller = GetComponent<CharacterController> ();
    }

    private void Start ()
    {
        m_motor.useFixedUpdate = true;
        m_height = m_controller.height; // Initial height
    }

    public void Movement ( Vector2 inputDirection, bool runInput, bool jumpInput, bool crouchInput, bool slideInput )
    {
        if ( m_player.IsDead )
        {
            return;
        }

        float fixedDeltaTime = Time.fixedDeltaTime;
        float height = m_height;
        float speed = m_walkSpeed;
        Vector3 movementVelocity = m_motor.movement.velocity;
        m_currentMovementState = MovementStates.WALKING;

        // Movement input direction
        Vector3 movementInputVector = ( transform.right * inputDirection.x + transform.forward * inputDirection.y ).normalized;
        m_motor.inputMoveDirection = !slideInput ? movementInputVector : Vector3.zero;

        if ( runInput )
        {
            speed = m_runSpeed;
            m_currentMovementState = MovementStates.RUNNING;
        }

        // Crouching
        if ( crouchInput )
        {
            height = m_height / 2f;
            speed = m_crouchSpeed;
            m_currentMovementState = MovementStates.CROUCHING;
        }

        // Jumping
        m_motor.inputJump = jumpInput;

        // Sliding
        if ( slideInput &&  // Slide input
            !m_isSliding && // Not already sliding
            m_motor.IsGrounded () && // Player is grounded
            movementVelocity.magnitude > m_minSlideThreshold ) // Moving faster than threshold
        {
            m_slideTimer = 0.0f; // Start timer
            m_isSliding = true;

            m_initialSlideVelocity = movementVelocity;
            float slopeAngle = Vector3.Angle ( Vector3.up, m_initialSlideVelocity.normalized );
            // Set initial slide speed
            speed = m_initialSlideVelocity.magnitude; //Mathf.Max ( m_minSlideThreshold, m_slideSpeed * ( slopeAngle - 89f ) );
        }
        else if ( m_isSliding )
        {
            height = m_height / 2f;
            speed -= m_slideTimer;
            Debug.Log ( $"speed = {speed}" );

            if ( speed < m_minSlideThreshold )
            {
                m_isSliding = false;
                return;
            }
            m_motor.movement.velocity = m_initialSlideVelocity.normalized * speed;

            m_slideTimer += fixedDeltaTime;
            if ( !slideInput || !m_motor.IsGrounded () )
            {
                m_isSliding = false;
            }
        }

        // Apply movement modifiers   
        m_motor.movement.maxForwardSpeed = speed; // Set max forward speed
        m_motor.movement.maxSidewaysSpeed = CalculateSidewaysSpeed (); // Set max sideways speed
        m_motor.movement.maxBackwardsSpeed = CalculateBackwardSpeed (); // Set max backward speed

        float lastHeight = m_controller.height; // Crouch/stand up smoothly 
        m_controller.height = Mathf.SmoothDamp ( m_controller.height, height, ref m_crouchCurrVelocity, m_crouchSmoothTime * fixedDeltaTime );
        transform.position += new Vector3 ( 0f, ( m_controller.height - lastHeight ) / 2, 0f ); // Fix vertical position

        // Server Sends
        ServerSend.PlayerPosition ( m_player.Id, transform.position );
        ServerSend.PlayerRotation ( m_player.Id, transform.rotation );
        ServerSend.PlayerMovementVector ( m_player, inputDirection ); // TODO: Send controller velocity as Vector2
    }

    private Vector3 CalculateSlideVector ( float time )
    {
        time = Mathf.Clamp ( time, 0f, m_slideTimerMax );
        Vector3 slopeVector = CalculateSlopeDirection ( transform.position );
        Debug.DrawRay ( transform.position, slopeVector, Color.red );

        return Vector3.Lerp ( m_initialSlideVelocity, slopeVector, time / m_slideTimerMax );
    }

    private Vector3 CalculateSlopeDirection ( Vector3 position )
    {
        float maxDist = 0f;
        int maxDistCheckIndex = 0;

        // Get slope check hits
        for ( int i = 0; i < m_slopeHitChecks.Length; i++ )
        {
            m_slopeCheckOffsets [ i ] = position + new Vector3 ( Mathf.Cos ( ( i / 8f ) * 360 * Mathf.Deg2Rad ), 0, Mathf.Sin ( ( i / 8f ) * 360 * Mathf.Deg2Rad ) );

            if ( Physics.Raycast ( m_slopeCheckOffsets [ i ], Vector3.down, out RaycastHit hit, m_controller.height ) )
            {
                m_slopeHitChecks [ i ] = hit.point;
            }
            else
            {
                m_slopeHitChecks [ i ] = position + m_slopeCheckOffsets [ i ];
            }
        }

        // Get largest slope vector
        for ( int i = 0; i < m_slopeHitChecks.Length; i++ )
        {
            float checkDist = Vector3.Distance ( m_slopeCheckOffsets [ i ], m_slopeHitChecks [ i ] );
            if ( checkDist > maxDist )
            {
                maxDist = checkDist;
                maxDistCheckIndex = i;
            }
        }

        Vector3 playerBottom = transform.position - ( Vector3.up * ( m_controller.height / 2f ) );
        return ( m_slopeHitChecks [ maxDistCheckIndex ] - playerBottom ).normalized;
    }

    private float CalculateSidewaysSpeed ()
    {
        switch ( m_currentMovementState )
        {
            case MovementStates.NONE:
            case MovementStates.WALKING:
                return m_walkSpeed;
            case MovementStates.RUNNING:
                return m_walkSpeed;
            case MovementStates.CROUCHING:
                return m_crouchSpeed;
            default:
                return m_walkSpeed;
        }
    }

    private float CalculateBackwardSpeed ()
    {
        switch ( m_currentMovementState )
        {
            case MovementStates.NONE:
            case MovementStates.WALKING:
                return m_walkSpeed;
            case MovementStates.RUNNING:
                return m_runSpeed * 0.75f;
            case MovementStates.CROUCHING:
                return m_crouchSpeed;
            default:
                return m_walkSpeed;
        }
    }

    private void OnDrawGizmos ()
    {
        if ( !m_drawGizmos )
        {
            return;
        }

        float maxDist = 0f;
        int maxDistCheckIndex = 0;

        Gizmos.color = Color.blue;

        for ( int i = 0; i < m_slopeHitChecks.Length; i++ )
        {
            float checkDist = Vector3.Distance ( m_slopeCheckOffsets [ i ], m_slopeHitChecks [ i ] );
            if ( checkDist > maxDist )
            {
                maxDist = checkDist;
                maxDistCheckIndex = i;
            }

            // Draw debug sphere indication ground collision
            Gizmos.DrawSphere ( m_slopeHitChecks [ i ], 0.1f );
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere ( m_slopeHitChecks [ maxDistCheckIndex ], 0.15f );
        Vector3 playerBottom = transform.position - Vector3.up;
        Gizmos.DrawRay ( transform.position, ( m_slopeHitChecks [ maxDistCheckIndex ] - playerBottom ) );
    }
}
