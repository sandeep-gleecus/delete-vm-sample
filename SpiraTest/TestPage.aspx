<%@ Page Language="c#" CodeBehind="TestPage.aspx.cs" AutoEventWireup="True" Inherits="Inflectra.SpiraTest.Web.TestPage"
    MasterPageFile="~/MasterPages/Main.Master" %>
<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">

    <tstsc:RichTextBoxJ ID="txtDescription" runat="server" />

</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
    <script type="text/javascript">
        CKEDITOR.on('instanceCreated', function (e) {
            e.editor.on('key', function (event) {
                // change event in CKEditor 4.x
                CKEDITOR.instances.cplMainContent_txtDescription.updateElement()
            });
        });
    </script>
</asp:Content>