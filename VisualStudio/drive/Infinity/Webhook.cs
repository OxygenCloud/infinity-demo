using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Webhook
    {
        static public ArrayList ListWebhooks(string serverUrl, string accessToken)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);

            try
            {
                //  get registered control
                String response = SendGetWebhook(serverUrl, accessToken);

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

        static public Dictionary<String, Object> RegisterWebhook(string serverUrl, string accessToken, string eventName, String url)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);
            Debug.Assert(eventName != null);
            Debug.Assert(url != null);

            try
            {
                // reregister access control
                String response = SendPostWebhook(serverUrl, accessToken, eventName, url);

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

        static public bool DeleteWebhook(string serverUrl, string accessToken, string url, string eventName)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(accessToken != null);
            Debug.Assert(url != null);
            Debug.Assert(eventName != null);

            try
            {
                // deregister webhook
                SendDeleteWebhook(serverUrl, accessToken, url, eventName);
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

        // GET /v1/webhook
        // https://docs.oxygencloud.com/reference/system-extensions#get-webhooks
        static private string SendGetWebhook(string serverUrl, string accessToken)
        {
            // setup request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverUrl + "/v1/webhook");
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

        // POST /v1/webhook
        // https://docs.oxygencloud.com/reference/system-extensions#register-webhook
        static private String SendPostWebhook(string serverUrl, string accessToken, string eventName, string url)
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
                    { "event", eventName },
                    { "url", url }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/webhook"), json.Serialize(data));

            }
        }

        // DELETE /v1/webhook
        // https://docs.oxygencloud.com/reference/system-extensions#delete-webhook
        static private void SendDeleteWebhook(string serverUrl, string accessToken, string url, string eventName)
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
                client.UploadString(new Uri(serverUrl + "/v1/webhook?url=" + url + "&event=" + eventName), "DELETE", request_body);
            }
        }
    }
}
