<%@ Page 
    Title="" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Administration.master"
	AutoEventWireup="true" 
    CodeBehind="TestingSettings.aspx.cs" 
    Inherits="Inflectra.SpiraTest.Web.Administration.Project.TestingSettings" 
    %>

<asp:Content 
    ID="Content1" 
    ContentPlaceHolderID="cplAdministrationContent" 
    runat="server"
    >
    <h2>
        <asp:Localize ID="Localize1" runat="server" Text="<%$Resources:Main,TestingSettings_Title %>" />
        <small>
            <tstsc:HyperLinkEx 
                ID="lnkAdminHome" 
                runat="server" 
                Title="<%$Resources:Main,Admin_Project_BackToHome %>"
                >
                <tstsc:LabelEx ID="lblProjectName" runat="server" />
			</tstsc:HyperLinkEx>
        </small>
    </h2>

    <tstsc:MessageBox ID="lblMessage" runat="server" SkinID="MessageBox" />
	<asp:ValidationSummary 
        ID="ValidationSummary" 
        runat="server" 
        CssClass="ValidationMessage"
		DisplayMode="BulletList" 
        ShowSummary="True"
        ShowMessageBox="False" 
        />

    <div class="u-box_3 px4 px0-sm mt5">
		<ul class="u-box_list pl0">
			<li class="ma0 mb5 pa0 df">
				<tstsc:LabelEx 
                    runat="server" 
                    ID="chkExecutionDisplayBuildLabel" 
                    AssociatedControlID="chkExecutionDisplayBuild" 
                    Text="<%$Resources:Main,TestCaseExecution_Title %>" 
                    AppendColon="true"
                    />
                <div class="dib">
                    <div>
				        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionDisplayBuild" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="litExecutionDisplayBuild" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_DisplayBuildDuringExecution %>" 
                            />
                    </div>
                    <div class="mt5 df">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionDisablePassAll" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4"
                            ID="litExecutionDisablePassAll" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_DisablePassAll %>" 
                            />
                    </div>
                    <div class="mt4 df">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionDisableBlocked" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="litExecutionDisableBlocked" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_DisableBlocked %>" 
                            />
                    </div>
                    <div class="mt4 df">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionDisableCaution" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="litExecutionDisableCaution" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_DisableCaution %>" 
                            />
                    </div>
                    <div class="mt4 df">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionDisableNA" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="litExecutionDisableNA" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_DisableNA %>" 
                            />
                    </div>
                    <div class="mt5 df">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionActualResultAlwaysRequire" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="Localize2" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_ActualResultAlways %>" 
                            />
                    </div>
                    <div class="mt5 df">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionRequireIncident" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="Localize3" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_RequireIncident %>" 
                            />
                    </div>
                    <div class="mt5 df" runat="server" Visible="true" id="wrapperExecutionAllowTasks">
                        <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkExecutionAllowTasks" 
                            NoValueItem="false" 
                            />
                        <asp:Label
                            CssClass="ml4" 
                            ID="Localize4" 
                            runat="server" 
                            Text="<%$Resources:Main,Admin_TestingSettings_AllowTasks %>" 
                            />
                    </div>
                </div>
            </li>
            <li class="ma0 mb5 pa0 df">
				<tstsc:LabelEx 
                    ID="chkTestAutoUnassignLabel" 
                    AssociatedControlID="chkTestCaseAutoUnassign" 
                    Text="<%$Resources:Main,GeneralSettings_TestAutoUnassign %>" 
                    runat="server" 
                    AppendColon="true"
                    />
                <div class="dib">
                    <div class="df">
					    <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkTestCaseAutoUnassign" 
                            NoValueItem="false" 
                            />
					    <asp:Label
                            CssClass="ml4" 
                            ID="litTestCaseAutoUnassign" 
                            runat="server" 
                            Text="<%$Resources:Main,GeneralSettings_TestCaseAutoUnassign_Notes %>" 
                            />
                    </div>
                    <div class="mt4 df">
					    <tstsc:CheckBoxYnEx 
                            runat="server" 
                            ID="chkTestSetAutoUnassign" 
                            NoValueItem="false" 
                            />
					    <asp:Label
                            CssClass="ml4" 
                            ID="litTestSetAutoUnassign" 
                            runat="server" 
                            Text="<%$Resources:Main,GeneralSettings_TestSetAutoUnassign_Notes %>" 
                            />
                    </div>
                </div>
            </li>
            <li class="ma0 mb5 pa0 df">
				<tstsc:LabelEx
                    ID="chkCreateDefaultTestStepLabel" 
                    AssociatedControlID="chkCreateDefaultTestStep" 
                    Text="<%$Resources:Main,TestingSettings_CreateDefaultTestStep %>" 
                    runat="server" 
                    Required="true" 
                    AppendColon="true"
                    />
                <div class="dif">
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkCreateDefaultTestStep" 
                        NoValueItem="false" 
                        />
					<asp:Label
                        CssClass="ml4" 
                        ID="litCreateDefaultTestStep" 
                        runat="server" 
                        Text="<%$Resources:Main,TestingSettings_CreateDefaultTestStepNotes %>" 
                        />
                </div>
            </li>
            <li class="ma0 mb5 pa0 df">
				<tstsc:LabelEx 
                    ID="chkExecuteSetsOnlyLabel" 
                    AssociatedControlID="chkEnableWorX" 
                    Text="<%$Resources:Main,Admin_TestingSettings_ExecuteSetsOnly %>" 
                    runat="server" 
                    Required="true" 
                    AppendColon="true"
                    />
                <div class="dif">
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkExecuteSetsOnly" 
                        NoValueItem="false" 
                        />
					<asp:Label
                        CssClass="ml4" 
                        ID="litExecuteSetsOnly" 
                        runat="server" 
                        Text="<%$Resources:Main,Admin_TestingSettings_ExecuteSetsOnlyNotes %>" 
                        />
                </div>
            </li>
            <li class="ma0 mb4 pa0">
				<tstsc:LabelEx 
                    ID="chkEnableWorXLabel" 
                    AssociatedControlID="chkEnableWorX" 
                    Text="<%$Resources:Main,GeneralSettings_WorXEnabled %>" 
                    runat="server" 
                    Required="true" 
                    AppendColon="true"
                    />
                <div class="dif">
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkEnableWorX" 
                        NoValueItem="false" 
                        />
					<asp:Label
                        CssClass="ml4" 
                        ID="chkEnableWorXNotes" 
                        runat="server" 
                        Text="<%$Resources:Main,GeneralSettings_WorXEnabledNotes %>" 
                        />
                </div>
            </li>
            <li class="ma0 mb4 pa0">
				<tstsc:LabelEx 
                    ID="chkDisableRollupCalculationsLabel" 
                    AssociatedControlID="chkDisableRollupCalculations" 
                    Text="<%$Resources:Main,GeneralSettings_DisableRollupCalculations %>" 
                    runat="server" 
                    Required="true" 
                    AppendColon="true"
                    />
                <div class="dif">
					<tstsc:CheckBoxYnEx 
                        runat="server" 
                        ID="chkDisableRollupCalculations" 
                        NoValueItem="false" 
                        />
                    <div class="mw10 ml4">
					    <asp:Label                             
                            runat="server" 
                            Text="<%$Resources:Main,TestingSettings_DisableRollupCalculationsNotes %>" 
                            />
                    </div>
                </div>
            </li>
            <li class="ma0 pa0">
				<div class="btn-group ml4 mt4 ml_u-box-label">
				    <tstsc:ButtonEx 
                        ID="btnGeneralUpdate" 
                        SkinId="ButtonPrimary" 
                        runat="server" 
                        Text="<%$Resources:Buttons,Save %>" 
                        CausesValidation="True" 
                        />
				    <tstsc:ButtonEx 
                        ID="btnCancel" 
                        runat="server" 
                        Text="<%$Resources:Buttons,Cancel %>" 
                        CausesValidation="false" 
                        />
                </div>
            </li>
        </ul>
    </div>
</asp:Content>

<asp:Content ContentPlaceHolderID="cplScripts" runat="server">
	<script type="text/javascript">
		$(document).ready(function () {
			$('#<%=chkDisableRollupCalculations.ClientID%>').on('switchChange.bootstrapSwitch', function (evt, state) {
				var message = '<%=Inflectra.SpiraTest.Web.GlobalFunctions.JSEncode(Resources.Main.GeneralSettings_DisableRollupCalculationsNotes) %>';
				globalFunctions.globalAlert(message, "warning", true);
			});
		});
    </script>
</asp:Content>
