using System.Linq;
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
                Initialize ();
                return m_instance;
            }
            return m_instance;
        }
    }
    private static PlayerItemBoundsData m_instance;

    #endregion

    private static BoundsData [] m_boundsData = null;

    #endregion

    private static void Initialize ()
    {
        m_boundsData = PlayerItemExporterTool.Instance.GetBoundsData ();
    }

    public BoundsData GetBoundsData ( string id )
    {
        return m_boundsData.FirstOrDefault ( b => b.Id == id );
    }
}
