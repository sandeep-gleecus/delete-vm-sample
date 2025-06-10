<%@ Page
    AutoEventWireup="true"
    CodeBehind="PlanningBoard.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.PlanningBoard"
    Language="C#"
    MasterPageFile="~/MasterPages/Main.Master"
    Title="" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="PlanningBoardStylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <table id="tblMainBody" class="MainContent w-100 ">
        <tr>
            <td class="px0 v-mid">
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />





				<div class="bg-white w-100 flex justify-between pt3 pb4">

					<div class="flex items-center pl4" id="board-controls-view-groupby">

						<div class="flex items-center pr4">
							<tstsc:LabelEx
								AppendColon="true"
								AssociatedControlID="ddlSelectRelease"
								CssClass="mr3 ma0"
								ID="ddlSelectReleaseLabel"
								runat="server"
								Text="<%$Resources:Main,SiteMap_Planning %>" />
							<tstsc:UnityDropDownHierarchy
								ActiveItemField="IsActive"
								AutoPostBack="false"
								ClientScriptMethod="updateRelease"
								ClientScriptServerControlId="ajxPlanningBoard"
								DataTextField="FullName"
								DataValueField="ReleaseId"
								ID="ddlSelectRelease"
								NoValueItem="true"
								NoValueItemText="<%$Resources:Main,PlanningBoard_ProductBacklog %>"
								runat="server"
								DisabledCssClass="u-dropdown disabled"
								SkinID="UnityDropDownList_ListOnRight" />

						</div>

						<div class="flex items-center">
							<tstsc:HyperLinkEx
								CssClass="btn btn-launch br0 mr5"
								ID="imgRefresh"
								runat="server"
								ClientScriptServerControlId="ajxPlanningBoard"
								ClientScriptMethod="load_data()">
                            <span class="fas fa-sync"></span>
							</tstsc:HyperLinkEx>
						</div>

						<div class="flex items-center">
							<tstsc:LabelEx
								AppendColon="true"
								AssociatedControlID="ddlGroupBy"
								CssClass="mr3 ma0"
								ID="ddlGroupByLabel"
								runat="server"
								Text="<%$Resources:Main,PlanningBoard_GroupBy %>" />
							<tstsc:UnityDropDownListEx
								ActiveItemField="IsActive"
								AutoPostBack="false"
								ClientScriptMethod="updateGroupBy"
								ClientScriptServerControlId="ajxPlanningBoard"
								CssClass="u-dropdown"
								DataTextField="Caption"
								DataValueField="Id"
								DisabledCssClass="u-dropdown disabled"
								ID="ddlGroupBy"
								NoValueItem="false"
								runat="server" />
						</div>
						
					</div>


					<div class="flex" id="board-controls-options">
						<div class="u-checkbox-toggle nx3">
							<tstsc:CheckBoxEx ID="chkIncludeDetails" runat="server"
								Text="<%$Resources:Main,PlanningBoard_IncludeDetails%>" ClientScriptMethod="chkIncludeDetails_click()" />
							<tstsc:CheckBoxEx ID="chkIncludeIncidents" runat="server"
								Text="<%$Resources:Main,SiteMap_Incidents%>" Authorized_ArtifactType="Incident" Authorized_Permission="View"
								ClientScriptMethod="chkIncludeIncidents_click()" />
							<tstsc:CheckBoxEx ID="chkIncludeTasks" runat="server"
								Text="<%$Resources:Main,SiteMap_Tasks%>" Authorized_ArtifactType="Task" Authorized_Permission="View"
								ClientScriptMethod="chkIncludeTasks_click()" />
							<tstsc:CheckBoxEx ID="chkIncludeTestCases" runat="server" Authorized_ArtifactType="TestCase" Authorized_Permission="View"
								Text="<%$Resources:Main,SiteMap_TestCases%>"
								ClientScriptMethod="chkIncludeTestCases_click()" />
						</div>

						<div class="btn-group mr3 dn" role="group">
							<%-- hiding the print button as the print experience is not yet as good as it needs to be [IN:2430] --%>
							<button
								runat="server"
								type="button"
								class="btn btn-default"
								onclick="window.print()"
								title="<%$ Resources:Buttons,Print %>">
								<i class="fad fa-print"></i>
							</button>
						</div>
					</div>

				</div>




                <tstsc:PlanningBoard Width="100%" runat="server" ID="ajxPlanningBoard" CssClass="PlanningBoard"
                    ErrorMessageControlId="lblMessage" Authorized_ArtifactType="Requirement"
                    GroupByControlId="ddlGroupBy" ReleaseControlId="ddlSelectRelease" SupportsRanking="true"
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.PlanningBoardService" />
                <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>
                        <asp:ServiceReference Path="~/Services/Ajax/PlanningBoardService.svc" />
                        <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />
                        <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
                    </Services>
                </tstsc:ScriptManagerProxyEx>
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
                <br />
                <br />
            </td>
        </tr>
    </table>



    <div class="modal fade" tabindex="-1" role="dialog" id="dlgAddRequirement">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">
                        <span id="spnAddRequirement" class="dn">
                            <asp:Localize runat="server" Text="<%$Resources:Main,PlanningBoard_AddRequirement %>" />
                        </span>
                        <span id="spnEditRequirement" class="db">
                            <asp:Label runat="server" Text="<%$Resources:Main,PlanningBoard_EditRequirement %>" ClientIDMode="Static" ID="dlgLblEditRequirement" />
                            <asp:Label runat="server" Text="<%$Resources:Main,PlanningBoard_ViewRequirement %>" ClientIDMode="Static" ID="dlgLblViewRequirement" CssClass="dn" />
                            <asp:Label runat="server" ID="txtRequirementToken" />
                            <a class="mx4 fs-90 transition-all" id="aNavigateRequirement" href="#" runat="server" title="<%$Resources:Buttons,EditFullScreen %>">
                                <span class="far fa-edit"></span>
                            </a>
                        </span>
                    </h4>
                </div>
                <div class="modal-body">



                    <tstsc:MessageBox ID="lblMessage2" runat="server" SkinID="MessageBox" />
                    <div class="u-wrapper width_md clearfix">
                        <div class="u-box_3">



                            <%-- NAME AND DESCRIPTION --%>
                            <ul
                                class="u-box_list"
                                runat="server">
                                <li class="ma0 pa0 mb2">
                                    <tstsc:LabelEx
                                        AppendColon="true"
                                        AssociatedControlID="txtRequirementName"
                                        ID="txtRequirementNameLabel"
                                        runat="server"
                                        Text="<%$Resources:Fields,Name %>" />
                                    <tstsc:UnityTextBoxEx
                                        CssClass="u-input is-active"
                                        DisabledCssClass="u-input disabled"
                                        ID="txtRequirementName"
                                        MaxLength="255"
                                        runat="server"
                                        TextMode="SingleLine" />
                                </li>
                            </ul>
                            <ul
                                class="u-box_list labels_absolute u-cke_is-minimal"
                                runat="server">
                                <li class="ma0 pa0 mb2">
                                    <tstsc:RichTextBoxJ
                                        Authorized_ArtifactType="Requirement"
                                        Authorized_Permission="Create"
                                        ID="txtDescription"
                                        runat="server"
                                        Screenshot_ArtifactType="Requirement" />
                                    <tstsc:LabelEx
                                        AppendColon="true"
                                        AssociatedControlID="txtDescription"
                                        ID="txtDescriptionLabel"
                                        runat="server"
                                        Text="<%$Resources:Fields,Description %>" />
                                </li>
                            </ul>
                        </div>



                        <%-- RELEASE AND STATUS FIELDS --%>
                        <div class="u-box_1">
                            <div
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_releases">
                                <ul class="u-box_list">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            AssociatedControlID="ddlStatus"
                                            ID="ddlStatusLabel"
                                            Required="true"
                                            runat="server"
                                            Text="<%$Resources:Fields,RequirementStatusId %>" />
                                        <tstsc:UnityDropDownListEx
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="RequirementStatusId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlStatus"
                                            runat="server"
                                            />

                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            AssociatedControlID="ddlType"
                                            ID="ddlTypeLabel"
                                            Required="true"
                                            runat="server"
                                            Text="<%$Resources:Fields,RequirementTypeId %>" />
                                        <tstsc:UnityDropDownListEx
                                            ActiveItemField="IsActive"
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="RequirementTypeId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlType"
                                            runat="server"
                                            Width="250" />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            runat="server"
                                            ID="ddlReleaseLabel"
                                            AssociatedControlID="ddlRelease"
                                            Text="<%$Resources:Fields,ReleaseId %>"
                                            Required="false" />
                                        <tstsc:UnityDropDownHierarchy
                                            ActiveItemField="IsActive"
                                            AutoPostBack="false"
                                            DataTextField="FullName"
                                            DataValueField="ReleaseId"
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed disabled"
                                            DisabledCssClass="u-dropdown disabled"
                                            SkinID="ReleaseDropDownListFarRight"
                                            ID="ddlRelease"
                                            NoValueItem="true"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server" />
                                    </li>
                                </ul>
                            </div>





                            <%-- USER FIELDS --%>
                            <div
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_people">
                                <ul
                                    class="u-box_list"
                                    id="customFieldsUsers"
                                    runat="server">

                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            AssociatedControlID="ddlAuthor"
                                            ID="ddlAuthorLabel"
                                            Required="true"
                                            runat="server"
                                            Text="<%$Resources:Fields,AuthorId %>" />
                                        <tstsc:UnityDropDownUserList
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName"
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                            ID="ddlAuthor"
                                            runat="server" />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            AssociatedControlID="ddlOwner"
                                            ID="ddlOwnerLabel"
                                            Required="false"
                                            runat="server"
                                            Text="<%$Resources:Fields,OwnerId %>" />
                                        <tstsc:UnityDropDownUserList
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName"
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                            ID="ddlOwner"
                                            NoValueItem="true"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server" />
                                    </li>
                                </ul>
                            </div>
                        </div>




                        <%-- DEFAULT FIELDS --%>
                        <div class="u-box_1">
                            <div
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_properties">
                                <ul
                                    class="u-box_list"
                                    id="customFieldsDefault"
                                    runat="server">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            AssociatedControlID="ddlImportance"
                                            ID="ddlImportanceLabel"
                                            Required="false"
                                            runat="server"
                                            Text="<%$Resources:Fields,ImportanceId %>" />
                                        <tstsc:UnityDropDownListEx
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="ImportanceId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlImportance"
                                            NoValueItem="True"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server" />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx
                                            AppendColon="true"
                                            AssociatedControlID="ddlComponent"
                                            ID="ddlComponentLabel"
                                            Required="false"
                                            runat="server"
                                            Text="<%$Resources:Fields,ComponentId %>" />
                                        <tstsc:UnityDropDownListEx
                                            ActiveItemField="IsActive"
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="ComponentId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlComponent"
                                            NoValueItem="True"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server" />
                                    </li>
                                </ul>
                            </div>





                            <%-- DATE TIME FIELDS --%>
                            <div
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_dates">
                                <ul
                                    class="u-box_list"
                                    id="customFieldsDates"
                                    runat="server">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtEstimatePoints" 
                                            ID="txtEstimatePointsLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,EstimateWithPoints %>" 
                                            />
							            <tstsc:UnityTextBoxEx 
                                            CssClass="u-input w6"
                                            ID="txtEstimatePoints"
                                            MaxLength="9" 
                                            runat="server" 
                                            type="text"
                                            />
                                        <asp:PlaceHolder runat="server" ID="plcEstimatedEffort">
                                            <span class="badge">
                                                <tstsc:LabelEx 
                                                    runat="server" 
                                                    ID="lblEstimatedEffort" 
                                                    />
                                            </span>
                                        </asp:PlaceHolder>
                                    </li>
                                </ul>
                            </div>
                        </div>




                        <%-- RICH TEXT CUSTOM FIELDS --%>
                        <div class="u-box_3">
                            <div
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="form-group_richtext">
                                <ul
                                    class="u-box_list labels_absolute"
                                    id="customFieldsRichText"
                                    runat="server">
                                </ul>
                            </div>
                        </div>


                        <%-- COMMENTS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="form-group_comments" >
                                <ul class="u-box_list u-box_list labels_absolute" runat="server">
                                    <li class="ma0 mb2 pa0">
								        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="Requirement"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="txtNewComment" 
                                            runat="server" 
                                            Screenshot_ArtifactType="Requirement" 
                                            />
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtNewComment" 
                                            ID="lblNewComment" 
                                            runat="server" 
                                            Text="<%$Resources:Buttons,AddComment %>" 
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>




                    <div class="btn-group">
                        <tstsc:ButtonEx
                            Authorized_ArtifactType="Requirement"
                            Authorized_Permission="Create"
                            ClientScriptMethod="save_data(null, 'Save', true);"
                            ClientScriptServerControlId="ajxFormManager"
                            ID="btnCreateRequirement"
                            runat="server"
                            SkinID="ButtonPrimary"
                            Text="<%$Resources:Main,PlanningBoard_AddRequirement %>" />
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="Requirement" 
                            Authorized_Permission="BulkEdit" 
                            ClientScriptMethod="save_data(null, 'Save', true);" 
                            ClientScriptServerControlId="ajxFormManager" 
                            ID="btnSaveRequirement" 
                            runat="server" 
                            SkinID="ButtonPrimary" 
                            Text="<%$Resources:Buttons,Save %>" 
                            />
                        <button
                            class="btn btn-default"
                            id="btnCancel"
                            data-dismiss="modal"
                            aria-label="Close">
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                        </button>
                    </div>
                </div>
            </div>
        </div>

        <%-- hidden fields --%>
        <asp:HiddenField ID="hdnParentRequirementId" runat="server" />
    </div>

    <tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,Requirement%>" 
        AutoLoad="false"
        CheckUnsaved="false" 
        ErrorMessageControlId="lblMessage2" 
        ID="ajxFormManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService"
        WorkflowEnabled="true"
        ReadOnly="true"
        >
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="txtRequirementToken" DataField="RequirementId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtRequirementName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlImportance" DataField="ImportanceId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlAuthor" DataField="AuthorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlStatus" DataField="RequirementStatusId" Direction="InOut" IsWorkflowStep="true" />
            <tstsc:AjaxFormControl ControlId="ddlType" DataField="RequirementTypeId" Direction="InOut" ChangesWorkflow="true" />
            <tstsc:AjaxFormControl ControlId="txtEstimatePoints" DataField="EstimatePoints" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblEstimatedEffort" DataField="EstimatedEffort" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="hdnParentRequirementId" DataField="_ParentRequirementId" Direction="InOut" PropertyName="intValue" />
        </ControlReferences>
    </tstsc:AjaxFormManager>

    <div class="modal fade" tabindex="-1" role="dialog" ID="dlgAddIncident">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">
                        <span ID="spnAddIncident" class="dn">
                            <asp:Localize runat="server" Text="<%$Resources:Main,IncidentBoard_AddIncident %>" />
                        </span>
                        <span ID="spnEditIncident" class="db">
                            <asp:Label runat="server" Text="<%$Resources:Main,IncidentBoard_EditIncident %>" ClientIDMode="Static" ID="dlgLblEditIncident" />
                            <asp:Label runat="server" Text="<%$Resources:Main,IncidentBoard_ViewIncident %>" ClientIDMode="Static" ID="dlgLblViewIncident" CssClass="dn" />
                            <asp:Label runat="server" ID="incident_txtIncidentToken" />
                            <a class="mx4 fs-90 transition-all" ID="aNavigateIncident" href="#" runat="server" title="<%$Resources:Buttons,EditFullScreen %>">
                                <span class="far fa-edit"></span>
                            </a>
                        </span>
                    </h4>
                </div>
                <div class="modal-body">



                    <tstsc:MessageBox ID="lblMessage3" runat="server" SkinID="MessageBox" />
                    <div class="u-wrapper width_md clearfix">
                        <div class="u-box_3">



                            <%-- NAME AND DESCRIPTION --%>
                            <ul 
                                class="u-box_list" 
                                runat="server"
                                >
                                <li class="ma0 pa0 mb2">
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="incident_txtIncidentName" 
                                        ID="incident_txtIncidentNameLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Name %>" 
                                        />
                                    <tstsc:UnityTextBoxEx 
                                        CssClass="u-input is-active"
                                        DisabledCssClass="u-input disabled" 
                                        ID="incident_txtIncidentName" 
                                        MaxLength="255" 
                                        runat="server" 
                                        TextMode="SingleLine" 
                                        />
                                </li>
                            </ul>
                            <ul 
                                class="u-box_list labels_absolute u-cke_is-minimal" 
                                runat="server"
                                >
                                <li class="ma0 pa0 mb2">
                                    <tstsc:RichTextBoxJ 
                                        Authorized_ArtifactType="Incident" 
                                        Authorized_Permission="Create"
                                        ID="incident_txtDescription" 
                                        runat="server"
                                        Screenshot_ArtifactType="Incident" 
                                        />
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="incident_txtDescription" 
                                        ID="incident_txtDescriptionLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Description %>" 
                                        />
                                </li>
                            </ul>
			            </div>



                        <%-- RELEASE AND STATUS FIELDS --%>
                        <div class="u-box_1">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                ID="incident_form-group_releases" >
                                <ul class="u-box_list" >
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="incident_lblIncidentStatusValue" 
                                            ID="incident_ddlStatusLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,IncidentStatusId %>" 
                                            />
        					            <tstsc:LabelEx 
                                            ID="incident_lblIncidentStatusValue" 
                                            runat="server" 
                                            MetaData="" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="incident_ddlType" 
                                            ID="incident_ddlTypeLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,IncidentTypeId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            ActiveItemField="IsActive" 
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="IncidentTypeId" 
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="incident_ddlType" 
                                            runat="server" 
                                            Width="250" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            runat="server" 
                                            ID="incident_ddlDetectedReleaseLabel" 
                                            AssociatedControlID="incident_ddlDetectedRelease" 
                                            Text="<%$Resources:Fields,DetectedReleaseId %>" 
                                            Required="false" 
                                            />
					                    <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="" 
                                            AutoPostBack="false"
						                    DataTextField="FullName" 
                                            DataValueField="ReleaseId" 
                                            ID="incident_ddlDetectedRelease" 
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed disabled"
                                            DisabledCssClass="u-dropdown disabled"
                                            SkinID="ReleaseDropDownListFarRight"
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            runat="server" 
                                            ID="incident_ddlResolvedReleaseLabel" 
                                            AssociatedControlID="incident_ddlResolvedRelease" 
                                            Text="<%$Resources:Fields,ResolvedReleaseId %>" 
                                            Required="false" 
                                            />
					                    <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="IsActive" 
                                            AutoPostBack="false"
						                    DataTextField="FullName" 
                                            DataValueField="ReleaseId" 
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed disabled"
                                            DisabledCssClass="u-dropdown disabled"
                                            SkinID="ReleaseDropDownListFarRight"
                                            ID="incident_ddlResolvedRelease" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            runat="server" 
                                            ID="LabelEx2" 
                                            AssociatedControlID="incident_ddlVerifiedRelease" 
                                            Text="<%$Resources:Fields,VerifiedReleaseId %>" 
                                            Required="false" 
                                            />
					                    <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="IsActive" 
                                            AutoPostBack="false"
						                    DataTextField="FullName" 
                                            DataValueField="ReleaseId" 
                                            ID="incident_ddlVerifiedRelease" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed disabled"
                                            DisabledCssClass="u-dropdown disabled"
                                            SkinID="ReleaseDropDownListFarRight"
                                            />
                                    </li>
                                </ul>
                            </div>





                            <%-- USER FIELDS --%>
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                ID="incident_form-group_people" >
                                <ul 
                                    class="u-box_list" 
                                    ID="incident_customFieldsUsers" 
                                    runat="server"
                                    >

                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="incident_ddlOpener" 
                                            ID="incident_ddlOpenerLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,OpenerId %>" 
                                            />
					                    <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"  
                                            ID="incident_ddlOpener" 
                                            runat="server"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="incident_ddlOwner" 
                                            ID="incident_ddlOwnerLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,OwnerId %>" 
                                            />
					                    <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId" 
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled" 
                                            ID="incident_ddlOwner" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>




                        <%-- DEFAULT FIELDS --%>
                        <div class="u-box_1">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                ID="incident_form-group_properties" >
                                <ul 
                                    class="u-box_list" 
                                    ID="incident_customFieldsDefault" 
                                    runat="server"
                                    >
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="incident_ddlPriority" 
                                            ID="incident_ddlPriorityLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,PriorityId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown" 
                                            DataTextField="Name" 
                                            DataValueField="PriorityId" 
                                            DisabledCssClass="u-dropdown disabled" 
                                            ID="incident_ddlPriority" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="incident_ddlSeverity" 
                                            ID="incident_ddlSeverityLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,SeverityId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown" 
                                            DataTextField="Name" 
                                            DataValueField="SeverityId" 
                                            DisabledCssClass="u-dropdown disabled" 
                                            ID="incident_ddlSeverity" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="incident_ddlComponents" 
                                            ID="ddlComponentsLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ComponentIds %>" 
                                            />
					                    <tstsc:UnityDropDownMultiList 
                                            ActiveItemField="IsActive" 
                                            CssClass="u-dropdown"
                                            SelectionMode="Multiple"
                                            DataTextField="Name" 
                                            DataValueField="ComponentId" 
                                            DisabledCssClass="u-dropdown disabled" 
                                            ID="incident_ddlComponents" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                </ul>
                            </div>
  
        
        
        
        
                            <%-- DATE TIME FIELDS --%> 
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                ID="incident_form-group_dates" >
                                <ul 
                                    class="u-box_list" 
                                    ID="incident_customFieldsDates" 
                                    runat="server"
                                    >
                                    <li class="ma0 mb2 pa0">
    									<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="datStartDate" 
                                            ID="lblStartDate" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,StartDate %>" 
                                            />
                                        <tstsc:UnityDateControl 
                                            CssClass="u-datepicker"
                                            DisabledCssClass="u-datepicker disabled"
                                            ID="datStartDate" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
										<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="datClosedDate" 
                                            ID="lblClosedDate" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ClosedDate %>" 
                                            />
										<tstsc:UnityDateTimePicker 
                                            ID="datClosedDate" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
										<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lblProjectedEffort" 
                                            ID="lblProjectedEffortLabel" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ProjectedEffortWithHours %>" 
                                            />
										<tstsc:LabelEx 
                                            ID="lblProjectedEffort" 
                                            runat="server" />
                                    </li>
                                    <li class="ma0 mb2 pa0">
										<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtEstimatedEffort" 
                                            ID="LabelEx1" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,EstimatedEffortWithHours%>" 
                                            />
										<tstsc:UnityTextBoxEx 
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtEstimatedEffort" 
                                            MaxLength="9"
                                            runat="server" 
                                            type="text"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
    									<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtActualEffort" 
                                            ID="lblActualEffort" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ActualEffortWithHours %>" 
                                            />
										<tstsc:UnityTextBoxEx 
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtActualEffort" 
                                            MaxLength="9"
                                            runat="server" 
                                            type="text"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
	    								<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtRemainingEffort" 
                                            ID="lblRemainingEffort" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,RemainingEffortWithHours %>" 
                                            />
										<tstsc:UnityTextBoxEx 
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtRemainingEffort"
                                            MaxLength="9" 
                                            runat="server" 
                                            type="text"
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>




                        <%-- RICH TEXT CUSTOM FIELDS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                ID="incident_form-group_richtext" >
                                <ul 
                                    class="u-box_list labels_absolute" 
                                    ID="incident_customFieldsRichText" 
                                    runat="server"
                                    >
                                </ul>
                            </div>
                        </div>



                        <%-- COMMENTS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="incident_form-group_comments" >
                                <ul class="u-box_list u-box_list labels_absolute" runat="server">
                                    <li class="ma0 mb2 pa0">
								        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="Requirement"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="incident_txtNewComment" 
                                            runat="server" 
                                            Screenshot_ArtifactType="Requirement" 
                                            />
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="incident_txtNewComment" 
                                            ID="incident_lblNewComment" 
                                            runat="server" 
                                            Text="<%$Resources:Buttons,AddComment %>" 
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>




                    <div class="btn-group">
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="Incident" 
                            Authorized_Permission="Create" 
                            ClientScriptMethod="save_data(null, 'Save', true);" 
                            ClientScriptServerControlID="incident_ajxFormManager" 
                            ID="btnCreateIncident" 
                            runat="server" 
                            SkinID="ButtonPrimary" 
                            Text="<%$Resources:Main,IncidentBoard_AddIncident %>" 
                            />
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="Incident" 
                            Authorized_Permission="BulkEdit" 
                            ClientScriptMethod="save_data(null, 'Save', true);" 
                            ClientScriptServerControlID="incident_ajxFormManager" 
                            ID="btnSaveIncident" 
                            runat="server" 
                            SkinID="ButtonPrimary" 
                            Text="<%$Resources:Buttons,Save %>" 
                            />
                        <button 
                            class="btn btn-default" 
                            ID="incident_btnCancel" 
                            data-dismiss="modal" 
                            aria-label="Close"
                            >
                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </div>




    <tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,Incident%>" 
        AutoLoad="false"
        CheckUnsaved="false" 
        ErrorMessageControlID="lblMessage3" 
        ID="incident_ajxFormManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService"
        WorkflowEnabled="true" 
        >
		<ControlReferences>
        	<tstsc:AjaxFormControl ControlID="incident_txtIncidentToken" DataField="IncidentId" Direction="In" />
        	<tstsc:AjaxFormControl ControlID="incident_txtIncidentName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlID="incident_txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlID="incident_ddlPriority" DataField="PriorityId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlID="incident_ddlSeverity" DataField="SeverityId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlID="incident_ddlOpener" DataField="OpenerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlID="incident_ddlDetectedRelease" DataField="DetectedReleaseId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlID="incident_ddlResolvedRelease" DataField="ResolvedReleaseId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="incident_ddlVerifiedRelease" DataField="VerifiedReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlID="incident_ddlOwner" DataField="OwnerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlID="incident_lblIncidentStatusValue" DataField="IncidentStatusId" Direction="In" IsWorkflowStep="true" />

            <tstsc:AjaxFormControl ControlId="datStartDate" DataField="StartDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="datClosedDate" DataField="ClosedDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtEstimatedEffort" DataField="EstimatedEffort" Direction="InOut" PropertyName="intValue" />
			<tstsc:AjaxFormControl ControlId="txtRemainingEffort" DataField="RemainingEffort" Direction="InOut" PropertyName="intValue" />
			<tstsc:AjaxFormControl ControlId="txtActualEffort" DataField="ActualEffort" Direction="InOut" PropertyName="intValue" />

            <tstsc:AjaxFormControl ControlId="incident_ddlComponents" DataField="ComponentIds" Direction="InOut" />
			<tstsc:AjaxFormControl ControlID="incident_ddlType" DataField="IncidentTypeId" Direction="InOut" ChangesWorkflow="true" />
            <tstsc:AjaxFormControl ControlId="incident_txtNewComment" DataField="Resolution" Direction="InOut" />
		</ControlReferences>
	</tstsc:AjaxFormManager>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
    <script type="text/javascript">
		//Enums
		$(document).ready(function () {
			$('#global-nav-keyboard-shortcuts #shortcuts-planning-board').removeClass('dn');
		});

        var _pageInfo = {
			CONTAINER_ID_UNASSIGNED: -1,
			CONTAINER_ID_CURRENT_RELEASE: -2,
			CONTAINER_ID_CURRENT_ITERATION: -3,
            planByPoints: <%=PlanByPoints%>,
            bulkEditStatus: <%=BulkEditStatus%>,
            canBulkEditRequirements: globalFunctions.isAuthorized(globalFunctions.permissionEnum.BulkEdit, globalFunctions.artifactTypeEnum.requirement) === globalFunctions.authorizationStateEnum.authorized,
			canBulkEditIncidents: globalFunctions.isAuthorized(globalFunctions.permissionEnum.BulkEdit, globalFunctions.artifactTypeEnum.incident) === globalFunctions.authorizationStateEnum.authorized
        };

		function chkIncludeDetails_click() {
			var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
			var chkIncludeDetails = $get('<%=chkIncludeDetails.ClientID %>');
			ajxPlanningBoard.updateOptions('IncludeDetails', chkIncludeDetails.checked)
		}
		function chkIncludeTasks_click() {
			var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
			var chkIncludeTasks = $get('<%=chkIncludeTasks.ClientID %>');
			ajxPlanningBoard.updateOptions('IncludeTasks', chkIncludeTasks.checked)
		}
		function chkIncludeIncidents_click() {
			var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
			var chkIncludeIncidents = $get('<%=chkIncludeIncidents.ClientID %>');
			ajxPlanningBoard.updateOptions('IncludeIncidents', chkIncludeIncidents.checked)
		}
		function chkIncludeTestCases_click() {
			var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
			var chkIncludeTestCases = $get('<%=chkIncludeTestCases.ClientID %>');
			ajxPlanningBoard.updateOptions('IncludeTestCases', chkIncludeTestCases.checked)
		}
		function ajxPlanningBoard_changeGroupBy(groupById) {
			//For certain group-by options some of the checkboxes should be disabled
			var includeTasksEnabled = true;
			var includeTestCasesEnabled = true;
			var includeIncidentsEnabled = true;
			if (groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority %>) {
				includeIncidentsEnabled = false;
			}
			if (groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus %>) {
				includeIncidentsEnabled = false;
			}

			//Update the states
			var chkIncludeTasks = $get('<%=chkIncludeTasks.ClientID %>');
			chkIncludeTasks.disabled = !includeTasksEnabled;
			var chkIncludeTestCases = $get('<%=chkIncludeTestCases.ClientID %>');
			chkIncludeTestCases.disabled = !includeTestCasesEnabled;
			var chkIncludeIncidents = $get('<%=chkIncludeIncidents.ClientID %>');
			chkIncludeIncidents.disabled = !includeIncidentsEnabled;
		}
		var _planningBoard_groupById;
		var _planningBoard_containerId;
		function ajxPlanningBoard_addItem(groupById, containerId) {
			_planningBoard_groupById = groupById;
			_planningBoard_containerId = containerId;

            //load the form manager, set to insert
            var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
            ajxFormManager.set_primaryKey(null, true);
            ajxFormManager.load_data(true);
            //Reset form manager to readonly false - in case it was set to true while editing (for adding it must be false so users with create but not bulkedit permissions can add)
			ajxFormManager.set_readOnly(false);

			//Display the add requirement dialog box
			$get('spnAddRequirement').className = 'di';
			$get('spnEditRequirement').className = 'dn';
			$get('<%=btnCreateRequirement.ClientID%>').style = 'display:inline';
            $get('<%=btnSaveRequirement.ClientID%>').style = 'display:none';

			$('#dlgAddRequirement').modal('show');
        }
        function ajxPlanningBoard_editItem(artifactTypeId, artifactId) {
            //reset the planning board metadata so that the edit form is not polluted by previous attempts to add new items to the board
			_planningBoard_groupById = null;
            _planningBoard_containerId = null;

			//Make sure this is a requirement or incident we're editing
			if (artifactTypeId == globalFunctions.artifactTypeEnum.requirement) {

				//load the form manager
				var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
                ajxFormManager.set_primaryKey(artifactId, true);
				//make the form read only if the user does not have bulk edit permission
				ajxFormManager.set_readOnly(!_pageInfo.canBulkEditRequirements);
                if (!_pageInfo.canBulkEditRequirements) {
					document.getElementById("dlgLblEditRequirement").classList.add("dn");
					document.getElementById("dlgLblViewRequirement").classList.remove("dn");
                } else {
					document.getElementById("dlgLblEditRequirement").classList.remove("dn");
					document.getElementById("dlgLblViewRequirement").classList.add("dn");
				}
				ajxFormManager.load_data(true);

				//Display the add requirement dialog box
				$get('spnAddRequirement').className = 'dn';
				$get('spnEditRequirement').className = 'di';
				$get('<%=btnCreateRequirement.ClientID%>').style = 'display:none';
				$get('<%=btnSaveRequirement.ClientID%>').style = 'display:inline';
				$get('<%=aNavigateRequirement.ClientID%>').href = urlTemplate_requirementDetails.replace(globalFunctions.artifactIdToken, artifactId);
				$('#dlgAddRequirement').modal('show');
            }
			else if (artifactTypeId == globalFunctions.artifactTypeEnum.incident) {

				//load the form manager
				var ajxFormManager = $find('<%=incident_ajxFormManager.ClientID%>');
                ajxFormManager.set_primaryKey(artifactId, true);
				//make the form read only if the user does not have bulk edit permission
				ajxFormManager.set_readOnly(!_pageInfo.canBulkEditIncidents);
                if (!_pageInfo.canBulkEditIncidents) {
					document.getElementById("dlgLblEditIncident").classList.add("dn");
					document.getElementById("dlgLblViewIncident").classList.remove("dn");
                } else {

					document.getElementById("dlgLblEditIncident").classList.remove("dn");
					document.getElementById("dlgLblViewIncident").classList.add("dn");
				}
				ajxFormManager.load_data(true);

				//Display the add incident dialog box
				$get('spnAddIncident').className = 'dn';
				$get('spnEditIncident').className = 'di';
				$get('<%=btnCreateIncident.ClientID%>').style = 'display:none';
				$get('<%=btnSaveIncident.ClientID%>').style = 'display:inline';
				$get('<%=aNavigateIncident.ClientID%>').href = urlTemplate_incidentDetails.replace(globalFunctions.artifactIdToken, artifactId);
				$('#dlgAddIncident').modal('show');
			}
		}
        function ajxFormManager_loaded()
        {
            var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');

			//set bool variables
			var isGroupByRelease = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByRelease%> || _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByIteration %>,
				isGroupByPriority = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority%>,
                isGroupByPerson = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPerson%>,
                isGroupByStatus = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus%>,
				isGroupByPackage = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPackage%>,
				isGroupByComponent = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByComponent%>,
				isAddingItem = _planningBoard_containerId != null,
				isContainerAssigned = _planningBoard_containerId && (_planningBoard_containerId > 0 || _planningBoard_containerId == _pageInfo.CONTAINER_ID_CURRENT_RELEASE || _planningBoard_containerId == _pageInfo.CONTAINER_ID_CURRENT_ITERATION),
				isContainerUnassigned = _planningBoard_containerId == _pageInfo.CONTAINER_ID_UNASSIGNED;

            //make sure the status dropdown is always disabled (overriding the workflow)
            var ddlStatus = $find('<%=ddlStatus.ClientID %>');
            ddlStatus.set_enabled(false);

			//Get the release field
            var ddlRelease = $find('<%=ddlRelease.ClientID %>');

			//Set the release/iteration for new items (may be overwritten if grouping by release - in code below) 
			if (isAddingItem) {
				var ddlSelectRelease = $find('<%=ddlSelectRelease.ClientID %>');
				if (ddlSelectRelease.get_selectedItem()) {
					if (isContainerUnassigned && isGroupByRelease) {
						//leave the release unset
					} else {
						ddlRelease.set_selectedItem(ddlSelectRelease.get_selectedItem().get_value());
					}
				}
			}

			//Set the default values based on the container
			if (isContainerAssigned) {
                if (isGroupByPackage) {
					document.getElementById('<%=hdnParentRequirementId.ClientID %>').value = _planningBoard_containerId;
                }
				if (isGroupByComponent) {
					var ddlComponent = $find('<%=ddlComponent.ClientID %>');
					ddlComponent.set_selectedItem(_planningBoard_containerId);
                }
                if (isGroupByPriority) {
					var ddlImportance = $find('<%=ddlImportance.ClientID %>');
					ddlImportance.set_selectedItem(_planningBoard_containerId);
                }
                if (isGroupByRelease) {
                    ddlRelease.set_selectedItem(_planningBoard_containerId);
                }
                if (isGroupByPerson) {
					var ddlOwner = $find('<%=ddlOwner.ClientID %>');
					ddlOwner.set_selectedItem(_planningBoard_containerId);
                }
                //Only set the value of the status if the template allows bulk editing of statuses
				if (isGroupByStatus && _pageInfo.bulkEditStatus) {
                    ddlStatus.set_selectedItem(_planningBoard_containerId);
					//We now need to update the workflow to match this status
					var ddlType = $find('<%=ddlType.ClientID %>');
					ajxFormManager.load_workflow_states(ddlType.get_selectedItem()._value, _planningBoard_containerId);
				}
            }
			// after the form is fully loaded we need to mark the form as not having saved changes - this stops getting a confirm popup if we cancel and then try to add an item again
			ajxFormManager.update_saveButtons(false);
        }
        function incident_ajxFormManager_loaded() {
            //The incident form manager is not used for adding, so no need to set default values
        }
		function ajxFormManager_dataSaved() {
			//Close the dialog
			$('#dlgAddRequirement').modal('hide');

			//Reload the planning board
			var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
			ajxPlanningBoard.load_data();
        }
		function incident_ajxFormManager_dataSaved() {
			//Close the dialog
			$('#dlgAddIncident').modal('hide');

			//Reload the planning board
			var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
			ajxPlanningBoard.load_data();
		}

		//Binding for keyboard shortcuts
		Mousetrap.bind('d d', function () {
			var currentState = $('#' + '<%=chkIncludeDetails.ClientID %>').prop('checked'),
				newState = !currentState;
			$('#' + '<%=chkIncludeDetails.ClientID %>').prop('checked', newState);
			chkIncludeDetails_click();
		});
		Mousetrap.bind('d i', function () {
			var currentState = $('#' + '<%=chkIncludeIncidents.ClientID %>').prop('checked'),
				newState = !currentState;
			$('#' + '<%=chkIncludeIncidents.ClientID %>').prop('checked', newState);
			chkIncludeIncidents_click();
		});
		Mousetrap.bind('d t', function () {
			var currentState = $('#' + '<%=chkIncludeTasks.ClientID %>').prop('checked'),
				newState = !currentState;
			$('#' + '<%=chkIncludeTasks.ClientID %>').prop('checked', newState);
			chkIncludeTasks_click();
		});
		Mousetrap.bind('d c', function () {
			var currentState = $('#' + '<%=chkIncludeTestCases.ClientID %>').prop('checked'),
				newState = !currentState;
			$('#' + '<%=chkIncludeTestCases.ClientID %>').prop('checked', newState);
			chkIncludeTestCases_click();
		});
		Mousetrap.bind('g c', function () {
			var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByComponent%>;
			$find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
		});
		Mousetrap.bind('g p', function () {
			var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPackage%>;
			$find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
		});
		Mousetrap.bind('g r', function () {
			var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority%>;
			$find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
		});
		Mousetrap.bind('g s', function () {
			var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus%>;
			$find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
		});

		//URL Templates
		var urlTemplate_requirementDetails = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2))%>';
		var urlTemplate_incidentDetails = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Incidents, ProjectId, -2))%>';
    </script>
</asp:Content>
