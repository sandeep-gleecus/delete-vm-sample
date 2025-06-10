<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DataMapping.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.DataMapping" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<tstsc:GridViewEx ID="grdDataSynchronization" Runat="server" AutoGenerateColumns="False" CssClass="DataGrid" ShowHeader="true" ShowFooter="false" ShowSubHeader="false" Width="100%">
	<HeaderStyle CssClass="Header" />
	<Columns>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_DataSynchronization_PlugIn %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
		    <ItemTemplate>
                <h4>
    		        <tstsc:LabelEx ID="lblDataSyncName" Text='<%#: ((DataSyncSystem) Container.DataItem).DisplayName%>' ToolTip='<%# ((DataSyncSystem) Container.DataItem).Description%>' runat="server" />
                    <small>
                        <asp:Literal runat="server" Text="<%#: ((DataSyncSystem) Container.DataItem).Name%>" />
                    </small>
                </h4>
		    </ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
		    <ItemTemplate>
		        <tstsc:LabelEx ID="lblActive" runat="server" Text='<%#GlobalFunctions.DisplayYnFlag(((DataSyncSystem) Container.DataItem).IsActive) %>' />
		    </ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:BoundFieldEx DataField="DataSyncStatusName" HeaderText="<%$Resources:Fields,Status %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
	</Columns>													
</tstsc:GridViewEx>
