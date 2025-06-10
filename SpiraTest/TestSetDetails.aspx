<%@ Page 
    AutoEventWireup="true" 
    CodeBehind="TestSetDetails.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.TestSetDetails" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestRunListPanel" Src="UserControls/TestRunListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentListPanel" Src="UserControls/IncidentListPanel.ascx" %>

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
                            Authorized_ArtifactType="TestSet" 
                            Authorized_Permission="Modify" 
                            ClientScriptMethod="refresh_data();" 
                            ClientScriptServerControlId="navTestSetList" 
                            CssClass="FolderTree" 
                            EditDescriptions="false"
                            ErrorMessageControlId="lblMessage"
                            ID="trvFolders" 
                            ItemName="<%$Resources:Fields,Folder %>"
                            LoadingImageUrl="Images/action-Spinner.svg" 
                            NodeLegendFormat="{0}" 
                            runat="server" 
                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"
                            />
                </div>    
            </tstsc:SidebarPanel>

            <tstsc:NavigationBar 
                AutoLoad="true"
                BodyHeight="580px" 
                ErrorMessageControlId="lblMessage"
                ID="navTestSetList" 
                ItemImage="Images/artifact-TestSet.svg"
                ListScreenCaption="<%$Resources:Main,TestSetDetails_BackToList%>"
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"
                EnableLiveLoading="true" 
                FormManagerControlId="ajxFormManager"
                />
		</div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="clearfix">
                    <div class="btn-group priority1 hidden-md hidden-lg mr3" role="group">
                        <tstsc:HyperLinkEx 
                            ID="btnBack" 
                            NavigateUrl="<%#ReturnToTestSetListUrl%>" 
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
                                    Authorized_ArtifactType="TestSet" 
                                    Authorized_Permission="Modify" 
                                    ClientScriptMethod="save_data(null, 'close'); void(0);" 
                                    GlyphIconCssClass="mr3 far fa-file-excel" 
                                    ID="DropMenuItem2" 
                                    Name="SaveAndClose" 
                                    runat="server" 
                                    Value="<%$Resources:Buttons,SaveAndClose %>" 
                                    />
								<tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestSet" 
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
                            ClientScriptMethod="load_data()" 
                            ClientScriptServerControlId="ajxFormManager" 
                            ConfirmationMessage="<%$Resources:Messages,TestSetDetails_RefreshConfirm %>"  
                            GlyphIconCssClass="mr3 fas fa-sync" 
                            ID="btnRefresh" 
                            runat="server" 
                            Text="<%$Resources:Buttons,Refresh %>" 
                            />
    					<tstsc:DropMenu 
                            Authorized_ArtifactType="TestSet" 
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
                                <tstsc:DropMenuItem Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="TestSet" />
                                <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="TestSet" />
                            </DropMenuItems>
						</tstsc:DropMenu>
                    </div>
                    <div 
                        class="btn-group priority3" 
                        role="group"
                        >
						<tstsc:DropMenu 
                            Authorized_ArtifactType="TestSet" 
                            Authorized_Permission="Delete" 
                            Confirmation="True" 
                            ConfirmationMessage="<%$Resources:Messages,TestSetDetails_DeleteConfirm %>"  
                            GlyphIconCssClass="mr3 fas fa-trash-alt" 
                            ID="btnDelete" 
                            runat="server" 
                            SkinID="HideOnScroll" 
                            Text="<%$Resources:Buttons,Delete %>" 
                            ClientScriptMethod="delete_item()" 
                            ClientScriptServerControlId="ajxFormManager" 
                            />
                    </div>
                    <div 
                        class="btn-group priority1" 
                        role="group"
                        >
						<tstsc:DropMenu 
                            Authorized_ArtifactType="TestRun" 
                            Authorized_Permission="Create" 
                            ClientScriptMethod="page.execute_test_set()" 
                            Confirmation="False" 
                            GlyphIconCssClass="mr3 fas fa-play" 
                            ID="btnExecuteTestSet" 
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
                                    Authorized_ArtifactType="TestSet" 
                                    Authorized_Permission="View" 
                                    ClientScriptMethod="print_item('html')" 
                                    GlyphIconCssClass="mr3 fas fa-print" 
                                    Name="Print" 
                                    Value="<%$Resources:Dialogs,Global_PrintItems %>" 
                                    />
                                <tstsc:DropMenuItem Divider="true" />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestSet" 
                                    Authorized_Permission="View" 
                                    ClientScriptMethod="print_item('excel')"
                                    ImageUrl="Images/Filetypes/Excel.svg" 
                                    Name="ExportToExcel" 
                                    Value="<%$Resources:Dialogs,Global_ExportToExcel %>" 
                                    />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestSet" 
                                    Authorized_Permission="View" 
                                    ClientScriptMethod="print_item('word')" 
                                    ImageUrl="Images/Filetypes/Word.svg" 
                                    Name="ExportToWord" 
                                    Value="<%$Resources:Dialogs,Global_ExportToWord %>" 
                                    />
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestSet" 
                                    Authorized_Permission="View" 
                                    ClientScriptMethod="print_item('pdf')" 
                                    ImageUrl="Images/Filetypes/Acrobat.svg" 
                                    Name="ExportToPdf" 
                                    Value="<%$Resources:Dialogs,Global_ExportToPdf %>" 
                                    />
                            </DropMenuItems>
                        </tstsc:DropMenu>
                    </div>
                    <div 
                        id="pnlEmailToolButtons"
                        class="btn-group priority2" 
                        role="group"
                        >
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
                            Text="<%$Resources:Buttons,Subscribe %>" 
                            >
                            <DropMenuItems>
                                <tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestSet"
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
                        <div class="py1 pr4 dif items-center ma0-children fs-h4 fs-h6-xs">
                            <tstsc:ImageEx 
                                AlternateText="<%$Resources:Fields,TestSet %>" 
                                CssClass="w5 h5"
                                ID="imgTestSet" 
                                ImageUrl="Images/artifact-TestSet.svg" 
                                runat="server" 
                                />
                            <span class="pl4 silver nowrap">
                                <tstsc:LabelEx 
                                    CssClass="pointer dib orange-hover transition-all"
                                    title="<%$Resources:Buttons,CopyToClipboard %>"
                                    data-copytoclipboard='true'
                                    ID="lblTestSetId" 
                                    runat="server" 
                                    />
                            </span>
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ddlTestRunType" 
                                ID="ddlTestRunTypeLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,Type %>" 
                                />
							<tstsc:UnityDropDownListEx 
                                CssClass="u-dropdown"
                                DataMember="TestRunType" 
                                DataTextField="Name" 
                                DataValueField="TestRunTypeId" 
                                DisabledCssClass="u-dropdown disabled" 
                                ID="ddlTestRunType" 
                                runat="server" 
                                />
                        </div>
                         <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true"  
                                AssociatedControlID="ddlTestSetStatus" 
                                ID="ddlTestSetStatusLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,Status %>" 
                                />
                            <tstsc:UnityDropDownListEx 
                                CssClass="u-dropdown" 
                                DataTextField="Name" 
                                DataValueField="TestSetStatusId" 
                                DisabledCssClass="u-dropdown disabled"
                                ID="ddlTestSetStatus" 
                                runat="server" 
                                />
                        </div>
                        <div class="py1 dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="eqlExecutionStatus"
                                CssClass="silver"
                                ID="eqlExecutionStatusLabel" 
                                Required="false" 
                                runat="server" 
                                Text="<%$Resources:Fields,ExecutionStatus %>" 
                                />
                            <span class="pl3 pr4">
                                <tstsc:Equalizer 
                                    ID="eqlExecutionStatus" 
                                    runat="server" 
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
                ArtifactId="<%# this.testSetId %>" 
                ArtifactTypeEnum="TestSet" 
                ID="tstEmailPanel" 
                runat="server" 
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="Incident" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />

            <div class="main-content">
				<tstsc:TabControl 
                    CssClass="TabControl2" 
                    DividerCssClass="Divider" 
                    ID="tclTestSetDetails" 
                    runat="server"
                    TabCssClass="Tab" 
                    TabHeight="25" 
                    TabWidth="100" 
                    SelectedTabCssClass="TabSelected" 
                    >
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
                            AjaxControlContainer="tstTestRunListPanel" 
                            AjaxServerControlId="grdTestRunList" 
                            AuthorizedArtifactType="TestRun" 
                            Caption="<%$Resources:ServerControls,TabControl_TestRuns %>" 
                            CheckPermissions="true" 
                            ID="tabTestRuns" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_TESTRUN %>"
                            TabPageControlId="pnlTestRuns" 
                            TabPageImageUrl="Images/artifact-TestRun.svg"
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
                            AjaxControlContainer="tstIncidentListPanel" 
                            AjaxServerControlId="grdIncidentList" 
                            Caption="<%$Resources:ServerControls,TabControl_Incidents %>" 
                            ID="tabIncidents" 
                            runat="server" 
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_INCIDENT %>"
                            TabPageControlId="pnlIncidents" 
                            TabPageImageUrl="Images/artifact-Incident.svg"
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
                                id="form-group_releases" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Main,SiteMap_Releases %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
									<ul class="u-box_list">
										<li class="ma0 mb2 pa0">
											<tstsc:LabelEx
												AppendColon="true"
												AssociatedControlID="ddlRelease"
												ID="ddlReleaseLabel"
												Required="false"
												runat="server"
												Text="<%$Resources:Fields,ScheduledRelease %>" />
											<tstsc:UnityDropDownHierarchy
												ID="ddlRelease"
												runat="server"
												NoValueItem="true"
												NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
												ActiveItemField="IsActive"
												NavigateToText="<%$Resources:ClientScript,DropDownHierarchy_NavigateToRelease %>"
												DataTextField="FullName"
												SkinID="ReleaseDropDownListFarRight"
												DataValueField="ReleaseId" />
										</li>
										<li class="ma0 pa0">
											<tstsc:LabelEx
												AppendColon="true"
												AssociatedControlID="lblDisplayForRelease"
												ID="lblDisplayForReleaseLabel"
												Required="false"
												runat="server"
												Text="<%$Resources:Fields,DisplayForRelease %>" />
											<asp:Label
												ID="lblDisplayForRelease"
												runat="server" />
										</li>
									</ul>
                                
                            </div>

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
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
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
                                            AssociatedControlID="ddlAutomationHost" 
                                            ID="ddlAutomationHostLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,AutomationHostId %>" 
                                            />
										<tstsc:UnityDropDownListEx 
                                            ClientScriptMethod="page.ddlAutomationHost_changed" 
                                            CssClass="u-dropdown" 
                                            DataMember="AutomationHost" 
                                            DataTextField="Name" 
                                            DataValueField="AutomationHostId" 
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlAutomationHost" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                            />
									</li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlTestConfigurationSet" 
                                            ID="ddlTestConfigurationSetLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,TestConfigurationSetId %>" 
                                            />
										<tstsc:UnityDropDownListEx 
                                            ClientScriptMethod="page.ddlAutomationHost_changed" 
                                            CssClass="u-dropdown" 
                                            DataMember="AutomationHost" 
                                            DataTextField="Name" 
                                            DataValueField="TestConfigurationSetId" 
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlTestConfigurationSet" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
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
                                id="form-group_dates" 
                                >
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
                                    <li class="ma0 pa0">
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
                                    <li class="ma0 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lblExecutionDate" 
                                            ID="lblExecutionDateLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ExecutionDate %>" 
                                            />
										<asp:Label 
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
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="datPlannedDate" 
                                            ID="datPlannedDateLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,PlannedDate %>" 
                                            />
										<tstsc:UnityDateTimePicker
                                            ID="datPlannedDate" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlRecurrence" 
                                            ID="ddlRecurrenceLabel" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,RecurrenceId %>" 
                                            />
                                       	<tstsc:UnityDropDownListEx 
                                                CssClass="u-dropdown" 
                                                DataTextField="Name" 
                                                DataValueField="RecurrenceId" 
                                                DisabledCssClass="u-dropdown disabled"
                                                ID="ddlRecurrence" 
                                                NoValueItem="true" 
                                                NoValueItemText="<%$Resources:Main,TestSetDetails_RecurrenceNone %>" 
                                                runat="server"
                                                />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="chkIsAutoScheduled" 
                                            Required="false" 
                                            runat="server" ID="chkIsAutoScheduledLabel" 
                                            Text="<%$Resources:Fields,IsAutoScheduled %>" 
                                            />
                                        <tstsc:CheckBoxYnEx 
                                            ID="chkIsAutoScheduled" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            runat="server" 
                                            ID="txtBuildExecuteTimeIntervalLabel" 
                                            AssociatedControlID="txtBuildExecuteTimeInterval" 
                                            Text="<%$Resources:Fields,BuildExecuteTimeIntervalWithSeconds %>" 
                                            Required="false" 
                                            AppendColon="true" 
                                            />
                                        <tstsc:UnityTextBoxEx 
                                            CssClass="u-input w7"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtBuildExecuteTimeInterval" 
                                            MaxLength="3" 
                                            runat="server" 
                                            type="text"
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
                                            Authorized_ArtifactType="TestSet" 
                                            Authorized_Permission="Modify"
                                            ID="txtDescription" 
                                            runat="server" 
                                            Screenshot_ArtifactType="TestSet"
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
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="pnlOverview_Parameters" 
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_Parameters %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <div class="u-box_item mb5" >
                                    <tstsc:MessageBox 
                                        ID="msgTestSetParameters" 
                                        runat="server" 
                                        SkinID="MessageBox" 
                                        />
                                    <div class="alert alert-warning alert-narrow">
                                        <span class="fas fa-info-circle"></span>
                                        <asp:Localize 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestSetDetails_ParametersIntro %>" 
                                            />
                                        <button 
                                            class="btn btn-default btn-sm"
                                            id="btnAddTestSetParameterValue" 
                                            type="button" 
                                            >
                                            <span class="fas fa-plus">
                                            </span>
                                            <asp:Localize 
                                                runat="server" 
                                                Text="<%$Resources:Main,TestSetDetails_AddParameterValue %>" 
                                                />
                                        </button>
                                    </div>

                                    <table 
                                        id="tblTestSetParameterValues" 
                                        class="DataGrid dt-fixed w-100" 
                                        >
                                        <thead>
                                            <tr class="Header">
                                                <th>
                                                    <asp:Localize runat="server" Text="<%$Resources:Fields,Parameter %>" />
                                                </th>
                                                <th>
                                                    <asp:Localize runat="server" Text="<%$Resources:Main,TestSetDetails_TestSetValue %>" />
                                                </th>
                                                <th>
                                                    <asp:Localize runat="server" Text="<%$Resources:Fields,DefaultValue %>" />
                                                </th>
                                                <th>
                                                    <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Fields,Operations %>" />
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody data-bind="foreach: $root">
                                            <tr>
                                                <td>
                                                    ${<span data-bind="text: Fields.Name.textValue"></span>}
                                                </td>
                                                <td class="fw-b">
                                                    <span 
                                                        class="w-100"
                                                        data-bind="text: (Fields.Value.textValue() && Fields.Value.textValue() != '') ? Fields.Value.textValue : '-', style: { display: (editable()) ? 'none' : 'inline-block' }"></span>
                                                    <input 
                                                        type="text" 
                                                        maxlength="255" 
                                                        class="u-input u-input_fullBox is-active w-100" 
                                                        data-bind="textInput: Fields.Value.textValue, style: { display: (editable()) ? 'inline-block' : 'none' }" 
                                                        />
                                                </td>
                                                <td>
                                                    <span data-bind="text: (Fields.DefaultValue.textValue() && Fields.DefaultValue.textValue() != '') ? Fields.DefaultValue.textValue : '-'"></span>
                                                </td>
                                                <td>
                                                    <div class="btn-group" role="group">
                                                        <button 
                                                            class="btn btn-xs btn-default"
                                                            data-bind="click: function(data,evt) { page.testSetParameters_edit(data); }, visible: !editable() "
                                                            type="button" 
                                                            >
                                                            <span class="fas fa-edit fa-fw"></span>
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>" />
                                                        </button>
                                                        <button 
                                                            class="btn btn-xs btn-primary"
                                                            data-bind="click: function(data,evt) { page.testSetParameters_save(data); }, visible: editable()"
                                                            type="button" 
                                                            >
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Save %>" />
                                                        </button>
                                                        <button 
                                                            class="btn btn-xs btn-default"
                                                            data-bind="click: function(data,evt) { page.testSetParameters_edit(data); }, visible: editable() "
                                                            type="button" 
                                                            >
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Cancel %>" />
                                                        </button>
                                                        <button 
                                                            type="button" 
                                                            class="btn btn-xs btn-default"
                                                            data-bind="click: function(data,evt) { page.testSetParameters_delete(data); }"
                                                            >
                                                            <span class="fas fa-trash-alt fa-fw"></span>
                                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
                                                        </button>
                                                    </div>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                    <tstsc:DialogBoxPanel 
                                        CssClass="PopupPanel"
                                        ID="pnlAddTestSetParameter" 
                                        Modal="false" 
                                        runat="server" 
                                        Title="<%$Resources:Main,TestSetDetails_AddParameterValue %>"
                                        Width="500px" 
                                        >
                                        <tstsc:MessageBox 
                                            ID="msgAddTestSetParameter" 
                                            runat="server" 
                                            SkinID="MessageBox" 
                                            Width="100%" 
                                            />
                                        <div class="my3 u-wrapper">
                                            <div class="u-box_3">
                                                <ul class="u-box_list">
                                                    <li class="ma0 pa0 mb3">
                                                        <tstsc:LabelEx 
                                                            AppendColon="true" 
                                                            AssociatedControlID="ddlParameterName"
                                                            ID="ddlParameterNameLabel" 
                                                            Required="true" 
                                                            runat="server" 
                                                            Text="<%$Resources:Fields,Parameter %>" 
                                                            />
                                                        <tstsc:UnityDropDownListEx 
                                                            CssClass="u-dropdown"
                                                            DataTextField="Name" 
                                                            DataValueField="TestCaseParameterId"
                                                            DisabledCssClass="u-dropdown disabled"
                                                            ID="ddlParameterName" 
                                                            NoValueItem="true" 
                                                            NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" 
                                                            runat="server" 
                                                            />
                                                    </li>
                                                    <li class="ma0 pa0">
                                                        <tstsc:LabelEx 
                                                            AppendColon="true" 
                                                            AssociatedControlID="txtNewParameterValue"
                                                            ID="txtNewParameterValueLabel" 
                                                            Required="true" 
                                                            runat="server" 
                                                            Text="<%$Resources:Fields,Value %>" 
                                                            />
                                                        <tstsc:UnityTextBoxEx 
                                                            CssClass="u-input is-active"
                                                            DisabledCssClass="u-input disabled"
                                                            ID="txtNewParameterValue" 
                                                            MaxLength="255" 
                                                            runat="server" 
                                                            />
                                                    </li>
                                                </ul>

                                                <div class="btn-group">
                                                    <tstsc:ButtonEx 
                                                        Authorized_ArtifactType="TestSet" 
                                                        Authorized_Permission="Modify" 
                                                        ClientScriptMethod="page.testSetParameters_add()" 
                                                        ID="btnParametersSave" 
                                                        runat="server" 
                                                        SkinID="ButtonPrimary"
                                                        Text="<%$Resources:Buttons,Save %>" 
                                                        />
								                    <tstsc:ButtonEx 
                                                        ClientScriptMethod="close()" 
                                                        ClientScriptServerControlId="pnlAddTestSetParameter"
                                                        ID="btnParametersCancel" 
                                                        runat="server" 
                                                        Text="<%$Resources:Buttons,Cancel %>" 
                                                        />
                                                </div>
                                            </div>
                                        </div>
                                    </tstsc:DialogBoxPanel>
                                </div>
                            </div>
                        </div>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="pnlOverview_TestCases" 
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_TestCases %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <div class="u-box_item mb5" >
                                    <div runat="server" id="addTestCasePanelId"></div>
									<div class="TabControlHeader w-100">
                                        <div class="btn-group priority1">
											<tstsc:HyperLinkEx 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                ClientScriptMethod="displayAddTestCases(event)" 
                                                ID="lnkAddTestCases" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server" 
                                                SkinID="ButtonDefault"
                                                >
                                                <span class="fas fa-plus"></span>
                                                <asp:Localize 
                                                    runat="server" 
                                                    Text="<%$Resources:Buttons,Add %>" 
                                                    />
											</tstsc:HyperLinkEx>
											<tstsc:HyperLinkEx 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                ClientScriptMethod="delete_items()" 
                                                ClientScriptServerControlId="grdTestCases" 
                                                ID="lnkRemoveTestCases" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server"  
                                                SkinID="ButtonDefault"
                                                >
                                                <span class="fas fa-trash-alt"></span>
                                                <asp:Localize 
                                                    runat="server" 
                                                    Text="<%$Resources:Buttons,Remove %>" 
                                                    />
											</tstsc:HyperLinkEx>
											<tstsc:HyperLinkEx 
                                                ClientScriptMethod="load_data()" 
                                                ClientScriptServerControlId="grdTestCases" 
                                                ID="lnkRefreshTestCases" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server"  
                                                SkinID="ButtonDefault"
                                                >
                                                <span class="fas fa-sync"></span>
                                                <asp:Localize 
                                                    runat="server" 
                                                    Text="<%$Resources:Buttons,Refresh %>" 
                                                    />
											</tstsc:HyperLinkEx>
                                        </div>
                                        <div class="btn-group priority3">
											<tstsc:HyperLinkEx 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                ClientScriptMethod="page.displayEditTestCaseParameters(event)" 
                                                ID="lnkEditParameterValues" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server"  
                                                SkinID="ButtonDefault"
                                                >
                                                <span class="fas fa-code"></span>
                                                <asp:Localize 
                                                    runat="server" 
                                                    Text="<%$Resources:Dialogs,TestSetDetails_EditParameters %>" 
                                                    />
											</tstsc:HyperLinkEx>
				                            <tstsc:DropDownListEx 
                                                AutoPostBack="false" 
                                                ClientScriptMethod="toggle_visibility" 
                                                ClientScriptServerControlId="grdTestCases" 
                                                CssClass="DropDownList" 
                                                DataTextField="Value" 
                                                DataValueField="Key" 
                                                ID="ddlShowHideColumns" 
                                                NoValueItem="True" 
                                                NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" 
                                                Runat="server" 
                                                Width="180px" 
                                                />
											<tstsc:HyperLinkEx 
                                                Authorized_ArtifactType="TestRun" 
                                                Authorized_Permission="Create" 
                                                ClientScriptMethod="page.grdTestCases_execute()" 
                                                ID="lnkExecuteTestCases" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server"  
                                                SkinID="ButtonDefault"
                                                >
                                                <span class="fas fa-play"></span>
                                                <asp:Localize 
                                                    runat="server" 
                                                    Text="<%$Resources:Dialogs,TestSetDetails_ExecuteTests %>" 
                                                    />
											</tstsc:HyperLinkEx>
										</div>
                                        <div class="legend-group">
											<span class="Legend">
                                                <asp:Localize 
                                                    ID="Localize1" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,EstimatedDuration %>" 
                                                    />
												<span class="badge ma0-children">
                                                    <tstsc:LabelEx 
                                                        ID="lblEstimatedDuration" 
                                                        runat="server"
                                                        />
												</span>
											</span>&nbsp;/ <span class="Legend">
                                                <asp:Localize 
                                                    ID="Localize3" 
                                                    runat="server" 
                                                    Text="<%$Resources:Fields,ActualDuration %>" 
                                                    />
												<span class="badge ma0-children">
												    <tstsc:LabelEx 
                                                        ID="lblActualDuration" 
                                                        runat="server" 
                                                        />
												</span>
											</span>
										</div>
									</div>
                                    <tstsc:MessageBox 
                                        ID="lblTestCaseMessages" 
                                        runat="server" 
                                        SkinID="MessageBox" 
                                        />
									<tstsc:OrderedGrid 
                                        AllowInlineEditing="true" 
                                        AlternateItemImage="artifact-TestCase.svg" 
                                        Authorized_ArtifactType="TestSet" 
                                        Authorized_Permission="Modify" 
                                        AutoLoad="false"
                                        CssClass="DataGrid DataGrid-no-bands" 
                                        EditRowCssClass="Editing" 
                                        ErrorMessageControlId="lblTestCaseMessages" 
                                        HeaderCssClass="SubHeader" 
                                        ID="grdTestCases" 
                                        ItemImage="artifact-TestCaseNoSteps.svg" 
                                        RowCssClass="Normal" 
                                        runat="server"
                                        SelectedRowCssClass="Highlighted" 
                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetTestCaseService" 
                                        >
										<ContextMenuItems>
                                            <tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestCase" 
                                                Authorized_Permission="View" 
                                                Caption="<%$Resources:Buttons,OpenItem %>" 
                                                CommandArgument="_self" 
                                                CommandName="open_item" 
                                                GlyphIconCssClass="mr3 fas fa-mouse-pointer" 
                                                />
                                            <tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestCase" 
                                                Authorized_Permission="View" 
                                                Caption="<%$Resources:Buttons,OpenItemNewTab %>" 
                                                CommandName="open_item" 
                                                CommandArgument="_blank" 
                                                GlyphIconCssClass="mr3 fas fa-external-link-alt" 
                                                />
                                            <tstsc:ContextMenuItem Divider="True" />
											<tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                Caption="<%$Resources:Dialogs,TestSetDetails_InsertTests %>" 
                                                ClientScriptMethod="displayAddTestCases" 
                                                GlyphIconCssClass="mr3 fas fa-plus" 
                                                />
											<tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                Caption="<%$Resources:Dialogs,TestSetDetails_RemoveTests %>" 
                                                CommandName="delete_items" 
                                                GlyphIconCssClass="mr3 fas fa-trash-alt" 
                                                />
                                            <tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                Caption="<%$Resources:Buttons,EditItems%>" 
                                                CommandName="edit_items" 
                                                GlyphIconCssClass="mr3 far fa-edit" 
                                                />
											<tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                Caption="<%$Resources:Dialogs,TestSetDetails_EditParameters %>" 
                                                ClientScriptMethod="page.displayEditTestCaseParameters" 
                                                GlyphIconCssClass="mr3 fas fa-code" 
                                                />
											<tstsc:ContextMenuItem Divider="True" />
											<tstsc:ContextMenuItem 
                                                Authorized_ArtifactType="TestRun" 
                                                Authorized_Permission="Create" 
                                                Caption="<%$Resources:Dialogs,TestSetDetails_ExecuteTests %>" 
                                                ClientScriptMethod="page.grdTestCases_execute" 
                                                GlyphIconCssClass="mr3 fas fa-play" 
                                                />
										</ContextMenuItems>
									</tstsc:OrderedGrid>
									
                                    <tstsc:DialogBoxPanel 
                                        ID="pnlEditTestCaseParameters" 
                                        runat="server" 
                                        CssClass="PopupPanel" 
                                        Width="550px" 
                                        Height="320px"
                                        Title="<%$ Resources:Dialogs,TestSetDetails_TestCaseParametersTitle%>" 
                                        Modal="false"
                                        >
                                        <tstsc:MessageBox 
                                            ID="msgTestCaseParameters" 
                                            runat="server" 
                                            SkinID="MessageBox" 
                                            />
										<div 
                                            class="alert alert-warning alert-narrow ma3"
                                            id="divHasParameters" 
                                            >
                                            <span class="fas fa-info-circle"></span>
											<asp:Localize 
                                                ID="Localize9" 
                                                runat="server" 
                                                Text="<%$ Resources:Dialogs,TestSetDetails_TestCaseParametersLegend%>" 
                                                />
										</div>
										<div 
                                            class="alert alert-danger ma3"
                                            id="divNoParameters" 
                                            >
											<span class="fas fa-exclamation-triangle"></span>
											<asp:Localize 
                                                ID="Localize13" 
                                                runat="server" 
                                                Text="<%$ Resources:Dialogs,TestSetDetails_TestCaseParametersNone%>" 
                                                />
										</div>
										<div 
                                            class="mb4 ov-x-hidden ov-y-scroll"
                                            id="divTestCaseParametersEdit" 
                                            >
											<table 
                                                class="w-100"
                                                id="tblTestCaseParametersEdit" 
                                                >
                                                <tbody data-bind="foreach: $root">
                                                    <tr>
													    <td class="text-right pr3">
														    <label data-bind="text: Fields.Name.textValue, attr: { for: 'tblTestCaseParametersEdit' + primaryKey() }"></label>:
													    </td>
													    <td class="pb3">
                                                            <input 
                                                                class="u-input is-active" 
                                                                data-bind="textInput: Fields.Value.textValue, event: { keydown: Function.createDelegate(page, page.pnlEditTestCaseParameters_tblTestCaseParametersEdit_keydown) }, attr: { id: 'tblTestCaseParametersEdit' + primaryKey() }" 
                                                                maxlength="255"
                                                                type="text" 
                                                                />  
													    </td>
												    </tr>
                                                </tbody>
                                                <tfoot>
                                                    <tr>
                                                        <td></td>
                                                        <td>
                                                            <div class="btn-group mt3">
											                    <button 
                                                                    id="btnTestParametersUpdate" 
                                                                    type="button" 
                                                                    class="btn btn-primary"
                                                                    >
                                                                    <asp:Localize 
                                                                        ID="Localize14" 
                                                                        runat="server" 
                                                                        Text="<%$Resources:Buttons,Save %>" 
                                                                        />
											                    </button>
                                                                <tstsc:HyperLinkEx 
                                                                    ClientScriptMethod="close()" 
                                                                    ClientScriptServerControlId="pnlEditTestCaseParameters" 
                                                                    ID="lnkTestParametersCancel" 
                                                                    NavigateUrl="javascript:void(0)" 
                                                                    runat="server" 
                                                                    SkinID="ButtonDefault"
                                                                    Text="<%$Resources:Buttons,Cancel %>" 
                                                                    />
										                    </div>
                                                        </td>
                                                    </tr>
                                                </tfoot>
											</table>
										</div>
									</tstsc:DialogBoxPanel>
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
                                            ArtifactType="TestSet" 
                                            AutoLoad="false" 
                                            ErrorMessageControlId="lblMessage"
                                            ID="lstComments" 
                                            runat="server" 
									        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService" 
                                            Width="100%" 
                                            />
								        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="TestSet"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="txtNewComment" 
                                            runat="server" 
                                            Screenshot_ArtifactType="TestSet" 
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
				<asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlTestRuns" 
                    runat="server" 
                    Width="100%" 
                    >
					<tstuc:TestRunListPanel 
                        ID="tstTestRunListPanel" 
                        runat="server" 
                        />
				</asp:Panel>
				<asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlAttachments" 
                    runat="server" 
                    Width="100%" 
                    >
					<tstuc:AttachmentPanel 
                        ID="tstAttachmentPanel" 
                        runat="server" 
                        />
					<br />
				</asp:Panel>

                <asp:Panel 
                    ID="pnlIncidents" 
                    Width="100%" 
                    runat="server" 
                    CssClass="TabControlPanel"
                    >
					<tstuc:IncidentListPanel 
                        ID="tstIncidentListPanel" 
                        runat="server" 
                        />
				</asp:Panel>

				<asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlHistory" 
                    runat="server" 
                    Width="100%" 
                    >
					<tstuc:HistoryPanel 
                        ID="tstHistoryPanel" 
                        runat="server" 
                        />
					<br />
				</asp:Panel>
            </div>
        </div>
	</div>
    <tstsc:BackgroundProcessManager 
        ErrorMessageControlId="lblMessage"
        ID="ajxBackgroundProcessManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" 
        />
    <tstsc:AjaxFormManager 
        AlternateItemImage="Images/TestSetDynamic.gif"
        ArtifactImageControlId="imgTestSet" 
        ArtifactTypeName="<%$Resources:Fields,TestSet%>" 
        CheckUnsaved="true"
        ErrorMessageControlId="lblMessage" 
        ID="ajxFormManager" 
        ItemImage="Images/artifact-TestSet.svg" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService"
        DisplayPageName="true"
        NameField="Name"
        WorkflowEnabled="false" 
        FolderPathControlId="pnlFolderPath"
        >
	    <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblTestSetName" DataField="Name" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblTestSetId" DataField="TestSetId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlCreator" DataField="CreatorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlTestRunType" DataField="TestRunTypeId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlAutomationHost" DataField="AutomationHostId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="ddlTestSetStatus" DataField="TestSetStatusId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="datPlannedDate" DataField="PlannedDate" Direction="InOut" />
		    <tstsc:AjaxFormControl ControlId="lblExecutionDate" DataField="ExecutionDate" PropertyName="tooltip" Direction="In" />
		    <tstsc:AjaxFormControl ControlId="lblEstimatedDuration" DataField="EstimatedDuration" Direction="In" />
		    <tstsc:AjaxFormControl ControlId="lblActualDuration" DataField="ActualDuration" Direction="In" />
		    <tstsc:AjaxFormControl ControlId="ddlRecurrence" DataField="RecurrenceId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="eqlExecutionStatus" DataField="ExecutionStatusId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="chkIsAutoScheduled" DataField="IsAutoScheduled" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtBuildExecuteTimeInterval" DataField="BuildExecuteTimeInterval" Direction="InOut" PropertyName="intValue" />
            <tstsc:AjaxFormControl ControlId="ddlTestConfigurationSet" DataField="TestConfigurationSetId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblDisplayForRelease" DataField="DisplayReleaseId" Direction="In" />
	    </ControlReferences>
		<SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
		</SaveButtons>
    </tstsc:AjaxFormManager>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/TestSetTestCaseService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/AssociationService.svc" />  
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/Followers.js" />
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/TestSetDetails.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
	<script type="text/javascript">
	    var resx = Inflectra.SpiraTest.Web.GlobalResources;

	    SpiraContext.pageId = "Inflectra.Spira.Web.TestSetDetails";
	    SpiraContext.ArtifactId = <%=testSetId%>;
	    SpiraContext.ArtifactIdOnPageLoad = <%=testSetId%>;
        SpiraContext.ArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.TestSet%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
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
	    var grdTestCases_id = '<%=grdTestCases.ClientID%>';
	    var lblTestCaseMessages_id ='<%=lblTestCaseMessages.ClientID%>';
	    var msgTestCaseParameters_id ='<%=msgTestCaseParameters.ClientID%>';
	    var msgTestSetParameters_id ='<%=msgTestSetParameters.ClientID%>';
	    var ajxBackgroundProcessManager_id = '<%=ajxBackgroundProcessManager.ClientID%>';
	    var pnlEditTestCaseParameters_id = '<%=pnlEditTestCaseParameters.ClientID%>';
	    var ddlAutomationHost_id = '<%=ddlAutomationHost.ClientID%>';
	    var ddlTestRunType_id = '<%=ddlTestRunType.ClientID%>';
	    var pnlAddTestSetParameter_id = '<%=pnlAddTestSetParameter.ClientID%>';
	    var ddlParameterName_id = '<%=ddlParameterName.ClientID%>';
	    var txtNewParameterValue_id = '<%=txtNewParameterValue.ClientID%>';
	    var tabControl_id = '<%=this.tclTestSetDetails.ClientID%>';
	    var navigationBar_id = '<%=this.navTestSetList.ClientID%>';
	    var btnEmail_id = '<%=btnEmail.ClientID%>';

	    //TabControl Panel IDs
	    var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
	    var pnlHistory_id = '<%=pnlHistory.ClientID%>';
	    var pnlTestRuns_id = '<%=pnlTestRuns.ClientID%>';
	    var pnlIncidents_id = '<%=pnlIncidents.ClientID%>';

	    //Base URLs
	    var urlTemplate_artifactRedirectUrl = '<%=TestSetRedirectUrl %>';
	    var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToTestSetListUrl) %>';
	    var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
	    var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';
	    var urlTemplate_launchUrl = '<%=TestSetLaunchUrl %>';
	    var urlTemplate_testRunsPendingExploratory = '<%=TestRunsPendingExploratoryUrl %>';
	    var urlTemplate_testRunsPending = '<%=TestRunsPendingUrl %>';

	    //Create the page class
	    var page = $create(Inflectra.SpiraTest.Web.TestSetDetails, { testSetId: <%=testSetId%> });

	    //Initialize certain handlers
	    $(document).ready(function() {
			$('#btnTestParametersUpdate').on("click", function() { page.pnlEditTestCaseParameters_btnTestParametersUpdate_click(); });
			$('#btnAddTestSetParameterValue').on("click", function(evt) { page.btnAddTestSetParameterValue_click(evt); });
	    });

	    //Prints the current items
	    function print_item(format)
	    {
            var artifactId = SpiraContext.ArtifactId;

            //Get the report type from the format
            var reportToken;
            var filter;
            if (format == 'excel')
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestSetSummary%>";
                filter = "&af_8_91=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestSetDetailed%>";
                filter = "&af_8_91=";
            }

            //Open the report for the specified format
	        globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
	    }

	    //Updates any page specific content
	    var testSetDetails_testSetId = -1;
	    function updatePageContent()
	    {
	        //See if the artifact id has changed
	        var grdTestCases = $find('<%=this.grdTestCases.ClientID%>');
	        if (testSetDetails_testSetId != SpiraContext.ArtifactId)
	        {
	            var filters = {};
	            filters[globalFunctions.keyPrefix + 'TestSetId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
	            grdTestCases.set_standardFilters(filters);
	            grdTestCases.load_data();
	            testSetDetails_testSetId = SpiraContext.ArtifactId;

	            //Load the parameter values list
	            page.set_testSetId(SpiraContext.ArtifactId);
	            page.load_parameterValues();
	        }
	    }

        //Raised events
	    function ajxBackgroundProcessManager_success(msg, returnCode)
	    {
	        page.ajxBackgroundProcessManager_success(msg, returnCode);
	    }
	    function grdTestCases_loaded()
	    {
	        page.grdTestCases_loaded();
	    }
	    function displayAddTestCases()
	    {
	        //populate general data into the global panelAssociationAdd object, so it is accessible by React on render
	        panelAssociationAdd.lnkAddBtnId = '<%=lnkAddTestCases.ClientID%>';
	        panelAssociationAdd.addPanelId = '<%=addTestCasePanelId.ClientID%>';
	        panelAssociationAdd.addPanelObj = page;
	        panelAssociationAdd.displayType = <%=(int)Artifact.DisplayTypeEnum.TestSet_TestCases%>;
	        panelAssociationAdd.sortedGridId = '<%=grdTestCases.ClientID%>';
	        panelAssociationAdd.messageBox = '<%=lblTestCaseMessages.ClientID%>';
	        panelAssociationAdd.listOfViewableArtifactTypeIds = globalFunctions.artifactTypeEnum.testCase.toString();
        
	        //now render the panel
	        panelAssociationAdd.showPanel();
	    }
    </script>
</asp:Content>
