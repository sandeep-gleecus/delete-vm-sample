<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DetectedIncidents.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.DetectedIncidents" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx id="grdDetectedIncidents" EnableViewState="false" Runat="server" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService">
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderStyle-CssClass="priority1" ControlStyle-CssClass="priority1 w4 h4">
			<HeaderTemplate>
				<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx ImageUrl="Images/artifact-Incident.svg" AlternateText="Incident" ID="imgIcon" runat="server" />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" DataField="Name" NameMaxLength="40" CommandArgumentField="IncidentId"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
		<tstsc:BoundFieldEx ItemStyle-Wrap="True" DataField="ProjectName" HeaderText="<%$Resources:Fields,Project %>" MaxLength="20" HtmlEncode="true" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"  />
		<tstsc:BoundFieldEx DataField="IncidentStatusName" HeaderText="<%$Resources:Fields,Status %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" />
		<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="IncidentTypeName" HeaderText="<%$Resources:Fields,Type %>" HtmlEncode="true" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3"  />
		<tstsc:BoundFieldEx ItemStyle-Wrap="False" DataField="PriorityName" HeaderText="<%$Resources:Fields,Priority %>" HtmlEncode="true" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2" />
		<tstsc:TemplateFieldEx ItemStyle-Wrap="False" HeaderStyle-Wrap="False" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" >
			<HeaderTemplate>
                <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Fields,LastUpdateDate %>" />
                <span class="fas fa-chevron-down"></span>
 			</HeaderTemplate>
			<ItemTemplate>
				<tstsc:LabelEx Runat="server" Text='<%# String.Format(GlobalFunctions.FORMAT_DATE,  GlobalFunctions.LocalizeDate(((IncidentView) Container.DataItem).LastUpdateDate)) %>'
                    Tooltip='<%# String.Format(GlobalFunctions.FORMAT_DATE_TIME,  GlobalFunctions.LocalizeDate(((IncidentView) Container.DataItem).LastUpdateDate)) %>' ID="lblCreationDate"/>
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
