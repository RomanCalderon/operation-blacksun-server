using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;

public class IPManager : MonoBehaviour
{
    #region Models

    [Serializable]
    public class PublicIpInfo
    {
        private readonly string ip;

        public PublicIpInfo ( string ipAddress )
        {
            ip = ipAddress;
        }

        public string GetIP ()
        {
            return ip;
        }

        public override string ToString ()
        {
            return "ip:" + ip;
        }
    }

    #endregion

    #region Members

    private const string API_URL = "https://api.ipify.org";

    #endregion

    #region Methods

    public async Task<string> GetPublicIpInfo ()
    {
        HttpClient httpClient = new HttpClient ();
        try
        {
            var result = await httpClient.GetStringAsync ( API_URL );
            string checkResult = result.ToString ();
            httpClient.Dispose ();
            return checkResult;
        }
        catch ( Exception ex )
        {
            string checkResult = "Error " + ex.ToString ();
            httpClient.Dispose ();
            return checkResult;
        }

        //WebClient myWebClient = new WebClient
        //{
        //    Credentials = CredentialCache.DefaultCredentials
        //};

        //byte [] pageData = myWebClient.DownloadData ( API_URL );
        //string pageHtml = Encoding.UTF8.GetString ( pageData );
        //PublicIpInfo info = JsonUtility.FromJson<PublicIpInfo> ( pageHtml );
        //return info;
    }

    #endregion
}
