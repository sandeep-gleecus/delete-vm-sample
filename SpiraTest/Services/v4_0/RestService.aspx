<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RestService.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.v4_0.RestService1" EnableTheming="false" Theme="" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head id="Head1" runat="server">
        <title></title>
        <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
        <link rel="stylesheet" type="text/css" href="Documentation.css" />
        <link rel="stylesheet" type="text/css" href="../../App_Themes/InflectraTheme/InflectraTheme_Unity.css" />
    </head>
    <body class="pa0 ma0">
        <form id="frmMain" runat="server">
            <p class="pa4 mt0 mb6 bg-tiber fs-h3 ff-josefin white">
                <asp:Literal runat="server" ID="ltrProductName2"/>: REST Web Service (v4.0)
            </p>
            <div id="content" class="vw-50-xl vw-66-lg vw-75-md vw-75_sm vw-90-xs vw-90-xxs mx-auto">
                <h1>
                    Resources
                </h1>
                <p>
                    This table provides a list of all the resources exposed by this REST web service. More detail about each resource is explored after this table:
                    <tstsc:HyperLinkEx 
                        CssClass="br1 px4 py1 ba b-gray gray bg-yellow-pale-hover"
                        runat="server" 
                        Text="Export as WADL-JSON" 
                        NavigateUrl="RestServiceDescription.ashx" 
                        Target="_blank" />
                </p>
                <asp:GridView 
                    ID="grdResources" 
                    runat="server" 
                    AutoGenerateColumns="false"
                    CssClass="bn bb bw2"
                    HeaderStyle-CssClass="bg-tiber white"
                    >
                    <Columns>
                        <asp:TemplateField HeaderText="Resource" HeaderStyle-CssClass="pa3 tl fw-normal bn">
                            <ItemTemplate>
                                <asp:HyperLink runat="server" NavigateUrl='<%#"#" + ((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Key %>' Text="<%#((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Key %>" />
                            </ItemTemplate>                        
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Methods" HeaderStyle-CssClass="pa3 tl fw-normal bn">
                            <ItemTemplate>
                                <asp:Repeater ID="rptMethods" runat="server" DataSource="<%#((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Value.Methods %>">
                                    <ItemTemplate>
                                        <span class="fixedWidth"><%#(string)(Container.DataItem) %></span>
                                    </ItemTemplate>
                                    <SeparatorTemplate>,</SeparatorTemplate>
                                </asp:Repeater>                            
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Description" HeaderStyle-CssClass="pa3 tl fw-normal bn">
                            <ItemTemplate>
                                <%#((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Value.Description %>
                            </ItemTemplate>                        
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

                <h1 class="fs-h2 mt7">How To Access The API</h1>
                <h2 class="silver fs-h3 mb2">Using the Right URL</h2>
                <p class="mt2">To access these REST web services, you need to use the specific URL of the application.</p>
                <p>For instance, if you access the application in the browser from <strong>https://companyname.spiraservice.net</strong>, then that will be the base of the URL used for accessing the API.
                    And if you are on version 4, the API URL would be: <strong>https://companyname.spiraservice.net</strong>/services/v4_0/RestService.svc
                </p>
                <p>If you are viewing this page from your application, the base URL is:</p>
                <p class="fs-h4 ff-josefin mb4"><strong><asp:HyperLink ID="lnkBaseUrl" runat="server" /></strong></p>

                <h2 class="silver fs-h3 mt5 mb2">Specifying The Data Format</h2>
                <p class="mt2">You can specify the format of data that will be returned (XML or JSON) by passing the following HTTP Headers:</p>
                <ul class="ma0 ml3 pl5">
                    <li class="mb2">
                        <strong>Content-Type: application/xml</strong> - Sends data in XML format
                    </li>
                    <li class="mb2">
                        <strong>accept: application/xml</strong> - Returns data in XML format
                    </li>
                    <li class="mb2">
                        <strong>Content-Type: application/json</strong> - Sends data in JSON format                    
                    </li>
                    <li class="mb2">
                        <strong>accept: application/json</strong> - Returns data in JSON format                    
                    </li>
                </ul>
                <p><strong>Note: </strong> when sending data to Spira, make sure all tags are sent in alphabetical order</p>

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

                <h2 class="silver fs-h3 mt5 mb2">Accessing Across Domains</h2>
                <p>
                    Finally if you need to call one of the <span class="fixedWidth">GET</span> methods from a web page in another domain
                    you need to use the special JSONP (JSON with Padding) syntax. This tells the web service to return
                    the data in the form of a JavaScript &lt;script&gt; tag so that it be can be called across web site domains safely.
                    To request the data in JSONP format, just add the following extra querystring parameter:
                </p>
                <pre>?callback=[name of callback function]</pre>
                    
                <h2 class="silver fs-h3 mt5 mb2">Example REST Call</h2>
                <p class="mt2"> When starting to try out the API we recommend starting with a simple <span class="fixedWidth">GET</span> request, to make sure the connection is working as expected.</p>
                <p>A good first API call to make is to get a list of all projects the user connecting via the API has access to. To make this call we need the following information</p>
                <ol class="ma0 ml3 pl5">
                    <li class="mb2">The base url for accessing the service (for this current application it is <span class="fixedWidth"><asp:Literal runat="server" ID="baseUrlText" /></span>)</li>
                    <li class="mb2">The additional query string to get the list of projects (<span class="fixedWidth">projects</span>)</li>
                    <li class="mb2">The username of a user with sufficient access to the system (let's use <span class="fixedWidth">Fred Bloggs</span>)</li>
                    <li class="mb2">The API-key for that user (in this case, let's assume it's <span class="fixedWidth">{XXXXXXXXXXXXXXXX}</span>)</li>
                    <li class="mb2">Finally, we need to decide what form we want the information to be sent to us in (we will ask for JSON here)</li>
                </ol>
                <p>Putting that all together we can create the URL we need. If we put the authentication into the URL, the full URL will be:</p>
                <pre><asp:Literal runat="server" ID="baseUrlTextExample" />projects?fredbloggs&{XXXXXXXXXXXXXXXX}<br />with a header of accept: application/json</pre>

                <h1 class="fs-h2 mt7">
                    Resources Details
                </h1>
                <asp:Repeater ID="rptResourceOperations" runat="server">
                    <ItemTemplate>
                        <h2 class="silver fs-h3 mt6 mb3"><a runat="server" name='<%#((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Key %>' />
                        <%#((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Key%></h2>
                        <table class="mt2 w-100">
                            <thead>
                                <tr class="bg-slate white">
                                    <th class="pa3 tl fw-normal bn">URI</th>
                                    <th class="pa3 tl fw-normal bn">Method</th>
                                    <th class="pa3 tl fw-normal bn">Description</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptOperations" runat="server" DataSource="<%#((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Value.Operations.OrderBy(o => o.Uri) %>">
                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <asp:HyperLink runat="server" Text="<%#((RestOperationInfo)(Container.DataItem)).Uri%>" CssClass="Uri"
                                                    NavigateUrl='<%# "RestServiceOperation.aspx?uri=" + Server.UrlEncode(((RestOperationInfo)(Container.DataItem)).Uri) + "&method=" + Server.UrlEncode(((RestOperationInfo)(Container.DataItem)).Method) %>' />                                                
                                            </td>
                                            <td>
                                                <span class="fixedWidth">
                                                    <%#((RestOperationInfo)(Container.DataItem)).Method%>
                                                </span>
                                            </td>
                                            <td>
                                                <asp:PlaceHolder ID="plcDescription" runat="server" Visible="<%#!String.IsNullOrEmpty(((RestOperationInfo)(Container.DataItem)).Description) %>">
                                                    <%#((RestOperationInfo)(Container.DataItem)).Description%>
                                                </asp:PlaceHolder>
                                                <asp:PlaceHolder ID="plcNotImplemented" runat="server" Visible="<%#String.IsNullOrEmpty(((RestOperationInfo)(Container.DataItem)).Description) %>">
                                                    <span class="notImplemented">Not Implemented</span>
                                                </asp:PlaceHolder>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                            <tfoot class="bg-vlight-gray dark-gray">
                                <tr>
                                    <td colspan="3">
                                        &gt; <tstsc:HyperLinkEx CssClass="br1 px4 py1 bg-white gray" runat="server" Text="Export to Rapise .REST File" NavigateUrl='<%# "RestServiceExport.ashx?resource=" + Server.UrlEncode(((KeyValuePair<string, RestResourceInfo>)(Container.DataItem)).Key)%>' Target="_blank" />
                                    </td>
                                </tr>
                            </tfoot>    
                        </table>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </form>
    </body>
</html>

