<%@ Page
    Language="c#" 
    CodeBehind="AutomationHostDetails.aspx.cs" 
    AutoEventWireup="True"
    Inherits="Inflectra.SpiraTest.Web.AutomationHostDetails" 
    MasterPageFile="~/MasterPages/Main.Master" 
%>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="TestRunListPanel" Src="UserControls/TestRunListPanel.ascx" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:NavigationBar 
                AutoLoad="true"
                EnableLiveLoading="true" 
                BodyHeight="580px" 
                ErrorMessageControlId="lblMessage"
                FormManagerControlId="ajxFormManager"
                ID="navHostList" 
                IncludeAssigned="false"
                ItemImage="Images/artifact-AutomationHost.svg" 
                ListScreenCaption="<%$Resources:Main,AutomationHostDetails_BackToList%>"
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.AutomationHostService" 
                />
        </div>



        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div 
                    class="btn-group priority1" 
                    role="group"
                    >
					<tstsc:DropMenu 
                        Authorized_ArtifactType="AutomationHost" 
                        Authorized_Permission="Modify" 
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
                                Authorized_ArtifactType="AutomationHost" 
                                Authorized_Permission="Modify" 
                                ClientScriptMethod="save_data(null); void(0);"
                                GlyphIconCssClass="mr3 fas fa-save" 
                                ID="DropMenuItem1" 
                                Name="Save" 
                                runat="server" 
                                Value="<%$Resources:Buttons,Save %>"  
                                />
							<tstsc:DropMenuItem 
                                Authorized_ArtifactType="AutomationHost" 
                                Authorized_Permission="Modify" 
                                ClientScriptMethod="save_data(null, 'close'); void(0);" 
                                GlyphIconCssClass="mr3 fas fa-save" 
                                ID="DropMenuItem2" 
                                Name="SaveAndClose" 
                                runat="server" 
                                Value="<%$Resources:Buttons,SaveAndClose %>" 
                                />
							<tstsc:DropMenuItem 
                                Authorized_ArtifactType="AutomationHost" 
                                Authorized_Permission="Create" 
                                ClientScriptMethod="save_data(null, 'new'); void(0);" 
                                GlyphIconCssClass="mr3 fas fa-plus" 
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
                        GlyphIconCssClass="mr3 fas fa-sync" 
                        ID="btnRefresh" 
                        runat="server" 
                        Text="<%$Resources:Buttons,Refresh %>" 
                        />
                </div>
				<div 
                    class="btn-group priority2" 
                    role="group"
                    >
					<tstsc:DropMenu 
                        Authorized_ArtifactType="AutomationHost" 
                        Authorized_Permission="Delete" 
                        Confirmation="True" 
                        ConfirmationMessage="<%$Resources:Messages,AutomationHostDetails_DeleteConfirm %>"  
                        GlyphIconCssClass="mr3 fas fa-trash-alt" 
                        ID="btnDelete" 
                        runat="server" 
                        ClientScriptMethod="delete_item()" 
                        ClientScriptServerControlId="ajxFormManager" 
                        Text="<%$Resources:Buttons,Delete %>" 
                        />
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
                <div class="py2 px3 mb2 bg-near-white br2 dif items-center flex-wrap">
                    <div class="py1 pr4 dif items-center ma0-children fs-h4 fs-h6-xs">
                        <tstsc:ImageEx 
                            AlternateText="<%$Resources:Fields,AutomationHost %>" 
                            CssClass="w5 h5"
                            ID="imgAutomationHost" 
                            ImageUrl="Images/artifact-AutomationHost.svg" 
                            runat="server" 
                            />
                        <span class="px4 silver nowrap">
                            <tstsc:LabelEx 
                                CssClass="pointer dib orange-hover transition-all"
                                title="<%$Resources:Buttons,CopyToClipboard %>"
                                data-copytoclipboard="true"
                                ID="lblAutomationHostId" 
                                runat="server" 
                                />
                        </span>
                    </div>
                </div>
                <tstsc:MessageBox ID="MessageBox1" runat="server" SkinID="MessageBox" />
            </div>
            <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />




            <div class="main-content">
                <tstsc:TabControl 
                    CssClass="TabControl2" 
                    DividerCssClass="Divider"
                    ID="tclHostDetails" 
                    runat="Server"
                    SelectedTabCssClass="TabSelected" 
                    TabCssClass="Tab" 
                    TabHeight="25"
                    TabWidth="140" 
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






                <%-- PANEL START --%>
                <%-- OVERVIEW --%>
                <asp:Panel 
                    CssClass="TabControlPanel"
                    ID="pnlOverview" 
                    Runat="server" 
                    >
                    <section class="u-wrapper width_md">






                        <%-- SECTION START --%>
                        <%-- PEOPLE FIELDS - CUSTOM ONLY --%>
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
                                </ul>
                            </div>
                        </div>






                        <%-- SECTION START --%>
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
                                            AppendColon="true" 
                                            AssociatedControlID="txtToken" 
                                            ID="txtTokenLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,Token %>" 
                                            />
										<tstsc:UnityTextBoxEx
                                            CssClass="u-input"
                                            DisabledCssClass="u-input disabled"
                                            id="txtToken" 
                                            MaxLength="20" 
                                            runat="server" 
                                            TextMode="SingleLine" 
                                            />
                                    </li>
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:LabelEx 
                                            AppendColon="true" 
                                            AssociatedControlID="chkIsActive" 
                                            ID="chkIsActiveLabel" 
                                            Required="true" 
                                            runat="server" 
                                            Text="<%$Resources:Fields,ActiveYn %>" 
                                            />
                                        <tstsc:CheckBoxYnEx 
                                            ID="chkIsActive"
                                            runat="server"
                                            />
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
                                <ul class="u-box_list labels_absolute" id="customFieldsRichText" runat="server">
                                    <li class="ma0 mb2 pa0">
                                        <tstsc:RichTextBoxJ 
                                            Authorized_ArtifactType="AutomationHost" 
                                            Authorized_Permission="Modify"
                                            ID="txtDescription" 
                                            runat="server"
                                            Screenshot_ArtifactType="AutomationHost" 
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
                    </section>
                </asp:Panel>






                <%-- PANEL START --%>
                <%-- REST --%>
                <asp:Panel 
                    CssClass="TabControlPanel"
                    Runat="server" 
                    ID="pnlTestRuns" 
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
            </div>
        </div>
    </div>

    <tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,AutomationHost%>" 
        CheckUnsaved="true"
        ErrorMessageControlId="lblMessage"
        ID="ajxFormManager" 
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.AutomationHostService" 
        DisplayPageName="true"
        WorkflowEnabled="false" 
        NameField="Name"
        >
        <ControlReferences>
        	<tstsc:AjaxFormControl ControlId="lblAutomationHostId" DataField="AutomationHostId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblAutomationHostName" DataField="Name" Direction="In" />
        	<tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtToken" DataField="Token" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="chkIsActive" DataField="IsActive" Direction="InOut" />

            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdateDate" Direction="In" PropertyName="tooltip" />
        </ControlReferences>
    	<SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
		</SaveButtons>
    </tstsc:AjaxFormManager>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/AutomationHostService.svc" />
        </Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
   <script type="text/javascript">
       SpiraContext.pageId = "Inflectra.Spira.Web.AutomationHostDetails";
       SpiraContext.ArtifactId = <%=automationHostId%>;
       SpiraContext.ArtifactIdOnPageLoad = <%=automationHostId%>;
	   SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
	   SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
       SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.AutomationHost%>;
       SpiraContext.EmailEnabled = false;
       SpiraContext.HasCollapsiblePanels = true;
       SpiraContext.Mode = 'update';

       //Server Control IDs
       var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
       var btnSave_id = '<%=btnSave.ClientID%>';
       var txtName_id = '<%=txtName.ClientID%>';
       var tabControl_id = '<%=this.tclHostDetails.ClientID%>';
       var navigationBar_id = '<%=this.navHostList.ClientID%>';

       //TabControl Panel IDs
       var pnlAttachments_id = '<%=pnlAttachments.ClientID%>';
       var pnlHistory_id = '<%=pnlHistory.ClientID%>';
       var pnlTestRuns_id = '<%=pnlTestRuns.ClientID%>';

       //Base URLs
       var urlTemplate_artifactRedirectUrl = '<%=AutomationHostRedirectUrl %>';
       var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToListPageUrl) %>';
       var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
       var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';
   </script>
</asp:Content>
