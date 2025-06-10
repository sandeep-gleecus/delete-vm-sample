<%@ Page 
    AutoEventWireup="True" 
    CodeBehind="RequirementDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.RequirementDetails" 
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master"
    ValidateRequest="false" 
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
<%@ Register TagPrefix="tstuc" TagName="TaskListPanel" Src="UserControls/TaskListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="DiagramPanel" Src="UserControls/DiagramPanel.ascx" %>


<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="DiagramsStylesheet" />
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:NavigationBar 
                AlternateItemImage="Images/artifact-UseCase.svg"
                AutoLoad="true"
                BodyHeight="500px" 
                ErrorMessageControlId="lblMessage" 
                ID="navRequirementsList" 
                ItemImage="Images/artifact-Requirement.svg" 
                ListScreenCaption="<%$Resources:Main,RequirementDetails_BackToList%>"
                runat="server" 
                SummaryItemImage="Images/artifact-RequirementSummary.svg"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService" 
                EnableLiveLoading="true" 
                FormManagerControlId="ajxFormManager"
                />
        </div>



        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">



                <%-- BUTTON TOOLBAR --%>
                <div class="clearfix">
                    <div class="btn-group priority1 hidden-md hidden-lg mr3" role="group">
                        <tstsc:HyperlinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" NavigateUrl=<%#ReturnToRequirementsListUrl%> ToolTip="<%$Resources:Main,RequirementDetails_BackToList%>">
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperlinkEx>
                    </div>
                    <div class="btn-group priority1" role="group">
						<tstsc:DropMenu ID="btnSave" runat="server" Text="<%$Resources:Buttons,Save %>" GlyphIconCssClass="mr3 fas fa-save" MenuWidth="125px" ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="save_data(evt)">
							<DropMenuItems>
								<tstsc:DropMenuItem runat="server" GlyphIconCssClass="mr3 fas fa-save" Name="Save" Value="<%$Resources:Buttons,Save %>" Authorized_ArtifactType="Requirement" Authorized_Permission="Modify"  ClientScriptMethod="save_data(null); void(0);" />
								<tstsc:DropMenuItem runat="server" GlyphIconCssClass="mr3 far fa-file-excel" Name="SaveAndClose" Value="<%$Resources:Buttons,SaveAndClose %>" Authorized_ArtifactType="Requirement" Authorized_Permission="Modify" ClientScriptMethod="save_data(null, 'close'); void(0);" />
								<tstsc:DropMenuItem runat="server" GlyphIconCssClass="mr3 far fa-copy" Name="SaveAndNew" Value="<%$Resources:Buttons,SaveAndNew %>" Authorized_ArtifactType="Requirement" Authorized_Permission="Create" ClientScriptMethod="save_data(null, 'new'); void(0);" />
							</DropMenuItems>
						</tstsc:DropMenu>
						<tstsc:DropMenu ID="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync" ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="load_data()" />
						<tstsc:DropMenu 
                            Authorized_ArtifactType="Requirement" 
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
                                <tstsc:DropMenuItem Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="Requirement" />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="Requirement" 
                                    Authorized_Permission="Create" 
                                    ClientScriptMethod="create_childItem()" 
                                    ImageUrl="Images/action-InsertChildRequirement.svg" 
                                    Name="ChildRequirement" 
                                    runat="server"
                                    Value="<%$Resources:Dialogs,RequirementList_ChildRequirement %>" 
                                    />
                                <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="Requirement" />
                            </DropMenuItems>
						</tstsc:DropMenu>
                    </div>
                    <div class="btn-group priority3" role="group">
						<tstsc:DropMenu ID="btnDelete" SkinID="HideOnScroll" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt" ConfirmationMessage="<%$Resources:Messages,RequirementDetails_DeleteConfirm %>" CausesValidation="False" Confirmation="True"
                            ClientScriptMethod="delete_item()" 
                            ClientScriptServerControlId="ajxFormManager"                                     
                            Authorized_ArtifactType="Requirement" Authorized_Permission="Delete" />
                    </div>
                    <div class="btn-group" role="group">
                        <tstsc:DropMenu id="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog"
			                Text="<%$Resources:Buttons,Tools %>" MenuCssClass="DropMenu" PostBackOnClick="false">
			                <DropMenuItems>
                                <tstsc:DropMenuItem Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="print_item('html')" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
				                <tstsc:DropMenuItem Divider="true" />
				                <tstsc:DropMenuItem Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Requirement" Authorized_Permission="View" ClientScriptMethod="print_item('excel')" />
				                <tstsc:DropMenuItem Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Requirement" Authorized_Permission="View" ClientScriptMethod="print_item('word')" />
				                <tstsc:DropMenuItem Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Requirement" Authorized_Permission="View" ClientScriptMethod="print_item('pdf')" />
				                <tstsc:DropMenuItem Divider="true" />
                                <tstsc:DropMenuItem ID="DropMenuItem5" runat="server" GlyphIconCssClass="mr3 fas fa-unlink"
                                    Name="Split" Value="<%$Resources:Buttons,Split %>" Authorized_ArtifactType="Requirement"
                                    Authorized_Permission="Modify" ClientScriptMethod="displaySplitDialog()" />
			                </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                    <div class="btn-group priority2" role="group" id="pnlEmailToolButtons">
						<tstsc:DropMenu ID="btnEmail" runat="server"
                            Text="<%$Resources:Buttons,Email %>"
                            GlyphIconCssClass="mr3 far fa-envelope"
                            ClientScriptMethod="ArtifactEmail_pnlSendEmail_display(evt)"
                            data-requires-email="true" 
                            CausesValidation="false"
                            Confirmation="false" />
						<tstsc:DropMenu 
                            Authorized_ArtifactType="Requirement"
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
                                    Authorized_ArtifactType="Requirement"
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
                        

                <asp:Panel 
                    ID="pnlFolderPath" 
                    runat="server" 
                    />


                <%-- TITLE BAR / INFORMATION --%>
                <div class="u-wrapper width_md sm-hide-isfixed xs-hide-isfixed xxs-hide-isfixed">
                    <div class="textarea-resize_container mb3">
                        <tstsc:UnityTextBoxEx 
                            CssClass="u-input_title u-input textarea-resize_field mt2 mb1"
                            ID="txtName" 
	                        MaxLength="255" 
                            TextMode="MultiLine"
                            Rows="1"
                            runat="server"
                            placeholder="<%$Resources:ClientScript,Artifact_EnterNewName %>"
                            />
                        <div class="textarea-resize_checker"></div>
                    </div>
                    <div class="py2 px3 mb2 bg-near-white br2 dif items-center flex-wrap">
                        <div class="py1 pr4 dif items-center ma0-children fs-h4 fs-h6-xs">
                            <tstsc:ImageEx 
                                CssClass="w5 h5"
                                ID="imgRequirement" 
                                runat="server" 
                                />
                            <span class="pl4 silver nowrap">
                                <tstsc:LabelEx 
                                    CssClass="pointer dib orange-hover transition-all"
                                    title="<%$Resources:Buttons,CopyToClipboard %>"
                                    data-copytoclipboard="true"
                                    ID="lblRequirementId" 
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
                                Text="<%$Resources:Fields,RequirementTypeId %>" 
                                />
							<tstsc:UnityDropDownListEx 
                                ActiveItemField="IsActive" 
                                CssClass="u-dropdown is-active" 
                                DataTextField="Name" 
                                DataValueField="RequirementTypeId" 
                                DisabledCssClass="u-dropdown disabled" 
                                ID="ddlType" 
                                runat="server" 
                                />
                        </div>

                        <div class="py1 dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxWorkflowOperations"
                                ID="ddlStatusLabel" 
                                Required="True" 
                                runat="server" 
                                Text="<%$Resources:Fields,RequirementStatusId %>" 
                                />
                            <div 
                                class="dib v-mid-children dif flex-wrap items-center pl3"
                                id="pnlWorkflowOperations"
                                runat="server"
                                >
								<tstsc:LabelEx 
                                    ID="lblRequirementStatusValue" 
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
						                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService" 
                                        />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
			</div>
			<tstuc:ArtifactEmail 
                ID="tstEmailPanel" 
                runat="server" 
                ArtifactId="<%# this.requirementId %>" 
                ArtifactTypeEnum="Requirement"
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="Requirement" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />



            <div class="main-content">
						<tstsc:TabControl ID="tclRequirementDetails" CssClass="TabControl2" TabWidth="100" TabHeight="25" TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider" DisabledTabCssClass="TabDisabled" runat="server">
							<TabPages>
								<tstsc:TabPage 
                                    Caption="<% $Resources:ServerControls,TabControl_Overview %>" 
                                    ID="tabOverview" 
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_OVERVIEW %>"
                                    TabPageControlId="pnlOverview" 
                                    TabPageIcon="fas fa-home"
                                    />
                                <tstsc:TabPage 
                                    AjaxControlContainer="tstCoveragePanel" 
                                    AjaxServerControlId="grdAssociationLinks" 
                                    AuthorizedArtifactType="TestCase" 
                                    Caption="<% $Resources:ServerControls,TabControl_TestCoverage %>" 
                                    CheckPermissions="true" 
                                    ID="tabCoverage" 
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_COVERAGE %>" 
                                    TabPageImageUrl="Images/artifact-TestCase.svg"
                                    TabPageControlId="pnlCoverage" 
                                    />
								<tstsc:TabPage 
                                    AjaxControlContainer="tstTaskListPanel" 
                                    AjaxServerControlId="grdTaskList" 
                                    AuthorizedArtifactType="Task" 
                                    Caption="<% $Resources:ServerControls,TabControl_Tasks %>" 
                                    CheckPermissions="true" 
                                    ID="tabTasks"
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_TASK %>"
                                    TabPageControlId="pnlTasks" 
                                    TabPageImageUrl="Images/artifact-Task.svg"
                                    />
								<tstsc:TabPage 
                                    Caption="<% $Resources:ServerControls,TabControl_Diagram %>" 
                                    ID="tabDiagram"
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_DIAGRAM %>"
                                    TabPageControlId="pnlDiagram" 
                                    TabPageIcon="fas fa-project-diagram"
                                    />
								<tstsc:TabPage 
                                    AjaxControlContainer="tstAttachmentPanel" 
                                    AjaxServerControlId="grdAttachmentList" 
                                    Caption="<% $Resources:ServerControls,TabControl_Attachments %>" 
                                    ID="tabAttachments" 
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_ATTACHMENTS %>"
                                    TabPageControlId="pnlAttachments" 
                                    TabPageImageUrl="Images/artifact-Document.svg"
                                    />
								<tstsc:TabPage 
                                    AjaxControlContainer="tstHistoryPanel" 
                                    AjaxServerControlId="grdHistoryList" 
                                    Caption="<% $Resources:ServerControls,TabControl_History %>" 
                                    ID="tabHistory" 
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_HISTORY %>"
                                    TabPageControlId="pnlHistory" 
                                    TabPageIcon="fas fa-history"
                                    />
								<tstsc:TabPage 
                                    AjaxControlContainer="tstAssociationPanel" 
                                    AjaxServerControlId="grdAssociationLinks" 
                                    Caption="<% $Resources:ServerControls,TabControl_Associations %>" 
                                    ID="tabAssociations" 
                                    runat="server" 
                                    TabName="<%$ GlobalFunctions:PARAMETER_TAB_ASSOCIATION %>"
                                    TabPageControlId="pnlAssociations" 
                                    TabPageIcon="fas fa-link"
                                    />
							</TabPages>
						</tstsc:TabControl>
                        <asp:Panel ID="pnlOverview" runat="server" CssClass="TabControlPanel">
                            <div class="u-wrapper width_md">


         
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
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true" 
                                                    AssociatedControlID="ddlRelease" 
                                                    ID="ddlReleaseLabel" 
                                                    Required="false" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,ReleaseId %>" 
                                                    />
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
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true" 
                                                    AssociatedControlID="lblCreationDate" 
                                                    ID="lblCreationDateLabel" 
                                                    Required="false" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,CreationDate %>" 
                                                    />
										        <asp:Label 
                                                    ID="lblCreationDate" 
                                                    runat="server" 
                                                    />

                                            </li>
                                            <li class="ma0 mb2 pa0">
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
                                                    Authorized_ArtifactType="Requirement" 
                                                    Authorized_Permission="Modify"
                                                    ID="txtDescription" 
                                                    runat="server"
                                                    Screenshot_ArtifactType="Requirement" 
                                                    />
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                



                                <%-- SCENARIO PANEL - only shown when the type is set to use case - ID on the wrapper DIV required to enable this --%>
                                <div 
                                    class="u-box_3"
                                    id="divScenarioPanel"
                                    style="display: none;"
                                    >
                                    <div 
                                        class="u-box_group u-cke_is-minimal"
                                        data-collapsible="true"
                                        id="form-group_scenarios" >
                                        <div 
                                            class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                            aria-expanded="true">
                                            <asp:Localize 
                                                runat="server" 
                                                Text="<%$Resources:ServerControls,TabControl_Scenario %>" />
                                            <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                        </div>
                                        <ul class="u-box_list" >
                                            <li class="ma0 mb2 pa0">
                                                <div class="TabControlHeader">
										            <div class="btn-group priority4">
											            <tstsc:HyperLinkEx 
                                                            ID="lnkInsertStep" 
                                                            SkinID="ButtonDefault" 
                                                            runat="server" 
                                                            NavigateUrl="javascript:void(0)" 
                                                            ClientScriptServerControlId="grdScenarioSteps" 
                                                            ClientScriptMethod="insert_item('Step')" 
                                                            >
                                                            <span class="fas fa-plus"></span>
                                                            <asp:Localize runat="server" Text="<%$Resources:Dialogs,TestCaseDetails_InsertStep %>" />
											            </tstsc:HyperLinkEx>
											            <tstsc:HyperLinkEx 
                                                            ID="lnkDeleteStep" 
                                                            SkinID="ButtonDefault" 
                                                            runat="server" 
                                                            NavigateUrl="javascript:void(0)" 
                                                            ClientScriptServerControlId="grdScenarioSteps" 
                                                            ClientScriptMethod="delete_items()" 
                                                            Confirmation="true" 
                                                            ConfirmationMessage="<%$Resources:Messages,ReleaseDetails_ScenarioStepDeleteConfirm %>"
                                                            >
                                                            <span class="fas fa-trash-alt"></span>
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
											            </tstsc:HyperLinkEx>
											            <tstsc:HyperLinkEx 
                                                            ID="lnkCopyStep" 
                                                            SkinID="ButtonDefault" 
                                                            runat="server" 
                                                            NavigateUrl="javascript:void(0)" 
                                                            ClientScriptServerControlId="grdScenarioSteps"
                                                            ClientScriptMethod="copy_items()"
                                                            >
                                                            <span class="far fa-clone"></span>
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Clone %>" />
											            </tstsc:HyperLinkEx>
											        </div>
                                                    <div class="btn-group priority1">
											            <tstsc:HyperLinkEx ID="lnkRefreshSteps" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdScenarioSteps" ClientScriptMethod="load_data()">
                                                            <span class="fas fa-sync"></span>
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>" />
											            </tstsc:HyperLinkEx>
										            </div>
									            </div>
									            <tstsc:MessageBox ID="lblScenarioMessages" runat="server" SkinID="MessageBox" />
									            <tstsc:OrderedGrid 
                                                    AllowInlineEditing="true"
                                                    AlternateItemHasHyperlink="false" 
                                                    AlternateItemImage="artifact-UseCaseStep.svg"
                                                    Authorized_ArtifactType="Requirement" 
                                                    Authorized_Permission="Modify" 
                                                    AutoLoad="false" 
                                                    ConcurrencyEnabled="true" 
                                                    CssClass="DataGrid DataGrid-no-bands" 
                                                    EditRowCssClass="Editing" 
                                                    ErrorMessageControlId="lblScenarioMessages"
                                                    HeaderCssClass="SubHeader" 
                                                    ID="grdScenarioSteps" 
								                    RowCssClass="Normal" 
                                                    runat="server" 
                                                    SelectedRowCssClass="Highlighted" 
								                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementStepService" 
                                                    >
										            <ContextMenuItems>
                                                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                                                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Requirement" Authorized_Permission="View" />
                                                        <tstsc:ContextMenuItem Divider="True" />
											            <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Buttons,Insert%>" CommandName="insert_item" CommandArgument="Step" Authorized_ArtifactType="Requirement" Authorized_Permission="Modify" />
                                                        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Requirement" Authorized_Permission="Modify" />
											            <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-copy" Caption="<%$Resources:Buttons,CopyItems%>" CommandName="copy_items" Authorized_ArtifactType="Requirement" Authorized_Permission="Modify" />
                                                        <tstsc:ContextMenuItem Divider="True" />
											            <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="Requirement" Authorized_Permission="Modify" ConfirmationMessage="<%$Resources:Messages,ReleaseDetails_ScenarioStepDeleteConfirm %>" />
										            </ContextMenuItems>
									            </tstsc:OrderedGrid>
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
                                                    ArtifactType="Requirement" 
                                                    AutoLoad="false" 
                                                    ErrorMessageControlId="lblMessage"
                                                    ID="lstComments" 
                                                    runat="server" 
									                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService" 
                                                    Width="100%" 
                                                    />
								                <tstsc:RichTextBoxJ 
                                                    Authorized_ArtifactType="Requirement"
									                Authorized_Permission="Modify" 
                                                    Height="80px" 
                                                    ID="txtNewComment" 
                                                    runat="server" 
                                                    Screenshot_ArtifactType="Requirement" 
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
                            </div>

                        </asp:Panel>




                        <%-- OTHER TABS --%>
                        <asp:Panel ID="pnlTasks" runat="server" CssClass="TabControlPanel">
							<tstuc:TaskListPanel ID="tstTaskListPanel" runat="server" />
						</asp:Panel>
                        <asp:Panel ID="pnlCoverage" runat="server" CssClass="TabControlPanel">
							<tstuc:AssociationsPanel ID="tstCoveragePanel" runat="server" />
						</asp:Panel>
						<asp:Panel ID="pnlAttachments" runat="server" CssClass="TabControlPanel">
							<tstuc:AttachmentPanel ID="tstAttachmentPanel" runat="server" />
							<br />
						</asp:Panel>
						<asp:Panel ID="pnlHistory" runat="server" CssClass="TabControlPanel">
							<tstuc:HistoryPanel ID="tstHistoryPanel" runat="server" ShowTestStepData="true" />
							<br />
						</asp:Panel>
                        <asp:Panel ID="pnlAssociations" runat="server" CssClass="TabControlPanel">
							<tstuc:AssociationsPanel ID="tstAssociationPanel" runat="server" />
						</asp:Panel>
                        <asp:Panel ID="pnlDiagram" runat="server" CssClass="TabControlPanel">
							<tstuc:DiagramPanel ID="tstDiagramPanel" runat="server" />
                        </asp:Panel>
                    </div>
        </div>
	</div>
    <tstsc:DialogBoxPanel ID="dlgSplitRequirement" runat="server" Title="<%$Resources:Main,RequirementDetails_SplitRequirement %>"
        CssClass="PopupPanel" Width="500px" Height="300px" Modal="true" Top="100px" Left="300px">
        <p>
            <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,RequirementDetails_SplitRequirement_Intro %>" />
        </p>
        <div class="u-wrapper clearfix">
            <ul class="u-box_list" >
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtSplitRequirementName" 
                        ID="txtSplitRequirementNameLabel" 
                        Required="true"
                        runat="server" 
                        Text="<%$Resources:Fields,Name %>"
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input"
                        DisabledCssClass="u-input disabled"
                        ID="txtSplitRequirementName" 
                        MaxLength="255" 
                        runat="server" 
                        />
                </li>
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true"
                        AssociatedControlID="ddlSplitRequirementOwner" 
                        ID="ddlSplitRequirementOwnerLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,OwnerId %>"
                        />
                    <tstsc:UnityDropDownUserList 
                        CssClass="u-dropdown u-dropdown_user" 
                        DataTextField="FullName" 
                        DataValueField="UserId"
                        DisabledCssClass="u-dropdown u-dropdown_user disabled"
                        ID="ddlSplitRequirementOwner" 
                        NoValueItem="true" 
                        NoValueItemText="<%$Resources:Dialogs,RequirementDetails_SameAsCurrentRequirement %>" 
                        runat="server" 
                        />
                </li>
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtSplitRequirementEstimate" 
                        ID="txtSplitRequirementEstimateLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,Estimate %>"
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input w7"
                        DisabledCssClass="u-input disabled"
                        ID="txtSplitRequirementEstimate" 
                        runat="server" 
                        MaxLength="3"
                        type="text"
                        />
                </li>
                <li class="ma0 mb2 pa0">
                    <tstsc:LabelEx 
                        AppendColon="true" 
                        AssociatedControlID="txtSplitRequirementComment" 
                        ID="txtSplitRequirementCommentLabel" 
                        Required="false" 
                        runat="server" 
                        Text="<%$Resources:Fields,Comment %>"
                        />
                    <tstsc:UnityTextBoxEx 
                        CssClass="u-input"
                        DisabledCssClass="u-input disabled"
                        ID="txtSplitRequirementComment" 
                        runat="server" 
                        MaxLength="255" 
                        />
                </li>
            </ul>
        </div>
        <p class="ml2 fs-90 gray">
            <asp:Localize runat="server" Text="<%$Resources:Main,RequirementDetails_SplitRequirement_Notes %>" />
        </p>
        <div class="btn-group ml2">
            <tstsc:ButtonEx ID="btnSplit" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,Split %>" runat="server"
                ClientScriptMethod="dlgSplitRequirement_splitRequirement()" />
            <tstsc:ButtonEx ID="btnSplitCancel" Text="<%$Resources:Buttons,Cancel %>" runat="server"
                ClientScriptServerControlId="dlgSplitRequirement" ClientScriptMethod="close()" />
        </div>
    </tstsc:DialogBoxPanel>

    <tstsc:AjaxFormManager ID="ajxFormManager" runat="server" ErrorMessageControlId="lblMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService" ArtifactImageControlId="imgRequirement"
        ArtifactTypeName="<%$Resources:Fields,Requirement%>" WorkflowEnabled="true" CheckUnsaved="true" RevertButtonControlId="btnRevert" WorkflowOperationsControlId="ajxWorkflowOperations"
        DisplayPageName="true" NameField="Name" FolderPathControlId="pnlFolderPath"
        ItemImage="Images/artifact-Requirement.svg" SummaryItemImage="Images/artifact-RequirementSummary.svg" AlternateItemImage="Images/artifact-UseCase.svg">
		<ControlReferences>
        	<tstsc:AjaxFormControl ControlId="lblRequirementId" DataField="RequirementId" Direction="In" />
        	<tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlImportance" DataField="ImportanceId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlAuthor" DataField="AuthorId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="lblRequirementStatusValue" DataField="RequirementStatusId" Direction="In" IsWorkflowStep="true" />
			<tstsc:AjaxFormControl ControlId="ddlType" DataField="RequirementTypeId" Direction="InOut" ChangesWorkflow="true" />
            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="txtEstimatePoints" DataField="EstimatePoints" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="lblEstimatedEffort" DataField="EstimatedEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblTaskEstimatedEffort" DataField="TaskEstimatedEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblTaskProjectedEffort" DataField="TaskProjectedEffort" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
		</ControlReferences>
		<SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
		</SaveButtons>
	</tstsc:AjaxFormManager>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/RequirementStepService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/Followers.js" />
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;

        SpiraContext.pageId = "Inflectra.Spira.Web.RequirementDetails";
        SpiraContext.ArtifactId = <%=requirementId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=requirementId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
        SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Requirement%>;
        SpiraContext.EmailEnabled = <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.EmailSettings_Enabled.ToString().ToLowerInvariant()%>;
        SpiraContext.HasCollapsiblePanels = true;
        SpiraContext.Mode = 'update';

        //Server Control IDs
        var btnSubscribe_id = '<%=this.btnSubscribe.ClientID%>';
        var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
        var lstComments_id = '<%=this.lstComments.ClientID %>';
        var btnNewComment_id = '<%=this.btnNewComment.ClientID%>';
        var txtName_id = '<%=txtName.ClientID%>';
        var btnSave_id = '<%=btnSave.ClientID%>';
        var grdScenarioSteps_id = '<%=grdScenarioSteps.ClientID%>';
        var tabControl_id = '<%=this.tclRequirementDetails.ClientID%>';
        var navigationBar_id = '<%=this.navRequirementsList.ClientID%>';
        var btnEmail_id = '<%=btnEmail.ClientID%>';
        var lnkStepInsert_id = '<%=lnkInsertStep.ClientID%>';
        var lnkStepDelete_id = '<%=lnkDeleteStep.ClientID%>';
        var lnkStepCopy_id = '<%=lnkCopyStep.ClientID%>';

        //TabControl Panel IDs
        var pnlOverview_id = '<%=pnlOverview.ClientID%>';
        var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
        var pnlHistory_id = '<%=pnlHistory.ClientID%>';
        var pnlCoverage_id = '<%=pnlCoverage.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';
        var pnlTasks_id = '<%=pnlTasks.ClientID%>';
        var pnlDiagram_id = '<%=pnlDiagram.ClientID%>';

        //Base URLs
        var urlTemplate_artifactRedirectUrl = '<%=RequirementRedirectUrl %>';
        var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToRequirementsListUrl) %>';
        var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
        var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

        //Set any page properties
        var pageProps = {};
        pageProps.includeStepHistory = true;

        function ajxWorkflowOperations_operationExecuted(transitionId, isStatusOpen)
        {
            //Put any post-workflow operations here
        }
        function ajxFormManager_operationReverted(statusId, isStatusOpen)
        {
            //Put any post-revert operations here
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
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RequirementSummary%>";
                filter = "&af_2_85=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RequirementDetailed%>";
                filter = "&af_3_85=";
            }

            //Open the report for the specified format
            globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
        }

        //Updates any page specific content
        var requirementDetails_requirementId = -1;
        function updatePageContent()
        {
            var ajxFormManager = $find(ajxFormManager_id);
            if (ajxFormManager && ajxFormManager.get_dataItem())
            {
                //Hide the scenario unless this is a use-case
                var dataItem = ajxFormManager.get_dataItem();
                var requirementHasSteps = dataItem.alternate;
                var displayValue = requirementHasSteps ? '' : 'none';
                $('#divScenarioPanel').css('display', displayValue);
            }

            //see if we have permissions to edit steps - required due to modify owned case
            var isDisabled = false;
            var ajxFormManager = $find(ajxFormManager_id);
            var isCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
            var authorizedViewTS = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, globalFunctions.artifactTypeEnum.requirement);
            if (authorizedViewTS == globalFunctions.authorizationStateEnum.prohibited) {
                isDisabled = true;
            }
            if (authorizedViewTS == globalFunctions.authorizationStateEnum.limited && !isCreatorOrOwner) {
                isDisabled = true;
            }

            if (isDisabled) {
                document.getElementById(lnkStepInsert_id).setAttribute("disabled", "disabled");
                document.getElementById(lnkStepDelete_id).setAttribute("disabled", "disabled");
                document.getElementById(lnkStepCopy_id).setAttribute("disabled", "disabled");
            }
            else {
                document.getElementById(lnkStepInsert_id).removeAttribute("disabled");
                document.getElementById(lnkStepDelete_id).removeAttribute("disabled");
                document.getElementById(lnkStepCopy_id).removeAttribute("disabled");
            }

            //See if the artifact id has changed
            var grdScenarioSteps = $find('<%=this.grdScenarioSteps.ClientID%>');
            if (requirementDetails_requirementId != SpiraContext.ArtifactId)
            {
                // enable/disable editing of the grid itself
                grdScenarioSteps.set_allowEdit(!isDisabled);

                //Load the scenario steps
                var filters = {};
                filters[globalFunctions.keyPrefix + 'RequirementId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                grdScenarioSteps.set_standardFilters(filters);
                grdScenarioSteps.load_data();
                requirementDetails_requirementId = SpiraContext.ArtifactId;
            }

        }

        function displaySplitDialog(evt)
        {
            var dlgSplitRequirement = $find('<%=dlgSplitRequirement.ClientID %>');
            $get('<%=txtSplitRequirementName.ClientID %>').value = $get('<%=txtName.ClientID %>').value;
            dlgSplitRequirement.display(evt);
        }
        function dlgSplitRequirement_splitRequirement()
        {
            var projectId = SpiraContext.ProjectId;
            var requirementId = SpiraContext.ArtifactId;
            var txtSplitRequirementName = $get('<%=txtSplitRequirementName.ClientID %>');
            var ddlSplitRequirementOwner = $find('<%=ddlSplitRequirementOwner.ClientID %>');
            var txtSplitRequirementEstimate = $get('<%=txtSplitRequirementEstimate.ClientID %>');
            var txtSplitRequirementComment = $get('<%=txtSplitRequirementComment.ClientID %>');

            //Make sure that we have valid data
            if (!txtSplitRequirementName.value || txtSplitRequirementName.value == '')
            {
                alert (resx.RequirementDetails_SplitRequirementNameRequired);
                return;
            }

            var estimatePoints = null;
            if (txtSplitRequirementEstimate.value && txtSplitRequirementEstimate.value != '')
            {
                console.debug(parseFloat(txtSplitRequirementEstimate.value));
                if (isNaN(parseFloat(txtSplitRequirementEstimate.value)) || parseFloat(txtSplitRequirementEstimate.value) < 0)
                {
                    alert (resx.RequirementDetails_SplitRequirementEstimateNotNumeric);
                    return;
                }
                estimatePoints = parseFloat(txtSplitRequirementEstimate.value);
            }

            //Now actually perform the split
            var name = txtSplitRequirementName.value;
            var ownerId = null;
            if (ddlSplitRequirementOwner.get_selectedItem() && ddlSplitRequirementOwner.get_selectedItem().get_value() && ddlSplitRequirementOwner.get_selectedItem().get_value() != '')
            {
                ownerId = ddlSplitRequirementOwner.get_selectedItem().get_value();
            }
            var comment = txtSplitRequirementComment.value;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService.Requirement_Split(projectId, requirementId, name, estimatePoints, ownerId, comment, dlgSplitRequirement_splitRequirement_success, dlgSplitRequirement_splitRequirement_failure);
        }
        function dlgSplitRequirement_splitRequirement_success(newReqId)
        {
            globalFunctions.hide_spinner();
            $get('<%=txtSplitRequirementEstimate.ClientID %>').value = '';
            $get('<%=txtSplitRequirementComment.ClientID %>').value = '';
            var dlgSplitRequirement = $find('<%=dlgSplitRequirement.ClientID %>');
            dlgSplitRequirement.close();

            //Redirect to the new task
            var baseUrl = '<%=UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Requirements, ProjectId, -2)) %>';
            var newUrl = baseUrl.replace(globalFunctions.artifactIdToken, newReqId);
            window.location = newUrl;
        }
        function dlgSplitRequirement_splitRequirement_failure(ex)
        {
            globalFunctions.hide_spinner();
            //display the message
            globalFunctions.display_error(null, ex);
        }
        function tclRequirementDetails_selectedTabChanged(tabPage)
        {
            //See if the tab is visible
            var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlDiagram_id);

            //Reload the tab's data
            if (tstucDiagramPanel && tstucDiagramPanel.load_data)
            {
                tstucDiagramPanel.load_data(null, loadNow);
            }
        }
    </script>
</asp:Content>
