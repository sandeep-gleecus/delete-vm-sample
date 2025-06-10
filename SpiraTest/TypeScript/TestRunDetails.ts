//external dependendencies (js libraries)
declare var $: any;
declare var $find: any;
declare var $get: any;
declare var Type: any;
declare var ko: any;
declare var AspNetAjax$Function: any;
declare function hideddrivetip(): void;
declare function ddrivetip(tooltipdata: string): void;

//inflectra services
declare var globalFunctions: any;
declare var Inflectra: any;
declare var SpiraContext: any;

//inflectra objects
declare var ajxBackgroundProcessManager_id: string;
declare var ajxFormManager_id: string;

//Page objects
declare var msgChangeTestSetMessages_id: string;
declare var tclTestRunDetails_id: string;
declare var pnlOverview_id: string;
declare var lblMessage_id: string;
declare var dlgTestRunStepIncidents_id: string;
declare var ddlBuild_id: string;
declare var pnlChangeTestSet_id: string;
declare var ddlTestSetFolders_id: string;
declare var ajxTestSetSelector_id: string;
declare var lnkTestSet_id: string;
declare var pnlChangeTestSet_id: string;
declare var formGroup_testSteps_id: string;
declare var pnlAddAssociation_id: string;

//URL templates
declare var urlTemplate_testRunsPending: string;
declare var urlTemplate_testRunsPendingExploratory: string;

