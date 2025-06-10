//external dependendencies (js libraries)
declare var $: any;
declare var $find: any;
declare var $get: any;
declare var Type: any;
declare var ko: any;
declare var AspNetAjax$Function: any;

//inflectra services
declare var globalFunctions: any;
declare var Inflectra: any;
declare var SpiraContext: any;

//inflectra objects
declare var grdTestCases_id: string;
declare var lblTestCaseMessages_id: string;
declare var msgTestCaseParameters_id: string;
declare var msgTestSetParameters_id: string;
declare var ajxBackgroundProcessManager_id: string;
declare var pnlEditTestCaseParameters_id: string;
declare var ddlAutomationHost_id: string;
declare var ddlTestRunType_id: string;
declare var pnlAddTestSetParameter_id: string;
declare var ddlParameterName_id: string;
declare var txtNewParameterValue_id: string;

//URL templates
declare var urlTemplate_launchUrl: string;
declare var urlTemplate_testRunsPending: string;
declare var urlTemplate_testRunsPendingExploratory: string;

/* The Page Class */
Type.registerNamespace('Inflectra.SpiraTest.Web');
Inflectra.SpiraTest.Web.TestSetDetails = function ()
{
    this._currentTestSetTestCaseId = null;
    this._addPanelIsOpen = false;

    this._testFolders = null;
    this._testSetTestCaseParametersViewModel = null;
    this._testSetParametersViewModel = null;

    this._lblTestCaseMessages = $get(lblTestCaseMessages_id);
    this._msgTestCaseParameters = $get(msgTestCaseParameters_id);
    this._msgTestSetParameters = $get(msgTestSetParameters_id);

    Inflectra.SpiraTest.Web.TestSetDetails.initializeBase(this);
};
Inflectra.SpiraTest.Web.TestSetDetails.prototype =
    {
        /* Constructors */
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.TestSetDetails.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            Inflectra.SpiraTest.Web.TestSetDetails.callBaseMethod(this, 'dispose');
        },

        /* Properties */
        get_testSetId: function ()
        {
            return this._testSetId;
        },
        set_testSetId: function (val)
        {
            this._testSetId = val;
        },

        get_addPanelIsOpen: function ()
        {
            return this._addPanelIsOpen;
        },
        set_addPanelIsOpen: function (value)
        {
            this._addPanelIsOpen = value;
        },

        /* Methods */

        displayEditTestCaseParameters: function (evt)
        {
            //Make sure that only one test case in the grid is selected
            var grdTestCases = $find(grdTestCases_id);
            var testSetTestCaseIds = grdTestCases.get_selected_items();
            if (testSetTestCaseIds.length != 1)
            {
                alert(Inflectra.SpiraTest.Web.GlobalResources.TestSetDetails_SelectOnlyOneTestCase);
                return;
            }

            //Clear the labels
            $get('divHasParameters').style.display = 'none';
            $get('divNoParameters').style.display = 'none';

            //Fetch the data to populate the test case parameters dataview
            var testSetTestCaseId = testSetTestCaseIds[0];
            this._currentTestSetTestCaseId = testSetTestCaseId;
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetTestCaseService.RetrieveParameters(testSetTestCaseId, AspNetAjax$Function.createDelegate(this, this.tblTestCaseParametersEdit_success), AspNetAjax$Function.createDelegate(this, this.tblTestCaseParametersEdit_failure), evt);
        },
        tblTestCaseParametersEdit_success: function (data, evt)
        {
			globalFunctions.hide_spinner();

			//Remove duplicate names from the array - each item has a unique parameter ID, but the name is the only relevant part for setting the param values at execution
			var valuesUnique = [],
				dataUnique = [];

			data.forEach(item => {
				const name = item.Fields.Name.textValue,
					value = item.Fields.Value.textValue;
				// This dedupe code was added in 6.7.1 - before then users may have set values on identically named parameters. 
				// In such cases we should still show that parameter so the user can clearly see what is going on
				if (valuesUnique.indexOf(name) < 0 || value) {
					valuesUnique.push(name);
					dataUnique.push(item);
				}
			})
            //databind the grid using knockout
            if (this._testSetTestCaseParametersViewModel)
            {
				ko.mapping.fromJS(dataUnique, this._testSetTestCaseParametersViewModel);
            }
            else
            {
				this._testSetTestCaseParametersViewModel = ko.mapping.fromJS(dataUnique);
                ko.applyBindings(this._testSetTestCaseParametersViewModel, $get('tblTestCaseParametersEdit'));
            }

            //Display the edit parameters dialog box
            var pnlEditTestCaseParameters = $find(pnlEditTestCaseParameters_id);
            pnlEditTestCaseParameters.display(evt);

            //Display the appropriate legend
            if (data && data.length > 0)
            {
                $get('divHasParameters').style.display = 'block';
            }
            else
            {
                $get('divNoParameters').style.display = 'block';
            }
        },
        tblTestCaseParametersEdit_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._lblTestCaseMessages, exception);
        },
        pnlEditTestCaseParameters_tblTestCaseParametersEdit_keydown: function (data, evt)
        {
            //If enter pressed, stop event propagation and updated test case parameters
            var keynum = evt.keyCode | evt.which;
            if (keynum == 13)
            {
                // stop the event bubble
                evt.preventDefault();
                evt.stopPropagation();
                //Fire the update event
                this.pnlEditTestCaseParameters_btnTestParametersUpdate_click(evt);
                return false;
            }
            return true;
        },
        pnlEditTestCaseParameters_btnTestParametersUpdate_click: function (evt)
        {
            //Get the updated data object from the viewmodel and pass to the update web service
            var testSetTestCaseId = this._currentTestSetTestCaseId;
            var parameterValues = ko.mapping.toJS(this._testSetTestCaseParametersViewModel);

            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetTestCaseService.UpdateParameters(testSetTestCaseId, parameterValues, AspNetAjax$Function.createDelegate(this, this.tblTestCaseParametersUpdate_success), AspNetAjax$Function.createDelegate(this, this.tblTestCaseParametersUpdate_failure));
        },
        tblTestCaseParametersUpdate_success: function ()
        {
            globalFunctions.hide_spinner();
            //Close the dialog box
            var pnlEditTestCaseParameters = $find(pnlEditTestCaseParameters_id);
            pnlEditTestCaseParameters.close();

            //Reload the test cases grid (in case parameters displayed as a column)
            var grdTestCases = $find(grdTestCases_id);
            grdTestCases.load_data();
        },
        tblTestCaseParametersUpdate_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgTestCaseParameters, exception);
        },

        execute_test_set: function ()
        {
            var ajxBackgroundProcessManager = $find(ajxBackgroundProcessManager_id);

            //Actually start the background process of creating the test runs
            ajxBackgroundProcessManager.display(SpiraContext.ProjectId, 'TestSet_Execute', resx.TestSetList_ExecuteTestSet, resx.TestSetList_ExecuteTestSetDesc, this._testSetId);
        },
        grdTestCases_execute: function ()
        {
            var ajxBackgroundProcessManager = $find(ajxBackgroundProcessManager_id);

            //Get the list of selected test cases
            var grdTestCases = $find(grdTestCases_id);
            var testCaseIds = grdTestCases.get_selected_items();
            if (testCaseIds.length < 1)
            {
                alert(resx.TestSetDetails_SelectTestCaseFirst);
                return;
            }

            //Actually start the background process of creating the test runs
            ajxBackgroundProcessManager.display(SpiraContext.ProjectId, 'TestSet_TestCaseExecute', resx.TestSetDetails_ExecuteTestCases, resx.TestSetDetails_ExecuteTestCasesDesc, this._testSetId, testCaseIds);
        },

        ajxBackgroundProcessManager_success: function (msg, returnCode)
        {
            //Need to redirect to the test runs pending or to the test automation launch file
            //A return code of -2 means that this is an automated test and need to redirect to the .TST launch page
            if (returnCode == -2)
            {
                window.location.href = urlTemplate_launchUrl;
            }
            else if (returnCode && returnCode > 0)
            {
                var ajxBackgroundProcessManager = $find(ajxBackgroundProcessManager_id);
                var projectId = ajxBackgroundProcessManager.get_projectId();

                //set the base url to either exploratory or normal - based on what is returned from the server in the success params
                var baseUrl = msg === "testcase_executeexploratory" ? urlTemplate_testRunsPendingExploratory : urlTemplate_testRunsPending;

                var url = baseUrl.replace(globalFunctions.artifactIdToken, returnCode).replace(globalFunctions.projectIdToken, projectId);
                window.location.href = url;
            }
        },

        grdTestCases_loaded: function ()
        {
        },

        ddlAutomationHost_changed: function (args)
        {
            //If the automation host is set, change test type to automated and vice-versa
            var ddlAutomationHost = $find(ddlAutomationHost_id);
            var ddlTestRunType = $find(ddlTestRunType_id);
            if (ddlAutomationHost.get_selectedItem().get_value() == '')
            {
                ddlTestRunType.set_selectedItem(globalFunctions.testRunTypeEnum.manual);
            }
            else
            {
                ddlTestRunType.set_selectedItem(globalFunctions.testRunTypeEnum.automated);
            }
        },

        load_parameterValues: function ()
        {
            //Call the service to get the list of test set parameter values
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.RetrieveParameterValues(SpiraContext.ProjectId, this._testSetId, AspNetAjax$Function.createDelegate(this, this.load_parameterValues_success), AspNetAjax$Function.createDelegate(this, this.load_parameterValues_failure));
        },
        load_parameterValues_success: function (data)
        {
            //Clear the local state of any parameter names / make sure the array has been created
            SpiraContext.uiState.testSetParametersWithValues = [];

            //Add the editable flag to the elements and set a value for value if null
            if (data)
            {
                for (var item in data)
                {
                    data[item].editable = false;
                    if (!data[item].Fields.Value.textValue)
                    {
                        data[item].Fields.Value.textValue = '';
                    }
                    if (!data[item].Fields.DefaultValue.textValue)
                    {
                        data[item].Fields.DefaultValue.textValue = '';
                    }

                    //Add the parameter names used to state on page for use elsewhere
                    SpiraContext.uiState.testSetParametersWithValues.push(data[item].Fields.Name.textValue);
                }
            }

            //databind the grid using knockout
            if (this._testSetParametersViewModel)
            {
                ko.mapping.fromJS(data, this._testSetParametersViewModel);
            }
            else
            {
                this._testSetParametersViewModel = ko.mapping.fromJS(data);
                ko.applyBindings(this._testSetParametersViewModel, $get('tblTestSetParameterValues'));
            }
        },
        load_parameterValues_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgTestSetParameters, exception);
        },

        testSetParameters_edit: function (testSetParameter)
        {
            //Toggle the editable flag
            var editable = testSetParameter.editable();
            testSetParameter.editable(!editable);
        },

        testSetParameters_delete: function (testSetParameter)
        {
            //Delete the value
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.DeleteParameterValue(SpiraContext.ProjectId, this._testSetId, testSetParameter.primaryKey(), AspNetAjax$Function.createDelegate(this, this.update_parameterValues_success), AspNetAjax$Function.createDelegate(this, this.load_parameterValues_failure));
        },
        testSetParameters_save: function (testSetParameter)
        {
            //Save the current value
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.UpdateParameterValue(SpiraContext.ProjectId, this._testSetId, testSetParameter.primaryKey(), testSetParameter.Fields.Value.textValue(), AspNetAjax$Function.createDelegate(this, this.update_parameterValues_success), AspNetAjax$Function.createDelegate(this, this.load_parameterValues_failure));
        },
        update_parameterValues_success: function ()
        {
            this.load_parameterValues();
        },
        btnAddTestSetParameterValue_click: function (evt)
        {
            //load the dropdown
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.RetrieveParameters(SpiraContext.ProjectId, this._testSetId, AspNetAjax$Function.createDelegate(this, this.btnAddTestSetParameterValue_click_success), AspNetAjax$Function.createDelegate(this, this.load_parameterValues_failure), evt);
        },
        btnAddTestSetParameterValue_click_success: function (data, evt)
        {
            globalFunctions.hide_spinner();

            //Remove duplicate names from the object - each item has a unique parameter ID, but the name is the only relevant part for setting the param values at execution
            var valuesUnique = [],
                dataUnique = {},
                dataUniqueCount = 0;

            //Make a deep copy of the parameteres that already have values set (if any) - so when we change valuesUnique we do not change the uiState of parameters in use
            if (SpiraContext.uiState.testSetParametersWithValues && SpiraContext.uiState.testSetParametersWithValues.length) {
                valuesUnique = [...SpiraContext.uiState.testSetParametersWithValues];
            }

            for (const property in data) {
                if (property !== "__type") {
                    if (valuesUnique.indexOf(data[property]) < 0) {
                        valuesUnique.push(data[property]);
                        dataUnique[property] = data[property];
                        dataUniqueCount++;
                    }
                }
            }

            if (dataUniqueCount > 0) {
                //databind the dropdown list
                var ddlParameterName = $find(ddlParameterName_id);
                ddlParameterName.clearItems();
                ddlParameterName.addItem('', resx.Global_PleaseSelect, true);
                ddlParameterName.set_dataSource(dataUnique);
                ddlParameterName.dataBind();
                ddlParameterName.set_selectedItem('', true);
                $get(txtNewParameterValue_id).value = '';

                //Display the add parameter value dialog box and load the dropdown list if not already loaded
                var pnlAddTestSetParameter = $find(pnlAddTestSetParameter_id);
                pnlAddTestSetParameter.display(evt);
            } else {
                //Display messages that there are no parameters to add/show
                globalFunctions.globalAlert(resx.TestSetDetails_ParamatersNoneToAdd, "info", true, null, "fas fa-info-circle mr3 o-70");
            }
        },
        testSetParameters_add: function ()
        {
            globalFunctions.hide_spinner();
            var ddlParameterName = $find(ddlParameterName_id);
            var txtNewParameterValue = $get(txtNewParameterValue_id);
            var newValue = globalFunctions.trim(txtNewParameterValue.value);
            if (!ddlParameterName.get_selectedItem() || ddlParameterName.get_selectedItem().get_value() == '')
            {
                alert(resx.TestSetDetails_ParameterRequired);
                return;
            }
            if (!newValue || newValue == '')
            {
                alert(resx.TestSetDetails_ParameterValueRequired);
                return;
            }

            var testCaseParameterId = parseInt(ddlParameterName.get_selectedItem().get_value());

            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.AddParameterValue(SpiraContext.ProjectId, this._testSetId, testCaseParameterId, newValue, AspNetAjax$Function.createDelegate(this, this.testSetParameters_add_success), AspNetAjax$Function.createDelegate(this, this.load_parameterValues_failure));
        },
        testSetParameters_add_success: function ()
        {
            //Close the dialog box and release
            globalFunctions.hide_spinner();
            var pnlAddTestSetParameter = $find(pnlAddTestSetParameter_id);
            pnlAddTestSetParameter.close();

            this.load_parameterValues();
        },
        closeAddAssocationPanel: function ()
        {
            //first check to see if the panel is actually open
            var domId = panelAssociationAdd.lnkAddBtnId
            if (SpiraContext.uiState[domId].isMounted)
            {
                $("#" + panelAssociationAdd.lnkAddBtnId).removeClass('disabled');
                SpiraContext.uiState[domId].isMounted = false;
                ReactDOM.unmountComponentAtNode(document.getElementById(domId));
            }
        }
    };


