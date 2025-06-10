var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations.initializeBase(this, [element]);

    this._webServiceClass = null;
    this._errorMessageControlId = '';
    this._dataSource = null;
    this._loadingComplete = false;
    this._primaryKey = -1;
    this._formManagerId = '';
    this._projectId = -1;
    this._autoLoad = true;
    this._verticalGroup = true;
    this._horizontalGroup = false;
    this._singleButton = false;

    this._buttonGroup = null;
    this._dropdownButton = null;
    this._dropdownUl = null;
    this._noOperationsDiv = null;
    this._saveChangesDiv = null;

    this._links = new Array();
    this._linkMouseOverHandlers = new Array();
    this._linkMouseOutHandlers = new Array();
    this._linkClickHandlers = new Array();
}
Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations.callBaseMethod(this, 'initialize');

        //Create the shell of the control
        this._createChildControls();

        //Let other controls know we've initialized
        this.raise_init();

        //Load the transitions
        if (this._autoLoad)
        {
            this.load_data();
        }
        else
        {
            //Hide the divs
            this._noOperationsDiv.style.display = 'none';
            this._saveChangesDiv.style.display = 'none';
        }
    },
    dispose: function ()
    {
        this._clearLinkHandlers();
        delete this._buttonGroup;
        delete this._noOperationsDiv;
        delete this._saveChangesDiv;

        Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function ()
    {
        return this._element;
    },
    get_loadingComplete: function ()
    {
        return this._loadingComplete;
    },
    clear_loadingComplete: function ()
    {
        this._loadingComplete = false;
    },

    get_errorMessageControlId: function ()
    {
        return this._errorMessageControlId;
    },
    set_errorMessageControlId: function (value)
    {
        this._errorMessageControlId = value;
    },

    get_webServiceClass: function ()
    {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value)
    {
        this._webServiceClass = value;
    },

    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        this._dataSource = value;
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
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },

    get_formManagerId: function ()
    {
        return this._formManagerId;
    },
    set_formManagerId: function (value)
    {
        this._formManagerId = value;
    },

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },

    get_verticalGroup: function () {
        return this._verticalGroup;
    },
    set_verticalGroup: function (value) {
        this._verticalGroup = value;
    },

    get_horizontalGroup: function () {
        return this._horizontalGroup;
    },
    set_horizontalGroup: function (value) {
        this._horizontalGroup = value;
    },

    /*  =========================================================
    The event handler managers
    =========================================================  */
    add_loaded: function (handler)
    {
        this.get_events().addHandler('loaded', handler);
    },
    remove_loaded: function (handler)
    {
        this.get_events().removeHandler('loaded', handler);
    },
    raise_loaded: function ()
    {
        var h = this.get_events().getHandler('loaded');
        if (h) h();
    },
    add_init: function (handler)
    {
        this.get_events().addHandler('init', handler);
    },
    remove_init: function (handler)
    {
        this.get_events().removeHandler('init', handler);
    },
    raise_init: function ()
    {
        var h = this.get_events().getHandler('init');
        if (h) h();
    },

    /*  =========================================================
    Event Handlers
    =========================================================  */
    _onLinkMouseOver: function (sender, e)
    {
        //Display the tooltip
        var tooltip = e.operation + '<br /> &gt;&gt;&gt; ' + e.status;
        ddrivetip(tooltip);
    },
    _onLinkMouseOut: function (sender, e)
    {
        hideddrivetip();
    },
    _onLinkClick: function (sender, e)
    {
        if (e.thisRef.get_formManagerId())
        {
            var formManagerControl = $find(e.thisRef.get_formManagerId());
            if (formManagerControl)
            {
                //Call the form manager
                formManagerControl.workflow_operation(e.statusId, e.statusName, e.isOutputStatusOpen, e.isSignatureRequired);

                //Display the 'save changes' legend
                e.thisRef._saveChangesDiv.style.display = '';
                e.thisRef._buttonGroup.style.display = 'none';
            }
        }

        //Raise an event
        e.thisRef.raise_operationExecuted(e.transitionId, e.isOutputStatusOpen);
    },

    /*  =========================================================
    Event handlers managers
    =========================================================  */
    add_operationExecuted: function (handler)
    {
        this.get_events().addHandler('operationExecuted', handler);
    },
    remove_operationExecuted: function (handler)
    {
        this.get_events().removeHandler('operationExecuted', handler);
    },
    raise_operationExecuted: function (transitionId, isOutputStatusOpen)
    {
        var h = this.get_events().getHandler('operationExecuted');
        if (h) h(transitionId, isOutputStatusOpen);
    },

    /*  =========================================================
    Public methods/functions
    =========================================================  */
    _createChildControls: function ()
    {
        //Create the button group (either as vertical button group or in a div wrapper for a button dropdown)
        this._buttonGroup = d.ce('div');
        this.get_element().appendChild(this._buttonGroup);

        //work out which of the 3 display modes to use - default is vertical. If horizontal group included, this overrides and vertical group setting, if vertical is set to false, display is as dropdown group
        if (this._horizontalGroup)
        {
            $(this.get_element()).addClass('WorkflowOperations-horizontal dib')
            this._buttonGroup.className = 'btn-group dib';
            this._buttonGroup.setAttribute('role', 'group');
        }
        else if (this._verticalGroup)
        {
            $(this.get_element()).addClass('WorkflowOperations-vertical pa3')
            this._buttonGroup.className = 'btn-group-vertical db';
            this._buttonGroup.setAttribute('role', 'group');
        }
        else
        {
            $(this.get_element()).addClass('dib')
            this._buttonGroup.className = 'WorkflowOperations-dropdown btn-group priority1';
            this._dropdownButton = d.ce('button');
            this._dropdownButton.className = 'btn btn-default dropdown-toggle';
            this._dropdownButton.setAttribute('data-toggle', 'dropdown');
            this._dropdownButton.setAttribute('aria-haspopup', 'true');
            this._dropdownButton.setAttribute('aria-expanded', 'false');
            this._dropdownButton.appendChild(d.createTextNode(resx.Global_Operations));
            var caretSpan = d.ce('span');
            caretSpan.className = 'caret';
            this._dropdownButton.appendChild(caretSpan);
            this._buttonGroup.appendChild(this._dropdownButton);

            this._dropdownUl = d.ce('ul');
            this._dropdownUl.className = 'dropdown-menu';
            this._buttonGroup.appendChild(this._dropdownUl);
        }

        //Create the div for no operations
        this._noOperationsDiv = d.ce('div');
        this._noOperationsDiv.appendChild(d.createTextNode(resx.WorkflowOperations_NoOperationsAvailable));
        
        if (this._verticalGroup || this._horizontalGroup)
        {
            this.get_element().appendChild(this._noOperationsDiv);
        }
        else
        {
            this._dropdownButton.appendChild(this._noOperationsDiv);
        }

        //Create the div for 'save changes first'
        this._saveChangesDiv = d.ce('div');
        this.get_element().appendChild(this._saveChangesDiv);
        this._saveChangesDiv.appendChild(d.createTextNode(resx.WorkflowOperations_SaveChangesFirst));
    },

    clear_operations: function ()
    {
        //Hide the divs
        this._noOperationsDiv.style.display = 'none';
        this._saveChangesDiv.style.display = 'none';

        //Clear the divs and hide
        this._buttonGroup.style.display = 'none';
        var element = (this._verticalGroup || this._horizontalGroup) ? this._buttonGroup : this._dropdownUl;
        if (element.firstChild)
        {
            while (element.firstChild)
            {
                element.removeChild(element.firstChild);
            }
        }

        //Clear any existing handlers
        this._clearLinkHandlers();
        this._links = new Array();
        this._linkMouseOverHandlers = new Array();
        this._linkMouseOutHandlers = new Array();
        this._linkClickHandlers = new Array();
    },

    load_data: function (dontClearMessages)
    {
        //Clear any existing error messages
        if (!dontClearMessages)
        {
            globalFunctions.clear_errors($get(this.get_errorMessageControlId()));
        }

        //Clear any existing content
        this.clear_operations();

        //Get the list of transitions (check for artifact id)
        var artifactId = this.get_primaryKey();
        if (artifactId && artifactId > 0)
        {
            var webService = this.get_webServiceClass();
            globalFunctions.display_spinner();
            webService.WorkflowOperations_Retrieve(this.get_projectId(), artifactId, null, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
        }
    },

    retrieve_success: function (dataSource)
    {
        //Set the datasource
        this.set_dataSource(dataSource);
        //Databind
        this.dataBind();

        //Mark as complete
        this._loadingComplete = true;
        globalFunctions.hide_spinner();

        //Raise the loaded event
        this.raise_loaded();

    },
    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        globalFunctions.hide_spinner();
        //Display validation exceptions in a friendly manner
        var messageBox = document.getElementById(this.get_errorMessageControlId());
        globalFunctions.display_error(messageBox, exception);
    },

    dataBind: function ()
    {
        var dataSource = this.get_dataSource();
        var buttonGroup = this._buttonGroup;
        if (dataSource && dataSource.length > 0)
        {
            for (var i = 0; i < dataSource.length; i++)
            {
                var dataItem = dataSource[i];
                var operationName = dataItem.Fields.Name.textValue;
                var outputStatusName = dataItem.Fields.OutputStatusId.textValue;
                var isOutputStatusOpen = (dataItem.Fields.OutputStatusOpenYn.textValue == 'Y');
                var isSignatureRequired = (dataItem.Fields.SignatureYn && dataItem.Fields.SignatureYn.textValue == 'Y');

                //Create the hyperlink
                var a = d.ce('a');
                a.appendChild(d.createTextNode(operationName));
                a.href = 'javascript:void(0)';

                //Add the link to the relevant elements (if as a vertical group or  a dropdown
                if (this._verticalGroup)
                {
                    a.className = 'btn btn-default tl ws-normal';
                    buttonGroup.appendChild(a);
                }
                else if (this._horizontalGroup) {
                    a.className = 'btn btn-default';
                    buttonGroup.appendChild(a);
                }
                else
                {
                    var li = d.ce('li');
                    this._dropdownUl.appendChild(li);
                    li.appendChild(a);
                }

                //Add the padlock for signed operations
                if (isSignatureRequired)
                {
                    a.appendChild(d.createTextNode(' '));
                    var span = d.ce('span');
                    span.className = 'fas fa-lock';
                    a.appendChild(span);
                }
                    
                //Add the click and tooltip handlers
                var mouseOverHandler = Function.createCallback(this._onLinkMouseOver, { thisRef: this, operation: operationName, status: outputStatusName });
                var mouseOutHandler = Function.createCallback(this._onLinkMouseOut, { thisRef: this });
                var clickHandler = Function.createCallback(this._onLinkClick, { thisRef: this, transitionId: dataItem.primaryKey, transitionName: operationName, statusId: dataItem.Fields.OutputStatusId.intValue, statusName: outputStatusName, isOutputStatusOpen: isOutputStatusOpen, isSignatureRequired: isSignatureRequired });
                $addHandler(a, 'mouseover', mouseOverHandler);
                $addHandler(a, 'mouseout', mouseOutHandler);
                $addHandler(a, 'click', clickHandler);
                this._links.push(a);
                this._linkMouseOverHandlers.push(mouseOverHandler);
                this._linkMouseOutHandlers.push(mouseOutHandler);
                this._linkClickHandlers.push(clickHandler);
            }

            //Show the table
            this._buttonGroup.style.display = '';
        }
        else
        {
            this._noOperationsDiv.style.display = '';
        }
    },

    revert_operation: function ()
    {
        //Hide the 'save changes' legend
        this._saveChangesDiv.style.display = 'none';
        this._buttonGroup.style.display = '';
    },

    _clearLinkHandlers: function ()
    {
        for (var i = 0; i < this._links.length; i++)
        {
            $removeHandler(this._links[i], 'mouseover', this._linkMouseOverHandlers[i]);
            $removeHandler(this._links[i], 'mouseout', this._linkMouseOutHandlers[i]);
            $removeHandler(this._links[i], 'click', this._linkClickHandlers[i]);
            delete this._linkMouseOverHandlers[i];
            delete this._linkMouseOutHandlers[i];
            delete this._linkClickHandlers[i];
        }

        delete _links;
        delete this._linkMouseOverHandlers;
        delete this._linkMouseOutHandlers;
        delete this._linkClickHandlers;
    }
}
Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations.registerClass('Inflectra.SpiraTest.Web.ServerControls.WorkflowOperations', Sys.UI.Control);
        
//  always end with this goodnight statement        
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

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
