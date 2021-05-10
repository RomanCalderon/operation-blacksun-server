using UnityEngine;

namespace PlayerItemExporter
{
    #region Data Model

    [System.Serializable]
    public struct PlayerItemData
    {
        public string PlayerItemId;
        public GameObject Source { get => m_source; }
        private readonly GameObject m_source;

        public PlayerItemData ( string playerItemId, GameObject source )
        {
            PlayerItemId = playerItemId;
            m_source = source;
        }
    }

    [System.Serializable]
    public struct BoundsData
    {
        public string Id;
        public Vector3 Center;
        public Vector3 Size;

        public BoundsData ( string id, Vector3 center, Vector3 size )
        {
            Id = id;
            Center = center;
            Size = size;
        }
    }

    [System.Serializable]
    public struct BoundsDataCollection
    {
        public BoundsData [] BoundsData;

        public BoundsDataCollection ( BoundsData [] data )
        {
            BoundsData = data;
        }
    }

    #endregion

    public class ExporterDataManager
    {
        private const string DATA_PATH = "Assets/Resources/PlayerItemExporterData/player-item-exporter-data.json";

        public static void SaveData ( BoundsData [] boundsData )
        {
            if ( boundsData == null )
            {
                Debug.LogWarning ( "BoundsData array is null." );
                return;
            }
            string json = JsonUtility.ToJson ( new BoundsDataCollection ( boundsData ) );
            JsonFileUtility.WriteToFile ( DATA_PATH, json );
        }

        public static bool LoadData ( out BoundsDataCollection data )
        {
            if ( JsonFileUtility.ReadFromFile ( DATA_PATH, out string json ) )
            {
                data = JsonUtility.FromJson<BoundsDataCollection> ( json );
                return true;
            }
            Debug.LogError ( "Failed to read from file" );
            data = new BoundsDataCollection ();
            return false;
        }

        public static bool LoadData ( string path, out BoundsDataCollection data )
        {
            if ( JsonFileUtility.ReadFromFile ( path, out string json ) )
            {
                data = JsonUtility.FromJson<BoundsDataCollection> ( json );
                return true;
            }
            Debug.LogError ( $"Failed to read from file at {path}" );
            data = new BoundsDataCollection ();
            return false;
        }
    }
}