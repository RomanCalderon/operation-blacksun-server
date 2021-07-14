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
        // DEBUG - Revert to private
        public string ip;
        public short port;
        public int sceneIndex;
        private short playerCount;
        [SerializeField]
        private short maxPlayers;

        public ServerRegistrationRequestData ( string name, string ip, short port, int sceneIndex, short maxPlayers )
        {
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.sceneIndex = sceneIndex;
            this.playerCount = 0;
            this.maxPlayers = maxPlayers;
        }
    }

    [System.Serializable]
    public class ServerRegistrationResponseData
    {
        [System.Serializable]
        public class LinkContainer
        {
            [System.Serializable]
            public struct Link
            {
                public string href;
            }

            public Link self;
            public Link gameservers;
        }

        public int id;
        public string name;
        public string ip;
        public short port;
        public string sceneIndex;
        public string playerCount;
        public string maxPlayers;
        public LinkContainer _links;

        public ServerRegistrationResponseData ( int id, string name, string ip, short port, string sceneIndex, string playerCount, string maxPlayers, LinkContainer links )
        {
            this.id = id;
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.sceneIndex = sceneIndex;
            this.playerCount = playerCount;
            this.maxPlayers = maxPlayers;
            _links = links;
        }

        public override string ToString ()
        {
            return $"Server registration response: id={id} | name={name}";
        }
    }

    #endregion

    #region Members

    private const string REQUEST_URL_RELATIVE = "/gameservers";

    [SerializeField]
    private WebServiceCommunication m_communicator = null;
    private IPManager m_ipManager = null;


    [Header ( "DEBUG" )]
    [SerializeField]
    private ServerRegistrationRequestData m_serverRegistrationRequest;
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

    private async void ServerStartup ( int port )
    {
        // Retrieve public ip address and port
        string ip = await m_ipManager.GetPublicIpInfo ();

        m_communicator.Get ( REQUEST_URL_RELATIVE, RegisterServer );
        //m_communicator.Put ( "/" + m_serverRegistrationResponse.id, requestBody, ServerRegistrationResponse );
    }

    private void RegisterServer ( string responseBody )
    {
        // TODO: Get port value
        // TODO: Create registration request at runtime
        string requestBody = JsonUtility.ToJson ( /*DEBUG*/ m_serverRegistrationRequest );

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
