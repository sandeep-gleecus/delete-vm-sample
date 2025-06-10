<%@ Page
    AutoEventWireup="true"
    CodeBehind="TestCaseExecutionExploratory.aspx.cs"
    Inherits="Inflectra.SpiraTest.Web.TestCaseExecutionExploratory"
    Language="C#"
    MasterPageFile="~/MasterPages/Main.Master"
    Title=""
%>

<%@ Register TagPrefix="tstuc" TagName="AttachmentPanel" Src="UserControls/AttachmentPanel.ascx" %>

<%@ Import Namespace="Inflectra.SpiraTest.Web" %>




<asp:Content ID="Content2" ContentPlaceHolderID="cplMainContent" runat="server">




    <%-- Div wrapper to make sure page always looks consistent --%>
    <div id="te-container" class="vw-100 h-insideHeadAndFoot ov-hidden ">
        
        <%-- Div wrapper to enable animation --%>
        <div id="te-panels" class="relative" style="width: 200vw">


            <!-- initial wizard screen -->
            <div
                class="w-100 pt6 mvw-100 vw-100 ov-y-auto ov-x-hidden bg-near-white h-insideHeadAndFoot absolute"
                id="te-wizard-panel"
                >
                <section class="py4 px5 br3 ov-visible mvw-100 bg-white w-70 shadow-a-mid-gray gray mx-auto mt0 mb5">
                    <h2>
                        <tstsc:LabelEx
                            ID="txtWizardTitle"
                            runat="server"
                            />
                    </h2>
                    <tstsc:MessageBox
                        id="lblFirstPageMessages"
                        Runat="server"
                        SkinID="MessageBox"
                        />
                    <p
                        class="alert alert-danger alert-narrow"
                        id="no-testRunsFound-message"
                        style="display: none"
                        >
                        <span class="fas fa-exclamation-circle"></span>
                        <asp:Localize
                            ID="litAlertMessageNoTestRunsFound"
                            runat="server"
                            Text="<%$Resources:Messages,TestExecutionService_NoTestCasesWithStepsFound%>"
                            />
                    </p>
                    <p class="mb0">
                        <asp:Localize
                            ID="lblChooseReleaseAndCustomProps"
                            runat="server"
                            Text="<%$Resources:Main,TestCaseExecution_ChooseReleaseAndCustomProps %>"
                            />
                    </p>
                    <p class="mb4">
                        <asp:Localize
                            ID="lblCustomPropertiesPresetLegend"
                            runat="server"
                            Text="<%$Resources:Main,TestCaseExecution_CustomPropertiesPresetLegend %>"
                            />
                    </p>




                    <!-- fields -->
                    <div
                        class="u-wrapper width_md clearfix"
                        id="tblTestRunReleaseCustomProps"
                        enableviewstate="false"
                        >
                        <div class="u-box_1">
                            <ul class="u-box_list">
                                <li class="ma0 pa0">
                                    <tstsc:LabelEx
                                        AppendColon="true"
                                        AssociatedControlID="ddlRelease"
                                        ID="ddlReleaseLabel"
                                        Required="true"
                                        runat="server"
                                        Text="<%$Resources:Fields,Release %>"
                                        />
                                    <tstsc:UnityDropDownHierarchy
                                        AlternateItemField="IsIteration"
                                        AlternateItemImage="Images/artifact-Iteration.svg"
                                        AutoPostBack="false"
                                        ClientScriptMethod="pageWizard.ddlRelease_selectedItemChanged"
                                        DataTextField="FullName"
                                        DataValueField="ReleaseId"
                                        ID="ddlRelease"
                                        IndentLevelField="IndentLevel"
                                        ItemImage="Images/artifact-Release.svg"
                                        Runat="server"
                                        SummaryItemField="IsSummary"
                                        SummaryItemImage="Images/artifact-Release.svg"
                                        />
                                </li>
                                <li
                                    class="ma0 pa0"
                                    id="liBuildInfo"
                                    runat="server"
                                    >
                                    <tstsc:LabelEx
                                        AppendColon="true"
                                        AssociatedControlID="ddlBuild"
                                        ID="ddlBuildLabel"
                                        Required="false"
                                        runat="server"
                                        Text="<%$Resources:Fields,BuildId %>"
                                        />
                                    <tstsc:UnityDropDownListEx
                                        CssClass="u-dropdown"
                                        DataTextField="Name"
                                        DataValueField="BuildId"
                                        DisabledCssClass="u-dropdown disabled"
                                        id="ddlBuild"
                                        NoValueItem="true"
                                        NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                        Runat="server"
                                        />
                                </li>
                            </ul>
                        </div>


                        <%-- CUSTOM FIELDS --%>
                        <div class="u-box_1">
                            <ul
                                class="u-box_list"
                                id="customFieldsTestRunDefault"
                                runat="server"
                                >
                            </ul>
                        </div>
                        <div class="u-box_3 u-cke_is-minimal mbn3">
                            <ul
                                class="u-box_list labels_absolute"
                                id="customFieldsTestRunRichText"
                                runat="server"
                                >
                            </ul>
                        </div>
                        <div class="u-box_3 pl4">
                            <div class="btn-group">
                                <tstsc:ButtonEx
                                    Authorized_ArtifactType="TestRun"
                                    Authorized_Permission="Create"
                                    ClientScriptMethod="ajxTestRunFormManager_saveData(event)"
                                    ID="btnSubmitRelease"
                                    runat="server"
                                    SkinID="ButtonPrimary"
                                    Text="<%$Resources:Buttons,Next %>"
                                    />
                                <tstsc:ButtonEx
                                    CausesValidation="false"
                                    ID="btnCancel"
                                    runat="server"
                                    Text="<%$Resources:Buttons,Cancel %>"
                                    />
                            </div>
                        </div>
                    </div>
                    <tstsc:AjaxFormManager
                        ArtifactTypeName="<%$Resources:Fields,TestRun %>"
                        AutoLoad="true"
                        CheckUnsaved="false"
                        ErrorMessageControlId="lblFirstPageMessages"
                        ID="ajxTestRunFormManager"
                        runat="server"
                        WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestExecutionExploratoryService"
                        WorkflowEnabled="true"
                        >
                        <ControlReferences>
                            <tstsc:AjaxFormControl ControlId="txtWizardTitle" Direction="InOut" DataField="WizardTitle" PropertyName="textValue" />
                            <tstsc:AjaxFormControl ControlId="ddlRelease" Direction="InOut" DataField="ReleaseId" PropertyName="intValue" />
                            <tstsc:AjaxFormControl ControlId="ddlBuild" Direction="InOut" DataField="BuildId" PropertyName="intValue" />
                        </ControlReferences>
                    </tstsc:AjaxFormManager>
                </section>
            </div>





            <%-- TEST EXECUTION --%>
            <div class="container-fluid transition-all-med h-insideHeadAndFoot mvw-100 vw-100 ov-y-auto absolute left-100vw bg-white" id="te-execution-panel">
                <tstsc:RichTextBoxJ runat="server" RenderOnlyScripts="true" /><!-- Used to load the scripts for ckeditor only -->



                <%-- HEADER AREA --%>
                <div class="mt3 db" id="te-header">




                    <%-- TOP ROW --%>
                    <div class="df items-start">



                        <%-- TITLE - test case name --%>
                        <div class="textarea-resize_container mb3 mx3 shrink-1">
                            <textarea
                                class="u-input_title u-input textarea-resize_field mt2 mb1"
                                data-bind="textInput: testRunName, event: { blur: testRunNameLosesFocus }"
                                ID="txtName"
                                maxlength="255"
                                runat="server"
                                placeholder="<%$Resources:ClientScript,Artifact_EnterNewName %>"
                                rows="1"
                                ></textarea>
                        <div class="textarea-resize_checker"></div>
                        </div>





                        <%-- OPTION BUTTONS - pause, leave, customization settings --%>
                        <div
                            class="btn-group"
                            id="te-display-toolbar-options"
                            >
                            <button
                                class="btn btn-default te-pb-btn"
                                id="pb-btn-play-pause"
                                >
                                <span class="fas fa-pause"></span>
                            </button>
                            <button
                                class="btn btn-default te-pb-btn"
                                id="pbBtnLeave"
                                runat="server"
                                title="<%$Resources:Buttons,Leave%>"
                                >
                                <span class="fas fa-eject"></span>
                            </button>
                            <div class="btn-group">
                                <button
                                    aria-expanded="false"
                                    aria-haspopup="true"
                                    class="btn btn-primary dropdown-toggle"
                                    data-bind="css: { 'is-hidden': !allRunsHaveBeenRun()}"
                                    data-toggle="dropdown"
                                    id="pbBtnFinish"
                                    runat="server"
                                    title="<%$Resources:Buttons,Finish%>"
                                    type="button"
                                    >
                                    <span class="fas fa-stop"></span>
                                </button>
                                <ul class="dropdown-menu right0 left-auto bg-orange">
                                    <li>
                                        <tstsc:LinkButtonEx
                                            Authorized_ArtifactType="TestRun"
                                            Authorized_Permission="Create"
                                            CausesValidation="false"
                                            Confirmation="true"
                                            ConfirmationMessage="<%$Resources:Messages,TestCaseExecution_FinishConfirm %>"
                                            id="endTestUpdateCase"
                                            runat="server"
                                            SkinID="NoStyling"
                                            Text="<%$Resources:Buttons,UpdateTestCase %>"
                                            >
                                        </tstsc:LinkButtonEx>
                                    </li>
                                    <li>
                                        <tstsc:LinkButtonEx
                                            Authorized_ArtifactType="TestRun"
                                            Authorized_Permission="Create"
                                            CausesValidation="false"
                                            Confirmation="true"
                                            ConfirmationMessage="<%$Resources:Messages,TestCaseExecution_FinishConfirm %>"
                                            id="endTestJustEnd"
                                            runat="server"
                                            SkinID="NoStyling"
                                            Text="<%$Resources:Buttons,JustFinish %>"
                                            >
                                        </tstsc:LinkButtonEx>
                                    </li>
                                </ul>
                            </div>
                            <div class="btn-group">

                                <button
                                    aria-expanded="false"
                                    aria-haspopup="true"
                                    class="btn btn-default dropdown-toggle"
                                    data-toggle="dropdown"
                                    id="toolbar-options-btn-settings"
                                    type="button"
                                    >
                                    <span class="fas fa-bars"></span>
                                </button>
                                <ul class="dropdown-menu right0 left-auto">
                                    <li>
                                        <a href="#">
                                            <input
                                                class="fancy-checkbox"
                                                data-bind="checked: $root.userSettingsShowCaseDescription"
                                                id="te-menu-show-case-description"
                                                type="checkbox"
                                                >
                                            <label
                                                class="fancy-checkbox-label w-100"
                                                for="te-menu-show-case-description"
                                                id="lblShowCaseDescription"
                                                >
                                                <asp:Localize
                                                    ID="litOptionsShowCaseDescription"
                                                    runat="server"
                                                    Text="<%$Resources:Main,TestCaseExecution_ShowCaseDescription%>"
                                                    />
                                            </label>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="#">
                                            <input
                                                class="fancy-checkbox"
                                                data-bind="checked: $root.userSettingsShowExpectedResult"
                                                id="te-menu-show-expected-result"
                                                type="checkbox"
                                                >
                                            <label
                                                class="fancy-checkbox-label w-100"
                                                for="te-menu-show-expected-result"
                                                id="lblShowExpectedResult"
                                                >
                                                <asp:Localize
                                                    ID="litOptionsShowExpectedResult"
                                                    runat="server"
                                                    Text="<%$Resources:Main,TestCaseExecution_ShowExpectedResult%>"
                                                    />
                                            </label>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="#">
                                            <input
                                                class="fancy-checkbox"
                                                data-bind="checked: $root.userSettingsShowSampleData"
                                                id="te-menu-show-sample-data"
                                                type="checkbox"
                                                >
                                            <label
                                                class="fancy-checkbox-label w-100"
                                                for="te-menu-show-sample-data"
                                                id="lblShowSampleData"
                                                >
                                                <asp:Localize
                                                    ID="litOptionsShowSampleData"
                                                    runat="server"
                                                    Text="<%$Resources:Main,TestCaseExecution_ShowSampleData%>"
                                                    />
                                            </label>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="#">
                                            <input
                                                class="fancy-checkbox"
                                                data-bind="checked: $root.userSettingsShowCustomProperties"
                                                id="te-menu-show-custom-properties"
                                                type="checkbox"
                                                >
                                            <label
                                                class="fancy-checkbox-label w-100"
                                                for="te-menu-show-custom-properties"
                                                id="lblShowCustomProperties"
                                                >
                                                <asp:Localize
                                                    ID="litOptionsShowCustomProperties"
                                                    runat="server"
                                                    Text="<%$Resources:Main,TestCaseExecution_ShowCustomProperties%>"
                                                    />
                                            </label>
                                        </a>
                                    </li>
                                    <li>
                                        <a href="#">
                                            <input
                                                class="fancy-checkbox"
                                                data-bind="checked: $root.userSettingsShowLastResult"
                                                id="te-menu-show-last-result"
                                                type="checkbox"
                                                >
                                            <label
                                                class="fancy-checkbox-label w-100"
                                                for="te-menu-show-last-result"
                                                id="lblShowLastResult"
                                                >
                                                <asp:Localize
                                                    ID="litOptionsShowLastResult"
                                                    runat="server"
                                                    Text="<%$Resources:Main,TestCaseExecution_ShowLastResult%>"
                                                    />
                                            </label>
                                        </a>
                                    </li>
                                    <li
                                        class="divider"
                                        role="separator"
                                        ></li>
                                    <li>
                                        <a
                                            href="#"
                                            id="te-menu-show-guided-tour"
                                            >
                                            <span class="far fa-eye mr2 ml2"></span>
                                            <asp:Literal
                                                ID="litOptionsShowGuidedTour"
                                                runat="server"
                                                Text="<%$Resources:Buttons,ShowGuidedTour %>"
                                                ></asp:Literal>
                                        </a>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>




                    <%-- SECOND ROW - test case description --%>
                    <div
                        class="mx3 mbn5 mtn4 u-cke_is-minimal u-cke_shadow-on-focus"
                        data-bind="visible: $root.userSettingsShowCaseDescription"
                        >
                        <textarea
                            data-bind="richText: testRunDescription, field: 'testRunDescription', isTestRun: true"
                            id="txtDescription"
                            >
                        </textarea>
                    </div>




                    <%-- METADATA BAR --%>
                    <div class="ml3 mb3 dib bg-near-white br2 py2 px3 relative">
                        <label
                            class="mr3 my0 fw-b"
                            id="te-title-small"
                            >
                            <asp:Literal runat="server" ID="litRelease" Text="<%$Resources:Fields,Release%>"></asp:Literal>
                            <span data-bind="text: releaseVersion"></span>
                        </label>
                        TC:<span data-bind="text: testCaseId"></span>&nbsp;
                        <span class="my0">
                            ( <span data-bind="text: liveTotalRun"></span>
                            /
                            <span data-bind="text: totalTestSteps"></span>
                            <span>
                                <asp:Localize runat="server" ID="litProgressBarComplete" Text="<%$Resources:Buttons,Steps%>" />
                                )
                            </span>
                        </span>
                    </div>




                    <%-- TEST RUN CASE CUSTOM PROPERTIES --%>
                    <div class="clearfix" data-bind="stopBinding: true, visible: $root.userSettingsShowCustomProperties">
                        <tstsc:AjaxFormManager
                            AutoLoad="false"
                            CheckUnsaved="false"
                            ID="ajxTestRunCaseFormManager"
                            ErrorMessageControlId="lblFirstPageMessages"
                            runat="server"
                            WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService"
                            WorkflowEnabled="true"
                            >
                        </tstsc:AjaxFormManager>
                        <div
                            class="u-box_children-1 children-read-only"
                            ID="pnlTestRunCaseCustomProperties"
                            >
                            <ul
                                class="u-box_list pl4"
                                id="customFieldsTestRunCaseDefault"
                                runat="server"
                                ></ul>
                        </div>
                        <div class="u-box_3 u-cke_is-minimal children-read-only" onclick="function(e) {e.preventDefault()}" >
                            <ul
                                class="u-box_list pl4 labels_absolute"
                                id="customFieldsTestRunCaseRichText"
                                runat="server"
                                ></ul>
                        </div>
                    </div>



                </div>



                <%-- MESSAGE BOXES --%>
                <tstsc:MessageBox id="divNewIncidentsMessage" Runat="server" SkinID="MessageBox" />
                <tstsc:MessageBox id="divNewTasksMessage" Runat="server" SkinID="MessageBox" />
                <tstsc:MessageBox id="lblMessage" Runat="server" SkinID="MessageBox" />




                <%-- MAIN CONTENT --%>
                <div class="my3" id="te-main-content">




                    <%-- LEFT SIDERBAR GRID VIEW --%>
                    <div class="w-100 w-25-xl w-33-lg w-33-md dib fl pa3 relative" id="te-grid">
                        <!-- inline style required to allow FF to handle drag and drop properly - given ul also has overflow set: see https://bugzilla.mozilla.org/show_bug.cgi?id=339293 -->
                        <ul
                            class="border ov-y-auto pl0"
                            data-bind="foreach: testRunStep"
                            style="-moz-user-select: none"
                            >


                                <li
                                    class="js-grid-row bt b-light-gray ws-nowrap mh6 py3 pl4 pr3 o-1-child-hover df items-center justify-between ov-hidden"
                                    data-bind=
                                        "css: { 'bg-peach-light shadow-il-orange-dark selected': $data.testRunStepId == $root.chosenTestStep().testRunStepId, 'o-40 dragging': dragging, 'o-80': isReadOnly },
                                        click: pageModelSetChosen.selectTest,
                                        disable: isReadOnly,
                                        dragZone: { name: 'sortable', dragStart: $parent.dragStart, dragEnd: $parent.dragEnd},
                                        dragEvents: { accepts: 'sortable', dragOver: $parent.reorder, data: { items: $parent.testRunStep, item: $data } }"
                                    >
                                    <div class="mr3 dif mh5 lh-4 w-100 ov-hidden">
                                        <span class="fa-stack v-mid fs-66 gray mr2 shrink-0">
                                            <i
                                                class="fa-stack-2x execution-status-bg"
                                                data-bind="css: executionStatusIcon"
                                                ></i>
                                            <i
                                                class="fa-stack-1x execution-status-glyph"
                                                data-bind="css: executionStatusGlyph"
                                                ></i>
                                        </span>
                                        <span
                                            class="fw-b shrink-0"
                                            data-bind="text: positionTitle"
                                            ></span>:
                                        <!--inline style required for IE but makes this not responsive in IE-->
                                        <span
                                            class=" ml2 dib ma0-children"
                                            data-bind="html: cleanAndFilterXss(description())"
                                            style="flex-basis: calc(24vw - 130px);"
                                            ></span>
                                    </div>




                                    <%-- ACTIONS BUTTON (fr class below only needed to manage non flexbox browsers) --%>
                                    <div class="btn-group" data-bind="if: !isReadOnly()">
                                        <button
                                            aria-expanded="false"
                                            aria-haspopup="true"
                                            class="u-btn u-btn_flat dropdown-toggle pa0 ma0 b0 gray o-1-on-parent-hover fr transition-all bg-transparent orange-dark-hover"
                                            data-toggle="dropdown"
                                            type="button"
                                            >
                                            <span class="fas fa-ellipsis-h"></span>
                                        </button>
                                        <ul class="dropdown-menu right-200 left-auto top-05 min-w4 pa0 bg-peach-light">
                                            <li class="dib">
                                                <a
                                                    class="ma0"
                                                    data-bind="click: function() {pageStepCRUD.cloneStep($data.testRunStepId)}, if: !isBeingCloned(), css: {pa2: !isBeingCloned(), pa0: isBeingCloned()}"
                                                    href="#"
                                                    id="grid-menu-clone-step"
                                                    >
                                                    <span class="far fa-clone fa-fw"></span>
                                                    <asp:Literal
                                                        ID="litCloneStep"
                                                        runat="server"
                                                        Text="<%$Resources:Buttons,Clone %>"
                                                        ></asp:Literal>
                                                </a>
                                                <div
                                                    class="ma0 o-60"
                                                    data-bind="css: {pa2: isBeingCloned()}, if: isBeingCloned()"
                                                    >
                                                    <span class="fas fa-spinner fa-spin fa-fw"></span>
                                                    <asp:Literal
                                                        ID="litCloningStep"
                                                        runat="server"
                                                        Text="<%$Resources:Buttons,Cloning %>"
                                                        ></asp:Literal>
                                                </div>
                                            </li>
                                            <li class="dib">
                                                <a
                                                    class="ma0 pa2"
                                                    data-bind="click: function() {pageStepCRUD.deleteStep($data.testRunStepId)}, if: $root.testRunStep().length > 1"
                                                    href="#"
                                                    id="grid-menu-delete-step"
                                                    >
                                                    <span class="fas fa-trash-alt fa-fw"></span>
                                                    <asp:Literal
                                                        ID="litDeleteStep"
                                                        runat="server"
                                                        Text="<%$Resources:Buttons,Delete %>"
                                                        ></asp:Literal>
                                                </a>
                                            </li>
                                        </ul>
                                    </div>
                                </li>


                        </ul>
                        <button
                            class="btn btn-primary my4 h5 w5 x-center o-40 o-100-hover transition-all br-100 lh0"
                            data-bind="disable: isAdding(), click: function() { pageStepCRUD.createNewTestRunStep() }"
                            id="gridAddBtn"
                            runat="server"
                            title="<%$Resources:Buttons,Add %>"
                            type="button"
                            >
                            <i
                                class="fa tc"
                                data-bind="css: {'fa-plus': !isAdding(), 'fa-spinner fa-spin fa-fw': isAdding()}"
                                ></i>
                        </button>
                    </div>





                    <%-- INSPECTOR VIEW --%>
                    <div class="w-100 w-75-xl w-66-lg w-66-md dib fl pa3 relative" id="te-inspector">
                        <div class="border u-wrapper width_md">




                            <%-- EXECUTUION BUTTON GROUP --%>
                            <div
                                class="btn-group mb3 px0"
                                data-bind="with: chosenTestStep"
                                id="te-inspector-execution-status-btn-grp"
                                >
                                <div class="btn-group">
                                    <button
                                        class="btn btn-default shadow-ib-mid-gray-if-hover bb-gap bb-gap-peach-if-hover execution-status ExecutionStatusPassed"
                                        data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 2), css: { 'shadow-ib-mid-gray bb-gap-mid-gray': executionStatusId() == 2 }, disable: $root.appIsBusy"
                                        type="button"
                                        >
                                        <span class="fas fa-check fa-fw"></span>
                                        <span class="label-text"><asp:Literal runat="server" ID="lblBtnPass" Text="<%$Resources:Buttons,Pass %>"></asp:Literal></span>
                                    </button>
                                    <button
                                        id="btnExecutionStatusPassAll_inspector"
                                        runat="server"
                                        type="button" 
                                        class="btn btn-default dropdown-toggle execution-status ExecutionStatusPassed"
                                        data-toggle="dropdown" 
                                        aria-haspopup="true" 
                                        aria-expanded="false"
                                        data-bind="disable: $root.appIsBusy"
                                        >
                                        <span class="caret"></span>
                                        <span class="label-text caret-dropdown-mobile-fix">&nbsp;</span>
                                        <span class="sr-only">Toggle Dropdown</span>
                                    </button>
                                    <ul class="dropdown-menu ma0 pa0 min-w4 ExecutionStatusPassed execution-status-bg">
                                        <li class="ma0 pa0">
                                            <a href="#" class="ma0 pa3" data-bind="click: pageModelStatusUpdate.handlePassAll.bind($data, true), disable: $root.appIsBusy">
                                                <span class="fas fa-check fa-fw"></span>
                                                <asp:Literal runat="server" ID="lblBtnPassAll" Text="<%$Resources:Buttons,PassAll %>"></asp:Literal>
                                            </a>
                                        </li>
                                    </ul>
                                </div>
                                <button
                                    id="btnExecutionStatusBlocked_inspector"
                                    runat="server"
                                    class="btn btn-default shadow-ib-mid-gray-if-hover bb-gap bb-gap-peach-if-hover execution-status ExecutionStatusBlocked"
                                    data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 5), css: { 'shadow-ib-mid-gray bb-gap-mid-gray': executionStatusId() == 5 }, disable: $root.appIsBusy"
                                    type="button"
                                    >
                                    <span class="fas fa-ban fa-fw"></span>
                                    <span class="label-text">
                                        <asp:Literal runat="server" ID="lblBtnBlocked" Text="<%$Resources:Buttons,Blocked %>"></asp:Literal
                                    ></span>
                                </button>
                                <button
                                    id="btnExecutionStatusCaution_inspector"
                                    runat="server"
                                    class="btn btn-default shadow-ib-mid-gray-if-hover bb-gap bb-gap-peach-if-hover execution-status ExecutionStatusCaution"
                                    data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 6), css: { 'shadow-ib-mid-gray bb-gap-mid-gray': executionStatusId() == 6 }, disable: $root.appIsBusy"
                                    type="button"
                                    >
                                    <span class="fas fa-exclamation-triangle fa-fw"></span>
                                    <span class="label-text">
                                        <asp:Literal runat="server" ID="lblBtnCaution" Text="<%$Resources:Buttons,Caution %>"></asp:Literal
                                    ></span>
                                </button>
                                <button
                                    class="btn btn-default shadow-ib-mid-gray-if-hover bb-gap bb-gap-peach-if-hover execution-status ExecutionStatusFailed"
                                    data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 1), css: { 'shadow-ib-mid-gray bb-gap-mid-gray': executionStatusId() == 1 }, disable: $root.appIsBusy"
                                    type="button"
                                    >
                                    <span class="fas fa-times fa-fw"></span>
                                    <span class="label-text">
                                        <asp:Literal runat="server" ID="lblBtnFail" Text="<%$Resources:Buttons,Fail %>"></asp:Literal
                                    ></span>
                                </button>
                                <button
                                    id="btnExecutionStatusNA_inspector"
                                    runat="server"
                                    class="btn btn-default shadow-ib-mid-gray-if-hover bb-gap bb-gap-peach-if-hover execution-status ExecutionStatusNotApplicable"
                                    data-bind="click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, 4), css: { 'shadow-ib-mid-gray bb-gap-mid-gray': executionStatusId() == 4 }, disable: $root.appIsBusy"
                                    type="button"
                                    >
                                    <span class="fas fa-minus fa-fw"></span>
                                    <span class="label-text">N/A</span>
                                </button>
                            </div>





                            <%-- TEST RUN STEP FIELDS --%>
                            <div class="u-box_3">
                                <p
                                    class="alert alert-warning alert-narrow"
                                    data-bind="visible: $root.chosenPendingStatusUpdate"
                                    >
                                    <span data-bind="text: $root.chosenPendingWarningMessage"></span>
                                </p>
                                <ul class="u-box_list mt3 mbn4 u-cke_is-minimal labels_absolute" data-bind="css: {'o-80': chosenTestStep().isReadOnly}">
                                    <li class="mt0 pa0 mbn4">
                                        <h3 class="mt3 mbn5 df mln4">
                                            <span class="fa-stack fs-50 bg-transparent gray mr3">
                                                <i class="fa-stack-2x eecution-status-bg" data-bind="css: chosenTestStep().executionStatusIcon"></i>
                                                <i class="fa-stack-1x execution-status-glyph" data-bind="css: chosenTestStep().executionStatusGlyph"></i>
                                            </span>
                                            <span data-bind="text: chosenTestStep().positionTitle"></span>
                                        </h3>
                                        <textarea
                                            data-bind="richText: chosenTestStep().description, field: 'description'"
                                            id="testRunStepDescription"
                                            >
                                        </textarea>
                                    </li>
                                    <li
                                        class="mt0 pa0 mbn4"
                                        data-bind="visible: $root.userSettingsShowExpectedResult"
                                        >
                                        <textarea
                                            data-bind="richText: chosenTestStep().expectedResult, field: 'expectedResult'"
                                            id="testRunStepExpectedResult"
                                            >
                                        </textarea>
                                        <label class="pl1 fw-b">
                                            <asp:Localize
                                                runat="server"
                                                ID="lblExpectedResultInspector"
                                                Text="<%$Resources:Main,TestCaseExecution_ExpectedResult%>"
                                                />
                                        </label>
                                    </li>
                                    <li
                                        class="mt0 pa0 mbn4"
                                        data-bind="visible: $root.userSettingsShowSampleData"
                                        >
                                        <textarea
                                            data-bind="richText: chosenTestStep().sampleData, field: 'sampleData'"
                                            id="testRunStepSampleData"
                                            >
                                        </textarea>
                                        <label class="pl1 fw-b">
                                            <asp:Localize
                                                runat="server"
                                                ID="lblSampleDataInspector"
                                                Text="<%$Resources:Main,TestCaseExecution_SampleData%>"
                                                />
                                        </label>
                                    </li>
                                </ul>




                                <!-- results from last time this test step was run, if at all  -->
                                <ul 
                                    class="u-box_list mt0 mb3" 
                                    data-bind="if: $root.userSettingsShowLastResult"
                                    >
                                    <li class="ma0 mb2 pa0">
                                        <label class="pl1 fw-b">
                                            <asp:Localize
                                                runat="server"
                                                ID="lblLastResultInspector"
                                                Text="<%$Resources:Main,TestCaseExecution_LastResult%>"
                                                />
                                            <span 
                                            class="db o-70"
                                            data-bind="if: chosenTestStep().lastEndDate"
                                            >
                                                (<span data-bind="text: chosenTestStep().lastEndDate"></span>)
                                            </span>
                                        </label>

                                        <div class="dib bg-near-white br2 py2 px3 ml2 u-box_list_control-inline">
                                            <!-- ko if: chosenTestStep().lastExecutionStatusId && chosenTestStep().lastExecutionStatusId != 3 -->
                                                <div class="responsive-images resize-v h6 ov-y-auto">
                                                    <span class="fa-stack bg-transparent gray mr3">
                                                        <i class="fa-stack-2x execution-status-bg" data-bind="css: chosenTestStep().lastExecutionStatusIcon"></i>
                                                        <i class="fa-stack-1x execution-status-glyph" data-bind="css: chosenTestStep().lastExecutionStatusGlyph"></i>
                                                    </span>
                                                    <label
                                                        class="mr3 my0 fw-b"
                                                        id="te-title-small"
                                                        >
                                                        <asp:Literal runat="server" ID="litLastRelease" Text="<%$Resources:Fields,Release%>"></asp:Literal>
                                                        <span data-bind="text: $root.lastReleaseVersion"></span>
                                                    </label>
                                                    <div class="ml2" data-bind="html: cleanAndFilterXss(chosenTestStep().lastActualResult)"></div>
                                                </div>
                                            <!-- /ko -->

                                            <!-- ko ifnot: chosenTestStep().lastExecutionStatusId && chosenTestStep().lastExecutionStatusId != 3 -->
                                                <div class="dib bg-near-white br2 py2 px3 mr2">
                                                    <asp:Localize
                                                        runat="server"
                                                        ID="lblLastResultNoneAvailable"
                                                        Text="<%$Resources:Fields,NotRun%>"
                                                        />
                                                </div>
                                            <!-- /ko -->
                                        </div>
                                    </li>
                                </ul>





                                <!--separate actual result so that other fields - not normally editable during test execution can be styled separately as needed-->
                                <ul class="u-box_list mt0 mb3 u-cke_is-minimal labels_absolute">
                                    <li class="mt0 pa0 mbn4">
                                        <textarea
                                            data-bind="richText: chosenTestStep().actualResult, field: 'actualResult'"
                                            id="testRunStepActualResult"
                                            >
                                        </textarea>
                                        <label class="pl1 fw-b">
                                            <asp:Localize
                                                runat="server"
                                                ID="lblActualResultInspector"
                                                Text="<%$Resources:Main,TestCaseExecution_ActualResult%>"
                                                />
                                        </label>
                                    </li>
                                    <li class="ma0 pa0">
                                        <div class="btn-group">
                                            <button type="button" class="btn btn-default execution-status my3" data-bind="visible: $root.chosenPendingStatusUpdate, css: $root.chosenPendingStatusBgClass, click: pageModelStatusUpdate.handleStatusUpdateAttempt.bind($data, $root.chosenPendingStatusUpdate())">
                                                <span class="fas fa-fw" data-bind="css: $root.chosenPendingStatusGlyph"></span>
                                                <span data-bind="text: $root.chosenPendingStatusName"></span>
                                            </button>
                                        </div>
                                    </li>
                                </ul>
                            </div>




                            <%-- TEST RUN STEP CUSTOM PROPERTIES --%>
                            <div data-bind="stopBinding: true, visible: $root.userSettingsShowCustomProperties">
                                <tstsc:AjaxFormManager
                                    AutoLoad="false"
                                    CheckUnsaved="false"
                                    ErrorMessageControlId="lblFirstPageMessages"
                                    ID="ajxTestRunStepFormManager"
                                    runat="server"
                                    WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.TestStepService"
                                    WorkflowEnabled="true"
                                    >
                                </tstsc:AjaxFormManager>
                                <div
                                    class="u-box_children-1 children-read-only"
                                    ID="pnlTestRunStepCustomProperties"
                                    >
                                    <ul
                                        class="u-box_list pl4"
                                        id="customFieldsTestRunStepDefault"
                                        runat="server"
                                        ></ul>
                                </div>
                                <div class="u-box_3 u-cke_is-minimal children-read-only" >
                                    <ul
                                        class="u-box_list pl4 labels_absolute"
                                        id="customFieldsTestRunStepRichText"
                                        runat="server"
                                        ></ul>
                                </div>
                            </div>




                            <%-- TABS --%>
                            <div class="row px4">
                                <ul class="nav nav-tabs te-tab-buttons mb3 mt4" role="tablist">
                                    <li role="presentation" data-bind="if: pageProps.canViewTasks, css: {active: pageProps.canViewTasks}">
                                        <a href="#te-inspector_tab_tasks" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_tasks">
                                            <tstsc:ImageEx 
                                                CssClass="w4 h4 mr3"
                                                ImageUrl="Images/artifact-Task.svg"
                                                runat="server"
                                                />
                                            <asp:Localize ID="lblTasks" runat="server" Text="<%$Resources:Fields,Tasks%>" />
                                        </a>
                                    </li>
                                    <li role="presentation" data-bind="css: {active: !pageProps.canViewTasks}">
                                        <a href="#te-inspector_tab_incidents" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_incidents" data-bind="click: function() { pageControls.handleIncidentTabClick() }">
                                            <tstsc:ImageEx 
                                                CssClass="w4 h4 mr3"
                                                ImageUrl="Images/artifact-Incident.svg"
                                                runat="server"
                                                />
                                            <asp:Localize ID="lblTabIncidents" runat="server" Text="<%$Resources:ServerControls,TabControl_Incidents%>" />
                                            <i class="fas fa-asterisk ml1 light-silver fs-80 fw-b" data-bind="visible: _pageInfo.requireIncident && $root.chosenTestStepIncidents().length"></i> 
                                        </a>
                                    </li>
                                    <li role="presentation">
                                        <a href="#te-inspector_tab_attachments" role="tab" data-toggle="tab" id="te-inspector-tab-buttons_attachments">
                                            <tstsc:ImageEx 
                                                CssClass="w4 h4 mr3"
                                                ImageUrl="Images/artifact-Document.svg"
                                                runat="server"
                                                />
                                            <asp:Localize ID="lblTabAttachments" runat="server" Text="<%$Resources:ServerControls,TabControl_Attachments%>" />
                                        </a>
                                    </li>
                                </ul>
                            </div>





                            <div class="tab-content">


                                <%-- TASKS PANEL --%>
                                <div 
                                    role="tabpanel" 
                                    class="tab-pane" 
                                    id="te-inspector_tab_tasks" 
                                    data-bind="if: pageProps.canViewTasks, css: {active: pageProps.canViewTasks}"
                                    >
                                    <section class="u-wrapper width_md">
                                        <div class="u-box_3">
                                            <ul class="u-box_list u-cke_is-minimal labels_absolute">
                                                <li class="ma0 pa0 mb4 fs-h4">
                                                    <tstsc:UnityTextBoxEx
                                                        CssClass="u-input is-active"
                                                        data-bind="textInput: taskName"
                                                        ID="txtTaskName"
                                                        MaxLength="255"
                                                        PlaceHolder="<%$ Resources:Fields,TestCase_Execution_EnterTaskName %>"
                                                        runat="server"
                                                        TextMode="SingleLine"
                                                        />
                                                </li>
                                                <li class="ma0 pa0">
                                                    
                                                </li>
                                                <li class="ma0 pa0 mb2">
                                                    <textarea
                                                        data-bind="richText: taskDescription, field: 'taskDescription', isTestRun: true"
                                                        id="taskDescription"
                                                        >
                                                    </textarea>
                                                    <label class="pl1 fw-b ov-hidden ws-nowrap" style="width: 100%;">
                                                        <asp:Localize
                                                            runat="server"
                                                            ID="lblTaskDescription"
                                                            Text="<%$Resources:Fields,Description%>"
                                                            />
                                                        <asp:HyperLink 
                                                            class="light-gray transition-all fs-90 pointer ml2"
                                                            onclick="pageManageTasks.setDescriptionFromTestStep();" 
                                                            runat="server"
                                                            Text="<%$Resources:Main,TestCaseExecution_SetTaskDescriptionFromTestStep%>"
                                                            Title="<%$Resources:Main,TestCaseExecution_SetTaskDescriptionFromTestStep%>"
                                                            />
                                                    </label>
                                                </li>
                                            </ul>
                                        </div>
                                        <div class="u-box_1">
                                            <ul class="u-box_list">
                                                <li class="mx0 pa0 mtn4 mb2">
                                                    <tstsc:LabelEx
                                                        AppendColon="true"
                                                        AssociatedControlID="ddlTaskOwner"
                                                        ID="lblTaskOwner"
                                                        runat="server"
                                                        Text="<%$Resources:Fields,OwnerId %>"
                                                        />
                                                    <tstsc:UnityDropDownUserList
                                                        CssClass="u-dropdown u-dropdown_user"
                                                        ID="ddlTaskOwner"
                                                        DataTextField="FullName"
                                                        DataValueField="UserId"
                                                        DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                                        NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                        NoValueItem="True"
                                                        runat="server"
                                                        />
                                                </li>
                                            </ul>
                                        </div>




                                        <!-- list of existing tasks  -->
                                        <div class="u-box_3" data-bind="if: tasks().length > 0">
                                            <ul class="pa0 df flex-wrap items-start" data-bind="foreach: tasks">
                                                <li class="pr4 py3 ma3 ba b-light-gray br2 dib w8 dif">
                                                    <div 
                                                        class="fl w3 myn3 mr4"
                                                        data-bind="style: { backgroundColor: '#' + priorityColor }"
                                                        ></div>
                                                    <a data-bind="attr: { href: url }" target="_blank">
                                                        <span data-bind="text: taskToken"></span>&nbsp;
                                                        <span data-bind="text: name"></span>
                                                    </a>
                                                </li>
                                            </ul>
                                        </div>

                                        <button 
                                            class="btn btn-primary mx4 my3"
                                            data-bind="enable: hasTaskName"
                                            id="btnNewTask"
                                            type="button"
                                            >
                                            <asp:Literal runat="server" Text="<%$Resources:Buttons,Add %>" />
                                        </button>
                                    </section>
                                </div>




                                <%-- ATTACHMENT PANEL --%>
                                <div role="tabpanel" class="tab-pane" id="te-inspector_tab_attachments" data-bind="stopBinding: true">
                                    <p class="alert alert-narrow alert-info center">
                                        <asp:Literal runat="server" ID="msgAddScreenshotUsingEditor2" Text="<%$Resources:Messages,TestCaseExecution_AddScreenshotUsingEditor %>"></asp:Literal>
                                    </p>
                                    <asp:Panel runat="server" ID="pnlTestExecution_Attachments">
                                        <tstuc:AttachmentPanel id="tstAttachmentPanel" Runat="server" ArtifactTypeEnum="TestRun" />
                                    </asp:Panel>
                                </div>




                                <%-- INCIDENT PANEL --%>
                                <div 
                                    role="tabpanel" 
                                    class="tab-pane" 
                                    id="te-inspector_tab_incidents" 
                                    data-bind="css: {active: !pageProps.canViewTasks}"
                                    >
                                    <p class="pa3" >


                                        <span 
                                            class="mr4"
                                            data-bind="visible: ($root.chosenTestStepIncidents().length === 0)"
                                            >
                                            <asp:literal runat="server" ID="msgNoExistingIncidents" Text="<%$Resources:Main,TestCaseExecution_NoExistingIncident%>" />
                                        </span>
                                        <span 
                                            class="mr4"
                                            data-bind="visible: $root.chosenTestStepIncidents().length > 0"
                                            >
                                            <asp:literal runat="server" ID="Literal1" Text="<%$Resources:Main,TestCaseExecution_ExistingIncidents%>" />
                                        </span>
                                        
                                        
                                        <tstsc:HyperLinkEx 
                                            SkinID="ButtonDefaultSmall" 
                                            runat="server" 
                                            ID="btnLinkExistingIncident" 
                                            Text="<%$Resources:Main,TestCaseExecution_LinkExisting %>" 
                                            ClientScriptMethod="pageManageIncidentForm.associateExistingIncident()" 
                                            Authorized_Permission="Modify" 
                                            Authorized_ArtifactType="Incident" 
                                            NavigateUrl="javascript:void(0)" 
                                            />
                                    </p>

                                    <div id="associateIncidentPanel"></div>

                                    <div data-bind="visible: $root.chosenTestStepIncidents().length > 0">
                                        <table id="tblExistingIncidents" class="DataGrid" style="width:100%; margin-top:10px;">
                                            <thead>
                                                <tr class="Header">
                                                    <th class="priority3"><asp:Localize ID="lblIncidentIdGrid" runat="server" Text="<%$Resources:Fields,IncidentId %>" /></th>
                                                    <th class="priority1"><asp:Localize ID="lblIncidentNameGrid" runat="server" Text="<%$Resources:Fields,Name %>" /></th>
                                                    <th class="priority1"><asp:Localize ID="lblIncidentTypeIdGrid" runat="server" Text="<%$Resources:Fields,IncidentTypeId %>" /></th>
                                                    <th class="priority2"><asp:Localize ID="lblIncidentStatusIdGrid" runat="server" Text="<%$Resources:Fields,IncidentStatusId %>" /></th>
                                                    <th class="priority4"><asp:Localize ID="lblIncidentPriorityIdGrid" runat="server" Text="<%$Resources:Fields,PriorityId %>" /></th>
                                                    <th class="priority4"><asp:Localize ID="lblIncidentSeverityIdGrid" runat="server" Text="<%$Resources:Fields,SeverityId %>" /></th>
                                                    <th class="priority2"><asp:Localize ID="lblIncidentOwnerIdGrid" runat="server" Text="<%$Resources:Fields,OwnerId %>" /></th>
                                                    <th class="priority4"><asp:Localize ID="lblIncidentCreationDateGrid" runat="server" Text="<%$Resources:Fields,CreationDate %>" /></th>
                                                    <th class="priority4"><asp:Localize ID="lblIncidentOpenerIdGrid" runat="server" Text="<%$Resources:Fields,OpenerId %>" /></th>
                                                </tr>
                                            </thead>
                                            <tbody data-bind="foreach: $root.chosenTestStepIncidents">
                                                <tr class="Normal">
                                                    <td data-bind="text: incidentId" class="priority3">
                                                    </td>
                                                    <td class="priority1">
                                                        <tstsc:ImageEx ID="imgIncident" runat="server" CssClass="w4 h4" ImageUrl="Images/artifact-Incident.svg" AlternateText="Incident" />
                                                        <a data-bind="text: name,
                                                            attr: { href: _pageInfo.get_incidentBaseUrl().replace(globalFunctions.artifactIdToken, $data.incidentIdIntValue) },
                                                            event: { mouseover: pageTooltips.displayIncidentTooltip.bind($data), mouseout: pageTooltips.hideIncidentTooltip.bind() }">
                                                        </a>
                                                    </td>
                                                    <td class="priority1" data-bind="text: incidentType"></td>
                                                    <td class="priority2" data-bind="text: incidentStatus"></td>
                                                    <td class="priority4" data-bind="text: globalFunctions.displayIfDefined(priorityValue), style: { backgroundColor: (priorityId && priorityClass) ? '#' + priorityClass : '', whiteSpace: 'nowrap' }" ></td>
                                                    <td class="priority4" data-bind="text: globalFunctions.displayIfDefined(severityValue), style: { backgroundColor: (severityId && severityClass) ? '#' + severityClass : '', whiteSpace: 'nowrap' }"></td>
                                                    <td class="priority2" data-bind="text: globalFunctions.displayIfDefined(owner)"></td>
                                                    <td class="priority4" data-bind="text: creationDate"></td>
                                                    <td class="priority4" data-bind="text: opener"></td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>





                                    <%-- INCIDENT FORM FOR ADDING NEW INCIDENT --%>
                                    <asp:Panel runat="server" ID="pnlTestExecution_Incidents">
                                        <section class="u-wrapper width_md">
                                            <div class="u-box_3">
                                                <tstsc:MessageBox
                                                    ID="msgNewIncidentMessageInline"
                                                    runat="server"
                                                    SkinID="MessageBox"
                                                    />
                                                <div class="u-box_group" >
                                                    <div
                                                        class="u-box_header fs-h6 orange-dark bb b-orange-dark w-100 db mb3 pl2 pt3"
                                                        aria-expanded="false">
                                                        <asp:Localize
                                                            runat="server"
                                                            Text="<%$Resources:ServerControls,TabControl_NewIncident %>" />
                                                    </div>

                                                     <%-- warning message to show when an incident is required and has not yet been added --%>
                                                    <p class="alert alert-error my4 fw-110" id="msgIncidentRequired" data-bind="visible: $root.showAddIncidentMessage">
                                                        <asp:Localize 
                                                            ID="Localize1" 
                                                            runat="server" 
                                                            Text="<%$Resources:Main,TestCaseExecution_IncidentRequired %>" 
                                                            />
                                                    </p>

                                                    <p class="my3 fs-110 pa3 bg-info br3">
                                                        <asp:Localize
                                                            ID="lblEnterIncidentInfo"
                                                            runat="server"
                                                            Text="<%$Resources:Main,TestCaseExecution_EnterIncidentInfo %>"
                                                            />
                                                    </p>
                                                    <div class="u-box_3">
                                                        <ul class="u-box_list">
                                                            <li class="ma0 pa0 mb4 fs-h4">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="txtIncidentName"
                                                                    ID="lblIncidentName"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,Name %>"
                                                                    />
                                                                <asp:HiddenField ID="hdnIncidentStatus" runat="server" />
                                                                <tstsc:UnityTextBoxEx
                                                                    CssClass="u-input is-active"
                                                                    ClientIDMode="Static"
                                                                    ID="txtIncidentName"
                                                                    MaxLength="255"
                                                                    PlaceHolder="<%$ Resources:Fields,TestCase_Execution_EnterIncidentName %>"
                                                                    runat="server"
                                                                    TextMode="SingleLine"
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul class="u-box_list">
                                                            <li class="ma0 pa0 mb2">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlIncidentType"
                                                                    ID="lblIncidentType"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,IncidentTypeId %>"
                                                                    />
                                                                <tstsc:UnityDropDownListEx
                                                                    CssClass="u-dropdown"
                                                                    DataMember="IncidentType"
                                                                    DataTextField="Name"
                                                                    DataValueField="IncidentTypeId"
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlIncidentType"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0 mb2">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlResolvedRelease"
                                                                    ID="lblResolvedRelease"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,ResolvedReleaseId %>"
                                                                    />
                                                                <tstsc:UnityDropDownHierarchy
                                                                    ActiveItemField="Isactive"
                                                                    DataTextField="FullName"
                                                                    DataValueField="ReleaseId"
                                                                    ID="ddlResolvedRelease"
                                                                    NoValueItem="True"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                            <li class="ma0 pa0 mb2">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlVerifiedRelease"
                                                                    ID="lblVerifiedRelease"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,VerifiedReleaseId %>"
                                                                    />
                                                                <tstsc:DropDownHierarchy
                                                                    ActiveItemField="IsActive"
                                                                    DataTextField="FullName"
                                                                    DataValueField="ReleaseId"
                                                                    ID="ddlVerifiedRelease"
                                                                    NoValueItem="True"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                        </ul>
                                                        <ul
                                                            class="u-box_list"
                                                            id="customFieldsIncidentsUsers"
                                                            runat="server"
                                                            >
                                                            <li class="ma0 pa0 mb2">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlOwner"
                                                                    ID="lblOwner"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,OwnerId %>"
                                                                    />
                                                                <tstsc:UnityDropDownUserList
                                                                    CssClass="u-dropdown u-dropdown_user"
                                                                    ID="ddlOwner"
                                                                    DataTextField="FullName"
                                                                    DataValueField="UserId"
                                                                    DisabledCssClass="u-dropdown u-dropdown_user disabled"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                                    NoValueItem="True"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul
                                                            class="u-box_list"
                                                            id="customFieldsIncidentsDefault"
                                                            runat="server"
                                                            >
                                                            <li class="ma0 mb2 pa0">
                                                                <tstsc:LabelEx
                                                                    AssociatedControlID="ddlPriority"
                                                                    ID="lblPriority"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,PriorityId %>"
                                                                    />
                                                                <tstsc:UnityDropDownListEx
                                                                    CssClass="u-dropdown"
                                                                    DataMember="IncidentPriority"
                                                                    DataTextField="Name"
                                                                    DataValueField="PriorityId"
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlPriority"
                                                                    NoValueItem="True"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                            <li class="ma0 mb2 pa0">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlSeverity"
                                                                    ID="lblSeverity"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,SeverityId %>"
                                                                    />
                                                                <tstsc:UnityDropDownListEx
                                                                    CssClass="u-dropdown"
                                                                    DataMember="IncidentSeverity"
                                                                    DataTextField="Name"
                                                                    DataValueField="SeverityId"
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlSeverity"
                                                                    NoValueItem="True"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown%>"
                                                                    runat="server"
                                                                    />
                                                            </li>
                                                            <li class="ma0 mb2 pa0">
                                                                <tstsc:LabelEx
                                                                    AppendColon="true"
                                                                    AssociatedControlID="ddlComponent"
                                                                    ID="ddlComponentLabel"
                                                                    runat="server"
                                                                    Text="<%$Resources:Fields,ComponentId %>"
                                                                    />
                                                                <tstsc:UnityDropDownMultiList
                                                                    ActiveItemField="IsActive"
                                                                    CssClass="u-dropdown"
                                                                    DataTextField="Name"
                                                                    DataValueField="ComponentId"
                                                                    DisabledCssClass="u-dropdown disabled"
                                                                    ID="ddlComponent"
                                                                    NoValueItem="True"
                                                                    NoValueItemText="<%$Resources:Dialogs,Global_NoneDropDown %>"
                                                                    runat="server"
                                                                    SelectionMode="Multiple"
                                                                    />
                                                            </li>
                                                        </ul>
                                                    </div>
                                                    <div class="u-box_1">
                                                        <ul
                                                            class="u-box_list"
                                                            id="customFieldsIncidentsDates"
                                                            runat="server"
                                                            ></ul>
                                                    </div>
                                                    <div class="u-box_3 u-cke_is-minimal">
                                                        <ul
                                                            class="u-box_list labels_absolute"
                                                            id="customFieldsIncidentsRichText"
                                                            runat="server"
                                                            ></ul>
                                                    </div>
                                                </div>
                                            </div>
                                        </section>

                                        <div class="pa4">
                                            <tstsc:HyperLinkEx 
                                                SkinID="ButtonPrimary" 
                                                id="newIncidentAdd" 
                                                runat="server" 
                                                NavigateUrl="javascript:void(0)" 
                                                Text="<%$Resources:Buttons,Add %>" 
                                                ClientScriptMethod="pageManageIncidentForm.newIncidentAdd_click()" 
                                                />
                                        </div>

                                        <tstsc:AjaxFormManager ID="ajxIncidentFormManager" runat="server" ArtifactTypeName="<%$Resources:Fields,TestRun %>" AutoLoad="true" CheckUnsaved="false"
                                                ErrorMessageControlId="divNewIncidentsMessage" WebServiceClass="Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService" WorkflowEnabled="true">
                                            <ControlReferences>
                                                <tstsc:AjaxFormControl ControlId="txtIncidentName" DataField="Name" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlOpener" DataField="OpenerId" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlIncidentType" DataField="IncidentTypeId" Direction="InOut" ChangesWorkflow="true" />
                                                <tstsc:AjaxFormControl ControlId="ddlPriority" DataField="PriorityId" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlSeverity" DataField="SeverityId" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlOwner" DataField="OwnerId" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlResolvedRelease" DataField="ResolvedReleaseId" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlVerifiedRelease" DataField="VerifiedReleaseId" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="ddlComponent" DataField="ComponentIds" Direction="InOut" />
                                                <tstsc:AjaxFormControl ControlId="hdnIncidentStatus" DataField="IncidentStatusId" Direction="In" IsWorkflowStep="true" />
                                            </ControlReferences>
                                        </tstsc:AjaxFormManager>
                                    </asp:Panel>




                                    <%-- ALERT IF NO TEST STEPS FOUND --%>
                                    <asp:Panel ID="pnlNoTestStepsFound" runat="server" Style="display:none">
                                        <div class="ma4">
                                            <h2>
                                                <asp:Localize runat="server" ID="msgNoStepsAvailableForTestCase" Text="<%$Resources:Main,TestCaseExecution_NoStepsAvailableForTestCase %>" />
                                            </h2>
                                            <p>
                                                <asp:Localize  runat="server" ID="msgTestCaseExecution_ChooseTestStep1" Text="<%$Resources:Main,TestCaseExecution_ChooseTestStep1 %>" />
                                                <tstsc:LinkButtonEx ID="btnNoStepsBackToTests" runat="server" CausesValidation="false" Text="<%$Resources:Main,TestCaseExecution_ChooseTestStep2 %>" />
                                                <asp:Localize runat="server" ID="msgTestCaseExecution_ChooseTestStep2" Text="<%$Resources:Main,TestCaseExecution_ChooseTestStep3 %>" />
                                            </p>
                                        </div>
                                    </asp:Panel>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

        </div>
