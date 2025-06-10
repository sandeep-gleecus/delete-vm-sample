<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TaskList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.TaskList" %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdTasks" EnableViewState="false" Runat="server" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService">
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" ItemStyle-CssClass="Icon" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="w4 h4 priority1">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-Task.svg" AlternateText="Task" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name" NameMaxLength="30" CommandArgumentField="TaskId" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
		<tstsc:BoundFieldEx 
            DataField="ProjectName" 
            HeaderText="<%$Resources:Fields,Project %>" 
            MaxLength="20" 
            HtmlEncode="false" 
            HeaderStyle-CssClass="priority2" 
            ItemStyle-CssClass="priority2" 
            />
		<tstsc:BoundFieldEx DataField="ReleaseVersionNumber" HeaderText="<%$Resources:Fields,Release %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Progress %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <tstsc:Equalizer ID="eqlProgress" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:BoundFieldEx 
            DataField="TaskPriorityName" 
            LocalizedBase="Task_Priority_" 
            HeaderText="<%$Resources:Fields,Priority %>" 
            HeaderStyle-CssClass="priority1" 
            ItemStyle-CssClass="priority1"
        />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,DueDate %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx Runat="server" ID="lblEndDate"/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
