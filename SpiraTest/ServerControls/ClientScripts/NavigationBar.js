var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.NavigationBar = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.NavigationBar.initializeBase(this, [element]);

    this._webServiceClass = null;
    this._projectId = -1;
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._dataSource = null;
    this._loadingComplete = false;
    this._autoLoad = true;
    this._itemImage = '';
    this._summaryItemImage = '';
    this._expandedItemImage = '';
    this._alternateItemImage = '';
    this._bodyHeight = '';
    this._bodyWidth = '';
    this._listScreenCaption = '';
    this._listScreenUrl = '';
    this._itemBaseUrl = '';
    this._isOverNameDesc = false;
    this._list = null;
    this._lastIndent = '';
    this._dropdown = null;
    this._refreshIcon = null;
    this._displayMode = 1;  //Default to Filtered Items
    this._selectedItemId = null;
    this._containerId = null;
    this._bodyDiv = null;
    this._header = null;
    this._subheader = null;
    this._footer = null;
    this._minimizedHeader = null;
    this._minimizedHeaderSpan = null;
    this._minimized = false;
    this._includeAssigned = true;
    this._includeDetected = false;
    this._includeTestCase = false;
    this._includeTestSet = false;
    this._includeRelease = false;
    this._includeRequirement = false;
    this._isInDrag = false;
    this._posX = -1;
    this._widthOffset = 5;
    this._paginationOptions = null;
    this._initialLoad = true;
    this._enableLiveLoading = false;
    this._formManagerId = '';

    this._resizeHandle = null;
    this._resizeHandler = null;
    this._documentMouseMoveHandler = null;
    this._documentMouseUpHandler = null;
    this._ddlPaginationOptions = null;
    this._paginationOptionsChangedHandler = null;

    this._tooltips = new Array();
    this._tooltipOverHandlers = new Array();
    this._tooltipOutHandlers = new Array();

    this._clickElements = new Array();
    this._otherClickHandlers = new Array();
}
Inflectra.SpiraTest.Web.ServerControls.NavigationBar.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.NavigationBar.callBaseMethod(this, 'initialize');

        //Create the shell of the control
        this._createChildControls();

        //Let other controls know we've initialized
        this.raise_init();

        //Now load in the data if autoload set
        if (this._autoLoad)
        {
            this.load_data();
        }

        //Finally populate the pagination options
        var webServiceClass = this.get_webServiceClass();
        webServiceClass.NavigationBar_RetrievePaginationOptions(this.get_projectId(), Function.createDelegate(this, this.retreivePagination_success), Function.createDelegate(this, this.operation_failure));
    },
    dispose: function ()
    {
        //Clear the various handlers
        this._clearTooltipHandlers();
        this._clearClickElementHandlers();

        this._dropdown.remove_selectedItemChanged(this._dropdownHandler);
        delete this._dropdownHandler;

        $removeHandler(this._refreshIcon, 'click', this._refreshIconClickHandler);
        $removeHandler(this._resizeHandle, 'mousedown', this._resizeHandler);
        $removeHandler(document, 'mousemove', this._documentMouseMoveHandler);
        $removeHandler(document, 'mouseup', this._documentMouseUpHandler);
        delete this._refreshIconClickHandler;
        delete this._resizeHandler;
        delete this._resizeHandle;
        delete this._documentMouseMoveHandler;
        delete this._documentMouseUpHandler;
        delete this._paginationOptionsChangedHandler;

        delete this._tooltipOverHandlers;
        delete this._tooltipOutHandlers;
        delete this._tooltips;

        delete this._otherClickHandlers;
        delete this._clickElements;

        delete this._bodyDiv;
        delete this._dropdown;
        delete this._refreshIcon;
        this._clearContent(this.get_list());
        delete this._list;
        delete this._header;
        delete this._subheader;
        delete this._footer;
        delete this._minimizedHeader;
        delete this._ddlPaginationOptions;

        Inflectra.SpiraTest.Web.ServerControls.NavigationBar.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function ()
    {
        return this._element;
    },
    get_list: function ()
    {
        return this._list;
    },

    get_projectId: function ()
    {
        return this._projectId;
    },
    set_projectId: function (value)
    {
        this._projectId = value;
    },

    get_selectedItemId: function ()
    {
        return this._selectedItemId;
    },
    set_selectedItemId: function (value)
    {
        this._selectedItemId = value;
    },

    get_containerId: function ()
    {
        return this._containerId;
    },
    set_containerId: function (value)
    {
        this._containerId = value;
    },

    get_displayMode: function ()
    {
        return this._displayMode;
    },
    set_displayMode: function (value)
    {
        this._displayMode = value;
    },

    get_minimized: function ()
    {
        return this._minimized;
    },
    set_minimized: function (value)
    {
        this._minimized = value;
    },

    get_includeAssigned: function ()
    {
        return this._includeAssigned;
    },
    set_includeAssigned: function (value)
    {
        this._includeAssigned = value;
    },

    get_includeDetected: function ()
    {
        return this._includeDetected;
    },
    set_includeDetected: function (value)
    {
        this._includeDetected = value;
    },

    get_includeTestCase: function ()
    {
        return this._includeTestCase;
    },
    set_includeTestCase: function (value)
    {
        this._includeTestCase = value;
    },

    get_includeTestSet: function ()
    {
        return this._includeTestSet;
    },
    set_includeTestSet: function (value)
    {
        this._includeTestSet = value;
    },

    get_includeRelease: function ()
    {
        return this._includeRelease;
    },
    set_includeRelease: function (value)
    {
        this._includeRelease = value;
    },

    get_includeRequirement: function ()
    {
        return this._includeRequirement;
    },
    set_includeRequirement: function (value)
    {
        this._includeRequirement = value;
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
    get_loadingComplete: function ()
    {
        return this._loadingComplete;
    },
    clear_loadingComplete: function ()
    {
        this._loadingComplete = false;
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

    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        this._dataSource = value;
    },

    get_paginationOptions: function ()
    {
        return this._paginationOptions;
    },
    set_paginationOptions: function (value)
    {
        this._paginationOptions = value;
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
    get_expandedItemImage: function ()
    {
        return this._expandedItemImage;
    },
    set_expandedItemImage: function (value)
    {
        this._expandedItemImage = value;
    },

    get_alternateItemImage: function ()
    {
        return this._alternateItemImage;
    },
    set_alternateItemImage: function (value)
    {
        this._alternateItemImage = value;
    },

    get_listScreenCaption: function ()
    {
        return this._listScreenCaption;
    },
    set_listScreenCaption: function (value)
    {
        this._listScreenCaption = value;
    },

    get_listScreenUrl: function ()
    {
        return this._listScreenUrl;
    },
    set_listScreenUrl: function (value)
    {
        this._listScreenUrl = value;
    },

    get_itemBaseUrl: function ()
    {
        return this._itemBaseUrl;
    },
    set_itemBaseUrl: function (value)
    {
        this._itemBaseUrl = value;
    },

    get_formManagerId: function ()
    {
        return this._formManagerId;
    },
    set_formManagerId: function (value)
    {
        this._formManagerId = value;
    },

    get_enableLiveLoading: function ()
    {
        return this._enableLiveLoading;
    },
    set_enableLiveLoading: function (value)
    {
        this._enableLiveLoading = value;
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
    _onPaginationOptionsChanged: function (e)
    {
        if (!this._initialLoad)
        {
            //Change the number of rows displayed per page
            var dropdown = this._ddlPaginationOptions;
            var itemsPerPage = dropdown.get_selectedItem().get_value();

            //Now call the webservice to update the pagination options and reload the data
            var webServiceClass = this.get_webServiceClass();
            webServiceClass.NavigationBar_UpdatePagination(this.get_projectId(), itemsPerPage, -1, Function.createDelegate(this, this._paginationOptionsChanged_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    _paginationOptionsChanged_success: function ()
    {
        //Clear the existing content
        this._clearContent(this.get_list());

        //Reload the data
        this.load_data();
    },
    _onRefreshIconClick: function (evt)
    {
        //Clear the content and reload the data
        this.refresh_data();
    },
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
            if (width < 200)
            {
                width = 200;
            }
            if (width > 1000)
            {
                width = 1000;
            }
            $('.side-panel > .panel .panel-body').outerWidth(width);

            //Now update the position of the resize handle(s) - to make sure that multiple handles are moved to same position
            $('.NavigationBarResize').offset({ left: x + width + this._widthOffset });

            //if on a unity page, refresh any wrapper to make the columns flow correctly
            if ($('.u-wrapper').length > 0 && typeof uWrapper_onResize == 'function') {
                uWrapper_onResize();
            }

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
    _onNameDescMouseOver: function (sender, e)
    {
        //If we're dragging, don't fire tooltips
        if (!e.thisRef._isInDrag)
        {
            //Display the loading message
            ddrivetip(resx.Global_Loading);
            e.thisRef._isOverNameDesc = true;   //Set the flag since asynchronous
            //Now get the real tooltip via Ajax web-service call
            var artifactId = e.primaryKey;
            var webService = e.thisRef.get_webServiceClass();
            webService.RetrieveNameDesc(e.thisRef._projectId, artifactId, null, Function.createDelegate(e.thisRef, e.thisRef.retrieveNameDesc_success), Function.createDelegate(e.thisRef, e.thisRef.retrieveNameDesc_failure));
        }
    },
    retrieveNameDesc_success: function (tooltipData)
    {
        if (this._isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    },
    retrieveNameDesc_failure: function (exception)
    {
        //Fail quietly
    },
    _onNameDescMouseOut: function (sender, e)
    {
        hideddrivetip();
        e.thisRef._isOverNameDesc = false;
    },

    _onExpandCollapseNode: function (sender, e)
    {
        //Call the expand or collapse handlers as appropriate
        var img = e.img;
        if (img.classList.contains("js-Collapsed"))
        {
            e.thisRef.expandNode(e);
        }
        else
        {
            e.thisRef.collapseNode(e);
        }
    },

    _refreshSelectedItem: function()
    {
        //Refreshes the selected item
        //$('#cplMainContent_navIncidentList li[data-tstid]').removeClass('Selected');
        $('#' + this.get_element().id + ' li[data-tstid]').removeClass('Selected');
        $('#' + this.get_element().id + ' li[data-tstid="' + this._selectedItemId + '"]').addClass('Selected');
    },

    _liveLoadForm: function(evt, context)
    {
        //Ignore shift/ctrl clicks
        if (!evt.shiftKey && !evt.ctrlKey && !evt.metaKey)
        {
            var thisRef = context.thisRef;
            var artifactId = context.artifactId;
            evt.stopPropagation();
            evt.preventDefault();

            // ignore clicking on the current artifact id
            if (artifactId !== SpiraContext.ArtifactId)
            {
                //Set the new selected item
                thisRef.set_selectedItemId(artifactId);
                thisRef._refreshSelectedItem();

                //update the spira context mode
                SpiraContext.Mode = "update";

                //make sure initialBuildLoad, if it exists (only on the Incident Details page), is set to false
                //after initial page load it should be false, but may not be if the Incident does not have a resolved release
                if (typeof(initialBuildLoad) != undefined)
                {
                    initialBuildLoad = false;
                }

                //Load the data in the form manager
                var ajxFormManager = $find(thisRef._formManagerId);
                if (ajxFormManager)
                {
                    ajxFormManager.set_primaryKey(artifactId, true);
                    var success = ajxFormManager.load_data();
                    if (success)
                    {
                        //Also need to rewrite the URL to match
                        if (history && history.pushState)
                        {
                            var clickedElement = evt.target;
                            if (clickedElement)
                            {
                                //We set the URL of the page to match the item we're loading
                                //See if we have a custom url
                                if (context.customUrl && context.customUrl != '')
                                {
                                    history.pushState({ artifactId: artifactId }, null, context.customUrl);
                                }
                                else
                                {
									var href = thisRef.get_itemBaseUrl().replace(globalFunctions.artifactIdToken, '' + artifactId);
									//Check to see if the URL contains a tab name at the end (eg "...54/Overview.aspx")
									//This tab name is set server side so will not necessarily be correct
									var tabNameRegex = new RegExp("/" + SpiraContext.ArtifactTabNameInitial + ".aspx");
									if (href.match(tabNameRegex))
									{
										//If we have a tab name update it to the current one the user is on
										href = href.replace("/" + SpiraContext.ArtifactTabNameInitial + ".aspx", "/" + SpiraContext.ArtifactTabName + ".aspx")
									}
                                    history.pushState({ artifactId: artifactId }, null, href);
                                }
                            }
                        }
                    }
                }
            }
        }
    },

    //Handles browser pop state - eg when hitting browser back button
    _handlePopState: function (event, thisRef)
    {
        //check if a data value for the back/forward page was stored using pushState above
        var artifactIdPrevious = event.state && event.state.artifactId ? event.state.artifactId : null;
        var artifactIdToLoad = artifactIdPrevious || SpiraContext.ArtifactIdOnPageLoad;

        // live reload the data if we are actually moving to a different artifact id
        if (artifactIdToLoad)
        {
            //first update the navbar ui to reflect the new id
            thisRef.set_selectedItemId(artifactIdToLoad);
            thisRef._refreshSelectedItem();

            //then update the form manager
            var ajxFormManager = $find(thisRef._formManagerId);
            if (ajxFormManager)
            {
                ajxFormManager.set_primaryKey(artifactIdToLoad, true);
                ajxFormManager.load_data();
            }
            else
            {
                window.location = location;
            }
        }
    },


    /*  =========================================================
    Public methods/functions
    =========================================================  */
    hideNavigation: function ()
    {
        //Hide the body and appropriate header/footer rows
        this._header.style.display = 'none';
        this._bodyDiv.style.display = 'none';
        if (this._subheader)
        {
            this._subheader.style.display = 'none';
        }
        this._minimizedHeader.style.display = 'block';
        this._resizeHandle.style.display = 'none';
        this._footer.style.display = 'none';
        $(this._minimizedHeader).addClass('minimized-heading-on');
        var headerText = $('#_text').val();
        this._minimizedHeaderSpan.innerHTML = headerText;
        this._minimizedHeader.title = headerText;

        this.areAllMinimizedNavBar();
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
        this._footer.style.display = 'block';
        $(this._minimizedHeader).removeClass('minimized-heading-on');
        $('.minimized-heading').removeClass('minimized-heading-all-min');
    },
    areAllMinimizedNavBar: function () {
        var panels = $('.minimized-heading').length;
        var minimized = $('.minimized-heading-on').length;
        if (panels === minimized) {
            $('.minimized-heading').addClass('minimized-heading-all-min');
        }
        else
        {
            $('.minimized-heading').removeClass('minimized-heading-all-min');
        }
    },
    refresh_data: function (dontClearMessages)
    {
        //Now reload the data
        this.load_data(true, dontClearMessages);
    },
    load_data: function (clearExisting, dontClearMessages)
    {
        //Clear any existing error messages
        if (!dontClearMessages)
        {
            globalFunctions.clear_errors($get(this.get_errorMessageControlId()));
        }

        //Get the list of items to be displayed in the navigation bar
        var context = { clearExisting: clearExisting };
        var webService = this.get_webServiceClass();
        var indentLevel = '';
        this.display_spinner();
        webService.NavigationBar_RetrieveList(this.get_projectId(), indentLevel, this.get_displayMode(), this.get_selectedItemId(), this.get_containerId(), Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure), context);
    },
    retrieve_success: function (dataSource, context)
    {
        //Set the datasource
        this.set_dataSource(dataSource);

        //Clear the existing content
        if (context.clearExisting)
        {
            this._clearContent(this.get_list());
        }

        //Databind
        this.dataBind();

        //Make sure selected li is in view
        var scrollTo = $('.nav-bar-panel li.Selected'),
            container = $('.nav-bar-panel');
        if (scrollTo.offset())
        {
            container.scrollTop(scrollTo.offset().top - container.offset().top + container.scrollTop());
        }

        //Mark as complete
        this._loadingComplete = true;
        this.hide_spinner();

        //Raise the loaded event
        this.raise_loaded();

        //Add any event listeners
        //Need to add event listener for handling browsing back on load - so that listener is always running
        var _this = this;
        window.addEventListener("popstate", function (event) {
            _this._handlePopState(event, _this);
        }); 

    },

    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        this.hide_spinner();

        //If we have an authorization failure, display that as a pseudo-item
        if (exception.get_message() == globalFunctions.authorizationMessage)
        {
            var list = this.get_list();
            var li = d.ce('li');
            list.appendChild(li);
            li.appendChild(d.createTextNode(resx.NavigationBar_NotAuthorizedToView));
        }
        else
        {
            //Display validation exceptions in a friendly manner
            var messageBox = document.getElementById(this.get_errorMessageControlId());
            globalFunctions.display_error(messageBox, exception);
        }
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

    dataBind: function (ul)
    {
        var dataSource = this.get_dataSource();
        var list = this.get_list();
        this._lastIndent = '';
        if (dataSource.length > 0)
        {
            //Need to iterate through and make sure that we don't have lower indents than the starting one
            var lowestIndent = dataSource[0].indent.length;
            for (var i = 0; i < dataSource.length; i++)
            {
                var dataRow = dataSource[i];
                if (dataRow.indent.length < lowestIndent)
                {
                    lowestIndent = dataRow.indent.length;
                }
            }

            //If we are passed an indent level, then only add those rows to the existing ul
            if (!ul)
            {
                ul = list;
            }

            //See if we need to prepend any levels
            if (lowestIndent < dataSource[0].indent.length)
            {
                var numberLevels = (dataSource[0].indent.length - lowestIndent) / 3;
                for (var i = 0; i < numberLevels; i++)
                {
                    var li = d.ce('li');
                    ul.appendChild(li);
                    ul = d.ce('ul');
                    li.appendChild(ul);
                }
            }

            var prevLi = null;
            for (var i = 0; i < dataSource.length; i++)
            {
                var dataRow = dataSource[i];

                //Check the indent level to see if we need to create or close the previous UL
                var indent = dataRow.indent;
                var isSiblingOfLastItem = this._lastIndent != '' && indent.length === this._lastIndent.length;
                var isChildOfLastItem = this._lastIndent != '' && indent.length - 3 === this._lastIndent.length;
                var isOutdentedFromLastItem = this._lastIndent != '' && indent.length < this._lastIndent.length;
                var isFirstItem = this._lastIndent == '';

                // create a new UL if we have a child of the last item
                if (isChildOfLastItem)
                {
                    //Need to create a child UL
                    ul = d.ce('ul');
                    ul.className = 'Child';
                    prevLi.appendChild(ul);
                }
                // outdent back out to the correct UL for an item that is more outdented than the last item
                if (isOutdentedFromLastItem)
                {
                    var recursiveLength = this._lastIndent.length;
                    while (indent.length < recursiveLength)
                    {
                        //Need to get the parent UL (UI/LI/UL)
                        if (ul.parentNode && ul.parentNode.parentNode)
                        {
                            ul = ul.parentNode.parentNode;
                        }
                        recursiveLength = recursiveLength - 3;
                    }
                }

                // we render the item, update lastIndent, and databind only when certains conditions are met
                // this was introduced to fix [IN:5374] where the sidebar for requirements could get messed up based on what was collapsed or not
                if (isFirstItem || isSiblingOfLastItem || isChildOfLastItem || isOutdentedFromLastItem)
                {
                    var li = d.ce('li');
                    ul.appendChild(li);
                    prevLi = li;

                    this._lastIndent = indent;

                    //Databind the row
                    this._dataBindRow(dataRow, li);
                }

            }
        }
    },

    expandNode: function (e)
    {
        //First we need to change the expand icon
        var img = e.img;
        img.className = 'fas w3 fa-caret-right ti-0 pointer rotate45 gray js-Expanded';
        img.setAttribute('Tooltip', resx.HierarchicalGrid_CollapseNode);
        img.title = resx.HierarchicalGrid_CollapseNode;

        //Now expand the child UL - sending to web service if method exists
        var webService = this.get_webServiceClass();
        if (webService.Expand)
        {
            webService.Expand(this.get_projectId(), e.primaryKey, Function.createDelegate(this, this.expandNode_success), Function.createDelegate(this, this.operation_failure), e);
        }
        else
        {
            this.expandNode_success(null, e);
        }
    },
    expandNode_success: function (dataSource, e)
    {
        //Now expand the child UL if we have one
        var li = e.li;
        var ul = null;
        for (var i = 0; i < li.childNodes.length; i++)
        {
            if (li.childNodes[i].tagName == 'UL')
            {
                ul = li.childNodes[i];
                break;
            }
        }
        if (ul)
        {
            ul.style.display = 'block';
        }
        else
        {
            //The child ul needs to be loaded
            var webService = this.get_webServiceClass();
            var indentLevel = li.getAttribute('data-indentlevel');
            this.display_spinner();
            webService.NavigationBar_RetrieveList(this.get_projectId(), indentLevel, this.get_displayMode(), this.get_selectedItemId(), this.get_containerId(), Function.createDelegate(this, this.expandNode_success2), Function.createDelegate(this, this.operation_failure), e);
        }
    },
    expandNode_success2: function (dataSource, e)
    {
        //Create the new child UL
        var li = e.li;
        ul = d.ce('ul');
        ul.className = 'Child';
        li.appendChild(ul);

        //Set the datasource
        this.set_dataSource(dataSource);

        //Databind
        this.dataBind(ul);

        //Mark as complete
        this.hide_spinner();
    },
    collapseNode: function (e)
    {
        //First we need to change the expand icon
        var img = e.img;
        var li = e.li;
        img.className = 'fas w3 fa-caret-right pointer ti-0 dark-gray js-Collapsed';
        img.setAttribute('Tooltip', resx.HierarchicalGrid_ExpandNode);
        img.title = resx.HierarchicalGrid_ExpandNode;

        //Now collapse the child UL - call the web service if it implements the collapse method
        var webService = this.get_webServiceClass();
        if (webService.Collapse)
        {
            webService.Collapse(this.get_projectId(), e.primaryKey, Function.createDelegate(this, this.collapseNode_success), Function.createDelegate(this, this.operation_failure), e);
        }
        else
        {
            this.collapseNode_success(null, e);
        }
    },
    collapseNode_success: function (dataSource, e)
    {
        var li = e.li;

        var ul = null;
        for (var i = 0; i < li.childNodes.length; i++)
        {
            if (li.childNodes[i].tagName == 'UL')
            {
                ul = li.childNodes[i];
                break;
            }
        }
        if (ul)
        {
            ul.style.display = 'none';
        }
    },

    _dropdownChanged: function (item)
    {
        //Update the display mode
        this.set_displayMode(item.get_value());

        //Update the setting on the server - no need to do anything if successful
        var webService = this.get_webServiceClass();
        webService.NavigationBar_UpdateSettings(this.get_projectId(), this.get_displayMode(), null, null, null, Function.createDelegate(this, this.operation_failure));

        //Clear the existing content
        this._clearContent(this.get_list());

        //Now reload the data
        this.load_data();
    },

    /*  =========================================================
    Internal methods/functions
    =========================================================  */
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
        //Add the cells for the containing panel
        var panel = this.get_element();

        //Create the table header
        var heading = d.ce('div');
        heading.className = 'panel-heading';
        panel.appendChild(heading);

        //The list screen link
        var a = d.ce('a');
        heading.appendChild(a);
        a.href = this.get_listScreenUrl();
        var span = d.ce('span');
        span.className = 'fas fa-angle-double-left';
        a.appendChild(span);
        a.appendChild(d.createTextNode('\u00a0'));
        a.appendChild(d.createTextNode(this.get_listScreenCaption()));
        var div = d.ce('div');
        heading.appendChild(div);
        div.className = 'minimize';
        div.title = resx.NavigationBar_Minimize;
        this._header = heading;
        if (this.get_minimized())
        {
            heading.style.display = 'none';
        }

        //Add the event handler to the minimize icon
        var _onMinimizeHandler = Function.createDelegate(this, this._onMinimize);
        $addHandler(div, 'click', _onMinimizeHandler);
        this._clickElements.push(div);
        this._otherClickHandlers.push(_onMinimizeHandler);

        //Create the Minimized Header
        var miniHeader = d.ce('div');
        miniHeader.className = 'minimized-heading';
        panel.appendChild(miniHeader);

        //add contents to the minimized header
        var headerText = resx.NavigationBar_ItemList;
        this._minimizedHeaderSpan = d.ce('span');
        this._minimizedHeaderSpan.className = "minimized-heading-title";
        this._minimizedHeaderSpan.innerHTML = headerText;
        miniHeader.appendChild(this._minimizedHeaderSpan);

        btn = d.ce('button');
        btn.type = 'button';
        miniHeader.appendChild(btn);
        btn.className = 'maximize';
        btn.title = resx.NavigationBar_Maximize;
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

        //Create the table subheader
        var subHeading = d.ce('div');
        subHeading.className = 'panel-subheading';
        panel.appendChild(subHeading);
        var span = d.ce('span');
        subHeading.appendChild(span);
        span.appendChild(d.createTextNode(resx.NavigationBar_Display));
        this._subheader = subHeading;
        var that = this;
        if (this.get_minimized())
        {
            subHeading.style.display = 'none';
            $(this._minimizedHeader).addClass('minimized-heading-on minimized-heading-all-min');
            $(document).ready(function () { that.areAllMinimizedNavBar() });
        }

        //Create the group
        var grp = d.ce('div');
        grp.className = 'btn-group mbn3'; //negative margin added to offset default negative margin of the buttons in the button group
        subHeading.appendChild(grp);

        //Create the dropdown list
        div = d.ce('div');
        grp.appendChild(div);
        div.style.width = '120px';
        this._dropdown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { listWidth: '150px' }, null, null, div);
        this._dropdown.addItem(1, resx.NavigationBar_CurrentFilter);
        this._dropdown.addItem(2, resx.NavigationBar_AllItems);
        if (this.get_includeAssigned())
        {
            this._dropdown.addItem(3, resx.NavigationBar_Assigned);
        }
        if (this.get_includeDetected())
        {
            this._dropdown.addItem(4, resx.NavigationBar_Detected);
        }
        if (this.get_includeTestCase())
        {
            this._dropdown.addItem(5, resx.NavigationBar_ForTestCase);
        }
        if (this.get_includeTestSet())
        {
            this._dropdown.addItem(6, resx.NavigationBar_ForTestSet);
        }
        if (this.get_includeRelease())
        {
            this._dropdown.addItem(7, resx.NavigationBar_ForRelease);
        }
        if (this.get_includeRequirement())
        {
            this._dropdown.addItem(8, resx.NavigationBar_ForRequirement);
        }
        this._dropdown.set_selectedItem(this.get_displayMode());

        //Add an event handler
        this._dropdownHandler = Function.createDelegate(this, this._dropdownChanged);
        this._dropdown.add_selectedItemChanged(this._dropdownHandler);

        //Create the refresh icon
        this._refreshIcon = d.ce('button');
        this._refreshIcon.type = 'button';
        this._refreshIcon.className = 'btn btn-default fas fa-sync';
        this._refreshIconClickHandler = Function.createDelegate(this, this._onRefreshIconClick);
        $addHandler(this._refreshIcon, 'click', this._refreshIconClickHandler);
        grp.appendChild(this._refreshIcon);

        //Create the panel body
        var body = d.ce('div');
        body.className = 'panel-body nav-bar-panel';
        panel.appendChild(body);
        if (this.get_minimized())
        {
            body.style.display = 'none';
        }

        //Now create the div that contains the child table that scrolls
        body.style.width = this.get_bodyWidth();
        body.style.height = this.get_bodyHeight();
        this._bodyDiv = body;

        //Now create the inner list that scrolls within this div
        this._list = d.ce('ul');
        this._bodyDiv.appendChild(this._list);

        //Finally we need to create the resize handle and position
        var bodyWidth = parseInt(this.get_bodyWidth().substr(0, this.get_bodyWidth().length - 2));
        this._resizeHandle = d.ce('div');
        this.get_element().parentNode.appendChild(this._resizeHandle);
        this._resizeHandle.className = 'NavigationBarResize';
        this._resizeHandle.style.height = this.get_bodyHeight();
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

        //Create the table footer
        this._footer = d.ce('div');
        this._footer.className = 'panel-footer';
        panel.appendChild(this._footer);

        //Add the number of rows link
        div = d.ce('div');
        div.className = 'Legend';
        div.appendChild(d.createTextNode(resx.Global_Show + '\u00a0'));
        this._footer.appendChild(div);
        div = d.ce('div');
        this._ddlPaginationOptions = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, null, null, null, div);
        this._paginationOptionsChangedHandler = Function.createDelegate(this, this._onPaginationOptionsChanged);
        this._ddlPaginationOptions.add_selectedItemChanged(this._paginationOptionsChangedHandler);
        this._footer.appendChild(div);
        div = d.ce('div');
        div.className = 'Legend';
        div.appendChild(d.createTextNode('\u00a0' + resx.Global_Rows));
        this._footer.appendChild(div);

        if (this.get_minimized())
        {
            this._footer.style.display = 'none';
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
    },

    _dataBindRow: function (dataRow, li)
    {
        var img;
        var primaryKey = dataRow.primaryKey;
        //We add the indent and load status so that we can track which items to get when expanded
        li.setAttribute('data-tstid', dataRow.primaryKey);
        if (dataRow.indent)
        {
            li.setAttribute('data-indentlevel', dataRow.indent);
        }

        //See if this row is currently selected or not
        if (this.get_selectedItemId() && this.get_selectedItemId() == primaryKey)
        {
            li.className = 'Selected';
        }

        //Create the +/- icon
        img = d.ce('span');
        if (dataRow.summary)
        {
            if (li.className == 'Selected')
            {
                li.className = 'SummarySelected Selected';
            }
            else
            {
                li.className = 'Summary';
            }
            if (dataRow.expanded)
            {
                img.className = 'fas w3 fa-caret-right ti-0 pointer rotate45 gray js-Expanded';
                img.setAttribute('Tooltip', resx.HierarchicalGrid_CollapseNode);
                img.title = resx.HierarchicalGrid_CollapseNode;
            }
            else
            {
                img.className = 'fas w3 fa-caret-right pointer ti-0 dark-gray js-Collapsed';
                img.setAttribute('Tooltip', resx.HierarchicalGrid_ExpandNode);
                img.title = resx.HierarchicalGrid_ExpandNode;
            }

            //Add the expand/collapse node handler
            var _onExpandCollapseNodeHandler = Function.createCallback(this._onExpandCollapseNode, { thisRef: this, primaryKey: dataRow.primaryKey, img: img, li: li });
            $addHandler(img, 'click', _onExpandCollapseNodeHandler);
            this._clickElements.push(img);
            this._otherClickHandlers.push(_onExpandCollapseNodeHandler);
        }
        else
        {
            img.src = this._themeFolder + '/Images/ExpandedX.gif';
            if (li.className != 'Selected')
            {
                li.className = 'Normal';
            }
        }
        li.appendChild(img);
        li.appendChild(document.createTextNode(' '));

        //See if we have a custom class to add
        if (dataRow.Fields.Name.cssClass)
        {
            $(li).addClass(dataRow.Fields.Name.cssClass)
        }

        //Now the icon and name
        var dataField = dataRow.Fields.Name;
        img = d.ce('img');
        img.className = "w4 h4";
        if (dataRow.summary)
        {
            if (dataRow.alternate && this.get_alternateItemImage() != '')
            {
                img.src = this._themeFolder + '/' + this.get_alternateItemImage();
            }
            else if ((!this.get_summaryItemImage() || this.get_summaryItemImage() == '') && dataField.tooltip)
            {
                //If the image is not set, try and get it from the tooltip
                img.src = this._themeFolder + '/' + dataField.tooltip;
            }
            else
            {
                img.src = this._themeFolder + '/' + this.get_summaryItemImage();
            }
        }
        else
        {
            if (dataRow.alternate && this.get_alternateItemImage() != '')
            {
                img.src = this._themeFolder + '/' + this.get_alternateItemImage();
            }
            else
            {
                //If the image is not set, try and get it from the tooltip
                if ((!this.get_itemImage() || this.get_itemImage() == '') && dataField.tooltip)
                {
                    img.src = this._themeFolder + '/' + dataField.tooltip;
                }
                else
                {
                    img.src = this._themeFolder + '/' + this.get_itemImage();
                }
            }
        }


        li.appendChild(img);

        //We don't truncate the name because the whole nav-bar can be resized
        li.appendChild(document.createTextNode(' '));
        var name = dataField.textValue || resx.Global_None2;
        var a = d.ce('a');
        li.appendChild(a);
        if (dataRow.customUrl && dataRow.customUrl != '')
        {
            a.href = dataRow.customUrl;

            //See if the custom url means disabled
            if (dataRow.customUrl == '#')
            {
                a.href = 'javascript:void(0)';
                a.className = 'aspNetDisabled';
            }
        }
        else
        {
            a.href = this.get_itemBaseUrl().replace(globalFunctions.artifactIdToken, '' + primaryKey);
        }
        a.appendChild(document.createTextNode(name));

        //Add the asynchronous tooltip handler
        if (dataRow.primaryKey)
        {
            var _nameDescMouseOverHandler = Function.createCallback(this._onNameDescMouseOver, { thisRef: this, primaryKey: dataRow.primaryKey });
            var _nameDescMouseOutHandler = Function.createCallback(this._onNameDescMouseOut, { thisRef: this });
            $addHandler(a, 'mouseover', _nameDescMouseOverHandler);
            $addHandler(a, 'mouseout', _nameDescMouseOutHandler);
            this._tooltips.push(a);
            this._tooltipOverHandlers.push(_nameDescMouseOverHandler);
            this._tooltipOutHandlers.push(_nameDescMouseOutHandler);
        }

        //Add the click handler if 'live-loading' is enabled
        if (this._enableLiveLoading && this._formManagerId && this._formManagerId != '' && dataRow.primaryKey)
        {
            var clickHandler = Function.createCallback(this._liveLoadForm, { thisRef: this, artifactId: dataRow.primaryKey, customUrl: dataRow.customUrl });
            $addHandler(a, 'click', clickHandler);
            this._otherClickHandlers.push(clickHandler);
            this._clickElements.push(a);
        }
    },
    retreivePagination_success: function (paginationList)
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

        //Set the initial log flag
        this._initialLoad = false;
    }
}
Inflectra.SpiraTest.Web.ServerControls.NavigationBar.registerClass('Inflectra.SpiraTest.Web.ServerControls.NavigationBar', Sys.UI.Control);
        
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
