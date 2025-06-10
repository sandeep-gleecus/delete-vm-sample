<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="FileTypeList.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.FileTypeList" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <asp:Literal ID="Literal1" runat="server" Text="<% $Resources:Main,Admin_FileTypes_List %>" />
    </h2>
	<p>
		<asp:Literal ID="Literal2" runat="server" Text="<% $Resources:Messages,Admin_FileTypes_AllListed %>" />
	</p>
	<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <div class="TabControlHeader">
        <div class="btn-group priority1">
			<tstsc:LinkButtonEx ID="DropMenu1" runat="server" CausesValidation="False" OnClientClick="javascript:window.location = 'FileTypeEdit.aspx?filetypeid=0';return false;">
                <span class="fas fa-plus"></span>
                <asp:Localize runat="server" Text="<% $Resources:Buttons,Add %>" />
			</tstsc:LinkButtonEx>
        </div>
    </div>

	<tstsc:GridViewEx ID="grdFileTypeList" CssClass="DataGrid" runat="server" ShowSubHeader="False" AutoGenerateColumns="false" Width="100%">
		<HeaderStyle CssClass="Header" />
		<Columns>
			<tstsc:BoundFieldEx DataField="FileExtension" HeaderText="<% $Resources:Main,Admin_FileTypes_Extension %>" HtmlEncode="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" />
			<tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Admin_FileTypes_Icon %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
				<ItemTemplate>
					<tstsc:ImageEx runat="server" Title='<%# ((Filetype)Container.DataItem).Icon %>' AlternateText='<%# ((Filetype)Container.DataItem).Icon %>' ImageUrl='<%# "Images/Filetypes/" + ((Filetype)Container.DataItem).Icon %>' ID="imgFile" ImageAlign="AbsMiddle" CssClass="w5 h5" />
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
			<tstsc:BoundFieldEx HeaderText="<% $Resources:Main,Admin_FileTypes_Description %>" DataField="Description" HtmlEncode="true" ItemStyle-Wrap="false" HeaderStyle-CssClass="priority2" ItemStyle-CssClass="priority2"/>
			<tstsc:BoundFieldEx HeaderText="<% $Resources:Main,Admin_FileTypes_MimeType %>" DataField="Mime" HtmlEncode="true" ItemStyle-Wrap="true" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4"/>
			<tstsc:TemplateFieldEx HeaderText="<% $Resources:Main,Global_Operations %>" ItemStyle-Wrap="false" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
				<ItemTemplate>
                    <div class="btn-group">
						<tstsc:LinkButtonEx ID="actEdit" runat="server" CommandName="EditFile" CommandArgument='<%# ((Filetype)Container.DataItem).FiletypeId %>'>
                            <span class="far fa-edit"></span>
                            <asp:Localize runat="server" Text="<% $Resources:Buttons,Edit %>" />
						</tstsc:LinkButtonEx>
						<tstsc:LinkButtonEx ID="actDelete" runat="server" CommandName="DeleteFile" CommandArgument='<%# ((Filetype)Container.DataItem).FiletypeId %>' Confirmation="true" ConfirmationMessage="<% $Resources:Main,Admin_FileTypes_SureDelete %>">
                            <span class="fas fa-trash-alt"></span>
                            <asp:Localize runat="server" Text="<% $Resources:Buttons,Delete %>" />
						</tstsc:LinkButtonEx>
                    </div>
				</ItemTemplate>
			</tstsc:TemplateFieldEx>
		</Columns>
	</tstsc:GridViewEx>
</asp:Content>
