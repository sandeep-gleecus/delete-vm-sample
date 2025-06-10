<%@ Page 
    AutoEventWireup="true" 
    CodeBehind="RequirementsMap.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.RequirementsMap" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    Title="" 
%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DiagramsStylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">

    <div id="tblMainBody" class="MainContent w-100 ">
        <div class="flex flex-column min-h-insideHeadAndFoot">

            <div class="flex items-center justify-between flex-wrap-xs mvw-100">
                <h1 class="w-100-xs fs-h3-xs fw-b-xs pl4 px3-xs">
                    <asp:Localize runat="server" Text="<%$Resources:Main,SiteMap_RequirementsMap %>" />
                </h1>
                <!-- Requirements View Selector -->
                <asp:PlaceHolder ID="plcListBoardSelector" runat="server">
                    <span class="btn-group priority1 mb4-xs ml2 ml0-xs pr5 pr0-xs" role="group">
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -6) %>' runat="server" title="<%$ Resources:Main,Global_Tree %>">
                            <span class="fas fa-indent"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -4) %>' runat="server" title="<%$ Resources:Main,Global_List %>">
                            <span class="fas fa-list"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -5) %>' runat="server" title="<%$ Resources:Main,Global_Board %>">
                            <span class="fas fa-align-left rotate90"></span>
                        </a>
                        <a class="btn btn-default" aria-selected="false" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -7) %>' runat="server" title="<%$ Resources:Main,Global_Document %>">
                            <span class="fas fa-paragraph"></span>
                        </a>
                        <a class="btn btn-default active" aria-selected="true" href='<%# UrlRewriterModule.RetrieveRewriterURL(Inflectra.SpiraTest.Common.UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -8) %>' runat="server" title="<%$ Resources:Main,Global_MindMap %>">
                            <span class="fas fa-project-diagram"></span>
                        </a>
                    </span>
                </asp:PlaceHolder>
            </div>

            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />

            <!-- Requirements Mind Map Toolbar -->
            <div class="sticky top2 bg-white w-100 pt5 bb b-near-white bw2" role="toolbar">
                <div class="flex px4">
			        <div class="btn-group priority1 pr4" role="group">
			            <button 
                            class="btn btn-default"
                            id="btnRefresh" 
                            onclick="reloadDiagram()"
                            runat="server" 
                            title="<%$Resources:Buttons,Refresh %>" 
                            type="button"
                            >
                            <i class="fas fa-sync"></i>
			            </button>
				        <tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="False" DataTextField="Value" DataValueField="Key" CssClass="DropDownList" ClientScriptMethod="expandToLevel" Width="110px" />
                    </div>

                    <div class="input-group input-group-sm priority2 pr4" role="group">
                        <div class="input-group-btn v-top">
			                <button
                                class="btn btn-default"
                                id="btnDecreaseZoom" 
                                onclick="decrease_zoom()"
                                runat="server" 
                                title="<%$Resources:Buttons,Decrease %>" 
                                type="button"
                                >
                                <i class="fas fa-minus"></i>
			                </button>
                        </div>
						<tstsc:TextBoxEx 
                            ID="txtZoomLevel" 
                            runat="server" 
                            SkinID="NarrowPlusFormControl"
                            Text="100"
                            TextMode="SingleLine"
                            />
                        <span class="input-group-addon">%</span>
                        <div class="input-group-btn v-top">
                            <button
                                class="btn btn-default"
                                id="btnIncreaseZoom" 
                                onclick="increase_zoom()"
                                runat="server" 
                                title="<%$Resources:Buttons,Increase %>" 
                                type="button"
                                >
                                <i class="fas fa-plus"></i>
			                </button>
                            <button
                                class="btn btn-default"
                                id="btnResetZoom" 
                                onclick="reset_zoom()"
                                runat="server" 
                                title="<%$Resources:Buttons,Reset %>" 
                                type="button"
                                >
                                <i class="fas fa-search"></i>
			                </button>
                        </div>
                    </div>

                    <div class="u-checkbox-toggle pl2 priority3" role="group">
                        <tstsc:CheckBoxEx ID="chkIncludeAssociations" runat="server"
                                Text="<%$Resources:Main,RequirementsMap_IncludeAssociations%>" ClientScriptMethod="chkIncludeAssociations_click()" />
                    </div>   
                </div>
            </div>
                
            <div id="outer-container" class="w-100 px4 mt4">
                <div id="requirements-model" class="graphviz-container">
                    <svg id="svgRequirementsMap" class="graphviz-hierarchical">
                        <g/>
                    </svg>
                </div>
            </div>
        </div>

        <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
            <Scripts>
                <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.graphlib-dot.js" Assembly="Web" />
                <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.dagre-d3.js" Assembly="Web" />
            </Scripts>
            <Services>
                <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />
            </Services>
        </tstsc:ScriptManagerProxyEx>
        <asp:Button ID="btnEnterCatch" runat="server" UseSubmitBehavior="true" />
    </div>

    <div class="modal fade" tabindex="-1" role="dialog" id="dlgAddRequirement">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">
                        <span id="spnEditRequirement" class="db">
                            <asp:Label runat="server" Text="<%$Resources:Main,PlanningBoard_EditRequirement %>" ClientIDMode="Static" ID="dlgLblEditArtifact" />
                            <asp:Label runat="server" Text="<%$Resources:Main,PlanningBoard_ViewRequirement %>" ClientIDMode="Static" ID="dlgLblViewArtifact" CssClass="dn" />
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
                                runat="server"
                                >
                                <li class="ma0 pa0 mb2">
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="txtRequirementName" 
                                        ID="txtRequirementNameLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Name %>" 
                                        />
                                    <tstsc:UnityTextBoxEx 
                                        CssClass="u-input is-active"
                                        DisabledCssClass="u-input disabled" 
                                        ID="txtRequirementName" 
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
                                        Authorized_ArtifactType="Requirement" 
                                        Authorized_Permission="Create"
                                        ID="txtDescription" 
                                        runat="server"
                                        Screenshot_ArtifactType="Requirement" 
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
                                            Text="<%$Resources:Fields,RequirementStatusId %>" 
                                            />
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
                                            Text="<%$Resources:Fields,RequirementTypeId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            ActiveItemField="IsActive" 
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="RequirementTypeId" 
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
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed disabled"
                                            DisabledCssClass="u-dropdown disabled"
                                            SkinID="ReleaseDropDownListFarRight"
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
                                            AssociatedControlID="ddlAuthor" 
                                            ID="ddlAuthorLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,AuthorId %>" 
                                            />
					                    <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"  
                                            ID="ddlAuthor" 
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
                                            AssociatedControlID="ddlImportance" 
                                            ID="ddlImportanceLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ImportanceId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown" 
                                            DataTextField="Name" 
                                            DataValueField="ImportanceId" 
                                            DisabledCssClass="u-dropdown disabled" 
                                            ID="ddlImportance" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="ddlComponent" 
                                            ID="ddlComponentLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ComponentId %>" 
                                            />
					                    <tstsc:UnityDropDownListEx 
                                            ActiveItemField="IsActive" 
                                            CssClass="u-dropdown" 
                                            DataTextField="Name" 
                                            DataValueField="ComponentId" 
                                            DisabledCssClass="u-dropdown disabled" 
                                            ID="ddlComponent" 
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
                                id="form-group_dates" >
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsDates" 
                                    runat="server"
                                    >
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





                    <div class="btn-group">
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
                            aria-label="Close"
                            >
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

