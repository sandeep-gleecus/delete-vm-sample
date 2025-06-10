<%@ Page 
    AutoEventWireup="True"
    CodeBehind="TestRunDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.TestRunDetails"
    Language="c#" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="IncidentListPanel" Src="UserControls/IncidentListPanel.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:NavigationBar 
                AutoLoad="true"
                BodyHeight="580px" 
                ErrorMessageControlId="lblMessage"
                ID="navTestRunList" 
                IncludeAssigned="false"
                ItemImage="Images/artifact-TestRun.svg" 
                ListScreenCaption="<%$Resources:Main,TestRunDetails_BackToList%>"
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestRunService" 
                EnableLiveLoading="true"
                FormManagerControlId="ajxFormManager"
                />
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                        <div class="clearfix">
                            <div class="btn-group priority1 hidden-md hidden-lg" role="group">
                                <tstsc:HyperLinkEx 
                                    ID="btnBack" 
                                    NavigateUrl="<%#ReturnToTestRunListUrl%>" 
                                    runat="server" 
                                    SkinID="ButtonDefault" 
                                    ToolTip="<%$Resources:Main,TestRunDetails_BackToList%>"
                                    >
                                    <span class="fas fa-arrow-left"></span>
                                </tstsc:HyperLinkEx>
                            </div>
                            <div class="btn-group priority2" role="group">
                                <tstsc:DropMenu 
                                    Authorized_ArtifactType="TestRun" 
                                    Authorized_Permission="Modify" 
                                    ClientScriptMethod="save_data(evt)"
                                    ClientScriptServerControlId="ajxFormManager" 
                                    Confirmation="false" 
                                    GlyphIconCssClass="mr3 fas fa-save" 
                                    ID="btnSave" 
                                    runat="server" 
                                    Text="<%$Resources:Buttons,Save %>"
                                    />
                                <tstsc:DropMenu 
                                    Authorized_ArtifactType="TestRun"
                                    Authorized_Permission="Delete" 
                                    Confirmation="True" 
                                    ConfirmationMessage="<%$Resources:Messages,TestRunDetails_DeleteConfirm %>"
                                    GlyphIconCssClass="mr3 fas fa-trash-alt"
                                    ID="btnDelete" 
                                    runat="server"
                                    ClientScriptMethod="delete_item()" 
                                    ClientScriptServerControlId="ajxFormManager" 
                                    Text="<%$Resources:Buttons,Delete %>"
                                    />
                            </div>
                            <div class="btn-group priority1" role="group">
                                <tstsc:DropMenu 
                                    ClientScriptMethod="load_data()" 
                                    ClientScriptServerControlId="ajxFormManager" 
                                    GlyphIconCssClass="mr3 fas fa-sync" 
                                    ID="btnRefresh" 
                                    runat="server" 
                                    Text="<%$Resources:Buttons,Refresh %>"
                                    />
                                <tstsc:DropMenu 
                                    Authorized_ArtifactType="TestRun"
                                    Authorized_Permission="Create" 
                                    ClientScriptMethod="page.execute_test_case()" 
                                    Confirmation="False" 
                                    GlyphIconCssClass="mr3 fas fa-play" 
                                    ID="btnExecuteTest" 
                                    runat="server" 
                                    Text="<%$Resources:Buttons,ReTest %>"
                                    />
                                <tstsc:DropMenu 
                                    ID="btnTools" 
                                    GlyphIconCssClass="mr3 fas fa-cog"
                                    MenuCssClass="DropMenu" 
                                    PostBackOnClick="false"
                                    runat="server" 
                                    Text="<%$Resources:Buttons,Tools %>" 
                                    >
                                    <DropMenuItems>
                                        <tstsc:DropMenuItem 
                                            Authorized_ArtifactType="TestRun" 
                                            Authorized_Permission="View" 
                                            ClientScriptMethod="print_item('html')" 
                                            GlyphIconCssClass="mr3 fas fa-print" 
                                            Name="Print" 
                                            Value="<%$Resources:Dialogs,Global_PrintItems %>" 
                                            />
                                        <tstsc:DropMenuItem Divider="true" />
                                        <tstsc:DropMenuItem 
                                            Authorized_ArtifactType="TestRun" 
                                            Authorized_Permission="View" 
                                            ClientScriptMethod="print_item('excel')" 
                                            ImageUrl="Images/Filetypes/Excel.svg" 
                                            Name="ExportToExcel" 
                                            Value="<%$Resources:Dialogs,Global_ExportToExcel %>" 
                                            />
                                        <tstsc:DropMenuItem 
                                            Authorized_ArtifactType="TestRun" 
                                            Authorized_Permission="View" 
                                            ClientScriptMethod="print_item('word')" 
                                            ImageUrl="Images/Filetypes/Word.svg" 
                                            Name="ExportToWord" 
                                            Value="<%$Resources:Dialogs,Global_ExportToWord %>" 
                                            />
                                        <tstsc:DropMenuItem 
                                            Authorized_ArtifactType="TestRun" 
                                            Authorized_Permission="View" 
                                            ClientScriptMethod="print_item('pdf')" 
                                            ImageUrl="Images/Filetypes/Acrobat.svg" 
                                            Name="ExportToPdf" 
                                            Value="<%$Resources:Dialogs,Global_ExportToPdf %>" 
                                            />
                                    </DropMenuItems>
                                </tstsc:DropMenu>
                            </div>

                            <div id="plcWorX" class="dn">
                                <div class="btn-group priority3 ml2" role="group">
                                    <tstsc:DropMenu ID="mnuWorX" runat="server" Text="WorX">
                                        <DropMenuItems>
                                            <tstsc:DropMenuItem Name="Open" Value="Open" NavigateUrl="javascript:void()" />
                                            <tstsc:DropMenuItem Name="Review" Value="Review" NavigateUrl="javascript:void()" />
                                        </DropMenuItems>
                                    </tstsc:DropMenu>
                                </div>
                            </div>
                        </div>
                        <div class="u-wrapper width_md sm-hide-isfixed xs-hide-isfixed xxs-hide-isfixed">
                            <h2>
                                <tstsc:LabelEx ID="lblTestRunName" runat="server" />
                            </h2>
                            <div class="pl3 px4 mb2 dib bg-near-white br2 ">
                                <div class="py2 pr4 dib v-mid dif items-center ma0-children fs-h4 silver">
                                    <tstsc:ImageEx 
                                        AlternateText="<%$Resources:Fields,TestRun %>" 
                                        CssClass="w4 h4"
                                        ID="imgTestRun" 
                                        ImageUrl="Images/artifact-TestRun.svg" 
                                        runat="server" 
                                        />
                                    <span class="pl4">
                                        <tstsc:LabelEx 
                                            CssClass="pointer dib orange-hover transition-all"
                                            title="<%$Resources:Buttons,CopyToClipboard %>"
                                            data-copytoclipboard="true"
                                            ID="lblTestRunId" 
                                            runat="server" 
                                            />
                                    </span>
                                </div>
                                <div class="py2 dib v-mid dif items-center ma0-children">
                                    <tstsc:LabelEx 
                                        AssociatedControlID="ajxExecutionStatus" 
                                        CssClass="pr3 silver"
                                        ID="lblExecutionStatusLegend" 
                                        runat="server" 
                                        Text="<%$Resources:Main,TestRunDetails_ExecutionStatus%>" 
                                        />
                                    <tstsc:StatusBox 
                                        ID="ajxExecutionStatus" 
                                        runat="server" 
                                        />
                                </div>
                            </div>
                            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                        </div>
                    </div>

            <div class="main-content">
                <div class="px4 pb3">
    				<tstsc:RichTextLabel 
                        CssClass="w-100 ov-y-auto light-silver"
                        ID="txtDescription" 
                        runat="server" 
                        MultiLine="true" 
                        MaxHeight="100px" 
                        />
                </div>
                <tstsc:TabControl 
                    CssClass="TabControl2" 
                    DividerCssClass="Divider"
                    ID="tclTestRunDetails" 
                    runat="server"
                    SelectedTabCssClass="TabSelected" 
                    TabCssClass="Tab" 
                    TabHeight="25"
                    TabWidth="100" 
                    >
                    <TabPages>
                        <tstsc:TabPage 
                            ID="tabOverview" 
                            Caption="<%$Resources:ServerControls,TabControl_Overview %>"
                            runat="server"
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_OVERVIEW %>"
                            TabPageControlId="pnlOverview" 
                            TabPageIcon="fas fa-home"
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
                            AjaxControlContainer="tstAssociationPanel"
                            AjaxServerControlId="grdAssociationLinks"
                            AuthorizedArtifactType="Task" 
                            Caption="<%$Resources:ServerControls,TabControl_Tasks %>" 
                            CheckPermissions="true"
                            ID="tabAssociations" 
                            runat="server"
                            TabName="<%$ GlobalFunctions:PARAMETER_TAB_ASSOCIATION %>"
                            TabPageControlId="pnlAssociations" 
                            TabPageImageUrl="Images/artifact-Task.svg"
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
                    </TabPages>
                </tstsc:TabControl>

                <asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlOverview" 
                    runat="server" 
                    >
                    <div class="u-wrapper width_md">
                                
                            



                        <%-- RELEASE FIELDS --%>
                        <%-- PEOPLE FIELDS --%>
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
                                            AssociatedControlID="ddlRelease" 
                                            ID="lblRelease" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_Release%>" 
                                            />
                                        <tstsc:UnityDropDownHierarchy 
                                            AutoPostBack="false" 
                                            DataTextField="FullName" 
                                            DataValueField="ReleaseId" 
                                            ID="ddlRelease" 
                                            NoValueItem="true" 
                                            CssClass="u-dropdown u-dropdown_hierarchy is-closed"
                                            NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true"
                                                AssociatedControlID="ddlBuild" 
                                                ID="ddlBuildLabel" 
                                                Required="false"
                                                runat="server" 
                                                Text="<%$Resources:Fields,BuildId%>" 
                                                />
                                        <tstsc:UnityDropDownListEx 
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
                                <ul 
                                    class="u-box_list"
                                    id="customFieldsUsers"
                                    runat="server"
                                    >
                                    <li class="ma0 pa0 mb2">
                                        <tstsc:LabelEx 
                                            AppendColon="true"
                                            AssociatedControlID="ddlOwner" 
                                            ID="lblOwner" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_TesterName%>" 
                                            />
                                        <tstsc:UnityDropDownUserList 
                                            CssClass="u-dropdown u-dropdown_user"
                                            DataTextField="FullName" 
                                            DataValueField="UserId" 
                                            DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                            ID="ddlOwner" 
                                            NoValueItem="false" 
                                            runat="server" 
                                            />
                                    </li>
                                </ul>
							</div>
                        </div>
                                


                        <%-- GENERAL FIELDS --%>
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
                                            AssociatedControlID="lnkTestSet" 
                                            ID="lblTestSet" 
                                            runat="server" 
                                            Required="false" 
                                            Text="<%$Resources:Main,TestRunDetails_TestSet%>" 
                                            />
                                        <tstsc:ArtifactHyperLink 
                                            AlternateText="<%$Resources:Fields,TestSet %>"
                                            CssClass="ArtifactHyperLink"
                                            DisplayChangeLink="true"
                                            ID="lnkTestSet" 
                                            ItemImage="Images/artifact-TestSet.svg" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AssociatedControlID="lnkTestCase" 
                                            ID="lblTestCase" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_TestCase%>" 
                                            />
                                        <tstsc:ArtifactHyperLink 
                                            AlternateText="<%$Resources:Fields,TestCase %>"
                                            CssClass="ArtifactHyperLink"
                                            DisplayChangeLink="false"
                                            ID="lnkTestCase" 
                                            ItemImage="Images/artifact-TestCase.svg" 
                                            runat="server" 
                                            />
									</li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AssociatedControlID="lblTestRunType" 
                                            ID="lblTestRunTypeLegend" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_TestRunType%>" />
                                        <asp:Label 
                                            ID="lblTestRunType" 
                                            runat="server"
                                            ></asp:Label>
                                    </li>
                                </ul>
                            </div>
                        </div>



                        <%-- SECTION START --%>
                        <%-- DATE AND TIME FIELDS --%>
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
                                            AssociatedControlID="lblExecutionDate" 
                                            CssClass="light-silver"
                                            ID="lblExecutionDateLegend" 
                                            Required="false" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_ExecutionDate%>" 
                                            />
                                        <asp:Label 
                                            ID="lblExecutionDate" 
                                            runat="server" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AssociatedControlID="txtEstimatedDuration" 
                                            ID="lblEstimatedDuration" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_EstimatedDurationWithHours%>" 
                                            />
                                        <tstsc:UnityTextBoxEx 
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtEstimatedDuration" 
                                            MaxLength="9" 
                                            runat="server" 
                                            type="text"
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AssociatedControlID="txtActualDuration" 
                                            ID="lblActualDuration" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Main,TestRunDetails_ActualDurationWithHours%>" 
                                            />
                                        <tstsc:UnityTextBoxEx
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            ID="txtActualDuration" 
                                            MaxLength="9" 
                                            runat="server" 
                                            type="text"
                                            />
                                    </li>
                                </ul>
							</div>
                        </div>
                                
                            

                        <%-- SECTION START --%>
                        <%-- RICH TEXT FIELDS --%>
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
                                    ></ul>
                            </div>
                        </div>
                    </div>                         



                    <div class="u-wrapper width_md">



                        <%-- SECTION START --%>
                        <%-- TEST STEPS LIST --%>
                        <div class="u-box_3 mb4">
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
                                    <asp:Panel runat="server" ID="pnlOverview_TestSteps" class="mx3">
                                        <tstsc:MessageBox runat="server" ID="divIncidentsMessage" />

                                        <div id="grdTestRunSteps" class="w-100 dt inner-table">
                                            <div class="dt-row bb b-orange fw-b">
                                                <div class="priority1 dtc pa1"><asp:Localize Text="<%$Resources:Fields,Step %>" runat="server"/></div>
                                                <div class="priority1 dtc pa1"><asp:Localize Text="<%$Resources:Fields,TestStepDescription %>" runat="server"/></div>
                                                <div class="priority2 dtc pa1"><asp:Localize Text="<%$Resources:Fields,ExpectedResult %>" runat="server"/></div>
                                                <div class="priority2 dtc pa1"><asp:Localize Text="<%$Resources:Fields,SampleData %>" runat="server" /></div>
                                                <div class="priority4 dtc pa1 ws-nowrap"><asp:Localize Text="<%$Resources:Main,TestRunDetails_TestCaseAndTestStep %>" runat="server" /></div>
                                                <div class="priority4 dtc pa1"><asp:Localize Text="<%$Resources:Fields,ActualResult %>" runat="server"/></div>
                                                <div class="priority1 dtc pa1"><asp:Localize Text="<%$Resources:Fields,ExecutionStatus %>" runat="server"/></div>
                                                <div class="priority2 dtc pa1"></div>
                                            </div>
                                            <!-- ko foreach: items -->
                                                <div class="dt-row bb b-vlight-gray">
                                                    <div class="dtc pa1 priority1" data-bind="text: Fields.Position.intValue"></div>
                                                    <div class="dtc pa1 priority1 mw9 relative o-1-child-hover js-fullscreen">
                                                        <div 
                                                            class="fullscreen-toggle pointer absolute pr2 pt2 right0 top0 z-2 o-1-on-parent-hover light-gray orange-hover transition-all fas fa-expand-arrows-alt"
                                                            onclick="toggleFullscreen(event);" 
                                                            role="button" 
                                                            >
                                                        </div>
                                                        <span data-bind="html: displayRichText(Fields.Description)" class="mw-100-children"></span>
                                                    </div>
                                                    <div class="dtc pa1 priority2 mw9 relative o-1-child-hover js-fullscreen">
                                                        <div 
                                                            class="fullscreen-toggle pointer absolute pr2 pt2 right0 top0 z-2 o-1-on-parent-hover light-gray orange-hover transition-all fas fa-expand-arrows-alt"
                                                            onclick="toggleFullscreen(event);" 
                                                            role="button" 
                                                            >
                                                        </div>
                                                        <span data-bind="html: displayRichText(Fields.ExpectedResult)" class="mw-100-children"></span>
                                                    </div>
                                                    <div class="dtc pa1 priority2 mw9 relative o-1-child-hover js-fullscreen">
                                                        <div 
                                                            class="fullscreen-toggle pointer absolute pr2 pt2 right0 top0 z-2 o-1-on-parent-hover light-gray orange-hover transition-all fas fa-expand-arrows-alt"
                                                            onclick="toggleFullscreen(event);" 
                                                            role="button" 
                                                            >
                                                        </div>
                                                        <span data-bind="html: displayRichText(Fields.SampleData)" class="mw-100-children"></span>
                                                    </div>
                                                    <div class="dtc pa1 ws-nowrap priority4">
                                                        <a data-bind="text: (globalFunctions.isNullOrUndefined(Fields.TestCaseId)) ? '' : Fields.TestCaseId.textValue, attr: { href: (globalFunctions.isNullOrUndefined(Fields.TestCaseId)) ? '' : Fields.TestCaseId.tooltip }"></a>
                                                        /
                                                        <a data-bind="text: (globalFunctions.isNullOrUndefined(Fields.TestStepId)) ? '' : Fields.TestStepId.textValue, attr: { href: (globalFunctions.isNullOrUndefined(Fields.TestStepId)) ? '' : Fields.TestStepId.tooltip }"></a>
                                                    </div>
                                                    <div class="priority4 dtc pa1 mw9 relative o-1-child-hover js-fullscreen">
                                                        <div 
                                                            class="fullscreen-toggle pointer absolute pr2 pt2 right0 top0 z-2 o-1-on-parent-hover light-gray orange-hover transition-all fas fa-expand-arrows-alt"
                                                            onclick="toggleFullscreen(event);" 
                                                            role="button" 
                                                            >
                                                        </div>
                                                        <span data-bind="html: displayRichText(Fields.ActualResult)" class="mw-100-children">
                                                        </span>
                                                        <tstsc:HyperLinkEx 
                                                            Authorized_ArtifactType="Incident" 
                                                            Authorized_Permission="View"
                                                            data-bind="style: { display: Fields.IncidentCount.intValue ? 'inline-block' : 'none' }, attr: { 'data-primarykey': primaryKey }, click: grdTestRunStepList_HyperLinkClick" 
                                                            ID="HyperLinkEx1" 
                                                            runat="server" 
                                                            SkinID="ButtonDefault"
                                                            >
                                                            <asp:Localize runat="server" Text="<%$Resources:Main,TestRunDetails_ViewIncidents %>" />
                                                            <span class="badge" data-bind="text: Fields.IncidentCount.intValue"></span>
                                                        </tstsc:HyperLinkEx>
                                                    </div>
                                                    <div data-bind="text: Fields.ExecutionStatusId.textValue, attr: { 'class': 'priority1 dtc pa1' }, css: Fields.ExecutionStatusId.cssClass ">
                                                    </div>
                                                    <div class="priority2 dtc pa1">
                                                        <tstsc:HyperLinkEx 
                                                            Authorized_ArtifactType="Incident" 
                                                            Authorized_Permission="Modify" 
                                                            data-bind="click: Function.createDelegate(page, page.display_link_incidents)" 
                                                            runat="server" 
                                                            CssClass="btn btn-default py1 px3"
                                                            ToolTip="<%$Resources:Main,TestRunDetails_LinkExisting %>" 
                                                            >
                                                            <span class="fas fa-link"></span>
                                                        </tstsc:HyperLinkEx>
                                                    </div>
                                                </div>
                                            <!-- /ko -->
                                        </div>

                                        <tstsc:DialogBoxPanel 
                                            CssClass="PopupPanel" 
                                            ErrorMessageControlId="lblMessage"
                                            ID="dlgTestRunStepIncidents" 
                                            Height=250px 
                                            Modal="false"
                                            runat="server" 
                                            Title="<%$Resources:Dialogs,TestRunDetails_IncidentList%>" 
                                            >
                                            <div 
                                                class="my3 ov-y-auto" 
                                                style="height: 200px; max-width: 850px; "
                                                >
                                                <table 
                                                    id="tblTestRunStepIncidents" 
                                                    class="DataGrid w-100" 
                                                    >
                                                    <thead>
                                                        <tr class="Header">
                                                            <th class="priority3">
                                                                <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Fields,ID %>" /></th>
                                                            <th class="priority1">
                                                                <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Fields,IncidentName %>" /></th>
                                                            <th class="priority1">
                                                                <asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Fields,IncidentTypeId %>" /></th>
                                                            <th class="priority2">
                                                                <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Fields,IncidentStatusId %>" /></th>
                                                            <th class="priority4">
                                                                <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Fields,PriorityId %>" /></th>
                                                            <th class="priority4">
                                                                <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Fields,SeverityId %>" /></th>
                                                            <th class="priority4">
                                                                <asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Fields,OwnerId %>" /></th>
                                                            <th class="priority2">
                                                                <asp:Localize ID="Localize14" runat="server" Text="<%$Resources:Fields,CreationDate %>" /></th>
                                                            <th class="priority4">
                                                                <asp:Localize ID="Localize15" runat="server" Text="<%$Resources:Fields,OpenerId %>" /></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody data-bind="foreach: items">
                                                        <tr class="Normal">
                                                            <td data-bind="text: Fields.IncidentId.textValue" class="priority3">
                                                            </td>
                                                            <td class="priority1">
                                                                <tstsc:ImageEx ID="ImageEx1" runat="server" CssClass="w4 h4" ImageUrl="Images/artifact-Incident.svg" AlternateText="Incident" />
                                                                <a data-bind="text: Fields.Name.textValue, attr: { href: urlTemplate_incidentBaseUrl.replace(globalFunctions.artifactIdToken, primaryKey()) }, event: { mouseover: tblTestRunStepIncidents_mouseOver, mouseout: tblTestRunStepIncidents_mouseOut }">
                                                                </a>
                                                            </td>
                                                            <td data-bind="text: Fields.IncidentTypeId.textValue" class="priority1">
                                                            </td>
                                                            <td data-bind="text: Fields.IncidentStatusId.textValue" class="priority2">
                                                            </td>
                                                            <td class="priority4" data-bind="text: (Fields.PriorityId && Fields.PriorityId.textValue) ? Fields.PriorityId.textValue : '-', style: {backgroundColor: (Fields.PriorityId && Fields.PriorityId.cssClass) ? '#' + Fields.PriorityId.cssClass() : '', whiteSpace: 'nowrap'}">
                                                            </td>
                                                            <td class="priority4" data-bind="text: (Fields.SeverityId && Fields.SeverityId.textValue) ? Fields.SeverityId.textValue : '-', style: {backgroundColor: (Fields.SeverityId && Fields.SeverityId.cssClass) ? '#' + Fields.SeverityId.cssClass() : '', whiteSpace: 'nowrap'}" >
                                                            </td>
                                                            <td data-bind="text: (Fields.OwnerId && Fields.OwnerId.textValue) ? Fields.OwnerId.textValue : '-'" class="priority4">
                                                            </td>
                                                            <td data-bind="text: Fields.CreationDate.textValue" class="priority2">
                                                            </td>
                                                            <td data-bind="text: Fields.OpenerId.textValue" class="priority4">
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </tstsc:DialogBoxPanel>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>

                            



                        <%-- SECTION START --%>
                        <%-- CONSOLE OUTPUT FOR AUTOMATED RUNS --%>
                        <div class="u-box_3">
                            <div 
                                class="u-box_group"
                                data-collapsible="true"
                                id="pnlOverview_Console"
                                runat="server" 
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_Console %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <div class="u-box_1">
                                    <ul class="u-box_list">
                                        <li class="ma0 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="lblRunnerName" 
                                                ID="lblRunnerNameLabel" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,RunnerName %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblRunnerName" 
                                                runat="server" 
                                                />
                                        </li>
                                        <li class="ma0 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="lblRunnerTestName" 
                                                ID="lblRunnerTestNameLabel" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,RunnerTestName %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblRunnerTestName" 
                                                runat="server" 
                                                />
                                        </li>
                                                
                                    </ul>
                                </div>
                                <div class="u-box_1">
                                    <ul class="u-box_list">
                                        <li class="ma0 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="lblAssertCount" 
                                                ID="lblAssertCountLabel" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,AssertCount %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblAssertCount" 
                                                runat="server" 
                                                />
                                        </li>
                                        <li class="ma0 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="lnkAutomationHost" 
                                                ID="lblAutomationHost" 
                                                Required="false" 
                                                runat="server" 
                                                Text="<%$Resources:Main,TestRunDetails_AutomationHost%>" 
                                                />
                                            <tstsc:ArtifactHyperLink 
                                                AlternateText="<%$Resources:Fields,AutomationHost %>"
                                                CssClass="ArtifactHyperLink"
                                                DisplayChangeLink="false"
                                                ID="lnkAutomationHost" 
                                                ItemImage="Images/artifact-AutomationHost.svg" 
                                                runat="server" 
                                                />
                                        </li>
                                    </ul>
                                </div>
                                <div class="u-box_3">
                                    <ul class="u-box_list">
                                        <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="lblRunnerMessage" 
                                                ID="lblRunnerMessageLabel" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,RunnerMessage %>" 
                                                />
                                            <tstsc:LabelEx 
                                                ID="lblRunnerMessage" 
                                                runat="server" 
                                                />
                                        </li>
                                        <li class="ma0 mb2 pa0">
                                            <tstsc:LabelEx 
                                                AppendColon="true" 
                                                AssociatedControlID="divStackTrace" 
                                                ID="divStackTraceLabel" 
                                                runat="server" 
                                                Text="<%$Resources:Fields,RunnerDetails %>" 
                                                />
                                            <div 
                                                class="DisplayBox dib u-box_list_control h9 ov-y-auto resize-v ov-x-hidden wb-word" 
                                                id="divStackTrace" 
                                                runat="server">
                                                <tstsc:RichTextLabel ID="lblStackTrace" runat="server" />
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>



					<tstsc:DialogBoxPanel 
                        AjaxServerControlId="ajxIncidentsSelector" 
                        CssClass="PopupPanel u-wrapper" 
                        ID="pnlAddAssociation" 
                        Height="400px" 
                        Modal="true"
                        runat="server" 
                        Title="<%$Resources:Dialogs,TestStepDetails_LinkExistingIncident %>" 
                        Width="550px" 
                        >
                        <div class="alert alert-warning alert-narrow">
                            <span class="fas fa-info-circle"></span>
							<tstsc:LabelEx 
                                ID="LabelEx2" 
                                runat="server" 
                                Text="<%$Resources:Dialogs,TestRunDetails_LinkExistingIncidentIntro %>" 
                                />
                        </div>
                        <div class="u-box_3 mw-100">
                            <ul class="u-box_list">
                                <li class="ma0 pa0">
									<tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="txtArtifactId" 
                                        ID="txtArtifactIdLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Dialogs,TestStepDetails_EnterId %>" 
                                        />
                                    <span>
										<span id="spnArtifactPrefix" class="ml2">
											<%=GlobalFunctions.ARTIFACT_PREFIX_INCIDENT %></span>&nbsp;
                                        <asp:TextBox 
                                            Columns="3" 
                                            CssClass="u-input is-active" 
                                            ID="txtArtifactId" 
                                            Placeholder="42"
                                            runat="server" 
                                            TextMode="SingleLine" 
                                            Width="80px" 
                                            />
                                    </span>
                                </li>
                                <li class="mx0 mt0 mb2 pa0 fw-b">
                                    <span class="flourish_before flourish_after">
                                        <asp:Localize
                                            id="orlabel"
                                            runat="server"
                                            Text="<%$ Resources:Main,Global_Or %>"
                                            />
                                    </span>
                                </li>
                                <li class="ma0 pa0">
									<tstsc:LabelEx 
                                        ID="lblChooseFromList" 
                                        runat="server" 
                                        Text="<%$Resources:Dialogs,ArtifactLinkPanel_ChooseFromList %>" 
                                        />
                                </li>
                                <li class="ma0 pa0">
									<div class="scrollbox resize-v h7 mb3 ov-y-auto ov-x-hidden dib">
                                        <tstsc:ItemSelector 
                                            AutoLoad="false" 
                                            CssClass="HierarchicalSelector db" 
                                            ErrorMessageControlId="divIncidentsMessage"
                                            ID="ajxIncidentsSelector" 
                                            ItemImage="Images/artifact-Incident.svg" 
                                            MultipleSelect="true" 
                                            runat="server" 
                                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" 
                                            Width="480px" 
                                            />
                                    </div>
								</li>
                                <li class="ma0 pa0">
									<tstsc:LabelEx 
                                        AppendColon="true"
                                        AssociatedControlID="txtComment" 
                                        ID="txtCommentLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Comment %>" 
                                        />
									<tstsc:UnityTextBoxEx 
                                        Columns="60" 
                                        CssClass="u-input is-active resize-v ml2" 
                                        DynamicHeight="false" 
                                        ID="txtComment" 
                                        MaxLength="255" 
                                        Rows="1" 
                                        runat="server" 
                                        TextMode="MultiLine" 
                                        />
								</li>
							</ul>
                        </div>
						<div class="btn-group ml3">
							<tstsc:HyperLinkEx 
                                ID="lnkAddAssociation" 
                                NavigateUrl="javascript:void(0)" 
                                runat="server" 
                                SkinID="ButtonPrimary" 
                                Text="<%$Resources:Buttons,Link %>" 
                                />
                            <tstsc:HyperLinkEx 
                                ClientScriptMethod="close()" 
                                ClientScriptServerControlId="pnlAddAssociation" 
                                ID="lnkCancel" 
                                NavigateUrl="javascript:void(0)" 
                                runat="server" 
                                SkinID="ButtonDefault" 
                                Text="<%$Resources:Buttons,Cancel %>" 
                                />
						</div>
					</tstsc:DialogBoxPanel>

                    <tstsc:DialogBoxPanel 
                        AjaxServerControlId="ajxTestSetSelector"
                        CssClass="PopupPanel u-wrapper" 
                        ID="pnlChangeTestSet" 
                        Modal="true"
                        runat="server" 
                        Title="<%$Resources:Main,TestRunDetails_ChangeTestSet %>" 
                        Width="510px" 
                        >
                        <tstsc:MessageBox 
                            ID="msgChangeTestSetMessages" 
                            runat="server" 
                            SkinID="MessageBox" 
                            />
                        <p class="alert alert-warning alert-narrow">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize 
                                runat="server" 
                                Text="<%$Resources:Main,TestRunDetails_ChangeTestSet_Intro %>" 
                                />
                        </p>
                        <div class="u-box_3 mw-100">
                            <ul class="u-box_list">
                                <li class="ma0 pa0">
                                    <tstsc:LabelEx 
                                        AppendColon="true" 
                                        AssociatedControlID="ddlTestSetFolders" 
                                        Font-Bold="true" 
                                        ID="ddlTestSetFoldersLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,Folder %>" 
                                        />
                                    <tstsc:UnityDropDownHierarchy 
                                        ClientScriptMethod="page.ddlTestSetFolders_changed" 
                                        DataTextField="Name" 
                                        DataValueField="TestSetFolderId" 
                                        ID="ddlTestSetFolders" 
                                        SkinID="UnityDropDownListAttachments"
                                        NoValueItem="true" 
                                        NoValueItemText="<%$Resources:Dialogs,Global_RootFolderDropDown %>" 
                                        runat="server" 
                                        />
                                </li>
                                <li class="ma0 pa0">
                                    <div class="scrollbox resize-v h7 mb3 ov-y-auto ov-x-hidden">
                                        <tstsc:ItemSelector 
                                            AutoLoad="false" 
                                            CssClass="HierarchicalSelector"
                                            ErrorMessageControlId="msgChangeTestSetMessages" 
                                            ID="ajxTestSetSelector" 
                                            ItemImage="Images/artifact-TestSet.svg"
                                            MultipleSelect="false"                                    
                                            runat="server" 
                                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestSetService" 
                                            Width="460px" 
                                            />
                                    </div>
                                </li>
                            </ul>
                            <div class="ml3 mt4 btn-group">
                                <tstsc:ButtonEx 
                                    ClientScriptMethod="page.pnlChangeTestSet_updateItem()" 
                                    ID="btnUpdate" 
                                    runat="server"
                                    SkinID="ButtonPrimary" 
                                    Text="<%$Resources:Buttons,Update %>" 
                                    />
                                <tstsc:ButtonEx 
                                    ClientScriptServerControlId="pnlChangeTestSet" ClientScriptMethod="close()" 
                                    ID="btnCancel" 
                                    runat="server"
                                    Text="<%$Resources:Buttons,Cancel %>" 
                                    />
                            </div>
                        </div>
                    </tstsc:DialogBoxPanel>

                </asp:Panel>


                                                        
                            


                <%-- OTHER PANELS START --%>
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



                <asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlIncidents" 
                    runat="server" 
                    Width="100%" 
                    >
					<tstuc:IncidentListPanel 
                        ID="tstIncidentListPanel" 
                        runat="server" 
                        />
                </asp:Panel>

                <asp:Panel ID="pnlAssociations" runat="server" Width="100%">
                    <tstuc:AssociationsPanel ID="tstAssociationPanel" runat="server" />
                </asp:Panel>


            </div>
        </div>
    </div>



    <tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,TestRun%>" 
        CheckUnsaved="true"
        ErrorMessageControlId="lblMessage" 
        ID="ajxFormManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestRunService"
        WorkflowEnabled="false"
        DisplayPageName="true" 
        >
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblTestRunId" DataField="TestRunId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblTestRunName" DataField="Name" Direction="In" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="In" />
            <tstsc:AjaxFormControl ControlId="ddlRelease" DataField="ReleaseId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtEstimatedDuration" DataField="EstimatedDuration" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="TesterId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtActualDuration" DataField="ActualDuration" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlBuild" DataField="BuildId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblExecutionDate" DataField="EndDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblTestRunType" DataField="TestRunTypeId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="ajxExecutionStatus" DataField="ExecutionStatusId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lnkTestSet" DataField="TestSetId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lnkTestCase" DataField="TestCaseId" Direction="In" />

            <tstsc:AjaxFormControl ControlId="lblRunnerName" DataField="RunnerName" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblAssertCount" DataField="RunnerAssertCount" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblRunnerMessage" DataField="RunnerMessage" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblRunnerTestName" DataField="RunnerTestName" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lnkAutomationHost" DataField="AutomationHostId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblStackTrace" DataField="RunnerStackTrace" Direction="In" />

        </ControlReferences>
        <SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
        </SaveButtons>
    </tstsc:AjaxFormManager>

    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/TestRunService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestRunStepService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestSetService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/BackgroundProcessService.svc" />
        </Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/TestRunDetails.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>

    <tstsc:BackgroundProcessManager ID="ajxBackgroundProcessManager" runat="server" ErrorMessageControlId="lblMessage"
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BackgroundProcessService" />
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">       
        /* The Page Class */
        var resx = Inflectra.SpiraTest.Web.GlobalResources;

        SpiraContext.pageId = "Inflectra.Spira.Web.TestRunDetails";
        SpiraContext.ArtifactId = <%=testRunId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=testRunId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
        SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestRun%>;
        SpiraContext.EmailEnabled = false;
        SpiraContext.HasCollapsiblePanels = true;
        SpiraContext.Mode = 'update';

        //Server Control IDs
        var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
        var btnSave_id = '<%=btnSave.ClientID%>';
        var txtName_id = '<%=lblTestRunName.ClientID%>';
        var tabControl_id = '<%=this.tclTestRunDetails.ClientID%>';
        var navigationBar_id = '<%=this.navTestRunList.ClientID%>';
        var ajxBackgroundProcessManager_id = '<%=this.ajxBackgroundProcessManager.ClientID%>';

        //Page Specific IDs
        var msgChangeTestSetMessages_id = '<%=msgChangeTestSetMessages.ClientID%>';
        var tclTestRunDetails_id = '<%=tclTestRunDetails.ClientID%>';
        var pnlOverview_id = '<%=pnlOverview.ClientID%>';
        var lblMessage_id = '<%=lblMessage.ClientID%>';
        var dlgTestRunStepIncidents_id = '<%=dlgTestRunStepIncidents.ClientID%>';
        var ddlBuild_id = '<%=ddlBuild.ClientID%>';
        var pnlChangeTestSet_id = '<%=pnlChangeTestSet.ClientID%>';
        var ddlTestSetFolders_id = '<%=ddlTestSetFolders.ClientID%>';
        var ajxTestSetSelector_id = '<%=ajxTestSetSelector.ClientID%>';
        var lnkTestSet_id = '<%=lnkTestSet.ClientID%>';
        var pnlChangeTestSet_id = '<%=pnlChangeTestSet.ClientID%>';
        var formGroup_testSteps_id = '<%=formGroup_testSteps.ClientID%>';
        var pnlAddAssociation_id = '<%=pnlAddAssociation.ClientID%>';

        //TabControl Panel IDs
        var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
        var pnlHistory_id = '<%=pnlHistory.ClientID%>';
        var pnlIncidents_id = '<%=pnlIncidents.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';

        //URL Templates
        var urlTemplate_artifactRedirectUrl = '<%=TestRunRedirectUrl %>';
        var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToTestRunListUrl) %>';
        var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
        var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(UrlRewriterModule.ResolveUrl(UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';
        var urlTemplate_incidentBaseUrl = '<%=incidentBaseUrl%>';
        var urlTemplate_testRunsPending = '<%=TestRunsPendingUrl%>';
        var urlTemplate_testRunsPendingExploratory= '<%=TestRunsPendingExploratoryUrl%>';

        var page = $create(Inflectra.SpiraTest.Web.TestRunDetails);



        //Handles fullscreen of test step cells
        function toggleFullscreen (e) {
            console.log(e.currentTarget);
            $(e.currentTarget).toggleClass('fa-arrows-alt right0').toggleClass('fa-times fa-2x right1');
            $(e.currentTarget).closest('.js-fullscreen').toggleClass('mw9 relative').toggleClass('fixed left0 top0 vw-100 vh-100 bg-white pa4 z-9999 ov-y-auto')
        };



        //Prints the current items
        function print_item(format)
        {
            var artifactId = SpiraContext.ArtifactId;

            //Get the report type from the format
            var reportToken;
            var filter;
            if (format == 'excel')
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestRunSummary%>";
                filter = "&af_10_100=";
            }
            else
            {
                reportToken = "<%=(string)Inflectra.SpiraTest.DataModel.Report.StandardReports.TestRunDetailed%>";
                filter = "&af_11_100=";
            }

            //Open the report for the specified format
            globalFunctions.launchStandardReport(reportToken, format, filter, artifactId);
        }

        //Updates any page specific content
        var testRunDetails_testRunId = -1;
        function updatePageContent()
        {
            //Hides the description if empty
            var description = $('#<%=txtDescription.ClientID%>').html();
            if (!description || description.length < 1)
            {
                var description = $('#<%=txtDescription.ClientID%>').css('display', 'none');
            }

            //See if the artifact id has changed and reload the steps
            if (testRunDetails_testRunId != SpiraContext.ArtifactId)
            {
                //Load the test run steps when the DOM is ready
                page.loadTestRunSteps();

                //Show or hide the automation section
                var ajxFormManager = $find(ajxFormManager_id);
                var dataItem = ajxFormManager.get_dataItem();
                if (dataItem.Fields.TestRunTypeId && dataItem.Fields.TestRunTypeId.intValue)
                {
                    if (dataItem.Fields.TestRunTypeId.intValue == globalFunctions.testRunTypeEnum.automated)
                    {
                        $('#<%=pnlOverview_Console.ClientID%>').show();
                    }
                    else
                    {
                        $('#<%=pnlOverview_Console.ClientID%>').hide();
                    }
                }

                //Update the test case, set and test case in set ids
                page.set_testCaseId(dataItem.Fields.TestCaseId.intValue);
                if (dataItem.Fields.TestSetId && dataItem.Fields.TestSetId.intValue)
                {
                    page.set_testSetId(dataItem.Fields.TestSetId.intValue);
                }
                else
                {
                    page.set_testSetId(null);
                }
                if (dataItem.Fields.TestSetTestCaseId && dataItem.Fields.TestSetTestCaseId.intValue)
                {
                    page.set_testSetTestCaseId(dataItem.Fields.TestSetTestCaseId.intValue);
                }
                else
                {
                    page.set_testSetTestCaseId(null);
                }

                //Update WorX URLs
                if ('<%=Feature_Local_Worx%>' == "true")
                {
                    //Set the dropdown URLs
                    var mnuWorX = $find('<%=mnuWorX.ClientID%>');
                    if (mnuWorX)
                    {
                        mnuWorX.get_items()[0].set_navigateUrl("worx:spira/open/pr" + SpiraContext.ProjectId + "/tr" + SpiraContext.ArtifactId);
                        mnuWorX.get_items()[1].set_navigateUrl("worx:spira/review/pr" + SpiraContext.ProjectId + "/tr" + SpiraContext.ArtifactId);
                        mnuWorX.refreshItems();
                    }
                }

            }
        }

        function grdTestRunStepList_HyperLinkClick (testRunStep, evt)
        {
            page.display_incidents(testRunStep, evt);
        }

        function tblTestRunStepIncidents_mouseOver(incident, evt)
        {
            page.displayIncidentTooltip(incident.primaryKey());
        }
        function tblTestRunStepIncidents_mouseOut(incident, evt)
        {
            page.hideIncidentTooltip();
        }

        function pnlAddAssociation_displayed()
        {
            //Clear the existing fields
            $get('<%=this.txtArtifactId.ClientID%>').value = '';
            $get('<%=this.txtComment.ClientID%>').value = '';
        }
        function pnlAddAssociation_txtArtifactId_OnKeyDown(evt)
        {
            //If enter was clicked, same as clicking Add button
            var keynum = evt.keyCode | evt.which;
            if (keynum == 13)
            {
                pnlAddAssociation_lnkAddAssociation_OnClick(evt);
                // stop the event bubble
                evt.preventDefault();
                evt.stopPropagation();
            }
        }
        function pnlAddAssociation_lnkAddAssociation_OnClick(evt)
        {
            var ajxIncidentsSelector = $find('<%=this.ajxIncidentsSelector.ClientID%>');
            var projectId = <%=ProjectId %>;
            var testRunStepId = page.get_testRunStepId();

            //Get the artifact id
            var artifactId = globalFunctions.trim($get('<%=this.txtArtifactId.ClientID%>').value);
            var incidentIds = new Array();

            //See if we have an individual item selected
            if (artifactId == '')
            {
                incidentIds = ajxIncidentsSelector.get_selectedItems();
            }
            else
            {
                if (!globalFunctions.isInteger(artifactId))
                {
                    alert(resx.ArtifactLinkPanel_IncidentIdNotValid);
                    return;
                }
                incidentIds.push(artifactId);
            }

            var comment = $get('<%=this.txtComment.ClientID%>').value;
            //Validate
            if (incidentIds.length < 1)
            {
                alert(resx.ArtifactLinkPanel_NeedToSelectOneArtifact);
                return;
            }

            //Add the association
            Inflectra.SpiraTest.Web.Services.Ajax.TestRunStepService.TestRunStep_AddIncidentAssociation(projectId, testRunStepId, incidentIds, comment, pnlAddAssociation_lnkAddAssociation_success, pnlAddAssociation_lnkAddAssociation_failure);
        }
        function pnlAddAssociation_lnkAddAssociation_success()
        {
            //Close the dialog
            var pnlAddAssociation = $find('<%=this.pnlAddAssociation.ClientID%>');
            pnlAddAssociation.close();

            //Reload the test run steps
            page.loadTestRunSteps();
        }
        function pnlAddAssociation_lnkAddAssociation_failure(ex)
        {
            //Close the dialog
            var pnlAddAssociation = $find('<%=this.pnlAddAssociation.ClientID%>');
            pnlAddAssociation.close();

            //Display the error message
            var divIncidentsMessage = $get('<%=divIncidentsMessage.ClientID %>');
            globalFunctions.display_error(divIncidentsMessage, ex);
        }

        //Attach event handlers
        $(document).ready(function(){
			$('#<%=this.lnkAddAssociation.ClientID%>').on("click", function(evt) {
                pnlAddAssociation_lnkAddAssociation_OnClick(evt);
            });
			$('#<%=this.lnkAddAssociation.ClientID%>').on("keydown", function(evt) {
                pnlAddAssociation_txtArtifactId_OnKeyDown(evt);
            });

            //Display WorX
            if ('<%=Feature_Local_Worx%>' == "true")
            {
                 document.getElementById('plcWorX').classList.remove("dn");
            }
        });

        function displayRichText(field)
        {
            if (field && field.textValue)
            {
                //this code deals with [IN:5960] where users with bad formatted and imported data see html and body tags in their test execution
                //previously we just applied filterXSS to the string in question, so now, before that, we remove html tags that bother customers
                var cleanedString = field.textValue();
                var regexStart = /^\<html\>[^a-zA-Z0-9]*\<body\>/;
                var regexEnd = /\<\/body\>[^a-zA-Z0-9]*\<\/html\>$/;
                cleanedString = cleanedString.replace(regexStart, "").replace(regexEnd, "");
                return filterXSS(cleanedString, filterXssInlineStyleOptions);
            }
            return '';
        }

    </script>
</asp:Content>
