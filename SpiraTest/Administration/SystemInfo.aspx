<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="SystemInfo.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.SystemInfo" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-xs-12 pa2">
                <h1>
                    <tstsc:LabelEx
                        ID="lblAdminGroup"
                        CssClass="pale"
                        runat="server"
                        Text="<%$ Resources:Main,Admin_SystemInfo %>" AppendColon="true" />
                    <asp:Literal runat="server" ID="lblProductName" />
                </h1>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12 lead pa2">
                <asp:Literal runat="server"
                    Text='<%# Resources.Main.System_About_Intro.Replace("{0}", Inflectra.SpiraTest.Common.ConfigurationSettings.Default.License_ProductType) %>' />
            </div>
        </div>

        <div class="row">
            <div class="col-xs-6 col-md-3 pa2">
                <asp:Literal runat="server"
                    Text="<%$ Resources:Fields,ProductType %>" />:
            </div>
            <div class="col-xs-6 col-md-9 pa2">
                <asp:Literal ID="lblLicenseProduct" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6 col-md-3 pa2">
                <asp:Literal runat="server"
                    Text="<%$ Resources:Main,System_About_Version %>" />:
            </div>
            <div class="col-xs-6 col-md-9 pa2">
                <asp:Literal ID="lblVersionBuild" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6 col-md-3 pa2">
                <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_Libraries %>" />:
            </div>
            <div class="col-xs-6 col-md-9 pa2">
                <asp:Literal ID="lblRefAssemblies" runat="server" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6 col-md-3 pa2">
                <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_BrowserDetected %>" />:
            </div>
            <div class="col-xs-6 col-md-9 pa2">
                <asp:Literal ID="lblBrowser" runat="server" />
            </div>
        </div>

        <div class="row">
            <div class="col-xs-12 col-md-12 pa2">
                <h3>
                    <asp:Literal runat="server"
                        Text="<%$ Resources:Main,System_About_DatabaseServer %>" />:
                </h3>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_DatabaseServer_Name %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblDBServer" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_DatabaseServer_Database %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblDatabase" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_DatabaseServer_User %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblDBUser" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_DatabaseServer_Version %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblDBSvrVer" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_DatabaseServer_Language %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblServerColl" runat="server" />
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-xs-12 col-md-12 pa2">
                <h3>
                    <asp:Literal runat="server"
                        Text="<%$ Resources:Main,System_About_ApplicationServer %>" />:
                </h3>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_ApplicationServer_IISVersion %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblApp_IIS" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_ApplicationServer_OSVersion %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblApp_OS" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_ApplicationServer_RuntimeVersion %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblApp_Asp" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_ApplicationServer_ApplPath %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblApp_Path" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_ApplicationServer_ServerName %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblApp_SvrPort" runat="server" />
                </div>
            </div>
            <div class="row ma0">
                <div class="col-xs-6 col-md-3 pa2">
                    <asp:Literal runat="server" Text="<%$ Resources:Main,System_About_ApplicationServer_Protocol %>" />:
                </div>
                <div class="col-xs-6 col-md-9 pa2">
                    <asp:Literal ID="lblApp_Prot" runat="server" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
</asp:Content>
