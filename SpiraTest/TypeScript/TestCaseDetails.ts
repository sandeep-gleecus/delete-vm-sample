//external dependendencies (js libraries)
declare var $: any;
declare var $find: any;
declare var $get: any;
declare var Type: any;
declare var ko: any;
declare var AspNetAjax$Function: any;
declare var React: any;

//inflectra services
declare var globalFunctions: any;
declare var Inflectra: any;
declare var SpiraContext: any;

//inflectra objects
declare var ajxBackgroundProcessManager_id: string;
declare var ajxFormManager_id: string;
declare var lstComments_id: string;

//Page objects
declare var radRepository_id: string;
declare var radAttached_id: string;
declare var radLinked_id: string;
declare var lblRepository_id: string;
declare var txtAutomationScript_id: string;
declare var lnkAutomationDocument_id: string;
declare var automationScriptSection: HTMLElement;
declare var lblParameterMessage_id: string;
declare var lblTestStepMessages_id: string;
declare var msgInsertTestLinkMessages_id: string;
declare var msgImportTestCaseMessages_id: string;
declare var lblAttached_id: string;
declare var lblLinked_id: string;

declare var txtNewParameter_id: string;
declare var txtNewParameterDefaultValue_id: string;
declare var pnlEditParameters_id: string;
declare var txtNewLinkedTestCase_id: string;
declare var grdTestSteps_id: string;
declare var pnlInsertTestLink_id: string;
declare var pnlImportTestCase_id: string;
declare var pnlEditTestLink_id: string;
declare var hdnTestScriptChanged_id: string;
declare var ddlNewLinkedTestCaseFolders_id: string;
declare var ddlLinkedTestCaseFolders_id: string;
declare var ddlImportTestCaseFolders_id: string;
declare var ajxLinkedTestCaseSelector_id: string;
declare var ajxImportTestCaseSelector_id: string;
declare var lnkAddTestLink: any;

//URL templates
declare var urlTemplate_launchUrl: string;
declare var urlTemplate_testRunsPending: string;
declare var urlTemplate_testRunsPendingExploratory: string;

//External functions
declare function ddlAutomationEngine_selected(args?:any):void;

