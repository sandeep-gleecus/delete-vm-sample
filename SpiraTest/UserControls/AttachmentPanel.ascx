<%@ Control Language="c#" AutoEventWireup="True" Codebehind="AttachmentPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.AttachmentPanel" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
<asp:Panel runat="server" ID="panelControl" class="TabControlHeader">
    <div class="btn-toolbar-mid-page">
        <div class="btn-group priority1">
            <tstsc:HyperLinkEx ID="lnkAddNew" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="if (!this.disabled) { tstucAttachmentPanel.upload_attachment(event) }">
                <span class="fas fa-plus"></span>
                <asp:Localize Text="<%$Resources:Dialogs,AttachmentPanel_AddNew %>" runat="server" />
            </tstsc:HyperLinkEx>
            <tstsc:HyperLinkEx ID="lnkAddExisting" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod="if (!this.disabled) { tstucAttachmentPanel.add_existing(event) }">
                <span class="fas fa-link"></span>
                <asp:Localize Text="<%$Resources:Dialogs,AttachmentPanel_AddExisting %>" runat="server" />
            </tstsc:HyperLinkEx>
            <tstsc:HyperLinkEx ID="lnkRemove" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)">
                <span class="fas fa-trash-alt"></span>
                <asp:Localize Text="<%$Resources:Buttons,Remove %>" runat="server" />
            </tstsc:HyperLinkEx>
        </div>
        <div class="btn-group priority3">
            <tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdAttachmentList" ClientScriptMethod="load_data()">
                <span class="fas fa-sync"></span>
                <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
            </tstsc:HyperLinkEx>
            <tstsc:DropMenu id="mnuView" runat="server" GlyphIconCssClass="mr3 fas fa-mouse-pointer" Text="<%$Resources:Buttons,View %>" ClientScriptMethod="grdAttachmentList_viewItem()">
			    <DropMenuItems>
				    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="View" Value="<%$Resources:Buttons,ViewItem %>" GlyphIconCssClass="mr3 fas fa-mouse-pointer" ClientScriptMethod="grdAttachmentList_viewItem('_self')" />
				    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="ViewNewTab" Value="<%$Resources:Buttons,ViewItemNewTab %>" GlyphIconCssClass="mr3 fas fa-external-link-alt" ClientScriptMethod="grdAttachmentList_viewItem('_blank')" />
			    </DropMenuItems>
		    </tstsc:DropMenu>
        </div>
        <div class="btn-group">
            <tstsc:DropDownListEx ID="ddlShowHideColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdAttachmentList" ClientScriptMethod="toggle_visibility" />
        </div>
        <div class="btn-group priority3">
            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			    ClientScriptServerControlId="grdAttachmentList" ClientScriptMethod="apply_filters()">
			    <DropMenuItems>
				    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				    <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			    </DropMenuItems>
		    </tstsc:DropMenu>
        </div>
    </div>
</asp:Panel>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,AttachmentPanel_Attachments %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>
<tstsc:SortedGrid ID="grdAttachmentList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header" EditRowCssClass="Editing"
    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
    RowCssClass="Normal" DisplayAttachments="false" AllowEditing="true" BaseUrlTarget="_blank"
    runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"
    Authorized_ArtifactType="Document" Authorized_Permission="BulkEdit"
    NegativePrimaryKeysDisabled="false">
    <ContextMenuItems>
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-download" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Document" Authorized_Permission="View" />
        <tstsc:ContextMenuItem Divider="True" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,ViewItem %>" ClientScriptMethod="grdAttachmentList_viewItem('_self')" Authorized_ArtifactType="Document" Authorized_Permission="View" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,ViewItemNewTab %>" ClientScriptMethod="grdAttachmentList_viewItem('_blank')" Authorized_ArtifactType="Document" Authorized_Permission="View" />
        <tstsc:ContextMenuItem Divider="True" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Document" Authorized_Permission="Modify" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-trash-alt" Caption="<%$Resources:Buttons,Remove%>" ClientScriptMethod="grdAttachmentList_removeFromArtifact()" Authorized_ArtifactType="Document" Authorized_Permission="Modify" />
    </ContextMenuItems>    
</tstsc:SortedGrid>
    
<tstsc:DialogBoxPanel 
    ID="dlgUploadDocument" 
    runat="server" 
    CssClass="PopupPanel mobile-fullscreen overflow-hidden" 
    ErrorMessageControlId="msgUploadMessage"
    Width="500px" 
    Height="490px" 
    Modal="true" 
    Title="<%$Resources:Dialogs,FileUploadDialog_AddNewDocument %>"
