<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="ActiveSessions.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ActiveSessions" %>
<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls"
    Assembly="Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <h2>
        <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ActiveSessions_Title %>" />
    </h2>
    <div class="Spacer"></div>
    <div class="alert alert-warning alert-narrow">
        <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
        <asp:Literal runat="server" ID="ltrLicenseUsage" runat="server" />
    </div>
    <div class="Spacer"></div>
    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ActiveSessions_Intro %>" /> <%=this.productName%>:
    <div class="Spacer"></div>
    <tstsc:GridViewEx ID="grdActiveSessions" Runat="server" AutoGenerateColumns="false" CssClass="DataGrid" ShowHeader="true" ShowFooter="false" ShowSubHeader="false" Width="100%">
	    <HeaderStyle CssClass="Header" />
	    <Columns>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,FirstName %>" 
                HeaderStyle-CssClass="priority2" 
                ItemStyle-CssClass="priority2" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblFirstName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((User)Container.DataItem).Profile.FirstName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,LastName %>" 
                HeaderStyle-CssClass="priority2" 
                ItemStyle-CssClass="priority2" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblLastName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((User)Container.DataItem).Profile.LastName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
            <tstsc:TemplateFieldEx 
                HeaderText="<%$Resources:Fields,UserName %>" 
                HeaderStyle-CssClass="priority1" 
                ItemStyle-CssClass="priority1" 
                >
                <ItemTemplate>
                    <tstsc:LabelEx 
                        ID="lblUserName" 
                        Runat="server" 
                        Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((User)Container.DataItem).UserName) %>'
                        />
                </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:BoundFieldEx DataField="SessionId" HeaderText="<%$Resources:Fields,SessionId %>" />
		    <tstsc:BoundFieldEx DataField="PlugInName" HeaderText="<%$Resources:Fields,Interface %>" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4"/>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,LastLogon %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
			    <ItemTemplate>
				    <%# (((User)Container.DataItem).LastLoginDate.HasValue) ? String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate((DateTime)(((User)Container.DataItem).LastLoginDate.Value))) : "-"%>
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" >
			    <ItemTemplate>
				    <%# GlobalFunctions.ARTIFACT_PREFIX_USER + String.Format(GlobalFunctions.FORMAT_ID, ((User)Container.DataItem).UserId)%>
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
		    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
			    <ItemTemplate>
				    <tstsc:LinkButtonEx Runat="server" CausesValidation="false" CommandName="EndSession" CommandArgument='<%# ((User) Container.DataItem).UserId%>' ID="Linkbuttonex2" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_ActiveSessions_ConfirmEnd %>" Text="<%$Resources:Buttons,EndSession %>" />
			    </ItemTemplate>
		    </tstsc:TemplateFieldEx>
	    </Columns>													
    </tstsc:GridViewEx>
    <div class="Spacer"></div>
    <div class="Spacer"></div>
    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_ActiveSessions_Notes %>" />
</asp:Content>
