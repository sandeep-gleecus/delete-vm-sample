<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls"
	Assembly="Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>

<%@ Page Language="c#" ValidateRequest="false" CodeBehind="NotificationEvents.aspx.cs"
	AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.NotificationEvents"
	MasterPageFile="~/MasterPages/Administration.master" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="container-fluid">
		<div class="row">
			<div class="col-lg-9">
				<h2>
					<asp:Literal ID="Literal1" runat="server" Text="<% $Resources:Main,Admin_Notification_ViewEditNotification %>" />
					<small>
                        <tstsc:HyperLinkEx 
                            ID="lnkAdminHome" 
                            runat="server" 
                            Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                            >
    						<tstsc:LabelEx ID="lblProjectName" runat="server" />
    			        </tstsc:HyperLinkEx>
					</small>
				</h2>
				<p class="mt3">
					<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_Notification_Intro %>" />
				</p>
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				<div class="Spacer"></div>
			</div>
		</div>
		<div class="row">
			<div class="col-lg-9">
				<div class="toolbar btn-toolbar-mid-page">
					<div class="btn-group priority1" role="group">
						<tstsc:DropMenu runat="server"
							ID="btnEventAdd"
							GlyphIconCssClass="fas fa-plus"
							CausesValidation="False"
							Text="<%$ Resources:Buttons,Add %>" />
					</div>
				</div>
				<div class="table-responsive">
					<tstsc:GridViewEx ID="grdNotificationEvents" CssClass="DataGrid" runat="server" ShowSubHeader="False" AutoGenerateColumns="false" Width="100%">
						<HeaderStyle CssClass="Header" />
						<Columns>
							<tstsc:BoundFieldEx HeaderText="<% $Resources:Main,Admin_Notification_EventName %>"
								DataField="Name" HtmlEncode="false" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
							<tstsc:BoundFieldEx HeaderText="<% $Resources:Main,Global_Artifact %>" DataField="ArtifactType.Name" HtmlEncode="false" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" />
							<tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Admin_Notification_OnCreation %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
								<ItemTemplate>
									<asp:Label ID="Label1" runat="server" Text='<%# GlobalFunctions.DisplayYnFlag((((NotificationEvent)Container.DataItem).IsArtifactCreation)) %>' />
								</ItemTemplate>
							</tstsc:TemplateFieldEx>
							<tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Global_Active %>" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
								<ItemTemplate>
									<asp:Label runat="server" Text='<%# GlobalFunctions.DisplayYnFlag((((NotificationEvent)Container.DataItem).IsActive)) %>' />
								</ItemTemplate>
							</tstsc:TemplateFieldEx>
							<tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Global_Operations %>" ItemStyle-Wrap="false" ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1">
								<ItemTemplate>
									<div class="btn-group">
										<tstsc:HyperLinkEx ID="actEdit" runat="server" SkinID="ButtonDefault" NavigateUrl='<%# UrlRoots.RetrieveTemplateAdminUrl(ProjectTemplateId, "NotificationEventDetails") + "?" + GlobalFunctions.PARAMETER_EVENT_ID + "=" + ((NotificationEvent) Container.DataItem).NotificationEventId %>'>
                                            <span class="far fa-edit"></span>
                                            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Buttons,Edit %>" />
										</tstsc:HyperLinkEx>
										<tstsc:LinkButtonEx ID="actDelete" runat="server" CommandName="DeleteRow" Confirmation="true"
											ConfirmationMessage="<% $Resources:Messages,Admin_Notification_DeleteEvent %>"
											CommandArgument='<%# ((NotificationEvent) Container.DataItem).NotificationEventId%>'>
                                            <span class="fas fa-trash-alt"></span>
                                            <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Buttons,Delete %>" />
										</tstsc:LinkButtonEx>
									</div>
								</ItemTemplate>
							</tstsc:TemplateFieldEx>
						</Columns>
					</tstsc:GridViewEx>
				</div>
				<br />
				<div style="padding-left: 10px;">
					<p>
						<asp:Literal runat="server" Text="<% $Resources:Messages,Admin_Notification_WorkflowNotice %>" />
					</p>
					<tstsc:HyperLinkEx SkinID="ButtonDefault" runat="server" ID="lnkIncidentWorkflows">
                        <asp:Literal runat="server" Text="<% $Resources:Main,Global_GoTo %>" />
                        <span> </span>                       
                        <asp:Literal runat="server" Text="<% $Resources:Main,Admin_EditIncidentWorkflows_Title %>" />
                    </tstsc:HyperLinkEx>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
