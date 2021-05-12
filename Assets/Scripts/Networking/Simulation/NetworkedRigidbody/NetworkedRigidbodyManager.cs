using System.Collections.Generic;

public class NetworkedRigidbodyManager : LazySingleton<NetworkedRigidbodyManager>
{
    private Dictionary<string, NetworkedRigidbody> m_networkBodies = new Dictionary<string, NetworkedRigidbody> ();

    public void AddBody ( string id, NetworkedRigidbody newBody )
    {
        if ( !m_networkBodies.ContainsKey ( id ) && newBody != null )
        {
            m_networkBodies.Add ( id, newBody );
        }
    }

    public void RemoveBody ( string bodyId )
    {
        if ( m_networkBodies.ContainsKey ( bodyId ) )
        {
            m_networkBodies.Remove ( bodyId );
        }
    }

    public void SendData ()
    {
        foreach ( NetworkedRigidbody networkedBody in m_networkBodies.Values )
        {
            if ( networkedBody != null )
            {
                ServerSend.NetworkedRigidbodyData ( networkedBody.GetData () );
            }
        }
    }

    public void UpdateBody ( byte [] data )
    {
        NetworkedRigidbody.NetworkData networkData = NetworkedRigidbody.NetworkData.FromArray ( data );

        if ( m_networkBodies.ContainsKey ( networkData.Id ) )
        {
            m_networkBodies [ networkData.Id ].SetData ( networkData );
        }
    }
}
