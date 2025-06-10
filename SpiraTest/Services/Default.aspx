<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Services.Default" EnableTheming="false" Theme="" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <link rel="stylesheet" type="text/css" href="Documentation.css" />
    <link rel="stylesheet" type="text/css" href="../App_Themes/InflectraTheme/InflectraTheme_Unity.css" />
</head>
<body class="pa0 ma0">
    <form id="frmAspNet" runat="server">
        <div id="content">
            <h1 class="pa4 bg-tiber fs-h3 ff-josefin white fw-normal ma0">
                <asp:Literal runat="server" ID="ltrProductName"/>: Web Services
            </h1>
            <br />
                <p class="tc fw-b fs-110 mb5">
                    This table provides a list of all the web services available, by version and protocol in this instance:
                </p>
            <table class="mx-auto">
                <thead>
                    <tr class="bg-tiber white">
                        <th class="px3 py2 tl fw-normal">
                            Protocol
                        </th>
                        <th class="px3 py2 tl fw-normal">
                            Documentation
                        </th>
                        <th class="px3 py2 tl fw-normal">
                            Service URL
                        </th>
                    </tr>
                </thead>
                <tbody>
                   <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 6.0
                        </td>
                    </tr>
                    <tr class="bb b-light-gray">
                        <td class="px3 fw-b">
                            REST
                        </td>
                        <td class="px3">
                            <a id="A17" href="~/Services/v6_0/RestService.aspx" runat="server">/Services/v6_0/RestService.aspx</a>
                        </td>
                        <td class="px3">
                            <a id="A18" href="~/Services/v6_0/RestService.svc" runat="server">/Services/v6_0/RestService.svc</a>
                        </td>
                    </tr>
                    <tr class="bb b-light-gray">
                        <td class="px3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">
                            <a id="A19" href="~/Services/v6_0/SoapService.aspx" runat="server">/Services/v6_0/SoapService.aspx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A20" href="~/Services/v6_0/SoapService.svc" runat="server">/Services/v6_0/SoapService.svc</a>
                        </td>
                    </tr>
                    <tr runat="server" id="trOdata">
                        <td class="px3 pb3 fw-b">
                            ODATA
                        </td>
                        <td class="px3 pb3">
                            <a id="A23" href="~/Services/OData.aspx" runat="server">/Services/OData.aspx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A22" href="~/api/odata" runat="server">/api/odata</a>
                        </td>
                    </tr>
                    <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 5.0
                        </td>
                    </tr>
                    <tr class="bb b-light-gray">
                        <td class="px3 fw-b">
                            REST
                        </td>
                        <td class="px3">
                            <a id="A3" href="~/Services/v5_0/RestService.aspx" runat="server">/Services/v5_0/RestService.aspx</a>
                        </td>
                        <td class="px3">
                            <a id="A4" href="~/Services/v5_0/RestService.svc" runat="server">/Services/v5_0/RestService.svc</a>
                        </td>
                    </tr>
                    <tr>
                        <td class="px3 pb3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">
                            <a id="A1" href="~/Services/v5_0/SoapService.aspx" runat="server">/Services/v5_0/SoapService.aspx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A2" href="~/Services/v5_0/SoapService.svc" runat="server">/Services/v5_0/SoapService.svc</a>
                        </td>
                    </tr>
                    <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 4.0
                        </td>
                    </tr>
                    <tr class="bb b-light-gray">
                        <td class="px3 fw-b">
                            REST
                        </td>
                        <td class="px3">
                            <a id="A5" href="~/Services/v4_0/RestService.aspx" runat="server">/Services/v4_0/RestService.aspx</a>
                        </td>
                        <td class="px3">
                            <a id="A6" href="~/Services/v4_0/RestService.svc" runat="server">/Services/v4_0/RestService.svc</a>
                        </td>
                    </tr>
                    <tr>
                        <td class="px3 pb3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">
                            <a id="A7" href="~/Services/v4_0/ImportExport.aspx" runat="server">/Services/v4_0/ImportExport.aspx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A8" href="~/Services/v4_0/ImportExport.svc" runat="server">/Services/v4_0/ImportExport.svc</a><br />
                            <a id="A11" href="~/Services/v4_0/DataSync.svc" runat="server">/Services/v4_0/DataSync.svc</a>
                        </td>
                    </tr>

                    <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 3.0
                        </td>
                    </tr>
                    <tr>
                        <td class="px3 pb3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">
                            <a href="~/Services/v3_0/ImportExport.aspx" runat="server">/Services/v3_0/ImportExport.aspx</a>
                        </td>
                        <td class="px3 pb3">
                            <a href="~/Services/v3_0/ImportExport.svc" runat="server">/Services/v3_0/ImportExport.svc</a><br />
                            <a id="A9" href="~/Services/v3_0/DataSync.svc" runat="server">/Services/v3_0/DataSync.svc</a>
                        </td>
                    </tr>

                    <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 2.2
                        </td>
                    </tr>
                    <tr>
                        <td class="px3 pb3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">                            
                            <a id="A16" href="~/Services/v2_2/ImportExport.asmx" runat="server">/Services/v2_2/ImportExport.asmx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A10" href="~/Services/v2_2/ImportExport.asmx" runat="server">/Services/v2_2/ImportExport.asmx</a>
                        </td>
                    </tr>

                    <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 1.5
                        </td>
                    </tr>
                    <tr>
                        <td class="px3 pb3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">                            
                            <a id="A15" href="~/Services/v1_5_2/Import.asmx" runat="server">/Services/v1_5_2/Import.asmx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A12" href="~/Services/v1_5_2/Import.asmx" runat="server">/Services/v1_5_2/Import.asmx</a>
                        </td>
                    </tr>
                    <tr class="bg-slate light-gray">
                        <td colspan="3" class="px3 py2 tl">
                            Version 1.2
                        </td>
                    </tr>
                    <tr>
                        <td class="px3 pb3 fw-b">
                            SOAP
                        </td>
                        <td class="px3 pb3">                            
                            <a id="A14" href="~/Services/v1_2_0/TestExecute.asmx" runat="server">/Services/v1_2_0/TestExecute.asmx</a>
                        </td>
                        <td class="px3 pb3">
                            <a id="A13" href="~/Services/v1_2_0/TestExecute.asmx" runat="server">/Services/v1_2_0/TestExecute.asmx</a>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </form>
</body>
</html>
