<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Contacts.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.MyPage.Contacts" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Classes" %>
<%@ Import namespace="Inflectra.SpiraTest.Common" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<tstsc:GridViewEx ID="grdContacts" runat="server" SkinID="WidgetGrid" DataKeyNames="UserId">
    <Columns>
        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
            <ItemTemplate>
                <tstsc:HyperLinkEx runat="server" ID="lnkAvatar" NavigateUrl="<%#UrlRewriterModule.RetrieveRewriterURL(UrlRoots.NavigationLinkEnum.Resources, ProjectId, ((User)Container.DataItem).UserId) %>">
                    <tstsc:UserNameAvatar ID="imgAvatar" runat="server" AvatarSize="16" ShowFullName="true" UserId="<%#((User)Container.DataItem).UserId %>" BoldUserName="false" />
                </tstsc:HyperLinkEx>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Department %>"  HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
            <ItemTemplate>
                <%# String.IsNullOrEmpty(((User)Container.DataItem).Profile.Department) ? "-" : ((User)Container.DataItem).Profile.Department%>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Online %>"  HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
            <ItemTemplate>
                <div>
                    <tstsc:UserOnlineStatus ID="ajxUserStatus" runat="server" UserId="<%#((User)Container.DataItem).UserId %>" />
                </div>
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
            <ItemTemplate>
                <tstsc:HyperLinkEx ID="lnkSendMessage" runat="server" Text="<%$Resources:Buttons,SendMessage %>" NavigateUrl="javascript:void(0)" ClientScriptMethod='<%# "tstucMessageManager.send_new_message(" + ((User)Container.DataItem).UserId + ",event)"%>' />
                <tstsc:LinkButtonEx ID="btnRemove" runat="server" Text="<%$Resources:Buttons,Remove %>" CommandName="Remove" CommandArgument="<%# ((User)Container.DataItem).UserId%>" />
            </ItemTemplate>
        </tstsc:TemplateFieldEx>
    </Columns>
</tstsc:GridViewEx>