<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="TaskBoard.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.TaskBoard" %>
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

                    <div class="dif items-center pl4" id="board-controls-view-groupby">
                        <tstsc:LabelEx 
                            AppendColon="true" 
                            AssociatedControlID="ddlSelectRelease" 
                            CssClass="mr3 ma0"
                            ID="ddlSelectReleaseLabel" 
                            runat="server" 
                            Text="<%$Resources:Main,SiteMap_Planning %>" 
                            />
                        <tstsc:UnityDropDownHierarchy 
                            ActiveItemField="IsActive"
                            AutoPostBack="false" 
                            ClientScriptMethod="updateRelease" 
                            ClientScriptServerControlId="ajxPlanningBoard" 
                            DataTextField="FullName"
                            DataValueField="ReleaseId" 
                            ID="ddlSelectRelease" 
                            NoValueItem="false"
                            runat="server" 
                            SkinID="UnityDropDownList_ListOnRight"
                            />
                        <tstsc:HyperLinkEx 
                            CssClass="btn btn-launch br0 mr5" 
                            ID="imgRefresh" 
                            runat="server" 
                            ClientScriptServerControlId="ajxPlanningBoard" 
                            ClientScriptMethod="load_data()"
                            >
                            <span class="fas fa-sync"></span>
                        </tstsc:HyperLinkEx>


                        <div class="dif items-center pr4">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ddlGroupBy" 
                                CssClass="mr3 ma0"
                                ID="ddlGroupByLabel" 
                                runat="server" 
                                Text="<%$Resources:Main,PlanningBoard_GroupBy %>" 
                                />
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
                                runat="server" 
                                />
                        </div>
                    </div>


                    <div class="dif" id="board-controls-options">
                        <div class="u-checkbox-toggle mr4">
                            <tstsc:CheckBoxEx ID="chkIncludeDetails" runat="server"
                                    Text="<%$Resources:Main,PlanningBoard_IncludeDetails%>" ClientScriptMethod="chkIncludeDetails_click()" />
                        </div>



                        <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                            <div id="board-page-selector" class="df">
                                <div class="btn-group mr3 dn" role="group">
                                    <%-- hiding the print button as the print experience is not yet as good as it needs to be [IN:2430] --%>
                                    <button 
                                        runat="server" 
                                        type="button" 
                                        class="btn btn-default" 
                                        onclick="window.print()"
                                        title="<%$ Resources:Buttons,Print %>"
                                        >
                                        <i class="fad fa-print"></i>
                                    </button>
                                </div>
                                <div class="btn-group priority1 mx3" role="group">
                                    <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -4) %>' runat="server" title="<%$ Resources:Main,Global_List %>">
                                        <span class="fas fa-list"></span>
                                    </a>
                                    <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -5) %>' runat="server" title="<%$ Resources:Main,Global_Board %>">
                                        <span class="fas fa-align-left rotate90"></span>
                                    </a>
                                    <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -9) %>' runat="server" title="<%$ Resources:Main,Global_Gantt %>">
                                        <span class="fas fa-align-left"></span>
                                    </a>
                                </div>
                            </div>
                        </asp:PlaceHolder> 
                    </div> 
                </div>




                <tstsc:PlanningBoard Width="100%" runat="server" ID="ajxPlanningBoard" CssClass="PlanningBoard"
                    ErrorMessageControlId="lblMessage" Authorized_ArtifactType="Task"
                    GroupByControlId="ddlGroupBy" ReleaseControlId="ddlSelectRelease"
                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TaskBoardService" />
                <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
                    <Services>
                        <asp:ServiceReference Path="~/Services/Ajax/TaskBoardService.svc" />
                        <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />
                    </Services>
                </tstsc:ScriptManagerProxyEx>
                <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
                <br />
                <br />
            </td>
        </tr>
    </table>



    <div class="modal fade" tabindex="-1" role="dialog" id="dlgAddTask">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">
                        <span id="spnAddTask" class="dn">
                            <asp:Localize runat="server" Text="<%$Resources:Main,TaskBoard_AddTask %>" />
                        </span>
                        <span id="spnEditTask" class="db">
                            <asp:Label runat="server" Text="<%$Resources:Main,TaskBoard_EditTask %>" ClientIDMode="Static" ID="dlgLblEditArtifact" />
                            <asp:Label runat="server" Text="<%$Resources:Main,TaskBoard_ViewTask %>" ClientIDMode="Static" ID="dlgLblViewArtifact" CssClass="dn" />
                            <asp:Label runat="server" ID="txtTaskToken" />
                            <a class="mx4 fs-90 transition-all" id="aNavigateTask" href="#" runat="server" title="<%$Resources:Buttons,EditFullScreen %>">
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
                                runat="server"
                                >
                                <li class="ma0 pa0 mb2">
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="txtTaskName" 
                                        ID="txtTaskNameLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Name %>" 
                                        />
                                    <tstsc:UnityTextBoxEx 
                                        CssClass="u-input is-active"
                                        DisabledCssClass="u-input disabled" 
                                        ID="txtTaskName" 
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
                                        Authorized_ArtifactType="Task" 
                                        Authorized_Permission="Create"
                                        ID="txtDescription" 
                                        runat="server"
                                        Screenshot_ArtifactType="Task" 
                                        />
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="txtDescription" 
                                        ID="txtDescriptionLabel" 
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
                                id="form-group_releases" >
                                <ul class="u-box_list" >
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlStatus" 
                                            ID="ddlStatusLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,TaskStatusId %>" 
                                            />
        					            <tstsc:UnityDropDownListEx
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="TaskStatusId"
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
                                            Text="<%$Resources:Fields,TaskTypeId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            ActiveItemField="IsActive" 
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="TaskTypeId" 
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlType" 
                                            runat="server" 
                                            Width="250" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            runat="server" 
                                            ID="ddlReleaseLabel" 
                                            AssociatedControlID="ddlRelease" 
                                            Text="<%$Resources:Fields,ReleaseId %>" 
                                            Required="false" 
                                            />
					                    <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="IsActive" 
                                            AutoPostBack="false"
						                    DataTextField="FullName" 
                                            DataValueField="ReleaseId" 
                                            ID="ddlRelease" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                </ul>
                            </div>





                            <%-- USER FIELDS --%>
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_people" >
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsUsers" 
                                    runat="server"
                                    >

                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlCreator" 
                                            ID="ddlCreatorLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,AuthorId %>" 
                                            />
					                    <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"  
                                            ID="ddlCreator" 
                                            runat="server"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="ddlOwner" 
                                            ID="ddlOwnerLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,OwnerId %>" 
                                            />
					                    <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId" 
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled" 
                                            ID="ddlOwner" 
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
                                id="form-group_properties" >
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsDefault" 
                                    runat="server"
                                    >
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="ddlPriority" 
                                            ID="ddlPriorityLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,TaskPriorityId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown" 
                                            DataTextField="Name" 
                                            DataValueField="TaskPriorityId" 
                                            DisabledCssClass="u-dropdown disabled" 
                                            ID="ddlPriority" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                        <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="datStartDate"
                                            ID="datStartDateLabel" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,StartDate %>" 
                                            />
                                        <tstsc:UnityDateControl 
                                            CssClass="u-datepicker"
                                            DisabledCssClass="u-datepicker disabled"
                                            ID="datStartDate" 
                                            runat="Server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="datEndDate"
                                            ID="datEndDateLabel" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,EndDate %>" 
                                            />
                                        <tstsc:UnityDateControl 
                                            CssClass="u-datepicker"
                                            DisabledCssClass="u-datepicker disabled" 
                                            ID="datEndDate" 
                                            runat="Server" 
                                            />
                                    </li>
                                </ul>
                            </div>
  
        
        
        
        
                            <%-- DATE TIME FIELDS --%> 
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_dates" >
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsDates" 
                                    runat="server"
                                    >
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtEstimatedEffort"
                                            ID="lblEstimatedEffort" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,EstimatedEffortWithHours %>" 
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
                                            ID="txtActualEffortLabel" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ActualEffortWithHours %>" 
                                            />
                                        <tstsc:UnityTextBoxEx 
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtActualEffort" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtRemainingEffort"
                                            ID="txtRemainingEffortLabel" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,RemainingEffortWithHours %>" 
                                            />
                                        <tstsc:UnityTextBoxEx 
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtRemainingEffort" 
                                            runat="server"
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
                                id="form-group_richtext" >
                                <ul 
                                    class="u-box_list labels_absolute" 
                                    id="customFieldsRichText" 
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


                    <%-- HIDDEN FIELDS --%>
                    <asp:HiddenField runat="server" ID="hdnRequirementId" />


                    <div class="btn-group">
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="Task" 
                            Authorized_Permission="Create" 
                            ClientScriptMethod="save_data(null, 'Save', true);" 
                            ClientScriptServerControlId="ajxFormManager" 
                            ID="btnCreateTask" 
                            runat="server" 
                            SkinID="ButtonPrimary" 
                            Text="<%$Resources:Main,TaskBoard_AddTask %>" 
                            />
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="Task" 
                            Authorized_Permission="BulkEdit" 
                            ClientScriptMethod="save_data(null, 'Save', true);" 
                            ClientScriptServerControlId="ajxFormManager" 
                            ID="btnSaveTask" 
                            runat="server" 
                            SkinID="ButtonPrimary" 
                            Text="<%$Resources:Buttons,Save %>" 
                            />
                        <button 
                            class="btn btn-default" 
                            id="btnCancel" 
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
        ArtifactTypeName="<%$Resources:Fields,Task%>" 
        AutoLoad="false"
        CheckUnsaved="false" 
        ErrorMessageControlId="lblMessage2" 
        ID="ajxFormManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService"
        WorkflowEnabled="true" 
        >
		<ControlReferences>
            <tstsc:AjaxFormControl ControlID="txtTaskToken" DataField="TaskId" Direction="In" />
        	<tstsc:AjaxFormControl ControlId="txtTaskName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlPriority" DataField="TaskPriorityId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlCreator" DataField="CreatorId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlStatus" DataField="TaskStatusId" Direction="InOut" IsWorkflowStep="true" />
			<tstsc:AjaxFormControl ControlId="ddlType" DataField="TaskTypeId" Direction="InOut" ChangesWorkflow="true" />
            <tstsc:AjaxFormControl ControlId="txtEstimatedEffort" DataField="EstimatedEffort" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtActualEffort" DataField="ActualEffort" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtRemainingEffort" DataField="RemainingEffort" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="datStartDate" DataField="StartDate" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="datEndDate" DataField="EndDate" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="hdnRequirementId" DataField="RequirementId" Direction="InOut" PropertyName="intValue" />
		</ControlReferences>
	</tstsc:AjaxFormManager>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $('#global-nav-keyboard-shortcuts #shortcuts-planning-board').removeClass('dn');
        });

        var _pageInfo = {
			CONTAINER_ID_UNASSIGNED: -1,
			CONTAINER_ID_CURRENT_RELEASE: -2,
			CONTAINER_ID_CURRENT_ITERATION: -3,
            bulkEditStatus: <%=BulkEditStatus%>,
			canBulkEdit: globalFunctions.isAuthorized(globalFunctions.permissionEnum.BulkEdit, globalFunctions.artifactTypeEnum.task) === globalFunctions.authorizationStateEnum.authorized
        };

        function chkIncludeDetails_click()
        {
            var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
            var chkIncludeDetails = $get('<%=chkIncludeDetails.ClientID %>');
            ajxPlanningBoard.updateOptions('IncludeDetails', chkIncludeDetails.checked)
        }
        var _planningBoard_groupById;
        var _planningBoard_containerId;
        function ajxPlanningBoard_addItem(groupById, containerId)
        {
            _planningBoard_groupById = groupById;
            _planningBoard_containerId = containerId;
            
            //load the form manager, set to insert
            var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
            ajxFormManager.set_primaryKey(null, true);
            ajxFormManager.load_data(true);
			//Reset form manager to readonly false - in case it was set to true while editing (for adding it must be false so users with create but not bulkedit permissions can add)
			ajxFormManager.set_readOnly(false);

            //Display the add task dialog box
			$get('spnAddTask').className = 'di';
			$get('spnEditTask').className = 'dn';
			$get('<%=btnCreateTask.ClientID%>').style = 'display:inline';
            $get('<%=btnSaveTask.ClientID%>').style = 'display:none';

            $('#dlgAddTask').modal('show');
        }
        function ajxPlanningBoard_editItem(artifactTypeId, artifactId) {
			//reset the planning board metadata so that the edit form is not polluted by previous attempts to add new items to the board
			_planningBoard_groupById = null;
            _planningBoard_containerId = null;

			//Make sure this is a task we're editing
			if (artifactTypeId == globalFunctions.artifactTypeEnum.task) {

				//load the form manager
				var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
                ajxFormManager.set_primaryKey(artifactId, true);
				//make the form read only if the user does not have bulk edit permission
    			ajxFormManager.set_readOnly(!_pageInfo.canBulkEdit);
                if (!_pageInfo.canBulkEdit) {
                    document.getElementById("dlgLblEditArtifact").classList.add("dn");
					document.getElementById("dlgLblViewArtifact").classList.remove("dn");
				} else {
					document.getElementById("dlgLblEditArtifact").classList.remove("dn");
					document.getElementById("dlgLblViewArtifact").classList.add("dn");
				}
				ajxFormManager.load_data(true);

				//Display the add task dialog box
				$get('spnAddTask').className = 'dn';
				$get('spnEditTask').className = 'di';
				$get('<%=btnCreateTask.ClientID%>').style = 'display:none';
				$get('<%=btnSaveTask.ClientID%>').style = 'display:inline';
				$get('<%=aNavigateTask.ClientID%>').href = urlTemplate_taskDetails.replace(globalFunctions.artifactIdToken, artifactId);
			    $('#dlgAddTask').modal('show');
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
				isGroupByRequirement = _planningBoard_groupById == <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByRequirement%>,
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
				if (isGroupByPriority) {
                    var ddlPriority = $find('<%=ddlPriority.ClientID %>');
                    ddlPriority.set_selectedItem(_planningBoard_containerId);
                }
				if (isGroupByRelease) {
                    ddlRelease.set_selectedItem(_planningBoard_containerId);
                }
				if (isGroupByPerson) {
                    var ddlOwner = $find('<%=ddlOwner.ClientID %>');
                    ddlOwner.set_selectedItem(_planningBoard_containerId);
                }
				if (isGroupByRequirement) {
                    var hdnRequirementId = $get('<%=hdnRequirementId.ClientID %>');
                    hdnRequirementId.value = _planningBoard_containerId;
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
        function ajxFormManager_dataSaved()
        {
            //Close the dialog
            $('#dlgAddTask').modal('hide');


            //Reload the planning board
            var ajxPlanningBoard = $find('<%=ajxPlanningBoard.ClientID %>');
            ajxPlanningBoard.load_data();
        }

        //Binding for keyboard shortcuts
        Mousetrap.bind('d d', function() {
            var currentState = $('#' + '<%=chkIncludeDetails.ClientID %>').prop('checked'),
                newState = !currentState;
            $('#' + '<%=chkIncludeDetails.ClientID %>').prop('checked', newState);
            chkIncludeDetails_click();
        });
        Mousetrap.bind('g c', function() {
            var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByComponent%>;
            $find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
        });
        Mousetrap.bind('g p', function() {
            var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPackage%>;
            $find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
        });
        Mousetrap.bind('g r', function() {
            var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByPriority%>;
            $find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
        });
        Mousetrap.bind('g s', function() {
            var groupById = <%=(int)Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.PlanningGroupByOptions.ByStatus%>;
            $find('<%=ddlGroupBy.ClientID%>').set_selectedItem(groupById);
        });


		//URL Templates
		var urlTemplate_taskDetails = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2))%>';
    </script>
</asp:Content>
