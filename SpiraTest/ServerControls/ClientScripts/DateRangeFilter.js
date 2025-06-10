var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter = function(element)
{
    this._element = element;

    if (!element)
    {
        this._element = d.ce('div');
    }
      
    //Create the internal elements
    this._input = d.ce('input');
    this._innerDiv = d.ce('div');
    this._dateFormat = 'm/d/Y';
            
    Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter.initializeBase(this, [this._element]);
};

Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter.prototype =
{
    /* Init / Dispose */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter.callBaseMethod(this, 'initialize');

        /* Event delegates and callbacks */
        this._onClickHandler = Function.createDelegate(this, this._onClick);
        this._documentClickHandler = Function.createDelegate(this, this._onDocumentClick);

        //Attach the read-only input box and display div that lives inside the outer div
        this._input.type = 'hidden';
        this._element.appendChild(this._input);
        this._innerDiv.className = 'value';
        this._element.appendChild(this._innerDiv);
        this._element.appendChild(d.createTextNode(''));

        //Attach the icon next to _innerDiv
        this._icon = d.ce('div');
        this._icon.className = 'date-icon fas fa-calendar-alt';
        this._element.appendChild(this._icon);

        //Set any member variables
        this._rangeSelectorExpanded = false;
        this._controlClicked = false;
        this._rangeSelector = null;
        this._startDate = null;
        this._endDate = null;

        //Add the various event handlers
        $addHandler(this._element, 'click', this._onClickHandler);
        $addHandler(document, 'click', this._documentClickHandler);
    },
    dispose: function ()
    {
        //Clear the handlers
        $clearHandlers(this.get_element());
        $removeHandler(document, 'click', this._documentClickHandler);

        //Clear the delegates
        if (this._onClickHandler) delete this._onClickHandler;
        if (this._documentClickHandler) delete this._documentClickHandler;

        //Delete the popup range selector if created
        if (this._rangeSelector)
        {
            this.deleteRangeSelector();
        }

        Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter.callBaseMethod(this, 'dispose');
    },

    /* public properties */
    get_element: function ()
    {
        return this._element;
    },
    get_name: function ()
    {
        return this._input.name;
    },
    set_name: function (name)
    {
        if (this._input.name != name)
        {
            this._input.name = name;
            this.raisePropertyChanged('name');
        }
    },
    get_valueElement: function ()
    {
        return this._input;
    },
    get_value: function ()
    {
        return this._input.value;
    },
    set_value: function (value)
    {
        if (this._input.value != value)
        {
            //Set the actual value of the hidden field
            this._input.value = value;

            //Set the display version as well
            var displayText = '';
            if (value != null && value != '')
            {
                var dates = value.split('|');
                if (dates.length < 2)
                {
                    displayText = '> ' + dates[0];
                }
                else
                {
                    if (dates[0] == '' && dates[1] != '')
                    {
                        displayText = '< ' + dates[1];
                    }
                    if (dates[0] != '' && dates[1] == '')
                    {
                        displayText = '> ' + dates[0];
                    }
                    if (dates[0] != '' && dates[1] != '')
                    {
                        displayText = dates[0].substr(0, 5) + ' - ' + dates[1].substr(0, 5);
                    }
                }
            }
            if (this._innerDiv.childNodes.length > 0)
            {
                this._innerDiv.removeChild(this._innerDiv.firstChild);
            }
            this._innerDiv.appendChild(d.createTextNode(displayText));

            //Raise the event
            this.raisePropertyChanged('value');
        }
    },

    get_dateFormat: function ()
    {
        return this._dateFormat;
    },
    set_dateFormat: function (dateFormat)
    {
        if (this._dateFormat != dateFormat)
        {
            this._dateFormat = dateFormat;
            this.raisePropertyChanged('dateFormat');
        }
    },

    /* Event Handlers */
    _onClick: function ()
    {
        //Call the method to display the date-range selector (if not already displayed)
        if (!this._rangeSelectorExpanded)
        {
            this._controlClicked = true;
            this.displayRangeSelector();
        }
    },
    _onDocumentClick: function ()
    {
        //Call the method to hide the popup range immediately as long as nothing in the range-selector was clicked
        if (!this._controlClicked)
        {
            this.hideRangeSelector();
        }
        this._controlClicked = false;
    },
    _onRangeSelectorClick: function ()
    {
        this._controlClicked = true;
    },
    _onUpdateClick: function (evt)
    {
        //Update the hidden input field and the display div (remove any bar characters as it's the delimiter)
        this._input.value = this._startDate.get_value().replace(/|/, '') + '|' + this._endDate.get_value().replace(/|/, '');
        //Display the appropriate text depending on if one, both or neither date was set
        var displayText = '';
        if (this._startDate.get_value() == '' && this._endDate.get_value() != '')
        {
            displayText = '< ' + this._endDate.get_value();
        }
        if (this._startDate.get_value() != '' && this._endDate.get_value() == '')
        {
            displayText = '> ' + this._startDate.get_value();
        }
        if (this._startDate.get_value() != '' && this._endDate.get_value() != '')
        {
            displayText = this._startDate.get_value().substr(0, 5) + ' - ' + this._endDate.get_value().substr(0, 5);
        }
        if (this._innerDiv.childNodes.length > 0)
        {
            this._innerDiv.removeChild(this._innerDiv.firstChild);
        }
        this._innerDiv.appendChild(d.createTextNode(displayText));

        //Close the dialog
        this.hideRangeSelector();
        evt.preventDefault();
        evt.stopPropagation();

        //Raise the event
        this.raise_updated(this._input.value);
    },
    _onStandardRangeSelected: function (item)
    {
        //See which value was selected and apply the appropriate range
        if (item && item.get_value)
        {
            var value = item.get_value();
            switch (value)
            {
                case '1':
                    //Today
                    var now = new Date();
                    this._startDate.set_date(now);
                    this._endDate.set_date(now);
                    break;

                case '2':
                    //Yesterday
                    var now = new Date();
                    var startDate = new Date();
                    startDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - 1);
                    var endDate = new Date();
                    endDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - 1);
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '3':
                    //This Week
                    var now = new Date();
                    var dayOfWeek = now.getDay();
                    var startDate = new Date();
                    startDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - dayOfWeek);
                    var endDate = new Date();
                    endDate.setFullYear(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + 6);
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '4':
                    //Last Week
                    var now = new Date();
                    var dayOfWeek = now.getDay();
                    var startDate = new Date();
                    startDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - dayOfWeek - 7);
                    var endDate = new Date();
                    endDate.setFullYear(startDate.getFullYear(), startDate.getMonth(), startDate.getDate() + 6);
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '5':
                    //Last 7 Days
                    var now = new Date();
                    var startDate = new Date();
                    startDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - 7);
                    var endDate = new Date();
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '6':
                    //Last 14 Days
                    var now = new Date();
                    var startDate = new Date();
                    startDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - 14);
                    var endDate = new Date();
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '7':
                    //This Month
                    var now = new Date();
                    var year = now.getFullYear();
                    var month = now.getMonth();
                    var startDate = new Date();
                    startDate.setFullYear(year, month, 1);
                    var endDate = new Date(year, month, 31);
                    //Handle months with less than 31 days
                    if (endDate.getMonth() > month)
                    {
                        endDate = new Date(year, month, 30);
                        if (endDate.getMonth() > month)
                        {
                            endDate = new Date(year, month, 29);
                            if (endDate.getMonth() > month)
                            {
                                endDate = new Date(year, month, 28);
                            }
                        }
                    }
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '8':
                    //Last Month
                    var now = new Date();
                    var year = now.getFullYear();
                    var month = now.getMonth() - 1;
                    if (month < 0)
                    {
                        month = 11;
                        year--;
                    }
                    var startDate = new Date();
                    startDate.setFullYear(year, month, 1);
                    var endDate = new Date(year, month, 31);
                    //Handle months with less than 31 days
                    if (endDate.getMonth() > month)
                    {
                        endDate = new Date(year, month, 30);
                        if (endDate.getMonth() > month)
                        {
                            endDate = new Date(year, month, 29);
                            if (endDate.getMonth() > month)
                            {
                                endDate = new Date(year, month, 28);
                            }
                        }
                    }
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;

                case '9':
                    //Last 30 Days
                    var now = new Date();
                    var startDate = new Date();
                    startDate.setFullYear(now.getFullYear(), now.getMonth(), now.getDate() - 30);
                    var endDate = new Date();
                    this._startDate.set_date(startDate);
                    this._endDate.set_date(endDate);
                    break;
            }
        }
    },

    /* Public Methods */
    activate: function (evt)
    {
        //Called when an associated label is clicked
        this._controlClicked = true;
        this.displayRangeSelector();
    },
    displayRangeSelector: function ()
    {
        //See if we need to create the range selector or not
        if (!this._rangeSelector)
        {
            this.createRangeSelector();
        }
        //Display the popup div that contains the start-date and end-date
       // var yoffset = 0	// Any padding between the element raising the event and the calendar itself
       // var xoffset = 1	// Any static shift between the value item and the calendar list
        this._rangeSelector.style.display = 'block';
       // this._rangeSelector.x = globalFunctions.getposOffset(this._element, 'left') + xoffset;
       // this._rangeSelector.y = globalFunctions.getposOffset(this._element, 'top') + yoffset;
       // this._rangeSelector.style.left = this._rangeSelector.x - globalFunctions.clearbrowseredge(this._rangeSelector, 'rightedge') + 'px';
       // this._rangeSelector.style.top = this._rangeSelector.y - globalFunctions.clearbrowseredge(this._rangeSelector, 'bottomedge') + this._element.offsetHeight + 'px';
        this._rangeSelectorExpanded = true;

        //Reset the standard range dropdown
        this._ddlStandardRanges.set_selectedItem('');

        //Now set the initial values of the start and end date boxes
        var value = this.get_value();
        if (value == null || value == '')
        {
            this._startDate.set_value('');
            this._endDate.set_value('');
        }
        else
        {
            var dates = value.split('|');
            if (dates.length < 2)
            {
                this._startDate.set_value(dates[0]);
            }
            else
            {
                if (dates[0] == '')
                {
                    this._startDate.set_value('');
                }
                else
                {
                    this._startDate.set_value(dates[0]);
                }
                if (dates[1] == '')
                {
                    this._endDate.set_value('');
                }
                else
                {
                    this._endDate.set_value(dates[1]);
                }
            }
        }
    },
    hideRangeSelector: function ()
    {
        this._rangeSelectorExpanded = false;
        if (this._rangeSelector)
        {
            this._rangeSelector.style.display = 'none';
        }
    },
    createRangeSelector: function ()
    {

        /* Get position of element on the page */
        var DateRangeFilterRequiredClearance = 350;
        var elementNearBottom = $(document).height() - $(this._element).offset().top < DateRangeFilterRequiredClearance;

        //Create the floating range selector itself
        this._rangeSelector = d.ce('div');
        this._element.appendChild(this._rangeSelector);
        this._rangeSelector.className = elementNearBottom ? 'DateRangeFilter dropup-popup' : 'DateRangeFilter dropdown-popup';
        this._rangeSelector.style.display = 'none';

        //Create the title        
        var h1 = d.ce('h1');
        h1.appendChild(d.createTextNode(resx.DateRangeFilter_EnterDateRange));
        this._rangeSelector.appendChild(h1);

        //Create the list of standard date ranges
        var p = d.ce('p');
        p.appendChild(d.createTextNode(resx.DateRangeFilter_StandardDates));
        this._rangeSelector.appendChild(p);

        var div = d.ce('div');
        this._ddlStandardRanges = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { multiSelectable: false}, null, null, div);
        this._ddlStandardRanges.addItem('', resx.Global_PleaseSelect);
        this._ddlStandardRanges.addItem('1', resx.DateRangeFilter_Today);
        this._ddlStandardRanges.addItem('2', resx.DateRangeFilter_Yesterday);
        this._ddlStandardRanges.addItem('3', resx.DateRangeFilter_ThisWeek);
        this._ddlStandardRanges.addItem('4', resx.DateRangeFilter_LastWeek);
        this._ddlStandardRanges.addItem('5', resx.DateRangeFilter_Last7Days);
        this._ddlStandardRanges.addItem('6', resx.DateRangeFilter_Last14Days);
        this._ddlStandardRanges.addItem('7', resx.DateRangeFilter_ThisMonth);
        this._ddlStandardRanges.addItem('8', resx.DateRangeFilter_LastMonth);
        this._ddlStandardRanges.addItem('9', resx.DateRangeFilter_Last30Days);
        this._ddlStandardRanges.set_selectedItem('');
        this._rangeSelector.appendChild(div);

        //Add the event handler
        this._standardRangeSelectedHandler = Function.createDelegate(this, this._onStandardRangeSelected);
        this._ddlStandardRanges.add_selectedItemChanged(this._standardRangeSelectedHandler);

        //Add the legend for custom date ranges
        this._rangeSelector.appendChild(d.ce('br'));
        p = d.ce('p');
        p.appendChild(d.createTextNode(resx.DateRangeFilter_CustomDates));
        this._rangeSelector.appendChild(p);

        //Now add in the two date controls (start/end date)
        div = d.ce('div');
        div.className = 'DatePicker';
        this._rangeSelector.appendChild(div);
        this._startDate = $create(Inflectra.SpiraTest.Web.ServerControls.DatePicker, null, null, null, div);
        this._startDate.set_dateFormat(this._dateFormat);
        div = d.ce('div');
        div.appendChild(d.createTextNode('\u00a0-\u00a0'));
        this._rangeSelector.appendChild(div);
        div = d.ce('div');
        div.className = 'DatePicker';
        this._rangeSelector.appendChild(div);
        this._endDate = $create(Inflectra.SpiraTest.Web.ServerControls.DatePicker, null, null, null, div);
        this._endDate.set_dateFormat(this._dateFormat);

        //Add the link for updating the date filter value
        div = d.ce('div');
        div.className = 'Action';
        this._rangeSelector.appendChild(div);
        this._updateLink = d.ce('a');
        this._updateLink.className = "btn btn-primary";
        div.appendChild(this._updateLink);
        this._updateLink.appendChild(d.createTextNode(resx.Global_Update));
        this._updateLink.href = 'javascript:void(0)';

        //Add handlers
        this._onRangeSelectorClickHandler = Function.createDelegate(this, this._onRangeSelectorClick);
        this._onUpdateClickHandler = Function.createDelegate(this, this._onUpdateClick);
        $addHandler(this._rangeSelector, 'click', this._onRangeSelectorClickHandler);
        $addHandler(this._updateLink, 'click', this._onUpdateClickHandler);
    },
    deleteRangeSelector: function ()
    {
        //Clear the click handler
        $removeHandler(this._rangeSelector, 'click', this._onRangeSelectorClickHandler);
        $removeHandler(this._updateLink, 'click', this._onUpdateClickHandler);
        this._ddlStandardRanges.remove_selectedItemChanged(this._standardRangeSelectedHandler);

        if (this._onRangeSelectorClickHandler) delete this._onRangeSelectorClickHandler;
        if (this._onUpdateClickHandler) delete this._onUpdateClickHandler;
        if (this._standardRangeSelectedHandler) delete this._standardRangeSelectedHandler;

        delete this._startDate;
        delete this._endDate;
        delete this._rangeSelector;
        delete this._ddlStandardRanges;
    },

    /* Event handlers managers */
    add_updated: function (handler)
    {
        this.get_events().addHandler('updated', handler);
    },
    remove_updated: function (handler)
    {
        this.get_events().removeHandler('updated', handler);
    },
    raise_updated: function (value)
    {
        var h = this.get_events().getHandler('updated');
        if (h) h(value);
    }
}

Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter.registerClass('Inflectra.SpiraTest.Web.ServerControls.DateRangeFilter', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
