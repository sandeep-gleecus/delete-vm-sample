<%@ Page 
	AutoEventWireup="True" 
    CodeBehind="ResourceDetails.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.ResourceDetails" 
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    ValidateRequest="false" 
%>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>
<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
			<tstsc:NavigationBar 
                AutoLoad="true" 
                BodyHeight="580px" 
                ErrorMessageControlId="lblClientMessage"
                ID="navResourceList" 
				IncludeAssigned="false" 
                ItemImage="Images/artifact-Resource.svg"
				ListScreenCaption="<%$Resources:Main,ResourceDetails_BackToList%>" 
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.UserService" 
                />
		</div>



        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar" role="toolbar">
                <div class="btn-group priority1 hidden-md hidden-lg" role="group">
                    <tstsc:HyperlinkEx ID="btnBack" runat="server" SkinID="ButtonDefault" NavigateUrl=<%#ReturnToListPageUrl%> ToolTip="<%$Resources:Main,TaskDetails_BackToList%>">
                        <span class="fas fa-arrow-left"></span>
                    </tstsc:HyperlinkEx>
                </div>
            </div>



            <div class="main-content">
                <h2 class="ml4">
                    <tstsc:LabelEx 
                        runat="server" 
                        ID="lblResourceName" 
                        />
                    <small>
                        <asp:Localize 
                            runat="server" 
                            Text="<%$Resources:Main,ResourceDetails_ResourceDetails %>" 
                            />
                    </small>
                </h2>
				<tstsc:MessageBox ID="divMessage" runat="server" SkinID="MessageBox" />
				<tstsc:MessageBox ID="lblClientMessage" runat="server" SkinID="MessageBox" />






                <div class="u-wrapper width_md clearfix">
                            
                            
                            
                <%-- INFO FIELDS --%>
                    <div class="u-box_1">
                    <div class="u-box_group">
                        <ul class="u-box_list" >
                            <li class="ma0 mb2 pa0">
								<tstsc:LabelEx 
                                    AppendColon="true"
                                    AssociatedControlID="lblName" 
                                    ID="lblNameLabel" 
                                    runat="server" 
                                    Text="<%$Resources:Fields,Name %>" 
                                    />
								<span>
                                    <tstsc:UserOnlineStatus ID="ajxUserStatus" runat="server" />
									<asp:Label ID="lblName" runat="server" />
								</span>	
                            </li>
                            <li class="ma0 mb2 pa0">
                                <tstsc:LabelEx 
                                    AppendColon="true"
                                    AssociatedControlID="imgAvatar" 
                                    ID="imgAvatarLabel" 
                                    Required="false" 
                                    runat="server" 
                                    Text="<%$Resources:Main,UserProfile_Logo %>" 
                                    />
                                <tstsc:UserNameAvatar 
                                    ID="imgAvatar" 
                                    runat="server" 
                                    AvatarSize="100" 
                                    ShowFullName="false" 
                                    ShowUserName="false" 
                                    />
                            </li>
                            <li class="ma0 mb2 pa0">
								<tstsc:LabelEx 
                                    AppendColon="true"
                                    runat="server" 
                                    ID="lblEmailLabel" 
                                    AssociatedControlID="lblEmail" 
                                    Text="<%$Resources:Fields,Email %>" 
                                    />
								<asp:HyperLink 
                                    ID="lblEmail" 
                                    runat="server" 
                                    />
                            </li>
                            <li class="ma0 mb2 pa0">
								<tstsc:LabelEx 
                                    AppendColon="true" 
                                    AssociatedControlID="lblDepartment"
                                    ID="lblDepartmentLabel" 
                                    runat="server" 
									Text="<%$Resources:Fields,Department %>" 
                                    />
								<asp:Label 
                                    ID="lblDepartment" 
                                    runat="server" 
                                    />
                            </li>
                            <li class="ma0 mb2 pa0">
								<tstsc:LabelEx 
                                    AppendColon="true" 
                                    runat="server" 
                                    ID="lblRoleLabel" 
                                    AssociatedControlID="lblRole" 
                                    Text="<%$Resources:Fields,ProjectRole %>" 
                                    />
								<asp:Label 
                                    ID="lblRole" 
                                    runat="server" 
                                    />
                            </li>
                            <li class="ma0 mt4 pa0 pt2 br2-top pl4 mln4 bg-near-white">
								<tstsc:LabelEx 
                                    Font-Bold="true" 
                                    runat="server" 
                                    Text="<%$Resources:Main,ResourceDetails_CurrentPlanningSettings %>" 
                                    />
                            </li>
                            <li class="ma0 pa0 pl4 mln4 bg-near-white">
								<tstsc:LabelEx 
                                    AppendColon="true" 
                                    CssClass="u-box_list_label"
                                    runat="server" 
                                    Text="<%$Resources:Main,ResourceDetails_HousePerDay %>" 
                                    />
								<asp:Label 
                                    runat="server" 
                                    ID="lblHrsDay" 
                                    />
                            </li>
                            <li class="ma0 pa0 pl4 mln4 bg-near-white">
								<tstsc:LabelEx 
                                    AppendColon="true" 
                                    CssClass="u-box_list_label"
                                    ID="Localize1" 
                                    runat="server" 
                                    Text="<%$Resources:Main,ResourceDetails_DaysPerWeek %>" 
                                    />
								<asp:Label 
                                    runat="server" 
                                    ID="lblDaysWeek" 
                                    />
                            </li>
                            <li class="ma0 pa0 mb4 pb2 br2-bottom pl4 mln4 bg-near-white">
								<tstsc:LabelEx 
                                    AppendColon="true" 
                                    CssClass="u-box_list_label"
                                    runat="server" 
                                    Text="<%$Resources:Main,ResourceDetails_NonWorkHoursPerMonth %>"
                                    />
								<asp:Label 
                                    runat="server" 
                                    ID="lblNonWorkMonth" 
                                    />
                            </li>
                            <li class="ma0 mb2 pa0">
                                <div class="btn-group">
                                    <asp:PlaceHolder ID="plcMessageOptions" runat="server">
                                        <tstsc:HyperLinkEx ID="btnSendMessage" SkinID="ButtonDefault" runat="server"  NavigateUrl="javascript:void(0)">
                                            <span class="far fa-comment"></span>
                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,SendMessage %>" />
                                        </tstsc:HyperLinkEx>
                                        <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-trash-alt" ID="btnRemoveContact" runat="server" Text="<%$Resources:Buttons,RemoveContact %>" />
                                        <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-plus" ID="btnAddContact" runat="server" Text="<%$Resources:Buttons,AddContact %>" />
                                    </asp:PlaceHolder>
                                </div>
                            </li>
                        </ul>
                    </div>
                </div>
                </div>
                <div class="mt5 ml3">
                    <div>
						<tstsc:LabelEx ID="ddlSelectReleaseLabel" runat="server" Font-Bold="true" Text="<%$Resources:Main,Global_DisplayDataFor %>" AssociatedControlID="ddlSelectRelease" AppendColon="true" />
				    </div>
                    <tstsc:DropDownHierarchy ID="ddlSelectRelease" Runat="server" NoValueItem="true" NoValueItemText="<%$Resources:Dialogs,Global_AllReleases %>" AutoPostBack="true"
                        DataTextField="FullName" DataValueField="ReleaseId"
                        ActiveItemField="IsActive" SkinID="ReleaseDropDownList" />

					<tstsc:TabControl ID="tclResourceDetails" CssClass="TabControl2" TabWidth="125" TabHeight="25"
						TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider"
						DisabledTabCssClass="TabDisabled" runat="server">
						<TabPages>
							<tstsc:TabPage 
                                AjaxServerControlId="grdReqTaskList" 
                                AuthorizedArtifactType="Requirement"
								Caption="<%$Resources:ServerControls,TabControl_ReqsTasks %>" 
                                CheckPermissions="true" 
                                ID="tabRQ" 
                                runat="server" 
                                TabPageControlId="pnlRQTK" 
                                TabPageImageUrl="Images/artifact-Requirement.svg"
                                TabName="<%$ GlobalFunctions:PARAMETER_TAB_REQUIREMENT %>"
                                />
							<tstsc:TabPage 
                                AuthorizedArtifactType="Incident"
								Caption="<%$Resources:ServerControls,TabControl_Incidents %>" 
                                CheckPermissions="true" 
                                ID="tabIN" 
                                runat="server" 
                                TabPageControlId="pnlIN" 
                                TabPageImageUrl="Images/artifact-Incident.svg"
                                TabName="<%$ GlobalFunctions:PARAMETER_TAB_INCIDENT %>"
                                />
							<tstsc:TabPage 
                                AuthorizedArtifactType="TestCase"
								Caption="<%$Resources:ServerControls,TabControl_TestCases %>" 
                                CheckPermissions="true" 
                                ID="tabTC" 
                                runat="server" 
                                TabPageControlId="pnlTC" 
                                TabPageImageUrl="Images/artifact-TestCase.svg"
                                TabName="<%$ GlobalFunctions:PARAMETER_TAB_TESTCASE %>"
                                />
							<tstsc:TabPage 
                                AuthorizedArtifactType="TestSet"
								Caption="<%$Resources:ServerControls,TabControl_TestSets %>" 
                                CheckPermissions="true" 
                                ID="tabTX" 
                                runat="server" 
                                TabPageControlId="pnlTX" 
                                TabPageImageUrl="Images/artifact-TestSet.svg"
                                TabName="<%$ GlobalFunctions:PARAMETER_TAB_TESTSET %>"
                                />
							<tstsc:TabPage 
                                AjaxServerControlId="grdUserActivity"
                                Caption="<% $Resources:Fields,Actions %>" 
                                ID="tabActions" 
                                runat="server" 
								TabPageControlId="pnlActions"   
                                TabPageIcon="fas fa-history"
                                TabName="<%$ GlobalFunctions:PARAMETER_TAB_ACTION %>"
                                />
						</TabPages>
					</tstsc:TabControl>
					<asp:Panel ID="pnlRQTK" runat="server" CssClass="TabControlPanel">
                        <div style="width:100%;" class="TabControlHeader">
                            <div class="btn-group priority3">
                                <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkRefreshRQTK" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="load_data()">
                                    <span class="fas fa-sync"></span>
                                    <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
                                </tstsc:HyperLinkEx>
                                <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                        ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="apply_filters()">
			                        <DropMenuItems>
				                        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				                        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			                        </DropMenuItems>
		                        </tstsc:DropMenu>
                            </div>
                            <tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="False" DataTextField="Value" DataValueField="Key" CssClass="DropDownList" ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="expand_to_level" Width="150px">
                                <asp:ListItem Value="1" Text="<%$Resources:Dialogs,Global_ExpandAll %>" />
                                <asp:ListItem Value="2" Text="<%$Resources:Dialogs,Global_CollapseAll %>" />
                            </tstsc:DropDownListEx>
                            <div>
                                <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                                <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	                            <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                                <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	                            <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Main,RequirementTaskPanel_Items %>" />.
                                <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
                            </div>
                            <div style="clear: both">
                            </div>
                        </div>
						<tstsc:HierarchicalGrid ID="grdReqTaskList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
                            VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
							SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
							RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-Task.svg" SummaryItemImage="artifact-Requirement.svg"
							runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsTaskService"
							Authorized_ArtifactType="Requirement" Authorized_Permission="Modify" ConcurrencyEnabled="true"
							AutoLoad="false" ForceTwoLevelIndent="true">
						</tstsc:HierarchicalGrid>
					</asp:Panel>
					<asp:Panel ID="pnlIN" runat="server" CssClass="TabControlPanel">
						<tstsc:GridViewEx ID="grdOwnedIncidents" runat="server" EnableViewState="false" CssClass="DataGrid DataGrid-no-bands"
							WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" HeaderStyle-CssClass="Header"
							SubHeaderStyle-CssClass="SubHeader" AutoGenerateColumns="false" Width="100%">
							<Columns>
								<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon priority1" HeaderStyle-CssClass="priority1" HeaderText="<%$Resources:Fields,Name %>">
									<ItemTemplate>
										<tstsc:ImageEx CssClass="w4 h4" ImageUrl="Images/artifact-Incident.svg" AlternateText="Incident" ID="imgIcon"
											runat="server" />
									</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name"
									NameMaxLength="40" CommandArgumentField="IncidentId" ItemStyle-CssClass="priority1"/>
								<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>"  ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4">
									<ItemTemplate>
										[<%# GlobalFunctions.ARTIFACT_PREFIX_INCIDENT %>:<%# ((IncidentView)Container.DataItem).IncidentId.ToString("000000") %>]</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderText="<%$Resources:Fields,CreationDate %>">
									<ItemTemplate>
										<tstsc:LabelEx runat="server" Text='<%# string.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(((IncidentView) Container.DataItem).CreationDate)) %>'
											ToolTip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((IncidentView) Container.DataItem).CreationDate)) %>'
											ID="lblCreationDate" />
									</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:BoundFieldEx ItemStyle-Wrap="false" DataField="IncidentStatusName" HtmlEncode="false"
									HeaderText="<%$Resources:Fields,IncidentStatusId %>"/>
								<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="PriorityName" HtmlEncode="false"
									HeaderText="<%$Resources:Fields,PriorityId %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" />
								<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="SeverityName" HtmlEncode="false"
									HeaderText="<%$Resources:Fields,SeverityId %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" />
								<tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,Progress %>">
									<ItemTemplate>
										<tstsc:Equalizer ID="eqlProgress" runat="server" CssClass="Equalizer" />
									</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,EstimatedEffort %>">
									<ItemTemplate>
										<%# this.GetTimeFromMinutes(((IncidentView)Container.DataItem).EstimatedEffort) %></ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,ProjectedEffort %>">
									<ItemTemplate>
										<%# this.GetTimeFromMinutes(((IncidentView)Container.DataItem).ProjectedEffort)%></ItemTemplate>
								</tstsc:TemplateFieldEx>
							</Columns>
						</tstsc:GridViewEx>
					</asp:Panel>
					<asp:Panel ID="pnlTC" runat="server" CssClass="TabControlPanel">
						<tstsc:GridViewEx ID="grdOwnedTestCases" runat="server" EnableViewState="false" CssClass="DataGrid DataGrid-no-bands"
							WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService" HeaderStyle-CssClass="Header"
							SubHeaderStyle-CssClass="SubHeader" AutoGenerateColumns="false" Width="100%">
							<Columns>
								<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon priority1" HeaderStyle-CssClass="priority1" HeaderText="<%$Resources:Fields,Name %>">
									<ItemTemplate>
										<tstsc:ImageEx ImageUrl="Images/artifact-TestCase.svg" CssClass="w4 h4" AlternateText="TestCase" ID="imgIcon"
											runat="server" />
									</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name"
									NameMaxLength="40" CommandArgumentField="TestCaseId" ItemStyle-CssClass="priority1"/>
								<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>"  ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4">
									<ItemTemplate>
										[<%# GlobalFunctions.ARTIFACT_PREFIX_TEST_CASE%>:<%# ((int)((Entity)Container.DataItem)["TestCaseId"]).ToString("000000") %>]</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderText="<%$Resources:Fields,ExecutionDate %>">
									<ItemTemplate>
										<tstsc:LabelEx runat="server" Text='<%# (((Entity)Container.DataItem)["ExecutionDate"] == null) ? "-" : String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate((DateTime)((Entity)Container.DataItem)["ExecutionDate"])) %>'
											ToolTip='<%# (((Entity)Container.DataItem)["ExecutionDate"] == null) ? "-" : String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate((DateTime)((Entity)Container.DataItem)["ExecutionDate"])) %>'
											ID="lblExecutionDate" />
                                        </ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:BoundFieldEx ItemStyle-Wrap="false" DataField="ExecutionStatusName" HtmlEncode="false"
									HeaderText="<%$Resources:Fields,ExecutionStatus %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2"/>
								<tstsc:BoundFieldEx ItemStyle-Wrap="false" DataField="TestCasePriorityName" HtmlEncode="false"
									HeaderText="<%$Resources:Fields,TestCasePriorityId %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2"/>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,EstimatedDuration %>">
									<ItemTemplate>
										<%# this.GetTimeFromMinutes(((Entity)Container.DataItem)["EstimatedDuration"]) + "h"%></ItemTemplate>
								</tstsc:TemplateFieldEx>
							</Columns>
						</tstsc:GridViewEx>
					</asp:Panel>
					<asp:Panel ID="pnlTX" runat="server" CssClass="TabControlPanel">
						<tstsc:GridViewEx ID="grdOwnedTestSets" runat="server" EnableViewState="false" CssClass="DataGrid DataGrid-no-bands"
							WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService" HeaderStyle-CssClass="Header"
							SubHeaderStyle-CssClass="SubHeader" AutoGenerateColumns="false" Width="100%">
							<Columns>
								<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon priority1" HeaderStyle-CssClass="priority1" HeaderText="<%$Resources:Fields,Name %>">
									<ItemTemplate>
										<tstsc:ImageEx CssClass="w4 h4" ImageUrl="Images/artifact-TestSet.svg" AlternateText="TestCase" ID="imgIcon"
											runat="server" />
									</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name"
									NameMaxLength="40" CommandArgumentField="TestSetId" ItemStyle-CssClass="priority1"/>
								<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" ItemStyle-CssClass="priority4" HeaderStyle-CssClass="priority4">
									<ItemTemplate>
										[<%# GlobalFunctions.ARTIFACT_PREFIX_TEST_SET%>:<%# ((int)((Entity)Container.DataItem)["TestSetId"]).ToString("000000") %>]</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="TestSetStatusName" HeaderText="<%$Resources:Fields,TestSetStatusId %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2"/>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderText="<%$Resources:Fields,PlannedDate %>">
									<ItemTemplate>
										<tstsc:LabelEx runat="server" ID="lblPlannedDate" />
                                    </ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ExecutionStatus %>">
									<ItemTemplate>
										<tstsc:Equalizer runat="server" ID="eqlExecutionStatus" />
									</ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderText="<%$Resources:Fields,ExecutionDate %>" ItemStyle-CssClass="priority2" HeaderStyle-CssClass="priority2">
									<ItemTemplate>
										<tstsc:LabelEx runat="server" Text='<%# (((Entity)Container.DataItem)["ExecutionDate"] == null) ? "-" : String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate((DateTime)((Entity)Container.DataItem)["ExecutionDate"])) %>'
											ToolTip='<%# (((Entity)Container.DataItem)["ExecutionDate"] == null) ? "-" : String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate((DateTime)((Entity)Container.DataItem)["ExecutionDate"])) %>'
											ID="lblExecutionDate" />
                                        </ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,EstimatedDuration %>">
									<ItemTemplate>
										<%# this.GetTimeFromMinutes(((Entity)Container.DataItem)["EstimatedDuration"]) + "h" %></ItemTemplate>
								</tstsc:TemplateFieldEx>
								<tstsc:TemplateFieldEx ItemStyle-Wrap="false" HeaderText="<%$Resources:Fields,ActualDuration %>">
									<ItemTemplate>
										<%# this.GetTimeFromMinutes(((Entity)Container.DataItem)["ActualDuration"]) + "h" %></ItemTemplate>
								</tstsc:TemplateFieldEx>
							</Columns>
						</tstsc:GridViewEx>
					</asp:Panel>
					<asp:Panel ID="pnlActions" runat="server">
                        <div class="TabControlHeader">
                            <div class="btn-group priority3">
                                <tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" 
									NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdUserActivity"
									ClientScriptMethod="load_data()">
                                    <span class="fas fa-sync"></span>
                                    <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
                                </tstsc:HyperLinkEx>
                                <tstsc:DropMenu id="DropMenu1" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			                        ClientScriptServerControlId="grdUserActivity" ClientScriptMethod="apply_filters()">
			                        <DropMenuItems>
				                        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				                        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			                        </DropMenuItems>
		                        </tstsc:DropMenu>
                            </div>
                            <div>
								<asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
								<asp:Label ID="lblCount" runat="server" Font-Bold="True" />
								<asp:Localize runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
								<asp:Label ID="lblTotal" runat="server" Font-Bold="True" />
								<asp:Localize runat="server" Text="<%$Resources:Main,Global_Items %>" />.
                            </div>  
						</div>
						<tstsc:SortedGrid ID="grdUserActivity" runat="server" EnableViewState="false" CssClass="DataGrid DataGrid-no-bands"
							WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.HistoryActivityService" VisibleCountControlId="lblCount"
							TotalCountControlId="lblTotal" HeaderCssClass="Header" RowCssClass="Normal" DisplayAttachments="false" SubHeaderCssClass="SubHeader"
							AllowEditing="false" AutoLoad="false">
						</tstsc:SortedGrid>
					</asp:Panel>
                </div>
            </div>
        </div>
	</div>
	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
		<Services>
			<asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/RequirementsTaskService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/UserService.svc" />
			<asp:ServiceReference Path="~/Services/Ajax/HistoryActivityService.svc" />
		</Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" /> 
        </Scripts>
	</tstsc:ScriptManagerProxyEx>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="cplScripts">
	<script type="text/javascript">
		var resx = Inflectra.SpiraTest.Web.GlobalResources;

        SpiraContext.pageId = "Inflectra.Spira.Web.ResourceDetails";
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
    </script>
</asp:Content>
