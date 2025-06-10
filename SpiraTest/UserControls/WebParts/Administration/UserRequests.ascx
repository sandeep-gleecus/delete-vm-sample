<%@ Control 
    Language="C#" 
    AutoEventWireup="true" 
    CodeBehind="UserRequests.ascx.cs" 
    Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.UserRequests" 
    %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<tstsc:GridViewEx 
    ID="grdUserManagement" 
    CssClass="DataGrid" 
    runat="server"
    AllowSorting="false" 
    AllowCustomPaging="false" 
    AllowPaging="false" 
    ShowSubHeader="false"
    Width="100%" 
    AutoGenerateColumns="false" 
    EnableViewState="false"
    >
    <HeaderStyle CssClass="Header" />
    <Columns>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,FirstName %>"
            HeaderStyle-CssClass="priority2" 
            ItemStyle-CssClass="priority2" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    Text="<%#Microsoft.Security.Application.Encoder.HtmlEncode(((User)(Container.DataItem)).Profile.FirstName) %>" 
                    runat="server" 
                    />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,LastName %>"
            HeaderStyle-CssClass="priority2" 
            ItemStyle-CssClass="priority2" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    Text="<%#Microsoft.Security.Application.Encoder.HtmlEncode(((User)(Container.DataItem)).Profile.LastName) %>" 
                    runat="server" 
                    />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,UserName %>"
            HeaderStyle-CssClass="priority1" 
            ItemStyle-CssClass="priority1" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    Text="<%#Microsoft.Security.Application.Encoder.HtmlEncode(((User)(Container.DataItem)).UserName) %>" 
                    runat="server" 
                    />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,CreationDate %>"
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    Text="<%#((User)(Container.DataItem)).CreationDate %>" 
                    runat="server" 
                    />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>