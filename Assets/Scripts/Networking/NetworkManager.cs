using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;

    [SerializeField]
    private int m_maxPlayers = 50;
    public int Port { get => m_port; }
    [SerializeField]
    private int m_port = 26950;

    private void Awake ()
    {
        if ( instance == null )
        {
            instance = this;
        }
        else if ( instance != this )
        {
            Debug.Log ( "Instance already exists, destroying object!" );
        }
    }

    private void Start ()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start ( m_maxPlayers, m_port );
    }

    private void OnApplicationQuit ()
    {
        Server.Stop ();
    }

    public Player InstantiatePlayer ()
    {
        return Instantiate ( playerPrefab, new Vector3 ( 0f, 1.25f, 0f ), Quaternion.identity ).GetComponent<Player> ();
    }
}
