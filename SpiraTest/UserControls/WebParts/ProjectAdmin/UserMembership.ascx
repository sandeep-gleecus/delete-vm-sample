<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="UserMembership.ascx.cs" 
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.UserMembership" 
    %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx 
    ID="grdUserMembership" 
    CssClass="DataGrid" 
    runat="server" 
    AutoGenerateColumns="False" 
    Width="100%"
    >
    <HeaderStyle CssClass="Header" />
    <Columns>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,FullName %>" 
            HeaderStyle-CssClass="priority1" 
            ItemStyle-CssClass="priority1" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    ID="lblFullName" 
                    Runat="server" 
                    Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser) Container.DataItem).FullName) %>'
                    />
            </ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,UserName %>" 
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    ID="lblUserName" 
                    Runat="server" 
                    Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser) Container.DataItem).UserName) %>'
                    />
            </ItemTemplate>
		</tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,ProjectRole %>" 
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    ID="lblProjectRole" 
                    Runat="server" 
                    Text='<%# Microsoft.Security.Application.Encoder.HtmlEncode(((ProjectUser) Container.DataItem).ProjectRoleName) %>'
                    />
            </ItemTemplate>
		</tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>