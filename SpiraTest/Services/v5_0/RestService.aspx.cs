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

namespace Inflectra.SpiraTest.Web.Services.v5_0
{
    /// <summary>
    /// This web form displays the REST web service documentation in a human readable form for developers
    /// </summary>
    public partial class RestService1 : System.Web.UI.Page
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v5_0.RestService1";

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
                //Display the current product name in the title
                this.Title = ConfigurationSettings.Default.License_ProductType + ": REST Web Service";
                this.ltrProductName2.Text = ConfigurationSettings.Default.License_ProductType;

                //Populate the base url
                string baseUrl = Request.Url.AbsoluteUri;
                baseUrl = baseUrl.Replace(".aspx", ".svc/");
                this.lnkBaseUrl.Text = baseUrl;
                this.lnkBaseUrl.NavigateUrl = baseUrl;
                this.baseUrlText.Text = baseUrl;
                this.baseUrlTextExample.Text = baseUrl;

                //We need to load in the assembly's XML comments (Web.XML)
                string xmlCommentsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "Web.XML");
                XmlDocument xmlCommentsDoc = new XmlDocument();
                try
                {
                    xmlCommentsDoc.Load(xmlCommentsPath);
                }
                catch (Exception exception)
                {
                    Response.Write("Unable to load Web.XML comments file: " + exception.Message);
                }

                //We need to get a list of all the resources
                Dictionary<string, RestResourceInfo> resourceDic = new Dictionary<string, RestResourceInfo>();

                //The first thing we need to do is iterate through all the different service methods in the interface
                MethodInfo[] methods = typeof(IRestService).GetMethods();
                foreach (MethodInfo methodInfo in methods)
                {
                    //See if it implements the resource attribute
                    RestResourceAttribute resourceAttribute = (RestResourceAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(RestResourceAttribute));
                    if (resourceAttribute != null)
                    {
                        //Get the resource name and description
                        string name = resourceAttribute.ResourceName;
                        string description = resourceAttribute.Description;

                        //See if we have a WebGet or WebInvoke attribute
                        WebGetAttribute webGetAttribute = (WebGetAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(WebGetAttribute));
                        WebInvokeAttribute webInvokeAttribute = (WebInvokeAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(WebInvokeAttribute));

                        //Determine the HTTP Method (GET, PUT, POST, DELETE)
                        string httpMethodName = "";
                        string uri = "";
                        if (webGetAttribute != null)
                        {
                            httpMethodName = "GET";
                            uri = webGetAttribute.UriTemplate;
                        }
                        else if (webInvokeAttribute != null)
                        {
                            httpMethodName = webInvokeAttribute.Method;
                            uri = webInvokeAttribute.UriTemplate;
                        }
                        if (!String.IsNullOrEmpty(httpMethodName))
                        {
                            //Add it to the list of methods on this resource
                            RestResourceInfo resourceInfo;
                            if (resourceDic.ContainsKey(name))
                            {
                                resourceInfo = resourceDic[name];
                                if (String.IsNullOrEmpty(resourceInfo.Description) && !String.IsNullOrEmpty(description))
                                {
                                    resourceInfo.Description = description;
                                }
                                if (!resourceInfo.Methods.Contains(httpMethodName))
                                {
                                    resourceInfo.Methods.Add(httpMethodName);
                                }
                            }
                            else
                            {
                                resourceInfo = new RestResourceInfo() { Name = name, Description = description };
                                resourceInfo.Methods.Add(httpMethodName);
                                resourceDic.Add(name, resourceInfo);
                            }

                            //Now actually add the method info
                            RestOperationInfo operationInfo = new RestOperationInfo();
                            operationInfo.Uri = uri;
                            operationInfo.Method = httpMethodName;
                            resourceInfo.Operations.Add(operationInfo);

                            //See if we can find matching XML description for this operation
                            //We need to concatenate the class and method info
                            string methodNamespace = "M:" + typeof(RestService).FullName + "." + methodInfo.Name;
                            XmlNode xmlMethodSummary;
                            if (methodInfo.GetParameters().Length > 0)
                            {
                                xmlMethodSummary = xmlCommentsDoc.SelectSingleNode("/doc/members/member[substring-before(@name,'(')='" + methodNamespace + "']/summary");
                            }
                            else
                            {
                                xmlMethodSummary = xmlCommentsDoc.SelectSingleNode("/doc/members/member[@name='" + methodNamespace + "']/summary");
                            }
                            if (xmlMethodSummary != null)
                            {
                                operationInfo.Description = xmlMethodSummary.InnerText;
                            }
                        }
                    }
                }

                //Load the resource grid
                this.grdResources.DataSource = resourceDic.OrderBy(r => r.Key);

                //Display the resource operations
                this.rptResourceOperations.DataSource = resourceDic.OrderBy(r => r.Key);
                this.DataBind();
            }
            catch (System.Exception exception)
            {
                Response.Write(Resources.Messages.Services_DocumentationUnavailable + " (" + exception.Message + ")!");
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
            }
        }
    }
}