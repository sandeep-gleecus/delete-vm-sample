<%@ Page 
    AutoEventWireup="True"
    CodeBehind="IncidentDetails.aspx.cs" 
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
	Inherits="Inflectra.SpiraTest.Web.IncidentDetails" 
%>
<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
			<tstsc:NavigationBar 
                AutoLoad="true"  
                EnableLiveLoading="true" 
                ErrorMessageControlId="lblMessage" 
                FormManagerControlId="ajxFormManager"
                ID="navIncidentList" 
                IncludeAssigned="true" 
                IncludeDetected="true"
                ItemImage="Images/artifact-Incident.svg"
				ListScreenCaption="<%$Resources:Main,IncidentDetails_BackToList%>" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" 
                />
		</div>



        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="clearfix">
                    <div 
                        class="btn-group priority1 hidden-md hidden-lg mr3" 
                        role="group"
                        >
                        <tstsc:HyperLinkEx 
                            ID="btnBack" 
                            NavigateUrl=<%#ReturnToIncidentListUrl%> 
                            runat="server" 
                            SkinID="ButtonDefault" 
                            ToolTip="<%$Resources:Main,IncidentDetails_BackToList%>"
                            >
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperLinkEx>
                    </div>
                    <div 
                        id="plcNewArtifactSubmit" 
                        style="display:none"
                        >
                        <div class="btn-group priority1" role="group">
							<tstsc:DropMenu 
                                Authorized_ArtifactType="Incident" 
                                Authorized_Permission="Create" 
                                ClientScriptMethod="save_data(evt,'redirect')" 
                                ClientScriptServerControlId="ajxFormManager" 
                                GlyphIconCssClass="mr3 fas fa-save" 
                                ID="mnuSave" 
                                MenuWidth="125px" 
                                runat="server" 
                                Text="<%$Resources:Buttons,Save %>" 
                                />
							<tstsc:DropMenu 
                                Authorized_ArtifactType="Incident" 
                                Authorized_Permission="Create" 
                                ClientScriptMethod="save_data(evt, 'close')" 
                                ClientScriptServerControlId="ajxFormManager" 
                                GlyphIconCssClass="mr3 fas fa-save" 
                                ID="mnuSaveAndClose" 
                                MenuWidth="125px" 
                                runat="server" 
                                Text="<%$Resources:Buttons,SaveAndClose %>" 
                                />
							<tstsc:DropMenu 
                                Authorized_ArtifactType="Incident" 
                                Authorized_Permission="Create" 
                                ClientScriptMethod="save_data(evt, 'new')" 
                                ClientScriptServerControlId="ajxFormManager" 
                                GlyphIconCssClass="mr3 fas fa-plus" 
                                ID="mnuSaveAndNew" 
                                MenuWidth="125px" 
                                runat="server" 
                                Text="<%$Resources:Buttons,SaveAndNew %>" 
                                />
						</div>
				    </div>
				    <div id="plcToolbar">
					    <div 
                            class="btn-group priority1" 
                            role="group"
                            >
							<tstsc:DropMenu 
                                ClientScriptMethod="save_data(evt)"
                                ClientScriptServerControlId="ajxFormManager" 
                                GlyphIconCssClass="mr3 fas fa-save" 
                                ID="btnSave" 
                                MenuWidth="125px" 
                                runat="server" 
                                Text="<%$Resources:Buttons,Save %>" 
                                ToolTip="Alt+Enter"
                                >
								<DropMenuItems>
									<tstsc:DropMenuItem 
                                        ClientScriptMethod="save_data(null); void(0);" 
                                        GlyphIconCssClass="mr3 fas fa-save" 
                                        ID="DropMenuItem1" 
                                        runat="server" 
                                        Value="<%$Resources:Buttons,Save %>" 
                                        />
									<tstsc:DropMenuItem 
                                        ClientScriptMethod="save_data(null,'close'); void(0);" 
                                        GlyphIconCssClass="mr3 fas fa-save" 
                                        ID="DropMenuItem2" 
                                        runat="server" 
                                        Value="<%$Resources:Buttons,SaveAndClose %>" 
                                        />
									<tstsc:DropMenuItem 
                                        Authorized_ArtifactType="Incident" 
                                        Authorized_Permission="Create" 
                                        ClientScriptMethod="save_data(null,'new'); void(0);" 
                                        GlyphIconCssClass="mr3 fas fa-plus" 
                                        ID="DropMenuItem3" 
                                        runat="server" 
                                        Value="<%$Resources:Buttons,SaveAndNew %>" 
                                        />
								</DropMenuItems>
							</tstsc:DropMenu>
                        </div>
                        <div class="btn-group priority2" role="group">
							<tstsc:DropMenu 
								ClientScriptServerControlId="ajxFormManager" 
                                ClientScriptMethod="load_data()" 
                                GlyphIconCssClass="mr3 fas fa-sync"
                                ID="btnRefresh" 
                                runat="server" 
                                Text="<%$Resources:Buttons,Refresh %>" 
                                />
							<tstsc:DropMenu 
                                Authorized_ArtifactType="Incident" 
                                Authorized_Permission="Create" 
								Confirmation="false" 
								GlyphIconCssClass="mr3 fas fa-plus"
                                ID="btnCopy" 
                                runat="server" 
                                Text="<%$Resources:Buttons,New %>"
                                ClientScriptServerControlId="ajxFormManager"
                                ClientScriptMethod="create_item()" 
                            >
                                <DropMenuItems>
                                    <tstsc:DropMenuItem Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="Incident" />
                                    <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="Incident" />
                                </DropMenuItems>
							</tstsc:DropMenu>
					    </div>
						<div class="btn-group priority2" role="group">
							<tstsc:DropMenu 
								Authorized_ArtifactType="Incident" 
                                Authorized_Permission="Delete"
								Confirmation="True" 
                                ConfirmationMessage="<%$Resources:Messages,IncidentDetails_DeleteConfirm %>" 
								GlyphIconCssClass="mr3 fas fa-trash-alt"
                                ID="btnDelete" 
                                runat="server" 
                                Text="<%$Resources:Buttons,Delete %>"
                                ClientScriptMethod="delete_item()" 
                                ClientScriptServerControlId="ajxFormManager"
                                />
                        </div>
                        <div class="btn-group priority1" role="group">
							<asp:Panel 
                                runat="server" 
                                DefaultButton="btnFind"
                                >
                                <div class="input-group">
                                    <span 
                                        class="input-group-addon" 
                                        id="sizing-addon1"
                                        >
                                        <%#GlobalFunctions.ARTIFACT_PREFIX_INCIDENT%>
                                    </span>
									<tstsc:TextBoxEx 
                                        ID="txtIncidentId" 
                                        runat="server" 
                                        SkinID="NarrowPlusFormControl" 
                                        TextMode="SingleLine" 
                                        />
                                    <div class="input-group-btn">
                                        <tstsc:ButtonEx 
                                            ID="btnFind" 
                                            Enabled="True" 
                                            runat="server" 
                                            Text="<%$Resources:Buttons,Find %>"
                                            ClientScriptMethod="btnFind_click()" 
                                            />
                                    </div>
                                </div>
									
							</asp:Panel>
						</div>
                        <div class="btn-group" role="group">
                            <tstsc:DropMenu 
                                GlyphIconCssClass="mr3 fas fa-cog"
                                id="btnTools" 
                                MenuCssClass="DropMenu" 
                                PostBackOnClick="false"
                                runat="server" 
			                    Text="<%$Resources:Buttons,Tools %>" 
                                >
			                    <DropMenuItems>
                                    <tstsc:DropMenuItem 
                                        Name="Print" 
                                        Value="<%$Resources:Dialogs,Global_PrintItems %>" 
                                        GlyphIconCssClass="mr3 fas fa-print" 
                                        ClientScriptMethod="print_item('html')" 
                                        Authorized_ArtifactType="Incident" 
                                        Authorized_Permission="View" 
                                        />
				                    <tstsc:DropMenuItem Divider="true" />
				                    <tstsc:DropMenuItem 
                                        Name="ExportToExcel" 
                                        Value="<%$Resources:Dialogs,Global_ExportToExcel %>" 
                                        ImageUrl="Images/Filetypes/Excel.svg" 
                                        Authorized_ArtifactType="Incident" 
                                        Authorized_Permission="View" 
                                        ClientScriptMethod="print_item('excel')" 
                                        />
				                    <tstsc:DropMenuItem 
                                        Name="ExportToWord" 
                                        Value="<%$Resources:Dialogs,Global_ExportToWord %>" 
                                        ImageUrl="Images/Filetypes/Word.svg" 
                                        Authorized_ArtifactType="Incident" 
                                        Authorized_Permission="View" 
                                        ClientScriptMethod="print_item('word')" 
                                        />
				                    <tstsc:DropMenuItem 
                                        Authorized_ArtifactType="Incident" 
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
                            class="btn-group priority3" 
                            id="pnlEmailToolButtons"
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
                                Authorized_ArtifactType="Incident"
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
                                        Authorized_ArtifactType="Incident"
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
                </div>




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
                                AlternateText="<%$Resources:Fields,Incident %>" 
                                CssClass="w5 h5"
                                ID="ImageEx1" 
                                ImageUrl="Images/artifact-Incident.svg" 
                                runat="server" 
                                />
                            <tstsc:LabelEx 
                                CssClass="pointer dib orange-hover transition-all pl4"
                                title="<%$Resources:Buttons,CopyToClipboard %>"
                                data-copytoclipboard="true"
                                ID="lblIncidentId" 
                                runat="server" 
                                AppendColon="true" 
                                />
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true"  
                                AssociatedControlID="ddlIncidentType" 
                                ID="ddlIncidentTypeLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,Type %>" 
                                />
    						<tstsc:UnityDropDownListEx
                                 CssClass="u-dropdown"
                                DataTextField="Name" 
                                DataValueField="IncidentTypeId"
                                DisabledCssClass="u-dropdown disabled"
                                ID="ddlIncidentType"
                                runat="server" 
                                />
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
							<tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxWorkflowOperations"
                                ID="lblIncidentStatus" 
                                Required="True" 
                                runat="server" 
                                Text="<%$Resources:Fields,IncidentStatusId %>" 
                                />
                            <div class="dib v-mid-children dif flex-wrap items-center pl3 pr4">
								<tstsc:LabelEx 
                                    ID="lblIncidentStatusValue" 
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
                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" 
                                        />
                                </div>
                            </div>
                        </div>
                        <div class="py1 dif items-center ma0-children fs-h6">
							<tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="eqlProgress" 
                                CssClass="silver"
                                ID="lblProgress" 
                                runat="server" 
                                Text="<%$Resources:Fields,Progress %>" 
                                />
							<span class="pl3 pr4">
                                <tstsc:Equalizer 
                                    ID="eqlProgress" 
                                    runat="server" 
                                    />
                                <span class="fs-90 o-70 dib nowrap">
									(<tstsc:LabelEx 
                                        ID="lblPercentComplete" 
                                        runat="server" 
                                        />%)
                                </span>
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
				ArtifactTypeEnum="Incident" 
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
                    ID="tclIncidentDetails" 
                    runat="server" 
                    TabHeight="25" 
                    TabWidth="100"
                    >
					<TabPages>
						<tstsc:TabPage 
                            Caption="<% $Resources:ServerControls,TabControl_Overview %>"
                            ID="tabOverview" 
                            runat="server" 
							TabPageControlId="pnlOverview" 
                            TabPageIcon="fas fa-home"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_OVERVIEW %>"
                            />
						<tstsc:TabPage 
							AjaxControlContainer="tstAttachmentPanel" 
                            AjaxServerControlId="grdAttachmentList" 
                            Caption="<% $Resources:ServerControls,TabControl_Attachments %>" 
                            ID="tabAttachments" 
                            runat="server" 
                            TabPageControlId="pnlAttachments"
                            TabPageImageUrl="Images/artifact-Document.svg"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_ATTACHMENTS %>"
                            />
						<tstsc:TabPage 
							AjaxControlContainer="tstAssociationPanel" 
                            AjaxServerControlId="grdAssociationLinks" 
                            Caption="<% $Resources:ServerControls,TabControl_Associations %>" 
                            ID="tabAssociations" 
                            runat="server" 
                            TabPageControlId="pnlAssociations"
                            TabPageIcon="fas fa-link"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_ASSOCIATION %>"
                            />
						<tstsc:TabPage 
							AjaxControlContainer="tstHistoryPanel" 
                            AjaxServerControlId="grdHistoryList" 
                            Caption="<% $Resources:ServerControls,TabControl_History %>" 
                            ID="tabHistory" 
                            runat="server" 
                            TabPageControlId="pnlHistory"
                            TabPageIcon="fas fa-history"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_HISTORY %>"
                            />
					</TabPages>
				</tstsc:TabControl>
				<asp:Panel ID="pnlOverview" runat="server" CssClass="TabControlPanel">
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
                                            AssociatedControlID="ddlDetectedRelease" 
                                            CssClass="u-label" 
                                            ID="lblDetectedRelease" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,DetectedReleaseId %>" 
                                            />
                                        <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="" 
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed"
                                            SkinID="ReleaseDropDownListFarRight"
                                            AutoPostBack="false"
                                            DataTextField="FullName"
                                            DataValueField="ReleaseId"
                                            Enabled="false"
                                            ID="ddlDetectedRelease" 
                                            NavigateToText ="<%$Resources:ClientScript,DropDownHierarchy_NavigateToRelease %>"
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlResolvedRelease" 
                                            CssClass="u-label" 
                                            ID="lblResolvedRelease" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ResolvedReleaseId %>" 
                                            Title="<%$Resources:Fields,ResolvedReleaseId_Tooltip %>"
                                            />
                                        <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="IsActive" 
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed"
                                            SkinID="ReleaseDropDownListFarRight"
                                            AutoPostBack="false" 
                                            ClientScriptMethod="ddlResolvedRelease_selectedItemChanged"
                                            DataTextField="FullName"
                                            DataValueField="ReleaseId"
                                            ID="ddlResolvedRelease" 
                                            NoValueItem="true" 
                                            NavigateToText ="<%$Resources:ClientScript,DropDownHierarchy_NavigateToRelease %>"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlVerifiedRelease" 
                                            ID="lblVerifiedRelease" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,VerifiedReleaseId %>" 
                                            />
                                        <tstsc:UnityDropDownHierarchy 
                                            ActiveItemField="IsActive" 
                                            AutoPostBack="false" 
                                            DataTextField="FullName"
                                            DataValueField="ReleaseId"
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed"
                                            ID="ddlVerifiedRelease" 
                                            NavigateToText ="<%$Resources:ClientScript,DropDownHierarchy_NavigateToRelease %>"
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlBuild" 
                                            ID="lblBuild" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,FixedBuild %>" 
                                            />
                                        <tstsc:UnityDropDownListEx 
                                            ActiveItemField="IsActive" 
                                            AutoPostBack="false" 
                                            CssClass="u-dropdown"
                                            DataTextField="Name"
                                            DataValueField="BuildId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlBuild" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server"
                                            />
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
                                <ul class="u-box_list" id="customFieldsUsers" runat="server">
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
                                            AssociatedControlID="ddlOpener" 
                                            ID="lblOpener" 
                                            Required="True" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,OpenerId %>" 
                                            />
										<tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                            ID="ddlOpener" 
                                            NoValueItem="true" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" 
                                            runat="server" 
                                            />       
                                    </li>
                                    <li class="ma0 mb2 pa0">                                            
    									<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlOwner" 
                                            ID="lblOwner" 
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
                                </ul>
                            </div>
                        </div>
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
                                            ID="lblPriority" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,PriorityId %>" 
                                            />
										<tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="PriorityId"
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
                                            AssociatedControlID="ddlSeverity" 
                                            ID="lblSeverity" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,SeverityId %>" 
                                            />
										<tstsc:UnityDropDownListEx
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="SeverityId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlSeverity" 
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
                                <ul class="u-box_list" id="customFieldsDates" runat="server">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="CreationDate" 
                                            ID="lblCreationDate" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,CreationDate %>"
                                            />
                                        <tstsc:LabelEx 
                                            ID="CreationDate" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
	    								<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="lblLastUpdateDate" 
                                            ID="lblLastUpdateDateLabel" 
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
                                            ID="lblEstimatedEffort" 
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
                    </div>
                    <div class="u-wrapper width_md">
                        <div class="u-box_3">
                            <div 
                                class="u-box_group u-cke_is-minimal"
                                data-collapsible="true"
                                id="form-group_rte" >
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
                                            Authorized_ArtifactType="Incident" 
                                            ID="txtDescription" 
                                            runat="server" 
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
                            <div 
                                class="u-box_group u-box-c"
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
                                            ArtifactType="Incident" 
                                            AutoLoad="false" 
                                            ErrorMessageControlId="lblMessage"
                                            ID="lstComments" 
                                            runat="server" 
									        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" 
                                            Width="100%" 
                                            />
								        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="Incident"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="txtResolution" 
                                            runat="server" 
                                            Screenshot_ArtifactType="Incident" 
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
                    ID="pnlAttachments" 
                    runat="server" 
                    >
					<tstuc:AttachmentPanel 
                        ID="tstAttachmentPanel" 
                        runat="server" 
                        />
				</asp:Panel>
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
                    CssClass="TabControlPanel"
                    ID="pnlHistory" 
                    runat="server" 
                    >
					<tstuc:HistoryPanel 
                        ID="tstHistoryPanel" 
                        runat="server" 
                        />
				</asp:Panel>
            </div>
        </div>
	</div>
    <asp:HiddenField ID="hdnPlaceholderId" runat="server" />
	<tstsc:AjaxFormManager ID="ajxFormManager" runat="server" ErrorMessageControlId="lblMessage"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" RevertButtonControlId="btnRevert"
        ArtifactTypeName="<%$Resources:Fields,Incident%>" WorkflowEnabled="true" NameField="Name"
        WorkflowOperationsControlId="ajxWorkflowOperations" DisplayPageName="true">
		<ControlReferences>
			<tstsc:AjaxFormControl ControlId="lblIncidentId" DataField="IncidentId" Direction="In" />
			<tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlOpener" DataField="OpenerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlDetectedRelease" DataField="DetectedReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlResolvedRelease" DataField="ResolvedReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlVerifiedRelease" DataField="VerifiedReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlBuild" DataField="BuildId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlIncidentType" DataField="IncidentTypeId" Direction="InOut" ChangesWorkflow="true" />
			<tstsc:AjaxFormControl ControlId="ddlPriority" DataField="PriorityId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlSeverity" DataField="SeverityId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentIds" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="CreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="lblIncidentStatusValue" DataField="IncidentStatusId" Direction="In" IsWorkflowStep="true" />
			<tstsc:AjaxFormControl ControlId="datStartDate" DataField="StartDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="datClosedDate" DataField="ClosedDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtEstimatedEffort" DataField="EstimatedEffort" Direction="InOut" PropertyName="intValue" />
			<tstsc:AjaxFormControl ControlId="txtRemainingEffort" DataField="RemainingEffort" Direction="InOut" PropertyName="intValue" />
			<tstsc:AjaxFormControl ControlId="txtActualEffort" DataField="ActualEffort" Direction="InOut" PropertyName="intValue" />
			<tstsc:AjaxFormControl ControlId="lblProjectedEffort" DataField="ProjectedEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblPercentComplete" DataField="CompletionPercent" Direction="In" />
			<tstsc:AjaxFormControl ControlId="eqlProgress" DataField="ProgressId" Direction="In" />
			<tstsc:AjaxFormControl ControlId="txtResolution" DataField="Resolution" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="hdnPlaceholderId" DataField="PlaceholderId" Direction="Out" PropertyName="intValue" />
		</ControlReferences>
		<SaveButtons>
			<tstsc:AjaxFormSaveButton ControlId="btnSave" />
			<tstsc:AjaxFormSaveButton ControlId="mnuSave" />
			<tstsc:AjaxFormSaveButton ControlId="mnuSaveAndClose" />
			<tstsc:AjaxFormSaveButton ControlId="mnuSaveAndNew" />
		</SaveButtons>
	</tstsc:AjaxFormManager>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/PlaceHolderService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/IncidentDetails.js" />
            <asp:ScriptReference Path="~/TypeScript/Followers.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
	<script type="text/javascript">

        //SpiraContext
	    SpiraContext.pageId = "Inflectra.Spira.Web.IncidentDetails";
	    SpiraContext.ArtifactId = <%=incidentId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=incidentId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
	    SpiraContext.ArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.Incident%>;
	    SpiraContext.EmailEnabled = <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.EmailSettings_Enabled.ToString().ToLowerInvariant()%>;
	    SpiraContext.HasCollapsiblePanels = true;
	    SpiraContext.Mode = 'update';
	    SpiraContext.PlaceholderArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.Placeholder%>;
	    SpiraContext.PlaceholderId = null;

	    if (!SpiraContext.ArtifactId || SpiraContext.ArtifactId < 1)
	    {
            //New incident
	        SpiraContext.PlaceholderId = <%=placeholderId%>;
	        SpiraContext.Mode = 'new';
	    }

	    //Server Control IDs
	    var btnSubscribe_id = '<%=this.btnSubscribe.ClientID%>';
	    var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
	    var lstComments_id = '<%=this.lstComments.ClientID %>';
	    var tabControl_id = '<%=this.tclIncidentDetails.ClientID%>';
	    var btnNewComment_id = '<%=this.btnNewComment.ClientID%>';
	    var hdnPlaceholderId_id = '<%=hdnPlaceholderId.ClientID%>';
	    var txtName_id = '<%=txtName.ClientID%>';
	    var btnSave_id = '<%=btnSave.ClientID%>';
	    var btnEmail_id = '<%=btnEmail.ClientID%>';
	    var navigationBar_id = '<%=this.navIncidentList.ClientID%>';
	    var txtIncidentId_id = '<%=this.txtIncidentId.ClientID%>';
	    var lblMessage_id = '<%=lblMessage.ClientID%>';

	    //TabControl Panel IDs
	    var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
	    var pnlHistory_id = '<%=pnlHistory.ClientID%>';
	    var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';

	    //Other IDs
	    var ddlBuild_id = '<%=this.ddlBuild.ClientID%>';
	    var datClosedDate_id = '<%=this.datClosedDate.ClientID%>';

	    //URL Templates
	    var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
	    var urlTemplate_artifactListUrl = '<%=IncidentListUrl %>';
	    var urlTemplate_artifactNew = '<%=IncidentNewUrl %>';
	    var urlTemplate_artifactRedirectUrl = '<%=IncidentRedirectUrl %>';
	    var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

	    //Updates any page specific content
	    function updatePageContent()
	    {
	        //Nothing needed
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
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.IncidentSummary%>";
                filter = "&af_12_94=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.IncidentDetailed%>";
                filter = "&af_13_94=";
            }

            //Open the report for the specified format
	        globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
	    }
    </script>
</asp:Content>
