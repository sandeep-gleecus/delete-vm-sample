<%@ Page Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="True" CodeBehind="FileTypeEdit.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.FileTypeEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
	<div class="container-fluid">
        <div class="row">
            <div class="col-lg-9">
                <h2>
                    <asp:Localize ID="Localize1" runat="server" Text="<% $Resources:Main,Admin_FileTypes_EditFileType %>" />
                </h2>
				<p class="my4">
					<asp:Literal ID="localSubMessage" runat="server" Text="<% $Resources:Messages,Admin_FileTypes_EditFileTypeMessage %>" />
				</p>
				<tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
				<asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True" DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
            </div>
        </div>
        <div class="row data-entry-wide DataEntryForm view-edit">
            <div class="col-lg-9 col-sm-11">
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<asp:Label runat="server" Text="<% $Resources:Main,Admin_FileTypes_FileTypeID %>" />
                    </div>
                    <div class="DataEntry col-sm-9 mln3">
						<tstsc:TextBoxEx runat="server" MaxLength="4" SkinID="NarrowPlusFormControl" ReadOnly="true" ID="txtID" Enabled="false" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<asp:Label ID="Label1" runat="server" Text="<% $Resources:Main,Admin_FileTypes_FileExtension %>" AssociatedControlID="txtExtension" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx runat="server" MaxLength="15" SkinID="NarrowPlusFormControl" ID="txtExtension" />
						<asp:RequiredFieldValidator runat="server" ControlToValidate="txtExtension" ID="RequiredFieldValidator1" ErrorMessage="<% $Resources:Messages,Admin_FileTypes_RequiredExtension %>" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<asp:Label ID="Label2" runat="server" Text="<% $Resources:Main,Admin_FileTypes_FileDescription %>" AssociatedControlID="txtDescription" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtDescription" runat="server" MaxLength="32" />
						<asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ControlToValidate="txtDescription" ErrorMessage="<% $Resources:Messages,Admin_FileTypes_RequiredDecription %>" />
					</div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<asp:Label ID="Label3" runat="server" Text="<% $Resources:Main,Admin_FileTypes_FileMimeType %>" AssociatedControlID="txtMimeType" />
					</div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtMimeType" runat="server" MaxLength="255" />
                    </div>
                </div>
                <div class="form-group row">
                    <div class="DataLabel col-sm-3 col-lg-2">
						<asp:Label ID="Label4" runat="server" Text="<% $Resources:Main,Admin_FileTypes_FileIconImage %>" AssociatedControlID="txtImage" />
                    </div>
                    <div class="DataEntry col-sm-9">
						<tstsc:TextBoxEx ID="txtImage" runat="server" MaxLength="255" />
                    </div>
                </div>
                <div class="row">
                    <div class="btn-group ml4 mt4">
						<tstsc:ButtonEx ID="btnUpdate" runat="server" SkinID="ButtonPrimary" CausesValidation="True" Text="<% $Resources:Buttons,Save %>" />
						<tstsc:ButtonEx ID="btnCancel" runat="server" CausesValidation="False" Text="<% $Resources:Buttons,Cancel %>" OnClientClick="javascript:window.location = 'FileTypeList.aspx';return false;" />
					</div>
				</div>
			</div>
		</div>
	</div>
</asp:Content>
