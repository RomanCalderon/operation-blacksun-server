using InventorySystem.PlayerItems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnerManager : PersistentLazySingleton<ItemSpawnerManager>
{
    private Dictionary<int, ItemSpawner> m_spawners = new Dictionary<int, ItemSpawner> ();

    [SerializeField]
    private ItemSpawner m_itemSpawnerPrefab = null;
    [SerializeField]
    private ItemSpawner m_weaponSpawnerPrefab = null;
    [SerializeField]
    private ItemSpawner m_attachmentSpawnerPrefab = null;

    private IEnumerator Start ()
    {
        Debug.Assert ( m_itemSpawnerPrefab != null, "m_itemSpawnerPrefab is null" );
        yield return new WaitForEndOfFrame ();
        // Spawn start items
        foreach ( ItemSpawner spawner in m_spawners.Values )
        {
            spawner.SpawnItem ( true );
        }
    }

    #region Add/Remove Item

    public void AddItem ( int itemId, ItemSpawner spawner )
    {
        if ( !m_spawners.ContainsKey ( itemId ) )
        {
            m_spawners.Add ( itemId, spawner );
        }
    }

    public void RemoveItem ( int itemId )
    {
        if ( m_spawners.ContainsKey ( itemId ) )
        {
            m_spawners.Remove ( itemId );
        }
    }

    #endregion

    public void CreateClientSpawners ( int clientId )
    {
        foreach ( ItemSpawner itemSpawner in m_spawners.Values )
        {
            string [] accessKeys = Server.clients [ clientId ].GetAccessKeys ();
            byte [] spawnerData = itemSpawner.GetSpawnerData ( accessKeys );
            ServerSend.CreateItemSpawner ( clientId, spawnerData );
        }
    }

    public void SpawnItem ( PlayerItem playerItem, int quantity, Vector3 position )
    {
        if ( playerItem == null || quantity <= 0 )
        {
            return;
        }

        // Create an ItemSpawner
        ItemSpawner itemSpawner = playerItem switch
        {
            Weapon _ => Instantiate ( m_weaponSpawnerPrefab, position, Quaternion.identity, transform ), // Create a Weapon ItemSpawner
            Attachment _ => Instantiate ( m_attachmentSpawnerPrefab, position, Quaternion.identity, transform ), // Create an Attachment ItemSpawner
            _ => Instantiate ( m_itemSpawnerPrefab, position, Quaternion.identity, transform ), // Create a default ItemSpawner
        };

        itemSpawner.Initialize ();
        itemSpawner.SpawnItem ( playerItem, quantity );

        // Send spawner data to clients
        byte [] spawnerData = itemSpawner.GetSpawnerData ( new string [ 0 ] );
        ServerSend.CreateItemSpawner ( spawnerData );
    }
}
