using InventorySystem;
using InventorySystem.Presets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent ( typeof ( PlayerMovementController ) )]
[RequireComponent ( typeof ( CharacterMotor ) )]
public class Player : MonoBehaviour
{
    private const int NUM_PLAYER_INPUTS = 8;

    public int Id { get; private set; }
    public string Username { get; private set; }

    private CharacterController m_controller = null;
    private CharacterMotor m_motor = null;
    private PlayerMovementController m_movementController = null;

    [SerializeField]
    private Transform m_shootOrigin = null;
    private Vector3 m_shootOriginOffset;
    [SerializeField]
    private float m_maxHealth = 100f;
    public float Health { get; private set; }
    public bool IsDead { get { return Health == 0; } }
    private bool [] m_inputs;

    // Inventory
    [SerializeField]
    private Preset m_inventoryPreset = null;
    public Inventory Inventory { get; private set; } = null;


    private void Awake ()
    {
        m_controller = GetComponent<CharacterController> ();
        m_motor = GetComponent<CharacterMotor> ();
        m_movementController = GetComponent<PlayerMovementController> ();
    }

    private void Start ()
    {
        m_shootOriginOffset = m_shootOrigin.position - transform.position;
    }

    public void Initialize ( int _id, string _username )
    {
        Id = _id;
        Username = _username;
        Health = m_maxHealth;

        m_inputs = new bool [ NUM_PLAYER_INPUTS ];
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

        Vector2 inputDirection = Vector2.zero;
        if ( m_inputs [ 0 ] )
        {
            inputDirection.y += 1;
        }
        if ( m_inputs [ 1 ] )
        {
            inputDirection.y -= 1;
        }
        if ( m_inputs [ 2 ] )
        {
            inputDirection.x -= 1;
        }
        if ( m_inputs [ 3 ] )
        {
            inputDirection.x += 1;
        }

        m_movementController.Movement ( inputDirection, m_inputs [ 4 ], m_inputs [ 5 ], ( m_inputs [ 6 ] || m_inputs [ 7 ] ) );
    }

    public void SetInput ( bool [] _inputs, Quaternion _rotation )
    {
        m_inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Shoot ( Vector3 _shootDirection, float _damage, string _gunshotClip, float _gunshotVolume, float _minDistance, float _maxDistance )
    {
        // Shoot audio
        ServerSend.PlayAudioClip ( Id, _gunshotClip, _gunshotVolume, transform.position, _minDistance, _maxDistance );

        // Adjust shoot origin position
        m_shootOrigin.position = transform.position + m_shootOriginOffset + new Vector3 ( 0, m_controller.center.y, 0 );

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
