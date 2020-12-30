using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using InventorySystem;
using InventorySystem.Presets;

[RequireComponent ( typeof ( PlayerMovementController ) )]
[RequireComponent ( typeof ( CharacterMotor ) )]
public class Player : MonoBehaviour
{
    #region Models

    private struct Frame
    {
        // Frame timestamp
        public float timestamp;
        // Interpolation time on client
        public short lerp_msec;
        // Duration in ms of command
        public float deltaTime;
        // Position of player
        public Vector3 position;
        // Delta position
        public Vector3 deltaPosition;
        // Velocity this frame
        public Vector3 velocity;
        // Player movement inputs
        [MarshalAs ( UnmanagedType.ByValArray, SizeConst = Constants.NUM_PLAYER_INPUTS )]
        public bool [] inputs;
        // Player rotation
        public Quaternion rot;

        public Frame ( Frame other )
        {
            timestamp = other.timestamp;
            lerp_msec = other.lerp_msec;
            deltaTime = other.deltaTime;
            position = other.position;
            deltaPosition = other.deltaPosition;
            velocity = other.velocity;
            inputs = other.inputs;
            rot = other.rot;
        }
    }

    #endregion

    #region Members

    private const string PLAYER_TAG = "Player";

    public int Id { get; private set; }
    public string Username { get; private set; }
    public float Health { get; private set; }
    public bool IsDead { get { return Health == 0; } }

    private CharacterController m_controller = null;
    private CharacterMotor m_motor = null;
    private PlayerMovementController m_movementController = null;

    [SerializeField]
    private Transform m_shootOrigin = null;
    private Vector3 m_shootOriginInitialOffset;
    private Vector3 m_shootOriginCrouchProneOffset;

    [SerializeField]
    private float m_maxHealth = 100f;

    //private PlayerInputs m_playerInputs;
    private bool m_receivedInput = false;

    // Inventory
    [SerializeField]
    private Preset m_inventoryPreset = null;
    public Inventory Inventory { get; private set; } = null;

    #endregion


    private void Awake ()
    {
        m_controller = GetComponent<CharacterController> ();
        m_motor = GetComponent<CharacterMotor> ();
        m_movementController = GetComponent<PlayerMovementController> ();
    }

    private void Start ()
    {
        m_shootOriginInitialOffset = m_shootOrigin.position - transform.position;
    }

    public void Initialize ( int _id, string _username )
    {
        Id = _id;
        Username = _username;
        Health = m_maxHealth;

        Inventory = new Inventory ( this, m_inventoryPreset );
    }

    /// <summary>
    /// Use when the player is being respawned.
    /// </summary>
    private void Reinitialize ()
    {
        Health = m_maxHealth;
        m_controller.enabled = true;
        m_motor.enabled = true;
        Inventory = new Inventory ( this, m_inventoryPreset );
        Inventory.SendInitializedInventory ();
    }

    public void SendInitializedInventory ()
    {
        if ( Inventory == null )
        {
            return;
        }
        Inventory.SendInitializedInventory ();
    }

    private void FixedUpdate ()
    {
        if ( IsDead )
        {
            return;
        }
    }

    public void ReceiveInput ( byte [] clientFrame )
    {
        m_receivedInput = true;
        Frame input = FromBytes ( clientFrame );

        StartCoroutine ( ProcessInput ( input ) );
    }

    private IEnumerator ProcessInput ( Frame playerInput )
    {
        // Apply the input
        ApplyInput ( playerInput );

        // Wait one frame for simulation
        yield return new WaitForFixedUpdate ();

        // Update players new position
        playerInput.position = transform.position;

        // Send processed input back to client for reconciliation
        ServerSend.PlayerInputProcessed ( Id, GetBytes ( playerInput ) );
    }

