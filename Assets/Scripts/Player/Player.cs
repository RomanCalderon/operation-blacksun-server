using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using InventorySystem;
using InventorySystem.Presets;

[RequireComponent ( typeof ( PlayerMovementController ) )]
public class Player : MonoBehaviour
{
    #region Members

    private const string PLAYER_TAG = "Player";

    public int Id { get; private set; }
    public string Username { get; private set; }
    public float Health { get; private set; }
    public bool IsDead { get { return Health == 0; } }

    public PlayerMovementController MovementController { get; private set; } = null;
    public LookOriginController LookOriginController { get; private set; } = null;

    [SerializeField]
    private float m_maxHealth = 100f;

    // Inventory
    [SerializeField]
    private Preset m_inventoryPreset = null;
    public Inventory Inventory { get; private set; } = null;

    #endregion


    private void Awake ()
    {
        MovementController = GetComponent<PlayerMovementController> ();
        LookOriginController = GetComponent<LookOriginController> ();
    }

    public void Initialize ( int _id, string _username )
    {
        Id = _id;
        Username = _username;
        Health = m_maxHealth;

        LookOriginController.Initialize ();
        Inventory = new Inventory ( this, m_inventoryPreset );
    }

    /// <summary>
    /// Use when the player is being respawned.
    /// </summary>
    private void Reinitialize ()
    {
        Health = m_maxHealth;
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

    public SimulationState CurrentSimulationState ( int simulationFrame )
    {
        return new SimulationState
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Velocity = MovementController.Velocity,
            SimulationFrame = simulationFrame,
            DeltaTime = Time.deltaTime
        };
    }

    public void Shoot ( Vector3 _shootDirection, float _damage, string _gunshotClip, float _gunshotVolume, float _minDistance, float _maxDistance )
    {
        // Shoot audio
        ServerSend.PlayAudioClip ( Id, _gunshotClip, _gunshotVolume, transform.position, _minDistance, _maxDistance );

        // Adjust shoot origin position
        Vector3 shootOrigin = transform.position + LookOriginController.ShootOrigin;

        if ( Physics.Raycast ( shootOrigin, _shootDirection, out RaycastHit _hit, 500f ) )
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

    private void OnValidate ()
    {
        if ( Inventory != null )
        {
            Inventory.OnValidate ();
        }
    }

    #endregion
}
