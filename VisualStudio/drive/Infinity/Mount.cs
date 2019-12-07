using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Mount
    {
        static public ArrayList GetMounts(string serverUrl, string accessToken)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);

            try
            {
                //  get mounts
                String response = SendGetMount(serverUrl, accessToken);


                // format to array
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return (System.Collections.ArrayList)json.Deserialize(response, typeof(System.Collections.ArrayList));

            }
            catch (WebException e)
            {
                if (e.Response == null)
                    throw e;

                if ((int)((HttpWebResponse)e.Response).StatusCode == 403)
                    // handle not allowed
                    return null;

                // handle unexpected
                throw e;
            }
        }

        static public Dictionary<String, Object> CreateMount(string serverUrl, string accessToken, string localPath, string remotePath)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(localPath != null);
            Debug.Assert(remotePath != null);
            Debug.Assert(accessToken != null);

            try
            {
                // create new mount
                String response = SendPostMounts(serverUrl, accessToken, Utility.Utf16ToUtf8(localPath), remotePath);

                // format to dicationary
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return (Dictionary<String, Object>)json.Deserialize(response, typeof(Dictionary<String, Object>));
            }
            catch (WebException e)
            {
                if (e.Response == null)
                    throw e;

                if ((int)((HttpWebResponse)e.Response).StatusCode == 403)
                    // handle not allowed
                    return null;

                // handle unexpected
                throw e;
            }
        }

        static public bool DeleteMount(string serverUrl, string accessToken, string localPath, string remotePath)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(localPath != null);
            Debug.Assert(remotePath != null);
            Debug.Assert(accessToken != null);

            try
            {
                // ask infinity drive to refresh path
                SendDeleteMount(serverUrl, accessToken, Utility.Utf16ToUtf8(localPath), remotePath);
                return true;
            }
            catch (WebException e)
            {
                if (e.Response == null)
                    throw e;

                if ((int)((HttpWebResponse)e.Response).StatusCode == 403)
                    // handle not allowed
                    return false;

                // handle unexpected
                throw e;
            }
        }

        static public void RefreshPath(string serverUrl, string accessToken, string localPath, bool recursive)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(localPath != null);
            Debug.Assert(accessToken != null);

            try
            {
                // ask infinity drive to refresh path
                SendPostRefresh(serverUrl, Utility.Utf16ToUtf8(localPath), accessToken, recursive);

            }
            catch (WebException e)
            {
                // not expecting any errors
                throw e;
            }
        }

        //
        // Endpoint Wrappers
        //

        // POST /v1/refresh
        // https://docs.oxygencloud.com/reference/infinity-drive-refreshing-sync#refresh-directory
        static private String SendPostRefresh(string serverUrl, string accessToken, string path, bool recursive = false)
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
                    { "path", path },
                    { "queue", false },
                    { "recursive", recursive }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/refresh"), json.Serialize(data));

            }
        }

        // GET /v1/mount
        // https://docs.oxygencloud.com/reference/infinity-drive-mounts#get-drive-mounts
        static private String SendGetMount(string serverUrl, string accessToken)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl + "/v1/mount");
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
            request.Method = "GET";

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

        // POST /v1/mount
        // https://docs.oxygencloud.com/reference/infinity-drive-mounts#create-drive-mount
        static private String SendPostMounts(string serverUrl, string accessToken, string localPath, string remotePath)
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
                    { "localPath", localPath },
                    { "remotePath", remotePath }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/mount"), json.Serialize(data));

            }
        }

        // DELETE /v1/mount
        // https://docs.oxygencloud.com/reference/infinity-drive-mounts#remove-drive-mount
        static private void SendDeleteMount(string serverUrl, string accessToken, string localPath, string remotePath)
        {

            using (WebClient client = new WebClient())
            {

                // set encoding
                client.Encoding = System.Text.Encoding.UTF8;

                // set authorization
                client.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string request_body = serializer.Serialize(new Dictionary<String, object>());
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                // do delete
                client.UploadString(new Uri(serverUrl + "/v1/mount?local=" + localPath + "&remote=" + remotePath), "DELETE", request_body);
            }
        }

    }
}
