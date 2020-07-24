using InventorySystem;
using InventorySystem.Presets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( CharacterController ) )]
public class Player : MonoBehaviour
{
    public int Id { get; private set; }
    public string Username { get; private set; }

    private CharacterController m_controller;
    [SerializeField]
    private Transform m_shootOrigin = null;
    [SerializeField]
    private float m_gravity = -22f;
    [SerializeField]
    private float m_moveSpeed = 4f;
    [SerializeField]
    private float m_jumpSpeed = 6.5f;
    [SerializeField]
    private float m_maxHealth = 100f;
    public float Health { get; private set; }
    public bool IsDead { get { return Health == 0; } }
    private bool [] m_inputs;
    private float m_yVelocity = 0;

    // Inventory
    [SerializeField]
    private Preset m_inventoryPreset = null;
    public Inventory Inventory { get; private set; } = null;


    private void Awake ()
    {
        m_controller = GetComponent<CharacterController> ();
    }

    private void Start ()
    {
        m_gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        m_moveSpeed *= Time.fixedDeltaTime;
        m_jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize ( int _id, string _username )
    {
        Id = _id;
        Username = _username;
        Health = m_maxHealth;

        m_inputs = new bool [ 5 ];
        Inventory = new Inventory ( this, m_inventoryPreset );
    }

    /// <summary>
    /// Use when the player is being respawned.
    /// </summary>
    private void Reinitialize ()
    {
        Health = m_maxHealth;
        m_controller.enabled = true;
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

    private void OnValidate ()
    {
        if ( Inventory != null )
        {
            Inventory.OnValidate ();
        }
    }

    public void FixedUpdate ()
    {
        if ( IsDead )
        {
            return;
        }

        Vector2 _inputDirection = Vector2.zero;
        if ( m_inputs [ 0 ] )
        {
            _inputDirection.y += 1;
        }
        if ( m_inputs [ 1 ] )
        {
            _inputDirection.y -= 1;
        }
        if ( m_inputs [ 2 ] )
        {
            _inputDirection.x -= 1;
        }
        if ( m_inputs [ 3 ] )
        {
            _inputDirection.x += 1;
        }

        Move ( _inputDirection );
    }

    private void Move ( Vector2 _inputDirection )
    {
        Vector3 _moveDirection = ( transform.right * _inputDirection.x + transform.forward * _inputDirection.y ).normalized;
        _moveDirection *= m_moveSpeed;

        if ( m_controller.isGrounded )
        {
            m_yVelocity = 0f;
            if ( m_inputs [ 4 ] )
            {
                m_yVelocity = m_jumpSpeed;
            }
        }
        m_yVelocity += m_gravity;

        _moveDirection.y = m_yVelocity;
        m_controller.Move ( _moveDirection );

        ServerSend.PlayerPosition ( Id, transform.position );
        ServerSend.PlayerRotation ( Id, transform.rotation );
        ServerSend.PlayerMovementVector ( this, _inputDirection );
    }

    public void SetInput ( bool [] _inputs, Quaternion _rotation )
    {
        m_inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot ( Vector3 _viewDirection, float _damage )
    {
        if ( Physics.Raycast ( m_shootOrigin.position, _viewDirection, out RaycastHit _hit, 500f ) )
        {
            if ( _hit.collider.CompareTag ( "Player" ) )
            {
                _hit.collider.GetComponent<Player> ().TakeDamage ( _damage );
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
}
