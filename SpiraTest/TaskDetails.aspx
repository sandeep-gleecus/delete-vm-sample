<%@ Page 
    AutoEventWireup="True" 
    CodeBehind="TaskDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.TaskDetails"
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>

<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel 
                BodyHeight="150"
                ClientScriptMethod="load_data(true)" 
                ClientScriptServerControlId="trvFolders" 
                data-panel="folder-tree" 
                DisplayRefresh="true" 
                HeaderCaption="<%$Resources:Main,Global_Folders %>" 
                ID="pnlFolders" 
                MaxWidth="500" 
                MinWidth="100" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService"
                >
                <div class="Widget panel">
                    <tstsc:TreeView 
                        AllowEditing="false" 
                        Authorized_ArtifactType="Task" 
                        Authorized_Permission="Modify" 
                        ClientScriptMethod="refresh_data();" 
                        ClientScriptServerControlId="navTaskList" 
                        CssClass="FolderTree" 
                        ErrorMessageControlId="lblMessage"
                        ID="trvFolders" 
                        ItemName="<%$Resources:Fields,Folder %>"
                        LoadingImageUrl="Images/action-Spinner.svg" 
                        NodeLegendFormat="{0}"      
                        runat="server" 
                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService"
                        />
                </div>    
            </tstsc:SidebarPanel>

            <tstsc:NavigationBar 
                AutoLoad="true" 
                BodyHeight="520px"                    
                ErrorMessageControlId="lblMessage" 
                ID="navTaskList" 
                IncludeRelease="true" 
                IncludeRequirement="true"
                ItemImage="Images/artifact-Task.svg" 
                AlternateItemImage="Images/artifact-PullRequest.svg"
                ListScreenCaption="<%$Resources:Main,TaskDetails_BackToList%>"
                runat="server" 
                EnableLiveLoading="true"
                FormManagerControlId="ajxFormManager"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService" 
                />
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <%-- BUTTON TOOLBAR --%>
                <div class="clearfix">
                    <div class="btn-group priority1 hidden-md hidden-lg mr3" role="group">
                        <tstsc:HyperlinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" NavigateUrl=<%#ReturnToTasksListUrl%> ToolTip="<%$Resources:Main,TaskDetails_BackToList%>">
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperlinkEx>
                    </div>
                    <div class="btn-group priority1" role="group">
                        <tstsc:DropMenu ID="btnSave" runat="server" Text="<%$Resources:Buttons,Save %>" GlyphIconCssClass="mr3 fas fa-save"
                            MenuWidth="125px"
                            ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="save_data(evt)">
                            <DropMenuItems>
                                <tstsc:DropMenuItem ID="DropMenuItem1" runat="server" GlyphIconCssClass="mr3 fas fa-save"
                                    Name="Save" Value="<%$Resources:Buttons,Save %>" Authorized_ArtifactType="Task"
                                    Authorized_Permission="Modify" ClientScriptMethod="save_data(null); void(0);" />
                                <tstsc:DropMenuItem ID="DropMenuItem2" runat="server" GlyphIconCssClass="mr3 fas fa-save"
                                    Name="SaveAndClose" Value="<%$Resources:Buttons,SaveAndClose %>" Authorized_ArtifactType="Task"
                                    Authorized_Permission="Modify" ClientScriptMethod="save_data(null, 'close'); void(0);" />
                                <tstsc:DropMenuItem ID="DropMenuItem3" runat="server" GlyphIconCssClass="mr3 fas fa-plus"
                                    Name="SaveAndNew" Value="<%$Resources:Buttons,SaveAndNew %>" Authorized_ArtifactType="Task"
                                    Authorized_Permission="Create" ClientScriptMethod="save_data(null, 'new'); void(0);" />
                            </DropMenuItems>
                        </tstsc:DropMenu>
                        <tstsc:DropMenu ID="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>"
                            GlyphIconCssClass="mr3 fas fa-sync" ClientScriptServerControlId="ajxFormManager"
                            ClientScriptMethod="load_data()" />
						<tstsc:DropMenu 
                            Authorized_ArtifactType="Task" 
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
                                <tstsc:DropMenuItem Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="Task" />
                                <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="Task" />
                            </DropMenuItems>
						</tstsc:DropMenu>
                    </div>
                    <div class="btn-group priority2" role="group">
                        <tstsc:DropMenu ID="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>"
                            GlyphIconCssClass="mr3 fas fa-trash-alt" Authorized_ArtifactType="Task" Authorized_Permission="Delete"
                            ClientScriptMethod="delete_item()" 
                            ClientScriptServerControlId="ajxFormManager"         
                            Confirmation="True" ConfirmationMessage="<%$Resources:Messages,TaskDetails_DeleteConfirm %>" />
                    </div>
                    <div class="btn-group priority4" role="group">
                        <tstsc:DropMenu ID="mnuTools" runat="server" Text="<%$Resources:Buttons,Tools %>" PostBackOnClick="false"
                                GlyphIconCssClass="mr3 fas fa-cog" Confirmation="false" Enabled="true" MenuWidth="80px">
                            <DropMenuItems>
                                <tstsc:DropMenuItem ID="DropMenuItem4" runat="server" GlyphIconCssClass="mr3 fas fa-print"
                                    Name="Print" Value="<%$Resources:Buttons,Print %>" ClientScriptMethod="print_item('html')" Authorized_ArtifactType="Task"/>
                                <tstsc:DropMenuItem Divider="true" />
                                <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Task" Authorized_Permission="View" ClientScriptMethod="print_item('excel')" />
                                <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Task" Authorized_Permission="View" ClientScriptMethod="print_item('word')" />
                                <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Task" Authorized_Permission="View" ClientScriptMethod="print_item('pdf')" />
                                <tstsc:DropMenuItem Divider="true" />
                                <tstsc:DropMenuItem ID="DropMenuItem5" runat="server" GlyphIconCssClass="mr3 fas fa-unlink"
                                    Name="Split" Value="<%$Resources:Buttons,Split %>" Authorized_ArtifactType="Task"
                                    Authorized_Permission="Modify" ClientScriptMethod="displaySplitDialog()" />
                            </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                    <div 
                        class="btn-group priority3" 
                        id="pnlEmailToolButtons"
                        role="group"
                        >
                        <tstsc:DropMenu ID="btnEmail" runat="server"
                            Text="<%$Resources:Buttons,Email %>"
                            GlyphIconCssClass="mr3 far fa-envelope"
                            data-requires-email="true" 
                            ClientScriptMethod="ArtifactEmail_pnlSendEmail_display(evt)"
                            Confirmation="false" />
                        <tstsc:DropMenu 
                            Authorized_ArtifactType="Task"
                            Authorized_Permission_DropMenuItems="Modify"
                            ClientScriptMethod="ArtifactEmail_subscribeArtifactChange(this)" 
                            Confirmation="false" 
                            GlyphIconCssClass="mr3 far fa-star" 
                            ID="btnSubscribe" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Subscribe %>" 
                            >
                            <DropMenuItems>
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="Task"
                                    Authorized_Permission="Modify"
                                    ClientScriptMethod="ArtifactAddFollower_pnlAddFollower_display()" 
                                    Name="AddFollower" 
                                    GlyphIconCssClass="mr3 fas fa-user" 
                                    Value="<%$Resources:Buttons,AddFollower %>" 
                                    />
			                </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                </div>

                <asp:Panel ID="pnlFolderPath" runat="server" CssClass="" />

                <%-- HEADER AREA --%>
                <div class="u-wrapper width_md sm-hide-isfixed xs-hide-isfixed xxs-hide-isfixed">
                    <div class="textarea-resize_container mb3">
                        <tstsc:UnityTextBoxEx 
                            CssClass="u-input_title u-input textarea-resize_field mt2 mb1"
                            ID="txtName" 
	                        MaxLength="255" 
                            placeholder="<%$Resources:ClientScript,Artifact_EnterNewName %>"
                            TextMode="MultiLine"
                            Rows="1"
                            runat="server"
                            />
                        <div class="textarea-resize_checker"></div>
                    </div>
                    <div class="py2 px3 mb2 bg-near-white br2 dif items-center flex-wrap">
                        <div class="py1 pr4 dif items-center ma0-children fs-h4 fs-h6-xs">
                            <tstsc:ImageEx 
                                CssClass="w5 h5"
                                AlternateText="<%$Resources:Fields,Task %>" 
                                ID="imgTask" 
                                runat="server" 
                                />
                            <span class="pl4 silver nowrap">
                                <tstsc:LabelEx 
                                    CssClass="pointer dib orange-hover transition-all"
                                    title="<%$Resources:Buttons,CopyToClipboard %>"
                                    data-copytoclipboard="true"
                                    ID="lblTaskId" 
                                    runat="server" 
                                    />
                            </span>
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ddlType" 
                                ID="ddlTypeLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,TaskTypeId %>"
                                />
                            <tstsc:UnityDropDownListEx 
                                CssClass="u-dropdown" 
                                DataTextField="Name" 
                                DataValueField="TaskTypeId"
                                DisabledCssClass="u-dropdown disabled" 
                                ID="ddlType" 
                                runat="server" 
                                />
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxWorkflowOperations"
                                ID="LabelEx1" 
                                Required="True" 
                                runat="server" 
                                Text="<%$Resources:Fields,IncidentStatusId %>" 
                                />
                            <div class="dib v-mid-children dif flex-wrap items-center pl3">
								<tstsc:LabelEx 
                                    ID="lblTaskStatusValue" 
                                    MetaData="" 
                                    runat="server" 
                                    />
                                <div class="dib ml4 v-mid-children dif items-center">
									<tstsc:HyperLinkEx 
                                        ClientScriptMethod="workflow_revert()"
                                        ClientScriptServerControlId="ajxFormManager" 
                                        ID="btnRevert"
                                        CssClass="btn orange-dark mr4"
                                        NavigateUrl="javascript:void(0)" 
                                        runat="server" 
                                        >
                                        <span class="fas fa-undo"></span>
                                        <asp:Literal 
                                            runat="server" 
                                            Text="<%$Resources:Buttons,Revert %>" 
                                            />
									</tstsc:HyperLinkEx>
                                    <tstsc:WorkflowOperations 
                                        AutoLoad="false" 
                                        ErrorMessageControlId="lblMessage" 
                                        FormManagerControlId="ajxFormManager" 
                                        ID="ajxWorkflowOperations" 
                                        runat="server" 
                                        VerticalGroup="false"
						                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService" 
                                        />
                                </div>
                            </div>
                        </div>
                        <div class="py1 pr5 dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="eqlProgress"
                                CssClass="silver"
                                ID="lblProgress" 
                                Required="false" 
                                runat="server" 
                                Text="<%$Resources:Fields,Progress %>" 
                                />
                            <span class="pl3 pr4">
                                <tstsc:Equalizer 
                                    ID="eqlProgress" 
                                    runat="server" 
                                    />
                                &nbsp;(<tstsc:LabelEx 
                                    ID="lblPercentComplete" 
                                    runat="server" 
                                    />%)
                            </span>
                        </div>
                    </div>
                </div>
                <tstsc:MessageBox 
                    ID="lblMessage" 
                    runat="server" 
                    SkinID="MessageBox" 
                    />
            </div>



            <%-- TOOLBAR POPUPS --%>
            <tstuc:ArtifactEmail 
                ArtifactId="<%# this.taskId %>"
                ArtifactTypeEnum="Task" 
                ID="tstEmailPanel" 
                runat="server" 
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="Incident" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />




            <section class="main-content">



                <%-- TABS --%>
                <tstsc:TabControl 
                    ID="tclTaskDetails" CssClass="TabControl2" TabWidth="100" TabHeight="25"
                    TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
                    runat="Server">
                    <TabPages>
                        <tstsc:TabPage 
                            Caption="<% $Resources:ServerControls,TabControl_Overview %>"
                            ID="tabOverview" 
                            runat="server" 
                            TabPageControlId="pnlOverview" 
                            TabPageIcon="fas fa-home"
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_OVERVIEW %>"
                            />
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Revisions %>" 
                            ID="tabRevisions" 
                            runat="server"
                            TabPageControlId="pnlRevisions" 
                            TabPageImageUrl="Images/artifact-Revision.svg" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_REVISION %>"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstAssociationPanel"
                            AjaxServerControlId="grdAssociationLinks" 
                            Caption="<% $Resources:ServerControls,TabControl_Associations %>"
                            ID="tabAssociations" 
                            runat="server" 
                            TabPageControlId="pnlAssociations" 
                            TabPageIcon="fas fa-link"
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_ASSOCIATION %>"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstAttachmentPanel" 
                            AjaxServerControlId="grdAttachmentList" 
                            Caption="<% $Resources:ServerControls,TabControl_Attachments %>"
                            ID="tabAttachments" 
                            runat="server" 
                            TabPageControlId="pnlAttachments"
                            TabPageImageUrl="Images/artifact-Document.svg" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_ATTACHMENTS %>"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstHistoryPanel" 
                            AjaxServerControlId="grdHistoryList" 
                            Caption="<% $Resources:ServerControls,TabControl_History %>"
                            ID="tabHistory" 
                            runat="server" 
                            TabPageControlId="pnlHistory" 
                            TabPageIcon="fas fa-history"  
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_HISTORY %>"  
                            />
                    </TabPages>
                </tstsc:TabControl>




                <%-- OVERVIEW TAB --%>
                <asp:Panel ID="pnlOverview" runat="server" CssClass="TabControlPanel">
                    <section class="u-wrapper width_md">
                        <div class="u-box_1">
                                    
                                    
                                    
                                    
                            <%-- RELEASE FIELD --%>
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_releases" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Main,SiteMap_Releases %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul class="u-box_list" >
                                    <li class="ma0 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlRelease"
                                            ID="ddlReleaseLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ReleaseIteration %>" 
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
                                    <li class="ma0 pa0 pb2">
                                        <small>
                                            <tstsc:LabelEx 
                                                CssClass="ml_u-box-label"
                                                ID="lblReleaseDates" 
                                                runat="server" 
                                                />
                                        </small>
                                    </li>
                                    <li class="ma0 pa0" id="source-branch" style="display:none">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            Required="false" 
                                            runat="server" 
                                            AssociatedControlID="lblSourceBranch"
                                            Text="<%$Resources:Fields,SourceBranch %>" 
                                            />
                                        <span id="lblSourceBranch" runat="server"></span>
                                    </li>
                                    <li class="ma0 pa0 pb2" id="dest-branch" style="display:none">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            Required="false" 
                                            runat="server" 
                                            AssociatedControlID="lblDestBranch"
                                            Text="<%$Resources:Fields,DestBranch %>" 
                                            />
                                        <span id="lblDestBranch" runat="server"></span>
                                    </li>
                                </ul>
                            </div>
                            
                            
                            <%-- USER FIELDS --%>
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_people" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Fields,People %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsUsers" 
                                    runat="server"
                                    >
                                    <li class="ma0 pa0 lh0">
										<div 
                                            id="followersListBox" 
                                            class="u-box_list_control_no-label dib w-100"
                                            >
										</div>       
                                    </li>
                                    <li class="ma0 pa0 mb2">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlCreator"
                                            ID="ddlCreatorLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,CreatorId %>" 
                                            />
                                        <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user" 
                                            DataTextField="FullName" 
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                            ID="ddlCreator" 
                                            runat="server" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" 
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
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lnkRequirement"
                                            ID="lnkRequirementLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,Requirement %>" 
                                            />
                                        <tstsc:ArtifactHyperLink 
                                            AlternateText="<%$Resources:Fields,Requirement %>"
                                            CssClass="ArtifactHyperLink"
                                            DisplayChangeLink="true"
                                            ID="lnkRequirement" 
                                            ItemImage="Images/artifact-Requirement.svg" 
                                            runat="server" 
                                            SummaryItemImage="Images/artifact-RequirementSummary.svg" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lnkRisk"
                                            ID="lnkRiskLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,Risk %>" 
                                            />
                                        <tstsc:ArtifactHyperLink 
                                            AlternateText="<%$Resources:Fields,Risk %>"
                                            CssClass="ArtifactHyperLink"
                                            DisplayChangeLink="false"
                                            ID="lnkRisk" 
                                            ItemImage="Images/artifact-Risk.svg" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="lblComponentName"
                                            ID="lblComponentNameLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ComponentId %>"  
                                            />
                                        <tstsc:LabelEx 
                                            ID="lblComponentName" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                </ul>
                            </div>
                        </div>   
        
        
        
        
                        <%-- DATE TIME FIELDS --%> 
                        <div class="u-box_1">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_dates" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Fields,DatesAndTimes %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsDates" 
                                    runat="server"
                                    >
                                    <li class="ma0 pa0 mb2">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lblCreationDate"
                                            ID="lblCreationDateLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,CreationDate %>" 
                                            />
                                        <tstsc:LabelEx 
                                            ID="lblCreationDate" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 pa0 mb2">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lblLastUpdateDate"
                                            ID="lblLastUpdateDateLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,LastUpdateDate %>" 
                                            />
                                        <tstsc:LabelEx 
                                            ID="lblLastUpdateDate" 
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
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            ID="lblProjectedEffortLabel" 
                                            runat="server" 
                                            AssociatedControlID="lblProjectedEffort"
                                            Text="<%$Resources:Fields,ProjectedEffortWithHours %>" 
                                            AppendColon="true" 
                                            />
                                        <tstsc:LabelEx 
                                            ID="lblProjectedEffort" 
                                            runat="server" 
                                            />
                                    </li>
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





                        <%-- RICH TEXT FIELDS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="form-group_richtext" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Fields,RichTextFieldsTitle %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul 
                                    class="u-box_list labels_absolute" 
                                    id="customFieldsRichText" 
                                    runat="server"
                                    >
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:RichTextBoxJ 
                                            ID="txtDescription" 
                                            runat="server"
                                            Authorized_ArtifactType="Task" 
                                            Authorized_Permission="Modify"
                                            Height="120px" 
                                            Screenshot_ArtifactType="Task" 
                                            />
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="txtDescription" 
                                            ID="lblDescription" 
                                            runat="server" 
                                            Text="<%$Resources:ServerControls,TabControl_Description %>" 
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>




                        <%-- COMMENTS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_comments" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_Comments %>" />
                                    <span class="u-anim u-anim_open-close fr mr3 mt2"></span>
                                </div>
                                <ul class="u-box_list" runat="server">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:CommentList 
                                            ArtifactType="Task" 
                                            AutoLoad="false" 
                                            ErrorMessageControlId="lblMessage"
                                            ID="lstComments" 
                                            runat="server" 
                                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService" 
                                            Width="100%" 
                                            />
                                        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="Task"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="txtNewComment" 
                                            runat="server" 
                                            Screenshot_ArtifactType="Task" 
                                            />
                                        <div class="mtn4">
                                            <tstsc:ButtonEx 
                                                ClientScriptServerControlId="lstComments" 
                                                ID="btnNewComment" 
                                                runat="server"
                                                SkinID="Btn_withComments" 
                                                Text="<%$Resources:Buttons,AddComment %>" 
                                                />
                                        </div>
                                    </li>
                                </ul>
                            </div>
                        </div>


                    </section>
                </asp:Panel>




                <%-- SOURCE CODE COMMITS --%>
				<asp:Panel ID="pnlRevisions" Runat="server" CssClass="TabControlPanel">
                    <div style="width:100%" class="TabControlHeader">
                        <div class="btn-group priority3">
                            <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkRefreshRQTK" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdSourceCodeRevisionList" ClientScriptMethod="load_data()">
                                <span class="fas fa-sync"></span>
                                <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
                            </tstsc:HyperLinkEx>
                            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                    ClientScriptServerControlId="grdSourceCodeRevisionList" ClientScriptMethod="apply_filters()">
			                    <DropMenuItems>
				                    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				                    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			                    </DropMenuItems>
		                    </tstsc:DropMenu>
                        </div>
                    </div>

                    <div class="bg-near-white-hover py2 px3 br2 transition-all">
	                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                        <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	                    <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                        <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	                    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,TaskDetails_PullRequestRevisions %>" />
                        <asp:Label ID="lblMergeInfo" Runat="server" Font-Bold="True" />.
                        <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                    </div>

				    <tstsc:SortedGrid ID="grdSourceCodeRevisionList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
				        SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="lblMessage"
				        RowCssClass="Normal" AutoLoad="false" DisplayAttachments="false" AllowEditing="false"
                        ItemImage="artifact-Revision.svg"
                        FilterInfoControlId="lblFilterInfo" VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount"
				        runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService" />
                </asp:Panel>


                <%-- OTHER PANELS --%>
                <asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlAssociations" 
                    runat="server" 
                    >
                    <tstuc:AssociationsPanel 
                        ID="tstAssociationPanel" 
                        runat="server" 
                        />
                </asp:Panel>


                <asp:Panel 
                    ID="pnlAttachments" 
                    runat="server" 
                    CssClass="TabControlPanel"
                    >
                    <tstuc:AttachmentPanel 
                        ID="tstAttachmentPanel" 
                        runat="server" 
                        />
                </asp:Panel>


                <asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlHistory" 
                    runat="server" 
                    >
                    <tstuc:HistoryPanel ID="tstHistoryPanel" runat="server" />
                </asp:Panel>
            </section>
        </div>
	</div>
    <tstsc:AjaxFormManager ID="ajxFormManager" runat="server" ErrorMessageControlId="lblMessage"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService" ArtifactTypeName="<%$Resources:Fields,Task%>"
        WorkflowEnabled="true" CheckUnsaved="true" RevertButtonControlId="btnRevert" DisplayPageName="true"
        AlternateItemImage="Images/artifact-PullRequest.svg" ItemImage="Images/artifact-Task.svg" ArtifactImageControlId="imgTask"      
        WorkflowOperationsControlId="ajxWorkflowOperations" FolderPathControlId="pnlFolderPath">
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblTaskName" DataField="Name" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblTaskId" DataField="TaskId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlPriority" DataField="TaskPriorityId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lnkRequirement" DataField="RequirementId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lnkRisk" DataField="RiskId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In"
                PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblComponentName" DataField="ComponentId" Direction="In"
                PropertyName="textValue" />
            <tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlCreator" DataField="CreatorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In"
                PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="ddlStatus" DataField="TaskStatusId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtEstimatedEffort" DataField="EstimatedEffort"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtActualEffort" DataField="ActualEffort" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtRemainingEffort" DataField="RemainingEffort"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="datStartDate" DataField="StartDate" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="datEndDate" DataField="EndDate" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblProjectedEffort" DataField="ProjectedEffort"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblPercentComplete" DataField="CompletionPercent" Direction="In" />
            <tstsc:AjaxFormControl ControlId="eqlProgress" DataField="ProgressId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblReleaseDates" DataField="ReleasesDates" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblTaskStatusValue" DataField="TaskStatusId" Direction="In"
                IsWorkflowStep="true" />
            <tstsc:AjaxFormControl ControlId="ddlType" DataField="TaskTypeId" Direction="InOut"
                ChangesWorkflow="true" />
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
        </ControlReferences>
        <SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
        </SaveButtons>
    </tstsc:AjaxFormManager>
    <tstsc:DialogBoxPanel ID="pnlChangeRequirement" runat="server" AjaxServerControlId="ajxRequirementsSelector"
        Title="<%$Resources:Main,TaskDetails_ChangeRequirement %>" CssClass="PopupPanel"
        Width="510px" Height="370px" Modal="true">
        <p>
            <asp:Localize runat="server" Text="<%$Resources:Main,TaskDetails_ChangeRequirement_Intro %>" />
        </p>
        <tstsc:HierarchicalSelector ID="ajxRequirementsSelector" runat="server" SummarySelect="true"
            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService" MultipleSelect="false"
            ItemImage="Images/artifact-Requirement.svg" SummaryItemImage="Images/artifact-RequirementSummary.svg"
            ErrorMessageControlId="lblMessage" AutoLoad="false" CssClass="HierarchicalSelector "
            Width="500px" Height="230px" />
        <div class="btn-group mt3">
            <tstsc:ButtonEx ID="btnUpdate" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Update %>" runat="server"
                ClientScriptMethod="pnlChangeRequirement_updateItem()" />
            <tstsc:ButtonEx ID="btnCancel" Text="<%$Resources:Buttons,Cancel %>" runat="server"
                ClientScriptServerControlId="pnlChangeRequirement" ClientScriptMethod="close()" />
        </div>
    </tstsc:DialogBoxPanel>
    <tstsc:DialogBoxPanel ID="dlgSplitTask" runat="server" Title="<%$Resources:Main,TaskDetails_SplitTask %>"
        CssClass="PopupPanel" Width="500px" Height="300px" Modal="true" Top="100px" Left="300px">
        <p>
            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,TaskDetails_SplitTask_Intro %>" />
        </p>
        <div class="u-wrapper clearfix">
            <ul class="u-box_list" >
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtSplitTaskName" 
                        ID="txtSplitTaskNameLabel" 
                        Required="true"
                        runat="server" 
                        Text="<%$Resources:Fields,Name %>"
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input"
                        DisabledCssClass="u-input disabled"
                        ID="txtSplitTaskName" 
                        MaxLength="255" 
                        runat="server" 
                        />
                </li>
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true"
                        AssociatedControlID="ddlSplitTaskOwner" 
                        ID="ddlSplitTaskOwnerLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,OwnerId %>"
                        />
                    <tstsc:UnityDropDownUserList 
                        CssClass="u-dropdown u-dropdown_user" 
                        DataTextField="FullName" 
                        DataValueField="UserId"
                        DisabledCssClass="u-dropdown u-dropdown_user disabled"
                        ID="ddlSplitTaskOwner" 
                        NoValueItem="true" 
                        NoValueItemText="<%$Resources:Dialogs,TaskDetails_SameAsCurrentTask %>" 
                        runat="server" 
                        />
                </li>
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtSplitTaskPercentEffort" 
                        ID="txtSplitTaskPercentEffortLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,PercentEffort %>"
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input w7"
                        DisabledCssClass="u-input disabled"
                        ID="txtSplitTaskPercentEffort" 
                        runat="server" 
                        MaxLength="3"
                        type="text"
                        />
                </li>
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtSplitTaskComment" 
                        ID="txtSplitTaskCommentLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,Comment %>"
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input"
                        DisabledCssClass="u-input disabled"
                        ID="txtSplitTaskComment" 
                        runat="server" 
                        MaxLength="255" 
                        />
                </li>
            </ul>
        </div>
        <p class="ml2 fs-90 gray">
            <asp:Localize runat="server" Text="<%$Resources:Main,TaskDetails_SplitTask_Notes %>" />
        </p>
        <div class="btn-group ml2">
            <tstsc:ButtonEx ID="btnSplit" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Split %>" runat="server"
                ClientScriptMethod="dlgSplitTask_splitTask()" />
            <tstsc:ButtonEx ID="btnSplitCancel" Text="<%$Resources:Buttons,Cancel %>" runat="server"
                ClientScriptServerControlId="dlgSplitTask" ClientScriptMethod="close()" />
        </div>
    </tstsc:DialogBoxPanel>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/SourceCodeRevisionService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/PullRequestService.svc" />
        </Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/Followers.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;

        SpiraContext.pageId = "Inflectra.Spira.Web.TaskDetails";
        SpiraContext.ArtifactId = <%=taskId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=taskId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
        SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Task%>;
        SpiraContext.EmailEnabled = <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.EmailSettings_Enabled.ToString().ToLowerInvariant()%>;
        SpiraContext.HasCollapsiblePanels = true;
        SpiraContext.Mode = 'update';
		SpiraContext.IsActiveForSourceCode = <%=IsActiveForSourceCode.ToString().ToLowerInvariant()%>

        //Server Control IDs
        var btnSubscribe_id = '<%=this.btnSubscribe.ClientID%>';
        var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
        var lstComments_id = '<%=this.lstComments.ClientID %>';
        var btnNewComment_id = '<%=this.btnNewComment.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';
        var txtName_id = '<%=txtName.ClientID%>';
        var btnSave_id = '<%=btnSave.ClientID%>';
        var tabControl_id = '<%=this.tclTaskDetails.ClientID%>';
        var navigationBar_id = '<%=this.navTaskList.ClientID%>';
        var btnEmail_id = '<%=btnEmail.ClientID%>';

        //TabControl Panel IDs
        var pnlOverview_id= '<%=pnlOverview.ClientID%>';
        var pnlRevisions_id= '<%=pnlRevisions.ClientID%>';
        var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
        var pnlHistory_id = '<%=pnlHistory.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';

        //Base URLs
        var urlTemplate_artifactRedirectUrl = '<%=TaskRedirectUrl %>';
        var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToTasksListUrl) %>';
        var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
        var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

        function ajxWorkflowOperations_operationExecuted(transitionId, isStatusOpen)
        {
            //Put any post-workflow operations here
        }
        function ajxFormManager_operationReverted(statusId, isStatusOpen)
        {
            //Put any post-revert operations here
        }

        function lnkRequirement_changeClicked(evt)
        {
            //Display the dialog box
            var pnlChangeRequirement = $find('<%=pnlChangeRequirement.ClientID%>');
            pnlChangeRequirement.display(evt);
        }
        function pnlChangeRequirement_updateItem()
        {
            //Get the currently selected item
            var lnkRequirement = $find('<%=lnkRequirement.ClientID %>');
            var ajxRequirementsSelector = $find('<%=ajxRequirementsSelector.ClientID %>');
            var items = ajxRequirementsSelector.get_selectedItems();
            var requirementId = null;
            if (items.length > 0)
            {
                //Get the selected item
                requirementId = items[0];
                var requirementName = ajxRequirementsSelector.get_artifactName(requirementId);
                lnkRequirement.update_artifact(requirementId, requirementName, '');
            }
            else
            {
                //No requirement selected
                lnkRequirement.update_artifact(requirementId, '', '');
            }

            //Close the dialog box
            var pnlChangeRequirement = $find('<%=pnlChangeRequirement.ClientID%>');
            pnlChangeRequirement.close();

            //Need to also reload the release dropdown in case that has changed
            var projectId = <%=ProjectId %>;
            //Get the list of releases for this requirement from the web service
            Inflectra.SpiraTest.Web.Services.Ajax.TasksService.GetReleasesForTaskRequirement(projectId, requirementId, pnlChangeRequirement_updateItem_success, pnlChangeRequirement_updateItem_failure);
        }
        function pnlChangeRequirement_updateItem_success (data, unsavedChanges)
        {
            //Clear values and databind
            var ddlRelease = $find('<%=ddlRelease.ClientID %>');
            ddlRelease.clearItems();
            ddlRelease.addItem('', '0', '', '', 'Y', '-- ' + resx.Global_None + ' --');
            if (data)
            {
                ddlRelease.set_dataSource(data);
                ddlRelease.dataBind();
                ddlRelease.set_selectedItem('');
            }
        }
        function pnlChangeRequirement_updateItem_failure (ex, unsavedChanges)
        {
            //Display message
            globalFunctions.display_error($get('<%=lblMessage.ClientID%>') ,ex);
        }
        function displaySplitDialog(evt)
        {
            var dlgSplitTask = $find('<%=dlgSplitTask.ClientID %>');
            $get('<%=txtSplitTaskName.ClientID %>').value = $get('<%=txtName.ClientID %>').value;
            dlgSplitTask.display(evt);
        }
        function dlgSplitTask_splitTask()
        {
            var projectId = SpiraContext.ProjectId;
            var taskId = SpiraContext.ArtifactId;
            var txtSplitTaskName = $get('<%=txtSplitTaskName.ClientID %>');
            var ddlSplitTaskOwner = $find('<%=ddlSplitTaskOwner.ClientID %>');
            var txtSplitTaskPercentEffort = $get('<%=txtSplitTaskPercentEffort.ClientID %>');
            var txtSplitTaskComment = $get('<%=txtSplitTaskComment.ClientID %>');

            //Make sure that we have valid data
            if (!txtSplitTaskName.value || txtSplitTaskName.value == '')
            {
                alert (resx.TaskDetails_SplitTaskNameRequired);
                return;
            }

            var effortPercentage = null;
            if (txtSplitTaskPercentEffort.value && txtSplitTaskPercentEffort.value != '')
            {
                if (!globalFunctions.isInteger(txtSplitTaskPercentEffort.value))
                {
                    alert (resx.TaskDetails_SplitTaskEffortNotInteger);
                    return;
                }
                if (parseInt(txtSplitTaskPercentEffort.value) < 0 || parseInt(txtSplitTaskPercentEffort.value) > 100)
                {
                    alert (resx.TaskDetails_SplitTaskEffortNotInRange);
                    return;
                }
                effortPercentage = parseInt(txtSplitTaskPercentEffort.value);
            }

            //Now actually perform the split
            var name = txtSplitTaskName.value;
            var ownerId = null;
            if (ddlSplitTaskOwner.get_selectedItem() && ddlSplitTaskOwner.get_selectedItem().get_value() && ddlSplitTaskOwner.get_selectedItem().get_value() != '')
            {
                ownerId = ddlSplitTaskOwner.get_selectedItem().get_value();
            }
            var comment = txtSplitTaskComment.value;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TasksService.Task_Split(projectId, taskId, name, effortPercentage, ownerId, comment, dlgSplitTask_splitTask_success, dlgSplitTask_splitTask_failure);
        }
        function dlgSplitTask_splitTask_success(newTaskId)
        {
            globalFunctions.hide_spinner();
            $get('<%=txtSplitTaskPercentEffort.ClientID %>').value = '';
            $get('<%=txtSplitTaskComment.ClientID %>').value = '';
            var dlgSplitTask = $find('<%=dlgSplitTask.ClientID %>');
            dlgSplitTask.close();

            //Redirect to the new task
            var baseUrl = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Tasks, ProjectId, -2)) %>';
            var newUrl = baseUrl.replace(globalFunctions.artifactIdToken, newTaskId);
            window.location = newUrl;
        }
        function dlgSplitTask_splitTask_failure(ex)
        {
            globalFunctions.hide_spinner();
            //display the message
            globalFunctions.display_error(null, ex);
        }

        //Prints the current items
        function print_item(format)
        {
            var artifactId = SpiraContext.ArtifactId;

            //Get the report type from the format
            var reportToken;
            var filter;
            if (format == 'excel')
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TaskSummary%>";
                filter = "&af_14_95=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TaskDetailed%>";
                filter = "&af_15_95=";
            }

            //Open the report for the specified format
            globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
        }

        //Updates any page specific content
        var taskDetails_taskId = -1;
        function updatePageContent()
        {
            //See if the artifact id has changed
            if (taskDetails_taskId != SpiraContext.ArtifactId)
            {
                var ajxFormManager = $find(ajxFormManager_id);
                var ddlRelease = $find('<%=ddlRelease.ClientID%>');
                if (ajxFormManager && ajxFormManager.get_dataItem())
                {
                    var taskData = ajxFormManager.get_dataItem();

                    //See if we should only display a subset of releases/iterations
                    var requirementId = null;
                    if (taskData.Fields.RequirementId.intValue && taskData.Fields.RequirementId.intValue > 0)
                    {
                        //Get the release/iterations for this requirement
                        requirementId = taskData.Fields.RequirementId.intValue;
                    }
                    Inflectra.SpiraTest.Web.Services.Ajax.TasksService.GetReleasesForTaskRequirement(SpiraContext.ProjectId, taskData.Fields.RequirementId.intValue, updatePageContent_updateReleases_success, updatePageContent_updateReleases_failure);

                    //See if we should show/hide the Revisions tab (pull request only)
                    var tabControl = $find(tabControl_id);
                    if (taskData.alternate)
                    {
						if (SpiraContext.IsActiveForSourceCode && SpiraContext.ProjectRole.IsSourceCodeView) {
                            tabControl.get_tabPage('tabRevisions').show();

                            //-- Source Code Revisions --
                            var grdSourceCodeRevisionList = $find('<%=this.grdSourceCodeRevisionList.ClientID%>');
                            var revisionfilters = {};
                            revisionfilters[globalFunctions.keyPrefix + 'PullRequestId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                            grdSourceCodeRevisionList.set_standardFilters(revisionfilters);
                            grdSourceCodeRevisionList.load_data();

                            //See if we have data
                            Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeRevisionService.SourceCodeRevision_CountForPullRequest(SpiraContext.ProjectId, SpiraContext.ArtifactId, revisionsHasData_success);
                        }
                        else {
                            tabControl.get_tabPage('tabRevisions').hide();
                        }

                        //Display the merge info
                        Inflectra.SpiraTest.Web.Services.Ajax.PullRequestService.SortedList_Refresh(SpiraContext.ProjectId, SpiraContext.ArtifactId, null, displayMergeInfo_success);
                    }
                    else
                    {
                        tabControl.get_tabPage('tabRevisions').hide();
                        //Make sure it's not the selected panel, if so switch to overview tab
                        if (tabControl.get_selectedTabClientId() == pnlRevisions_id)
                        {
                            tabControl.set_selectedTabClientId(pnlOverview_id);
                        }
                        //Hide the branches
                        $get('source-branch').style.display = 'none';
                        $get('dest-branch').style.display = 'none';
                    }
                }
            }
        }
        function updatePageContent_updateReleases_success (data)
        {
            //Clear values and databind
            var ddlRelease = $find('<%=ddlRelease.ClientID %>');
            ddlRelease.clearItems();
            ddlRelease.addItem('', '0', '', '', 'Y', '-- ' + resx.Global_None + ' --');
            if (data)
            {
                ddlRelease.set_dataSource(data);
                ddlRelease.dataBind();
            }
        }
        function updatePageContent_updateReleases_failure (ex)
        {
            //Display message
            globalFunctions.display_error($get('<%=lblMessage.ClientID%>') ,ex);
        }
        function revisionsHasData_success(hasData)
        {
            $find(tabControl_id).updateHasData('tabRevisions', hasData);
        }
        function displayMergeInfo_success(dataItem)
        {
            var mergeInfo = '';
            if (dataItem && dataItem.Fields.SourceBranchId && dataItem.Fields.DestBranchId)
            {
                var sourceBranch = (dataItem.Fields.SourceBranchId.textValue) ? dataItem.Fields.SourceBranchId.textValue : '-';
                var destBranch = (dataItem.Fields.DestBranchId.textValue) ? dataItem.Fields.DestBranchId.textValue : '-';
                mergeInfo = sourceBranch + ' > ' + destBranch;

                //Hide the Commits tab if we don't have both branches or if they are the same as it will be useless
                if (sourceBranch == destBranch) {
                    var tabControl = $find(tabControl_id);
                    tabControl.get_tabPage('tabRevisions').hide();
                    //Make sure it's not the selected panel, if so switch to overview tab
                    if (tabControl.get_selectedTabClientId() == pnlRevisions_id) {
                        tabControl.set_selectedTabClientId(pnlOverview_id);
                    }
                }

                //Display the branch names as well
                $get('source-branch').style.display = 'inline';
                $get('dest-branch').style.display = 'inline';
                $('#<%=lblSourceBranch.ClientID%>').text(sourceBranch);
                $('#<%=lblDestBranch.ClientID%>').text(destBranch);
            }
            $('#<%=lblMergeInfo.ClientID%>').text(mergeInfo);
        }

        function grdSourceCodeRevisionList_loaded()
        {
            //Search the grid messages for any tokens and make them artifact hyperlinks
            var grdSourceCodeRevisionList_id = '<%=this.grdSourceCodeRevisionList.ClientID%>';
            var els = $('#' + grdSourceCodeRevisionList_id + ' tr.Normal td div:contains("[")');
            var regex = /\[(?<key>[A-Z]{2})[:\-](?<id>\d*?)\]/gi;
            for (var i = 0; i <= els.length; i++)
            {
                if (els[i])
                {
                    var text = els[i].innerHTML;
                    if (text)
                    {
                        els[i].innerHTML = text.replace(regex, replacer);
                    }
                }
            }
        }

        function replacer(match, artifactPrefix, artifactId, offset, string) {
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

        //Get artifact types
        var artifactTypes = [];
        $(document).ready(function(){
            artifactTypes = globalFunctions.getArtifactTypes();
        });
    </script>
</asp:Content>
