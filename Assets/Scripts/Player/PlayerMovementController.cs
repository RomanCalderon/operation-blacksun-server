using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( Rigidbody ) )]
[RequireComponent ( typeof ( CapsuleCollider ) )]
public class PlayerMovementController : MonoBehaviour
{
    #region Models

    public enum MovementStates
    {
        NONE,
        WALKING,
        RUNNING,
        CROUCHING
    }

    #endregion

    #region Constants

    // Movement
    private const float WALK_SPEED = 4f; // Base movement speed
    private const float RUN_SPEED_MULTIPLIER = 2.25f;
    private const float CROUCH_SPEED_MULTIPLIER = 0.5f;
    private const float CROUCH_POSITION_MODIFIER = 0.5f;
    private const float CROUCH_SMOOTH_TIME = 2.5f;
    private const float SLIDE_SPEED_BOOST = 2f;
    private const float MIN_SLIDE_SPEED = 3f;

    private const float MAX_VELOCITY_CHANGE = 10f;
    private const float GROUND_CONTROL = 1f;
    private const float AIR_CONTROL = 0.3f;
    private const float JUMP_HEIGHT = 1.3f; // Base jump height
    private const float MIN_JUMP_HEIGHT = 0.6f; // Crouch jumping

    private const float GROUND_CHECK_DISTANCE = 0.1f;
    private const float STATIONARY_DRAG = 100f;
    private const float SLIDING_DRAG = 1f;
    private const float NORMAL_DRAG = 0.1f;

    #endregion

    #region Members

    // Controllers
    private Rigidbody m_rigidbody = null;
    private CapsuleCollider m_collider = null;
    [SerializeField]
    private WeaponsController m_weaponsController = null;

    // Movement
    public Vector3 Velocity { get => m_rigidbody.velocity; }
    public bool IsGrounded { get => m_isGrounded; }
    private Vector3 m_gravity;
    private bool m_isGrounded;
    private bool m_jumpCheck = false;
    [SerializeField]
    private LayerMask m_groundMask;

    // Crouching
    private float m_crouchCurrVelocity;
    private float m_height;

    private Vector3 m_shootOriginOffset;

    // Sliding
    private bool m_isSliding = false;
    private bool m_appliedSlideForce = false;

    // Debug
    [Space, SerializeField]
    private bool m_showGizmos = false;

    #endregion


    private void Awake ()
    {
        m_rigidbody = GetComponent<Rigidbody> ();
        m_collider = GetComponent<CapsuleCollider> ();
        Debug.Assert ( m_weaponsController != null, "m_weaponsController is null." );
    }

    private void Start ()
    {
        m_gravity = Physics.gravity; // Global gravity
        m_height = m_collider.height; // Initial height
        m_rigidbody.useGravity = false;
        m_rigidbody.freezeRotation = true;

        m_shootOriginOffset = new Vector3 ( 0, 0.75f, 0 );
    }

    public void SetVelocty ( Vector3 newVelocity )
    {
        m_rigidbody.velocity = newVelocity;
    }

    public void SetRotation ( Quaternion newRotation )
    {
        m_rigidbody.MoveRotation ( newRotation );
    }

    #region Movement

    public void ProcessInputs ( ClientInputState state )
    {
        // If there's no state provided, use a default state.
        // This is used on the server to prevent resimulation of controller's
        // that have already been processed.
        if ( state == null )
        {
            Debug.Log ( "state is null" );
            state = new ClientInputState ();
        }

        // Get input
        bool moveForward = state.MoveForward;
        bool moveBackward = state.MoveBackward;
        bool moveLeft = state.MoveLeft;
        bool moveRight = state.MoveRight;
        bool jumping = state.Jump;
        bool running = state.Run;
        bool crouching = state.Crouch;
        bool aiming = state.Aiming;
        bool shooting = state.Shoot && m_weaponsController.CanShootWeapon;

        // Convert movement input to linear values
        float inputX = moveRight ? 1 : moveLeft ? -1 : 0;
        float inputZ = moveForward ? 1 : moveBackward ? -1 : 0;

        // Calculate movement speed
        float movementSpeed = WALK_SPEED * ( crouching ? CROUCH_SPEED_MULTIPLIER : running && moveForward && !aiming && !shooting ? RUN_SPEED_MULTIPLIER : 1f );

        // Set target velocity
        Vector3 targetVelocity = ( transform.right * inputX + transform.forward * inputZ ).normalized * movementSpeed;

        // Calculate a force that attempts to reach target velocity
        Vector3 velocity = m_rigidbody.velocity;
        float smoothTargetVelocityX = Mathf.Lerp ( velocity.x, targetVelocity.x, Time.fixedDeltaTime * 16f );
        float smoothTargetVelocityZ = Mathf.Lerp ( velocity.z, targetVelocity.z, Time.fixedDeltaTime * 16f );
        Vector3 velocityChange = ( new Vector3 ( smoothTargetVelocityX, 0, smoothTargetVelocityZ ) - velocity );
        velocityChange = Vector3.ClampMagnitude ( velocityChange, MAX_VELOCITY_CHANGE );
        velocityChange.y = 0;

        // Applies drag on rigidbody based on movement state
        ApplyDrag ( moveForward || moveBackward || moveRight || moveLeft || jumping );

        // If the player is not sliding, apply normal movement forces
        if ( !SlideControl ( velocity, crouching ) )
        {
            // Clamp velocity magnitude for ground/air control multiplier
            velocityChange = Vector3.ClampMagnitude ( velocityChange, velocityChange.magnitude * ( m_isGrounded ? GROUND_CONTROL : AIR_CONTROL ) );

            // Apply movement force
            m_rigidbody.AddForce ( velocityChange, ForceMode.VelocityChange );
        }

        // Add gravity
        m_rigidbody.AddForce ( m_gravity, ForceMode.Acceleration );

        // Jumping
        if ( jumping && m_isGrounded && !m_jumpCheck )
        {
            m_jumpCheck = true;
            m_rigidbody.AddForce ( transform.up * CalculateJumpVerticalSpeed ( velocity.magnitude, crouching ), ForceMode.VelocityChange );
        }
        if ( !jumping )
        {
            m_jumpCheck = false;
        }

        // Sets a flag for when the player is grounded
        GroundDetection ();

        // Updates collider height and position when crouching
        UpdateCrouchPosition ( crouching, Time.deltaTime );

        // Prevents rigidbody from sticking to walls
        FinalCollisionCheck ();

        // Update player rotation
        m_rigidbody.MoveRotation ( state.Rotation );
    }

