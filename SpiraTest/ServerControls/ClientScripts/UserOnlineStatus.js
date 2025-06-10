var d = document;
d.ce = d.createElement;
var resx = Inflectra.SpiraTest.Web.GlobalResources;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus = function (element)
{
    this._userId = null;
    this._timeInterval = 10;
    this._isOnline = false;
    this._innerDiv = null;
    this._clickHandler = null;

    //Load in all the passed parameters from the server-control
    Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.initializeBase(this, [element]);
}

Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.callBaseMethod(this, 'initialize');

        //Create the inner DIV
        this._innerDiv = d.ce('div');
        this.get_element().appendChild(this._innerDiv);

        //Display the initial status
        this._innerDiv.className = 'Unregistered';

        //Add handlers
        this._clickHandler = Function.createDelegate(this, this._onClick);
        $addHandler(this._innerDiv, 'click', this._clickHandler);

        //Register with the global array
        if (g_userOnlineStatusManager)
        {
            g_userOnlineStatusManager.register_control(this.get_element().id);
        }
    },
    dispose: function ()
    {
        $removeHandler(this._innerDiv, 'click', this._clickHandler);

        delete this._innerDiv;
        delete this._clickHandler;

        Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.callBaseMethod(this, 'dispose');
    },

    // -------- Properties -------- //
    get_userId: function ()
    {
        return this._userId;
    },
    set_userId: function (value)
    {
        this._userId = value;
    },

    get_timeInterval: function ()
    {
        return this._timeInterval;
    },
    set_timeInterval: function (value)
    {
        this._timeInterval = value;
    },

    get_isOnline: function ()
    {
        return this._isOnline;
    },
    set_isOnline: function (value)
    {
        this._isOnline = value;
        this.refresh_display();
    },

    // -------- Methods --------- //
    refresh_display: function ()
    {
        //Change the style based on the status
        this._innerDiv.className = (this._isOnline) ? 'Online' : 'Offline';
    },

    // ------ Event Handlers ------ //
    _onClick: function (evt)
    {
        if (tstucMessageManager && tstucMessageManager.send_new_message && this._userId)
        {
            tstucMessageManager.send_new_message(this._userId, evt);
        }
    }
}

Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.registerClass('Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus', Sys.UI.Control);

//Static object that is used to track all these controls and make the web service requests
Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager = function ()
{
    this._userIds = new Array();
    this._callbacks = new Array();
    this._controls = new Array();
    this._isInErrorStatus = false;
    Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager.initializeBase(this);
}
Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager.callBaseMethod(this, 'initialize');
    },
    dispose: function ()
    {
        delete this._controls;
        Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager.callBaseMethod(this, 'dispose');
    },

    /* Properties */
    get_timeInterval: function ()
    {
        return this._timeInterval;
    },
    set_timeInterval: function (value)
    {
        this._timeInterval = value;
    },

    get_isInErrorStatus: function ()
    {
        return this._isInErrorStatus;
    },
    set_isInErrorStatus: function (value)
    {
        this._isInErrorStatus = value;
    },

    //Checks if a user online
    check_status: function(userId)
    {
        if (this._userIds)
        {
            for (var i = 0; i < this._userIds.length; i++)
            {
                if (this._userIds[i] == userId)
                {
                    return true;
                }
            }
        }
        return false;
    },

    //Returns a list of user ids online
    get_onlineUsers: function()
    {
        return this._userIds;
    },

    /* Functions */
    register_control: function (controlId)
    {
        //Register and mark as 'offline' initially
        this._controls.push(controlId);
        $find(controlId).set_isOnline(false);
    },
    //Allows a function to be called each time the status is updated
    register_callback: function(callbackFunction) {
        this._callbacks.push(callbackFunction);
    },
    update_status: function (userIds)
    {
        if (userIds)
        {
            this._userIds = userIds;
            for (var i = 0; i < this._controls.length; i++)
            {
                var controlId = this._controls[i];
                var statusControl = $find(controlId);
                if (statusControl)
                {
                    //See if online or not
                    var isOnline = false;
                    for (var j = 0; j < userIds.length; j++)
                    {
                        if (userIds[j] == statusControl.get_userId())
                        {
                            isOnline = true;
                            break;
                        }
                    }
                    statusControl.set_isOnline(isOnline);
                }
            }
            //loop through all callback functions to make them run each time the function is called
            for (var c = 0; c < this._callbacks.length; c++)
            {
               this._callbacks[c]();
            }
        }
    },
    //same code used here as for _onClick function, but with userId passed in as a parameter
    sendMessageToSpecifiedUser: function (specifiedUserId) {
        if (tstucMessageManager && tstucMessageManager.send_new_message && specifiedUserId) {
            tstucMessageManager.send_new_message(specifiedUserId);
        }
    }
};
Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager.registerClass('Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager', Sys.Component);
var g_userOnlineStatusManager = $create(Inflectra.SpiraTest.Web.ServerControls.UserOnlineStatus.Manager);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
