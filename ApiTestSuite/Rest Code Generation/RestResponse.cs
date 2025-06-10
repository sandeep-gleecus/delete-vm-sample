using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation
{
    /// <summary>
    /// Defines a single REST web service response definition
    /// </summary>
    public class RestResponse
    {
        public RestResponse()
        {

        }

        public RestResponse(Exception ex)
        {
            IsErrorStatus = true;
            Body = ex.ToString();
            RawResponse = Body;
        }

        /// <summary>
        /// The response body
        /// </summary>
        public string Body
        {
            get;
            set;
        }

        public string RawResponse
        {
            get;set;
        }

        /// <summary>
        /// Did we receive an error status
        /// </summary>
        public bool IsErrorStatus
        {
            get;
            set;
        }

        /// <summary>
        /// The response headers
        /// </summary>
        public List<RestHeader> Headers
        {
            get
            {
                return this.headers;
            }
        }
        private List<RestHeader> headers = new List<RestHeader>();
    }
}