>
    <div class="PopupPanelBody">
        <tstsc:MessageBox ID="msgUploadMessage" runat="server" SkinID="MessageBox" />
        <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="true" ShowSummary="False" DisplayMode="BulletList"
		    Runat="server" id="ValidationSummary1" />
        <div id="tblAttachment" style="width:100%">
            <div class="hidden-mobile df mb3">
                <tstsc:LabelEx CssClass="w7 dib" ID="lblType" runat="server" Text="<%$Resources:Fields,Type %>" Required="true" />
                <tstsc:RadioButtonEx ID="radFile" runat="server" AutoPostBack="false" GroupName="AttachmentType" Checked="true" />
                <asp:Label CssClass="mr4 ml2" AssociatedControlID="radFile" runat="server" Text="<%$Resources:Fields,File %>" />
                <tstsc:RadioButtonEx ID="radUrl" runat="server" AutoPostBack="false" GroupName="AttachmentType" Checked="false" />
                <asp:Label CssClass="mr4 ml2" AssociatedControlID="radUrl" runat="server" Text="<%$Resources:Fields,URL %>" />
                <tstsc:RadioButtonEx ID="radScreenshot" runat="server" AutoPostBack="false" GroupName="AttachmentType" Checked="false" />
                <asp:Label CssClass="ml2" AssociatedControlID="radScreenshot" runat="server" Text="<%$Resources:Fields,Screenshot %>" />
            </div>

            <div id="tblAttachment_trFilename">
                <div>
                    <div id="file-upload-container" class="file-upload-single">
                        <div class="file-upload-icon">
                            <label for="filAttachment" id="lblFilAttachment">
                                <span class="fas fa-cloud-upload-alt pointer"></span> 
		                    </label>
                        </div>
                        <input type="file" id="filAttachment" />
                        <div>
                            <p class="file-upload-inner file-upload-large">
                                <asp:Literal runat="server" Text="<%$ Resources:Dialogs,FileUpload_SelectFiles %>" />
                            </p>
                            <p class="file-upload-inner mobile-hide">
                                <asp:Literal runat="server" Text="<%$ Resources:Dialogs,FileUpload_DragAndDrop %>" />
                            </p>
                            <p class="file-upload-filename">
                                <span id="file-upload-filename"></span>
                            </p>
                        </div>
                    </div>
                </div>
            </div>


            <div id="tblAttachment_trUrl" class="hidden-mobile df">
                <tstsc:LabelEx CssClass="w7" runat="server" ID="txtURLLabel" AssociatedControlID="txtURL" Required="true" Text="<%$Resources:Fields,URL %>" AppendColon="true"/>
                <div class="grow-1">
                    <tstsc:TextBoxEx ID="txtURL" Runat="server" MaxLength="255" CssClass="text-box" />
	    	        <asp:RegularExpressionValidator ID="vldWebSite" runat="server" ControlToValidate="txtURL" ErrorMessage="<%$Resources:Messages,FileUploadDialog_URLNotValid %>" Text="*" ValidationExpression="<%#GlobalFunctions.VALIDATION_REGEX_URL%>" />
                </div>
            </div>


            <div id="tblAttachment_trScreenshot" class="hidden-mobile df flex-column-xs">
                <tstsc:LabelEx CssClass="w7" runat="server" ID="txtScreenshotFilenameLabel" AssociatedControlID="txtScreenshotFilename" Required="true" Text="<%$Resources:Fields,Screenshot %>" AppendColon="true" />
                <div class="grow-1">
                    <tstsc:TextBoxEx ID="txtScreenshotFilename" runat="server" TextMode="SingleLine" />
                    <tstsc:ScreenshotCapture ID="ajxScreenshotCapture" runat="server" />
                </div>
            </div>


            <div class="mt3 df flex-column-xs">
                <tstsc:LabelEx CssClass="w7" runat="server" ID="txtAttachmentDescriptionLabel" AssociatedControlID="txtAttachmentDescription" Required="false" Text="<%$Resources:Fields,Description %>" AppendColon="true" />
                <div class="grow-1">
                    <tstsc:TextBoxEx ID="txtAttachmentDescription" runat="server" Height="50px" DynamicHeight="False" RichText="false"
                        TextMode="MultiLine"/>
                </div>
            </div>
            <div class="mt3 df flex-column-xs">
                <tstsc:LabelEx  CssClass="w7" runat="server" ID="lblDocType" AssociatedControlID="ddlDocType" Required="true" Text="<%$Resources:Fields,DocumentType %>" AppendColon="true" />
	            <tstsc:DropDownListEx id="ddlDocType" Runat="server" DataValueField="DocumentTypeId" DataTextField="Name" DataMember="DocumentType" NoValueItem="false" />
            </div>
            <div class="mt3 df flex-column-xs">
                <tstsc:LabelEx  CssClass="w7" runat="server" ID="lblDocFolder" Required="true" AssociatedControlID="ddlDocFolder" Text="<%$Resources:Fields,DocumentFolder %>" AppendColon="true" />
	            <tstsc:DropDownHierarchy id="ddlDocFolder" Runat="server" DataValueField="ProjectAttachmentFolderId" DataTextField="Name"
	                    IndentLevelField="IndentLevel" ItemImage="Images/Folder.svg" NoValueItem="false" />
            </div>
            <div class="mt3 df flex-column-xs">
		        <tstsc:LabelEx  CssClass="w7" runat="server" ID="txtTagsLabel" Required="false" AssociatedControlID="txtTags" Text="<%$Resources:Fields,Tags %>" AppendColon="true" />
		        <div class="grow-1">
                    <tstsc:TextBoxEx id="txtTags" Runat="server" MaxLength="255" />
	    	        <asp:RegularExpressionValidator ID="vldTags" runat="server" ControlToValidate="txtTags" ErrorMessage="<%$Resources:Messages,FileUploadDialog_TagsNotValid %>" Text="*" ValidationExpression="<%#GlobalFunctions.VALIDATION_REGEX_TAGS%>" />    
                </div>
            </div>
            <div class="mt4 mb3 btn-group ml7">
                <tstsc:HyperLinkEx ID="btnUpload" SkinID="ButtonPrimary" runat="server" CausesValidation="true" Text="<%$Resources:Buttons,Upload %>" ClientScriptMethod="btnUpload_click()" />
                <tstsc:HyperLinkEx ID="btnCancel" SkinID="ButtonDefault" runat="server" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgUploadDocument" ClientScriptMethod="close()" />
            </div>
        </div>
    </div>
