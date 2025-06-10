using Inflectra.SpiraTest.Web.Services.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Web;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v5_0
{
    /// <summary>
    /// Provides a way to download a set of REST resources as a Rapise .REST file
    /// </summary>
    public class RestServiceExport : IHttpHandler
    {
        /// <summary>Processes the inbound request </summary>
        public void ProcessRequest(HttpContext context)
        {
			try
			{
                //Get the resource name from the querystring, also get the version without any spaces for the filename
                string resourceName = context.Request.QueryString["resource"].Trim();
                string resourceFilename = resourceName.Replace(" ", "");
                if (GlobalFunctions.VerifyFilenameForPathChars(resourceFilename))
                {
                    //Set the MIME type and 'filename' for the attachment
                    context.Response.Buffer = false;
                    context.Response.ContentType = "application/json";
                    context.Response.AddHeader("Content-Disposition", "attachment; filename=" + resourceFilename + ".rest");
                    context.Response.Charset = "UTF-8"; //Only used for text

                    //Find the first request in the list to put as the current request
                    //Rapise needs this, otherwise it create an error loading the .REST file
                    MethodInfo[] methods = typeof(IRestService).GetMethods();
                    string currentRequestName = null;
                    foreach (MethodInfo methodInfo in methods)
                    {
                        RestResourceAttribute resourceAttribute = (RestResourceAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(RestResourceAttribute));
                        if (resourceAttribute != null && resourceAttribute.ResourceName == resourceName)
                        {
                            //Create a friendly function name
                            string requestName = methodInfo.Name;
                            if (requestName.Contains("_"))
                            {
                                requestName = requestName.Split('_')[1];
                            }
                            currentRequestName = requestName;
                            break;
                        }
                    }

                    //Write out the REST file as JSON for the resource
                    context.Response.Write("{\n");
                    if (!String.IsNullOrEmpty(currentRequestName))
                    {
                        context.Response.Write("  \"CurrentRequestName\": \"" + currentRequestName + "\",\n");
                    }
                    context.Response.Write("  \"CurrentPath\": \"" + resourceFilename + ".rest\",\n");
                    context.Response.Write("  \"Name\": \"" + resourceFilename + "\",\n");
                    context.Response.Write("  \"Requests\": [\n");

                    //Now we need to loop through the actual requests in this resource
                    bool isFirst = true;
                    foreach (MethodInfo methodInfo in methods)
                    {
                        //See if it implements the resource attribute
                        RestResourceAttribute resourceAttribute = (RestResourceAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(RestResourceAttribute));
                        if (resourceAttribute != null && resourceAttribute.ResourceName == resourceName)
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
                                //Populate the full URL as well
                                string baseUrl = context.Request.Url.AbsoluteUri;
                                baseUrl = baseUrl.Replace("RestServiceExport.ashx", "RestService.svc");
                                baseUrl = baseUrl.Replace(context.Request.Url.Query, "");
                                string fullUrl = baseUrl + "/" + uri;

                                //Create a friendly function name
                                string requestName = methodInfo.Name;
                                if (requestName.Contains("_"))
                                {
                                    requestName = requestName.Split('_')[1];
                                }

                                //Render out the request
                                if (!isFirst)
                                {
                                    context.Response.Write(",\n");
                                }
                                context.Response.Write("    {\n");
                                context.Response.Write("      \"Name\": \"" + requestName + "\",\n");
                                context.Response.Write("      \"Url\": \"" + fullUrl + "\",\n");
                                context.Response.Write("      \"Method\": \"" + httpMethodName + "\",\n");
                                context.Response.Write("      \"Body\": \"\",\n");

                                //Write out JSON header
                                context.Response.Write(@"
      ""Headers"": [
        {
          ""Name"": ""Accept"",
          ""Value"": ""application/json""
        },
        {
          ""Name"": ""Content-Type"",
          ""Value"": ""application/json""
        }
      ],
");

                                //Write out any parameters
                                context.Response.Write("      \"Parameters\": [\n");

                                ParameterInfo[] parameters = methodInfo.GetParameters();
                                ParameterInfo[] requestParameters = parameters.Where(p => uri.Contains(p.Name)).ToArray();
                                if (requestParameters.Length > 0)
                                {
                                    foreach (ParameterInfo requestParameter in requestParameters)
                                    {
                                        context.Response.Write(@"        {
          ""Name"": """ + requestParameter.Name + @""",
          ""TokenName"": ""{" + requestParameter.Name + @"}"",
          ""Value"": """"
        }");
                                        if (requestParameters.Last().Name == requestParameter.Name)
                                        {
                                            context.Response.Write("\n");
                                        }
                                        else
                                        {
                                            context.Response.Write(",\n");
                                        }
                                    }
                                }

                                context.Response.Write("      ]\n");

                                //End of request
                                context.Response.Write("    }");
                                isFirst = false;
                            }
                        }
                    }
                    context.Response.Write("\n");

                    context.Response.Write("  ]\n");
                    context.Response.Write("}\n");
                }
            }
            catch (Exception exception)
            {
                context.Response.Write(exception.Message);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}