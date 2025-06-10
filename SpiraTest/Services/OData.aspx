<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OData.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.OData" EnableTheming="false" Theme="" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head id="Head1" runat="server">
        <title></title>
        <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
        <link rel="stylesheet" type="text/css" href="Documentation.css" />
        <link rel="stylesheet" type="text/css" href="../App_Themes/InflectraTheme/InflectraTheme_Unity.css" />
    </head>
    <body class="pa0 ma0 mb6">
        <form id="frmMain" runat="server">
            <p class="pa4 mt0 mb6 bg-tiber fs-h3 ff-josefin white"><asp:Literal runat="server" ID="ltrProductName2"/>: OData Web Service</p>
            <div class="vw-50-xl vw-66-lg vw-75-md vw-75_sm vw-90-xs vw-90-xxs mx-auto">
                <div>
                    <h1 class="fs-h2">How To Access The OData API</h1>
                    <h2 class="silver fs-h3 mb2">Using the Right URL</h2>
                    <p class="mt2">To access OData web services, you need to use the specific URL of the application.</p>
                    <p>For instance, if you access the application in the browser from <strong>https://companyname.spiraservice.net</strong>, then that will be the base of the URL used for accessing the API.
                        The API URL will be: <strong>https://companyname.spiraservice.net/api/odata</strong>
                    </p>
                    <p>If you are viewing this page from your application, the base URL is:</p>
                    <p class="fs-h4 ff-josefin mb4"><strong><asp:HyperLink ID="lnkBaseUrl" runat="server" /></strong></p>

                    <h2 class="silver fs-h3 mt5 mb2">Specifying The Data Format</h2>
                    <p class="mt2">The OData API supported in <asp:Literal runat="server" ID="ltrProductName3"/> is <a href="https://www.odata.org/" target="_blank">OData v4.0</a> which is based on JSON,
                        so you need to use the following content type / accept headers:</p>
                    <ul class="ma0 ml3 pl5">
                        <li class="mb2">
                            <strong>Content-Type: application/json</strong> - Sends data in JSON format                    
                        </li>
                        <li class="mb2">
                            <strong>accept: application/json</strong> - Returns data in JSON format                    
                        </li>
                    </ul>

                    <h2 class="silver fs-h3 mt5 mb2">Authentication</h2>
                    <p class="mt2">
                        There are three different ways to authenticate with the web service. They all need a username and an api-key. The api-key used is the same as the RSS token created by the application.
                        You can find a user's RSS token by either going to the user profile (either by visiting your own profile page, or accessing it via the administration panel). If no RSS token is shown for a user, make sure "Enable RSS Feeds" is set to yes.
                    </p>
                    <p>
                        Copy the full RSS-token / api-key for the relevant user--including the curly braces { }. In the below examples the username "fredbloggs" is used, with an api-key of "{XXXXXXXXXXXXXXXX}"
                    </p>
                    <ol class="ma0 ml3 pl5">
                        <li class="mb2">
                            Append the username and application API-Key as an extra querystring parameter:                    
                            <pre>?username=fredbloggs&api-key={XXXXXXXXXXXXXXXX}</pre>
                        </li>
                        <li class="mb2">
                            Pass in the username application API-Key using the <strong>username</strong> and <strong>api-key</strong> HTTP headers:
                            <pre>username: fredbloggs<br />api-key: {XXXXXXXXXXXXXXXX}</pre>
                        </li>
                        <li class="mb2">
                            Pass in the <b>username</b> and <b>api-key</b> using the standard HTTP basic authentication header:
                            <pre>Authorization: Basic XXXXXXXXXXXXXXXXXXXXXXXXXX<br />where XXXXXXXXXXXXXXXXXXXXXXXXXX is username:api-key base64 encoded</pre>
                        </li>
                    </ol>
                    
                    <h2 class="silver fs-h3 mt5 mb2">Example OData Calls</h2>
                    <p class="alert-box">                        
                        <b>Note:</b> OData is primarily designed to allow third-party reporting tools such as PowerBI and Excel to access the data in 
                        <asp:Literal runat="server" ID="ltrProductName4"/> for the purposes of creating custom graphs and reports.
                        So although we include some examples here of using the raw OData protocol manually, we recommend
                        that you consider using a tool such as Excel or PowerBI instead.
                    </p>
                    <p class="mt2">
                        When you connect to the main OData endpoint URL (e.g. <code>https://companyname.spiraservice.net/api/odata</code>), you will receive a JSON response containing the list of reportable
                        views available for querying:
                    </p>
<pre class="code">
{
   "@odata.context":"https://companyname.spiraservice.net/api/odata/$metadata",
   "value":[
      {
         "name":"ArtifactAssociations",
         "kind":"EntitySet",
         "url":"ArtifactAssociations"
      },
      {
         "name":"ArtifactAttachments",
         "kind":"EntitySet",
         "url":"ArtifactAttachments"
      },
      {
         "name":"Attachments",
         "kind":"EntitySet",
         "url":"Attachments"
      },
      {
         "name":"AutomationHosts",
         "kind":"EntitySet",
         "url":"AutomationHosts"
      },
        ...
      {
         "name":"Users",
         "kind":"EntitySet",
         "url":"Users"
      }
   ]
}
</pre>
                    <p class="mt2">
                        From here, you can now add the name of the EntitySet to your URL and then it will display the first 50 rows
                        of data from the appropriate EntitySet.
                    </p>
                    <p class="mt2">
                        For example, if you were to query
                        <code>https://companyname.spiraservice.net/api/odata/Incidents</code>, you would see:
                    </p>
                    <pre class="code">
{
   "@odata.context":"https://companyname.spiraservice.net/api/odata/$metadata#Incidents",
   "value":[
      {
         "INCIDENT_ID":1,
         "PROJECT_ID":1,
         "INCIDENT_STATUS_ID":1,
         "INCIDENT_TYPE_ID":1,
         "OPENER_ID":2,
         "DESCRIPTION":"When trying to log into the application with a valid username and password, the system throws a fatal exception",
         "CREATION_DATE":"2021-03-02T20:04:45.62-05:00",
         "CLOSED_DATE":null,
         "LAST_UPDATE_DATE":"2021-03-19T20:04:45.62-04:00",
         "START_DATE":null,
         "COMPLETION_PERCENT":0,
         "IS_DELETED":false,
         "PRIORITY_NAME":null,
         "PRIORITY_COLOR":null,
         "SEVERITY_NAME":null,
         "SEVERITY_COLOR":null,
         "INCIDENT_STATUS_NAME":"New",
         "INCIDENT_TYPE_NAME":"Incident",
         "OPENER_NAME":"Fred Bloggs",
         "OWNER_NAME":null,
         "DETECTED_RELEASE_VERSION_NUMBER":"1.0.0.0",
         "RESOLVED_RELEASE_VERSION_NUMBER":"1.0.1.0",
         "VERIFIED_RELEASE_VERSION_NUMBER":"1.0.1.0",
         "PROJECT_GROUP_ID":2,
         "PROJECT_NAME":"Library Information System (Sample)",
         "CUST_01":null,
         "CUST_02":null,
         "CUST_03":null,
        ...
         "CUST_99":null,
         "NAME":"Cannot log into the application",
         "IS_ATTACHMENTS":true,
         "COMPONENT_IDS":null,
         "INCIDENT_STATUS_IS_OPEN_STATUS":true,
         "PROJECT_IS_ACTIVE":true,
         "CONCURRENCY_DATE":"2021-03-19T20:04:45.62-04:00",
         "END_DATE":null,
         "DETECTED_BUILD_NAME":null,
         "RESOLVED_BUILD_NAME":null
      },
      ...
      {
         "INCIDENT_ID":2,
         "PROJECT_ID":1,
         "INCIDENT_STATUS_ID":1,
         "INCIDENT_TYPE_ID":1,
         "OPENER_ID":3,
         "DESCRIPTION":"When I try and click on the button to add a new author the system simply displays the main screen and does nothing",
         "CREATION_DATE":"2021-03-02T20:04:45.62-05:00",
         "CLOSED_DATE":null,
         "LAST_UPDATE_DATE":"2021-03-07T20:04:45.62-05:00",
         "START_DATE":null,
         "COMPLETION_PERCENT":0,
         "IS_DELETED":false,
         "PRIORITY_NAME":null,
         "PRIORITY_COLOR":null,
         "SEVERITY_NAME":null,
         "SEVERITY_COLOR":null,
         "INCIDENT_STATUS_NAME":"New",
         "INCIDENT_TYPE_NAME":"Incident",
         "OPENER_NAME":"Joe P Smith",
         "OWNER_NAME":null,
         "DETECTED_RELEASE_VERSION_NUMBER":"1.0.0.0",
         "RESOLVED_RELEASE_VERSION_NUMBER":"1.0.1.0",
         "VERIFIED_RELEASE_VERSION_NUMBER":"1.0.1.0",
         "PROJECT_GROUP_ID":2,
         "PROJECT_NAME":"Library Information System (Sample)",
         "CUST_01":null,
         "CUST_02":null,
         "CUST_03":null,
         ...
         "CUST_99":null,
         "NAME":"Not able to add new author",
         "IS_ATTACHMENTS":false,
         "COMPONENT_IDS":"2",
         "INCIDENT_STATUS_IS_OPEN_STATUS":true,
         "PROJECT_IS_ACTIVE":true,
         "CONCURRENCY_DATE":"2021-03-07T20:04:45.62-05:00",
         "END_DATE":null,
         "DETECTED_BUILD_NAME":null,
         "RESOLVED_BUILD_NAME":null
      },
    ],
    "@odata.nextLink":"https://companyname.spiraservice.net/api/odata/Incidents?$skip=50"
}
                    </pre>

                    <h2 class="silver fs-h3 mt5 mb2">Advanced Querying Options</h2>
                    <p class="mt2">
                        The <a href="https://www.odata.org/getting-started/basic-tutorial/" target="_blank">OData protocol</a> lets you perform standard querying on the data, including filtering, sorting
                        and pagination. These are done using special operators that get added to the querystring:
                    </p>
                    <h3>Filtering</h3>
                    <p class="mt2">
                        To <a href="https://www.odata.org/getting-started/basic-tutorial/#filter" target="_blank">filter data using OData</a>,
                        you simply use the <code>$filter=</code> operator in the URL. A simple example is shown below for filtering
                        the list of incidents to be only those that are priority "1 - High":
                    </p>
                    <pre class="code">
https://companyname.spiraservice.net/api/odata/Incidents?$filter=PRIORITY_NAME eq '2 - High'
                    </pre>
                    <h3>Sorting</h3>
                    <p class="mt2">
                        To <a href="https://www.odata.org/getting-started/basic-tutorial/#orderby" target="_blank">sort data using OData</a>,
                        you simply use the <code>$orderby=</code> operator in the URL. A simple example is shown below for sorting
                        the list of incidents by priority:
                    </p>
                    <pre class="code">
https://companyname.spiraservice.net/api/odata/Incidents?$orderby=PRIORITY_NAME asc
                    </pre>
                    <h3>Other Query Operators</h3>
                    <p>
                        For more information on querying OData, please refer to the following guide:<br />
                        <a href="https://www.odata.org/getting-started/basic-tutorial/#queryData" target="_blank">
                            https://www.odata.org/getting-started/basic-tutorial/#queryData
                        </a>
                    </p>
                </div>
            </div>
        </form>
    </body>
</html>

