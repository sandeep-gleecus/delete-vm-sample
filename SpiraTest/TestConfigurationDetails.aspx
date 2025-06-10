<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="TestConfigurationDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.TestConfigurationDetails" %>
<%@ Register TagPrefix="tstuc" TagName="TestSetListPanel" Src="UserControls/TestSetListPanel.ascx" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:NavigationBar 
                AutoLoad="true"
                EnableLiveLoading="true" 
                BodyHeight="580px" 
                ErrorMessageControlId="lblMessage"
                FormManagerControlId="ajxFormManager"
                ID="navTestConfigurationSets" 
                IncludeAssigned="false"
                ItemImage="Images/artifact-Configuration.svg" 
                ListScreenCaption="<%$Resources:Main,TestConfigurationDetails_BackToList%>"
                runat="server" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationSetService" 
                />
        </div>


        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">
                <div class="clearfix">
                    <div 
                        class="btn-group priority1" 
                        role="group"
                        >
						<tstsc:DropMenu 
                            Authorized_ArtifactType="TestSet" 
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
                                    Authorized_ArtifactType="TestSet" 
                                    Authorized_Permission="Modify" 
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
                                    GlyphIconCssClass="mr3 fas fa-save" 
                                    ID="DropMenuItem2" 
                                    Name="SaveAndClose" 
                                    runat="server" 
                                    Value="<%$Resources:Buttons,SaveAndClose %>" 
                                    />
								<tstsc:DropMenuItem 
                                    Authorized_ArtifactType="TestSet" 
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
                            Authorized_ArtifactType="TestSet" 
                            Authorized_Permission="Delete" 
                            Confirmation="True" 
                            ConfirmationMessage="<%$Resources:Messages,TestConfigurationDetails_DeleteConfirm %>"  
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
                                AlternateText="<%$Resources:Fields,TestConfigurationSet %>" 
                                CssClass="w5 h5"
                                ID="imgTestConfigurationSet" 
                                ImageUrl="Images/artifact-Configuration.svg" 
                                runat="server" 
                                />
                            <span class="pl4 silver nowrap">
                                <tstsc:LabelEx 
                                    CssClass="pointer dib orange-hover transition-all"
                                    title="<%$Resources:Buttons,CopyToClipboard %>"
                                    data-copytoclipboard="true"
                                    ID="lblTestConfigurationSetId" 
                                    runat="server" 
                                    />
                            </span>
                        </div>
                    </div>
                    <tstsc:MessageBox ID="MessageBox1" runat="server" SkinID="MessageBox" />
                </div>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
            </div>




            <div class="main-content">
                <tstsc:TabControl 
                    CssClass="TabControl2" 
                    DividerCssClass="Divider"
                    ID="tclTestConfigurationDetails" 
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
                                        <asp:Label 
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
                                            Authorized_ArtifactType="TestSet" 
                                            Authorized_Permission="Modify"
                                            ID="txtDescription" 
                                            runat="server"
                                            Height="100px"
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
                                id="pnlOverview_TestConfigurationEntries" 
                                >
                                <div 
                                    class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3 pointer"
                                    aria-expanded="true">
                                    <asp:Localize 
                                        runat="server" 
                                        Text="<%$Resources:ServerControls,TabControl_TestConfigurationEntries %>" />
                                    <span class="u-anim u-anim_open-close is-open fr mr3 mt2"></span>
                                </div>
                                <div class="u-box_item mb5" >
									<div class="TabControlHeader w-100">
                                        <div class="btn-group priority1">
											<tstsc:HyperLinkEx 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                ClientScriptMethod="displayPopulatePanel()" 
                                                ID="btnPopulate" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server" 
                                                SkinID="ButtonDefault"
                                                >
                                                <span class="fas fa-plus"></span>
                                                <asp:Localize 
                                                    runat="server" 
                                                    Text="<%$Resources:Buttons,Populate %>" 
                                                    />
											</tstsc:HyperLinkEx>
											<tstsc:HyperLinkEx 
                                                Authorized_ArtifactType="TestSet" 
                                                Authorized_Permission="Modify" 
                                                ClientScriptMethod="delete_items()" 
                                                ClientScriptServerControlId="grdTestConfigurations" 
                                                ID="lnkRemoveTestConfigurations" 
                                                NavigateUrl="javascript:void(0)" 
                                                runat="server"
                                                Confirmation="true"
                                                ConfirmationMessage="<%$Resources:Messages,TestConfigurationDetails_ConfirmRemoveConfiguration %>"  
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
                                                ClientScriptServerControlId="grdTestConfigurations" 
                                                ID="lnkRefreshTestConfigurations" 
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
                                        <div class="legend-group">
										</div>
									</div>
                                    <div id="populateTestConfigurationsPanel"></div>
                                    <tstsc:MessageBox 
                                        ID="lblTestConfigurationEntryMessages" 
                                        runat="server" 
                                        SkinID="MessageBox" 
                                        />
                                    <div class="bg-near-white-hover py2 px3 br2 transition-all">
                                        <span class="fas fa-info-circle"></span>
	                                    <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
                                        <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	                                    <asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
                                        <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	                                    <asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,TestConfigurationDetails_TestConfigurationEntries %>" />.
                                    </div>
									<tstsc:OrderedGrid 
                                        AllowInlineEditing="false" 
                                        Authorized_ArtifactType="TestSet" 
                                        Authorized_Permission="Modify" 
                                        AutoLoad="false"
                                        CssClass="DataGrid DataGrid-no-bands" 
                                        EditRowCssClass="Editing" 
                                        ErrorMessageControlId="lblTestConfigurationEntryMessages" 
                                        HeaderCssClass="SubHeader" 
                                        ID="grdTestConfigurations" 
                                        RowCssClass="Normal" 
                                        runat="server"
                                        SelectedRowCssClass="Highlighted" 
                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationService" 
                                        VisibleCountControlId="lblVisibleCount"
                                        TotalCountControlId="lblTotalCount"
                                        >
									</tstsc:OrderedGrid>
                                </div>
                            </div>
                        </div>
                    </section>
                </asp:Panel>






                <%-- PANEL START --%>
                <%-- REST --%>
                <asp:Panel 
                    CssClass="TabControlPanel"
                    Runat="server" 
                    ID="pnlTestSets" 
                    >
                    <tstuc:TestSetListPanel 
                        ID="tstTestSetListPanel" 
                        ArtifactTypeEnum="TestConfigurationSet"
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
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestConfigurationSetService" 
        DisplayPageName="true"
        WorkflowEnabled="false" 
        NameField="Name"
        >
        <ControlReferences>
        	<tstsc:AjaxFormControl ControlId="lblTestConfigurationSetId" DataField="TestConfigurationSetId" Direction="In" />
        	<tstsc:AjaxFormControl ControlId="txtName" DataField="Name" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="chkIsActive" DataField="IsActive" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblCreationDate" DataField="CreationDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastUpdateDate" DataField="LastUpdatedDate" Direction="In" PropertyName="tooltip" />
        </ControlReferences>
    	<SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
		</SaveButtons>
    </tstsc:AjaxFormManager>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
            <asp:ServiceReference Path="~/Services/Ajax/TestConfigurationSetService.svc" />
            <asp:ServiceReference Path="~/Services/Ajax/TestConfigurationService.svc" />            
        </Services>
        <Scripts>
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
            <asp:ScriptReference Path="~/TypeScript/TestConfigurationDetails.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="cplScripts" runat="server">
   <script type="text/javascript">
       SpiraContext.pageId = "Inflectra.Spira.Web.TestConfigurationDetails";
       SpiraContext.ArtifactId = <%=testConfigurationSetId%>;
       SpiraContext.ArtifactIdOnPageLoad = <%=testConfigurationSetId%>;
	   SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
	   SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
       SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestConfigurationSet%>;
       SpiraContext.EmailEnabled = false;
       SpiraContext.HasCollapsiblePanels = true;
       SpiraContext.Mode = 'update';

       //Server Control IDs
       var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
       var btnSave_id = '<%=btnSave.ClientID%>';
       var txtName_id = '<%=txtName.ClientID%>';
       var tabControl_id = '<%=this.tclTestConfigurationDetails.ClientID%>';
       var navigationBar_id = '<%=this.navTestConfigurationSets.ClientID%>';

       //TabControl Panel IDs
       var pnlTestSets_id = '<%=pnlTestSets.ClientID%>';

       //Base URLs
       var urlTemplate_artifactRedirectUrl = '<%=TestConfigurationSetRedirectUrl %>';
       var urlTemplate_artifactListUrl = '<%=Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(ReturnToListPageUrl) %>';
       var urlTemplate_screenshot = '';
       var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';

       //Page specific updating, called by DetailsPage.ts
       var testConfigurationDetails_testConfigurationSetId = -1;
       function updatePageContent()
       {
           //See if the artifact id has changed
           var grdTestConfigurations = $find('<%=this.grdTestConfigurations.ClientID%>');
           if (testConfigurationDetails_testConfigurationSetId != SpiraContext.ArtifactId)
           {
               var filters = {};
               filters[globalFunctions.keyPrefix + 'TestConfigurationSetId'] = globalFunctions.serializeValueInt(SpiraContext.ArtifactId);
               grdTestConfigurations.set_standardFilters(filters);
               grdTestConfigurations.load_data();
               testConfigurationDetails_testConfigurationSetId = SpiraContext.ArtifactId;
           }
       }

       //Displays the populate panel
       function displayPopulatePanel()
       {
           //Disable the add button to prevent double clicks on it
           $("#" + '<%=btnPopulate.ClientID%>').addClass('disabled');

           //populate general data into the testConfigurationDetails react object, so it is accessible by React on render
           var testConfigurationSetId = SpiraContext.ArtifactId;
           testConfigurationDetails.populateTestConfigurationsPanelId = 'populateTestConfigurationsPanel';
           testConfigurationDetails.grdTestConfigurationsId = '<%=grdTestConfigurations.ClientID%>';
           testConfigurationDetails.displayPopulatePanel(testConfigurationSetId, '<%=btnPopulate.ClientID%>');
       }
   </script>
</asp:Content>
