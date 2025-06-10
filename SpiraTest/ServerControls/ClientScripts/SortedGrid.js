var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;
d.ctn = d.createTextNode;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.SortedGridColumn = function ()
{
    this.$header = null; //Start with $ prefix so that it does not includes in the Serialization used by the JavaScriptSerializer.
}
Inflectra.SpiraTest.Web.ServerControls.SortedGridColumn.registerClass('Inflectra.SpiraTest.Web.ServerControls.SortedGridColumn');

Inflectra.SpiraTest.Web.ServerControls.SortedGrid = function (element)
{
    this._insertArtifactId = -1;
    this._allowEdit = false;
    this._projectId = -1;
    this._webServiceClass = "";
    this._cssClass = '';
    this._baseUrl = '';
    this._baseUrlTarget = '';
    this._dateFormat = '';
    this._artifactPrefix = '';
    this._errorMessageControlId = '';
    this._totalCountControlId = '';
    this._visibleCountControlId = '';
    this._filterInfoControlId = '';
    this._appPath = '';
    this._holdActive = false;
	this._holdStarter = null;
	this._touchDragging = false;
	this._displayTypeId = null;

    this._headerCssClass = '';
    this._subHeaderCssClass = '';
    this._rowCssClass = '';
    this._selectedRowCssClass = '';
    this._editRowCssClass = '';
    this._themeFolder = '';

    this._itemImage = '';
    this._alternateItemImage = '';
    this._folderItemImage = '';
    this._emptyDataText = 'No records found.';

    this._columns = null;
    this._dataSource = null;
    this._loadingComplete = false;

    this._headerRow = null;
    this._subHeaderRow = null;
    this._tableRows = new Array();
    this._tableEditRows = new Array();
    this._selectedIndex = -1;
    this._editColumnHeader;
    this._ddlPaginationOptions = null;
    this._concurrencyValues = null;
    this._contextMenuItems = null;
    this._contextMenu = null;
    this._clipboard_type = '';
	this._clipboard_items = null;

    this._filters = new Array();
    this._edits = new Array();
    this._tooltips = new Array();
    this._buttonClickHandlers = new Array();
    this._headerDragHandlers = new Array();
    this._dragDownHandlers = new Array();
    this._dragUpHandlers = new Array();
    this._touchHandlers = new Array();
    this._tooltipOverHandlers = new Array();
    this._tooltipOutHandlers = new Array();
    this._otherHandlers = new Array();
    this._otherClickHandlers = new Array();
    this._keyDownElements = new Array();
    this._keyDownHandlers = new Array();
    this._tableRowDblClickHandlers = new Array();
    this._tableRowKeyDownHandlers = new Array();
    this._tableRowContextMenuHandlers = new Array();

    this._isInDrag = false;
    this._isInEdit = false;
    this._isOverNameDesc = false;
    this._posY = 0;
    this._posX = 0;
    this._draggingTable = null;
    this._draggingColumnIndex = -1;
    this._draggingTarget = null;
    this._headers = new Array();
    this._buttons = new Array();
	this._dragElements = new Array();
    this._clickElements = new Array();
    this._displayAttachments = true;
    this._displayCheckboxes = true;
    this._displayTooltip = true;
    this._negativePrimaryKeysDisabled = true;
    this._autoLoad = true;
    this._allowDragging = false;
    this._allowColumnPositioning = false;
    this._standardFilters = null;
    this._concurrencyEnabled = false;
    this._lastEditWasInsert = false;
    this._lastItemType = '';
    this._visual = null;
    this._customCssClasses = null;
    this._saveFilterDialog = null;
    this._folderUrlTemplate = '';
    this._updateSuccessCallback = null;

    //Pagination counts
    this._currPage = 0;
    this._pageCount = 0;
    this._visibleCount = 0;
    this._totalCount = 0;
    this._rowIndex = 1;

    //Sort information
    this._sortProperty = '';
    this._sortAscending = true;

    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.SortedGrid.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.SortedGrid.prototype =
{
    //Displays the activity spinner
    display_spinner: function ()
    {
        //Delegate to global functions object
        globalFunctions.display_spinner();
    },
    //Hides the activity spinner
    hide_spinner: function ()
    {
        //Delegate to global functions object
        globalFunctions.hide_spinner();
    },

    _onExpandNode: function (sender, e)
    {
        //Call the method to expand the node
        var artifactId = e.primaryKey;
        var webServiceClass = e.webServiceClass;
        webServiceClass.Expand(e.thisRef.get_projectId(), artifactId, Function.createDelegate(e.thisRef, e.thisRef.expandCollapseNode_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
    },
    _onCollapseNode: function (sender, e)
    {
        //Call the method to expand the node
        var artifactId = e.primaryKey;
        var webServiceClass = e.webServiceClass;
        webServiceClass.Collapse(e.thisRef.get_projectId(), artifactId, Function.createDelegate(e.thisRef, e.thisRef.expandCollapseNode_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
    },
    expandCollapseNode_success: function ()
    {
        //Now we need to reload the data
        this._load_data();
    },

    //External method for loading data (e.g. Refresh)
    load_data: function ()
    {
        //Delegate to private method
        this._lastEditWasInsert = false;
        this._load_data();
    },
    //Loads the data into the grid and databinds
    _load_data: function ()
    {
        //Clear any existing error messages
        this._clear_messages();

        //Get the main data from the Retrieve web method
        this.display_spinner();
        this.get_webServiceClass().SortedList_Retrieve(this.get_projectId(), this._standardFilters, this._displayTypeId, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
    },
    get_loadingComplete: function ()
    {
        return this._loadingComplete;
    },
    clear_loadingComplete: function ()
    {
        this._loadingComplete = false;
    },

    retrieve_success: function (dataSource)
    {
        //Set the datasource and databind
        this.set_dataSource(dataSource);
        this.dataBind(Function.createDelegate(this, this._retrieve_success_callback));
    },
    _retrieve_success_callback: function ()
    {
        this.hide_spinner();

        //Finally make any newly inserted rows editable
        if (this._insertArtifactId != -1)
        {
            //Need to call the next part after 1 second to avoid any timing issues with the DOM
            setTimeout(Function.createDelegate(this, this._retrieve_success_saveandnew), 1000);
        }

        //Raise the loaded event
        this.raise_loaded();
    },
    _retrieve_success_saveandnew: function()
    {
        //We need to find the row containing the item
        var artifactId = this._insertArtifactId;
        this._insertArtifactId = -1;
        var i;
        var tr = null;
        var tableRows = this.get_dataRows();
        for (i = 0; i < tableRows.length; i++)
        {
            var primaryKey = tableRows[i].getAttribute('tst:primarykey');
            if (primaryKey && primaryKey == artifactId)
            {
                tr = tableRows[i];
            }
        }

        if (tr && artifactId > 0)
        {
            //Now we need to make the newly inserted row editable
            var e = {};
            e.primaryKey = artifactId;
            e.thisRef = this;
            e.tr = tr;
            this._onEditClick(null, e);
        }
    },

    update_success: function (messages, context)
    {
        //See if we have a validation error or not
        this.hide_spinner();
        if (messages && messages.length > 0)
        {
            var message = '';
            for (var i = 0; i < messages.length; i++)
            {
                message += '&gt; ' + messages[i].Message + '<br />';
            }
            this.display_error(message);
        }
        else
        {
            //Now we need to reload the data
            this._load_data();

            //Finally display the success message if provided
            if (context && context.successMessage)
            {
                this.display_info(context.successMessage);
            }

            //If the last item was an insert,now need to add a new row
            if (this._lastEditWasInsert)
            {
                this._isInEdit = false;
                this.insert_item(this._lastItemType);
            }

            //If we have a passed in callback function run it now - then clear it out
            if (this._updateSuccessCallback && typeof this._updateSuccessCallback === "function")
            {
                this._updateSuccessCallback();
                this._updateSuccessCallback = null;
            }
        }
    },

    operation_success: function (result, context)
    {
        //See if we have a validation error or not
        this.hide_spinner();
        if (!this._operation_error_check(result))
        {
            //Now we need to reload the data
            this._load_data();

            //Finally display the success message if provided
            if (context && context.successMessage)
            {
                this.display_info(context.successMessage);
            }
        }
    },
    _operation_error_check: function (result)
    {
        if (result != undefined && result != '')
        {
            this.hide_spinner();
            this.display_error(result);

            return true;
        }
        return false;
    },
    operation_failure_quiet: function (exception)
    {
        this.hide_spinner();
    },
    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        this.hide_spinner();
        //Display validation exceptions in a friendly manner
        var messageBox = document.getElementById(this.get_errorMessageControlId());
        globalFunctions.display_error(messageBox, exception);
    },

    _clear_messages: function ()
    {
        //Clear any existing error messages
        globalFunctions.clear_errors($get(this.get_errorMessageControlId()));
    },

    display_error: function (message)
    {
        //If we have a display element, use that otherwise revert to an alert
        globalFunctions.display_error_message($get(this.get_errorMessageControlId()), message);
    },
    display_info: function (message)
    {
        //If we have a display element, use that otherwise revert to an alert
        globalFunctions.display_info_message($get(this.get_errorMessageControlId()), message);
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

    add_focusOn: function (handler)
    {
        this.get_events().addHandler('focusOn', handler);
    },
    remove_focusOn: function (handler)
    {
        this.get_events().removeHandler('focusOn', handler);
    },
    raise_focusOn: function (folderId)
    {
        var h = this.get_events().getHandler('focusOn');
        if (h) h(folderId);
    },

    /*  =========================================================
    Properties
    =========================================================  */

    get_standardFilters: function ()
    {
        return this._standardFilters;
    },
    set_standardFilters: function (value)
    {
        this._standardFilters = value;
    },

    get_displayTypeId: function ()
    {
        return this._displayTypeId;
    },
    set_displayTypeId: function (value)
    {
        this._displayTypeId = value;
    },

    get_touchDragging: function ()
    {
        return this._touchDragging;
    },
    set_touchDragging: function (value)
    {
        this._touchDragging = value;
    },

    get_folderUrlTemplate: function () {
        return this._folderUrlTemplate;
    },
    set_folderUrlTemplate: function (value) {
        this._folderUrlTemplate = value;
    },

    get_errorMessageControlId: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._errorMessageControlId;
    },
    set_errorMessageControlId: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._errorMessageControlId != value)
        {
            this._errorMessageControlId = value;
            this.raisePropertyChanged('errorMessageControlId');
        }
    },

    get_totalCountControlId: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._totalCountControlId;
    },
    set_totalCountControlId: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._totalCountControlId != value)
        {
            this._totalCountControlId = value;
            this.raisePropertyChanged('totalCountControlId');
        }
    },

    get_visibleCountControlId: function ()
    {
        return this._visibleCountControlId;
    },
    set_visibleCountControlId: function (value)
    {
        this._visibleCountControlId = value;
    },

    get_filterInfoControlId: function ()
    {
        return this._filterInfoControlId;
    },
    set_filterInfoControlId: function (value)
    {
        this._filterInfoControlId = value;
    },

    get_contextMenuItems: function ()
    {
        return this._contextMenuItems;
    },
    set_contextMenuItems: function (value)
    {
        this._contextMenuItems = value;
    },

    get_customCssClasses: function ()
    {
        return this._customCssClasses;
    },
    set_customCssClasses: function (value)
    {
        this._customCssClasses = value;
    },

    get_dateFormat: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._dateFormat;
    },
    set_dateFormat: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._dateFormat != value)
        {
            this._dateFormat = value;
            this.raisePropertyChanged('dateFormat');
        }
    },

    get_artifactPrefix: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._artifactPrefix;
    },

    set_artifactPrefix: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._artifactPrefix != value)
        {
            this._artifactPrefix = value;
            this.raisePropertyChanged('artifactPrefix');
        }
    },

    get_cssClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._cssClass;
    },

    set_cssClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._cssClass != value)
        {
            this._cssClass = value;
            this.raisePropertyChanged('cssClass');
        }
    },

    get_headerCssClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._headerCssClass;
    },

    set_headerCssClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._headerCssClass != value)
        {
            this._headerCssClass = value;
            this.raisePropertyChanged('headerCssClass');
        }
    },

    get_subHeaderCssClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._subHeaderCssClass;
    },

    set_subHeaderCssClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._subHeaderCssClass != value)
        {
            this._subHeaderCssClass = value;
            this.raisePropertyChanged('subHeaderCssClass');
        }
    },

    get_rowCssClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._rowCssClass;
    },

    set_rowCssClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._rowCssClass != value)
        {
            this._rowCssClass = value;
            this.raisePropertyChanged('rowCssClass');
        }
    },

    get_selectedRowCssClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._selectedRowCssClass;
    },

    set_selectedRowCssClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._selectedRowCssClass != value)
        {
            this._selectedRowCssClass = value;
            this.raisePropertyChanged('selectedRowCssClass');
        }
    },

    get_editRowCssClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._editRowCssClass;
    },

    set_editRowCssClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._editRowCssClass != value)
        {
            this._editRowCssClass = value;
            this.raisePropertyChanged('editRowCssClass');
        }
    },

    get_themeFolder: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._themeFolder;
    },

    set_themeFolder: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._themeFolder != value)
        {
            this._themeFolder = value;
            this.raisePropertyChanged('themeFolder');
        }
    },

    get_emptyDataText: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._emptyDataText;
    },

    set_emptyDataText: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._dataSource != value)
        {
            this._emptyDataText = value;
            this.raisePropertyChanged('emptyDataText');
        }
    },

    get_itemImage: function ()
    {
        return this._itemImage;
    },
    set_itemImage: function (value)
    {
        this._itemImage = value;
    },

    get_alternateItemImage: function ()
    {
        return this._alternateItemImage;
    },
    set_alternateItemImage: function (value)
    {
        this._alternateItemImage = value;
    },

    get_folderItemImage: function ()
    {
        return this._folderItemImage;
    },
    set_folderItemImage: function (value)
    {
        this._folderItemImage = value;
    },
    
    get_projectId: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._projectId;
    },
    set_projectId: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: Number}]);
        if (e) throw e;

        if (this._projectId != value)
        {
            this._projectId = value;
            this.raisePropertyChanged('projectId');
        }
    },

    get_allowEdit: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._allowEdit;
    },
    set_allowEdit: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: Boolean}]);
        if (e) throw e;

        if (this._allowEdit != value)
        {
            this._allowEdit = value;
            this.raisePropertyChanged('allowEdit');
        }
    },

    get_elementId: function ()
    {
        return this.get_element().id;
    },

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },

    get_concurrencyEnabled: function ()
    {
        return this._concurrencyEnabled;
    },
    set_concurrencyEnabled: function (value)
    {
        this._concurrencyEnabled = value;
    },

    get_allowColumnPositioning: function ()
    {
        return this._allowColumnPositioning;
    },
    set_allowColumnPositioning: function (value)
    {
        //turn off column resize/positioning on mobile devices
        this._allowColumnPositioning = SpiraContext.IsMobile ? false : value;
    },

    get_allowDragging: function ()
    {
        return this._allowDragging;
    },
    set_allowDragging: function (value)
    {
        this._allowDragging = value;
    },

    get_visual: function()
    {
        return this._visual;
    },
    set_visual: function(value)
    {
        this._visual = value;
    },

    get_displayAttachments: function ()
    {
        return this._displayAttachments;
    },
    set_displayAttachments: function (value)
    {
        this._displayAttachments = value;
    },

    get_displayCheckboxes: function ()
    {
        return this._displayCheckboxes;
    },
    set_displayCheckboxes: function (value)
    {
        this._displayCheckboxes = value;
    },

    get_displayTooltip: function ()
    {
        return this._displayTooltip;
    },
    set_displayTooltip: function (value)
    {
        this._displayTooltip = value;
    },

    get_negativePrimaryKeysDisabled: function ()
    {
        return this._negativePrimaryKeysDisabled;
    },
    set_negativePrimaryKeysDisabled: function (value)
    {
        this._negativePrimaryKeysDisabled = value;
    },

    get_webServiceClass: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._webServiceClass;
    },

    set_webServiceClass: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: Function}]);
        if (e) throw e;

        if (this._webServiceClass != value)
        {
            this._webServiceClass = value;
            this.raisePropertyChanged('webServiceClass');
        }
    },

    get_baseUrl: function ()
    {
        return this._baseUrl;
    },
    set_baseUrl: function (value)
    {
        this._baseUrl = value;
    },

    get_appPath: function ()
    {
        return this._appPath;
    },
    set_appPath: function (value)
    {
        this._appPath = value;
    },

    get_baseUrlTarget: function ()
    {
        return this._baseUrlTarget;
    },
    set_baseUrlTarget: function (value)
    {
        this._baseUrlTarget = value;
    },

    get_alternateItemImage: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._alternateItemImage;
    },

    set_alternateItemImage: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: String}]);
        if (e) throw e;

        if (this._alternateItemImage != value)
        {
            this._alternateItemImage = value;
            this.raisePropertyChanged('alternateItemImage');
        }
    },

    get_selectedIndex: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._selectedIndex;
    },

    set_selectedIndex: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: Number}]);
        if (e) throw e;

        if (this._selectedIndex != value)
        {
            this._select(value)
        }
    },

    get_columns: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._columns;
    },

    set_columns: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: Array, elementType: Inflectra.SpiraTest.Web.ServerControls.SortedGridColumn, elementMayBeNull: false}]);
        if (e) throw e;

        if (this._columns != value)
        {
            this._columns = value;
            this.raisePropertyChanged('columns');
        }
    },

    get_dataSource: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._dataSource;
    },

    set_dataSource: function (value)
    {
        var e = Function._validateParams(arguments, [{ name: 'value', type: Object, mayBeNull: true}]);
        if (e) throw e;

        if (this._dataSource != value)
        {
            this._dataSource = value;
            this.raisePropertyChanged('dataSource');
        }
    },

    get_headerRow: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._headerRow;
    },

    get_subHeaderRow: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._subHeaderRow;
    },

    get_dataRows: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        return this._tableRows;
    },

    get_selectedValue: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        if ((this.get_dataSource() == null) || (this.get_dataSource().length == 0))
        {
            return null;
        }

        if ((this.get_dataKeyName() == null) || (this.get_dataKeyName().length == 0))
        {
            return null;
        }

        if (this.get_selectedIndex() > -1)
        {
            return this.get_dataSource()[this.get_selectedIndex()][this.get_dataKeyName()];
        }

        return null;
    },

    get_selectedRow: function ()
    {
        if (arguments.length !== 0) throw Error.parameterCount();

        if ((this.get_dataRows() == null) || (this.get_dataRows().length == 0))
        {
            return null;
        }

        if (this.get_selectedIndex() > -1)
        {
            return this.get_dataRows()[this.get_selectedIndex()];
        }

        return null;
    },

    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SortedGrid.callBaseMethod(this, 'initialize');
        //Now load in the data if autoload set
        if (this._autoLoad)
        {
            this.load_data();
        }
    },

    dispose: function ()
    {
        //Clear the various handlers
        this._clearTableRowHandlers();
        this._clearKeyDownHandlers();
        this._clearOtherHandlers();
        this._clearButtonHandlers();
        this._clearDragHandlers();
        this._clearTooltipHandlers();
        this._clearClickElementHandlers();

        delete this._buttonClickHandlers;
        delete this._headerDragHandlers;
        delete this._iconDragHandlers;
        delete this._tooltipOverHandlers;
        delete this._tooltipOutHandlers;
        delete this._otherHandlers;
        delete this._otherClickHandlers;
        delete this._tableRowDblClickHandlers;
        delete this._tableRowKeyDownHandlers;
        delete this._keyDownElements;
        delete this._keyDownHandlers;

        delete this._tableRows;
        delete this._tableEditRows;
        delete this._filters;
        delete this._edits;
        delete this._headers;
        delete this._icons;
        delete this._tooltips;
        delete this._buttons;
        delete this._clickElements;
        delete this._headerRow;
        delete this._subHeaderRow;
        delete this._columns;
        delete this._editColumnHeader;
        delete this._concurrencyValues;
        delete this._contextMenuItems;
        delete this._contextMenu;
        delete this._customCssClasses;
        delete this._saveFilterDialog;

        Inflectra.SpiraTest.Web.ServerControls.SortedGrid.callBaseMethod(this, 'dispose');
    },

    dataBind: function (callback)
    {
        if (arguments.length !== 1) throw Error.parameterCount();

        // Create a new table right now to hold the regenerated DOM
        this._loadingComplete = false;  // Denotes when load is finished
        var table = this.get_element();

        //Clear any of the existing handlers
        this._clearFilters();
        this._clearEdits();
        this._clearTableRowHandlers();
        this._clearKeyDownHandlers();
        this._clearOtherHandlers();
        this._clearDragHandlers();
        this._clearButtonHandlers();
        this._clearTooltipHandlers();
        this._clearClickElementHandlers();

        //Reseting the module level variables
        this._headerRow = null;
        this._subHeaderRow = null;
        this._selectColumnHeader = null;
        this._deleteColumnHeader = null;
        this._tableRows = new Array();
        this._tableEditRows = new Array();
        this._selectedIndex = -1;
        this._isInDrag = false;
        this._isInEdit = false;
        this._isOverNameDesc = false;
        this._draggingColumnIndex = -1;
        this._draggingPrimaryKey = -1;
        this._draggingImg = null;
        this._draggingTarget = null;
        this._columns = null;
        this._editColumnHeader = null;

        if (this._concurrencyEnabled)
        {
            this._concurrencyValues = {};
        }

        var dataSource = this.get_dataSource();
        var dataItems = dataSource.items;
        var filterNames = dataSource.filterNames;
        //Shows the Empty Text if DataSource contains nothing 
        if ((dataItems == null) || (dataItems.length == 0))
        {
            if ((this.get_emptyDataText() != null) && (this.get_emptyDataText().length > 0))
            {
                var tBody = d.ce('tbody');

                table.appendChild(tBody);

                var tr = d.ce('tr');
                tBody.appendChild(tr);

                var td = d.ce('td');
                tr.appendChild(td);
                td.style.textAlign = 'left';

                td.innerHTML = this.get_emptyDataText();
            }
            //No need to proceed furhter as the datasource is empty
            return;
        }

        var dataRow;
        var column;
        var i = 0;
        var allowDragAndDrop = false;

        //The server control doesn't provide the columns, it is derived dynamically from the datasource
        //this is same as a GridView set to AutoGenerateColumn = True
        var columns = new Array();
        //The first row contains the filter values and columns (not real data) as well as pagination info
        dataRow = dataItems[0];
        this._currPage = dataSource.currPage;
        this._pageCount = dataSource.pageCount;
        var endRow = dataSource.startRow + dataSource.visibleCount - 1;
        this._visibleCount = dataSource.startRow + ' - ' + endRow;
        this._totalCount = dataSource.totalCount;
        this._sortProperty = dataSource.sortProperty;
        this._sortAscending = dataSource.sortAscending;

        for (var property in dataRow.Fields)
        {
            if (!property.startsWith('__')) // Excluding the Ajax Framework Properties
            {
                column = new Inflectra.SpiraTest.Web.ServerControls.SortedGridColumn();

                //Set the various column fields
                column.fieldType = dataRow.Fields[property].fieldType;
                column.headerText = dataRow.Fields[property].caption;
                column.dataField = property;
                column.lookupName = dataRow.Fields[property].lookupName;
                column.allowDragAndDrop = dataRow.Fields[property].allowDragAndDrop;
                column.width = dataRow.Fields[property].width;
                if (column.fieldType == globalFunctions._fieldType_nameDescription || column.fieldType == globalFunctions._fieldType_equalizer)
                {
                    column.nowrap = true;
                }
                //All columns except equalizers can be sorted
                if (column.fieldType == globalFunctions._fieldType_equalizer || dataRow.Fields[property].notSortable)
                {
                    column.sortable = false;
                }
                else
                {
                    column.sortable = true;
                }

                columns.push(column);

                //Update the global drag and drop flag
                if (column.allowDragAndDrop)
                {
                    allowDragAndDrop = true;
                }
            }
        }

        //Persist the columns collection
        this.set_columns(columns);

        //Apply the Css class if specified for the table
        if ((this.get_cssClass() != null) && (this.get_cssClass().length > 0))
        {
            Sys.UI.DomElement.addCssClass(table, this.get_cssClass());
        }

        //We will render the table as Per W3C so it will contain <thead><tbody> etc
        var thead = d.ce('thead');
        this._headerRow = d.ce('tr');
        this._headerRow.setAttribute('role', 'rowheader');

        thead.appendChild(this._headerRow);

        //Apply the header css in the thead row if specified
        if ((this.get_headerCssClass() != null) && (this.get_headerCssClass().length > 0))
        {
            this._headerRow.className = this.get_headerCssClass();
        }

        var th;
        var dragHandler;
        var a;
        var img;

        //Create the headers for checkbox column and attachments column
        if (this._displayCheckboxes)
        {
            th = d.ce('th');
            th.className = 'Checkbox priority3';
            this._headerRow.appendChild(th);
            img = d.ce('span');
            img.className = 'fas fa-check';
            img.setAttribute('Tooltip',resx.Global_SelectAll);
            img.title = resx.Global_SelectAll;
            th.appendChild(img);
        }

        if (this._displayAttachments)
        {
            th = d.ce('th');
            th.className = 'Attachment priority3';
            this._headerRow.appendChild(th);
            img = d.ce('span');
            img.className = 'fas fa-paperclip';
            img.setAttribute('tooltip', resx.Global_ArtifactHasAttachment);
            img.title = resx.Global_ArtifactHasAttachment;
            th.appendChild(img);
        }

        for (i = 0; i < columns.length; i++)
        {
            column = columns[i];

            th = d.ce('th');

            //We add a div inside the TH so that we can make the widths fixed
            var thDiv = d.ce('div');
            thDiv.className = 'fixed-width';
            if (column.width)
            {
                thDiv.style.width = column.width + 'px';
            }
            th.appendChild(thDiv);

            //Add the responsive classes
            if (column.fieldType == globalFunctions._fieldType_equalizer)
            {
                th.className = 'priority2';
            }
            if (column.fieldType == globalFunctions._fieldType_identifier)
            {
                th.className = 'priority4';
            }
            if (column.fieldType == globalFunctions._fieldType_nameDescription)
            {
                th.className = 'priority1';
            }

            if (this._customCssClasses && this._customCssClasses[globalFunctions.keyPrefix + column.dataField] && this._customCssClasses[globalFunctions.keyPrefix + column.dataField] != '')
            {
                th.className = this._customCssClasses[globalFunctions.keyPrefix + column.dataField];
            }

            this._headerRow.appendChild(th);
            column.$header = th; //Need to store the header in the column, so that we can calculate the index in later

            //First display the column header caption
            var spanCaption = d.ce('span');
            thDiv.appendChild(spanCaption);
            spanCaption.appendChild(d.ctn(column.headerText));

            if (column.allowDragAndDrop && this._allowColumnPositioning)
            {
                //Need to hook the mousedown event for drag n drop
                var dragSource = $create(Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior, { columnIndex: i, dataField: column.dataField, 'parent': this }, null, null, th);
                dragHandler = Function.createCallback(this._onHeaderMouseDown, { thisRef: this, th: th, dragSource: dragSource });
                $addHandler(spanCaption, 'mousedown', dragHandler);
                spanCaption.style.cursor = 'move';
                this._headers.push(spanCaption);
                this._headerDragHandlers.push(dragHandler);
            }

            //Apply different styles which is mentioned in the column
            if ((column.headerHorizontalAlign != null) && (column.headerHorizontalAlign.length > 0))
            {
                thDiv.style.textAlign = column.headerHorizontalAlign;
            }

            if ((column.headerVerticalAlign != null) && (column.headerVerticalAlign.length > 0))
            {
                thDiv.style.verticalAlign = column.headerVerticalAlign;
            }

            //If the columns are sortable, display the sort arrows
            if (column.sortable)
            {
                //Ascending Arrow
                var span = d.ce('span');
                //See if this sort is selected currently (for lookups need to use lookup name)
                var sortProperty = column.dataField;
                if (column.lookupName != '')
                {
                    sortProperty = column.lookupName;
                }
                if (this._sortProperty == sortProperty && this._sortAscending)
                {
                    span.className = 'sort-ascending-selected';
                }
                else
                {
                    span.className = 'sort-ascending';
                    span.title = resx.SortedList_SortByAscending.replace('{0}', column.headerText);
                    var sortClickHandler = Function.createCallback(this._onSortClick, { thisRef: this, sortProperty: sortProperty, sortAscending: true });
                    $addHandler(span, 'click', sortClickHandler);
                    this._buttons.push(span);
                    this._buttonClickHandlers.push(sortClickHandler);
                }
                thDiv.appendChild(span);

                //Descending Arrow
                span = d.ce('span');
                //See if this sort is selected currently
                if (this._sortProperty == sortProperty && !this._sortAscending)
                {
                    span.className = 'sort-descending-selected';
                }
                else
                {
                    span.className = 'sort-descending';
                    span.title = resx.SortedList_SortByDescending.replace('{0}', column.headerText);
                    var sortClickHandler = Function.createCallback(this._onSortClick, { thisRef: this, sortProperty: sortProperty, sortAscending: false });
                    $addHandler(span, 'click', sortClickHandler);
                    this._buttons.push(span);
                    this._buttonClickHandlers.push(sortClickHandler);
                }
                thDiv.appendChild(span);
            }
        }

        //Add the header for the Edit button column
        if (this._allowEdit)
        {
            this._editColumnHeader = d.ce('th');
            this._editColumnHeader.className = 'edit priority3';
            this._editColumnHeader.appendChild(d.ctn(resx.Global_Edit));
            this._headerRow.appendChild(this._editColumnHeader);
        }

        //Now build the subheader row
        var td;
        var checkbox;
        var textbox;
        var datecontrol;
        var dropdown;
        this._subHeaderRow = d.ce('tr');
        thead.appendChild(this._subHeaderRow);

        //Apply the header css in the thead row if specified
        if ((this.get_subHeaderCssClass() != null) && (this.get_subHeaderCssClass().length > 0))
        {
            this._subHeaderRow.className = this.get_subHeaderCssClass();
        }

        //Add the keypress handler for filtering
        var subHeaderKeyDownHandler = Function.createCallback(this._onSubHeaderKeyDown, { thisRef: this });
        $addHandler(this._subHeaderRow, 'keydown', subHeaderKeyDownHandler);
        this._keyDownElements.push(this._subHeaderRow);
        this._keyDownHandlers.push(subHeaderKeyDownHandler);

        //Create the subheaders for checkbox column and attachments column
        if (this._displayCheckboxes)
        {
            td = d.ce('td');
            td.className = 'Checkbox priority3';
            this._subHeaderRow.appendChild(td);
            checkbox = d.ce('input');
            checkbox.type = 'checkbox';
            checkbox.name = 'chkHead';
            td.appendChild(checkbox);

            //Add the checkbox handler
            var _checkboxSelectAllChangedHandler = Function.createCallback(this._onCheckboxSelectAllChanged, { thisRef: this, checkbox: checkbox });
            $addHandler(checkbox, 'click', _checkboxSelectAllChangedHandler);
            this._clickElements.push(checkbox);
            this._otherClickHandlers.push(_checkboxSelectAllChangedHandler);
        }
        if (this._displayAttachments)
        {
            td = d.ce('td');
            td.className = 'priority3';
            this._subHeaderRow.appendChild(td);
        }

        //Now all the filterable columns
        for (i = 0; i < columns.length; i++)
        {
            column = columns[i];
            td = d.ce('td');
            this._subHeaderRow.appendChild(td);

            //We add a div inside the TD so that we can make the widths fixed
            var tdDiv = d.ce('div');
            if (column.width)
            {
                tdDiv.style.width = column.width + 'px';
            }
            td.appendChild(tdDiv);

            //The resizable column behavior
            if (this._allowColumnPositioning)
            {
                tdDiv.className = 'fixed-width';
                var dragSource = $create(Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior, { parent: this, fieldName: column.dataField }, null, null, tdDiv);
                var resizeMouseDownHandler = Function.createCallback(this._onResizableColumnMouseDown, { thisRef: this, div: tdDiv, fieldName: column.dataField, dragSource: dragSource });
                var resizeMouseUpHandler = Function.createCallback(this._onResizableColumnMouseUp, { thisRef: this, div: tdDiv });
                var resizeTouchMoveHandler = Function.createCallback(this._onResizableColumnTouchMove, { thisRef: this, div: tdDiv, fieldName: column.dataField, dragSource: dragSource });
                $addHandler(tdDiv, 'mousedown', resizeMouseDownHandler);
                $addHandler(tdDiv, 'mouseup', resizeMouseUpHandler);
                if (tdDiv.addEventListener) //IE8 doesn't support
                {
                    tdDiv.addEventListener('touchmove', resizeTouchMoveHandler, false);
                }
                this._dragElements.push(tdDiv);
                this._dragDownHandlers.push(resizeMouseDownHandler);
                this._dragUpHandlers.push(resizeMouseUpHandler);
                this._touchHandlers.push(resizeTouchMoveHandler);
            }

            //Depending on the type of column, will need to display the appropriate filter
            if (column.fieldType == globalFunctions._fieldType_text)
            {
                textbox = d.ce('input');
                textbox.type = 'text';
                textbox.maxLength = 50;
                textbox.className = 'text-box fill';
                textbox.name = this.get_element().id + '_filter_' + column.dataField;
                if (dataRow.Fields[column.dataField] != null && dataRow.Fields[column.dataField].textValue)
                {
                    textbox.value = dataRow.Fields[column.dataField].textValue;
                }
                tdDiv.appendChild(textbox);
            }
            if (column.fieldType == globalFunctions._fieldType_integer || column.fieldType == globalFunctions._fieldType_decimal || column.fieldType == globalFunctions._fieldType_timeInterval)
            {
                var div = d.ce('div');
                div.id = this.get_elementId() + '_filter_' + column.dataField + '_rng';
                div.className = 'NumberRangeFilter';
                div.style.width = '90px';
                var rangeFilter = $create(Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter, null, null, null, div);
                rangeFilter.set_name(this.get_elementId() + '_filter_' + column.dataField);
                //Set the filter value
                if (column.fieldType == globalFunctions._fieldType_decimal || column.fieldType == globalFunctions._fieldType_timeInterval)
                {
                    if (dataRow.Fields[column.dataField] != null && dataRow.Fields[column.dataField].textValue)
                    {
                        rangeFilter.set_value(dataRow.Fields[column.dataField].textValue);
                    }
                }
                else
                {
                    if (dataRow.Fields[column.dataField] != null && !globalFunctions.isNullOrUndefined(dataRow.Fields[column.dataField].textValue))
                    {
                        rangeFilter.set_value(dataRow.Fields[column.dataField].textValue);
                    }
                    else if (dataRow.Fields[column.dataField] != null && !globalFunctions.isNullOrUndefined(dataRow.Fields[column.dataField].intValue))
                    {
                        rangeFilter.set_value(dataRow.Fields[column.dataField].intValue);
                    }
                }
                //Make it fire a filter event on change
                rangeFilter.add_updated(Function.createDelegate(this, this.apply_filters));
                tdDiv.appendChild(rangeFilter.get_element());
                this._filters.push(rangeFilter);
            }
            if (column.fieldType == globalFunctions._fieldType_lookup || column.fieldType == globalFunctions._fieldType_flag ||
                column.fieldType == globalFunctions._fieldType_customPropertyLookup || column.fieldType == globalFunctions._fieldType_customPropertyMultiList ||
                column.fieldType == globalFunctions._fieldType_multiList)
            {
                var div = d.ce('div');
                div.id = this.get_element().id + '_filter_' + column.dataField + '_ddl';
                if (column.fieldType == globalFunctions._fieldType_flag)
                {
                    div.style.width = '70px';
                }
                else
                {
                    div.style.width = '94px';
                }
                dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { multiSelectable: true, listWidth: '120px' }, null, null, div);
                dropdown.set_name(this.get_element().id + '_filter_' + column.dataField);
                //Add the any and (none) options
                dropdown.addItem('', resx.Global_Any);
                //Add the (None) option to non-flags
                if (column.fieldType != globalFunctions._fieldType_flag)
                {
                    dropdown.addItem('-999', resx.Global_None2);
                }
                //Add the lookup values
                var lookups = dataRow.Fields[column.dataField].lookups;
                dropdown.set_dataSource(lookups);
                dropdown.dataBind();
                //Default to the -- Any -- item
                dropdown.set_selectedItem('');
                if (dataRow.Fields[column.dataField] != null && dataRow.Fields[column.dataField].textValue)
                {
                    dropdown.set_selectedItem(dataRow.Fields[column.dataField].textValue);
                }
                //Make it fire a filter event on change
                dropdown.add_selectedItemChanged(Function.createDelegate(this, this.apply_filters));
                tdDiv.appendChild(div);
                this._filters.push(dropdown);
            }
            if (column.fieldType == globalFunctions._fieldType_hierarchyLookup)
            {
                var div = d.ce('div');
                div.id = this.get_element().id + '_filter_' + column.dataField + '_ddl';
                div.style.width = '94px';
                dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy, { "listWidth": "200px", "themeFolder": this._themeFolder }, null, null, div);
                //Handle the images (varies by field)
                globalFunctions.getHierarchyLookupImages(dropdown, column.dataField);
                dropdown.addItem('', '0', '', '', 'Y', resx.Global_Any);
                //Add the (None) option
                dropdown.addItem('-999', '0', '', '', 'Y', resx.Global_None2);
                dropdown.set_name(this.get_element().id + '_filter_' + column.dataField);
                //Add the lookup values
                var lookups = dataRow.Fields[column.dataField].lookups;
                dropdown.set_dataSource(lookups);
                dropdown.dataBind();
                //Default to the -- Any -- item
                dropdown.set_selectedItem('');
                if (dataRow.Fields[column.dataField] != null && !globalFunctions.isNullOrUndefined(dataRow.Fields[column.dataField].intValue))
                {
                    dropdown.set_selectedItem(dataRow.Fields[column.dataField].intValue);
                }
                tdDiv.appendChild(dropdown.get_element());
                //Make it fire a filter event on change
                dropdown.add_selectedItemChanged(Function.createDelegate(this, this.apply_filters));
                this._filters.push(dropdown);
            }
            if (column.fieldType == globalFunctions._fieldType_equalizer)
            {
                td.className = 'priority2';
                var div = d.ce('div');
                div.id = this.get_element().id + '_filter_' + column.dataField + '_ddl';
                div.style.width = '94px';
                dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, null, null, null, div);
                dropdown.set_name(this.get_element().id + '_filter_' + column.dataField);
                dropdown.addItem('', resx.Global_Any);
                //Add the lookup values
                var lookups = dataRow.Fields[column.dataField].lookups;
                dropdown.set_dataSource(lookups);
                dropdown.dataBind();
                //Default to the -- Any -- item
                dropdown.set_selectedItem('');
                if (dataRow.Fields[column.dataField] != null && !globalFunctions.isNullOrUndefined(dataRow.Fields[column.dataField].intValue))
                {
                    dropdown.set_selectedItem(dataRow.Fields[column.dataField].intValue);
                }
                //Make it fire a filter event on change
                dropdown.add_selectedItemChanged(Function.createDelegate(this, this.apply_filters));

                tdDiv.appendChild(dropdown.get_element());
                this._filters.push(dropdown);
            }
            if (column.fieldType == globalFunctions._fieldType_dateTime || column.fieldType == globalFunctions._fieldType_customPropertyDate)
            {
                var div = d.ce('div');
                div.id = this.get_element().id + '_filter_' + column.dataField + '_dat';
                div.className = 'DatePicker';
                var datePicker = $create(Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter, null, null, null, div);
                datePicker.set_name(this.get_element().id + '_filter_' + column.dataField);
                datePicker.set_dateFormat(this.get_dateFormat());
                //Set the filter value
                if (dataRow.Fields[column.dataField] != null && dataRow.Fields[column.dataField].textValue)
                {
                    datePicker.set_value(dataRow.Fields[column.dataField].textValue);
                }
                //Make it fire a filter event on change
                datePicker.add_updated(Function.createDelegate(this, this.apply_filters));
                tdDiv.appendChild(datePicker.get_element());
                this._filters.push(datePicker);
            }
            if (column.fieldType == globalFunctions._fieldType_nameDescription)
            {
                td.className = 'priority1';
                textbox = d.ce('input');
                textbox.type = 'text';
                textbox.placeholder = resx.Global_Filter;
                textbox.maxLength = 50;
                textbox.className = 'text-box fill';
                textbox.name = this.get_element().id + '_filter_' + column.dataField;
                if (dataRow.Fields[column.dataField] != null && dataRow.Fields[column.dataField].textValue)
                {
                    textbox.value = dataRow.Fields[column.dataField].textValue;
                }
                tdDiv.appendChild(textbox);
            }
            if (column.fieldType == globalFunctions._fieldType_identifier)
            {
                td.className = 'priority4';
                tdDiv.appendChild(d.ctn(this.get_artifactPrefix() + ' '));
                textbox = d.ce('input');
                textbox.type = 'text';    //Do not make type='number' breaks the filtering
                textbox.maxLength = 10;
                textbox.className = 'text-box narrow';
                textbox.name = this.get_element().id + '_filter_' + column.dataField;
                if (dataRow.Fields[column.dataField] != null && !globalFunctions.isNullOrUndefined(dataRow.Fields[column.dataField].intValue))
                {
                    textbox.value = dataRow.Fields[column.dataField].intValue;
                }
                tdDiv.appendChild(textbox);
            }

            if (this._customCssClasses && this._customCssClasses[globalFunctions.keyPrefix + column.dataField] && this._customCssClasses[globalFunctions.keyPrefix + column.dataField] != '')
            {
                td.className = this._customCssClasses[globalFunctions.keyPrefix + column.dataField];
            }
        }

        //Add the bulk edit button handlers
        if (this._allowEdit)
        {
            //Finally the bulk edit button column
            td = d.ce('td');
            td.className = 'priority3';
            this._subHeaderRow.appendChild(td);
            var btn = d.ce('button');
            btn.type = "button";
            btn.className = 'btn btn-default';
            btn.appendChild(d.createTextNode(resx.Global_Edit));
            td.appendChild(btn);

            var _bulkEditClickHandler = Function.createDelegate(this, this._onBulkEditClick);
            $addHandler(btn, 'click', _bulkEditClickHandler);
            this._buttons.push(btn);
            this._buttonClickHandlers.push(_bulkEditClickHandler);
        }

        //Display the list of filters (if any)
        var label = $get(this._filterInfoControlId);
        if (label)
        {
            globalFunctions.clearContent(label);
            if (filterNames && filterNames.length > 0)
            {
                var filterText = '';
                for (var i = 0; i < filterNames.length; i++)
                {
                    if (filterText == '')
                    {
                        filterText = filterNames[i];
                    }
                    else
                    {
                        filterText += ', ' + filterNames[i];
                    }
                }
                label.appendChild(d.createTextNode(resx.Global_FilteringBy.replace('{0}', filterText) + ' '));
                var link = d.ce('a');
                link.className = 'btn btn-default btn-sm';
                label.appendChild(link);
                link.appendChild(d.createTextNode(resx.Global_ClearFilters));
                link.href = 'javascript:$find(\'' + this._element.id + '\').clear_filters()';
            }
        }

        //Headers are rendered, now dumpt the data form the datasource
        var tBody = d.ce('tbody');

        //Spawn asynchronous process to load the data
        this._rowIndex = 1;
        this._dataBind_Process(dataSource, columns, table, tBody, thead, callback, this);
    },
    _dataBind_Process: function (dataSource, columns, table, tBody, thead, callback, thisRef)
    {
        if (arguments.length !== 7) throw Error.parameterCount();

        var internalCount = 0;
        var dataItems = dataSource.items;
        while (thisRef._rowIndex < dataItems.length)
        {
            var dataRow = dataItems[thisRef._rowIndex];
            thisRef._dataBindRow(dataRow, columns, table, tBody);
            internalCount++;
            thisRef._rowIndex++;
            if (internalCount == 15)
            {
                break;
            }
        }
        if (thisRef._rowIndex < dataItems.length)
        {
            setTimeout(function () { thisRef._dataBind_Process(dataSource, columns, table, tBody, thead, callback, thisRef); }, 0);
        }
        else
        {
            //Display the footer that lets the user change pagination options
            var tr = d.ce('tr');
            tr.className = 'Pagination';
            tr.setAttribute('role', 'rowfooter');
            tBody.appendChild(tr);
            var td = d.ce('td');
            tr.appendChild(td);
            td.setAttribute("colSpan", columns.length + 3);
            var divDisplay = d.ce('div');
            divDisplay.className = "pagination-display";
            var div = d.ce('div');
            td.appendChild(divDisplay);
            divDisplay.appendChild(div);
            div.appendChild(d.ctn(resx.Global_Show + '\u00a0'));
            div = d.ce('div');
            thisRef._ddlPaginationOptions = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, null, null, null, div);
            var dropdown = thisRef._ddlPaginationOptions;
            dropdown.set_name(thisRef.get_element().id + '_ddlPaginationOptions');
            //Add event handler
            thisRef._paginationOptionsChangedHandler = Function.createDelegate(thisRef, thisRef._onPaginationOptionsChanged);
            dropdown.add_selectedItemChanged(thisRef._paginationOptionsChangedHandler);
            divDisplay.appendChild(div);
            div = d.ce('div');
            divDisplay.appendChild(div);
            div.appendChild(d.ctn('\u00a0' + resx.Global_RowsPerPage));

            //Display Prev/Next if we need to
            div = d.ce('div');
            div.className = 'pagination-pages';
            td.appendChild(div);
            this._currPage
            this._pageCount

            //First/Previous buttons
            var prevLink = d.ce('a');
            div.appendChild(prevLink);
            prevLink.title = resx.Pagination_First;
            var glyph = d.ce('span');
            glyph.className = 'fas fa-fast-backward mr2';
            prevLink.appendChild(glyph);
            if (this._currPage > 1)
            {
                prevLink.style.cursor = 'pointer';
                var _firstClickHandler = Function.createCallback(this._onFirstClick, { thisRef: this });
                $addHandler(prevLink, 'click', _firstClickHandler);
                this._buttons.push(prevLink);
                this._buttonClickHandlers.push(_firstClickHandler);
            }
            else
            {
                //Make the button look non-functional
                prevLink.className = 'aspNetDisabled';
            }
            div.appendChild(document.createTextNode(' '));
            prevLink = d.ce('a');
            div.appendChild(prevLink);
            prevLink.title = resx.Pagination_Previous;
            var glyph = d.ce('span');
            glyph.className = 'fas fa-backward mr2';
            prevLink.appendChild(glyph);
            if (this._currPage > 1)
            {
                prevLink.style.cursor = 'pointer';
                var _prevClickHandler = Function.createCallback(this._onPrevClick, { thisRef: this });
                $addHandler(prevLink, 'click', _prevClickHandler);
                this._buttons.push(prevLink);
                this._buttonClickHandlers.push(_prevClickHandler);
            }
            else
            {
                //Make the button look non-functional
                prevLink.className = 'aspNetDisabled';
            }

            //Page number legend
            div.appendChild(document.createTextNode('\u00a0' + resx.Global_DisplayingPage + '\u00a0'));
            var textbox = d.ce('input');
            var textboxIdPrefix = this.get_element() && this.get_element().id ? this.get_element().id + "_" : ""; //used to make the id more likely to be unique
            textbox.type = 'text';
            textbox.className = 'text-box narrow';
            textbox.maxLength = 10;
            textbox.id = textboxIdPrefix + 'txtCurPage';
            textbox.value = this._currPage;
            div.appendChild(textbox);
            div.appendChild(document.createTextNode('\u00a0'));

            //Add the keypress handler for changing the page number
            var refreshKeyDownHandler = Function.createCallback(this._onChangePageKeyDown, { thisRef: this, textbox: textbox });
            $addHandler(textbox, 'keydown', refreshKeyDownHandler);
            this._keyDownElements.push(textbox);
            this._keyDownHandlers.push(refreshKeyDownHandler);

            //Refresh button
            var refreshLink = d.ce('a');
            div.appendChild(refreshLink);
            refreshLink.title = resx.Pagination_Refresh;
            refreshLink.style.cursor = 'pointer';
            var glyph = d.ce('span');
            glyph.className = 'fas fa-sync';
            refreshLink.appendChild(glyph);
            var _refreshClickHandler = Function.createCallback(this._onChangePageClick, { thisRef: this, textbox: textbox });
            $addHandler(refreshLink, 'click', _refreshClickHandler);
            this._buttons.push(refreshLink);
            this._buttonClickHandlers.push(_refreshClickHandler);

            div.appendChild(document.createTextNode('\u00a0' + resx.Global_Of + ' '));
            this._pageCountLabel = document.createTextNode(this._pageCount);
            div.appendChild(this._pageCountLabel);
            div.appendChild(document.createTextNode(' '));

            //Next/Last Buttons - always wire up in case pagination changes due to expand/collapse operations
            var nextLink = d.ce('a');
            div.appendChild(nextLink);
            nextLink.title = resx.Pagination_Next;
            var glyph = d.ce('span');
            glyph.className = 'fas fa-forward ml2';
            nextLink.appendChild(glyph);
            if (this._currPage < this._pageCount)
            {
                nextLink.style.cursor = 'pointer';
                var _nextClickHandler = Function.createCallback(this._onNextClick, { thisRef: this });
                $addHandler(nextLink, 'click', _nextClickHandler);
                this._buttons.push(nextLink);
                this._buttonClickHandlers.push(_nextClickHandler);
            }
            else
            {
                //Make the button look non-functional
                nextLink.className = 'aspNetDisabled';
            }
            div.appendChild(document.createTextNode(' '));
            this._nextLink = nextLink;
            nextLink = d.ce('a');
            div.appendChild(nextLink);
            nextLink.title = resx.Pagination_Last;
            var glyph = d.ce('span');
            glyph.className = 'fas fa-fast-forward ml2';
            nextLink.appendChild(glyph);
            if (this._currPage < this._pageCount)
            {
                nextLink.style.cursor = 'pointer';
                var _lastClickHandler = Function.createCallback(this._onLastClick, { thisRef: this });
                $addHandler(nextLink, 'click', _lastClickHandler);
                this._buttons.push(nextLink);
                this._buttonClickHandlers.push(_lastClickHandler);
            }
            else
            {
                //Make the button look non-functional
                nextLink.className = 'aspNetDisabled';
            }

            //Get the list of pagination options (used to be a separate ajax call)
            var context = {};
            context.callback = callback;
            context.table = table;
            context.tBody = tBody;
            context.thead = thead;
            this.retreivePagination_success(dataSource.paginationOptions, context);
        }
    },
    retreivePagination_success: function (paginationList, context)
    {
        //Populate the dropdown with pagination options
        var dropdown = this._ddlPaginationOptions;

        for (var itemKey in paginationList)
        {
            //Need to remove the prefix added by the serializer
            var item = itemKey.substring(1);
            dropdown.addItem(item, item);
            if (paginationList[itemKey] == 'true')
            {
                dropdown.set_selectedItem(item);
            }
        }

        //Call the end of data-binding event handler and mark load as complete
        this._loadingComplete = true;
        this._dataBind_Complete(context.callback, context.table, context.tBody, context.thead);
    },
    _onPaginationOptionsChanged: function (e)
    {
        //Make sure the load is completed (as dropdown is also changed on initial load)
        if (this._loadingComplete == true)
        {
            //Change the number of rows displayed per page
            var dropdown = this._ddlPaginationOptions;
            var itemsPerPage = dropdown.get_selectedItem().get_value();

            //Now call the webservice to update the pagination options and reload the data
            var webServiceClass = this.get_webServiceClass();
            webServiceClass.UpdatePagination(this.get_projectId(), itemsPerPage, -1, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    _onChangePageKeyDown: function (evt, e)
    {
        //If enter was clicked change the page
        var keynum = evt.keyCode | evt.which;
        if (keynum == 13)
        {
            e.thisRef._onChangePageClick(evt, e);
        }
    },
    _onChangePageClick: function (sender, e)
    {
        //Get the entered page number
        var pageNumber = parseInt(e.textbox.value);
        if (!isNaN(pageNumber))
        {
            //Now call the webservice to update the pagination and reload the data
            var webServiceClass = e.thisRef.get_webServiceClass();
            webServiceClass.UpdatePagination(e.thisRef.get_projectId(), -1, pageNumber, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
        }
    },
    _onFirstClick: function (sender, e)
    {
        //Go to the first page
        var pageNumber = 1;

        //Now call the webservice to update the pagination and reload the data
        var webServiceClass = e.thisRef.get_webServiceClass();
        webServiceClass.UpdatePagination(e.thisRef.get_projectId(), -1, pageNumber, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
    },
    _onLastClick: function (sender, e)
    {
        //Go to the last page
        var pageNumber = e.thisRef._pageCount;

        //Now call the webservice to update the pagination and reload the data
        var webServiceClass = e.thisRef.get_webServiceClass();
        webServiceClass.UpdatePagination(e.thisRef.get_projectId(), -1, pageNumber, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
    },
    _onPrevClick: function (sender, e)
    {
        //Go to the previous page
        if (e.thisRef._currPage > 1)
        {
            var pageNumber = e.thisRef._currPage - 1;

            //Now call the webservice to update the pagination and reload the data
            var webServiceClass = e.thisRef.get_webServiceClass();
            webServiceClass.UpdatePagination(e.thisRef.get_projectId(), -1, pageNumber, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
        }
    },
    _onNextClick: function (sender, e)
    {
        //Go to the next page
        if (e.thisRef._currPage < e.thisRef._pageCount)
        {
            var pageNumber = e.thisRef._currPage + 1;

            //Now call the webservice to update the pagination and reload the data
            var webServiceClass = e.thisRef.get_webServiceClass();
            webServiceClass.UpdatePagination(e.thisRef.get_projectId(), -1, pageNumber, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
        }
    },
    _onSubHeaderKeyDown: function (evt, e)
    {
        //If enter was clicked apply current filter
        var keynum = evt.keyCode | evt.which;
        if (keynum == 13)
        {
            e.thisRef.apply_filters();
            // stop the event bubble
            evt.preventDefault();
            evt.stopPropagation();
        }
    },
    _onTableRowContextMenu: function (evt, e)
    {
        //Prevent the normal context menu displaying
        evt.cancelBubble = true;
        evt.stopPropagation();
        evt.preventDefault();

        var tagName = evt.target.tagName.toLowerCase();
        if (tagName == 'img')
        {
            //Might be touch dragging
            return false;
        }

        //Activate the context menu
        if (e.thisRef._contextMenu == null)
        {
            var div = d.ce('div');
            div.className = 'ContextMenu';
            div.style.width = '150px';
            e.thisRef._contextMenu = $create(Inflectra.SpiraTest.Web.ServerControls.ContextMenu, { items: e.thisRef._contextMenuItems, parentControlId: e.thisRef.get_element().id, themeFolder: e.thisRef._themeFolder }, null, null, div);
            document.body.appendChild(div);
        }
        e.thisRef._contextMenu.display(evt, e.primaryKey);
        return false;
    },
    _onTableRowDblClick: function (evt, e)
    {
        //Make sure we have a valid primary key
        if (parseInt(e.primaryKey) >= 0)
        {
            //If the div, td or tr was clicked then make the row editable
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
            var tname;
            tname = targ.nodeName;
            if ((tname == 'TR' || tname == 'TD' || tname == 'DIV') && e.tr.className != e.thisRef.get_editRowCssClass())
            {
                //Fire the edit event
                e.thisRef._onEditClick(evt, e);
            }
        }
    },
    _onTableRowKeyDown: function (evt, e)
    {
        //If enter was clicked and we're in an editing mode
        var keynum = evt.keyCode | evt.which;
        if (e.thisRef._isInEdit)
        {
            if (keynum == 13)
            {
                //Fire update if ENTER pressed
                e.thisRef._onUpdateClick(evt, e);
                // stop the event bubble
                evt.preventDefault();
                evt.stopPropagation();
            }
            if (keynum == 27)
            {
                //Fire cancel if ESCAPE pressed
                e.thisRef._onCancelClick(evt, e);
                // stop the event bubble
                evt.preventDefault();
                evt.stopPropagation();
            }
        }
    },
    _dataBind_Complete: function (callback, table, tBody, thead)
    {
        //Delete the existing content of the table if there is any
        this._clearContent(table);

        //Add the new header and body to the table
        table.appendChild(thead);
        table.appendChild(tBody);

        //Display the counts if the display controls exist
        if (this.get_totalCountControlId() && this.get_totalCountControlId() != '')
        {
            var totalCountSpan = $get(this.get_totalCountControlId());
            if (totalCountSpan)
            {
                totalCountSpan.innerHTML = this._totalCount + ' ';
            }
        }
        if (this.get_visibleCountControlId() && this.get_visibleCountControlId() != '')
        {
            var visibleCountSpan = $get(this.get_visibleCountControlId());
            if (visibleCountSpan)
            {
                visibleCountSpan.innerHTML = this._visibleCount + ' ';
            }
        }
        //Finally call the callback function
        if (callback != undefined)
        {
            callback();
        }
    },
    _dataBindRow: function (dataRow, columns, table, tBody)
    {
        var tr;
        var td;
        var a;
        var img;
        var className;
        var dataRow;
        var editCallback;
        var checkbox;
        var span;
        var div;

        //Figure out the class to display
        className = this.get_rowCssClass();

        //Store the concurrency value if necessary
        if (this._concurrencyEnabled && this._concurrencyValues && dataRow.concurrencyValue)
        {
            this._concurrencyValues[dataRow.primaryKey] = dataRow.concurrencyValue;
        }

        tr = d.ce('tr');
        tr.setAttribute('role', 'row');
        tr.setAttribute('tst:primarykey', dataRow.primaryKey);
        if (dataRow.Fields.Name && dataRow.Fields.Name.textValue && dataRow.Fields.Name.textValue != '')
        {
            tr.setAttribute('aria-label', dataRow.Fields.Name.textValue);
        }
        tBody.appendChild(tr);
        this._tableRows.push(tr);
        if ((className != null) && (className.length > 0))
        {
            tr.className = className;
        }
        this._addTableRowHandlers(tr, dataRow.primaryKey);

        //Hook up an event handlers for dragging if necessary
        if (this._allowDragging)
        {
            //Attach the behavior to the row
            var dragSource = $create(Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior, { 'primaryKey': dataRow.primaryKey, 'parent': this }, null, null, tr);

            tr.style.cursor = 'move';
            var rowMouseDownHandler = Function.createCallback(this._onRowMouseDown, { thisRef: this, tr: tr, primaryKey: dataRow.primaryKey, dragSource: dragSource });
            var rowMouseUpHandler = Function.createCallback(this._onRowMouseUp, { thisRef: this, tr: tr });
            var rowTouchMoveHandler = Function.createCallback(this._onRowTouchMove, { thisRef: this, tr: tr, primaryKey: dataRow.primaryKey, dragSource: dragSource });
            $addHandler(tr, 'mousedown', rowMouseDownHandler);
            $addHandler(tr, 'mouseup', rowMouseUpHandler);
            if (tr.addEventListener) //IE8 doesn't support
            {
                tr.addEventListener('touchmove', rowTouchMoveHandler, false);
            }
            this._dragElements.push(tr);
            this._dragDownHandlers.push(rowMouseDownHandler);
            this._dragUpHandlers.push(rowMouseUpHandler);
            this._touchHandlers.push(rowTouchMoveHandler);
        }

        //Need to add the checkbox and attachments columns
        if (this._displayCheckboxes)
        {
            td = d.ce('td');
            td.className = 'Checkbox priority3';
            tr.appendChild(td);
            checkbox = d.ce('input');
            checkbox.type = 'checkbox';
            td.appendChild(checkbox);
            if (parseInt(dataRow.primaryKey) < 0 && this._negativePrimaryKeysDisabled)
            {
                //Mark as disabled
                checkbox.disabled = true;
            }
            else
            {
                checkbox.name = 'chkItem' + dataRow.primaryKey;
                checkbox.setAttribute('tst:controlid', this.get_element().id);
                checkbox.setAttribute('tst:primarykey', dataRow.primaryKey);
                //Add the checkbox handler
                var _checkboxChangedHandler = Function.createCallback(this._onCheckboxChanged, { thisRef: this, tr: tr, checkbox: checkbox });
                $addHandler(checkbox, 'click', _checkboxChangedHandler);
                this._clickElements.push(checkbox);
                this._otherClickHandlers.push(_checkboxChangedHandler);
            }
        }
        if (this._displayAttachments)
        {
            td = d.ce('td');
            td.className = 'Attachment priority3';
            tr.appendChild(td);
            if (dataRow.attachment)
            {
                img = d.ce('span');
                img.className = 'fas fa-paperclip';
                img.setAttribute('tooltip', resx.Global_ArtifactHasAttachment);
                img.title = resx.Global_ArtifactHasAttachment;
                td.appendChild(img);
            }
        }

        for (var j = 0; j < columns.length; j++)
        {
            var column = columns[j];

            td = d.ce('td');
            tr.appendChild(td);
            //Apply any selective formatting (colors begin with #, class names don't)
            var classOrColor = dataRow.Fields[column.dataField].cssClass;
            if (classOrColor && classOrColor.length > 0)
            {
                if (classOrColor.substr(0, 1) == '#')
                {
                    td.style.backgroundColor = classOrColor;
                }
                else
                {
                    td.className = classOrColor;
                }
            }

            //We add a div inside the TD so that we can make the widths fixed
            var tdDiv = d.ce('div');

            if (this._allowColumnPositioning)
            {
                tdDiv.className = 'fixed-width';
            }

            if (column.width)
            {
                tdDiv.style.width = column.width + 'px';
            }
            td.appendChild(tdDiv);

            //If we have a name/description column we need to add the various images and buttons
            if (column.fieldType == globalFunctions._fieldType_nameDescription)
            {
                //Set the id of the row
                td.className = 'priority1';
                td.id = 'tdName_' + dataRow.primaryKey;
                //First the various icons and buttons
                var imgSrc = '';
                var imgAlt = '';
                if (this.get_itemImage() != '')
                {
                    //This is the case where we have a specified image for all rows
                    imgSrc = (dataRow.folder) ? this.get_folderItemImage() : ((dataRow.alternate) ? this.get_alternateItemImage() : this.get_itemImage());
                }
                if (imgSrc == '' && (column.dataField != null) && (column.dataField.length > 0))
                {
                    //This is the case when the icon is specified per row
                    if (dataRow.Fields[column.dataField].tooltip)
                    {
                        imgSrc = dataRow.Fields[column.dataField].tooltip;
                    }
                }

                if (imgSrc != '')
                {
                    var img = d.ce('img');
                    img.id = this.get_elementId() + '_icon_' + dataRow.primaryKey;
                    img.alt = imgAlt;
                    img.className = "w4 h4 mr3";
                    img.src = this._themeFolder + 'Images/' + imgSrc;
                    tdDiv.appendChild(img);
                    tdDiv.appendChild(d.ctn(' '));
                }
            }

            if ((column.dataField != null) && (column.dataField.length > 0))
            {
                //For dates, decimals, multilists and time intervals, we need to display a tooltip containing more detailed information
                if (column.fieldType == globalFunctions._fieldType_dateTime || column.fieldType == globalFunctions._fieldType_timeInterval ||
                    column.fieldType == globalFunctions._fieldType_decimal || column.fieldType == globalFunctions._fieldType_customPropertyMultiList ||
                    column.fieldType == globalFunctions._fieldType_customPropertyDate || column.fieldType == globalFunctions._fieldType_multiList)
                {
                    if (dataRow.Fields[column.dataField].tooltip)
                    {
                        //Add the tooltip handler
                        var _tdMouseOverHandler = Function.createCallback(this._onTdMouseOver, { tooltip: dataRow.Fields[column.dataField].tooltip });
                        var _tdMouseOutHandler = Function.createCallback(this._onTdMouseOut, { tooltip: '' });
                        $addHandler(td, 'mouseover', _tdMouseOverHandler);
                        $addHandler(td, 'mouseout', _tdMouseOutHandler);
                        this._tooltips.push(td);
                        this._tooltipOverHandlers.push(_tdMouseOverHandler);
                        this._tooltipOutHandlers.push(_tdMouseOutHandler);
                    }
                }

                //Set the display priority for ID fields
                if (column.fieldType == globalFunctions._fieldType_identifier)
                {
                    td.className = 'priority4';
                }
                //Need to handle the HTML and equalizer columns separately (need to check the row since it may vary by row)
                if (column.fieldType == globalFunctions._fieldType_html || dataRow.Fields[column.dataField].fieldType == globalFunctions._fieldType_html)
                {
                    //We just insert the provided HTML as 'inner' html
                    //replacing any theme constants if necessary
                    var markup = dataRow.Fields[column.dataField].textValue;
                    if (markup)
                    {
                        markup = markup.replace('[SPIRA_THEME]', this._themeFolder);
                        tdDiv.innerHTML = markup;
                        //Clear any embedded events (to prevent XSS attacks)
                        if (!dataRow.allowScript)
                        {
                            globalFunctions.cleanHtml(tdDiv);
                        }
                    }
                    else
                    {
                        tdDiv.innerHTML = '';
                    }
                }
                else if (column.fieldType == globalFunctions._fieldType_equalizer)
                {
                    //Add the tooltip handler
                    td.className = 'equalizer priority2';
                    var _tdMouseOverHandler = Function.createCallback(this._onTdMouseOver, { tooltip: dataRow.Fields[column.dataField].tooltip });
                    var _tdMouseOutHandler = Function.createCallback(this._onTdMouseOut, { tooltip: '' });
                    $addHandler(td, 'mouseover', _tdMouseOverHandler);
                    $addHandler(td, 'mouseout', _tdMouseOutHandler);
                    this._tooltips.push(td);
                    this._tooltipOverHandlers.push(_tdMouseOverHandler);
                    this._tooltipOutHandlers.push(_tdMouseOutHandler);

                    if (dataRow.Fields[column.dataField].textValue == null || dataRow.Fields[column.dataField].textValue == '')
                    {
                        //Populate the equalizer bar
                        var dataField = dataRow.Fields[column.dataField];
                        div = d.ce('div');
                        div.style.width = '110px';
                        tdDiv.appendChild(div);
                        if (dataField.equalizerGreen)
                        {
                            span = d.ce('span');
                            span.className = 'EqualizerGreen';
                            if (typeof table.style.MozUserSelect != 'undefined')
                            {
                                span.style.paddingLeft = dataField.equalizerGreen + "px";
                            }
                            else
                            {
                                span.style.width = dataField.equalizerGreen + "px";
                            }
                            div.appendChild(span);
                        }

                        if (dataField.equalizerRed)
                        {
                            span = d.ce('span');
                            span.className = 'EqualizerRed';
                            if (typeof table.style.MozUserSelect != 'undefined')
                            {
                                span.style.paddingLeft = dataField.equalizerRed + "px";
                            }
                            else
                            {
                                span.style.width = dataField.equalizerRed + "px";
                            }
                            div.appendChild(span);
                        }

                        if (dataField.equalizerOrange)
                        {
                            span = d.ce('span');
                            span.className = 'EqualizerOrange';
                            if (typeof table.style.MozUserSelect != 'undefined')
                            {
                                span.style.paddingLeft = dataField.equalizerOrange + "px";
                            }
                            else
                            {
                                span.style.width = dataField.equalizerOrange + "px";
                            }
                            div.appendChild(span);
                        }

                        if (dataField.equalizerYellow)
                        {
                            span = d.ce('span');
                            span.className = 'EqualizerYellow';
                            if (typeof table.style.MozUserSelect != 'undefined')
                            {
                                span.style.paddingLeft = dataField.equalizerYellow + "px";
                            }
                            else
                            {
                                span.style.width = dataField.equalizerYellow + "px";
                            }
                            div.appendChild(span);
                        }

                        if (dataField.equalizerGray)
                        {
                            span = d.ce('span');
                            span.className = 'EqualizerGray';
                            if (typeof table.style.MozUserSelect != 'undefined')
                            {
                                span.style.paddingLeft = dataField.equalizerGray + "px";
                            }
                            else
                            {
                                span.style.width = dataField.equalizerGray + "px";
                            }
                            div.appendChild(span);
                        }
                    }
                    else
                    {
                        //Just set it to the provided class
                        td.className = 'equalizer priority2 ' + dataRow.Fields[column.dataField].cssClass;
                        tdDiv.appendChild(d.createTextNode(dataRow.Fields[column.dataField].textValue));
                    }
                }
                else
                {
                    //We will only dump the data if datafield is specifed,
                    //DataField might be empty if someone wants to put the
                    //value in rowBound event. For example a calculated value, lookup value et.
                    var value = dataRow.Fields[column.dataField].textValue;

                    //If we have a flag, need to convert Y/N to Yes/No
                    if (column.fieldType == globalFunctions._fieldType_flag)
                    {
                        if (value == 'Y')
                        {
                            value = 'Yes';
                        }
                        if (value == 'N')
                        {
                            value = 'No';
                        }
                    }

                    if (value != null)
                    {
                        //For text longer than 50 characters add a tooltip
                        if (value.length > 50)
						{
							var tooltip = globalFunctions.htmlEncode(value);
							var _tdMouseOverHandler = Function.createCallback(this._onTdMouseOver, { tooltip: tooltip });
                            var _tdMouseOutHandler = Function.createCallback(this._onTdMouseOut, { tooltip: '' });
                            $addHandler(td, 'mouseover', _tdMouseOverHandler);
                            $addHandler(td, 'mouseout', _tdMouseOutHandler);
                            this._tooltips.push(td);
                            this._tooltipOverHandlers.push(_tdMouseOverHandler);
                            this._tooltipOutHandlers.push(_tdMouseOutHandler);
                        }

                        //Name fields and hierarchy dropdowns are hyperlinks, others are just text
                        if (column.fieldType == globalFunctions._fieldType_nameDescription)
                        {
                            //Handle when the name field is blank (can happen as of 6.1 when creating new items and then clicking away from page without properly saving)
                            if (value == "")
                            {
                                value = resx.Global_None2;
                            }
                            a = d.ce('a');
                            a.appendChild(d.ctn(value));
                            if (dataRow.customUrl && dataRow.customUrl != '')
                            {
                                //Special case where the service provides a row-specific URL
                                a.href = dataRow.customUrl;
                            }
                            else if (dataRow.folder)
                            {
                                //Folder clicks are handled by events
                                if (this._folderUrlTemplate && this._folderUrlTemplate != '')
                                {
                                    a.href = this._folderUrlTemplate.replace(globalFunctions.artifactIdToken, '' + (-dataRow.primaryKey));
                                }
                                else
                                {
                                    a.href = 'javascript:void(0)';
                                }
                                var folderClickHandler = Function.createCallback(this._onFolderClick, { thisRef: this, primaryKey: dataRow.primaryKey });
                                $addHandler(a, 'click', folderClickHandler);
                                this._clickElements.push(a);
                                this._otherClickHandlers.push(folderClickHandler);

                            }
                            else
                            {
                                //make sure the primary key is made positive so URLs work properly
                                var primaryKeyForUrl = dataRow.primaryKey < 0 ? (dataRow.primaryKey * -1) : dataRow.primaryKey;
                                a.href = this.get_baseUrl().replace(globalFunctions.artifactIdToken, '' + primaryKeyForUrl);
                            }
                            if (this._baseUrlTarget != '')
                            {
                                a.target = this._baseUrlTarget;
                            }
                            tdDiv.appendChild(a);

                            //Display any count information
                            if (dataRow.childCount && dataRow.childCount > 0)
                            {
                                span = d.ce('span');
                                span.className = 'badge';
                                span.appendChild(d.createTextNode(dataRow.childCount))
                                tdDiv.appendChild(d.createTextNode(' '));
                                tdDiv.appendChild(span);
                            }

                            //Add the asynchronous tooltip handler
                            if (this.get_displayTooltip())
                            {
                                var _nameDescMouseOverHandler = Function.createCallback(this._onNameDescMouseOver, { thisRef: this, primaryKey: dataRow.primaryKey, webServiceClass: this.get_webServiceClass() });
                                var _nameDescMouseOutHandler = Function.createCallback(this._onNameDescMouseOut, { thisRef: this, primaryKey: -1, webServiceClass: '' });
                                $addHandler(a, 'mouseover', _nameDescMouseOverHandler);
                                $addHandler(a, 'mouseout', _nameDescMouseOutHandler);
                                this._tooltips.push(a);
                                this._tooltipOverHandlers.push(_nameDescMouseOverHandler);
                                this._tooltipOutHandlers.push(_nameDescMouseOutHandler);
                            }
                        }
                        else if (column.fieldType == globalFunctions._fieldType_hierarchyLookup)
                        {
                            var urlSuffix = dataRow.Fields[column.dataField].intValue;
                            a = d.ce('a');
                            a.appendChild(d.ctn(value));
                            a.href = globalFunctions.getArtifactDefaultUrl(this._appPath, this._projectId, column.dataField, urlSuffix);
                            tdDiv.appendChild(a);
                        }
                        else if ((column.fieldType == globalFunctions._fieldType_text || column.fieldType == globalFunctions._fieldType_identifier) && dataRow.Fields[column.dataField].tooltip)
                        {
                            //If we are a text/ID field and have a tooltip, it should be used as a url instead
                            a = d.ce('a');
                            a.appendChild(d.ctn(value));
                            a.href = dataRow.Fields[column.dataField].tooltip;
                            tdDiv.appendChild(a);
                        }
                        else
                        {
                            tdDiv.appendChild(d.ctn(value));
                        }
                    }
                }

                //Set any custom css classes
                if (this._customCssClasses && this._customCssClasses[globalFunctions.keyPrefix + column.dataField] && this._customCssClasses[globalFunctions.keyPrefix + column.dataField] != '')
                {
                    var className = this._customCssClasses[globalFunctions.keyPrefix + column.dataField];
                    if (td.classList && td.className != '')
                    {
                        td.className += ' ' + className;
                    }
                    else
                    {
                        td.className = className;
                    }
                }
            }
        }

        //Need to show the edit button
        if (this._allowEdit)
        {
            td = d.ce('td');
            tr.appendChild(td);
            td.className = 'priority3';

            //Make sure this rows supports editing (has positive primary key)
            if (parseInt(dataRow.primaryKey) >= 0)
            {
                var btn = d.ce('button');
                btn.type = "button";
                btn.className = 'btn btn-default';
                btn.appendChild(d.createTextNode(resx.Global_Edit));
                td.appendChild(btn);

                var _editClickHandler = Function.createCallback(this._onEditClick, { thisRef: this, primaryKey: dataRow.primaryKey, tr: tr });
                $addHandler(btn, 'click', _editClickHandler);
                this._buttons.push(btn);
                this._buttonClickHandlers.push(_editClickHandler);
                btn.style.cursor = 'pointer';
            }
        }
    },

    select: function (index)
    {
        var e = Function._validateParams(arguments, [{ name: 'index', type: Number}]);
        if (e) throw e;

        //No need to processed further if we dont have any data
        if ((this.get_dataRows() == null) || (this.get_dataRows().length == 0))
        {
            return;
        }

        //Ensure the index is valid
        if ((index < 0) || (index > (this._tableRows.length - 1)))
        {
            throw Error.argumentOutOfRange('index', index, 'Specfied index is out of range.');
        }

        //No need to proceed if the current selected index and new index is same
        if (this._selectedIndex == index)
        {
            return;
        }

        //Now clear the exisiting selection
        this.resetSelection();

        //The Select class will be if only if it is mentioned
        if ((this.get_selectedRowCssClass() != null) && (this.get_selectedRowCssClass().length > 0))
        {
            this.get_dataRows()[index].className = this.get_selectedRowCssClass();
        }

        //Store the new inded
        this._selectedIndex = index;
        //Since the index has changed raise the property change event
        this.raisePropertyChanged('selectedIndex');
    },

    resetSelection: function ()
    {
        //Ensure that this method is called without any parameter
        if (arguments.length !== 0) throw Error.parameterCount();

        //No need to processed further if we dont have any data
        if ((this.get_dataRows() == null) || (this.get_dataRows().length == 0))
        {
            return;
        }

        var className = '';
        var tr;

        //Now Revert back the datarows styles with the original state
        for (var i = 0; i < this.get_dataRows().length; i++)
        {
            tr = this.get_dataRows()[i];

            //Figuring out the proper class
            className = this.get_rowCssClass();
            if ((className != null) && (className.length > 0))
            {
                tr.className = className;
            }
        }

        //reseting the index
        this._selectedIndex = -1;
        //Raise the Property change event
        this.raisePropertyChanged('selectedIndex');
    },

    getColumnIndex: function (columnHeaderText)
    {
        //Argument validation
        var e = Function._validateParams(arguments, [{ name: 'columnHeaderText', type: String}]);
        if (e) throw e;

        //Since we are supporing drag n drop it is not possible
        //that the index of the column will be static,
        //that is way we need a helper function which will
        //resolve the proper index based upon the header text.
        //****Warning: if Same header is used for more than one column
        //it will only return the first

        if (this._headerRow != null)
        {
            var columns = this.get_columns();

            if ((columns != null) && (columns.length > 0))
            {
                var column;

                for (var i = 0; i < columns.length; i++)
                {
                    column = columns[i];

                    if (column.headerText == columnHeaderText)
                    {
                        //Header text match.
                        //now check the header index form the header row;
                        for (var j = 0; j < this._headerRow.childNodes.length; j++)
                        {
                            if (this._headerRow.childNodes[j] == column.$header)
                            {
                                return j;
                            }
                        }
                    }
                }
            }
        }

        return -1;
    },

    add_columnDragStart: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('columnDragStart', handler);
    },
    remove_columnDragStart: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('columnDragStart', handler);
    },

    add_columnDropped: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('columnDropped', handler);
    },

    remove_columnDropped: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('columnDropped', handler);
    },

    add_rowDataBound: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('rowDataBound', handler);
    },

    remove_rowDataBound: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('rowDataBound', handler);
    },

    add_selectedIndexChange: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('selectedIndexChange', handler);
    },

    remove_selectedIndexChange: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('selectedIndexChange', handler);
    },

    add_rowDelete: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('rowDelete', handler);
    },
    remove_rowDelete: function (handler)
    {
        var e = Function._validateParams(arguments, [{ name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('rowDelete', handler);
    },

    _clearKeyDownHandlers: function ()
    {
        //The various key down handlers
        for (var i = 0; i < this._keyDownElements.length; i++)
        {
            $removeHandler(this._keyDownElements[i], 'keydown', this._keyDownHandlers[i]);
            delete this._keyDownHandlers[i];
            delete this._keyDownElements[i];
        }
        this._keyDownElements = new Array();
        this._keyDownHandlers = new Array();
    },
    _addTableRowHandlers: function (tr, primaryKey)
    {
        //Add the keypress and click handlers for editing the row
        if (this._allowEdit)
        {
            var tableRowDblClickHandler = Function.createCallback(this._onTableRowDblClick, { thisRef: this, tr: tr, primaryKey: primaryKey });
            var tableRowKeyDownHandler = Function.createCallback(this._onTableRowKeyDown, { thisRef: this, tr: tr, primaryKey: primaryKey });
            var tableRowContextMenuHandler = Function.createCallback(this._onTableRowContextMenu, { thisRef: this, tr: tr, primaryKey: primaryKey });
            $addHandler(tr, 'dblclick', tableRowDblClickHandler);
            $addHandler(tr, 'keydown', tableRowKeyDownHandler);
            $addHandler(tr, 'contextmenu', tableRowContextMenuHandler);
            this._tableRowDblClickHandlers.push(tableRowDblClickHandler);
            this._tableRowKeyDownHandlers.push(tableRowKeyDownHandler);
            this._tableRowContextMenuHandlers.push(tableRowContextMenuHandler);
            this._tableEditRows.push(tr);
        }
    },
    _clearTableRowHandlers: function ()
    {
        if (this._allowEdit)
        {
            //The various table row handlers
            for (var i = 0; i < this._tableEditRows.length; i++)
            {
                $removeHandler(this._tableEditRows[i], 'dblclick', this._tableRowDblClickHandlers[i]);
                $removeHandler(this._tableEditRows[i], 'keydown', this._tableRowKeyDownHandlers[i]);
                $removeHandler(this._tableEditRows[i], 'contextmenu', this._tableRowContextMenuHandlers[i]);
                delete this._tableRowDblClickHandlers[i];
                delete this._tableRowKeyDownHandlers[i];
                delete this._tableRowContextMenuHandlers[i];
            }
            this._tableRowDblClickHandlers = new Array();
            this._tableRowKeyDownHandlers = new Array();
            this._tableRowContextMenuHandlers = new Array();
            this._tableEditRows = new Array();
        }
    },
    _clearOtherHandlers: function ()
    {
        if (this._paginationOptionsChangedHandler != null)
        {
            delete this._paginationOptionsChangedHandler;
        }
        if (this._ddlPaginationOptions != null)
        {
            delete this._ddlPaginationOptions;
        }
    },
    _clearDragHandlers: function ()
    {
        //The header drag handlers
        for (var i = 0; i < this._headers.length; i++)
        {
            $removeHandler(this._headers[i], 'mousedown', this._headerDragHandlers[i]);
            delete this._headerDragHandlers[i];
            delete this._headers[i];
        }

        //The row drag handlers
        for (var i = 0; i < this._dragElements.length; i++)
        {
            $removeHandler(this._dragElements[i], 'mousedown', this._dragDownHandlers[i]);
            $removeHandler(this._dragElements[i], 'mouseup', this._dragUpHandlers[i]);
            if (this._dragElements[i].removeEventListener) //IE8 doesn't support
            {
                this._dragElements[i].removeEventListener('touchmove', this._touchHandlers[i], false);
            }
            delete this._dragDownHandlers[i];
            delete this._dragUpHandlers[i];
            delete this._touchHandlers[i];
        }

        this._dragElements = new Array();
        this._headers = new Array();
        this._headerDragHandlers = new Array();
        this._dragDownHandlers = new Array();
        this._dragUpHandlers = new Array();
        this._touchHandlers = new Array();
    },
    _clearButtonHandlers: function ()
    {
        for (var i = 0; i < this._buttons.length; i++)
        {
            $removeHandler(this._buttons[i], 'click', this._buttonClickHandlers[i]);
            delete this._buttons[i];
            delete this._buttonClickHandlers[i];
        }

        this._buttons = new Array();
        this._buttonClickHandlers = new Array();
    },
    _clearClickElementHandlers: function ()
    {
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._otherClickHandlers[i]);
            delete this._clickElements[i];
            delete this._otherClickHandlers[i];
        }

        this._clickElements = new Array();
        this._otherClickHandlers = new Array();
    },
    _clearTooltipHandlers: function ()
    {
        for (var i = 0; i < this._tooltips.length; i++)
        {
            $removeHandler(this._tooltips[i], 'mouseover', this._tooltipOverHandlers[i]);
            $removeHandler(this._tooltips[i], 'mouseout', this._tooltipOutHandlers[i]);
            delete this._tooltipOverHandlers[i];
            delete this._tooltipOutHandlers[i];
        }

        this._tooltips = new Array();
        this._tooltipOverHandlers = new Array();
        this._tooltipOutHandlers = new Array();
    },

    _onCheckboxChanged: function (evt, e)
    {
        if (e.tr.className != e.thisRef._editRowCssClass)
        {
            //Don't highlight if in edit mode
            e.tr.className = (e.checkbox.checked) ? e.thisRef.get_selectedRowCssClass() : e.thisRef.get_rowCssClass();
        }

        var element;
        var i;
        var frm = document.forms[0];
        if (evt.shiftKey)
        {
            //Need to select all the rows between the selection and the next checkbox
            var foundChecked = 0;
            for (i = 0; i < frm.length; i++)
            {
                element = frm.elements[i];
                if (element.type == 'checkbox' && element.name.indexOf('chkItem') != -1)
                {
                    //If we find the first checked item set the flag
                    if (element.checked && foundChecked == 0)
                    {
                        //Distinguish between the recently clicked one and others
                        if (element.name == e.checkbox.name)
                        {
                            foundChecked = 2;
                        }
                        else
                        {
                            foundChecked = 1;
                        }
                    }
                    else
                    {
                        //Stop at the next already checked one
                        if (element.checked && foundChecked == 1 && element.name == e.checkbox.name)
                        {
                            foundChecked = 3;   //Stop
                        }
                        if (element.checked && foundChecked == 2 && element.name != e.checkbox.name)
                        {
                            foundChecked = 3;   //Stop
                        }

                        //Check any that need it
                        if (foundChecked > 0 && foundChecked < 3)
                        {
                            element.checked = true;
                        }
                    }
                }
            }
        }

        if (evt.ctrlKey || evt.metaKey)
        {
            //Deselect all other checkboxes
            for (i = 0; i < frm.length; i++)
            {
                element = frm.elements[i];
                if (element.type == 'checkbox' && element.name.indexOf('chkItem') != -1)
                {
                    if (element.name != e.checkbox.name)
                    {
                        element.checked = false;
                    }
                }
            }
        }

        //Change the select all checkbox
        var boolAllChecked = true;
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            if (element.type == 'checkbox' && element.name.indexOf('chkItem') != -1)
            {
                if (!element.checked)
                {
                    boolAllChecked = false;
                    break;
                }
            }
        }
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            if (element.type == 'checkbox' && element.name == 'chkHead')
            {
                element.checked = boolAllChecked;
                break;
            }
        }
    },

    _onFolderClick: function(evt, e)
    {
        //If we have a CTRL/SHIFT click and a URL is available, just let the browser open the new tab
        var newTab = false;
        if (evt && e.thisRef._folderUrlTemplate && e.thisRef._folderUrlTemplate != '') {
            if (evt.shiftKey || evt.ctrlKey || evt.metaKey) {
                newTab = true;
            }
        }
        if (!newTab) {
            //Stop the normal URL firing if it might do
            evt.stopPropagation();
            evt.preventDefault();

            //Update the URL to reflect the change in folder, if the page supports it
            if (e.thisRef._folderUrlTemplate && e.thisRef._folderUrlTemplate != '' && history && history.pushState) {
                var folderId = -e.primaryKey;
                var href = e.thisRef._folderUrlTemplate.replace(globalFunctions.artifactIdToken, '' + folderId);
                history.pushState(folderId, null, href);

                //Update the standard filter
                e.thisRef._standardFilters = {};
                e.thisRef._standardFilters['_FolderId'] = globalFunctions.serializeValueInt(folderId);
            }

            //Select this folder
            e.thisRef.focus_on(e.primaryKey);
        }
    },

    _onCheckboxSelectAllChanged: function (sender, e)
    {
        var chkState = e.checkbox.checked;
        var frm = document.forms[0];
        var i;
        var element;
        //Mark all the checkboxes
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            if (element.type == 'checkbox' && element.name.indexOf('chkItem') != -1 && !element.disabled)
            {
                element.checked = chkState;
            }
        }
        //Now mark all the rows
        var table = e.thisRef.get_element();
        for (i = 0; i < table.rows.length; i++)
        {
            if (table.rows[i].className == e.thisRef.get_rowCssClass() && chkState)
            {
                table.rows[i].className = e.thisRef.get_selectedRowCssClass();
            }
            if (table.rows[i].className == e.thisRef.get_selectedRowCssClass() && !chkState)
            {
                table.rows[i].className = e.thisRef.get_rowCssClass();
            }
        }
    },

    _onSortClick: function (sender, e)
    {
        //Now call the webservice to update the sort and reload the data
        var webServiceClass = e.thisRef.get_webServiceClass();
        webServiceClass.SortedList_UpdateSort(e.thisRef.get_projectId(), e.sortProperty, e.sortAscending, e.thisRef._displayTypeId, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
    },

    toggle_visibility: function (select)
    {
        if (select.get_value() != '')
        {
            var fieldName = select.get_value();
            //Call the appropriate webservice to toggle the visibility
            var webServiceClass = this.get_webServiceClass();
            this.display_spinner();
            webServiceClass.ToggleColumnVisibility(this.get_projectId(), fieldName, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));

            //Now reset the dropdown and change show to hide or vice versa
            var currentLabel = select.get_text();
            if (currentLabel.indexOf(resx.Global_Hide) !== -1)
            {
                select.set_text(currentLabel.replace(resx.Global_Hide, resx.Global_Show));
            }
            else if (currentLabel.indexOf(resx.Global_Show) !== -1)
            {
                select.set_text(currentLabel.replace(resx.Global_Show, resx.Global_Hide));
            }
            select.get_parent().set_selectedItem('');
        }
    },

    custom_operation: function (operation, value, failQuietly)
    {
        //Call the appropriate webservice to invoke the custom operation, then refresh the datagrid
        var webServiceClass = this.get_webServiceClass();
        this.display_spinner();
        if (failQuietly)
        {
            webServiceClass.CustomOperation(this.get_projectId(), operation, value, Function.createDelegate(this, this.operation_success));
        }
        else
        {
            webServiceClass.CustomOperation(this.get_projectId(), operation, value, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    custom_operation_extended: function (operation, parameters)
    {
        //Call the appropriate webservice to invoke the custom operation, then refresh the datagrid
        var webServiceClass = this.get_webServiceClass();
        this.display_spinner();
        webServiceClass.CustomOperationEx(this.get_projectId(), operation, parameters, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },
    custom_list_operation: function (operation, value, successMessage)
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        if (items.length > 0)
        {
            var context = {};
            context.successMessage = successMessage;

            if (!value)
            {
                //The service expects a non-nullable int
                value = -1;
            }

            //Call the appropriate webservice to invoke the custom operation that passes the item list, then refresh the datagrid
            var webServiceClass = this.get_webServiceClass();
            this.display_spinner();
            webServiceClass.CustomListOperation(operation, this.get_projectId(), value, items, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure), context);
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForCommand);
        }
    },
    custom_operation_select: function (operation, select)
    {
        var value = select.get_value();
        //Call the appropriate webservice to invoke the custom operation, then refresh the datagrid
        var webServiceClass = this.get_webServiceClass();
        this.display_spinner();
        webServiceClass.CustomOperation(this.get_projectId(), operation, value, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },

    focus_on: function(overrideArtifactId)
    {
        //Make sure at least one item is selected or an override artifact id provided
        var items = this._get_selected_items();
        if (items.length == 1 || overrideArtifactId)
        {
            //Call the appropriate webservice to focus on the item, then refresh the datagrid
            var artifactId; //Cannot OR the two values because one is negative giving unexpected results
            if (overrideArtifactId)
            {
                artifactId = overrideArtifactId;
            }
            else
            {
                artifactId = parseInt(items[0]);
            }
            var webServiceClass = this.get_webServiceClass();
            this.display_spinner();
            var clearFilters = (overrideArtifactId) ? false : true;
            webServiceClass.SortedList_FocusOn(this.get_projectId(), artifactId, clearFilters, Function.createDelegate(this, this.focus_on_success), Function.createDelegate(this, this.operation_failure));
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForFocusOn);
        }
    },
    focus_on_success: function (folderId, context)
    {
        //See if we have a validation error or not
        this.hide_spinner();

        //See if we found a folder
        if (folderId)
        {
            //Now we need to reload the data
            this._load_data();

            //Finally raise an event
            this.raise_focusOn(folderId);
        }
    },

    //Allows users to edit multiple items in the grid easily
    _onBulkEditClick: function (e)
    {
        this.edit_items();
    },

    edit_items: function (evt)
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        if (items.length > 0)
        {
            //Need to loop through all the rows in the grid (since we need a reference to the TR)
            for (var j = 0; j < this._tableRows.length; j++)
            {
                //Make sure not already in edit
                var tr = this._tableRows[j];
                if (tr.className != this._editRowCssClass)
                {
                    var primaryKey = tr.getAttribute('tst:primarykey');
                    if (primaryKey)
                    {
                        //Loop through the selected items
                        var artifactId = -1;
                        for (var i = 0; i < items.length; i++)
                        {
                            if (primaryKey == items[i])
                            {
                                artifactId = items[i];
                            }
                        }
                        //We need to get a refreshed copy of the particular row's data using a web service call
                        if (artifactId > 0)
                        {
                            var webServiceClass = this.get_webServiceClass();
                            this.display_spinner();
                            var context = {};
                            context.tr = tr;
                            context.evt = evt;
                            webServiceClass.SortedList_Refresh(this.get_projectId(), artifactId, this._displayTypeId, Function.createDelegate(this, this.edit_success), Function.createDelegate(this, this.operation_failure), context);
                        }
                    }
                }
            }
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForBulkEdit);
        }
    },

    //Sets a specific filter
    set_filter: function (newFilterName, newFilterValue, keepExisting)
    {
        var filterValues = {};
        if (keepExisting)
        {
            var frm = document.forms[0];
            var i;
            for (i = 0; i < frm.length; i++)
            {
                element = frm.elements[i];
                var prefix = this.get_elementId() + '_filter_';
                if ((element.type == 'text' || element.type == 'hidden') && element.name.indexOf(prefix) != -1)
                {
                    //Extract the name and value
                    var filterName = element.name.substring(prefix.length, element.name.length);
                    var filterValue = element.value;
                    if (filterValue.length > 0)
                    {
                        filterValues[filterName] = filterValue;
                    }
                }
            }
        }
        filterValues[newFilterName] = newFilterValue;
        //Now call the webservice to update the filters and reload the data
        this.display_spinner();
        var webServiceClass = this.get_webServiceClass();
        webServiceClass.UpdateFilters(this.get_projectId(), filterValues, this._displayTypeId, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },
    apply_filters: function ()
    {
        //Loop through all the form filter values and populate map
        var filterValues = {};
        var frm = document.forms[0];
        var i;
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            if ((element.type == 'text' || element.type == 'hidden') && element.name.indexOf(this.get_element().id + '_filter_') != -1)
            {
                //Extract the name and value
                var prefixName = this.get_element().id + '_filter_';
                var filterName = element.name.substring(prefixName.length, element.name.length);
                var filterValue = element.value;
                if (filterValue.length > 0)
                {
                    //handle a search in a text field that has a label of ID - this must search for an int, but handle the case where an artifact token is used ([IN:2])
                    if (element.type == 'text' && filterName.match(/id$/gmi))
                    {
                        var matches = filterValue.match(/^\[?[a-zA-Z]{2}\:?([0-9]+)\]?$/);
                        if (matches)
                        {
                            filterValue = matches[1];
                        }
                    }
                    filterValues[filterName] = filterValue;
                }
            }
        }
        //Now call the webservice to update the filters and reload the data
        this.display_spinner();
        var webServiceClass = this.get_webServiceClass();
        webServiceClass.UpdateFilters(this.get_projectId(), filterValues, this._displayTypeId, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },
    clear_filters: function ()
    {
        //Clear all the filters by calling the appropriate webservice
        var filterValues = {};
        var webServiceClass = this.get_webServiceClass();
        this.display_spinner();
        webServiceClass.UpdateFilters(this.get_projectId(), filterValues, this._displayTypeId, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },
    save_filters: function ()
    {
        //We need to display a panel that allows the user to enter the name for the filter being saved
        if (!this._saveFilterDialog) {
            var panel = d.ce('div');
            panel.style.minWidth = '350px';
            panel.className = 'PopupPanel';
            panel.id = 'pnl_filters_manage';
            this.get_element().parentNode.appendChild(panel);
            var title = resx.Global_SaveFilter;
            this._saveFilterDialog = $create(Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel, { themeFolder: this.get_themeFolder(), errorMessageControlId: this.get_errorMessageControlId(), title: title, persistent: true, modal: true }, null, null, panel);

            //Create the create/update options
            var p = d.ce('p');
            panel.appendChild(p);
            text = d.createTextNode(resx.Global_UpdateExistingOrSaveNew);
            p.appendChild(text);

            //The radio button
            var span = d.ce('span');
            span.className = 'btn-group radio-group mb3';
            span.setAttribute('role', 'group');
            panel.appendChild(span);

            //Save New
            var label1 = d.ce('label');
            label1.className = 'btn btn-default active';
            span.appendChild(label1);
            var radSaveNew = d.ce('input');
            radSaveNew.type = 'radio';
            radSaveNew.id = this.get_elementId() + '_radSaveNew';
            radSaveNew.name = 'SaveFilterType';
            radSaveNew.checked = true;
            radSaveNew.value = 'SaveNew';
            label1.appendChild(radSaveNew);
            label1.htmlFor = radSaveNew.id;
            label1.appendChild(d.createTextNode(resx.Global_SaveNew));

            //Update Existing
            var label2 = d.ce('label');
            label2.className = 'btn btn-default';
            span.appendChild(label2);
            var radUpdateExisting = d.ce('input');
            radUpdateExisting.type = 'radio';
            radUpdateExisting.name = 'SaveFilterType';
            radUpdateExisting.id = this.get_elementId() + '_radUpdateExisting';
            radUpdateExisting.value = 'UpdateExisting';
            label2.appendChild(radUpdateExisting);
            label2.htmlFor = radUpdateExisting.id;
            label2.appendChild(d.createTextNode(resx.Global_UpdateExisting));

            //Create New Panel
            var pnlCreateNew = d.ce('div');
            panel.appendChild(pnlCreateNew);
            p = d.ce('p');
            pnlCreateNew.appendChild(p);
            text = d.createTextNode(resx.Global_ChooseNameForFilter);
            p.appendChild(text);

            //The filter name textbox
            var para = d.ce('p');
            pnlCreateNew.appendChild(para);
            var textbox = d.ce('input');
            textbox.id = 'txt_filter_name';
            textbox.className = 'u-input is-active mb4';
            textbox.type = 'text';
            textbox.length = 50;
            textbox.style.width = '100%';
            para.appendChild(textbox);

            //The update existing panel
            var pnlUpdateExisting = d.ce('div');
            pnlUpdateExisting.style.display = 'none';
            panel.appendChild(pnlUpdateExisting);
            p = d.ce('p');
            pnlUpdateExisting.appendChild(p);
            text = d.createTextNode(resx.Global_ChooseFilterToUpdate);
            p.appendChild(text);

            var dropdown = d.ce('div');
            dropdown.id = this.get_elementId() + '_ddlSavedFilterToUpdate';
            dropdown.style.width = '250px';
            dropdown.className = 'u-dropdown is-active';
            pnlUpdateExisting.appendChild(dropdown);
            dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { enabledCssClass: "u-dropdown is-active" }, null, null, dropdown);

            var div = d.ce('div');
            div.className = 'mb4';
            pnlUpdateExisting.appendChild(div);

            //Add the handlers to the radio group
            var radSaveTypeClickHandler = Function.createCallback(this._onSaveTypeRadioClick, { thisRef: this, label1: label1, label2: label2, radSaveNew: radSaveNew, radUpdateExisting: radUpdateExisting, pnlCreateNew: pnlCreateNew, pnlUpdateExisting: pnlUpdateExisting });
            $addHandler(radSaveNew, 'click', radSaveTypeClickHandler);
            $addHandler(radUpdateExisting, 'click', radSaveTypeClickHandler);

            //The share checkbox
            para = d.ce('p');
            panel.appendChild(para);
            var chkShare = d.ce('input');
            chkShare.type = 'checkbox';
            para.appendChild(chkShare);
            para.appendChild(d.createTextNode('\u00a0'));
            para.appendChild(d.createTextNode(resx.Global_ShareFilter));

            //The include columns checkbox
            para = d.ce('p');
            panel.appendChild(para);
            var chkIncludeColumns = d.ce('input');
            chkIncludeColumns.type = 'checkbox';
            chkIncludeColumns.checked = true;   //Default to checked
            para.appendChild(chkIncludeColumns);
            para.appendChild(d.createTextNode('\u00a0'));
            para.appendChild(d.createTextNode(resx.Global_IncludeColumns));

            var br = d.ce('br');
            panel.appendChild(br);

            //The action links
            var div = d.ce('div');
            div.style.clear = 'both';
            div.className = "btn-group";
            panel.appendChild(div);

            //The Save link        
            var a = d.ce('a');
            a.href = 'javascript:void(0)';
            a.className = "btn btn-primary";
            var saveFilterSaveClickHandler = Function.createCallback(this._onSaveFilterSaveClick, { thisRef: this, panel: panel, textbox: textbox, chkShare: chkShare, chkIncludeColumns: chkIncludeColumns, dropdown: dropdown, radSaveNew: radSaveNew, radUpdateExisting: radUpdateExisting });
            $addHandler(a, 'click', saveFilterSaveClickHandler);
            a.appendChild(d.createTextNode(resx.Global_Save));
            div.appendChild(a);

            //The Cancel link
            a = d.ce('a');
            a.href = 'javascript:void(0)';
            a.className = "btn btn-default";
            var saveFilterCancelClickHandler = Function.createCallback(this._onSaveFilterCancelClick, { thisRef: this });
            $addHandler(a, 'click', saveFilterCancelClickHandler);
            a.appendChild(d.createTextNode(resx.Global_Cancel));
            div.appendChild(a);

            //Call the appropriate webservice to get the saved filter list
            var webServiceClass = this.get_webServiceClass();
            webServiceClass.RetrieveFilters(this._projectId, false, Function.createDelegate(this, this.retrieve_filter_for_saving_success), Function.createDelegate(this, this.operation_failure), { dropdown: dropdown });
        }
        this._saveFilterDialog.display();
    },
    retrieve_filter_for_saving_success: function (filterList, e) {
        //Add the items
        var dropdown = e.dropdown;
        dropdown.addItem('', '--- ' + resx.Global_SelectFilter + ' ---');
        dropdown.set_dataSource(filterList);
        dropdown.dataBind();
        dropdown.set_selectedItem('');
    },
    _onSaveTypeRadioClick: function (sender, e) {
        //Show/hide the panel
        if (e.radUpdateExisting.checked) {
            e.label1.className = 'btn btn-default';
            e.label2.className = 'btn btn-default active';
            e.pnlCreateNew.style.display = 'none';
            e.pnlUpdateExisting.style.display = 'block';
        }
        else {
            e.label1.className = 'btn btn-default active';
            e.label2.className = 'btn btn-default';
            e.pnlCreateNew.style.display = 'block';
            e.pnlUpdateExisting.style.display = 'none';
        }
    },
    _onSaveFilterCancelClick: function (sender, e)
    {
        //Close the dialog
        e.thisRef._saveFilterDialog.close();
    },
    _onSaveFilterSaveClick: function (sender, e)
    {
        if (e.radSaveNew.checked && e.textbox.value == '') {
            alert(resx.Global_YouNeedToEnterFilterName);
            return;
        }
        if (e.radUpdateExisting.checked && e.dropdown.get_selectedItem().get_value() == '') {
            alert(resx.Global_NeedToSelectFilter);
            return;
        }

        //Get the data from the provided fields
        var filterName = null;
        if (e.radSaveNew.checked) {
            filterName = e.textbox.value;
        }
        var isShared = e.chkShare.checked;
        var isIncludeColumns = e.chkIncludeColumns.checked;
        var existingSavedFilterId = null;
        if (e.radUpdateExisting.checked) {
            existingSavedFilterId = e.dropdown.get_selectedItem().get_value();
        }

        //Close the save filter dialog box
        e.thisRef._saveFilterDialog.close();

        //Now call the webservice to save the currently applied filters and reload the data
        var webServiceClass = e.thisRef.get_webServiceClass();
        e.thisRef.display_spinner();
        webServiceClass.SaveFilter(e.thisRef.get_projectId(), filterName, isShared, existingSavedFilterId, isIncludeColumns, Function.createDelegate(e.thisRef, e.thisRef.save_filter_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
    },
    save_filter_success: function (result)
    {
        this.hide_spinner();
        if (result == '')
        {
            this.display_info(resx.Global_FilterSaved);
        }
        else
        {
            this.display_error(result);
        }
    },

    set_selected_item: function (primaryKey)
    {
        //Find the matching checkbox
        var frm = document.forms[0];
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            if (element.type == 'checkbox' && !element.disabled && element.getAttribute('tst:primarykey') == primaryKey && element.getAttribute('tst:controlid') == this.get_element().id)
            {
                element.click();
            }
        }
    },
    get_selected_items: function ()
    {
        return this._get_selected_items();
    },
    _get_selected_items: function ()
    {
        //Get all the selected checkboxes
        var items = new Array();
        var frm = document.forms[0];
        var i;
        var element;
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            if (element.type == 'checkbox' && element.name.indexOf('chkItem') != -1)
            {
                if (element.checked)
                {
                    //Get the primary key from the name
                    var primaryKey = element.name.substring(7, element.name.length);
                    items.push(primaryKey);
                }
            }
        }
        return items;
    },

    //Opens up a new item
    open_item: function (target)
    {
        //Make sure that no more than one is selected
        var items = this._get_selected_items();
        if (items.length > 1)
        {
            alert(resx.Global_SelectOneCheckBoxForOpen);
        }
        else
        {
            var artifactId = items[0];
            //If we have a folder, just open it
            if (artifactId < 0)
                {
                    this.focus_on(artifactId);
                }
                else
                {
                    var url = this.get_baseUrl().replace(globalFunctions.artifactIdToken, '' + artifactId);
                window.open(url, target);
            }
        }
    },

    insert_item: function (artifact, displayTypeId)
    {
        this._lastItemType = artifact;

        //If the user was in edit mode, then make sure to update the list BEFORE inserting -  
        //the success from the update will call this insert_item method because we set _lastEditWasInsert to true 
        //the update call will also handle whether or not to set isInEdit to false
        if (this._isInEdit)
        {
            this._lastEditWasInsert = true;
            this._onUpdateClick(null, null);
        }
        else
        { 
            //Selecting checkboxes makes no difference since this is a sorted grid
            //Call the appropriate webservice to insert the items
            var webServiceClass = this.get_webServiceClass();
            this.display_spinner();

            webServiceClass.SortedList_Insert(
                this.get_projectId(),
                artifact,
                this._standardFilters,
                displayTypeId || null,
                Function.createDelegate(this, this.insert_success),
                Function.createDelegate(this, this.operation_failure)
            );
        }
    },
    insert_success: function (artifactId)
    {
        //Now we need to reload the data
        this.hide_spinner();

        //If the user doesn't have bulk edit permissions, we need to redirect to the details page instead of inline editing
        if (this._allowEdit)
        {
            //Capture the new artifact id and load the page
            this._insertArtifactId = artifactId;
            this._lastEditWasInsert = true;
            this._load_data();
        }
        else
        {
            var url = this._baseUrl.replace(globalFunctions.artifactIdToken, '' + artifactId);
            window.open(url, '_self');
        }
    },

    delete_items: function ()
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        if (items.length > 0)
        {
            //Call the appropriate webservice to delete the items
            this.display_spinner();
            var webServiceClass = this.get_webServiceClass();
            webServiceClass.SortedList_Delete(this._projectId, items, this._standardFilters, this._displayTypeId, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForDelete);
        }
    },
    clone_items: function ()
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        if (items.length > 0)
        {
            //Call the appropriate webservice to copy the items
            var webServiceClass = this.get_webServiceClass();
            this.display_spinner();
            webServiceClass.SortedList_Copy(this.get_projectId(), items, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForClone);
        }
    },
    copy_items: function ()
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        if(items.length > 0)
        {
            //Store the selected items in the clipboard
            this._clipboard_items = items;
            this._clipboard_type = 'Copy';
            this.display_info(resx.Global_ItemsCopiedToClipboard);
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForCopy);
        }
    },
    cut_items: function ()
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        if (items.length > 0)
        {
            //Store the selected items in the clipboard
            this._clipboard_items = items;
            this._clipboard_type = 'Move';
            this.display_info(resx.Global_ItemsCutToClipboard);
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForCut);
        }
    },
    paste_items: function ()
    {
        //copy/paste = clone
        //cut/paste = change folder

        //Make sure that the clipboard contains a type and items
        if (this._clipboard_type == '' || this._clipboard_items == null)
        {
            this.display_error(resx.Global_ClipboardEmpty);
        }
        else
        {
            if (this._clipboard_items.length < 1)
            {
                this.display_error(resx.Global_ClipboardEmpty);
            }
            else
            {
                //Call the appropriate webservice to copy/move the items
                var webServiceClass = this.get_webServiceClass();
                if (this._clipboard_type == 'Copy')
                {
                    this.display_spinner();
                    webServiceClass.SortedList_Copy(this.get_projectId(), this._clipboard_items, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
                }
                else if (this._clipboard_type == 'Move')
                {
                    this.display_spinner();
                    webServiceClass.SortedList_Move(this.get_projectId(), this._clipboard_items, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
                }
            }
        } 
    },
    export_items: function (operation, diaglogTitle, diaglogText, dialogButton)
    {
        //Make sure at least one item is selected
        var items = this._get_selected_items();
        var context = {};
        context.operation = operation;
        context.title = diaglogTitle;
        context.text = diaglogText;
        context.button = dialogButton;
        context.artifact = operation;
        if (items.length > 0)
        {
            //We need to display a panel that allows the user to select the destination project/artifact
            var panel = $get('pnl_export_items');
            if (panel == null)
            {
                //Call the appropriate webservice to get the appropriate lookup list
                var webServiceClass = this.get_webServiceClass();
                this.display_spinner();
                webServiceClass.RetrieveLookupList(this.get_projectId(), operation, Function.createDelegate(this, this.export_items_success), Function.createDelegate(this, this.operation_failure), context);
            }
        }
        else
        {
            alert(resx.Global_SelectOneCheckBoxForCommand);
        }
    },
    export_items_success: function (projectList, context)
    {
        this.hide_spinner();
        var panel = d.ce('div');
        panel.className = 'PopupPanel popup-centered-h top-nav';
        panel.id = 'pnl_export_items';
        document.body.appendChild(panel);
        var h1 = d.ce('h1');
        panel.appendChild(h1);
        var text = d.ctn(context.title);
        h1.appendChild(text);
        text = d.ctn(context.text);
        panel.appendChild(text);
        //The project dropdown
        var para = d.ce('p');
        panel.appendChild(para);
        var dropdown = $get('ddl_export_items');
        if (dropdown != null)
        {
            dropdown.dispose();
        }
        dropdown = d.ce('div');
        dropdown.id = 'ddl_export_items';
        panel.appendChild(dropdown);

        //try and localize the passed in artifact (as of 6.5.2 this was solely to projects/products) - ie we have the system name, but want to show the display name
        var artifactLocalized = resx["Global_" + context.artifact] || context.artifact;

        dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { enabledCssClass: "u-dropdown is-active" }, null, null, dropdown);
        dropdown.addItem('', '--- ' + resx.Global_Select + ' ' + artifactLocalized + ' ---');
        dropdown.set_dataSource(projectList);
        dropdown.dataBind();
        dropdown.set_selectedItem('');
        //The action links
        var div = d.ce('div');
        div.className = "btn-group db mt4";
        div.style.clear = 'both';
        panel.appendChild(div);
        var a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.className = 'btn btn-primary';

        //Add the appropriate handlers
        var _onExportConfirmClickHandler = Function.createCallback(this._onExportConfirmClick, { thisRef: this, panel: panel, dropdown: dropdown });
        $addHandler(a, 'click', _onExportConfirmClickHandler);
        a.appendChild(d.ctn(context.button));
        div.appendChild(a);
        a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.className = 'btn btn-default';
        var _onExportCancelClickHandler = Function.createCallback(this._onExportCancelClick, { thisRef: this, panel: panel, dropdown: dropdown });
        $addHandler(a, 'click', _onExportCancelClickHandler);
        a.appendChild(d.ctn(resx.Global_Cancel));
        div.appendChild(a);
    },
    _onExportCancelClick: function (sender, e)
    {
        document.body.removeChild(e.panel);
        e.dropdown.dispose();
        delete (e.panel);
    },
    _onExportConfirmClick: function (sender, e)
    {
        if (e.dropdown.get_selectedItem().get_value() == '')
        {
            alert(resx.Global_NeedToSelectDestinationProject);
        }
        else
        {
            var destProjectId = e.dropdown.get_selectedItem().get_value();
            document.body.removeChild(e.panel);
            e.dropdown.dispose();
            delete (e.panel);
            //Call the appropriate webservice to perform the export
            var webServiceClass = e.thisRef.get_webServiceClass();
            e.thisRef.display_spinner();
            webServiceClass.SortedList_Export(destProjectId, e.thisRef._get_selected_items(), Function.createDelegate(e.thisRef, e.thisRef.export_confirm_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure));
        }
    },
    export_confirm_success: function ()
    {
        this.hide_spinner();
        this.display_info(resx.Global_SuccessExport);
    },
    //Either displays the list of filters, or applies a specified one
    retrieve_filter: function (savedFilterId)
    {
        if (savedFilterId)
        {
            //Call the appropriate webservice to perform the export
            var webServiceClass = this.get_webServiceClass();
            this.display_spinner();
            webServiceClass.RestoreSavedFilter(savedFilterId, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
        }
        else
        {
            //We need to display a panel that allows the user to select the saved filter to apply
            var panel = $get('pnl_custom_list_operation');
            if (panel == null)
            {
                //Call the appropriate webservice to get the saved filter list
                var webServiceClass = this.get_webServiceClass();
                this.display_spinner();
                webServiceClass.RetrieveFilters(this._projectId, true, Function.createDelegate(this, this.retrieve_filter_success), Function.createDelegate(this, this.operation_failure));
            }
        }
    },
    retrieve_filter_success: function (filterList)
    {
        this.hide_spinner();
        var panel = d.ce('div');
        panel.className = 'PopupPanel popup-centered-h top-nav';
        panel.id = 'pnl_custom_list_operation';
        document.body.appendChild(panel);
        var h1 = d.ce('h1');
        panel.appendChild(h1);
        var text = d.ctn(resx.Global_RetrieveSavedFilter);
        h1.appendChild(text);
        text = d.ctn(resx.Global_PleaseSelectFilter);
        panel.appendChild(text);
        //The project dropdown
        var para = d.ce('p');
        panel.appendChild(para);
        var dropdown = $get('ddl_custom_list');
        if (dropdown != null)
        {
            dropdown.dispose();
        }
        dropdown = d.ce('div');
        dropdown.id = 'ddl_custom_list';
        dropdown.style.width = '250px';
        dropdown.className = 'u-dropdown is-active';
        panel.appendChild(dropdown);
        dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { enabledCssClass: "u-dropdown is-active" }, null, null, dropdown);
        //Add the items
        dropdown.addItem('', '--- ' + resx.Global_SelectFilter + ' ---');
        dropdown.set_dataSource(filterList);
        dropdown.dataBind();
        dropdown.set_selectedItem('');
        //The action links
        var div = d.ce('div');
        div.className = "btn-group mt4 db";
        div.style.clear = 'both';
        panel.appendChild(div);
        var a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.className = 'btn btn-primary';
        //Add the appropriate handlers
        var _onRetrieveFilterClickHandler = Function.createCallback(this._onRetrieveFilterClick, { thisRef: this, panel: panel, dropdown: dropdown });
        $addHandler(a, 'click', _onRetrieveFilterClickHandler);
        a.appendChild(d.ctn(resx.Global_ApplyFilter));
        div.appendChild(a);
        a = d.ce('a');
        a.href = 'javascript:void(0)';
        a.className = 'btn btn-default';
        var _onCustomListCancelClickHandler = Function.createCallback(this._onCustomListCancelClick, { thisRef: this, panel: panel, dropdown: dropdown });
        $addHandler(a, 'click', _onCustomListCancelClickHandler);
        a.appendChild(d.ctn(resx.Global_Cancel));
        div.appendChild(a);
    },
    _onRetrieveFilterClick: function (sender, e)
    {
        if (e.dropdown.get_selectedItem().get_value() == '')
        {
            alert(resx.Global_NeedToSelectFilter);
        }
        else
        {
            var destId = e.dropdown.get_selectedItem().get_value();
            document.body.removeChild(e.panel);
            e.dropdown.dispose();
            delete (e.panel);
            //Call the appropriate webservice to perform the export
            var webServiceClass = e.thisRef.get_webServiceClass();
            e.thisRef.display_spinner();
            webServiceClass.RestoreSavedFilter(destId, Function.createDelegate(e.thisRef, e.thisRef.operation_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure), e.artifact);
        }
    },
    _onCustomListCancelClick: function (sender, e)
    {
        document.body.removeChild(e.panel);
        e.dropdown.dispose();
        delete (e.panel);
    },
    _onUpdateClick: function (sender, e)
    {
        //Get the various items being updated
        var dataItems = new Array();
        var frm = document.forms[0];
        var i, j;
        var dataItem;
        var fieldName, primaryKey;
        var thisRef = e && e.thisRef ? e.thisRef : this;
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            //Handle text-boxes and hidden fields (dropdowns)
            if ((element.type == 'text' || element.type == 'hidden') && element.name.indexOf(thisRef.get_element().id + '_edit_') != -1)
            {
                //Extract the field name and primary key
                fieldName = element.name.substring(element.name.indexOf('-') + 1, element.name.length);
                primaryKey = element.name.substring(element.name.indexOf('_edit_') + 6, element.name.indexOf('-'));
                //See if we have already created this item
                dataItem = null;
                for (j = 0; j < dataItems.length; j++)
                {
                    if (dataItems[j].primaryKey == primaryKey)
                    {
                        dataItem = dataItems[j];
                    }
                }
                if (dataItem == null)
                {
                    dataItem = {};
                    dataItem.primaryKey = primaryKey;
                    dataItem.Fields = {};
                    dataItems.push(dataItem);

                    //Add concurrency value if needed
                    if (thisRef._concurrencyEnabled && thisRef._concurrencyValues)
                    {
                        var concurrencyValue = thisRef._concurrencyValues[primaryKey];
                        if (concurrencyValue)
                        {
                            dataItem.concurrencyValue = concurrencyValue;
                        }
                    }
                }
                //Now populate the field values
                dataItem.Fields[fieldName] = {};
                dataItem.Fields[fieldName].__type = globalFunctions.dataType_DataItemField;
                dataItem.Fields[fieldName].fieldName = fieldName;
                dataItem.Fields[fieldName].intValue = null;
                dataItem.Fields[fieldName].textValue = null;
                dataItem.Fields[fieldName].fieldType = element.getAttribute('fieldType');
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_text || dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_nameDescription ||
                    dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_decimal)
                {
                    dataItem.Fields[fieldName].textValue = element.value;
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_integer)
                {
                    //Make sure it's numeric
                    var chk = /^\d+$/.test(element.value);
                    if (element.value == '')
                    {
                        dataItem.Fields[fieldName].intValue = null;
                    }
                    else
                    {
                        if (chk)
                        {
                            dataItem.Fields[fieldName].intValue = element.value;
                        }
                        else
                        {
                            thisRef.display_error(resx.Global_IntegerValidation.replace('{0}', fieldName));
                            return;
                        }
                    }
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_timeInterval)
                {
                    //Make sure it's a valid decimal value
                    var chk = /^\d*[0-9](|.\d*[0-9]|,\d*[0-9])?$/.test(element.value);
                    if (element.value == '')
                    {
                        dataItem.Fields[fieldName].intValue = null;
                    }
                    else
                    {
                        if (chk)
                        {
                            dataItem.Fields[fieldName].intValue = Math.round(parseFloat(element.value) * 60.0);
                        }
                        else
                        {
                            alert(resx.Global_TimeIntervalValidation.replace('{0}', fieldName));
                            return;
                        }
                    }
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_lookup)
                {
                    if (element.value == '')
                    {
                        dataItem.Fields[fieldName].intValue = null;
                    }
                    else
                    {
                        dataItem.Fields[fieldName].intValue = element.value;
                    }
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_flag)
                {
                    dataItem.Fields[fieldName].textValue = element.value;
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_customPropertyLookup || dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_hierarchyLookup)
                {
                    if (element.value == '')
                    {
                        dataItem.Fields[fieldName].intValue = null;
                    }
                    else
                    {
                        dataItem.Fields[fieldName].intValue = element.value;
                    }
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_customPropertyMultiList || dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_multiList)
                {
                    dataItem.Fields[fieldName].textValue = element.value;
                }
                if (dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_dateTime || dataItem.Fields[fieldName].fieldType == globalFunctions._fieldType_customPropertyDate)
                {
                    dataItem.Fields[fieldName].textValue = element.value;
                }
            }
        }

        //Set the mode flag
        if (e && e.mode != 'Insert')
        {
            thisRef._lastEditWasInsert = false;
        }

        //Pass to webservice and refresh page
        var webServiceClass = thisRef.get_webServiceClass();
        if (typeof sender === "function") {
            this._updateSuccessCallback =  sender;
        }

        thisRef.display_spinner();
        webServiceClass.SortedList_Update(thisRef.get_projectId(), dataItems, thisRef._displayTypeId, Function.createDelegate(thisRef, thisRef.update_success), Function.createDelegate(thisRef, thisRef.operation_failure));
    },

    _onCancelClick: function (sender, e)
    {
        //Set the mode flag
        e.thisRef._lastEditWasInsert = false;

        //Simply reload the data
        e.thisRef._load_data();
    },

    _onFillClick: function (sender, e)
    {
        //Iterate through the other rows and copy
        var filterValues = {};
        var frm = document.forms[0];
        var i;
        var fieldName;
        for (i = 0; i < frm.length; i++)
        {
            element = frm.elements[i];
            //Handle text-boxes
            if (element.type == 'text' && element.name.indexOf(e.thisRef.get_element().id + '_edit_') != -1)
            {
                fieldName = element.name.substring(element.name.indexOf('-') + 1, element.name.length);
                if (fieldName == e.fieldName)
                {
                    element.value = e.source.value;
                }
            }
            //Handle dropdowns
            if (element.type == 'hidden' && element.name.indexOf(e.thisRef.get_element().id + '_edit_') != -1)
            {
                fieldName = element.name.substring(element.name.indexOf('-') + 1, element.name.length);
                if (fieldName == e.fieldName)
                {
                    var dropdown = $find(element.name + '_ddl');
                    if (dropdown)
                    {
                        dropdown.set_selectedItem(e.source.value);
                    }
                }
            }
        }
    },

    _onButtonMouseOver: function (sender, e)
    {
        e.img.src = e.url;
    },
    _onButtonMouseOut: function (sender, e)
    {
        e.img.src = e.url;
    },

    _onEditClick: function (evt, e)
    {
        //We need to get a refreshed copy of the particular row's data using a web service call
        var artifactId = e.primaryKey;
        var webServiceClass = e.thisRef.get_webServiceClass();
        var context = {};
        context.evt = evt;
        context.tr = e.tr;
        e.thisRef.display_spinner();
        webServiceClass.SortedList_Refresh(e.thisRef.get_projectId(), artifactId, e.thisRef._displayTypeId, Function.createDelegate(e.thisRef, e.thisRef.edit_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure), context);
    },
    edit_success: function (dataItem, context)
    {
        this.hide_spinner();
        var tr = context.tr;
        //Store the concurrency value if necessary
        if (this._concurrencyEnabled && this._concurrencyValues && dataItem.concurrencyValue)
        {
            this._concurrencyValues[dataItem.primaryKey] = dataItem.concurrencyValue;
        }
        //Now iterate through the columns and turn them into editable versions
        var columns = this.get_columns();
        var i, j;
        for (i = 0; i < columns.length; i++)
        {
            var editable = false;
            var fillSource = null;
            //Get the corresponding cell (add cell offset to account for checkbox and attachments)
            var offset = 0;
            if (this._displayCheckboxes)
            {
                offset++;
            }
            if (this._displayAttachments)
            {
                offset++;
            }
            var fieldName = columns[i].dataField;
            if (dataItem.Fields[fieldName] != null)
            {
                if (dataItem.Fields[fieldName].editable)
                {
                    var td = tr.cells[i + offset];
                    //Remove all child nodes and any cell specific styles (except for equalizers)
                    if (columns[i].fieldType != globalFunctions._fieldType_equalizer)
                    {
                        while (td.childNodes.length > 0)
                        {
                            td.removeChild(td.childNodes[0]);
                        }
                    }

                    //Create a specific-width div and two child inline-block divs
                    var outerDiv = d.ce('div');
                    outerDiv.className = this._allowColumnPositioning ? 'fixed-width priority1 df' : 'priority1 df';
                    td.appendChild(outerDiv);
                    var td1 = d.ce('div');
                    td1.className = 'editable-cell-1';
                    if (dataItem.Fields[fieldName].width)
                    {
                        outerDiv.style.width = dataItem.Fields[fieldName].width + 'px';
                    }
                    outerDiv.appendChild(td1);

                    //Handle each of the different types in turn
                    if (columns[i].fieldType == globalFunctions._fieldType_nameDescription)
                    {
                        //Add the icons
                        td1.classList.add("grow-1");
                        var img = d.ce('img');
                        img.className = "w4 h4 mr3";
                        img.src = this.get_themeFolder() + 'Images/' + this.get_itemImage();
                        td1.appendChild(img);
                        td1.appendChild(d.ctn(' '));

                        //Add a textbox
                        var textbox = d.ce('input');
                        textbox.name = this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName;
                        textbox.type = 'text';
                        textbox.className = 'text-box fill';
                        textbox.placeholder = resx.Artifact_EnterNewName;
                        td1.appendChild(textbox);

                        //Set the value from the datasource
                        textbox.value = dataItem.Fields[fieldName].textValue;
                        textbox.setAttribute('fieldType', dataItem.Fields[fieldName].fieldType);
                        fillSource = textbox;
                        editable = true;
                    }
                    if (columns[i].fieldType == globalFunctions._fieldType_lookup || columns[i].fieldType == globalFunctions._fieldType_customPropertyLookup ||
                        columns[i].fieldType == globalFunctions._fieldType_flag || columns[i].fieldType == globalFunctions._fieldType_customPropertyMultiList ||
                        columns[i].fieldType == globalFunctions._fieldType_multiList)
                    {
                        //Add a dropdown
                        var div = d.ce('div');
                        div.id = this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName + '_ddl';
                        if (columns[i].fieldType == globalFunctions._fieldType_flag)
                        {
                            div.style.width = '70px';
                        }
                        else
                        {
                            div.style.width = '94px';
                        }
                        dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, null, null, null, div);
                        dropdown.set_name(this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName);
                        //Allow multiple values for a 'multilist'
                        if (columns[i].fieldType == globalFunctions._fieldType_customPropertyMultiList || columns[i].fieldType == globalFunctions._fieldType_multiList)
                        {
                            dropdown.set_multiSelectable(true);
                        }
                        //Display 'None' or 'Please Select'
                        if (dataItem.Fields[fieldName].required)
                        {
                            dropdown.addItem('', resx.Global_PleaseSelect);
                        }
                        else
                        {
                            dropdown.addItem('', '-- ' + resx.Global_None + ' --');
                        }
                        //Add the lookup values
                        var lookups = dataItem.Fields[fieldName].lookups;
                        for (var lookupName in lookups)
                        {
                            //Need to remove the prefix added by the serializer
                            var item = lookupName.substring(1);
                            //Don't add any negative items as they are used for composite/negative filters
                            if (item.indexOf('-') == -1)
                            {
                                dropdown.addItem(item, lookups[lookupName]);
                            }
                        }
                        //Default to the -- None -- item
                        dropdown.set_selectedItem('');
                        dropdown.get_valueElement().setAttribute('fieldType', dataItem.Fields[fieldName].fieldType);
                        if (!globalFunctions.isNullOrUndefined(dataItem.Fields[fieldName].intValue) && columns[i].fieldType != globalFunctions._fieldType_customPropertyMultiList)
                        {
                            dropdown.set_selectedItem(dataItem.Fields[fieldName].intValue);
                        }
                        else if (dataItem.Fields[fieldName].textValue)
                        {
                            dropdown.set_selectedItem(dataItem.Fields[fieldName].textValue);
                        }
                        td1.style.whiteSpace = 'nowrap';
                        td1.appendChild(div);
                        this._edits.push(dropdown);
                        fillSource = dropdown.get_valueElement();
                        editable = true;
                    }
                    if (columns[i].fieldType == globalFunctions._fieldType_hierarchyLookup)
                    {
                        //Add a dropdown
                        var div = d.ce('div');
                        div.id = this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName + '_ddl';
                        if (columns[i].fieldType == globalFunctions._fieldType_flag)
                        {
                            div.style.width = '70px';
                        }
                        else
                        {
                            div.style.width = '94px';
                        }
                        dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownHierarchy, { "listWidth": "200px", "themeFolder": this._themeFolder }, null, null, div);
                        dropdown.set_name(this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName);
                        //Handle the images (varies by data)
                        globalFunctions.getHierarchyLookupImages(dropdown, fieldName);
                        if (!dataItem.Fields[fieldName].required)
                        {
                            dropdown.addItem('', '0', '', '', 'Y', '-- ' + resx.Global_None + ' --');
                        }
                        //Add the lookup values
                        var lookups = dataItem.Fields[fieldName].lookups;
                        dropdown.set_dataSource(lookups);
                        dropdown.dataBind();
                        //Default to the -- None -- item
                        dropdown.set_selectedItem('');
                        dropdown.get_valueElement().setAttribute('fieldType', dataItem.Fields[fieldName].fieldType);
                        if (!globalFunctions.isNullOrUndefined(dataItem.Fields[fieldName].intValue))
                        {
                            dropdown.set_selectedItem(dataItem.Fields[fieldName].intValue);
                        }
                        td1.style.whiteSpace = 'nowrap';
                        td1.appendChild(div);
                        this._edits.push(dropdown);
                        fillSource = dropdown.get_valueElement();
                        editable = true;
                    }
                    if (columns[i].fieldType == globalFunctions._fieldType_dateTime || columns[i].fieldType == globalFunctions._fieldType_customPropertyDate)
                    {
                        //Add a date control
                        var div = d.ce('div');
                        div.id = this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName + '_dat';
                        div.className = 'DatePicker';
                        div.style.width = '90px';
                        var datePicker = $create(Inflectra.SpiraTest.Web.ServerControls.DatePicker, null, null, null, div);
                        datePicker.set_name(this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName);
                        datePicker.set_dateFormat(this.get_dateFormat());
                        td1.appendChild(datePicker.get_element());
                        this._edits.push(datePicker);
                        td1.style.whiteSpace = 'nowrap';

                        //Set the value from the datasource
                        if (dataItem.Fields[fieldName].textValue)
                        {
                            datePicker.set_value(dataItem.Fields[fieldName].textValue);
                        }
                        datePicker.get_valueElement().setAttribute('fieldType', dataItem.Fields[fieldName].fieldType);
                        fillSource = datePicker.get_valueElement();
                        editable = true;
                    }
                    if (columns[i].fieldType == globalFunctions._fieldType_text || columns[i].fieldType == globalFunctions._fieldType_decimal)
                    {
                        //Add a textbox
                        td1.classList.add("grow-1");
                        var textbox = d.ce('input');
                        textbox.name = this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName;
                        textbox.type = 'text';
                        textbox.className = 'text-box fill';
                        td1.appendChild(textbox);

                        //Set the value from the datasource
                        if (dataItem.Fields[fieldName].textValue)
                        {
                            textbox.value = dataItem.Fields[fieldName].textValue;
                        }
                        textbox.setAttribute('fieldType', dataItem.Fields[fieldName].fieldType);
                        editable = true;
                        fillSource = textbox;
                    }
                    if (columns[i].fieldType == globalFunctions._fieldType_integer || columns[i].fieldType == globalFunctions._fieldType_timeInterval)
                    {
                        //Add a textbox
                        td1.classList.add("grow-1");
                        var textbox = d.ce('input');
                        textbox.name = this.get_element().id + '_edit_' + dataItem.primaryKey + '-' + fieldName;
                        textbox.type = 'text';    //Do not make type='number' breaks the filtering
                        textbox.className = 'text-box narrow';
                        td1.appendChild(textbox);

                        //Set the value from the datasource
                        if (globalFunctions.isNullOrUndefined(dataItem.Fields[fieldName].intValue))
                        {
                            textbox.value = '';
                        }
                        else
                        {
                            //For time intervals, need to convert from minutes to fractional hours (2 decimal places)
                            if (columns[i].fieldType == globalFunctions._fieldType_timeInterval)
                            {
                                var hours = '' + Math.floor(dataItem.Fields[fieldName].intValue / 60);
                                var fraction = Math.floor((dataItem.Fields[fieldName].intValue % 60) / 60 * 100);
                                if (fraction >= 10)
                                {
                                    textbox.value = hours + '.' + fraction;
                                }
                                else
                                {
                                    textbox.value = hours + '.0' + fraction;
                                }
                            }
                            else
                            {
                                textbox.value = dataItem.Fields[fieldName].intValue;
                            }
                        }
                        textbox.setAttribute('fieldType', dataItem.Fields[fieldName].fieldType);
                        editable = true;
                        fillSource = textbox;
                    }
                    if (columns[i].fieldType == globalFunctions._fieldType_identifier)
                    {
                        //IDs are always read-only text
                        td1.appendChild(d.createTextNode(dataItem.Fields[fieldName].textValue));
                    }

                    //If not already in edit mode, add the fill button
                    if (!this._isInEdit && editable)
                    {
                        var td2 = d.ce('div');
                        td2.className = 'editable-cell-2';
                        var img = d.ce('span');
                        img.className = "fas fa-paste pointer pl3";
                        img.setAttribute('tooltip', resx.Global_FillWithValue);
                        img.title = resx.Global_FillWithValue;
                        td2.appendChild(img);
                        outerDiv.appendChild(td2);
                        var _fillClickHandler = Function.createCallback(this._onFillClick, { thisRef: this, source: fillSource, fieldName: fieldName, primaryKey: dataItem.primaryKey });
                        $addHandler(img, 'click', _fillClickHandler);
                    }
                }
            }
        }
        //Finally mark the row as edited and add the update/cancel buttons if no other row is edited
        tr.className = this.get_editRowCssClass();
        var td = tr.cells[tr.cells.length - 1];
        for (j = 0; j < td.childNodes.length; j++)
        {
            td.removeChild(td.childNodes[j]);
        }
        if (!this._isInEdit)
        {
            this._isInEdit = true;
            //Insert
            if (this._lastEditWasInsert)
            {
                var btn = d.ce('button');
                btn.type = 'button';
                btn.className = 'btn btn-default';
                btn.appendChild(d.createTextNode(resx.Global_SaveAndNew));
                btn.style.cursor = 'pointer';
                td.appendChild(btn);
                td.appendChild(d.ctn(' '));
                var _updateClickHandler = Function.createCallback(this._onUpdateClick, { thisRef: this, mode: 'Insert' });
                $addHandler(btn, 'click', _updateClickHandler);
                this._buttons.push(btn);
                this._buttonClickHandlers.push(_updateClickHandler);
            }

            //Update
            var btn = d.ce('button');
            btn.type = 'button';
            btn.className = 'btn btn-primary';
            btn.appendChild(d.createTextNode(resx.Global_Save));
            btn.style.cursor = 'pointer';
            td.appendChild(btn);
            td.appendChild(d.ctn(' '));
            var _updateClickHandler = Function.createCallback(this._onUpdateClick, { thisRef: this, mode: 'Update' });
            $addHandler(btn, 'click', _updateClickHandler);
            this._buttons.push(btn);
            this._buttonClickHandlers.push(_updateClickHandler);

            //Cancel
            var btn = d.ce('button');
            btn.type = 'button';
            btn.className = 'btn btn-default';
            btn.appendChild(d.createTextNode(resx.Global_Cancel));
            btn.style.cursor = 'pointer';
            td.appendChild(btn);
            var _cancelClickHandler = Function.createCallback(this._onCancelClick, { thisRef: this });
            $addHandler(btn, 'click', _cancelClickHandler);
            this._buttons.push(btn);
            this._buttonClickHandlers.push(_cancelClickHandler);
        }
    },

    _onTdMouseOver: function (sender, e)
    {
        ddrivetip(e.tooltip);
    },
    _onTdMouseOut: function (sender, e)
    {
        hideddrivetip();
    },

    _onNameDescMouseOver: function (evt, e)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        e.thisRef._isOverNameDesc = true;   //Set the flag since asynchronous
        //Now get the real tooltip via Ajax web-service call
        var artifactId = e.primaryKey;
        var webServiceClass = e.webServiceClass;
        webServiceClass.RetrieveNameDesc(e.thisRef._projectId, artifactId, e.thisRef._displayTypeId, Function.createDelegate(e.thisRef, e.thisRef.retrieveNameDesc_success), Function.createDelegate(e.thisRef, e.thisRef.operation_failure_quiet));
    },
    retrieveNameDesc_success: function (tooltipData)
    {
        if (this._isOverNameDesc)
        {
            if (tooltipData)
            {
                ddrivetip(tooltipData);
            }
            else
            {
                hideddrivetip();
            }
        }
    },
    _onNameDescMouseOut: function (evt, e)
    {
        hideddrivetip();
        e.thisRef._isOverNameDesc = false;
    },

    _onResizableColumnMouseDown: function (evt, context)
    {
        var thisRef = context.thisRef;
        var target = evt.target;
        var tagName = target.tagName.toLowerCase();
        if (evt.button != 2 && tagName == 'div' && target.className == 'fixed-width')
        {
            evt.preventDefault();
            evt.stopPropagation();
            var div = context.div;
            var dragSource = context.dragSource;
            thisRef._holdStarter = setTimeout(function ()
            {
                thisRef._holdStarter = null;
                thisRef._holdActive = true;
                thisRef._startResize(evt, div, dragSource);
            }, globalFunctions.holdDelay);
        }
    },
    _onResizableColumnMouseUp: function (evt, context)
    {
        var thisRef = context.thisRef;
        thisRef._preventDrag();
    },
    _onResizableColumnTouchMove: function (evt, context)
    {
        //Make sure not already dragging, editing and only icon touched
        var div = context.div;
        var dragSource = context.dragSource;
        var thisRef = context.thisRef;
        if (!thisRef._touchDragging)
        {
            thisRef._touchDragging = true;
            thisRef._startResize(evt, div, dragSource);
        }
    },

    _onRowMouseDown: function (evt, context)
    {
        var thisRef = context.thisRef;
        var tagName = evt.target.tagName.toLowerCase();
        if (!thisRef._isInEdit && tagName != 'input' && tagName != 'button' && evt.button != 2)
        {
            evt.preventDefault();
            evt.stopPropagation();
            var tr = context.tr;
            var dragSource = context.dragSource;
            thisRef._holdStarter = setTimeout(function ()
            {
                thisRef._holdStarter = null;
                thisRef._holdActive = true;
                thisRef._startDrag(evt, tr, dragSource);
            }, globalFunctions.holdDelay);
        }
    },
    _onRowMouseUp: function (evt, context)
    {
        var thisRef = context.thisRef;
        thisRef._preventDrag();
    },
    _onRowTouchMove: function (evt, context)
    {
        //Make sure not already dragging
        var tr = context.tr;
        var dragSource = context.dragSource;
        var thisRef = context.thisRef;
        var tagName = evt.target.tagName.toLowerCase();
        if (!thisRef._touchDragging && !thisRef._isInEdit && (tagName == 'img' || tagName == 'a'))
        {
            thisRef._touchDragging = true;
            thisRef._startDrag(evt, tr, dragSource);
        }
    },

    //Prevents dragging on clicks on other things
    _preventDrag: function()
    {
        if (this._holdStarter)
        {
            clearTimeout(this._holdStarter);
        }
        else if (this._holdActive)
        {
            this._holdActive = false;
        }
    },
    _startDrag: function (evt, tr, dragSource)
    {
        window._event = evt; // Needed internally by _DragDropManager

        this._isInDrag = true;
        this._visual = d.ce('table');
        this._visual.className = 'table table-dragging';
        var tbody = d.ce('tbody');
        this._visual.appendChild(tbody);
        tbody.appendChild(tr.cloneNode(true));
        if (this._get_selected_items() && this._get_selected_items().length > 0)
        {
            $(tbody.rows[0].cells[2]).html('<span class="far fa-object-group"> ' + resx.Global_MultipleItems + '</span>');
            dragSource.set_selectedItems(this._get_selected_items());
        }
        this._element.parentNode.appendChild(this._visual);
        var location = Sys.UI.DomElement.getLocation(tr);
        Sys.UI.DomElement.setLocation(this._visual, location.x, (location.y - 160));// sorted
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(dragSource, this._visual, null);
    },
    _startResize: function (evt, div, dragSource)
    {
        window._event = evt; // Needed internally by _DragDropManager

        this._isInDrag = true;
        this._visual = d.ce('div');
        this._visual.className = 'resize-handle b-peach br bw1 h-75';
        this._visual.style.width = div.offsetWidth + 'px';
        this._element.parentNode.appendChild(this._visual);
        var location = Sys.UI.DomElement.getLocation(div);
		Sys.UI.DomElement.setLocation(this._visual, location.x, location.y); 
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(dragSource, this._visual, null);
    },
    finish_resize: function (fieldName, width)
    {
        //We need to make a web service call to ensure that the server updates the columns
        var webServiceClass = this.get_webServiceClass();
        webServiceClass.List_ChangeColumnWidth(this._projectId, fieldName, width, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },

    finish_columnDrop: function (dataField, columnIndex)
    {
        //We need to make a web service call to ensure that the server updates the columns
        var webServiceClass = this.get_webServiceClass();
        webServiceClass.List_ChangeColumnPosition(this._projectId, dataField, columnIndex, Function.createDelegate(this, this.operation_success), Function.createDelegate(this, this.operation_failure));
    },

    _onHeaderMouseDown: function (evt, context)
    {
        //This is the first method of drag and drop
        //Only allow left mouse button drag
        if (Sys.Browser.agent != Sys.Browser.Safari)
        {
            if (evt.button != Sys.UI.MouseButton.leftButton)
            {
                return;
            }
        }

        var thisRef = context.thisRef;
        evt.preventDefault();
        evt.stopPropagation();
        var th = context.th;
        var dragSource = context.dragSource;
        thisRef._holdStarter = setTimeout(function ()
        {
            thisRef._holdStarter = null;
            thisRef._holdActive = true;
            thisRef._startDrag(evt, th, dragSource);
        }, globalFunctions.holdDelay);
    },

    _clearFilters: function ()
    {
        var i;
        for (i = 0; i < this._filters.length; i++)
        {
            this._filters[i].dispose();
        }
        delete (this._filters);
        this._filters = new Array();
    },
    _clearEdits: function ()
    {
        var i;
        for (i = 0; i < this._edits.length; i++)
        {
            this._edits[i].dispose();
        }
        delete (this._edits);
        this._edits = new Array();
    },
    _clearContent: function (element)
    {
        if (element.firstChild)
        {
            while (element.firstChild)
            {
                element.removeChild(element.firstChild);
            }
        }
    }
}