</div>


    <tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
        <Services>
        <asp:ServiceReference Path="~/Services/Ajax/TestExecutionExploratoryService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/TestCaseService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/TestStepService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/AssociationService.svc" /> 
        <asp:ServiceReference Path="~/Services/Ajax/IncidentsService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/TasksService.svc" />
        <asp:ServiceReference Path="~/Services/Ajax/BuildService.svc" />
        </Services>

        <Scripts>
            <asp:ScriptReference Name="Inflectra.SpiraTest.Web.ClientScripts.hopscotch.min.js" Assembly="Web" />
            <asp:ScriptReference Path="~/TypeScript/DetailsPage.js" />
        </Scripts>
    </tstsc:ScriptManagerProxyEx>
</asp:Content>




<asp:Content ContentPlaceHolderID="cplScripts" ID="teCplScripts" runat="server">
    <script type="text/javascript">
        SpiraContext.HasCollapsiblePanels = false;

        _pageInfo = {
            projectId: <%=ProjectId%>,
            userId: <%=UserId%>,
            testRunsPendingId: <%=testRunsPendingId%>,
            clientId: '<%=lblMessage.ClientID%>',
            ddlRelease: '<%=ddlRelease.ClientID %>',
            ddlBuild: '<%=ddlBuild.ClientID %>',
            pbBtnLeave: '<%=pbBtnLeave.ClientID %>',
            pbBtnFinish: '<%=pbBtnFinish.ClientID %>',
            gridAddBtn: '<%=gridAddBtn.ClientID %>',
            btnSubmitRelease: '<%=btnSubmitRelease.ClientID %>',
            endTestUpdateCase: '<%=endTestUpdateCase.ClientID %>',
            appBusy: false,
            pnlTestExecution_Attachments: '<%=pnlTestExecution_Attachments.ClientID%>',
            artifactTypeFilter_TestRun:'<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestRun%>',
            artifactTypeFilter_TestCase:'<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestCase%>',
            artifactTypeFilter_TestStep:'<%=(int)Inflectra.SpiraTest.DataModel.Artifact.ArtifactTypeEnum.TestStep%>',
            txtIncidentName: '<%=txtIncidentName.ClientID%>',
            ajxIncidentFormManager: '<%=ajxIncidentFormManager.ClientID%>',
            ajxTestRunFormManager: '<%=ajxTestRunFormManager.ClientID%>',
            btnLinkExistingIncident: '<%=btnLinkExistingIncident.ClientID%>',
            divNewIncidentsMessage: '<%=divNewIncidentsMessage.ClientID%>',
            msgNewIncidentMessageInline : '<%=msgNewIncidentMessageInline.ClientID%>',
            ajxTestRunCaseFormManager: '<%=ajxTestRunCaseFormManager.ClientID%>',
            ajxTestRunStepFormManager: '<%=ajxTestRunStepFormManager.ClientID%>',
            addIncidentArea: '<%=pnlTestExecution_Incidents.ClientID%>',
            referrerUrl: '<%=ReferrerUrl%>',
            fullReferrerUrl: '<%=FullReferrerUrl%>',
            incidentBaseUrl: '<%=IncidentBaseUrl%>',
            get_incidentBaseUrl: function() {
                return _pageInfo.incidentBaseUrl;
            },
            screenshotUploadUrl: '<%=ScreenshotUploadUrl%>',
            ddlTaskOwner: '<%=ddlTaskOwner.ClientID%>',
            actualResultAlwaysRequired: '<%=ActualResultAlwaysRequired%>' == "true",
            requireIncident: '<%=RequireIncident%>' == "true",
            allowTasks: '<%=AllowTasks%>' == "true"
        };

        //fx to save the data, and load the main page. Coded here as opposed to through the btn control
        //this allows us to disable the button given that this save operation can take a while and is a necessary gateway to the main page
        //disabling means users won't double click, and get visual feedback on what is going on
        ajxTestRunFormManager_saveData=function(event) {
            var saveBtn = document.getElementById("<%=btnSubmitRelease.ClientID%>");
            saveBtn.disabled=true;
            saveBtn.value=resx.GlobalFunctions_SpinnerText;
            $find(_pageInfo.ajxTestRunFormManager).save_data(event, null, true)
        }

        //this function deals with [IN:4666] where users with bad formatted and imported data see html and body tags in their test execution
        //previously we just applied filterXSS to the string in question, so now, before that, we remove html tags that bother customers
        cleanAndFilterXss = function (string) {
            // only get rid of the tags if they are at the very start and very end of the string
            var cleanedString = string;
            var regexStart = /^\<html\>[^a-zA-Z0-9]*\<body\>/;
            var regexEnd = /\<\/body\>[^a-zA-Z0-9]*\<\/html\>$/;
            cleanedString = cleanedString.replace(regexStart, "").replace(regexEnd, "");
            return filterXSS(cleanedString, filterXssInlineStyleOptions);
        }


    </script>
    <script src='<%= Inflectra.SpiraTest.Web.Classes.UrlRewriterModule.ResolveUrl("~/TestCaseExecutionExploratory.aspx.js") + "?v=" + GlobalFunctions.SYSTEM_VERSION_NUMBER %>' type="text/javascript"></script>
</asp:Content>