Type.registerNamespace('Inflectra.SpiraTest.Web');
Inflectra.SpiraTest.Web.TestRunDetails = function ()
{
    this._isOverNameDesc = false;
    this._initialBuildLoad = true;
    this._testCaseId = null;
    this._testSetId = null;
    this._testSetTestCaseId = null;
/*    this._testCaseId = <%=testCaseId %>;
    this._testSetId = <%=(testSetId.HasValue) ? testSetId.Value.ToString() : "null" %>;
    this._testSetTestCaseId = <%=(testSetCaseId.HasValue) ? testSetCaseId.Value.ToString() : "null" %>;*/
    this._testRunStepsViewModel = null;
    this._incidentsViewModel = null;
    this._testRunStepId = null;
    this._testSetFolders = null;
    this._msgChangeTestSetMessages = $get(msgChangeTestSetMessages_id);

    Inflectra.SpiraTest.Web.TestRunDetails.initializeBase(this);
};
Inflectra.SpiraTest.Web.TestRunDetails.prototype =
    {
        /* Constructors */
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.TestRunDetails.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            delete this._dialog;
            Inflectra.SpiraTest.Web.TestRunDetails.callBaseMethod(this, 'dispose');
        },

        /* Properties */
        get_testRunStepId: function ()
        {
            return this._testRunStepId;
        },

        get_testCaseId: function ()
        {
            return this._testCaseId;
        },
        set_testCaseId: function (value)
        {
            this._testCaseId = value;
        },

        get_testSetId: function ()
        {
            return this._testSetId;
        },
        set_testSetId: function (value)
        {
            this._testSetId = value;
        },

        get_testSetTestCaseId: function ()
        {
            return this._testSetTestCaseId;
        },
        set_testSetTestCaseId: function (value)
        {
            this._testSetTestCaseId = value;
        },

        /* Public Methods */
        loadTestRunSteps: function ()
        {
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.TestRunStepService.TestRunStep_Retrieve(SpiraContext.ProjectId, SpiraContext.ArtifactId, AspNetAjax$Function.createDelegate(this, this.loadTestRunSteps_success), AspNetAjax$Function.createDelegate(this, this.loadTestRunSteps_failure));
        },
        loadTestRunSteps_success: function (data)
        {
            globalFunctions.hide_spinner();

            //See if we have data
            if (data && data.items && data.items.length > 0)
            {
                //Show the steps
                $('#' + formGroup_testSteps_id).show();

                //Set the flag in the tab control that we have data
                var tabControl = $find(tclTestRunDetails_id).updateHasData(pnlOverview_id, true);

                //databind the grid using knockout
                if (this._testRunStepsViewModel)
                {
                    ko.mapping.fromJS(data, this._testRunStepsViewModel);
                }
                else
                {
                    this._testRunStepsViewModel = ko.mapping.fromJS(data);
                    ko.applyBindings(this._testRunStepsViewModel, $('#grdTestRunSteps')[0]);
                }
            }
            else
            {
                //Hide the steps
                $('#' + formGroup_testSteps_id).hide();
            }
        },
        loadTestRunSteps_failure: function (exception)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get(lblMessage_id), exception);
        },

        display_incidents: function (testRunStep, evt)
        {
            if (!testRunStep)
            {
                return;
            }
            var testRunStepId = testRunStep.primaryKey();
            if (!evt)
            {
                evt = window.event;
            }
            if (!this._dialog)
            {
                this._dialog = $find(dlgTestRunStepIncidents_id);
            }
            //Set the title and position the dialog box
            var artifactPrefix = globalFunctions.getArtifactTypes(globalFunctions.artifactTypeEnum.testRunStep)[0].token;
            var title = resx.TestRunDetails_IncidentList + ' ' + globalFunctions.formatArtifactId(artifactPrefix, testRunStepId);
            this._dialog.set_title(title);
            this._dialog.auto_position(evt);
            this.load_incidents(testRunStepId, null, evt);
        },
        load_incidents: function (testRunStepId, testStepId, evt)
        {
            //Load the incident data for this test run step / test step
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.RetrieveByTestRunStepId(SpiraContext.ProjectId, testRunStepId, testStepId, AspNetAjax$Function.createDelegate(this, this.load_incidents_success), AspNetAjax$Function.createDelegate(this, this.load_incidents_failure), evt);
        },
        load_incidents_success: function (data, evt)
        {
            globalFunctions.hide_spinner();

            //databind the grid using knockout
            if (this._incidentsViewModel)
            {
                ko.mapping.fromJS(data, this._incidentsViewModel);
            }
            else
            {
                this._incidentsViewModel = ko.mapping.fromJS(data);
                ko.applyBindings(this._incidentsViewModel, $('#tblTestRunStepIncidents')[0]);
            }

            //Show dialog
            this._dialog.display(evt);
        },
        load_incidents_failure: function (exception)
        {
            globalFunctions.hide_spinner();
            globalFunctions.display_error($get(lblMessage_id), exception);
        },

        displayIncidentTooltip: function (incidentId)
        {
            //Display the loading message
            ddrivetip(resx.GlobalFunctions_TooltipLoading);
            this._isOverNameDesc = true;   //Set the flag since asynchronous

            //Now get the real tooltip via Ajax web-service call
            Inflectra.SpiraTest.Web.Services.Ajax.IncidentsService.RetrieveNameDesc(SpiraContext.ProjectId, incidentId, null, AspNetAjax$Function.createDelegate(this, this.displayIncidentTooltip_success), AspNetAjax$Function.createDelegate(this, this.displayIncidentTooltip_failure));
        },
        displayIncidentTooltip_success: function (tooltipData)
        {
            if (this._isOverNameDesc)
            {
                ddrivetip(tooltipData);
            }
        },
        displayIncidentTooltip_failure: function (exception)
        {
            //Fail quietly
        },
        hideIncidentTooltip: function ()
        {
            hideddrivetip();
            this._isOverNameDesc = false;
        },

        //Changes the build list when release changed
        ddlRelease_selectedItemChanged: function (item)
        {
            //Get the current state of the form manager
            var ajxFormManager = $find(ajxFormManager_id);
            var unsavedChanges = ajxFormManager.get_unsavedChanges();

            //Get the new releaseId
            if (item.get_value() != '')
            {
                var releaseId = item.get_value();
                this.updateBuildList(releaseId, unsavedChanges);
            }
        },

        updateBuildList: function (releaseId, unsavedChanges)
        {
            //Get the list of builds for this release from the web service
            globalFunctions.display_spinner();
            Inflectra.SpiraTest.Web.Services.Ajax.BuildService.GetBuildsForRelease(SpiraContext.ProjectId, releaseId, AspNetAjax$Function.createDelegate(this, this.updateBuildList_success), AspNetAjax$Function.createDelegate(this, this.updateBuildList_failure), unsavedChanges);
        },

        updateBuildList_success: function (data, unsavedChanges)
        {
            var ajxFormManager = $find(ajxFormManager_id);

            //Clear values and databind
            var ddlBuild = $find(ddlBuild_id);
            ddlBuild.clearItems();
            ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
            if (data)
            {
                ddlBuild.set_dataSource(data);
                ddlBuild.dataBind();
                var dataItem = ajxFormManager.get_dataItem();
                if (dataItem.Fields.BuildId && dataItem.Fields.BuildId.intValue > 0)
                {
                    ddlBuild.set_selectedItem(dataItem.Fields.BuildId && dataItem.Fields.BuildId.intValue);
                }
                else
                {
                    ddlBuild.set_selectedItem('');
                }
            }

            //Set the form manager 'unsaved changes' flag back to what it was before
            if (this._initialBuildLoad)
            {
                ajxFormManager.update_saveButtons(unsavedChanges);
                this._initialBuildLoad = false;
            }

            //Hide spinner
            globalFunctions.hide_spinner();
        },

        updateBuildList_failure: function (error)
        {
            var ddlBuild = $find(ddlBuild_id);
            ddlBuild.clearItems();
            ddlBuild.addItem('', '-- ' + resx.Global_None + ' --');
            ddlBuild.set_selectedItem('');

            //Hide spinner, ignore error, just clear dropdown values
            globalFunctions.hide_spinner();
        },
        display_link_incidents: function (testRunStep, evt)
        {
            this._testRunStepId = testRunStep.primaryKey();
            var pnlAddAssociation = $find(pnlAddAssociation_id);
            pnlAddAssociation.display(evt);
        },

        execute_test_case: function ()
        {
            var ajxBackgroundProcessManager = $find(ajxBackgroundProcessManager_id);

            //Actually start the background process of creating the test runs
            if (this._testSetId && this._testSetTestCaseId)
            {
                ajxBackgroundProcessManager.display(SpiraContext.ProjectId, 'TestSet_TestCaseExecute', resx.TestCaseList_ExecuteTestCase, resx.TestCaseList_ExecuteTestCaseDesc, this._testSetId, [this._testSetTestCaseId]);
            }
            else
            {
                ajxBackgroundProcessManager.display(SpiraContext.ProjectId, 'TestCase_Execute', resx.TestCaseList_ExecuteTestCase, resx.TestCaseList_ExecuteTestCaseDesc, this._testCaseId);
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

        lnkTestSet_changeClicked: function (evt)
        {
            //Load the folders once
            if (!this._testSetFolders)
            {
                globalFunctions.display_spinner();
                Inflectra.SpiraTest.Web.Services.Ajax.TestSetService.RetrieveTestSetFolders(SpiraContext.ProjectId, AspNetAjax$Function.createDelegate(this, this.load_testSetFolders_success), AspNetAjax$Function.createDelegate(this, this.load_testSetFolders_failure))
            }

            //Display the dialog box
            var pnlChangeTestSet = $find(pnlChangeTestSet_id);
            pnlChangeTestSet.display(evt);
        },

        load_testSetFolders_success: function (data)
        {
            //Databind the dropdown list
            globalFunctions.hide_spinner();
            this._testSetFolders = data;
            var ddlTestSetFolders = $find(ddlTestSetFolders_id);
            ddlTestSetFolders.set_dataSource(data);
            ddlTestSetFolders.dataBind();
        },
        load_testSetFolders_failure: function (exception)
        {
            //Display the error
            globalFunctions.hide_spinner();
            globalFunctions.display_error(this._msgChangeTestSetMessages, exception);
        },
        ddlTestSetFolders_changed: function (item)
        {
            var folderId = item.get_value();
            var standardFilters = { TestSetFolderId: globalFunctions.serializeValueInt(folderId) };
            var ajxTestSetSelector = $find(ajxTestSetSelector_id);
            ajxTestSetSelector.set_standardFilters(standardFilters);
            ajxTestSetSelector.load_data();
        },
        pnlChangeTestSet_updateItem: function ()
        {
            //Get the currently selected item
            var lnkTestSet = $find(lnkTestSet_id);
            var ajxTestSetSelector = $find(ajxTestSetSelector_id);
            var items = ajxTestSetSelector.get_selectedItems();
            var testSetId = null;
            if (items.length > 0)
            {
                //Get the selected item
                testSetId = items[0];
                var testSetName = ajxTestSetSelector.get_artifactName(testSetId);
                lnkTestSet.update_artifact(testSetId, testSetName, '');
            }
            else
            {
                //No test set selected
                lnkTestSet.update_artifact(testSetId, '', '');
            }

            //Close the dialog box
            var pnlChangeTestSet = $find(pnlChangeTestSet_id);
            pnlChangeTestSet.close();
        }
    };
