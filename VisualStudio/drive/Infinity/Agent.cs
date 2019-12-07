using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Agent
    {
        static public Dictionary<String, Object> AuthenticateKey(string serverUrl, String agentKey)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(agentKey != null);

            try
            {
                //  authenticate agent key
                String response = SendPostAuthentication(serverUrl, agentKey);

                // format to dictionary
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

        //
        // Endpoint Wrappers
        //

        // POST /v1/authentication
        // https://docs.oxygencloud.com/reference/oxygen-agent-authentication
        static private String SendPostAuthentication(string serverUrl, string agentKey)
        {
            using (WebClient client = new WebClient())
            {
                // set UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                // set content type
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");

                // set content
                Dictionary<String, object> data = new Dictionary<String, object>
                {
                    { "key", agentKey }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/authentication"), json.Serialize(data));
            }
        }

    }
}
