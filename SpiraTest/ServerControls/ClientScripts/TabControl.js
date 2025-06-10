var d = document;
d.ce = d.createElement;
d.ct = d.createTextNode;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

/* TabControl */
Inflectra.SpiraTest.Web.ServerControls.TabControl = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.TabControl.initializeBase(this, [element]);

    this._tabWidth = '';
    this._pageId = '';
    this._tabHeight = '';
    this._dividerCssClass = '';
    this._tabCssClass = '';
    this._disabledTabCssClass = '';
    this._selectedTabCssClass = '';
    this._tabPages = null;
    this._selectedTab = null;
    this._selectedTabClientId = ''; //Only set on initial load
    this._selectedTabHiddenField = null;
    this._themeFolder = '';
    this._tabDropDown = null;
    this._loaded = false;

    this._clickElements = new Array();
    this._clickHandlers = new Array();
}
Inflectra.SpiraTest.Web.ServerControls.TabControl.prototype =
{
    /* Constructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.TabControl.callBaseMethod(this, 'initialize');

        //Add an initial leading DIV
        var element = this.get_element();

        //Add the tab pages as child DIVs to the main DIV element
        var pageLookups = {};
        for (var i = 0; i < this._tabPages.length; i++)
        {
            var isFirst = i === 0,
                isLast = i === this._tabPages.length - 1;
            var tabPage = this._tabPages[i];
            if (tabPage.get_visible())
            {
                var div = d.ce('div');
                div.setAttribute('role', isFirst ? 'tab br2-tl' : isLast ? 'tab br2-tr' : 'tab');
                div.setAttribute('aria-label', tabPage.get_caption());
                element.appendChild(div);
                //Pass a handle to the tab div
                tabPage.set_element(div);

                //Create the icon if any and add it to the tab
                var icon = tabPage.get_tabPageIcon();
                if (icon)
                {
                    var iconSpan = d.ce('span');
                    iconSpan.className = icon + " mr3 tab-icon";
                    div.appendChild(iconSpan);
                }

                //Create the image if any and add it to the tab
                var tabImageUrl = tabPage.get_tabPageImageUrl();
                if (tabImageUrl) {
                    var img = d.ce('img');
                    img.src = SpiraContext.BaseThemeUrl + tabImageUrl;
                    img.className = "w4 h4 mr3 tab-image";
                    div.appendChild(img);
                }

                //Add the caption
                var captionSpan = d.ce('span');
                captionSpan.className = "tab-caption"
                captionSpan.appendChild(d.ct(tabPage.get_caption()));
                div.appendChild(captionSpan);
                this.setTabClass(tabPage);

                if (tabPage.get_enabled())
                {
                    //Finally add the click event handler to the tab
                    var onClickHandler = Function.createCallback(this._onClick, { thisRef: this, tabPage: tabPage });
                    $addHandler(div, 'click', onClickHandler);
                    this._clickElements.push(div);
                    this._clickHandlers.push(onClickHandler);

                    var caption = tabPage.get_caption();
                    if (tabPage.get_hasData())
                    {
                        caption += ' *';
                    }
                    pageLookups[globalFunctions.keyPrefix + tabPage.get_tabPageClientId()] = caption;
                }
            }
        }

        //Create the hidden field used to store postbacks
        this._selectedTabHiddenField = d.ce('input');
        this._selectedTabHiddenField.type = 'hidden';
        this._selectedTabHiddenField.name = this.get_element().id + '_selectedTab';
        this._selectedTabHiddenField.value = this._selectedTabClientId;
        this.get_element().appendChild(this._selectedTabHiddenField);

        //Finally make all the panels associated with the tabs hidden except for the selected one
        for (var i = 0; i < this._tabPages.length; i++)
        {
            var tabPage = this._tabPages[i];
            tabPage.set_pageId(this._pageId);
            tabPage.set_id(this._element.id);
            if (tabPage.get_tabPageClientId() && tabPage.get_tabPageClientId() != '')
            {
                var tabPageClient = $get(tabPage.get_tabPageClientId());
                if (tabPageClient)
                {
                    if (this._selectedTabClientId == tabPage.get_tabPageClientId() && tabPage.get_visible())
                    {
                        //Display the tab
                        tabPageClient.style.display = 'block';
                        var tabPageEl = tabPage.get_element();
                        tabPageEl.setAttribute('aria-selected', true);
                        this._selectedTab = tabPage;
                    }
                    else
                    {
                        if (tabPage.get_element())
                        {
                            tabPage.get_element().setAttribute('aria-selected', false);
                        }
                        tabPageClient.style.display = 'none';
                    }
                }
            }
        }

        //For small screens create a dropdown equivalent
        var ddl = d.ce('div');
        this._tabDropDown = $create(Inflectra.SpiraTest.Web.ServerControls.DropDownList, { multiSelectable: true, displayMobileAsDesktop: true }, { selectedItemChanged: Function.createDelegate(this, this._onDropDownChanged) }, null, ddl);
        element.appendChild(ddl);
        this._tabDropDown.set_dataSource(pageLookups);
        this._tabDropDown.dataBind();
        this._tabDropDown.set_selectedItem(this._selectedTabClientId);
        this._loaded = true;

        //Next add the divider under the tabs
        var div = d.ce('div');
        div.className = this._dividerCssClass;
        element.appendChild(div);
    },
    dispose: function ()
    {
        for (var i = 0; i < this._clickElements.length; i++)
        {
            $removeHandler(this._clickElements[i], 'click', this._clickHandlers[i]);
            delete this._clickElements[i];
            delete this._clickHandlers[i];
        }

        delete this._clickElements;
        delete this._clickHandlers;

        delete this._tabPages;
        delete this._tabDropDown;
        Inflectra.SpiraTest.Web.ServerControls.TabControl.callBaseMethod(this, 'dispose');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },

    set_selectedTabClientId: function (value, noEventRaise)
    {
        this._selectedTabClientId = value;
        if (this._tabPages && !noEventRaise)
        {
            for (var i = 0; i < this._tabPages.length; i++)
            {
                if (this._tabPages[i].get_tabPageClientId() == value)
                {
                    this._onClick(this, { thisRef: this, tabPage: this._tabPages[i] });
                }
            }
        }
    },
    get_selectedTabClientId: function ()
    {
        return this._selectedTabClientId;
    },

    get_themeFolder: function ()
    {
        return this._themeFolder;
    },
    set_themeFolder: function (value)
    {
        this._themeFolder = value;
    },

    get_pageId: function ()
    {
        return this._pageId;
    },
    set_pageId: function (value)
    {
        this._pageId = value;
    },

    get_tabWidth: function ()
    {
        return this._tabWidth;
    },
    set_tabWidth: function (value)
    {
        this._tabWidth = value;
    },

    get_tabHeight: function ()
    {
        return this._tabHeight;
    },
    set_tabHeight: function (value)
    {
        this._tabHeight = value;
    },

    get_dividerCssClass: function ()
    {
        return this._dividerCssClass;
    },
    set_dividerCssClass: function (value)
    {
        this._dividerCssClass = value;
    },

    get_tabCssClass: function ()
    {
        return this._tabCssClass;
    },
    set_tabCssClass: function (value)
    {
        this._tabCssClass = value;
    },

    get_disabledTabCssClass: function ()
    {
        return this._disabledTabCssClass;
    },
    set_disabledTabCssClass: function (value)
    {
        this._disabledTabCssClass = value;
    },

    get_selectedTabCssClass: function ()
    {
        return this._selectedTabCssClass;
    },
    set_selectedTabCssClass: function (value)
    {
        this._selectedTabCssClass = value;
    },

    //Used to set the list of tab pages
    get_tabPages: function ()
    {
        return this._tabPages;
    },
    set_tabPages: function (value)
    {
        //Need to make sure we get the expected array of controls
        var e = Function._validateParams(arguments, [{ name: 'value', type: Array, elementType: Inflectra.SpiraTest.Web.ServerControls.TabPage, elementMayBeNull: false}]);
        if (e) throw e;

        this._tabPages = value;
    },

    /* Functions */
    get_tabPage: function (name)
    {
        for (var i = 0; i < this._tabPages.length; i++)
        {
            if (this._tabPages[i].get_name() == name)
            {
                return this._tabPages[i];
            }
        }
        return null;
    },
    updateHasData: function(tabName, hasData)
    {
        for (var i = 0; i < this._tabPages.length; i++)
        {
            var tabPage = this._tabPages[i];
            if (tabPage.get_tabPageClientId() == tabName || tabPage.get_name() == tabName)
            {
                tabPage.set_hasData(hasData);
                this.setTabClass(tabPage);
                break;
            }
        }
    },

    setTabClass: function(tabPage)
    {
        var div = tabPage.get_element();
        var className = '';
        if (tabPage.get_enabled())
        {
            //Add the has-data class if necessary
            if (tabPage.get_hasData())
            {
                className = this._tabCssClass + ' HasData';
            }
            else
            {
                className = this._tabCssClass;
            }
        }
        else
        {
            //Add the has-data class if necessary
            if (tabPage.get_hasData())
            {
                className = this._disabledTabCssClass + ' HasData';
            }
            else
            {
                className = this._disabledTabCssClass;
            }
        }

        if (this._selectedTabClientId == tabPage.get_tabPageClientId())
        {
            className += ' ' + this._selectedTabCssClass;
        }
        div.className = className;
    },

    /*  =========================================================
    The event handler managers
    =========================================================  */
    add_selectedTabChanged: function (handler) {
        this.get_events().addHandler('selectedTabChanged', handler);
    },
    remove_selectedTabChanged: function (handler) {
        this.get_events().removeHandler('selectedTabChanged', handler);
    },
    raise_selectedTabChanged: function (tabPage) {
        var h = this.get_events().getHandler('selectedTabChanged');
        if (h) h(tabPage);
    },

    /* Event Handlers */
    _onClick: function (e, context)
    {
        var thisRef = context.thisRef;
        var tabPage = context.tabPage;
        if (thisRef._selectedTab)
        {
            if (thisRef._selectedTab != tabPage)
            {
                tabPage.select(thisRef);
                thisRef._selectedTab.unselect(thisRef);
            }
        }
        else
        {
            tabPage.select(thisRef);
        }
        thisRef._selectedTab = tabPage;
        thisRef._selectedTabHiddenField.value = tabPage.get_tabPageClientId();
        thisRef.raise_selectedTabChanged(tabPage);
    },

    //Selects the tab from the dropdown
    _onDropDownChanged: function(e)
    {
        if (this._loaded)
        {
            var selectedTabPageClientId = this._tabDropDown.get_selectedItem().get_value();
            for (var i = 0; i < this._tabPages.length; i++)
            {
                var tabPage = this._tabPages[i];
                if (tabPage.get_tabPageClientId() == selectedTabPageClientId)
                {
                    if (this._selectedTab)
                    {
                        if (this._selectedTab != tabPage)
                        {
                            tabPage.select(this);
                            this._selectedTab.unselect(this);
                        }
                    }
                    else
                    {
                        tabPage.select(this);
                    }
                    this._selectedTab = tabPage;
                    this._selectedTabHiddenField.value = tabPage.get_tabPageClientId();
                    break;
                }
            }
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.TabControl.registerClass('Inflectra.SpiraTest.Web.ServerControls.TabControl', Sys.UI.Control);

/* TabPage */
Inflectra.SpiraTest.Web.ServerControls.TabPage = function (name, caption, hasData, enabled, visible, tabPageClientId, ajaxClientId, tabPageIcon, tabPageImageUrl, tabName)
{
    Inflectra.SpiraTest.Web.ServerControls.TabPage.initializeBase(this);

    this._ajaxClientId = ajaxClientId;
    this._caption = caption;
    this._element = null;
    this._enabled = enabled;
    this._hasData = hasData;
    this._id = '';
    this._pageId = '';
    this._tabPageClientId = tabPageClientId;
    this._tabPageIcon = tabPageIcon;
	this._tabPageImageUrl = tabPageImageUrl;
	this._tabName = tabName;
    this._visible = visible;
    this._name = name;
}
Inflectra.SpiraTest.Web.ServerControls.TabPage.prototype =
{
    /* Contstructors */
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.TabPage.callBaseMethod(this, 'initialize');
    },
    dispose: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.TabPage.callBaseMethod(this, 'dispose');
    },

    /* Properties */
    get_element: function ()
    {
        return this._element;
    },
    set_element: function (value)
    {
        this._element = value;
    },

    get_id: function ()
    {
        return this._id;
    },
    set_id: function (value)
    {
        this._id = value;
    },

    get_name: function ()
    {
        return this._name;
    },
    set_name: function (value)
    {
        this._name = value;
    },

    get_pageId: function ()
    {
        return this._pageId;
    },
    set_pageId: function (value)
    {
        this._pageId = value;
    },

    get_caption: function ()
    {
        return this._caption;
    },
    set_caption: function (value)
    {
        this._caption = value;
    },
    get_tabPageIcon: function () {
        return this._tabPageIcon;
    },
    set_tabPageIcon: function (value) {
        this._tabPageIcon = value;
    },
    get_tabPageImageUrl: function () {
        return this._tabPageImageUrl;
    },
    set_tabPageImageUrl: function (value) {
        this._tabPageImageUrl = value;
	},
	get_tabName: function () {
		return this._tabName;
	},
	set_tabName: function (value) {
		this._tabName = value;
	},
    get_enabled: function ()
    {
        return this._enabled;
    },
    set_enabled: function (value)
    {
        this._enabled = value;
    },

    get_visible: function ()
    {
        return this._visible;
    },
    set_visible: function (value)
    {
        this._visible = value;
    },

    get_hasData: function ()
    {
        return this._hasData;
    },
    set_hasData: function (value)
    {
        this._hasData = value;
    },

    get_tabPageClientId: function ()
    {
        return this._tabPageClientId;
    },
    set_tabPageClientId: function (value)
    {
        this._tabPageClientId = value;
    },

    /* Methods */

    show: function()
    {
        //Makes the tab visible - we use inline style because class are programmatically set on the tab elsewhere
        if (this._element) {
            this._element.style.display = "unset";
        }
    },
    hide: function()
    {
        //Hides the tab itself - we use inline style because class are programmatically set on the tab elsewhere
        if (this._element) {
            this._element.style.display = "none";
        }
    },

    load_data: function ()
    {
        //Loads any ajax controls in the tab page
        if (this._ajaxClientId && this._ajaxClientId != '')
        {
            //Make sure that the necessary methods exist of the component
            var clientComponent = $find(this._ajaxClientId);
            if (clientComponent && clientComponent.get_loadingComplete && clientComponent.load_data)
            {
                if (!clientComponent.get_loadingComplete())
                {
                    clientComponent.load_data();
                }
            }
        }
    },

    //Called when the tab is clicked
    select: function (parent)
    {
        //Make the tab visible
        var tabPageClient = $get(this._tabPageClientId);
        if (tabPageClient)
        {
            tabPageClient.style.display = 'block';
            
            //if on a unity page, refresh any wrapper to make the columns flow correctly
            if ($('.u-wrapper').length > 0 && typeof uWrapper_onResize == 'function') {
                uWrapper_onResize();
            }
        }
        if (this._element)
        {
            parent.set_selectedTabClientId(this._tabPageClientId, true);
            parent.setTabClass(this);
            this._element.setAttribute('aria-selected', true);
        }

        //Load the data if necessary
        this.load_data();

        //Update the state stored in the web service for the current user
		var selectedTab = this._tabPageClientId;
        Inflectra.SpiraTest.Web.Services.Ajax.GlobalService.TabControl_UpdateState(this.get_pageId(), this.get_id(), selectedTab, Function.createDelegate(this, this._updateStateSuccess), Function.createDelegate(this, this._updateStateFailure));
		this._pushNewTabToUrl(window.location.href);
    },
    _updateStateSuccess: function ()
    {
        //Do nothing
    },
    _updateStateFailure: function (exception)
    {
        globalFunctions.display_error(null, exception);
	},

	_pushNewTabToUrl: function (url)
	{
		var urlToPush = "";
		var newTabName = this.get_tabName();
		var regexIdOnly = /(\d+)\.aspx/;
		var regexIdAndTabOnly = /(\d+)(\/[A-Za-z]+)\.aspx/;
		//If a tabName has been set on the selected tab control, see if we can update the url with it
		if (newTabName)
		{
			//If the URL is in the correct format, update it with the new tab name
			if (regexIdOnly.test(url)) {
				urlToPush = url.replace(regexIdOnly, '$1' + '/' + newTabName + '.aspx');
			} else if (regexIdAndTabOnly.test(url)) {
				urlToPush = url.replace(regexIdAndTabOnly, '$1' + '/' + newTabName + '.aspx');
			}
			//If the URL has changed, push it to the browser to update the url
			if (urlToPush != window.location.href) {
				history.replaceState({ selectedTab: this._tabPageClientId }, null, urlToPush);
				//Update the SpiraContext object so that other places can track the current tab name (eg NavigationBar.js _liveLoadForm)
				SpiraContext.ArtifactTabName = newTabName;
			}
		}
	},

    //Called when the tab has been been deselected
    unselect: function (parent)
    {
        //Make the tab visible
        var tabPageClient = $get(this._tabPageClientId);
        if (tabPageClient)
        {
            tabPageClient.style.display = 'none';
        }
        if (this._element)
        {
            this._element.setAttribute('aria-selected', false);
            parent.setTabClass(this);
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.TabPage.registerClass('Inflectra.SpiraTest.Web.ServerControls.TabPage', Sys.Component);

//  always end with this goodnight statement        
if (typeof(Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
