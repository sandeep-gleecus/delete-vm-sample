<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DataSynchronization.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.DataSynchronization" %>
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
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,LastSyncDate %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			<ItemTemplate>
				<%# (!((DataSyncSystem) Container.DataItem).LastSyncDate.HasValue) ? "-" : String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate((DateTime)(((DataSyncSystem) Container.DataItem) ["LastSyncDate"])))%>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:BoundFieldEx DataField="DataSyncStatusName" HeaderText="<%$Resources:Fields,Status %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1"/>
	</Columns>													
</tstsc:GridViewEx>
