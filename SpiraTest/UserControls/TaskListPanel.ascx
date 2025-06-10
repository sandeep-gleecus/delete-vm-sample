<%@ Control Language="c#" AutoEventWireup="True" Codebehind="TaskListPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.TaskListPanel" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
<div class="TabControlHeader">
    <div class="btn-group priority3">
        <tstsc:HyperLinkEx 
            ID="lnkNewTask" 
            SkinID="ButtonDefault" 
            runat="server" 
            Authorized_ArtifactType="Task"
            Authorized_Permission="Create" 
            NavigateUrl="javascript:void(0)" 
            ClientScriptServerControlId="grdTaskList"
            >
            <span class="fas fa-plus"></span>
            <asp:Localize Text="<%$Resources:Dialogs,TaskList_NewTask %>" runat="server" />
        </tstsc:HyperLinkEx>
        <tstsc:DropMenu ID="lnkRemoveTask"
            runat="server"
            Authorized_ArtifactType="Task"
            Authorized_Permission="Modify"
			GlyphIconCssClass="mr3 fas fa-times"
            Text="<%$Resources:Buttons,Remove %>"
            ClientScriptServerControlId="grdTaskList">
            <DropMenuItems>
                <tstsc:DropMenuItem Name="Remove" Value="<%$Resources:Buttons,Remove %>" GlyphIconCssClass="mr3 fas fa-times" Authorized_Permission="Modify" Authorized_ArtifactType="Task" />
                <tstsc:DropMenuItem Name="Delete" Value="<%$Resources:Buttons,Delete %>" GlyphIconCssClass="mr3 fas fa-trash-alt" ClientScriptMethod="delete_items()" Authorized_Permission="Delete" Authorized_ArtifactType="Task" Confirmation="True" ConfirmationMessage="<%$Resources:Messages,TaskList_DeleteConfirm %>" />
            </DropMenuItems>
        </tstsc:DropMenu>
        <tstsc:HyperLinkEx ID="lnkRefreshTasks" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="load_data()">
            <span class="fas fa-sync"></span>
            <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
        </tstsc:HyperLinkEx>
    </div>
    <div class="btn-group priority3">
        <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			ClientScriptServerControlId="grdTaskList" ClientScriptMethod="apply_filters()">
			<DropMenuItems>
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			</DropMenuItems>
		</tstsc:DropMenu>
        <tstsc:HyperLinkEx ID="lnkCloneTask" SkinID="ButtonDefault" runat="server" Authorized_ArtifactType="Task" Authorized_Permission="Create" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="clone_items()">
            <span class="fas fa-copy"></span>
            <asp:Localize Text="<%$Resources:Buttons,Clone %>" runat="server" />
        </tstsc:HyperLinkEx>
    </div>
    <div class="btn-group">
        <tstsc:DropDownListEx ID="ddlShowHideTaskColumns" Runat="server" DataValueField="Key" DataTextField="Value" CssClass="DropDownList" AutoPostBack="false" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowHideColumns %>" Width="180px" ClientScriptServerControlId="grdTaskList" ClientScriptMethod="toggle_visibility" />
    </div>
    <asp:PlaceHolder runat="server" ID="plcEffortLegend" Visible="true">
        <div class="legend-group">
            <span class="Legend">
                <asp:Localize runat="server" Text="<%$Resources:Fields,EstimatedEffort %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblTaskEstimatedEffort" Runat="server" />
                </span>
            </span>&nbsp;/
            <span class="Legend">
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,ProjectedEffort %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblTaskProjectedEffort" Runat="server" />
                </span>
            </span>
        </div>
    </asp:PlaceHolder>
</div>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,TaskListPanel_Tasks %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>

<tstsc:SortedGrid ID="grdTaskList" CssClass="DataGrid" HeaderCssClass="Header"
    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
    RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-Task.svg" AlternateItemImage="artifact-PullRequest.svg" ConcurrencyEnabled="true"
    runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService" AutoLoad="false"
    Authorized_ArtifactType="Task" Authorized_Permission="BulkEdit" AllowColumnPositioning="true">
    <ContextMenuItems>
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-mouse-pointer" Caption="<%$Resources:Buttons,OpenItem %>" CommandName="open_item" CommandArgument="_self" Authorized_ArtifactType="Task" Authorized_Permission="View" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-external-link-alt" Caption="<%$Resources:Buttons,OpenItemNewTab %>" CommandName="open_item" CommandArgument="_blank" Authorized_ArtifactType="Task" Authorized_Permission="View" />
        <tstsc:ContextMenuItem Divider="True" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Dialogs,TaskList_NewTask%>" CommandName="insert_item" CommandArgument="Task" Authorized_ArtifactType="Task" Authorized_Permission="Create" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-copy" Caption="<%$Resources:Dialogs,TaskList_CloneTask%>" CommandName="clone_items" Authorized_ArtifactType="Task" Authorized_Permission="Create" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Task" Authorized_Permission="BulkEdit" />
    </ContextMenuItems>    
</tstsc:SortedGrid>
<br />
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.TaskListPanel = function ()
    {
        this._projectId = <%=ProjectId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;

        Inflectra.SpiraTest.Web.UserControls.TaskListPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.TaskListPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.TaskListPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.TaskListPanel.callBaseMethod(this, 'dispose');
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

        /* Functions */
        check_hasData: function(callback)
        {
            // first check if user can view tasks
            var authorizedView = globalFunctions.isAuthorized(globalFunctions.permissionEnum.View, globalFunctions.artifactTypeEnum.task);
            var supportedByLicense = globalFunctions.isSupportedByLicense(SpiraContext.ProductType, globalFunctions.artifactTypeEnum.task)

            if (authorizedView != globalFunctions.authorizationStateEnum.prohibited && supportedByLicense) {
                //See if we have data
                var artifactReference = {
                    artifactId: this._artifactId,
                    artifactTypeId: this._artifactTypeId,
                };
                Inflectra.SpiraTest.Web.Services.Ajax.TasksService.Task_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
            }
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
            var grdTaskList = $find('<%=grdTaskList.ClientID%>');
            grdTaskList.set_standardFilters(filters);
            if (loadNow)
            {
                grdTaskList.load_data();
            }
            else
            {
                grdTaskList.clear_loadingComplete();
            }
        }
    }
    var tstucTaskListPanel = $create(Inflectra.SpiraTest.Web.UserControls.TaskListPanel);

    function grdTaskList_loaded()
    {
        //Find the form manager and call the load_data method
        var ajxLookupManager = $find('<%=this.FormManager.ClientID%>');
        ajxLookupManager.load_data();
    }
</script>
