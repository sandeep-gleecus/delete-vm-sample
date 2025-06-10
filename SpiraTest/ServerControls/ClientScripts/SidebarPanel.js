var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.SidebarPanel = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.SidebarPanel.initializeBase(this, [element]);

    this._webServiceClass = null;
    this._projectId = -1;
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._bodyHeight = '';
    this._bodyWidth = '';
    this._displayRefresh = true;
    this._clientScriptControlId = null;
    this._clientScriptMethod = null;
    this._minWidth = 200;
    this._maxWidth = 1000;
    this._isOverNameDesc = false;
    this._bodyDiv = null;
    this._header = null;
    this._subheader = null;
    this._minimizedHeader = null;
    this._minimized = false;
    this._isInDrag = false;
    this._posX = -1;
    this._widthOffset = 5;

    this._resizeHandle = null;
    this._resizeHandler = null;
    this._documentMouseMoveHandler = null;
    this._documentMouseUpHandler = null;

    this._clickElements = new Array();
    this._otherClickHandlers = new Array();
}
Inflectra.SpiraTest.Web.ServerControls.SidebarPanel.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.SidebarPanel.callBaseMethod(this, 'initialize');

        //Create the shell of the control
        this._createChildControls();

        //Let other controls know we've initialized
        this.raise_init();
    },
    dispose: function ()
    {
        //Clear the various handlers
        this._clearClickElementHandlers();

        $removeHandler(this._resizeHandle, 'mousedown', this._resizeHandler);
        $removeHandler(document, 'mousemove', this._documentMouseMoveHandler);
        $removeHandler(document, 'mouseup', this._documentMouseUpHandler);
        $removeHandler(this._bodyDiv, 'scroll', this._scrollHandler);
        delete this._resizeHandler;
        delete this._resizeHandle;
        delete this._documentMouseMoveHandler;
        delete this._documentMouseUpHandler;
        delete this._scrollHandler;

        delete this._otherClickHandlers;
        delete this._clickElements;

        delete this._bodyDiv;
        delete this._header;
        delete this._subheader;
        delete this._minimizedHeader;

        Inflectra.SpiraTest.Web.ServerControls.SidebarPanel.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function ()
    {
        return this._element;
    },

    get_projectId: function ()
    {
        return this._projectId;
    },
    set_projectId: function (value)
    {
        this._projectId = value;
    },

    get_minimized: function ()
    {
        return this._minimized;
    },
    set_minimized: function (value)
    {
        this._minimized = value;
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

    get_bodyHeight: function ()
    {
        return this._bodyHeight;
    },
    set_bodyHeight: function (value)
    {
        this._bodyHeight = value;
    },

    get_bodyWidth: function ()
    {
        return this._bodyWidth;
    },
    set_bodyWidth: function (value)
    {
        this._bodyWidth = value;
    },

    get_minWidth: function ()
    {
        return this._minWidth;
    },
    set_minWidth: function (value)
    {
        this._minWidth = value;
    },

    get_maxWidth: function ()
    {
        return this._maxWidth;
    },
    set_maxWidth: function (value)
    {
        this._maxWidth = value;
    },
    get_displayRefresh: function () {
        return this._displayRefresh;
    },
    set_displayRefresh: function (value) {
        this._displayRefresh = value;
    },
    get_clientScriptControlId: function () {
        return this._clientScriptControlId;
    },
    set_clientScriptControlId: function (value) {
        this._clientScriptControlId = value;
    },
    get_clientScriptMethod: function () {
        return this._clientScriptMethod;
    },
    set_clientScriptMethod: function (value) {
        this._clientScriptMethod = value;
    },

    /*  =========================================================
    The event handler managers
    =========================================================  */
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
    _onResize: function (evt)
    {
        //This is called when dragging starts

        //Only allow left mouse button drag
        if (Sys.Browser.agent != Sys.Browser.Safari)
        {
            if (evt.button != Sys.UI.MouseButton.leftButton)
            {
                return;
            }
        }
        //Already dragging so exit
        if (this._isInDrag)
        {
            return;
        }
        //Flag that we are dragging
        this._isInDrag = true;
        evt.preventDefault();
    },
    _onScroll: function (evt)
    {
        this._bodyDiv.scrollLeft = 0;
    },
    _onDocumentMouseMove: function (evt)
    {
        //This is called during the drag
        if (this._isInDrag)
        {
            if (document.all)
            {
                //IE with XHTML standards mode on
                this._posX = (evt.clientX + document.documentElement.scrollLeft);
            }
            else
            {
                //Other browsers
                this._posX = (evt.clientX + window.pageXOffset);
            }

            //Reposition the dragging handle
            this._resizeHandle.style.backgroundColor = '#f46515';
            this._resizeHandle.style.left = this._posX + 'px';

            //This will make sure the content is not selected when we are dragging
            evt.preventDefault();
        }
    },
    _onDocumentMouseUp: function (evt)
    {
        //This is called when dragging stops

        //Make sure we're dragging
        if (this._isInDrag)
        {
            //Flag that we're not dragging anymore
            this._isInDrag = false;
            //Set the color back to the CSS defined one
            this._resizeHandle.style.backgroundColor = '';

            //Now we need to update the width of the navigation bar
            var x = $('.side-panel .panel').offset().left;
            var width = parseInt(this._posX - x - this._widthOffset);
            if (isNaN(width))
            {
                return;
            }
            //Make sure the width isn't too narrow or wide
            if (width < this._minWidth)
            {
                width = this._minWidth;
            }
            if (width > this._maxWidth)
            {
                width = this._maxWidth;
            }
            $('.side-panel > .panel .panel-body').outerWidth(width);

            //Now update the position of the resize handle(s) - to make sure that multiple handles are moved to same position
            $('.NavigationBarResize').offset({ left: x + width + this._widthOffset });

            //if on a unity page, refresh any wrapper to make the columns flow correctly
            if ($('.u-wrapper').length > 0 && typeof uWrapper_onResize == 'function') {
                uWrapper_onResize();
            }

            //if any charts (C3) are on the page, resize them to fit the panel
            this.resizeCharts();

            //Update the setting on the server - no need to do anything if successful
            var webService = this.get_webServiceClass();
            webService.NavigationBar_UpdateSettings(this.get_projectId(), null, width, null, null, Function.createDelegate(this, this.operation_failure));
        }
    },

    _onMinimize: function (evt)
    {
        //Need to hide the navigation bar
        this.hideNavigation();

        //Update the setting on the server - no need to do anything if successful
        var webService = this.get_webServiceClass();
        webService.NavigationBar_UpdateSettings(this.get_projectId(), null, null, true, null, Function.createDelegate(this, this.operation_failure));
    },
    _onMaximize: function (evt)
    {
        //Need to show the navigation bar
        this.showNavigation();

        //Update the setting on the server - no need to do anything if successful
        var webService = this.get_webServiceClass();
        webService.NavigationBar_UpdateSettings(this.get_projectId(), null, null, false, null, Function.createDelegate(this, this.operation_failure));
    },

    /*  =========================================================
    Public methods/functions
    =========================================================  */
    hideNavigation: function ()
    {
        //Hide the body and appropriate header rows
        this._header.style.display = 'none';
        this._bodyDiv.style.display = 'none';
        if (this._subheader)
        {
            this._subheader.style.display = 'none';
        }
        this._minimizedHeader.style.display = 'block';
        this._resizeHandle.style.display = 'none';
        $(this._minimizedHeader).addClass('minimized-heading-on');
        
        this.areAllMinimized();
        
    },
    showNavigation: function ()
    {
        this._header.style.display = 'block';
        this._bodyDiv.style.display = 'block';
        if (this._subheader)
        {
            this._subheader.style.display = 'block';
        }
        this._minimizedHeader.style.display = 'none';
        this._resizeHandle.style.display = 'block';
        $(this._minimizedHeader).removeClass('minimized-heading-on');
        $('.minimized-heading').removeClass('minimized-heading-all-min');

        this.resizeCharts();
    },
    areAllMinimized: function() 
    {
        var panels = $('.minimized-heading').length;
        var minimized = $('.minimized-heading-on').length;
        if(panels === minimized)
        {
            $('.minimized-heading').addClass('minimized-heading-all-min');
        }
        else
        {
            $('.minimized-heading').removeClass('minimized-heading-all-min');
        }
    },
    resizeCharts: function()
    {
        //if any charts (C3) are on the page, resize them to fit the panel
        if (typeof(sidepanelCharts) != 'undefined' && sidepanelCharts)
        {
            for (var chart in sidepanelCharts)
            {
                if (sidepanelCharts.hasOwnProperty(chart))
                {
                    sidepanelCharts[chart].resize();
                }
            }
        }
    },
    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        this.hide_spinner();
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

    //Displays the activity spinner
    display_spinner: function ()
    {
    },
    //Hides the activity spinner
    hide_spinner: function ()
    {
    },

    /*  =========================================================
    Internal methods/functions
    =========================================================  */
    _clearClickElementHandlers: function ()
    {
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._otherClickHandlers[i]);
            delete this._clickElements[i];
            delete this._otherClickHandlers[i];
        }
    },

    _createChildControls: function ()
    {
        //Add the controls for making the sidebar stretch/minimize and for refreshing its contents
        var panel = this.get_element();
        var panelHeading = $(panel).find('.panel-heading')[0];
        var panelBody = $(panel).find('.panel-body')[0];

        //Add the span display the refresh icon (but only if associated control id and method are present)
        if(this.get_displayRefresh())
        {
            if (this._clientScriptControlId !== null && this._clientScriptMethod !== null)
            {
            var a = d.ce('a');
            a.className = 'fas fa-sync';
            a.title = resx.Global_Refresh;
            a.href = 'javascript:void(0)';
            a.setAttribute("onclick", "$find('" + this._clientScriptControlId + "')." + this._clientScriptMethod);
            panelHeading.appendChild(a); 
            }
        }

        //Add the max/minimize icon
        var btn = d.ce('button');
        btn.type = 'button';
        panelHeading.appendChild(btn);
        btn.className = 'minimize';
        btn.title = resx.SidebarPanel_Minimize;
        this._header = panelHeading;
        if (this.get_minimized())
        {
            panelHeading.style.display = 'none';
        }

        //Add the event handler to the minimize icon
        var _onMinimizeHandler = Function.createDelegate(this, this._onMinimize);
        $addHandler(btn, 'click', _onMinimizeHandler);
        this._clickElements.push(btn);
        this._otherClickHandlers.push(_onMinimizeHandler);

        //Create the Minimized Header
        var miniHeader = d.ce('div');
        miniHeader.className = 'minimized-heading';
        panel.appendChild(miniHeader);

        //add contents to the minimized header
        var headerText = $(this._header).text();
        var miniHeaderText = d.ce('span');
        miniHeaderText.className = "minimized-heading-title";
        miniHeaderText.innerHTML = headerText;
        miniHeader.title = headerText;
        miniHeader.appendChild(miniHeaderText);

        btn = d.ce('button');
        btn.type = 'button';
        miniHeader.appendChild(btn);
        btn.className = 'maximize';
        btn.title = resx.SidebarPanel_Maximize;
        this._minimizedHeader = miniHeader;
        if (!this.get_minimized())
        {
            miniHeader.style.display = 'none';
        }

        //Add the event handler to the maximize icon
        var _onMaximizeHandler = Function.createDelegate(this, this._onMaximize);
        $addHandler(miniHeader, 'click', _onMaximizeHandler);
        this._clickElements.push(miniHeader);
        this._otherClickHandlers.push(_onMaximizeHandler);

        //Get the table body
        if (this.get_minimized())
        {
            var that = this;
            panelBody.style.display = 'none';
            $(this._minimizedHeader).addClass('minimized-heading-on minimized-heading-all-min');
            $(document).ready(function () { that.areAllMinimized() });
        }

        //Now access the div that contains the child table that scrolls
        this._bodyDiv = panelBody;
        this._bodyDiv.style.width = this.get_bodyWidth();
        this._bodyDiv.style.height = this.get_bodyHeight();

        //Need to trap scroll events since that causes issues with webkit
        this._scrollHandler = Function.createDelegate(this, this._onScroll);
        $addHandler(this._bodyDiv, 'scroll', this._scrollHandler);

        //Finally we need to create the resize handle and position
        var bodyWidth = parseInt(this.get_bodyWidth().substr(0, this.get_bodyWidth().length - 2));
        
        this._resizeHandle = d.ce('div');
        this.get_element().parentNode.appendChild(this._resizeHandle);
        this._resizeHandle.className = 'NavigationBarResize';
        var x = globalFunctions.getposOffset(this.get_element(), "left");
        var y = $('body').offset().top;
        this._resizeHandle.style.left = x + bodyWidth + this._widthOffset + "px";
        this._resizeHandle.style.top = y + "px";
        if (this.get_minimized())
        {
            this._resizeHandle.style.display = 'none';
        }

        //Add the drag handlers
        this._resizeHandler = Function.createDelegate(this, this._onResize);
        $addHandler(this._resizeHandle, 'mousedown', this._resizeHandler);
        this._documentMouseMoveHandler = Function.createDelegate(this, this._onDocumentMouseMove);
        $addHandler(document, 'mousemove', this._documentMouseMoveHandler);
        this._documentMouseUpHandler = Function.createDelegate(this, this._onDocumentMouseUp);
        $addHandler(document, 'mouseup', this._documentMouseUpHandler);

        //Make sure that the left position of the resize handle is reset on full load to ensure it is in the expected place
		window.addEventListener("load", function () {
            $(this._resizeHandle).offset({ left: $(this._resizeHandle).parent().width() });
        });
    }
}
Inflectra.SpiraTest.Web.ServerControls.SidebarPanel.registerClass('Inflectra.SpiraTest.Web.ServerControls.SidebarPanel', Sys.UI.Control);
        
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
