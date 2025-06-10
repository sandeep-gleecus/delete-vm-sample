var d = document;
d.ce = d.createElement;
d.ct = d.createTextNode;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink.initializeBase(this, [element]);

    this._enabled = true;
    this._required = false;
    this._hidden = false;
    this._tooltip = '';
    this._itemImage = '';
    this._summaryItemImage = '';
    this._alternateItemImage = '';
    this._baseUrl = '';
    this._themeFolder = '';
    this._alternateText = '';
    this._displayChangeLink = false;
    this._changeLink = null;

    //The artifact data
    this._artifactId = null;
    this._artifactName = '';
    this._summary = false;
    this._alternate = false;
    this._customUrl = '';

    this._mouseOverHandler = null;
    this._mouseOutHandler = null;
    this._changeLinkClickHandler = null;
};
Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink.callBaseMethod(this, 'initialize');

        //Register the mouse over/out handler for tooltips
        this._mouseOverHandler = Function.createDelegate(this, this._onMouseOver);
        this._mouseOutHandler = Function.createDelegate(this, this._onMouseOut);
        $addHandler(this.get_element(), 'mouseover', this._mouseOverHandler);
        $addHandler(this.get_element(), 'mouseout', this._mouseOutHandler);

        //Create the change link object (don't add to DOM)
        this._changeLink = d.ce('a');
        this._changeLink.className = "btn btn-default btn-sm";
        this._changeLink.appendChild(d.ct(resx.Global_Change));
        this._changeLinkClickHandler = Function.createDelegate(this, this._onChangeLinkClick);
        $addHandler(this._changeLink, 'click', this._changeLinkClickHandler);

        //Render the current artifact
        this.render_artifact();
    },
    dispose: function ()
    {
        $removeHandler(this.get_element(), 'mouseover', this._mouseOverHandler);
        $removeHandler(this.get_element(), 'mouseout', this._mouseOutHandler);
        delete this._mouseOverHandler;
        delete this._mouseOutHandler;

        $removeHandler(this._changeLink, 'click', this._changeLinkClickHandler);
        delete this._changeLinkClickHandler;
        delete this._changeLink;

        Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink.callBaseMethod(this, 'dispose');
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

    get_tooltip: function ()
    {
        return this._tooltip;
    },
    set_tooltip: function (value)
    {
        this._tooltip = value;
    },

    get_alternateText: function ()
    {
        return this._alternateText;
    },
    set_alternateText: function (value)
    {
        this._alternateText = value;
    },

    get_displayChangeLink: function ()
    {
        return this._displayChangeLink;
    },
    set_displayChangeLink: function (value)
    {
        this._displayChangeLink = value;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
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

    get_alternateItemImage: function ()
    {
        return this._alternateItemImage;
    },
    set_alternateItemImage: function (value)
    {
        this._alternateItemImage = value;
    },

    get_baseUrl: function ()
    {
        return this._baseUrl;
    },
    set_baseUrl: function (value)
    {
        this._baseUrl = value;
    },

    get_artifactId: function ()
    {
        return this._artifactId;
    },
    set_artifactId: function (value)
    {
        this._artifactId = value;
    },

    get_artifactName: function ()
    {
        return this._artifactName;
    },
    set_artifactName: function (value)
    {
        this._artifactName = value;
    },

    get_customUrl: function () {
        return this._customUrl;
    },
    set_customUrl: function (value) {
        this._customUrl = value;
    },

    get_summary: function ()
    {
        return this._summary;
    },
    set_summary: function (value)
    {
        this._summary = value;
    },

    get_alternate: function ()
    {
        return this._alternate;
    },
    set_alternate: function (value)
    {
        this._alternate = value;
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
    _onChangeLinkClick: function (evt)
    {
        this.raise_changeClicked(evt);
    },

    /*  =========================================================
    Event handlers managers
    =========================================================  */
    add_changeClicked: function (handler)
    {
        this.get_events().addHandler('changeClicked', handler);
    },
    remove_changeClicked: function (handler)
    {
        this.get_events().removeHandler('changeClicked', handler);
    },
    raise_changeClicked: function (evt)
    {
        var h = this.get_events().getHandler('changeClicked');
        if (h) h(evt);
    },

    /* Public Methods */

    //Main interface for setting the artifact
    update_artifact: function (artifactId, artifactName, tooltip, summary, alternate)
    {
        this._artifactId = artifactId;
        this._artifactName = artifactName;
        this._summary = summary;
        this._alternate = alternate;
        //The tooltip could be an actual tooltip or a URL depending on if we have a base URL
        if (this._baseUrl && this._baseUrl != '') {
            this._tooltip = tooltip;
            this._customUrl = '';
        }
        else
        {
            this._tooltip = '';
            this._customUrl = tooltip;
        }
        this.render_artifact();
    },
    update_state: function (state)
    {
        if (!state || state == 'hidden')
        {
            if (this.get_hidden())
            {
                this._element.style.display = 'none';
            }
            else
            {
                this._element.style.display = 'inline-block';
            }
        }
        if (this._changeLink && (!state || state == 'enabled'))
        {
            if (this.get_enabled())
            {
                this._changeLink.style.display = 'inline-block';
            }
            else
            {
                this._changeLink.style.display = 'none';
            }
        }
    },
    render_artifact: function ()
    {
        //Clear the content
        var element = this.get_element();
        globalFunctions.clearContent(element);

        //See if we have an artifact specified or not
        if (this._artifactId && this._artifactName)
        {
            //See if we have an image specified
            var img = null;
            if (this._summary && this._summaryItemImage && this._summaryItemImage != '')
            {
                img = d.ce('img');
                img.src = this._themeFolder + this._summaryItemImage;
            }
            else if (this._alternate && this._alternateItemImage && this._alternateItemImage != '')
            {
                img = d.ce('img');
                img.src = this._themeFolder + this._alternateItemImage;
            }
            else if (this._itemImage && this._itemImage != '')
            {
                img = d.ce('img');
                img.src = this._themeFolder + this._itemImage;
            }
            if (img)
            {
                img.alt = this._alternateText;
                img.className = "h4 w4 v-mid mr2";
                element.appendChild(d.ct('\u00a0'));
                element.appendChild(img);
            }

            // now add the hyperlink. We may have a simple ID based standard URL or a custom URL
            var a = d.ce('a');
            element.appendChild(a);
            a.appendChild(d.ct(this._artifactName));
            if (this._baseUrl && this._baseUrl != '') {
                a.href = this._baseUrl.replace(globalFunctions.artifactIdToken, this._artifactId);
            }
            else if (this._customUrl && this._customUrl != '')
            {
                a.href = this._customUrl;
            }
        }
        else
        {
            element.appendChild(d.ct('-'));
        }

        if (this._displayChangeLink)
        {
            //Display the 'Change' hyperlink
            element.appendChild(d.ct('\u00a0'));
            element.appendChild(this._changeLink);
            this._changeLink.href = 'javascript:void(0)';
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink.registerClass('Inflectra.SpiraTest.Web.ServerControls.ArtifactHyperLink', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
