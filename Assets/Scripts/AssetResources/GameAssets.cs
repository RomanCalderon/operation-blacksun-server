using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameAssets : MonoBehaviour
{
    #region Models

    public enum AudioCategories
    {
        SFX,
        SOUNDTRACK,
        OTHER
    }

    #endregion

    #region Members

    private static GameAssets m_instance;
    public static GameAssets Instance
    {
        get
        {
            if ( m_instance == null )
            {
                m_instance = Instantiate ( Resources.Load<GameAssets> ( "GameAssets" ) );
                return m_instance;
            }
            return m_instance;
        }
    }

    [Header ( "Audio Clips" )]
    [SerializeField]
    private AudioClipReference [] m_audioClipReferences = null;

    #endregion

    public string GetAudioClip ( string name )
    {
        if ( string.IsNullOrEmpty ( name ) )
        {
            return null;
        }
        return m_audioClipReferences.FirstOrDefault ( c => c.Name.Contains ( name.ToLower () ) ).Name;
    }
}

[System.Serializable]
public struct AudioClipReference
{
    public string Name;
}
