using System.IO;
using UnityEngine;

public class NetworkedRigidbody : MonoBehaviour
{
    #region Models

    [System.Serializable]
    public struct NetworkData
    {
        public string Id;
        public Vector3 Position;
        public Vector3 Rotation;

        public NetworkData ( string id, Vector3 position, Vector3 rotation )
        {
            Id = id;
            Position = position;
            Rotation = rotation;
        }

        public byte [] ToArray ()
        {
            MemoryStream stream = new MemoryStream ();
            BinaryWriterExtended writer = new BinaryWriterExtended ( stream );

            writer.Write ( Id );
            writer.Write ( Position );
            writer.Write ( Rotation );

            return stream.ToArray ();
        }

        public static NetworkData FromArray ( byte [] bytes )
        {
            BinaryReaderExtended reader = new BinaryReaderExtended ( new MemoryStream ( bytes ) );
            NetworkData d = default;

            d.Id = reader.ReadString ();
            d.Position = reader.ReadVector3 ();
            d.Rotation = reader.ReadVector3 ();

            return d;
        }
    }

    #endregion

    private string m_id = null;
    private bool m_isInitialized = false;

    public void Initialize ( string id )
    {
        if ( !m_isInitialized )
        {
            m_isInitialized = true;
            NetworkedRigidbodyManager.Instance.AddBody ( m_id = id, this );
        }
    }

    public byte [] GetData ()
    {
        NetworkData instanceData = new NetworkData ( m_id, transform.position, transform.eulerAngles );
        return instanceData.ToArray ();
    }

    public void SetData ( NetworkData data )
    {
        transform.position = data.Position;
        transform.eulerAngles = data.Rotation;
    }

    private void OnDestroy ()
    {
        if ( m_isInitialized )
        {
            NetworkedRigidbodyManager.Instance.RemoveBody ( m_id );
        }
    }
}
