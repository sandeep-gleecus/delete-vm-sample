var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter = function (element)
{
    this._element = element;

    if (!element)
    {
        this._element = d.ce('div');
        this._element.style.width = '90px';    //Default width
    }

    //Create the internal elements
    this._input = d.ce('input');
    this._innerDiv = d.ce('div');

    Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter.initializeBase(this, [this._element]);
};

Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter.prototype =
{
    /* Init / Dispose */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter.callBaseMethod(this, 'initialize');

        /* Event delegates and callbacks */
        this._onClickHandler = Function.createDelegate(this, this._onClick);
        this._documentClickHandler = Function.createDelegate(this, this._onDocumentClick);

        //Attach the read-only input box and display div that lives inside the outer div
        this._input.type = 'hidden';
        this._element.appendChild(this._input);
        this._innerDiv.className = 'value';
        this._innerDiv.style.border = 'none';
        this._element.appendChild(this._innerDiv);
        this._element.appendChild(d.createTextNode(''));

        //Attach the icon next to _innerDiv
        this._icon = d.ce('div');
        this._icon.className = 'range-icon fas fa-calculator';
        this._element.appendChild(this._icon);

        //Set any member variables
        this._rangeSelectorExpanded = false;
        this._controlClicked = false;
        this._rangeSelector = null;
        this._minValue = null;
        this._maxValue = null;

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

        Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter.callBaseMethod(this, 'dispose');
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
        this._input.value = this._minValue.value.replace(/|/, '') + '|' + this._maxValue.value.replace(/|/, '');
        //Display the appropriate text depending on if one, both or neither date was set
        var displayText = '';
        if (this._minValue.value == '' && this._maxValue.value != '')
        {
            displayText = '< ' + this._maxValue.value;
        }
        if (this._minValue.value != '' && this._maxValue.value == '')
        {
            displayText = '> ' + this._minValue.value;
        }
        if (this._minValue.value != '' && this._maxValue.value != '')
        {
            displayText = this._minValue.value.substr(0, 5) + ' - ' + this._maxValue.value.substr(0, 5);
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
        this._rangeSelector.style.display = 'block';
        this._rangeSelectorExpanded = true;

        //Now set the initial values of the start and end date boxes
        var value = this.get_value();
        if (value == null || value == '')
        {
            this._minValue.value = '';
            this._maxValue.value = '';
        }
        else
        {
            var values = value.split('|');
            if (values.length < 2)
            {
                this._minValue.value = values[0];
            }
            else
            {
                if (values[0] == '')
                {
                    this._minValue.value = '';
                }
                else
                {
                    this._minValue.value = values[0];
                }
                if (values[1] == '')
                {
                    this._maxValue.value = '';
                }
                else
                {
                    this._maxValue.value = values[1];
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
        //Create the floating range selector itself
        this._rangeSelector = d.ce('div');
        this._element.appendChild(this._rangeSelector);
        this._rangeSelector.className = 'NumberRangeFilterSelector dropdown-popup';
        this._rangeSelector.style.display = 'none';

        //Create the title        
        var h1 = d.ce('h1');
        h1.appendChild(d.createTextNode(resx.NumberRangeFilter_Title));
        this._rangeSelector.appendChild(h1);

        //Add the legend for custom date ranges
        p = d.ce('p');
        p.appendChild(d.createTextNode(resx.NumberRangeFilter_EnterMinMaxValues));
        this._rangeSelector.appendChild(p);

        //Now add in the two text controls (min/max value)
        //MinValue
        var textbox = d.ce('input');
        textbox.type = 'text';
        textbox.maxLength = 10;
        textbox.className = 'text-box';
        this._rangeSelector.appendChild(textbox);
        this._minValue = textbox;
        //Divider
        this._rangeSelector.appendChild(d.createTextNode('\u00a0-\u00a0'));
        //MaxValue
        textbox = d.ce('input');
        textbox.type = 'text';
        textbox.maxLength = 10;
        textbox.className = 'text-box';
        this._rangeSelector.appendChild(textbox);
        this._maxValue = textbox;

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

        if (this._onRangeSelectorClickHandler) delete this._onRangeSelectorClickHandler;
        if (this._onUpdateClickHandler) delete this._onUpdateClickHandler;
        if (this._standardRangeSelectedHandler) delete this._standardRangeSelectedHandler;

        delete this._minValue;
        delete this._maxValue;
        delete this._rangeSelector;
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

Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter.registerClass('Inflectra.SpiraTest.Web.ServerControls.NumberRangeFilter', Sys.UI.Control);

if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