Inflectra.SpiraTest.Web.ServerControls.SortedGrid.registerClass('Inflectra.SpiraTest.Web.ServerControls.SortedGrid', Sys.UI.Control);

/* SortedGridRow */
Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior.initializeBase(this, [element]);
    this._element = element;
    this._parent = null;
    this._primaryKey = null;
    this._selectedItems = null;
}
Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior.callBaseMethod(this, 'initialize');
    },
    dispose: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior.callBaseMethod(this, 'initialize');
    },

    /* Properties */
    get_element: function()
    {
        return this._element;
    },

    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (value)
    {
        this._parent = value;
    },

    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },
    
    get_selectedItems: function ()
    {
        return this._selectedItems;
    },
    set_selectedItems: function (value)
    {
        this._selectedItems = value;
    },

    /* IDragSource Interface */

    get_dragDataType: function ()
    {
        return 'SortedGridRow';
    },

    getDragData: function (context)
    {
        return this;
    },

    get_dragMode: function ()
    {
        return Inflectra.SpiraTest.Web.ServerControls.DragMode.Move;
    },

    onDragStart: function () {},

    onDrag: function ()
    {
        var evt = window._event;
        var visual = this._parent.get_visual();
        var ddm = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance();
        if (evt.changedTouches)
        {
            //touch
            var touchPosition = { x: Math.floor(evt.changedTouches[0].pageX), y: Math.floor(evt.changedTouches[0].pageY) };
            var position = ddm.subtractPoints(touchPosition, visual.startingPoint);
            Sys.UI.DomElement.setLocation(visual, position.x, position.y);
        }
        else
        {
            //mouse
            var mousePosition = { x: evt.clientX, y: evt.clientY };
            var scrollOffset = ddm.getScrollOffset(visual, /* recursive */true);
            var position = ddm.addPoints(ddm.subtractPoints(mousePosition, visual.startingPoint), scrollOffset);
            Sys.UI.DomElement.setLocation(visual, position.x, position.y);
        }
    },

    onDragEnd: function (canceled)
    {
        this._parent._isInDrag = false;
        var visual = this._parent.get_visual();
        this._parent.set_touchDragging(false);
        //Clear the visual
        if (visual)
        {
            visual.parentNode.removeChild(visual);
            this._parent.set_visual(null);
        }
        //Make sure all targets returned to normal
        $(this._parent.get_element()).find('tr.drag-target').removeClass('drag-target');
    }
}
Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.SortedGridRowBehavior', Sys.UI.Behavior);

