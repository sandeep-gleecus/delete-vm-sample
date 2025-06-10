<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="EventList.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.Administration.EventList" %>
        <tstsc:GridViewEx ID="grdEventLog" CssClass="DataGrid" runat="server"
            AllowSorting="false" AllowCustomPaging="True" AllowPaging="false" ShowSubHeader="false"
            Width="100%" AutoGenerateColumns="false" EnableViewState="false">
            <HeaderStyle CssClass="Header" />
            <SubHeaderStyle CssClass="SubHeader" />
            <Columns>
                <tstsc:TemplateFieldEx 
                    HeaderText="<%$Resources:Fields,EventTime %>"
                    HeaderStyle-CssClass="priority1" 
                    ItemStyle-CssClass="priority1" 
                    SubHeaderStyle-CssClass="priority4"
                    >
                    <ItemTemplate>
                        <tstsc:LabelEx 
                            Text="<%#((Event)(Container.DataItem)).EventTimeUtc %>" 
                            runat="server" 
                            />
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
                <tstsc:TemplateFieldEx 
                    HeaderText="<%$Resources:Fields,Type %>"
                    HeaderStyle-CssClass="priority1" 
                    ItemStyle-CssClass="priority1" 
                    SubHeaderStyle-CssClass="priority4"
                    >
                    <ItemTemplate>
                        <tstsc:LabelEx 
                            Text="<%#Microsoft.Security.Application.Encoder.HtmlEncode(((Event)(Container.DataItem)).Type.Name) %>" 
                            runat="server" 
                            />
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
                <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Message %>"
                    HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" SubHeaderStyle-CssClass="priority2">
                    <ItemTemplate>
                        <tstsc:LabelEx ToolTip="<%#((Event)(Container.DataItem)).Details %>" Text="<%#((Event)(Container.DataItem)).Message %>" runat="server" />
                    </ItemTemplate>
                </tstsc:TemplateFieldEx>
            </Columns>
        </tstsc:GridViewEx>