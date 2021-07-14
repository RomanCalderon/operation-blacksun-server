using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

namespace WebServiceCommunications
{
    public class WebServiceCommunication : MonoBehaviour
    {
        [SerializeField, Tooltip ("The relative path from Application.streamingAssets")]
        private string m_configPath = null;
        private bool m_isInit = false;

        private string m_uri = null;

        public void Initialize ()
        {
            m_configPath = Path.Combine ( Application.streamingAssetsPath, m_configPath );
            if ( File.Exists ( m_configPath ) )
            {
                // Open config file and read json
                StreamReader reader = new StreamReader ( m_configPath );
                string json = reader.ReadToEnd ();
                reader.Close ();
                ConfigurationModel configuration = JsonUtility.FromJson<ConfigurationModel> ( json );

                // Get URI
                if ( configuration != null )
                {
                    m_uri = configuration.uri;
                    m_isInit = true;
                }
            }
            else
            {
                throw new Exception ( $"Failed locating config file at {m_configPath}" );
            }
        }

        #region REST Methods

        #region GET

        public void Get ( string requestUrlRelative, Action<string> response )
        {
            if ( !m_isInit ) Initialize ();
            string requestUrl = m_uri + requestUrlRelative;
            StartCoroutine ( GetEnum ( requestUrl, response ) );
        }

        private IEnumerator GetEnum ( string uri, Action<string> response )
        {
            Debug.Log ( $"GET uri={uri}" );
            UnityWebRequest request = new UnityWebRequest ( uri, "GET" );
            request.downloadHandler = new DownloadHandlerBuffer ();
            request.SetRequestHeader ( "Accept", "*/*" );

            yield return request.SendWebRequest ();

            if ( request.isNetworkError || request.isHttpError )
            {
                Debug.LogError ( "web service error (GET): " + request.error );
            }
            else
            {
                response?.Invoke ( request.downloadHandler.text );
            }
        }

        #endregion

        #region POST

        public void Post ( string requestUrlRelative, string requestJson, Action<string> response )
        {
            if ( !m_isInit ) Initialize ();
            string requestUrl = m_uri + requestUrlRelative;
            StartCoroutine ( PostEnum ( requestUrl, requestJson, response ) );
        }

        private IEnumerator PostEnum ( string uri, string requestJson, Action<string> response )
        {
            Debug.Log ( $"POST uri={uri}\nrequest={requestJson}" );
            UnityWebRequest request = new UnityWebRequest ( uri, "POST" );
            byte [] bodyRaw = Encoding.UTF8.GetBytes ( requestJson );
            request.uploadHandler = new UploadHandlerRaw ( bodyRaw );
            request.downloadHandler = new DownloadHandlerBuffer ();
            request.SetRequestHeader ( "Accept", "*/*" );
            request.SetRequestHeader ( "Content-Type", "application/json" );

            yield return request.SendWebRequest ();

            if ( request.isNetworkError || request.isHttpError )
            {
                Debug.LogError ( "web service error (POST): " + request.error );
            }
            else
            {
                response?.Invoke ( request.downloadHandler.text );
            }
        }

        #endregion

        #region PUT

        public void Put ( string requestUrlRelative, string requestJson, Action<string> response )
        {
            if ( !m_isInit ) Initialize ();
            string requestUrl = m_uri + requestUrlRelative;
            StartCoroutine ( PutEnum ( requestUrl, requestJson, response ) );
        }

        private IEnumerator PutEnum ( string uri, string requestJson, Action<string> response )
        {
            Debug.Log ( $"PUT uri={uri}\nrequest={requestJson}" );
            UnityWebRequest request = new UnityWebRequest ( uri, "PUT" );
            byte [] bodyRaw = Encoding.UTF8.GetBytes ( requestJson );
            request.uploadHandler = new UploadHandlerRaw ( bodyRaw );
            request.downloadHandler = new DownloadHandlerBuffer ();
            request.SetRequestHeader ( "Accept", "*/*" );
            request.SetRequestHeader ( "Content-Type", "application/json" );

            yield return request.SendWebRequest ();

            if ( request.isNetworkError || request.isHttpError )
            {
                Debug.LogError ( "web service error (PUT): " + request.error );
            }
            else
            {
                response?.Invoke ( request.downloadHandler.text );
            }
        }

        #endregion

        #region DELETE

        public void Delete ( string requestUrlRelative, Action<string> response )
        {
            if ( !m_isInit ) Initialize ();
            string requestUrl = m_uri + requestUrlRelative;
            StartCoroutine ( DeleteEnum ( requestUrl, response ) );
        }

        private IEnumerator DeleteEnum ( string uri, Action<string> response )
        {
            Debug.Log ( $"DELETE uri={uri}" );
            UnityWebRequest request = new UnityWebRequest ( uri, "DELETE" );

            yield return request.SendWebRequest ();

            if ( request.isNetworkError || request.isHttpError )
            {
                Debug.LogError ( "web service error (DELETE): " + request.error );
            }
            else
            {
                response?.Invoke ( request.responseCode.ToString () );
            }
        }

        #endregion

        #endregion
    }

    [Serializable]
    public class ConfigurationModel
    {
        public string uri;
    }
}
