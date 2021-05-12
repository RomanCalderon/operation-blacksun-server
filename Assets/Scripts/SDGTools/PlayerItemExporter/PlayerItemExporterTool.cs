using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InventorySystem.PlayerItems;
using PlayerItemExporter;

public class PlayerItemExporterTool : EditorWindowSingleton<PlayerItemExporterTool>
{
    #region Constants

    // GUI
    private const int MIN_WINDOW_SIZE_X = 380;
    private const int MIN_WINDOW_SIZE_Y = 360;
    private const int GUI_PADDING = 5;
    private const int BUTTON_HEIGHT = 30;
    private const int BUTTON_WIDTH = 100;

    // Data
    private const string EXPORT_FILENAME = "player-item-bounds.json";

    // Save/Load keys
    private const string IMPORT_PATH_KEY = "import-path";
    private const string EXPORT_DIRECTORY_KEY = "export-path";

    #endregion

    // New item data
    [SerializeField]
    private PlayerItem m_newPlayerItem = null;
    [SerializeField]
    private GameObject m_newSource = null;

    // Staged Bounds data
    [SerializeField]
    private List<BoundsData> m_boundsData = null;

    // Import
    private string m_importFile = null;

    // Export
    private string m_exportDirectory = null;


    // Editor GUI
    private Vector2 m_scrollPosition;

    #region Initialization

    [MenuItem ( "Window/SDG/Player Item Exporter" )]
    public static void ShowWindow ()
    {
        EditorWindow window = GetWindow ( typeof ( PlayerItemExporterTool ), false, "Player Item Exporter" );
        window.minSize = new Vector2 ( MIN_WINDOW_SIZE_X, MIN_WINDOW_SIZE_Y );
        window.Repaint ();
    }

    private void Awake ()
    {
        LoadExporterData ();
    }

    private void OnDestroy ()
    {
        SaveExporterData ();
    }

    #endregion

    #region GUI

