<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.Reports" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,Admin_Reports %>" />
                </h2>
                <p class="my4">
                    <asp:Localize runat="server" Text="<%$Resources:Main,Admin_Reports_Intro %>" />
                </p>
                <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
                <div class="Spacer"></div>
                <tstsc:GridViewEx ID="grdReports" runat="server" SkinID="DataGrid" DataKeyNames="ReportId" ShowHeader="true" ShowSubHeader="false" ShowFooter="true">
                    <FooterStyle CssClass="Highlighted" />
                    <Columns>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Category %>" HeaderColumnSpan="2" FooterColumnSpan="6" FooterStyle-HorizontalAlign="Left" FooterStyle-Font-Bold="true" FooterStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:ImageEx 
                                    CssClass="w4 h4"
                                    ID="imgIcon" 
                                    runat="server" 
                                    ImageUrl='<%#"Images/" + GlobalFunctions.GetIconForArtifactType((Artifact.ArtifactTypeEnum)((Report) Container.DataItem).Category.ArtifactTypeId) %>' 
                                    />
                            </ItemTemplate>
                            <FooterTemplate>
                                <tstsc:HyperLinkEx ID="lnkAddReport" runat="server" Text="<%$Resources:Main,Admin_Reports_AddNewReport %>" NavigateUrl="ReportDetails.aspx" SkinID="ButtonLinkAdd" />
                            </FooterTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderColumnSpan="-1" FooterColumnSpan="-1"  FooterStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <asp:Literal ID="ltrCategory" runat="server" Text="<%#((Report) Container.DataItem).Category.Name%>" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Name %>" FooterColumnSpan="-1"  FooterStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <tstsc:LabelEx ID="lblName" runat="server" Text="<%#:((Report) Container.DataItem).Name%>" ToolTip="<%#:((Report) Container.DataItem).Description%>" />
                            </ItemTemplate>                        
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,ActiveYn %>" ItemStyle-HorizontalAlign="Center" FooterColumnSpan="-1"  FooterStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                            <ItemTemplate>
                                <asp:PlaceHolder ID="plcActive" runat="server" Visible="<%#((Report) Container.DataItem).IsActive%>">
                                    <span class="fas fa-check"></span>
                                </asp:PlaceHolder>                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Default %>" ItemStyle-HorizontalAlign="Center" FooterColumnSpan="-1"  FooterStyle-CssClass="priority2" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
                            <ItemTemplate>
                                <asp:PlaceHolder ID="plcDefault" runat="server" Visible="<%#!String.IsNullOrEmpty(((Report) Container.DataItem).Token)%>">
                                    <span class="fas fa-check"></span>
                                </asp:PlaceHolder>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>" FooterColumnSpan="-1"  FooterStyle-CssClass="priority1" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                            <ItemTemplate>
                                <div class="btn-group">
                                    <tstsc:HyperLinkEx ID="lnkEdit" SkinID="ButtonDefault" runat="server" Visible="<%#String.IsNullOrEmpty(((Report) Container.DataItem).Token)%>" NavigateUrl='<%# "ReportDetails.aspx?" + GlobalFunctions.PARAMETER_REPORT_ID + "=" + ((Report)Container.DataItem).ReportId%>'>
                                        <span class="far fa-edit"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Edit %>"/>
                                    </tstsc:HyperLinkEx>
                                    <tstsc:HyperLinkEx ID="lnkView" SkinID="ButtonDefault" runat="server" Visible="<%#!String.IsNullOrEmpty(((Report) Container.DataItem).Token)%>" NavigateUrl='<%# "ReportDetails.aspx?" + GlobalFunctions.PARAMETER_REPORT_ID + "=" + ((Report)Container.DataItem).ReportId%>'>
                                        <span class="far fa-eye"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,View %>" />
                                    </tstsc:HyperLinkEx>
                                    <tstsc:LinkButtonEx ID="btnCopy" runat="server" CommandName="CopyReport" CommandArgument="<%#((Report) Container.DataItem).ReportId %>" >
                                        <span class="far fa-clone"></span>
                                        <asp:Localize runat="server" Text="<%$Resources:Buttons,Clone %>" />
                                    </tstsc:LinkButtonEx>
                                    <asp:PlaceHolder ID="plcDelete" runat="server" Visible="<%#String.IsNullOrEmpty(((Report) Container.DataItem).Token)%>">
                                        <tstsc:LinkButtonEx ID="btnDelete" runat="server" CausesValidation="false" Confirmation="true" ConfirmationMessage="<%$Resources:Messages,Admin_Reports_DeleteConfirm %>"
                                            CommandName="DeleteReport" CommandArgument="<%#((Report) Container.DataItem).ReportId %>">
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
