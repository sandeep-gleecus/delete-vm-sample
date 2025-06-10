<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RestServiceOperation.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.v4_0.RestServiceOperation" EnableTheming="false" Theme="" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <title></title>
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <link rel="stylesheet" type="text/css" href="Documentation.css" />
    <link rel="stylesheet" type="text/css" href="../../App_Themes/InflectraTheme/InflectraTheme_Unity.css" />
</head>
<body class="pa0 ma0">
    <form id="frmMain" runat="server">
        <p class="pa4 mt0 mb4 bg-tiber fs-h3 ff-josefin white">
            <asp:Literal runat="server" ID="ltrProductName2"/>: REST Web Service (v4.0)
        </p>
        <div id="content" class="mb6 vw-50-xl vw-66-lg vw-75-md vw-75_sm vw-90-xs vw-90-xxs mx-auto">
            <a class="mb4 br2 px4 py1 bg-near-white bg-vlight-gray-hover transition-all" href="RestService.aspx">See all operations</a>
            <h1 class="fs-h2">
                 <asp:Literal runat="server" ID="ltrMethodName" />: <asp:Literal runat="server" ID="ltrOperationUri" />
            </h1>
            <h2 class="silver fs-h3 mb2 mt5">Description</h2>
            <p class="intro">
               <asp:Literal runat="server" ID="ltrSummary" />
            </p>
            <p class="intro">
                <asp:Literal runat="server" ID="ltrRemarks" />
            </p>
            <asp:PlaceHolder ID="plcExample" runat="server">
                <h3>Example(s)</h3>
                <pre>
                  <asp:Literal runat="server" ID="ltrExample" />
                </pre>
            </asp:PlaceHolder>
            <h2 class="silver fs-h3 mb2 mt5">How to Execute</h2>
            <p class="mb0">
                To access this REST web service, you need to use the following URL:            
            </p>
            <p class="mt2">
                <asp:HyperLink ID="lnkFullUrl" runat="server" />
            </p>
            <asp:PlaceHolder ID="plcParameters" runat="server">
            <h2 class="silver fs-h3 mb2 mt5">Request Parameters</h2>
              <table>
                <thead>
                    <tr>
                      <th class="px3 tl">
                        Name
                      </th>
                      <th class="px3 tl">
                        Description
                      </th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptParameters" runat="server">
                        <ItemTemplate>
                            <tr>
                              <td>
                                <%#((System.Reflection.ParameterInfo)(Container.DataItem)).Name %>
                              </td>
                              <td>
                                <%#GetParameterDescription(((System.Reflection.ParameterInfo)(Container.DataItem)).Name) %>
                              </td>
                            </tr>                        
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
               </table>                      
            </asp:PlaceHolder>
            <h2 class="silver fs-h3 mb2 mt6">Request Body</h2>
            <p>
                <input type="button" id="btnTable2" name="btnTable2" value="Documentation" class="Selected" onclick="display2('table')" />
                <input type="button" id="btnJSON2" name="btnJSON2" value="JSON" onclick="display2('json')" />
                <input type="button" id="btnXML2" name="btnXML2" value="XML"  onclick="display2('xml')" />
            </p>
            <div id="divTable2">
            <asp:PlaceHolder ID="plcTable2" runat="server">
              <table>
                <thead>
                    <tr>
                      <th class="px3 tl">
                        Property
                      </th>
                      <th class="px3 tl">
                        Description
                      </th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptBody" runat="server">
                        <ItemTemplate>
                            <asp:PlaceHolder ID="plcBodyItem" runat="server" Visible="<%#IsReadWrite((System.Reflection.FieldInfo)(Container.DataItem)) %>">
                                <tr>
                                  <td>
                                    <%#((System.Reflection.FieldInfo)(Container.DataItem)).Name%>
                                  </td>
                                  <td>
                                    <%#GetBodyFieldDescription(((System.Reflection.FieldInfo)(Container.DataItem)))%>
                                  </td>
                                </tr>                        
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
               </table>  
            </asp:PlaceHolder>
            </div>

            <div id="divXml2" style="display:none">
                <pre class="ReturnData">
                    <asp:Literal ID="ltrXml2" runat="server" />
                </pre>
            </div>
            <div id="divJson2" style="display:none">
                <pre class="ReturnData">
                    <asp:Literal ID="ltrJson2" runat="server" />
                </pre>            
            </div>

            <h2 class="silver fs-h3 mb2 mt6">Return Data</h2>
            <p>The JSON and XML examples below show the shape of one entry that will be returned. It does not show an example of how that entry will be populated.</p>
            <p>
                <input type="button" id="btnTable" name="btnTable" value="Documentation" class="Selected" onclick="display('table')" />
                <input type="button" id="btnJSON" name="btnJSON" value="JSON" onclick="display('json')" />
                <input type="button" id="btnXML" name="btnXML" value="XML" onclick="display('xml')" />
            </p>
            <div id="divTable">
            <asp:PlaceHolder ID="plcTable" runat="server">
              <table>
                <thead>
                    <tr>
                      <th class="px3 tl">
                        Property
                      </th>
                      <th class="px3 tl">
                        Description
                      </th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptReturn" runat="server">
                        <ItemTemplate>
                            <tr>
                              <td>
                                <%#((System.Reflection.FieldInfo)(Container.DataItem)).Name%>
                              </td>
                              <td>
                                <%#GetReturnFieldDescription(((System.Reflection.FieldInfo)(Container.DataItem)))%>
                              </td>
                            </tr>                        
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
               </table>  
            </asp:PlaceHolder>
            </div>

            <div id="divXml" style="display:none">
