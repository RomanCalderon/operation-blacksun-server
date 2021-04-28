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
    public string ItemId { get => m_playerItem.Id; }
    public int ItemQuantity { get => m_quantity; }

    [Header ( "Pickup Instance Settings" )]
    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;
    private Interactable m_interactableInstance = null;
    [SerializeField]
    private bool m_isInteractable = true;
    [SerializeField]
    private string m_accessKey = null;

    [SerializeField]
    private PlayerItem m_playerItem = null;
    [SerializeField]
    private int m_quantity = 1;

    // Start is called before the first frame update
    void Start ()
    {
        m_spawnerId = m_nextSpawnerId;
        m_nextSpawnerId++;
        ItemSpawnerManager.Instance.AddItem ( m_spawnerId, this );
        SpawnItem ( m_playerItem, transform.position, ItemPickedUp, true );
    }

    /// <summary>
    /// Spawns <paramref name="item"/> at <paramref name="position"/> with an optional <paramref name="randomRotationY"/>.
    /// Uses Quaternion.identity if <paramref name="randomRotationY"/> is false.
    /// </summary>
    /// <param name="item">PlayerItem to spawn as a new PickupInstance.</param>
    /// <param name="position">Spawn position in worldspace.</param>
    /// <param name="randomRotationY">Use random rotation on y-axis.</param>
    private void SpawnItem ( PlayerItem item, Vector3 position, Action pickupCallback, bool randomRotationY = false )
    {
        Quaternion rotation = randomRotationY ? Quaternion.Euler ( 0, UnityEngine.Random.value * 360, 0 ) : Quaternion.identity;
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, position, rotation, transform );
        instance.Initialize ( item, pickupCallback, m_quantity, m_isInteractable, m_accessKey );
        m_interactableInstance = instance;
    }

    /// <summary>
    /// Spawns <paramref name="item"/> at <paramref name="position"/> with <paramref name="rotation"/>.
    /// </summary>
    /// <param name="item">PlayerItem to spawn as a new PickupInstance.</param>
    /// <param name="position">Spawn position in worldspace.</param>
    /// <param name="rotation">Spawn rotation.</param>
    private void SpawnItem ( PlayerItem item, Vector3 position, Quaternion rotation, Action pickupCallback )
    {
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, position, rotation, transform );
        instance.Initialize ( item, pickupCallback, m_quantity, m_isInteractable, m_accessKey );
        m_interactableInstance = instance;
    }

    private void ItemPickedUp ()
    {
        ServerSend.DestroyItem ( m_spawnerId );
        ItemSpawnerManager.Instance.RemoveItem ( m_spawnerId );
    }

    #region Accessors

    public byte [] GetSpawnerData ()
    {
        byte [] itemData = m_interactableInstance != null ? m_interactableInstance.GetData () : null;
        return new SpawnerData ( SpawnerId, transform.position, transform.eulerAngles, ItemId, ItemQuantity, itemData ).ToArray ();
    }

    #endregion
}
