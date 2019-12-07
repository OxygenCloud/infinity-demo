using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Error
    {
        static public ArrayList ListError(string serverUrl, string accessToken, bool upload, bool download, bool conflict, bool ignored, bool refresh)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);

            try
            {
                //  get errors
                String response = SendGetError(serverUrl, accessToken, upload, download, conflict, ignored, refresh);

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

        // GET /v1/error
        // https://docs.oxygencloud.com/reference/infinity-drive-sync-error-handling#get-sync-errors
        static private string SendGetError(string serverUrl, string accessToken, bool upload, bool download, bool conflict, bool ignored, bool refresh)
        {
            // setup query params
            ArrayList queryParams = new ArrayList();
            if (upload)
                queryParams.Add("upload=true");
            if (download)
                queryParams.Add("download=true");
            if (conflict)
                queryParams.Add("conflict=true");
            if (ignored)
                queryParams.Add("ignored=true");
            if (refresh)
                queryParams.Add("refrsh=true");
            string queryString = String.Join("&", queryParams.ToArray());

            // set url
            string url = serverUrl + "/v1/error?" + queryString;

            // setup request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
