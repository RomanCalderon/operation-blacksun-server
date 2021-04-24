using System.Collections;
using UnityEngine;

[RequireComponent ( typeof ( PlayerMovementController ) )]
[RequireComponent ( typeof ( Rigidbody ) )]
[RequireComponent ( typeof ( InventoryManager ) )]
[RequireComponent ( typeof ( WeaponsController ) )]
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
    public Rigidbody Rigidbody { get; private set; } = null;
    public InventoryManager InventoryManager { get; private set; } = null;
    public WeaponsController WeaponsController { get; private set; } = null;

    [SerializeField]
    private float m_maxHealth = 100f;


    #endregion


    private void Awake ()
    {
        MovementController = GetComponent<PlayerMovementController> ();
        LookOriginController = GetComponent<LookOriginController> ();
        Rigidbody = GetComponent<Rigidbody> ();
        InventoryManager = GetComponent<InventoryManager> ();
        WeaponsController = GetComponent<WeaponsController> ();
    }

    public void Initialize ( int _id, string _username )
    {
        Id = _id;
        Username = _username;
        Health = m_maxHealth;

        LookOriginController.Initialize ();
    }

    /// <summary>
    /// Used when the player first spawns.
    /// </summary>
    public void InitializeInventory ()
    {
        InventoryManager.InitializeInventory ();
    }

    /// <summary>
    /// Resets the player to max health and
    /// reinitializes inventory.
    /// </summary>
    private void Reinitialize ()
    {
        Health = m_maxHealth;
        InventoryManager.InitializeInventory ();
    }

    public void WeaponSwitch ( int activeWeaponIndex )
    {
        if ( IsDead )
        {
            return;
        }

        WeaponsController.ActivateWeapon ( activeWeaponIndex );
    }

    public void WeaponReload ()
    {
        if ( IsDead )
        {
            return;
        }

        WeaponsController.ReloadWeapon ();
    }

    public void CancelWeaponReload ()
    {
        WeaponsController.CancelWeaponReload ();
    }

    public void Shoot ( Vector3 lookDirection )
    {
        if ( IsDead )
        {
            return;
        }

        WeaponsController.Shoot ( lookDirection );
    }

    /// <summary>
    /// Reduces health by <paramref name="_damage"/> and returns <paramref name="killedPlayer"/>
    /// as true if this player has been killed.
    /// </summary>
    /// <param name="_damage">Amount of damage to deal to this player.</param>
    /// <param name="killedPlayer">Flag for player death.</param>
    public void TakeDamage ( float _damage, out bool killedPlayer )
    {
        if ( IsDead )
        {
            killedPlayer = false;
            return;
        }

        Health -= _damage;
        Health = Mathf.Max ( 0, Health );
        ServerSend.PlayerHealth ( Id, Health );

        if ( killedPlayer = Health <= 0 )
        {
            StartCoroutine ( DeathSequence () );
        }
    }

    public void SendPlayerStateAll ( ClientInputState state )
    {
        if ( this == null )
        {
            return;
        }

        ServerSend.PlayerPosition ( Id, transform.position );
        ServerSend.PlayerRotation ( Id, transform.rotation );
        int inputMoveX = state.MoveRight ? 1 : state.MoveLeft ? -1 : 0;
        int inputMoveY = state.MoveForward ? 1 : state.MoveBackward ? -1 : 0;
        float moveSpeed = MovementController.Velocity.magnitude;
        ServerSend.PlayerMovement ( Id, inputMoveX, inputMoveY, moveSpeed, state.Run, state.Crouch );
    }

    private IEnumerator DeathSequence ()
    {
        yield return new WaitForSeconds ( 0.1f );

        Transform respawnPoint = RespawnPointManager.Instance.GetRandomPoint ();
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        SendPlayerStateAll ( new ClientInputState () { Rotation = transform.rotation } );

        // RESPAWN
        yield return new WaitForSeconds ( Constants.PLAYER_RESPAWN_DELAY );

        Reinitialize ();
        ServerSend.PlayerRespawned ( Id );
    }

    #region Util

    private void OnValidate ()
    {
        if ( InventoryManager == null )
        {
            InventoryManager = GetComponent<InventoryManager> ();
        }
        InventoryManager.OnValidate ();
    }

    #endregion
}
