<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RecentBuilds.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectHome.RecentBuilds" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<tstsc:GridViewEx id="grdBuildList" Runat="server" EnableViewState="false" SkinID="WidgetGrid" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.BuildService">
	<FooterStyle CssClass="Footer" />
	<Columns>
		<tstsc:TemplateFieldEx HeaderColumnSpan="2" FooterColumnSpan="2" ItemStyle-CssClass="Icon">
			<HeaderTemplate>
				<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Fields,Name %>" />
			</HeaderTemplate>
			<ItemTemplate>
                <tstsc:ImageEx 
                    CssClass="w4 h4"
                    ID="imgReqIcon" 
                    Runat="server" 
                    ImageUrl="Images/artifact-Build.svg" 
                    AlternateText="<%$Resources:Fields,Build %>" 
                    />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx ItemStyle-Wrap="True" HeaderColumnSpan="-1" FooterColumnSpan="-1" DataField="Name" NameMaxLength="60" CommandArgumentField="BuildId" />
        <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,BuildStatusId %>" DataField="BuildStatusName" />
        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,CreationDate %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
			<ItemTemplate>
                <%# String.Format(GlobalFunctions.FORMAT_DATE_TIME, GlobalFunctions.LocalizeDate(((BuildView) Container.DataItem).CreationDate)) %>
			</ItemTemplate>        
        </tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
<tstsc:LabelEx ID="lblNoData" runat="server" />
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>