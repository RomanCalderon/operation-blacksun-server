using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPointManager : MonoBehaviour
{
    public static RespawnPointManager Instance;

    [SerializeField]
    private Transform [] m_respawnPoints = null;

    private void Awake ()
    {
        if ( Instance == null )
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Transform GetRandomPoint ()
    {
        return m_respawnPoints [ Random.Range ( 0, m_respawnPoints.Length ) ];
    }
}
