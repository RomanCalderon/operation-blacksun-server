using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( Player ) )]
[RequireComponent ( typeof ( CharacterMotor ) )]
[RequireComponent ( typeof ( CharacterController ) )]
public class PlayerMovementController : MonoBehaviour
{
    #region Constants

    private const float BASE_JUMP_HEIGHT = 1.0f;

    #endregion

    #region Models

    public enum MovementStates
    {
        NONE,
        WALKING,
        RUNNING,
        CROUCHING,
        PRONE,
        SLIDING
    }

    #endregion

    #region Members

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
    private float m_proneSpeed = 1f;
    private MovementStates m_currentMovementState = MovementStates.NONE;

    // Crouching
    private float m_crouchSmoothTime = 2.5f;
    private float m_crouchCurrVelocity;
    private float m_height;

    // Sliding
    private bool m_isSliding = false;
    private float m_slideSpeed = 12f;
    private float m_minSlideThreshold = 5.5f;
    private Vector3 m_initialSlideVelocity; // Initial direction of slide
    private float m_slideTimer = 0f;

    // Prone
    private bool m_proneToggle = false;

    // Gizmos
    [SerializeField]
    private bool m_drawGizmos = false;

    #endregion


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

    public void Movement ( Vector2 inputDirection, bool runInput, bool jumpInput, bool crouchInput, bool proneInput )
    {
        if ( m_player.IsDead )
        {
            return;
        }

        float fixedDeltaTime = Time.fixedDeltaTime;
        float height = m_height;
        float jumpHeight = BASE_JUMP_HEIGHT;
        float speed = m_walkSpeed;
        Vector3 movementVelocity = m_motor.movement.velocity;
        m_currentMovementState = MovementStates.WALKING;

        // Movement input direction
        if ( !m_isSliding )
        {
            Vector3 movementInputVector = ( transform.right * inputDirection.x + transform.forward * inputDirection.y ).normalized;
            m_motor.inputMoveDirection = movementInputVector;
        }

        // Running
        if ( runInput )
        {
            speed = m_runSpeed;
            m_currentMovementState = MovementStates.RUNNING;
        }

        // Jumping
        m_motor.jumping.baseHeight = jumpHeight;
        m_motor.inputJump = jumpInput;

        // Crouching
        if ( crouchInput )
        {
            height = m_height * 0.75f;
            speed = m_crouchSpeed;
            m_currentMovementState = MovementStates.CROUCHING;
        }
        else if ( proneInput ) // Prone
        {
            height = m_height * 0.5f;
            speed = m_proneSpeed;
            m_currentMovementState = MovementStates.PRONE;
        }

        // Sliding
        if ( crouchInput &&  // Crouch input
            !m_isSliding && // Not already sliding
            m_motor.IsGrounded () && // Player is grounded
            movementVelocity.magnitude >= m_minSlideThreshold ) // Minimum speed for sliding
        {
            m_currentMovementState = MovementStates.SLIDING;

            m_slideTimer = 0.0f; // Start timer
            m_isSliding = true;

            // Movement velocity boost
            m_motor.movement.velocity = m_initialSlideVelocity = movementVelocity;

            // Set initial slide speed
            speed = m_slideSpeed = m_initialSlideVelocity.magnitude + 3.5f;
        }
        if ( m_isSliding )
        {
            // Set controller height
            height = m_height * 0.75f;

            // Jump height boost
            jumpHeight = BASE_JUMP_HEIGHT * 2.5f;

            // Calculate slide speed by slope angle and time
            Vector3 slopeDir = CalculateSlopeDirection ( transform.position );
            float slopeResistance = ( movementVelocity.normalized - slopeDir.normalized ).magnitude;
            float slopeAngle = CalculateSlopeAngle ( transform.position );
            float slideTimeModifier = Mathf.Clamp01 ( ( Mathf.Clamp01 ( 30 - slopeAngle ) + slopeResistance ) / Mathf.Max ( 1, Mathf.Sqrt ( slopeResistance * 2 ) * ( slopeAngle / 30 ) ) );

            if ( m_drawGizmos )
            {
                Debug.DrawRay ( transform.position, movementVelocity.normalized, Color.yellow );
                Debug.DrawRay ( transform.position, slopeDir.normalized, Color.blue );
            }

            speed = m_slideSpeed -= m_slideTimer * slideTimeModifier;
            if ( m_motor.IsGrounded () )
            {
                m_motor.movement.velocity = m_initialSlideVelocity.normalized * speed;

                // Add jump velocity
                if ( jumpInput )
                {
                    m_motor.movement.velocity += Vector3.up * jumpHeight + transform.forward * m_motor.movement.velocity.magnitude * 0.65f;
                }
            }
            m_slideTimer += fixedDeltaTime;

            // Stop sliding if player is moving too slow OR no longer crouching OR is not grounded
            if ( m_slideSpeed < m_minSlideThreshold )
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
        m_controller.center += new Vector3 ( 0f, ( m_controller.height - lastHeight ) / 2, 0f ); // Fix vertical position

        // Server Sends
        ServerSend.PlayerPosition ( m_player.Id, transform.position );
        ServerSend.PlayerRotation ( m_player.Id, transform.rotation );
        ServerSend.PlayerMovement ( m_player, m_motor.movement.velocity, inputDirection * speed, runInput, crouchInput, proneInput );
    }

    private float CalculateSlopeAngle ( Vector3 position )
    {
        Vector3 slopeDirection = CalculateSlopeDirection ( position );
        return Mathf.FloorToInt ( Vector3.Angle ( Vector3.up, slopeDirection.normalized ) - 90f );
    }

    private Vector3 CalculateSlopeDirection ( Vector3 position )
    {
        Vector3 [] m_slopeCheckOrigins = new Vector3 [ 8 ];
        Vector3 [] slopeSamples = new Vector3 [ 8 ];
        Vector3 playerBottom = position - Vector3.up;
        float maxAngle = 0f;
        int maxAngleIndex = 0;

        // Get slope check hits
        for ( int i = 0; i < slopeSamples.Length; i++ )
        {
            m_slopeCheckOrigins [ i ] = position + new Vector3 ( Mathf.Cos ( ( i / 8f ) * 360 * Mathf.Deg2Rad ), 0, Mathf.Sin ( ( i / 8f ) * 360 * Mathf.Deg2Rad ) );

            if ( Physics.Raycast ( m_slopeCheckOrigins [ i ], Vector3.down, out RaycastHit hit, 3.0f ) )
            {
                if ( m_drawGizmos )
                {
                    Debug.DrawLine ( m_slopeCheckOrigins [ i ], hit.point, Color.white );
                }
                slopeSamples [ i ] = hit.point - playerBottom;
            }
            else
            {
                slopeSamples [ i ] = Vector3.up;
            }
        }

        // Get largest slope vector
        for ( int i = 0; i < slopeSamples.Length; i++ )
        {
            float checkAngle = Vector3.Angle ( Vector3.up, slopeSamples [ i ] );
            if ( checkAngle > maxAngle )
            {
                maxAngle = checkAngle;
                maxAngleIndex = i;
            }
        }

        if ( m_drawGizmos )
        {
            Debug.DrawRay ( playerBottom, slopeSamples [ maxAngleIndex ], Color.green );
        }
        return slopeSamples [ maxAngleIndex ].normalized;
    }

    private float CalculateSidewaysSpeed ()
    {
        switch ( m_currentMovementState )
        {
            case MovementStates.NONE:
            case MovementStates.WALKING:
                return m_walkSpeed;
            case MovementStates.RUNNING:
                return m_walkSpeed * 1.5f;
            case MovementStates.CROUCHING:
                return m_crouchSpeed;
            case MovementStates.SLIDING:
                return m_slideSpeed;
            case MovementStates.PRONE:
                return m_proneSpeed;
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
            case MovementStates.SLIDING:
                return m_slideSpeed;
            case MovementStates.PRONE:
                return m_proneSpeed;
            default:
                return m_walkSpeed;
        }
    }
}