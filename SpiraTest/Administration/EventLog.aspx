<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Administration.master" AutoEventWireup="true" CodeBehind="EventLog.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Administration.EventLog" %>
<%@ Import Namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import Namespace="Inflectra.SpiraTest.Web" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplAdministrationContent" runat="server">
    <h2>
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,EventLog_TitleLong %>" />
    </h2>
    <p class="my4">
        <asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,EventLog_Legend %>" />
    </p>
    <div class="Spacer"></div>
    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
    <div class="Spacer"></div>
    <div class="TabControlHeader">
        <div class="btn-group priority1">
            <tstsc:LinkButtonEx ID="btnRefresh" runat="server" >
                <span class="fas fa-sync"></span>
                <asp:Localize runat="server" Text="<%$Resources:Buttons,Refresh %>" />
            </tstsc:LinkButtonEx>
        </div>
        <div class="btn-group priority3">
            <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-filter" ID="btnFilter" runat="server" CausesValidation="False" Text="<%$Resources:Buttons,Filter %>" />
            <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-times" ID="btnClearFilters" runat="server" CausesValidation="False"
                Text="<%$Resources:Buttons,ClearFilter%>" />
            <tstsc:DropMenu GlyphIconCssClass="mr3 fas fa-trash-alt" ID="btnClearLog" runat="server" CausesValidation="False" Confirmation="true"
                ConfirmationMessage="<%$Resources:Messages,EventLog_ClearLogConfirm %>"
                Text="<%$Resources:Buttons,ClearLog%>" />
        </div>
    </div>


    <tstsc:GridViewEx ID="grdEventLog" CssClass="DataGrid" runat="server" PageSize="15"
        AllowSorting="true" AllowCustomPaging="True" AllowPaging="True" ShowSubHeader="True"
        Width="100%" AutoGenerateColumns="False" EnableViewState="false">
        <HeaderStyle CssClass="Header" />
        <SubHeaderStyle CssClass="SubHeader" />
        <Columns>
            <tstsc:FilterSortFieldEx DataField="EventTimeUtc" HeaderText="<%$Resources:Fields,EventTime %>" FilterField="EventTimeUtc"
                FilterType="DateControl" Sortable="true" FilterWidth="25px" ItemStyle-Wrap="false" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority4"/>
            <tstsc:FilterSortFieldEx DataField="Type.Name" HeaderText="<%$Resources:Fields,Type %>" FilterField="EventTypeId"
                FilterType="DropDownList" Sortable="true" FilterLookupDataField="EventTypeId" FilterLookupTextField="Name" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority4"/>
            <tstsc:FilterSortFieldEx DataField="EventCategory" HeaderText="<%$Resources:Fields,Category %>" FilterField="EventCategory"
                FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority4" />
            <tstsc:FilterSortFieldEx DataField="Message" HeaderText="<%$Resources:Fields,Message %>" FilterField="Message"
                FilterType="TextBox" Sortable="true" HeaderStyle-CssClass="priority3" ItemStyle-CssClass="priority3" SubHeaderStyle-CssClass="priority4" />
            <tstsc:FilterSortFieldEx DataField="EventCode" HeaderText="<%$Resources:Fields,EventCode %>" FilterField="EventCode"
                FilterType="TextBox" FilterWidth="70px" Sortable="true"  HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" SubHeaderStyle-CssClass="priority4"/>
            <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,Operations %>"  HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1" SubHeaderStyle-CssClass="priority4">
                <ItemTemplate>
                    <tstsc:HyperLinkEx ID="lnkViewDetails" SkinID="ButtonDefault" runat="server" NavigateUrl="javascript:void(0)" ClientScriptMethod='<%#"grdEventLog_viewDetails(event," + GlobalFunctions.JSEncode(((Event) Container.DataItem).ExceptionType, false, true) + "," + GlobalFunctions.JSEncode(((Event) Container.DataItem).Message, false, true) + "," + GlobalFunctions.JSEncode(((Event) Container.DataItem).Details, false, true) + ")"%>' Text="<%$Resources:Buttons,ViewItem %>" />
                </ItemTemplate>
            </tstsc:TemplateFieldEx>
        </Columns>
    </tstsc:GridViewEx>
    <asp:ObjectDataSource ID="srcEventLogEntryTypes" runat="server" SelectMethod="GetTypes"
        TypeName="Inflectra.SpiraTest.Business.EventManager" />


    <tstsc:DialogBoxPanel ID="dlgErrorDetails" runat="server" Persistent="true" Modal="false" Width="960px" Height="500px" Title="<%$Resources:Main,EventLog_EventDetails %>">
        <p>
            <asp:Localize runat="server" Text="<%$Resources:Main,EventLog_Exception %>" />: <span id="spnEventException" style="font-weight:bold"></span>
        </p>
		<div id="divEventDetails" style="margin-top: 10px; margin-bottom: 10px; overflow: auto; width: 100%; height: 400px; padding:5px; border: 1px solid #ddd"></div>
        <div>
		    <tstsc:ButtonEx ID="btnClose" SkinID="ButtonPrimary" runat="server" ClientScriptServerControlId="dlgErrorDetails" ClientScriptMethod="close()" Text="<%$Resources:Buttons,OK %>" />
        </div>
    </tstsc:DialogBoxPanel>
    <script language="javascript" type="text/javascript">
        function grdEventLog_viewDetails(evt, exception, message, details)
        {
            var dlgErrorDetails = $find('<%=dlgErrorDetails.ClientID %>');
            dlgErrorDetails.display(evt);

            //Populate the exception
            var spnException = $get('spnEventException');
            spnException.innerHTML = exception;
            var divEventDetails = $get('divEventDetails');
            divEventDetails.innerHTML = '<strong>' + message + '</strong><br/>' + details;
        }
    </script>
</asp:Content>
