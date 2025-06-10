<%@ page language="C#" masterpagefile="~/MasterPages/Administration.master" autoeventwireup="true" codebehind="HistoryList.aspx.cs" inherits="Inflectra.SpiraTest.Web.Administration.Project.HistoryList" title="" %>

<%@ import namespace="System.Data" %>
<%@ import namespace="Inflectra.SpiraTest.Web" %>
<%@ import namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">

	<tstsc:messagebox id="lblMessage" runat="server" skinid="MessageBox" />
	<tstsc:messagebox id="lblMessage2" runat="server" skinid="MessageBox" />
	<h2>
		<asp:Literal runat="server" Text="<%$Resources:Main,Admin_HistoryChangeset%>" />
		<small>
			<tstsc:hyperlinkex
				id="lnkAdminHome"
				runat="server"
				title="<%$Resources:Main,Admin_Project_BackToHome %>">
				<asp:Label id="lblProjectName" Runat="server" />
			</tstsc:hyperlinkex>
		</small>
	</h2>

	<p>
		<asp:Label runat="server" Text="<%$Resources:Messages,Admin_HistoryChangeset%>" />
	</p>


	<div class="df justify-between w-100 my4">
		<div class="mr4">
			<div class="btn-group priority3 ml0 mr4">
				<tstsc:hyperlinkex id="lnkRefresh" skinid="ButtonDefault" runat="server" navigateurl="javascript:void(0)" clientscriptservercontrolid="sgHistory" clientscriptmethod="load_data()">
					<span class="fas fa-sync"></span>
					<asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>"/>
				</tstsc:hyperlinkex>
				<tstsc:hyperlinkex id="lnkApplyFilter" skinid="ButtonDefault" runat="server" navigateurl="javascript:void(0)" clientscriptservercontrolid="sgHistory" clientscriptmethod="apply_filters()">
					<span class="fas fa-filter"></span>
					<asp:Localize runat="server" Text="<%$Resources:Buttons,ApplyFilter %>"/>
				</tstsc:hyperlinkex>
				<tstsc:hyperlinkex id="lnkClearFilter" skinid="ButtonDefault" runat="server" navigateurl="javascript:void(0)" clientscriptservercontrolid="sgHistory" clientscriptmethod="clear_filters()">
					<span class="fas fa-times"></span>
					<asp:Localize runat="server" Text="<%$Resources:Buttons,ClearFilter %>"/>
				</tstsc:hyperlinkex>
			</div>
			<div class="btn-group priority3 ml0">
				<tstsc:dropmenu
					id="btnRevert1"
					runat="server"
					glyphiconcssclass="fas fa-reply mr3"
					text="<%$Resources:Buttons,Revert%>"
					enableviewstate="False"
					usesubmitbehavior="False"
					viewstatemode="Disabled"
					confirmationmessage="<%$ Resources:Messages,Admin_HistoryRevertAll %>"
					confirmation="true"
					clientscriptmethod='<%# "javascript:purgeall_Execute(" + this.ProjectId + ", \"project_RevertSelected\")" %>' />
			</div>
		</div>

		<div class="btn-group priority3 ml0">
			<tstsc:dropmenu id="btnPurgeAll1" runat="server" glyphiconcssclass="fas fa-trash-alt mr3" text="<%$Resources:Buttons,PurgeAll%>" enableviewstate="False" usesubmitbehavior="True" viewstatemode="Disabled" confirmationmessage="<%$ Resources:Messages,Admin_HistoryPurgeAll %>" confirmation="true" clientscriptmethod='<%# "javascript:purgeall_Execute(" + this.ProjectId + ", \"project_PurgeHistory\")" %>' />
		</div>
	</div>


	<div class="alert alert-warning alert-narrow">
		<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
		<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
		<asp:Label ID="lblCount" runat="server" Font-Bold="True" />
		<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
		<asp:Label ID="lblTotal" runat="server" Font-Bold="True" />
		<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Items %>" />
	</div>

	<tstsc:sortedgrid
		runat="server"
		allowediting="false"
		authorized_permission="ProjectAdmin"
		autoload="true"
		enableviewstate="false"
		id="sgHistory"
		viewstatemode="Disabled"
		webserviceclass="Inflectra.SpiraTest.Web.Services.Ajax.HistoryChangeSetService"
		cssclass="DataGrid"
		headercssclass="Header"
		subheadercssclass="SubHeader"
		selectedrowcssclass="Highlighted"
		errormessagecontrolid="lblMessage"
		rowcssclass="Normal"
		displayattachments="false"
		displaytooltip="true"
		displaycheckboxes="true"
		totalcountcontrolid="lblTotal"
		visiblecountcontrolid="lblCount" />


	<tstsc:backgroundprocessmanager id="ajxBackgroundProcessManager" runat="server" errormessagecontrolid="lblMessage" webserviceclass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" autoredirect="false" refreshratesecs=".5" callsuccessonerror="true" />
	<tstsc:scriptmanagerproxyex id="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/HistoryChangeSetService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
		</Services>
	</tstsc:scriptmanagerproxyex>


	<script language="javascript" type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;
		function purgeall_Execute(projectId, processType)
		{
			var ajxBackgroundProcessManager = $find('<%=ajxBackgroundProcessManager.ClientID %>');

			//Get the selected items..
			var selItems = null;
			if (processType == 'project_RevertSelected')
			{
				var table = $find('<%= sgHistory.ClientID %>');
				if (table) selItems = table.get_selected_items();
			}

			//Actually start the background process. Display the appropriate message
			var message = resx.Admin_History_Processing;
			if (processType == 'project_RevertSelected')
			{
				message = resx.Admin_History_Processing2;
			}
			ajxBackgroundProcessManager.display(projectId, processType, resx.Admin_History_Title, message, projectId, selItems);
		}
		function purgeall_success(msg)
		{
			//Reload the data grid..
			var grid = $find('<%= sgHistory.ClientID %>');
			if (grid) grid.load_data();
		}
	</script>
</asp:Content>
