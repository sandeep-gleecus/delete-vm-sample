<%@ Control Language="c#" AutoEventWireup="True" Codebehind="RequirementTaskPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
<div class="TabControlHeader">
    <div class="btn-group priority3">
        <tstsc:HyperLinkEx ID="lnkNewTask" SkinID="ButtonDefault" runat="server" Authorized_ArtifactType="Task" Authorized_Permission="Create" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="insert_item('Task')">
            <span class="fas fa-plus"></span>
            <asp:Localize Text="<%$Resources:Dialogs,TaskList_InsertTask %>" runat="server" />
        </tstsc:HyperLinkEx>
        <tstsc:HyperLinkEx ID="lnkDeleteTask" SkinID="ButtonDefault" runat="server" Authorized_ArtifactType="Task" Authorized_Permission="Delete" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdReqTaskList" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,TaskList_DeleteConfirm %>" ClientScriptMethod="delete_items()">
            <span class="fas fa-trash-alt"></span>
            <asp:Localize Text="<%$Resources:Buttons,Delete %>" runat="server" />
        </tstsc:HyperLinkEx>
        <tstsc:HyperLinkEx ID="lnkRefreshTasks" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="load_data()">
            <span class="fas fa-sync"></span>
            <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
        </tstsc:HyperLinkEx>
    </div>

    <div class="btn-group priority3">
        <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
			ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="apply_filters()">
			<DropMenuItems>
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
				<tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
			</DropMenuItems>
		</tstsc:DropMenu>
    </div>
    <div class="btn-group priority1">
        <tstsc:DropDownListEx ID="ddlShowLevel" Runat="server" NoValueItem="True" NoValueItemText="<%$Resources:Dialogs,Global_ShowLevel %>" AutoPostBack="False" DataTextField="Value" DataValueField="Key" CssClass="DropDownList" ClientScriptServerControlId="grdReqTaskList" ClientScriptMethod="expand_to_level">
            <asp:ListItem Value="1" Text="<%$Resources:Dialogs,Global_ExpandAll %>" />
            <asp:ListItem Value="2" Text="<%$Resources:Dialogs,Global_CollapseAll %>" />
        </tstsc:DropDownListEx>
    </div>
    <asp:PlaceHolder runat="server" ID="plcHoursLegend" Visible="false">
        <div class="legend-group">
            <span class="Legend">
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,EstimatedEffort %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblEstimatedEffort" Runat="server" />
                </span>
            </span>&nbsp;
            <span class="Legend">
                <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,ActualEffort %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblActualEffort" Runat="server" />
                </span>
            </span>&nbsp;
            <span class="Legend">
                <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,ProjectedEffort %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblProjectedEffort" Runat="server" />
                </span>
            </span>
            <span class="Legend">
                <asp:Localize ID="Localize7" runat="server" Text="<%$Resources:Fields,AvailableEffort %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblAvailableEffort2" Runat="server" />
                </span>
            </span>
        </div>
    </asp:PlaceHolder>
    <asp:PlaceHolder runat="server" ID="plcPointsLegend" Visible="false">
        <div class="legend-group">
            <span class="Legend">
                <asp:Localize ID="Localize8" runat="server" Text="<%$Resources:Fields,PlannedPoints %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblPlannedPoints" Runat="server" />
                </span>
            </span>&nbsp;
            <span class="Legend">
                <asp:Localize ID="Localize9" runat="server" Text="<%$Resources:Fields,RequirementPoints %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblRequirementPoints" Runat="server" />
                </span>
            </span>&nbsp;
            <span class="Legend">
                <asp:Localize ID="Localize10" runat="server" Text="<%$Resources:Fields,AvailablePoints %>" />
                <span class="badge">
                    <tstsc:LabelEx id="lblAvailablePoints" Runat="server" />
                </span>
            </span>
        </div>
    </asp:PlaceHolder>
</div>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize6" runat="server" Text="<%$Resources:Main,RequirementTaskPanel_Items %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>
<tstsc:HierarchicalGrid ID="grdReqTaskList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
	SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
	RowCssClass="Normal" EditRowCssClass="Editing" ItemImage="artifact-Task.svg" SummaryItemImage="artifact-Requirement.svg"
	runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsTaskService"
	Authorized_ArtifactType="Requirement" Authorized_Permission="BulkEdit" ConcurrencyEnabled="true"
    AutoLoad="false" ForceTwoLevelIndent="true">
    <ContextMenuItems>
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 fas fa-plus" Caption="<%$Resources:Dialogs,TaskList_InsertTask %>" CommandName="insert_item" CommandArgument="Task" Authorized_ArtifactType="Task" Authorized_Permission="Create" />
        <tstsc:ContextMenuItem GlyphIconCssClass="mr3 far fa-edit" Caption="<%$Resources:Buttons,EditItems%>" CommandName="edit_items" Authorized_ArtifactType="Task" Authorized_Permission="BulkEdit" />
    </ContextMenuItems>     
</tstsc:HierarchicalGrid>
<br />
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/RequirementsTaskService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel = function ()
    {
        this._projectId = <%=ProjectId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;

        Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel.callBaseMethod(this, 'dispose');
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
            //See if we have data
            var artifactReference = {
                artifactId: this._artifactId,
                artifactTypeId: this._artifactTypeId,
            };
            Inflectra.SpiraTest.Web.Services.Ajax.RequirementsTaskService.RequirementsTask_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
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
            var grdReqTaskList = $find('<%=grdReqTaskList.ClientID%>');
            grdReqTaskList.set_standardFilters(filters);
            if (loadNow)
            {
                grdReqTaskList.load_data();
            }
            else
            {
                grdReqTaskList.clear_loadingComplete();
            }
        }
    }
    var tstucRequirementTaskPanel = $create(Inflectra.SpiraTest.Web.UserControls.RequirementTaskPanel);

    function grdReqTaskList_loaded()
    {
        //Find the form manager and call the load_data method
        var ajxFormManager = $find(ajxFormManager_id);
        ajxFormManager.load_data();
    }
</script>