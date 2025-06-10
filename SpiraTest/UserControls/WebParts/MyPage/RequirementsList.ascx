<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="RequirementsList.ascx.cs" 
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.RequirementsList" 
    %>
<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>

<tstsc:GridViewEx 
    id="grdRequirements" 
    EnableViewState="false" 
    Runat="server" 
    SkinID="WidgetGrid"
    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.RequirementsService"
    >
	<Columns>
		<tstsc:TemplateFieldEx 
            HeaderColumnSpan="2" 
            ItemStyle-CssClass="Icon"  
            HeaderStyle-CssClass="priority1" 
            ControlStyle-CssClass="w4 h4 priority1"
            >
			<HeaderTemplate>
				<asp:Localize 
                    ID="Localize1" 
                    runat="server" 
                    Text="<%$Resources:Fields,Name %>" 
                    />
			</HeaderTemplate>
			<ItemTemplate>
			    <tstsc:ImageEx 
                    ImageUrl="Images/artifact-Requirement.svg" 
                    AlternateText="Requirement" 
                    ID="imgIcon" 
                    runat="server" 
                    />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:NameDescriptionFieldEx  
            HeaderColumnSpan="-1" 
            DataField="Name" 
            NameMaxLength="40" 
            CommandArgumentField="RequirementId" 
            HeaderStyle-CssClass="priority1" 
            ItemStyle-CssClass="priority1" 
            />
		<tstsc:BoundFieldEx 
            DataField="ProjectName" 
            HeaderText="<%$Resources:Fields,Project %>" 
            MaxLength="20" 
            HtmlEncode="false" 
            HeaderStyle-CssClass="priority2" 
            ItemStyle-CssClass="priority2" 
            />
        <tstsc:BoundFieldEx 
            DataField="ReleaseVersionNumber" 
            HeaderText="<%$Resources:Fields,Release %>" 
            HtmlEncode="false" 
            HeaderStyle-CssClass="priority2" 
            ItemStyle-CssClass="priority2" 
            />
		<tstsc:BoundFieldEx 
            DataField="ImportanceName" 
            LocalizedBase="Requirement_Importance_" 
            HeaderText="<%$Resources:Fields,Importance %>" 
            HtmlEncode="false" 
            HeaderStyle-CssClass="priority1" 
            />
		<tstsc:BoundFieldEx 
            DataField="RequirementStatusId" 
            LocalizedBase="Requirement_Status_" 
            HeaderText="<%$Resources:Fields,Status %>" 
            HtmlEncode="false" 
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            />
	</Columns>
</tstsc:GridViewEx>


<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>  
    <asp:ServiceReference Path="~/Services/Ajax/RequirementsService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
