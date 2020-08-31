using InventorySystem;
using InventorySystem.Presets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent ( typeof ( PlayerMovementController ) )]
[RequireComponent ( typeof ( CharacterMotor ) )]
public class Player : MonoBehaviour
{
    #region Constants

    private const int NUM_BOOL_INPUTS = 8;

    #endregion

    #region Models

    private struct PlayerInputs
    {
        // Interpolation time on client
        public short lerp_msec;
        // Duration in ms of command
        public byte msec;
        // Player movement inputs
        [MarshalAs ( UnmanagedType.ByValArray, SizeConst = NUM_BOOL_INPUTS )]
        public bool [] inputs;
        // Player rotation
        public Quaternion rot;
    }

    #endregion

    #region Members

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

    private PlayerInputs m_playerInputs;
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

    public void FixedUpdate ()
    {
        if ( IsDead )
        {
            return;
        }

        ApplyInput ( m_playerInputs.inputs, m_playerInputs.rot );
    }

    public void ReceiveInput ( byte [] inputs )
    {
        m_receivedInput = true;
        m_playerInputs = FromBytes ( inputs );
    }

    public void ApplyInput ( bool [] _inputs, Quaternion _rotation )
    {
        if ( !m_receivedInput )
        {
            return;
        }

        // Movement / Run / Jump / Crouch / Prone
        Vector2 inputDirection = Vector2.zero;
        if ( m_playerInputs.inputs [ 0 ] )
        {
            inputDirection.y += 1;
        }
        if ( m_playerInputs.inputs [ 1 ] )
        {
            inputDirection.y -= 1;
        }
        if ( m_playerInputs.inputs [ 2 ] )
        {
            inputDirection.x -= 1;
        }
        if ( m_playerInputs.inputs [ 3 ] )
        {
            inputDirection.x += 1;
        }
        bool runInput = m_playerInputs.inputs [ 4 ];
        bool jumpInput = m_playerInputs.inputs [ 5 ];
        bool crouchInput = m_playerInputs.inputs [ 6 ];
        bool proneInput = m_playerInputs.inputs [ 7 ];
        m_movementController.Movement ( inputDirection, runInput, jumpInput, crouchInput, proneInput );
        
        // Look rotation
        transform.rotation = _rotation;

        // Set shoot position crouch/prone offsets
        if ( _inputs [ 6 ] ) // Crouch input
        {
            m_shootOriginCrouchProneOffset = new Vector3 ( 0, -0.75f, 0 );
        }
        else if ( _inputs [ 7 ] ) // Prone input
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
            if ( _hit.collider.CompareTag ( "Player" ) )
            {
                // Deal damage to player
                _hit.collider.GetComponent<Player> ().TakeDamage ( _damage );

                // Player hit object (blood effect)
                ServerSend.SpawnHitObject ( ( int ) ShootableObjectsManager.HitObjects.Skin, _hit.point, _hit.normal );
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

    private PlayerInputs FromBytes ( byte [] arr )
    {
        PlayerInputs str = new PlayerInputs ();

        int size = Marshal.SizeOf ( str );
        IntPtr ptr = Marshal.AllocHGlobal ( size );

        Marshal.Copy ( arr, 0, ptr, size );

        str = ( PlayerInputs ) Marshal.PtrToStructure ( ptr, str.GetType () );
        Marshal.FreeHGlobal ( ptr );

        return str;
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
