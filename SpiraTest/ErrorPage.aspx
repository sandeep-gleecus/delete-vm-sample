<%@ Page 
    Language="c#" 
    CodeBehind="ErrorPage.aspx.cs" 
    AutoEventWireup="True" 
    Inherits="Inflectra.SpiraTest.Web.ErrorPage"
    MasterPageFile="~/MasterPages/Main.Master" 
    %>

<%@ Import Namespace="System.Data" %>
<%@ Register 
    TagPrefix="tstsc" 
    Namespace="Inflectra.SpiraTest.Web.ServerControls"
    Assembly="Web" 
    %>
<asp:Content 
    ContentPlaceHolderID="cplHead" 
    runat="server" 
    ID="Content1"
    >
</asp:Content>
<asp:Content 
    ContentPlaceHolderID="cplMainContent" 
    runat="server" 
    ID="Content2"
    >
 	<div class="w960 mvw-100 pa4 mx-auto">
        <h2 class="mt7 mt5-sm df fs-h1">
            <i class="far fa-frown-open mr3 o-70"></i>
            <asp:Localize 
                runat="server" 
                Text="<%$Resources:Main,ErrorPage_Title %>" 
                />
        </h2>

        <div 
            class="fs-110" 
            id="lblMessage" 
            runat="server"
            >      
            <asp:Localize 
                runat="server" 
                Text="<%$Resources:Main,ErrorPage_PleaseClick %>" 
                />
            <tstsc:HyperLinkEx 
                CssClass="yolk-hover orange transition-all tdn tdn-hover"
                NavigateUrl="~/MyPage.aspx" 
                runat="server" 
                Text="<%$Resources:Main,ErrorPage_MyPage %>" 
                />
            <asp:Localize 
                ID="Localize1" 
                runat="server" 
                Text="<%$Resources:Main,ErrorPage_ReturnToApp %>" 
                />       
        </div>

        <p class="mt5 mb4 fs-110">
            <asp:Literal 
                ID="Literal1" 
                runat="server" 
                Text="<%$Resources:Messages,ErrorPage_ContactSupport1 %>" 
                />
            <a 
                class="yolk-hover orange transition-all tdn tdn-hover"
                href="<%= adminEmail %>"
                >
                <asp:Localize 
                    runat="server" 
                    Text="<%$Resources:Messages,ErrorPage_ContactLocalAdmin %>" 
                    />
            </a>
            <asp:Literal 
                ID="Literal2" 
                runat="server" 
                Text="<%$Resources:Messages,ErrorPage_ContactSupport3 %>" 
                />
        </p>

        <div 
            class="bg-near-white pa4 ba b-vlight-gray br3 pointer transition-all bg-vlight-gray-hover"
            data-copytoclipboard='true' 
            id="divErrorDetails"
            runat="server" 
            title="<%$Resources:Buttons,CopyToClipboard %>"
            >
        </div>
    </div>
</asp:Content>
