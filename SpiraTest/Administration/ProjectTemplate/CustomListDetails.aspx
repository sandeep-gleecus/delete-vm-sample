<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CustomListDetails.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.ProjectTemplate.CustomListDetails" MasterPageFile="~/MasterPages/Administration.master" %>
<%@ Register TagPrefix="tstsc" Namespace="Inflectra.SpiraTest.Web.ServerControls"
    Assembly="Web" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ContentPlaceHolderID="cplAdministrationContent" runat="server" ID="Content2">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <tstsc:LabelEx ID="lblListName" runat="server" />
                    <small>
                        <asp:Localize runat="server" Text="<%$Resources:Main,CustomListDetails_Title %>" />
                    </small>
                </h2>
                <div class="btn-group priority1" role="group">
                    <tstsc:HyperLinkEx SkinID="ButtonDefault" ID="lnkCustomList" runat="server">
                        <span class="fas fa-arrow-left"></span>
                        <asp:Localize runat="server" Text="<%$Resources:Main,CustomListDetails_BackToLists %>" />
                    </tstsc:HyperLinkEx>
                </div>
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />
        <div class="Spacer"></div>
        <div class="row data-entry-wide DataEntryForm">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
							<tstsc:LabelEx ID="LabelEx1" runat="server" AssociatedControlID="listName" Text="<%$ Resources:Main,Admin_System_CustomList_HeaderDisplayName %>" AppendColon="true"/>
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
							<tstsc:TextBoxEx runat="server" ID="listName" MaxLength="64" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<tstsc:LabelEx ID="LabelEx3" runat="server" AssociatedControlID="listSort" Text="<%$ Resources:Main,Admin_System_CustomListValue_SortBy %>" AppendColon="true" />
                    </div>
                    <div class="DataEntry col-sm-9 col-lg-10">
						<tstsc:DropDownListEx CssClass="DropDownList" runat="server" ID="listSort" DataTextField="Value" DataValueField="Key" NoValueItem="false" />
                    </div>
                </div>
            </div>
        </div>
        <div class="Spacer"></div>
        <div class="Spacer"></div>
        <div class="row">
            <div class="col-lg-9 col-sm-11">
			    <p>
                    <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,CustomListDetails_IntroText %>" />
                </p>
                <div class="Spacer"></div>
                <div class="TabControlHeader">
                    <div class="display-inline-block">
                        <strong><asp:Localize runat="server" Text="<%$Resources:Main,Global_Displaying %>"/>:</strong>
                    </div>
                    <tstsc:DropDownListEx ID="ddlFilterType" runat="server" AutoPostBack="true">
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_AllActive %>" Value="allactive" />
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_AllButDeleted %>" Value="allbutdeleted" />
                        <asp:ListItem Text="<%$Resources:Dialogs,Global_All %>" Value="all" />
                    </tstsc:DropDownListEx>
                </div>
				<tstsc:GridViewEx ID="grdCustomListValues" runat="server" AutoGenerateColumns="False" ShowHeader="true" ShowSubHeader="false" CssClass="DataGrid" Width="100%" HeaderStyle-CssClass="Header" ShowHeaderWhenEmpty="true" EmptyDataRowStyle-CssClass="NoRows" ShowFooter="true" FooterStyle-CssClass="AddValueFooter">
					<EmptyDataTemplate>
						<asp:Label ID="Label1" runat="server" Text="<%$ Resources:Main,Admin_System_CustomListValue_NoValuesDefined %>" />
						<tstsc:LinkButtonEx runat="server" Text="<%$ Resources:Main,Admin_System_CustomListValue_AddValue %>" ID="lnkNewRow" CommandName="CreateNew" />
					</EmptyDataTemplate>
					<Columns>
						<tstsc:TemplateFieldEx HeaderStyle-VerticalAlign="Middle" HeaderStyle-HorizontalAlign="Left" HeaderText="<%$ Resources:Main,Admin_System_CustomListValue_HeaderValueId %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" FooterStyle-CssClass="priority1" FooterColumnSpan="5">
							<ItemTemplate>
								<asp:Label ID="Label2" runat="server" Text='<%# "[" + Inflectra.SpiraTest.DataModel.CustomPropertyValue.ARTIFACT_PREFIX + ":" + String.Format(Inflectra.SpiraTest.Web.GlobalFunctions.FORMAT_ID, ((Inflectra.SpiraTest.DataModel.CustomPropertyValue)Container.DataItem).CustomPropertyValueId) + "]" %>' />
							</ItemTemplate>
                            <FooterTemplate>
                                <tstsc:LinkButtonEx ID="lnkNewRow" runat="server" CommandName="CreateNew" Text="<%$ Resources:Main,Admin_System_CustomListValue_AddValue %>" SkinID="ButtonLinkAdd" />
                            </FooterTemplate>
						</tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Main,Admin_System_CustomListValue_HeaderValueName %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" FooterColumnSpan="-1">
							<ItemTemplate>
								<tstsc:TextBoxEx MaxLength="255" Width="100%" MetaData='<%# ((Inflectra.SpiraTest.DataModel.CustomPropertyValue)Container.DataItem).CustomPropertyValueId %>' CssClass="text-box" runat="server" Text='<%# ((Inflectra.SpiraTest.DataModel.CustomPropertyValue)Container.DataItem).Name %>' ID="txtName" />
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,ActiveYn %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" FooterColumnSpan="-1">
                            <ItemTemplate>
                                <tstsc:CheckBoxYnEx runat="server" ID="chkActive" Checked="<%# (((CustomPropertyValue) Container.DataItem).IsActive) ? true : false %>"/>
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
                        <tstsc:TemplateFieldEx  HeaderText="<%$Resources:Fields,Status %>" FooterColumnSpan="-1">
                            <ItemTemplate>
                                <asp:Literal ID="Literal1" runat="server" Text="<%#GetStatus((CustomPropertyValue)Container.DataItem) %>" />
                            </ItemTemplate>
                        </tstsc:TemplateFieldEx>
						<tstsc:TemplateFieldEx HeaderText="<%$ Resources:Main,Admin_System_CustomList_HeaderActions %>" ItemStyle-VerticalAlign="Middle" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" FooterColumnSpan="-1">
							<ItemTemplate>
					            <tstsc:LinkButtonEx ID="btnDelete" runat="server" CommandName="Remove" CommandArgument="<%# ((CustomPropertyValue)Container.DataItem).CustomPropertyValueId %>" Visible="<%#!((CustomPropertyValue)Container.DataItem).IsDeleted %>">
                                    <span class="fas fa-trash-alt"></span>
                                    <asp:Localize runat="server" Text="<%$ Resources:Buttons,Delete %>" />
					            </tstsc:LinkButtonEx>
                                <tstsc:LinkButtonEx ID="btnUndelete" runat="server"  CommandName="Undelete" CommandArgument="<%# ((CustomPropertyValue)Container.DataItem).CustomPropertyValueId %>" Visible="<%#((CustomPropertyValue)Container.DataItem).IsDeleted %>">
                                    <span class="fas fa-undo"></span>
                                    <asp:Localize runat="server" Text="<%$ Resources:Buttons,Undelete %>" />
                                </tstsc:LinkButtonEx>
							</ItemTemplate>
						</tstsc:TemplateFieldEx>
					</Columns>
				</tstsc:GridViewEx>
			    <tstsc:TextBoxEx ID="gridCount" runat="server" Visible="false" />
                <div class="Spacer"></div>
                <div class="Spacer"></div>
                <div class="btn-group">
                    <tstsc:ButtonEx ID="btnSave" SkinID="ButtonPrimary" runat="server" Text="<%$ Resources:Buttons,Save %>" />
                    <tstsc:ButtonEx ID="btnCancel" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Cancel %>" /> 
                </div>
            </div>
        </div>
    </div>
</asp:Content>
