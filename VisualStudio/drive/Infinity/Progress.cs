using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Progress
    {
        static public ArrayList ListProgress(string serverUrl, string accessToken)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);

            try
            {
                //  get progress
                String response = SendGetProgress(serverUrl, accessToken);

                // format to array
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return (System.Collections.ArrayList)json.Deserialize(response, typeof(System.Collections.ArrayList));
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

        // GET /v1/progress
        // https://docs.oxygencloud.com/reference/infinity-drive-progress-monitoring#get-progress
        static private string SendGetProgress(string serverUrl, string accessToken)
        {
            // setup request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl + "/v1/progress");
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
