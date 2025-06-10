using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ServiceModel.Web;

using Inflectra.SpiraTest.Common;
using Inflectra.SpiraTest.Business;
using Inflectra.SpiraTest.Web.Services.Rest;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Inflectra.SpiraTest.Web.Services.Utils;

namespace Inflectra.SpiraTest.Web.Services.v5_0
{
    /// <summary>
    /// This web form displays the REST web service documentation in a human readable form for developers
    /// </summary>
    public partial class RestServiceOperation : System.Web.UI.Page
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.v5_0.RestServiceOperation";

        private XmlNode xmlMethodNode;
        private XmlDocument xmlCommentsDoc;

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

                //We need to load in the assembly's XML comments (Web.XML)
                string xmlCommentsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "Web.XML");
                xmlCommentsDoc = new XmlDocument();
                try
                {
                    xmlCommentsDoc.Load(xmlCommentsPath);
                }
                catch (Exception exception)
                {
                    Response.Write("Unable to load Web.XML comments file: " + exception.Message);
                }

                //Get the URL and Method from the querystring
                string uri = Request.QueryString["uri"].Trim();
                string method = Request.QueryString["method"].Trim().ToUpperInvariant();

                //Locate the URI via. reflection
                MethodInfo[] methods = typeof(IRestService).GetMethods();
                string httpMethod = "";
                MethodInfo matchedMethodInfo = null;
                foreach (MethodInfo methodInfo in methods)
                {
                    //See if we have a WebGet or WebInvoke attribute
                    WebGetAttribute webGetAttribute = (WebGetAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(WebGetAttribute));
                    WebInvokeAttribute webInvokeAttribute = (WebInvokeAttribute)System.Attribute.GetCustomAttribute(methodInfo, typeof(WebInvokeAttribute));
                    if (webGetAttribute != null && webGetAttribute.UriTemplate == uri && method == "GET")
                    {
                        httpMethod = "GET";
                        matchedMethodInfo = methodInfo;
                        break;
                    }
                    else if (webInvokeAttribute != null && webInvokeAttribute.UriTemplate == uri && method == webInvokeAttribute.Method)
                    {
                        httpMethod = webInvokeAttribute.Method;
                        matchedMethodInfo = methodInfo;
                        break;
                    }
                }
                if (matchedMethodInfo != null)
                {
                    //Display the URI and method
                    this.ltrOperationUri.Text = uri;
                    this.ltrMethodName.Text = method;

                    //Populate the full URL as well
                    string baseUrl = Request.Url.AbsoluteUri;
                    baseUrl = baseUrl.Replace("RestServiceOperation.aspx", "RestService.svc");
                    baseUrl = baseUrl.Replace(Request.Url.Query, "");
                    string fullUrl = baseUrl + "/" + uri;
                    this.lnkFullUrl.Text = fullUrl;
                    this.lnkFullUrl.NavigateUrl = fullUrl;

                    //Locate the method from the documentation XML

                    //See if we can find matching XML description for this operation
                    //We need to concatenate the class and method info
                    string methodNamespace = "M:" + typeof(RestService).FullName + "." + matchedMethodInfo.Name;
                    if (matchedMethodInfo.GetParameters().Length > 0)
                    {
                        xmlMethodNode = xmlCommentsDoc.SelectSingleNode("/doc/members/member[substring-before(@name,'(')='" + methodNamespace + "']");
                    }
                    else
                    {
                        xmlMethodNode = xmlCommentsDoc.SelectSingleNode("/doc/members/member[@name='" + methodNamespace + "']");
                    }

                    //Display the method documentation
                    if (xmlMethodNode != null)
                    {
                        //Summary
                        XmlNode xmlMethodSummary = xmlMethodNode.SelectSingleNode("summary");
                        if (xmlMethodSummary != null)
                        {
                            this.ltrSummary.Text = xmlMethodSummary.InnerText;
                        }

                        //Remarks
                        XmlNode xmlMethodRemarks = xmlMethodNode.SelectSingleNode("remarks");
                        if (xmlMethodRemarks != null)
                        {
                            this.ltrRemarks.Text = xmlMethodRemarks.InnerText;
                        }

                        //Example
                        XmlNode xmlMethodExample = xmlMethodNode.SelectSingleNode("example");
                        if (xmlMethodExample != null)
                        {
                            this.plcExample.Visible = true;
                            this.ltrExample.Text = xmlMethodExample.InnerText;
                        }
                        else
                        {
                            this.plcExample.Visible = false;
                        }
                    }

                    //Display any parameters that are contained in the URI template
                    ParameterInfo[] parameters =  matchedMethodInfo.GetParameters();
                    ParameterInfo[] requestParameters = parameters.Where(p => uri.Contains(p.Name)).ToArray();
                    if (requestParameters.Length > 0)
                    {
                        plcParameters.Visible = true;

                        this.rptParameters.DataSource = requestParameters;
                    }
                    else
                    {
                        plcParameters.Visible = false;
                    }

                    //See if the last parameter is not listed in the URI, if so, then it needs to be provided in the request body
                    //This is only applicable for POST, PUT and DELETE methods
                    this.ltrXml2.Text = "Nothing Expected";
                    this.ltrJson2.Text = "Nothing Expected";
                    if (method == "GET")
                    {
                        this.plcTable2.Visible = false;
                    }
                    else
                    {
                        if (parameters.Length > 0)
                        {
                            ParameterInfo lastParameter = parameters[parameters.Length - 1];
                            if (!uri.Contains(lastParameter.Name))
                            {
                                Type bodyType = lastParameter.ParameterType;

                                //First activate the return type
                                object obj;
                                if (bodyType.Name == "String")
                                {
                                    obj = (String)"";
                                }
                                else if (bodyType.Name == "Byte[]")
                                {
                                    //Sample byte array
                                    obj = new byte[] { 25, 50, 125, 255 };
                                }
                                else if (bodyType.Name == "Int32[]")
                                {
                                    //Sample int array
                                    obj = new int[] { 25, 50, 125, 255 };
                                }
                                else if (bodyType.Name == "Int64[]")
                                {
                                    //Sample long array
                                    obj = new long[] { 25, 50, 125, 255 };
                                }
                                else
                                {
                                    obj = Activator.CreateInstance(bodyType);
                                }

                                //See if the type is a list
                                PopulateLists(bodyType, obj);

                                //Now serialize into both xml and json
                                //First XML
                                DataContractSerializer xmlSerializer = new DataContractSerializer(bodyType);
                                StringWriter stringWriter = new StringWriter();
                                using (XmlWriter xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
                                {
                                    xmlSerializer.WriteObject(xmlWriter, obj);
                                }
                                string xml = stringWriter.ToString();
                                this.ltrXml2.Text = Server.HtmlEncode(xml);

                                //Now JSON
                                try
                                {
                                    MemoryStream stream = new MemoryStream();
                                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(bodyType);
                                    jsonSerializer.WriteObject(stream, obj);
                                    string json = Encoding.Default.GetString(stream.GetBuffer());

                                    //Make the json easier to read by adding newlines after commas
                                    json = json.Replace(",", ",\n");

                                    this.ltrJson2.Text = Server.HtmlEncode(json);
                                }
                                catch (SerializationException)
                                {
                                    //Ignore the error, since worst case, it just won't display the JSON,
                                    //but still the XML
                                    //There is a known (hard to fix) issue with JSON serialization where datetime objects
                                    //are not nullable and set to default value
                                    //https://benpowell.org/wcf-json-serialization-error-with-datetime-minval-and-utc/
                                    this.ltrJson2.Text = "Unable to display the JSON version.";
                                }

                                //Finally the documentation table
                                if (bodyType.Name == "String")
                                {
                                    FieldInfo field = typeof(FakeClassForDocumentation).GetField("StringField");
                                    this.rptBody.DataSource = new List<FieldInfo>() { field };
                                }
                                else if (bodyType.Name == "Byte[]")
                                {
                                    FieldInfo field = typeof(FakeClassForDocumentation).GetField("ByteArrayField");
                                    this.rptBody.DataSource = new List<FieldInfo>() { field };
                                }
                                else
                                {
                                    this.rptBody.DataSource = bodyType.GetFields();
                                }
                                this.rptBody.DataBind();
                            }
                        }
                    }

                    //Display the return value if there is one (both in XML and JSON formats)
                    Type returnType = matchedMethodInfo.ReturnType;
                    if (returnType != null && returnType != typeof(void))
                    {
                        //First activate the return type
                        object obj;
                        if (returnType == typeof(string))
                        {
                            obj = "String value";
                        }
                        else if (returnType.Name == "Byte[]")
                        {
                            //Sample byte array
                            obj = new byte[] { 25, 50, 125, 255 };
                        }
                        else if (returnType.Name == "Int32[]")
                        {
                            //Sample int array
                            obj = new int[] { 25, 50, 125, 255 };
                        }
                        else if (returnType.Name == "Int64[]")
                        {
                            //Sample long array
                            obj = new long[] { 25, 50, 125, 255 };
                        }
                        else
                        {
                            obj = Activator.CreateInstance(returnType);
                        }

                        //See if the type is a list
                        PopulateLists(returnType, obj);

                        //Now serialize into both xml and json
                        //First XML
                        DataContractSerializer xmlSerializer = new DataContractSerializer(returnType);
                        StringWriter stringWriter = new StringWriter();
                        using (XmlWriter xmlWriter = new XmlTextWriter(stringWriter) { Formatting = Formatting.Indented })
                        {
                            xmlSerializer.WriteObject(xmlWriter, obj);
                        }
                        string xml = stringWriter.ToString();
                        this.ltrXml.Text = Server.HtmlEncode(xml);

                        try
                        {
                            //Now JSON
                            MemoryStream stream = new MemoryStream();
                            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(returnType);
                            jsonSerializer.WriteObject(stream, obj);
                            string json = Encoding.Default.GetString(stream.GetBuffer());

                            //Make the json easier to read by adding newlines after commas
                            json = json.Replace(",", ",\n");

                            this.ltrJson.Text = Server.HtmlEncode(json);
                        }
                        catch (SerializationException)
                        {
                            //Ignore the error, since worst case, it just won't display the JSON,
                            //but still the XML
                            //There is a known (hard to fix) issue with JSON serialization where datetime objects
                            //are not nullable and set to default value
                            //https://benpowell.org/wcf-json-serialization-error-with-datetime-minval-and-utc/
                            this.ltrJson.Text = "Unable to display the JSON version.";
                        }

                        //Finally the documentation table
                        this.rptReturn.DataSource = returnType.GetFields();
                        this.rptReturn.DataBind();

                    }
                    else
                    {
                        this.ltrXml.Text = "Nothing Returned";
                        this.ltrJson.Text = "Nothing Returned";
                        this.plcTable.Visible = false;
                    }
                }

                //Databind
                this.DataBind();
            }
            catch (System.Exception exception)
            {
                Response.Write(Resources.Messages.Services_DocumentationUnavailable + " (" + exception.Message + ")!");
#if DEBUG
                Response.Write(exception.StackTrace);

#endif
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
            }
        }

        /// <summary>
        /// Looks for the presence of the [ReadOnly] custom attribute
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        protected bool IsReadWrite(FieldInfo fieldInfo)
        {
            if (fieldInfo.GetCustomAttributes(typeof(ReadOnlyAttribute), true).Length > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Populates a type if it's a list
        /// </summary>
        private void PopulateLists(Type type, object obj)
        {
            //If we have a list type, need to add one
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type itemType = type.GetGenericArguments()[0];
                object item = Activator.CreateInstance(itemType);
                ((System.Collections.IList)obj).Add(item);

/*                //Loop through all the item members and make sure all lists are populated
                FieldInfo[] fields = itemType.GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    if (!fieldInfo.IsStatic)
                    {
                        object fieldObj = fieldInfo.GetValue(item);
                        PopulateLists(fieldInfo.FieldType, fieldObj);
                    }
                }

                PropertyInfo[] properties = itemType.GetProperties();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object propObj = propertyInfo.GetValue(item, null);
                    PopulateLists(propertyInfo.PropertyType, propObj);
                }*/
            }
        }

        /// <summary>
        /// Returns the description of the specified parameter
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <returns>The parameter name</returns>
        protected string GetParameterDescription(string parameterName)
        {
            if (xmlMethodNode == null)
            {
                return "";
            }
            XmlNode xmlParamNode = xmlMethodNode.SelectSingleNode("param[@name='" + parameterName + "']");
            if (xmlParamNode == null)
            {
                return "";
            }
            return xmlParamNode.InnerText;
        }

        protected string GetBodyFieldDescription(FieldInfo fieldInfo)
        {
            string className = fieldInfo.DeclaringType.FullName;
            string fieldName = fieldInfo.Name;
            XmlNode xmlFieldNode = xmlCommentsDoc.SelectSingleNode("/doc/members/member[@name='F:" + className + "." + fieldName + "']");
            if (xmlFieldNode == null)
            {
                return "";
            }
            return xmlFieldNode.InnerText;
        }

        protected string GetReturnFieldDescription(FieldInfo fieldInfo)
        {
            string className = fieldInfo.DeclaringType.FullName;
            string fieldName = fieldInfo.Name;
            XmlNode xmlFieldNode = xmlCommentsDoc.SelectSingleNode("/doc/members/member[@name='F:" + className + "." + fieldName + "']");
            if (xmlFieldNode == null)
            {
                return "";
            }
            return xmlFieldNode.InnerText;
        }

        /// <summary>
        /// Used to just help generate documentation
        /// </summary>
        public class FakeClassForDocumentation
        {
            public string StringField;
            public byte[] ByteArrayField;
        }
    }
}