    private void OnGUI ()
    {
        ScriptableObject scriptableObj = this;
        SerializedObject serializedObject = new SerializedObject ( scriptableObj );

        // Staged PlayerItemData list
        SerializedProperty stagedBoundsData = serializedObject.FindProperty ( "m_boundsData" );

        Rect screenRect = new Rect ( GUI_PADDING, GUI_PADDING, position.width - GUI_PADDING * 2, position.height - GUI_PADDING * 2 );
        GUILayout.BeginArea ( screenRect );

        // Begin scrollview
        m_scrollPosition = EditorGUILayout.BeginScrollView ( m_scrollPosition );

        #region GUI

        #region New PlayerItem Data

        GUILayout.BeginVertical ( "New Player Item", "window" );

        // Help box
        EditorGUILayout.HelpBox ( "Add your PlayerItem and related GameObject assets then click \"Stage\" to stage it.", MessageType.Info, true );
        EditorGUILayout.Separator ();

        m_newPlayerItem = ( PlayerItem ) EditorGUILayout.ObjectField ( "Player Item", m_newPlayerItem, typeof ( PlayerItem ), false );
        m_newSource = ( GameObject ) EditorGUILayout.ObjectField ( "Source Object", m_newSource, typeof ( GameObject ), false );

        EditorGUILayout.Separator ();

        EditorGUI.BeginDisabledGroup ( m_newPlayerItem == null || m_newSource == null );

        if ( GUILayout.Button ( "Stage", GUILayout.Height ( BUTTON_HEIGHT ) ) )
        {
            serializedObject.ApplyModifiedProperties ();

            // Null check
            if ( m_newPlayerItem == null || m_newSource == null )
            {
                return;
            }
            // Add new PlayerItem data to stage list
            serializedObject.ApplyModifiedProperties ();

            // Generate Bounds data
            GenerateBoundsData ( new PlayerItemData ( m_newPlayerItem.Id, m_newSource ) );

            // Save data
            SaveExporterData ();

            // Clear new object fields
            m_newPlayerItem = null;
            m_newSource = null;
        }

        EditorGUI.EndDisabledGroup ();

        GUILayout.EndVertical ();

        #endregion

        EditorGUILayout.Separator ();

        #region Staged PlayerItem Data

        // Display list size of bounds data vs all PlayerItem Data added to list
        int boundsDataCount = m_boundsData.Count;
        GUILayout.BeginVertical ( $"Bounds Data Preview | Staged: {boundsDataCount}", "window" );

        EditorGUILayout.PropertyField ( stagedBoundsData, true );
        serializedObject.ApplyModifiedProperties ();

        GUILayout.EndVertical ();

        #endregion

        EditorGUILayout.Separator ();

        #region Import

        GUILayout.BeginVertical ( "Import", "window" );

        // Import directory
        EditorGUILayout.LabelField ( "Import file: ", string.IsNullOrEmpty ( m_importFile ) ? "(not specified)" : m_importFile );

        GUILayout.BeginHorizontal ();

        GUILayout.FlexibleSpace ();

        // File browser button
        if ( GUILayout.Button ( "Browse...", GUILayout.MinWidth ( BUTTON_WIDTH ), GUILayout.Height ( 20 ) ) )
        {
            m_importFile = EditorUtility.OpenFilePanel ( "Select import directory...", m_importFile, "json" );
        }

        EditorGUI.BeginDisabledGroup ( string.IsNullOrEmpty ( m_importFile ) );

        if ( GUILayout.Button ( "Open", GUILayout.MinWidth ( BUTTON_WIDTH ), GUILayout.Height ( 20 ) ) )
        {
            ShowExplorer ( m_importFile );
        }

        EditorGUI.EndDisabledGroup ();

        GUILayout.EndHorizontal ();

        EditorGUILayout.Separator ();

        EditorGUI.BeginDisabledGroup ( string.IsNullOrEmpty ( m_importFile ) );

        // Import button
        if ( GUILayout.Button ( $"Import", GUILayout.Height ( BUTTON_HEIGHT ) ) )
        {
            ImportData ();
        }

        EditorGUI.EndDisabledGroup ();

        GUILayout.EndVertical ();

        #endregion

        EditorGUILayout.Separator ();

        #region Export

        GUILayout.BeginVertical ( "Export", "window" );

        // Export directory
        EditorGUILayout.LabelField ( "Export directory: ", string.IsNullOrEmpty ( m_exportDirectory ) ? "(not specified)" : m_exportDirectory );

        GUILayout.BeginHorizontal ();

        GUILayout.FlexibleSpace ();

        // File browser button
        if ( GUILayout.Button ( "Browse...", GUILayout.MinWidth ( BUTTON_WIDTH ), GUILayout.Height ( 20 ) ) )
        {
            m_exportDirectory = EditorUtility.OpenFolderPanel ( "Select export directory...", "", "" );
        }

        EditorGUI.BeginDisabledGroup ( string.IsNullOrEmpty ( m_exportDirectory ) );

        if ( GUILayout.Button ( "Open", GUILayout.MinWidth ( BUTTON_WIDTH ), GUILayout.Height ( 20 ) ) )
        {
            ShowExplorer ( m_exportDirectory );
        }

        EditorGUI.EndDisabledGroup ();

        GUILayout.EndHorizontal ();

        EditorGUILayout.Separator ();

        EditorGUI.BeginDisabledGroup ( m_boundsData.Count == 0 || string.IsNullOrEmpty ( m_exportDirectory ) );

        // Export button
        if ( GUILayout.Button ( $"Export {boundsDataCount} objects", GUILayout.Height ( BUTTON_HEIGHT ) ) )
        {
            if ( m_boundsData == null || m_boundsData.Count == 0 )
            {
                Debug.LogWarning ( "Error exporting data. Bounds Data collection is null or empty." );
                return;
            }
            ExportData ();
        }

        EditorGUI.EndDisabledGroup ();

        GUILayout.EndVertical ();

        #endregion

        GUILayout.FlexibleSpace ();

        #endregion

        // End scrollview
        EditorGUILayout.EndScrollView ();

        GUILayout.EndArea ();
    }

    #endregion

    #region Accessors

    public BoundsData [] GetBoundsData ()
    {
        if ( m_boundsData == null || m_boundsData.Count == 0 )
        {
            ImportData ();
        }
        if ( m_boundsData == null )
        {
            return null;
        }
        return m_boundsData.ToArray ();
    }

    public BoundsData GetBoundsData ( string id )
    {
        if ( m_boundsData == null || string.IsNullOrEmpty ( id ) )
        {
            return new BoundsData ();
        }
        return m_boundsData.FirstOrDefault ( b => b.Id == id );
    }

    #endregion

    #region Importer

    private void ImportData ()
    {
        if ( JsonFileUtility.ReadFromFile ( m_importFile, out string json ) )
        {
            // Save import path
            PlayerPrefs.SetString ( IMPORT_PATH_KEY, m_importFile );

            if ( TryConvertFromJson ( json, out BoundsDataCollection data ) )
            {
                m_boundsData = new List<BoundsData> ( data.BoundsData );
            }
            else
            {
                Debug.LogError ( "Error converting imported json file." );
            }
        }
        else
        {
            Debug.LogError ( $"Error reading Json file from {m_importFile}" );
        }
    }

