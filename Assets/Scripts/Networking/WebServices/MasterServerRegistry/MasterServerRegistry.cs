using System;
using UnityEngine;
using WebServiceCommunications;

[RequireComponent ( typeof ( IPManager ) )]
public class MasterServerRegistry : MonoBehaviour
{
    #region Models

    [System.Serializable]
    public class ServerRegistrationRequestData
    {
        public string name;
        public string ip;
        public ushort port;
        public byte clientSceneIndex;
        public byte playerCount;
        public byte maxPlayers;

        public ServerRegistrationRequestData ( string name, string ip, ushort port, byte clientSceneIndex, byte playerCount, byte maxPlayers )
        {
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.clientSceneIndex = clientSceneIndex;
            this.playerCount = playerCount;
            this.maxPlayers = maxPlayers;
        }
    }

    [System.Serializable]
    public class ServerRegistrationResponseData
    {
        public long id;
        public string name;
        public string ip;
        public ushort port;
        public byte clientSceneIndex;
        public byte playerCount;
        public byte maxPlayers;

        public ServerRegistrationResponseData ( long id, string name, string ip, ushort port, byte clientSceneIndex, byte playerCount, byte maxPlayers )
        {
            this.id = id;
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.clientSceneIndex = clientSceneIndex;
            this.playerCount = playerCount;
            this.maxPlayers = maxPlayers;
        }

        public override string ToString ()
        {
            return $"Server registration response: id={id} | name={name}";
        }
    }

    #endregion

    #region Members

    private const string REQUEST_URL_RELATIVE = "/api/gameserver";

    [SerializeField]
    private WebServiceCommunication m_communicator = null;
    [SerializeField]
    private NetworkManager m_networkManager = null;
    [SerializeField]
    private int m_clientSceneIndex = 1;
    private IPManager m_ipManager = null;

    [Header ( "Registration Info" )]
    [SerializeField]
    private ServerRegistrationResponseData m_serverRegistrationResponse;

    #endregion

    private void Awake ()
    {
        m_ipManager = GetComponent<IPManager> ();

        m_communicator.Initialize ();
    }

    private void OnEnable ()
    {
        Server.onServerStarted += ServerStartup;
        Server.onServerStopped += ServerShutdown;
    }

    private void OnDisable ()
    {
        Server.onServerStarted -= ServerStartup;
        Server.onServerStopped -= ServerShutdown;
    }

    #region Server Event Listeners

    private void ServerStartup ( int port )
    {
        m_communicator.Get ( REQUEST_URL_RELATIVE, PostServer );
    }

    private async void PostServer ( string responseBody )
    {
        // DEBUG
        Debug.Log ( responseBody );

        // Create registration request at runtime
        string name = $"TS_{DateTime.Now.ToShortTimeString ()}";
        string public_ip = await m_ipManager.GetPublicIpInfo ();
        ushort port = ( ushort ) m_networkManager.Port;
        byte maxPlayers = ( byte ) m_networkManager.MaxPlayers;

        ServerRegistrationRequestData request = new ServerRegistrationRequestData ( name, public_ip, port, ( byte ) m_clientSceneIndex, 0, maxPlayers );
        string requestBody = JsonUtility.ToJson ( request );

        m_communicator.Post ( REQUEST_URL_RELATIVE, requestBody, ServerRegistrationResponse );
    }

    private void ServerShutdown ()
    {
        // Return if this server was never registered
        if ( m_serverRegistrationResponse.id == 0 )
        {
            return;
        }
        m_communicator.Delete ( REQUEST_URL_RELATIVE + "/" + m_serverRegistrationResponse.id, DebugGetResponse );
    }

    #endregion

    #region Web Service Response Listeners

    private void DebugGetResponse ( string responseBody )
    {
        Debug.Log ( responseBody );
    }

    private void ServerRegistrationResponse ( string responseBody )
    {
        if ( !string.IsNullOrEmpty ( responseBody ) )
        {
            Debug.Log ( responseBody );
            m_serverRegistrationResponse = JsonUtility.FromJson<ServerRegistrationResponseData> ( responseBody );
        }
    }

    #endregion
}
