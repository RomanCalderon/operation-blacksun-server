using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;

public class LootSpawner : MonoBehaviour
{
    [SerializeField]
    private PickupInstance m_pickupInstancePrefab = null;

    [SerializeField]
    private PlayerItem m_debugPlayerItem = null;

    private PickupInstance m_debugPickupInstance = null;

    // Start is called before the first frame update
    void Start ()
    {
        // DEBUG
        SpawnLoot ( m_debugPlayerItem, new Vector3 ( 0, 5, 8 ), true );
    }

    /// <summary>
    /// Spawns <paramref name="item"/> at <paramref name="position"/> with an optional <paramref name="randomRotationY"/>.
    /// Uses Quaternion.identity if <paramref name="randomRotationY"/> is false.
    /// </summary>
    /// <param name="item">PlayerItem to spawn as a new PickupInstance.</param>
    /// <param name="position">Spawn position in worldspace.</param>
    /// <param name="randomRotationY">Use random rotation on y-axis.</param>
    private void SpawnLoot ( PlayerItem item, Vector3 position, bool randomRotationY = false )
    {
        Quaternion rotation = randomRotationY ? Quaternion.Euler ( 0, Random.value * 360, 0 ) : Quaternion.identity;
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, position, rotation, transform );
        instance.Initialize ( item );
        m_debugPickupInstance = instance;
    }

    /// <summary>
    /// Spawns <paramref name="item"/> at <paramref name="position"/> with <paramref name="rotation"/>.
    /// </summary>
    /// <param name="item">PlayerItem to spawn as a new PickupInstance.</param>
    /// <param name="position">Spawn position in worldspace.</param>
    /// <param name="rotation">Spawn rotation.</param>
    private void SpawnLoot ( PlayerItem item, Vector3 position, Quaternion rotation )
    {
        PickupInstance instance = Instantiate ( m_pickupInstancePrefab, position, rotation, transform );
        instance.Initialize ( item );
    }
}
