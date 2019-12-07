using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Control
    {
        static public ArrayList ListAccessControls(string serverUrl, string accessToken)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);

            try
            {
                //  get registered control
                String response = SendGetControl(serverUrl, accessToken);

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

        static public Dictionary<String, Object> RegisterAccessControl(string serverUrl, string accessToken, string appName, bool blockAccess)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);
            Debug.Assert(appName != null);

            try
            {
                // reregister access control
                string response = SendPostControl(serverUrl, accessToken, appName, blockAccess);

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

        static public bool ForgetApplication(string serverUrl, string accessToken, string appName)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);
            Debug.Assert(appName != null);

            try
            {
                // remove access control
                SendDeleteControl(serverUrl, accessToken, appName);
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

        //
        // Endpoint Wrappers
        // 

        // GET /v1/control
        // https://docs.oxygencloud.com/reference/access-control#get-access-control-list
        static private string SendGetControl(string serverUrl, string accessToken)
        {
            // setup request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl + "/v1/control");
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

        // POST /v1/control
        // https://docs.oxygencloud.com/reference/access-control#register-access-control
        static private String SendPostControl(string serverUrl, string accessToken, string appName, bool blockAccess)
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
                    { "app", appName },
                    { "block", blockAccess }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/control"), json.Serialize(data));

            }
        }

        // DELETE /v1/control
        // https://docs.oxygencloud.com/reference/access-control#forget-application
        static private void SendDeleteControl(string serverUrl, string accessToken, string appName)
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
                client.UploadString(new Uri(serverUrl + "/v1/control?app=" + appName), "DELETE", request_body);
            }
        }


    }
}
