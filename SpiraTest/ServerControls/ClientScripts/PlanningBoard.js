var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.PlanningBoard = function (element)
{
    this._element = element;
    if (!element)
    {
        this._element = d.ce('table');
        this._element.style.width = '100%';    //Default width
    }

    Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.initializeBase(this, [element]);

    //Member variables
    this._webServiceClass = null;
    this._cssClass = '';
    this._projectId = -1;
    this._releaseId = -1;
    this._isIteration = false;
    this._themeFolder = '';
    this._errorMessageControlId = '';
    this._groupByControlId = '';
    this._releaseControlId = '';
    this._controlBaseId = '';
    this._isLoaded = false;
    this._dataSource = null;
    this._releaseInfo = null;
    this._groupIndex = 0;
    this._currItem = 0;
    this._itemCount = 0;
    this._iterationCount = 0;
	this._allowEdit = false;
	this._boardSupportsEditing = true;
    this._allowCreate = false;
    this._headerRows = new Array();
    this._itemRows = new Array();
    this._subHeaderRows = new Array();
    this._groupBy = -1;
    this._includeDetails = true;
    this._includeIncidents = true;
    this._includeTasks = false;
    this._includeTestCases = true;
    this._avatarBaseUrl = '';
    this._items = new Array();
    this._itemClicked = false;
    this._headerEqualizers = new Array();
    this._scrollableContainer = null;
    this._unassignedIsExpanded = false;
    this._lastScrollWidth = null;
    this._supportsRanking = false;

    //Event Handler References
    this._tooltips = new Array();
    this._tooltipOverHandlers = new Array();
    this._tooltipOutHandlers = new Array();
    this._buttons = new Array();
    this._buttonClickHandlers = new Array();
    this._onDocumentClickHandler = null;

    //Group by options types
    this._groupBy_ByComponent = 1;
    this._groupBy_ByPackage = 2;    /* Epic */
    this._groupBy_ByPriority = 3;
    this._groupBy_ByRelease = 4;
    this._groupBy_ByIteration = 5;
    this._groupBy_ByStatus = 6;
    this._groupBy_ByPerson = 7;
    this._groupBy_ByRequirement = 8;

    //Special Release selections
    this._release_ProductBacklog = -1;
    this._release_AllReleases = -2;

    /* Create the internal items */
    this._body = d.ce('tbody');
}

Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.callBaseMethod(this, 'initialize');

        //Add the internal items to the component
        this._element.appendChild(this._body);

        //Let other controls know we've initialized
        this.raise_init();

        //Add the doc click handler
        this._onDocumentClickHandler = Function.createDelegate(this, this._onDocumentClick);
        $addHandler(document, 'click', this._onDocumentClickHandler);

        //Now load the data
        this.load_data();
    },
    dispose: function ()
    {
        //Clear any handlers
        this._clearTooltipHandlers();
        this._clearButtonHandlers();
        if (this._onDocumentClickHandler)
        {
            $removeHandler(document, 'click', this._onDocumentClickHandler);
            delete this._onDocumentClickHandler;
        }

        //Dispose of any member objects
        delete this._body;
        delete this._headerRows;
        delete this._subHeaderRows;
        delete this._itemRows;
        delete this._headerEqualizers;
        delete this._scrollableContainer;

        //Dispose of any items
        this._clearItems();
        delete this._items;

        Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.callBaseMethod(this, 'dispose');
    },
    /*  =========================================================
    The properties
    =========================================================  */
    get_cssClass: function ()
    {
        return this._cssClass;
    },
    set_cssClass: function (value)
    {
        this._cssClass = value;
    },

    get_projectId: function ()
    {
        return this._projectId;
    },
    set_projectId: function (value)
    {
        this._projectId = value;
    },

    get_releaseId: function ()
    {
        return this._releaseId;
    },
    set_releaseId: function (value)
    {
        this._releaseId = value;
    },

    get_isIteration: function ()
    {
        return this._isIteration;
    },
    set_isIteration: function (value)
    {
        this._isIteration = value;
    },

    get_webServiceClass: function ()
    {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value)
    {
        this._webServiceClass = value;
    },

    get_avatarBaseUrl: function ()
    {
        return this._avatarBaseUrl;
    },
    set_avatarBaseUrl: function (value)
    {
        this._avatarBaseUrl = value;
    },

    get_groupBy: function ()
    {
        return this._groupBy;
    },
    set_groupBy: function (value)
    {
        this._groupBy = value;
    },

    get_includeDetails: function ()
    {
        return this._includeDetails;
    },
    set_includeDetails: function (value)
    {
        this._includeDetails = value;
    },

    get_includeIncidents: function ()
    {
        return this._includeIncidents;
    },
    set_includeIncidents: function (value)
    {
        this._includeIncidents = value;
    },

    get_includeTasks: function ()
    {
        return this._includeTasks;
    },
    set_includeTasks: function (value)
    {
        this._includeTasks = value;
    },

    get_includeTestCases: function ()
    {
        return this._includeTestCases;
    },
    set_includeTestCases: function (value)
    {
        this._includeTestCases = value;
    },

    get_allowEdit: function ()
    {
        return this._allowEdit;
    },
    set_allowEdit: function (value)
    {
        this._allowEdit = value;
    },

	get_boardSupportsEditing: function () {
		return this._boardSupportsEditing;
	},
	set_boardSupportsEditing: function (value) {
		this._boardSupportsEditing = value;
	},

    get_allowCreate: function ()
    {
        return this._allowCreate;
    },
    set_allowCreate: function (value)
    {
        this._allowCreate = value;
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

    get_groupByControlId: function ()
    {
        return this._groupByControlId;
    },
    set_groupByControlId: function (value)
    {
        this._groupByControlId = value;
    },

    get_releaseControlId: function ()
    {
        return this._releaseControlId;
    },
    set_releaseControlId: function (value)
    {
        this._releaseControlId = value;
    },

    get_itemClicked: function ()
    {
        return this._itemClicked;
    },
    set_itemClicked: function (value)
    {
        this._itemClicked = value;
    },

    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        this._dataSource = value;
    },


    get_releaseInfo: function ()
    {
        return this._releaseInfo;
    },
    set_releaseInfo: function (value)
    {
        this._releaseInfo = value;
    },

    get_headerRows: function ()
    {
        return this._headerRows;
    },
    get_itemRows: function ()
    {
        return this._itemRows;
    },
    get_items: function ()
    {
        return this._items;
    },

    get_supportsRanking: function ()
    {
        return this._supportsRanking;
    },
    set_supportsRanking: function (value)
    {
        this._supportsRanking = value;
    },
    

    /*  =========================================================
    The methods
    =========================================================  */

    //Move the items on the server
    moveItems: function (items, containerId, existingItem)
    {
        //Call the service
        var existingArtifactTypeId = null;
        var existingArtifactId = null;
        if (existingItem)
        {
            existingArtifactTypeId = existingItem.get_artifactTypeId();
            existingArtifactId = existingItem.get_primaryKey();
        }
        this._webServiceClass.PlanningBoard_MoveItems(this._projectId, this._releaseId, this._isIteration, this._groupBy, containerId, items, existingArtifactTypeId, existingArtifactId, Function.createDelegate(this, this.moveItems_success), Function.createDelegate(this, this.operation_failure));
    },
    moveItems_success: function ()
    {
        //We need to reload the release and containers in case their summary data has changed
        if (this._releaseId > 0 && this._groupBy != this._groupBy_ByRelease)
        {
            this._webServiceClass.PlanningBoard_RetrieveReleaseIterationInfo(this._projectId, this._releaseId, this._groupBy, Function.createDelegate(this, this.update_release_info), Function.createDelegate(this, this.operation_failure));
        }
        if (this._groupBy == this._groupBy_ByRelease || this._groupBy == this._groupBy_ByIteration || this._groupBy == this._groupBy_ByPerson)
        {
            this._webServiceClass.PlanningBoard_RetrieveGroupByContainers(this._projectId, this._releaseId, this._groupBy, Function.createDelegate(this, this.update_containers), Function.createDelegate(this, this.operation_failure));
        }
    },
    update_release_info: function (data)
    {
        if (data && data.items)
        {
            for (var i = 0; i < data.items.length; i++)
            {
                this._updateContainerInfo(data.items[i]);
            }
        }
    },
    update_containers: function (data)
    {
        if (data && data.items)
        {
            for (var i = 0; i < data.items.length; i++)
            {
                this._updateContainerInfo(data.items[i]);
            }
        }
    },
    _updateContainerInfo: function (dataItem)
    {
        //Update the effort fields
        if (dataItem.Fields.UtilizedEffort)
        {
            //Find the table
            var tables = this._element.getElementsByTagName('TABLE');
            var table = null;
            for (var i = 0; i < tables.length; i++)
            {
                if (tables[i].className == 'pb-efforts' && tables[i].getAttribute('tst:primarykey') == dataItem.primaryKey)
                {
                    table = tables[i];
                    break;
                }
            }
            if (table && table.parentNode)
            {
                var tableParent = table.parentNode;
                this._createEffortTable(tableParent, dataItem, table);
                tableParent.removeChild(table);
            }
        }
        //Update the Progress bar
        if (dataItem.Fields.Progress && this._headerEqualizers)
        {
            //Find the equalizer
            var dataItemField = dataItem.Fields.Progress;
            for (var i = 0; i < this._headerEqualizers.length; i++)
            {
                var equalizer = this._headerEqualizers[i];
                if (equalizer.get_primaryKey() == dataItem.primaryKey)
                {
                    equalizer.set_percentGray(dataItemField.equalizerGray);
                    equalizer.set_percentGreen(dataItemField.equalizerGreen);
                    equalizer.set_percentRed(dataItemField.equalizerRed);
                    equalizer.set_percentOrange(dataItemField.equalizerOrange);
                    equalizer.set_percentYellow(dataItemField.equalizerYellow);
                    equalizer.set_toolTip(dataItemField.tooltip);
                    equalizer.display();
                }
            }
        }
    },

    //Update the release
    updateRelease: function (releaseItem)
    {
        var releaseId = -1;
        var isIteration = false;
        var needToChangeGroupBy = false;
        if (releaseItem && releaseItem.get_value() && releaseItem.get_value() != '')
        {
            releaseId = releaseItem.get_value();
            isIteration = (releaseItem.get_alternate && releaseItem.get_alternate() == 'Y');
        }
        if (this._isIteration != isIteration || (releaseId > 0 && this._releaseId <= 0) || (releaseId <= 0 && this._releaseId > 0) || (releaseId < 0 && this._releaseId < 0 && releaseId != this._releaseId))
        {
            needToChangeGroupBy = true;
        }
        this._releaseId = releaseId;
        this._isIteration = isIteration;

        //Update the active group-by options (unless first load)
        if (this._isLoaded)
        {
            if (needToChangeGroupBy)
            {
                this._updateAvailableGroupByOptions();
            }
            this._webServiceClass.PlanningBoard_UpdateRelease(this._projectId, releaseId, null, Function.createDelegate(this, this.operation_failure));
            this._webServiceClass.PlanningBoard_UpdateGroupBy(this._projectId, this._groupBy, null, Function.createDelegate(this, this.operation_failure));

            //Now we need to reload the data
            this.load_data();

            //Raise the event
            this.raise_changeGroupBy(this._groupBy);
        }
    },

    //Update the group by
    updateGroupBy: function (groupByItem)
    {
        if (groupByItem)
        {
            var groupById = groupByItem.get_value();
            this._groupBy = groupById;
            this._webServiceClass.PlanningBoard_UpdateGroupBy(this._projectId, groupById, null, Function.createDelegate(this, this.operation_failure));

            if (this._isLoaded)
            {
                //Now we need to reload the data
                this.load_data();
            }

            //Raise the event
            this.raise_changeGroupBy(groupById);
        }
    },

    //Update the options
    updateOptions: function (option, value)
    {
        //Make sure the option has actually changed
        var optionChanged = false;
        switch (option)
        {
            case 'IncludeDetails':
                if (this._includeDetails != value)
                {
                    this._includeDetails = value;
                    optionChanged = true;
                }
                break;
            case 'IncludeIncidents':
                if (this._includeIncidents != value)
                {
                    this._includeIncidents = value;
                    optionChanged = true;
                }
                break;
            case 'IncludeTasks':
                if (this._includeTasks != value)
                {
                    this._includeTasks = value;
                    optionChanged = true;
                }
                break;
            case 'IncludeTestCases':
                if (this._includeTestCases != value)
                {
                    this._includeTestCases = value;
                    optionChanged = true;
                }
                break;
        }
        if (optionChanged)
        {
            this._webServiceClass.PlanningBoard_UpdateOptions(this._projectId, option, value, Function.createDelegate(this, this.updateOptions_success), Function.createDelegate(this, this.operation_failure));
        }
    },
    updateOptions_success: function ()
    {
        //Reload the planning board
        this.load_data();
    },

    custom_operation: function (operation, value)
    {
        if (value != undefined && value != '')
        {
            //Call the appropriate webservice to invoke the custom operation, then refresh the planning grid
            var webServiceClass = this._primaryWebServiceClasses[this._groupBy];
            var webServiceMethod = webServiceClass + '.CustomOperation';
            var webServiceParams = '(' + this.get_projectId() + ',operation,value,Function.createDelegate(this, this.operation_success),Function.createDelegate(this, this.operation_failure))';
            globalFunctions.display_spinner();
            eval(webServiceMethod + webServiceParams);
        }
    },
    custom_operation_select: function (operation, select)
    {
        var value = select.get_value();
        this.custom_operation(operation, value);
    },
    operation_success: function (result)
    {
        //See if we have a validation error or not
        globalFunctions.hide_spinner();
        if (!this._operation_error_check(result))
        {
            //Now we need to reload the data
            this.load_data();
        }
    },
    load_data: function ()
    {
        //Clear any existing error messages
        globalFunctions.clear_errors($get(this._errorMessageControlId));

        //Clear any release info and data items
        this.set_releaseInfo(null);
        this.set_dataSource(null);

        //Get the current x-scroll position if we have the vertical board
        if (this._scrollableContainer)
        {
            this._lastScrollWidth = this._scrollableContainer.scrollLeft;
        }

        //Get the primary data from the Retrieve web method
        //This is used to group the story cards
        globalFunctions.display_spinner();
        this._webServiceClass.PlanningBoard_RetrieveGroupByContainers(this._projectId, this._releaseId, this._groupBy, Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
    },
    retrieve_success: function (dataSource)
    {
        //Set the datasource
        this.set_dataSource(dataSource);

        //See if we need to load the release/iteration info
        if (this._releaseId > 0 && (this._groupBy != this._groupBy_ByRelease && this._groupBy != this._groupBy_ByPriority && this._groupBy != this._groupBy_ByRequirement && this._groupBy != this._groupBy_ByComponent && this._groupBy != this._groupBy_ByPackage))
        {
            this._webServiceClass.PlanningBoard_RetrieveReleaseIterationInfo(this._projectId, this._releaseId, this._groupBy, Function.createDelegate(this, this.retrieve_success2), Function.createDelegate(this, this.operation_failure));
        }
        else
        {
            //Databind
            this.set_releaseInfo(null);
            this.dataBind(Function.createDelegate(this, this._retrieve_success_callback));
        }
    },
    retrieve_success2: function (dataSource)
    {
        //Set the datasource
        this.set_releaseInfo(dataSource);

        //Databind
        this.dataBind(Function.createDelegate(this, this._retrieve_success_callback));
    },
    _retrieve_success_callback: function ()
    {
        globalFunctions.hide_spinner();
        this._isLoaded = true;
        this.raise_loaded();

        //Scroll to the former position (if saved)
        if (this._lastScrollWidth)
        {
            this.scroll('right', this._lastScrollWidth);
            this._lastScrollWidth = null;   //Prevent double fires of the event
        }
    },

    //Databinds the planning board framework
    dataBind: function (callback)
    {
        //Clear any handlers
        this._clearTooltipHandlers();
        this._clearButtonHandlers();

        //Reseting the module level variables
        this._headerRows = new Array();
        this._subHeaderRows = new Array();
        this._itemRows = new Array();
        this._headerEqualizers = new Array();
        this._scrollableContainer = null;

        //Clear out the content of the primary table head and body
        this._clearContent(this._body);
        this._clearItems();

        //Add the items from the datasource
        this._isLoaded = false;
        var dataSource = this.get_dataSource();

        if (dataSource == null)
        {
            dataSource = {};
            dataSource.items = new Array();
        }
        if (dataSource.items == null)
        {
            dataSource.items = new Array();
        }

        //Create the item that handles 'unassigned items'
        //Add to either the data source or release info
        var dataItems = dataSource.items;
        if (this._releaseInfo && this._releaseInfo.items)
        {
            dataItems = this._releaseInfo.items;
        }
        var dataItem = {};
        dataItem.primaryKey = -1;
        dataItem.Fields = {};
        dataItem.Fields.Name = { textValue: '(' + resx.PlanningBoard_UnassignedItems + ')' };
        dataItems.unshift(dataItem);
        dataItem.expanded = this._unassignedIsExpanded;

        //Determine which format we need to create (vertical or horizontal)
        if (this._groupBy == this._groupBy_ByComponent || this._groupBy == this._groupBy_ByPackage || this._groupBy == this._groupBy_ByPriority || this._groupBy == this._groupBy_ByRequirement)
        {
            this._createHorizontalBoard(dataSource);
        }
        else
        {
            this._createVerticalBoard(dataSource, this._releaseInfo);
        }

        //Add the release items to the main dataitem list
        if (this._releaseInfo && this._releaseInfo.items)
        {
            var dataItems = this._dataSource.items;
            for (var i = this._releaseInfo.items.length - 1; i >= 0; i--)
            {
                dataItems.unshift(this._releaseInfo.items[i]);
            }
        }

        //Set the group index count and load asynchronously
        this._groupIndex = 0;
        this._loadItems(this, callback);

        //Finally call the callback function
        if (callback != undefined)
        {
            callback();
        }
    },

    //Clears any items
    _clearItems: function ()
    {
        if (this._items && this._items.length > 0)
        {
            for (var i = 0; i < this._items.length; i++)
            {
                var item = this._items[i];
                item.dispose();
            }
            delete this._items;
        }
        this._items = new Array();
    },

    //Creates the effort table
    _createEffortTable: function (parent, dataItem, insertBefore)
    {
        //The three effort cells
        var table = d.ce('table');
        table.setAttribute('tst:primarykey', dataItem.primaryKey);
        table.className = 'pb-efforts';
        if (insertBefore)
        {
            parent.insertBefore(table, insertBefore);
        }
        else
        {
            parent.appendChild(table);
        }
        var tbody = d.ce('tbody');
        table.appendChild(tbody);
        var trOne = d.ce('tr');
        tbody.appendChild(trOne);
        //Planned Effort
        var td = d.ce('td');
        trOne.appendChild(td);
        td.appendChild(d.createTextNode(resx.PlanningGrid_Available + ": "));
        td.title = (typeof _pageInfo != 'undefined' && _pageInfo.planByPoints) ? resx.PlanningGrid_Available_PointsTooltip : resx.PlanningGrid_Available_HoursTooltip;
        if (dataItem.Fields.PlannedEffort && dataItem.Fields.PlannedEffort.textValue)
        {
            var tdVal = d.ce('td');
            tdVal.appendChild(d.createTextNode(dataItem.Fields.PlannedEffort.textValue));
            trOne.appendChild(tdVal);
        }
        //Estimated Effort
        var trTwo = d.ce('tr');
        tbody.appendChild(trTwo);
        td = d.ce('td');
        trTwo.appendChild(td);
        td.appendChild(d.createTextNode(resx.PlanningGrid_Utilized + ": "));
        td.title = (typeof _pageInfo != 'undefined' && _pageInfo.planByPoints) ? resx.PlanningGrid_Utilized_PointsTooltip : resx.PlanningGrid_Utilized_HoursTooltip;
        if (dataItem.Fields.UtilizedEffort && dataItem.Fields.UtilizedEffort.textValue)
        {
            var tdVal = d.ce('td');
            tdVal.appendChild(d.createTextNode(dataItem.Fields.UtilizedEffort.textValue));
            trTwo.appendChild(tdVal);
        }
        //Available Effort
        //Warn the user if negative with a different CSS class
        var trThree = d.ce('tr');
        tbody.appendChild(trThree);
        td = d.ce('td');
        trThree.appendChild(td);
        if (dataItem.Fields.AvailableEffort) {
            if (dataItem.Fields.AvailableEffort.intValue && dataItem.Fields.AvailableEffort.intValue < 0) {
                trThree.className = 'pb-efforts_warning';
                parent.classList.add('pb-efforts-warning-wrapper');
            }
            else {
                trThree.className = 'pb-efforts_highlighted';
            }
        }
        td.appendChild(d.createTextNode(resx.PlanningGrid_Remaining + ": "));
        td.title = (typeof _pageInfo != 'undefined' && _pageInfo.planByPoints) ? resx.PlanningGrid_Remaining_PointsTooltip : resx.PlanningGrid_Remaining_HoursTooltip;
        if (dataItem.Fields.AvailableEffort && dataItem.Fields.AvailableEffort.textValue)
        {
            var tdVal = d.ce('td');
            tdVal.appendChild(d.createTextNode(dataItem.Fields.AvailableEffort.textValue));
            trThree.appendChild(tdVal);
        }
        return table;
    },

    //Creates the vertical board format
    _createVerticalBoard: function (groupByData, releaseInfo)
    {
        //First add the header row
        var trHeaderRow = d.ce('tr');
        this._body.appendChild(trHeaderRow);
        trHeaderRow.className = 'pb-header';
        var tdHeaderRow = d.ce('td');
        trHeaderRow.appendChild(tdHeaderRow);
        tdHeaderRow.colSpan = 2;
        var divHeaderRow = d.ce('div');
        divHeaderRow.className = "df justify-between";
        tdHeaderRow.appendChild(divHeaderRow);

        //Create the scroll arrows
        var scrollLeft = d.ce('div');
        scrollLeft.className = 'pb-scrollLeft fas fa-chevron-left';
        divHeaderRow.appendChild(scrollLeft);
        var clickHandler = Function.createCallback(this._onScrollClick, { thisRef: this, direction: 'left' });
        $addHandler(scrollLeft, 'click', clickHandler);
        this._buttons.push(scrollLeft);
        this._buttonClickHandlers.push(clickHandler);

        var scrollRight = d.ce('div');
        scrollRight.className = 'pb-scrollRight fas fa-chevron-right';
        divHeaderRow.appendChild(scrollRight);
        clickHandler = Function.createCallback(this._onScrollClick, { thisRef: this, direction: 'right' });
        $addHandler(scrollRight, 'click', clickHandler);
        this._buttons.push(scrollRight);
        this._buttonClickHandlers.push(clickHandler);

        //We need to create the header items from the release info or the first row of the data source
        var dataItems;
        var dataSource;
        var div;
        if (releaseInfo)
        {
            dataSource = releaseInfo;
            dataItems = releaseInfo.items;
        }
        else
        {
            dataSource = groupByData;
            dataItems = [groupByData.items[0]];
        }
        for (var i = 0; i < dataItems.length; i++)
        {
            var dataItem = dataItems[i];
            var tr = d.ce('tr');
            this._body.appendChild(tr);
            tr.className = 'pb-columns';
            tr.setAttribute('tst:primarykey', dataItem.primaryKey);
            var td = d.ce('td');
            td.className = (releaseInfo) ? 'pb-columns_header_strong' : 'pb-columns_header';
            tr.appendChild(td);

            //Add to the list of headers
            this.get_headerRows().push(tr);

            //Add the expand/collapse icon
            if (!dataItem.Fields.Disabled)
            {
                div = d.ce('div');
                div.setAttribute('tst:primarykey', dataItem.primaryKey);
                if (dataItem.expanded)
                {
                    div.setAttribute('tst:expanded', 'Y');
                    div.className = 'pb-minimize fas fa-caret-down';
                }
                else
                {
                    div.setAttribute('tst:expanded', 'N');
                    div.className = 'pb-minimize fas fa-caret-right';
                }
                td.appendChild(div);

                //Add the expand/collapse handler
                var onExpandCollapseHandler = Function.createCallback(this._onExpandCollapse, { thisRef: this, div: div, style: 'vertical' });
                $addHandler(div, 'click', onExpandCollapseHandler);
                this._buttons.push(div);
                this._buttonClickHandlers.push(onExpandCollapseHandler);
            }

            //Add the icon for the header
            if (dataItem.primaryKey != 0 && dataItem.primaryKey != -1 && dataSource.artifactImage)
            {
                var img = d.ce('img');
                img.className = "w4 h4";
                if (dataItem.alternate && dataSource.alternateImage)
                {
                    img.src = this._themeFolder + 'Images/' + dataSource.alternateImage;
                }
                else
                {
                    img.src = this._themeFolder + 'Images/' + dataSource.artifactImage;
                }
                td.appendChild(img);
                td.appendChild(d.createTextNode('\u00a0'));
            }

            //See if we have a URL for this container type
            var span = d.ce('span');
            span.className = "mr3";
            td.appendChild(span);
            if (dataItem.customUrl)
            {
                var a = d.ce('a');
                span.appendChild(a);
                a.appendChild(d.createTextNode(dataItem.Fields.Name.textValue));
                //See if we have a special internal link
                if (dataItem.customUrl == 'spira://release')
                {
                    a.href = 'javascript:void(0)';
                    var releaseSelector = Function.createCallback(this._onReleaseSelect, { thisRef: this, releaseId: dataItem.Fields.ReleaseId.intValue, isIteration: dataItem.alternate });
                    $addHandler(a, 'click', releaseSelector);
                    this._buttons.push(a);
                    this._buttonClickHandlers.push(releaseSelector);
                }
                else
                {
                    a.href = dataItem.customUrl;
                }
            }
            else
            {
                span.appendChild(d.createTextNode(dataItem.Fields.Name.textValue));
            }

            //Add the tooltip handler
            if (dataItem.Fields.Name.tooltip && dataItem.Fields.Name.tooltip != '')
            {
                var _headerMouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: dataItem.Fields.Name.tooltip });
                var _headerMouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(span, 'mouseover', _headerMouseOverHandler);
                $addHandler(span, 'mouseout', _headerMouseOutHandler);
                this._tooltips.push(span);
                this._tooltipOverHandlers.push(_headerMouseOverHandler);
                this._tooltipOutHandlers.push(_headerMouseOutHandler);
            }

            //Add the button to add item
            if (this._allowCreate) {
                div = d.ce('div');
                td.appendChild(div);
                div.className = 'pb-add-item fas fa-plus fr ml3 dib';
                var addItemHandler = Function.createCallback(this._onAddItemClick, { thisRef: this, containerId: dataItem.primaryKey });
                $addHandler(div, 'click', addItemHandler);
                this._buttons.push(div);
                this._buttonClickHandlers.push(addItemHandler);
            }

            //Add the effort fields
            if (dataItem.Fields.UtilizedEffort)
            {
                this._createEffortTable(td, dataItem);
            }

            //Add the progress bar (if there is one)
            if (dataItem.Fields.Progress)
            {
                var dataItemField = dataItem.Fields.Progress;
                var div = d.ce('div');
                div.className = 'pb-header-bar_equalizer dib';
                var props = {};
                props.primaryKey = dataItem.primaryKey;
                props.percentGray = dataItemField.equalizerGray;
                props.percentGreen = dataItemField.equalizerGreen;
                props.percentRed = dataItemField.equalizerRed;
                props.percentOrange = dataItemField.equalizerOrange;
                props.percentYellow = dataItemField.equalizerYellow;
                props.toolTip = dataItemField.tooltip;
                var progressBar = $create(Inflectra.SpiraTest.Web.ServerControls.Equalizer, props, null, null, div);
                this._headerEqualizers.push(progressBar);
                td.appendChild(div);
            }

            //Now create the empty row and cell that holds the items
            tr = d.ce('tr');
            this._body.appendChild(tr);
            tr.className = 'Items';
            tr.setAttribute('tst:primarykey', dataItem.primaryKey);
            if (!dataItem.expanded || dataItem.Fields.Disabled)
            {
                tr.style.display = 'none';
            }
            var td = d.ce('td');
            tr.appendChild(td);

            //Now create the container div
            div = d.ce('div');
            td.appendChild(div);

            div.className = 'pb-container-full-width ' + (dataItem.primaryKey === -1 ? 'container-unassigned' : 'container-release');

            //Attach the special behavior
            $create(Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior, { 'primaryKey': dataItem.primaryKey, 'parent': this }, null, null, div);

            //Add the DIV to the list of item rows
            this.get_itemRows().push(div);
        }

        //Now we need to create the vertical sections in their own sub-table
        var startIndex = (releaseInfo) ? 0 : 1; //Start with index 1 if the first item is the 'unassigned group'
        var tr = d.ce('tr');
        this._body.appendChild(tr);
        tr.className = 'Items';
        var td = d.ce('td');
        tr.appendChild(td);

        div = d.ce('div');
        div.className = 'pb-columns_box ov-x-scroll ov-y-visible';
        div.style.width = (td.offsetWidth - 20) + 'px';
        td.appendChild(div);
        this._scrollableContainer = div;

        var table = d.ce('table');
        table.className = 'pb-columns';
        div.appendChild(table);
        var thead = d.ce('thead');
        table.appendChild(thead);
        tr = d.ce('tr');
        thead.appendChild(tr);

        //Now create the container groups
        dataSource = groupByData;
        dataItems = groupByData.items;
        for (var i = startIndex; i < dataItems.length; i++)
        {
            var dataItem = dataItems[i];
            var td = d.ce('td');
            td.className = 'pb-columns_subheader' + (dataItem.expanded ? "" : ' pb-columns_subheader_collapsed');
            td.id = 'pb-columns_subheader' + '_' + dataItem.primaryKey;
            tr.appendChild(td);
            td.setAttribute('tst:primarykey', dataItem.primaryKey);
            this._subHeaderRows.push(td);

            //Add a wrapper for the icon and title/url to aid with styling
            var titleDiv = d.ce('div');
            td.appendChild(titleDiv);

            //Add the expand/collapse icon
            var div = d.ce('div');
            div.setAttribute('tst:primarykey', dataItem.primaryKey);
            if (dataItem.expanded)
            {
                div.setAttribute('tst:expanded', 'Y');
                div.className = 'pb-minimize fas fa-caret-down';
            }
            else
            {
                div.setAttribute('tst:expanded', 'N');
                div.className = 'pb-minimize fas fa-caret-right';
            }
            titleDiv.appendChild(div);

            //Add the expand/collapse handler
            var onExpandCollapseHandler = Function.createCallback(this._onExpandCollapse, { thisRef: this, div: div, style: 'verticalSection' });
            $addHandler(div, 'click', onExpandCollapseHandler);
            this._buttons.push(div);
            this._buttonClickHandlers.push(onExpandCollapseHandler);

            //Add the icon for the header
            if (dataItem.primaryKey > 0 && dataSource.artifactImage)
            {
                var img = d.ce('img');
                img.className = "w4 h4 pb-suhbheader_icon";
                img.title = dataItem.Fields.Name.textValue;
                if (dataItem.alternate && dataSource.alternateImage)
                {
                    img.src = this._themeFolder + 'Images/' + dataSource.alternateImage;
                }
                else
                {
                    img.src = this._themeFolder + 'Images/' + dataSource.artifactImage;
                }
                titleDiv.appendChild(img);
                titleDiv.appendChild(d.createTextNode('\u00a0'));
            }

            //If grouping by user, add the avatar
            if (dataItem.primaryKey > 0 && (this._groupBy == this._groupBy_ByPerson))
            {
                var img = d.ce('img');
                img.className = 'pb-suhbheader_avatar w5 br2';
                img.src = this._avatarBaseUrl.replace('{0}', dataItem.primaryKey);
                img.alt = dataItem.Fields.Name.textValue;
                img.title = dataItem.Fields.Name.textValue;
                titleDiv.appendChild(img);
                titleDiv.appendChild(d.createTextNode('\u00a0'));
            }

            //See if we have a URL for this container type
            var span = d.ce('span');
            span.className = "mr3 pb-subheder_name";
            titleDiv.appendChild(span);
            if (dataItem.customUrl)
            {
                var a = d.ce('a');
                span.appendChild(a);
                a.appendChild(d.createTextNode(dataItem.Fields.Name.textValue));
                //See if we have a special internal link
                if (dataItem.customUrl == 'spira://release')
                {
                    a.href = 'javascript:void(0)';
                    var releaseSelector = Function.createCallback(this._onReleaseSelect, { thisRef: this, releaseId: dataItem.Fields.ReleaseId.intValue, isIteration: dataItem.alternate });
                    $addHandler(a, 'click', releaseSelector);
                    this._buttons.push(a);
                    this._buttonClickHandlers.push(releaseSelector);
                }
                else
                {
                    a.href = dataItem.customUrl;
                }
            }
            else
            {
                span.appendChild(d.createTextNode(dataItem.Fields.Name.textValue));
            }

            //Add the tooltip handler
            if (dataItem.primaryKey > 0 && dataItem.Fields.Name.tooltip && dataItem.Fields.Name.tooltip != '')
            {
                var _headerMouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: dataItem.Fields.Name.tooltip });
                var _headerMouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(span, 'mouseover', _headerMouseOverHandler);
                $addHandler(span, 'mouseout', _headerMouseOutHandler);
                this._tooltips.push(span);
                this._tooltipOverHandlers.push(_headerMouseOverHandler);
                this._tooltipOutHandlers.push(_headerMouseOutHandler);
            }


            //Add the button to add item - below the title div inside the cell
            if (this._allowCreate) {
                div = d.ce('div');
                td.appendChild(div);
                div.style.display = (dataItem.expanded) ? '' : 'none';
                div.className = 'pb-add-item fas fa-plus dib mx1';
                var addItemHandler = Function.createCallback(this._onAddItemClick, { thisRef: this, containerId: dataItem.primaryKey });
                $addHandler(div, 'click', addItemHandler);
                this._buttons.push(div);
                this._buttonClickHandlers.push(addItemHandler);
            }
            
            //Add the effort fields
            if (dataItem.Fields.UtilizedEffort) {
                var effortTable = this._createEffortTable(td, dataItem);
                effortTable.style.display = (dataItem.expanded) ? '' : 'none';
            }


            //Add the progress bar (if there is one)
            if (dataItem.Fields.Progress)
            {
                var dataItemField = dataItem.Fields.Progress;
                var div = d.ce('div');
                div.style.display = (dataItem.expanded) ? '' : 'none';
                div.className = 'pb-subheader-bar_equalizer dib mt3';
                var props = {};
                props.primaryKey = dataItem.primaryKey;
                props.percentGray = dataItemField.equalizerGray;
                props.percentGreen = dataItemField.equalizerGreen;
                props.percentRed = dataItemField.equalizerRed;
                props.percentOrange = dataItemField.equalizerOrange;
                props.percentYellow = dataItemField.equalizerYellow;
                props.toolTip = dataItemField.tooltip;
                var progressBar = $create(Inflectra.SpiraTest.Web.ServerControls.Equalizer, props, null, null, div);
                this._headerEqualizers.push(progressBar);
                td.appendChild(div);
            }

            //Add WIP limit info if a limit is set
            if (dataItem.Fields.WipLimit) {
                //add the badge - defaults to 0 of wipLimit in UI - this gets overridden if column has any cards
                span = d.ce('span');
                span.className = 'badge wip-badge bg-mid-gray white ml2';
                span.appendChild(d.createTextNode(resx.PlanningBoard_WIP + ': 0 / ' + dataItem.Fields.WipLimit.intValue));
                td.appendChild(span);

                //set the default indicator classes - assume there is space, this gets updated if cards added to column
                td.classList.add('pb-wip-not-exceeded');
            }
        }

        //Now create the Container TR/TD/DIVs that contains the items
        var tbody = d.ce('tbody');
        table.appendChild(tbody);
        tr = d.ce('tr');
        tbody.appendChild(tr);
        for (var i = startIndex; i < dataItems.length; i++)
        {
            var dataItem = dataItems[i];
            var td = d.ce('td');
            td.className = 'pb-columns_content';
            tr.appendChild(td);
            div = d.ce('div');
            td.appendChild(div);
            div.className = 'pb-columns_content-container';
            div.id = 'pb-columns_content-container' + '_' + dataItem.primaryKey;
            div.style.display = (dataItem.expanded) ? '' : 'none';
            div.setAttribute('tst:primarykey', dataItem.primaryKey);
            //wip limit
            if (dataItem.Fields.WipLimit && dataItem.Fields.WipLimit.intValue) {
                div.setAttribute('tst:wiplimit', dataItem.Fields.WipLimit.intValue);
                //set the default indicator classes - assume there is space, this gets updated if cards added to column
                td.classList.add('pb-wip-not-exceeded');
            }
            //check effort value
            var headerRow = document.getElementById("pb-columns_subheader_" + dataItem.primaryKey);
            if (headerRow && headerRow.classList.contains("pb-efforts-warning-wrapper")) {
                div.parentNode.classList.add("pb-efforts-warning-wrapper");
            }
            //Attach the special behavior to the TD
            $create(Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior, { 'primaryKey': dataItem.primaryKey, 'parent': this }, null, null, td);

            //Add the DIV to the list of item rows
            this._itemRows.push(div);
        }
    },

    //Creates the horizontal board format
    _createHorizontalBoard: function (dataSource)
    {
        //First add the header row
        var tr = d.ce('tr');
        this._body.appendChild(tr);
        tr.className = 'pb-header';
        var td = d.ce('td');
        tr.appendChild(td);
        td.colSpan = 2;
        var div = d.ce('div');
        td.appendChild(div);
        div.className = 'btn-group ma3';
        div.setAttribute('role', 'group');

        //Add the expand/collapse all buttons
        var button = d.ce('button');
        button.type = 'button';
        button.className = 'btn btn-default df items-center';
        button.id = "btnExpandAll";
        var buttonIcon = d.ce('span');
        buttonIcon.className = "fas fa-expand mr3";
        buttonIcon.id = "btnExpandAllIcon";
        button.appendChild(buttonIcon);
        var buttonText = d.ce('span');
        buttonText.textContent = resx.PlanningBoard_ExpandAll;
        button.appendChild(buttonText);
        div.appendChild(button);

        //Add the handler
        var onExpandAllHandler = Function.createDelegate(this, this._onExpandAll);
        $addHandler(button, 'click', onExpandAllHandler);
        this._buttons.push(button);
        this._buttonClickHandlers.push(onExpandAllHandler);

        button = d.ce('button');
        button.type = 'button';
        button.className = 'btn btn-default df items-center';
        button.id = "btnCollapseAll";
        buttonIcon = d.ce('span');
        buttonIcon.className = "fas fa-compress mr3";
        buttonIcon.id = "btnCollapseAllIcon";
        button.appendChild(buttonIcon);
        buttonText = d.ce('span');
        buttonText.textContent = resx.PlanningBoard_CollapseAll;
        button.appendChild(buttonText);
        div.appendChild(button);

        //Add the handler
        var onCollapseAllHandler = Function.createDelegate(this, this._onCollapseAll);
        $addHandler(button, 'click', onCollapseAllHandler);
        this._buttons.push(button);
        this._buttonClickHandlers.push(onCollapseAllHandler);

        var dataItems = dataSource.items;
        for (var i = 0; i < dataItems.length; i++)
        {
            var dataItem = dataItems[i];
            tr = d.ce('tr');
            this._body.appendChild(tr);
            tr.className = 'pb-rows';
            tr.setAttribute('tst:primarykey', dataItem.primaryKey);
            td = d.ce('td');
            tr.appendChild(td);

            //See if we have a custom color
            if (dataItem.Fields.CssClass && dataItem.Fields.CssClass.textValue)
            {
                td.className = 'pb-rows_sideheader ' + dataItem.Fields.CssClass.textValue;
            }
            else
            {
                td.className = 'pb-rows_sideheader';
            }

            //Add the button to add item
            if (this._allowCreate)
            {
                div = d.ce('div');
                td.appendChild(div);
                div.className = 'pb-add-item fas fa-plus';
                var addItemHandler = Function.createCallback(this._onAddItemClick, { thisRef: this, containerId: dataItem.primaryKey });
                $addHandler(div, 'click', addItemHandler);
                this._buttons.push(div);
                this._buttonClickHandlers.push(addItemHandler);
            }

            //Add to the list of headers
            this.get_headerRows().push(tr);

            //Add the expand/collapse icon
            var div = d.ce('div');
            div.setAttribute('tst:primarykey', dataItem.primaryKey);
            if (dataItem.expanded)
            {
                div.setAttribute('tst:expanded', 'Y');
                div.className = 'pb-minimize fas fa-caret-down';
            }
            else
            {
                div.setAttribute('tst:expanded', 'N');
                div.className = 'pb-minimize fas fa-caret-right';
            }
            td.appendChild(div);

            //See if we have to indent the text
            if (dataItem.Fields.IndentLevel && dataItem.Fields.IndentLevel.textValue)
            {
                var indentPosition = dataItem.Fields.IndentLevel.textValue.length / 3;
                td.style.paddingLeft = ((indentPosition - 1) * 10) + 'px';
            }

            //Add the expand/collapse handler
            var onExpandCollapseHandler = Function.createCallback(this._onExpandCollapse, { thisRef: this, div: div, style: 'horizontal' });
            $addHandler(div, 'click', onExpandCollapseHandler);
            this._buttons.push(div);
            this._buttonClickHandlers.push(onExpandCollapseHandler);

            //Add the icon for the header
            if (dataItem.primaryKey > 0 && dataSource.artifactImage)
            {
                var img = d.ce('img');
                img.className = "w4 h4";
                if (dataItem.alternate && dataSource.alternateImage)
                {
                    img.src = this._themeFolder + 'Images/' + dataSource.alternateImage;
                }
                else
                {
                    img.src = this._themeFolder + 'Images/' + dataSource.artifactImage;
                }
                td.appendChild(img);
                td.appendChild(d.createTextNode('\u00a0'));
            }

            //See if we have a URL for this container type
            var span = d.ce('span');
            td.appendChild(span);
            if (dataItem.customUrl)
            {
                var a = d.ce('a');
                span.appendChild(a);
                a.appendChild(d.createTextNode(dataItem.Fields.Name.textValue));
                //See if we have a special internal link
                if (dataItem.customUrl == 'spira://release')
                {
                    a.href = 'javascript:void(0)';
                    var releaseSelector = Function.createCallback(this._onReleaseSelect, { thisRef: this, releaseId: dataItem.Fields.ReleaseId.intValue, isIteration: dataItem.alternate });
                    $addHandler(a, 'click', releaseSelector);
                    this._buttons.push(a);
                    this._buttonClickHandlers.push(releaseSelector);
                }
                else
                {
                    a.href = dataItem.customUrl;
                }
            }
            else
            {
                span.appendChild(d.createTextNode(dataItem.Fields.Name.textValue));
            }

            //Add the asynchronous tooltip handler
            if (dataItem.primaryKey > 0 && dataItem.Fields.Name.tooltip && dataItem.Fields.Name.tooltip != '')
            {
                var _headerMouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: dataItem.Fields.Name.tooltip });
                var _headerMouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(span, 'mouseover', _headerMouseOverHandler);
                $addHandler(span, 'mouseout', _headerMouseOutHandler);
                this._tooltips.push(span);
                this._tooltipOverHandlers.push(_headerMouseOverHandler);
                this._tooltipOutHandlers.push(_headerMouseOutHandler);
            }

            //Add the progress bar (if there is one)
            if (dataItem.Fields.Progress)
            {
                var dataItemField = dataItem.Fields.Progress;
                var div = d.ce('div');
                div.className = 'pb-sidebar_equalizer';
                var props = {};
                props.primaryKey = dataItem.primaryKey;
                props.percentGray = dataItemField.equalizerGray;
                props.percentGreen = dataItemField.equalizerGreen;
                props.percentRed = dataItemField.equalizerRed;
                props.percentOrange = dataItemField.equalizerOrange;
                props.percentYellow = dataItemField.equalizerYellow;
                props.toolTip = dataItemField.tooltip;
                var progressBar = $create(Inflectra.SpiraTest.Web.ServerControls.Equalizer, props, null, null, div);
                this._headerEqualizers.push(progressBar);

                span.appendChild(div);
                span.className = 'dif flex-column ti-0';
            }

            //Now create the empty cell that holds the items
            td = d.ce('td');
            tr.appendChild(td);

            //Now create the Container DIV that contains the items
            div = d.ce('div');
            td.appendChild(div);
            div.className = 'pb-rows_content' + (dataItem.primaryKey === -1 ? ' container-unassigned' : '');
            div.style.display = (dataItem.expanded) ? '' : 'none';

            //Attach the special behavior
            $create(Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior, { 'primaryKey': dataItem.primaryKey, 'parent': this }, null, null, div);

            //Add the DIV to the list of item rows
            this._itemRows.push(div);
        }
    },

    //Only provide containerId if you want to load a single specified group
    _loadItems: function (thisRef, callback, containerId)
    {
        //See if we're loading a single group
        if (containerId)
        {
            var context = {};
            context.callback = callback;
            context.containerId = containerId;
            context.loadingSingle = true;
            thisRef._webServiceClass.PlanningBoard_RetrieveItems(thisRef._projectId, thisRef._releaseId, thisRef._isIteration, thisRef._groupBy, (containerId == -1) ? null : containerId, thisRef._includeDetails, thisRef._includeIncidents, thisRef._includeTasks, thisRef._includeTestCases, Function.createDelegate(thisRef, thisRef._loadItems_success), Function.createDelegate(thisRef, thisRef.operation_failure), context);
        }
        else
        {
            //We need to loop through the list of groups
            var groupIndex = thisRef._groupIndex;
            var dataSource = thisRef.get_dataSource();
            if (dataSource && dataSource.items) {
                var dataItems = dataSource.items;

                //We only load the groups that are expanded already
                if (dataItems[groupIndex].expanded) {
                    //Get the list of items for this group
                    var containerId = (groupIndex == 0) ? null : dataItems[groupIndex].primaryKey;
                    var context = {};
                    context.callback = callback;
                    context.containerId = containerId;
                    thisRef._webServiceClass.PlanningBoard_RetrieveItems(thisRef._projectId, thisRef._releaseId, thisRef._isIteration, thisRef._groupBy, containerId, thisRef._includeDetails, thisRef._includeIncidents, thisRef._includeTasks, thisRef._includeTestCases, Function.createDelegate(thisRef, thisRef._loadItems_success), Function.createDelegate(thisRef, thisRef.operation_failure), context);
                }
                else {
                    thisRef._incrementIndex(callback);
                }
            }
        }
    },
    _loadItems_success: function (itemData, context)
    {
        var groupIndex = this._groupIndex;
        var dataSource = this.get_dataSource();

        //Make sure we have data
        if (dataSource && dataSource.items) {
            var groupItems = dataSource.items;

            //Access the item div
            var div = this._itemRows[groupIndex];

            //Add the items to the div
            if (itemData && itemData.items && itemData.items.length > 0) {
                var dataItems = itemData.items;

                //See if we have exceeded the WIP limit, highlight the cell and its matching header
                var wipLimit = div.getAttribute('tst:wiplimit');
                var subheader = document.getElementById('pb-columns_subheader' + '_' + div.getAttribute('tst:primarykey'));

                planningBoardWipUpdateUI(div, subheader, wipLimit, dataItems.length);

                for (var i = 0; i < dataItems.length; i++) {
                    var dataItem = dataItems[i];
                    var itemTbl = document.createElement('table');
                    var item = $create(Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem, null, null, null, itemTbl);
                    div.appendChild(itemTbl);
                    this._items.push(item);

                    //Set the properties
                    item.set_parent(this);
                    item.set_containerId(context.containerId);
                    item.set_primaryKey(dataItem.primaryKey);
                    item.set_artifactTypeId(dataItem.Fields.ArtifactTypeId.intValue);
                    item.set_token(dataItem.Fields.Token.textValue);
                    item.set_image(dataItem.Fields.Image.textValue);
					item.set_url(dataItem.customUrl);
                    item.set_name(dataItem.Fields.Name.textValue);
                    if (this._supportsRanking) {
                        item.set_position(i + 1);
                    }
                    if (dataItem.Fields.Description && dataItem.Fields.Description.textValue) {
                        item.set_description(dataItem.Fields.Description.textValue);
                    }
                    item.set_plannable(dataItem.Fields.PlannableYn.textValue == 'Y' && this._allowEdit);
                    if (dataItem.Fields.ImportanceId) {
                        if (dataItem.Fields.ImportanceId.cssClass) {
                            item.set_priorityColor(dataItem.Fields.ImportanceId.cssClass);
                        }
                        if (dataItem.Fields.ImportanceId.textValue) {
                            item.set_priorityTooltip(dataItem.Fields.ImportanceId.textValue);
                        }
                    }
                    if (dataItem.Fields.PriorityId) {
                        if (dataItem.Fields.PriorityId.cssClass) {
                            item.set_priorityColor(dataItem.Fields.PriorityId.cssClass);
                        }
                        if (dataItem.Fields.PriorityId.textValue) {
                            item.set_priorityTooltip(dataItem.Fields.PriorityId.textValue);
                        }
                    }
                    if (dataItem.Fields.OwnerId) {
                        if (dataItem.Fields.OwnerId.intValue) {
							item.set_ownerId(dataItem.Fields.OwnerId.intValue);
							item.set_ownerIconInitials(dataItem.ownerIconInitials);
                        }
                        if (dataItem.Fields.OwnerId.textValue) {
                            item.set_ownerName(dataItem.Fields.OwnerId.textValue);
                        }
                    }
                    if (dataItem.Fields.Estimate) {
                        if (dataItem.Fields.Estimate.textValue) {
                            item.set_estimate(dataItem.Fields.Estimate.textValue);
                        }
                        if (dataItem.Fields.Estimate.tooltip) {
                            item.set_estimateTooltip(dataItem.Fields.Estimate.tooltip);
                        }
                    }
                    if (dataItem.Fields.Progress) {
                        item.set_progressEqualizer(dataItem.Fields.Progress);
                    }
                    if (dataItem.Fields.TaskCount && !globalFunctions.isNullOrUndefined(dataItem.Fields.TaskCount.intValue)) {
                        item.set_taskCount(dataItem.Fields.TaskCount.intValue);
                        if (dataItem.Fields.TaskCount.tooltip && dataItem.Fields.TaskCount.tooltip != '') {
                            item.set_taskCountTooltip(dataItem.Fields.TaskCount.tooltip);
                        }
                    }
                    if (dataItem.Fields.TestCaseCount && !globalFunctions.isNullOrUndefined(dataItem.Fields.TestCaseCount.intValue)) {
                        item.set_testCaseCount(dataItem.Fields.TestCaseCount.intValue);
                        if (dataItem.Fields.TestCaseCount.tooltip && dataItem.Fields.TestCaseCount.tooltip != '') {
                            item.set_testCaseCountTooltip(dataItem.Fields.TestCaseCount.tooltip);
                        }
                    }

                    //Child tasks
                    if (dataItem.childTasks && dataItem.childTasks.length > 0) {
                        item.set_childTasks(dataItem.childTasks);
                    }

                    //Child test cases
                    if (dataItem.childTestCases && dataItem.childTestCases.length > 0) {
                        item.set_childTestCases(dataItem.childTestCases);
                    }

                    //Render the item
                    item.render();
                }
                div.setAttribute('tst:loaded', 'Y');
            }
        }

        //Increment the index unless loading a single group
        if (context.loadingSingle)
        {
            if (context.callback != undefined)
            {
                context.callback();
            }
        }
        else
        {
            this._incrementIndex(context.callback);
        }
    },
    _incrementIndex: function (callback)
    {
        var dataSource = this.get_dataSource();
        var groupItems = dataSource && dataSource.items;
        this._groupIndex++;
        if (groupItems && this._groupIndex < groupItems.length)
        {
            //Loop again
            var thisRef = this;
            setTimeout(function () { thisRef._loadItems(thisRef, callback); }, 0);
        }
        else
        {
            //Finally call the callback function
            if (callback != undefined)
            {
                callback();
            }
        }
    },

    get_selectedItems: function ()
    {
        //Get all the selected items
        var selectedItems = new Array();
        if (this._items && this._items.length > 0)
        {
            for (i = 0; i < this._items.length; i++)
            {
                var item = this._items[i];
                if (item.get_selected())
                {
                    selectedItems.push(item);
                }
            }
        }
        return selectedItems;
    },

    _operation_error_check: function (result)
    {
        if (result != undefined && result != '')
        {
            globalFunctions.hide_spinner();
            this.display_error(result);

            return true;
        }
        return false;
    },
    operation_failure_quiet: function (exception)
    {
        globalFunctions.hide_spinner();
    },
    operation_failure: function (exception)
    {
        globalFunctions.hide_spinner();
        //Populate the error message control if we have one (if not use alert instead)
        var messageBox = document.getElementById(this.get_errorMessageControlId());
        globalFunctions.display_error(messageBox, exception);
    },

    display_error: function (message)
    {
        //If we have a display element, use that otherwise revert to an alert
        globalFunctions.display_error_message($get(this.get_errorMessageControlId()), message);
    },

    //Scrolls the vertical group by amount (set to null for all the way)
    scroll: function (direction, amount)
    {
        if (this._scrollableContainer)
        {
            if (direction == 'left')
            {
                if (amount)
                {
                    this._scrollableContainer.scrollLeft -= amount;
                }
                else
                {
                    this._scrollableContainer.scrollLeft = 0;
                }
            }
            else
            {
                if (amount)
                {
                    this._scrollableContainer.scrollLeft += amount;
                }
                else
                {
                    this._scrollableContainer.scrollLeft = this._scrollableContainer.scrollWidth;
                }
            }
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

        this._tooltips = new Array();
        this._tooltipOverHandlers = new Array();
        this._tooltipOutHandlers = new Array();
    },
    _clearButtonHandlers: function ()
    {
        for (var i = 0; i < this._buttons.length; i++)
        {
            $removeHandler(this._buttons[i], 'click', this._buttonClickHandlers[i]);
            delete this._buttonClickHandlers[i];
        }
        this._buttons = new Array();
        this._buttonClickHandlers = new Array();
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

    /* Event handler delegates */
    _onTooltipMouseOver: function (evt, e)
    {
        //Display the tooltip
        ddrivetip(e.tooltip);
    },
    _onTooltipMouseOut: function (evt, e)
    {
        hideddrivetip();
    },
    _onDocumentClick: function (evt)
    {
        if (this._itemClicked)
        {
            this._itemClicked = false;
        }
        else
        {
            var items = this.get_selectedItems();
            if (items && items.length > 0)
            {
                //Clear all items
                for (var i = 0; i < items.length; i++)
                {
                    items[i].select_item(true);
                }
            }
        }
    },
    _onAddItemClick: function (evt, args)
	{		
        args.thisRef.raise_addItem(args.thisRef._groupBy, args.containerId);

        // set color mode of the ckeditors - these get reset on loading the add item form
        // this is in a set timeout as a hack to handle it being reset before this code runs
        setTimeout(function () {
            if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.ckeditorSetColorScheme) {
                window.rct_comp_globalNav.ckeditorSetColorScheme(document.body.dataset.colorscheme);
            }
        }, 2500);
    },
    _onReleaseSelect: function (evt, args)
    {
        //Select the appropriate release
        if (args.releaseId && args.thisRef._releaseControlId && args.thisRef._releaseControlId != '')
        {
            var releaseId = args.releaseId;
            var ddlRelease = $find(args.thisRef._releaseControlId);
            ddlRelease.set_selectedItem(releaseId);
        }
    },

    _onScrollClick: function (evt, args)
    {
        //Scroll all the way on ctrl/command-click
        if (evt.ctrlKey || evt.metaKey)
        {
            args.thisRef.scroll(args.direction);
        }
        else
        {
            args.thisRef.scroll(args.direction, 200);
        }
    },
    _onExpandCollapse: function (evt, args)
    {
        //Change the icon
        var div = args.div;
        var primaryKey = div.getAttribute('tst:primarykey');
        var expanding;
        var groupIndex = -1;
        var loadedYn;
        if (div.getAttribute('tst:expanded') == 'Y')
        {
            expanding = false;
            div.className = 'pb-minimize fas fa-caret-right';
            div.setAttribute('tst:expanded', 'N');
        }
        else
        {
            expanding = true;
            div.className = 'pb-minimize fas fa-caret-down';
            div.setAttribute('tst:expanded', 'Y');
        }

        //Hide or show the row below if vertical style
        if (args.style == 'vertical')
        {
            var items = args.thisRef._itemRows;
            for (var i = 0; i < items.length; i++)
            {
                if (items[i].parentNode)
                {
                    loadedYn = items[i].getAttribute('tst:loaded');
                    //TR/TD/DIV
                    var tr = items[i].parentNode.parentNode;
                    if (tr && tr.getAttribute('tst:primarykey') && tr.getAttribute('tst:primarykey') == primaryKey)
                    {
                        //Either show or hide
                        groupIndex = i;
                        if (expanding)
                        {
                            tr.style.display = '';
                        }
                        else
                        {
                            tr.style.display = 'none';
                        }

                        //If it is the unassigned group, locally store expanded state
                        if (primaryKey == -1)
                        {
                            args.thisRef._unassignedIsExpanded = expanding;
                        }

                        //Call the web service to persist this change on the server, we don't do anything if it passes or fails
                        args.thisRef._webServiceClass.PlanningBoard_UpdateExpandCollapsed(args.thisRef._projectId, args.thisRef._groupBy, (primaryKey == -1) ? null : primaryKey, expanding);
                        break;
                    }
                }
            }
        }

        //Hide the containing DIV if horizontal style
        if (args.style == 'horizontal')
        {
            var items = args.thisRef._itemRows;
            for (var i = 0; i < items.length; i++)
            {
                var div = items[i];
                loadedYn = div.getAttribute('tst:loaded');
                if (div.parentNode)
                {
                    //TR/TD/DIV
                    var tr = div.parentNode.parentNode;
                    if (tr && tr.getAttribute('tst:primarykey') && tr.getAttribute('tst:primarykey') == primaryKey)
                    {
                        //Either show or hide
                        groupIndex = i;
                        if (expanding)
                        {
                            div.style.display = '';
                        }
                        else
                        {
                            div.style.display = 'none';
                        }

                        //If it is the unassigned group, locally store expanded state
                        if (primaryKey == -1)
                        {
                            args.thisRef._unassignedIsExpanded = expanding;
                        }

                        //Call the web service to persist this change on the server, we don't do anything if it passes or fails
                        args.thisRef._webServiceClass.PlanningBoard_UpdateExpandCollapsed(args.thisRef._projectId, args.thisRef._groupBy, (primaryKey == -1) ? null : primaryKey, expanding);
                        break;
                    }
                }
            }
        }
        //Hide the items if vertical section and rotate text of header
        if (args.style == 'verticalSection')
        {
            //Hide and rotate the header
            if (div.parentNode)
            {
                if (div.parentNode.parentNode) //this should be the TD
                {
                    if (expanding)
                    {
                        div.parentNode.parentNode.classList.remove("pb-columns_subheader_collapsed");
                    }
                    else
                    {
                        div.parentNode.parentNode.classList.add("pb-columns_subheader_collapsed");
                    }
                }
            }

            //Hide the items
            var items = args.thisRef._itemRows;
            for (var i = 0; i < items.length; i++)
            {
                var itemDiv = items[i];
                loadedYn = itemDiv.getAttribute('tst:loaded');
                if (itemDiv.getAttribute('tst:primarykey') && itemDiv.getAttribute('tst:primarykey') == primaryKey)
                {
                    //Either show or hide
                    groupIndex = i;
                    if (expanding)
                    {
                        itemDiv.style.display = '';
                    }
                    else
                    {
                        itemDiv.style.display = 'none';
                    }

                    //Call the web service to persist this change on the server, we don't do anything if it passes or fails
                    args.thisRef._webServiceClass.PlanningBoard_UpdateExpandCollapsed(args.thisRef._projectId, args.thisRef._groupBy, (primaryKey == -1) ? null : primaryKey, expanding);
                    break;
                }
            }
        }

        //If the group was not already loaded, load it now
        if (groupIndex != -1 && (!loadedYn || loadedYn == 'N'))
        {
            args.thisRef._groupIndex = groupIndex;
            globalFunctions.display_spinner();
            args.thisRef._loadItems(args.thisRef, Function.createDelegate(args.thisRef, args.thisRef._retrieve_success_callback), primaryKey);
        }
    },

    _onExpandAll: function (evt)
    {
        // disable the buttons
        var button = document.getElementById("btnExpandAll");
        var buttonOpposite = document.getElementById("btnCollapseAll");
        button.disabled = true;
        buttonOpposite.disabled = true;
        var icon = document.getElementById("btnExpandAllIcon");
        icon.classList.add("fa-spin");

        globalFunctions.display_spinner();
		var rows = this._itemRows;

		// Reset the counter and then set it to the number of rows that have a parent node - these are the ones we make a server call for / need to wait for
		SpiraContext.uiState.expandCollapseCount = 0;
		rows.forEach(row => row.parentNode && SpiraContext.uiState.expandCollapseCount++);

		// Loop over the rows
        for (var i = 0; i < rows.length; i++)
        {
            var div = rows[i];
            var loadedYn = div.getAttribute('tst:loaded');
            if (div.parentNode) {
                //TR/TD/DIV
                var tr = div.parentNode.parentNode;
                if (tr && tr.getAttribute('tst:primarykey')) {
                    var primaryKey = tr.getAttribute('tst:primarykey');
                    //Show the group
                    div.style.display = '';

                    //If it is the unassigned group, locally store expanded state
                    if (primaryKey == -1) {
                        this._unassignedIsExpanded = true;
					}

                    //Call the web service to persist this change on the server
					this._webServiceClass.PlanningBoard_UpdateExpandCollapsed(
						this._projectId,
						this._groupBy,
						(primaryKey == -1) ? null : primaryKey,
						true,
						Function.createDelegate(this, this._updateExpandCollapsed_Success)
					);
                }
            }
        }
    },
    _onCollapseAll: function (evt)
    {
        // disable the buttons
        var button = document.getElementById("btnCollapseAll");
        var buttonOpposite = document.getElementById("btnExpandAll");
        button.disabled = true;
        buttonOpposite.disabled = true;
        var icon = document.getElementById("btnCollapseAllIcon");
        icon.classList.add("fa-spin");

        globalFunctions.display_spinner();
		var rows = this._itemRows;

		// Reset the counter and then set it to the number of rows that have a parent node - these are the ones we make a server call for / need to wait for
		SpiraContext.uiState.expandCollapseCount = 0;
		rows.forEach(row => row.parentNode && SpiraContext.uiState.expandCollapseCount++);

        for (var i = 0; i < rows.length; i++) {
            var div = rows[i];
            var loadedYn = div.getAttribute('tst:loaded');
            if (div.parentNode) {
                //TR/TD/DIV
                var tr = div.parentNode.parentNode;
                if (tr && tr.getAttribute('tst:primarykey')) {
                    var primaryKey = tr.getAttribute('tst:primarykey');
                    //Hide the group
                    div.style.display = 'none';

                    //If it is the unassigned group, locally store expanded state
                    if (primaryKey == -1) {
                        this._unassignedIsExpanded = false;
                    }

                    //Call the web service to persist this change on the server
					this._webServiceClass.PlanningBoard_UpdateExpandCollapsed(
						this._projectId,
						this._groupBy,
						(primaryKey == -1) ? null : primaryKey,
						false,
						Function.createDelegate(this, this._updateExpandCollapsed_Success),
						Function.createDelegate(this, this._updateExpandCollapsed_Failure)
					);
                }
            }
        }
	},

	_updateExpandCollapsed_Success: function () {
		//On success we decrement the count (that is set to the total number of server calls we need to make for the relevant expand or collapse operation)
		SpiraContext.uiState.expandCollapseCount--;
		//If the counter is at 0 then all updates have succeeded and we can reset and reload the data
		if (SpiraContext.uiState.expandCollapseCount === 0) {
			//Reset the UI
			globalFunctions.hide_spinner();
			document.getElementById("btnCollapseAll").disabled = false;
			document.getElementById("btnExpandAll").disabled = false;
			document.getElementById("btnCollapseAllIcon").classList.remove("fa-spin");
			document.getElementById("btnExpandAllIcon").classList.remove("fa-spin");

			//Load the data
			this.load_data();
		}
	},

	//handle failures identically to successes so that the expand / collapse does not hang in case one of the calls comes back with an error message
	_updateExpandCollapsed_Failure: function () {
		SpiraContext.uiState.expandCollapseCount--;
		//If the counter is at 0 then all updates have succeeded and we can reset and reload the data
		if (SpiraContext.uiState.expandCollapseCount === 0) {
			//Reset the UI
			globalFunctions.hide_spinner();
			document.getElementById("btnCollapseAll").disabled = false;
			document.getElementById("btnExpandAll").disabled = false;
			document.getElementById("btnCollapseAllIcon").classList.remove("fa-spin");
			document.getElementById("btnExpandAllIcon").classList.remove("fa-spin");

			//Load the data
			this.load_data();
		}
	},

    //Updates the group by dropdown with available options
    _updateAvailableGroupByOptions: function ()
    {
        if (this._groupByControlId && this._groupByControlId != '')
        {
            var releaseId = this._releaseId;
            var isIteration = this._isIteration;
            var ddlGroupBy = $find(this._groupByControlId);
            if (ddlGroupBy)
            {
                var items = ddlGroupBy.get_items();
                if (items)
                {
                    //We first try and select the same item (in case it is still available), if not, get the first active item
                    var firstActiveItem = '';
                    var currentItemStillActive = '';
                    for (var i = 0; i < items.length; i++)
                    {
                        var groupById = items[i].get_value();
                        var isActive = this._determineGroupByActiveFlag(groupById, (releaseId == -1), (releaseId == -2), isIteration);
                        items[i].set_active(isActive);
                        if (isActive == 'Y' && firstActiveItem == '')
                        {
                            firstActiveItem = groupById;
                        }
                        if (isActive == 'Y' && groupById == this._groupBy) {
                            currentItemStillActive = groupById;
                        }
                    }
                    if (currentItemStillActive && currentItemStillActive != '')
                    {
                        this.set_groupBy(currentItemStillActive);
                        ddlGroupBy.set_selectedItem(currentItemStillActive, true); //Don't fire a second event
                    }
                    else if (firstActiveItem && firstActiveItem != '')
                    {
                        this.set_groupBy(firstActiveItem);
                        ddlGroupBy.set_selectedItem(firstActiveItem, true); //Don't fire a second event
                    }
                    ddlGroupBy.render_items(false);
                }
            }
        }
    },

    _determineGroupByActiveFlag: function (groupById, noReleaseSelected, allReleasesSelected, iterationSelected)
    {
        var result = true;
        if (groupById == this._groupBy_ByComponent)
        {
            result = true;  //All options now allow this grouping
        }
        if (groupById == this._groupBy_ByPackage)
        {
            result = true;  //All options now allow this grouping
        }
        if (groupById == this._groupBy_ByPriority)
        {
            result = true;  //All options now allow this grouping
        }
        if (groupById == this._groupBy_ByRelease)
        {
            result = (allReleasesSelected);
        }
        if (groupById == this._groupBy_ByIteration)
        {
            result = (!noReleaseSelected && !allReleasesSelected && !iterationSelected);
        }
        if (groupById == this._groupBy_ByStatus)
        {
            result = true;  //All options now allow this grouping
        }
        if (groupById == this._groupBy_ByPerson)
        {
            result = (!noReleaseSelected);
        }
        if (groupById == this._groupBy_ByRequirement) {
            //You need to have a release or iteration selected
            result = (!noReleaseSelected && !allReleasesSelected);
        }
        return (result) ? 'Y' : 'N';
    },

    /*  =========================================================
    Event handlers managers
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

    add_changeGroupBy: function (handler)
    {
        this.get_events().addHandler('changeGroupBy', handler);
    },
    remove_changeGroupBy: function (handler)
    {
        this.get_events().removeHandler('changeGroupBy', handler);
    },
    raise_changeGroupBy: function (primaryKey)
    {
        var h = this.get_events().getHandler('changeGroupBy');
        if (h) h(primaryKey);
    },

    add_addItem: function (handler)
    {
        this.get_events().addHandler('addItem', handler);
    },
    remove_addItem: function (handler)
    {
        this.get_events().removeHandler('addItem', handler);
    },
    raise_addItem: function (groupBy, containerId)
    {
        var h = this.get_events().getHandler('addItem');
        if (h) h(groupBy, containerId);
	},

	add_editItem: function (handler) {
		this.get_events().addHandler('editItem', handler);
	},
	remove_editItem: function (handler) {
		this.get_events().removeHandler('editItem', handler);
	},
	raise_editItem: function (artifactTypeId, artifactId) {
		var h = this.get_events().getHandler('editItem');
		if (h) h(artifactTypeId, artifactId);
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
    }
}
Inflectra.SpiraTest.Web.ServerControls.PlanningBoard.registerClass('Inflectra.SpiraTest.Web.ServerControls.PlanningBoard', Sys.UI.Control);

/* PlanningBoardContainerBehavior */
Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior = function (element)
{
    // initialize base class
    Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior.initializeBase(this, [element]);

    this._primaryKey = null;
    this._parent = null;
}
Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior.prototype =
{
    initialize: function ()
    {
        // initialize base class
        Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior.callBaseMethod(this, "initialize");

        // register this drop target in DragDropManager
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);
    },

    dispose: function ()
    {
        // unregister this drop target in DragDropManager
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.unregisterDropTarget(this);

        // disponse base class
        Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior.callBaseMethod(this, "dispose");
    },

    /* Properties */

    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },

    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (value)
    {
        this._parent = value;
    },

    /* Methods */
    _resolveItem: function (x, y, width, height)
    {
        //Loop through all the items
        var items = this._parent.get_items();
        var resolvedItem = null;
        for (var i = 0; i < items.length; i++)
        {
			var item = items[i];
			//Only check items in the correct container
            if (item.get_containerId() == this._primaryKey)
            {
				//Get the coordinates
				var element = item.get_element();
				var position = globalFunctions.getElementWindowPosition(element);
				var itemx = position.left;
				var itemWidth = element.offsetWidth;
				var itemy = position.top;
				
				//Check if the item is immediately below the card being dropped
				var inRangeLeft = (x + width) >= itemx; // the card's right edge must to the right of the item's left edge
				var inRangeRight = x <= (itemx + itemWidth); //the card's left edge must be to the left of the item's right edge 
				var inRangeTop = y <= itemy; //the card's top edge must be above the the item's own top edge 
				var inRangeBottom = (y + height) >= itemy; // the card's bottom edge must be below the item's top edge

				if (inRangeLeft && inRangeRight && inRangeTop && inRangeBottom)
                {
                    resolvedItem = item;
                    break;
                }
            }
        }
        return resolvedItem;
    },

    //Move an item
    moveItem: function (draggedItem, existingItem)
    {
        //Get the ids for the item being dragged
        var artifactTypeId = draggedItem.get_artifactTypeId();
        var artifactId = draggedItem.get_primaryKey();

        //Visually move this item
        this.moveItemDisplay(draggedItem, existingItem);

        //Create an array of items being moved
        var items = [{ 'artifactTypeId': artifactTypeId, 'artifactId': artifactId}];

        //Add any of the already selected items (unless the one being dragged)
        var selectedItems = this._parent.get_selectedItems();
        if (selectedItems && selectedItems.length > 0)
        {
            for (var i = 0; i < selectedItems.length; i++)
            {
                var item = selectedItems[i];
                if (item.get_artifactTypeId() != artifactTypeId || item.get_primaryKey() != artifactId)
                {
                    this.moveItemDisplay(item, existingItem);
                    items.push({ 'artifactTypeId': item.get_artifactTypeId(), 'artifactId': item.get_primaryKey() });
                }
            }
        }

        //Now call service to persist
        this._parent.moveItems(items, this._primaryKey, existingItem);
    },
    moveItemDisplay: function (item, existingItem)
    {
        //Detatch item from position
        var parentNode = item.get_element().parentNode;
        if (parentNode)
        {
            parentNode.removeChild(item.get_element());
            var wipLimit = parentNode.getAttribute('tst:wiplimit');
            var subheader = document.getElementById('pb-columns_subheader' + '_' + parentNode.getAttribute('tst:primaryKey'));
            planningBoardWipUpdateUI(parentNode, subheader, wipLimit, parentNode.childElementCount);

            if (existingItem && existingItem.get_element() && existingItem.get_element().parentNode)
            {
                var existingItemParent = existingItem.get_element().parentNode;
                existingItemParent.insertBefore(item.get_element(), existingItem.get_element());

                wipLimit = existingItemParent.getAttribute('tst:wiplimit');
                subheader = document.getElementById('pb-columns_subheader' + '_' + existingItemParent.getAttribute('tst:primaryKey'));
                planningBoardWipUpdateUI(parentNode, subheader, wipLimit, existingItemParent.childElementCount);
            }
            else
            {
                //See if we have a TD or DIV, get child if TD
                var div = this._element;
                if (this._element.tagName == 'TD')
                {
                    div = this._element.firstChild;
                }
                
                //add the card to the div
                div.appendChild(item.get_element());

                //handle wip limits
                wipLimit = div.getAttribute('tst:wiplimit');
                subheader = document.getElementById('pb-columns_subheader' + '_' + div.getAttribute('tst:primaryKey'));

                planningBoardWipUpdateUI(div, subheader, wipLimit, div.childElementCount);
            }
        }
    },

    /* IDragTarget Interface */

    // return a DOM element represents the container
    get_dropTargetElement: function ()
    {
        return this.get_element();
    },

    // if this draggable item can be dropped into this drop target
    canDrop: function (dragMode, dataType, data)
    {
        return (dataType == "PlanningBoardItem" && data);
    },

    // drop this draggable item into this drop target
    drop: function (dragMode, dataType, data)
    {
        if (dataType == "PlanningBoardItem" && data)
        {
            //Clear the color
			var element = this.get_element()
			element.classList.remove('is-active');

            //Get the coordinates
            var visual = data.get_visual();
			var position = globalFunctions.getElementWindowPosition(visual);
			var x = position.left;
			var y = position.top;
            var visualHeight = visual.offsetHeight;
            var visualWidth = visual.offsetWidth;

            //See if the item was dropped in front of an existing item
            var existingItem = null;
            if (!isNaN(x) && !isNaN(y))
            {
                existingItem = this._resolveItem(x, y, visualWidth, visualHeight);
            }

            //Clear the visual
            visual.parentNode.removeChild(visual);
            data.clear_visual();

            //Call the function to move the item(s)
            this.moveItem(data, existingItem);
        }
    },

    // this method will be called when a draggable item is entering this drop target
    onDragEnterTarget: function (dragMode, dataType, data)
    {
        if (dataType == "PlanningBoardItem" && data)
        {
            this.get_element().classList.add('is-active');
        }
    },

    // this method will be called when a draggable item is leaving this drop target
    onDragLeaveTarget: function (dragMode, dataType, data)
    {
        if (dataType == "PlanningBoardItem" && data)
        {
            this.get_element().classList.remove('is-active');
        }
    },

    // this method will be called when a draggable item is hovering on this drop target
    onDragInTarget: function (dragMode, dataType, data)
    {
    }
};
Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior.registerClass("Inflectra.SpiraTest.Web.ServerControls.PlanningBoardContainerBehavior", Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDropTarget);
        
