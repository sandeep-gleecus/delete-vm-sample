<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RequirementIncidentCount.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RequirementIncidentCount" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdRequirementIncidentCount" Runat="server" EnableViewState="false" SkinID="WidgetGrid" ShowFooter="True" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService">
	<FooterStyle CssClass="Footer" />
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" FooterColumnSpan="2" ItemStyle-CssClass="Icon">
			<HeaderTemplate>
				<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,RequirementName %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <tstsc:ImageEx CssClass="w4 h4" ID="imgReqIcon" Runat="server" ImageUrl='<%# GlobalFunctions.DisplayRequirementIcon (((RequirementIncidentCount) Container.DataItem).IsSummary) %>' />
			</ItemTemplate>
			<FooterTemplate>
				&nbsp;&nbsp;&nbsp;<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,TotalCaps %>" />
			</FooterTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" FooterColumnSpan="-1" DataField="Name" NameMaxLength="60" CommandArgumentField="RequirementId" />
		<tstsc:TemplateFieldEx FooterField="IncidentOpenCount" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" FooterStyle-CssClass="priority2">
			<ItemStyle HorizontalAlign="Center" />
			<HeaderStyle HorizontalAlign="Center" Wrap="False" />
			<FooterStyle HorizontalAlign="Center" />
			<HeaderTemplate>
				# <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,Open %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="Label4" Runat="server" Text='<%# ((RequirementIncidentCount) Container.DataItem).IncidentOpenCount %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx FooterField="IncidentTotalCount">
			<ItemStyle HorizontalAlign="Center" />
			<HeaderStyle HorizontalAlign="Center" Wrap="False" />
			<FooterStyle HorizontalAlign="Center" />
			<HeaderTemplate>
				# <asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,Total %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="Label5" Runat="server" Text='<%# ((RequirementIncidentCount) Container.DataItem).IncidentTotalCount %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>