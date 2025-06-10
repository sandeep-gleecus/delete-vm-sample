<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="Graphs.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.Administration.Graphs" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_Graphs %>" />
                </h2>
                <p class="my4">
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_Graphs_Intro %>" />
                </p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <div class="Spacer"></div>
                <tstsc:GridViewEx ID="grdGraphs" runat="server" SkinID="DataGrid" DataKeyNames="GraphId" ShowHeader="true" ShowSubHeader="false" ShowFooter="true">
                    <FooterStyle CssClass="Highlighted" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" FooterColumnSpan="4"  FooterStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:LabelEx ID="lblName" runat="server" Text="<%#:((GraphCustom) Container.DataItem).Name%>" ToolTip="<%#:((GraphCustom) Container.DataItem).Description%>" />
                            </ItemTemplate>                        
                            <FooterTemplate>
                                <tstsc:LinkButtonEx ID="btnAddGraph" runat="server" Text="<%$Resources:Main,Admin_Reports_AddNewGraph %>" SkinID="ButtonLinkAdd" CommandName="AddGraph" />
                            </FooterTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-HorizontalAlign="Center" FooterColumnSpan="-1"  FooterStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                            <ItemTemplate>
                                <asp:PlaceHolder ID="plcActive" runat="server" Visible="<%#((GraphCustom) Container.DataItem).IsActive%>">
                                    <span class="fas fa-check"></span>
                                </asp:PlaceHolder>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Position %>" FooterColumnSpan="-1" FooterStyle-CssClass="priority3" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
                            <ItemTemplate>
                                <tstsc:LabelEx ID="lblPosition" runat="server" Text="<%#((GraphCustom) Container.DataItem).Position%>" />
                            </ItemTemplate>                        
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" FooterColumnSpan="-1"  FooterStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <div class="btn-group">
                                    <tstsc:HyperLinkEx ID="lnkEdit" SkinID="ButtonDefault" runat="server" NavigateUrl='<%# "GraphDetails.aspx?" + GlobalFunctions.PARAMETER_GRAPH_ID + "=" + ((GraphCustom)Container.DataItem).GraphId%>'>
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>"/>
                                    </tstsc:HyperLinkEx>
                                    <tstsc:LinkButtonEx ID="btnCopy" runat="server" CommandName="CloneGraph" CommandArgument="<%#((GraphCustom) Container.DataItem).GraphId %>" >
                                        <span class="far fa-clone"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Clone %>" />
                                    </tstsc:LinkButtonEx>
                                    <asp:PlaceHolder ID="plcDelete" runat="server">
                                        <tstsc:LinkButtonEx ID="btnDelete" runat="server" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_Graphs_DeleteConfirm %>"
                                            CommandName="DeleteGraph" CommandArgument="<%#((GraphCustom) Container.DataItem).GraphId %>">
                                            <span class="fas fa-trash-alt"></span>
                                            <asp:Localize runat="server" Text="<%$Resources:Buttons,Delete %>" />
                                        </tstsc:LinkButtonEx>
                                    </asp:PlaceHolder>
                                </div>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                    </Columns>
                </tstsc:GridViewEx>
            </div>
        </div>
    </div>
</asp:Content>