</tstsc:DialogBoxPanel>

<tstsc:DialogBoxPanel 
    ErrorMessageControlId="divMessage" 
    Height="400px" 
    ID="dlgAddExistingAttachment" 
    Modal="true" 
    runat="server" 
    Title="<%$Resources:Dialogs,AttachmentPanel_AddExistingDocument %>"
    Width="550px" 
    >
	<h4>
        <asp:Localize runat="server" Text="<%$Resources:Dialogs,AttachmentPanel_ChooseTypeOfExisting %>" />
    </h4>
	<div class="Spacer"></div>
    <div class="radio-inline">
	    <tstsc:RadioButtonEx ID="radDocuments" Checked="true" Runat="server" GroupName="AttachmentType" Text="<%$Resources:Main,SiteMap_Documents %>" Style="margin-right:1rem;" ClientScriptMethod="tstucAttachmentPanel.existingSourceChanged(event)" />
	</div>
    <div class="radio-inline">
        <tstsc:RadioButtonEx ID="radSourceCode" Runat="server" GroupName="AttachmentType" Text="<%$Resources:Main,SiteMap_SourceCode %>" Style="margin-right:1rem;" ClientScriptMethod="tstucAttachmentPanel.existingSourceChanged(event)" />
    </div>
    <div id="divDocuments" runat="server" style="display:block">
        <div class="display-inline-block">
            <table class="SelectorContainer" style="width: 240px">
                <tr>
                    <th>
                        <tstsc:ImageEx
                            CssClass="w4 h4"
                            AlternateText="<%$Resources:Fields,Folder %>" 
                            ID="imgFolder" 
                            ImageUrl="Images/Folder.svg" 
                            runat="server" 
                            />
                        <asp:Localize runat="server" Text="<%$Resources:Fields,Folder %>" />
                    </th>
                </tr>
                <tr>
                    <td>
                        <div class="Body" style="height:200px">
                            <tstsc:TreeView ID="ajxDocFolderSelector" runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService"
                                LoadingImageUrl="Images/action-Spinner.svg" CssClass="FolderTree" ErrorMessageControlId="divMessage"
                                ClientScriptMethod="load_data();" ClientScriptServerControlId="ajxDocFileSelector" />
                        </div>
                    </td>
                </tr>
            </table>
		</div>
        <div class="display-inline-block">
			<tstsc:ItemSelector ID="ajxDocFileSelector" 
                Runat="server" 
                NameLegend="<%$Resources:Fields,Document %>"
				WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService" 
                AlternateSelect="false"
				MultipleSelect="true" 
                NameField="Filename" 
                Height="234px" 
                style="width: 240px;"
				ErrorMessageControlId="divMessage" 
                AutoLoad="false" 
                CssClass="HierarchicalSelector"
			/>            
        </div>
    </div>
    <div id="divSourceCode" runat="server" style="display:none">
        <div>
		    <div class="display-inline-block">
                <table class="SelectorContainer" style="width: 250px">
                    <tr>
                        <th>
                            <tstsc:ImageEx
                                CssClass="w4 h4"
                                AlternateText="<%$Resources:Fields,Folder %>" 
                                ID="ImageEx1" 
                                ImageUrl="Images/Folder.svg" 
                                runat="server" 
                                />
                            <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Folder %>" />
                        </th>
                    </tr>
                    <tr>
                        <td>
                            <div class="Body" style="height:130px;">
                                <tstsc:TreeView ID="ajxSourceCodeFolderSelector" runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService"
                                    LoadingImageUrl="Images/action-Spinner.svg" CssClass="FolderTree" ErrorMessageControlId="divMessage"
                                    ClientScriptMethod="load_data();" ClientScriptServerControlId="ajxSourceCodeFileSelector" />
                            </div>
                        </td>
                    </tr>
                </table>
		    </div>
		    <div class="display-inline-block" >
			    <tstsc:ItemSelector ID="ajxSourceCodeFileSelector" Runat="server" NameLegend="<%$Resources:Fields,File %>"
				    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.SourceCodeFileService" AlternateSelect="false"
				    MultipleSelect="true" NameField="Name" Height="164px" Width="250px"
				    ErrorMessageControlId="divMessage" AutoLoad="false" CssClass="HierarchicalSelector"
			    />            
		    </div>
        </div>
        <div>
                <tstsc:LabelEx ID="txtAssociationCommentLabel" AssociatedControlID="txtAssociationComment" Text="<%$Resources:Fields,Comment %>" Required="false" runat="server" />:<br />
                <tstsc:TextBoxEx MaxLength="255" RichText="false" TextMode="MultiLine" ID="txtAssociationComment" runat="server"/>
        </div>
    </div>
    <div class="py3 px2 btn-group">
        <tstsc:HyperLinkEx ID="lnkAddAssociation" SkinID="ButtonPrimary" runat="server" NavigateUrl="javascript:void(0)" Text="<%$Resources:Buttons,Add %>" ClientScriptMethod="tstucAttachmentPanel.addAssociation()" />
        <tstsc:HyperLinkEx ID="lnkCancel" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" Text="<%$Resources:Buttons,Cancel %>" ClientScriptServerControlId="dlgAddExistingAttachment" ClientScriptMethod="close()" />
    </div>
