using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Refresh
    {
        static public void RefreshDirectory(string serverUrl, string accessToken, string localPath, bool recursive = false)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);
            Debug.Assert(localPath != null);

            try
            {
                // start refresh
                SendPostRefresh(serverUrl, accessToken, Utility.Utf16ToUtf8(localPath), recursive);
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

        // POST /v1/refresh
        // https://docs.oxygencloud.com/reference/infinity-drive-refreshing-sync#refresh-directory
        static private String SendPostRefresh(string serverUrl, string accessToken, string localPath, bool recursive = false)
        {
            using (WebClient client = new WebClient())
            {
                // set UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                // set required authorization header
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);

                // set content type
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                // set content
                Dictionary<String, object> data = new Dictionary<String, object>
                {
                    { "path", localPath },
                    { "queue", false },
                    { "recursive", recursive }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/refresh"), json.Serialize(data));
            }
        }
    }
}
