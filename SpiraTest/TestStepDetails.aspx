<%@ Page 
    AutoEventWireup="true" 
    CodeBehind="TestStepDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.TestStepDetails" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
			<tstsc:NavigationBar ID="navTestStepList" runat="server" AutoLoad="true" ItemImage="Images/artifact-TestStep.svg"
                IncludeAssigned="false" SummaryItemImage="Images/artifact-TestCase.svg" AlternateItemImage="Images/artifact-TestLink.svg"
                BodyHeight="580px" ErrorMessageControlId="lblMessage" ListScreenCaption="<%$Resources:Main,TestStepDetails_BackToList%>"
                EnableLiveLoading="true" FormManagerControlId="ajxFormManager"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestStepService" />
		</div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="clearfix">
                    <div class="btn-group priority1 hidden-md hidden-lg" role="group">
                        <tstsc:HyperLinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" NavigateUrl="<%#ReturnToListPageUrl%>" ToolTip="<%$Resources:Main,TestCaseDetails_BackToList%>">
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperLinkEx>
                    </div>
                    <div class="btn-group priority2" role="group">
						<tstsc:DropMenu ID="btnSave" runat="server" Text="<%$Resources:Buttons,Save %>" GlyphIconCssClass="mr3 fas fa-save" Authorized_ArtifactType="TestStep" Authorized_Permission="Modify" MenuWidth="125px" ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="save_data(evt)">
							<DropMenuItems>
								<tstsc:DropMenuItem ID="DropMenuItem1" runat="server" GlyphIconCssClass="mr3 fas fa-save" Name="Save" Value="<%$Resources:Buttons,Save %>" Authorized_ArtifactType="TestStep" Authorized_Permission="Modify" ClientScriptMethod="save_data(null); void(0);" />
								<tstsc:DropMenuItem ID="DropMenuItem2" runat="server" GlyphIconCssClass="mr3 far fa-file-excel" Name="SaveAndClose" Value="<%$Resources:Buttons,SaveAndClose %>" Authorized_ArtifactType="TestStep" Authorized_Permission="Modify" ClientScriptMethod="save_data(null, 'close'); void(0);" />
								<tstsc:DropMenuItem ID="DropMenuItem3" runat="server" GlyphIconCssClass="mr3 far fa-copy" Name="SaveAndNew" Value="<%$Resources:Buttons,SaveAndNew %>" Authorized_ArtifactType="TestStep" Authorized_Permission="Create" ClientScriptMethod="save_data(null, 'new'); void(0);" />
							</DropMenuItems>
						</tstsc:DropMenu>
						<tstsc:DropMenu ID="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync" ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="load_data()" />
        				<tstsc:DropMenu 
                            Authorized_ArtifactType="TestStep" 
                            Authorized_Permission="Create" 
							Confirmation="false" 
							GlyphIconCssClass="mr3 fas fa-plus"
                            ID="btnCreate" 
                            runat="server" 
                            Text="<%$Resources:Buttons,New %>"
                            ClientScriptServerControlId="ajxFormManager"
                            ClientScriptMethod="create_item()" 
                        >
                            <DropMenuItems>
                                <tstsc:DropMenuItem Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="TestStep" />
                                <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="TestStep" />
                            </DropMenuItems>
						</tstsc:DropMenu>
                    </div>
                    <div class="btn-group priority3" role="group">
						<tstsc:DropMenu ID="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt"
                            ConfirmationMessage="<%$Resources:Messages,TestStepDetails_DeleteConfirm %>" Confirmation="True"
                            ClientScriptMethod="delete_item()" 
                            ClientScriptServerControlId="ajxFormManager" 
                            Authorized_ArtifactType="TestStep" Authorized_Permission="Delete" />
                    </div>
                </div>
                <div class="u-wrapper width_md sm-hide-isfixed xs-hide-isfixed xxs-hide-isfixed">
                    <h2>
                        <tstsc:LabelEx ID="lblTestCase" runat="server" />
					</h2>
                    <div class="px3 mb2 silver dib bg-near-white br2 flex-wrap">
                        <div class="py2 pr4 dib v-mid dif items-center ma0-children fs-h4">
                            <tstsc:ImageEx 
                                CssClass="w5 h5"
                                ID="imgTestStep" 
                                ImageUrl="Images/artifact-TestStep.svg" 
                                runat="server" 
                                />
                            <span class="px4 nowrap">
                                [<tstsc:LabelEx ID="lblTestStepNameNumber" runat="server" />]
                            </span>
                            <div class="py2 dib v-mid dif items-center ma0-children">
                                <tstsc:LabelEx 
                                    AppendColon="true" 
                                    AssociatedControlID="ajxExecutionStatus"
                                    CssClass="silver"
                                    ID="ajxExecutionStatusLabel" 
                                    Required="false" 
                                    runat="server" 
                                    Text="<%$Resources:Fields,ExecutionStatus %>" 
                                    />
                                <span class="pl3 pr4">
                                    <tstsc:StatusBox 
                                        ID="ajxExecutionStatus" 
                                        runat="server" 
                                        Width="100px" 
                                        />
                                </span>
                            </div>

                        </div>
                    </div>
					<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				</div>
            </div>
            <div class="main-content mb5">
                <asp:Panel runat="server" ID="pnlOverview_Details" CssClass="row DataEntryForm form-horizontal" enableviewstate="false">
                    <div class="u-wrapper width_md">
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="form-group_rte" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_Details %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul 
                                    class="u-box_list labels_absolute"
                                    id="customFieldsRichText"
                                    runat="server"
                                    >
                                    <li class="ma0 pa0">
                                        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="TestStep" 
                                            Authorized_Permission="Modify"
                                            ID="txtDescription" 
                                            runat="server"
                                            Screenshot_ArtifactType="TestStep" 
                                            />
									    <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="txtDescription" 
                                            ID="txtDescriptionLabel" 
                                            runat="server" 
                                            Required="true" 
                                            Text="<%$Resources:Fields,Description %>" 
                                            />
                                    </li>
                                    <li class="ma0 mtn4 pa0">
                                        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="TestStep" 
                                            Authorized_Permission="Modify" 
                                            ID="txtExpectedResult" 
                                            runat="server"
                                            Screenshot_ArtifactType="TestStep" 
                                            />
									    <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="txtExpectedResult" 
                                            ID="txtExpectedResultLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ExpectedResult %>" 
                                            />

                                    </li>
                                    <li class="ma0 mtn4 pa0">
                                        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="TestStep" 
                                            Authorized_Permission="Modify"
                                            ID="txtSampleData" 
                                            runat="server"
                                            Screenshot_ArtifactType="TestStep" 
                                            />
									    <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="txtSampleData" 
                                            ID="txtSampleDataLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,SampleData %>" 
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <div class="u-box_1 mb5">
                            <div 
                                class="u-box_group u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="group_properties" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Properties %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul 
                                    class="u-box_list"
                                    id="customFieldsDefault"
                                    runat="server"
                                    >
                                </ul>
                            </div>
                        </div>
                    </div>
                </asp:Panel>



				<tstsc:TabControl 
                    ID="tclTestStepDetails" 
                    CssClass="TabControl2" 
                    TabWidth="100" 
                    TabHeight="25" 
                    TabCssClass="Tab" 
                    SelectedTabCssClass="TabSelected" 
                    DividerCssClass="Divider" 
                    runat="server"
                    >
					<TabPages>
						<tstsc:TabPage 
                            AjaxServerControlId="grdIncidentList" 
                            Caption="<%$Resources:ServerControls,TabControl_Incidents %>" 
                            ID="tabIncidents"
                            runat="server"  
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_INCIDENT %>"
                            TabPageControlId="pnlIncidents" 
                            TabPageImageUrl="Images/artifact-Incident.svg"
                            />
						<tstsc:TabPage 
                            AjaxControlContainer="tstAttachmentPanel" 
                            AjaxServerControlId="grdAttachmentList" 
                            Caption="<%$Resources:ServerControls,TabControl_Attachments %>" 
                            ID="tabAttachments" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_ATTACHMENTS %>"
                            TabPageControlId="pnlAttachments" 
                            TabPageImageUrl="Images/artifact-Document.svg"
                            />
						<tstsc:TabPage 
                            AjaxControlContainer="tstHistoryPanel" 
                            AjaxServerControlId="grdHistoryList" 
                            Caption="<%$Resources:ServerControls,TabControl_History %>" 
                            ID="tabHistory" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_HISTORY %>"
                            TabPageControlId="pnlHistory" 
                            TabPageIcon="fas fa-history"
                            />
						<tstsc:TabPage 
                            AjaxControlContainer="tstCoveragePanel" 
                            AjaxServerControlId="grdAssociationLinks" 
                            AuthorizedArtifactType="Requirement" 
                            Caption="<%$Resources:ServerControls,TabControl_Requirements %>" 
                            CheckPermissions="true" 
                            ID="tabCoverage" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_REQUIREMENT %>"
                            TabPageControlId="pnlRequirements" 
                            TabPageImageUrl="Images/artifact-Requirement.svg"
                            />
					</TabPages>
				</tstsc:TabControl>
						
                <asp:Panel ID="pnlAttachments" runat="server" CssClass="TabControlPanel">
					<tstuc:AttachmentPanel ID="tstAttachmentPanel" runat="server" />
				</asp:Panel>
						
                <asp:Panel ID="pnlHistory" runat="server" CssClass="TabControlPanel">
					<tstuc:HistoryPanel ID="tstHistoryPanel" runat="server" />
				</asp:Panel>

				<asp:Panel ID="pnlIncidents" runat="server" CssClass="TabControlPanel">
					<tstsc:MessageBox ID="divIncidentsMessage" runat="server" SkinID="MessageBox" />
                    <div runat="server" id="addPanelId"></div>
					<div style="width: 100%" class="TabControlHeader">
                        <div class="btn-group priority1">
							<tstsc:HyperLinkEx ID="lnkRefresh" runat="server"
                                NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdIncidentList"
                                ClientScriptMethod="load_data()" SkinID="ButtonDefault">
                                <span class="fas fa-sync"></span>
                                <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>" />
							</tstsc:HyperLinkEx>
                            <tstsc:DropDownListEx ID="ddlShowHideIncidentColumns" runat="server" DataValueField="Key"
                                DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True"
                                NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px"
                                ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="toggle_visibility" />
                            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
		                        ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="apply_filters()">
		                        <DropMenuItems>
			                        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
			                        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
		                        </DropMenuItems>
	                        </tstsc:DropMenu>        
                            <tstsc:HyperLinkEx ID="lnkAddIncidentAssociation" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="lnkAddIncidentAssociation_click(event)" Authorized_Permission="Modify" Authorized_ArtifactType="TestStep"  SkinID="ButtonDefault">
                                <span class="fas fa-link"></span>
                                <asp:Localize runat="server" Text="<%$Resources:Main,TestRunDetails_LinkExisting %>" />
                            </tstsc:HyperLinkEx>
                        </div>
					</div>
                    <div class="alert alert-warning alert-narrow">
                        <span class="fas fa-info-circle"></span>
						<asp:Localize ID="Localize25" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
						<asp:Label ID="lblIncidentVisibleCount" runat="server" Font-Bold="True" />
						<asp:Localize ID="Localize26" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
						<asp:Label ID="lblIncidentTotalCount" runat="server" Font-Bold="True" />
						<asp:Localize ID="Localize27" runat="server" Text="<%$Resources:Main,TestStepDetails_Incidents %>" />.
						<tstsc:LabelEx ID="lblIncidentFilterInfo" runat="server" />
					</div>
					<tstsc:SortedGrid ID="grdIncidentList" CssClass="DataGrid" HeaderCssClass="Header" AllowColumnPositioning="true" VisibleCountControlId="lblIncidentVisibleCount" TotalCountControlId="lblIncidentTotalCount" FilterInfoControlId="lblIncidentFilterInfo" SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divIncidentsMessage" RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-Incident.svg" runat="server" Authorized_ArtifactType="Incident" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" Authorized_Permission="Modify" ConcurrencyEnabled="true" />

                    <script type="text/javascript">
                    /* The User Control Class */
                    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
                    Inflectra.SpiraTest.Web.UserControls.IncidentPanel = function ()
                    {
                        this._projectId = <%=ProjectId%>;
                        this._artifactId = <%=testStepId%>;
                        this._artifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestStep%>;
                        this._addPanelIsOpen = false;

                        Inflectra.SpiraTest.Web.UserControls.IncidentPanel.initializeBase(this);
                    }
                    Inflectra.SpiraTest.Web.UserControls.IncidentPanel.prototype =
                    {
                        initialize: function ()
                        {
                            Inflectra.SpiraTest.Web.UserControls.IncidentPanel.callBaseMethod(this, 'initialize');
                        },
                        dispose: function ()
                        {
                            Inflectra.SpiraTest.Web.UserControls.IncidentPanel.callBaseMethod(this, 'dispose');
                        },

                        /* Properties */
                        get_artifactId: function()
                        {
                            return this._artifactId;
                        },
                        set_artifactId: function(value)
                        {
                            this._artifactId = value;
                        },

                        get_artifactTypeId: function()
                        {
                            return this._artifactTypeId;
                        },
                        set_artifactTypeId: function(value)
                        {
                            this._artifactTypeId = value;
                        },

                        get_addPanelIsOpen: function()
                        {
                            return this._addPanelIsOpen;
                        },
                        set_addPanelIsOpen: function(value)
                        {
                            this._addPanelIsOpen = value;
                        },

                        /* Functions */
                        check_hasData: function(callback)
                        {
                            //See if we have data
                            var artifactReference = {
                                artifactId: this._artifactId,
                                artifactTypeId: this._artifactTypeId,
                            };
                            Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
                        },
                        check_hasData_success: function(count, callback)
                        {
                            if (callback)
                            {
                                //Specify if we have data or not
                                callback(count > 0);
                            }
                        },
                        check_hasData_failure: function(ex)
                        {
                            //Fail quietly
                        },

                        load_data: function(filters, loadNow)
                        {
                            var grdIncidentList = $find('<%=grdIncidentList.ClientID%>');
                            grdIncidentList.set_standardFilters(filters);
                            if (loadNow)
                            {
                                grdIncidentList.load_data();
                            }
                            else
                            {
                                grdIncidentList.clear_loadingComplete();
                            }
                        }
                    }
                    var tstucIncidentPanel = $create(Inflectra.SpiraTest.Web.UserControls.IncidentPanel);
                    </script>
				</asp:Panel>

                <asp:Panel ID="pnlRequirements" runat="server" CssClass="TabControlPanel">
					<tstuc:AssociationsPanel ID="tstCoveragePanel" runat="server" DisplayTypeEnum="TestStep_Requirements" />
				</asp:Panel>
            </div>
        </div>
	</div>

	<tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,TestStep%>" 
        CheckUnsaved="true"
        ErrorMessageControlId="lblMessage" 
        ID="ajxFormManager" 
        runat="server" 
        DisplayPageName="true"
        NameField="Position"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestStepService" 
        WorkflowEnabled="true" 
        >
		<ControlReferences>
			<tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtExpectedResult" DataField="ExpectedResult" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtSampleData" DataField="SampleData" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblTestStepNameNumber" DataField="Position" PropertyName="textValue" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblTestCase" DataField="TestCase" PropertyName="textValue" Direction="In" />
            <tstsc:AjaxFormControl ControlId="ajxExecutionStatus" DataField="ExecutionStatusId"
                Direction="In" />
		</ControlReferences>
		<SaveButtons>
			<tstsc:AjaxFormSaveButton ControlId="btnSave" />
		</SaveButtons>
	</tstsc:AjaxFormManager>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/TestStepService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
	<script type="text/javascript">
	    var resx = Inflectra.SpiraTest.Web.GlobalResources;

	    SpiraContext.pageId = "Inflectra.Spira.Web.TestStepDetails";
        SpiraContext.ArtifactId = <%=testStepId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
	    SpiraContext.ArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.TestStep%>;
	    SpiraContext.HasCollapsiblePanels = true;
	    SpiraContext.Mode = 'update';

	    //Server Control IDs
	    var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
	    var txtName_id = '<%=lblTestCase.ClientID%>';
	    var btnSave_id = '<%=btnSave.ClientID%>';
	    var tabControl_id = '<%=this.tclTestStepDetails.ClientID%>';
	    var navigationBar_id = '<%=this.navTestStepList.ClientID%>';

	    //TabControl Panel IDs
	    var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
	    var pnlHistory_id = '<%=pnlHistory.ClientID%>';
	    var pnlIncidents_id = '<%=pnlIncidents.ClientID%>';
	    var pnlCoverage_id = '<%=pnlRequirements.ClientID%>';

	    //URL Templates
	    var urlTemplate_artifactRedirectUrl = '<%=this.TestStepRedirectUrl %>';
	    var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(this.ReturnToListPageUrl) %>';
	    var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
	    var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

        
	    //adding a new association
	    function lnkAddIncidentAssociation_click(evt)
        {
	        //populate general data into the global panelAssociationAdd object, so it is accessible by React on render
	        panelAssociationAdd.lnkAddBtnId = '<%=lnkAddIncidentAssociation.ClientID%>';
	        panelAssociationAdd.addPanelId = '<%=addPanelId.ClientID%>';
	        panelAssociationAdd.addPanelObj = tstucIncidentPanel;
	        panelAssociationAdd.displayType = globalFunctions.displayTypeEnum.TestStep_Incidents;
	        panelAssociationAdd.sortedGridId = '<%=grdIncidentList.ClientID%>';
	        panelAssociationAdd.messageBox = '<%=divIncidentsMessage.ClientID%>';
	        panelAssociationAdd.listOfViewableArtifactTypeIds = "<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Incident %>";
        
	        //now render the panel
	        panelAssociationAdd.showPanel();
	    }

    </script>
</asp:Content>
