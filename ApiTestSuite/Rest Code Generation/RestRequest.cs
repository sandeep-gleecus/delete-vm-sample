using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation
{
    /// <summary>
    /// Defines a single REST web service request definition
    /// </summary>
    public class RestRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the new request</param>
        public RestRequest(string name)
        {
            this.Name = name;
        }

        private bool isModified;
        public bool IsModified()
        {
            if (isModified)
            {
                return true;
            }

            foreach (RestHeader rh in this.Headers)
            {
                if (rh.IsModified()) return true;
            }

            foreach (RestParameter rp in this.Parameters)
            {
                if (rp.IsModified()) return true;
            }
            return false;
        }
        public void SetModified(bool val)
        {
            isModified = val;

            if (!val)
            {
                foreach (RestHeader rh in this.Headers)
                {
                    rh.SetModified(false);
                }

                foreach (RestParameter rp in this.Parameters)
                {
                    rp.SetModified(false);
                }
            }
        }

        public void SetModified(bool val, string oldVal, string newVal)
        {
            if(!val)
            {
                SetModified(false);
            }
            else if(oldVal!=newVal)
            {
                SetModified(true);
            }
        }

        #region Properties

        private string name;
        /// <summary>
        /// The name of the request
        /// </summary>
        public string Name
        {
            get { return name; }
            set { SetModified(true, name, value); name = value;  }
        }

        private string url;
        /// <summary>
        /// The resource URL
        /// </summary>
        public string Url
        {
            get { return url; }
            set { SetModified(true, url, value); url = value; }
        }


        private string method;
        /// <summary>
        /// The HTTP Method
        /// </summary>
        public string Method
        {
            get { return method; }
            set { SetModified(true, method, value); method = value; }
        }

        private string body;
        /// <summary>
        /// The request body
        /// </summary>
        public string Body
        {
            get { return body; }
            set { SetModified(true, body, value); body = value; }
        }

        private NetworkCredential credential;
        /// <summary>
        /// The request authentication credentials
        /// </summary>
        public NetworkCredential Credential
        {
            get { return credential; }
            set { credential = value; SetModified(true); }
        }

        /// <summary>
        /// The request headers
        /// </summary>
        public List<RestHeader> Headers
        {
            get
            {
                return this.headers;
            }
        }
        private List<RestHeader> headers = new List<RestHeader>();

        /// <summary>
        /// The request parameters
        /// </summary>
        public List<RestParameter> Parameters
        {
            get
            {
                return this.parameters;
            }
        }
        private List<RestParameter> parameters = new List<RestParameter>();

        #endregion

        #region Methods

        /// <summary>
        /// Makes a clone of the current request
        /// </summary>
        /// <param name="name">The name of the new cloned request</param>
        /// <returns>The new request</returns>
        public RestRequest Clone(string name)
        {
            //First populate the simple properties
            RestRequest newRequest = new RestRequest(name);
            newRequest.Method = this.Method;
            newRequest.Url = this.Url;
            newRequest.Body = this.Body;

            //Now the collections
            if (this.Credential != null)
            {
                newRequest.Credential = new NetworkCredential();
                newRequest.Credential.UserName = this.Credential.UserName;
                newRequest.Credential.Password = this.Credential.Password;
            }
            foreach (RestHeader header in this.Headers)
            {
                RestHeader newHeader = new RestHeader();
                newHeader.Name = header.Name;
                newHeader.Value = header.Value;
                newRequest.Headers.Add(newHeader);
            }
            foreach (RestParameter parameter in this.Parameters)
            {
                RestParameter newParameter = new RestParameter();
                newParameter.Name = parameter.Name;
                newParameter.Value = parameter.Value;
                newRequest.Parameters.Add(newParameter);
            }
            newRequest.SetModified(true);
            //Return the cloned request
            return newRequest;
        }

        #endregion
    }
}
