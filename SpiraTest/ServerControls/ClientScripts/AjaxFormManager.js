var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

//Allows forms to retrieve/save data without needing to use postbacks
Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');
Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager.initializeBase(this, [element]);

    this._webServiceClass = null;
    this._primaryKey = null;
    this._errorMessageControlId = '';
    this._folderPathControlId = '';
    this._controlReferences = null;
    this._controlLabels = null;
    this._saveButtons = null;
    this._hyperLinkControls = null;
    this._unsavedChanges = false;
    this._dataItem = null;
    this._artifactTypeName = 'Unknown';
    this._windowOnBeforeUnloadHandler = null;
    this._documentOnClickHandler = null;
    this._autoLoad = true;
	this._workflowEnabled = false;
	this._readOnly = false;
    this._loadedOnce = false;
    this._checkUnsaved = true;
    this._suppressErrors = false;
    this._workflowStepLabel = null;
    this._workflowStepField = null;
    this._revertButtonControlId = null;
    this._artifactImageControlId = null;
    this._workflowOperationsControlId = null;
    this._workflowChangeControl = null;
    this._oldStatusId = null;
    this._oldStatusName = '';
    this._newStatusName = '';
    this._ignoreUnloadEvent = false;
    this._projectId = -1;
    this._revertStatusOpen = true;
    this._themeFolder = '';
    this._itemImage = '';
    this._summaryItemImage = '';
    this._alternateItemImage = '';
    this._artifactTypePrefix = '';
    this._signatureRequired = false;
    this._signatureDialogBox = null;
    this._signatureLogin = null;
    this._signaturePassword = null;
    this._signatureMeaning = null;
    this._displayPageName = false;
    this._nameField = '';
    this._descriptionField = '';
    this._licensedProduct == '';
    this._newItemName = '';
    this._hideOnAuthorizationFailure = false;
    this._folderPathUrlTemplate = '';

    this._fieldType_text = 1;
    this._fieldType_lookup = 2;
    this._fieldType_dateTime = 3;
    this._fieldType_identifier = 4;
    this._fieldType_equalizer = 5;
    this._fieldType_nameDescription = 6;
    this._fieldType_customPropertyLookup = 7;
    this._fieldType_integer = 8;
    this._fieldType_timeInterval = 9;
    this._fieldType_flag = 10;
    this._fieldType_hierarchyLookup = 11;
    this._fieldType_html = 12;

    this._formControlDirection_In = 1;
    this._formControlDirection_Out = 2;
    this._formControlDirection_InOut = 3;

    this._textboxChangedHandlers = new Array();
    this._textboxReferences = new Array();
    this._checkboxChangedHandlers = new Array();
    this._checkboxReferences = new Array();
    this._clickHandlers = new Array();
    this._clickElements = new Array();
}
Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager.callBaseMethod(this, 'initialize');

        //Register the window.onbeforeunload and document.onclick handlers
        //window.onlick was not reliable in different versions of IE
        this._windowOnBeforeUnloadHandler = Function.createDelegate(this, this._onBeforeUnload);
        this._documentOnClickHandler = Function.createDelegate(this, this._onClick);
        window.addEventListener('beforeunload', this._windowOnBeforeUnloadHandler);
        $addHandler(document, 'click', this._documentOnClickHandler);

        //Hide the Revert button
        this.update_revert();

        //Disable the save buttons
        this.update_saveButtons();
        //Load once initially
        if (this._autoLoad)
		{
			var that = this;
			//Make sure DOM ready
			$(function ()
			{
				that.load_data(true);
			}); 
        }
    },
    dispose: function ()
    {
        //Clear the various handlers
        this._clearTextboxHandlers();
        this._clearCheckboxHandlers();
        this._clearClickHandlers();

        window.removeEventListener('beforeunload', this._windowOnBeforeUnloadHandler);
        $removeHandler(document, 'click', this._documentOnClickHandler);
        delete this._windowOnBeforeUnloadHandler;
        delete this._documentOnClickHandler;

        //Delete references
        delete this._controlReferences;
        delete this._controlLabels;
        delete this._textboxChangedHandlers;
        delete this._textboxReferences;
        delete this._checkboxChangedHandlers;
        delete this._checkboxReferences;
        delete this._workflowChangeControl;
        delete this._signatureDialogBox;
        delete this._signatureLogin;
        delete this._signaturePassword;
        delete this._signatureMeaning;

        Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function ()
    {
        return this._element;
    },
    get_dataItem: function ()
    {
        return this._dataItem;
    },

    get_isArtifactCreatorOrOwner: function()
    {
        //_IsArtifactCreatorOrOwner is a special field used to return back if we're the owner/author
        if (this._dataItem && this._dataItem.Fields['_IsArtifactCreatorOrOwner'] && this._dataItem.Fields['_IsArtifactCreatorOrOwner'].textValue)
        {
            return (this._dataItem.Fields['_IsArtifactCreatorOrOwner'].textValue == 'Y');
        }
        return false;
    },

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },
    
    get_hideOnAuthorizationFailure: function ()
    {
        return this._hideOnAuthorizationFailure;
    },
    set_hideOnAuthorizationFailure: function (value)
    {
        this._hideOnAuthorizationFailure = value;
    },

    get_newItemName: function ()
    {
        return this._newItemName;
    },
    set_newItemName: function (value)
    {
        this._newItemName = value;
    },
    
    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
    },

    get_workflowEnabled: function ()
    {
        return this._workflowEnabled;
    },
    set_workflowEnabled: function (value)
    {
        this._workflowEnabled = value;
	},

	get_readOnly: function () {
		return this._readOnly;
	},
	set_readOnly: function (value) {
		this._readOnly = value;
	},

    get_displayPageName: function(value)
    {
        this._displayPageName = value;
    },
    set_displayPageName: function (value)
    {
        this._displayPageName = value;
    },

    get_nameField: function (value)
    {
        this._nameField = value;
    },
    set_nameField: function (value)
    {
        this._nameField = value;
    },

    get_descriptionField: function (value)
    {
        this._descriptionField = value;
    },
    set_descriptionField: function (value)
    {
        this._descriptionField = value;
    },

    get_licensedProduct: function (value)
    {
        this._licensedProduct = value;
    },
    set_licensedProduct: function (value)
    {
        this._licensedProduct = value;
    },

    get_checkUnsaved: function ()
    {
        return this._checkUnsaved;
    },
    set_checkUnsaved: function (value)
    {
        this._checkUnsaved = value;
    },

    get_signatureRequired: function ()
    {
        return this._signatureRequired;
    },
    set_signatureRequired: function (value)
    {
        this._signatureRequired = value;
    },

    get_webServiceClass: function ()
    {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value)
    {
        this._webServiceClass = value;
    },

    get_projectId: function ()
    {
        return this._projectId;
    },
    set_projectId: function (value)
    {
        this._projectId = value;
    },

    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value, updateKeyForWorkflowOperations)
    {
        this._primaryKey = value;
        if (updateKeyForWorkflowOperations && this._workflowOperationsControlId)
        {
            //See if we have one control or many
            if (this._workflowOperationsControlId.indexOf(',') == -1)
            {
                var workflowOperationsControl = $find(this._workflowOperationsControlId);
                if (workflowOperationsControl)
                {
                    workflowOperationsControl.set_primaryKey(this._primaryKey);
                }
            }
            else
            {
                var controlIds = this._workflowOperationsControlId.split(',');
                for (var i = 0; i < controlIds.length; i++)
                {
                    var workflowOperationsControl = $find(controlIds[i]);
                    if (workflowOperationsControl)
                    {
                        workflowOperationsControl.set_primaryKey(this._primaryKey);
                    }
                }
            }
        }
    },

    get_workflowStepLabel: function ()
    {
        return this._workflowStepLabel;
    },
    set_workflowStepLabel: function (value)
    {
        this._workflowStepLabel = value;
    },

    get_workflowStepField: function ()
    {
        return this._workflowStepField;
    },
    set_workflowStepField: function (value)
    {
        this._workflowStepField = value;
    },

    get_errorMessageControlId: function ()
    {
        return this._errorMessageControlId;
    },
    set_errorMessageControlId: function (value)
    {
        this._errorMessageControlId = value;
    },

    get_folderPathControlId: function ()
    {
        return this._folderPathControlId;
    },
    set_folderPathControlId: function (value)
    {
        this._folderPathControlId = value;
    },

    //folderPathUrlTemplate
    get_folderPathUrlTemplate: function () {
        return this._folderPathUrlTemplate;
    },
    set_folderPathUrlTemplate: function (value) {
        this._folderPathUrlTemplate = value;
    },

    get_revertButtonControlId: function ()
    {
        return this._revertButtonControlId;
    },
    set_revertButtonControlId: function (value)
    {
        this._revertButtonControlId = value;
    },

    get_artifactImageControlId: function ()
    {
        return this._artifactImageControlId;
    },
    set_artifactImageControlId: function (value)
    {
        this._artifactImageControlId = value;
    },

    get_workflowOperationsControlId: function ()
    {
        return this._workflowOperationsControlId;
    },
    set_workflowOperationsControlId: function (value)
    {
        this._workflowOperationsControlId = value;
    },

    get_controlReferences: function ()
    {
        return this._controlReferences;
    },
    set_controlReferences: function (value)
    {
        this._controlReferences = value;
    },

    get_controlLabels: function ()
    {
        return this._controlLabels;
    },
    set_controlLabels: function (value)
    {
        this._controlLabels = value;
    },

    get_saveButtons: function ()
    {
        return this._saveButtons;
    },
    set_saveButtons: function (value)
    {
        this._saveButtons = value;
    },

    get_hyperLinkControls: function ()
    {
        return this._hyperLinkControls;
    },
    set_hyperLinkControls: function (value)
    {
        this._hyperLinkControls = value;
    },

    get_artifactTypeName: function ()
    {
        return this._artifactTypeName;
    },
    set_artifactTypeName: function (value)
    {
        this._artifactTypeName = value;
    },

    get_artifactTypePrefix: function ()
    {
        return this._artifactTypePrefix;
    },
    set_artifactTypePrefix: function (value)
    {
        this._artifactTypePrefix = value;
    },

    //Used when we want to prevent errors being displayed
    get_suppressErrors: function ()
    {
        return this._suppressErrors;
    },
    set_suppressErrors: function (value)
    {
        this._suppressErrors = value;
    },

    get_unsavedChanges: function ()
    {
        return this._unsavedChanges;
    },
    set_unsavedChanges: function (value)
    {
        this._unsavedChanges = value;
    },

    get_itemImage: function ()
    {
        return this._itemImage;
    },
    set_itemImage: function (value)
    {
        this._itemImage = value;
    },

    get_summaryItemImage: function ()
    {
        return this._summaryItemImage;
    },
    set_summaryItemImage: function (value)
    {
        this._summaryItemImage = value;
    },

    get_alternateItemImage: function ()
    {
        return this._alternateItemImage;
    },
    set_alternateItemImage: function (value)
    {
        this._alternateItemImage = value;
    },

    get_loadedOnce: function ()
    {
        return this._loadedOnce;
    },

    /*  =========================================================
    The event handlers
    =========================================================  */
    _onClick: function (evt)
    {
        //We store the clicked object so that the onbeforeunload event know what was clicked
        this._ignoreUnloadEvent = false;

        var targ;
        if (evt.target)
        {
            targ = evt.target;
        }
        else if (evt.srcElement)
        {
            targ = evt.srcElement;
        }
        if (targ.nodeType == 3) // defeat Safari bug
        {
            targ = targ.parentNode;
        }
        //If this was a void(0) anchor tag, set the flag that it's an unload event to be ignored
        //(we really only need this for IE)
        if (targ && targ.href && targ.href.indexOf('javascript:') != -1)
        {
            this._ignoreUnloadEvent = true;
        }
    },
    _onBeforeUnload: function (evt)
    {
        //For IE it erroneously occurs on lots of javascript:void(0) links we need to disable for now
        if (document.all)
        {
            return;
        }

        //Customize the message
        if (this._unsavedChanges && this._checkUnsaved && !this._ignoreUnloadEvent)
        {
            //Prevent error messages being turned off
            globalFunctions.set_suppressErrors(false);
            evt.preventDefault();
            evt.stopPropagation();
            evt.returnValue = resx.AjaxFormManager_ConfirmUnload;
            return resx.AjaxFormManager_ConfirmUnload;
        }
    },

    //Called when the field changes that determines the workflow
    onWorkflowChanged: function (item)
    {
        var typeId = item.get_value();
        //See if we need to revert
        if (!this._oldStatusId)
        {
            //Load the workflow states
            var statusId = this._dataItem.Fields[this.get_workflowStepField()].intValue;
            this.load_workflow_states(typeId, statusId);
        }
        else
        {
            this.workflow_revert();
        }
    },

    /*  =========================================================
    Event handlers managers
    =========================================================  */
    add_dataSaved: function (handler)
    {
        this.get_events().addHandler('dataSaved', handler);
    },
    remove_dataSaved: function (handler)
    {
        this.get_events().removeHandler('dataSaved', handler);
    },
    raise_dataSaved: function (operation, newId)
    {
        var h = this.get_events().getHandler('dataSaved');
        if (h) h(operation, newId);
    },

    add_dataFailure: function (handler) {
        this.get_events().addHandler('dataFailure', handler);
    },
    remove_dataFailure: function (handler) {
        this.get_events().removeHandler('dataFailure', handler);
    },
    raise_dataFailure: function (exception) {
        var h = this.get_events().getHandler('dataFailure');
        if (h) h(exception);
    },

    add_loaded: function (handler)
    {
        this.get_events().addHandler('loaded', handler);
    },
    remove_loaded: function (handler)
    {
        this.get_events().removeHandler('loaded', handler);
    },
    raise_loaded: function (dontClearMessages)
    {
        var h = this.get_events().getHandler('loaded');
        if (h) h(dontClearMessages);
    },

    add_operationReverted: function (handler)
    {
        this.get_events().addHandler('operationReverted', handler);
    },
    remove_operationReverted: function (handler)
    {
        this.get_events().removeHandler('operationReverted', handler);
    },
    raise_operationReverted: function (statusId, revertStatusOpen)
    {
        var h = this.get_events().getHandler('operationReverted');
        if (h) h(statusId, revertStatusOpen);
    },

    /*  =========================================================
    The methods
    =========================================================  */
    retrieve_failure: function (exception)
    {
        //Check for auth errors
        if (exception && exception.get_message && exception.get_message() == 'Authorization failed')
        {
            //See if we have to fail quietly
            if (this._hideOnAuthorizationFailure)
            {
                //Do nothing
                globalFunctions.hide_spinner();
            }
            else
            {
                window.location.href = window.location.protocol + "//" + window.location.host + window.location.pathname.substring(0, window.location.pathname.indexOf('/', 1));
            }
        }
        else
        {
            this.operation_failure(exception);
        }
    },
    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        globalFunctions.hide_spinner();
        this.display_error(exception);
    },
    display_error: function (exception)
    {
        if (!this._suppressErrors)
        {
            globalFunctions.display_error($get(this.get_errorMessageControlId()), exception);
        }
    },
    display_error_message: function (message)
    {
        if (!this._suppressErrors)
        {
            globalFunctions.display_error_message($get(this.get_errorMessageControlId()), message);
        }
    },
    display_info: function (message)
    {
        globalFunctions.display_info_message($get(this.get_errorMessageControlId()), message);
    },

    findAssociatedLabel: function (controlId)
    {
        var labels = new Array();
        for (var labelId in this._controlLabels)
        {
            if (this._controlLabels[labelId] == controlId)
            {
                labels.push(labelId);
            }
        }
        return labels;
    },
    updateLabelState: function (labels, dataField)
    {     
        if (labels && labels.length > 0)
        {
            for (var i = 0; i < labels.length; i++)
            {
                var label = $find(labels[i]);
                if (label)
                {
                    label.set_enabled(dataField.editable);
                    label.set_required(dataField.required);
                    label.set_hidden(dataField.hidden);
                }
            }
        }
    },

    //Updates the state of the save buttons
    update_saveButtons: function (unsavedChanges)
    {
        var stateChange = true;
        if (unsavedChanges == this._unsavedChanges)
        {
            stateChange = false;
        }

        if (unsavedChanges != undefined)
        {
            this._unsavedChanges = unsavedChanges;
        }

        //loop through each save button
        for (var saveButton in this._saveButtons)
        {
            var controlType = this._saveButtons[saveButton];
            //Make sure the button is a valid type
            if (controlType == 'ImageEx' || controlType == 'ButtonEx')
            {
                //This is a dom element so use $get
                var domControl = $get(saveButton);
                if (domControl)
                {
                    //Make enabled or disabled
                    if (this._unsavedChanges)
                    {
                        //Make sure we're authorized
                        var authorized = true;
                        if (domControl.getAttribute('tst:unauthorized') || this.get_readOnly())
                        {
                            authorized = false;
                        }
                        if (authorized)
                        {
                            domControl.style.filter = ''; // IE Only
                            domControl.style.opacity = '1.0';  //others
                            domControl.style.mozOpacity = '1.0'; //FireFox only
                            domControl.disabled = false;
                        }
                    }
                    else
                    {
                        domControl.style.filter = 'alpha(opacity=50)'; // IE Only
                        domControl.style.opacity = '0.5';  //others
                        domControl.style.mozOpacity = '0.5'; //FireFox only
                        domControl.disabled = true;
                    }
                }

            }
            if (controlType == 'DropMenu')
            {
                //This is an ajax control and needs to have an enabled flag set
                var ajaxControl = $find(saveButton);
                if (ajaxControl)
                {
                    var domControl = ajaxControl.get_element();
                    if (ajaxControl.get_authorized())
                    {
                        //Make enabled or disabled
                        if (this._unsavedChanges)
                        {
                            domControl.style.filter = ''; // IE Only
                            domControl.style.opacity = '1.0';  //others
                            domControl.style.mozOpacity = '1.0'; //FireFox only
                            domControl.disabled = false;
                            $(domControl).removeClass('btn-default').addClass('btn-primary');
                        }
                        else
                        {
                            domControl.style.filter = 'alpha(opacity=50)'; // IE Only
                            domControl.style.opacity = '0.5';  //others
                            domControl.style.mozOpacity = '0.5'; //FireFox only
                            domControl.disabled = true;
                            $(domControl).removeClass('btn-primary').addClass('btn-default');
                        }

                        ajaxControl.set_enabled(this._unsavedChanges);
                    }
                    else
                    {
                        domControl.style.filter = 'alpha(opacity=50)'; // IE Only
                        domControl.style.opacity = '0.5';  //others
                        domControl.style.mozOpacity = '0.5'; //FireFox only
                        domControl.disabled = true;
                        $(domControl).removeClass('btn-primary').addClass('btn-default');
                    }
                }
            }
        }

        //Also update the title bar
        if (this._checkUnsaved && stateChange)
        {
            if (this._unsavedChanges)
            {
                window.document.title = window.document.title.replace(' - ', '* - ');
            }
            else
            {
                window.document.title = window.document.title.replace('* - ', ' - ');
            }
        }

        //Enable keyboard shortcut for save if changes are made
        if (this._unsavedChanges)
        {
            var self = this;
			Mousetrap.bindGlobal('mod+s', function callSave(e) {
				e.preventDefault();
                //if on a new page (eg new IN page), shortcut is set to the correct function, otherwise use standard save function
                if (SpiraContext.PlaceholderId) {
                    self.save_data(null, 'redirect');
                } else {
                    self.save_data();
                }
            });
        }
        else
        {
            Mousetrap.unbind('mod+enter');
        }
    },

    //Loads the lookup data (true=success)
    load_data: function (registerHandlers, dontClearMessages, hideSpinner)
    {
        //If we have unsaved, changes confirm with the user
        if (this._unsavedChanges)
        {
            if (!confirm(resx.AjaxFormManager_ConfirmRefresh))
            {
                //Data not changed
                return false;
            }
        }

        //Clear any messages
        if (!dontClearMessages)
        {
            globalFunctions.clear_errors($get(this.get_errorMessageControlId()));
        }
        this._loadedOnce = true;

        //Revert back any transitions
        this._newStatusName = this._oldStatusName;
        this._oldStatusId = null;
        this._oldStatusName = '';
        this.set_signatureRequired(false);
        this.update_revert();

        //Hide the spinner if a true value for the hideSpinner parameter was passed back
        if (!hideSpinner)
        {
            globalFunctions.display_spinner();
        }
        var context = {};
        context.registerHandlers = registerHandlers;
        context.dontClearMessages = dontClearMessages;
        var webService = this.get_webServiceClass();
        webService.Form_Retrieve(this._projectId, this._primaryKey, Function.createDelegate(this, this._dataBind), Function.createDelegate(this, this.retrieve_failure), context);
        return true;
    },
    _dataBind: function (dataItem, context)
    {
        //See if we have some data returned
        if (dataItem != null)
        {
            //Store the data item
            this._dataItem = dataItem;

            //Populate the artifact image if required
            if (this._artifactImageControlId && this._artifactImageControlId != '')
            {
                var artifactImageControl = $get(this._artifactImageControlId);
                if (artifactImageControl)
                {
                    //Display the correct image
                    if (dataItem.summary)
                    {
                        artifactImageControl.src = this._themeFolder + '/' + this._summaryItemImage;
                    }
                    else
                    {
                        if (dataItem.alternate)
                        {
                            artifactImageControl.src = this._themeFolder + '/' + this._alternateItemImage;
                        }
                        else
                        {
                            artifactImageControl.src = this._themeFolder + '/' + this._itemImage;
                        }
                    }
                }
            }

            //Set the folder path if we have one
            if (this._folderPathControlId && this._folderPathControlId != '' && dataItem.Fields['_FolderPath'])
            {
                var container = $('#' + this._folderPathControlId);
                globalFunctions.clearContent(container[0]);
                try
                {
                    var items = dataItem.Fields['_FolderPath'].textValue ? JSON.parse(dataItem.Fields['_FolderPath'].textValue) : false;
                    var ul = $('<ul class="ma0 pa0 pl2 silver fs-80"></ul>');

                    var hasFolders = items && items.length && items.length > 0,
                        rootText = hasFolders ? '&#47;&nbsp;' : resx.Folders_Root;
                    ul.append($('<li class="lsn dib">' + rootText + '</li>')); //For the root - the word if the item is actually in root, otherwise a slash

                    if (hasFolders)
                    {
                        for (var i = 0; i < items.length; i++)
                        {
                            var folderSeparator = (i < (items.length - 1)) ? '&nbsp;&#47;&nbsp;' : ''; // set a folder separator to a slash only if there is a folder to add after this one
                            innerText = items[i].name;
                            var url = this._folderPathUrlTemplate.replace(globalFunctions.artifactIdToken, items[i].id);
                            ul.append($('<li class="lsn dib"><a class="tdn tdu-hover:hover" href="' + url + '">' + innerText + '</a>' + folderSeparator + '</li>'));
                        }
                    }
                    $(container).append(ul);
                }
                catch (e) { }
            }

            //Set the page name if appropriate
            if (this._displayPageName && this._nameField)
            {
                if (this._primaryKey && this._primaryKey > 0)
                {
                    if (dataItem.Fields[this._nameField].textValue);
                    {
                        document.title = this._artifactTypePrefix + this._primaryKey + " - " + dataItem.Fields[this._nameField].textValue + ' | ' + this._licensedProduct;
                    }
                }
                else
                {
                    document.title = this._newItemName + ' | ' + this._licensedProduct;
                }
                if (this._descriptionField && dataItem.Fields[this._descriptionField] && dataItem.Fields[this._descriptionField].textValue)
                {
                    var desc = dataItem.Fields[this._descriptionField].textValue;
                    var meta = d.ce('meta');
                    meta.name = "description";
                    meta.content = desc;
                    d.getElementsByTagName('head')[0].appendChild(meta);
                }
            }

            //Now loop through the various controls and populate
            for (var controlReference in this._controlReferences)
            {
                var controlTypeAndId = controlReference.split(':');
                //Split up into type and id
                var controlType = controlTypeAndId[0];
                var controlId = controlTypeAndId[1];

                //Get the associated labels
                var labels = this.findAssociatedLabel(controlId);

                var fieldNamePropertyDirection = this._controlReferences[controlReference].split(':');
                //Split up into name, property and direction
                var fieldName = fieldNamePropertyDirection[0];
                var fieldProperty = fieldNamePropertyDirection[1];
                var direction = fieldNamePropertyDirection[2];
                var changesWorkflow = (fieldNamePropertyDirection[3] == 'true');
                var isWorkflowStep = (fieldNamePropertyDirection[4] == 'true');

                //Set the background color to the default (in case previously highlighted)
                $('#' + controlId).removeClass('validation-error');
                if (typeof (CKEDITOR) != 'undefined' && CKEDITOR)
                {
                    var editor = CKEDITOR.instances[controlId];
                    if (editor && editor.container)
                    {
                        $(editor.container.$).removeClass('validation-error');
                    }
                }

                //See if we can find the element and populate its value appropriately
                if (controlType == 'Label' || controlType == 'LabelEx' || controlType == 'RichTextLabel')
                {
                    //Labels are DOM elements so use $get
                    var domControl = $get(controlId);
                    if (domControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }
                            var dataField = dataItem.Fields[fieldName];
                            if (dataField != null && dataField[fieldProperty])
                            {
                                if (controlType == 'RichTextLabel')
                                {
                                    domControl.innerHTML = dataField[fieldProperty];
                                    globalFunctions.cleanHtml(domControl);
                                }
                                else
                                {
                                    //We have plain text
                                    globalFunctions.clearContent(domControl);
                                    domControl.appendChild(d.createTextNode(dataField[fieldProperty]));
                                }
                            }
                            else
                            {
                                domControl.innerHTML = '-';
                            }
                        }

                        //Handle hidden workflow state only
                        if (this.get_workflowEnabled())
                        {
                            domControl.style.display = (dataField.hidden) ? 'none' : '';
                            this.updateLabelState(labels, dataField);
                        }
                    }
                }
                if (controlType == 'StatusBox')
                {
                    //Status boxes are Ajax controls so use $find
                    var dataField = dataItem.Fields[fieldName];
                    var ajaxControl = $find(controlId);
                    if (ajaxControl && dataField)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //For status boxes, the properties are fixed
                            if (dataField.textValue) {
                                ajaxControl.set_text(dataField.textValue);
                                if (dataField.tooltip && dataField.tooltip != '') {
                                    ajaxControl.set_navigateUrl(dataField.tooltip);
                                }
                                ajaxControl.set_dataCssClass(dataField.cssClass);
                            }
                            else
                            {
                                ajaxControl.set_text('');
                                ajaxControl.set_dataCssClass('');
                            }
                            ajaxControl.display();
                        }
                    }
                }
                if (controlType == 'ImageEx' || controlType == 'Image')
                {
                    //Images are DOM elements so use $get
                    var dataField = dataItem.Fields[fieldName];
                    var domControl = $get(controlId);
                    if (domControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }
                            if (dataField == null || dataField[fieldProperty] == null)
                            {
                                domControl.src = '';
                            }
                            else
                            {
                                domControl.src = this._themeFolder + 'Images/' + dataField[fieldProperty];
                            }
                            if (dataField.tooltip)
                            {
                                //Also set the ALT tag if available, always use tooltip property
                                domControl.alt = dataField.tooltip;
                            }
                        }
                    }
                }
                if (controlType == 'ArtifactHyperLink')
                {
                    //Artifact hyperlinks are Ajax controls so use $find
                    var dataField = dataItem.Fields[fieldName];
                    var ajaxControl = $find(controlId);
                    if (ajaxControl && dataField)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //For artifact hyperlinks, the properties are fixed
                            ajaxControl.update_artifact(dataField.intValue, dataField.textValue, dataField.tooltip);
                        }
                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled())
                        {
                            ajaxControl.set_enabled(dataField.editable);
                            ajaxControl.set_required(dataField.required);
                            ajaxControl.set_hidden(dataField.hidden);
                            this.updateLabelState(labels, dataField);
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            //Register the change handler (self-disposing)
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            ajaxControl.add_changeClicked(changeHandler);
                        }
                    }
                }
                if (controlType == 'Equalizer')
                {
                    //Equalizers are Ajax controls so use $find
                    var dataField = dataItem.Fields[fieldName];
                    var ajaxControl = $find(controlId);
                    if (ajaxControl && dataField)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //For equalizers, the properties are fixed
                            ajaxControl.resetValues();
                            if (dataField.equalizerBlue)
                            {
                                ajaxControl.set_percentBlue(dataField.equalizerBlue);
                            }
                            if (dataField.equalizerGreen)
                            {
                                ajaxControl.set_percentGreen(dataField.equalizerGreen);
                            }
                            if (dataField.equalizerRed)
                            {
                                ajaxControl.set_percentRed(dataField.equalizerRed);
                            }
                            if (dataField.equalizerOrange)
                            {
                                ajaxControl.set_percentOrange(dataField.equalizerOrange);
                            }
                            if (dataField.equalizerYellow)
                            {
                                ajaxControl.set_percentYellow(dataField.equalizerYellow);
                            }
                            if (dataField.equalizerGray)
                            {
                                ajaxControl.set_percentGray(dataField.equalizerGray);
                            }
                            ajaxControl.set_toolTip(dataField.tooltip);
                            ajaxControl.display();
                        }
                    }
                }
                if (controlType == 'CheckBox' || controlType == 'CheckBoxEx')
                {
                    //Checkboxes are DOM elements so use $get
                    var dataField = dataItem.Fields[fieldName];
                    var domControl = $get(controlId);
                    if (domControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }
                            if (dataField == null || dataField[fieldProperty] == null)
                            {
                                domControl.checked = false;
                            }
                            else
                            {
                                var flagValue = dataField[fieldProperty];
                                domControl.checked = (flagValue == 'Y' || flagValue == 'True' || flagValue == 'true' || flagValue == '1');
                            }
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            $addHandler(domControl, 'click', changeHandler);
                            this._checkboxChangedHandlers.push(changeHandler);
                            this._checkboxReferences.push(domControl);
                        }

                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled())
                        {
                            domControl.disabled = !dataField.editable;
                            domControl.style.display = (dataField.hidden) ? 'none' : '';
                            this.updateLabelState(labels, dataField);
						}

						//Handle global read only state
						if (this.get_readOnly())
						{
							domControl.disabled = true;
							this.updateLabelState(labels, dataField);
						}
                    }
                }
                if (controlType == 'CheckBoxYnEx')
                {
                    //Checkbox Yes/No sliders are jQuery elements
                    var dataField = dataItem.Fields[fieldName];
                    var checkBoxYn = $('#' + controlId);
                    if (checkBoxYn)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }
                            var isChecked;
                            if (dataField == null || dataField[fieldProperty] == null)
                            {
                                isChecked = false;
                            }
                            else
                            {
                                var flagValue = dataField[fieldProperty];
                                isChecked = (flagValue == 'Y' || flagValue == 'True' || flagValue == 'true' || flagValue == '1');
                            }
                            checkBoxYn.bootstrapSwitch('state', isChecked);
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            checkBoxYn.on('switchChange.bootstrapSwitch', changeHandler);
                        }

                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled())
                        {
                            $(checkBoxYn).bootstrapSwitch('disabled', !dataField.editable);
                            if (dataField.hidden)
                            {
                                $(checkBoxYn).parent().parent().hide();
                            }
                            else
                            {
                                $(checkBoxYn).parent().parent().show();
                            }
                            this.updateLabelState(labels, dataField);
						}

						//Handle global read only state
						if (this.get_readOnly())
						{
							$(checkBoxYn).bootstrapSwitch('disabled', true);
							this.updateLabelState(labels, dataField);
						}
                    }
                }
                if (controlType == 'DropDownMultiList' || controlType == 'UnityDropDownMultiList')
                {
                    var dataField = dataItem.Fields[fieldName];
                    //Dropdowns are Ajax controls so use $find
                    var ajaxControl = $find(controlId);
                    if (ajaxControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            if (dataField != null)
                            {
                                if (dataField.textValue)
                                {
                                    ajaxControl.set_selectedItem(dataField.textValue);
                                }
                                else
                                {
                                    ajaxControl.set_selectedItem('');
                                }
                            }
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            //Register the change handler (self-disposing)
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            ajaxControl.add_selectedItemChanged(changeHandler);
                        }

                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled())
                        {
                            ajaxControl.set_enabled(dataField.editable);
                            ajaxControl.get_element().style.display = (dataField.hidden) ? 'none' : 'inline-block';
                            this.updateLabelState(labels, dataField);
						}

						//Handle global read only state
						if (this.get_readOnly())
						{
							ajaxControl.set_enabled(false);
							this.updateLabelState(labels, dataField);
						}
                    }
                }
                if (controlType == 'DropDownListEx' ||
                    controlType == 'DropDownHierarchy' ||
                    controlType == 'DropDownUserList' || 
                    controlType == 'UnityDropDownListEx' ||
                    controlType == 'UnityDropDownHierarchy' ||
                    controlType == 'UnityDropDownUserList')
                {
                    var dataField = dataItem.Fields[fieldName];

                    //Dropdowns are Ajax controls so use $find
                    var ajaxControl = $find(controlId);
                    if (ajaxControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            //Flags use the text property, so need to check if that's what they've specified
                            //otherwise we assume they're using the intValue property
                            if (fieldProperty == 'textValue')
                            {
                                if (dataField != null && dataField.textValue)
                                {
                                    ajaxControl.set_selectedItem(dataField.textValue);
                                }
                            }
                            else
                            {
                                if (dataField != null)
                                {
                                    if (globalFunctions.isNullOrUndefined(dataField.intValue))
                                    {
                                        ajaxControl.set_selectedItem('');
                                    }
                                    else
                                    {
                                        ajaxControl.set_selectedItem(dataField.intValue);
                                    }
                                }
                            }
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            //Register the change handler (self-disposing)
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            ajaxControl.add_selectedItemChanged(changeHandler);

                            //See if this field drives the choice of workflow
                            if (changesWorkflow && this._workflowChangeControl == null)
                            {
                                var workflowChangedHandler = Function.createDelegate(this, this.onWorkflowChanged);
                                this._workflowChangeControl = ajaxControl;
                                ajaxControl.add_selectedItemChanged(workflowChangedHandler);
                            }
                        }

                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled() && dataField)
                        {
                            ajaxControl.set_enabled(dataField.editable);
                            ajaxControl.set_hidden(dataField.hidden);
                            this.updateLabelState(labels, dataField);
						}

						//Handle global read only state
						if (this.get_readOnly())
						{
							ajaxControl.set_enabled(false);
							this.updateLabelState(labels, dataField);
						}
                    }
                }
                if (controlType == 'DateTimePicker' || controlType == 'UnityDateTimePicker')
                {
                    var dataField = dataItem.Fields[fieldName];
                    //datetimepickers are Ajax controls so use $find
                    var ajaxControl = $find(controlId);
                    if (ajaxControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            if (dataField != null)
                            {
                                if (dataField.dateValue)
                                {
                                    //For this control we need to use the entire JS date/time-value
                                    var dateObj = globalFunctions.parseJsonDate(dataField.dateValue);
                                    ajaxControl.set_datetime(dateObj);
                                }
                                else
                                {
                                    ajaxControl.set_datetime('');
                                }
                            }
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            //Register the change handler (self-disposing)
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            ajaxControl.add_dateOrTimeChanged(changeHandler);
                        }

                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled())
                        {
                            ajaxControl.set_enabled(dataField.editable);
                            ajaxControl.set_hidden(dataField.hidden);
                            this.updateLabelState(labels, dataField);
						}

						//Handle global read only state
						if (this.get_readOnly())
						{
							ajaxControl.set_enabled(false);
							this.updateLabelState(labels, dataField);
						}
                    }
                }
                if (controlType == 'DateControl' || controlType == 'UnityDateControl')
                {
                    var dataField = dataItem.Fields[fieldName];
                    //datepickers are Ajax controls so use $find
                    var ajaxControl = $find(controlId);
                    if (ajaxControl)
                    {
                        if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                        {
                            if (dataField != null)
                            {
                                if (dataField.textValue)
                                {
                                    ajaxControl.set_value(dataField.textValue);
                                }
                                else
                                {
                                    ajaxControl.set_value('');
                                }
                            }
                        }
                        if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                        {
                            //Register the change handler (self-disposing)
                            var changeHandler = Function.createDelegate(this, this.data_changed);
                            ajaxControl.add_dateChanged(changeHandler);
                        }

                        //Handle disabled/hidden workflow state
                        if (this.get_workflowEnabled())
                        {
                            ajaxControl.set_enabled(dataField.editable);
                            ajaxControl.get_element().style.display = (dataField.hidden) ? 'none' : '';
                            this.updateLabelState(labels, dataField);
						}

						//Handle global read only state
						if (this.get_readOnly())
						{
							ajaxControl.set_enabled(false);
							this.updateLabelState(labels, dataField);
						}
                    }
                }

                if (controlType == 'RichTextBoxJ')
                {
                    var dataField = dataItem.Fields[fieldName];
                    if (dataField != null)
                    {
                        //RTEJs are ckEditor instances
                        var editor = CKEDITOR.instances[controlId];
                        if (editor)
                        {
                            if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                            {
                                //Default to using the text value
                                if (fieldProperty == '')
                                {
                                    fieldProperty = 'textValue';
                                }

                                //Set the editor data using the API (two methods to make sure)
                                if (globalFunctions.isNullOrUndefined(dataField[fieldProperty]))
                                {
                                    editor.on('instanceReady', function (e) { e.editor.setData(''); });
                                    editor.setData('');
                                }
                                else
                                {
                                    editor.on('instanceReady', function (e) { e.editor.setData(e.listenerData); }, null, dataField[fieldProperty]);
                                    editor.setData(dataField[fieldProperty]);
                                }
                            }

                            if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                            {
                                //Add change handler when a key is pressed or paste
                                editor.on('key', Function.createDelegate(this, this.data_changed));
                                editor.on('paste', Function.createDelegate(this, this.data_changed));
                            }

                            //Handle disabled/hidden workflow state
                            if (this.get_workflowEnabled())
                            {
                                if (!dataField.editable)
                                {
                                    this._disableCKEDITOR(editor, 0);
                                    //editor.on('instanceReady', function (e) { e.editor.setReadOnly(true); });
                                }
                                if (dataField.hidden)
                                {
                                    this._hideCKEDITOR(editor, 0);
                                }
                                this.updateLabelState(labels, dataField);
							}

							//Handle global read only state
							if (this.get_readOnly())
							{
								this._disableCKEDITOR(editor, 0);
								this.updateLabelState(labels, dataField);
							}
                        }
                    }
                }

                if (controlType == 'TextBoxEx' || controlType == 'UnityTextBoxEx')
                {
                    //See if we have a rich text box
                    var dataField = dataItem.Fields[fieldName];
                    if (dataField != null)
                    {
                        //Plain Textboxes are DOM elements so use $get
                        var domControl = $get(controlId);
                        if (domControl)
                        {
                            if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut)
                            {
                                //If we have the special type of field - timespan, need to convert to minutes
                                if (dataField.fieldType == globalFunctions._fieldType_timeInterval && !globalFunctions.isNullOrUndefined(dataField.intValue))
                                {
                                    var hours = '' + Math.floor(dataField.intValue / 60);
                                    var fraction = '' + Math.floor((dataField.intValue % 60) / 60 * 100);
                                    if (fraction >= 10)
                                    {
                                        domControl.value = hours + '.' + fraction;
                                    }
                                    else
                                    {
                                        domControl.value = hours + '.0' + fraction;
                                    }
                                }
                                else
                                {
                                    //Default to using the text value
                                    if (fieldProperty == '')
                                    {
                                        fieldProperty = 'textValue';
                                    }

                                    //See if we have a value
                                    if (globalFunctions.isNullOrUndefined(dataField[fieldProperty]))
                                    {
                                        domControl.value = '';
                                    }
                                    else
                                    {
                                        domControl.value = dataField[fieldProperty];
                                    }
                                }
                            }

                            if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut))
                            {
                                //Plain text handlers are not self-disposing
                                var changeHandler = Function.createDelegate(this, this.data_changed);
                                $addHandler(domControl, 'keydown', changeHandler);
                                this._textboxChangedHandlers.push(changeHandler);
                                this._textboxReferences.push(domControl);
                            }

                            //Handle disabled/hidden workflow state
                            if (this.get_workflowEnabled())
                            {
                                domControl.readOnly = !dataField.editable;
                                if (dataField.editable)
                                {
                                    $(domControl).removeClass('disabled');
                                }
                                else
                                {
                                    $(domControl).addClass('disabled');
                                }
                                domControl.style.display = (dataField.hidden) ? 'none' : '';
                                this.updateLabelState(labels, dataField);
							}

							//Handle global read only state
							if (this.get_readOnly())
							{
								domControl.readOnly = true;
								$(domControl).addClass('disabled');
								this.updateLabelState(labels, dataField);
							}
                        }
                    }
				}

				if (controlType == 'DiagramEditor') {
					var dataField = dataItem.Fields[fieldName];
					if (dataField != null && typeof dataField != "undefined" && dataItem.Fields._MimeType != null) {
						var ajaxControl = $find(controlId);
						if (ajaxControl) {
							var mimeType = dataItem.Fields._MimeType.textValue;
							ajaxControl.setDiagramTypeFromMimeType(mimeType);
							//If the mimetype is one that supports diagrams proceed to render
							if (ajaxControl.get_diagramType()) {
								if (direction == this._formControlDirection_In || direction == this._formControlDirection_InOut) {
									//Set the diagram type

									//Default to using the text value
									if (fieldProperty == '') {
										fieldProperty = 'textValue';
									}

									//Set the editor data - if we have data
									if (ajaxControl.get_diagramType() && !globalFunctions.isNullOrUndefined(dataField[fieldProperty])) {
										var diagramData = dataField[fieldProperty];
										ajaxControl.set_data(diagramData);
										ajaxControl.loadData();
									}
								}

								//Add change handler for when the diagram is clicked on - as of v3.1 of the diagram library there are no change events to listen for
								if (context.registerHandlers && (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)) {
									var domControl = $get(controlId);
									domControl.addEventListener("click", Function.createDelegate(this, this.data_changed));
								}

								//Handle disabled/hidden workflow state
								if (this.get_workflowEnabled()) {
									ajaxControl.set_enabled(dataField.editable);
									ajaxControl.reRender();

									ajaxControl.get_element().style.display = (dataField.hidden) ? 'none' : '';

									this.updateLabelState(labels, dataField);
								}
							}
						}
					}
				}
            }

            //Now loop through the hyperlinks and populate
            for (var hyperLinkControlId in this._hyperLinkControls)
            {
                var dataFieldAndFormatString = this._hyperLinkControls[hyperLinkControlId].split(':');
                var fieldName = dataFieldAndFormatString[0];
                var formatString = dataFieldAndFormatString[1];
                //Hyperlinks are DOM elements so use $get
                var domControl = $get(hyperLinkControlId);
                if (domControl)
                {
                    //Get the datafield
                    var dataField = dataItem.Fields[fieldName];
                    if (dataField != null)
                    {
						domControl.innerHTML = globalFunctions.htmlEncode(dataField.textValue);
                        if (!globalFunctions.isNullOrUndefined(dataField.intValue) && dataField.intValue > -1)
                        {
                            domControl.href = formatString.replace('{0}', dataField.intValue);
                        }
                        else if (dataField.tooltip && dataField.tooltip != '')
                        {
                            domControl.href = dataField.tooltip;
                        }
                    }
                }
            }

            //No unsaved changes
            this.update_saveButtons(false);
        }
        else
        {
            this._dataItem = null;
        }
        globalFunctions.hide_spinner();

        //Refresh the operation list
        if (this._workflowOperationsControlId)
        {
            //See if we have one control or many
            if (this._workflowOperationsControlId.indexOf(',') == -1)
            {
                var workflowOperationsControl = $find(this._workflowOperationsControlId);
                if (workflowOperationsControl)
                {
                    workflowOperationsControl.load_data(context.dontClearMessages);
                }
            }
            else
            {
                var controlIds = this._workflowOperationsControlId.split(',');
                for (var i = 0; i < controlIds.length; i++)
                {
                    var workflowOperationsControl = $find(controlIds[i]);
                    if (workflowOperationsControl)
                    {
                        workflowOperationsControl.load_data(context.dontClearMessages);
                    }
                }
            }
        }

        //Raise loaded event
        this.raise_loaded(context.dontClearMessages);

        //Update ckeditors for compatibility with dark mode
        setTimeout(function () {
            if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.ckeditorSetColorScheme) {
                window.rct_comp_globalNav.ckeditorSetColorScheme(document.body.dataset.colorscheme);
            }
        }, 500);
    },

    _hideCKEDITOR: function (editor, retryCount)
    {
        if (editor && editor.container)
        {
            editor.container.$.style.display = 'none';
        }
        else if (retryCount < 10)
        {
            var thisRef = this;
            setTimeout(function () { thisRef._hideCKEDITOR(editor, retryCount + 1); }, 100);
        }
    },
    _disableCKEDITOR: function (editor, retryCount)
    {
        if (editor.editable() == undefined && retryCount < 10)
        {
            var thisRef = this;
            setTimeout(function () { thisRef._disableCKEDITOR(editor, retryCount + 1); }, 100);
        }
        else
        {
            try
            {
                editor.setReadOnly(true);
            }
            catch (ex)
            {
                if (retryCount < 10)
                {
                    setTimeout(function () { thisRef._disableCKEDITOR(editor, retryCount + 1); }, 100);
                }
            }
        }
    },

    _clearTextboxHandlers: function ()
    {
        for (var i = 0; i < this._textboxReferences.length; i++)
        {
            $removeHandler(this._textboxReferences[i], 'keydown', this._textboxChangedHandlers[i]);
            delete this._textboxReferences[i];
            delete this._textboxChangedHandlers[i];
        }
    },
    _clearCheckboxHandlers: function ()
    {
        for (var i = 0; i < this._checkboxReferences.length; i++)
        {
            $removeHandler(this._checkboxReferences[i], 'click', this._checkboxChangedHandlers[i]);
            delete this._checkboxReferences[i];
            delete this._checkboxChangedHandlers[i];
        }
    },
    _clearClickHandlers: function ()
    {
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._clickHandlers[i]);
            delete this._clickElements[i];
            delete this._clickHandlers[i];
        }
    },

    //Called by a page control to let us know that data has changed on the page
    data_changed: function ()
    {
        if (!this._unsavedChanges)
        {
            this.update_saveButtons(true);
        }
    },

    update_revert: function ()
    {
        if (this.get_revertButtonControlId())
        {
            var revertButton = $get(this.get_revertButtonControlId());
            if (revertButton)
            {
                revertButton.style.display = (!this._oldStatusId) ? 'none' : 'inline-block';
            }
        }
    },

    workflow_revert: function ()
    {
        //Revert the workflow
        var statusId = this._oldStatusId;
        this._dataItem.Fields[this.get_workflowStepField()].intValue = statusId;

        if (this.get_workflowStepLabel())
        {
            var stepLabel = $get(this.get_workflowStepLabel());
			stepLabel.innerHTML = globalFunctions.htmlEncode(this._oldStatusName);
        }
        this._newStatusName = this._oldStatusName;
        this._oldStatusId = null;
        this._oldStatusName = '';
        this.set_signatureRequired(false);
        this.update_revert();

        //Change the workflow operations control
        if (this._workflowOperationsControlId)
        {
            //See if we have one control or many
            if (this._workflowOperationsControlId.indexOf(',') == -1)
            {
                var workflowOperationsControl = $find(this._workflowOperationsControlId);
                if (workflowOperationsControl)
                {
                    workflowOperationsControl.revert_operation();
                }
            }
            else
            {
                var controlIds = this._workflowOperationsControlId.split(',');
                for (var i = 0; i < controlIds.length; i++)
                {
                    var workflowOperationsControl = $find(controlIds[i]);
                    if (workflowOperationsControl)
                    {
                        workflowOperationsControl.revert_operation();
                    }
                }
            }
        }

        var typeId = this._workflowChangeControl.get_selectedItem().get_value();
        this.load_workflow_states(typeId, statusId);

        //Raise the revert event
        this.raise_operationReverted(statusId, this._revertStatusOpen);
    },

    workflow_operation: function (statusId, statusName, isStatusOpen, isSignatureRequired)
    {
        //We need to change the status label and display the revert option
        if (this.get_workflowStepLabel())
        {
            var stepLabel = $get(this.get_workflowStepLabel());
			stepLabel.innerHTML = globalFunctions.htmlEncode(statusName);
        }
        this._newStatusName = statusName;

        //Set if signature is required
        if (isSignatureRequired)
        {
            this.set_signatureRequired(true);
        }

        //Store the old status and display the revert link
        this._oldStatusId = this._dataItem.Fields[this.get_workflowStepField()].intValue;
        this._oldStatusName = this._dataItem.Fields[this.get_workflowStepField()].textValue;
        this._revertStatusOpen = !isStatusOpen;
        this.update_revert();

        //Update the status of the data item
        if (this.get_workflowStepField())
        {
            this._dataItem.Fields[this.get_workflowStepField()].intValue = statusId;
        }

        //Mark the item as changed
        this.update_saveButtons(true);

        //Now update the various workflow field states
        var typeId = this._workflowChangeControl.get_selectedItem().get_value();
        this.load_workflow_states(typeId, statusId);
    },

    load_workflow_states: function (typeId, statusId)
    {
        //Make sure we have a valid type
        if (typeId && typeId > 0)
        {
            globalFunctions.display_spinner();
            var webService = this.get_webServiceClass();
            webService.Form_RetrieveWorkflowFieldStates(this._projectId, typeId, statusId, Function.createDelegate(this, this._load_workflow_states_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    _load_workflow_states_success: function (fields, context)
    {
        //See if we have some data returned
        if (fields != null && fields.length > 0)
        {
            //Update any page specific content not part of the form manager for after a status change
            if (typeof (updatePageStatusContent) != 'undefined') {
                updatePageStatusContent(fields);
            }

            //Update the workflow state of the various controls
            for (var i = 0; i < fields.length; i++)
            {
                var dataField = fields[i];
                //Now loop through the various controls and update the workflow state
                for (var controlReference in this._controlReferences)
                {
                    var controlTypeAndId = controlReference.split(':');
                    //Split up into type and id
                    var controlType = controlTypeAndId[0];
                    var controlId = controlTypeAndId[1];

                    //Get the field name
                    var fieldNamePropertyDirection = this._controlReferences[controlReference].split(':');
                    var fieldName = fieldNamePropertyDirection[0];
                    if (fieldName == dataField.fieldName)
                    {
                        //Get the associated label
                        var labels = this.findAssociatedLabel(controlId);

                        //Update the label state
                        this.updateLabelState(labels, dataField);

                        //Update the control state
                        var ajaxControl = $find(controlId);
                        var ckEditorInstance = null;
                        if (typeof(CKEDITOR) != 'undefined' && CKEDITOR)
                        {
                            ckEditorInstance = CKEDITOR.instances[controlId];
                        }
                        if (ajaxControl)
                        {
							var isDisabled = this.get_readOnly() ? true : !dataField.editable;
							if (ajaxControl.set_enabled)
                            {
								ajaxControl.set_enabled(!isDisabled);
                            }
                            else
                            {
                                if (ajaxControl.set_isReadOnly)
                                {
									ajaxControl.set_isReadOnly(isDisabled);
                                }
                                else
                                {
									ajaxControl.get_element().readOnly = isDisabled;
                                }
                            }
                            if (ajaxControl.set_hidden)
                            {
                                ajaxControl.set_hidden(dataField.hidden);
                            }
                            else
                            {
                                ajaxControl.get_element().style.display = (dataField.hidden) ? 'none' : '';
                            }
                        }
                        else if (ckEditorInstance)
                        {
                            //ckEditor RTEs
                            var editor = ckEditorInstance;
                            if (editor)
                            {
								var isDisabled = this.get_readOnly() ? true : !dataField.editable;
								editor.setReadOnly(isDisabled);
                                editor.container.$.style.display =(dataField.hidden) ? 'none': '';
                            }
                        }
                        else if (controlType == 'CheckBoxYnEx')
                        {
                            var checkBoxYn = $('#' + controlId);
                            if (checkBoxYn)
                            {
								var isDisabled = this.get_readOnly() ? true : !dataField.editable;
								$(checkBoxYn).bootstrapSwitch('disabled', isDisabled);
                                if (dataField.hidden)
                                {
                                    $(checkBoxYn).parent().parent().hide();
                                }
                                else
                                {
                                    $(checkBoxYn).parent().parent().show();
                                }
                            }
                        }
                        else
                        {
                            var domControl = $get(controlId);
                            if (domControl)
                            {
								var isDisabled = this.get_readOnly() ? true : !dataField.editable;
								//Some controls use disabled, some use readOnly
                                if (domControl.tagName == 'INPUT' && domControl.type == 'text')
                                {
									domControl.readOnly = isDisabled;
                                }
                                else
                                {
									domControl.disabled = isDisabled;
                                }
                                //Handle inline/block controls separately
                                if (domControl.tagName == 'INPUT')
                                {
                                    domControl.style.display = (dataField.hidden) ? 'none' : '';

                                    //If a textbox, need to also switch its CSS
                                    if (domControl.type == 'text')
                                    {
										if (!isDisabled)
                                        {
                                            $(domControl).removeClass('disabled');
                                        }
                                        else 
                                        {
                                            $(domControl).addClass('disabled');
                                        }
                                    }
                                }
                                else
                                {
                                    domControl.style.display = (dataField.hidden) ? 'none' : '';
                                }
                            }
                        }
                    }
                }
            }
        }

        //Hide the spinner
        globalFunctions.hide_spinner();
    },

    //Validates the data and populates the data-item without sending to the server
    //useful when we want to pass to a specialized function
    validate_and_update: function ()
    {
        //Fire validators on the page
        if (typeof (Page_ClientValidate) == 'function')
        {
            var isPageValid = Page_ClientValidate();
            if (!isPageValid)
            {
                //Validation failed, so return false
                return false;
            }
        }

        //Loop through the controls updating the stored data item
        this.update_dataItem(this._dataItem);
        return true;
    },
    //Used to allow an external page to just have the form manager simply display validation message
    //overrideMsgBox - pass to use a different error box
    display_validation_messages: function (messages, overrideMsgBox)
    {
        if (messages && messages.length > 0)
        {
            var message = '';
            for (var i = 0; i < messages.length; i++)
            {
                message += '<span class="fas fa-exclamation-triangle"></span> ' + messages[i].Message + '<br />';
                if (messages[i].FieldName)
                {
                    this.highlight_field(messages[i].FieldName);
                }
                errorFound = true;
            }
            if (errorFound)
            {
                this._suppressErrors = false;
                if (overrideMsgBox)
                {
                    globalFunctions.display_error_message(overrideMsgBox, message);
                }
                else
                {
                    this.display_error_message(message);
                }
            }
        }
    },

    //Clones the current item and redirects to the new item
    clone_item: function (evt)
    {
        //If we have unsaved, changes confirm with the user
        if (this._unsavedChanges)
        {
            if (!confirm(resx.AjaxFormManager_ConfirmRefresh))
            {
                return;
            }
        }
        globalFunctions.display_spinner();
        var webService = this.get_webServiceClass();
        webService.Form_Clone(this._projectId, this._primaryKey, Function.createDelegate(this, this._clone_item_success), Function.createDelegate(this, this.operation_failure));
    },
    _clone_item_success: function (newArtifactId)
    {
        globalFunctions.hide_spinner();

        if (newArtifactId)
        {
            //Redirect to the new item using live loading
            this.set_suppressErrors(true);
            this.set_primaryKey(newArtifactId, true);
            this.load_data();

            //Also need to rewrite the URL to match
            if (history && history.pushState)
            {
                //We set the URL of the page to match the item we're loading
                var href = urlTemplate_artifactRedirectUrl.replace(globalFunctions.artifactIdToken, '' + newArtifactId);
                history.pushState(newArtifactId, null, href);

                //TODO: Add code to handle browser forward/back buttons like in the navigation bar
            }
        }
        else
        {
            //This forces the page to redirect back to list page
            this.set_primaryKey(-1, true);
            this.load_data();
        }
    },

    //Deletes the current item and loads the next item or redirects to the list page
    delete_item: function(evt)
    {
        //If we have unsaved, changes confirm with the user
        if (this._unsavedChanges)
        {
            if (!confirm(resx.AjaxFormManager_ConfirmRefresh))
            {
                return;
            }
        }
        globalFunctions.display_spinner();
        var webService = this.get_webServiceClass();
        webService.Form_Delete(this._projectId, this._primaryKey, Function.createDelegate(this, this._delete_item_success), Function.createDelegate(this, this.operation_failure));
    },
    _delete_item_success: function(newArtifactId)
    {
        globalFunctions.hide_spinner();

        if (newArtifactId)
        {
            //Redirect to the new item using live loading
            this.set_suppressErrors(true);
            this.set_primaryKey(newArtifactId, true);
            this.load_data();

            //Also need to rewrite the URL to match
            if (history && history.pushState)
            {
                //We set the URL of the page to match the item we're loading
                var href = urlTemplate_artifactRedirectUrl.replace(globalFunctions.artifactIdToken, '' + newArtifactId);
                history.pushState(newArtifactId, null, href);

                //TODO: Add code to handle browser forward/back buttons like in the navigation bar
            }
        }
        else
        {
            //This forces the page to redirect back to list page
            this.set_primaryKey(-1, true);
            this._dataItem = null;
            this.load_data();
        }
    },

    //Creates a new record without saving
    create_item: function (evt)
    {
        //Prevent event bubbling
        if (evt) {
            if (evt.preventDefault) {
                evt.preventDefault();
            }
            else {
                //IE8 and older
                evt.returnValue = false;
            }
            if (evt.stopPropagation) {
                evt.stopPropagation();
            }
        }

        //If we have unsaved, changes confirm with the user
        if (this._unsavedChanges) {
            if (!confirm(resx.AjaxFormManager_ConfirmRefresh)) {
                //Data not changed
                return false;
            }
        }

        //Get the ID from the service (will be null for some artifact types)
        var webService = this.get_webServiceClass();
        webService.Form_New(this._projectId, this._primaryKey, Function.createDelegate(this, this._create_item_success), Function.createDelegate(this, this.operation_failure));
	},

	//Creates a new child record without saving (for hierarchical items, like requirements)
	create_childItem: function (evt) {
		//Prevent event bubbling
		if (evt) {
			if (evt.preventDefault) {
				evt.preventDefault();
			}
			else {
				//IE8 and older
				evt.returnValue = false;
			}
			if (evt.stopPropagation) {
				evt.stopPropagation();
			}
		}

		//If we have unsaved, changes confirm with the user
		if (this._unsavedChanges) {
			if (!confirm(resx.AjaxFormManager_ConfirmRefresh)) {
				//Data not changed
				return false;
			}
		}

		//Get the ID from the service (will be null for some artifact types)
		var webService = this.get_webServiceClass();
		webService.Form_InsertChild(this._projectId, this._primaryKey, Function.createDelegate(this, this._create_item_success), Function.createDelegate(this, this.operation_failure));
	},

    _create_item_success: function(newArtifactId)
    {
        //Without actually saving, raise the event as if it had been saved
        var operation = 'new';
        this.raise_dataSaved(operation, newArtifactId);
    },

    //Saves the form data after firing validators and checking that content has changed
    //Operation = Save, SaveAndNew, etc.
    //Force - pass true if we don't want to check that content has changed
    save_data: function (evt, operation, force)
    {
        //Prevent event bubbling
        if (evt)
        {
            if (evt.preventDefault)
            {
                evt.preventDefault();
            }
            else
            {
                //IE8 and older
                evt.returnValue = false;
            }
            if (evt.stopPropagation)
            {
                evt.stopPropagation();
            }
        }

        //Make sure we have unsaved changes and a populated data item
        if (!force && (!this._unsavedChanges || !this._dataItem))
        {
            return;
        }

        //Make the save buttons invalid to avoid 'double-saves'
        this.update_saveButtons(false);

        //Fire validators on the page
        if (typeof (Page_ClientValidate) == 'function')
        {
            var isPageValid = Page_ClientValidate();
            if (!isPageValid)
            {
                //Validation failed, so return false
                this.update_saveButtons(true);
                return false;
            }
        }

        //See if we need to capture a signature for this change
        if (this.get_signatureRequired())
        {
            this.displaySignatureDialog(operation);
        }
        else
        {
            this._save_data2(operation);
        }
    },
    _save_data2: function (operation)
    {
        //Loop through the controls updating the stored data item
        this.update_dataItem(this._dataItem);

        //See if we have a digital signature to send
        var signature = null;
        if (this._signatureRequired)
        {
            signature = {};
            signature.login = this._signatureLogin.value;
            signature.password = this._signaturePassword.value;
            signature.meaning = this._signatureMeaning.value;
        }

        //Now we need to update the item on the server
        var c = {};
        c.operation = operation;
        globalFunctions.display_spinner();
        var webService = this.get_webServiceClass();
        webService.Form_Save(this._projectId, this._dataItem, operation, signature, Function.createDelegate(this, this._save_data_success), Function.createDelegate(this, this._save_data_failure), c);
    },
    _save_data_success: function (messages, c)
    {
        //Hide the spinner
        globalFunctions.hide_spinner();

        //See if we have any validation messages
        this._suppressErrors = false;
        globalFunctions.set_suppressErrors(false);
        var newArtifactId = undefined;
        var errorFound = false;
        if (messages && messages.length > 0)
        {
            var message = '';
            for (var i = 0; i < messages.length; i++)
            {
                //See if we have the special reserved message for a new artifact id
                if (messages[i].FieldName == '$NewArtifactId')
                {
                    newArtifactId = messages[i].Message;
                }
                else
                {
                    message += '<span class="fas fa-exclamation-triangle"></span> ' + messages[i].Message + '<br />';
                    if (messages[i].FieldName)
                    {
                        this.highlight_field(messages[i].FieldName);
                    }
                    errorFound = true;
                }
            }
            if (errorFound)
            {
                this.display_error_message(message);
                this.update_saveButtons(true);
                //Raise the event, including the message
                this.raise_dataFailure(message);
                return;
            }
        }

        //Finally reload the data (in case other fields have changed) - not for new item case (no primary key)
        this.update_saveButtons(false);
        if (this._primaryKey && c.operation != 'new')
        {
            this.load_data(false, true);
        }

        //Display a confirmation message
        this.display_info(resx.AjaxFormManager_SaveSuccess.replace('{0}', this._artifactTypeName));

        //Raise the event, including the operation name
        this.raise_dataSaved(c.operation, newArtifactId);
    },
    _save_data_failure: function (exception)
    {
        //See if we have a permissions error
        this._suppressErrors = false;
        if (exception.get_message() == globalFunctions.authorizationMessage)
        {
            globalFunctions.hide_spinner();
            this.display_error_message(resx.AjaxFormManager_NotAuthorizedToModify);
        }
        else
        {
            this.update_saveButtons(true);
            this.operation_failure(exception);
        }

        //Raise the event, including the exception
        this.raise_dataFailure(exception);

    },

    displaySignatureDialog: function (operation)
    {
        //Display the modal e-signature dialog if it doesn't exist
        if (!this._signatureDialogBox)
        {
            this.createSignatureDialog();
        }

        //Populate/clear the fields and display
        this._signatureLogin.value = SpiraContext.CurrentUsername;
        this._signaturePassword.value = '';
        this._signatureMeaning.value = resx.AjaxFormManager_DefaultMeaning.replace('{0}', this.get_artifactTypeName()).replace('{1}', this.get_artifactTypePrefix()).replace('{2}', this.get_primaryKey()).replace('{3}', this._oldStatusName).replace('{4}', this._newStatusName);
        this._signatureDialogBox.display();
        this._operation = operation;
    },
    createSignatureDialog: function ()
    {
        var div = d.ce('div');
        div.className = 'PopupPanel w10';
        div.style.position = 'absolute';
        div.style.display = 'none';
        //div.style.height = '300px';
        this.get_element().parentNode.appendChild(div);
        var title = resx.AjaxFormManager_EnterSignature;
        this._signatureDialogBox = $create(Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel, { themeFolder: this.get_themeFolder(), errorMessageControlId: this.get_errorMessageControlId(), title: title, persistent: true, modal: true, top: 100, left: 200 }, null, null, div);
        var p = d.ce('p');
        div.appendChild(p);
        p.appendChild(d.createTextNode(resx.AjaxFormManager_EnterSignatureInfo));
        var table = d.ce('table');
        table.className = 'DataEntryForm';
        div.appendChild(table);
        var tbody = d.ce('tbody');
        table.appendChild(tbody);
        var tr = d.ce('tr');
        tbody.appendChild(tr);
        var td = d.ce('td');
        tr.appendChild(td);
        td.className = 'DataLabel';
        var label = d.ce('label');
        td.appendChild(label);
        label.className = 'required';
        label.appendChild(d.createTextNode(resx.AjaxFormManager_Login + ':'));
        td = d.ce('td');
        td.className = 'DataEntry';
        tr.appendChild(td);
        this._signatureLogin = d.ce('input');
        this._signatureLogin.id = this.get_element().id + '_signatureLogin';
        this._signatureLogin.type = 'text';
        this._signatureLogin.maxLength = 50;
        this._signatureLogin.className = 'u-input disabled';
        this._signatureLogin.style.width = '100%';
        this._signatureLogin.disabled = true;
        td.appendChild(this._signatureLogin);
        label.htmlFor = this._signatureLogin.id;
        tr = d.ce('tr');
        tbody.appendChild(tr);
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'DataLabel';
        label = d.ce('label');
        td.appendChild(label);
        label.className = 'required';
        label.appendChild(d.createTextNode(resx.AjaxFormManager_Password + ':'));
        td = d.ce('td');
        td.className = 'DataEntry';
        tr.appendChild(td);
        this._signaturePassword = d.ce('input');
        this._signaturePassword.id = this.get_element().id + '_signaturePassword';
        this._signaturePassword.type = 'password';
        this._signaturePassword.maxLength = 128;
        this._signaturePassword.className = 'u-input is-active';
        this._signaturePassword.style.width = '100%';
        this._signaturePassword.autocomplete = 'off';
        td.appendChild(this._signaturePassword);
        label.htmlFor = this._signaturePassword.id;
        tr = d.ce('tr');
        tbody.appendChild(tr);
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'DataLabel';
        label = d.ce('label');
        td.appendChild(label);
        label.className = 'required';
        label.appendChild(d.createTextNode(resx.AjaxFormManager_Meaning + ':'));
        td = d.ce('td');
        td.className = 'DataEntry';
        tr.appendChild(td);
        this._signatureMeaning = d.ce('textarea');
        this._signatureMeaning.id = this.get_element().id + '_signatureMeaning';
        this._signatureMeaning.maxLength = 255;
        this._signatureMeaning.className = 'text-box';
        this._signatureMeaning.style.width = '100%';
        this._signatureMeaning.style.height = '80px';
        td.appendChild(this._signatureMeaning);
        label.htmlFor = this._signatureMeaning.id;
        tr = d.ce('tr');
        tbody.appendChild(tr);
        td = d.ce('td');
        tr.appendChild(td);
        td = d.ce('td');
        tr.appendChild(td);


        // buttons td
        td.className = "btn-group";

        //Sign button        
        td.appendChild(d.createTextNode(' '));
        btn = d.ce('button');
        btn.type = 'button';
        btn.className = 'btn btn-primary';
        btn.appendChild(d.createTextNode(resx.AjaxFormManager_Sign));
        td.appendChild(btn); clickHandler = Function.createDelegate(this, this.signChange);
        $addHandler(btn, 'click', clickHandler);
        this._clickHandlers.push(clickHandler);
        this._clickElements.push(btn);

        //Cancel button
        var btn = d.ce('button');
        btn.type = 'button';
        btn.className = 'btn btn-default';
        btn.appendChild(d.createTextNode(resx.Global_Cancel));
        td.appendChild(btn);
        var clickHandler = Function.createDelegate(this, this.hideSignatureDialog);
        $addHandler(btn, 'click', clickHandler);
        this._clickHandlers.push(clickHandler);
        this._clickElements.push(btn);
    },

    signChange: function (evt)
    {
        //Validate the fields
        if (!this._signatureLogin || !this._signatureLogin.value || globalFunctions.trim(this._signatureLogin.value).length < 1)
        {
            alert(resx.AjaxFormManager_LoginRequired);
            return;
        }
        if (!this._signaturePassword || !this._signaturePassword.value || globalFunctions.trim(this._signaturePassword.value).length < 1)
        {
            alert(resx.AjaxFormManager_PasswordRequired);
            return;
        }
        if (!this._signatureMeaning || !this._signatureMeaning.value || globalFunctions.trim(this._signatureMeaning.value).length < 1)
        {
            alert(resx.AjaxFormManager_MeaningRequired);
            return;
        }

        //Save the data
        this._save_data2(this._operation);

        //Close the dialog
        this._signatureDialogBox.close();
    },
    hideSignatureDialog: function (evt) {
        //Close the dialog
        this._signatureDialogBox.close();
        
        //now reset the save button to active, so save can be clicked again
        this.update_saveButtons(true);
    },

    highlight_field: function (fieldNameToHighlight)
    {
        //Now loop through the various controls and highlight the field
        for (var controlReference in this._controlReferences)
        {
            var controlTypeAndId = controlReference.split(':');
            //Split up into type and id
            var controlType = controlTypeAndId[0];
            var controlId = controlTypeAndId[1];

            //Get the field name
            var fieldNamePropertyDirection = this._controlReferences[controlReference].split(':');
            var fieldName = fieldNamePropertyDirection[0];

            if (fieldName == fieldNameToHighlight)
            {
                $('#' + controlId).addClass('validation-error');
                //RTEs need extra code
                if (typeof (CKEDITOR) != 'undefined' && CKEDITOR)
                {
                    var editor = CKEDITOR.instances[controlId];
                    if (editor)
                    {
                        $(editor.container.$).addClass('validation-error');
                    }
                }
            }
        }
    },

    update_dataItem: function (dataItem)
    {
        //Now loop through the various controls and update the data item
        for (var controlReference in this._controlReferences)
        {
            var controlTypeAndId = controlReference.split(':');
            //Split up into type and id
            var controlType = controlTypeAndId[0];
            var controlId = controlTypeAndId[1];

            var fieldNamePropertyDirection = this._controlReferences[controlReference].split(':');
            //Split up into name, property and direction
            var fieldName = fieldNamePropertyDirection[0];
            var fieldProperty = fieldNamePropertyDirection[1];
            var direction = fieldNamePropertyDirection[2];

            if (controlType == 'HiddenField')
            {
                //HiddenFields are DOM controls so use $get
                var domControl = $get(controlId);
                if (domControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }

                            //If the field property is the 'intValue' need to handle the special case of a null integer
                            if (fieldProperty == 'intValue') {
                                if (domControl.value == '') {
                                    dataField.intValue = null;
                                    dataField.textValue = '';
                                }
                                else if (globalFunctions.isInteger(domControl.value)) {
                                    dataField.intValue = domControl.value;
                                }
                                else {
                                    dataField.intValue = null;
                                    dataField.textValue = '';
                                }
                            }
                            else {
                                dataField[fieldProperty] = domControl.value;
                            }                        }
                    }
                }

            }
            if (controlType == 'DropDownListEx' ||
                controlType == 'DropDownHierarchy' ||
                controlType == 'DropDownUserList' || 
                controlType == 'UnityDropDownListEx' ||
                controlType == 'UnityDropDownHierarchy' ||
                controlType == 'UnityDropDownUserList')
            {
                //Dropdowns are Ajax controls so use $find
                var ajaxControl = $find(controlId);
                if (ajaxControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            var item = ajaxControl.get_selectedItem();
                            //Unless we have a specified property name (e.g. Y/N flags), we always use the intValue for dropdowns
                            if (fieldProperty == 'textValue')
                            {
                                dataField.textValue = item.get_value();
                            }
                            else
                            {
                                if (item && item.get_value() && item.get_value() != '')
                                {
                                    dataField.intValue = item.get_value();
                                }
                                else
                                {
                                    dataField.intValue = null;
                                }
                            }
                        }
                    }
                }
            }
            if (controlType == 'DateTimePicker' || controlType == 'UnityDateTimePicker')
            {
                //Date/Time pickers are Ajax controls so use $find
                var ajaxControl = $find(controlId);
                if (ajaxControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            var moment = ajaxControl.get_datetime();
                            dataField.dateValue = globalFunctions.createJsonDateFromMoment(moment);
                            dataField.textValue = null;
                        }
                    }
                }
            }
            if (controlType == 'DateControl' || controlType == 'UnityDateControl')
            {
                //Date pickers are Ajax controls so use $find
                var ajaxControl = $find(controlId);
                if (ajaxControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            var dateText = ajaxControl.get_value();
                            if (dateText && dataField.textValue != dateText)
                            {
                                //We also set the datetime value to something else to tell the service that the value actually changed
                                dataField.dateValue = new Date();
                            }
                            dataField.textValue = dateText;
                        }
                    }
                }
            }
            if (controlType == 'DropDownMultiList' || controlType == 'UnityDropDownMultiList')
            {
                //Dropdowns are Ajax controls so use $find
                var ajaxControl = $find(controlId);
                if (ajaxControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            var itemList = ajaxControl.get_valueElement().value;
                            dataField.textValue = itemList;
                        }
                    }
                }
            }
            if (controlType == 'ArtifactHyperLink')
            {
                //Artifact hyperlinks are Ajax controls so use $find
                var ajaxControl = $find(controlId);
                if (ajaxControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            var artifactId = ajaxControl.get_artifactId();
                            if (artifactId && artifactId != '')
                            {
                                dataField.intValue = artifactId;
                            }
                            else
                            {
                                dataField.intValue = null;
                            }
                        }
                    }
                }
            }
            if (controlType == 'CheckBox' || controlType == 'CheckBoxEx' || controlType == 'CheckBoxYnEx')
            {
                //Checkboxes are DOM controls so use $get
                var domControl = $get(controlId);
                if (domControl)
                {
                    if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                    {
                        var dataField = dataItem.Fields[fieldName];
                        if (dataField != null)
                        {
                            var flagValue = (domControl.checked) ? 'Y' : 'N';
                            dataField.textValue = flagValue;
                        }
                    }
                }
            }
            if (controlType == 'RichTextBox')
            {
                var dataField = dataItem.Fields[fieldName];
                if (dataField != null)
                {
                    //RTEs are Ajax controls so use $find
                    var ajaxControl = $find(controlId);
                    if (ajaxControl)
                    {
                        if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }

                            //Force the RTE to update
                            ajaxControl.updateRTE();
                            dataField[fieldProperty] = ajaxControl.get_element().value;
                        }
                    }
                }
            }
            if (controlType == 'RichTextBoxJ')
            {
                var dataField = dataItem.Fields[fieldName];
                if (dataField != null)
                {
                    //RTEJs are ckEditor instances
                    var editor = CKEDITOR.instances[controlId];
                    if (editor)
                    {
                        if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                        {
                            //Default to using the text value
                            if (fieldProperty == '')
                            {
                                fieldProperty = 'textValue';
                            }
                            dataField[fieldProperty] = editor.getData();
                        }
                    }
                }
            }
            if (controlType == 'TextBoxEx' || controlType == 'UnityTextBoxEx')
            {
                var dataField = dataItem.Fields[fieldName];
                if (dataField != null)
                {
                    //Plain Textboxes are DOM elements so use $get
                    var domControl = $get(controlId);
                    if (domControl)
                    {
                        if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut)
                        {
                            //If we have the special type of field - timespan, need to convert to minutes
                            if (dataField.fieldType == globalFunctions._fieldType_timeInterval)
                            {
                                dataField.intValue = Math.round(parseFloat(domControl.value) * 60.0);
                                if (isNaN(dataField.intValue))
                                {
                                    dataField.intValue = null;
                                }
                            }
                            else
                            {
                                //Default to using the text value
                                if (fieldProperty == '')
                                {
                                    fieldProperty = 'textValue';
                                }

                                //If the field property is the 'intValue' need to handle the special case of a null integer
                                if (fieldProperty == 'intValue')
                                {
                                    if (domControl.value == '')
                                    {
                                        dataField.intValue = null;
                                        dataField.textValue = '';
                                    }
                                    else if (globalFunctions.isInteger(domControl.value))
                                    {
                                        dataField.intValue = domControl.value;
                                    }
                                    else
                                    {
                                        dataField.intValue = null;
                                        dataField.textValue = '';
                                    }
                                }
                                else
                                {
                                    dataField[fieldProperty] = domControl.value;
                                }
                            }
                        }
                    }
                }
			}
			if (controlType == 'DiagramEditor') {
				//Only continue if we have a data field
				var dataField = dataItem.Fields[fieldName];
				if (dataField !== null) {
					//Only continue if we can find the control
					var ajaxControl = $find(controlId);
					if (ajaxControl && ajaxControl.get_diagramType()) {
						//Retrieve the latest diagram data from the editor engine
						if (direction == this._formControlDirection_Out || direction == this._formControlDirection_InOut) {
							//Default to using the text value
							if (fieldProperty == '') {
								fieldProperty = 'textValue';
							}
							dataField[fieldProperty] = JSON.stringify(ajaxControl._editor.serialize());
						}
					}
				}
			}
        }
    }
};

Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager.registerClass ('Inflectra.SpiraTest.Web.ServerControls.AjaxFormManager', Sys.UI.Control);


if (typeof(Sys) != 'undefined')
{
    Sys.Application.notifyScriptLoaded();
}

// attaches a handler that is fired first after web service is returned, before specific handler is called   
// specifically, if the web service errored because of authentication failure (auth cookie expired)   
// then redirect the page to the login page.  
Sys.Net.WebRequestManager.add_completedRequest(On_WebRequestCompleted);

function On_WebRequestCompleted(sender, eventArgs)
{
    if (sender.get_statusCode() === 500)
    {
        if (sender.get_object().Message === "Authentication failed.")
        {
            window.location.href = window.location.protocol + "//" + window.location.host + window.location.pathname.substring(0, window.location.pathname.indexOf('/', 1)) + '/Login.aspx?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
        }
    }
}  
