using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Status
    {
        static public Dictionary<String, Object> GetStatus(string serverUrl, string accessToken, string path, Boolean verbose = false)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);
            Debug.Assert(path != null);

            try
            {
                //  get progress
                String response = SendGetStatus(serverUrl, accessToken, path, verbose);

                // format to array
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return (Dictionary<String, Object>)json.Deserialize(response, typeof(Dictionary<String, Object>));
            }
            catch (WebException e)
            {
                // handle unexpected
                throw e;
            }
        }

        //
        // Endpoint Wrappers
        //

        // GET /v1/status
        // https://docs.oxygencloud.com/reference/infinity-drive-sync-status
        static private String SendGetStatus(string serverUrl, string accessToken, string path, Boolean verbose)
        {
            // setup request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl + "/v1/status?path=" + path + "&verbose=" + verbose);
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
            request.Method = "GET";

            // make request and return response data
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                String data = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                return data;
            }
        }
    }
}
