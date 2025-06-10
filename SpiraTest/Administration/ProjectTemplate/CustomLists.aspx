<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master"
    AutoEventWireup="true" CodeBehind="CustomLists.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.CustomLists" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web.Classes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx runat="server" ID="lblTitle" Text="<%$Resources:Main,CustomLists_Title %>" />
                    <small>
                        <tstsc:HyperLinkEx 
                            ID="lnkAdminHome" 
                            runat="server" 
                            Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                            >
                            <asp:Literal ID="ltrProjectName" runat="server" />
				        </tstsc:HyperLinkEx>
                    </small>
                </h2>
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="row">
            <div class="col-lg-9">
                <p>
                    <asp:Localize runat="server" Text="<%$Resources:Main,CustomLists_IntroText %>" />
                </p>
                <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
                <div class="TabControlHeader">
                    <div class="btn-group priority1">
                        <tstsc:HyperLinkEx ID="btnAddList" SkinID="ButtonDefault" runat="server">
                            <span class="fas fa-plus"></span>
                            <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_System_CustomList_AddList %>" />
                        </tstsc:HyperLinkEx>
                    </div>
                </div>
                <div class="table-responsive">
			        <tstsc:GridViewEx ID="grdCustomLists" runat="server" AutoGenerateColumns="False" ShowFooter="false" ShowHeader="true" ShowSubHeader="false" CssClass="DataGrid" Width="100%" HeaderStyle-CssClass="Header" FooterStyle-CssClass="SubHeader" ShowHeaderWhenEmpty="true" EmptyDataRowStyle-CssClass="NoRows">
				        <EmptyDataTemplate>
					        <tstsc:LabelEx ID="Label1" runat="server" Text="<%$ Resources:Main,Admin_System_CustomList_NoListsDefined %>" />
					        <tstsc:HyperLinkEx 
                                ID="lnkAddList" 
                                runat="server" 
                                Text="<%$ Resources:Main,Admin_System_CustomList_AddList %>" 
                                NavigateUrl='<%# UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomListDetails") %>' 
                                />
				        </EmptyDataTemplate>
				        <Columns>
					        <tstsc:TemplateFieldEx HeaderStyle-VerticalAlign="Middle" HeaderStyle-HorizontalAlign="Left" HeaderText="<%$ Resources:Main,Admin_System_CustomList_HeaderListId %>" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2">
						        <ItemTemplate>
							        <asp:Label ID="Label2" runat="server" Text='<%# "[" + Inflectra.SpiraTest.DataModel.CustomPropertyList.ARTIFACT_PREFIX + ":" + String.Format(Inflectra.SpiraTest.Web.GlobalFunctions.FORMAT_ID, ((Inflectra.SpiraTest.DataModel.CustomPropertyList)Container.DataItem).CustomPropertyListId) + "]" %>' />
						        </ItemTemplate>
					        </tstsc:TemplateFieldEx>
					        <tstsc:TemplateFieldEx HeaderText="<%$ Resources:Main,Admin_System_CustomList_HeaderDisplayName %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
						        <ItemTemplate>
							        <tstsc:LabelEx runat="server" Text='<%#: ((Inflectra.SpiraTest.DataModel.CustomPropertyList)Container.DataItem).Name %>' Style="width: 390px" ID="txtName" />
						        </ItemTemplate>
					        </tstsc:TemplateFieldEx>
					        <tstsc:TemplateFieldEx HeaderText="<%$Resources:Main,Admin_System_CustomList_HeaderValues %>" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3">
						        <ItemTemplate>
							        <asp:Label ID="Label1" runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.CustomPropertyList)Container.DataItem).Values.Count() %>' />
						        </ItemTemplate>
					        </tstsc:TemplateFieldEx>
					        <tstsc:TemplateFieldEx HeaderText="<%$ Resources:Main,Admin_System_CustomList_HeaderActions %>" ItemStyle-VerticalAlign="Middle" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
						        <ItemTemplate>
                                    <div class="btn-group">
							            <tstsc:HyperLinkEx ID="HyperLinkEx2" runat="server" SkinID="ButtonDefault" NavigateUrl='<%# UrlRewriterModule.RetrieveTemplateAdminUrl(ProjectTemplateId, "CustomListDetails") + "?" + GlobalFunctions.PARAMETER_CUSTOM_PROPERTY_LIST_ID + "=" + ((Inflectra.SpiraTest.DataModel.CustomPropertyList)Container.DataItem).CustomPropertyListId %>'>
                                            <span class="far fa-edit"></span>
                                            <asp:Localize runat="server" Text="<%$ Resources:Main,Admin_System_CustomList_HeaderActions_ActionEditValue %>" />
							            </tstsc:HyperLinkEx>
							            <tstsc:LinkButtonEx ID="LinkButtonEx1" runat="server" CommandName="Remove" CommandArgument="<%# ((Inflectra.SpiraTest.DataModel.CustomPropertyList)Container.DataItem).CustomPropertyListId %>" Confirmation="True" ConfirmationMessage="<%$ Resources:Main,Admin_System_CustomListValue_ConfirmDeleteValue %>">
                                            <span class="fas fa-trash-alt"></span>
                                            <asp:Localize runat="server" Text="<%$ Resources:Buttons,Remove %>"/>
							            </tstsc:LinkButtonEx>
                                    </div>
						        </ItemTemplate>
					        </tstsc:TemplateFieldEx>
				        </Columns>
			        </tstsc:GridViewEx>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