    private void ApplyInput ( Frame playerInput )
    {
        if ( !m_receivedInput )
        {
            return;
        }

        // Movement / Run / Jump / Crouch / Prone
        Vector2 inputDirection = Vector2.zero;
        if ( playerInput.inputs [ 0 ] ) // Forward input
        {
            inputDirection.y += 1;
        }
        if ( playerInput.inputs [ 1 ] ) // Backward input
        {
            inputDirection.y -= 1;
        }
        if ( playerInput.inputs [ 2 ] ) // Left input
        {
            inputDirection.x -= 1;
        }
        if ( playerInput.inputs [ 3 ] ) // Right input
        {
            inputDirection.x += 1;
        }
        bool runInput = playerInput.inputs [ 4 ]; // Run input
        bool jumpInput = playerInput.inputs [ 5 ]; // Jump input
        bool crouchInput = playerInput.inputs [ 6 ]; // Crouch input
        bool proneInput = playerInput.inputs [ 7 ]; // Prone input

        // Send movement-related inputs to movement controller
        m_movementController.Movement ( inputDirection, runInput, jumpInput, crouchInput, proneInput );

        // Look rotation
        transform.rotation = playerInput.rot;

        // Set shoot position crouch/prone offsets
        if ( crouchInput ) // Crouch input
        {
            m_shootOriginCrouchProneOffset = new Vector3 ( 0, -0.75f, 0 );
        }
        else if ( proneInput ) // Prone input
        {
            m_shootOriginCrouchProneOffset = new Vector3 ( 0, -1.5f, 0 );
        }
        else // Standing input (no crouch or prone input)
        {
            m_shootOriginCrouchProneOffset = Vector3.zero;
        }
    }

    public void Shoot ( Vector3 _shootDirection, float _damage, string _gunshotClip, float _gunshotVolume, float _minDistance, float _maxDistance )
    {
        // Shoot audio
        ServerSend.PlayAudioClip ( Id, _gunshotClip, _gunshotVolume, transform.position, _minDistance, _maxDistance );

        // Adjust shoot origin position
        m_shootOrigin.position = transform.position + m_shootOriginInitialOffset + m_shootOriginCrouchProneOffset;

        if ( Physics.Raycast ( m_shootOrigin.position, _shootDirection, out RaycastHit _hit, 500f ) )
        {
            if ( _hit.collider.CompareTag ( PLAYER_TAG ) )
            {
                // Get player hit
                Player playerHit = _hit.collider.GetComponent<Player> ();

                // Deal damage to player hit
                playerHit.TakeDamage ( _damage );

                // Send hitmarker to shooter
                ServerSend.Hitmarker ( Id, playerHit.IsDead ? 1 : 0 );

                // Player hit blood effect
                ServerSend.SpawnHitObject ( ( int ) ShootableObjectsManager.HitObjects.Skin, _hit.point, _hit.normal );
            }
            else if ( _hit.collider.CompareTag ( "TestPlayer" ) ) // DEBUG
            {
                ServerSend.Hitmarker ( Id, 0 );
            }
            else
            {
                ShootableObjectsManager.Instance.ProjectileHit ( _hit, _damage );
            }
        }
    }

    public void TakeDamage ( float _damage )
    {
        if ( IsDead )
        {
            return;
        }

        Health -= _damage;
        Health = Mathf.Max ( 0, Health );
        ServerSend.PlayerHealth ( Id, Health );

        if ( Health <= 0 )
        {
            m_controller.enabled = false;
            m_motor.enabled = false;
            m_movementController.Movement ( Vector2.zero, false, false, false, false );
            StartCoroutine ( DeathSequence () );
        }
    }

    private IEnumerator DeathSequence ()
    {
        yield return new WaitForSeconds ( 0.1f );

        Transform respawnPoint = RespawnPointManager.Instance.GetRandomPoint ();
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        ServerSend.PlayerPosition ( Id, transform.position );
        ServerSend.PlayerRotation ( Id, transform.rotation );

        // RESPAWN
        yield return new WaitForSeconds ( Constants.PLAYER_RESPAWN_DELAY );

        Reinitialize ();
        ServerSend.PlayerRespawned ( Id );
    }

    #region Util

    private Frame FromBytes ( byte [] arr )
    {
        Frame str = new Frame ();

        int size = Marshal.SizeOf ( str );
        IntPtr ptr = Marshal.AllocHGlobal ( size );

        Marshal.Copy ( arr, 0, ptr, size );

        str = ( Frame ) Marshal.PtrToStructure ( ptr, str.GetType () );
        Marshal.FreeHGlobal ( ptr );

        return str;
    }

    private byte [] GetBytes ( Frame str )
    {
        int size = Marshal.SizeOf ( str );
        byte [] arr = new byte [ size ];

        IntPtr ptr = Marshal.AllocHGlobal ( size );
        Marshal.StructureToPtr ( str, ptr, true );
        Marshal.Copy ( ptr, arr, 0, size );
        Marshal.FreeHGlobal ( ptr );
        return arr;
    }

    private void OnValidate ()
    {
        if ( Inventory != null )
        {
            Inventory.OnValidate ();
        }
    }

    #endregion
}
