var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DatePicker = function (element)
{
    this._element = element;
    if (!element)
    {
        this._element = d.ce('div');
    }
    this._input = d.ce('input');
    this._dateFormat = 'm/d/Y';
    this._enabled = true;
    this._enabledCssClass = 'u-datepicker';
    this._disabledCssClass = 'u-datepicker Disabled';

    Inflectra.SpiraTest.Web.ServerControls.DatePicker.initializeBase(this, [this._element]);
};

Inflectra.SpiraTest.Web.ServerControls.DatePicker.prototype =
{
    /* Init / Dispose */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.DatePicker.callBaseMethod(this, 'initialize');

        /* Get position of element on the page */
        var DatePickerRequiredClearance = 350;
        var elementNearBottom = $(document).height() - $(this._element).offset().top < DatePickerRequiredClearance;


        /* Event delegates and callbacks */
        this._onClickHandler = Function.createDelegate(this, this._onClick);
        this._onMouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        this._onMouseOutHandler = Function.createDelegate(this, this._onMouseOut);
        this._onNextMonthClickHandler = Function.createDelegate(this, this._onNextMonthClick);
        this._onPrevMonthClickHandler = Function.createDelegate(this, this._onPrevMonthClick);
        this._onInputKeydownHandler = Function.createDelegate(this, this._onInputKeydown);
        this._callbacks = new Array();
        this._calendarExpanded = false;

        //Create a div for the always visible form control
        this._formDiv = d.ce('div');
        this._element.appendChild(this._formDiv);

        //Attach the input box that lives inside the outer div
        this._input.type = 'text';
        this._input.className = 'value';
        this._input.length = 20;    //Max length
        this._formDiv.appendChild(this._input);


        //Then attach the calendar icon next to the input box
        this._icon = d.ce('div');
        this._icon.className = 'date-icon fas fa-calendar-alt';
        this._formDiv.appendChild(this._icon);

        //Create the floating calendar itself
        this._calendar = d.ce('div');
        this._element.appendChild(this._calendar);
        this._calendar.className = elementNearBottom ? 'ppcCalendar dropup-popup' : 'ppcCalendar dropdown-popup';

        //Now create the month selector
        this._monthSelector = d.ce('div');
        this._monthSelector.className = 'ppcMonthSelector';
        this._calendar.appendChild(this._monthSelector);

        //Now create the first table
        this._table1 = d.ce('table');
        this._table1.className = 'pplTable1';
        this._monthSelector.appendChild(this._table1);
        var tbody = d.ce('tbody');
        this._table1.appendChild(tbody);
        var tr = d.ce('tr');
        tbody.appendChild(tr);
        var td = d.ce('td');
        tr.appendChild(td);
        this._prevMonthLink = d.ce('a');
        td.appendChild(this._prevMonthLink);
        this._prevMonthLink.href = 'javascript:void(0)';
        var div = d.ce('div');
        div.className = 'fas fa-caret-left fa-fw';
        this._prevMonthLink.appendChild(div);
        td = d.ce('td');
        tr.appendChild(td);
        this._monthYearText = d.ce('div');
        this._monthYearText.className = 'ppcMonthLegend';
        td.appendChild(this._monthYearText);
        this._monthYearValue = d.ce('input');
        this._monthYearValue.type = 'hidden';
        td.appendChild(this._monthYearValue);
        td = d.ce('td');
        tr.appendChild(td);
        this._nextMonthLink = d.ce('a');
        td.appendChild(this._nextMonthLink);
        this._nextMonthLink.href = 'javascript:void(0)';
        div = d.ce('div');
        div.className = 'fas fa-caret-right fa-fw';
        this._nextMonthLink.appendChild(div);

        //Now create the second table
        this._table2 = d.ce('table');
        this._table2.className = 'pplTable2';
        this._monthSelector.appendChild(this._table2);
        tbody = d.ce('tbody');
        this._table2.appendChild(tbody);
        tr = d.ce('tr');
        tbody.appendChild(tr);
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekend';
        td.appendChild(d.createTextNode(resx.DatePicker_Su));
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekday';
        td.appendChild(d.createTextNode(resx.DatePicker_Mo));
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekday';
        td.appendChild(d.createTextNode(resx.DatePicker_Tu));
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekday';
        td.appendChild(d.createTextNode(resx.DatePicker_We));
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekday';
        td.appendChild(d.createTextNode(resx.DatePicker_Th));
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekday';
        td.appendChild(d.createTextNode(resx.DatePicker_Fr));
        td = d.ce('td');
        tr.appendChild(td);
        td.className = 'ppcWeekend';
        td.appendChild(d.createTextNode(resx.DatePicker_Sa));

        //Now create the month days div
        this._monthDays = d.ce('div');
        this._monthDays.className = 'ppcMonthDays';
        this._monthSelector.appendChild(this._monthDays);

        //Add the various event handlers
        $addHandler(this._element, 'click', this._onClickHandler);
        $addHandler(this._calendar, 'mouseover', this._onMouseOverHandler);
        $addHandler(this._calendar, 'mouseout', this._onMouseOutHandler);
        $addHandler(this._nextMonthLink, 'click', this._onNextMonthClickHandler);
        $addHandler(this._prevMonthLink, 'click', this._onPrevMonthClickHandler);
        $addHandler(this._input, 'keydown', this._onInputKeydownHandler);
        //Add a handler when somewhere else on the page is clicked to close the popup calendar
        var documentClickHandler = Function.createDelegate(this, this._onDocumentClick);
        $addHandlers(document, { 'click': documentClickHandler }, this);

        //Specify any initial values of properties
        this._ppcMN = new Array(resx.DatePicker_January, resx.DatePicker_February, resx.DatePicker_March, resx.DatePicker_April, resx.DatePicker_May, resx.DatePicker_June, resx.DatePicker_July, resx.DatePicker_August, resx.DatePicker_September, resx.DatePicker_October, resx.DatePicker_November, resx.DatePicker_December);
        this._ppcWN = new Array(resx.DatePicker_Sunday, resx.DatePicker_Monday, resx.DatePicker_Tuesday, resx.DatePicker_Wednesday, resx.DatePicker_Thursday, resx.DatePicker_Friday, resx.DatePicker_Saturday);
        this._ppcER = new Array(4);
        this._ppcER[0] = resx.DatePicker_DHTMLUnsupported;
        this._ppcER[1] = resx.DatePicker_TargetFormInvalid;
        this._ppcER[2] = resx.DatePicker_ChosenDateInvalid;
        this._ppcER[3] = resx.DatePicker_UnknownError;
        this._ppcUC = false;
        this._ppcUX = 4;
        this._ppcUY = 4;

        //Some additional constants
        this._ppcFC = true;
        this._ppcTI = false;
        this._ppcRL = null;
        this._ppcXC = null;
        this._ppcYC = null;
        this._ppcML = new Array(31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);
        this._ppcWE = new Array(resx.DatePicker_Sunday, resx.DatePicker_Monday, resx.DatePicker_Tuesday, resx.DatePicker_Wednesday, resx.DatePicker_Thursday, resx.DatePicker_Friday, resx.DatePicker_Saturday);
        this._ppcNow = new Date();
        this._ppcPtr = new Date();
        this._ppcInitMonth;
        this._ppcInitYear;
        this._ppcInitDate;
        this._controlClicked = false;
    },

    dispose: function ()
    {
        //Clear the handlers
        $clearHandlers(this.get_element());
        $clearHandlers(this._calendar);
        $clearHandlers(this._prevMonthLink);
        $clearHandlers(this._nextMonthLink);
        $clearHandlers(this._input);

        //Clear the delegates
        if (this._onClickHandler) delete this._onClickHandler;
        if (this._onMouseOverHandler) delete this._onMouseOverHandler;
        if (this._onMouseOutHandler) delete this._onMouseOutHandler;
        if (this._onNextMonthClickHandler) delete this._onNextMonthClickHandler;
        if (this._onPrevMonthClickHandler) delete this._onPrevMonthClickHandler;
        if (this._onInputKeydownHandler) delete this._onInputKeydownHandler;

        Inflectra.SpiraTest.Web.ServerControls.DatePicker.callBaseMethod(this, 'dispose');
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
            this._input.value = value;
            this.raisePropertyChanged('value');
        }
    },
    set_date: function (date)
    {
        if (date)
        {
            var year = this.getFullYear(date);
            var month = date.getMonth();
            var day = date.getDate();
            this._input.value = this.dateFormat(year, month, day);
        }
        else
        {
            this._input.value = '';
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

    get_enabledCssClass: function ()
    {
        return this._enabledCssClass;
    },
    set_enabledCssClass: function (value)
    {
        this._enabledCssClass = value;
    },
    get_disabledCssClass: function ()
    {
        return this._disabledCssClass;
    },
    set_disabledCssClass: function (value)
    {
        this._disabledCssClass = value;
    },
    get_enabled: function ()
    {
        return this._enabled;
    },
    set_enabled: function (value)
    {
        this._enabled = value;
        if (this._enabled)
        {
            this._input.disabled = '';
            this.get_element().className = this._enabledCssClass;
        }
        else
        {
            this._input.disabled = 'disabled';
            this.get_element().className = this._disabledCssClass;
        }
    },

    /* Event Handlers */
    _onClick: function (e)
    {
        //See what was clicked
        if (e.target)
        {
            targ = e.target;
        }
        else if (e.srcElement)
        {
            targ = e.srcElement;
        }
        if (targ.nodeType == 3) // defeat Safari bug
        {
            targ = targ.parentNode;
        }
        var tname;
        tname = targ.tagName;

        //Call the method to display the calendar (if not already displayed and not on the input)
        if (!this._calendarExpanded && tname != 'INPUT' && this._enabled)
        {
            this._controlClicked = true;
            this.displayCalendar();
        }
    },
    _onMouseOver: function ()
    {
        //Resets the timeout count
        if (this._ppcTI)
        {
            clearTimeout(this._ppcTI);
            this._ppcTI = false;
        }
    },
    _onMouseOut: function ()
    {
        //Call the method to hide the calendar
        var delegate = Function.createDelegate(this, this.hideCalendar);
        this._ppcTI = setTimeout(delegate, 500);
    },
    _onDocumentClick: function ()
    {
        //Call the method to hide the calendar immediately as long as nothing on the calendar was clicked
        if (!this._controlClicked)
        {
            this.hideCalendar();
        }
        this._controlClicked = false;
    },
    _onDayClick: function (sender, e)
    {
        //Select the day - get the param value
        this._controlClicked = true;
        var param = e.param;
        e.thisRef.selectDate(param);
        e.thisRef.raise_dateChanged();
    },
    _onDayMouseOver: function (sender, e)
    {
        //Change the cell background
        $(sender.target).addClass('ppcDayHover');

    },
    _onDayMouseOut: function (sender, e)
    {
        //Change the cell background back
        sender.target.className = e.cls;
    },
    _onNextMonthClick: function (e)
    {
        //Call the method to display the calendar
        this._controlClicked = true;
        this.moveMonth('Forward');
    },
    _onPrevMonthClick: function (e)
    {
        //Call the method to display the calendar
        this._controlClicked = true;
        this.moveMonth('Back');
    },
    _onInputKeydown: function (e)
    {
        //Raise the change event
        this.raise_dateChanged();
    },

    /* Public Functions */
    activate: function (evt)
    {
        //Called when an associated label is clicked
        this._input.select();
    },

    /* Event handler managers */
    add_dateChanged: function (handler)
    {
        this.get_events().addHandler('dateChanged', handler);
    },
    remove_dateChanged: function (handler)
    {
        this.get_events().removeHandler('dateChanged', handler);
    },
    raise_dateChanged: function ()
    {
        var h = this.get_events().getHandler('dateChanged');
        if (h) h();
    },

    /* Internal Functions */
    displayCalendar: function (rules)
    {
        this._ppcRL = rules;
        this._calendarExpanded = true;

        //Get the initial date from the target
        var dateString = this.get_value();
        if (dateString == null || dateString == "")
        {
            this._ppcInitYear = this.getFullYear(this._ppcNow);
            this._ppcInitMonth = this._ppcNow.getMonth();
            this._ppcInitDate = this._ppcNow.getDate();
        }
        else
        {
            this._ppcInitMonth = this.parseMonth(dateString) - 1;
            this._ppcInitYear = this.parseYear(dateString);
            this._ppcInitDate = this.parseDay(dateString);

            //Verify that we have a valid date
            if (this._ppcInitMonth && this._ppcInitYear && this._ppcInitDate)
            {
                if (this._ppcInitMonth < 0 || this._ppcInitMonth > 11 || this._ppcInitDate < 1 || this._ppcInitDate > 31)
                {
                    this._ppcInitYear = this.getFullYear(this._ppcNow);
                    this._ppcInitMonth = this._ppcNow.getMonth();
                    this._ppcInitDate = this._ppcNow.getDate();
                }
            }
            else
            {
                this._ppcInitYear = this.getFullYear(this._ppcNow);
                this._ppcInitMonth = this._ppcNow.getMonth();
                this._ppcInitDate = this._ppcNow.getDate();
            }
        }
        this.setCalendar();

        if ((this._input != null) && (this._input))
        {
            //Display the popup calendar
            //var yoffset = 0	// Any padding between the element raising the event and the calendar itself
            //var xoffset = 1	// Any static shift between the value item and the calendar list
            this._calendar.style.display = 'block';
            //this._calendar.x = globalFunctions.getposOffset(this._element, 'left') + xoffset;
            //this._calendar.y = globalFunctions.getposOffset(this._element, 'top') + yoffset;
            //this._calendar.style.left = this._calendar.x - globalFunctions.clearbrowseredge(this._element, 'rightedge') + 'px';
            //this._calendar.style.top = this._calendar.y - globalFunctions.clearbrowseredge(this._element, 'bottomedge') + this._element.offsetHeight + 'px';
        }
        else
        {
            this.showError(this._ppcER[1]);
        }
    },
    moveMonth: function (dir)
    {
        var tmp, dptrYear, dptrMonth;
        var objText = this._monthYearText;
        var objValue = this._monthYearValue;

        //Get the current month and year
        tmp = objValue.value.split("|");
        dptrYear = tmp[0];
        dptrMonth = tmp[1];

        if (objValue != null && objText != null)
        {
            if (dir.toLowerCase() == "back")
            {
                dptrMonth--;
                if (dptrMonth < 0)
                {
                    dptrYear--;
                    dptrMonth = 11;
                }
            }
            if (dir.toLowerCase() == "forward")
            {
                dptrMonth++;
                if (dptrMonth > 11)
                {
                    dptrYear++;
                    dptrMonth = 0;
                }
            }

            //Update the calendar display and hidden field
            objValue.value = dptrYear + "|" + dptrMonth;
            objText.innerHTML = dptrYear + " - " + this._ppcMN[dptrMonth];

            //Now update the calendar grid
            this.setCalendar(dptrYear, dptrMonth);
        }
    },
    selectDate: function (param)
    {
        var arr = param.split("|");
        var year = arr[0];
        var month = arr[1];
        var date = arr[2];
        var ptr = parseInt(date);
        this._ppcPtr.setDate(ptr);
        if ((this._input != null) && (this._input))
        {
            if (this.validDate(date))
            {
                this._input.value = this.dateFormat(year, month, date);
                this.hideCalendar();
            }
            else
            {
                this.showError(this._ppcER[2]);
                if (this._ppcTI)
                {
                    this.clearTimeout(this._ppcTI);
                    this._ppcTI = false;
                }
            }
        }
        else
        {
            this.showError(this._ppcER[1]);
            this.hideCalendar();
        }
    },
    setCalendar: function (year, month)
    {
        if (year == null)
        {
            year = this._ppcInitYear;
        }
        if (month == null)
        {
            month = this._ppcInitMonth;
        }
        if (month == 1)
        {
            this._ppcML[1] = (this.isLeap(year)) ? 29 : 28;
        }
        if (year != null && month != null)
        {
            this.setSelectList(year, month);
            this._ppcPtr.setYear(year);
            this._ppcPtr.setMonth(month);
            this._ppcPtr.setDate(1);
            this.updateContent();
        }
    },
    updateContent: function ()
    {
        //Clear the existing content and handlers
        for (var i = 0; i < this._callbacks.length; i++)
        {
            delete this._callbacks[i];
        }

        var element = this._monthDays;
        if (element.firstChild)
        {
            while (element.firstChild)
            {
                $clearHandlers(element.firstChild);
                element.removeChild(element.firstChild);
            }
        }
        //Now add the new content
        this.generateContent();
    },
    generateContent: function ()
    {
        var year = this.getFullYear(this._ppcPtr);
        var month = this._ppcPtr.getMonth();
        var date = 1;
        var day = this._ppcPtr.getDay();
        var len = this._ppcML[month];
        var cls, cnt = '';
        var j, i = 0;

        //First create the table
        var table = d.ce('table');
        table.className = 'ppcDays';
        this._monthDays.appendChild(table);
        var tbody = d.ce('tbody');
        table.appendChild(tbody);

        //Now create the day rows and cells
        for (j = 0; j < 7; ++j)
        {
            if (date > len)
            {
                break;
            }
            //Create the new row
            var tr = d.ce('tr');
            tbody.appendChild(tr);

            //Now add each day cell
            for (i = 0; i < 7; ++i)
            {
                cls = ((i == 0) || (i == 6)) ? "ppcWeekend" : "ppcWeekday";
                if (((j == 0) && (i < day)) || (date > len))
                {
                    this.makeCell(tr, cls, year, month, 0);
                }
                else
                {
                    this.makeCell(tr, cls, year, month, date);
                    ++date;
                }
            }
        }
    },
    makeCell: function (tr, cls, year, month, date)
    {
        var param = year + "|" + month + "|" + date;
        //Set today's date
        if ((this._ppcNow.getDate() == date) && (this._ppcNow.getMonth() == month) && (this._ppcNow.getFullYear() == year))
        {
            cls = "ppcToday";
        }
        //Set the initial date
        if ((this._ppcInitDate == date) && (this._ppcInitMonth == month) && (this._ppcInitYear == year))
        {
            cls = "ppcDaySelected";
        }
        if ( ((this._ppcNow.getDate() == date) && (this._ppcNow.getMonth() == month) && (this._ppcNow.getFullYear() == year)) && ((this._ppcInitDate == date) && (this._ppcInitMonth == month) && (this._ppcInitYear == year)) )
        {
            cls = "ppcDaySelected ppcToday";
        }

        //Now create the cell
        var td = d.ce('td');
        tr.appendChild(td);
        td.className = cls;

        //Build the cell contents - including the parameter
        var cellValue = (date != 0) ? date + '' : '\u00a0';
        td.appendChild(d.createTextNode(cellValue));
        if (date == 0)
        {
            td.style.cursor = 'default';
        }
        else
        {
            td.style.cursor = 'pointer';

            //Create a callback to handle the click event
            var onDayClickHandler = Function.createCallback(this._onDayClick, { thisRef: this, param: param });
            var onDayMouseOverHandler = Function.createCallback(this._onDayMouseOver, { thisRef: this, cls: cls });
            var onDayMouseOutHandler = Function.createCallback(this._onDayMouseOut, { thisRef: this, cls: cls });

            this._callbacks.push(onDayClickHandler);
            this._callbacks.push(onDayMouseOverHandler);
            this._callbacks.push(onDayMouseOutHandler);

            //Add the event handlers
            $addHandler(td, 'click', onDayClickHandler);
            $addHandler(td, 'mouseover', onDayMouseOverHandler);
            $addHandler(td, 'mouseout', onDayMouseOutHandler);
        }
    },
    setSelectList: function (year, month)
    {
        var i = 0;
        var objText = this._monthYearText;
        var objValue = this._monthYearValue;
        objValue.value = year + "|" + month;
        objText.innerHTML = year + " - " + this._ppcMN[month];
    },
    hideCalendar: function ()
    {
        this._calendarExpanded = false;
        this._calendar.style.display = 'none';
        this._ppcTI = false;
        this.setCalendar();
    },
    showError: function (message)
    {
        alert(message);
    },
    isLeap: function (year)
    {
        if ((year % 400 == 0) || ((year % 4 == 0) && (year % 100 != 0))) { return true; }
        else { return false; }
    },
    getFullYear: function (obj)
    {
        return obj.getFullYear();
    },
    validDate: function (date)
    {
        var reply = true;
        if (this._ppcRL == null)
        { /* NOP */ }
        else
        {
            var arr = this._ppcRL.split(":");
            var mode = arr[0];
            var arg = arr[1];
            var key = arr[2].charAt(0).toLowerCase();
            if (key != "d")
            {
                var day = this._ppcPtr.getDay();
                var orn = this._isEvenOrOdd(date);
                reply = (mode == "[^]") ? !((day == arg) && ((orn == key) || (key == "a"))) : ((day == arg) && ((orn == key) || (key == "a")));
            }
            else
            {
                reply = (mode == "[^]") ? (date != arg) : (date == arg);
            }
        }
        return reply;
    },
    isEvenOrOdd: function (date)
    {
        if (date - 21 > 0)
        {
            return "e";
        }
        else if (date - 14 > 0)
        {
            return "o";
        }
        else if (date - 7 > 0)
        {
            return "e";
        }
        else
        {
            return "o";
        }
    },
    dateFormat: function (year, month, date)
    {
        var day = this._ppcPtr.getDay();
        var crt = "";
        var str = "";
        var chars = this._dateFormat.length;
        for (var i = 0; i < chars; ++i)
        {
            crt = this._dateFormat.charAt(i);
            switch (crt)
            {
                case "M": str += this._ppcMN[month]; break;
                case "m": str += (month < 9) ? ("0" + (++month)) : ++month; break;
                case "Y": str += year; break;
                case "y": str += year.substring(2); break;
                case "d": str += ((this._dateFormat.indexOf("m") != -1) && (date < 10)) ? ("0" + date) : date; break;
                case "W": str += this._ppcWN[day]; break;
                default: str += crt;
            }
        }
        return unescape(str);
    },
    getDelimiter: function ()
    {
        //First find the delimiter
        var crt = "";
        var delimitChar = '/';
        for (var i = 0; i < this._dateFormat.length; ++i)
        {
            crt = this._dateFormat.charAt(i);
            if (crt != "M" && crt != "m" && crt != "Y" && crt != "y" && crt != "d" && crt != "w")
            {
                delimitChar = crt;
            }
        }
        return delimitChar;
    },
    parseYear: function (dateString)
    {
        //Need to handle 2-digit and 4-digit years
        var year = this.parseDate(dateString, "Y");
        if (year == -1)
        {
            return this.parseDate(dateString, "y");
        }
        else
        {
            return year;
        }
    },
    parseMonth: function (dateString)
    {
        return this.parseDate(dateString, "m");
    },
    parseDay: function (dateString)
    {
        return this.parseDate(dateString, "d");
    },
    parseDate: function (dateString, datePart)
    {
        var dchr = this.getDelimiter();

        //Get the dateformat and datestring segments
        var dateFormatArray = this._dateFormat.split(dchr);
        var dateStringArray = dateString.split(dchr);
        for (var i = 0; i < dateFormatArray.length; i++)
        {
            if (dateFormatArray[i] == datePart)
            {
                return dateStringArray[i];
            }
        }
        return -1; //If not able to match
    },
    checkDate: function (arg1)
    {
        //TODO: Need to get the dateformat, and fix the locations of the three variables.
        function isInteger(s)
        {
            var i;
            for (i = 0; i < s.length; i++)
            {
                // Check that current character is number.
                var c = s.charAt(i);
                if (((c < "0") || (c > "9"))) return false;
            }
            // All characters are numbers.
            return true;
        }
        function stripCharsInBag(s, bag)
        {
            var i;
            var returnString = "";
            // Search through string's characters one by one.
            // If character is not in bag, append to returnString.
            for (i = 0; i < s.length; i++)
            {
                var c = s.charAt(i);
                if (bag.indexOf(c) == -1) returnString += c;
            }
            return returnString;
        }
        function daysInFebruary(year)
        {
            // February has 29 days in any year evenly divisible by four,
            // EXCEPT for centurial years which are not also divisible by 400.
            return (((year % 4 == 0) && ((!(year % 100 == 0)) || (year % 400 == 0))) ? 29 : 28);
        }
        function DaysArray(n)
        {
            for (var i = 1; i <= n; i++)
            {
                this[i] = 31;
                if (i == 4 || i == 6 || i == 9 || i == 11) { this[i] = 30; }
                if (i == 2) { this[i] = 29; }
            }
            return this;
        }

        if (!arg1)
            arg1 = this.get_value();

        var daysInMonth = DaysArray(12);
        var pos1 = arg1.indexOf(this.getDelimiter());
        var pos2 = arg1.indexOf(this.getDelimiter(), pos1 + 1);
        var strMonth = arg1.substring(0, pos1);
        var strDay = arg1.substring(pos1 + 1, pos2);
        var strYear = arg1.substring(pos2 + 1);
        strYr = strYear;
        if (strDay.charAt(0) == "0" && strDay.length > 1) strDay = strDay.substring(1);
        if (strMonth.charAt(0) == "0" && strMonth.length > 1) strMonth = strMonth.substring(1);
        for (var i = 1; i <= 3; i++)
        {
            if (strYr.charAt(0) == "0" && strYr.length > 1) strYr = strYr.substring(1);
        }
        month = parseInt(strMonth);
        day = parseInt(strDay);
        year = parseInt(strYr);
        if (strYear.length < 4) year += 2000;
        if (pos1 == -1 || pos2 == -1)
            return false;
        if (strMonth.length < 1 || month < 1 || month > 12)
            return false;
        if (strDay.length < 1 || day < 1 || day > 31 || (month == 2 && day > daysInFebruary(year)) || day > daysInMonth[month])
            return false;
        if (year == 0 || year < 1700 || year > 2100)
            return false;
        if (arg1.indexOf(this.getDelimiter(), pos2 + 1) != -1 || isInteger(stripCharsInBag(arg1, this.getDelimiter())) == false)
            return false;

        return true;
    }
}

Inflectra.SpiraTest.Web.ServerControls.DatePicker.registerClass('Inflectra.SpiraTest.Web.ServerControls.DatePicker', Sys.UI.Control);

if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
