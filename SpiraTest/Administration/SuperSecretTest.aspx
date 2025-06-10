<%@ Page Language="c#" ValidateRequest="false" CodeBehind="SuperSecretTest.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.Administration.SuperSecretTest" MasterPageFile="~/MasterPages/Administration.master" %>

<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<asp:Panel ID="pnlProjectDataTools" runat="server">
		<h2>
			<asp:Label runat="server" Text="The Super Secret Testing Page" />
		</h2>

		<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />


		<p class="mw720 mt5">
			<asp:Label runat="server" Text="Clicking the button below will start the URL test." />
		</p>

		<div class="btn-group-vertical priority1">
			<tstsc:HyperLinkEx
				Authorized_Permission="SystemAdmin"
				CssClass="btn btn-default tl"
				ID="lnkRefreshIndexes"
				NavigateUrl='javascript:grdDataTools_Execute("system_urltimer")'
				runat="server">
					<span class="fas fa-stopwatch fa-fw mr2"></span><span class="fas fa-link fa-fw mr2"></span>
					Start URL Timer Test
			</tstsc:HyperLinkEx>
		</div>
		<p></p>
		<pre id="output">Press button above to start test and see results here.</pre>

	</asp:Panel>


	<tstsc:BackgroundProcessManager
		ID="ajxBackgroundProcessManager"
		runat="server"
		ErrorMessageControlId="lblMessage"
		WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>
	<script type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;
		function grdDataTools_Execute(procActivity) {
			var ajxBackgroundProcessManager = $find('<%= ajxBackgroundProcessManager.ClientID %>');
			ajxBackgroundProcessManager.display(null, procActivity, resx.Admin_DataTools_Title, "Hitting URLs...");
		}

		function ajxBackgroundProcessManager_success(msg, returnCode) {
			$('#output').html(msg);
		}
	</script>
</asp:Content>
