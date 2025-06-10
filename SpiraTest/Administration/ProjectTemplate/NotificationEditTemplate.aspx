<%@ Page Language="c#" CodeBehind="NotificationEditTemplate.aspx.cs" AutoEventWireup="True"
	Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.NotificationEditTemplate" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize runat="server" Text="<% $Resources:Main,Admin_Notification_ViewNotificationTemplates %>" />
                    <small>
                        <asp:Literal runat="server" ID="lblArtifactTypeTitle" />
                    </small>
                </h2>
                <tstsc:HyperLinkEx runat="server" SkinID="ButtonDefault" ID="lnkBackToNotificationList">
                    <span class="fas fa-arrow-left"></span>
                    <asp:Localize runat="server" Text="<% $Resources:Main,Admin_Notification_ReturnToTemplateList %>" />
                </tstsc:HyperLinkEx>
				<p class="my4">
					<asp:Literal ID="localSubMessage" runat="server" Text="<% $Resources:Messages,Admin_Notification_EditArtifactTemplate %>" />
				</p>
			    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
			    <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
				    DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
            </div>
            <div class="col-lg-12">
                <tstsc:RichTextBoxJ ID="txtTemplate" runat="server" Height="250px" AllowAllMarkup="true" />
                <div class="my4">
                    <tstsc:HyperLinkEx runat="server" NavigateUrl="javascript:void(0)" Text="<%$Resources:Main,Admin_Notification_EditTemplate_Tokens %>" ClientScriptServerControlId="dlgEmailTokens" ClientScriptMethod="display(event)" />
                </div>
                <div class="my4 btn-group">
					<tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" runat="server" Text="<%$Resources:Buttons,Save%>" CausesValidation="True" />
					<tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel%>" CausesValidation="False" />
					<asp:HiddenField ID="artNum" runat="server" />
                </div>
            </div>
            <tstsc:DialogBoxPanel ID="dlgEmailTokens" runat="server" Title="<%$Resources:Main,Admin_Notification_EmailTokens %>" Modal="false" Persistent="true" Width="800px">
				<p><asp:Literal ID="litTokenHeader" runat="server" /></p>
				<tstsc:DataListEx ID="grdFields" runat="server" CssClass="DataGrid ma0"
					ViewStateMode="Disabled" RepeatColumns="2" RepeatDirection="Horizontal"
					Width="100%" ItemStyle-CssClass="priority1">
					<ItemTemplate>
						<a runat="server" id="aTokenClick" style="cursor: pointer;">${<asp:Label runat="server"
							ID="lblToken" Text='<%# ((ArtifactField)Container.DataItem).Name %>' />}</a>
						</td>
						<td colspan="2" class="priority1">
							<tstsc:LabelEx runat="server" ID="lblTokenDesc" Text='<%# ((ArtifactField)Container.DataItem).Description %>' />
						&nbsp;
					</ItemTemplate>
				</tstsc:DataListEx>
            </tstsc:DialogBoxPanel>
        </div>
    </div>
	<script type="text/javascript">
	    function insert_token(insertToken)
	    {
	        var editor = CKEDITOR.instances['<%= txtTemplate.ClientID %>'];
	        if (editor && editor.insertHtml)
	        {
	            editor.insertHtml(insertToken);
			}
		}
	</script>
</asp:Content>
