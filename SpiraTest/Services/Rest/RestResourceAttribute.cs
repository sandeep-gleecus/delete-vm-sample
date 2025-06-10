using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Documentation attribute that describes the resource associated with a specific web service
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RestResourceAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceName">The name of the resource</param>
        public RestResourceAttribute(string resourceName)
        {
            this.ResourceName = resourceName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="description">The description of the resource (only one instance needs it)</param>
        /// <param name="resourceName">The name of the resource</param>
        public RestResourceAttribute(string resourceName, string description)
        {
            this.ResourceName = resourceName;
            this.Description = description;
        }

        /// <summary>
        /// The name of the resource (we don't localize)
        /// </summary>
        public string ResourceName
        {
            get;
            set;
        }

        /// <summary>
        /// The description of the resource (we don't localize)
        /// </summary>
        public string Description
        {
            get;
            set;
        }
    }
}