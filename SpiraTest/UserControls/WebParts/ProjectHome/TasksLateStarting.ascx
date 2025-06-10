<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TasksLateStarting.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.TasksLateStarting" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Microsoft.Security.Application" %>
<tstsc:GridViewEx id="grdLateStartingTasks" Runat="server" EnableViewState="false" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService">
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:ImageEx ID="imgTask" CssClass="w4 h4" ImageUrl="Images/artifact-Task.svg" AlternateText="Task" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name" NameMaxLength="40" CommandArgumentField="TaskId" />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="True" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Owner %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label Runat="server" Text='<%# String.IsNullOrEmpty(((TaskView) Container.DataItem).OwnerName) ? "-" : Microsoft.Security.Application.Encoder.HtmlEncode(((TaskView) Container.DataItem).OwnerName) %>' ID="Label9" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Priority %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <asp:Label ID="Label1" Runat="server" Text='<%#: ((TaskView) Container.DataItem).TaskPriorityName %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,StartDate %>"/>
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx Runat="server" ID="lblStartDate"/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
