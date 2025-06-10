var d = document;
d.ce = d.createElement;
d.ct = d.createTextNode;
Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DateTimePicker = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.DateTimePicker.initializeBase(this, [element]);

    //Workflow states
    this._enabled = true;
    this._required = false;
    this._hidden = false;
    this._options = null;
    this._innerDiv = null;
    this._locale = 'en-us';
    this._utcOffset = 0;
};
Inflectra.SpiraTest.Web.ServerControls.DateTimePicker.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DateTimePicker.callBaseMethod(this, 'initialize');

        //Set the options
        this._options = {
            icons: {
                time: 'far fa-clock',
                date: 'fas fa-calendar-alt',
                up: 'fas fa-chevron-up',
                down: 'fas fa-chevron-down',
                previous: 'fas fa-chevron-left',
                next: 'fas fa-chevron-right',
                today: 'fas fa-location-arrow',
                clear: 'fas fa-trash-alt',
                close: 'fas fa-times'
            },
            showTodayButton: true,
            showClose: true,
            useCurrent: true,
            locale: this._locale,
            utcOffset: this._utcOffset,
            debug: false /* keeps it open for checking */
        };

        this.createChildControls();
    },
    dispose: function ()
    {
        delete this._options;
        delete this._innerDiv;
        Inflectra.SpiraTest.Web.ServerControls.DateTimePicker.callBaseMethod(this, 'dispose');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },

    get_enabled: function ()
    {
        return this._enabled;
    },
    set_enabled: function (value)
    {
        this._enabled = value;
        this.update_state('enabled');
    },

    get_required: function ()
    {
        return this._required;
    },
    set_required: function (value)
    {
        this._required = value;
        this.update_state('required');
    },

    get_hidden: function ()
    {
        return this._hidden;
    },
    set_hidden: function (value)
    {
        this._hidden = value;
        this.update_state('hidden');
    },

    get_locale: function ()
    {
        return this._locale;
    },
    set_locale: function (value)
    {
        this._locale = value;
    },

    get_utcOffset: function ()
    {
        return this._utcOffset;
    },
    set_utcOffset: function (value)
    {
        this._utcOffset = value;
    },

    /* Event handler managers */
    add_dateOrTimeChanged: function (handler)
    {
        this.get_events().addHandler('dateOrTimeChanged', handler);
    },
    remove_dateOrTimeChanged: function (handler)
    {
        this.get_events().removeHandler('dateOrTimeChanged', handler);
    },
    raise_dateOrTimeChanged: function ()
    {
        var h = this.get_events().getHandler('dateOrTimeChanged');
        if (h) h();
    },
    
    /* Methods */
    createChildControls: function()
    {
        this._innerDiv = d.ce('div');
        this._innerDiv.className = 'datepickergroup relative clearfix'; //primary class used for identification by js. Extra classes to ensure correct display of popup.
        this._element.appendChild(this._innerDiv);
        var textBox = d.ce('input');
        textBox.type = 'text';
        textBox.className = 'value';
        this._innerDiv.appendChild(textBox);
        var span = d.ce('span');
        span.className = 'datepickerbutton date-icon fas fa-calendar-alt';
        this._innerDiv.appendChild(span);

        //Init the controls when ready
        $(document).ready(Function.createDelegate(this, this.initControls));
    },
    initControls: function()
    {
        //Now we need to use jQuery to actually turn into a date/time picker
        $(this._innerDiv).datetimepicker(this._options).on('dp.change', Function.createDelegate(this, this._onDateChange));
    },

    _onDateChange: function(e)
    {
        this.raise_dateOrTimeChanged();
    },

    //Selects the current date/time
    selectCurrent: function()
    {
        var currentMoment = moment();
        $(this._innerDiv).data("DateTimePicker").date(currentMoment);
    },
    //Clears the date/time picker
    clearDatetime: function()
    {
        $(this._innerDiv).data("DateTimePicker").date(null);
    },

    //Called when the associated label is clicked on
    activate: function()
    {
        $(this._innerDiv).data("DateTimePicker").show();
    },

    set_datetime: function (dateObj)
    {
        //Note: the dateObj is the localtime from the server (using the user's profile) encoded
        //as UTC so that the Javascript Date object doesn't try and do any local time conversion based on the client
        //See if we have a null object or empty string
        if (!dateObj || dateObj == '')
        {
            $(this._innerDiv).data("DateTimePicker").date(null);
        }
        else
        {
            $(this._innerDiv).data("DateTimePicker").date(dateObj);
        }
    },
    get_datetime: function()
    {
        //Returns a MomentJS object
        var momentObj = $(this._innerDiv).data("DateTimePicker").date();
        if (momentObj)
        {
            //The True we pass keeps the selected day/time but changes the timezone
            return momentObj.utcOffset(this._utcOffset, true).clone();
        }
        else
        {
            return null;
        }
    },

    update_state: function (state)
    {
        var element = this.get_element();
        if (!state || state == 'hidden')
        {
            if (this.get_hidden())
            {
                $(element).addClass('dn');
            }
            else
            {
                $(element).removeClass('dn');
            }
        }
        if (!state || state == 'enabled')
        {
            if (this.get_enabled())
            {
                $(this._innerDiv).data("DateTimePicker").enable();
            }
            else
            {
                $(this._innerDiv).data("DateTimePicker").disable();
            }
        }
        if (!state || state == 'required')
        {
            if (this.get_required())
            {
                $(element).addClass('required');
            }
            else
            {
                $(element).removeClass('required');
            }
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.DateTimePicker.registerClass('Inflectra.SpiraTest.Web.ServerControls.DateTimePicker', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