</tstsc:DialogBoxPanel>

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>
    <asp:ServiceReference Path="~/Services/Ajax/DocumentsService.svc" />  
    <asp:ServiceReference Path="~/Services/Ajax/SourceCodeFileService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
	var resx = Inflectra.SpiraTest.Web.GlobalResources;

    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.AttachmentPanel = function ()
    {
        this._userId = <%=UserId%>;
        this._projectId = <%=ProjectId%>;
        this._projectTemplateId = <%=ProjectTemplateId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;
        this._radDocuments = null;
		this._radSourceCode = null;
		this._ajxDocFolderSelector = null;
		this._ajxDocFileSelector = null;
		this._ajxSourceCodeFolderSelector = null;
		this._ajxSourceCodeFileSelector = null;
		this._hasData = false;
        this._fileExtension = 'png';
        this._trFile = null;
        this._trUrl = null;
        this._trScreenshot = null;
        this._btnUpload = null;
        this._radFile_id = '<%=radFile.ClientID%>';
        this._radUrl_id = '<%=radUrl.ClientID%>';
        this._radScreenshot_id = '<%=radScreenshot.ClientID%>';
        this._grdAttachmentList_id = '<%=grdAttachmentList.ClientID%>';
        this._dlgUploadDocument_id = '<%=dlgUploadDocument.ClientID%>';
        this._droppedFile = null;

        Inflectra.SpiraTest.Web.UserControls.AttachmentPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.AttachmentPanel.prototype =
    {
        initialize: function ()
        {
            //Get handles
            this._trFile = $get('tblAttachment_trFilename');
            this._trUrl = $get('tblAttachment_trUrl');
            this._trScreenshot = $get('tblAttachment_trScreenshot');
            this._btnUpload = $get('<%=btnUpload.ClientID%>');

            //Set the default mode to file
            this.show_attachment_type_file();
            $get(this._radFile_id).checked = true;
        },
        dispose: function ()
        {
            delete this._trFile;
            delete this._trUrl;
            delete this._trScreenshot;
            delete this._btnUpload;
        },

        /* Properties */
        get_artifactId: function()
        {
            return this._artifactId;
        },
        set_artifactId: function(value)
        {
            this._artifactId = value;
        },

        get_artifactTypeId: function()
        {
            return this._artifactTypeId;
        },
        set_artifactTypeId: function(value)
        {
            this._artifactTypeId = value;
        },

        /* Properties */
        set_droppedFile: function(value)
        {
            this._droppedFile = value;
        },

        /* Public Methods */
        upload_attachment: function (evt)
        {
            //Make sure we have an artifact
            if (this._artifactId == -1 || this._artifactId == 0)
            {
                return;
            }
            if (!evt)
            {
                evt = window.event;
            }
            //get a handle to the dialog box
            var dialog = $find('<%=dlgUploadDocument.ClientID%>');
            var ajxScreenshotCapture = $find('<%=ajxScreenshotCapture.ClientID%>');

            //Position the dialog box and display
            dialog.auto_position(evt);
            dialog.display();

            //Clear certain fields
            $('#<%=txtAttachmentDescription.ClientID%>').val('');
            $('#<%=txtScreenshotFilename.ClientID%>').val('');
            $('#<%=txtTags.ClientID%>').val('');
            $('#<%=txtURL.ClientID%>').val('');
            $('#file-upload-filename').text('');
            $('.file-upload-inner').show();
            //Hide any drag-and-drop mentions on mobile
            if (SpiraContext.IsMobile)
            {
                $('.file-upload-inner.mobile-hide').hide();
            }
            this.set_droppedFile(null);
            $get('filAttachment').value = null;
            ajxScreenshotCapture.clearPreview();

            // set the color scheme
            setTimeout(function () {
                if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.ckeditorSetColorScheme) {
                    window.rct_comp_globalNav.ckeditorSetColorScheme(document.body.dataset.colorscheme);
                }
            }, 500);
        },

        load_data: function (filters, loadNow)
        {
            var grdAttachments = $find('<%=grdAttachmentList.ClientID%>');
            grdAttachments.set_standardFilters(filters);
            if (loadNow)
            {
                grdAttachments.load_data();
            }
            else
            {
                grdAttachments.clear_loadingComplete();
            }

            //Update the permissions
            this.update_permissions();
        },
        update_permissions: function()
        {
            //Set the permissions on the Add/Remove Attachments buttons (limited Modify is OK if we own the associated artifact)
            //We also need to be able to create or modify documents depending on the action
            //If we have the special case of a Test Run or new incident, the upload link permission we need is 'Create' not 'Modify' - for test run this is only for test execution pages (where there is no page 'mode')
            var isAuthorizedToModify = false;
            var isAuthorizedToAdd = false;
            var authorizedStateModifyArtifact;
            if ((SpiraContext.ArtifactTypeId == globalFunctions.artifactTypeEnum.testRun && !SpiraContext.Mode) || SpiraContext.Mode == 'new')
            {
                authorizedStateModifyArtifact = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, SpiraContext.ArtifactTypeId);
            }
            else
            {
                authorizedStateModifyArtifact = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, SpiraContext.ArtifactTypeId);
            }
            var authorizedStateAddDocs = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Create, globalFunctions.artifactTypeEnum.document);
            var authorizedStateModifyDocs = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, globalFunctions.artifactTypeEnum.document);
            
            if (authorizedStateModifyArtifact != globalFunctions.authorizationStateEnum.prohibited)
            {
                var isCreatorOrOwner = SpiraContext.IsArtifactCreatorOrOwner;
                if (authorizedStateModifyArtifact == globalFunctions.authorizationStateEnum.authorized || isCreatorOrOwner)
                {
                    isAuthorizedToModify = true;
                    isAuthorizedToAdd = true;
                }
            }
            if (authorizedStateAddDocs != globalFunctions.authorizationStateEnum.authorized)
            {
                isAuthorizedToAdd = false;
            }
            if (authorizedStateModifyDocs == globalFunctions.authorizationStateEnum.prohibited)
            {
                isAuthorizedToModify = false;
            }
            if (isAuthorizedToAdd)
            {
                $('#<%=lnkAddNew.ClientID%>').removeClass('disabled');
            }
            else
            {
                $('#<%=lnkAddNew.ClientID%>').addClass('disabled');
            }
            if (isAuthorizedToModify)
            {
                $('#<%=lnkAddExisting.ClientID%>').removeClass('disabled');
                $('#<%=lnkRemove.ClientID%>').removeClass('disabled');
            }
            else
            {
                $('#<%=lnkAddExisting.ClientID%>').addClass('disabled');
                $('#<%=lnkRemove.ClientID%>').addClass('disabled');
            }
        },

        //Displays the add-existing dialog box
        add_existing: function(evt)
        {
            //Make sure we have an artifact
            if (this._artifactId == -1 || this._artifactId == 0)
            {
                return;
            }
            if (!evt)
            {
                evt = window.event;
            }
            //get a handle to the dialog box
            var dialog = $find('<%=dlgAddExistingAttachment.ClientID%>');

            //Display the dialog box
            dialog.display(evt);

            //Get handles to the various controls
            this._radDocuments = $get('<%=radDocuments.ClientID%>');
		    this._radSourceCode = $get('<%=radSourceCode.ClientID%>');
		    this._ajxDocFolderSelector = $find('<%=this.ajxDocFolderSelector.ClientID%>');
		    this._ajxDocFileSelector = $find('<%=this.ajxDocFileSelector.ClientID%>');
		    this._ajxSourceCodeFolderSelector = $find('<%=this.ajxSourceCodeFolderSelector.ClientID%>');
		    this._ajxSourceCodeFileSelector = $find('<%=this.ajxSourceCodeFileSelector.ClientID%>');

            //Load the document folders
            this._ajxDocFolderSelector.load_data();
        },

        existingSourceChanged: function(evt)
        {
            var divDocuments = $get('<%=divDocuments.ClientID %>');
            var divSourceCode = $get('<%=divSourceCode.ClientID %>');
		    if (this._radDocuments && this._radDocuments.checked)
		    {
			    divDocuments.style.display = 'block';
			    divSourceCode.style.display = 'none';
			    this._ajxDocFolderSelector.load_data();
		    }
		    if (this._radSourceCode && this._radSourceCode.checked)
		    {
			    divDocuments.style.display = 'none';
			    divSourceCode.style.display = 'block';
			    this._ajxSourceCodeFolderSelector.load_data();
		    }
        },

        //Actually adds the new association
        addAssociation: function()
        {
		    var grdAttachmentList = $find('<%=this.grdAttachmentList.ClientID%>');
		    var parameters = {};
		    parameters.artifactTypeId = this._artifactTypeId;
		    parameters.artifactId = this._artifactId;
		    //See what kind of association we have
            if (this._radSourceCode && this._radSourceCode.checked)
            {
                //Get the list of source code files selected
                var sourceCodeFileIds = this._ajxSourceCodeFileSelector.get_selectedItems();
                if (sourceCodeFileIds.length < 1)
		        {
			        alert(resx.AttachmentPanel_NeedToSelectOneFile);
			        return;
		        }
                //Turn the array into a comma-separated list
		        var sourceCodeFileIdList = '';
		        for (var i = 0; i < sourceCodeFileIds.length; i++)
		        {
			        if (i == 0)
			        {
				        sourceCodeFileIdList = sourceCodeFileIds[i];
			        }
			        else
			        {
				        sourceCodeFileIdList += ',' + sourceCodeFileIds[i];
			        }
		        }

                parameters.fileIds = sourceCodeFileIdList;
                var comment = $get('<%=txtAssociationComment.ClientID %>').value;
		        parameters.comment = comment;
		        var result = grdAttachmentList.custom_operation_extended('AddSourceCodeFile', parameters);

                //Clear the comment and selection
                $get('<%=txtAssociationComment.ClientID %>').value = '';
                this._ajxSourceCodeFileSelector.load_data();
            }
            else
            {
                //Get the list of documents selected
                var attachmentIds = this._ajxDocFileSelector.get_selectedItems();
                if (attachmentIds.length < 1)
		        {
			        alert(resx.AttachmentPanel_NeedToSelectOneDocument);
			        return;
		        }

                //Turn the array into a comma-separated list
		        var attachmentIdList = '';
		        for (var i = 0; i < attachmentIds.length; i++)
		        {
			        if (i == 0)
			        {
				        attachmentIdList = attachmentIds[i];
			        }
			        else
			        {
				        attachmentIdList += ',' + attachmentIds[i];
			        }
		        }
                parameters.attachmentIds = attachmentIdList;
		        var result = grdAttachmentList.custom_operation_extended('AddExistingDocument', parameters);

                //Clear the selection
                this._ajxDocFileSelector.load_data();
            }

		    //Close the dialog
		    var dlgAddExistingAttachment = $find('<%=this.dlgAddExistingAttachment.ClientID%>');
		    dlgAddExistingAttachment.close();
        },

        check_hasData: function(callback)
        {
            //See if we have data
            var artifactReferences = [
            {
                artifactId: this._artifactId,
                artifactTypeId: this._artifactTypeId,
            }];
            Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.Documents_Count(this._projectId, artifactReferences, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
        },
        check_hasData_success: function(count, callback)
        {
            if (callback)
            {
                //Specify if we have data or not
                callback(count > 0);
            }
        },
        check_hasData_failure: function(ex)
        {
            //Fail quietly
        },

        show_attachment_type_file: function ()
        {
            //Show the file attachment row and hide the url one
            this._trFile.classList.remove("dn");
            this._trUrl.classList.add("dn");
            this._trScreenshot.classList.add("dn");
        },
        show_attachment_type_url: function ()
        {
            //Show the url attachment row and hide the file one
            this._trUrl.classList.remove("dn");
            this._trFile.classList.add("dn");
            this._trScreenshot.classList.add("dn");
        },
        show_attachment_type_screenshot: function ()
        {
            //Show the url attachment row and hide the file one
            this._trScreenshot.classList.remove("dn");
            this._trFile.classList.add("dn");
            this._trUrl.classList.add("dn");

            //Focus on the screenshot
            var ajxScreenshotCapture = $find('<%=ajxScreenshotCapture.ClientID %>');
            if (ajxScreenshotCapture)
            {
                ajxScreenshotCapture.focusOnPasteCatcher();
            }
        },

        pasteScreenshot: function (data)
        {
            //Clear any prior errors
            globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));

            //Store the data and get the filetype
            //data:image/png;base64,XXXXXXXXXX
            var reDataUrl = /^data:image\/(png|gif|jpg|jpeg|tiff|bmp);base64,(.*)/;
            if (reDataUrl.test(data))
            {
                var m = reDataUrl.exec(data);
                this._fileExtension = m[1];
                this._encodedData = data;

                //Provide a default filename
                var txtScreenshotFilename = $get('<%=txtScreenshotFilename.ClientID%>');
                txtScreenshotFilename.value = resx.FileArtifactUploadDialog_DefaultScreenshotFilename + '.' + this._fileExtension;
            }
            else
            {
                alert(resx.ScreenshotCapture_InvalidImageFormat);
            }
        },

        upload: function ()
        {
            //Clear any prior errors
            globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));

            //First fire the asp.net client-side validators
            var isPageValid = Page_ClientValidate();
            if (!isPageValid)
            {
                //Validation failed, so return false
                return false;
            }

            //Get a handle to the three radio buttons
            var radFile = $get(this._radFile_id);
            var radUrl = $get(this._radUrl_id);
            var radScreenshot = $get(this._radScreenshot_id);

            //See what type of attachment we have
            var filename = null;
            var url = null;
            var file = null;
            if (radScreenshot.checked)
            {
                //Get the data from the form and do some additional validation
                var txtScreenshotFilename = $get('<%=txtScreenshotFilename.ClientID%>');
                filename = txtScreenshotFilename.value;
                if (globalFunctions.trim(filename) == '')
                {
                    globalFunctions.display_error_message($get('<%=msgUploadMessage.ClientID%>'), resx.FileArtifactUploadDialog_ScreenshotFilenameRequired);
                    return;
                }
                if (!filename.toLowerCase().match('\.' + this._fileExtension + '$'))
                {
                    globalFunctions.display_error_message($get('<%=msgUploadMessage.ClientID%>'), resx.FileArtifactUploadDialog_ScreenshotFilenameWrongExtension);
                    return;
                }
                //Get the image from the stored data
                if (!this._encodedData)
                {
                    globalFunctions.display_error_message($get('<%=msgUploadMessage.ClientID%>'), resx.FileArtifactUploadDialog_SupaPasteImageFirst);
                    return;
                }
            }
            if (radFile.checked)
            {
                //See if we have a dropped file
                if (this._droppedFile)
                {
                    file = this._droppedFile;
                }
                else
                {
                    //Get a handle to the upload box
                    var files = $get('filAttachment').files;
                    if (files && files.length > 0)
                    {
                        file = files[0];
                    }
                }

                //Make sure we have file data present
                if (!file)
                {
                    alert(resx.FileArtifactUploadDialog_FilenameRequired);
                    return;
                }
                if (file.size <= 0)
                {
                    alert(resx.FileUpload_AttachmentEmpty);
                    return;
                }
                if (file.size > SpiraContext.MaxAllowedContentLength)
                {
                    alert(resx.FileUpload_AttachmentTooLarge.replace('{0}', (SpiraContext.MaxAllowedContentLength/1024)));
                    return;
                }
            }
            if (radUrl.checked)
            {
                //Get the URL from the form
                var txtURL = $get('<%=txtURL.ClientID%>');
                url = txtURL.value;
            }
            var description = $get('<%=txtAttachmentDescription.ClientID%>').value;
            var tags = $get('<%=txtTags.ClientID%>').value;
            var typeId = null;
            var ddlDocType = $find('<%=ddlDocType.ClientID%>');
            if (ddlDocType && ddlDocType.get_selectedItem && ddlDocType.get_selectedItem().get_value() != '')
            {
                typeId = parseInt(ddlDocType.get_selectedItem().get_value());
            } 

            var folderId = null;
            var ddlDocFolder = $find('<%=ddlDocFolder.ClientID%>');
            if (ddlDocFolder && ddlDocFolder.get_selectedItem && ddlDocFolder.get_selectedItem().get_value() != '')
            {
                folderId = parseInt(ddlDocFolder.get_selectedItem().get_value());
            } 

            //Call the webservice and upload the file or url
            globalFunctions.display_info_message($get('<%=msgUploadMessage.ClientID%>'), resx.Global_Uploading);
            if (radUrl.checked)
            {
                this.uploadUrl(url, description, '', tags, typeId, folderId);
            }
            if (radScreenshot.checked)
            {
                this.uploadScreenshot(filename, description, '', tags, typeId, folderId);
            }
            if (radFile.checked && file)
            {
                var reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = Function.createCallback(this.uploadFile, { thisRef: this, filename: file.name, description: description, version: '', tags: tags, typeId: typeId, folderId: folderId});
            }
        },
        uploadScreenshot: function(filename, description, version, tags, typeId, folderId)
        {
            var context = {};
            Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.UploadFile(this._projectId, filename, description, this._userId, this._encodedData, this._artifactId, this._artifactTypeId, version, tags, typeId, folderId, Function.createDelegate(this, this.upload_success), Function.createDelegate(this, this.upload_failure), context);
        },
        uploadFile: function(evt, args)
        {
            var result = evt.target.result;
            var context = {};
            Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.UploadFile(args.thisRef._projectId, args.filename, args.description, args.thisRef._userId, result, args.thisRef._artifactId, args.thisRef._artifactTypeId, args.version, args.tags, args.typeId, args.folderId, Function.createDelegate(args.thisRef, args.thisRef.upload_success), Function.createDelegate(args.thisRef, args.thisRef.upload_failure), context);
        },
        uploadUrl: function(url, description, version, tags, typeId, folderId)
        {
            var context = {};
            Inflectra.SpiraTest.Web.Services.Ajax.DocumentsService.UploadUrl(this._projectId, url, description, this._userId, this._artifactId, this._artifactTypeId, version, tags, typeId, folderId, Function.createDelegate(this, this.upload_success), Function.createDelegate(this, this.upload_failure), context);
        },
        upload_success: function (attachmentId, context)
        {
            globalFunctions.clear_errors($get('<%=msgUploadMessage.ClientID%>'));
                    
            //Now we need to close the dialog box and refresh the datagrid
            $find(this._dlgUploadDocument_id).close();
            $find(this._grdAttachmentList_id).load_data();
        },
        upload_failure: function (exception, context)
        {
            globalFunctions.display_error($get('<%=msgUploadMessage.ClientID%>'), exception);
        }
    }
    var tstucAttachmentPanel = $create(Inflectra.SpiraTest.Web.UserControls.AttachmentPanel);
    tstucAttachmentPanel.initialize();

    //Register event handlers
    $(document).ready(function() {
		$('#<%=lnkRemove.ClientID%>').on("click", function() {
            grdAttachmentList_removeFromArtifact();
        });

        //Hide any drag-and-drop mentions on mobile
        if (SpiraContext.IsMobile)
        {
            $('.mobile-hide').hide();
        }

        // because we allow users to click on the surrounding container (file-upload-container) we need to stop the events from clickin the label and the input itself from bubbling up to that container
        // if we do not then clicking the label registers as a click on the container which as below then in code clicks the input again which causes errors on the page
		$('#filAttachment').on("click", function (event) {
            event.stopPropagation();
        });
		$('#lblFilAttachment').on("click", function (event) {
            event.stopPropagation();
        });

        $('#file-upload-container')
			.on("click", function () {
                $('#filAttachment')[0].click();
            })
            .on('drag dragstart dragend dragover dragenter dragleave drop', function (e) {
                // preventing the unwanted behaviours
                e.preventDefault();
                e.stopPropagation();
            })
            .on('dragover dragenter', function ()
            {
                $('#file-upload-container').addClass('file-upload-is-dragover');
            })
            .on('dragleave dragend drop', function () {
                $('#file-upload-container').removeClass('file-upload-is-dragover');
            })
            .on('drop', function (e) {
                droppedFiles = e.originalEvent.dataTransfer.files; // the files that were dropped
                if (droppedFiles && droppedFiles.length > 0)
                {
                    $('#file-upload-filename').text(droppedFiles[0].name);
                    $('.file-upload-inner').hide();
                    tstucAttachmentPanel.set_droppedFile(droppedFiles[0]);
                }
            });

		$('#filAttachment').on("change", function () {
            var files = $('#filAttachment')[0].files;
            if (files && files.length > 0)
            {
                $('#file-upload-filename').text(files[0].name);
                $('.file-upload-inner').hide();
                tstucAttachmentPanel.set_droppedFile(null);
            }
        });
    });

    //Global handler for screenshot pasting
    function ajxScreenshotCapture_imagePaste(data)
    {
        tstucAttachmentPanel.pasteScreenshot(data);
    }

    //Handlers
    function btnUpload_click()
    {
        tstucAttachmentPanel.upload();
    }
    function radFile_click()
    {
        tstucAttachmentPanel.show_attachment_type_file();
    }
    function radUrl_click()
    {
        tstucAttachmentPanel.show_attachment_type_url();
    }
    function radScreenshot_click()
    {
        tstucAttachmentPanel.show_attachment_type_screenshot();
    }

    function grdAttachmentList_removeFromArtifact()
    {
        var grdAttachmentList = $find('<%=grdAttachmentList.ClientID%>');
        grdAttachmentList.custom_list_operation('RemoveFromArtifact:' + SpiraContext.ArtifactTypeId, SpiraContext.ArtifactId);
    }
    function grdAttachmentList_viewItem(target)
    {
        var grdAttachmentList = $find('<%=grdAttachmentList.ClientID%>');
        var items = grdAttachmentList.get_selected_items();
        if (items.length == 1)
        {
            var url = '<%=DocumentDetailsUrl%>';
            url = url.replace(globalFunctions.artifactIdToken, items[0]);
            if (target)
            {
                window.open(url, target);
            }
            else
            {
                window.open(url);
            }
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForOpen);
        }
    }
</script>
