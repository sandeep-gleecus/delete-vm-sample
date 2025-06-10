<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestTestExecutionStatusPage.aspx.cs" Inherits="Inflectra.SpiraTest.Web.TestTestExecutionStatusPage" MasterPageFile="~/MasterPages/Main.Master" %>

<%@ Import namespace="System.Data" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Register TagPrefix="tstuc" TagName="TestPreparationStatus" Src="~/UserControls/WebParts/ProjectHome/TestPreparationStatus.ascx" %>

<asp:Content ContentPlaceHolderID="cplHead" runat="server" ID="Content1">
</asp:Content>

<asp:Content ContentPlaceHolderID="cplMainContent" runat="server" ID="Content2">
	<tstuc:TestPreparationStatus ID="testExecutionStatusControl" runat="server"   />
</asp:Content>
