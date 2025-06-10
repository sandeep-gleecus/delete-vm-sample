<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReleaseTaskProgress.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ReleaseTaskProgress" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx ID="grdReleaseTaskProgress" Runat="server" EnableViewState="false" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService">
	<Columns>
		<tstsc:TemplateFieldEx>
			<HeaderTemplate>
				<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,Release %>" /> / <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,Iteration %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <%# GlobalFunctions.DisplayIndent(((ReleaseView)Container.DataItem).IndentLevel)%>
			    <tstsc:ImageEx CssClass="w4 h4" ID="imgIcon" Runat="server" ImageUrl='<%# GlobalFunctions.DisplayReleaseIcon (((ReleaseView) Container.DataItem).ReleaseTypeId) %>' />
			    <tstsc:LinkButtonEx SkinID="ButtonLink" Runat="server" Text='<%#: ((ReleaseView) Container.DataItem).VersionNumber + " - " + GlobalFunctions.TruncateName(((ReleaseView) Container.DataItem).Name)%>' ID="btnSelectRelease" CommandName="SelectRelease" CommandArgument='<%# ((ReleaseView) Container.DataItem).ReleaseId%>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Tasks %>" ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" >
			<ItemTemplate>
				<tstsc:HyperLinkEx Text='<%# ((ReleaseView) Container.DataItem).TaskCount%>' ID="lnkTaskCount" runat="server" ToolTip="<%$Resources:Main,Global_ViewReleaseDetails %>" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Est %>" ItemStyle-HorizontalAlign="Left" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" >
			<ItemTemplate>
				<%# (((ReleaseView)Container.DataItem).TaskEstimatedEffort.HasValue) ? String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS, (decimal)(((ReleaseView)Container.DataItem).TaskEstimatedEffort.Value) / (decimal)60) : "-"%>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Proj %>" ItemStyle-HorizontalAlign="Left" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" >
			<ItemTemplate>
				<%# (((ReleaseView)Container.DataItem).TaskProjectedEffort.HasValue) ? String.Format(GlobalFunctions.FORMAT_TIME_INTERVAL_HOURS, (decimal)(((ReleaseView)Container.DataItem).TaskProjectedEffort.Value) / (decimal)60): "-"%>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Progress %>">
			<ItemTemplate>
			    <tstsc:Equalizer runat="server" ID="eqlTaskProgress" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
