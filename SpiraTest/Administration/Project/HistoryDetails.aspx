<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="HistoryDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Project.HistoryDetails" Title="" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="~/UserControls/HistoryPanel.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<tstsc:MessageBox ID="lblMessage2" runat="server" SkinID="MessageBox" />
	<h2>
		<asp:Literal runat="server" Text="<%$Resources:Main,Admin_HistoryChangeDetails%>" />
		<small>
			<tstsc:HyperLinkEx
				ID="lnkAdminHome"
				runat="server"
				title="<%$Resources:Main,Admin_Project_BackToHome %>">
				<asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
		</small>
	</h2>

	<p>
		<tstsc:LabelEx runat="server" Text="<%$ Resources:Messages,Admin_HistoryChangeDetails %>" />
		<tstsc:LabelEx runat="server" Text="<%$ Resources:Messages,Admin_HistoryChangeDetails_NoField %>" ID="lblNoChanges" Visible="false" />
	</p>


	<div class="toolbar btn-toolbar-mid-page mt4">
		<div class="btn-group priority1" role="group">
			<tstsc:DropMenu ID="btnBackList" runat="server" Text="<%$Resources:Buttons,BackList %>" GlyphIconCssClass="fas fa-arrow-left mr3" CausesValidation="false" />
			<tstsc:DropMenu ID="btnViewItem" runat="server" Text="<%$Resources:Buttons,ViewItem %>" GlyphIconCssClass="fas fa-list mr3" CausesValidation="false" />
		</div>
		<div class="btn-group priority3" role="group">
			<tstsc:DropMenu
				ClientScriptMethod='<%# "javascript:historyItem_Execute(\"project_RevertSelected\")" %>'
				Confirmation="True"
				ConfirmationMessage="<%$Resources:Messages,Admin_History_RestoreSure %>"
				GlyphIconCssClass="fas fa-reply mr3"
				ID="btnRestore"
				runat="server"
				Text="<%$Resources:Buttons,Revert %>" />
			<tstsc:DropMenu
				ClientScriptMethod='<%# "javascript:historyItem_Execute(\"project_PurgeItem\")" %>'
				Confirmation="True"
				ConfirmationMessage="<%$Resources:Messages,Admin_History_PurgeSure %>"
				GlyphIconCssClass="fas fa-trash-alt mr3"
				ID="btnPurge"
				runat="server"
				Text="<%$Resources:Buttons,Purge %>" />
		</div>
	</div>

	<section class="u-wrapper width_md">
		<div class="u-box_2 mt4">
			<%-- PROPERTIES --%>
			<div
				class="u-box_group"
				id="form-group_admin-product-history-properties">
				<div
					class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
					aria-expanded="true">
					<asp:Localize
						runat="server"
						Text="<%$Resources:Fields,Properties %>" />
				</div>
				<ul class="u-box_list">
					<li class="ma0 pa0">
						<div runat="server" class="alert alert-warning" id="divIsDeleted">
							<span class="fas fa-exclamation-circle"></span>
							<asp:Label runat="server" Text="<%$Resources:Messages,Admin_History_ArtifactIsDeleted%>" />
						</div>
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="lblName" AssociatedControlID="txtName" runat="server" Text="<%$Resources:Fields,ChangeSetID %>" AppendColon="true" />
						<tstsc:LabelEx ID="txtName" runat="server" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="lblDate" runat="server" Text="<%$Resources:Fields,ChangeDate %>" AssociatedControlID="txtDate" AppendColon="true" />
						<tstsc:LabelEx ID="txtDate" runat="server" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="lblUser" runat="server" Text="<%$Resources:Fields,ChangerId %>" AssociatedControlID="txtuser" AppendColon="true" />
						<tstsc:LabelEx ID="txtUser" runat="server" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="lblChangeType" runat="server" Text="<%$Resources:Fields,ChangeType %>" AssociatedControlID="txtChangeType" AppendColon="true" />
						<tstsc:LabelEx ID="txtChangeType" runat="server" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="lblArtifact" runat="server" AssociatedControlID="lnkArtifact" Text="<%$Resources:Fields,Artifact %>" AppendColon="true" />
						<tstsc:HyperLinkEx ID="lnkArtifact" runat="server" />
					</li>
					<li class="ma0 pa0" runat="server" id="liFieldType">
						<tstsc:LabelEx ID="lblFieldType" runat="server" AssociatedControlID="txtFieldType" Text="<%$Resources:Fields,FieldSet %>" AppendColon="true" />
						<tstsc:LabelEx ID="txtFieldType" runat="server" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx runat="server" AssociatedControlID="txtArtifactDesc2" Text="<%$Resources:Fields,ArtifactName %>" AppendColon="true" />
						<tstsc:LabelEx ID="txtArtifactDesc2" runat="server" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="statusSignedLabel" AssociatedControlID="statusSigned" runat="server" Text="<%$Resources:Fields,Signed %>" AppendColon="true" />
						<tstsc:StatusBox runat="server" ID="statusSigned" />
					</li>
					<li class="ma0 pa0">
						<tstsc:LabelEx ID="lblSignatureHashLabel" AssociatedControlID="lblSignatureHash" runat="server" Text="<%$Resources:Fields,SignatureHash %>" AppendColon="true" />
						<tstsc:LabelEx ID="lblSignatureHash" runat="server" />
					</li>
				</ul>
			</div>
		</div>


		<%-- GRID --%>
		<div class="u-box_3 mt5" runat="server" id="changesdiv">
			<div
				class="u-box_group"
				id="form-group_admin-product-baseline-artifactChanges">
				<div class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3">
					<asp:Localize
						runat="server"
						Text="<%$ Resources:ServerControls,TabControl_ChangeActions %>" />
				</div>
				<div class="mt4 px4">
					<tstsc:SortedGrid runat="server"
						ID="sgHistory"
						AllowEditing="false"
						Authorized_Permission="ProjectAdmin"
						AutoLoad="true"
						EnableViewState="false"
						ViewStateMode="Disabled"
						WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.HistoryService"
						CssClass="DataGrid DataGridNoPagination"
						HeaderCssClass="Header"
						SubHeaderCssClass="Hidden"
						SelectedRowCssClass="Highlighted"
						ErrorMessageControlId="lblMessage"
						RowCssClass="Normal"
						DisplayAttachments="false"
						DisplayTooltip="false"
						DisplayCheckboxes="false"
						TotalCountControlId="lblTotal"
						VisibleCountControlId="lblCount"
						AllowDragging="false" />
				</div>
			</div>
		</div>
	</section>

	<tstsc:BackgroundProcessManager
		ID="ajxBackgroundProcessManager"
		runat="server"
		ErrorMessageControlId="lblMessage"
		WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService"
		AutoRedirect="false"
		RefreshRateSecs=".5"
		CallSuccessOnError="true" />
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/HistoryService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>
	<script type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;
		function historyItem_Execute(command) {
			//Set message, first..
			var message = resx.Admin_History_Processing3;
			if (command == "project_RevertSelected")
				message = resx.Admin_History_Processing2;

			//Start background processing..
			var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');
			ajxBackgroundProcessManager.display(<%= this.ProjectId  %>, command, resx.Admin_History_Title, message, <%= _changeSetId  %>, [<%= _changeSetId  %>]);
		}
		function operation_success(msg) {
			//Jump back to the history list.
			var urlStub = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveProjectAdminUrl(ProjectId, "HistoryList")%>';
			window.location = globalFunctions.replaceBaseUrl(urlStub);
		}
	</script>
</asp:Content>
