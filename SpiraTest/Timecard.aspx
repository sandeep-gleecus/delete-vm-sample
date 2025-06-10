<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/Main.Master" AutoEventWireup="true" CodeBehind="Timecard.aspx.cs" Inherits="Inflectra.SpiraTest.Web.Timecard" %>
<%@ Import namespace="Inflectra.SpiraTest.DataModel" %>
<%@ Import namespace="Inflectra.SpiraTest.Web" %>
<%@ Import namespace="Inflectra.SpiraTest.Web.Administration" %>
<%@ Import namespace="Inflectra.SpiraTest.Business" %>
<asp:Content ID="Content1" ContentPlaceHolderID="cplHead" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">
    <div class="mw1280 mx-auto mt6 px4">
		<h2>
            <tstsc:LabelEx runat="server" CssClass="Title" Text="<%$Resources:Main,SiteMap_MyTimecard %>" />
            <small>
                <tstsc:LabelEx ID="lblTimecardFullName" runat="server" CssClass="SubTitle" />
            </small>
        </h2>
        <p class="my4">
            <asp:Localize runat="server" Text="<%$Resources:Main,Timecard_InstructionText %>" />
        </p>
        <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
        <asp:ValidationSummary CssClass="ValidationMessage" ShowMessageBox="False" ShowSummary="True"
            DisplayMode="BulletList" runat="server" ID="ValidationSummary1" />
        <asp:Panel ID="pnlTasks" runat="server">
            <h3><asp:Localize runat="server" Text="<%$Resources:Main,ProjectList_MyAssignedTasks %>" /></h3>
            <tstsc:GridViewEx 
                AutoGenerateColumns="false" 
                CssClass="DataGrid DataGrid-no-bands"  
                DataKeyNames="TaskId"                       
                ID="grdTasks" 
                runat="server" 
                ShowHeader="true" 
                ShowSubHeader="false" 
                ShowFooter="false" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TasksService"
                Width="100%" 
                >
                <HeaderStyle CssClass="Header" />
                <Columns>
                    <tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderText="<%$Resources:Fields,TaskName %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
                        <ItemTemplate>
                            <tstsc:ImageEx ID="ImageEx1" runat="server" CssClass="w4 h4" AlternateText="<%$Resources:Fields,Task %>" ImageAlign="TextTop" ImageUrl="Images/artifact-Task.svg" />
                            <asp:Literal ID="Literal1" runat="server" Text="<%$GlobalFunctions:ARTIFACT_PREFIX_TASK %>" /><asp:Literal ID="Literal2" runat="server" Text='<%#((TaskView)Container.DataItem).TaskId %>' />
                        </ItemTemplate>                            
                    </tstsc:TemplateFieldEx>
                    <tstsc:NameDescriptionFieldEx HeaderColumnSpan="1"  DataField="Name" CommandArgumentField="TaskId"  HeaderText="<%$Resources:Fields,TaskName %>" HeaderStyle-CssClass="priority4-inverse" ItemStyle-CssClass="priority1" />
                    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,TaskPriorityId %>" DataField="TaskPriorityName" HeaderStyle-CssClass="priority4" />
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,StartDate %>" HeaderStyle-CssClass="priority4" >
                        <ItemTemplate>
                            <asp:Literal ID="ltrStartDate" runat="server" Text=""/>
                        </ItemTemplate>                            
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,EndDate %>" HeaderStyle-CssClass="priority2">
                        <ItemTemplate>
                            <asp:Literal ID="ltrEndDate" runat="server" Text="" />
                        </ItemTemplate>                            
                    </tstsc:TemplateFieldEx>
                    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ProjectName %>" DataField="ProjectName" />
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,EffortToDate %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
                            <asp:Literal ID="Literal4" runat="server" Text='<%#(((TaskView)Container.DataItem).ActualEffort == null) ? "" : GlobalFunctions.GetEffortInFractionalHours(((TaskView)Container.DataItem).ActualEffort.Value)%>' />
							<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_hours%>" Visible='<%#((TaskView)Container.DataItem).ActualEffort != null %>' />
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,AdditionalEffort %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
                            <tstsc:TextBoxEx ID="txtAdditionalEffort" runat="server" Width="50%" style="min-width: 2.5rem; text-align: center;" />
							<asp:Localize ID="Localize3" runat="server" Text="<%$Resources:Main,Global_hours%>" />
                            <asp:RegularExpressionValidator ID="vldAdditionalEffort" ControlToValidate="txtAdditionalEffort" runat="server"
                                ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EFFORT_HOURS_NEGATIVE_ALLOWED %>" ErrorMessage="<%$Resources:Messages,Timecard_AdditionalEffortInvalid %>" />                                        
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,RemainingEffort %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
                            <tstsc:TextBoxEx ID="txtRemainingEffort" runat="server" Width="50%" style="min-width: 2.5rem; text-align: center;" Text='<%#(((TaskView)Container.DataItem).RemainingEffort == null) ? "" : GlobalFunctions.GetEffortInFractionalHours(((TaskView)Container.DataItem).RemainingEffort.Value)%>' />
							<asp:Localize runat="server" Text="<%$Resources:Main,Global_hours%>" />
                            <asp:RegularExpressionValidator ID="vldRemainingEffort" ControlToValidate="txtRemainingEffort" runat="server"
                                ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EFFORT_HOURS %>" ErrorMessage="<%$Resources:Messages,Timecard_RemainingEffortInvalid %>" />                                        
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>    
                </Columns>
            </tstsc:GridViewEx>
            <br />
        </asp:Panel>
        <asp:Panel ID="pnlIncidents" runat="server">
            <h3><asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,ProjectList_MyAssignedIncidents %>" /></h3>
            <tstsc:GridViewEx 
                AutoGenerateColumns="false" 
                CssClass="DataGrid DataGrid-no-bands" 
                DataKeyNames="IncidentId"
                ID="grdIncidents" 
                runat="server" 
                ShowHeader="true" 
                ShowSubHeader="false" 
                ShowFooter="false" 
                WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" 
                Width="100%"
                >
                <HeaderStyle CssClass="Header" />
                <Columns>
                    <tstsc:TemplateFieldEx HeaderColumnSpan="2" HeaderText="<%$Resources:Fields,IncidentName %>" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4">
                        <ItemTemplate>
                            <tstsc:ImageEx ID="ImageEx1" runat="server" CssClass="w4 h4" AlternateText="<%$Resources:Fields,Incident %>" ImageAlign="TextTop" ImageUrl="Images/artifact-Incident.svg" />
                            <asp:Literal ID="Literal1" runat="server" Text="<%$GlobalFunctions:ARTIFACT_PREFIX_INCIDENT %>" /><asp:Literal ID="Literal2" runat="server" Text='<%#((IncidentView)Container.DataItem).IncidentId %>' />
                        </ItemTemplate>                            
                    </tstsc:TemplateFieldEx>
                    <tstsc:NameDescriptionFieldEx HeaderColumnSpan="1" HeaderText="<%$Resources:Fields,IncidentName %>" DataField="Name" CommandArgumentField="IncidentId"  HeaderStyle-CssClass="priority4-inverse" ItemStyle-CssClass="priority1"/>
                    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,PriorityId %>" DataField="PriorityName" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4" />
                    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,SeverityId %>" DataField="SeverityName" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4"/>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,StartDate %>" HeaderStyle-CssClass="priority4">
                        <ItemTemplate>
                            <asp:Literal ID="ltrStartDate" runat="server" Text="" />
                        </ItemTemplate>                            
                    </tstsc:TemplateFieldEx>
                    <tstsc:BoundFieldEx HeaderText="<%$Resources:Fields,ProjectName %>" DataField="ProjectName" HeaderStyle-CssClass="priority4" ItemStyle-CssClass="priority4"/>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,EffortToDate %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
                            <asp:Literal ID="Literal4" runat="server" Text='<%#(((IncidentView)Container.DataItem).ActualEffort.HasValue) ? GlobalFunctions.GetEffortInFractionalHours(((IncidentView)Container.DataItem).ActualEffort.Value) : ""%>' />
							<asp:Localize ID="Localize2" runat="server" Text="<%$Resources:Main,Global_hours%>" Visible='<%#((IncidentView)Container.DataItem).ActualEffort.HasValue %>' />
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,AdditionalEffort %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
                            <tstsc:TextBoxEx ID="txtAdditionalEffort" runat="server" Width="50%" style="min-width: 2.5rem; text-align: center;"/>
							<asp:Localize runat="server" Text="<%$Resources:Main,Global_hours%>" />
                            <asp:RegularExpressionValidator ID="vldAdditionalEffort" ControlToValidate="txtAdditionalEffort" runat="server"
                                ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EFFORT_HOURS_NEGATIVE_ALLOWED %>" ErrorMessage="<%$Resources:Messages,Timecard_AdditionalEffortInvalid %>" />                                        
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                    <tstsc:TemplateFieldEx HeaderText="<%$Resources:Fields,RemainingEffort %>" HeaderStyle-CssClass="priority1" ItemStyle-CssClass="priority1">
                        <ItemTemplate>
                            <tstsc:TextBoxEx ID="txtRemainingEffort" runat="server" Width="50%" style="min-width: 2.5rem; text-align: center;" Text='<%#(((IncidentView)Container.DataItem).RemainingEffort.HasValue) ? GlobalFunctions.GetEffortInFractionalHours(((IncidentView)Container.DataItem).RemainingEffort.Value) : ""%>' />
							<asp:Localize runat="server" Text="<%$Resources:Main,Global_hours%>" />
                            <asp:RegularExpressionValidator ID="vldRemainingEffort" ControlToValidate="txtRemainingEffort" runat="server"
                                ValidationExpression="<%$GlobalFunctions:VALIDATION_REGEX_EFFORT_HOURS %>" ErrorMessage="<%$Resources:Messages,Timecard_RemainingEffortInvalid %>" />                                        
                        </ItemTemplate>
                    </tstsc:TemplateFieldEx>
                </Columns>
            </tstsc:GridViewEx>
        </asp:Panel>
        <div class="btn-group my3">
            <tstsc:ButtonEx ID="btnSubmit" runat="server" SkinID="ButtonPrimary" Text="<%$Resources:Buttons,SubmitTimecard %>" CausesValidation="true" Confirmation="true" ConfirmationMessage="<%$Resources:Dialogs,Timecard_SubmitConfirm %>" />
            <tstsc:ButtonEx ID="btnCancel" runat="server" Text="<%$Resources:Buttons,Cancel %>" CausesValidation="false" />
        </div>
    </div>
    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>  
            <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />  
            <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />  
        </Services>  
    </tstsc:ScriptManagerProxyEx>
</asp:Content>
