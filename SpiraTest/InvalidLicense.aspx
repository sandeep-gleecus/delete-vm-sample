<%@ Page 
    Language="c#" 
    CodeBehind="InvalidLicense.aspx.cs" 
    AutoEventWireup="True"
	Inherits="Inflectra.SpiraTest.Web.InvalidLicense" 
    MasterPageFile="~/MasterPages/Login.Master" 
    %>

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
	<title>
        <asp:Localize 
            runat="server" 
            Text="<%$Resources:Main,InvalidLicense_Title %>" 
            />
	</title>


</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm tc">
        <h1 
            class="fw-b fs-h3 my5 mt4-xs blue-strong"
            id="lblMessage" 
            runat="server" 
            >
        </h1>

        <div class="btn-group">
            <tstsc:HyperLinkEx 
                SkinID="ButtonPrimary" 
                ID="btnChange" 
                runat="server" 
                NavigateUrl="~/Administration/LicenseDetails.aspx"
                >
                <%= Resources.Main.InvalidLicense_Change + " " + Resources.Main.InvalidLicense_LicenseDetails %>
            </tstsc:HyperLinkEx>

            <tstsc:HyperLinkEx 
                SkinID="ButtonDefault" 
                ID="btnReturn" 
                runat="server" 
                NavigateUrl="Login.aspx"
                >
                <%= Resources.Main.InvalidLicense_ReturnTo + " " + Resources.Main.InvalidLicense_Login + " " + Resources.Main.InvalidLicense_Page%>
            </tstsc:HyperLinkEx>
        </div>
    </div>
</asp:Content>
