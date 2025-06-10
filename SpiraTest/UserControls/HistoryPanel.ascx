<%@ Control Language="c#" AutoEventWireup="True" Codebehind="HistoryPanel.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.HistoryPanel" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="tstsc" NameSpace="Inflectra.SpiraTest.Web.ServerControls" Assembly="Web" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:MessageBox id="divMessage" Runat="server" SkinID="MessageBox" />
<asp:Panel runat="server" ID="panelControl" class="TabControlHeader">
    <div class="btn-toolbar-mid-page">
        <div class="btn-group priority3">
            <tstsc:HyperLinkEx ID="lnkRefresh" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptServerControlId="grdHistoryList" ClientScriptMethod="load_data()">
                <span class="fas fa-sync"></span>
                <asp:Localize Text="<%$Resources:Buttons,Refresh %>" runat="server" />
            </tstsc:HyperLinkEx>
            <tstsc:DropMenu id="btnFilters" runat="server" GlyphIconCssClass="mr3 fas fa-filter" Text="<%$Resources:Buttons,Filter %>"
		        ClientScriptServerControlId="grdHistoryList" ClientScriptMethod="apply_filters()">
		        <DropMenuItems>
			        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Apply" Value="<%$Resources:Buttons,ApplyFilter %>" GlyphIconCssClass="mr3 fas fa-filter" ClientScriptMethod="apply_filters()" />
			        <tstsc:DropMenuItem SkinID="ButtonDefault" runat="server" Name="Clear" Value="<%$Resources:Buttons,ClearFilter %>" GlyphIconCssClass="mr3 fas fa-times" ClientScriptMethod="clear_filters()" />
		        </DropMenuItems>
	        </tstsc:DropMenu>        
            <tstsc:HyperLinkEx ID="lnkAdminView" SkinID="ButtonDefault" runat="server" Authorized_Permission="ProjectAdmin" ToolTip="<%$Resources:Main,HistoryPanel_AdminViewTooltip %>">
                <span class="fas fa-cog"></span>
                <asp:Localize Text="<%$Resources:Buttons,AdminView %>" runat="server" />
            </tstsc:HyperLinkEx>
        </div>
    </div>
</asp:Panel>
<div class="bg-near-white-hover py2 px3 br2 transition-all">
	<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_Displaying %>" />
    <asp:Label ID="lblVisibleCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize4" runat="server" Text="<%$Resources:Main,Global_OutOf %>" />
    <asp:Label ID="lblTotalCount" Runat="server" Font-Bold="True" />
	<asp:Localize ID="Localize5" runat="server" Text="<%$Resources:Main,HistoryPanel_Changes %>" />.
    <tstsc:LabelEx ID="lblFilterInfo" runat="server" />
</div>
<tstsc:SortedGrid ID="grdHistoryList" CssClass="DataGrid DataGrid-no-bands" HeaderCssClass="Header"
    VisibleCountControlId="lblVisibleCount" TotalCountControlId="lblTotalCount" FilterInfoControlId="lblFilterInfo"
    SubHeaderCssClass="SubHeader" SelectedRowCssClass="Highlighted" ErrorMessageControlId="divMessage"
    RowCssClass="Normal" DisplayAttachments="false" AllowEditing="false" DisplayCheckboxes="false"
    runat="server" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.HistoryService" />
<br />
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/HistoryService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>

<script type="text/javascript">
    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.HistoryPanel = function ()
    {
        this._userId = <%=UserId%>;
        this._projectId = <%=ProjectId%>;
        this._artifactId = <%=ArtifactId%>;
        this._artifactTypeId = <%=(int)this.ArtifactTypeEnum%>;

        Inflectra.SpiraTest.Web.UserControls.HistoryPanel.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.HistoryPanel.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.HistoryPanel.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.HistoryPanel.callBaseMethod(this, 'dispose');
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
            Inflectra.SpiraTest.Web.Services.Ajax.HistoryService.History_Count(this._projectId, artifactReference, Function.createDelegate(this, this.check_hasData_success), this.check_hasData_failure, callback);
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
			console.log("Error calling service: " + ex._message);
        },

        load_data: function(filters, loadNow)
        {
            var grdHistoryList = $find('<%=grdHistoryList.ClientID%>');
            grdHistoryList.set_standardFilters(filters);
            if (loadNow)
            {
                grdHistoryList.load_data();
            }
            else
            {
                grdHistoryList.clear_loadingComplete();
            }

            //Add a link to the administration view (to make changes)
            var url = '<%=AdminViewUrl%>';
            url = url.replace('{0}', SpiraContext.ProjectId).replace('{1}', SpiraContext.ArtifactTypeId).replace('{2}', SpiraContext.ArtifactId);
            var lnkAdminView = $get('<%=this.lnkAdminView.ClientID%>');
            if (lnkAdminView && lnkAdminView.getAttribute('aria-disabled') != 'true')
            {
                lnkAdminView.href = url;
            }
        }
    }
    var tstucHistoryPanel = $create(Inflectra.SpiraTest.Web.UserControls.HistoryPanel);

</script>