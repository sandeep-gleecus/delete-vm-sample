<%@ Register 
    TagPrefix="tstsc" 
    Namespace="Inflectra.SpiraTest.Web.ServerControls"
	Assembly="Web" 
    %>

<%@ Page 
    Language="c#" 
    CodeBehind="ForceSession.aspx.cs" 
    AutoEventWireup="True" 
    Inherits="Inflectra.SpiraTest.Web.ForceSession"
	MasterPageFile="~/MasterPages/Login.Master" 
    %>


<asp:Content 
    ContentPlaceHolderID="cplHead" 
    runat="server" 
    ID="Content1"
    >
	<title>
        <asp:Literal 
        runat="server" 
        Text="<%$Resources:Main,ForceSession_Title %>" 
        />
	</title>
</asp:Content>


<asp:Content 
    ContentPlaceHolderID="cplMainContent" 
    runat="server" 
    ID="Content2"
    >
    <div class="px5 px4-sm px0-xs mx-auto mb5 w10 w-100-sm">
        <h1 
            class="fs-h3 fw-b my5 mt4-xs blue-strong"
            id="lblMessage" 
            >
		    <asp:Literal 
                ID="lblLoggedInMessage" 
                runat="server" 
                />
        </h1>
        <p class="fw4 fs-125 mt0 mb4">
	        <asp:Localize 
                runat="server" 
                Text="<%$Resources:Main,ForceSession_Body1 %>" 
                />:
        </p>
	    <asp:LinkButton 
            CssClass="btn btn-default" 
            ID="btnLogOut" 
            runat="server"  
            Text="<%$Resources:Buttons,LogOut %>" 
            />
        <p class="mt3 mb4">
	        <asp:Localize 
                ID="Localize1" 
                runat="server" 
                Text="<%$Resources:Main,ForceSession_Body2 %>" 
                />
	    </p>
	    <asp:LinkButton 
            CssClass="btn btn-primary" 
            ID="btnSignOffOthers" 
            runat="server"
                Text="<%$Resources:Buttons,SignOffOtherLocations %>" 
            />
        <p class="my3">
	        <asp:Localize 
                ID="Localize2" 
                runat="server" 
                Text="<%$Resources:Main,ForceSession_Body3 %>" 
                />
        </p>
    </div>
</asp:Content>
