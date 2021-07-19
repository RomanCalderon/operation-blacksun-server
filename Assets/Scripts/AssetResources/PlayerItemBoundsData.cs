using System.Linq;
using System.Collections;
using UnityEngine;
using PlayerItemExporter;

public class PlayerItemBoundsData : MonoBehaviour
{
    #region Members

    #region Singleton

    public static PlayerItemBoundsData Instance
    {
        get
        {
            if ( m_instance == null )
            {
                m_instance = Instantiate ( Resources.Load<PlayerItemBoundsData> ( "PlayerItemBoundsData" ) );
                m_instance.Initialize ();
                return m_instance;
            }
            return m_instance;
        }
    }
    private static PlayerItemBoundsData m_instance;

    #endregion

    [SerializeField]
    private string m_dataPath = null;

    [SerializeField]
    private BoundsData [] m_boundsData = null;

    #endregion

    private void Initialize ()
    {
        if ( JsonFileUtility.ReadFromFile ( m_dataPath, out string json ) )
        {
            m_boundsData = JsonUtility.FromJson<BoundsDataCollection> ( json ).BoundsData;
        }
        else
        {
            Debug.LogError ( $"Failed to read json from file {m_dataPath}" );
        }
        //m_boundsData = PlayerItemExporterTool.Instance.GetBoundsData ();
    }

    public BoundsData GetBoundsData ( string id )
    {
        return m_boundsData.FirstOrDefault ( b => b.Id == id );
    }
}
