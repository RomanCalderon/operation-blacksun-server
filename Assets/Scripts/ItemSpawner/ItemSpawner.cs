using System;
using System.IO;
using UnityEngine;
using InventorySystem.PlayerItems;

public class ItemSpawner : MonoBehaviour
{
    #region Models

    [Serializable]
    public struct SpawnerData
    {
        public int SpawnerId;
        public Vector3 SpawnerPosition;
        public Vector3 SpawnerRotation;
        public string ItemId;
        public int ItemQuantity;
        public byte [] InteractableData;

        public SpawnerData ( int spawnerId, Vector3 position, Vector3 rotation, string itemId, int itemQuantity, byte [] itemData )
        {
            SpawnerId = spawnerId;
            SpawnerPosition = position;
            SpawnerRotation = rotation;
            ItemId = itemId;
            ItemQuantity = itemQuantity;
            InteractableData = itemData;
        }

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriterExtended writer = new BinaryWriterExtended ( stream );

            writer.Write ( SpawnerId );
            writer.Write ( SpawnerPosition );
            writer.Write ( SpawnerRotation );
            writer.Write ( ItemId );
            writer.Write ( ItemQuantity );
            writer.Write ( InteractableData.Length );
            writer.Write ( InteractableData );

            return stream.ToArray ();
        }

        public static SpawnerData FromArray ( byte [] bytes )
        {
            BinaryReaderExtended reader = new BinaryReaderExtended ( new MemoryStream ( bytes ) );
            SpawnerData s = default;

            s.SpawnerId = reader.ReadInt32 ();
            s.SpawnerPosition = reader.ReadVector3 ();
            s.SpawnerRotation = reader.ReadVector3 ();
            s.ItemId = reader.ReadString ();
            s.ItemQuantity = reader.ReadInt32 ();
            s.InteractableData = reader.ReadBytes ( reader.ReadInt32 () );

            return s;
        }
    }

    #endregion

    private static int m_nextSpawnerId = 1;

    public int SpawnerId { get => m_spawnerId; }
    private int m_spawnerId;

    private bool m_initialized = false;

    [Header ( "Interaction Settings" )]
    [SerializeField]
    private PickupInstanceConfig m_config;
    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;

    private Interactable m_interactableInstance = null;

    #region Initialization

    // Start is called before the first frame update
    private void Start ()
    {
        Initialize ();
    }

    public void Initialize ()
    {
        if ( m_initialized )
        {
            return;
        }
        m_initialized = true;
        m_spawnerId = m_nextSpawnerId;
        m_nextSpawnerId++;
        ItemSpawnerManager.Instance.AddItem ( m_spawnerId, this );
    }

    #endregion

    #region Spawn Item

    /// <summary>
    /// Spawns pre-configured PickupInstance at this spawner's position with an optional <paramref name="randomRotationY"/>.
    /// Uses Quaternion.identity if <paramref name="randomRotationY"/> is false.
    /// </summary>
    /// <param name="randomRotationY">Use random rotation on y-axis.</param>
    public void SpawnItem ( bool randomRotationY = false )
    {
        if ( m_config == null || m_config.PlayerItem == null || m_config.Quantity <= 0 )
        {
            return;
        }
        Quaternion rotation = randomRotationY ? Quaternion.Euler ( 0, UnityEngine.Random.value * 360, 0 ) : Quaternion.identity;
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, transform.position, rotation, transform );
        instance.Initialize ( m_config, ItemPickedUp );
        m_interactableInstance = instance;
    }

    /// <summary>
    /// Spawns PickupInstance from <paramref name="pickupInstanceConfig"/> at this spawner's position with an optional <paramref name="randomRotationY"/>.
    /// Uses Quaternion.identity if <paramref name="randomRotationY"/> is false.
    /// </summary>
    /// <param name="randomRotationY">Use random rotation on y-axis.</param>
    public void SpawnItem ( PickupInstanceConfig pickupInstanceConfig, bool randomRotationY = false )
    {
        if ( pickupInstanceConfig == null || pickupInstanceConfig.PlayerItem == null || pickupInstanceConfig.Quantity <= 0 )
        {
            return;
        }
        m_config = pickupInstanceConfig;
        Quaternion rotation = randomRotationY ? Quaternion.Euler ( 0, UnityEngine.Random.value * 360, 0 ) : Quaternion.identity;
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, transform.position, rotation, transform );
        instance.Initialize ( pickupInstanceConfig, ItemPickedUp );
        m_interactableInstance = instance;
    }

    /// <summary>
    /// Spawns configured PickupInstance at this spawner's position with an optional <paramref name="randomRotationY"/>.
    /// Uses Quaternion.identity if <paramref name="randomRotationY"/> is false.
    /// </summary>
    /// <param name="randomRotationY">Use random rotation on y-axis.</param>
    public void SpawnItem ( PlayerItem playerItem, int quantity, bool randomRotationY = false )
    {
        if ( playerItem == null || quantity <= 0 )
        {
            return;
        }
        m_config = CreateConfig ( playerItem, quantity );
        Quaternion rotation = randomRotationY ? Quaternion.Euler ( 0, UnityEngine.Random.value * 360, 0 ) : Quaternion.identity;
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, transform.position, rotation, transform );
        instance.Initialize ( m_config, ItemPickedUp );
        m_interactableInstance = instance;
    }

    private void ItemPickedUp ()
    {
        ServerSend.DestroyItem ( m_spawnerId );
        ItemSpawnerManager.Instance.RemoveItem ( m_spawnerId );
        Destroy ( gameObject );
    }

    #endregion

    #region Accessors

    public byte [] GetSpawnerData ( string [] accessKeys )
    {
        if ( !HasItem () )
        {
            Debug.LogError ( "Failed retrieving spawner data. PickupInstance not configured properly." );
            return null;
        }

        byte [] itemData = m_interactableInstance != null ? m_interactableInstance.GetData ( accessKeys ) : null;
        Debug.Log ( $"GetSpawnerData() - SpawnerId={SpawnerId}" );
        return new SpawnerData (
            SpawnerId,
            transform.position,
            transform.eulerAngles,
            m_config.PlayerItem.Id,
            m_config.Quantity,
            itemData ).ToArray ();
    }

    #endregion

    #region Util

    private PickupInstanceConfig CreateConfig ( PlayerItem playerItem, int quantity )
    {
        PickupInstanceConfig config = ScriptableObject.CreateInstance ( typeof ( PickupInstanceConfig ) ) as PickupInstanceConfig;
        config.PlayerItem = playerItem;
        config.Quantity = quantity;
        return config;
    }

    private bool HasItem ()
    {
        return m_config.PlayerItem != null && m_config.Quantity > 0;
    }

    #endregion
}
