var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.ItemSelector = function(element)
{
    Inflectra.SpiraTest.Web.ServerControls.ItemSelector.initializeBase(this, [element]);
    this._webServiceClass = '';
    this._cssClass = '';
    this._projectId = -1;
    this._artifactTypeId = -1;
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._controlBaseId = '';
    this._table = null;
    this._dataSource = null;
    this._loadingComplete = false;
    this._autoLoad = true;
    this._multipleSelect = true;
    this._alternateSelect = true;
    this._checkboxes = new Array();
    this._rowIndex = 0;
    this._width = 0;
    this._height = 0;
    this._standardFilters = null;
    this._firstRowContainsFilters = false;
    
    this._itemImage = '';
    this._alternateItemImage = '';
    this._nameField = '';
    this._nameLegend = '';
    
    this._clickElements = new Array();
    this._otherClickHandlers = new Array();
    
    this._tooltips = new Array();
    this._tooltipOverHandlers = new Array();
    this._tooltipOutHandlers = new Array(); 
}
Inflectra.SpiraTest.Web.ServerControls.ItemSelector.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.ItemSelector.callBaseMethod(this, 'initialize');

        //Used to prefix all element IDs
        this._controlBaseId = this.get_element().id + '_';

        //Get the pixel width and height of the box
        this._width = this.get_element().style.width.substr(0, this.get_element().style.width.length - 2);
        this._height = this.get_element().style.height.substr(0, this.get_element().style.height.length - 2);

        //Create the shell of the control
        this._createChildControls();

        //Let other controls know we've initialized
        this.raise_init();

        //Now load in the data if autoload set
        if (this._autoLoad)
        {
            this.load_data();
        }
    },
    dispose: function ()
    {
        //Clear the various handlers
        this._clearTooltipHandlers();
        this._clearClickElementHandlers();

        delete this._tooltipOverHandlers;
        delete this._tooltipOutHandlers;
        delete this._tooltips;

        delete this._otherClickHandlers;
        delete this._clickElements;

        delete this._table;
        delete this._checkboxes;

        Inflectra.SpiraTest.Web.ServerControls.ItemSelector.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function ()
    {
        return this._element;
    },
    get_cssClass: function ()
    {
        return this._cssClass;
    },
    set_cssClass: function (value)
    {
        this._cssClass = value;
    },

    get_multipleSelect: function ()
    {
        return this._multipleSelect;
    },
    set_multipleSelect: function (value)
    {
        this._multipleSelect = value;
    },

    get_alternateSelect: function ()
    {
        return this._alternateSelect;
    },
    set_alternateSelect: function (value)
    {
        this._alternateSelect = value;
    },

    get_projectId: function ()
    {
        return this._projectId;
    },
    set_projectId: function (value)
    {
        this._projectId = value;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
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

    get_nameField: function ()
    {
        return this._nameField;
    },
    set_nameField: function (value)
    {
        this._nameField = value;
    },

    get_nameLegend: function ()
    {
        return this._nameLegend;
    },
    set_nameLegend: function (value)
    {
        this._nameLegend = value;
    },

    get_standardFilters: function ()
    {
        return this._standardFilters;
    },
    set_standardFilters: function (value)
    {
        this._standardFilters = value;
    },

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },
    get_loadingComplete: function ()
    {
        return this._loadingComplete;
    },
    clear_loadingComplete: function ()
    {
        this._loadingComplete = false;
    },

    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        this._dataSource = value;
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

    get_selectedItems: function ()
    {
        //Get selected checkboxes/radio buttons
        var items = new Array();
        for (var i = 0; i < this._checkboxes.length; i++)
        {
            var checkbox = this._checkboxes[i];
            if (checkbox.checked)
            {
                var primaryKey = checkbox.id.substring(this.get_element().id.length + 5, checkbox.id.length);
                items.push(primaryKey);
            }
        }
        return items;
    },
    get_artifactName: function (primaryKey)
    {
        //Get the name of an item if we know its key
        var dataSource = this.get_dataSource();
        var dataItems = dataSource.items;
        if (dataItems.length > 0)
        {
            for (var i = 0; i < dataItems.length; i++)
            {
                var dataItem = dataItems[i];
                if (dataItem.primaryKey == primaryKey)
                {
                    return dataItem.Fields.Name.textValue;
                }
            }
        }
        return '';
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
    add_selected: function (handler)
    {
        this.get_events().addHandler('selected', handler);
    },
    remove_selected: function (handler)
    {
        this.get_events().removeHandler('selected', handler);
    },
    raise_selected: function (primaryKey)
    {
        var h = this.get_events().getHandler('selected');
        if (h) h(primaryKey);
    },

    /*  =========================================================
    Event Handlers
    =========================================================  */
    _onNameDescMouseOver: function (sender, e)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        e.thisRef._isOverNameDesc = true;   //Set the flag since asynchronous
        //Now get the real tooltip via Ajax web-service call
        var artifactId = e.primaryKey;
        var webServiceClass = e.webServiceClass;
        webServiceClass.RetrieveNameDesc(e.thisRef._projectId, artifactId, null, Function.createDelegate(e.thisRef, e.thisRef.retrieveNameDesc_success));
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
    _onNameDescMouseOut: function (sender, e)
    {
        hideddrivetip();
        e.thisRef._isOverNameDesc = false;
    },
    _onSelectItem: function (sender, e)
    {
        //Mark as checked if radio (defeat IE bug)
        if (e.checkbox.type == 'radio')
        {
            e.checkbox.checked = true;
        }
        //Raise the selected event
        e.thisRef.raise_selected(e.primaryKey);
    },
    _onSelectClearAllClick: function (evt)
    {
        //Toggle all the checkboxes
        for (var i = 0; i < this._checkboxes.length; i++)
        {
            this._checkboxes[i].checked = !this._checkboxes[i].checked;
        }
    },


    /*  =========================================================
    Public methods/functions
    =========================================================  */
    load_data: function ()
    {
        //Clear any existing error messages
        globalFunctions.clear_errors($get(this.get_errorMessageControlId()));

        //Get the list of available items
        //Check which type of service we have (ordered vs. sorted) if we don't have the special item selector method available
        this.display_spinner();
        if (this.get_webServiceClass().ItemSelector_Retrieve)
        {
            this._firstRowContainsFilters = false;
            this.get_webServiceClass().ItemSelector_Retrieve(this.get_projectId(), this._standardFilters, Function.createDelegate(this, this.retrieve_available_success), Function.createDelegate(this, this.operation_failure));
        }
        else
        {
            this._firstRowContainsFilters = true;
            if (this.get_webServiceClass().SortedList_Retrieve)
            {
                this.get_webServiceClass().SortedList_Retrieve(this.get_projectId(), this._standardFilters, null, Function.createDelegate(this, this.retrieve_available_success), Function.createDelegate(this, this.operation_failure));
            }
            if (this.get_webServiceClass().OrderedList_Retrieve)
            {
                this.get_webServiceClass().OrderedList_Retrieve(this.get_projectId(), this._standardFilters, Function.createDelegate(this, this.retrieve_available_success), Function.createDelegate(this, this.operation_failure));
            }
        }
    },
    retrieve_available_success: function (dataSource)
    {
        //Set the datasource
        this.set_dataSource(dataSource);

        //Databind
        this.dataBind();

        //Mark as complete
        this._loadingComplete = true;
        this.hide_spinner();
    },

    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        this.hide_spinner();
        //Display validation exceptions in a friendly manner    
        var errorMessage = 'Please check Application Event log for detailed message.';
        var errorType = 'System Error';
        if (exception.get_exceptionType())
        {
            errorType = exception.get_exceptionType();
        }
        if (exception.get_message())
        {
            errorMessage = exception.get_message();
        }
        var errorDisplay = errorType + ': ' + errorMessage + ' - ' + exception.get_stackTrace();

        this.display_error(errorDisplay);
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

    dataBind: function (tr)
    {
        //Clear any existing content (if tr not set)
        if (tr == undefined)
        {
            this._checkboxes = new Array();
            this._clearContent(this._table);
        }

        var dataSource = this.get_dataSource();
        if (!dataSource)
        {
            return;
        }
        var dataItems = dataSource.items;
        if (dataItems && dataItems.length > 0)
        {
            var tbody = d.ce('tbody');
            this._table.appendChild(tbody);
            //See if the first row contains filters which are not used
            var startRow = (this._firstRowContainsFilters) ? 1 : 0;
            for (var i = startRow; i < dataItems.length; i++)
            {
                var dataRow = dataItems[i];
                var tr = d.ce('tr');
                tbody.appendChild(tr);
                this._dataBindRow(dataRow, tr);
            }
        }
    },

    /*  =========================================================
    Internal methods/functions
    =========================================================  */
    _dataBindRow: function (dataRow, tr)
    {
        var td, img, checkbox;
        var primaryKey = dataRow.primaryKey;

        //Checkbox/Radio
        td = d.ce('td');
        td.style.width = '20px';
        tr.appendChild(td);
        checkbox = d.ce('input');
        if (this._multipleSelect)
        {
            checkbox.type = 'checkbox';
        }
        else
        {
            //Specify the radio selection name 
            checkbox.type = 'radio';
            checkbox.name = this.get_element().id;
        }
        if (!this._alternateSelect && dataRow.alternate)
        {
            checkbox.disabled = true;
        }
        else
        {
            //Add the click handler
            var _onSelectItemHandler = Function.createCallback(this._onSelectItem, { thisRef: this, primaryKey: dataRow.primaryKey, checkbox: checkbox });
            $addHandler(checkbox, 'click', _onSelectItemHandler);
            this._clickElements.push(checkbox);
            this._otherClickHandlers.push(_onSelectItemHandler);
        }
        checkbox.id = this.get_element().id + '_avl_' + primaryKey;
        td.appendChild(checkbox);
        this._checkboxes.push(checkbox);

        //Name/icon
        dataField = dataRow.Fields[this._nameField];
        td = d.ce('td');
        td.style.width = '280px';
        tr.appendChild(td);

        //Now the folder/file icon
        img = d.ce('img');
        img.className = "w4 h4";
        var imgSrc = '';
        if (dataRow.summary)
        {
            imgSrc = this.get_summaryItemImage();
        }
        else
        {
            if (dataRow.alternate && this.get_alternateItemImage() != '')
            {
                imgSrc = this.get_alternateItemImage();
            }
            else
            {
                imgSrc = this.get_itemImage();
            }
        }
        if (imgSrc == '')
        {
            //This is the case when the icon is specified per row
            if (dataField.tooltip != '')
            {
                imgSrc = 'Images/' + dataField.tooltip;
            }
        }
        img.src = this._themeFolder + '/' + imgSrc;
        td.appendChild(img);

        var name = dataField.textValue;
        if (name.length > 50)
        {
            name = name.substr(0, 50) + '...';
        }
        var label = d.ce('label');
        label.htmlFor = checkbox.id;
        td.appendChild(label);
        label.appendChild(document.createTextNode(name));

        //Add the asynchronous tooltip handler
        var _nameDescMouseOverHandler = Function.createCallback(this._onNameDescMouseOver, { thisRef: this, primaryKey: dataRow.primaryKey, webServiceClass: this.get_webServiceClass() });
        var _nameDescMouseOutHandler = Function.createCallback(this._onNameDescMouseOut, { thisRef: this, primaryKey: -1, webServiceClass: '' });
        $addHandler(label, 'mouseover', _nameDescMouseOverHandler);
        $addHandler(label, 'mouseout', _nameDescMouseOutHandler);
        this._tooltips.push(label);
        this._tooltipOverHandlers.push(_nameDescMouseOverHandler);
        this._tooltipOutHandlers.push(_nameDescMouseOutHandler);
    },
    _createChildControls: function ()
    {
        //Add the cells for the containing table
        var table = this.get_element();
        var tbody = d.ce('tbody');
        table.appendChild(tbody);
        var mainRow = d.ce('tr');
        tbody.appendChild(mainRow);

        //Create the cell that holds all the content
        var td = d.ce('td');
        mainRow.appendChild(td);
        td.className = 'Main';

        //Create the header table
        var headerTable = d.ce('table');
        td.appendChild(headerTable);
        headerTable.className = 'Header';
        headerTable.style.width = (parseInt(this._width) - 18) + 'px';
        var thead = d.ce('thead');
        headerTable.appendChild(thead);
        var tr = d.ce('tr');
        thead.appendChild(tr);
        var th = d.ce('th');
        th.style.textAlign = 'center';
        th.style.width = '20px';
        tr.appendChild(th);
        //If we allow multiple selects, make the select all/none option available
        if (this._multipleSelect)
        {
            img = d.ce('button');
            img.type = 'button';
            //Add the click handler
            var _onSelectClearAllHandler = Function.createDelegate(this, this._onSelectClearAllClick);
            $addHandler(img, 'click', _onSelectClearAllHandler);
            this._clickElements.push(img);
            this._otherClickHandlers.push(_onSelectClearAllHandler);
            img.className = 'fas fa-check u-btn u-btn-minimal';
        }
        else
        {
            img = d.ce('span');
            img.className = 'fas fa-check';
        }
        th.appendChild(img);
        th = d.ce('th');
        th.className = 'Name';
        th.style.width = (parseInt(this._width) - 40) + 'px';
        tr.appendChild(th);
        th.appendChild(d.createTextNode(this._nameLegend));


        //Now create the div that contains the child table that scrolls
        var bodyDiv = d.ce('div');
        bodyDiv.style.width = (parseInt(this._width) - 18) + 'px';
        bodyDiv.style.height = (parseInt(this._height) - 34) + 'px';
        bodyDiv.className = 'Body';
        td.appendChild(bodyDiv);

        //Now create the inner table that scrolls within this div
        this._table = d.ce('table');
        this._table.style.width = (parseInt(this._width) - 40) + 'px';
        bodyDiv.appendChild(this._table);
    },

    _clearClickElementHandlers: function ()
    {
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._otherClickHandlers[i]);
            delete this._clickElements[i];
            delete this._otherClickHandlers[i];
        }
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
Inflectra.SpiraTest.Web.ServerControls.ItemSelector.registerClass('Inflectra.SpiraTest.Web.ServerControls.ItemSelector', Sys.UI.Control);
        
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
            window.location.href = window.location.protocol + "//" + window.location.host + window.location.pathname.substring(0,window.location.pathname.indexOf('/',1)) + '/Login.aspx?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);   
        }   
    }   
}  