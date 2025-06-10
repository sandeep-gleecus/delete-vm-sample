var d = document;
d.ce = d.createElement;
d.ct = d.createTextNode;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.LabelEx = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.LabelEx.initializeBase(this, [element]);

    this._enabled = true;
    this._required = false;
    this._hidden = false;
    this._containsMarkup = false;
    this._tooltip = '';
    this._baseCss = element.className || "";
    this._associatedControlId = null;
    this._oldClassName = '';

    this._mouseOverHandler = null;
    this._mouseOutHandler = null;
    this._clickHandler = null;
};
Inflectra.SpiraTest.Web.ServerControls.LabelEx.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.LabelEx.callBaseMethod(this, 'initialize');

        //Register the mouse over/out handler for tooltips
        this._mouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        this._mouseOutHandler = Function.createDelegate(this, this._onMouseOut);
        $addHandler(this.get_element(), 'mouseover', this._mouseOverHandler);
        $addHandler(this.get_element(), 'mouseout', this._mouseOutHandler);

        //Add the click handler if we have an associated ajax control
        if (this._associatedControlId)
        {
            this._clickHandler = Function.createDelegate(this, this._onClick);
            $addHandler(this.get_element(), 'click', this._clickHandler);
        }

        //Store the old class
        if (this.get_element().className)
        {
            this._oldClassName = this.get_element().className;
        }
        else
        {
            this._oldClassName = this._baseCss;
        }

        if (this._containsMarkup)
        {
            //Clean out any script (prevents XSS)
            globalFunctions.cleanHtml(this.get_element());
        }
    },
    dispose: function ()
    {
        if (this._associatedControlId)
        {
            $removeHandler(this.get_element(), 'click', this._clickHandler);
            delete this._clickHandler;
        }
        $removeHandler(this.get_element(), 'mouseover', this._mouseOverHandler);
        $removeHandler(this.get_element(), 'mouseout', this._mouseOutHandler);
        delete this._mouseOverHandler;
        delete this._mouseOutHandler;

        Inflectra.SpiraTest.Web.ServerControls.LabelEx.callBaseMethod(this, 'dispose');
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

    get_containsMarkup: function ()
    {
        return this._containsMarkup;
    },
    set_containsMarkup: function (value)
    {
        this._containsMarkup = value;
    },

    get_tooltip: function ()
    {
        return this._tooltip;
    },
    set_tooltip: function (value)
    {
        this._tooltip = value;
    },

    get_associatedControlId: function ()
    {
        return this._associatedControlId;
    },
    set_associatedControlId: function (value)
    {
        this._associatedControlId = value;
    },

    /* Functions */
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
                $(element).removeClass('aspNetDisabled');
                element.disabled = ''
            }
            else
            {
                $(element).addClass('aspNetDisabled');
                element.disabled = 'disabled';
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
    },

    /* Event Handlers */
    _onMouseOver: function (evt)
    {
        if (this.get_tooltip() && this.get_tooltip() != '')
        {
            ddrivetip(this.get_tooltip());
        }
    },
    _onMouseOut: function (evt)
    {
        hideddrivetip();
    },
    _onClick: function (evt)
    {
        //Ajax controls
        var ajaxControl = $find(this.get_associatedControlId());
        if (ajaxControl && ajaxControl.activate)
        {
            ajaxControl.activate(evt);
        }

        //ckEditor instances
        if (CKEDITOR && CKEDITOR.instances[this.get_associatedControlId()])
        {
            var editor = CKEDITOR.instances[this.get_associatedControlId()];
            editor.focus();
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.LabelEx.registerClass('Inflectra.SpiraTest.Web.ServerControls.LabelEx', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
