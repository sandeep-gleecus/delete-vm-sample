using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.ServiceModel;
using System.Net;
using System.Reflection;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Rest;
using System.ServiceModel.Web;

namespace Inflectra.SpiraTest.Web.Services
{
	/// <summary>
	/// This web form displays the OData web service documentation
	/// </summary>
	public partial class OData : System.Web.UI.Page
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.OData";

        /// <summary>
        /// Stores information about a specific REST Resource
        /// </summary>
        protected class RestResourceInfo
        {
            /// <summary>
            /// The name of the resource
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The description of the resource
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// List of the supported HTTP Methods
            /// </summary>
            public List<string> Methods = new List<string>();

            /// <summary>
            /// List of the supported HTTP Methods
            /// </summary>
            public List<RestOperationInfo> Operations = new List<RestOperationInfo>();
        }

        /// <summary>
        /// Stores information about a specific REST Operation
        /// </summary>
        protected class RestOperationInfo
        {
            /// <summary>
            /// The URI used to access the operation
            /// </summary>
            public string Uri { get; set; }

            /// <summary>
            /// The HTTP Method used to access the operation
            /// </summary>
            public string Method { get; set; }

            /// <summary>
            /// The description of the operation
            /// </summary>
            public string Description { get; set; }
        }

        /// <summary>
        /// This method displays the documentation page
        /// </summary>
        /// <param name="sender">The object raising the event</param>
        /// <param name="e">The event arguments</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            const string METHOD_NAME = "Page_Load";
            try
            {
				//Redirect if they do not have access to ODATA
				if (!Common.Global.Feature_OData)
				{
					Response.Redirect("~/Services");
				}

				//Display the current product name in the title
				this.Title = ConfigurationSettings.Default.License_ProductType + ": ODATA Web Service";
                this.ltrProductName2.Text = ConfigurationSettings.Default.License_ProductType;
				this.ltrProductName3.Text = ConfigurationSettings.Default.License_ProductType;
				this.ltrProductName4.Text = ConfigurationSettings.Default.License_ProductType;

				//Populate the base url
				string baseUrl = Request.Url.AbsoluteUri;
                baseUrl = baseUrl.Replace("Services/OData.aspx", "api/odata");
                this.lnkBaseUrl.Text = baseUrl;
                this.lnkBaseUrl.NavigateUrl = baseUrl;
            }
            catch (System.Exception exception)
            {
                Response.Write(Resources.Messages.Services_DocumentationUnavailable + " (" + exception.Message + ")!");
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
            }
        }
    }
}
