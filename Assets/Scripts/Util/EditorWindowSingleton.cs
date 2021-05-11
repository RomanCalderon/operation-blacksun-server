using UnityEngine;
using UnityEditor;

public class EditorWindowSingleton<TSelfType> : EditorWindow where TSelfType : EditorWindow
{
    private static TSelfType m_Instance = null;
    public static TSelfType FindFirstInstance ()
    {
        var windows = ( TSelfType [] ) Resources.FindObjectsOfTypeAll ( typeof ( TSelfType ) );
        if ( windows.Length == 0 )
            return null;
        return windows [ 0 ];
    }

    public static TSelfType Instance
    {
        get
        {
            if ( m_Instance == null )
            {
                m_Instance = FindFirstInstance ();
                if ( m_Instance == null )
                    m_Instance = GetWindow<TSelfType> ();
            }
            return m_Instance;
        }
    }
}
