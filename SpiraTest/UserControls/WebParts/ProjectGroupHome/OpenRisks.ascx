<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OpenRisks.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectGroupHome.OpenRisks" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdRiskList" Runat="server" EnableViewState="false" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService">
	<HeaderStyle CssClass="SubHeader" />
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,Description %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-Risk.svg" AlternateText="Risk" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" DataField="Name" NameMaxLength="60" CommandArgumentField="RiskId" NavigateUrlFormat="#" HeaderColumnSpan="-1" />
		<tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,Project%>" DataField="ProjectName"  />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Width="75px">
			<HeaderTemplate>
				<asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,RiskExposure %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="lblPriority" Runat="server" Text='<%#: (((RiskView) Container.DataItem).RiskExposure.HasValue) ? ((RiskView) Container.DataItem).RiskExposure.Value.ToString() : "-" %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Width="75px">
			<HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,RiskTypeId %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<asp:Label ID="lblSeverity" Runat="server" Text='<%#: ((RiskView) Container.DataItem).RiskTypeName %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderStyle-Width="100px"
            HeaderText="<%$Resources:Fields,OwnedBy %>" 
            HeaderStyle-CssClass="priority4" 
            ItemStyle-CssClass="priority4" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    ID="lblOwnedBy" 
                    Runat="server" 
                    Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((RiskView) Container.DataItem).OwnerName) %>'
                    />
            </ItemTemplate>
		</tstsc:TemplateFieldEx>
 		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderStyle-Width="90px" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" >
			<HeaderTemplate>
				<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,ReviewDate %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx ID="lblReviewDate" Runat="server" Text='<%# String.Format(GlobalFunctions.FORMAT_DATE, GlobalFunctions.LocalizeDate(((RiskView) Container.DataItem).ReviewDate)) %>'
                    Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((RiskView) Container.DataItem).ReviewDate)) %>' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/RisksService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
