<%@ Page 
    AutoEventWireup="True" 
    CodeBehind="TestCaseDetails.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.TestCaseDetails" 
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    ValidateRequest="false" 
%>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Assembly Name="System.Data.Entity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" %>
<%@ Import Namespace="System.Data.EntityClient" %>

<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestRunListPanel" Src="UserControls/TestRunListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentListPanel" Src="UserControls/IncidentListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestSetListPanel" Src="UserControls/TestSetListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="SignaturePanel" Src="UserControls/SignaturePanel.ascx" %>
 


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
                        Authorized_ArtifactType="TestCase" 
                        Authorized_Permission="Modify" 
                        ClientScriptMethod="refresh_data();" 
                        ClientScriptServerControlId="navTestCaseList" 
                        CssClass="FolderTree" 
                        EditDescriptions="false"
                        ErrorMessageControlId="lblMessage"
                        ID="trvFolders" 
                        ItemName="<%$Resources:Fields,Folder %>"
                        LoadingImageUrl="Images/action-Spinner.svg" 
                        NodeLegendFormat="{0}" 
                        runat="server" 
                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService"
                        />
                </div>    
            </tstsc:SidebarPanel>
            <tstsc:NavigationBar 
                AlternateItemImage="Images/artifact-TestCase.svg" 
                AutoLoad="true" 
                BodyHeight="580px" 
                ErrorMessageControlId="lblMessage"
                ID="navTestCaseList" 
                ItemImage="Images/artifact-TestCaseNoSteps.svg"
                ListScreenCaption="<%$Resources:Main,TestCaseDetails_BackToList%>"
                runat="server" 
                EnableLiveLoading="true" 
                FormManagerControlId="ajxFormManager"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" 
                />
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="clearfix">
                    <div class="btn-group priority1 hidden-md hidden-lg mr3"
                            role="group">
                        <tstsc:HyperLinkEx 
                            ID="btnBack" 
                            NavigateUrl="<%#ReturnToTestCaseListUrl%>" 
                            runat="server" 
                            SkinID="ButtonDefault" 
                            ToolTip="<%$Resources:Main,TestCaseDetails_BackToList%>"
                            >
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperLinkEx>
					            
                    </div>

                    <div class="btn-group priority1" role="group">
                        <tstsc:DropMenu 
                            ClientScriptMethod="save_data(evt)"
                            ClientScriptServerControlId="ajxFormManager" 
                            GlyphIconCssClass="mr3 fas fa-save"
                            ID="btnSave" 
                            MenuWidth="125px"
                            runat="server" 
                            Text="<%$Resources:Buttons,Save %>" 
                            >
                            <DropMenuItems>
                                <tstsc:DropMenuItem 
                                    ClientScriptMethod="save_data(null); void(0);" 
                                    GlyphIconCssClass="mr3 fas fa-save"
                                    ID="DropMenuItem1" 
                                    Name="Save" 
                                    runat="server" 
                                    Value="<%$Resources:Buttons,Save %>" 
                                    />
                                <tstsc:DropMenuItem 
                                    ClientScriptMethod="save_data(null, 'close'); void(0);" 
                                    GlyphIconCssClass="mr3 far fa-file-excel"
                                    ID="DropMenuItem2" 
                                    Name="SaveAndClose" 
                                    runat="server" 
                                    Value="<%$Resources:Buttons,SaveAndClose %>" 
                                    />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestCase" 
                                    Authorized_Permission="Create" 
                                    ClientScriptMethod="save_data(null, 'new'); void(0);" 
                                    GlyphIconCssClass="mr3 far fa-copy"
                                    ID="DropMenuItem3" 
                                    Name="SaveAndNew" 
                                    runat="server" 
                                    Value="<%$Resources:Buttons,SaveAndNew %>" 
                                    />
                            </DropMenuItems>
                        </tstsc:DropMenu>
                        <tstsc:DropMenu 
                            ID="btnRefresh" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Refresh %>"
                            GlyphIconCssClass="mr3 fas fa-sync" 
                            ClientScriptServerControlId="ajxFormManager"
                            ClientScriptMethod="load_data()" 
                            />
    					<tstsc:DropMenu 
                            Authorized_ArtifactType="TestCase" 
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
                                <tstsc:DropMenuItem 
                                    Name="New" 
                                    runat="server"
                                    Value="<%$Resources:Buttons,New %>" 
                                    GlyphIconCssClass="mr3 fas fa-plus" 
                                    ClientScriptMethod="create_item()" 
                                    Authorized_Permission="Create" 
                                    Authorized_ArtifactType="TestCase" 
                                    />
                                <tstsc:DropMenuItem 
                                    Name="Clone" 
                                    runat="server"
                                    Value="<%$Resources:Buttons,Clone %>" 
                                    GlyphIconCssClass="mr3 far fa-clone" 
                                    ClientScriptMethod="clone_item()" 
                                    Authorized_Permission="Create" 
                                    Authorized_ArtifactType="TestCase" 
                                    />
                            </DropMenuItems>
						</tstsc:DropMenu>
                    </div>
                    <div class="btn-group priority3" role="group">
                        <tstsc:DropMenu 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="Delete" 
                            Confirmation="True" 
                            ConfirmationMessage="<%$Resources:Messages,TestCaseDetails_DeleteConfirm %>"
                            GlyphIconCssClass="mr3 fas fa-trash-alt" 
                            ID="btnDelete" 
                            runat="server" 
                            SkinID="HideOnScroll" 
                            Text="<%$Resources:Buttons,Delete_TestCase %>" 
                            ClientScriptMethod="delete_item()" 
                            ClientScriptServerControlId="ajxFormManager" 
                            />
                    </div>
                    <div class="btn-group priority1" role="group">
                        <tstsc:DropMenu 
                            Authorized_ArtifactType="TestRun"
                            Authorized_Permission="Create" 
                            ClientScriptMethod="page.execute_test_case()" 
                            Confirmation="False" 
                            GlyphIconCssClass="mr3 fas fa-play" 
                            ID="btnExecuteTest" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Execute %>"
                            />
                        <tstsc:DropMenu 
                            GlyphIconCssClass="mr3 fas fa-cog"
                            ID="btnTools" 
                            MenuCssClass="DropMenu" 
                            PostBackOnClick="false"
                            runat="server" 
                            Text="<%$Resources:Buttons,Tools %>" 
                            >
                            <DropMenuItems>
                                <tstsc:DropMenuItem 
                                    Name="Print" 
                                    runat="server"
                                    Value="<%$Resources:Dialogs,Global_PrintItems %>" 
                                    GlyphIconCssClass="mr3 fas fa-print" 
                                    ClientScriptMethod="print_item('html')" 
                                    Authorized_ArtifactType="TestCase" 
                                    Authorized_Permission="View" 
                                    />
                                <tstsc:DropMenuItem Divider="true" runat="server"/>
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestCase" 
                                    Authorized_Permission="View" 
                                    runat="server"
                                    ClientScriptMethod="print_item('excel')" 
                                    ImageUrl="Images/Filetypes/Excel.svg" 
                                    Name="ExportToExcel" 
                                    Value="<%$Resources:Dialogs,Global_ExportToExcel %>" 
                                    />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestCase" 
                                    Authorized_Permission="View" 
                                    runat="server"
                                    ClientScriptMethod="print_item('word')" 
                                    ImageUrl="Images/Filetypes/Word.svg" 
                                    Name="ExportToWord" 
                                    Value="<%$Resources:Dialogs,Global_ExportToWord %>" 
                                    />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestCase" 
                                    Authorized_Permission="View" 
                                    runat="server"
                                    ClientScriptMethod="print_item('pdf')" 
                                    ImageUrl="Images/Filetypes/Acrobat.svg" 
                                    Name="ExportToPdf" 
                                    Value="<%$Resources:Dialogs,Global_ExportToPdf %>" 
                                    />
                            </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                    <div class="btn-group priority2" role="group" id="pnlEmailToolButtons">
                        <tstsc:DropMenu 
                            ClientScriptMethod="ArtifactEmail_pnlSendEmail_display(evt)"
                            Confirmation="false" 
                            GlyphIconCssClass="mr3 far fa-envelope" 
                            ID="btnEmail"
                            data-requires-email="true" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Email %>"
                            />
                        <tstsc:DropMenu 
                            ClientScriptMethod="ArtifactEmail_subscribeArtifactChange(this)" 
                            Confirmation="false" 
                            GlyphIconCssClass="mr3 far fa-star" 
                            ID="btnSubscribe" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Subscribe %>">
                            <DropMenuItems>
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestCase"
                                    runat="server"
                                    Authorized_Permission="Modify"
                                    ClientScriptMethod="ArtifactAddFollower_pnlAddFollower_display()" 
                                    Name="AddFollower" 
                                    GlyphIconCssClass="mr3 fas fa-user" 
                                    Value="<%$Resources:Buttons,AddFollower %>" 
                                    />
			                </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                    <div id="cplMainContent_btnSubscribe" class="btn btn-default">
                        <span class="fas fa-dot-circle recordicon"></span>
                        <asp:HyperLink role="group" id="btnRecord" runat="server" CssClass="record" Text="Record" />
                    </div>
                    <div id="plcWorX" class="dn">
                        <div class="btn-group ml2" role="group">
                            <tstsc:DropMenu ID="mnuWorX" runat="server" Text="WorX" MenuWidth="100px">
                                <DropMenuItems>
                                    <tstsc:DropMenuItem Name="Open" Value="Open" NavigateUrl="javascript:void()" />
                                    <tstsc:DropMenuItem Name="Execute" Value="Execute" NavigateUrl="javascript:void()" />
                                    <tstsc:DropMenuItem Name="Review" Value="Review" NavigateUrl="javascript:void()" />
                                </DropMenuItems>
                            </tstsc:DropMenu>
                        </div>
                    </div>

                    
                </div>





                <asp:Panel 
                    ID="pnlFolderPath" 
                    runat="server" 
                    />




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
                    <div class="py2 px3 mb2 bg-near-white br2 dif items-center flex">
                        <div class="py1 pr4 py1 dif items-center ma0-children fs-h4 fs-h6-xs">
                            <tstsc:ImageEx 
                                AlternateText="<%$Resources:Fields,TestCase %>" 
                                CssClass="w5 h5" 
                                ID="imgTestCase" 
                                ImageUrl="Images/artifact-TestCase.svg" 
                                runat="server" 
                                />
                            <span class="pl4 silver nowrap">
                                <tstsc:LabelEx 
                                    CssClass="pointer dib orange-hover transition-all"
                                    title="<%$Resources:Buttons,CopyToClipboard %>"
                                    data-copytoclipboard="true"
                                    ID="lblTestCaseId" 
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
                                Text="<%$Resources:Fields,TestCaseTypeId %>"
                                />
							<tstsc:UnityDropDownListEx 
                                ActiveItemField="IsActive" 
                                CssClass="u-dropdown"
                                DataTextField="Name"
                                DataValueField="TestCaseTypeId"
                                DisabledCssClass="u-dropdown disabled"
                                ID="ddlType"
                                runat="server"
                                Width="250"
                                />
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxWorkflowOperations"
                                ID="ddlStatusLabel" 
                                Required="True" 
                                runat="server" 
                                Text="<%$Resources:Fields,IncidentStatusId %>" 
                                />
                            <div class="dib v-mid-children dif flex-wrap items-center pl3">
								<tstsc:LabelEx 
                                    ID="lblTestCaseStatusValue" 
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
						                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" 
                                        />
                                </div>
                            </div>
                        </div>
                        <div class="py1 dif items-center ma0-children fs-h6">
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
                <tstsc:MessageBox 
                    ID="lblMessage" 
                    runat="server" 
                    SkinID="MessageBox"
                    />
            </div>

            <tstuc:ArtifactEmail 
                ArtifactId="<%# this.testCaseId %>"
                ArtifactTypeEnum="TestCase" 
                ID="tstEmailPanel" 
                runat="server" 
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="TestCase" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />



            <div class="main-content">
                <tstsc:TabControl ID="tclTestCaseDetails" TabWidth="90" runat="server">
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
                            AuthorizedArtifactType="Requirement" 
                            Caption="<% $Resources:ServerControls,TabControl_ReqCoverage %>"
                            CheckPermissions="true" 
                            ID="tabCoverage" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_COVERAGE %>"
                            TabPageControlId="pnlRequirements"
                            TabPageImageUrl="Images/artifact-Requirement.svg" 
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstTestRunListPanel" 
                            AjaxServerControlId="grdTestRunList" 
                            AuthorizedArtifactType="TestRun"
                            Caption="<% $Resources:ServerControls,TabControl_TestRuns %>"
                            CheckPermissions="true" 
                            ID="tabTestRuns" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_TESTRUN %>"
                            TabPageControlId="pnlTestRuns" 
                            TabPageImageUrl="Images/artifact-TestRun.svg"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstReleaseMappingPanel" 
                            AjaxServerControlId="grdAssociationLinks" 
                            AuthorizedArtifactType="Release"
                            Caption="<% $Resources:ServerControls,TabControl_Releases %>"
                            CheckPermissions="true" 
                            ID="tabReleases" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_RELEASE %>"
                            TabPageControlId="pnlReleases" 
                            TabPageImageUrl="Images/artifact-Release.svg"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstIncidentListPanel" 
                            AjaxServerControlId="grdIncidentList" 
                            AuthorizedArtifactType="Incident"
                            Caption="<% $Resources:ServerControls,TabControl_Incidents %>"
                            CheckPermissions="true" 
                            ID="tabIncidents" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_INCIDENT %>"
                            TabPageControlId="pnlIncidents" 
                            TabPageImageUrl="Images/artifact-Incident.svg"
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
                            AjaxControlContainer="tstTestSetListPanel" 
                            AjaxServerControlId="grdTestSetList" 
                            AuthorizedArtifactType="TestSet" 
                            Caption="<% $Resources:ServerControls,TabControl_TestSets %>"
                            CheckPermissions="true" 
                            ID="tabTestSets" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_TESTSET %>"
                            TabPageControlId="pnlTestSets" 
                            TabPageImageUrl="Images/artifact-TestSet.svg"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstAssociationPanel"
                            AjaxServerControlId="grdAssociationLinks"
                            AuthorizedArtifactType="Task" 
                            Caption="<%$Resources:ServerControls,TabControl_Associations %>" 
                            CheckPermissions="true"
                            ID="tabAssociations"
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_ASSOCIATION %>"
                            TabPageControlId="pnlAssociations" 
                            TabPageIcon="fas fa-link"
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
                            AjaxControlContainer="tstSignaturePanel" 
                            AjaxServerControlId="grdSignaturesList" 
                            Caption="Signatures"
                            ID="tabSignatures" 
                            runat="server" 
                            TabName="Signatures"
                            TabPageControlId="pnlSignature" 
                            TabPageIcon="fas fa-history"
                            />
                    </TabPages>
                </tstsc:TabControl>

                <asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlOverview" 
                    runat="server" 
                    >
                    <div class="u-wrapper width_md">
                        <div class="u-box_1">
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
                                            Width="200" 
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
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server" 
                                            Width="200" 
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>
                        <div class="u-box_1">
                            <div 
                                class="u-box_group"
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
                                    <li class="ma0 mb2 pa0">
										<tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="ddlComponent"
                                            ID="ddlComponentLabel"
                                            runat="server"
                                            Text="<%$Resources:Fields,ComponentId %>"
                                            />
										<tstsc:UnityDropDownMultiList 
                                            ActiveItemField="IsActive"
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="ComponentId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlComponent"
                                            NoValueItem="True"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server"
                                            SelectionMode="Multiple" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlPriority"
                                            ID="ddlPriorityLabel"
                                            Required="false"
                                            runat="server"
                                            Text="<%$Resources:Fields,TestCasePriorityId %>"
                                            />
                                        <tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown" 
                                            DataTextField="Name"
                                            DataValueField="TestCasePriorityId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlPriority"
                                            NoValueItem="True"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server"
                                            Width="200"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="chkIsSuspect"
                                            ID="chkIsSuspectLabel"
                                            Required="false"
                                            runat="server"
                                            Text="<%$Resources:Fields,IsSuspect %>"
                                            />
                                        <tstsc:CheckBoxYnEx 
                                            ID="chkIsSuspect"
                                            runat="server"
                                            />
                                    </li>
                                </ul>
                            </div>
                        </div>
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
                                            AssociatedControlID="txtEstimatedDuration"
                                            ID="lblEstimatedDuration" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,EstimatedDurationWithHours %>" 
                                            />
                                        <tstsc:UnityTextBoxEx 
                                            CssClass="u-input w7"
                                            ID="txtEstimatedDuration" 
                                            runat="server" 
                                            MaxLength="9"
                                            type="text"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
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
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lblExecutionDate"
                                            ID="lblExecutionDateLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ExecutionDate %>" 
                                            />
                                        <tstsc:LabelEx
                                            ID="lblExecutionDate" 
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
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="u-wrapper width_md">
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="form-group_rte" 
                                >
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
                                            Authorized_ArtifactType="TestCase" 
                                            Authorized_Permission="Modify"
                                            ID="txtDescription" 
                                            runat="server" 
                                            Screenshot_ArtifactType="TestCase"
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





                        <%-- TEST STEPS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="formGroup_testSteps" 
                                runat="server"
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_TestSteps %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <div class="u-box_item mb2" >
                                    <asp:Panel runat="server" ID="pnlOverview_TestSteps">
                                            <div class="TabControlHeader w-100">
                                                <div class="btn-group priority2">
											        <tstsc:DropMenu 
                                                        Authorized_ArtifactType="TestStep" 
                                                        Authorized_Permission="Create" 
                                                        ClientScriptMethod="insert_item('Step')"
                                                        ClientScriptServerControlId="grdTestSteps" 
                                                        ID="lnkInsertStep" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 fas fa-plus"
                                                        Text="<%$Resources:Dialogs,TestCaseDetails_InsertStep %>"
                                                        />
											        <tstsc:DropMenu 
                                                        Authorized_ArtifactType="TestStep" 
                                                        Authorized_Permission="Create" 
                                                        ClientScriptMethod="page.lnkInsertLink_click(evt)"
                                                        ID="lnkInsertLink" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 fas fa-link"
                                                        Text="<%$Resources:Dialogs,TestCaseDetails_InsertLink %>"
                                                        />
                                                </div>
                                                <div class="btn-group priority1">
											        <tstsc:DropMenu 
                                                        ClientScriptMethod="load_data()" 
                                                        ClientScriptServerControlId="grdTestSteps" 
                                                        ID="lnkRefreshSteps" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 fas fa-sync"
                                                        Text="<%$Resources:Buttons,Refresh %>"
                                                        />
                                                    <tstsc:DropMenu 
                                                        Authorized_ArtifactType="TestStep"
                                                        Authorized_Permission="Create" 
                                                        ClientScriptMethod="copy_items()" 
                                                        ClientScriptServerControlId="grdTestSteps" 
                                                        ID="lnkCopyStep" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 far fa-clone"
                                                        Text="<%$Resources:Buttons,Clone %>"
                                                        />
                                                    <tstsc:DropMenu 
                                                        Authorized_ArtifactType="TestStep"
                                                        Authorized_Permission="Modify" 
                                                        ClientScriptMethod="page.displayImportStepsDialog()" 
                                                        ID="lnkImportSteps" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 fas fa-sign-in-alt"
                                                        Text="<%$Resources:Buttons,Import %>"
                                                        />
                                                </div>
                                                <div class="btn-group priority3">
											        <tstsc:DropMenu 
                                                        Authorized_ArtifactType="TestStep"
                                                        Authorized_Permission="Delete" 
                                                        ClientScriptMethod="delete_items()" 
                                                        ClientScriptServerControlId="grdTestSteps" 
                                                        Confirmation="true"
                                                        ConfirmationMessage="<%$Resources:Messages,TestCaseDetails_TestStepDeleteConfirm %>" 
                                                        ID="lnkDeleteStep" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 fas fa-trash-alt"
                                                        Text="<%$Resources:Buttons,Delete_TestStep %>"
                                                        />
                                                    <tstsc:DropDownListEx 
                                                        AutoPostBack="false" 
                                                        ClientScriptMethod="toggle_visibility" 
                                                        ClientScriptServerControlId="grdTestSteps" 
                                                        CssClass="DropDownList" 
                                                        DataTextField="Value" 
                                                        DataValueField="Key"
                                                        ID="ddlShowHideTestStepColumns" 
                                                        NoValueItem="True"
                                                        NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" 
                                                        runat="server" 
                                                        Width="180px"
                                                        />
                                                    <tstsc:DropMenu 
                                                        Authorized_ArtifactType="TestCase" 
                                                        Authorized_Permission="Modify"
                                                        ClientScriptMethod="page.displayEditParameters(evt)" 
                                                        ID="lnkParameters" 
                                                        runat="server"
                                                        GlyphIconCssClass="mr3 fas fa-code"
                                                        Text="<%$Resources:Dialogs,TestCaseDetails_EditParameters %>"
                                                        />
                                                </div>
                                            </div>
                                            <tstsc:MessageBox ID="lblTestStepMessages" runat="server" SkinID="MessageBox" />
                                            <tstsc:OrderedGrid 
                                                AllowColumnPositioning="true"
                                                AllowInlineEditing="true"
                                                AlternateItemHasHyperlink="false" 
                                                AlternateItemImage="artifact-TestLink.svg"
                                                Authorized_ArtifactType="TestStep" 
                                                Authorized_Permission="Modify" 
                                                CssClass="DataGrid DataGrid-no-bands v-top-children mvw-20-children text-force-break" 
                                                ConcurrencyEnabled="true" 
                                                EditRowCssClass="Editing" 
                                                ErrorMessageControlId="lblTestStepMessages"
                                                HeaderCssClass="SubHeader"
                                                ID="grdTestSteps" 
                                                ItemImage="artifact-TestStep.svg" 
                                                RowCssClass="Normal" 
                                                runat="server" 
                                                SelectedRowCssClass="Highlighted" 
                                                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestStepService"
                                                >
                                                <ContextMenuItems>
                                                    <tstsc:ContextMenuItem 
                                                        GlyphIconCssClass="mr3 fas fa-mouse-pointer" 
                                                        Caption="<%$Resources:Buttons,OpenItem %>"
                                                        CommandName="open_item" 
                                                        CommandArgument="_self" 
                                                        Authorized_ArtifactType="TestStep"
                                                        Authorized_Permission="View" 
                                                        />
                                                    <tstsc:ContextMenuItem 
                                                        Authorized_ArtifactType="TestStep"
                                                        Authorized_Permission="View" 
                                                        Caption="<%$Resources:Buttons,OpenItemNewTab %>"
                                                        CommandName="open_item" 
                                                        CommandArgument="_blank" 
                                                        GlyphIconCssClass="mr3 fas fa-external-link-alt" 
                                                        />
                                                    <tstsc:ContextMenuItem Divider="True" />
                                                    <tstsc:ContextMenuItem 
                                                        Authorized_ArtifactType="TestStep"
                                                        Authorized_Permission="Create" 
                                                        Caption="<%$Resources:Buttons,InsertStep%>"
                                                        CommandArgument="Step" 
                                                        CommandName="insert_item" 
                                                        GlyphIconCssClass="mr3 fas fa-plus" 
                                                        />
                                                    <tstsc:ContextMenuItem 
                                                        Authorized_ArtifactType="TestStep" 
                                                        Authorized_Permission="Create" 
                                                        Caption="<%$Resources:Buttons,InsertLink%>"
                                                        ClientScriptMethod="page.lnkInsertLink_click" 
                                                        GlyphIconCssClass="mr3 fas fa-link" 
                                                        />
                                                    <tstsc:ContextMenuItem 
                                                        Authorized_ArtifactType="TestStep" 
                                                        Authorized_Permission="Modify" 
                                                        Caption="<%$Resources:Buttons,EditItems%>"
                                                        CommandName="edit_items" 
                                                        GlyphIconCssClass="mr3 far fa-edit" 
                                                        />
                                                    <tstsc:ContextMenuItem 
                                                        Authorized_ArtifactType="TestStep" 
                                                        Authorized_Permission="Create" 
                                                        Caption="<%$Resources:Buttons,Clone%>"
                                                        CommandName="copy_items" 
                                                        GlyphIconCssClass="mr3 far fa-clone" 
                                                        />
                                                    <tstsc:ContextMenuItem Divider="True" />
                                                    <tstsc:ContextMenuItem 
                                                        Authorized_ArtifactType="TestStep" 
                                                        Authorized_Permission="Delete"
                                                        Caption="<%$Resources:Buttons,Delete_TestStep %>"
                                                        CommandName="delete_items" 
                                                        ConfirmationMessage="<%$Resources:Messages,TestCaseDetails_TestStepDeleteConfirm %>" 
                                                        GlyphIconCssClass="mr3 fas fa-trash-alt" 
                                                        />
                                                </ContextMenuItems>
                                            </tstsc:OrderedGrid>

                                            <tstsc:DialogBoxPanel 
                                                AjaxServerControlId="ajxLinkedTestCaseSelector"
                                                CssClass="PopupPanel" 
                                                ID="pnlInsertTestLink" 
                                                Modal="true" 
                                                runat="server" 
                                                Title="<%$Resources:Dialogs,TestCaseDetails_AddLinkedTestCase %>"
                                                Width="550px" 
                                                >
                                                <tstsc:MessageBox 
                                                    ID="msgInsertTestLinkMessages" 
                                                    runat="server" 
                                                    SkinID="MessageBoxNarrowWithMargin" 
                                                    />
                                                <p>
                                                    <asp:Localize runat="server" Text="<%$Resources:Dialogs,InsertTestLink_LinkTypeLegend %>" />&nbsp;
                                                    <span class="btn-group radio-group" role="group">
                                                        <label 
                                                            class="btn btn-default active" 
                                                            for="radLinkExisting"
                                                            >
                                                            <input 
                                                                checked="checked" 
                                                                id="radLinkExisting" 
                                                                name="radLinkType" 
                                                                type="radio" 
                                                                value="link-existing" 
                                                                />
                                                            <asp:Localize 
                                                                runat="server" 
                                                                Text="<%$Resources:Dialogs,InsertTestLink_LinkExistingTestCase %>" 
                                                                />
                                                        </label>
                                                        <tstsc:PlaceHolderEx 
                                                            Authorized_ArtifactType="TestCase" 
                                                            Authorized_Permission="Create"
                                                            ID="plcLinkNew" 
                                                            runat="server" 
                                                            >
                                                            <label 
                                                                class="btn btn-default" 
                                                                for="radLinkNew"
                                                                >
                                                                <input 
                                                                    id="radLinkNew" 
                                                                    name="radLinkType" 
                                                                    type="radio" 
                                                                    value="link-create-new" 
                                                                    />
                                                                <asp:Localize 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Dialogs,InsertTestLink_CreateNewTestCase %>" 
                                                                    />
                                                            </label>
                                                        </tstsc:PlaceHolderEx>
                                                    </span>
                                                    &nbsp;?
                                                </p>
                                                <div class="u-wrapper" id="section-link-existing">
                                                    <div class="alert alert-info alert-narrow">
                                                        <span class="fas fa-info-circle"></span>
                                                        <asp:Localize 
                                                            ID="Localize16" 
                                                            runat="server" 
                                                            Text="<%$Resources:Dialogs,TestCaseDetails_ChooseTestCaseForLink %>" 
                                                            />
                                                    </div>
                                                    <div class="u-box_3">
                                                        <ul class="u-box_list">
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    ID="ddlLinkedTestCaseFoldersLabel" 
                                                                    AssociatedControlID="ddlLinkedTestCaseFolders" 
                                                                    runat="server" 
                                                                    AppendColon="true" 
                                                                    Font-Bold="true" 
                                                                    Text="<%$Resources:Fields,Folder %>" 
                                                                    />
                                                                <tstsc:UnityDropDownHierarchy 
                                                                    ClientScriptMethod="page.ddlLinkedTestCaseFolders_changed" 
                                                                    DataTextField="Name" 
                                                                    DataValueField="TestCaseFolderId" 
                                                                    ID="ddlLinkedTestCaseFolders" 
                                                                    IndentLevelField="IndentLevel" 
                                                                    ItemImage="Images/FolderOpen.svg"
                                                                    NoValueItem="true" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_RootFolderDropDown %>" 
                                                                    runat="server" 
                                                                    SkinId="UnityDropDownListAttachments"
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0">
                                                                <div class="scrollbox resize-v h7 mb3 ov-y-auto ov-x-hidden">
                                                                    <tstsc:ItemSelector 
                                                                        AlternateItemImage="Images/artifact-TestCase.svg"                                                
                                                                        AutoLoad="false" 
                                                                        CssClass="HierarchicalSelector"
                                                                        ErrorMessageControlId="msgInsertTestLinkMessages"
                                                                        ID="ajxLinkedTestCaseSelector" 
                                                                        ItemImage="Images/artifact-TestCaseNoSteps.svg" 
                                                                        MultipleSelect="false" 
                                                                        NameLegend="<%$Resources:Fields,TestCase %>" 
                                                                        runat="server" 
                                                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService"
                                                                        Width="528px" 
                                                                        />
                                                                </div>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_3 resize-v ov-x-hidden ov-y-auto w-100 h7" id="divTestStepParametersAdd" style="display: none">
                                                        <div  
                                                            id="divHasParametersAdd"
                                                            style="display: none"
                                                            >
                                                            <div class="alert alert-warning alert-narrow ma3">
                                                                <span class="fas fa-info-circle"></span>
                                                                <asp:Localize 
                                                                    ID="Localize17" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Dialogs,TestCaseDetails_PleaseFillOutParameters %>" 
                                                                    />
                                                            </div>
                                                            <ul 
                                                                class="u-box_list w-100"
                                                                data-bind="foreach: $root"
                                                                id="tblTestStepParametersAdd"
                                                                >
                                                                <li class="ma0 pa0">
                                                                    <label class="fw-b">
                                                                        <span data-bind="text: Fields.Name.textValue"></span>:
                                                                    </label>
                                                                    <input 
                                                                        type="text" 
                                                                        class="u-input" 
                                                                        maxlength="255" 
                                                                        data-bind="textInput: Fields.DefaultValue.textValue, event: { keydown: Function.createDelegate(page, page.pnlInsertTestLink_rptTestStepParameters_keydown) }" 
                                                                        />
                                                                </li>
                                                            </ul>
                                                        </div>
                                                        <div 
                                                            class="alert alert-danger alert-narrow ma3"
                                                            id="divNoParametersAdd" 
                                                            style="display: none"
                                                            >
                                                            <span class="fas fa-exclamation-triangle"></span>
                                                            <asp:Localize ID="Localize18" runat="server" Text="<%$Resources:Messages,TestCaseDetails_TestHasNoParameters %>" />
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="u-wrapper" id="section-link-new" style="display:none">
                                                    <div class="alert alert-info alert-narrow">
                                                        <span class="fas fa-info-circle"></span>
                                                        <asp:Localize ID="Localize31" runat="server" Text="<%$Resources:Dialogs,TestCaseDetails_CreateTestCaseForLink %>" />
                                                    </div>
                                                    <div class="u-box_3">
                                                        <ul class="u-box_list">
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlNewLinkedTestCaseFolders" 
                                                                    Font-Bold="true" 
                                                                    ID="ddlNewLinkedTestCaseFoldersLabel" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,Folder %>" 
                                                                    />
                                                                <tstsc:UnityDropDownHierarchy 
                                                                    DataTextField="Name" 
                                                                    DataValueField="TestCaseFolderId" 
                                                                    ID="ddlNewLinkedTestCaseFolders" 
                                                                    IndentLevelField="IndentLevel" 
                                                                    ItemImage="Images/FolderOpen.svg"
                                                                    NoValueItem="true" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_RootFolderDropDown %>" 
                                                                    runat="server" 
                                                                    SkinId="UnityDropDownListAttachments"
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="txtNewLinkedTestCase" 
                                                                    ID="txtNewLinkedTestCaseLabel" 
                                                                    Required="true" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,Name %>" 
                                                                    />
                                                                <tstsc:UnityTextBoxEx 
                                                                    CssClass="u-input"
                                                                    ID="txtNewLinkedTestCase" 
                                                                    runat="server" 
                                                                    MaxLength="255" 
                                                                    />
                                                            </li>
                                                        </ul>
                                                        <h4 class="mx3">
                                                            <asp:Localize runat="server" Text="<%$Resources:Dialogs,TestCaseDetails_EnterParameters %>" />
                                                        </h4>
                                                        <div class="u-box_3 resize-v ov-x-hidden ov-y-auto w-100 h8 ba b-vlight-gray">
                                                            <table 
                                                                id="tblNewLinkedTestCaseParameters" 
                                                                class="DataGrid DataGrid-no-bands" 
                                                                style="width: calc(100% - 1rem)"
                                                                >
                                                                <thead>
                                                                    <tr class="Header">
                                                                        <th>
                                                                            <asp:Localize ID="Localize32" runat="server" Text="<%$Resources:Fields,Name %>" />
                                                                        </th>
                                                                        <th>
                                                                            <asp:Localize ID="Localize33" runat="server" Text="<%$Resources:Fields,Value %>" />
                                                                        </th>
                                                                    </tr>
                                                                </thead>
                                                                <tbody data-bind="foreach: parameters">
                                                                    <tr>
                                                                        <td>
                                                                            <div class="input-group u-input-grp w-100">
                                                                                <span class="input-group-addon">${</span>
                                                                                <input 
                                                                                    class="u-input" 
                                                                                    data-bind="textInput: Fields.Name.textValue, event: { keyup: Function.createDelegate(page, page.tblNewLinkedTestCaseParameters_keyup) }" 
                                                                                    maxlength="50" 
                                                                                    type="text" 
                                                                                    />
                                                                                <span class="input-group-addon">}</span>
                                                                            </div>
                                                                        </td>
                                                                        <td>
                                                                            <input 
                                                                                class="u-input w-100" 
                                                                                data-bind="textInput: Fields.Value.textValue" 
                                                                                maxlength="255" 
                                                                                type="text" 
                                                                                />
                                                                        </td>
                                                                    </tr>
                                                                </tbody>
                                                            </table>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="ml2 mt4 btn-group">
                                                    <button 
                                                        class="btn btn-primary"
                                                        id="lnkAddTestLink" 
                                                        type="button" 
                                                        >
                                                        <asp:Localize 
                                                            ID="Localize19" 
                                                            runat="server" 
                                                            Text="<%$Resources:Buttons,Add %>" 
                                                            />
                                                    </button>
											        <tstsc:HyperLinkEx 
                                                        ClientScriptMethod="close()" 
                                                        ClientScriptServerControlId="pnlInsertTestLink" 
                                                        ID="lnkInsertLinkCancel" 
                                                        NavigateUrl="javascript:void(0)"
                                                        runat="server" 
                                                        SkinID="ButtonDefault"
                                                        Text="<%$Resources:Buttons,Cancel %>" 
                                                        />
                                                </div>
                                            </tstsc:DialogBoxPanel>

                                            <tstsc:DialogBoxPanel 
                                                AjaxServerControlId="ajxImportTestCaseSelector"
                                                CssClass="PopupPanel" 
                                                Height="450px" 
                                                ID="pnlImportTestCase" 
                                                Modal="true" 
                                                runat="server" 
                                                Title="<%$Resources:Dialogs,TestCaseDetails_ImportStepsFromTestCase %>"
                                                Width="550px" 
                                                >
                                                <tstsc:MessageBox 
                                                    ID="msgImportTestCaseMessages" 
                                                    runat="server" 
                                                    SkinID="MessageBoxNarrowWithMargin"
                                                    />
                                                <div class="u-wrapper">
                                                    <div class="alert alert-warning alert-narrow ma3">
                                                        <span class="fas fa-info-circle"></span>
                                                        <asp:Localize 
                                                            ID="Localize8" 
                                                            runat="server" 
                                                            Text="<%$Resources:Dialogs,TestCaseDetails_ChooseTestCaseToImport %>" 
                                                            />
                                                    </div>
                                                    <div class="u-box_3">
                                                        <ul class="u-box_list">
                                                            <li class="ma0 pa0">
                                                                <tstsc:LabelEx 
                                                                    AppendColon="true" 
                                                                    AssociatedControlID="ddlImportTestCaseFolders" 
                                                                    Font-Bold="true" 
                                                                    ID="ddlImportTestCaseFoldersLabel" 
                                                                    runat="server" 
                                                                    Text="<%$Resources:Fields,Folder %>" 
                                                                    />
                                                                <tstsc:UnityDropDownHierarchy
                                                                    ClientScriptMethod="page.ddlImportTestCaseFolders_changed" 
                                                                    DataTextField="Name" 
                                                                    DataValueField="TestCaseFolderId" 
                                                                    ID="ddlImportTestCaseFolders" 
                                                                    IndentLevelField="IndentLevel" 
                                                                    ItemImage="Images/FolderOpen.svg"
                                                                    NoValueItem="true" 
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_RootFolderDropDown %>" 
                                                                    runat="server" 
                                                                    SkinId="UnityDropDownListAttachments"
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0">
                                                                <div class="scrollbox resize-v h8 mb3 ov-y-auto ov-x-hidden">
                                                                    <tstsc:ItemSelector 
                                                                        AlternateItemImage="Images/artifact-TestCase.svg"                                                
                                                                        AutoLoad="false" 
                                                                        CssClass="HierarchicalSelector"
                                                                        ErrorMessageControlId="msgImportTestCaseMessages" 
                                                                        ID="ajxImportTestCaseSelector" 
                                                                        ItemImage="Images/artifact-TestCaseNoSteps.svg" 
                                                                        MultipleSelect="false" 
                                                                        NameLegend="<%$Resources:Fields,TestCase %>" 
                                                                        runat="server" 
                                                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService"
                                                                        Width="540px" 
                                                                        />
                                                                </div>
                                                            </li>
                                                        </ul>
                                                    </div>
                                                </div>
                                                <div class="ml2 mt4 btn-group">
                                                    <button 
                                                        class="btn btn-primary"
                                                        id="btnImportTestCase" 
                                                        type="button" 
                                                        >
                                                        <asp:Localize 
                                                            ID="Localize30" 
                                                            runat="server" 
                                                            Text="<%$Resources:Buttons,Import %>" 
                                                            />
                                                    </button>
											        <tstsc:HyperLinkEx 
                                                        ClientScriptMethod="close()" 
                                                        ClientScriptServerControlId="pnlImportTestCase" 
                                                        ID="btnImportTestCaseCancel" 
                                                        NavigateUrl="javascript:void(0)"
                                                        runat="server" 
                                                        SkinID="ButtonDefault"
                                                        Text="<%$Resources:Buttons,Cancel %>" 
                                                        />
                                                </div>
                                            </tstsc:DialogBoxPanel>

                                            <tstsc:DialogBoxPanel 
                                                CssClass="PopupPanel" 
                                                Height="350px" 
                                                ID="pnlEditTestLink" 
                                                Modal="true"
                                                runat="server" 
                                                Title="<%$Resources:Dialogs,TestCaseDetails_EditLinkedTestParameters %>"
                                                Width="600px"
                                                >
                                                <div 
                                                    class="alert alert-info alert-narrow"
                                                    id="divHasParameters" 
                                                    >
                                                    <span class="fas fa-info-circle"></span>
                                                    <asp:Localize 
                                                        ID="Localize21" 
                                                        runat="server" 
                                                        Text="<%$Resources:Dialogs,TestCaseDetails_PleaseFillOutParameters %>" 
                                                        />
                                                </div>
                                                <div 
                                                    class="alert alert-info"
                                                    id="divNoParameters" 
                                                    >
                                                    <span class=""></span>
                                                    <asp:Localize 
                                                        ID="Localize22" 
                                                        runat="server" 
                                                        Text="<%$Resources:Messages,TestCaseDetails_TestHasNoParameters %>" 
                                                        />
                                                </div>
                                                <div
                                                    class="ma3 pa3 ov-y-scroll h8" 
                                                    id="divTestStepParametersEdit" 
                                                    >
                                                    <ul 
                                                        class="u-box_list w-100" 
                                                        data-bind="foreach: $root"
                                                        id="tblTestStepParametersEdit"
                                                        >
                                                        <li class="ma0 mb2 pa0">
                                                            <label>
                                                                <span 
                                                                    class="fw-b" 
                                                                    data-bind="text: Fields.Name.textValue">
                                                                </span>:
                                                            </label>
                                                            <input 
                                                                type="text" 
                                                                class="u-input is-active" 
                                                                maxlength="255" 
                                                                data-bind="textInput: Fields.Value.textValue, event: { keydown: Function.createDelegate(page, page.pnlEditTestLink_tblTestStepParametersEdit_keydown)  }"
                                                                />
                                                        </li>
                                                    </ul>
                                                </div>
                                                <div class="btn-group pl3">
                                                    <button 
                                                        class="btn btn-primary"
                                                        id="btnTestLinkUpdate" 
                                                        onclick="page.pnlEditTestLink_lnkTestLinkUpdate_click(event)" 
                                                        runat="server" type="button" 
                                                        >
                                                        <asp:Localize 
                                                            ID="Localize23" 
                                                            runat="server" 
                                                            Text="<%$Resources:Buttons,Save %>" 
                                                            />
                                                    </button>
											        <tstsc:HyperLinkEx 
                                                        ClientScriptMethod="close()" 
                                                        ClientScriptServerControlId="pnlEditTestLink" 
                                                        ID="lnkTestLinkEditCancel" 
                                                        NavigateUrl="javascript:void(0)" 
                                                        runat="server" 
                                                        SkinID="ButtonDefault"
                                                        Text="<%$Resources:Buttons,Cancel %>" 
                                                        />
                                                </div>
                                            </tstsc:DialogBoxPanel>
                                        </asp:Panel>
                                </div>
                            </div>
                        </div>





                        <%-- AUTOMATION --%>
                        <div class="u-box_3 mt4">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_automation" 
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_Automation %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <div class="u-box_item mb2" >
                                    <div class="u-box_3">
                                        <ul class="u-box_list">
                                            <li class="ma0 mb2 pa0">
                                                <div class="alert alert-warning alert-narrow">
                                                    <strong>
                                                        <span class="fas fa-info-circle"></span>
                                                    </strong>
                                                    <asp:Localize 
                                                        ID="Localize20" 
                                                        runat="server" 
                                                        Text="<%$Resources:Main,TestCaseDetails_AutomatedSectionIntro %>" 
                                                        />
                                                </div>
                                            </li>
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true"
                                                    AssociatedControlID="radAttached"
                                                    ID="lblScriptType" 
                                                    Required="true" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,ScriptType %>"
                                                    />
                                                <span class="btn-group btn-group-justified radio-group" role="group">
                                                    <asp:Label 
                                                        AssociatedControlID="radAttached" 
                                                        CssClass="btn btn-default"
                                                        ID="lblAttached" 
                                                        runat="server" 
                                                        >
                                                        <tstsc:RadioButtonEx 
                                                            ClientScriptMethod="page.automationScript_changed(event)"
                                                            GroupName="ScriptType" 
                                                            ID="radAttached" 
                                                            runat="server" 
                                                            />
                                                        <span class="fas fa-paperclip dark-gray"></span>
                                                        <asp:Label runat="server" CssClass="dark-gray" Text="<%$Resources:Fields,TestScript_Attached %>" />
                                                    </asp:Label>
                                                    <asp:Label 
                                                        AssociatedControlID="radLinked" 
                                                        CssClass="btn btn-default"
                                                        ID="lblLinked" 
                                                        runat="server" 
                                                        >
                                                        <tstsc:RadioButtonEx 
                                                            ClientScriptMethod="page.automationScript_changed(event)"
                                                            GroupName="ScriptType" 
                                                            ID="radLinked" 
                                                            runat="server" 
                                                            />
                                                        <span class="fas fa-link dark-gray"></span>
                                                        <asp:Label runat="server" CssClass="dark-gray" Text="<%$Resources:Fields,TestScript_Linked %>" />
                                                    </asp:Label>
                                                    <asp:Label 
                                                        AssociatedControlID="radRepository" 
                                                        CssClass="btn btn-default"
                                                        ID="lblRepository" 
                                                        runat="server" 
                                                        >
                                                        <tstsc:RadioButtonEx 
                                                            ClientScriptMethod="page.automationScript_changed(event)"
                                                            GroupName="ScriptType" 
                                                            ID="radRepository" 
                                                            runat="server" 
                                                            />
                                                        <span class="fas fa-database dark-gray"></span>
                                                        <asp:Label runat="server" CssClass="dark-gray" Text="<%$Resources:Main,TestCaseDetails_Repository %>" />
                                                    </asp:Label>
                                                </span>
                                            </li>
                                        </ul>
                                    </div>
                                    <div class="u-box_2">
                                        <ul class="u-box_list">
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true"
                                                    AssociatedControlID="ddlAutomationEngine"
                                                    ID="ddlAutomationEngineLabel" 
                                                    Required="true" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,AutomationEngineId %>" 
                                                    />
                                                <tstsc:UnityDropDownListEx 
                                                    ClientScriptMethod="ddlAutomationEngine_selected"
                                                    CssClass="u-dropdown" 
                                                    DataTextField="Name" 
                                                    DataValueField="AutomationEngineId" 
                                                    DisabledCssClass="u-dropdown disabled" 
                                                    ID="ddlAutomationEngine" 
                                                    NoValueItem="true" 
                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                                    runat="server"
                                                    />
                                            </li>
                                            <li class="ma0 mb0 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true"
                                                    AssociatedControlID="txtAutomationLink"
                                                    ID="txtAutomationLinkLabel" 
                                                    Required="true" 
                                                    runat="server" 
                                                    >
                                                    <tstsc:ImageEx 
                                                        AlternateText="<%$Resources:Fields,FileType %>"
                                                        CssClass="w4 h4 mr2"
                                                        ID="imgAutomationFileIcon" 
                                                        ImageUrl="Images/artifact-Document.svg" 
                                                        runat="server" 
                                                        />
                                                    <asp:Localize 
                                                        Text="<%$Resources:Fields,Filename %>"
                                                        runat="server" 
                                                        />
                                                </tstsc:LabelEx>
                                                <tstsc:UnityTextBoxEx 
                                                    CssClass="u-input"
                                                    ID="txtAutomationLink" 
                                                    runat="server" 
                                                    TextMode="SingleLine"
                                                    MaxLength="255" 
                                                    />
                                                <div 
                                                    class="dib"
                                                    id="divAutomationRepository"
                                                    runat="server"
                                                    >
                                                    <tstsc:ImageEx CssClass="w4 h4" ID="imgRepositoryTest" runat="server" SkinID="RepositoryTest" ImageAlign="TextTop" />
                                                    <tstsc:HyperLinkEx ID="lnkAutomationRepository" runat="server" />
                                                </div>
                                            </li>
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:ArtifactHyperLink 
                                                    AlternateText="<%$Resources:Fields,View_File %>"
                                                    CssClass="ArtifactHyperLink ml_u-box-label"
                                                    DisplayChangeLink="false"
                                                    ID="lnkAutomationDocument" 
                                                    runat="server" />
                                            </li>
                                        </ul>
                                    </div>
                                    <div class="u-box_1">
                                        <ul class="u-box_list">  
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true"
                                                    AssociatedControlID="txtAutomationVersion"
                                                    ID="txtAutomationVersionLabel" 
                                                    Required="false" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,Version %>" 
                                                    />
                                                <tstsc:UnityTextBoxEx 
                                                    CssClass="u-input"
                                                    DisabledCssClass="u-input disabled"
                                                    ID="txtAutomationVersion" 
                                                    MaxLength="5" 
                                                    runat="server" 
                                                    TextMode="SingleLine" 
                                                    />
                                            </li>
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true"
                                                    AssociatedControlID="ddlDocType" 
                                                    ID="lblDocType" 
                                                    Required="true"
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,DocumentType %>"
                                                    />
                                                <tstsc:UnityDropDownListEx 
                                                    CssClass="u-dropdown"
                                                    DataMember="DocumentType" 
                                                    DataTextField="Name" 
                                                    DataValueField="DocumentTypeId"
                                                    DisabledCssClass="u-dropdown disabled"
                                                    ID="ddlDocType" 
                                                    NoValueItem="false" 
                                                    runat="server" 
                                                    />
                                            </li>
                                            <li class="ma0 mb2 pa0">
                                                <tstsc:LabelEx 
                                                    AppendColon="true"
                                                    AssociatedControlID="ddlDocFolder"
                                                    ID="lblDocFolder" 
                                                    Required="true" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,DocumentFolder %>" 
                                                    />
                                                <tstsc:UnityDropDownHierarchy 
                                                    DataTextField="Name" 
                                                    DataValueField="ProjectAttachmentFolderId"
                                                    ID="ddlDocFolder" 
                                                    NoValueItem="false" 
                                                    runat="server" 
                                                    SkinID="UnityDropDownListAttachments"
                                                    />
                                            </li>
                                        </ul>
                                    </div>
                                    <div class="u-box_3">
                                    <ul class="u-box_list">
                                        <li class="ma0 mb2 pa0" id="automationScriptSection">
                                            <tstsc:LabelEx 
                                                AppendColon="true"
                                                AssociatedControlID="txtAutomationScript"
                                                ID="txtAutomationScriptLabel" 
                                                Required="true" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,TestScript %>" 
                                                />
                                            <div class="fr mb3">
								                <tstsc:HyperLinkEx 
                                                    Authorized_ArtifactType="TestCase" 
                                                    Authorized_Permission="Modify"
                                                    ClientScriptMethod="page.displayEditParameters(event)"
                                                    ID="lnkEditParameters2" 
                                                    NavigateUrl="javascript:void(0)" 
                                                    runat="server" 
                                                    SkinID="ButtonDefault"
                                                    >
                                                    <span class="fas fa-code"></span>
                                                    <asp:Localize 
                                                        runat="server" 
                                                        Text="<%$Resources:Dialogs,TestCaseDetails_EditParameters %>" 
                                                        />
								                </tstsc:HyperLinkEx>
                                            </div>
                                            <tstsc:UnityTextBoxEx 
                                                CssClass="clearfix br2 ws-pre pa3 ba b-light-gray bg-white w-100 h8"
                                                ID="txtAutomationScript" 
                                                RichText="false" 
                                                runat="server" 
                                                TextMode="MultiLine"
                                                />
                                            <asp:HiddenField ID="hdnTestScriptChanged" runat="server" Value="false" />
                                        </li>
                                    </ul>
                                </div>
                                </div>
                            </div>
                        </div>

                        <div class="u-box_3">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_comments" 
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_Comments %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul class="u-box_list" runat="server">
                                    <li class="ma0 mb2 pa0">
								        <tstsc:CommentList 
                                            ArtifactType="TestCase" 
                                            AutoLoad="false" 
                                            ErrorMessageControlId="lblMessage"
                                            ID="lstComments" 
                                            runat="server" 
									        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" 
                                            Width="100%" 
                                            />
								        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="TestCase"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="txtNewComment" 
                                            runat="server" 
                                            Screenshot_ArtifactType="TestCase" 
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

                <tstsc:DialogBoxPanel 
                    CssClass="PopupPanel"
                    Height="470px" 
                    ID="pnlEditParameters" 
                    Modal="false" 
                    runat="server" 
                    Title="<%$Resources:Main,TestCaseDetails_EditTestCaseParameters %>"
                    Width="650px" 
                    >
                    <tstsc:MessageBox 
                        ID="lblParameterMessage" 
                        runat="server" 
                        SkinID="MessageBox" 
                        />
                    <div class="w-100 mx3">
                        <asp:Localize 
                            ID="Localize3" 
                            runat="server" 
                            Text="<%$Resources:Main,TestCaseDetails_ParametersDefinedForTestCase %>" 
                            />
                    </div>
                    <div class="ma3 ov-x-hidden ov-y-scroll h8">
                        <table 
                            id="tblTestCaseParameters" 
                            class="DataGrid DataGrid-no-bands priority1 dt-fixed w-100" 
                            >
                            <thead>
                                <tr class="Header">
                                    <th>
                                        <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Fields,Name %>" />
                                    </th>
                                    <th>
                                        <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Fields,DefaultValue %>" />
                                    </th>
                                    <th>
                                        <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Fields,Operations %>" />
                                    </th>
                                </tr>
                            </thead>
                            <tbody data-bind="foreach: $root">
                                <tr>
                                    <td>
                                        <span data-bind="text: Fields.Token.textValue, style: { display: (editable()) ? 'none' : 'inline' }"></span>
                                        <div 
                                            class="input-group u-input-grp is-active" 
                                            data-bind="style: { display: (editable()) ? 'table' : 'none' }"
                                            >
                                            <span class="input-group-addon py1">${</span>
                                            <input 
                                                class="u-input py1" 
                                                data-bind="textInput: Fields.Name.textValue"
                                                maxlength="50" 
                                                style="width:130px"
                                                type="text" 
                                                />
                                            <span class="input-group-addon py1">}</span>
                                        </div>
                                    </td>
                                    <td>
                                        <span data-bind="text: (Fields.DefaultValue.textValue() && Fields.DefaultValue.textValue() != '') ? Fields.DefaultValue.textValue : '-', style: { display: (editable()) ? 'none' : 'inline' }"></span>
                                        <input 
                                            class="u-input is-active py1" 
                                            data-bind="textInput: Fields.DefaultValue.textValue, style: { display: (editable()) ? 'inline' : 'none' }" 
                                            maxlength="255" 
                                            style="width:130px" 
                                            type="text" 
                                            />
                                    </td>
                                    <td>
                                        <div class="btn-group" role="group">
                                            <button 
                                                class="btn btn-xs btn-default" 
                                                data-bind="event: { mousedown: function() { page.testCaseParameters_storeFocusElement(); } }, click: function(data, evt) { page.testCaseParameters_insertAtCursor(data.Fields.Token.textValue(), evt); }"
                                                type="button" 
                                                >
                                                <span class="fas fa-i-cursor"></span>
                                                <asp:Localize 
                                                    ID="Localize29" 
                                                    runat="server" 
                                                    Text="<%$Resources:Buttons,InsertAtCursor %>" 
                                                    />
                                            </button>
                                            <button 
                                                class="btn btn-xs btn-default" 
                                                data-bind="click: function(data,evt) { page.testCaseParameters_edit(data); }, css: { active: editable() }"
                                                title="<%$Resources:Buttons,Edit %>" runat="server"
                                                type="button" 
                                                >
                                                <span class="fas fa-edit fa-fw"></span>
                                            </button>
                                            <button 
                                                class="btn btn-xs btn-default" 
                                                data-bind="click: function(data,evt) { page.testCaseParameters_delete(data); }"
                                                title="<%$Resources:Buttons,Delete %>" runat="server"
                                                type="button" 
                                                >
                                                <span class="fas fa-trash-alt fa-fw"></span>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <div class="u-wrapper">
                        <div class="u-box_3">
                            <ul class="u-box_list">
                                <li class="ma0 mb2 pa0">
                                    <asp:Localize 
                                        ID="Localize15" 
                                        runat="server" 
                                        Text="<%$Resources:Main,TestCaseDetails_AddNewParameterToTestCase %>" 
                                        />
                                </li>
                                <li class="ma0 mb2 pa0">
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="txtNewParameter"
                                        ID="txtNewParameterLabel" 
                                        Required="true" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Name %>" 
                                        />
                                    <div class="dib u-box_list_control">
                                        <div class="input-group u-input-grp is-active w-100" >
                                            <span class="input-group-addon">${</span>
                                            <tstsc:UnityTextBoxEx 
                                                CssClass="u-input"
                                                DisabledCssClass="u-input disabled"
                                                ID="txtNewParameter" 
                                                MaxLength="50" 
                                                runat="server" 
                                                />
                                            <span class="input-group-addon">}</span>
                                        </div>
                                    </div>
                                </li>
                                <li class="ma0 mb2 pa0">
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="txtNewParameterDefaultValue"
                                        ID="txtNewParameterDefaultValueLabel" 
                                        Required="false" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,DefaultValue %>" 
                                        />
                                    <div class="dib u-box_list_control">
                                        <div class="input-group u-input-grp is-active w-100">
                                            <tstsc:UnityTextBoxEx 
                                                CssClass="u-input"
                                                DisabledCssClass="u-input disabled"
                                                ID="txtNewParameterDefaultValue" 
                                                MaxLength="255" 
                                                runat="server" 
                                                />
                                            <span class="input-group-btn">
                                                <button 
                                                    class="btn btn-default mr0" 
                                                    type="button" 
                                                    id="btnAddParameter"
                                                    >
                                                    <span class="fas fa-plus"></span>
                                                    <asp:Localize runat="server" Text="<%$Resources:Buttons,Add %>" />
                                                </button>
                                            </span>
                                        </div>
                                    </div>
                                </li>
                            </ul>
                        </div>
                    </div>
                    <div class="btn-group ml4 mt3">
                        <tstsc:ButtonEx 
                            Authorized_ArtifactType="TestCase" 
                            Authorized_Permission="Modify" 
                            ClientScriptMethod="page.testCaseParameters_save()" 
                            ID="btnParametersSave" 
                            runat="server" 
                            SkinID="ButtonPrimary"
                            Text="<%$Resources:Buttons,Save %>" 
                            />
						<tstsc:ButtonEx 
                            ClientScriptMethod="close()" 
                            ClientScriptServerControlId="pnlEditParameters"
                            ID="btnParametersCancel" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Cancel %>" 
                            />
                    </div>
                </tstsc:DialogBoxPanel>





                <asp:Panel ID="pnlIncidents" Width="100%" runat="server" CssClass="TabControlPanel">
					<tstuc:IncidentListPanel 
                        ID="tstIncidentListPanel" 
                        runat="server" 
                        />
                </asp:Panel>


                <asp:Panel ID="pnlAssociations" runat="server" Width="100%">
                    <tstuc:AssociationsPanel ID="tstAssociationPanel" runat="server" />
                </asp:Panel>



                <asp:Panel ID="pnlTestRuns" Width="100%" runat="server" CssClass="TabControlPanel">
                    <tstuc:TestRunListPanel ID="tstTestRunListPanel" runat="server" />
                </asp:Panel>
                        
                        
                        
                        
                        
                <asp:Panel ID="pnlRequirements" runat="server" Width="100%">
                    <tstuc:AssociationsPanel ID="tstCoveragePanel" runat="server" />
                </asp:Panel>
                        
                        
                        
                        
                        
                <asp:Panel ID="pnlReleases" runat="server" Width="100%">
                    <tstuc:AssociationsPanel ID="tstReleaseMappingPanel" runat="server" />
                </asp:Panel>
                        
                        
                        
                        
                        
                <asp:Panel ID="pnlAttachments" runat="server" Width="100%" CssClass="TabControlPanel">
                    <tstuc:AttachmentPanel ID="tstAttachmentPanel" runat="server" />
                </asp:Panel>
                        
                        
                        
                        
                <asp:Panel ID="pnlHistory" runat="server" Width="100%" CssClass="TabControlPanel">
                    <tstuc:HistoryPanel ID="tstHistoryPanel" runat="server" />
                </asp:Panel>
                
                  <asp:Panel ID="pnlSignature" runat="server" Width="100%" CssClass="TabControlPanel">
                    <tstuc:SignaturePanel ID="tstSignaturePanel" runat="server"  ArtifactId="<%# this.testCaseId %>" />
                </asp:Panel>

                   
                        
                        
                        
                <asp:Panel ID="pnlTestSets" runat="server" Width="100%" CssClass="TabControlPanel">
                    <tstuc:TestSetListPanel 
                        ID="tstTestSetListPanel" 
                        ArtifactTypeEnum="TestConfigurationSet"
                        runat="server" 
                        />
                </asp:Panel>
            </div>
        </div>
    </div>

    <tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />

    <tstsc:AjaxFormManager ID="ajxFormManager" runat="server" ErrorMessageControlId="lblMessage" ArtifactImageControlId="imgTestCase"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" ArtifactTypeName="<%$Resources:Fields,TestCase%>"
        ItemImage="Images/artifact-TestCaseNoSteps.svg" AlternateItemImage="Images/artifact-TestCase.svg" DisplayPageName="true" NameField="Name"
        FolderPathControlId="pnlFolderPath"
        WorkflowEnabled="true" CheckUnsaved="true" RevertButtonControlId="btnRevert" WorkflowOperationsControlId="ajxWorkflowOperations">
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblTestCaseId" DataField="TestCaseId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="lblTestCaseStatusValue" DataField="TestCaseStatusId" Direction="In" IsWorkflowStep="true" />
			<tstsc:AjaxFormControl ControlId="ddlType" DataField="TestCaseTypeId" Direction="InOut" ChangesWorkflow="true" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentIds" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlAuthor" DataField="AuthorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlCreator" DataField="CreatorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In"
                PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblExecutionDate" DataField="ExecutionDate" Direction="In"
                PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In"
                PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="txtEstimatedDuration" DataField="EstimatedDuration"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlPriority" DataField="TestCasePriorityId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ajxExecutionStatus" DataField="ExecutionStatusId"
                Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlAutomationEngine" DataField="AutomationEngineId"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="imgAutomationFileIcon" DataField="AutomationFileIcon" Direction="In" PropertyName="textValue" />  
            <tstsc:AjaxFormControl ControlId="lnkAutomationDocument" DataField="AutomationDocumentId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtAutomationLink" DataField="AutomationLink" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlDocType" DataField="AutomationDocumentTypeId" 
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlDocFolder" DataField="AutomationDocumentFolderId"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtAutomationVersion" DataField="AutomationVersion"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtAutomationScript" DataField="AutomationScript"
                Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="hdnTestScriptChanged" DataField="AutomationChanged"
                Direction="Out" />
            <tstsc:AjaxFormControl ControlId="chkIsSuspect" DataField="IsSuspect" Direction="InOut" />
        </ControlReferences>
        <HyperLinkControls>
            <tstsc:AjaxHyperLinkControl ControlId="lnkAutomationRepository" DataField="AutomationLink" />
        </HyperLinkControls>
        <SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
        </SaveButtons>
    </tstsc:AjaxFormManager>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestStepService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />
        </Services>
        <Scripts>
            <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.jQuery-Plugins.js" Assembly="Web" />
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/TestCaseDetails.js" />
            <asp:ScriptReference Path="~/TypeScript/rct_comp_testRunsPendingExecuteNewOrExisting.js" />
            <asp:ScriptReference Path="~/TypeScript/Followers.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        var resx = Inflectra.SpiraTest.Web.GlobalResources;

        //SpiraContext
        SpiraContext.pageId = "Inflectra.Spira.Web.TestCaseDetails";
        SpiraContext.ArtifactId = <%=testCaseId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=testCaseId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
        SpiraContext.ArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.TestCase%>;
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
        var tabControl_id = '<%=this.tclTestCaseDetails.ClientID%>';
        var navigationBar_id = '<%=this.navTestCaseList.ClientID%>';
        var ajxBackgroundProcessManager_id = '<%=this.ajxBackgroundProcessManager.ClientID%>';
        var btnExecuteTest_id = '<%=btnExecuteTest.ClientID%>';
        var btnEmail_id = '<%=btnEmail.ClientID%>';
        var lnkAddTestLink = document.getElementById("lnkAddTestLink");

        //TabControl Panel IDs
        var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
        var pnlHistory_id = '<%=pnlHistory.ClientID%>';
        var pnlTestRuns_id = '<%=pnlTestRuns.ClientID%>';
        var pnlIncidents_id = '<%=pnlIncidents.ClientID%>';
        var pnlCoverage_id = '<%=pnlRequirements.ClientID%>';
        var pnlReleases_id = '<%=pnlReleases.ClientID%>';
        var pnlTestSets_id = '<%=pnlTestSets.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';

        //Other IDs
        var radAttached_id = '<%=radAttached.ClientID%>';
        var radLinked_id = '<%=radLinked.ClientID%>';
        var radRepository_id = '<%=radRepository.ClientID %>';
        var lblAttached_id = '<%=lblAttached.ClientID%>';
        var lblLinked_id = '<%=lblLinked.ClientID%>';
        var lblRepository_id = '<%=lblRepository.ClientID %>';
		var automationScriptSection = document.getElementById("automationScriptSection");
        var txtAutomationScript_id = '<%=txtAutomationScript.ClientID%>';
		var lnkAutomationDocument_id = '<%=lnkAutomationDocument.ClientID%>';
        var lblParameterMessage_id = '<%=lblParameterMessage.ClientID%>';
        var lblTestStepMessages_id = '<%=lblTestStepMessages.ClientID%>';
        var msgInsertTestLinkMessages_id = '<%=msgInsertTestLinkMessages.ClientID%>';
        var msgImportTestCaseMessages_id = '<%=msgImportTestCaseMessages.ClientID%>';
        var txtNewParameter_id = '<%=txtNewParameter.ClientID%>';
        var txtNewParameterDefaultValue_id = '<%=txtNewParameterDefaultValue.ClientID%>';
        var pnlEditParameters_id = '<%=pnlEditParameters.ClientID%>';
        var txtNewLinkedTestCase_id = '<%=txtNewLinkedTestCase.ClientID%>';
        var grdTestSteps_id = '<%=grdTestSteps.ClientID%>';
        var pnlInsertTestLink_id = '<%=pnlInsertTestLink.ClientID%>';
        var pnlImportTestCase_id = '<%=pnlImportTestCase.ClientID%>';
        var pnlEditTestLink_id = '<%=pnlEditTestLink.ClientID%>';
        var hdnTestScriptChanged_id = '<%=hdnTestScriptChanged.ClientID%>';
        var ddlNewLinkedTestCaseFolders_id = '<%=ddlNewLinkedTestCaseFolders.ClientID%>';
        var ddlLinkedTestCaseFolders_id = '<%=ddlLinkedTestCaseFolders.ClientID%>';
        var ddlImportTestCaseFolders_id = '<%=ddlImportTestCaseFolders.ClientID%>';
        var ajxLinkedTestCaseSelector_id = '<%=ajxLinkedTestCaseSelector.ClientID%>';
        var ajxImportTestCaseSelector_id = '<%=ajxImportTestCaseSelector.ClientID%>';
        var pnlOverview_TestSteps_id = '<%=pnlOverview_TestSteps.ClientID%>';

        //URL Templates
        var urlTemplate_artifactRedirectUrl = '<%=TestCaseRedirectUrl %>';
        var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToTestCaseListUrl) %>';
        var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
        var urlTemplate_testCaseList = '<%=TestCaseListUrl %>';
        var urlTemplate_testCaseRedirect = '<%=TestCaseRedirectUrl %>';
        var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';
        var urlTemplate_testRunsPendingExploratory = '<%=TestRunsPendingExploratoryUrl %>';
        var urlTemplate_testRunsPending = '<%=TestRunsPendingUrl %>';

        //Set any page properties
        var pageProps = {};
        pageProps.includeStepHistory = true;

        //Create the page class
        var page = $create(Inflectra.SpiraTest.Web.TestCaseDetails);

        /* Other Functions */

        //Updates any page specific content
        var testCaseDetails_testCaseId = -1;
        function updatePageContent()
        {
            page.loadAutomationInfo();
            var ajxFormManager = $find(ajxFormManager_id);

            //See if the artifact id has changed and reload the steps
            var grdTestSteps = $find('<%=this.grdTestSteps.ClientID%>');
            if (testCaseDetails_testCaseId != SpiraContext.ArtifactId)
            {
                //Load the scenario steps
                var filters = {};
                filters[globalFunctions.keyPrefix + 'TestCaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
                grdTestSteps.set_standardFilters(filters);
                grdTestSteps.load_data();
                testCaseDetails_testCaseId = SpiraContext.ArtifactId;

                //Only allow create/delete of test steps if can modify the current test case
                var lnkInsertStep = '<%=lnkInsertStep.ClientID%>',
                    lnkInsertLink = '<%=lnkInsertLink.ClientID%>',
                    lnkCopyStep = '<%=lnkCopyStep.ClientID%>',
                    lnkDeleteStep = '<%=lnkDeleteStep.ClientID%>';

                var isAuthorizedToCreateSteps = false,
                    isAuthorizedToDeleteSteps = false;

                //First get the permissions for the test case
                var canModifyTestCase = globalFunctions.isAuthorizedToModifyCurrentArtifact(globalFunctions.artifactTypeEnum.testCase, ajxFormManager);
                if (canModifyTestCase) {
                    //Get the permissions for test step create and delete
                    var authorizedStateCreateSteps = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.testStep);
                    var authorizedStateDeleteSteps = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Delete, globalFunctions.artifactTypeEnum.testStep);

                    isAuthorizedToCreateSteps = authorizedStateCreateSteps != globalFunctions.authorizationStateEnum.prohibited;
                    isAuthorizedToDeleteSteps = authorizedStateDeleteSteps != globalFunctions.authorizationStateEnum.prohibited;
                }
                //Now update permissions on relevant test step buttons
                if (typeof (lnkInsertStep) !== 'undefined')   { $find(lnkInsertStep).set_authorized(isAuthorizedToCreateSteps); }
                if (typeof (lnkInsertLink) !== 'undefined')   { $find(lnkInsertLink).set_authorized(isAuthorizedToCreateSteps); }
                if (typeof (lnkCopyStep) !== 'undefined')     { $find(lnkCopyStep).set_authorized(isAuthorizedToCreateSteps); }
                if (typeof (lnkDeleteStep) !== 'undefined') { $find(lnkDeleteStep).set_authorized(isAuthorizedToDeleteSteps); }

              
            }
            
            //Update WorX URLs
            if ('<%=Feature_Local_Worx%>' == "true")
            {
                //Set the dropdown URLs
                var mnuWorX = $find('<%=mnuWorX.ClientID%>');
                if (mnuWorX)
                {
                    mnuWorX.get_items()[0].set_navigateUrl("worx:spira/open/pr" + SpiraContext.ProjectId + "/tc" + SpiraContext.ArtifactId);
                    mnuWorX.get_items()[1].set_navigateUrl("worx:spira/execute/pr" + SpiraContext.ProjectId + "/tc" + SpiraContext.ArtifactId);
                    mnuWorX.get_items()[2].set_navigateUrl("worx:spira/review/pr" + SpiraContext.ProjectId + "/tc" + SpiraContext.ArtifactId);
                    mnuWorX.refreshItems();
                }
            }

            //See if we should display or hide the test steps based on permissions
            showHideTestSteps();

            //Prevent test execution if workflow rules specify
            var ajxFormManager = $find(ajxFormManager_id);
            var dataItem = ajxFormManager.get_dataItem();
            if (dataItem && dataItem.Fields)
            {
                checkWorkflowStatus(dataItem.Fields);
            }
        }

        function updatePageStatusContent(fields)
        {
            checkWorkflowStatus(fields);
        }

        function showHideTestSteps()
        {
            var isVisible = true;

            //First see if we have permissions to view test steps
            //Display the test steps if we own the test case and have limited view, otherwise not
            var ajxFormManager = $find(ajxFormManager_id);
            var isCreatorOrOwner = ajxFormManager.get_isArtifactCreatorOrOwner();
            var authorizedViewTS =globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.testStep);
            if (authorizedViewTS == globalFunctions.authorizationStateEnum.prohibited)
            {
                isVisible = false;
            }
            if (authorizedViewTS == globalFunctions.authorizationStateEnum.limited && !isCreatorOrOwner)
            {
                isVisible = false;
            }

            if (isVisible)
            {
                $('#' + pnlOverview_TestSteps_id).show();
            }
            else
            {
                $('#' + pnlOverview_TestSteps_id).hide();
            }

            //If we are the owner/creator then also allow test step editing if limited modify permission of test steps
            var isAuthorizedToModifySteps = false;
            var authorizedModifyTS = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, globalFunctions.artifactTypeEnum.testStep);
            if (authorizedModifyTS == globalFunctions.authorizationStateEnum.authorized)
            {
                isAuthorizedToModifySteps = true;
            }
            if (authorizedModifyTS == globalFunctions.authorizationStateEnum.limited && isCreatorOrOwner)
            {
                isAuthorizedToModifySteps = true;
            }
            //Check workflow status as well
            var dataItem = ajxFormManager.get_dataItem();
            if (dataItem && dataItem.Fields._AreTestStepsEditableInStatus && dataItem.Fields._AreTestStepsEditableInStatus.textValue == 'N')
            {
                isAuthorizedToModifySteps = false;
            }
            var grdTestSteps = $find(grdTestSteps_id);
            if (grdTestSteps.get_allowEdit() != isAuthorizedToModifySteps)
            {
                grdTestSteps._clearTableRowHandlers();
                grdTestSteps.set_allowEdit(isAuthorizedToModifySteps);
                grdTestSteps.load_data();
            }
        }
        function checkWorkflowStatus(fields)
        {
            //See if we can execute the test case in its current status - if the execute button exists at all (can be hidden server side due to projectSettings)
            var btnExecuteTest = $find(btnExecuteTest_id);
            if (btnExecuteTest) {
                if (fields._IsTestCaseInExecutableStatus && fields._IsTestCaseInExecutableStatus.textValue == 'N') {
                    btnExecuteTest.set_enabled(false);
                } else {
                    //Enable the button based on role
                    btnExecuteTest.set_enabled(btnExecuteTest.get_authorized());
                }
            }

            //See if we can edit the steps in its current status
            if (fields._AreTestStepsEditableInStatus && fields._AreTestStepsEditableInStatus.textValue == 'N')
            {
                //Disable the buttons
                var lnkInsertStep = $find('<%=this.lnkInsertStep.ClientID%>');
                lnkInsertStep.set_enabled(false);
                var lnkInsertLink = $find('<%=this.lnkInsertLink.ClientID%>');
                lnkInsertLink.set_enabled(false);
                var lnkCopyStep = $find('<%=this.lnkCopyStep.ClientID%>');
                lnkCopyStep.set_enabled(false);
                var lnkDeleteStep = $find('<%=this.lnkDeleteStep.ClientID%>');
                lnkDeleteStep.set_enabled(false);
                var lnkImportSteps = $find('<%=this.lnkImportSteps.ClientID%>');
                lnkImportSteps.set_enabled(false);
            }
            else
            {
                //Enable the buttons
                var lnkInsertStep = $find('<%=this.lnkInsertStep.ClientID%>');
                lnkInsertStep.set_enabled(lnkInsertStep.get_authorized());
                var lnkInsertLink = $find('<%=this.lnkInsertLink.ClientID%>');
                lnkInsertLink.set_enabled(lnkInsertLink.get_authorized());
                var lnkCopyStep = $find('<%=this.lnkCopyStep.ClientID%>');
                lnkCopyStep.set_enabled(lnkCopyStep.get_authorized());
                var lnkDeleteStep = $find('<%=this.lnkDeleteStep.ClientID%>');
                lnkDeleteStep.set_enabled(lnkDeleteStep.get_authorized());
                var lnkImportSteps = $find('<%=this.lnkImportSteps.ClientID%>');
                lnkImportSteps.set_enabled(lnkImportSteps.get_authorized());
            }
        }

        //These two functions need to match the names in the AJAX web service
        function TestStepService_onLinkMouseOver(primaryKey, evt)
        {
            var grdTestSteps = $find('<%=grdTestSteps.ClientID%>');
            var args = {};
            args.thisRef = grdTestSteps;
            args.primaryKey = primaryKey;
            args.webServiceClass = <%=grdTestSteps.WebServiceClass%>;
	       grdTestSteps.onNameDescMouseOver(evt, args);
       }
       function TestStepService_onLinkMouseOut(evt)
       {
           var grdTestSteps = $find('<%=grdTestSteps.ClientID%>');
          var args = {};
          args.thisRef = grdTestSteps;
          args.primaryKey = -1;
          args.webServiceClass = null;
          grdTestSteps.onNameDescMouseOut(evt, args);
       }

        function ddlAutomationEngine_selected(args)
        {
            //If no engine is selected, disable the automation items
            var ddlAutomationEngine = $find('<%=ddlAutomationEngine.ClientID%>');
	        var radAttached = $get('<%=radAttached.ClientID%>');
	        var radLinked = $get('<%=radLinked.ClientID%>');
            var radRepository = $get('<%=radRepository.ClientID %>');
            var lblAttached = $get('<%=lblAttached.ClientID%>');
            var lblLinked = $get('<%=lblLinked.ClientID%>');
            var lblRepository = $get('<%=lblRepository.ClientID %>');
	        var txtAutomationScript = $get('<%=txtAutomationScript.ClientID%>');
	        var txtAutomationLink = $get('<%=txtAutomationLink.ClientID%>');
	        var txtAutomationVersion = $get('<%=txtAutomationVersion.ClientID%>');
	        var ddlDocFolder = $find('<%=ddlDocFolder.ClientID%>');
	        var ddlDocType = $find('<%=ddlDocType.ClientID%>');
            var divAutomationRepository = $get('<%=divAutomationRepository.ClientID%>');

            if (ddlAutomationEngine.get_selectedItem().get_value() == '')
            {
                radAttached.disabled = true;
                radLinked.disabled = true;
                radRepository.disabled = true;
                lblAttached.setAttribute('disabled', 'disabled');
                lblLinked.setAttribute('disabled', 'disabled');
                lblRepository.setAttribute('disabled', 'disabled');
                divAutomationRepository.style.display = 'none';
                txtAutomationLink.style.display = 'inline';
                txtAutomationScript.readOnly = true;
                txtAutomationLink.readOnly = true;
                txtAutomationVersion.readOnly = true;
                if (radRepository.checked)
                {
                    radAttached.checked = true;
                }
				automationScriptSection.style.display = 'none';
                txtAutomationScript.style.display = 'none';
                $(txtAutomationLink).addClass('disabled');
                $(txtAutomationVersion).addClass('disabled');
            }
            else
            {
                radAttached.disabled = false;
                radLinked.disabled = false;
                lblAttached.removeAttribute('disabled');
                lblLinked.removeAttribute('disabled');
                txtAutomationLink.style.display = 'inline';
                if (radRepository.checked)
                {
                    divAutomationRepository.style.display = '';
                    txtAutomationLink.style.display = 'none';
                    txtAutomationScript.readOnly = true;
                    txtAutomationVersion.readOnly = true;
                    $(txtAutomationVersion).addClass('disabled');
					automationScriptSection.style.display = '';
                    txtAutomationScript.style.display = 'inline';
                }
                else if (radAttached.checked)
                {
                    divAutomationRepository.style.display = 'none';
                    txtAutomationLink.style.display = 'inline';
                    txtAutomationScript.readOnly = false;
                    txtAutomationVersion.readOnly = false;
                    $(txtAutomationVersion).removeClass('disabled');
					automationScriptSection.style.display = '';
                    txtAutomationScript.style.display = 'inline';
                }
                else if (radLinked.checked)
                {
                    divAutomationRepository.style.display = 'none';
                    txtAutomationLink.style.display = 'inline';
                    txtAutomationScript.readOnly = true;
                    txtAutomationVersion.readOnly = false;
                    $(txtAutomationVersion).removeClass('disabled');
					automationScriptSection.style.display = 'none';
                    txtAutomationScript.style.display = 'none';
                }
                else
                {
                    //Default to a linked script
                    radLinked.checked = true;
                    divAutomationRepository.style.display = 'none';
                    txtAutomationLink.style.display = 'inline';
                    txtAutomationScript.readOnly = true;
                    txtAutomationVersion.readOnly = false;
                    $(txtAutomationVersion).removeClass('disabled');
					automationScriptSection.style.display = '';
                    txtAutomationScript.style.display = 'inline';
                }
                txtAutomationLink.readOnly = false;
                $(txtAutomationLink).removeClass('disabled');
            }
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
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestCaseSummary%>";
                filter = "&af_5_87=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestCaseDetailed%>";
                filter = "&af_6_87=";
            }

            //Open the report for the specified format
            globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
        }

        //Initialize certain handlers
        $(document).ready(function() {
			$('#btnAddParameter').on("click", function() { page.testCaseParameters_add(); });
			$('#lnkAddTestLink').on("click", function() { page.pnlInsertTestLink_lnkAddTestLink_click(); });
			$('#btnImportTestCase').on("click", function() { page.pnlImportTestCase_btnImportTestCase_click(); });
			$('#<%=txtAutomationScript.ClientID%>').on("keydown", function(evt) { page.txtAutomationScript_changed(evt); });
			$('input:radio[name=radLinkType]').on("click", function () { page.radLinkType_clicked(); });

            //Display WorX
            if ('<%=Feature_Local_Worx%>' == "true")
            {
                document.getElementById('plcWorX').classList.remove("dn");
            }
        });
	</script>

    <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
  <script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js"></script> 
  <script>
      $(document).ready(function () {
          $('#cplMainContent_btnRecord').click(function () {
              $('#dialog').dialog();
          });
      });

  </script>
  
	<style>
	.ui-dialog{
	z-index:10000
	}

    .recordicon{
        margin-left:0px;
    }
	
	.fixed .record {
	padding:0;
	width:auto;
	height:auto;
	padding-bottom: 2px;
	padding-top: 2px;
	}
	
	.fixed #recordGlyphicon {
		font-size: 10px;
	}
	
	.fixed #cplMainContent_link1{
		font-size: 8pt !important;
	}
	</style>
		<div style="display:none">
					
					
					<div style="width: auto; min-height: 81px; max-height: none; height: auto;" class="ui-dialog-content ui-widget-content" id="dialog">
						<h3>We're opening Test Case in Validation TestMaster...</h3>
						<h4>Did you run into issues?</h4>
						<a href="https://validationmaster.com/wp-content/uploads/elementor/testmaster/TestMaster.msi">Download Validation TestMaster</a>
						<p style="color:darkgray">Validation TestMaster should be preinstalled on your PC</p>
					</div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-n"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-e"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-s"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-w"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-se ui-icon ui-icon-gripsmall-diagonal-se"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-sw"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-ne"></div>
				  <div style="z-index: 90;" class="ui-resizable-handle ui-resizable-nw"></div>
		</div>
</asp:Content>
