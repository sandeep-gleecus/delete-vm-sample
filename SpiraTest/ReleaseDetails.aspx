<%@ Page
	AutoEventWireup="True"
	CodeBehind="ReleaseDetails.aspx.cs"
	Inherits="Inflectra.SpiraTest.Web.ReleaseDetails"
	Language="c#"
	MasterPageFile="~/MasterPages/Main.Master"
	ValidateRequest="false" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="RequirementTaskPanel" Src="UserControls/RequirementTaskPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestRunListPanel" Src="UserControls/TestRunListPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="BaselinePanel" Src="UserControls/BaselinePanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>


<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1" />

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
	<div class="panel-container flex">
		<div class="side-panel dn-sm dn-xs sticky top-nav self-start">
			<tstsc:NavigationBar
				AlternateItemImage="Images/artifact-Iteration.svg"
				AutoLoad="true"
				BodyHeight="580px"
				ErrorMessageControlId="lblMessage"
				ID="navReleaseList"
				IncludeAssigned="false"
				ItemImage="Images/artifact-Release.svg"
				ListScreenCaption="<%$Resources:Main,ReleaseDetails_BackToList%>"
				runat="server"
				SummaryItemImage="Images/artifact-Release.svg"
				WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService"
				EnableLiveLoading="true"
				FormManagerControlId="ajxFormManager" />
		</div>



		<div class="main-panel pl4 grow-1">
			<div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">



				<%-- TOOLBAR AREA --%>
				<div class="clearfix">
					<div class="btn-group priority1 hidden-md hidden-lg mr3" role="group">
						<tstsc:HyperLinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" NavigateUrl="<%#ReturnToReleaseListUrl%>" ToolTip="<%$Resources:Main,ReleaseDetails_BackToList%>">
                            <span class="fas fa-arrow-left"></span>
						</tstsc:HyperLinkEx>
					</div>
					<div class="btn-group priority1" role="group">
						<tstsc:DropMenu ID="btnSave" runat="server" Text="<%$Resources:Buttons,Save %>"
							GlyphIconCssClass="mr3 fas fa-save" ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="save_data(evt)"
							Authorized_ArtifactType="Release" Authorized_Permission="Modify" MenuWidth="125px">
							<DropMenuItems>
								<tstsc:DropMenuItem ID="DropMenuItem1" runat="server" GlyphIconCssClass="mr3 fas fa-save" Name="Save" Value="<%$Resources:Buttons,Save %>" Authorized_ArtifactType="Release" Authorized_Permission="Modify" ClientScriptMethod="save_data(null); void(0);" />
								<tstsc:DropMenuItem ID="DropMenuItem2" runat="server" GlyphIconCssClass="mr3 far fa-file-excel" Name="SaveAndClose" Value="<%$Resources:Buttons,SaveAndClose %>" Authorized_ArtifactType="Release" Authorized_Permission="Modify" ClientScriptMethod="save_data(null, 'close'); void(0);" />
								<tstsc:DropMenuItem ID="DropMenuItem3" runat="server" GlyphIconCssClass="mr3 far fa-copy" Name="SaveAndNew" Value="<%$Resources:Buttons,SaveAndNew %>" Authorized_ArtifactType="Release" Authorized_Permission="Create" ClientScriptMethod="save_data(null, 'new'); void(0);" />
							</DropMenuItems>
						</tstsc:DropMenu>
						<tstsc:DropMenu ID="btnRefresh" runat="server" Text="<%$Resources:Buttons,Refresh %>" GlyphIconCssClass="mr3 fas fa-sync" ConfirmationMessage="<%$Resources:Messages,ReleaseDetails_RefreshConfirm %>" ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="load_data()" />
						<tstsc:DropMenu
							Authorized_ArtifactType="Release"
							Authorized_Permission="Create"
							Confirmation="false"
							GlyphIconCssClass="mr3 fas fa-plus"
							ID="btnCreate"
							runat="server"
							Text="<%$Resources:Buttons,New %>"
							ClientScriptServerControlId="ajxFormManager"
							ClientScriptMethod="create_item()">
							<DropMenuItems>
								<tstsc:DropMenuItem runat="server" Name="New" Value="<%$Resources:Buttons,New %>" GlyphIconCssClass="mr3 fas fa-plus" ClientScriptMethod="create_item()" Authorized_Permission="Create" Authorized_ArtifactType="Release" />
								<tstsc:DropMenuItem runat="server" Name="Clone" Value="<%$Resources:Buttons,Clone %>" GlyphIconCssClass="mr3 far fa-clone" ClientScriptMethod="clone_item()" Authorized_Permission="Create" Authorized_ArtifactType="Release" />
							</DropMenuItems>
						</tstsc:DropMenu>
					</div>
					<div class="btn-group priority2" role="group">
						<tstsc:DropMenu ID="btnDelete" runat="server" Text="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt"
							ConfirmationMessage="<%$Resources:Messages,ReleaseDetails_DeleteConfirm %>"
							ClientScriptMethod="delete_item()"
							ClientScriptServerControlId="ajxFormManager"
							Confirmation="True" Authorized_ArtifactType="Release" Authorized_Permission="Delete" />
					</div>
					<div class="btn-group priority4" role="group">
						<tstsc:DropMenu ID="btnTools" runat="server" GlyphIconCssClass="mr3 fas fa-cog"
							Text="<%$Resources:Buttons,Tools %>" MenuCssClass="DropMenu" PostBackOnClick="false">
							<DropMenuItems>
								<tstsc:DropMenuItem runat="server" Name="Print" Value="<%$Resources:Dialogs,Global_PrintItems %>" GlyphIconCssClass="mr3 fas fa-print" ClientScriptMethod="print_item('html')" Authorized_ArtifactType="Release" Authorized_Permission="View" />
								<tstsc:DropMenuItem runat="server" Divider="true" />
								<tstsc:DropMenuItem runat="server" Name="ExportToExcel" Value="<%$Resources:Dialogs,Global_ExportToExcel %>" ImageUrl="Images/Filetypes/Excel.svg" Authorized_ArtifactType="Release" Authorized_Permission="View" ClientScriptMethod="print_item('excel')" />
								<tstsc:DropMenuItem runat="server" Name="ExportToWord" Value="<%$Resources:Dialogs,Global_ExportToWord %>" ImageUrl="Images/Filetypes/Word.svg" Authorized_ArtifactType="Release" Authorized_Permission="View" ClientScriptMethod="print_item('word')" />
								<tstsc:DropMenuItem runat="server" Name="ExportToPdf" Value="<%$Resources:Dialogs,Global_ExportToPdf %>" ImageUrl="Images/Filetypes/Acrobat.svg" Authorized_ArtifactType="Release" Authorized_Permission="View" ClientScriptMethod="print_item('pdf')" />
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
                            Authorized_ArtifactType="Release"
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
                                    Authorized_ArtifactType="Release"
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
					runat="server" />


				<%-- HEADER BAR AREA --%>
				<div class="u-wrapper width_md sm-hide-isfixed xs-hide-isfixed xxs-hide-isfixed">
					<div class="textarea-resize_container mb3">
						<tstsc:UnityTextBoxEx
							CssClass="u-input_title u-input textarea-resize_field mt2 mb1"
							ID="txtName"
							MaxLength="255"
							PlaceHolder="<%$Resources:ClientScript,Artifact_EnterNewName %>"
							TextMode="MultiLine"
							Rows="1"
							runat="server" />
						<div class="textarea-resize_checker"></div>
					</div>
					<div class="py2 px3 mb2 bg-near-white br2 flex items-center flex-wrap">
						<div class="py1 pr4 dif items-center ma0-children fs-h4 fs-h6-xs">
							<tstsc:ImageEx
								ID="imgReleaseIteration"
								CssClass="w5 h5"
								runat="server" />
							<span class="pl4 silver nowrap">
								<asp:Literal
									ID="ltrReleaseIteration"
									runat="server" />
								<tstsc:LabelEx
									CssClass="pointer dib orange-hover transition-all"
									title="<%$Resources:Buttons,CopyToClipboard %>"
									data-copytoclipboard='true'
									ID="lblReleaseVersionNumber"
									runat="server" />
								<tstsc:LabelEx
									CssClass="pointer dib orange-hover transition-all"
									title="<%$Resources:Buttons,CopyToClipboard %>"
									data-copytoclipboard="true"
									ID="lblReleaseToken"
									runat="server" />
							</span>
						</div>
						<div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
							<tstsc:LabelEx
								AppendColon="true"
								AssociatedControlID="ddlType"
								ID="ddlTypeLabel"
								Required="true"
								runat="server"
								Text="<%$Resources:Fields,ReleaseTypeId %>" />
							<tstsc:UnityDropDownListEx
								ActiveItemField="IsActive"
								CssClass="pl2 u-dropdown is-active"
								DataTextField="Name"
								DataValueField="ReleaseTypeId"
								DisabledCssClass="u-dropdown disabled"
								ID="ddlType"
								runat="server" />
						</div>
                        <div class="py1 dif items-center ma0-children fs-h6 pr4 pr3-xs">
							<tstsc:LabelEx
								AppendColon="true"
								AssociatedControlID="ajxWorkflowOperations"
								ID="lblReleaseStatusLabel"
								Required="True"
								runat="server"
								Text="<%$Resources:Fields,ReleaseStatusId %>" />
							<div class="dib v-mid-children dif flex-wrap items-center pl3">
								<tstsc:LabelEx
									ID="lblReleaseStatusValue"
									MetaData=""
									runat="server" />
								<div class="dib ml4 v-mid-children dif items-center">
									<tstsc:HyperLinkEx
										ClientScriptMethod="workflow_revert()"
										ClientScriptServerControlId="ajxFormManager"
										ID="btnRevert"
										CssClass="btn orange-dark mln4"
										NavigateUrl="javascript:void(0)"
										runat="server">
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
										WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService" />
								</div>
							</div>
                        </div>
                        <div class="py1 dif items-center ma0-children fs-h6">
                            <tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="eqlCompletion"
                                CssClass="silver"
                                ID="eqlCompletionLabel" 
                                Required="false" 
                                runat="server" 
                                Text="<%$Resources:Fields,CompletionId %>" 
                                />
                            <span class="pl3 pr4">
                                <tstsc:Equalizer 
                                    ID="eqlCompletion" 
                                    runat="server" 
                                    />
                            </span>
						</div>
					</div>
				</div>
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
			</div>

			<tstuc:ArtifactEmail 
                ID="tstEmailPanel" 
                runat="server" 
                ArtifactId="<%# this.releaseId %>" 
                ArtifactTypeEnum="Release"
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="Release" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />

			<div class="main-content">



				<%-- TABS --%>
				<tstsc:TabControl ID="tclReleaseDetails" TabWidth="80"
					TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider" runat="server">
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
							AuthorizedArtifactType="Incident"
							AjaxServerControlId="grdIncidentList"
							Caption="<% $Resources:ServerControls,TabControl_Incidents %>"
							CheckPermissions="true"
							ID="tabIncidents"
							runat="server"
							TabPageControlId="pnlIncidents"
							TabPageImageUrl="Images/artifact-Incident.svg" 
							TabName="<%$GlobalFunctions:PARAMETER_TAB_INCIDENT %>"
							/>
						<tstsc:TabPage
							AjaxControlContainer="tstRequirementTaskPanel"
							AjaxServerControlId="grdReqTaskList"
							AuthorizedArtifactType="Task"
							Caption="<% $Resources:ServerControls,TabControl_ReqsTasks %>"
							CheckPermissions="true"
							ID="tabTasks"
							runat="server"
							TabPageControlId="pnlTasks"
							TabPageImageUrl="Images/artifact-Requirement.svg" 
							TabName="<%$GlobalFunctions:PARAMETER_TAB_REQUIREMENT %>"
							/>
						<tstsc:TabPage
							AjaxControlContainer="tstTestCaseMapping"
							AjaxServerControlId="grdAssociationLinks"
							AuthorizedArtifactType="TestCase"
							Caption="<% $Resources:ServerControls,TabControl_TestCases %>"
							CheckPermissions="true"
							ID="tabTestCases"
							runat="server"
							TabPageControlId="pnlTestCases"
							TabPageImageUrl="Images/artifact-TestCase.svg" 
							TabName="<%$GlobalFunctions:PARAMETER_TAB_COVERAGE %>"
							/>
						<tstsc:TabPage
							AjaxControlContainer="tstTestRunListPanel"
							AjaxServerControlId="grdTestRunList"
							AuthorizedArtifactType="TestRun"
							Caption="<% $Resources:ServerControls,TabControl_TestRuns %>"
							CheckPermissions="true"
							ID="tabTestRuns"
							runat="server"
							TabPageControlId="pnlTestRuns"
							TabPageImageUrl="Images/artifact-TestRun.svg" 
							TabName="<%$GlobalFunctions:PARAMETER_TAB_TESTRUN %>"
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
							AjaxControlContainer="tstBaselinePanel"
							AjaxServerControlId="grdBaselineList"
							Caption="<% $Resources:ServerControls,TabControl_Baselines %>"
							ID="tabBaseline"
							runat="server"
							TabPageControlId="pnlBaseline"
							TabPageImageUrl="Images/artifact-Baseline.svg" 
							TabName="<%$GlobalFunctions:PARAMETER_TAB_BASELINE %>"
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




				<%-- OVERVIEW PANEL --%>
				<asp:Panel ID="pnlOverview" runat="server" CssClass="TabControlPanel">
					<div class="u-wrapper width_md">





						<%-- USER FIELDS --%>
						<div class="u-box_1">
							<div
								class="u-box_group"
								data-collapsible="true"
								id="form-group_people">
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
									runat="server">
									<li class="ma0 pa0 lh0">
										<div 
                                            id="followersListBox" 
                                            class="u-box_list_control_no-label dib w-100"
                                            >
										</div>       
                                    </li>
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											runat="server"
											ID="ddlCreatorLabel"
											AssociatedControlID="ddlCreator"
											Text="<%$Resources:Fields,CreatorId %>"
											AppendColon="true"
											Required="true" />
										<tstsc:UnityDropDownUserList
											CssClass="u-dropdown u-dropdown_user"
											DataTextField="FullName"
											DataValueField="UserId"
											DisabledCssClass="u-dropdown u-dropdown_user disabled"
											ID="ddlCreator"
											runat="server"
											NoValueItem="True"
											NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>" />
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
											DataValueField="UserId"
											DataTextField="FullName"
											DisabledCssClass="u-dropdown u-dropdown_user disabled"
											ID="ddlOwner"
											NoValueItem="true"
											NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
											runat="server" />
									</li>
								</ul>
							</div>

                            <%-- ESTIMATE FIELDS --%>
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="form-group_estimates" >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:Fields,EstimatesAndEffort %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <ul 
                                    class="u-box_list" 
                                    id="customFieldsEstimates" 
                                    runat="server"
                                    >
                                    <asp:PlaceHolder runat="server" ID="plcPlannedEffort">
                                        <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="lblPlannedEffort" 
                                                ID="lblPlannedEffortLabel" 
                                                Required="false" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,PlannedEffort %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblPlannedEffort" 
                                                runat="server" 
                                                />
                                        </li>
                                        <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true"
                                                AssociatedControlID="lblUtilizedEffort" 
                                                ID="lblUtilizedEffortLabel" 
                                                Required="false"  
                                                runat="server" 
                                                Text="<%$Resources:Fields,UtilizedEffort %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblUtilizedEffort" 
                                                runat="server" 
                                                />
                                        </li>

                                        <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true"
                                                AssociatedControlID="lblAvailableEffort" 
                                                ID="lblAvailableEffortLabel" 
                                                Required="false"  
                                                runat="server" 
                                                Text="<%$Resources:Fields,AvailableEffort %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblAvailableEffort" 
                                                runat="server" 
                                                />
                                        </li>
                                    </asp:PlaceHolder>

                                    <asp:PlaceHolder runat="server" ID="plcPlannedPoints">
                                        <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="txtPlannedPoints" 
                                                ID="txtPlannedPointsLabel" 
                                                Required="false" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,PlannedPoints %>" 
                                                />
							                <tstsc:UnityTextBoxEx 
                                                CssClass="u-input w6"
                                                ID="txtPlannedPoints"
                                                MaxLength="9" 
                                                runat="server" 
                                                type="text"
                                                />
                                    </asp:PlaceHolder>
                                </ul>
                            </div>
						</div>




						<%-- DEFAULT FIELDS --%>
						<div class="u-box_1">
							<div
								class="u-box_group"
								data-collapsible="true"
								id="form-group_properties">
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
									runat="server">
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="txtVersionNumber"
											ID="txtVersionNumberLabel"
											Required="true"
											runat="server"
											Text="<%$Resources:Fields,VersionNumber %>" />
										<tstsc:UnityTextBoxEx
											CssClass="u-input"
											DisabledCssClass="u-input disabled"
											ID="txtVersionNumber"
											MaxLength="20"
											runat="server"
											TextMode="SingleLine" />
									</li>
								</ul>
							</div>

							<div
								class="u-box_group"
								data-collapsible="true"
								id="form-periodic-review">
								<div
									class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
									aria-expanded="true">
									<asp:Localize
										runat="server"
										Text="<%$Resources:Fields,PeriodicReviewLabel %>" />
									<span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
								</div>
								<ul
									class="u-box_list"
									id="Ul1"
									runat="server">
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="datPeriodicReviewDate"
											ID="periodReviewDateLabel"
											runat="server"
											Text="<%$Resources:Fields,PeriodicReviewDate %>" />
										<tstsc:UnityDateControl
											CssClass="u-datepicker"
											DisabledCssClass="u-datepicker disabled"
											ID="datPeriodicReviewDate"
											runat="Server" />
									</li>

									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="ddlPeriodicAlertTypes"
											ID="ddlPeriodicReviewAlertTypeLabel"
											Required="true"
											runat="server"
											Text="<%$Resources:Fields,PeriodicReviewAlert %>" />
										<tstsc:UnityDropDownListEx
											ActiveItemField="IsActive"
											CssClass="pl2 u-dropdown is-active"
											DataTextField="Name"
											DataValueField="PeriodicReviewAlertId"
											DisabledCssClass="u-dropdown disabled"
											ID="ddlPeriodicAlertTypes"
											NoValueItem="true"
											runat="server" />
									</li>

								</ul>
							</div>
						</div>

						
						<%-- DATE TIME FIELDS --%>
						<div class="u-box_1">
							<div
								class="u-box_group"
								data-collapsible="true"
								id="form-group_dates">
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
									runat="server">
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="lblCreationDate"
											ID="lblCreationDateLabel"
											Required="false"
											runat="server"
											Text="<%$Resources:Fields,CreationDate %>" />
										<asp:Label
											ID="lblCreationDate"
											runat="server" />
									</li>
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="lblLastUpdateDate"
											ID="lblLastUpdateDateLabel"
											Required="false"
											runat="server"
											Text="<%$Resources:Fields,LastUpdateDate %>" />
										<asp:Label
											ID="lblLastUpdateDate"
											runat="server" />
									</li>
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="datStartDate"
											ID="datStartDateLabel"
											Required="true"
											runat="server"
											Text="<%$Resources:Fields,StartDate %>" />
										<tstsc:UnityDateControl
											CssClass="u-datepicker"
											DisabledCssClass="u-datepicker disabled"
											ID="datStartDate"
											runat="Server" />
									</li>
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="datEndDate"
											ID="datEndDateLabel"
											Required="true"
											runat="server"
											Text="<%$Resources:Fields,EndDate %>" />
										<tstsc:UnityDateControl
											CssClass="u-datepicker"
											DisabledCssClass="u-datepicker disabled"
											ID="datEndDate"
											runat="Server" />
									</li>
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											runat="server"
											ID="txtResourceCountLabel"
											AssociatedControlID="txtResourceCount"
											Text="<%$Resources:Fields,NumberResources %>"
											Required="true"
											AppendColon="true" />
										<tstsc:UnityTextBoxEx
											CssClass="u-input"
											DisabledCssClass="u-input disabled"
											ID="txtResourceCount"
											MaxLength="6"
											runat="server"
											type="text" />
									</li>
									<li class="ma0 mb2 pa0">
										<tstsc:LabelEx
											AppendColon="true"
											AssociatedControlID="txtNonWorkDays"
											ID="txtNonWorkDaysLabel"
											Required="true"
											runat="server"
											Text="<%$Resources:Fields,NonWorkingWithPersonDays %>" />
										<tstsc:UnityTextBoxEx
											CssClass="u-input"
											DisabledCssClass="u-input disabled"
											ID="txtNonWorkDays"
											MaxLength="3"
											runat="server"
											type="text" />
									</li>
								</ul>
							</div>

							
						</div>

						<%-- PERIODIC REVIEW FIELDS --%>
						 
						<%-- RICH TEXT FIELDS --%>
						<div class="u-box_3">
							<div
								class="u-box_group u-cke_is-minimal"
								data-collapsible="true"
								id="form-group_richtext">
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
									runat="server">
									<li class="ma0 mb2 pa0">
										<tstsc:RichTextBoxJ
											Authorized_ArtifactType="Release"
											Authorized_Permission="Modify"
											ID="txtDescription"
											runat="server"
											Screenshot_ArtifactType="Release" />
									</li>
								</ul>
							</div>
						</div>


						<%-- BUILDS --%>
						<div class="u-box_3">
							<div
								class="u-box_group"
								data-collapsible="true"
								id="form-group_builds">
								<div
									class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
									aria-expanded="true">
									<asp:Localize
										runat="server"
										Text="<%$Resources:ServerControls,TabControl_Builds %>" />
									<span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
								</div>
								<ul class="u-box_list">
									<li class="ma0 mb5 pa0">
										<asp:Panel runat="server" ID="pnlOverview_Builds">
											<div class="btn-group">
												<tstsc:HyperLinkEx ID="lnkBuildRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)"
													ClientScriptServerControlId="grdBuildList" ClientScriptMethod="load_data()">
                                                    <span class="fas fa-sync"></span>
                                                    <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
												</tstsc:HyperLinkEx>
												<tstsc:HyperLinkEx ID="lnkBuildApplyFilter" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)"
													ClientScriptServerControlId="grdBuildList" ClientScriptMethod="apply_filters()">
                                                    <span class="fas fa-filter"></span>
                                                    <asp:Localize Text="<%$Resources:Buttons,ApplyFilter %>" runat="server" />
												</tstsc:HyperLinkEx>
												<tstsc:HyperLinkEx ID="lnkBuildClearFilter" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)"
													ClientScriptServerControlId="grdBuildList" ClientScriptMethod="clear_filters()">
                                                    <span class="fas fa-times"></span>
                                                    <asp:Localize Text="<%$Resources:Buttons,ClearFilter %>" runat="server" />
												</tstsc:HyperLinkEx>
											</div>
											<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
											<asp:Label ID="lblVisibleCount" runat="server" Font-Bold="True" />
											<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
											<asp:Label ID="lblTotalCount" runat="server" Font-Bold="True" />
											<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,ReleaseDetails_BuildsInRelease %>" />.
                                            <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
											<div class="Spacer"></div>
											<tstsc:SortedGrid
												AllowEditing="false"
												ConcurrencyEnabled="false"
												CssClass="DataGrid DataGrid-no-bands"
												DisplayAttachments="false"
												DisplayCheckboxes="false"
												EditRowCssClass="Editing"
												ErrorMessageControlId="lblMessages"
												FilterInfoControlId="lblFilterInfo"
												HeaderCssClass="Header"
												ID="grdBuildList"
												ItemImage="artifact-Build.svg"
												RowCssClass="Normal"
												runat="server"
												SelectedRowCssClass="Highlighted"
												SubHeaderCssClass="SubHeader"
												TotalCountControlId="lblTotalCount"
												VisibleCountControlId="lblVisibleCount"
												WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BuildService"
												AutoLoad="false" />
										</asp:Panel>
									</li>
								</ul>
							</div>
						</div>


						<%-- COMMENTS --%>
						<div class="u-box_3">
							<div
								class="u-box_group"
								data-collapsible="true"
								id="form-group_comments">
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
											ArtifactType="Release"
											AutoLoad="false"
											ErrorMessageControlId="lblMessage"
											ID="lstComments"
											runat="server"
											WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService"
											Width="100%" />
										<tstsc:RichTextBoxJ
											Authorized_ArtifactType="Release"
											Authorized_Permission="Modify"
											Height="80px"
											ID="txtNewComment"
											runat="server"
											Screenshot_ArtifactType="Release" />
										<div class="mtn4">
											<tstsc:ButtonEx
												ClientScriptServerControlId="lstComments"
												ID="btnNewComment"
												runat="server"
												SkinID="Btn_withComments"
												Text="<%$Resources:Buttons,AddComment %>" />
										</div>
									</li>
								</ul>
							</div>
						</div>
					</div>
				</asp:Panel>




				<%-- INCIDENT TAB --%>
				<asp:Panel runat="server" ID="pnlIncidents" CssClass="TabControlPanel">
					<tstsc:MessageBox ID="divIncidentsMessage" runat="server" SkinID="MessageBox" />
					<div class="TabControlHeader">
						<div class="Legend">
							<asp:Localize runat="server" Text="<%$Resources:Main,ReleaseDetails_DisplayIncidents %>" />
						</div>
						<div class="btn-group priority1">
							<tstsc:DropDownListEx ID="ddlIncidentReleaseFilterType" runat="server">
								<asp:ListItem Value="1" Text="<%$Resources:ServerControls,IncidentReleaseType_DetectedRelease %>" />
								<asp:ListItem Value="2" Text="<%$Resources:ServerControls,IncidentReleaseType_ResolvedRelease %>" />
								<asp:ListItem Value="3" Text="<%$Resources:ServerControls,IncidentReleaseType_VerifiedRelease %>" />
							</tstsc:DropDownListEx>
						</div>
						<div class="btn-group priority3">
							<tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)"
								ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="load_data()">
                                <span class="fas fa-sync"></span>
                                <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
							</tstsc:HyperLinkEx>
							<tstsc:DropMenu ID="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
								ClientScriptServerControlId="grdIncidentList" ClientScriptMethod="apply_filters()">
								<DropMenuItems>
									<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
									<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
								</DropMenuItems>
							</tstsc:DropMenu>
						</div>
						<div class="btn-group priority4">
							<tstsc:DropDownListEx ID="ddlShowHideIncidentColumns" runat="server" DataValueField="Key"
								DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True"
								NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdIncidentList"
								ClientScriptMethod="toggle_visibility" />
						</div>
					</div>
					<div class="bg-near-white-hover py2 px3 br2 transition-all">
						<button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
						<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
						<asp:Label ID="lblIncidentVisibleCount" runat="server" Font-Bold="True" />
						<asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
						<asp:Label ID="lblIncidentTotalCount" runat="server" Font-Bold="True" />
						<asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,ReleaseDetails_Incidents %>" />.
                        <tstsc:LabelEx ID="lblIncidentFilterInfo" runat="server" />
					</div>
					<tstsc:SortedGrid ID="grdIncidentList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" AllowColumnPositioning="true"
						VisibleCountControlId="lblIncidentVisibleCount" TotalCountControlId="lblIncidentTotalCount" FilterInfoControlId="lblIncidentFilterInfo"
						SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divIncidentsMessage"
						RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-Incident.svg" runat="server"
						WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" Authorized_Permission="ProjectAdmin"
						ConcurrencyEnabled="true" />
					<br />
					<script type="text/javascript">
						/* The User Control Class */
						Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
						Inflectra.SpiraTest.Web.UserControls.IncidentPanel = function () {
							this._projectId = <%=ProjectId %>;
							this._artifactId = <%=releaseId %>;
							this._artifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Release %>;

							Inflectra.SpiraTest.Web.UserControls.IncidentPanel.initializeBase(this);
						}
						Inflectra.SpiraTest.Web.UserControls.IncidentPanel.prototype =
						{
							initialize: function () {
								Inflectra.SpiraTest.Web.UserControls.IncidentPanel.callBaseMethod(this, 'initialize');
							},
							dispose: function () {
								Inflectra.SpiraTest.Web.UserControls.IncidentPanel.callBaseMethod(this, 'dispose');
							},

							/* Properties */
							get_artifactId: function () {
								return this._artifactId;
							},
							set_artifactId: function (value) {
								this._artifactId = value;
							},

							get_artifactTypeId: function () {
								return this._artifactTypeId;
							},
							set_artifactTypeId: function (value) {
								this._artifactTypeId = value;
							},

							/* Functions */
							check_hasData: function (callback) {
								//See if we have data
								var artifactReference = {
									artifactId: this._artifactId,
									artifactTypeId: this._artifactTypeId,
								};
								Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
							},
							check_hasData_success: function (count, callback) {
								if (callback) {
									//Specify if we have data or not
									callback(count > 0);
								}
							},
							check_hasData_failure: function (ex) {
								//Fail quietly
							},

							load_data: function (filters, loadNow) {
								var grdIncidentList = $find('<%=grdIncidentList.ClientID%>');
								grdIncidentList.set_standardFilters(filters);
								if (loadNow) {
									grdIncidentList.load_data();
								}
								else {
									grdIncidentList.clear_loadingComplete();
								}
							}
						}
						var tstucIncidentPanel = $create(Inflectra.SpiraTest.Web.UserControls.IncidentPanel);
					</script>
				</asp:Panel>




				<%-- TEST RUN TABS --%>
				<asp:Panel runat="server" ID="pnlTestRuns" CssClass="TabControlPanel">
					<tstuc:TestRunListPanel ID="tstTestRunListPanel" runat="server" />
				</asp:Panel>

				<!-- TASK TAB -->
				<asp:Panel ID="pnlTasks" runat="server" CssClass="TabControlPanel">
					<tstuc:RequirementTaskPanel ID="tstRequirementTaskPanel" runat="server" />
				</asp:Panel>

				<!-- TEST CASES TAB -->
				<asp:Panel
					CssClass="TabControlPanel"
					ID="pnlTestCases"
					runat="server">
					<tstuc:AssociationsPanel
						ID="tstTestCaseMapping"
						runat="server" />
				</asp:Panel>

				<!-- ATTACHMENTS TAB -->
				<asp:Panel ID="pnlAttachments" runat="server" CssClass="TabControlPanel">
					<tstuc:AttachmentPanel ID="tstAttachmentPanel" runat="server" />
					<br />
				</asp:Panel>

				<!-- HISTORY TAB -->
				<asp:Panel ID="pnlHistory" runat="server" CssClass="TabControlPanel">
					<tstuc:HistoryPanel ID="tstHistoryPanel" runat="server" />
				</asp:Panel>

				<!-- BASELINES TAB -->
				<asp:Panel ID="pnlBaseline" runat="server" CssClass="TabControlPanel">
					<tstuc:BaselinePanel ID="tstBaselinePanel" runat="server" />
				</asp:Panel>
			</div>
		</div>
	</div>

	<tstsc:AjaxFormManager
		AlternateItemImage="Images/artifact-Iteration.svg"
		ArtifactImageControlId="imgReleaseIteration"
		ArtifactTypeName="<%$Resources:Fields,Release%>"
		CheckUnsaved="true"
		DisplayPageName="true"
		ErrorMessageControlId="lblMessage"
		FolderPathControlId="pnlFolderPath"
		ID="ajxFormManager"
		ItemImage="Images/artifact-Release.svg"
		SummaryItemImage="Images/artifact-Release.svg"
		RevertButtonControlId="btnRevert"
		runat="server"
		WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService"
		WorkflowEnabled="true"
		WorkflowOperationsControlId="ajxWorkflowOperations">
		<ControlReferences>
			<tstsc:AjaxFormControl ControlId="lblReleaseStatusValue" DataField="ReleaseStatusId" Direction="In" IsWorkflowStep="true" />
			<tstsc:AjaxFormControl ControlId="ddlType" DataField="ReleaseTypeId" Direction="InOut" ChangesWorkflow="true" />
			<tstsc:AjaxFormControl ControlId="lblReleaseName" DataField="Name" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblReleaseVersionNumber" DataField="VersionNumber" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblReleaseToken" DataField="ReleaseId" Direction="In" />
			<tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtVersionNumber" DataField="VersionNumber" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlCreator" DataField="CreatorId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="datPeriodicReviewDate" DataField="PeriodicReviewDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="ddlPeriodicAlertTypes" DataField="PeriodicReviewAlertId" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="datStartDate" DataField="StartDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="datEndDate" DataField="EndDate" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In" PropertyName="tooltip" />
			<tstsc:AjaxFormControl ControlId="txtResourceCount" DataField="ResourceCount" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtNonWorkDays" DataField="DaysNonWorking" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />
			<tstsc:AjaxFormControl ControlId="lblPlannedEffort" DataField="PlannedEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblAvailableEffort" DataField="AvailableEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblEstimatedEffort" DataField="TaskEstimatedEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblProjectedEffort" DataField="TaskProjectedEffort" Direction="In" />
			<tstsc:AjaxFormControl ControlId="lblActualEffort" DataField="TaskActualEffort" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtPlannedPoints" DataField="PlannedPoints" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblUtilizedEffort" DataField="TaskProjectedEffort" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblAvailableEffort2" DataField="AvailableEffort" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblPlannedPoints" DataField="PlannedPoints" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblRequirementPoints" DataField="RequirementPoints" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblAvailablePoints" DataField="AvailablePoints" Direction="In" />
            <tstsc:AjaxFormControl ControlId="eqlCompletion" DataField="PercentComplete" Direction="In" />
		</ControlReferences>
		<SaveButtons>
			<tstsc:AjaxFormSaveButton ControlId="btnSave" />
		</SaveButtons>
	</tstsc:AjaxFormManager>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />
		</Services>
		<Scripts>
			<asp:ScriptReference Path="~/TypeScript/Followers.js" />
			<asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
		</Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
	<script type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;

		SpiraContext.pageId = "Inflectra.Spira.Web.ReleaseDetails";
		SpiraContext.ArtifactId = <%=releaseId %>;
		SpiraContext.ArtifactIdOnPageLoad = <%=releaseId %>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTypeId = <%= (int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Release %>;
		SpiraContext.EmailEnabled = <%= Inflectra.SpiraTest.Common.ConfigurationSettings.Default.EmailSettings_Enabled.ToString().ToLowerInvariant() %>;
		SpiraContext.HasCollapsiblePanels = true;
		SpiraContext.Mode = 'update';

		//Server Control IDs
		var btnSubscribe_id = '<%=this.btnSubscribe.ClientID%>';
		var btnEmail_id = '<%=btnEmail.ClientID%>';
		var ajxFormManager_id = '<%= ajxFormManager.ClientID %>';
		var lstComments_id = '<%= lstComments.ClientID %>';
		var btnNewComment_id = '<%= btnNewComment.ClientID %>';
		var txtName_id = '<%= txtName.ClientID %>';
		var btnSave_id = '<%= btnSave.ClientID %>';
		var grdScenarioSteps_id = '<%= grdBuildList.ClientID %>';
		var tabControl_id = '<%= tclReleaseDetails.ClientID %>';
		var navigationBar_id = '<%= navReleaseList.ClientID %>';

		//TabControl Panel IDs
		var pnlAttachments_id = '<%= pnlAttachments.ClientID %>';
		var pnlHistory_id = '<%= pnlHistory.ClientID %>';
		var pnlReqsTasks_id = '<%= pnlTasks.ClientID %>';
		var pnlTestCases_id = '<%= pnlTestCases.ClientID %>';
		var pnlIncidents_id = '<%= pnlIncidents.ClientID %>';
		var pnlTestRuns_id = '<%= pnlTestRuns.ClientID %>';
		var pnlBaseline_id = '<%= pnlBaseline.ClientID %>';

		//Base URLs
		var urlTemplate_artifactRedirectUrl = '<%= ReleaseRedirectUrl %>';
		var urlTemplate_artifactListUrl = '<%= Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToReleaseListUrl) %>';
		var urlTemplate_screenshot = '<%= GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}"))) %>';
		var urlTemplate_projectHome = '<%= GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0))) %>';

		function ddlIncidentReleaseFilterType_selectedIndexChanged(evt) {
			//Change the filter and reload the incident list
			var filterType = loadIncidents();

			//Finally update the stored settings
			var projectId = SpiraContext.ProjectId;
			Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.Incident_UpdateReleaseDetailsFilter(projectId, filterType);
		}
		function ajxWorkflowOperations_operationExecuted(transitionId, isStatusOpen) {
			//Put any post-workflow operations here
		}

		//Prints the current items
		function print_item(format) {
			var artifactId = SpiraContext.ArtifactId;

			//Get the report type from the format
			var reportToken;
			var filter;
			if (format == 'excel') {
				reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.ReleaseSummary%>";
				filter = "&af_16_98=";
			}
			else {
				reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.ReleaseDetailed%>";
				filter = "&af_17_98=";
			}

			//Open the report for the specified format
			globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
		}

		//Updates any page specific content
		var releaseDetails_releaseId = -1;
		function updatePageContent() {
			tstucIncidentPanel.set_artifactId(SpiraContext.ArtifactId);

			//Check to see if we have data
			tstucIncidentPanel.check_hasData(incidents_callback);

			//See if the artifact id has changed
			var grdBuildList = $find('<%=this.grdBuildList.ClientID%>');
			if (releaseDetails_releaseId != SpiraContext.ArtifactId) {
				//Load the scenario steps
				var filters = {};
				filters[globalFunctions.keyPrefix + 'ReleaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
				grdBuildList.set_standardFilters(filters);
				grdBuildList.load_data();
				releaseDetails_releaseId = SpiraContext.ArtifactId;

				//See if the tab is visible
				var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlIncidents_id);

				//Reload the incidents grid
				loadIncidents();
			}
		}

		function loadIncidents() {
			//Get the display type from the dropdown list
			var ddlIncidentReleaseFilterType = $find('<%=ddlIncidentReleaseFilterType.ClientID%>');
			var filterType = ddlIncidentReleaseFilterType.get_selectedItem().get_value();

			var filters = {};
			if (filterType == 1) {
				filters['DetectedReleaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
			}
			if (filterType == 2) {
				filters['ResolvedReleaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
			}
			if (filterType == 3) {
				filters['VerifiedReleaseId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
			}
			var grdIncidentList = $find('<%=grdIncidentList.ClientID%>');
			grdIncidentList.set_standardFilters(filters);
			grdIncidentList.load_data();

			return filterType;
		}
    </script>
</asp:Content>
