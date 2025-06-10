<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Components.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.Components" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>

<tstsc:GridViewEx ID="grdComponents" runat="server" ShowSubHeader="False" SkinID="DataGrid" Width="100%">
	<Columns>
        <tstsc:TemplateFieldEx FooterColumnSpan="6" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" FooterStyle-CssClass="priority1">
            <ItemTemplate>
                <tstsc:ImageEx 
                    CssClass="w4 h4"
                    runat="server" 
                    ImageUrl="Images/org-Component.svg" 
                    AlternateText="<%$Resources:Fields,ComponentId %>" 
                    />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,Name %>" 
            HeaderStyle-CssClass="priority4" 
            ItemStyle-CssClass="priority4" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    ID="lblName" 
                    Runat="server" 
                    Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((Component)Container.DataItem).Name) %>'
                    />
            </ItemTemplate>
		</tstsc:TemplateFieldEx>
		<tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ID %>" FooterColumnSpan="-1" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" >
			<ItemTemplate>
				<asp:Literal ID="Label2" runat="server" Text='<%# "[" + Component.ARTIFACT_PREFIX + ":" %>' /><asp:Literal runat="server" ID="ltrComponentId" Text='<%# ((Component)Container.DataItem).ComponentId %>' /><asp:Literal ID="Label3" runat="server" Text=']' />
			</ItemTemplate>
		</tstsc:TemplateFieldEx>
	</Columns>
</tstsc:GridViewEx>
