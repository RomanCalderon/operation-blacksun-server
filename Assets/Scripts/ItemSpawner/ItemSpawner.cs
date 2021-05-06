using System;
using System.IO;
using UnityEngine;

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

    [Header ( "Interaction Settings" )]
    [SerializeField]
    private PickupInstanceConfig m_config;
    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;

    private Interactable m_interactableInstance = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_spawnerId = m_nextSpawnerId;
        m_nextSpawnerId++;
        ItemSpawnerManager.Instance.AddItem ( m_spawnerId, this );
        SpawnItem ( true );
    }

    /// <summary>
    /// Spawns configured PickupInstance at this spawner's position with an optional <paramref name="randomRotationY"/>.
    /// Uses Quaternion.identity if <paramref name="randomRotationY"/> is false.
    /// </summary>
    /// <param name="randomRotationY">Use random rotation on y-axis.</param>
    private void SpawnItem ( bool randomRotationY = false )
    {
        Quaternion rotation = randomRotationY ? Quaternion.Euler ( 0, UnityEngine.Random.value * 360, 0 ) : Quaternion.identity;
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, transform.position, rotation, transform );
        instance.Initialize ( m_config, ItemPickedUp );
        m_interactableInstance = instance;
    }

    /// <summary>
    /// Spawns configured PickupInstance at <paramref name="position"/> with <paramref name="rotation"/>.
    /// </summary>
    /// <param name="position">Spawn position in worldspace.</param>
    /// <param name="rotation">Spawn rotation.</param>
    private void SpawnItem ( Vector3 position, Quaternion rotation )
    {
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, position, rotation, transform );
        instance.Initialize ( m_config, ItemPickedUp );
        m_interactableInstance = instance;
    }

    private void ItemPickedUp ()
    {
        ServerSend.DestroyItem ( m_spawnerId );
        ItemSpawnerManager.Instance.RemoveItem ( m_spawnerId );
    }

    #region Accessors

    public byte [] GetSpawnerData ( string [] accessKeys )
    {
        if ( !HasItem () )
        {
            Debug.LogError ( "Failed retrieving spawner data. PickupInstance not configured." );
            return null;
        }

        byte [] itemData = m_interactableInstance != null ? m_interactableInstance.GetData ( accessKeys ) : null;
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

    private bool HasItem ()
    {
        return m_config.PlayerItem != null;
    }

    #endregion
}