    private bool TryConvertFromJson ( string json, out BoundsDataCollection data )
    {
        data = new BoundsDataCollection ( null );

        // Only export data if there's at least one
        // BoundsData object in collection
        if ( !string.IsNullOrEmpty ( json ) )
        {
            data = JsonUtility.FromJson<BoundsDataCollection> ( json );
            return true;
        }
        Debug.LogError ( "Error importing data. BoundsData array is null or empty." );
        return false;
    }

    #endregion

    #region Exporter

    private void ExportData ()
    {
        if ( TryConvertToJson ( out string jsonData ) )
        {
            string path = Path.Combine ( m_exportDirectory, EXPORT_FILENAME );
            if ( JsonFileUtility.WriteToFile ( path, jsonData ) )
            {
                Debug.Log ( $"Successfully exported Bounds data to: {path}" );
            }
            else
            {
                Debug.LogError ( $"Export failed. Couldn't write Json data to: {path}" );
            }
        }
    }

    private bool TryConvertToJson ( out string jsonString )
    {
        jsonString = null;

        // Only export data if there's at least one
        // BoundsData object in collection
        if ( m_boundsData != null || m_boundsData.Count > 0 )
        {
            BoundsDataCollection dataCollection = new BoundsDataCollection ( m_boundsData.ToArray () );
            jsonString = JsonUtility.ToJson ( dataCollection, true );
            return true;
        }
        Debug.LogError ( "Error exporting data. BoundsData array is null or empty." );
        return false;
    }

    #endregion

    #region Editor Data

    #region Saving

    private void SaveExporterData ()
    {
        ExporterDataManager.SaveData ( m_boundsData.ToArray () );

        PlayerPrefs.SetString ( EXPORT_DIRECTORY_KEY, m_exportDirectory );
    }

    #endregion

    #region Loading

    private void LoadExporterData ()
    {
        m_boundsData = new List<BoundsData> ();

        if ( ExporterDataManager.LoadData ( out BoundsDataCollection data ) )
        {
            m_boundsData = new List<BoundsData> ( data.BoundsData );
        }

        m_importFile = PlayerPrefs.GetString ( IMPORT_PATH_KEY );
        m_exportDirectory = PlayerPrefs.GetString ( EXPORT_DIRECTORY_KEY );
    }

    #endregion

    #endregion

    #region Util

    #region Bounds calculations

    private void GenerateBoundsData ( PlayerItemData playerItemData )
    {
        if ( playerItemData.Source == null )
        {
            Debug.LogWarning ( $"PlayerItemData [{playerItemData.PlayerItemId}] is missing Source Object reference. Skipping this item." );
            return;
        }
        // Get bounds for PlayerItem source
        Bounds bounds = GetColliderBounds ( playerItemData.Source );

        // Add bounds data to list
        m_boundsData.Add ( new BoundsData ( playerItemData.PlayerItemId, bounds.center, bounds.size ) );
    }

    private Bounds GetColliderBounds ( GameObject assetModel )
    {
        GameObject assetInstance = Instantiate ( assetModel, Vector3.zero, Quaternion.identity );

        // Start with root object's bounds
        Bounds bounds = new Bounds ( Vector3.zero, Vector3.zero );
        if ( assetInstance.transform.TryGetComponent<Renderer> ( out var mainRenderer ) )
        {
            // New Bounds() will include 0,0,0 which you may not want to Encapsulate
            // because the vertices of the mesh may be way off the model's origin,
            // so instead start with the first renderer bounds and Encapsulate from there
            bounds = mainRenderer.bounds;
        }

        // Encapsulate all child renderers
        Transform [] descendants = assetInstance.GetComponentsInChildren<Transform> ();
        foreach ( Transform desc in descendants )
        {
            if ( desc.TryGetComponent<Renderer> ( out var childRenderer ) )
            {
                if ( bounds.extents == Vector3.zero )
                    bounds = childRenderer.bounds;
                bounds.Encapsulate ( childRenderer.bounds );
            }
        }

        // Destroy instance
        DestroyImmediate ( assetInstance );

        return bounds;
    }

    #endregion

    private void ShowExplorer ( string path )
    {
        //path = Path.Combine ( path, EXPORT_FILENAME );
        path = path.Replace ( @"/", @"\" );
        System.Diagnostics.Process.Start ( "explorer.exe", "/open," + path );
    }

    #endregion
}