<pre class="ReturnData">
<asp:Literal ID="ltrXml" runat="server" />
</pre>
            </div>
            <div id="divJson" style="display:none">
<pre class="ReturnData">
<asp:Literal ID="ltrJson" runat="server" />
</pre>            
            </div>
        </div>
        <script type="text/javascript">
            function display(type)
            {
                if (type == 'json')
                {
                    document.getElementById('btnTable').className = '';
                    document.getElementById('btnXML').className = '';
                    document.getElementById('btnJSON').className = 'Selected';
                    document.getElementById('divTable').style.display = 'none';
                    document.getElementById('divXml').style.display = 'none';
                    document.getElementById('divJson').style.display = 'block';
                }
                if (type == 'xml')
                {
                    document.getElementById('btnTable').className = '';
                    document.getElementById('btnXML').className = 'Selected';
                    document.getElementById('btnJSON').className = '';
                    document.getElementById('divTable').style.display = 'none';
                    document.getElementById('divXml').style.display = 'block';
                    document.getElementById('divJson').style.display = 'none';
                }
                if (type == 'table')
                {
                    document.getElementById('btnTable').className = 'Selected';
                    document.getElementById('btnXML').className = '';
                    document.getElementById('btnJSON').className = '';
                    document.getElementById('divTable').style.display = 'block';
                    document.getElementById('divXml').style.display = 'none';
                    document.getElementById('divJson').style.display = 'none';
                }
            }

            function display2(type)
            {
                if (type == 'json')
                {
                    document.getElementById('btnTable2').className = '';
                    document.getElementById('btnXML2').className = '';
                    document.getElementById('btnJSON2').className = 'Selected';
                    document.getElementById('divTable2').style.display = 'none';
                    document.getElementById('divXml2').style.display = 'none';
                    document.getElementById('divJson2').style.display = 'block';
                }
                if (type == 'xml')
                {
                    document.getElementById('btnTable2').className = '';
                    document.getElementById('btnXML2').className = 'Selected';
                    document.getElementById('btnJSON2').className = '';
                    document.getElementById('divTable2').style.display = 'none';
                    document.getElementById('divXml2').style.display = 'block';
                    document.getElementById('divJson2').style.display = 'none';
                }
                if (type == 'table')
                {
                    document.getElementById('btnTable2').className = 'Selected';
                    document.getElementById('btnXML2').className = '';
                    document.getElementById('btnJSON2').className = '';
                    document.getElementById('divTable2').style.display = 'block';
                    document.getElementById('divXml2').style.display = 'none';
                    document.getElementById('divJson2').style.display = 'none';
                }
            }
        </script>
    </form>
</body>
</html>