/* The Page Class */
Type.registerNamespace('Inflectra.SpiraTest.Web');
Inflectra.SpiraTest.Web.TestCaseDetails = function ()
{
    this._currentTestStepId = null;

    this._radAttached = $get(radAttached_id);
    this._radLinked = $get(radLinked_id);
    this._radRepository = $get(radRepository_id);
    this._lblAttached = $get(lblAttached_id);
    this._lblLinked = $get(lblLinked_id);
    this._lblRepository = $get(lblRepository_id);

	this._txtAutomationScript = $get(txtAutomationScript_id);
	this._lnkAutomationDocument = $get(lnkAutomationDocument_id);
    this._lblParameterMessage = $get(lblParameterMessage_id);
    this._lblTestStepMessages = $get(lblTestStepMessages_id);
    this._msgInsertTestLinkMessages = $get(msgInsertTestLinkMessages_id);
    this._msgImportTestCaseMessages = $get(msgImportTestCaseMessages_id);

    this._ajxFormManager = null;
    this._lstComments = null;
    this._self = this;
    this._testCaseParametersViewModel = null;
    this._testStepParametersViewModel = null;
    this._insertLinkParametersViewModel = null;
    this._insertNewLinkParametersViewModel = null;
    this._testFolders = null;
    this._testFolders2 = null;
    this._testFolders3 = null;

    Inflectra.SpiraTest.Web.TestCaseDetails.initializeBase(this);
};
Inflectra.SpiraTest.Web.TestCaseDetails.prototype =
    {
        /* Constructors */
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.TestCaseDetails.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
			delete this._txtAutomationScript;
			delete this._lnkAutomationDocument;
            delete this._ajxFormManager;
            delete this._lstComments;

            delete this._lblAttached;
            delete this._lblLinked;
            delete this._lblRepository;
            delete this._radAttached;
            delete this._radLinked;
            delete this._radRepository;
            delete this._lblParameterMessage;
            delete this._lblTestStepMessages;
            delete this._msgInsertTestLinkMessages;
            Inflectra.SpiraTest.Web.TestCaseDetails.callBaseMethod(this, 'dispose');
        },

        /* Properties */
        get_ajxFormManager: function ()
        {
            if (!this._ajxFormManager)
            {
                this._ajxFormManager = $find(ajxFormManager_id);
            }
            return this._ajxFormManager;
        },
        get_lstComments: function ()
        {
            if (!this._lstComments)
            {
                this._lstComments = $find(lstComments_id);
            }
            return this._lstComments;
        },

        /* Functions */
        updateAutomationScriptRadios: function ()
        {
            if (this._radAttached.checked)
            {
                this._lblAttached.setAttribute('data-checked', 'checked');
                this._lblLinked.setAttribute('data-checked', '');
                this._lblRepository.setAttribute('data-checked', '');
            }
            else if (this._radLinked.checked)
            {
                this._lblAttached.setAttribute('data-checked', '');
                this._lblLinked.setAttribute('data-checked', 'checked');
                this._lblRepository.setAttribute('data-checked', '');
            }
            else if (this._radRepository.checked)
            {
                this._lblAttached.setAttribute('data-checked', '');
                this._lblLinked.setAttribute('data-checked', '');
                this._lblRepository.setAttribute('data-checked', 'checked');
            }
        },

        automationScript_changed: function (evt)
        {
            this.updateAutomationScriptRadios();
            var dataItem = this.get_ajxFormManager().get_dataItem();
            var automationTypeField = dataItem.Fields.AutomationType;
            if (this._radAttached.checked)
            {
				automationScriptSection.style.display = '';
				this._txtAutomationScript.style.display = 'inline';
				this._txtAutomationScript.readOnly = false;
				this._lnkAutomationDocument.style.display = '';
                automationTypeField.textValue = 'attached';
            }
            else if (this._radLinked.checked)
            {
				automationScriptSection.style.display = 'none';
				this._txtAutomationScript.style.display = 'none';
				this._txtAutomationScript.readOnly = true;
				this._lnkAutomationDocument.style.display = '';
                automationTypeField.textValue = 'linked';
            }
            else if (this._radRepository.checked)
            {
				automationScriptSection.style.display = '';
				this._txtAutomationScript.style.display = 'inline';
				this._txtAutomationScript.readOnly = true;
				this._lnkAutomationDocument.style.display = 'none';
                automationTypeField.textValue = 'repository';
            }
        },

        loadAutomationInfo: function (dontClearMessages)
        {
            //We need to set the automation radio button based on the provided info
            var dataItem = this.get_ajxFormManager().get_dataItem();
            if (this._radAttached && this._radLinked && this._radRepository && dataItem)
            {
                var automationTypeField = dataItem.Fields.AutomationType;
                if (automationTypeField)
                {
                    var automationType = automationTypeField.textValue;
                    this._radAttached.checked = false;
                    this._radLinked.checked = false;
                    this._radRepository.checked = false;
                    if (automationType == 'attached')
                    {
						this._radAttached.checked = true;
						this._lnkAutomationDocument.style.display = '';
                    }
                    else if (automationType == 'linked')
                    {
						this._radLinked.checked = true;
						this._lnkAutomationDocument.style.display = '';
                    }
                    else if (automationType == 'repository')
                    {
						this._radRepository.checked = true;
						this._lnkAutomationDocument.style.display = 'none';
                    }

                    //Update the radio buttons display
                    this.updateAutomationScriptRadios();

                    //Update the state of the fields according to what's checked
                    ddlAutomationEngine_selected();

                    //Also we need to hide repository unless it was initially selected
                    //and hide attached/linked if repository was initially selected
                    if (automationType == 'attached')
                    {
                        this._lblRepository.setAttribute('disabled', 'disabled');
                        this._radRepository.disabled = true;
                    }
                    else if (automationType == 'linked')
                    {
                        this._lblRepository.setAttribute('disabled', 'disabled');
                        this._radRepository.disabled = true;
                    }
                    else if (automationType == 'repository')
                    {
                        this._lblLinked.setAttribute('disabled', 'disabled');
                        this._lblAttached.setAttribute('disabled', 'disabled');
                        this._radLinked.disabled = true;
                        this._radAttached.disabled = true;
                    }
                }
            }
        },
        ajxWorkflowOperations_operationExecuted: function (transitionId, isStatusOpen)
        {
            //Put any post-workflow operations here
        },
        ajxFormManager_operationReverted: function (statusId, isStatusOpen)
        {
            //Put any post-revert operations here
        },

        displayEditParameters: function (evt)
        {
            //Reset the message box and new parameter text boxes
            globalFunctions.clear_errors(this._lblParameterMessage);
            $get(txtNewParameter_id).value = '';
            $get(txtNewParameterDefaultValue_id).value = '';

            //Load the parameters list
            this.load_testCaseParameters();

            //Display dialog box
            var pnlEditParameters = $find(pnlEditParameters_id);
            pnlEditParameters.display(evt);
        },
        load_testCaseParameters: function ()
        {
            //Specify the test case and load
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveParameters(SpiraContext.ArtifactId, false, false, AspNetAjax$Function.createDelegate(this, this.load_testCaseParameters_success), AspNetAjax$Function.createDelegate(this, this.load_testCaseParameters_failure));
        },
        load_testCaseParameters_success: function (data)
        {
            globalFunctions.hide_spinner();

            //Add the editable flag to the elements and set a value for default value if null
            if (data)
            {
                for (var item in data)
                {
                    data[item].editable = false;
                    if (!data[item].Fields.DefaultValue.textValue)
                    {
                        data[item].Fields.DefaultValue.textValue = '';
                    }
                }
            }

            //databind the grid using knockout
            if (this._testCaseParametersViewModel)
            {
                ko.mapping.fromJS(data, this._testCaseParametersViewModel);
            }
            else
            {
                this._testCaseParametersViewModel = ko.mapping.fromJS(data);
                ko.applyBindings(this._testCaseParametersViewModel, $get('tblTestCaseParameters'));
            }
        },
        load_testCaseParameters_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._lblParameterMessage, exception);
        },
        testCaseParameters_storeFocusElement: function ()
        {
            this._focusedElement = document.activeElement;
        },
        testCaseParameters_insertAtCursor: function (token, evt)
        {
            //get the focused element and insert - handle RTEs separately
            if (this._focusedElement)
            {
                if (this._focusedElement.title && this._focusedElement.tagName == 'IFRAME')
                {
                    var controlId = this._focusedElement.title.replace('Rich Text Editor, ', '');
                    //IE/Edge append a message that we need to strip off as well
                    if (controlId.indexOf(',') > 0)
                    {
                        controlId = controlId.split(',')[0];
                    }
                    var editor = CKEDITOR.instances[controlId];
                    if (editor && editor.insertHtml)
                    {
                        editor.insertHtml(token);
                    }
                }
                else
                {
                    this._focusedElement.focus();
                    $(this._focusedElement).insertAtCaret(token);
                }
            }
        },
        testCaseParameters_delete: function (testCaseParameter)
        {
            //Delete the specified test case parameter from the view model
            if (this._testCaseParametersViewModel)
            {
                for (var i = 0; i < this._testCaseParametersViewModel().length; i++)
                {
                    if (this._testCaseParametersViewModel()[i] == testCaseParameter)
                    {
                        this._testCaseParametersViewModel.remove(this._testCaseParametersViewModel()[i]);
                    }
                }
            }
        },
        testCaseParameters_add: function ()
        {
            //Add  the new parameter and value to the model
            //Reset the message box
            globalFunctions.clear_errors(this._lblParameterMessage);

            //Make sure that a parameter name was provided
            var parameterName = $get(txtNewParameter_id).value.trim();
            var defaultValue = $get(txtNewParameterDefaultValue_id).value.trim();

            if (parameterName == '')
            {
                globalFunctions.display_error(this._lblParameterMessage, resx.TestCaseDetails_ParameterNameRequired);
            }
            else if (parameterName.indexOf('\'') >= 0 || parameterName.indexOf('\\') >= 0 || parameterName.indexOf(' ') >= 0)
            {
                globalFunctions.display_error(this._lblParameterMessage, resx.TestCaseDetails_ParameterNameInvalid);
            }
            else if (parameterName.indexOf('$') >= 0 || parameterName.indexOf('{') >= 0 || parameterName.indexOf('}') >= 0)
            {
                globalFunctions.display_error(this._lblParameterMessage, resx.TestCaseDetails_ParameterNameInvalid2);
            }
            else
            {
                //Add the new parameter to the model
                var testCaseParameter = {
                    __type: globalFunctions.dataType_DataItem,
                    Fields: {
                        Name: {
                            __type: globalFunctions.dataType_DataItemField,
                            fieldName: "Name",
                            textValue: parameterName
                        },
                        DefaultValue: {
                            __type: globalFunctions.dataType_DataItemField,
                            fieldName: "DefaultValue",
                            textValue: defaultValue
                        },
                        Token: {
                            __type: globalFunctions.dataType_DataItemField,
                            fieldName: "Token",
                            textValue: '${' + parameterName + '}'
                        }
                    }
                };

                //Make sure we don't already have the parameter listed
                var match = false;
                for (var i = 0; i < this._testCaseParametersViewModel().length; i++)
                {
                    if (this._testCaseParametersViewModel()[i].Fields.Name.textValue() == parameterName)
                    {
                        match = true;
                        break;
                    }
                }
                if (!match)
                {
                    var observable = ko.mapping.fromJS(testCaseParameter);
                    observable.editable = ko.observable(false);
                    this._testCaseParametersViewModel.push(observable);
                }
            }
        },
        testCaseParameters_edit: function (testCaseParameter)
        {
            //Toggle the editable flag
            var editable = testCaseParameter.editable();
            testCaseParameter.editable(!editable);

            //Update the token from the name
            testCaseParameter.Fields.Token.textValue('${' + testCaseParameter.Fields.Name.textValue() + '}');
        },
        testCaseParameters_save: function ()
        {
            //See if we have a new parameter that has not been added, if so add it first
            var newParam = $get(txtNewParameter_id).value.trim();
            if (newParam.length > 0)
            {
                this.testCaseParameters_add();
            }

            //Unmap the json
            var mapping = {
                'ignore': ['editable']
            }
            var parameters = ko.mapping.toJS(this._testCaseParametersViewModel, mapping);

            //Save the parameters
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.SaveParameters(SpiraContext.ProjectId, SpiraContext.ArtifactId, parameters, AspNetAjax$Function.createDelegate(this, this.testCaseParameters_save_success), AspNetAjax$Function.createDelegate(this, this.testCaseParameters_save_failure));
        },
        testCaseParameters_save_success: function ()
        {
            //Hide the spinner and close the dialog
            globalFunctions.hide_spinner();

            var pnlEditParameters = $find(pnlEditParameters_id);
            pnlEditParameters.close();
        },
        testCaseParameters_save_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._lblParameterMessage, exception);
        },

        txtAutomationScript_changed: function (evt)
        {
            var hdnTestScriptChanged = $get(hdnTestScriptChanged_id);
            hdnTestScriptChanged.value = 'true';
        },

        grdTestSteps_rowEditAlternate: function (dataItem, evt)
        {
            //Make sure we have an event sent (prevents it being displayed on new item insert scenario)
            if (evt)
            {
                globalFunctions.clear_errors(this._lblTestStepMessages);

                //Clear the labels
                $get('divHasParameters').style.display = 'none';
                $get('divNoParameters').style.display = 'none';

                //Fetch the data to populate the test step parameters dataview
                var testStepId = dataItem.primaryKey;
                var linkedTestCaseId = dataItem.alternateKey;
                this._currentTestStepId = testStepId;
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.TestStepService.RetrieveParameters(testStepId, linkedTestCaseId, AspNetAjax$Function.createDelegate(this, this.grdTestSteps_rowEditAlternate_success), AspNetAjax$Function.createDelegate(this, this.grdTestSteps_rowEditAlternate_failure));

                //Display the link panel
                var pnlEditTestLink = $find(pnlEditTestLink_id);
                pnlEditTestLink.display(evt);
            }
        },
        grdTestSteps_rowEditAlternate_success: function (data)
        {
            globalFunctions.hide_spinner();
            //Display the appropriate legend
            if (data && data.length > 0)
            {
                $get('divHasParameters').style.display = 'block';
            }
            else
            {
                $get('divNoParameters').style.display = 'block';
            }

            //databind the grid using knockout
            if (this._testStepParametersViewModel)
            {
                ko.mapping.fromJS(data, this._testStepParametersViewModel);
            }
            else
            {
                this._testStepParametersViewModel = ko.mapping.fromJS(data);
                ko.applyBindings(this._testStepParametersViewModel, $get('tblTestStepParametersEdit'));
            }
        },
        grdTestSteps_rowEditAlternate_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._lblTestStepMessages, exception);
        },
        pnlEditTestLink_tblTestStepParametersEdit_keydown: function (data, evt)
        {
            //If enter pressed, stop event propagation and updated test link parameters
            var keynum = evt.keyCode | evt.which;
            if (keynum == 13)
            {
                // stop the event bubble
                evt.preventDefault();
                evt.stopPropagation();
                //Fire the update event
                this.pnlEditTestLink_lnkTestLinkUpdate_click(evt);
                return false;
            }
            return true;
        },
        pnlEditTestLink_lnkTestLinkUpdate_click: function (evt)
        {
            var testStepId = this._currentTestStepId;
            //Get the updated data object from the viewmodel and pass to the update web service
            var parameterValues = ko.mapping.toJS(this._testStepParametersViewModel);

            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestStepService.UpdateParameters(SpiraContext.ProjectId, testStepId, parameterValues, AspNetAjax$Function.createDelegate(this, this.tblTestStepParametersUpdate_success), AspNetAjax$Function.createDelegate(this, this.tblTestStepParametersUpdate_failure));
        },
        tblTestStepParametersUpdate_success: function ()
        {
            //Close the dialog box
            globalFunctions.hide_spinner();
            var pnlEditTestLink = $find(pnlEditTestLink_id);
            pnlEditTestLink.close();

            //Reload the grid
            $find(grdTestSteps_id).load_data();
        },
        tblTestStepParametersUpdate_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._lblTestStepMessages, exception);
        },

        lnkInsertLink_click: function (evt)
        {
            //Make sure that no more than one is selected
            var grdTestSteps = $find(grdTestSteps_id);
            var items = grdTestSteps._get_selected_items();
            if (items.length <= 1)
            {
                var pnlInsertTestLink = $find(pnlInsertTestLink_id);
                pnlInsertTestLink.display(evt);

                //Load the folders once
                if (!this._testFolders)
                {
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveTestFolders(SpiraContext.ProjectId, AspNetAjax$Function.createDelegate(this, this.load_linkedTestCaseFolders_success), AspNetAjax$Function.createDelegate(this, this.load_linkedTestCaseFolders_failure))
                }
            }
            else
            {
                globalFunctions.globalAlert(resx.Global_SelectOneCheckBoxForInsert);
            }
        },
        radLinkType_clicked: function ()
        {
            //Get the current selection
            var val = $('input:radio[name=radLinkType]:checked').val();
            if (val == 'link-existing')
            {
                $('label.btn[for=radLinkNew]').removeClass('active');
                $('label.btn[for=radLinkExisting]').addClass('active');
                $('#section-link-existing').css('display', 'block');
                $('#section-link-new').css('display', 'none');
            }
            if (val == 'link-create-new')
            {
                $('label.btn[for=radLinkExisting]').removeClass('active');
                $('label.btn[for=radLinkNew]').addClass('active');
                $('#section-link-existing').css('display', 'none');
                $('#section-link-new').css('display', 'block');

                //Load the folders if first time
                if (!this._testFolders3)
                {
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveTestFolders(SpiraContext.ProjectId, AspNetAjax$Function.createDelegate(this, this.load_newLinkedTestCaseFolders_success), AspNetAjax$Function.createDelegate(this, this.load_newLinkedTestCaseFolders_failure))
                }

                //Databind parameters grid
                this.tblNewLinkedTestCaseParameters_dataBind();
            }
        },
        load_linkedTestCaseFolders_success: function (data)
        {
            //Databind the dropdown list
            globalFunctions.hide_spinner();
            this._testFolders = data;
            var ddlLinkedTestCaseFolders = $find(ddlLinkedTestCaseFolders_id);
            ddlLinkedTestCaseFolders.set_dataSource(data);
            ddlLinkedTestCaseFolders.dataBind();
        },
        load_linkedTestCaseFolders_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgInsertTestLinkMessages, exception);
        },
        ddlLinkedTestCaseFolders_changed: function (item)
        {
            var ajxLinkedTestCaseSelector = $find(ajxLinkedTestCaseSelector_id);
            var folderId = item.get_value();
            if (folderId && folderId != '')
            {
                var standardFilters = { TestCaseFolderId: globalFunctions.serializeValueInt(folderId) };
                ajxLinkedTestCaseSelector.set_standardFilters(standardFilters);
            }
            else
            {
                ajxLinkedTestCaseSelector.set_standardFilters(null);
            }
            ajxLinkedTestCaseSelector.load_data();
        },

        execute_test_case: function ()
        {
            //First check if the user has an existing testrunpending for this test case
            Inflectra.SpiraTest.Web.Services.Ajax.TestRunService.RetrievePendingByUserIdAndTestCase(
                SpiraContext.ProjectId,
                SpiraContext.ArtifactId,
                AspNetAjax$Function.createDelegate(this, this.retrieveExistingPending_success),
                AspNetAjax$Function.createDelegate(this, this.execute_test_case_process)
            );
        },
        retrieveExistingPending_success: function(data)
        {
            if (data && data.length)
            {
                // make sure the message dialog is clear then render
                globalFunctions.dlgGlobalDynamicClear();
                ReactDOM.render(
                    React.createElement(RctTestRunsPendingExecuteNewOrExisting, {
                        data: data,
                        newTestName: resx.TestCaseList_ExecuteTestCase,
                        executeFunction: this.execute_test_case_process
                    }, null),
                    document.getElementById('dlgGlobalDynamic')
                );
            } 
            else
            {
                this.execute_test_case_process();
            }
        },
        execute_test_case_process: function(testRunsPendingId)
        {
            if (!testRunsPendingId) {
                //there are two types of test execution - normal and exploratory. The server function determines the correct one
                //start the background process of creating the test run
                var ajxBackgroundProcessManager = $find(ajxBackgroundProcessManager_id);
                ajxBackgroundProcessManager.display(
                    SpiraContext.ProjectId,
                    'TestCase_Execute',
                    resx.TestCaseList_ExecuteTestCase,
                    resx.TestCaseList_ExecuteTestCaseDesc,
                    SpiraContext.ArtifactId
                );
            } else {
                window.open(globalFunctions.getArtifactDefaultUrl(SpiraContext.BaseUrl, SpiraContext.ProjectId, "TestExecute", testRunsPendingId), "_self");
            }
        },

        ajxBackgroundProcessManager_success: function (msg, returnCode)
        {
            //Need to redirect to the test runs pending
            if (returnCode && returnCode > 0)
            {
                var ajxBackgroundProcessManager = $find(ajxBackgroundProcessManager_id);

                //set the base url to either exploratory or normal - based on what is returned from the server in the success params
                var baseUrl = msg === "testcase_executeexploratory" ? urlTemplate_testRunsPendingExploratory : urlTemplate_testRunsPending;
                var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, SpiraContext.ProjectId);
                window.location.href = url;
            }
        },

        pnlInsertTestLink_ajxLinkedTestCaseSelector_loaded: function ()
        {
            //Hide the parameters panel
            var divTestStepParametersAdd = $get('divTestStepParametersAdd');
            divTestStepParametersAdd.style.display = 'none';
        },
        pnlInsertTestLink_ajxLinkedTestCaseSelector_selected: function (testCaseId)
        {
            //Make sure we're not linking to ourselves
            if (testCaseId == SpiraContext.ArtifactId)
            {
                globalFunctions.display_error_message(this._msgInsertTestLinkMessages, resx.TestCaseDetails_CannotLinkToSelf);
                lnkAddTestLink.disabled = true;
            }
            else
            {
                //Clear the labels
                $get('divHasParametersAdd').style.display = 'none';
                $get('divNoParametersAdd').style.display = 'none';

                //Enable the save button
                lnkAddTestLink.disabled = false;

                //Show the parameters panel and load the data
                var divTestStepParametersAdd = $get('divTestStepParametersAdd');
                divTestStepParametersAdd.style.display = 'block';
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveParameters(testCaseId, true, false, AspNetAjax$Function.createDelegate(this, this.tblTestStepParametersAdd_success), AspNetAjax$Function.createDelegate(this, this.tblTestStepParametersAdd_failure));
            }
        },
        tblTestStepParametersAdd_success: function (data)
        {
            //Set a value for default value if null to ensure it's observable
            if (data)
            {
                for (var item in data)
                {
                    if (!data[item].Fields.DefaultValue.textValue)
                    {
                        data[item].Fields.DefaultValue.textValue = '';
                    }
                }
            }

            //Databind using knockout
            globalFunctions.hide_spinner();
            if (this._insertLinkParametersViewModel)
            {
                ko.mapping.fromJS(data, this._insertLinkParametersViewModel);
            }
            else
            {
                this._insertLinkParametersViewModel = ko.mapping.fromJS(data);
                ko.applyBindings(this._insertLinkParametersViewModel, $get('tblTestStepParametersAdd'));
            }

            //Display the appropriate legend
            if (data && data.length > 0)
            {
                $get('divHasParametersAdd').style.display = 'block';
            }
            else
            {
                $get('divNoParametersAdd').style.display = 'block';
            }
        },
        tblTestStepParametersAdd_failure: function (exception)
        {
            //See if we have any known exception cases
            globalFunctions.hide_spinner();
            if (exception.get_exceptionType() == 'Inflectra.SpiraTest.Business.EntityInfiniteRecursionException')
            {
                globalFunctions.display_error_message(this._msgInsertTestLinkMessages, resx.TestCaseDetails_CannotLinkToParent);
            }
            else
            {
                //Display the error
                globalFunctions.display_error(this._msgInsertTestLinkMessages, exception);
            }
        },

        pnlInsertTestLink_lnkAddTestLink_click: function ()
        {
            //See if we're linking to an existing or creating new
            var val = $('input:radio[name=radLinkType]:checked').val();
            if (val == 'link-existing')
            {
                //Get the test case we're linking to
                var ajxLinkedTestCaseSelector = $find(ajxLinkedTestCaseSelector_id);
                var testCaseIds = ajxLinkedTestCaseSelector.get_selectedItems();
                if (testCaseIds && testCaseIds.length > 0)
                {
                    //Get the test case id
                    var grdTestSteps = $find('cplMainContent_grdTestSteps');
                    var existingFilters = grdTestSteps.get_standardFilters();
                    var testCaseId = testCaseIds[0];

                    // Only proceed if we are adding a different test case than the one we are on
                    if (testCaseId != SpiraContext.ArtifactId)
                    {
                        //Get the updated parameter data objects from the view model
                        var filterPrefix = 'TestStepParameter_';
                        for (var i = 0; i < this._insertLinkParametersViewModel().length; i++)
                        {
                            var dataItem = this._insertLinkParametersViewModel()[i];
                            if (dataItem.Fields.DefaultValue.textValue() && dataItem.Fields.DefaultValue.textValue() != '')
                            {
                                var parameterAttribute = dataItem.Fields.Name.textValue();
                                existingFilters[filterPrefix + parameterAttribute] = globalFunctions.serializeValueString(dataItem.Fields.DefaultValue.textValue());
                            }
                        }

                        //Now we need to actually insert the link, passing the data as 'standard filters'
                        existingFilters['LinkedTestCaseId'] = globalFunctions.serializeValueInt(testCaseId);
                        grdTestSteps.insert_item('Link');
                        //Clear filter afterwards
                        existingFilters['LinkedTestCaseId'] = '';
                        for (var i = 0; i < this._insertLinkParametersViewModel().length; i++)
                        {
                            var dataItem = this._insertLinkParametersViewModel()[i];
                            if (dataItem.Fields.DefaultValue.textValue() && dataItem.Fields.DefaultValue.textValue() != '')
                            {
                                var parameterAttribute = dataItem.Fields.Name.textValue();
                                existingFilters[filterPrefix + parameterAttribute] = '';
                            }
                        }
                        //Close the dialog box
                        var pnlInsertTestLink = $find(pnlInsertTestLink_id);
                        pnlInsertTestLink.close();
                    }

                }
                else
                {
                    globalFunctions.globalAlert(resx.TestCaseDetails_SelectTestCaseFirst);
                }
            }
            if (val == 'link-create-new')
            {
                if (!SpiraContext.uiState) {
                    SpiraContext.uiState = {};
                }
                SpiraContext.uiState.testCaseDetails = {};
                var pageState = SpiraContext.uiState.testCaseDetails;

                //Make sure a test case name was entered
                pageState.testCaseName = globalFunctions.trim($get(txtNewLinkedTestCase_id).value);
                if (pageState.testCaseName && pageState.testCaseName.length > 0)
                {
                    //Get the test folder and parameters
                    pageState.folderId = null;
                    var item = $find(ddlNewLinkedTestCaseFolders_id).get_selectedItem();
                    if (item && item.get_value() && item.get_value() != '')
                    {
                        pageState.folderId = item.get_value();
                    }

                    //Get the selected test step id (if any)
                    pageState.testStepId = null;
                    var grdTestSteps = $find(grdTestSteps_id);
                    var items = grdTestSteps._get_selected_items();
                    if (items.length > 0)
                    {
                        pageState.testStepId = items[0];
                    }

                    //Get the parameters in JSON
                    pageState.parameters = ko.mapping.toJS(this._insertNewLinkParametersViewModel.parameters);

                    //Set local this context so that it can be referenced in any callback
                    pageState.thisRef = this;

                    //Check if the main grid of test steps is currently being edited
                    //Get the grid js object and check if it is in edit mode
                    var grdTestStepsMain = $find('cplMainContent_grdTestSteps');
                    if (grdTestStepsMain._isInEdit) {
                        //update the grid edits
                        //Pass in a callback to redirect to the new RK page - only if the updates have succeeded
                        grdTestStepsMain._onUpdateClick(this.pnlInsertTestLink_lnkAddNewTestAsLink, null);
                    } else {
                        //Else immediately open the new incident page
                        this.pnlInsertTestLink_lnkAddNewTestAsLink();
                    }

                }
                else
                {
                    globalFunctions.globalAlert(resx.TestCaseDetails_EnterTestCaseNameForNewLink);
                }
            }
        },

        pnlInsertTestLink_lnkAddNewTestAsLink: function ()
        {
            var localState = SpiraContext.uiState.testCaseDetails;
            //Create the test case with parameters
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestStepService.TestStep_CreateNewLinkedTestCase(
                SpiraContext.ProjectId,
                SpiraContext.ArtifactId,
                localState.testStepId,
                localState.folderId,
                localState.testCaseName,
                localState.parameters,
                AspNetAjax$Function.createDelegate(localState.thisRef, localState.thisRef.pnlInsertTestLink_createNewLinkedTestCase_success),
                AspNetAjax$Function.createDelegate(localState.thisRef, localState.thisRef.pnlInsertTestLink_createNewLinkedTestCase_failure)
            );

            //Clear out temporary state
            SpiraContext.uiState.testCaseDetails = {};
        },

        pnlInsertTestLink_rptTestStepParameters_keydown: function (data, evt)
        {
            //If enter pressed, stop event propagation and insert new test link
            var keynum = evt.keyCode | evt.which;
            if (keynum == 13)
            {
                // stop the event bubble
                evt.preventDefault();
                evt.stopPropagation();
                // Fire the Insert event
                this.pnlInsertTestLink_lnkAddTestLink_click();
                return false;
            }
            return true;
        },

        displayImportStepsDialog: function (evt)
        {
            //Make sure that no more than one is selected
            var grdTestSteps = $find(grdTestSteps_id);
            var items = grdTestSteps._get_selected_items();
            if (items.length <= 1)
            {
                var pnlImportTestCase = $find(pnlImportTestCase_id);
                pnlImportTestCase.display(evt);

                //Load the folders once
                if (!this._testFolders2)
                {
                    globalFunctions.display_spinner();
                    Inflectra.SpiraTest.Web.Services.Ajax.TestCaseService.RetrieveTestFolders(SpiraContext.ProjectId, AspNetAjax$Function.createDelegate(this, this.load_importTestCaseFolders_success), AspNetAjax$Function.createDelegate(this, this.load_importTestCaseFolders_failure))
                }
            }
            else
            {
                globalFunctions.globalAlert(resx.Global_SelectOneCheckBoxForImport);
            }
        },
        load_importTestCaseFolders_success: function (data)
        {
            //Databind the dropdown list
            globalFunctions.hide_spinner();
            this._testFolders2 = data;
            var ddlImportTestCaseFolders = $find(ddlImportTestCaseFolders_id);
            ddlImportTestCaseFolders.set_dataSource(data);
            ddlImportTestCaseFolders.dataBind();
        },
        load_importTestCaseFolders_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgImportTestCaseMessages, exception);
        },
        ddlImportTestCaseFolders_changed: function (item)
        {
            var folderId = item.get_value();
            var ajxImportTestCaseSelector = $find(ajxImportTestCaseSelector_id);
            if (folderId && folderId != '')
            {
                var standardFilters = { TestCaseFolderId: globalFunctions.serializeValueInt(folderId) };
                ajxImportTestCaseSelector.set_standardFilters(standardFilters);
            }
            else
            {
                ajxImportTestCaseSelector.set_standardFilters(null);
            }
            ajxImportTestCaseSelector.load_data();
        },

        pnlImportTestCase_ajxImportTestCaseSelector_selected: function (testCaseId)
        {
            //Make sure we're not importing ourself
            if (testCaseId == SpiraContext.ArtifactId)
            {
                globalFunctions.display_error_message(this._msgImportTestCaseMessages, resx.TestCaseDetails_CannotImportSelf);
                $('#btnImportTestCase').prop('disabled', true);
            }
            else
            {
                $('#btnImportTestCase').prop('disabled', false);
            }
        },
        pnlImportTestCase_btnImportTestCase_click: function ()
        {
            //Get the test case we're importing
            var ajxImportTestCaseSelector = $find(ajxImportTestCaseSelector_id);
            var testCaseIds = ajxImportTestCaseSelector.get_selectedItems();
            if (testCaseIds && testCaseIds.length > 0)
            {
                //Get the test case id
                var grdTestSteps = $find('cplMainContent_grdTestSteps');
                var testCaseToImportId = testCaseIds[0];

                //Get the selected test step id (if any)
                var testStepId = null;
                var grdTestSteps = $find(grdTestSteps_id);
                var items = grdTestSteps._get_selected_items();
                if (items.length > 0)
                {
                    testStepId = items[0];
                }

                //Call the service to do the import
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.TestStepService.TestStep_ImportTestCase(SpiraContext.ProjectId, SpiraContext.ArtifactId, testCaseToImportId, testStepId, AspNetAjax$Function.createDelegate(this, this.pnlImportTestCase_btnImportTestCase_success), AspNetAjax$Function.createDelegate(this, this.pnlImportTestCase_btnImportTestCase_failure));
            }
        },

        pnlImportTestCase_btnImportTestCase_success: function ()
        {
            //Close the dialog box
            globalFunctions.hide_spinner();
            var pnlImportTestCase = $find(pnlImportTestCase_id);
            pnlImportTestCase.close();

            //Reload the test steps
            $find(grdTestSteps_id).load_data();
        },
        pnlImportTestCase_btnImportTestCase_failure: function (exception)
        {
            //Display the error message
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgImportTestCaseMessages, exception);
        },

        load_newLinkedTestCaseFolders_success: function (data)
        {
            //Databind the dropdown list
            globalFunctions.hide_spinner();
            this._testFolders3 = data;
            var ddlNewLinkedTestCaseFolders = $find(ddlNewLinkedTestCaseFolders_id);
            ddlNewLinkedTestCaseFolders.set_dataSource(data);
            ddlNewLinkedTestCaseFolders.dataBind();
        },
        load_newLinkedTestCaseFolders_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgInsertTestLinkMessages, exception);
        },

        tblNewLinkedTestCaseParameters_dataBind: function ()
        {
            //Create the view model if not already created
            if (!this._insertNewLinkParametersViewModel)
            {
                this._insertNewLinkParametersViewModel = {
                    parameters: new ko.observableArray()
                };
                ko.applyBindings(this._insertNewLinkParametersViewModel, $get('tblNewLinkedTestCaseParameters'));
            }
            else
            {
                //Just empty the data from the model
                this._insertNewLinkParametersViewModel.parameters.removeAll();
            }

            //Now add one initial item
            var testCaseParameter = {
                __type: globalFunctions.dataType_DataItem,
                Fields: {
                    Name: {
                        __type: globalFunctions.dataType_DataItemField,
                        fieldName: "Name",
                        textValue: ''
                    },
                    Value: {
                        __type: globalFunctions.dataType_DataItemField,
                        fieldName: "Value",
                        textValue: ''
                    }
                }
            };

            this._insertNewLinkParametersViewModel.parameters.push(testCaseParameter);
        },
        tblNewLinkedTestCaseParameters_keyup: function (data, evt)
        {
            //Add a new parameter if we don't have at least one blank entry
            var newParamNeeded = true;
            for (var i = 0; i < this._insertNewLinkParametersViewModel.parameters().length; i++)
            {
                if (this._insertNewLinkParametersViewModel.parameters()[i].Fields.Name.textValue == '')
                {
                    newParamNeeded = false;
                    break;
                }
            }

            if (newParamNeeded)
            {
                var testCaseParameter = {
                    __type: globalFunctions.dataType_DataItem,
                    Fields: {
                        Name: {
                            __type: globalFunctions.dataType_DataItemField,
                            fieldName: "Name",
                            textValue: ''
                        },
                        Value: {
                            __type: globalFunctions.dataType_DataItemField,
                            fieldName: "Value",
                            textValue: ''
                        }
                    }
                };
                this._insertNewLinkParametersViewModel.parameters.push(testCaseParameter);
            }
        },

        pnlInsertTestLink_createNewLinkedTestCase_success: function ()
        {
            //Clear the parameters and reset the test case name
            $get(txtNewLinkedTestCase_id).value = '';
            this.tblNewLinkedTestCaseParameters_dataBind();

            //Close the dialog box
            globalFunctions.hide_spinner();
            var pnlInsertTestLink = $find(pnlInsertTestLink_id);
            pnlInsertTestLink.close();

            //Reload the test steps
            $find(grdTestSteps_id).load_data();
        },
        pnlInsertTestLink_createNewLinkedTestCase_failure: function (exception)
        {
            //Display the error message
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgInsertTestLinkMessages, exception);
        },
    };
