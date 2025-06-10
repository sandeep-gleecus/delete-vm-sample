<%@ Control Language="c#" AutoEventWireup="true" CodeBehind="AssociationsPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.AssociationsPanel" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>

<tstsc:MessageBox id="divMessage" runat="server" SkinID="MessageBox" />
<div runat="server" id="addPanelId"></div>

<div class="TabControlHeader">
    <div class="btn-group priority1">
		<tstsc:HyperLinkEx 
            ID="lnkAdd" 
            SkinID="ButtonDefault" 
            runat="server" 
            NavigateUrl="javascript:void(0)" >
            <span class="fas fa-plus"></span>
            <asp:Localize Text="<%$Resources:Buttons,Add %>" runat="server" />
		</tstsc:HyperLinkEx>
		<tstsc:HyperLinkEx ID="lnkDelete" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdAssociationLinks" ClientScriptMethod="delete_items()" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,ArtifactLinkPanel_DeleteConfirm %>">
            <span class="fas fa-trash-alt"></span>
            <asp:Localize Text="<%$Resources:Buttons,Remove %>" runat="server" />
		</tstsc:HyperLinkEx>
		<tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdAssociationLinks" ClientScriptMethod="load_data()">
            <span class="fas fa-sync"></span>
            <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
		</tstsc:HyperLinkEx>
	</div>
    <div class="btn-group priority3">
        <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			ClientScriptServerControlId="grdAssociationLinks" ClientScriptMethod="apply_filters()">
			<DropMenuItems>
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			</DropMenuItems>
		</tstsc:DropMenu>
    </div>
</div>

<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,ArtifactLinkPanel_Associations %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>
<tstsc:SortedGrid 
    Authorized_Permission="Modify" 
    CssClass="DataGrid DataGrid-no-bands" 
    DisplayAttachments="false" 
    EditRowCssClass="Editing"
    ErrorMessageControlId="divMessage"
    FilterInfoControlId="lblFilterInfo"
    HeaderCssClass="Header"
    ID="grdAssociationLinks" 
	RowCssClass="Normal" 
	runat="server" 
    SelectedRowCssClass="Highlighted" 
	SubHeaderCssClass="SubHeader" 
    TotalCountControlId="lblTotalCount" 
    VisibleCountControlId="lblVisibleCount" 
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.AssociationService" 
    />

<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
	<asp:ServiceReference Path="~/Services/Ajax/AssociationService.svc" />  
	<asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
	<asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />  
	<asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />  
	<asp:ServiceReference Path="~/Services/Ajax/TestStepService.svc" />  
	<asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.<%=this.ClientID%> = function ()
    {
        this._userId = <%=UserId%>;
        this._projectId = <%=ProjectId%>;
        this._displayTypeId = <%=(int)this.DisplayTypeEnum%>;
        this._artifactId = SpiraContext.ArtifactId;
        this._artifactTypeId = SpiraContext.ArtifactTypeId;
        this._addPanelIsOpen = false;

        //set SpiraContext variables if necessary
        $(document).ready(function() {
            if (!SpiraContext.ArtifactId) SpiraContext.ArtifactId =<%= this.ArtifactId %>;
            if (!SpiraContext.ArtifactTypeId) SpiraContext.ArtifactTypeId = <%= (int)this.ArtifactTypeEnum %>;
        });

        Inflectra.SpiraTest.Web.UserControls.<%=this.ClientID%>.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.<%=this.ClientID%>.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.<%=this.ClientID%>.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.<%=this.ClientID%>.callBaseMethod(this, 'dispose');
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
        get_addPanelIsOpen: function()
        {
            return this._addPanelIsOpen;
        },
        set_addPanelIsOpen: function(value)
        {
            this._addPanelIsOpen = value;
        },

        /* Functions */
        check_hasData: function(callback)
        {
            //See if we have data
            var artifactReference = {
                artifactId: SpiraContext.ArtifactId,
                artifactTypeId: SpiraContext.ArtifactTypeId,
            };
            Inflectra.SpiraTest.Web.Services.Ajax.AssociationService.Association_Count(
                this._projectId, 
                artifactReference, 
                this._displayTypeId, 
                Function.createDelegate(this, this.check_hasData_success), 
                this.check_hasData_failure, 
                callback
                );
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

        load_data: function(filters, loadNow)
        {
            var isGridEditable = <%=IsGridEditable.ToString().ToLowerInvariant()%>;

            //Set the permissions on the Add/Remove Associations buttons (limited Modify is OK if we own the item)
            var isAuthorizedToModify = false;
            var authorizedState = globalFunctions.isAuthorized(globalFunctions.permissionEnum.Modify, SpiraContext.ArtifactTypeId);
            if (authorizedState != globalFunctions.authorizationStateEnum.prohibited)
            {
                var isCreatorOrOwner = SpiraContext.IsArtifactCreatorOrOwner;
                if (authorizedState == globalFunctions.authorizationStateEnum.authorized || isCreatorOrOwner)
                {
                    isAuthorizedToModify = true;
                }
            }
            if (isAuthorizedToModify)
            {
                $('#<%=lnkAdd.ClientID%>').removeClass('disabled');
                $('#<%=lnkDelete.ClientID%>').removeClass('disabled');
            }
            else
            {
                $('#<%=lnkAdd.ClientID%>').addClass('disabled');
                $('#<%=lnkDelete.ClientID%>').addClass('disabled');
            }

            //Also specify if the grid allows modification
            var grdAssociationLinks = $find('<%=grdAssociationLinks.ClientID%>');
            grdAssociationLinks.set_allowEdit(isAuthorizedToModify && isGridEditable);

            //Load the data
            grdAssociationLinks.set_standardFilters(filters);
            if (loadNow)
            {
                grdAssociationLinks.load_data();
            }
            else
            {
                grdAssociationLinks.clear_loadingComplete();
            }
        },
        //close the add association panel
        closeAddAssocationPanel: function () 
        {
            //first check to see if the panel is actually open
            if (this.get_addPanelIsOpen())
            {
                var domId = panelAssociationAdd.addPanelId;
                $("#" + '<%=lnkAdd.ClientID%>').removeClass('disabled');
                //only unmount the panel if it is currently mounted
                if (typeof SpiraContext.uiState[domId] != "undefined" && SpiraContext.uiState[domId].isMounted) {
                    SpiraContext.uiState[domId].isMounted = false;
                    ReactDOM.unmountComponentAtNode(document.getElementById(domId));
                }
            }
        }
    }
    var tstuc_<%=this.ClientID%> = $create(Inflectra.SpiraTest.Web.UserControls.<%=this.ClientID%>);

    //adding a new association
	$('#<%=lnkAdd.ClientID%>').on("click", function () {
        //populate general data into the global panelAssociationAdd object, so it is accessible by React on render
        panelAssociationAdd.lnkAddBtnId = '<%=lnkAdd.ClientID%>';
        panelAssociationAdd.addPanelId = '<%=addPanelId.ClientID%>';
        panelAssociationAdd.addPanelObj = tstuc_<%=this.ClientID%>;
        panelAssociationAdd.displayType = $('#<%=addPanelId.ClientID%>').data("display");
        panelAssociationAdd.sortedGridId = '<%=grdAssociationLinks.ClientID%>';
        panelAssociationAdd.messageBox = '<%=divMessage.ClientID%>';
        panelAssociationAdd.listOfViewableArtifactTypeIds = "<%= GetListOfViewableArtifactTypeIds() %>";
        
        //now render the panel (and set open flag)
        panelAssociationAdd.showPanel();
        tstuc_<%=this.ClientID%>.set_addPanelIsOpen(true);
    });

</script>
