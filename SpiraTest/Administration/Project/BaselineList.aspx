<%@ Page
	Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="BaselineList.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.Project.BaselineList" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">
	
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<tstsc:MessageBox ID="lblMessage2" runat="server" SkinID="MessageBox" />

	<h2>
		<asp:Literal runat="server" Text="<%$ Resources:Main,Admin_BaselineList %>" />
		<small>
			<tstsc:HyperLinkEx
				ID="lnkAdminHome"
				runat="server"
				Title="<%$ Resources:Main,Admin_Project_BackToHome %>">
                <asp:Label id="lblProjectName" Runat="server" />
			</tstsc:HyperLinkEx>
		</small>
	</h2>

	<p>
		<asp:Label runat="server" Text="<%$ Resources:Messages,Admin_ProjectBaselineList_Summary %>" />
	</p>


	<div class="alert alert-warning alert-narrow mt5 mb3">
		<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
		<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
		<asp:Label ID="lblCount" runat="server" Font-Bold="True" />
		<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
		<asp:Label ID="lblTotal" runat="server" Font-Bold="True" />
		<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Items %>" />.
        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
	</div>


	<div class="df justify-between w-100">
		<div class="mr4">
			<div class="btn-group priority3 ml0 mr4">
				<tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdBaselines" ClientScriptMethod="load_data()">
                    <span class="fas fa-sync"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>"/>
				</tstsc:HyperLinkEx>
				<tstsc:HyperLinkEx ID="lnkApplyFilter" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdBaselines" ClientScriptMethod="apply_filters()">
                    <span class="fas fa-filter"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Buttons,ApplyFilter %>"/>
				</tstsc:HyperLinkEx>
				<tstsc:HyperLinkEx ID="lnkClearFilter" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdBaselines" ClientScriptMethod="clear_filters()">
                    <span class="fas fa-times"></span>
                    <asp:Localize runat="server" Text="<%$Resources:Buttons,ClearFilter %>"/>
				</tstsc:HyperLinkEx>
			</div>
		</div>
	</div>

	<tstsc:SortedGrid
		runat="server"
		AllowEditing="false"
		Authorized_Permission="ProjectAdmin"
		AutoLoad="true"
		EnableViewState="false"
		ID="grdBaselines"
        FilterInfoControlId="lblFilterInfo"
        ItemImage="artifact-Baseline.svg"
		ViewStateMode="Disabled"
		WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BaselineAdminService"
		CssClass="DataGrid"
		HeaderCssClass="Header"
		SubHeaderCssClass="SubHeader"
		SelectedRowCssClass="Highlighted"
		ErrorMessageControlId="lblMessage"
		RowCssClass="Normal"
		DisplayAttachments="false"
		DisplayTooltip="true"
		DisplayCheckboxes="true"
		TotalCountControlId="lblTotal"
		VisibleCountControlId="lblCount" />


	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BaselineAdminService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>