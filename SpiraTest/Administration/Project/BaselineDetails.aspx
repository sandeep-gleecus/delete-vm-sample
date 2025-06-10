<%@ Page
	Language="C#"
	MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true"
	CodeBehind="BaselineDetails.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.Administration.Project.BaselineDetails" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>

<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server">

	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<tstsc:MessageBox ID="lblMessage2" runat="server" SkinID="MessageBox" />
	<h2 class="mb2">
		<asp:Literal runat="server" Text="<%$ Resources:Main,Admin_BaselineDetails %>" />:
        <tstsc:LabelEx ID="txtName" runat="server" />
	</h2>
	
    <div class="btn-group mb4" role="group">
		<tstsc:DropMenu ID="btnBackList" runat="server" Text="<%$Resources:Buttons,BackList %>" GlyphIconCssClass="fas fa-arrow-left mr3" CausesValidation="false" />
	</div>

	<p>
		<tstsc:LabelEx runat="server" Text="<%$Resources:Messages,Admin_BaselineDetails %>" />
	</p>


    <section class="u-wrapper width_md">
        <%-- DESCRIPTION --%>
        <div class="u-box_3 mt4">
            <div 
                class="u-box_group"
                data-collapsible="true"
                id="form-group_admin-product-baseline-description" >
                <div class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Fields,Description %>" />
                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                </div>
                <div class="pa2 u-box_item">
				    <tstsc:LabelEx ID="txtDescription" runat="server" />
                </div>
            </div>
        </div>


        <div class="u-box_2 mt5">
            <%-- PROPERTIES --%>
            <div 
                class="u-box_group"
                data-collapsible="true"
                id="form-group_admin-product-baseline-properties" >
                <div 
                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                    aria-expanded="true">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Fields,Properties %>" />
                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                </div>
                <ul class="u-box_list" >
                    <li class="ma0 pa0 mb2">
						<tstsc:LabelEx ID="lblRelease" runat="server" Text="<%$ Resources:Fields,ReleaseIteration %>" AssociatedControlID="lnkRelease" AppendColon="true"/>
						<tstsc:ImageEx runat="server" ImageUrl="Images/artifact-Release.svg" AlternateText="<%$Resources:Fields,Release %>" CssClass="w4 h4" />
                        <tstsc:HyperLinkEx ID="lnkRelease" runat="server" />
                    </li>
                    <li class="ma0 pa0 mb2">
						<tstsc:LabelEx ID="lblUser" runat="server" Text="<%$ Resources:Fields,CreatorId %>" AssociatedControlID="txtuser" AppendColon="true"/>
						<tstsc:LabelEx ID="txtUser" CssClass="color-inherit" runat="server" />
                    </li>
                    <li class="ma0 pa0 mb2">
						<tstsc:LabelEx ID="lblDate" runat="server" Text="<%$ Resources:Fields,CreationDate %>" AssociatedControlID="txtDate" AppendColon="true"/>
						<tstsc:LabelEx ID="txtDate" CssClass="color-inherit" runat="server" />
                    </li>
                    <li class="ma0 pa0 mb2">
						<tstsc:LabelEx ID="lvlPrevBaseline" runat="server" AssociatedControlID="lnkPrevBaseline" Text="<%$ Resources:Fields,PreviousBaseline %>" AppendColon="true"/>
						<tstsc:ImageEx ID="imgBaseline" runat="server" ImageUrl="Images/artifact-Baseline.svg" AlternateText="<%$Resources:Fields,BaselineId %>" CssClass="w4 h4" />
						<tstsc:HyperLinkEx ID="lnkPrevBaseline" runat="server" />
                    </li>
                    <li class="ma0 pa0 mb2">
					    <tstsc:LabelEx ID="lblIsActive" runat="server" AssociatedControlID="txtIsActive" Text="<%$ Resources:Fields,IsActive %>" AppendColon="true"/>
					    <tstsc:LabelEx ID="txtIsActive" CssClass="color-inherit" runat="server" />
                    </li>
                    <li class="ma0 pa0 mb2">
						<tstsc:LabelEx ID="lblChangeSet" runat="server" Text="<%$ Resources:Fields,ChangeSetId %>" AssociatedControlID="lnkChangeSet" AppendColon="true"/>
						<tstsc:HyperLinkEx ID="lnkChangeSet" runat="server" />
                    </li>
                    <li class="ma0 pa0 mb2 is-inherit">
						<tstsc:LabelEx ID="lblBaselineID" runat="server" Text="<%$ Resources:Fields,BaselineId %>" AssociatedControlID="txtBaselineID" AppendColon="true"/>
						<tstsc:LabelEx ID="txtBaselineID" CssClass="color-inherit" runat="server" />
                    </li>
                </ul>
            </div>
        </div>


        <%-- CHANGES GRID --%>
        <div class="u-box_3 mt5">
            <div 
                class="u-box_group"
                id="form-group_admin-product-baseline-artifactChanges" >
                <div class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3">
                    <asp:Localize 
                        runat="server" 
                        Text="<%$Resources:Main,Admin_BaselineDetails_ChangesInBaseline %>" />
                </div>
				<div class="mt4 px4">
					<div class="btn-group priority3 ml0 mr4">
						<tstsc:HyperLinkEx runat="server"
							ID="lnkRefresh"
							SkinID="ButtonDefault"
							NavigateUrl="javascript:void(0)"
							ClientScriptServerControlId="sgChanges"
							ClientScriptMethod="load_data()">
								<span class="fas fa-sync"></span>
								<asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>"/>
						</tstsc:HyperLinkEx>
						<tstsc:HyperLinkEx runat="server"
							ID="lnkApplyFilter"
							SkinID="ButtonDefault"
							NavigateUrl="javascript:void(0)"
							ClientScriptServerControlId="sgChanges"
							ClientScriptMethod="apply_filters()">
								<span class="fas fa-filter"></span>
								<asp:Localize runat="server" Text="<%$Resources:Buttons,ApplyFilter %>"/>
						</tstsc:HyperLinkEx>
						<tstsc:HyperLinkEx runat="server"
							ID="lnkClearFilter"
							SkinID="ButtonDefault"
							NavigateUrl="javascript:void(0)"
							ClientScriptServerControlId="sgChanges"
							ClientScriptMethod="clear_filters()">
								<span class="fas fa-times"></span>
								<asp:Localize runat="server" Text="<%$Resources:Buttons,ClearFilter %>"/>
						</tstsc:HyperLinkEx>
					</div>
	                <div class="alert alert-warning alert-narrow">
		                <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
		                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
		                <asp:Label ID="lblCount" runat="server" Font-Bold="True" />
		                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
		                <asp:Label ID="lblTotal" runat="server" Font-Bold="True" />
		                <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Items %>" />.
                        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
	                </div>
				    <tstsc:SortedGrid runat="server"
					    ID="sgChanges"
					    AllowEditing="false"
					    AutoLoad="true"
					    EnableViewState="false"
					    ViewStateMode="Disabled"
					    CssClass="DataGrid"
					    HeaderCssClass="Header"
					    SubHeaderCssClass="SubHeader"
					    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BaselineArtifactService"
					    SelectedRowCssClass="Highlighted"
					    ErrorMessageControlId="lblMessage"
					    RowCssClass="Normal"
					    DisplayAttachments="false"
					    DisplayTooltip="false"
					    DisplayCheckboxes="false"
					    TotalCountControlId="lblTotal"
					    VisibleCountControlId="lblCount"
                        FilterInfoControlId="lblFilterInfo"
					    AllowDragging="false" />
				</div>
            </div>
        </div>  
    </section>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/BaselineArtifactService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/HistoryService.svc" />
		</Services>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
	<script type="text/javascript">
	    SpiraContext.HasCollapsiblePanels = true;
    </script>
</asp:Content>