/* SortedResizableColumn */
Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior.initializeBase(this, [element]);
    this._element = element;
    this._parent = null;
    this._fieldName = null;
    this._startX = 0;
    this._startWidth = 0;
}
Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior.callBaseMethod(this, 'initialize');
    },
    dispose: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior.callBaseMethod(this, 'initialize');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },

    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (value)
    {
        this._parent = value;
    },

    get_fieldName: function ()
    {
        return this._fieldName;
    },
    set_fieldName: function (value)
    {
        this._fieldName = value;
    },

    /* IDragSource Interface */

    get_dragDataType: function ()
    {
        return 'SortedResizableColumn';
    },

    getDragData: function (context)
    {
        return this;
    },

    get_dragMode: function ()
    {
        return Inflectra.SpiraTest.Web.ServerControls.DragMode.Move;
    },

    onDragStart: function ()
    {
        var visual = this._parent.get_visual();
        this._startX = Sys.UI.DomElement.getLocation(visual).x;
        this._startWidth = visual.offsetWidth;
    },

    onDrag: function ()
    {
        var evt = window._event;
        var visual = this._parent.get_visual();
        var ddm = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance();
        if (evt.changedTouches)
        {
            //touch
            var touchPosition = { x: evt.changedTouches[0].pageX, y: evt.changedTouches[0].pageY };
            var position = ddm.subtractPoints(touchPosition, visual.startingPoint);
            Sys.UI.DomElement.setLocation(visual, Math.floor(position.x), Math.floor(position.y));
        }
        else
        {
            //mouse
            var mousePosition = { x: evt.clientX, y: evt.clientY };
            var scrollOffset = ddm.getScrollOffset(visual, /* recursive */true);
            var position = ddm.addPoints(ddm.subtractPoints(mousePosition, visual.startingPoint), scrollOffset);
            Sys.UI.DomElement.setLocation(visual, position.x, position.y);
        }
    },

    onDragEnd: function (canceled)
    {
        this._parent._isInDrag = false;
        var visual = this._parent.get_visual();
        this._parent.set_touchDragging(false);

        if (visual)
        {
            //Calculate width
            var endX = Sys.UI.DomElement.getLocation(visual).x;
            var width = this._startWidth + (endX - this._startX);
            if (width < 100)
            {
                width = 100; //min width
            }

            //Clear the visual
            visual.parentNode.removeChild(visual);
            this._parent.set_visual(null);

            //Call the resize finish event on parent
            this._parent.finish_resize(this._fieldName, width);
        }
    }
}
Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.SortedResizableColumnBehavior', Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDropTarget);

