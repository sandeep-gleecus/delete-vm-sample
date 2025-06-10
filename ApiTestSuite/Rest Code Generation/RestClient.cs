using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation
{
    /// <summary>
    /// This class is responsible for actually making the HTTP/HTTPS calls and capturing the response
    /// </summary>
    public class RestClient
    {
        #region Properties

        /// <summary>
        /// The current request
        /// </summary>
        public RestRequest CurrentRequest
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public RestClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                      delegate (
                          Object obj,
                          X509Certificate certificate,
                          X509Chain chain,
                          SslPolicyErrors errors)
                      {
                          // Validate the certificate and return true or false as appropriate.
                          // Note that it not a good practice to always return true because not
                          // all certificates should be trusted.
                          return true;
                      };
        }

        System.Net.CookieContainer CookieContainer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request">The request object</param>
        public RestClient(RestRequest request) : this()
        {
            this.CurrentRequest = request;

            CookieContainer = new System.Net.CookieContainer();
        }

        /// <summary>
        /// Sends the current request and captures the response
        /// </summary>
        /// <returns>The response</returns>
        public RestResponse SendRequest()
        {
            if (this.CurrentRequest == null)
            {
                throw new InvalidOperationException("You need to specify the CurrentRequest before calling SendRequest");
            }

            //First we need to resolve any parameters in the URL
            string resolvedUrl = this.CurrentRequest.Url;
            string resolvedRequestBody = this.CurrentRequest.Body;
            if (this.CurrentRequest.Parameters != null && this.CurrentRequest.Parameters.Count > 0)
            {
                foreach (RestParameter parameter in this.CurrentRequest.Parameters)
                {
                    resolvedUrl = resolvedUrl.Replace(parameter.TokenName, parameter.Value);
                    resolvedRequestBody = resolvedRequestBody.Replace(parameter.TokenName, parameter.Value);
                }
            }


            //Next build the URI
            Uri uri;
            if (Uri.TryCreate(resolvedUrl, UriKind.Absolute, out uri))
            {
                //Create the request and specify the HTTP Method
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = this.CurrentRequest.Method;

                //Add any authentication credentials
                if (this.CurrentRequest.Credential != null && !String.IsNullOrEmpty(this.CurrentRequest.Credential.UserName) && !String.IsNullOrEmpty(this.CurrentRequest.Credential.Password))
                {
                    if (this.CurrentRequest.Credential.UserName.IndexOf("\\")>0)
                    {
                        string[] domainUser = this.CurrentRequest.Credential.UserName.Split('\\');
                        request.UseDefaultCredentials = false;
                        request.PreAuthenticate = true;
                        NetworkCredential c = new NetworkCredential(domainUser[1], this.CurrentRequest.Credential.Password, domainUser[0]);

                        CredentialCache credentialCache = new CredentialCache();
                        credentialCache.Add(uri, "NTLM", c);
                        request.Credentials = credentialCache;
                    } else
                    {
                        //http://stackoverflow.com/questions/1683398/passing-networkcredential-to-httpwebrequest-in-c-sharp-from-asp-net-page
                        byte[] credentialBuffer = new UTF8Encoding().GetBytes((this.CurrentRequest.Credential.UserName + ":" + (this.CurrentRequest.Credential.Password)));
                        request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialBuffer);
                    }
                }

                //Add the standard headers
                //Content-Type

                //Add the other headers
                foreach (RestHeader restHeader in this.CurrentRequest.Headers)
                {
                    if (restHeader.Name.ToLowerInvariant() == StandardHeaders.Content_Type.ToLowerInvariant())
                    {
                        request.ContentType = restHeader.Value;
                    }
                    else if (restHeader.Name.ToLowerInvariant() == StandardHeaders.Accept.ToLowerInvariant())
                    {
                        request.Accept = restHeader.Value;
                    }
                    else if (restHeader.Name.ToLowerInvariant() == StandardHeaders.User_Agent.ToLowerInvariant())
                    {
                        request.UserAgent = restHeader.Value;
                    }
                    else
                    {
                        request.Headers.Add(restHeader.Name, restHeader.Value);
                    }
                }

                //TODO: Add future support for HTTP cookies
                request.CookieContainer = this.CookieContainer;

                //TODO: Add support for configurable timeouts

                //See if we have a body to POST
                if (String.IsNullOrEmpty(this.CurrentRequest.Body))
                {
                    request.ContentLength = 0;
                }
                else
                {
                    byte[] binaryData = Encoding.UTF8.GetBytes(resolvedRequestBody);
                    request.ContentLength = binaryData.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(binaryData, 0, binaryData.Length);
                    }
                }

                //Get the response
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response == null)
                    {
                        RestResponse emptyResponse = new RestResponse();
                        emptyResponse.IsErrorStatus = false;
                        emptyResponse.Headers.Add(new RestHeader() { Name = "X-Empty-Response", Value = "No Response Received from Server" });
                        emptyResponse.Body = "No Response Received from Server!";
                        return emptyResponse;
                    }

                    //We need to make sure the coding does not expect a UTF8 Byte Order Mark (BOM)
                    //Since that is not supposed to be sent per the JSON spec
                    Stream responseStream = response.GetResponseStream();
                    UTF8Encoding utf8NoBom = new UTF8Encoding(false);
                    string responseString;
                    using (StreamReader reader = new StreamReader(responseStream, utf8NoBom))
                    {
                        responseString = reader.ReadToEnd();

                        //Make sure no BOM was returned
                        if (!Equals(reader.CurrentEncoding, utf8NoBom))
                        {
                            throw new InvalidOperationException("The server returned an UTF8 BOM which is not allowed!");
                        }
                    }

                    //Populate response object
                    RestResponse restResponse = new RestResponse();
                    restResponse.IsErrorStatus = false;
                    restResponse.Body = responseString;
                    //Add the status code as a header
                    RestHeader restHeader = new RestHeader() { Name = Properties.Resources.Rest_StatusCode, Value = (int)response.StatusCode + " " + response.StatusDescription};
                    restResponse.Headers.Add(restHeader);
                    //Add the various headers
                    foreach (string key in response.Headers.AllKeys)
                    {
                        restHeader = new RestHeader() { Name = key, Value = response.Headers[key] };
                        restResponse.Headers.Add(restHeader);
                    }
                    return restResponse;
                }
                catch (WebException webException)
                {
                    HttpWebResponse response = (HttpWebResponse)webException.Response;
                    if (response == null)
                    {
                        RestResponse emptyResponse = new RestResponse();
                        emptyResponse.IsErrorStatus = true;
                        emptyResponse.Headers.Add(new RestHeader() { Name = "X-Empty-Response", Value = "No Response Received from Server" });
                        emptyResponse.Body = "No Response Received from Server!";
                        return emptyResponse;
                    }
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    string responseString = reader.ReadToEnd();

                    //Populate response object
                    RestResponse restResponse = new RestResponse();
                    restResponse.IsErrorStatus = true;
                    restResponse.Body = responseString;
                    //Add the status code as a header
                    RestHeader restHeader = new RestHeader() { Name = Properties.Resources.Rest_StatusCode, Value = (int)response.StatusCode + " " + response.StatusDescription };
                    restResponse.Headers.Add(restHeader);
                    //Add the various headers
                    foreach (string key in response.Headers.AllKeys)
                    {
                        restHeader = new RestHeader() { Name = key, Value = response.Headers[key] };
                        restResponse.Headers.Add(restHeader);
                    }
                    return restResponse;
                }
            }
            else
            {
                throw new InvalidOperationException("The URL specified is not valid, please check the syntax and try again!");
            }
        }
    }
}
