<%@ Page 
    Title="" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    AutoEventWireup="true" 
    CodeBehind="BuildDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.BuildDetails" 
%>

<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<%@ Register TagPrefix="tstuc" TagName="TestRunListPanel" Src="UserControls/TestRunListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentListPanel" Src="UserControls/IncidentListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:NavigationBar 
                AutoLoad="true"
                BodyHeight="580px" 
                ErrorMessageControlId="lblMessage"
                ID="navBuildList" 
                IncludeAssigned="false"
                ItemImage="Images/artifact-Build.svg" 
                ListScreenCaption="<%$Resources:Main,BuildDetails_BackToList%>"
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BuildService" 
                EnableLiveLoading="true"
                FormManagerControlId="ajxFormManager"
                />
        </div>



        <div class="main-panel pl4 grow-1">
            <div class="main-content">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />





                <%-- HEADER AREA --%>
                <div class="df justify-between items-center pr4 py3 sticky top-nav bg-white z-10">
                    <h2 class="my0">
                        <tstsc:ImageEx 
                            ID="imgBuild" 
                            runat="server" 
                            ImageUrl="Images/artifact-Build.svg"
                            CssClass="w5 h5"
                            AlternateText="<%$Resources:Fields,Build %>" 
                            />
                        <tstsc:LabelEx ID="lblBuildName" runat="server" />
                    </h2>
                </div>


                <%-- FIELDS --%>
                <div class="py2 px4 mb2 bg-near-white br2 df items-center flex-wrap mr4 justify-between">
                    <div>
                        <tstsc:LabelEx 
                            AppendColon="true" 
                            AssociatedControlID="ajxBuildStatus" 
                            CssClass="fw-b mb1"
                            ID="ajxBuildStatusLabel" 
                            runat="server" 
                            Text="<%$Resources:Fields,BuildStatusId %>" 
                            />
                        <tstsc:StatusBox 
                            ID="ajxBuildStatus" 
                            runat="server" 
                            />
                    </div>
                    <div class="fs-90 tr">
                        <div>
							<tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="lblCreationDateLabel" 
                                CssClass="fw-b mb0"
                                ID="lblCreationDateLabel" 
                                runat="server" 
                                Text="<%$Resources:Fields,CreationDate %>" 
                                />
							<tstsc:LabelEx 
                                ID="lblCreationDate" 
                                runat="server" 
                                />
                        </div>
                        <div>
							<tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="lblLastUpdatedDate" 
                                CssClass="fw-b mb0"
                                ID="lblLastUpdatedDateLabel" 
                                runat="server" 
                                Text="<%$Resources:Fields,LastUpdateDate %>" 
                                />
							<tstsc:LabelEx 
                                ID="lblLastUpdatedDate"
                                runat="server" 
                                />
                        </div>
                    </div>
                </div>
                


                <section class="u-wrapper width_md">
                    <%-- COMMITS --%>
                    <div class="u-box_3" id="boxCommits" runat="server">
                        <div 
                            class="u-box_group mb5"
                            data-collapsible="true"
                            id="form-group_commits">
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
                                <img src="//" data-themeicon="artifact-Revision.svg" class="w4 h4 pa1" />
                                <asp:Localize 
                                    runat="server" 
                                    Text="<%$Resources:ServerControls,TabControl_Revisions %>" />
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>  
                            <div class="u-box_item">
                                <div class="TabControlHeader w-100">
                                    <div class="btn-group priority4">
                                        <tstsc:HyperLinkEx 
                                            ID="lnkRefresh" 
                                            SkinID="ButtonDefault" 
                                            runat="server"  
                                            NavigateUrl="javascript:void(0)"
                                            ClientScriptServerControlId="grdSourceCodeRevisionList" 
                                            ClientScriptMethod="load_data()" 
                                            >
                                            <i class="fas fa-sync"></i>
                                            <asp:Localize 
                                                runat="server" 
                                                Text="<%$Resources:Buttons,Refresh %>" />
                                        </tstsc:HyperLinkEx>
                                    </div>
                                    <div class="btn-group priority2">
                                        <tstsc:DropMenu 
                                            id="btnFilters" 
                                            runat="server" 
                                            GlyphIconCssClass="mr3 fas fa-filter" 
                                            Text="<%$Resources:Buttons,Filter %>"
			                                ClientScriptServerControlId="grdSourceCodeRevisionList" 
                                            ClientScriptMethod="apply_filters()"
                                            >
			                                <DropMenuItems>
				                                <tstsc:DropMenuItem 
                                                    SkinID="ButtonDefault" 
                                                    runat="server" 
                                                    Name="Apply" 
                                                    Value="<%$Resources:Buttons,ApplyFilter %>" 
                                                    GlyphIconCssClass="mr3 fas fa-filter" 
                                                    ClientScriptMethod="apply_filters()" 
                                                    />
				                                <tstsc:DropMenuItem 
                                                    SkinID="ButtonDefault" 
                                                    runat="server" 
                                                    Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" 
                                                    GlyphIconCssClass="mr3 fas fa-times" 
                                                    ClientScriptMethod="clear_filters()" 
                                                    />
			                                </DropMenuItems>
		                                </tstsc:DropMenu>
                                    </div>
                                </div>

                                <tstsc:MessageBox ID="lblRevisionMessages" runat="server" SkinID="MessageBox" />

                                <div class="bg-near-white-hover py2 px3 br2 transition-all">
	                                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                                    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	                                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                                    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	                                <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,SourceCodeRevisions_Revisions %>" />.
                                    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                                </div>

				                <tstsc:SortedGrid 
                                    ID="grdSourceCodeRevisionList" 
                                    CssClass="DataGrid DataGrid-no-bands" 
                                    HeaderCssClass="Header" 
                                    ItemImage="artifact-Revision.svg"
				                    SubHeaderCssClass="SubHeader" 
                                    SelectedRowCssClass="Highlighted" 
                                    ErrorMessageControlId="lblRevisionMessages"
				                    RowCssClass="Normal" 
                                    AutoLoad="true" 
                                    DisplayAttachments="false" 
                                    AllowEditing="false"
				                    runat="server" 
                                    VisibleCountControlId="lblVisibleCount"
                                    TotalCountControlId="lblTotalCount"
                                    FilterInfoControlId="lblFilterInfo"
                                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService" 
                                    />
                            </div>
                        </div>
                    </div>

                    <%-- ASSOCIATIONS --%>
                    <div class="u-box_3">
                        <div 
                            class="u-box_group mb5"
                            data-collapsible="true"
                            id="form-group_associations">
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
                                <i class="far fa-link fs-75 mr2"></i>
                                <asp:Localize 
                                    runat="server" 
                                    Text="<%$Resources:ServerControls,TabControl_Associations %>" />
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>  
                            <div class="u-box_item">
                                <tstuc:AssociationsPanel 
                                    ID="tstAssociationPanel" 
                                    runat="server" 
                                    ShowAddButton="false"
                                    />
                            </div>
                        </div>
                    </div>

                    <%-- INCIDENTS --%>
                    <div class="u-box_3">
                        <div 
                            class="u-box_group mb5"
                            data-collapsible="true"
                            id="form-group_incidents">
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
                                <img src="//" data-themeicon="artifact-Incident.svg" class="w4 h4 pa1" />
                                <asp:Localize 
                                    runat="server" 
                                    Text="<%$Resources:ServerControls,TabControl_Incidents %>" />
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>  
                            <div class="u-box_item">
                                <tstuc:IncidentListPanel 
                                    ID="tstIncidentListPanel" 
                                    runat="server" 
                                    />
                            </div>
                        </div>
                    </div>

                    <%-- TEST RUNS --%>
                    <div class="u-box_3">
                        <div 
                            class="u-box_group mb5"
                            data-collapsible="true"
                            id="form-group_testRuns">
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
                                <img src="//" data-themeicon="artifact-TestRun.svg" class="w4 h4 pa1" />
                                <asp:Localize 
                                    runat="server" 
                                    Text="<%$Resources:ServerControls,TabControl_TestRuns %>" />
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>  
                            <div class="u-box_item">
                                <tstuc:TestRunListPanel 
                                    ID="tstTestRunListPanel" 
                                    runat="server" 
                                    />
                            </div>
                        </div>
                    </div>




                    <%-- DESCRIPTION --%>
                    <div class="u-box_3 mb4">
                        <div 
                            class="u-box_group"
                            data-collapsible="true"
                            id="form-group_description">
                            <div 
                                class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                aria-expanded="true">
                                <i class="far fa-clipboard fs-75 mr2"></i>
                                <asp:Localize 
                                    runat="server" 
                                    Text="<%$Resources:Fields,Builds_FullLog %>" />
                                <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                            </div>  
                            <div class="u-box_item ma0 mb4 pa0 h8 ov-y-auto resize-v ov-x-hidden wb-word br3 bg-off-white pa3 mr3">
                                <tstsc:LabelEx 
                                    ID="txtDescription" 
                                    runat="server" 
                                    CssClass="ws-prewrap ff-mono fs-90"
                                    />
                            </div>
                        </div>
                    </div>
                </section>
            </div>
        </div>
    </div>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/AssociationService.svc" />
      </Services>  
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>

       <tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,Build%>"
        CheckUnsaved="false"
        ItemImage="Images/artifact-Build.svg"
        ArtifactImageControlId="imgBuild" 
        ErrorMessageControlId="lblMessage" 
        ID="ajxFormManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BuildService" 
        WorkflowEnabled="false" 
        DisplayPageName="true"
        >
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblBuildName" DataField="Name" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="In" />
            <tstsc:AjaxFormControl ControlId="ajxBuildStatus" DataField="BuildStatusId"
                Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In"
                PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdatedDate" DataField="LastUpdateDate" Direction="In"
                PropertyName="tooltip" />
        </ControlReferences>
    </tstsc:AjaxFormManager>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        SpiraContext.pageId = "Inflectra.Spira.Web.BuildDetails";
        SpiraContext.ArtifactId = <%=buildId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=buildId%>;
        SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Build%>;
        SpiraContext.EmailEnabled = false;
        SpiraContext.HasCollapsiblePanels = true;
        SpiraContext.Mode = 'update';

        //Server Control IDs
        var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
        var navigationBar_id = '<%=this.navBuildList.ClientID%>';

        // User Control IDs
        var pnlTestRuns_id = "form-group_testRuns";
        var pnlIncidents_id = "form-group_incidents";

        //Base URLs
        var urlTemplate_artifactRedirectUrl = '<%=BuildRedirectUrl %>';
        var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToBuildListUrl) %>';
        var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
        var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

        //Updates any page specific content
        var buildDetails_buildId = -1;
        function updatePageContent()
        {
            //See if the artifact id has changed and reload the other tabs
            if (buildDetails_buildId != SpiraContext.ArtifactId)
            {
                //-- Source Code Revisions --
                var grdSourceCodeRevisionList = $find('<%=this.grdSourceCodeRevisionList.ClientID%>');
                var revisionfilters = {};
                revisionfilters[globalFunctions.keyPrefix + 'BuildId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                grdSourceCodeRevisionList.set_standardFilters(revisionfilters);
                grdSourceCodeRevisionList.load_data();

                //See if we have data
                var artifactReference = {
                    artifactId: SpiraContext.ArtifactId,
                    artifactTypeId: SpiraContext.ArtifactTypeId,
                };
                Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService.SourceCodeRevision_Count(SpiraContext.ProjectId, artifactReference, revisionsHasData_success);
            }
        }

        function revisionsHasData_success(hasData)
        {
            // no action required
        }

        function grdSourceCodeRevisionList_loaded() {
            if (!SpiraContext.uiState.artifactTypes) {
                SpiraContext.uiState.artifactTypes = globalFunctions.getArtifactTypes();
            }
            //Search the grid messages for any tokens and make them artifact hyperlinks
            var grdSourceCodeRevisionList_id = '<%=this.grdSourceCodeRevisionList.ClientID%>';
            var els = $('#' + grdSourceCodeRevisionList_id + ' tr.Normal td div:contains("[")');
            var regex = /\[(?<key>[A-Z]{2})[:\-](?<id>\d*?)\]/gi;
            for (var i = 0; i <= els.length; i++) {
                if (els[i]) {
                    var text = els[i].innerHTML;
                    if (text) {
                        els[i].innerHTML = text.replace(regex, replacer);
                    }
                }
            }
        }
        function replacer(match, artifactPrefix, artifactId, offset, string) {
            var artifactTypes = SpiraContext.uiState.artifactTypes;
            if (artifactPrefix && artifactId) {
                var artifactUrlPart = null;
                for (var i = 0; i < artifactTypes.length; i++) {
                    if (artifactTypes[i].token == artifactPrefix) {
                        artifactUrlPart = artifactTypes[i].val;
                        break;
                    }
                }
                if (artifactUrlPart) {
                    return '<a href="' + globalFunctions.replaceBaseUrl('~/' + SpiraContext.ProjectId + '/' + artifactUrlPart + '/' + artifactId + '.aspx') + '">[' + artifactPrefix + ':' + artifactId + ']</a>';
                }
            }
            return '';
        }
    </script>
</asp:Content>
