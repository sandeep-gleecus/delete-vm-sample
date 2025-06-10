<%@ Page Language="c#" CodeBehind="NotificationTemplates.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.NotificationTemplates" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Literal runat="server" Text="<% $Resources:Main,Admin_Notification_ViewNotificationTemplates %>" />
                    <small>
                        <tstsc:HyperLinkEx 
                            ID="lnkAdminHome" 
                            runat="server" 
                            Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                            >
                            <tstsc:LabelEx ID="lblProjectTemplateName" runat="server" />
				        </tstsc:HyperLinkEx>
                    </small>
                </h2>
				<p><asp:Literal runat="server" Text="<% $Resources:Messages,Admin_Notification_EditArtifactTemplates %>" /></p>
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True" DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
                <div class="Spacer"></div>
                <div style="max-width: 500px">
					<tstsc:GridViewEx ID="grdArtifactTemplates" CssClass="DataGrid" runat="server" ShowSubHeader="False" AutoGenerateColumns="false" Width="100%">
						<HeaderStyle CssClass="Header" />
						<Columns>
							<tstsc:BoundFieldEx HeaderText="<% $Resources:Main,Admin_Notification_ArtifactType %>" DataField="Name" HtmlEncode="false" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
							<tstsc:BoundFieldEx HeaderText="<% $Resources:Main,Admin_Notification_ArtifactPrefix %>" DataField="Prefix" HtmlEncode="false" ItemStyle-Width="50px"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" />
							<tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Global_Operations %>" ItemStyle-Wrap="false"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
								<ItemTemplate>
                                    <div class="btn-group">
									    <tstsc:HyperLinkEx ID="actEdit" SkinID="ButtonDefault" runat="server" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationEditTemplate") + "?" + GlobalFunctions.PARAMETER_ARTIFACT_TYPE_ID + "=" + ((ArtifactType) Container.DataItem).ArtifactTypeId %>'>
                                            <span class="far fa-edit fa-fw" runat="server"></span>
                                            <asp:Localize runat="server" Text="<% $Resources:Main,Global_Edit %>" />
									    </tstsc:HyperLinkEx>
									    <tstsc:LinkButtonEx ID="actTest" runat="server" CommandName="SendTestEmail" CommandArgument='<%# ((ArtifactType) Container.DataItem).ArtifactTypeId %>'>
                                            <span class="far fa-envelope fa-fw" runat="server"></span>
                                            <asp:Localize runat="server" Text="<% $Resources:Main,Global_Test %>" />
									    </tstsc:LinkButtonEx>
                                    </div>
								</ItemTemplate>
							</tstsc:TemplateFieldEx>
						</Columns>
					</tstsc:GridViewEx>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
