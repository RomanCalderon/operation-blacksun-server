using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

public class ItemSpawner : MonoBehaviour
{
    private static int m_nextSpawnerId = 1;

    public int SpawnerId { get => m_spawnerId; }
    private int m_spawnerId;
    public string ItemId { get => m_playerItem.Id; }
    public int ItemQuantity { get => m_quantity; }

    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;

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
        instance.Initialize ( item, pickupCallback, m_quantity );
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
        instance.Initialize ( item, pickupCallback, m_quantity );
    }

    private void ItemPickedUp ()
    {
        ServerSend.DestroyItem ( m_spawnerId );
        ItemSpawnerManager.Instance.RemoveItem ( m_spawnerId );
    }
}
