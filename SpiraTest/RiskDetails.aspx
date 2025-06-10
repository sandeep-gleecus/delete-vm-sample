<%@ Page 
    AutoEventWireup="True"
    CodeBehind="RiskDetails.aspx.cs" 
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
	Inherits="Inflectra.SpiraTest.Web.RiskDetails" 
%>
<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TaskListPanel" Src="UserControls/TaskListPanel.ascx" %>

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
                ID="navRiskList" 
                IncludeAssigned="true" 
                IncludeDetected="true"
                ItemImage="Images/artifact-Risk.svg"
				ListScreenCaption="<%$Resources:Main,RiskDetails_BackToList%>" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService" 
                />
		</div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="clearfix">
                    <div 
                        class="btn-group probability1 hidden-md hidden-lg mr3" 
                        role="group"
                        >
                        <tstsc:HyperLinkEx 
                            ID="btnBack" 
                            NavigateUrl=<%#ReturnToRiskListUrl%> 
                            runat="server" 
                            SkinID="ButtonDefault" 
                            ToolTip="<%$Resources:Main,RiskDetails_BackToList%>"
                            >
                            <span class="fas fa-arrow-left"></span>
                        </tstsc:HyperLinkEx>
                    </div>
                    <div 
                        id="plcNewArtifactSubmit" 
                        style="display:none"
                        >
                        <div class="btn-group probability1" role="group">
							<tstsc:DropMenu 
                                Authorized_ArtifactType="Risk" 
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
                                Authorized_ArtifactType="Risk" 
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
                                Authorized_ArtifactType="Risk" 
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
                                        Authorized_ArtifactType="Risk" 
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
                                Authorized_ArtifactType="Risk" 
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
                                    <tstsc:DropMenuItem Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="Risk" />
                                    <tstsc:DropMenuItem Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="Risk" />
                                </DropMenuItems>
							</tstsc:DropMenu>
					    </div>
						<div class="btn-group priority2" role="group">
							<tstsc:DropMenu 
								Authorized_ArtifactType="Risk" 
                                Authorized_Permission="Delete"
								Confirmation="True" 
                                ConfirmationMessage="<%$Resources:Messages,RiskDetails_DeleteConfirm %>" 
								GlyphIconCssClass="mr3 fas fa-trash-alt"
                                ID="btnDelete" 
                                runat="server" 
                                Text="<%$Resources:Buttons,Delete %>"
                                ClientScriptMethod="delete_item()" 
                                ClientScriptServerControlId="ajxFormManager"
                                />
                        </div>
                        <div class="btn-group priority3" role="group">
							<asp:Panel 
                                runat="server" 
                                DefaultButton="btnFind"
                                >
                                <div class="input-group">
                                    <span 
                                        class="input-group-addon" 
                                        id="sizing-addon1"
                                        >
                                        <%#GlobalFunctions.ARTIFACT_PREFIX_RISK%>
                                    </span>
									<tstsc:TextBoxEx 
                                        ID="txtRiskId" 
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
                                            ClientScriptMethod="riskDetails_btnFind_click()" 
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
                                        Authorized_ArtifactType="Risk" 
                                        Authorized_Permission="View" 
                                        />
				                    <tstsc:DropMenuItem Divider="true" />
				                    <tstsc:DropMenuItem 
                                        Name="ExportToExcel" 
                                        Value="<%$Resources:Dialogs,Global_ExportToExcel %>" 
                                        ImageUrl="Images/Filetypes/Excel.svg" 
                                        Authorized_ArtifactType="Risk" 
                                        Authorized_Permission="View" 
                                        ClientScriptMethod="print_item('excel')" 
                                        />
				                    <tstsc:DropMenuItem 
                                        Name="ExportToWord" 
                                        Value="<%$Resources:Dialogs,Global_ExportToWord %>" 
                                        ImageUrl="Images/Filetypes/Word.svg" 
                                        Authorized_ArtifactType="Risk" 
                                        Authorized_Permission="View" 
                                        ClientScriptMethod="print_item('word')" 
                                        />
				                    <tstsc:DropMenuItem 
                                        Authorized_ArtifactType="Risk" 
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
                            class="btn-group probability3" 
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
                                Authorized_ArtifactType="Risk"
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
                                        Authorized_ArtifactType="Risk"
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
                                AlternateText="<%$Resources:Fields,Risk %>" 
                                CssClass="w5 h5"
                                ID="ImageEx1" 
                                ImageUrl="Images/artifact-Risk.svg" 
                                runat="server" 
                                />
                            <tstsc:LabelEx 
                                CssClass="pointer dib orange-hover transition-all pl4"
                                title="<%$Resources:Buttons,CopyToClipboard %>"
                                data-copytoclipboard="true"
                                ID="lblRiskId" 
                                runat="server" 
                                AppendColon="true" 
                                />
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true"  
                                AssociatedControlID="ddlRiskType" 
                                ID="ddlRiskTypeLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,Type %>" 
                                />
    						<tstsc:UnityDropDownListEx
                                CssClass="pl2 u-dropdown is-active"
                                DataTextField="Name" 
                                DataValueField="RiskTypeId"
                                DisabledCssClass="u-dropdown disabled"
                                ID="ddlRiskType"
                                runat="server" 
                                />
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
							<tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxWorkflowOperations"
                                ID="lblRiskStatus" 
                                Required="True" 
                                runat="server" 
                                Text="<%$Resources:Fields,RiskStatusId %>" 
                                />
                            <div class="dib v-mid-children dif flex-wrap items-center pl3">
								<tstsc:LabelEx 
                                    ID="lblRiskStatusValue" 
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
                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService" 
                                        />
                                </div>
                            </div>
                        </div>
                        <div class="py1 dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxRiskExposure"
                                CssClass="silver"
                                ID="ajxRiskExposureLabel" 
                                Required="false" 
                                runat="server" 
                                Text="<%$Resources:Fields,RiskExposure %>" 
                                />
                            <span class="pl3 pr4">
                                <tstsc:StatusBox 
                                    ID="ajxRiskExposure" 
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
				ArtifactTypeEnum="Risk" 
                ID="tstEmailPanel" 
                runat="server" 
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="Risk" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />
            <div class="main-content">
					    
				<tstsc:TabControl 
                    ID="tclRiskDetails" 
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
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_OVERVIEW %>"
                            />
						<tstsc:TabPage 
                            AjaxControlContainer="tstTaskListPanel" 
                            AjaxServerControlId="grdTaskList" 
                            AuthorizedArtifactType="Task" 
                            Caption="<% $Resources:ServerControls,TabControl_Tasks %>" 
                            CheckPermissions="true" 
                            ID="tabTasks"
                            runat="server" 
                            TabPageControlId="pnlTasks" 
                            TabPageImageUrl="Images/artifact-Task.svg"
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_TASK %>"
                            />
						<tstsc:TabPage 
							AjaxControlContainer="tstAttachmentPanel" 
                            AjaxServerControlId="grdAttachmentList" 
                            Caption="<% $Resources:ServerControls,TabControl_Attachments %>" 
                            AuthorizedArtifactType="Document"
                            CheckPermissions="true" 
                            ID="tabAttachments" 
                            runat="server" 
                            TabPageControlId="pnlAttachments"
                            TabPageImageUrl="Images/artifact-Document.svg"
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_ATTACHMENTS %>"
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
                                        Text="<%$Resources:Fields,Source %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul class="u-box_list">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlRelease" 
                                            CssClass="u-label" 
                                            ID="lblRelease" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ReleaseId %>" 
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
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlComponent" 
                                            ID="lblComponent" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ComponentId %>" 
                                            />
										<tstsc:UnityDropDownListEx
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
                                            AssociatedControlID="ddlCreator" 
                                            ID="lblCreator" 
                                            Required="True" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,CreatorId %>" 
                                            />
										<tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId"
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                            ID="ddlCreator" 
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
                                            AssociatedControlID="ddlProbability" 
                                            ID="lblProbability" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,RiskProbabilityId %>" 
                                            />
										<tstsc:UnityDropDownListEx 
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="RiskProbabilityId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlProbability" 
                                            NoValueItem="True" 
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>" 
                                            runat="server" 
                                                />
                                    </li>
                                    <li class="ma0 mb2 pa0">
										<tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="ddlImpact" 
                                            ID="lblImpact" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,RiskImpactId %>" 
                                            />
										<tstsc:UnityDropDownListEx
                                            CssClass="u-dropdown"
                                            DataTextField="Name" 
                                            DataValueField="RiskImpactId"
                                            DisabledCssClass="u-dropdown disabled"
                                            ID="ddlImpact" 
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
                                            AssociatedControlID="datReviewDate" 
                                            ID="datReviewDateLabel" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ReviewDate %>" 
                                            />
                                        <tstsc:UnityDateControl 
                                            CssClass="u-datepicker"
                                            DisabledCssClass="u-datepicker disabled"
                                            ID="datReviewDate" 
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
                                            Authorized_ArtifactType="Risk" 
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

                            <%-- Mitigations Panel --%>
                            <div 
                                class="u-box_3"
                                id="divMitigationsPanel"
                                >
                                <div 
                                    class="u-box_group u-cke_is-minimal"
                                    data-collapsible="true"
                                    id="form-group_mitigations" >
                                    <div 
                                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                        aria-expanded="true">
                                        <asp:Localize 
                                            runat="server" 
                                            Text="<%$Resources:ServerControls,TabControl_Mitigations %>" />
                                        <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                    </div>
                                    <ul class="u-box_list" >
                                        <li class="ma0 mb2 pa0">
                                            <div class="TabControlHeader">
										        <div class="btn-group priority4">
											        <tstsc:HyperLinkEx 
                                                        ClientScriptMethod="insert_item('Mitigation')" 
                                                        ClientScriptServerControlId="grdMitigations" 
                                                        ID="lnkInsertStep" 
                                                        NavigateUrl="javascript:void(0)" 
                                                        SkinID="ButtonDefault" 
                                                        runat="server" 
                                                        >
                                                        <span class="fas fa-plus"></span>
                                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Add %>" />
											        </tstsc:HyperLinkEx>
											        <tstsc:HyperLinkEx 
                                                        ClientScriptMethod="delete_items()" 
                                                        ClientScriptServerControlId="grdMitigations" 
                                                        Confirmation="true" 
                                                        ConfirmationMessage="<%$Resources:Messages,RiskDetails_MitigationDeleteConfirm %>"
                                                        ID="lnkDeleteStep" 
                                                        NavigateUrl="javascript:void(0)" 
                                                        SkinID="ButtonDefault" 
                                                        runat="server" 
                                                        >
                                                        <span class="fas fa-trash-alt"></span>
                                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
											        </tstsc:HyperLinkEx>
											        <tstsc:HyperLinkEx 
                                                        ClientScriptMethod="copy_items()"
                                                        ClientScriptServerControlId="grdMitigations" 
                                                        ID="lnkCopyStep" 
                                                        NavigateUrl="javascript:void(0)" 
                                                        SkinID="ButtonDefault" 
                                                        runat="server" 
                                                        >
                                                        <span class="far fa-clone"></span>
                                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Clone %>" />
											        </tstsc:HyperLinkEx>
											    </div>
                                                <div class="btn-group priority1">
											        <tstsc:HyperLinkEx ID="lnkRefreshSteps" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdMitigations" ClientScriptMethod="load_data()">
                                                        <span class="fas fa-sync"></span>
                                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>" />
											        </tstsc:HyperLinkEx>
										        </div>
									        </div>
									        <tstsc:MessageBox ID="lblMitigationMessages" runat="server" SkinID="MessageBox" />
									        <tstsc:OrderedGrid 
                                                AllowInlineEditing="true"
                                                AlternateItemHasHyperlink="false" 
                                                AlternateItemImage="artifact-RiskMitigation.svg"
                                                Authorized_ArtifactType="Risk" 
                                                Authorized_Permission="Modify" 
                                                AutoLoad="false" 
                                                ConcurrencyEnabled="true" 
                                                CssClass="DataGrid DataGrid-no-bands" 
                                                EditRowCssClass="Editing" 
                                                ErrorMessageControlId="lblMitigationMessages"
                                                HeaderCssClass="SubHeader" 
                                                ID="grdMitigations" 
								                RowCssClass="Normal" 
                                                runat="server" 
                                                SelectedRowCssClass="Highlighted" 
								                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RiskMitigationService" 
                                                >
										        <ContextMenuItems>
                                                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                                                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Risk" Authorized_Permission="View" />
                                                    <tstsc:ContextMenuItem Divider="True" />
											        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Buttons,Insert%>" CommandName="insert_item" CommandArgument="Mitigation" Authorized_ArtifactType="Risk" Authorized_Permission="Modify" />
                                                    <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Risk" Authorized_Permission="Modify" />
											        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-copy" Caption="<%$Resources:Buttons,CopyItems%>" CommandName="copy_items" Authorized_ArtifactType="Risk" Authorized_Permission="Modify" />
                                                    <tstsc:ContextMenuItem Divider="True" />
											        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Delete %>" CommandName="delete_items" Authorized_ArtifactType="Risk" Authorized_Permission="Modify" ConfirmationMessage="<%$Resources:Messages,RiskDetails_MitigationDeleteConfirm %>" />
										        </ContextMenuItems>
									        </tstsc:OrderedGrid>
                                        </li>
                                    </ul>
                                </div>
                            </div>

                            <%-- Comments Panel --%>
                            <div 
                                class="u-box_group u-box_3 my4"
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
                                            ArtifactType="Risk" 
                                            AutoLoad="false" 
                                            ErrorMessageControlId="lblMessage"
                                            ID="lstComments" 
                                            runat="server" 
									        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService" 
                                            Width="100%" 
                                            />
								        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="Risk"
									        Authorized_Permission="Modify" 
                                            Height="80px" 
                                            ID="txtNewComment" 
                                            runat="server" 
                                            Screenshot_ArtifactType="Risk" 
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
            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService" RevertButtonControlId="btnRevert"
            ArtifactTypeName="<%$Resources:Fields,Risk%>" WorkflowEnabled="true" NameField="Name"
            WorkflowOperationsControlId="ajxWorkflowOperations" DisplayPageName="true">
		<ControlReferences>
			<tstsc:AjaxFormControl ControlId="lblRiskId" DataField="RiskId" Direction="In" />
			<tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlCreator" DataField="CreatorId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlRiskType" DataField="RiskTypeId" Direction="InOut" ChangesWorkflow="true" />
			<tstsc:AjaxFormControl ControlId="ddlProbability" DataField="RiskProbabilityId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlImpact" DataField="RiskImpactId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="CreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="lblRiskStatusValue" DataField="RiskStatusId" Direction="In" IsWorkflowStep="true" />
			<tstsc:AjaxFormControl ControlId="datReviewDate" DataField="ReviewDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="datClosedDate" DataField="ClosedDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="hdnPlaceholderId" DataField="PlaceholderId" Direction="Out" PropertyName="intValue" />
            <tstsc:AjaxFormControl ControlId="ajxRiskExposure" DataField="RiskExposure" Direction="In" />
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
			<asp:ServiceReference Path="~/Services/Ajax/RisksService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/RiskMitigationService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/PlaceHolderService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/RiskDetails.js" />
            <asp:ScriptReference Path="~/TypeScript/Followers.js" />
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
	<script type="text/javascript">

        //SpiraContext
	    SpiraContext.pageId = "Inflectra.Spira.Web.RiskDetails";
	    SpiraContext.ArtifactId = <%=riskId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=riskId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
	    SpiraContext.ArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.Risk%>;
	    SpiraContext.EmailEnabled = <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.EmailSettings_Enabled.ToString().ToLowerInvariant()%>;
	    SpiraContext.HasCollapsiblePanels = true;
	    SpiraContext.Mode = 'update';
	    SpiraContext.PlaceholderArtifactTypeId = <%=(int)Artifact.ArtifactTypeEnum.Placeholder%>;
	    SpiraContext.PlaceholderId = null;

	    if (!SpiraContext.ArtifactId || SpiraContext.ArtifactId < 1)
	    {
            //New risk
	        SpiraContext.PlaceholderId = <%=placeholderId%>;
	        SpiraContext.Mode = 'new';
	    }

	    //Server Control IDs
	    var btnSubscribe_id = '<%=this.btnSubscribe.ClientID%>';
	    var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
	    var lstComments_id = '<%=this.lstComments.ClientID %>';
	    var tabControl_id = '<%=this.tclRiskDetails.ClientID%>';
	    var btnNewComment_id = '<%=this.btnNewComment.ClientID%>';
	    var hdnPlaceholderId_id = '<%=hdnPlaceholderId.ClientID%>';
	    var txtName_id = '<%=txtName.ClientID%>';
	    var btnSave_id = '<%=btnSave.ClientID%>';
	    var btnEmail_id = '<%=btnEmail.ClientID%>';
	    var navigationBar_id = '<%=this.navRiskList.ClientID%>';
	    var txtRiskId_id = '<%=this.txtRiskId.ClientID%>';
	    var lblMessage_id = '<%=lblMessage.ClientID%>';
        var grdMitigations_id = '<%=grdMitigations.ClientID%>';
        var lnkStepInsert_id = '<%=lnkInsertStep.ClientID%>';
        var lnkStepDelete_id = '<%=lnkDeleteStep.ClientID%>';
        var lnkStepCopy_id = '<%=lnkCopyStep.ClientID%>';

	    //TabControl Panel IDs
	    var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
	    var pnlHistory_id = '<%=pnlHistory.ClientID%>';
	    var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';
        var pnlTasks_id = '<%=pnlTasks.ClientID%>';

	    //Other IDs
	    var datClosedDate_id = '<%=this.datClosedDate.ClientID%>';

	    //URL Templates
	    var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
	    var urlTemplate_artifactListUrl = '<%=RiskListUrl %>';
	    var urlTemplate_artifactNew = '<%=RiskNewUrl %>';
	    var urlTemplate_artifactRedirectUrl = '<%=RiskRedirectUrl %>';
	    var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

	    //Set any page properties
	    var pageProps = {};
	    pageProps.includeStepHistory = true;

	    //Prints the current items
	    function print_item(format)
	    {
            var artifactId = SpiraContext.ArtifactId;

            //Get the report type from the format
            var reportToken;
            var filter;
            if (format == 'excel')
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RiskSummary%>";
                filter = "&af_22_200=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.RiskDetailed%>";
                filter = "&af_23_200=";
            }

            //Open the report for the specified format
	        globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
	    }
	</script>
</asp:Content>
