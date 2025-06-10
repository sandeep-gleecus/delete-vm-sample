<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ReleaseTestSummary.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.ReleaseTestSummary" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx ID="grdReleaseTestSummary" Runat="server" EnableViewState="false" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.ReleasesService">
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
		<tstsc:TemplateFieldEx HeaderText='<%$Resources:Main,Global_NumTests%>' ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			<ItemTemplate>
				<tstsc:HyperLinkEx ID="lnkTestCaseCount" runat="server" ToolTip="<%$Resources:Main,Global_ViewReleaseDetails %>" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ExecutionStatus %>">
			<ItemTemplate>
			    <tstsc:Equalizer runat="server" ID="eqlExecutionStatus" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/ReleasesService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
