<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LockedOutUsers.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.LockedOutUsers" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<tstsc:GridViewEx ID="grdUserManagement" CssClass="DataGrid" runat="server"
    AllowSorting="false" AllowCustomPaging="false" AllowPaging="false" ShowSubHeader="false"
    Width="100%" AutoGenerateColumns="false" EnableViewState="false">
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
        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,UserName %>" 
            ItemStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority1">
            <ItemTemplate>
                <tstsc:HyperLinkEx runat="server" Text="<%#((User)(Container.DataItem)).UserName %>" NavigateUrl='<%# "~/Administration/UserDetailsEdit.aspx?" + GlobalFunctions.PARAMETER_USER_ID + "=" + ((User)(Container.DataItem)).UserId %>' />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx 
            HeaderText="<%$Resources:Fields,LastLockoutDate %>"
            HeaderStyle-CssClass="priority3" 
            ItemStyle-CssClass="priority3" 
            SubHeaderStyle-CssClass="priority3"
            >
            <ItemTemplate>
                <tstsc:LabelEx 
                    Text="<%#((User)(Container.DataItem)).LastLockoutDate %>" 
                    runat="server" 
                    />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>