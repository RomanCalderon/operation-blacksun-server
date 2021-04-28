using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawnerManager : PersistentLazySingleton<ItemSpawnerManager>
{
    private Dictionary<int, ItemSpawner> m_spawners = new Dictionary<int, ItemSpawner> ();

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

    public void SendSpawners ( int clientId )
    {
        foreach ( ItemSpawner itemSpawner in m_spawners.Values )
        {
            int spawnerId = itemSpawner.SpawnerId;
            Vector3 spawnerPosition = itemSpawner.transform.position;
            Vector3 spawnerRotation = itemSpawner.transform.eulerAngles;
            string itemId = itemSpawner.ItemId;
            int quantity = itemSpawner.ItemQuantity;
            ServerSend.CreateItemSpawner ( clientId, spawnerId, spawnerPosition, spawnerRotation, itemId, quantity );
        }
    }
}
