<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AssignedRisks.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.AssignedRisks" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdOwnedRisks" Runat="server" EnableViewState="false" SkinId="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RisksService">
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-Risk.svg" AlternateText="Risk" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name" NameMaxLength="40" CommandArgumentField="RiskId" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
		<tstsc:BoundFieldEx DataField="ProjectName" HeaderText="<%$Resources:Fields,Project %>" MaxLength="20" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" />
		<tstsc:BoundFieldEx DataField="RiskStatusName" HeaderText="<%$Resources:Fields,Status %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="RiskTypeName" HeaderText="<%$Resources:Fields,Type %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="RiskExposure" HeaderText="<%$Resources:Fields,RiskExposure %>" HtmlEncode="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
			<HeaderTemplate>
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,ReviewDate %>" />
			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx Runat="server" Text='<%# String.Format(GlobalFunctions.FORMAT_DATE,  GlobalFunctions.LocalizeDate(((RiskView) Container.DataItem).ReviewDate)) %>' Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME,  GlobalFunctions.LocalizeDate(((RiskView) Container.DataItem).ReviewDate)) %>' ID="lblReviewDate"/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns> 
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/RisksService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
