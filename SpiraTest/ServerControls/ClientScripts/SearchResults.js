var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.SearchResults = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.SearchResults.initializeBase(this, [element]);
    this._webServiceClass = '';
    this._cssClass = '';
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._table = null;
    this._dataSource = null;
    this._autoLoad = true;
    this._rowIndex = 0;
    this._width = 0;
    this._height = 0;
    this._keywords = '';
    this._resultCount = 0;
    this._pageSize = 250;
    this._pageIndex = 0;
    this._loaded = false;

    this._table = null;
    this._tbody = null;
}
Inflectra.SpiraTest.Web.ServerControls.SearchResults.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SearchResults.callBaseMethod(this, 'initialize');

        //Used to prefix all element IDs
        this._controlBaseId = this.get_element().id + '_';

        //Get the pixel width and height of the box
        this._width = this.get_element().style.width.substr(0, this.get_element().style.width.length - 2);
        this._height = this.get_element().style.height.substr(0, this.get_element().style.height.length - 2);

        /* Event delegates */
        this._onScrollHandler = Function.createDelegate(this, this._onScroll);
        $addHandlers(this._element, { 'scroll': this._onScrollHandler }, this);

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
        $clearHandlers(this.get_element());
        if (this._onScrollHandler) delete this._onScrollHandler;

        //Clear any results
        this._clearResults();

        //Delete any item references
        delete this._tbody;
        delete this._table;

        Inflectra.SpiraTest.Web.ServerControls.SearchResults.callBaseMethod(this, 'dispose');
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

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },

    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        this._dataSource = value;
    },

    get_keywords: function ()
    {
        return this._keywords;
    },
    set_keywords: function (value)
    {
        this._keywords = value;
    },

    get_resultCount: function ()
    {
        return this._resultCount;
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
    _onScroll: function (e)
    {
        //Make sure we've already loaded and that we have more results
        if (this._loaded && this._resultCount > this._pageIndex)
        {
            //Make sure that the user scrolled down to the bottom
            var scrollBottom = this.get_element().scrollHeight - this.get_element().scrollTop - this._height;
            if (scrollBottom < 25)
            {
                this._pageIndex += this._pageSize;
                var webService = this.get_webServiceClass();
                globalFunctions.display_spinner();
                webService.RetrieveResults(this._keywords, this._pageIndex, this._pageSize, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
            }
        }
    },

    /*  =========================================================
    Internal methods/functions
    =========================================================  */
    _createChildControls: function ()
    {
        //Add the table to the containing DIV
        var div = this.get_element();
        this._table = d.ce('table');
        div.appendChild(this._table);
        this._tbody = d.ce('tbody');
        this._table.appendChild(this._tbody);
    },

    _clearResults: function ()
    {
        var element = this._tbody;
        if (element.firstChild)
        {
            while (element.firstChild)
            {
                element.removeChild(element.firstChild);
            }
        }
    },

    /*  =========================================================
    Public methods/functions
    =========================================================  */
    load_data: function (keywords)
    {
        //Clear any existing error messages
        globalFunctions.clear_errors($get(this.get_errorMessageControlId()));

        //Clear any existing results and reset the page index
        this._clearResults();
        this._pageIndex = 0;

        //Set the keywords if provided
        if (keywords)
        {
            this._keywords = keywords;
        }

        //Get the list of search items to be displayed
        var webService = this.get_webServiceClass();
        globalFunctions.display_spinner();
        webService.RetrieveResults(this._keywords, this._pageIndex, this._pageSize, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
    },

    retrieve_success: function (dataSource)
    {
        //Get the total result count
        this._resultCount = dataSource.Count;

        //Set the datasource
        this.set_dataSource(dataSource.Values);

        //Databind
        this.dataBind();

        //Mark as complete
        globalFunctions.hide_spinner();

        //Make sure the main div is scrolled to the top if this is the first load
        if (!this._loaded)
        {
            this.get_element().scrollTop = 0;
        }

        //Raise the loaded event
        this._loaded = true;
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

    dataBind: function ()
    {
        var dataSource = this.get_dataSource();

        //Add each row to the search results
        for (var i = 0; i < dataSource.length; i++)
        {
            var dataRow = dataSource[i];

            //Add a table row
            var tr = d.ce('tr');
            //The icon cell
            var td = d.ce('td');
            tr.appendChild(td);
            if (dataRow.Icon && dataRow.Icon != '')
            {
                var img = d.ce('img');
                td.appendChild(img);
                //Prepend the images folder unless we have an absolute path
                if (dataRow.Icon.substr(0, 1) == '/')
                {
                    img.src = dataRow.Icon;
                }
                else
                {
                    img.src = this.get_themeFolder() + '/Images/' + dataRow.Icon;
                }
                img.alt = dataRow.IconAlt;
            }

            //The title/desc cell
            td = d.ce('td');
            tr.appendChild(td);

            //Add the link
            var link = d.ce('a');
            link.className = 'search-results-link';
            td.appendChild(link);
            link.href = dataRow.Url;
            var title = dataRow.Token + ' - ' + dataRow.Title;
            link.appendChild(d.createTextNode(title));

            //Add the project name
            if (dataRow.ProjectName && dataRow.ProjectName != '')
            {
                var span = d.ce('span');
                td.appendChild(span);
                span.className = 'Project';
                var projectName = ' (' + dataRow.ProjectName + ')';
                span.appendChild(d.createTextNode(projectName));
            }

            //Add the description
            if (dataRow.Description && dataRow.Description != '')
            {
                td.appendChild(d.ce('br'));
                td.appendChild(d.createTextNode(dataRow.Description));
            }

            //Add the date and project name
            if (dataRow.LastUpdateDate)
            {
                var p = d.ce('p');
                td.appendChild(p);
                p.className = 'Date';
                p.appendChild(d.createTextNode(dataRow.LastUpdateDate));
            }

            //Add the table to the DOM
            this._tbody.appendChild(tr);
        }
    }
}
Inflectra.SpiraTest.Web.ServerControls.SearchResults.registerClass('Inflectra.SpiraTest.Web.ServerControls.SearchResults', Sys.UI.Control);
        
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