/* SortedGridColumn */
Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior.initializeBase(this, [element]);
    this._element = element;
    this._parent = null;
    this._primaryKey = null;
}
Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior.callBaseMethod(this, 'initialize');

        // register this drop target in DragDropManager
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);
    },
    dispose: function ()
    {
        // unregister this drop target in DragDropManager
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.unregisterDropTarget(this);

        Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior.callBaseMethod(this, 'initialize');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },

    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (value)
    {
        this._parent = value;
    },

    get_dataField: function ()
    {
        return this._dataField;
    },
    set_dataField: function (value)
    {
        this._dataField = value;
    },

    get_columnIndex: function ()
    {
        return this._columnIndex;
    },
    set_columnIndex: function (value)
    {
        this._columnIndex = value;
    },

    /* IDragSource Interface */

    get_dragDataType: function ()
    {
        return 'SortedGridColumn';
    },

    getDragData: function (context)
    {
        return this;
    },

    get_dragMode: function ()
    {
        return Inflectra.SpiraTest.Web.ServerControls.DragMode.Move;
    },

    onDragStart: function () { },

    onDrag: function ()
    {
        var evt = window._event;
        var visual = this._parent.get_visual();
        var ddm = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance();
        if (evt.changedTouches)
        {
            //touch
            var touchPosition = { x: evt.changedTouches[0].pageX, y: evt.changedTouches[0].pageY };
            var position = ddm.subtractPoints(touchPosition, visual.startingPoint);
            Sys.UI.DomElement.setLocation(visual, Math.floor(position.x), Math.floor(position.y));
        }
        else
        {
            //mouse
            var mousePosition = { x: evt.clientX, y: evt.clientY };
            var scrollOffset = ddm.getScrollOffset(visual, /* recursive */true);
            var position = ddm.addPoints(ddm.subtractPoints(mousePosition, visual.startingPoint), scrollOffset);
            Sys.UI.DomElement.setLocation(visual, position.x, position.y);
        }
    },

    onDragEnd: function (canceled)
    {
        this._parent._isInDrag = false;
        var visual = this._parent.get_visual();
        this._parent.set_touchDragging(false);
        //Clear the visual
        if (visual)
        {
            visual.parentNode.removeChild(visual);
            this._parent.set_visual(null);
        }
        //Make sure all targets returned to normal
        $(this._parent.get_element()).find('th.drag-target').removeClass('drag-target');
    },

    /* IDropTarget Interface */

    // return a DOM element represents the container
    get_dropTargetElement: function ()
    {
        return this.get_element();
    },

    // if this draggable item can be dropped into this drop target
    canDrop: function (dragMode, dataType, data)
    {
        return (dataType == "SortedGridColumn" && data);
    },

    // drop this draggable item into this drop target
    drop: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridColumn" && data)
        {
            $(this.get_element()).removeClass('drag-target');
            var dataField = this._dataField;
            this._parent.finish_columnDrop(data.get_dataField(), this._columnIndex);
        }
    },

    // this method will be called when a draggable item is entering this drop target
    onDragEnterTarget: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridColumn" && data)
        {
            $(this.get_element()).addClass('drag-target');
        }
    },

    // this method will be called when a draggable item is leaving this drop target
    onDragLeaveTarget: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridColumn" && data)
        {
            $(this.get_element()).removeClass('drag-target');
        }
    },

    // this method will be called when a draggable item is hovering on this drop target
    onDragInTarget: function (dragMode, dataType, data)
    {
        if (dataType == "SortedGridColumn" && data)
        {
        }
    }
}
Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.SortedGridColumnBehavior', Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDropTarget);

if (typeof (Sys) != 'undefined')
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
        if (sender.get_object().Message === "Reload Page.")
        {
            //Force a reload
            window.location.href = window.location.href;
        }
    }
}  