/* PlanningBoardItem */
Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem = function (element)
{
    if (!element)
    {
        element = d.ce('table');
    }
    this._element = element;

    //Member variables
    this._containerId = null;
    this._primaryKey = null;
    this._artifactTypeId = null;
    this._token = '';
    this._name = '';
    this._url = '';
    this._image = '';
    this._description = '';
    this._plannable = true;
    this._selected = false;
    this._priorityClass = '';
    this._priorityColor = '';
    this._priorityTooltip = '';
    this._ownerId = '';
    this._ownerName = '';
    this._taskCount = null;
    this._testCaseCount = null;
    this._taskCountTooltip = null;
    this._testCaseCountTooltip = null;
    this._estimate = '';
    this._estimateTooltip = '';
    this._isOverNameDesc = false;
    this._parent = null;
    this._progressEqualizer = null;
    this._childTasks = null;
    this._childTestCases = null;
    this._progressBar = null;
    this._visual = null;
    this._holdActive = false;
    this._holdStarter = null;
    this._holdDelay = 200; // Milliseconds to wait before recognizing a hold
    this._touchDragging = false;
    this._position = null;

    //Event Handler References
    this._mouseDownHandler = null;
    this._mouseUpHandler = null;
    this._touchMoveHandler = null;
    this._tooltips = new Array();
    this._tooltipOverHandlers = new Array();
	this._tooltipOutHandlers = new Array();
	this._clickItems = new Array();
	this._clickHandlers = new Array();

    Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem.initializeBase(this, [element]);
}
Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem.callBaseMethod(this, 'initialize');
    },
    dispose: function ()
    {
        //Clear any handlers
		this._clearTooltipHandlers();
		this._clearClickHandlers();

        if (this._plannable && this._element)
        {
            if (this._mouseDownHandler)
            {
                $removeHandler(this._element, 'mousedown', this._mouseDownHandler);
                delete this._mouseDownHandler;
            }
            if (this._mouseUpHandler)
            {
                $removeHandler(this._element, 'mouseup', this._mouseUpHandler);
                delete this._mouseUpHandler;
            }
            if (this._touchMoveHandler && this._element.removeEventListener)
            {
                this._element.removeEventListener('touchmove', this._touchMoveHandler, false);
                delete this._touchMoveHandler;
            }
        }

        //Clear other objects
        if (this._progressBar)
        {
            this._progressBar.dispose();
            delete this._progressBar;
        }

        delete this._progressEqualizer;
        delete this._childTasks;
        delete this._childTestCases;
        delete this._visual;

        Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem.callBaseMethod(this, 'dispose');
    },

    /* Properties */
    get_containerId: function ()
    {
        return this._containerId;
    },
    set_containerId: function (value)
    {
        this._containerId = value;
    },

    get_primaryKey: function ()
    {
        return this._primaryKey;
    },
    set_primaryKey: function (value)
    {
        this._primaryKey = value;
    },

    get_position: function ()
    {
        return this._position;
    },
    set_position: function (value)
    {
        this._position = value;
    },

    get_artifactTypeId: function ()
    {
        return this._artifactTypeId;
    },
    set_artifactTypeId: function (value)
    {
        this._artifactTypeId = value;
    },

    get_token: function ()
    {
        return this._token;
    },
    set_token: function (value)
    {
        this._token = value;
    },

    get_selected: function ()
    {
        return this._selected;
    },

    get_image: function ()
    {
        return this._image;
    },
    set_image: function (value)
    {
        this._image = value;
    },

    get_url: function ()
    {
        return this._url;
    },
    set_url: function (value)
    {
        this._url = value;
    },

    get_name: function ()
    {
        return this._name;
    },
    set_name: function (value)
    {
        this._name = value;
    },

    get_description: function ()
    {
        return this._description;
    },
    set_description: function (value)
    {
        this._description = value;
    },

    get_plannable: function ()
    {
        return this._plannable;
    },
    set_plannable: function (value)
    {
        this._plannable = value;
    },

    get_priorityClass: function ()
    {
        return this._priorityClass;
    },
    set_priorityClass: function (value)
    {
        this._priorityClass = value;
    },

    get_priorityColor: function ()
    {
        return this._priorityColor;
    },
    set_priorityColor: function (value)
    {
        this._priorityColor = value;
    },

    get_priorityTooltip: function ()
    {
        return this._priorityTooltip;
    },
    set_priorityTooltip: function (value)
    {
        this._priorityTooltip = value;
    },

    get_ownerId: function ()
    {
        return this._ownerId;
    },
    set_ownerId: function (value)
    {
        this._ownerId = value;
    },

    get_ownerName: function ()
    {
        return this._ownerName;
    },
    set_ownerName: function (value)
    {
        this._ownerName = value;
    },

    get_estimate: function ()
    {
        return this._estimate;
    },
    set_estimate: function (value)
    {
        this._estimate = value;
    },

    get_estimateTooltip: function ()
    {
        return this._estimateTooltip;
    },
    set_estimateTooltip: function (value)
    {
        this._estimateTooltip = value;
    },

    get_parent: function ()
    {
        return this._parent;
    },
    set_parent: function (value)
    {
        this._parent = value;
    },

    get_progressEqualizer: function ()
    {
        return this._progressEqualizer;
    },
    set_progressEqualizer: function (value)
    {
        this._progressEqualizer = value;
    },

    get_taskCount: function ()
    {
        return this._taskCount;
    },
    set_taskCount: function (value)
    {
        this._taskCount = value;
    },

    get_testCaseCount: function ()
    {
        return this._testCaseCount;
    },
    set_testCaseCount: function (value)
    {
        this._testCaseCount = value;
    },

    get_taskCountTooltip: function ()
    {
        return this._taskCountTooltip;
    },
    set_taskCountTooltip: function (value)
    {
        this._taskCountTooltip = value;
    },

    get_testCaseCountTooltip: function ()
    {
        return this._testCaseCountTooltip;
    },
    set_testCaseCountTooltip: function (value)
    {
        this._testCaseCountTooltip = value;
    },

    get_childTasks: function ()
    {
        return this._childTasks;
    },
    set_childTasks: function (value)
    {
        this._childTasks = value;
    },

    get_childTestCases: function ()
    {
        return this._childTestCases;
    },
    set_childTestCases: function (value)
    {
        this._childTestCases = value;
    },

    get_visual: function ()
    {
        return this._visual;
    },
    clear_visual: function ()
    {
        delete this._visual;
	},

	get_ownerIconInitials: function () {
		return this._ownerIconInitials;
	},
	set_ownerIconInitials: function (value) {
		this._ownerIconInitials = value;
	},

    /* Methods */
    render: function ()
    {
        var table = this._element;
        //Make the item look inactive if it can't be planned with
        if (this._plannable)
        {
            table.className = 'pb-card';
        }
        else
        {
            table.className = 'pb-card inactive';
        }

        //First we add the color indicator on the side
        var tbody = d.ce('tbody');
        table.appendChild(tbody);
        var tr = d.ce('tr');
        tbody.appendChild(tr);
        var th = d.ce('th');
        tr.appendChild(th);
        th.className = 'pb-card_sidebar';
        th.rowSpan = 3;

        //Color based on CSS or color
        if (this._priorityClass && this._priorityClass != '')
        {
            th.className = 'pb-card_sidebar ' + this._priorityClass;
        }
        else if (this._priorityColor && this._priorityColor != '')
        {
            th.style.backgroundColor = this._priorityColor;
        }
        th.title = this._priorityTooltip;

        //Next the body
        td = d.ce('td');
        td.className = "pb-card_body"
        tr.appendChild(td);

        //Create the icon and token
        var div = d.ce('div');
        div.className = 'pb-card_body-header';
        td.appendChild(div);

        //Icon - make draggable if active and authorized
        var img = d.ce('img');
        img.className = 'pb-card_artifact-icon w4 h4';
        if (this._plannable)
        {
            //Add the drag handlers
            this._element.style.cursor = 'move';

            this._mouseDownHandler = Function.createDelegate(this, this._onMouseDown);
            this._mouseUpHandler = Function.createDelegate(this, this._onMouseUp);
            this._touchMoveHandler = Function.createDelegate(this, this._onTouchMove);
            $addHandler(this._element, 'mousedown', this._mouseDownHandler);
            $addHandler(this._element, 'mouseup', this._mouseUpHandler);
            if (this._element.addEventListener) //IE8 doesn't support
            {
                this._element.addEventListener('touchmove', this._touchMoveHandler, false);
            }
        }
        img.src = this._parent.get_themeFolder() + 'Images/' + this._image;
        img.alt = this._image;
        div.appendChild(img);

        //Artifact ID and prefix
        var a = d.ce('a');
        div.appendChild(a);
        a.appendChild(d.createTextNode(this._token));
		a.href = this._url;

		//Add the edit item click handler, overrides the URL navigation unless new browser tab/page
		var editItemHandler = Function.createDelegate(this, this._onEditItemClick);
		$addHandler(a, 'click', editItemHandler);
		this._clickItems.push(a);
		this._clickHandlers.push(editItemHandler);

        //Add the asynchronous tooltip handler
        var tooltipMouseOverHandler = Function.createDelegate(this, this._onNameTooltipMouseOver);
        var tooltipMouseOutHandler = Function.createDelegate(this, this._onNameTooltipMouseOut);
        $addHandler(a, 'mouseover', tooltipMouseOverHandler);
        $addHandler(a, 'mouseout', tooltipMouseOutHandler);
        this._tooltips.push(a);
        this._tooltipOverHandlers.push(tooltipMouseOverHandler);
        this._tooltipOutHandlers.push(tooltipMouseOutHandler);

        //Add the avatar/estimate
        div = d.ce('div');
        div.className = 'pb-card_avatar-estimate';
        td.appendChild(div);
        if (this._ownerId)
		{
			// if the _ownerIconInitials property is NOT in the data item it means the user has a non-default avatar that we should use
			if (!this._ownerIconInitials) {
				img = d.ce('img');
				img.className = 'pb-card_avatar';
				img.src = this._parent.get_avatarBaseUrl().replace('{0}', this._ownerId);
				img.alt = this._ownerName;
				div.appendChild(img);
			}
			//if the _ownerIconInitials property IS in the data item then we should use to display as text
			else if (this._ownerName && this._ownerName != '')
			{
				img = d.ce('div');
				img.className = "pb-card_avatar dit bg-light-gray ov-hidden tc";
				var imgSpan = d.ce('span');
				imgSpan.className = "fs-200 white y-center dib lh0 fw-b";
				imgSpan.innerText = this._ownerIconInitials;
				img.appendChild(imgSpan);
				div.appendChild(img);
			}
            if (this._ownerName && this._ownerName != '')
            {
                var mouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: this._ownerName });
                var mouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(img, 'mouseover', mouseOverHandler);
                $addHandler(img, 'mouseout', mouseOutHandler);
                this._tooltips.push(img);
                this._tooltipOverHandlers.push(mouseOverHandler);
                this._tooltipOutHandlers.push(mouseOutHandler);
            }
        }
        if (this._estimate)
        {
            var span = d.ce('span');
            span.appendChild(d.createTextNode(this._estimate));
            div.appendChild(span);
            if (this._estimateTooltip && this._estimateTooltip != '')
            {
                var mouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: this._estimateTooltip });
                var mouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(span, 'mouseover', mouseOverHandler);
                $addHandler(span, 'mouseout', mouseOutHandler);
                this._tooltips.push(span);
                this._tooltipOverHandlers.push(mouseOverHandler);
                this._tooltipOutHandlers.push(mouseOutHandler);
            }
        }

        //Now we add the name/details underneath
        var detailedView = this._parent.get_includeDetails();
        div = d.ce('div');
        div.className = 'pb-card_body-details';
        div.style.height = (detailedView) ? '80px' : '32px';
        td.appendChild(div);

        //Add the artifact name and description
        var span = d.ce('span');
        if (detailedView)
        {
            span.className = 'pb-card_body-details-title';
        }
        div.appendChild(span);
        span.appendChild(d.createTextNode(this._name));
        if (detailedView && this._description)
        {
            div.appendChild(d.createTextNode(' - '));
            div.appendChild(d.createTextNode(this._description));
        }

        if (detailedView)
        {
            //The progress bar
            if (this._progressEqualizer)
            {
                var dataItemField = this._progressEqualizer;
                var div = d.ce('div');
                div.className = 'pb-card_equalizer';
                var props = {};
                props.percentGray = dataItemField.equalizerGray;
                props.percentGreen = dataItemField.equalizerGreen;
                props.percentRed = dataItemField.equalizerRed;
                props.percentOrange = dataItemField.equalizerOrange;
                props.percentYellow = dataItemField.equalizerYellow;
                props.toolTip = dataItemField.tooltip;
                this._progressBar = $create(Inflectra.SpiraTest.Web.ServerControls.Equalizer, props, null, null, div);
                td.appendChild(div);
            }

            //The task count
            if (!globalFunctions.isNullOrUndefined(this._taskCount))
            {
                var div = d.ce('div');
                div.className = 'TaskCount';
                div.appendChild(d.createTextNode(this._taskCount));
                div.title = this._taskCountTooltip;
                td.appendChild(div);
            }

            //The task count
            if (!globalFunctions.isNullOrUndefined(this._testCaseCount))
            {
                var div = d.ce('div');
                div.className = 'TestCaseCount';
                div.appendChild(d.createTextNode(this._testCaseCount));
                div.title = this._testCaseCountTooltip;
                td.appendChild(div);
            }

            //Also render the position (ranking) as a number
            //If the position has 5 digits we need to make it smaller
            if (this._position)
            {
                var div = d.ce('div');
                div.className = this._position > 9999 ? 'pb-card-position-legend-sm' : 'pb-card-position-legend';
                div.appendChild(d.createTextNode(this._position));
                td.appendChild(div);
            }
        }

        //The list of child tasks
        if (this._childTasks)
        {
            tr = d.ce('tr');
            tbody.appendChild(tr);
            td = d.ce('td');
            td.colSpan = 2;
            tr.appendChild(td);
            var childContainer = d.ce('div');
            childContainer.className = 'pb-subcard-box';
            td.appendChild(childContainer);
            for (var j = 0; j < this._childTasks.length; j++)
            {
                var childTask = this._childTasks[j];
                var taskDiv = d.ce('div');
                taskDiv.className = 'pb-subcard';
                childContainer.appendChild(taskDiv);
                var token = childTask.Fields.Token.textValue;
                var a = d.ce('a');
                taskDiv.appendChild(a);
                a.appendChild(d.createTextNode(token));
                a.href = childTask.customUrl;
                //Tooltip
                var mouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: childTask.Fields.Token.tooltip });
                var mouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(a, 'mouseover', mouseOverHandler);
                $addHandler(a, 'mouseout', mouseOutHandler);
                this._tooltips.push(a);
                this._tooltipOverHandlers.push(mouseOverHandler);
                this._tooltipOutHandlers.push(mouseOutHandler);

                //The progress bar
                if (childTask.Fields.Progress)
                {
                    var dataItemField = childTask.Fields.Progress;
                    var div = d.ce('div');
                    div.className = 'pb-subcard_equalizer';
                    var props = {};
                    props.percentGray = dataItemField.equalizerGray;
                    props.percentGreen = dataItemField.equalizerGreen;
                    props.percentRed = dataItemField.equalizerRed;
                    props.percentOrange = dataItemField.equalizerOrange;
                    props.percentYellow = dataItemField.equalizerYellow;
                    props.toolTip = dataItemField.tooltip;
                    props.pixelWidth = 40;
                    var progressBar = $create(Inflectra.SpiraTest.Web.ServerControls.Equalizer, props, null, null, div);
                    taskDiv.appendChild(div);
                }
            }
        }

        //The list of child test cases
        if (this._childTestCases)
        {
            tr = d.ce('tr');
            tbody.appendChild(tr);
            td = d.ce('td');
            td.colSpan = 2;
            tr.appendChild(td);
            var childContainer = d.ce('div');
            childContainer.className = 'pb-subcard-box';
            td.appendChild(childContainer);
            for (var j = 0; j < this._childTestCases.length; j++)
            {
                var childTestCase = this._childTestCases[j];
                var tcDiv = d.ce('div');
                tcDiv.className = 'pb-subcard';
                childContainer.appendChild(tcDiv);
                var token = childTestCase.Fields.Token.textValue;
                var a = d.ce('a');
                a.className = 'gray orange-dark-hover';
                tcDiv.appendChild(a);
                a.appendChild(d.createTextNode(token));
                a.href = childTestCase.customUrl;
                //Tooltip
                var mouseOverHandler = Function.createCallback(this._onTooltipMouseOver, { thisRef: this, tooltip: childTestCase.Fields.Token.tooltip });
                var mouseOutHandler = Function.createCallback(this._onTooltipMouseOut, { thisRef: this });
                $addHandler(a, 'mouseover', mouseOverHandler);
                $addHandler(a, 'mouseout', mouseOutHandler);
                this._tooltips.push(a);
                this._tooltipOverHandlers.push(mouseOverHandler);
                this._tooltipOutHandlers.push(mouseOutHandler);

                //The progress bar
                if (childTestCase.Fields.ExecutionStatus)
                {
                    var dataItemField = childTestCase.Fields.ExecutionStatus;
                    var div = d.ce('div');
                    div.className = 'pb-subcard_equalizer';
                    var props = {};
                    props.percentGray = dataItemField.equalizerGray;
                    props.percentGreen = dataItemField.equalizerGreen;
                    props.percentRed = dataItemField.equalizerRed;
                    props.percentOrange = dataItemField.equalizerOrange;
                    props.percentYellow = dataItemField.equalizerYellow;
                    props.toolTip = dataItemField.tooltip;
                    props.pixelWidth = 40;
                    var progressBar = $create(Inflectra.SpiraTest.Web.ServerControls.Equalizer, props, null, null, div);
                    tcDiv.appendChild(div);
                }
            }
        }
    },

    select_item: function (forceDeselected)
    {
        if (this._plannable)
        {
            //Set flag and change color
            if (forceDeselected)
            {
                this._selected = false;
            }
            else
            {
                this._selected = !this._selected;
            }
            this._element.className = (this._selected) ? 'pb-card is-selected' : 'pb-card';
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

        this._tooltips = new Array();
        this._tooltipOverHandlers = new Array();
        this._tooltipOutHandlers = new Array();
	},
	_clearClickHandlers: function () {
		for (var i = 0; i < this._clickItems.length; i++) {
			$removeHandler(this._clickItems[i], 'click', this._clickHandlers[i]);
			delete this._clickHandlers[i];
		}

		this._clickItems = new Array();
		this._clickHandlers = new Array();
	},
    _startDrag: function (evt)
    {
        window._event = evt; // Needed internally by _DragDropManager

        this._visual = this._element.cloneNode(true);
        this._visual.style.opacity = '0.7';
        this._visual.style.filter = 'progid:DXImageTransform.Microsoft.BasicImage(opacity=0.7)';
        this._visual.style.zIndex = 99999;
        this._element.parentNode.appendChild(this._visual);
        var location = Sys.UI.DomElement.getLocation(this._element);
        Sys.UI.DomElement.setLocation(this._visual, location.x, (location.y - 80));
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(this, this._visual, null);
        if (evt.preventDefault) { evt.preventDefault(); }
    },

    /* Event Handlers */
    _onNameTooltipMouseOver: function (evt)
    {
        //Display the loading message
        ddrivetip(resx.Global_Loading);
        this._isOverNameDesc = true;   //Set the flag since asynchronous

        //Now get the real tooltip via Ajax web-service call
        this._parent.get_webServiceClass().PlanningBoard_RetrieveItemTooltip(this._parent.get_projectId(), this._primaryKey, this._artifactTypeId, Function.createDelegate(this, this.retrieveTooltip_success));
    },
    _onNameTooltipMouseOut: function (evt)
    {
        hideddrivetip();
        this._isOverNameDesc = false;
    },
    retrieveTooltip_success: function (tooltipData)
    {
        if (this._isOverNameDesc)
        {
            ddrivetip(tooltipData);
        }
    },

    _onMouseDown: function (evt)
    {
        evt.cancelBubble = true;
        evt.stopPropagation();
        if (evt.preventDefault) { evt.preventDefault(); }
        this._parent.set_itemClicked(true);
        var thisRef = this;
        this._holdStarter = setTimeout(function ()
        {
            thisRef._holdStarter = null;
            thisRef._holdActive = true;
            thisRef._startDrag(evt);
        }, this._holdDelay);
    },
    _onMouseUp: function (evt)
    {
        if (this._holdStarter)
        {
            clearTimeout(this._holdStarter);
            this.select_item();
        }
        else if (this._holdActive)
        {
            this._holdActive = false;
        }
    },
    _onTouchMove: function (evt)
    {
        //Make sure not already dragging
        if (!this._touchDragging)
        {
            this._touchDragging = true;
            this._startDrag(evt);
        }
    },

    _onTooltipMouseOver: function (evt, e)
    {
        //Display the tooltip
        ddrivetip(e.tooltip);
    },
    _onTooltipMouseOut: function (evt, e)
    {
        hideddrivetip();
	},

	_onEditItemClick: function (evt) {
		//Ignore shift/ctrl clicks, and process click as normal too if on a board that does not support editing
		if (this._parent._boardSupportsEditing && !evt.shiftKey && !evt.ctrlKey && !evt.metaKey) {
			evt.stopPropagation();
			evt.preventDefault();
			this._parent.set_itemClicked(false);	//Stops it selecting the item
			this._parent.raise_editItem(this._artifactTypeId, this._primaryKey);

			// set color mode of the ckeditors - these get reset on loading the add item form
			// this is in a set timeout as a hack to handle it being reset before this code runs
			setTimeout(function () {
				if (window && window.rct_comp_globalNav && window.rct_comp_globalNav.ckeditorSetColorScheme) {
					window.rct_comp_globalNav.ckeditorSetColorScheme(document.body.dataset.colorscheme);
				}
			}, 2500);
		}
	},

    /* IDragSource Interface */

    get_dragDataType: function ()
    {
        return 'PlanningBoardItem';
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
        //Scroll left/right any overflow-x elements
        var evt = window._event;
        var x;
        if (evt.changedTouches)
        {
            //touch
            x = evt.changedTouches[0].pageX;
        }
        else
        {
            //mouse
            x = evt.clientX;
        }
        var scrolled = false;
        if (x < 150)
        {
            this._parent.scroll('left', 20);
            scrolled = true;
        }
        if (x > window.screen.width - 200)
        {
            this._parent.scroll('right', 20);
            scrolled = true;
        }
        if (scrolled)
        {
            var ddm = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance();
            if (evt.changedTouches)
            {
                //touch
                var touchPosition = { x: evt.changedTouches[0].pageX, y: evt.changedTouches[0].pageY };
                var position = ddm.subtractPoints(touchPosition, this._visual.startingPoint);
                Sys.UI.DomElement.setLocation(this._visual, position.x, position.y);
            }
            else
            {
                //mouse
                var mousePosition = { x: evt.clientX, y: evt.clientY };
                var scrollOffset = ddm.getScrollOffset(this._visual, /* recursive */true);
                var position = ddm.addPoints(ddm.subtractPoints(mousePosition, this._visual.startingPoint), scrollOffset);
                Sys.UI.DomElement.setLocation(this._visual, position.x, position.y);
            }
        }
    },

    onDragEnd: function (canceled)
    {
        this._touchDragging = false;
        //Clear the visual
        if (this._visual)
        {
            this._visual.parentNode.removeChild(this._visual);
            delete this._visual;
        }
        //Deselect all items
        var items = this._parent.get_selectedItems();
        if (items && items.length > 0)
        {
            //Clear all items
            for (var i = 0; i < items.length; i++)
            {
                items[i].select_item(true);
            }
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem.registerClass('Inflectra.SpiraTest.Web.ServerControls.PlanningBoardItem', Sys.UI.Control);

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

//handles the UI for the wip badge and color indicator on the column and subheader
//div = element of container div of the cards
//subheader = element of subheader on the column (wip only applies in column view)
//wipLimit = int of the limit for the column in question
//itemCount = int of cards in the column (these are direct children of the div)
//returns = updates the UI
function planningBoardWipUpdateUI(div, subheader, wipLimit, itemCount)
{
    if (!div || !subheader)
    {
        return null;
    }
    
    var wipBadge = subheader.querySelector('.wip-badge');
    var divParent = div.parentNode
    //update UI if there are wip limits
    if (wipLimit && wipLimit > 0) {
        // handle classes when the limit is exceeded
        if (itemCount > wipLimit) {
            divParent.classList.add('pb-wip-exceeded');
            subheader.classList.add('pb-wip-exceeded');
            divParent.classList.remove('pb-wip-not-exceeded');
            subheader.classList.remove('pb-wip-not-exceeded');
        }
        // handle classes when the limit is not / no longer exceeded
        else
        {
            divParent.classList.remove('pb-wip-exceeded');
            subheader.classList.remove('pb-wip-exceeded');
            divParent.classList.add('pb-wip-not-exceeded');
            subheader.classList.add('pb-wip-not-exceeded');
        }

        // separately update the badge to show the correct information
        if (wipBadge) {
            wipBadge.innerHTML = "";
            wipBadge.appendChild(d.createTextNode(resx.PlanningBoard_WIP + ': ' + itemCount + ' / ' + wipLimit));
        }
    }
    // if no wip limits remove any wip classes on the column
    else {
        divParent.classList.remove('pb-wip-exceeded');
        subheader.classList.remove('pb-wip-exceeded');
        divParent.classList.remove('pb-wip-not-exceeded');
        subheader.classList.remove('pb-wip-not-exceeded');
    }
}