    private bool SlideControl ( Vector3 velocity, bool crouchInput )
    {
        Vector3 slideVelocity = Vector3.ClampMagnitude ( velocity, MAX_VELOCITY_CHANGE ) * SLIDE_SPEED_BOOST;

        m_isSliding = velocity.magnitude > MIN_SLIDE_SPEED && crouchInput;

        if ( m_isSliding && !m_appliedSlideForce )
        {
            m_appliedSlideForce = true;
            m_rigidbody.AddForce ( slideVelocity, ForceMode.Impulse );
        }
        else if ( !m_isSliding )
        {
            m_appliedSlideForce = false;
        }
        return m_isSliding;
    }

    /// <summary>
    /// Sets a flag for when the player is grounded.
    /// </summary>
    private void GroundDetection ()
    {
        Vector3 groundCheckOrigin = ( transform.position + m_collider.center ) - Vector3.up * m_collider.height / 2f;
        m_isGrounded = Physics.CheckSphere ( groundCheckOrigin, GROUND_CHECK_DISTANCE, m_groundMask );
    }

    /// <summary>
    /// Applies drag on the rigidbody based on movement state. Increases drag when not moving and decreases drag when moving or not grounded.
    /// </summary>
    /// <param name="movementInput">Input that causes player to move.</param>
    /// <param name="crouchInput">Crouch input.</param>
    private void ApplyDrag ( bool movementInput )
    {
        float dragAmount;

        if ( m_isGrounded )
        {
            if ( !movementInput && !m_isSliding )
            {
                dragAmount = STATIONARY_DRAG;
            }
            else if ( m_isSliding )
            {
                dragAmount = SLIDING_DRAG;
            }
            else
            {
                dragAmount = NORMAL_DRAG;
            }
        }
        else
        {
            dragAmount = 0f;
        }
        // Apply drag
        m_rigidbody.drag = dragAmount;
    }

    /// <summary>
    /// Calculates proper rigidbody jump height based on movement speed and crouch state.
    /// </summary>
    /// <param name="currentSpeed">Rigidbody speed.</param>
    /// <param name="crouching">Crouch input.</param>
    /// <returns></returns>
    private float CalculateJumpVerticalSpeed ( float currentSpeed, bool crouching )
    {
        // From the jump height and gravity we deduce the upwards speed 
        // for the character to reach at the apex.
        float speedFactor = currentSpeed / MAX_VELOCITY_CHANGE;
        float jumpHeight = ( crouching ? Mathf.Lerp ( MIN_JUMP_HEIGHT, JUMP_HEIGHT, speedFactor ) : JUMP_HEIGHT );
        return Mathf.Sqrt ( 2 * jumpHeight * -m_gravity.y );
    }

    /// <summary>
    /// Prevents rigidbody from sticking to walls.
    /// </summary>
    private void FinalCollisionCheck ()
    {
        if ( m_isGrounded )
        {
            return;
        }

        // Get the velocity
        Vector3 moveDirection = m_rigidbody.velocity * Time.fixedDeltaTime;

        // Calculate the approximate distance that will be traversed
        float distance = moveDirection.magnitude * Time.fixedDeltaTime;
        // Normalize horizontalMove since it should be used to indicate direction
        moveDirection.Normalize ();

        // Check if the body's current velocity will result in a collision
        if ( m_rigidbody.SweepTest ( moveDirection, out _, distance, QueryTriggerInteraction.Ignore ) )
        {
            // If so, stop the movement
            m_rigidbody.velocity = new Vector3 ( 0, m_rigidbody.velocity.y, 0 );
        }
    }

    /// <summary>
    /// Updates collider height and position when crouching.
    /// </summary>
    /// <param name="isCrouching"></param>
    /// <param name="deltaTime"></param>
    private void UpdateCrouchPosition ( bool isCrouching, float deltaTime )
    {
        // Set height target
        float height = isCrouching ? m_height * CROUCH_POSITION_MODIFIER : m_height;
        float lastHeight = m_collider.height;
        // Crouch/stand up smoothly 
        m_collider.height = Mathf.SmoothDamp ( m_collider.height, height, ref m_crouchCurrVelocity, CROUCH_SMOOTH_TIME * deltaTime );
        // Fix vertical position
        m_collider.center += new Vector3 ( 0f, ( m_collider.height - lastHeight ) * CROUCH_POSITION_MODIFIER, 0f );
    }

    #endregion

    #region Util

    private void OnDrawGizmos ()
    {
        if ( !m_showGizmos )
        {
            return;
        }

        // Ground check
        Vector3 checkOrigin = ( transform.position + m_collider.center ) - Vector3.up * m_collider.height / 2f;
        Gizmos.color = m_isGrounded ? Color.red : Color.blue;
        Gizmos.DrawSphere ( checkOrigin, GROUND_CHECK_DISTANCE );
    }

    #endregion
}