</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
<script type="text/javascript">

    //Global IDs
    var lblMessage_id = '<%=lblMessage.ClientID%>';
    var g_projectId = <%=ProjectId%>;
    var txtZoomLevel_id = '<%=txtZoomLevel.ClientID%>';
    var ddlShowLevel_id = '<%=ddlShowLevel.ClientID%>';
    var chkIncludeAssociations_id = '<%=chkIncludeAssociations.ClientID%>';
    var g_baseUrl = '<%=this.ResolveUrl("~")%>';

    var unscaled_width;
    var unscaled_height;

	var _pageInfo = {
		canBulkEdit: globalFunctions.isAuthorized(globalFunctions.permissionEnum.BulkEdit, globalFunctions.artifactTypeEnum.requirement) === globalFunctions.authorizationStateEnum.authorized
	};

	//URL Templates
	var urlTemplate_requirementDetails = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2))%>';

    // Create and configure the renderer
    var render = dagreD3.render();
    var g;
    var g_isOverNameDesc = false;

    $(document).ready(function () {
        //Check for IE, doesn't support Array.find which is needed by graphviz
        if (!Array.prototype.find) {
            $('#requirements-model').addClass('not-supported-ie');
        }
        else
        {
            loadRequirementsMap();
        }

        //disable text box
        $('#' + txtZoomLevel_id).prop('disabled', true);
    });

    function loadRequirementsMap()
    {
        var projectId = <%=ProjectId%>;
        var numberOfLevels = null; //All
        var ddlShowLevel = $find(ddlShowLevel_id);
        if (ddlShowLevel.get_selectedItem() && ddlShowLevel.get_selectedItem().get_value() && ddlShowLevel.get_selectedItem().get_value() != '' && ddlShowLevel.get_selectedItem().get_value() > 0)
        {
            numberOfLevels = ddlShowLevel.get_selectedItem().get_value();
        }
        var includeAssociations = $get(chkIncludeAssociations_id).checked;
        var releaseId = null;   //All
        Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.Requirement_RetrieveAsDotNotation(projectId, numberOfLevels, includeAssociations, releaseId, loadRequirementsMap_success, loadRequirementsMap_failure);
    }
    function loadRequirementsMap_success(dotNotation)
    {
        renderMap(dotNotation);
    }
    function loadRequirementsMap_failure(ex)
    {
        globalFunctions.display_error($get(lblMessage_id), ex);
    }

    function renderMap(dotNotation)
    {
        try
        {
            //Get the initial width of the container
            var containerWidth = $get('outer-container').offsetWidth + 'px';

            //Check for IE11

            //create the graph from the notation
            g = graphlibDot.read(dotNotation);

            // Set margins, if not present
            if (!g.graph().hasOwnProperty("marginx") &&
                !g.graph().hasOwnProperty("marginy")) {
                g.graph().marginx = 20;
                g.graph().marginy = 20;
            }

            g.graph().transition = function(selection) {
                return selection.transition().duration(500);
            };

            // Render the graph into svg g
            d3.select("svg g").call(render, g);

            //Next set the width of the SVG so that the scrollbars in the container DIV appear
            var svgRequirementsMap = $get('svgRequirementsMap');
            var bBox = svgRequirementsMap.getBBox();
            unscaled_width = parseInt(bBox.width);
            unscaled_height = parseInt(bBox.height);
            // add 50px to each dimension because otherwise the edges of the svg get cut off - not sure why (simon - 2019-07)
            svgRequirementsMap.style.width = (unscaled_width + 50) +  'px';
            svgRequirementsMap.style.height = (unscaled_height + 50) + 'px';

            //Next set the width of the DIV container
            //$get('requirements-model').style.width = containerWidth;

            //Finally, register event handlers for node tooltips and clicks on the node
            $("#requirements-model svg g a[data-requirement-id]").on("mouseover", function(evt){
                node_displayTooltip(evt.target.getAttribute('data-requirement-id'));
            });
			$("#requirements-model svg g a[data-requirement-id]").on("mouseout", function(evt){
                node_hideTooltip();
            });
			$("#requirements-model svg g a[data-requirement-id]").click(function (evt) {
				node_click(evt.target.getAttribute('data-requirement-id'), evt);
			});
        }
        catch(err)
        {
            globalFunctions.display_error_message($get(lblMessage_id), err);
        }
    }

    //Will either display the inline edit dialog or will redirect to details page if CTRL+click
    function node_click(requirementId, evt) {
        //Check for ctrl/shift click
        if (!evt.shiftKey && !evt.ctrlKey && !evt.metaKey) {
			evt.stopPropagation();
            evt.preventDefault();

			//load the form manager
			var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');
			ajxFormManager.set_primaryKey(requirementId, true);
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

			//Display the edit requirement dialog box
			$get('spnEditRequirement').className = 'di';
			$get('<%=btnSaveRequirement.ClientID%>').style = 'display:inline';
			$get('<%=aNavigateRequirement.ClientID%>').href = urlTemplate_requirementDetails.replace(globalFunctions.artifactIdToken, requirementId);
			$('#dlgAddRequirement').modal('show');

		}
	}

    function node_displayTooltip(requirementId)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        g_isOverNameDesc = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.RetrieveNameDesc(g_projectId, requirementId, null, node_displayTooltip_success);

    }
    function node_displayTooltip_success(tooltipData)
    {
        if (g_isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    }
    function node_hideTooltip()
    {
        hideddrivetip();
        g_isOverNameDesc = false;
    }

    function chkIncludeAssociations_click()
    {
        //The settings are sent on reload so just reload
        //Need to wait a second to allow to the checkbox state to be correct
        setTimeout(function() { reloadDiagram(); }, 200);        
    }
    function reloadDiagram()
    {
        //Clear the elements
        var inner = $("#requirements-model svg g")[0];
        globalFunctions.clearContent(inner);

        //Reload
        loadRequirementsMap();
    }
    function expandToLevel()
    {
        //The settings are sent on reload so just reload
        reloadDiagram();
    }
    function increase_zoom()
    {
        var zoom = parseInt($('#' + txtZoomLevel_id).val());
        if (zoom < 100) {
            zoom += 25; //Increase by 25% increments
            $('#' + txtZoomLevel_id).val(zoom);
            changeZoom(zoom);
        }
    }
    function decrease_zoom()
    {
        var zoom = parseInt($('#' + txtZoomLevel_id).val());
        zoom -= 25; //Decrease by 25% increments, lowest is 25%
        if (zoom < 25)
        {
            zoom = 25;
        }
        $('#' + txtZoomLevel_id).val(zoom);
        changeZoom(zoom);
    }
    function reset_zoom()
    {
        zoom = 100; //reset to 100%
        $('#' + txtZoomLevel_id).val(zoom);
        changeZoom(zoom);
    }
    function changeZoom(percent)
    {        
        var scale = percent / 100;

        //Update the width of the SVG
        var svgRequirementsMap = $get('svgRequirementsMap');
        var width = parseInt(unscaled_width * scale) + 20;
        var height = parseInt(unscaled_height * scale) + 20;
        svgRequirementsMap.style.width = width + 'px';
        svgRequirementsMap.style.height = height + 'px';

        //Now actually scale the SVG
        var svg = d3.select("svg");
        var inner = d3.select("svg g");
        inner.attr("transform", "translate(0,0) scale(" + scale + ")");
    }

    function ajxFormManager_loaded() {
        var ajxFormManager = $find('<%=ajxFormManager.ClientID%>');

		//make sure the status dropdown is always disabled (overriding the workflow)
		var ddlStatus = $find('<%=ddlStatus.ClientID %>');
		ddlStatus.set_enabled(false);
    }
	function ajxFormManager_dataSaved() {
		//Close the dialog
		$('#dlgAddRequirement').modal('hide');

		//Reload the mind map
        reloadDiagram();
	}

    /* TODO: Not implemented
    function export_as_image(format)
    {
        //Currently we only support SVG format export
        if (format == 'SVG')
        {
            //Get the SVG markup from C3 and have the server write it out
            var svgStr = $get('svgRequirementsMap').firstChild.outerHTML;
            if (!svgStr) {
                //IE11
                var inner = $("#svgRequirementsMap:first-child").html();
                svgStr = '<svg style="overflow: hidden;" width="' + unscaled_width + '" height="' + unscaled_height + '">' + inner + '</svg>';
            }
            if (!svgStr) {
                //Other, older browsers
                alert(resx.JqPlot_ImageExportNotSupport);
                return;
            }

            //Now we need to convert the SVG to a file
            var url = g_baseUrl + 'JqPlot/GraphImageSvg.ashx';
            var ajax = new XMLHttpRequest();
            ajax.open("POST", url, true);
            ajax.setRequestHeader('Content-Type', 'image/svg+xml');
            ajax.onreadystatechange = function () {
                if (ajax.readyState == 4) {
                    //Call the same URL, but this time, pass the guid for the file
                    window.location.href = url + "?guid=" + ajax.responseText;
                }
            }
            ajax.send(svgStr);
        }
    }*/
</script>
</asp:Content>
