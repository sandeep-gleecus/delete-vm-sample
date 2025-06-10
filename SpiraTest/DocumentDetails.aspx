<%@ Page 
    AutoEventWireup="true"
    CodeBehind="DocumentDetails.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.DocumentDetails"
    Language="C#" 
    MasterPageFile="~/MasterPages/Main.Master" 
    Title="Untitled Page" 
%>


<%@ Import Namespace="Inflectra.SpiraTest.Business" %>
<%@ Import Namespace="Inflectra.SpiraTest.Common" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>


<%@ Register TagPrefix="tstuc" TagName="ArtifactAddFollower" Src="UserControls/ArtifactAddFollower.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="ArtifactEmail" Src="UserControls/ArtifactEmail.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AssociationsPanel" Src="UserControls/AssociationsPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="HistoryPanel" Src="~/UserControls/HistoryPanel.ascx" %>
<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
    <tstsc:ThemeStylePlaceHolder ID="themeStylePlaceHolder" runat="server" SkinID="PrismSyntaxHighlighterStylesheet" />
    <tstsc:ThemeStylePlaceHolder ID="diagramThemeStylePlaceholder" runat="server" SkinID="DhtmlxDiagramStylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="panel-container flex">
        <div class="side-panel dn-sm dn-xs sticky top-nav self-start">
            <tstsc:SidebarPanel ID="pnlFolders" runat="server" HeaderCaption="<%$Resources:Main,Global_Folders %>" MinWidth="100" MaxWidth="500" data-panel="folder-tree" BodyHeight="150"
                DisplayRefresh="true" ClientScriptServerControlId="trvFolders" ClientScriptMethod="load_data(true)" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.GlobalService">
                <div class="Widget panel" style="max-height:150px;">
                    <tstsc:TreeView ID="trvFolders" runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"
                        LoadingImageUrl="Images/action-Spinner.svg" CssClass="FolderTree" ErrorMessageControlId="lblMessage"
                        NodeLegendFormat="{0}" AllowEditing="false" ItemName="<%$Resources:Fields,Folder %>"
                        ClientScriptMethod="refresh_data();" ClientScriptServerControlId="navDocumentList" />
                </div>
            </tstsc:SidebarPanel>

            <tstsc:NavigationBar ID="navDocumentList" runat="server" AutoLoad="true"
                IncludeAssigned="false" SummaryItemImage="Images/FolderOpen.svg"
                BodyHeight="480px" ErrorMessageControlId="lblMessage"
                EnableLiveLoading="true"
                FormManagerControlId="ajxFormManager"
                ListScreenCaption="<%$Resources:Main,DocumentDetails_BackToList%>"
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService" />
         </div>



        <div class="main-panel pl4 grow-1">
            <div class="btn-toolbar toolbar sticky relative-sm top-nav top0-sm" role="toolbar">




                <%-- BUTTON TOOLBAR --%>
                <div class="clearfix">
                    <div class="btn-group priority1" role="group">
                        <tstsc:DropMenu ID="btnSave" GlyphIconCssClass="mr3 fas fa-save" runat="server" Text="<%$Resources:Buttons,Save %>" 
                            Authorized_ArtifactType="Document" Authorized_Permission="Modify"
                            ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="save_data(evt)" />
                        <tstsc:DropMenu ID="btnDelete" GlyphIconCssClass="mr3 fas fa-trash-alt" runat="server" Text="<%$Resources:Buttons,Delete %>"
                            Authorized_ArtifactType="Document" Authorized_Permission="Delete"
                            ConfirmationMessage="<%$Resources:Messages,DocumentDetails_DeleteConfirm %>"
                            ClientScriptMethod="delete_item()" 
                            ClientScriptServerControlId="ajxFormManager"         
                            CausesValidation="False" Confirmation="True" />
                    </div>
                    <div class="btn-group priority2" role="group">
                        <tstsc:DropMenu ID="btnRefresh" GlyphIconCssClass="mr3 fas fa-sync" runat="server" Text="<%$Resources:Buttons,Refresh %>"
                            ConfirmationMessage="<%$Resources:Messages,DocumentDetails_RefreshConfirm %>"
                            ClientScriptServerControlId="ajxFormManager" ClientScriptMethod="load_data()" />
                    </div>
                    <div class="btn-group priority3" role="group" id="pnlEmailToolButtons">
                        <tstsc:DropMenu ID="btnEmail" runat="server"
                            Text="<%$Resources:Buttons,Email %>"
                            GlyphIconCssClass="mr3 far fa-envelope"
                            ClientScriptMethod="ArtifactEmail_pnlSendEmail_display(evt)"
                            data-requires-email="true" 
                            Confirmation="false" />
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
                                    Authorized_ArtifactType="Document"
                                    Authorized_Permission="Modify"
                                    ClientScriptMethod="ArtifactAddFollower_pnlAddFollower_display()" 
                                    Name="AddFollower" 
                                    GlyphIconCssClass="mr3 fas fa-user" 
                                    Value="<%$Resources:Buttons,AddFollower %>" 
                                    runat="server"
                                    />
			                </DropMenuItems>
						</tstsc:DropMenu>
                    </div>
                </div>

                        
                <asp:Panel ID="pnlFolderPath" runat="server" CssClass="" />


                <%-- HEADER BAR --%>
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
                    <div class="py2 px3 mb2 bg-near-white br2 flex items-center flex-wrap">
                        <div class="py1 pr4 dif items-center ma0-children fs-h4 fs-h6-xs">
                            <tstsc:ImageEx 
                                CssClass="w5 h5"
                                ID="imgDocumentFileType" 
                                runat="server" 
                                />
                            <tstsc:HyperLinkEx 
                                CssClass="pl4"
                                ID="lnkDocumentName" 
                                runat="server" 
                                Target="_blank"
                                >
                                <span class="fas fa-download"></span>
                            </tstsc:HyperLinkEx> 
                            <span class="pl4 silver nowrap">
                                <tstsc:LabelEx 
                                    CssClass="pointer dib orange-hover transition-all"
                                    title="<%$Resources:Buttons,CopyToClipboard %>"
                                    data-copytoclipboard="true"
                                    ID="lblDocumentId" 
                                    runat="server" 
                                    />
                            </span>
                        </div>
                        <div class="py1 pr5 pr4-xs dif items-center ma0-children fs-h6">
    						<tstsc:LabelEx 
                                AppendColon="true"
                                AssociatedControlID="ddlDocType" 
                                ID="ddlDocTypeLabel" 
                                Required="true" 
                                runat="server" 
                                Text="<%$Resources:Fields,ProjectAttachmentTypeId %>" 
                                />
							<tstsc:UnityDropDownListEx 
                                CssClass="pl2 u-dropdown is-active" 
                                DataMember="DocumentType"  
                                DataTextField="Name" 
                                DataValueField="DocumentTypeId" 
                                DisabledCssClass="u-dropdown disabled" 
                                id="ddlDocType" 
                                NoValueItem="false" 
                                Runat="server" 
                                />
                        </div>

                        <div class="py1 dif items-center ma0-children fs-h6">
							<tstsc:LabelEx 
                                AppendColon="true" 
                                AssociatedControlID="ajxWorkflowOperations"
                                ID="lblDocumentStatus" 
                                Required="True" 
                                runat="server" 
                                Text="<%$Resources:Fields,DocumentStatusId %>" 
                                />
                            <div class="dib v-mid-children dif flex-wrap items-center pl3">
								<tstsc:LabelEx 
                                    ID="lblDocumentStatusValue" 
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
                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService" 
                                        />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
            </div>



            <%-- POPUPS --%>
            <tstuc:ArtifactEmail 
                ArtifactId="<%# this.attachmentId %>"
                ArtifactTypeEnum="Document" 
                ID="tstEmailPanel" 
                runat="server" 
                />
            <tstuc:ArtifactAddFollower 
				ArtifactTypeEnum="Incident" 
                ID="ArtifactAddFollower" 
                runat="server" 
                />
            <asp:ValidationSummary 
                CssClass="ValidationMessage" 
                DisplayMode="BulletList" 
                ID="ValidationSummary1" 
                runat="server" 
                ShowMessageBox="False" 
                ShowSummary="True"
                />




            <div class="main-content pb5">


                <%-- TABS --%>
                <tstsc:TabControl ID="tclDocumentDetails" CssClass="TabControl2" TabWidth="100" TabHeight="25"
                    TabCssClass="Tab" SelectedTabCssClass="TabSelected" DividerCssClass="Divider" 
                    runat="Server">
                    <TabPages>
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_View %>" 
                            ID="tabView" 
                            runat="server"
                            TabPageControlId="pnlPreview" 
                            TabPageIcon="far fa-eye"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_PREVIEW%>"
                            />
                        <tstsc:TabPage 
                            Caption="<%$Resources:Buttons,Edit %>" 
                            ID="tabEdit" 
                            runat="server"
                            TabPageControlId="pnlEdit" 
                            TabPageIcon="far fa-edit"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_EDIT%>"
                            />
                        <tstsc:TabPage 
                            Caption="<% $Resources:ServerControls,TabControl_Properties %>"
                            ID="tabOverview" 
                            runat="server" 
                            TabPageControlId="pnlOverview" 
                            TabPageIcon="fas fa-line-columns"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_OVERVIEW%>"
                            />
                        <tstsc:TabPage 
                            Caption="<%$Resources:ServerControls,TabControl_Versions %>" 
                            ID="tabVersions" 
                            runat="server"
                            TabPageControlId="pnlVersions" 
                            TabPageIcon="fas fa-bring-forward"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_VERSION%>"
                            />
                        <tstsc:TabPage 
                            AjaxControlContainer="tstAssociationPanel"
                            AjaxServerControlId="grdAssociationLinks" 
                            Caption="<%$Resources:ServerControls,TabControl_Associations %>"
                            ID="tabAssociations"
                            runat="server" 
                            TabPageControlId="pnlAssociations" 
                            TabPageIcon="fas fa-link"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_ASSOCIATION%>"
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
                            TabPageControlId="pnlHistory" 
                            TabPageIcon="fas fa-history"
                            TabName="<%$GlobalFunctions:PARAMETER_TAB_HISTORY%>"
                            />
                    </TabPages>
                </tstsc:TabControl>

                <%-- OVERVIEW TAB --%>
                <asp:Panel ID="pnlOverview" runat="server" CssClass="TabControlPanel">
                    <section class="u-wrapper width_md">
                    <%-- USER FIELDS --%>
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
                                        AssociatedControlID="ddlCreatedBy" 
                                        ID="ddlCreatedByLabel" 
                                        runat="server" 
                                        Required="true" 
                                        Text="<%$Resources:Fields,AuthorId %>" 
                                        />
                                    <tstsc:UnityDropDownUserList 
                                        CssClass="u-dropdown u-dropdown_user" 
                                        DataTextField="FullName" 
                                        DataValueField="UserId" 
                                        DisabledCssClass="u-dropdown u-dropdown_user disabled" 
                                        ID="ddlCreatedBy" 
                                        NoValueItem="True" 
                                        NoValueItemText="<%$Resources:Dialogs,Global_PleaseSelectDropDown %>"                                                 Runat="server" 
                                        />
                                </li>
                                <li class="ma0 mb2 pa0">
    								<tstsc:LabelEx 
                                        AppendColon="true"
                                        AssociatedControlID="ddlEditedBy" 
                                        ID="ddlEditedByLabel" 
                                        Required="true" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,EditorId %>" 
                                        />
                                    <tstsc:UnityDropDownUserList 
                                        CssClass="u-dropdown u-dropdown_user" 
                                        DataTextField="FullName" 
                                        DataValueField="UserId" 
                                        DisabledCssClass="u-dropdown u-dropdown_user disabled" 
                                        ID="ddlEditedBy" 
                                        NoValueItem="false" 
                                        Runat="server" 
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
                                    <label>
                                        <asp:Localize runat="server" Text="<%$Resources:Fields,FileType %>"/>
                                        (<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Size %>"/>):
                                    </label>
                                    <span>
									    <tstsc:LabelEx 
                                            ID="lblFileType" 
                                            runat="server" />
									    <tstsc:ImageEx 
                                            CssClass="w4 h4" 
                                            ID="imgFileType" 
                                            runat="server" 
                                            />
									    (<tstsc:LabelEx ID="lblSize" runat="server" />)
                                    </span>
                                </li>
                                <li class="ma0 mb2 pa0">
                                    <tstsc:LabelEx 
                                        AppendColon="true"
                                        AssociatedControlID="ddlDocFolder" 
                                        ID="ddlDocFolderLabel" 
                                        Required="true" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,DocumentFolder %>" 
                                        />
	                                <tstsc:UnityDropDownHierarchy 
                                        DataMember="ProjectAttachmentFolder" 
                                        DataTextField="Name" 
                                        DataValueField="ProjectAttachmentFolderId" 
                                        id="ddlDocFolder" 
	                                    IndentLevelField="IndentLevel" 
                                        ItemImage="Images/Folder.svg" 
                                        NoValueItem="false" 
                                        Runat="server"
                                        SkinId="UnityDropDownListAttachments"
                                        />
                                </li>
                                <li class="ma0 mb2 pa0">
                                    <tstsc:LabelEx 
                                        AppendColon="true"
                                        AssociatedControlID="lblCurrentVersion" 
                                        ID="lblCurrentVersionLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,CurrentVersion %>" 
                                        />
									<tstsc:LabelEx 
                                        ID="lblCurrentVersion" 
                                        runat="server" 
                                        />
                                </li>
                                <li class="ma0 mb2 pa0">
    								<tstsc:LabelEx 
                                        AppendColon="true"
                                        AssociatedControlID="txtTags" 
                                        ID="txtTagsLabel"
                                        runat="server" 
                                        Text="<%$Resources:Fields,Tags %>" 
                                        />
									<tstsc:UnityTextBoxEx 
                                        CssClass="u-input" 
                                        DisabledCssClass="u-input disabled" 
                                        id="txtTags" 
                                        MaxLength="255" 
                                        Runat="server" 
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
                                        AssociatedControlID="lblCreatedOn" 
                                        ID="lblCreatedOnLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,CreationDate %>" 
                                        />
									<tstsc:LabelEx 
                                        ID="lblCreatedOn" 
                                        runat="server" 
                                        />
                                </li>
                                <li class="ma0 mb2 pa0">
                                    <tstsc:LabelEx 
                                        AppendColon="true"
                                        AssociatedControlID="lblLastEdited" 
                                        ID="lblLastEditedLabel" 
                                        runat="server" 
                                        Text="<%$Resources:Fields,LastEdited %>" 
                                        />
									<tstsc:LabelEx 
                                        ID="lblLastEdited" 
                                        runat="server" 
                                        />
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
                                <li class="ma0 mb5 pa0">
                                    <tstsc:RichTextBoxJ 
                                        Authorized_ArtifactType="Document" 
                                        Authorized_Permission="Modify"
                                        ID="txtDescription" 
                                        runat="server"
                                        Screenshot_ArtifactType="Document" 
                                        />
                                </li>
                            </ul>
                        </div>
                    </div>
                            
 
 
 
 
                    <%-- COMMENTS --%> 
                    <div class="u-box_3 mb6"> 
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
                                        ArtifactType="Document"  
                                        AutoLoad="false"  
                                        ErrorMessageControlId="lblMessage" 
                                        ID="lstComments"  
                                        runat="server"  
                                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"  
                                        Width="100%"  
                                        /> 
                                    <tstsc:RichTextBoxJ  
                                        Authorized_ArtifactType="Document" 
									    Authorized_Permission="Modify"  
                                        Height="80px"  
                                        ID="txtNewComment"  
                                        runat="server"  
                                        Screenshot_ArtifactType="Document"  
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
                    </section>
   				</asp:Panel>

   				<asp:Panel ID="pnlVersions" Runat="server">
                    <div id="documentVersions"></div>
   				</asp:Panel>
   						
                <asp:Panel ID="pnlAssociations" Runat="server" CssClass="TabControlPanel">
                    <tstuc:AssociationsPanel 
                        ID="tstAssociationPanel" 
                        runat="server" 
                        />
   				</asp:Panel>

                <asp:Panel ID="pnlHistory" runat="server" CssClass="TabControlPanel">
                    <tstuc:HistoryPanel ID="tstHistoryPanel" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlAttachments" runat="server" CssClass="TabControlPanel">
					<tstuc:AttachmentPanel ID="tstAttachmentPanel" runat="server" />
					<br />
				</asp:Panel>
                        
                <asp:Panel ID="pnlPreview" runat="server" CssClass="TabControlPanel">
                    <div id="codePreview"  style="display: none">                                
                    </div>
                    <div id="noPreview" style="display: none">
                        <div class="alert alert-info alert-narrow">
                            <span class="fas fa-info-circle"></span>
                            <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Messages,SourceCodeFileDetails_PreviewNotAvailable %>" />
                        </div>
                    </div>
                    <div id="imagePreview" class="preview-image" style="display: none">
                        <a id="imgPreviewHyperLink" target="_blank">
                            <img ID="imgPreview" />
                        </a>
                    </div>
                    <div id="htmlPreview"  style="display: none"></div>
                    <iframe 
                        class="bn br3 w-100 h-100 ov-y-hidden"
                        id="htmlPreviewIframe" 
                        style="display: none; background: #FFFFFF;"
                        >
                    </iframe>
                    <div id="diagramPreview"  style="display: none">
                        <div class="btn-group mb3">
                            <button 
                                class="btn btn-default" 
                                ClientIDMode="Static"
                                id="diagramPreviewExportPng"
                                runat="server"
                                type="button" 
                                >
                                <i class="fal fa-image mr2"></i>
                                <asp:Localize runat="server" Text="<%$Resources:ClientScript,Diagram_ExportPNG %>" />
                            </button>
                            <button
                                class="btn btn-default" 
                                ClientIDMode="Static"
                                id="diagramPreviewExportPdf"
                                runat="server"
                                type="button" 
                                >
                                <i class="fal fa-file-pdf mr2"></i>
                                <asp:Localize runat="server" Text="<%$Resources:ClientScript,Diagram_ExportPDF %>" />
                            </button>
                        </div>
                        <div id="diagramPreview-preview"></div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server" CssClass="TabControlPanel">
                    <div id="edit-richtext">
                        <tstsc:RichTextBoxJ ID="txtEditRichText" runat="server" Height="400px" />
                    </div>
                    <div id="edit-plaintext">
                        <tstsc:TextBoxEx ID="txtEditPlainText" runat="server" Height="450px" Width="100%" TextMode="MultiLine" />
                    </div>
                    <div id="edit-diagram">
                        <tstsc:DiagramEditor 
                            ID="diagramEditor"
                            runat="server"
                            />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>
    

    <tstsc:AjaxFormManager 
        ArtifactTypeName="<%$Resources:Fields,Document%>"
        CheckUnsaved="true" 
        DisplayPageName="true"
        ErrorMessageControlId="lblMessage" 
        FolderPathControlId="pnlFolderPath" 
        ID="ajxFormManager" 
        NameField="Filename"
        RevertButtonControlId="btnRevert"
        runat="server" 
        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService" 
        WorkflowEnabled="true"
        WorkflowOperationsControlId="ajxWorkflowOperations" 
        >
        <ControlReferences>
            <tstsc:AjaxFormControl ControlId="lblDocumentStatusValue" DataField="DocumentStatusId" Direction="In" IsWorkflowStep="true" />
            <tstsc:AjaxFormControl ControlId="txtName" DataField="Filename" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtDescription" DataField="Description" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblDocumentId" DataField="AttachmentId" Direction="In" />
            <tstsc:AjaxFormControl ControlId="ddlDocFolder" DataField="ProjectAttachmentFolderId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlDocType" DataField="DocumentTypeId" ChangesWorkflow="true" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlCreatedBy" DataField="AuthorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="ddlEditedBy" DataField="EditorId" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtTags" DataField="Tags" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="lblCreatedOn" DataField="UploadDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblLastEdited" DataField="EditedDate" Direction="In" PropertyName="tooltip" />
            <tstsc:AjaxFormControl ControlId="lblCurrentVersion" DataField="CurrentVersion" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblSize" DataField="Size" Direction="In" />
            <tstsc:AjaxFormControl ControlId="lblFileType" DataField="Filetype" Direction="In" PropertyName="tooltip" />            
            <tstsc:AjaxFormControl ControlId="imgFileType" DataField="Filetype" Direction="In" PropertyName="textValue" />            
            <tstsc:AjaxFormControl ControlId="imgDocumentFileType" DataField="Filetype" Direction="In" PropertyName="textValue" />     
            <tstsc:AjaxFormControl ControlId="txtNewComment" DataField="NewComment" Direction="InOut" />                   
            <tstsc:AjaxFormControl ControlId="txtEditRichText" DataField="_Html" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="txtEditPlainText" DataField="_Text" Direction="InOut" />
            <tstsc:AjaxFormControl ControlId="diagramEditor" DataField="_Data" Direction="InOut" />
        </ControlReferences>
        <SaveButtons>
            <tstsc:AjaxFormSaveButton ControlId="btnSave" />
        </SaveButtons>
    </tstsc:AjaxFormManager>

    <tstsc:DialogBoxPanel ID="dlgUploadNewVersion" runat="server" Title="<%$Resources:Main,DocumentDetails_UploadNewVersion %>" Modal="true" Width="800px" Height="350px" CssClass="PopupPanel">
        <tstsc:MessageBox ID="msgUploadMessage" runat="server" SkinID="MessageBox" />
        <ul class="lsn px3">
            <li class="ma0 mb4 pa0">
                <tstsc:LabelEx 
                    ID="lblVersionNameType" 
                    runat="server" 
                    Required="true" 
                    Text="<%$Resources:Fields,File %>" 
                    />
                <div id="fileVersion-upload-container" class="file-upload-single">
                    <div class="file-upload-icon">
                        <label for="filVersionAttachment" id="lblFilVersionAttachment">
                            <span class="fas fa-cloud-upload-alt pointer"></span> 
		                </label>
                    </div>
                    <input type="file" id="filVersionAttachment" />
                    <div>
                        <p class="fileVersion-upload-inner file-upload-large tc">
                            <asp:Literal runat="server" Text="<%$ Resources:Dialogs,FileUpload_SelectFiles %>" />
                        </p>
                        <p class="fileVersion-upload-inner mobile-hide tc">
                            <asp:Literal runat="server" Text="<%$ Resources:Dialogs,FileUpload_DragAndDrop %>" />
                        </p>
                        <p class="file-upload-filename">
                            <span id="fileVersion-upload-filename"></span>
                        </p>
                    </div>
                </div>
            </li>
            <li class="ma0 mb2 pa0 rich-text-mini">
                <tstsc:LabelEx 
                    AppendColon="true"
                    AssociatedControlID="txtVersionDescription" 
                    ID="txtVersionDescriptionLabel" 
                    runat="server" 
                    Text="<%$Resources:Fields,Description %>" 
                    />
                <tstsc:RichTextBoxJ 
                    Height="80px" 
                    ID="txtVersionDescription" 
                    runat="server" 
                    />
            </li>
            <li class="ma0 mb4 pa0">
                <tstsc:LabelEx 
                    AppendColon="true"
                    AssociatedControlID="txtVersionNumber" 
                    CssClass="w-100"
                    ID="txtVersionNumberLabel" 
                    Required="true" 
                    runat="server" 
                    Text="<%$Resources:Fields,Version %>" 
                    />
                <div>
                    <tstsc:UnityTextBoxEx 
                        CssClass="text-box w6" 
                        id="txtVersionNumber" 
                        MaxLength="5" 
                        Runat="server"  
                        />
		            &nbsp;    
                    <tstsc:CheckBoxEx 
                        ID="chkVersionActive" 
                        runat="server" 
                        Checked="true" 
                        Text="<%$Resources:Main,DocumentDetails_MakeThisActiveVersion %>" 
                        role="v-top-children"
                        />
                </div>
            </li>
            <li class="ma0 pa0 btn-group">
                <tstsc:ButtonEx 
                    id="btnVersionUpload" 
                    SkinID="ButtonPrimary" 
                    runat="server" 
                    Text="<%$Resources:Buttons,Upload %>" 
                    ClientScriptMethod="btnVersionUpload_click()" 
                    />
                <tstsc:ButtonEx 
                    id="btnUploadCancel" 
                    runat="server" 
                    Text="<%$Resources:Buttons,Cancel %>" 
                    CausesValidation="true" 
                    ClientScriptMethod="close()" 
                    ClientScriptServerControlId="dlgUploadNewVersion" 
                    />
            </li>
        </ul>
    </tstsc:DialogBoxPanel>

	<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
      <Scripts>
        <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.xss.min.js" Assembly="Web" />
        <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" /> 
        <asp:ScriptReference Path="~/TypeScript/Followers.js" /> 

        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.prism.js" />
        <asp:ScriptReference Path="~/TypeScript/SyntaxHighlighting.js" />

        <asp:ScriptReference Assembly="Web" Name="Inflectra.SpiraTest.Web.ClientScripts.dhtmlx-diagramWithEditor.js" />

        <asp:ScriptReference Path="~/TypeScript/DocumentDetails.js" />       
      </Scripts>
      <Services>  
        <asp:ServiceReference Path="~/Services/Ajax/DocumentsService.svc" />  
        <asp:ServiceReference Path="~/Services/Ajax/DocumentVersionService.svc" />  
      </Services>  
    </tstsc:ScriptManagerProxyEx>
    <script type="text/javascript">
        SpiraContext.pageId = "Inflectra.Spira.Web.DocumentDetails";
        SpiraContext.ArtifactId = <%=attachmentId%>;
        SpiraContext.ArtifactIdOnPageLoad = <%=attachmentId%>;
		SpiraContext.ArtifactTabNameInitial = "<%=ArtifactTabName%>";
		SpiraContext.ArtifactTabName = "<%=ArtifactTabName%>";
        SpiraContext.ArtifactTypeId = <%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.Document%>;
        SpiraContext.EmailEnabled = <%=Inflectra.SpiraTest.Common.ConfigurationSettings.Default.EmailSettings_Enabled.ToString().ToLowerInvariant()%>;
        SpiraContext.HasCollapsiblePanels = true;
        SpiraContext.Mode = 'update';
        SpiraContext.MaxAllowedContentLength = 2147483648;    //Can be changed in Web.config
		SpiraContext.uiState.inputFilename = null; //set when document is ready
        SpiraContext.uiState.fileExtension = "";

        //Server Control IDs
        var btnSubscribe_id = '<%=this.btnSubscribe.ClientID%>';
        var ajxFormManager_id = '<%=this.ajxFormManager.ClientID%>';
        var lstComments_id = '<%=this.lstComments.ClientID %>'; 
        var btnNewComment_id = '<%=this.btnNewComment.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';
        var txtName_id = '<%=txtName.ClientID%>';
        var btnSave_id = '<%=btnSave.ClientID%>';
        var tabControl_id = '<%=this.tclDocumentDetails.ClientID%>';
        var navigationBar_id = '<%=this.navDocumentList.ClientID%>';
        var btnEmail_id = '<%=btnEmail.ClientID%>';
		var pnlPreview_id = '<%=pnlPreview.ClientID%>';

        //TabControl Panel IDs
        var pnlAttachments_id = '<%=pnlVersions.ClientID%>';
        var pnlHistory_id = '<%=pnlHistory.ClientID%>';
        var pnlAssociations_id = '<%=pnlAssociations.ClientID%>';

        //Base URLs
        var urlTemplate_artifactRedirectUrl = '<%=ArtifactRedirectUrl %>';
        var urlTemplate_artifactListUrl = '<%=ArtifactListPageUrl %>';
        var urlTemplate_screenshot = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ScreenshotUpload, ProjectId, -3, "{1}")))%>';
        var urlTemplate_projectHome = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.ProjectHome, this.ProjectId, 0)))%>';
        var urlTemplate_attachmentOpenUrl = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Attachment, this.ProjectId, -2)))%>';
        var urlTemplate_attachmentVersionOpenUrl = '<%=GlobalFunctions.JSEncode(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl(Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.AttachmentVersion, this.ProjectId, -2)))%>';

        //Page-specific controls
        var lnkDocumentName_id = '<%=lnkDocumentName.ClientID%>';
        var msgUploadMessage_id = '<%=msgUploadMessage.ClientID%>';
        var _documentDetails_droppedFile = null;

        function lnkAddVersion_click(evt)
        {
            //Prepopulate the version number with the latest version number
            var newVersion = '';
            var increment = 1.0;
            var currentVersion = $('#<%=this.lblCurrentVersion.ClientID%>').text();
            var maxVersion = SpiraContext.uiState.documentMaxVersionNumber;
            if (!isNaN(parseFloat(currentVersion)))
            {
                if (!isNaN(parseFloat(maxVersion))) {
                    newVersion = (parseFloat(maxVersion) + increment).toFixed(1);
                } else {
                    newVersion = (parseFloat(currentVersion) + increment).toFixed(1);
				}
            }
            $('#<%=this.txtVersionNumber.ClientID%>').val(newVersion);

            //Clear the existing files (if any)
			$('#fileVersion-upload-filename').text('');
			$('.fileVersion-upload-inner').show();
            //Hide any drag-and-drop mentions on mobile
            if (SpiraContext.IsMobile)
            {
				$('.fileVersion-upload-inner.mobile-hide').hide();
            }
            _documentDetails_droppedFile = null;

            //Display the dialog box
            var dlgUploadNewVersion_click = $find('<%=dlgUploadNewVersion.ClientID %>');
            dlgUploadNewVersion_click.display(evt);
        }
        function btnVersionUpload_click(evt)
        {
            //See if we have a dropped file
            if (_documentDetails_droppedFile)
            {
                uploadFile(_documentDetails_droppedFile);
            }
            else
            {
                //Get a handle to the upload box
                var files = $get('filVersionAttachment').files;
                if (files && files.length > 0)
                {
                    uploadFile(files[0]);
                }
            }
        }
        function uploadFile(file)
        {
            //Validate the data
            var description = CKEDITOR.instances['<%=this.txtVersionDescription.ClientID%>'].getData();
            var versionNumber = $('#<%=this.txtVersionNumber.ClientID%>').val();
            if (globalFunctions.isNullOrUndefined(versionNumber) || globalFunctions.trim(versionNumber) == '' )
            {
                alert(resx.FileUpload_VersionNumberRequired);
                return;
            }
            if (versionNumber.length > 5)
            {
                alert(resx.FileUpload_VersionNumberTooLong);
                return;
            }
            var makeActive = $('#<%=this.chkVersionActive.ClientID%>').prop('checked'); 
            documentDetails.uploadFile(file, SpiraContext.ArtifactId, description, versionNumber, makeActive);
        }

        //Page specific updating, called by DetailsPage.ts
        var documentDetails_attachmentId = -1;
        function updatePageContent()
        {
            documentDetails.updateUrl();
            documentDetails.updatePreview();
            documentDetails.check_hasData();
            documentDetails.versionGridId = 'documentVersions';
            documentDetails.uploadButton = '<%=Resources.Main.DocumentDetails_UploadNewVersion %>';
            documentDetails.makeActiveButton = '<%=Resources.Buttons.MakeActive %>';
            documentDetails.deleteButton = '<%=Resources.Buttons.Delete %>';
            documentDetails.messageBoxId = '<%=this.lblMessage.ClientID%>';
            documentDetails.dlgUploadNewVersion_id = '<%=this.dlgUploadNewVersion.ClientID%>';
            documentDetails.uploadClickHandler = lnkAddVersion_click;
            documentDetails.versionHeaders = [
                '<%=Resources.Fields.ActiveYn %>',
                '<%=Resources.Fields.Filename %>',
                '<%=Resources.Fields.Version %>',
                '<%=Resources.Fields.Description %>',
                '<%=Resources.Fields.Size %>',
                '<%=Resources.Fields.AuthorId %>',
                '<%=Resources.Fields.UploadDate %>',
                '<%=Resources.Fields.Operations %>'
            ];
            documentDetails.displayVersionsGrid(SpiraContext.ArtifactId);

            //Handle workflow status for versions
            var ajxFormManager = $find(ajxFormManager_id);
            var dataItem = ajxFormManager.get_dataItem();
            if (dataItem && dataItem.Fields)
            {
                //set the file extension so we can control how it gets updated
				SpiraContext.uiState.fileExtension = "." + dataItem.Fields.Filename.textValue.split('.').pop();
                //stop the user being able to change the file extension - clear any existing listener first
				SpiraContext.uiState.inputFilename.removeEventListener("input", keepFilenameExtension);
				SpiraContext.uiState.inputFilename.addEventListener("input", keepFilenameExtension);
                function keepFilenameExtension(event) {
                    //only add the event listener if the type is file
                    if (dataItem.Fields._AttachmentTypeId && dataItem.Fields._AttachmentTypeId.intValue == globalFunctions.attachmentTypeEnum.file) {
                        var newValue = event.target.value;
						if (!newValue.endsWith(SpiraContext.uiState.fileExtension)) {
                            var fixedValue = null;
                            var extensionRegex = new RegExp("\\.(.+)?$");
                            //if there is an attempt to make an extension like string at the end of the filename fix it
                            if (newValue.match(extensionRegex)) {
								fixedValue = newValue.replace(extensionRegex, SpiraContext.uiState.fileExtension);
                                //if there's no extension at all at the end of the filename add it back
                            } else {
								fixedValue = newValue + SpiraContext.uiState.fileExtension;
                            }
                            //update the input value
							SpiraContext.uiState.inputFilename.value = fixedValue;
                        }
                    }
				}


                var canEditContent = handleVersionsOnStatusChange(dataItem.Fields);
                var tabControl = $find('<%=this.tclDocumentDetails.ClientID%>');
                var tabEdit = tabControl.get_tabPage("tabEdit");
                var pnlEdit = document.getElementById('<%=this.pnlEdit.ClientID%>');
                //Determine if we can edit the content (needs to be allowed by workflow and the type of file)
                if (dataItem.Fields._MimeType)
                {

                    var mimeType = dataItem.Fields._MimeType.textValue;
                    var isFile = dataItem.Fields._AttachmentTypeId.intValue == globalFunctions.attachmentTypeEnum.file;
                    var isEditable = mimeType.indexOf('text/') != -1 ||
                        mimeType == 'application/json' ||
                        mimeType == 'application/xml' ||
						mimeType == 'application/x-diagram' ||
						mimeType == 'application/x-orgchart' ||
						mimeType == 'application/x-mindmap' ||
                        mimeType == 'application/x-bat';
                    if (isEditable && canEditContent && isFile)
                    {
                        tabEdit.show();

                        //See what sort of editing window to show
                        if (mimeType == 'text/html')
                        {
                            //Rich Text
                            $get('edit-richtext').style.display = 'block';
                            $get('edit-diagram').style.display = 'none';
                            $get('edit-plaintext').style.display = 'none';
                        }
                        else if (mimeType == 'application/x-diagram' || mimeType == 'application/x-orgchart' || mimeType == 'application/x-mindmap')
						{
							//Diagrams
							$get('edit-richtext').style.display = 'none';
							$get('edit-diagram').style.display = 'block';
                            $get('edit-plaintext').style.display = 'none';
                        }
                        else
                        {
                            //Plain Text / Markdown
                            $get('edit-richtext').style.display = 'none';
							$get('edit-diagram').style.display = 'none';
                            $get('edit-plaintext').style.display = 'block';
                        }
                    }
                    else
                    {
                        tabEdit.hide();
                        pnlEdit.style.display = "none";

                        //... and if the edit tab was active, switch to the default tab (view)
                        if (tabControl.get_selectedTabClientId() == tabEdit._tabPageClientId) {
                            var tabView = tabControl.get_tabPage("tabView");
                            tabControl.set_selectedTabClientId(tabView._tabPageClientId);
                        }
                    }
                }
                //If we can't edit... 
                else
                {
                    //... hide all editing elements
                    tabEdit.hide();
                    pnlEdit.style.display = "none";
                    $get('edit-richtext').style.display = 'none';
					$get('edit-diagram').style.display = 'none';
					$get('edit-plaintext').style.display = 'none';
                    //... and if the edit tab was active, switch to the default tab (view)
                    if (tabControl.get_selectedTabClientId() == tabEdit._tabPageClientId)
                    {
                        var tabView = tabControl.get_tabPage("tabView");
                        tabControl.set_selectedTabClientId(tabView._tabPageClientId);
                    }
                }
            }
        }
        function updatePageStatusContent(fields) {
            handleVersionsOnStatusChange(fields);
        }

        //Checks if we can add a new version, returns True if the content is currently editable
        function handleVersionsOnStatusChange(fields) 
        {
            var canEditContent = false;

			var ajxFormManager = $find(ajxFormManager_id);
			//First get the permissions for the document
			var canModifyDocument = globalFunctions.isAuthorizedToModifyCurrentArtifact(globalFunctions.artifactTypeEnum.document, ajxFormManager);
            if (canModifyDocument) {
                // first get the version field from the form manager - this contains workflow info
                var versionField = Array.isArray(fields) ? getFieldFromArray(fields, "DocumentVersions") : fields.DocumentVersions || null;
                if (versionField) 
                {
                    // gather required controls
                    var pnlVersions = document.getElementById('<%=this.pnlVersions.ClientID%>');
                    var tabControl = $find('<%=this.tclDocumentDetails.ClientID%>');
                    var tabVersions = tabControl.get_tabPage("tabVersions");
                    var nonVersionsTabs = tabControl.get_tabPages().filter(function(t) {return t._name != "tabVersions"});

                    // if we have a versions tab to manage with the workflow proceed
                    if (pnlVersions && tabVersions) 
                    {
                        // handle hidden state
                        if (versionField.hidden)
                        {
                            // if the user is currently viewing the version tab, we have to switch to another tab first (otherwise when a new tab is clicked, the versions tab pops back again)
                            if (nonVersionsTabs && tabControl.get_selectedTabClientId() == tabVersions.get_tabPageClientId()) 
                            {
                                tabControl.set_selectedTabClientId(nonVersionsTabs[0].get_tabPageClientId());
                            }
                            // then hide the panel contents and the tab itself
                            pnlVersions.style.display = "none";
                            tabVersions.hide();
                        } 
                        else 
                        {
                            // first make sure that the panel tab is visible - as we are not in the hide state but may have been previously
                            tabVersions.show();
                            // handle disabled state
                            if (!versionField.editable)
                            {
                                // verify the react control for the version grid exists. If it does, change its state
                                if (window && window.rct_comp_documentVersionGrid && window.rct_comp_documentVersionGrid.set_disabled) {
                                    window.rct_comp_documentVersionGrid.set_disabled();
                                }
                            }
                            // make sure nothing is disabled
                            else 
                            {
                                // verify the react control for the version grid exists. If it does, change its state
                                if (window && window.rct_comp_documentVersionGrid && window.rct_comp_documentVersionGrid.set_enabled) {
                                    window.rct_comp_documentVersionGrid.set_enabled();
                                    canEditContent = true;
                                }

                                if (versionField.required) 
                                {
                                    // handle required state

                                }
                            }
                        }
                    }
                }
			}
            return canEditContent;
        }

        function getFieldFromArray(arr, field) {
            var hasField = arr.filter(function(x) { return x.fieldName == field});
            return hasField ? hasField[0] : null; 
        }

        function ajxWorkflowOperations_operationExecuted(transitionId, isStatusOpen)
        {
            //Put any post-workflow operations here
        }

        function tclDocumentDetails_updateViewIframeHeight(tabPage) {
			//See if the tab is visible
			var loadNow = ($find(tabControl_id).get_selectedTabClientId() == pnlPreview_id);
            if (loadNow) {
			    //Update the iframe's height - if it should be dispalyed
			    var element = $get('htmlPreviewIframe');
			    if (!element.style.display || element.style.display != "none") {
				    var newHeight = (element.contentWindow.document.body.scrollHeight + 50) + 'px';
				    //only update the height if it has changed
				    if (element.style.height != newHeight) {
					    element.style.height = newHeight;
				    }
			    }
			}
		}

        //Set some initialstate
        $(document).ready(function() {
            //Hide any drag-and-drop mentions on mobile
            if (SpiraContext.IsMobile)
            {
                $('.mobile-hide').hide();
            }

            // because we allow users to click on the surrounding container (file-upload-container) we need to stop the events from clickin the label and the input itself from bubbling up to that container
            // if we do not then clicking the label registers as a click on the container which as below then in code clicks the input again which causes errors on the page
			$('#filVersionAttachment').on("click", function (event) {
                event.stopPropagation();
            });
			$('#lblFilVersionAttachment').on("click", function (event) {
                event.stopPropagation();
            });

            $('#fileVersion-upload-container')
				.on("click", function() {
                    $('#filVersionAttachment')[0].click();
                })
                .on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
                    // preventing the unwanted behaviours
                    e.preventDefault();
                    e.stopPropagation();
                })
				.on('dragover dragenter', function ()
				{
					$('#fileVersion-upload-container').addClass('file-upload-is-dragover');
				})
				.on('dragleave dragend drop', function () {
					$('#fileVersion-upload-container').removeClass('file-upload-is-dragover');
				})
				.on('drop', function (e) {
				    droppedFiles = e.originalEvent.dataTransfer.files; // the files that were dropped
				    if (droppedFiles && droppedFiles.length > 0)
				    {
                        $('#fileVersion-upload-filename').text(droppedFiles[0].name);
						$('.fileVersion-upload-inner').hide();
				        _documentDetails_droppedFile = droppedFiles[0];
                    }
				});

            $('#filVersionAttachment').on("change", function () {
                var files = $('#filVersionAttachment')[0].files;
                if (files && files.length > 0)
                {
                    document.getElementById("fileVersion-upload-filename").textContent = files[0].name;
                    $('.fileVersion-upload-inner').hide();
                    _documentDetails_droppedFile = null;
                }
            });

            // make sure the preview tabs render properly - needs to do each time you open the tab to make sure line numbers align correctly
            var previewObserver = new MutationObserver(function(mutations) {
                mutations.forEach(function(mutationRecord) {
                    Prism.highlightAllUnder(mutationRecord.target);
                });    
            });

            var target = document.getElementById('<%=pnlPreview.ClientID%>');
            previewObserver.observe(target, { attributes: true, attributeFilter: ['style'] });

            // make sure the filename is set on page load so event listeners can access it
            SpiraContext.uiState.inputFilename = document.getElementById(txtName_id);
        });
    </script>
</asp:Content>
