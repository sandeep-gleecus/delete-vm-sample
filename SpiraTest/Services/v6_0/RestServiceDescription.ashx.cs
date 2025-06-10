using Inflectra.SpiraTest.Web.Services.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0
{
    /// <summary>
    /// Provides a way to download the entire .REST service into a WADL-JSON File
    /// </summary>
    /// <remarks>
    /// WADL-JSON is not a real standard, just a JSON version of the proposed WADL XML schema for REST.
    /// https://dzone.com/articles/json-schema-wadl
    /// </remarks>
    public class RestServiceDescription : IHttpHandler
    {
        /// <summary>Processes the inbound request </summary>
        public void ProcessRequest(HttpContext context)
        {
			try
			{
                //Construct a service 'filename' from the assembly
                Type thisType = this.GetType();
                string serviceFilename = thisType.FullName;

                //Set the MIME type and 'filename' for the attachment
                context.Response.Buffer = false;
                context.Response.ContentType = "application/vnd.sun.wadl+json";
                context.Response.AddHeader("Content-Disposition", "attachment; filename=" + serviceFilename + ".json");
                context.Response.Charset = "UTF-8"; //Only used for text

                //Get the base URL for the service
                string baseUrl = context.Request.Url.AbsoluteUri;
                baseUrl = baseUrl.Replace("RestServiceDescription.ashx", "RestService.svc");

                //Write out the overall service information
                context.Response.Write("{\n");
                context.Response.Write("  \"id\": \"" + serviceFilename + "\",\n");
                context.Response.Write("  \"base\": \"" + baseUrl + "\",\n");

                //Next we need to loop through all of the resources and get a unique list of names
                MethodInfo[] methods = typeof(IRestService).GetMethods();
                List<string> resourceNames = new List<string>();
                Dictionary<string, string> resourceDescriptions = new Dictionary<string, string>();
                foreach (MethodInfo methodInfo in methods)
                {
                    //See if it implements the resource attribute
                    RestResourceAttribute resourceAttribute = (RestResourceAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(RestResourceAttribute));
                    //Get the resource name and description
                    string name = resourceAttribute.ResourceName;
                    if (!resourceNames.Contains(name))
                    {
                        resourceNames.Add(name);
                    }
                    if (!String.IsNullOrEmpty(resourceAttribute.Description) && !resourceDescriptions.ContainsKey(name))
                    {
                        resourceDescriptions.Add(name, resourceAttribute.Description);
                    }
                }

                //Now display the list of resources
                context.Response.Write("  \"resources\": {\n");
                bool isFirst = true;
                foreach (string name in resourceNames.OrderBy(r => r))
                {
                    if (!isFirst)
                    {
                        context.Response.Write(",\n");
                    }
                    context.Response.Write("    \"" + name + "\": {\n");
                    context.Response.Write("      \"name\": \"" + name + "\",\n");
                    if (resourceDescriptions.ContainsKey(name))
                    {
                        context.Response.Write("      \"description\": \"" + resourceDescriptions[name] + "\",\n");
                    }
                    DisplayResourceDetails(context, methods, name);
                    context.Response.Write("    }");
                    isFirst = false;
                }
                context.Response.Write("\n");
                context.Response.Write("  }\n");
                context.Response.Write("}\n");
            }
            catch (Exception exception)
            {
                context.Response.Write(exception.Message);
            }
        }

        /// <summary>
        /// Displays the details of a single resource
        /// </summary>
        /// <param name="context">Http Context</param>
        /// <param name="methods">The list of all REST methods</param>
        /// <param name="name">The name of the current resource</param>
        private void DisplayResourceDetails(HttpContext context, MethodInfo[] methods, string name)
        {
            //Write out the resource description (if we have one)

            context.Response.Write("    \"methods\": {\n");

            //Now we need to loop through the actual requests in this resource
            bool isFirst = true;
            foreach (MethodInfo methodInfo in methods)
            {
                //Make sure the resource name matches
                RestResourceAttribute resourceAttribute = (RestResourceAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(RestResourceAttribute));
                //Get the resource name and description
                if (resourceAttribute.ResourceName == name)
                {
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
                        //Render out the request
                        if (!isFirst)
                        {
                            context.Response.Write(",\n");
                        }
                        context.Response.Write("      \"" + methodInfo.Name + "\": {\n");
                        context.Response.Write("        \"id\": \"" + methodInfo.Name + "\",\n");
                        context.Response.Write("        \"path\": \"" + uri + "\",\n");
                        context.Response.Write("        \"name\": \"" + httpMethodName + "\",\n");

                        //Write out the request
                        context.Response.Write("        \"request\": {\n");

                        //Get the parameters and the body object
                        ParameterInfo[] parameters = methodInfo.GetParameters();
                        ParameterInfo[] requestParameters = parameters.Where(p => uri.Contains(p.Name)).ToArray();
                        ParameterInfo[] bodyParameters = parameters.Where(p => !uri.Contains(p.Name)).ToArray();

                        //Display the request body in JSON format
                        if (bodyParameters != null && bodyParameters.Length > 0)
                        {
                            ParameterInfo bodyParameter = bodyParameters[0];
                            string body = GenerateBodyJson(bodyParameter.ParameterType);
                            context.Response.Write("          \"mediaType\": \"application/json\",\n");
                            context.Response.Write("          \"representation\": { \"" + bodyParameter.Name + "\": " + body + " },\n");
                        }

                        //Write out any parameters
                        context.Response.Write("          \"parameters\": {\n");
                        if (requestParameters.Length > 0)
                        {
                            foreach (ParameterInfo requestParameter in requestParameters)
                            {
                                context.Response.Write("            \"" + requestParameter.Name + @""": {
              ""name"": """ + requestParameter.Name + @""",
              ""token"": ""{" + requestParameter.Name + @"}""
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

                        context.Response.Write("          }\n");

                        //End of request
                        context.Response.Write("        },\n");

                        //Write out the response
                        context.Response.Write("        \"response\": {\n");

                        //Display the return value in JSON format
                        Type returnType = methodInfo.ReturnType;
                        if (returnType != null && returnType != typeof(void))
                        {
                            string body = GenerateBodyJson(returnType);
                            context.Response.Write("          \"mediaType\": \"application/json\",\n");
                            context.Response.Write("          \"representation\": " + body + "\n");
                        }

                        //End of response
                        context.Response.Write("        }\n");


                        //End of method
                        context.Response.Write("      }");
                        isFirst = false;
                    }
                }
            }
            context.Response.Write("\n");

            context.Response.Write("    }\n");
        }

        /// <summary>
        /// Returns the datatype of the request or response body in a way that can be used for unit testing
        /// </summary>
        /// <param name="type">The type to be serialized into JSON</param>
        /// <returns>The JSON notation</returns>
        private string GenerateBodyJson(Type type)
        {
            string json = "";

            //First create the main type
            if (type == typeof(Array) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                json += "[\n";
                Type itemType = type.GetGenericArguments()[0];
                string innerJson = GenerateBodyJson(itemType);
                json += innerJson;
                json += "]\n";
            }
            else
            {
                json += "{\n";
                json += "\"type\": \"" + type.Name + "\",\n";
                if (type.IsPrimitive)
                {
                    json += "\"namespace\": \"" + type.FullName + "\"\n";
                }
                else
                {
                    json += "\"namespace\": \"" + type.FullName + "\",\n";
                    json += "\"members\": {\n";
                    string innerJson = GenerateMembersJson(type);
                    json += innerJson;
                    json += "}\n ";
                }
                json += "}\n";
            }

            return json;
        }

        /// <summary>
        /// Returns the list of properties associated with a type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The JSON representation</returns>
        private string GenerateMembersJson(Type type)
        {
            string json = "";

            //Get a list of all the fields
            FieldInfo[] fields = type.GetFields();
            bool isFirst = true;
            foreach (FieldInfo field in fields)
            {
                if (!isFirst)
                {
                    json += ",\n";
                }
                json += "\"" + field.Name + "\": {\n";
                json += "\"name\": \"" + field.Name + "\",\n";
                json += "\"type\": \"" + field.FieldType.Name+ "\",\n";
                json += "\"namespace\": \"" + GetFriendlyTypeName(field.FieldType) + "\"\n";
                json += "}\n ";
                isFirst = false;
            }

            return json;
        }

        /// <summary>
        /// Gets the friendly name for nullable and non-nullable types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            var builder = new System.Text.StringBuilder();
            var name = type.Name;
            var index = name.IndexOf("`");
            builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
            builder.Append('<');
            var first = true;
            foreach (var arg in type.GetGenericArguments())
            {
                if (!first)
                {
                    builder.Append(',');
                }
                builder.Append(GetFriendlyTypeName(arg));
                first = false;
            }
            builder.Append('>');
            return builder.ToString();
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