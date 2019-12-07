using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Web.Script.Serialization;


namespace Infinity
{
    class Auth
    {
        static public Dictionary<String, Object> AuthorizeAgent(string serverUrl, String agentToken)
        {
            Debug.Assert(serverUrl != null);
            Debug.Assert(agentToken != null);

            try
            {
                //  authorize agent
                String response = SendPostOauthToken(serverUrl, agentToken);

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

        // POST /v1/oauth/token
        // https://docs.oxygencloud.com/reference/oxygen-auth-oauth
        static private String SendPostOauthToken(string serverUrl, string agentToken)
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
                    { "agentToken", agentToken }
                };

                // do post
                JavaScriptSerializer json = new JavaScriptSerializer();
                json.MaxJsonLength = Int32.MaxValue;
                return client.UploadString(new Uri(serverUrl + "/v1/oauth/token"), json.Serialize(data));
            }
        }
    }
}
