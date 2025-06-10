<%@ Page 
    Language="c#" 
    CodeBehind="InvalidDatabase.aspx.cs" 
    AutoEventWireup="True"
	Inherits="Inflectra.SpiraTest.Web.InvalidDbRevision" 
    MasterPageFile="~/MasterPages/Login.Master"
	Title="Test" 
    %>

<asp:Content 
    ContentPlaceHolderID="cplMainContent" 
    runat="server" 
    ID="Content2"
    >

    <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm tc">
        <h1 class="fw-b fs-h3 my5 mt4-xs blue-strong">
            <asp:Literal 
            ID="ltrMessage" 
            runat="server" 
            />
        </h1>
                
        <tstsc:HyperLinkEx 
            CssClass="btn btn-primary" 
            SkinID="CustomerSupport" 
            ID="btnContact" 
            runat="server" 
            NavigateUrl="~/Administration/LicenseDetails.aspx">
            <%= Resources.Messages.InvalidDatabase_ContactSupport1 + " " + Resources.Messages.InvalidDatabase_ContactSupport2 + " " + Resources.Messages.InvalidDatabase_ContactSupport3%>
        </tstsc:HyperLinkEx>
    </div>
</asp:Content>
