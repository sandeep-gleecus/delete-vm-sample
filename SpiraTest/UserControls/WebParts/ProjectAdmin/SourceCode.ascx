<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SourceCode.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.WebParts.ProjectAdmin.SourceCode" %>
<div class="form-horizonal">
    <div class="form-group row">
        <tstsc:LabelEx runat="server" Text="<%$ Resources:Fields,Provider %>" AppendColon="true" CssClass="control-label col-sm-4" />
        <div class="col-sm-8">
            <tstsc:HyperLinkEx runat="server" ID="lnkProvider" />
        </div>
    </div>
    <div class="form-group row">
        <tstsc:LabelEx runat="server" Text="<%$ Resources:Fields,ConnectionInfo %>" AppendColon="true" CssClass="control-label col-sm-4" />
        <div class="col-sm-8">
            <asp:Literal runat="server" ID="ltrConnectionInfo" Mode="Encode" />
        </div>
    </div>
    <div class="form-group row">
        <tstsc:LabelEx runat="server" Text="<%$ Resources:Fields,ActiveYn %>" AppendColon="true" CssClass="control-label col-sm-4" />
        <div class="col-sm-8">
            <asp:Literal runat="server" ID="ltrActive" Mode="Encode" />
        </div>
    </div>
</div>