<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MessageManager.ascx.cs" Inherits="Inflectra.SpiraTest.Web.UserControls.MessageManager" %>
<tstsc:ScriptManagerProxyEx ID="ajxScriptManager" runat="server">
  <Services>
    <asp:ServiceReference Path="~/Services/Ajax/MessageService.svc" />  
  </Services>  
</tstsc:ScriptManagerProxyEx>
<div id="divDialogContainers" runat="server" />

<script type="text/javascript">
    var resx = Inflectra.SpiraTest.Web.GlobalResources;

    /* The User Control Class */
    Type.registerNamespace('Inflectra.SpiraTest.Web.UserControls');
    Inflectra.SpiraTest.Web.UserControls.MessageManager = function ()
    {
        this._themeFolder = '<%=ThemeFolder %>';
        this._avatarBaseUrl = '<%=AvatarBaseUrl %>';
        this._userId = <%=UserId%>;
        this._timeInterval = 5000; //5 seconds
        this._inactiveTimeInterval = 10000; //10 seconds
        this._isInError = false;
        this._dialogBoxes = {};
        this._messageLists = {};
        this._unreadMessagesTotal = -1;
        this._unreadMessages = {};
        this._lastNotifyMessageId = -1;

        this._artifactId = null;
        this._artifactTypeId = null;
        this._commentControlId = null;
        this._commentProjectId = null;
        this._windowOnFocusHandler = null;

        Inflectra.SpiraTest.Web.UserControls.MessageManager.initializeBase(this);
    }
    Inflectra.SpiraTest.Web.UserControls.MessageManager.prototype =
    {
        initialize: function ()
        {
            Inflectra.SpiraTest.Web.UserControls.MessageManager.callBaseMethod(this, 'initialize');
        },
        dispose: function ()
        {
            $removeHandler(window, 'focus', this._windowOnFocusHandler);
            delete this._windowOnFocusHandler;

            Inflectra.SpiraTest.Web.UserControls.MessageManager.callBaseMethod(this, 'dispose');
        },

        /* Properties */
        get_artifactId: function ()
        {
            return this._artifactId;
        },
        set_artifactId: function (value)
        {
            this._artifactId = value;
        },

        get_artifactTypeId: function ()
        {
            return this._artifactTypeId;
        },
        set_artifactTypeId: function (value)
        {
            this._artifactTypeId = value;
        },

        get_commentControlId: function ()
        {
            return this._commentControlId;
        },
        set_commentControlId: function (value)
        {
            this._commentControlId = value;
        },

        get_commentProjectId: function ()
        {
            return this._commentProjectId;
        },
        set_commentProjectId: function (value)
        {
            this._commentProjectId = value;
        },

        /* Methods */
        clear_counts:function()
        {
            //Clear the counts (used when an IM window is closed)
            this._unreadMessagesTotal = -1;
            this._unreadMessages = {};
        },
        start_polling: function()
        {
            //Register the event handler for document focus
            this._windowOnFocusHandler = Function.createDelegate(this, this._onWindowFocus);
            $addHandler(window, 'focus', this._windowOnFocusHandler);

            //check for the status immediately (then hand over to the timer)
            tstucMessageManager.check_status(false);
        },
        check_status: function(oneTime)
        {
            if (this._isInError)
            {
                //Stop the loop if we're in error
                return;
            }

            //Get the list of messages/users
            Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_GetInfo(Function.createDelegate(this, this.check_status_success), Function.createDelegate(this, this.check_status_failure), oneTime);
        },
        check_status_success: function (info, oneTime)
        {
            //Get the list of online users and update any status controls on the page, using the static manager class
            var onlineUsers = info.OnlineUsers;
            if (onlineUsers && typeof g_userOnlineStatusManager != 'undefined')
            {
                g_userOnlineStatusManager.update_status(onlineUsers);
            }

            //See if we have any unread messages, 
            if (info.UnreadMessages && info.UnreadMessages > 0)
            {
                //Change title bar if necessary
                var titleChanged = false;
                if (window.document.title.substr(0,1) == '(')
                {
                    var i = window.document.title.indexOf(') ');
                    if (i > -1)
                    {
                        var oldTitle = window.document.title;
                        var pageTitle = window.document.title.substring(i + 2);
                        window.document.title = '(' + info.UnreadMessages + ') ' + pageTitle;
                        if (window.document.title != oldTitle)
                        {
                            titleChanged = true;
                        }
                    }
                }
                else
                {
                    window.document.title = '(' + info.UnreadMessages + ') ' + window.document.title;
                    titleChanged = true;
                }

                //Make sure browser has focus first - checked method exists first
                if (document.hasFocus && document.hasFocus())
                {
                    this.handle_unread_messages(info.UnreadMessages);
                }
                else if (titleChanged)
                {
                    //Get the latest message for a browser notification
                    this.push_latest_notification(info.UnreadMessages);
                }
            }
            else
            {
                //Clear title bar
                if (window.document.title.substr(0,1) == '(')
                {
                    var i = window.document.title.indexOf(') ');
                    if (i > -1)
                    {
                        var pageTitle = window.document.title.substring(i + 2);
                        window.document.title = pageTitle;
                    }
                }
            }

            //Finally, reset the timer again, use a longer timer if window does not have focus
            if (!oneTime)
            {
                var timeInterval = (document.hasFocus && document.hasFocus()) ? this._timeInterval : this._inactiveTimeInterval;
                setTimeout(function () { tstucMessageManager.check_status(false); }, timeInterval);
            }
        },
        check_status_failure: function (exception, oneTime)
        {
            //If we have an error, simply mark system in error status.
            if (!oneTime)
            {
                this._isInError = true;
            }
        },
        handle_unread_messages: function(unreadCount)
        {
            //See if the number unread has changed since the last time
            if (unreadCount == this._unreadMessagesTotal)
            {
                //We simply use the existing dictionary of counts without hitting server
                this.handle_unread_messages_success(this._unreadMessages);
            }
            else
            {
                this._unreadMessagesTotal = unreadCount;
                //We need to get the list of users that have sent us messages that are unread
                Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_GetUnreadMessageSenders(Function.createDelegate(this, this.handle_unread_messages_success), Function.createDelegate(this, this.handle_unread_messages_failure));
            }
        },
        handle_unread_messages_success: function(senders)
        {
            if (senders)
            {
                for (var key in senders)
                {
                    //Make sure we don't use the __type internal key
                    if (key.substring(0,1) == globalFunctions.keyPrefix)
                    {
                        //Need to remove the prefix added by the serializer
                        var senderUserId = key.substring(globalFunctions.keyPrefix.length);
                        var unreadCount = senders[key];
                        //See if the message count has changed
                        var loadMessages = true;
                        if (this._unreadMessages[key] && this._unreadMessages[key] == unreadCount)
                        {
                            loadMessages = false;
                        }
                        this._unreadMessages[key] = unreadCount;
                        this.open_message_window(senderUserId, null, loadMessages);
                    }
                }
            }
        },
        handle_unread_messages_failure: function(exception)
        {
            globalFunctions.display_error(null, exception);
        },

        send_new_message: function (userId, evt)
        {
            //Simply open the window for this conversation
            this.open_message_window(userId, evt, true);
        },
        open_message_window: function(userId, evt, loadMessages)
        {
            //See if we already have a dialog box for this user
            var dlgMessenger = this._dialogBoxes[userId];
            var lstMessenger = this._messageLists[userId];
            if (!dlgMessenger || !lstMessenger)
            {
                //Get the name of the user, fail quietly
                var context = {};
                context.userId = userId;
                context.evt = evt;
                Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_GetUserName(userId, Function.createDelegate(this, this.open_message_window_success), Function.createDelegate(this, this.open_message_window_failure), context);
            }
            else
            {
                //Display the existing dialog box, don't re-position.
                dlgMessenger.display_noPositionChange();

                //Load the message list if necessary
                if (loadMessages)
                {
                    lstMessenger.load_data();
                }
            }
        },
        open_message_window_success: function(username, context)
        {
            var userId = context.userId;
            var evt = context.evt;

            //Create a new instance of the dialog box
            var div = document.createElement('div');
            div.className = 'PopupPanel message-popup mobile-tablet-partscreen';
            div.style.display = 'none';
            var dlgMessenger = $create(Inflectra.SpiraTest.Web.ServerControls.DialogBoxPanel, { themeFolder: this._themeFolder, title: username, persistent: true, modal: false, left: 10, top: 10 }, null, null, div);

            //Add an avatar to the dialog box top
            var img = document.createElement('img');
            img.className = 'message-avatar message-user-avatar';
            img.src = this._avatarBaseUrl.replace('{0}', userId);
            dlgMessenger.get_element().appendChild(img);

            //Add the messenger control to the dialog box
            div = document.createElement('div');
            div.className = 'message-list-panel'
            dlgMessenger.get_element().appendChild(div);
            var lstMessages = $create(Inflectra.SpiraTest.Web.ServerControls.Messenger, { autoLoad: false, userId: userId, artifactTypeId: this._artifactTypeId, artifactId: this._artifactId, commentProjectId: this._commentProjectId, commentControlId: this._commentControlId }, { close: Function.createDelegate(dlgMessenger, dlgMessenger.close)  }, null, div);
            lstMessages.set_avatarBaseUrl(this._avatarBaseUrl);
            this._messageLists[userId] = lstMessages;

            //Add the close handlers to the dialog box
            dlgMessenger.add_closed(Function.createDelegate(lstMessages, lstMessages.mark_as_read));
            dlgMessenger.add_closed(Function.createDelegate(this, this.clear_counts));

            //Display the dialog box (autoposition)
            $get('<%=divDialogContainers.ClientID%>').appendChild(dlgMessenger.get_element());
            this._dialogBoxes[userId] = dlgMessenger;
            dlgMessenger.display(evt);

            //Load the existing messages
            lstMessages.load_data();
        },
        open_message_window_failure: function(exception)
        {
            globalFunctions.display_error(null, exception);
        },

        _onWindowFocus: function(evt)
        {
            //Check status immediately (one-time only)
            this.check_status(true);
        },

        push_latest_notification: function (unreadCount)
        {
            //Ignore any failures
            Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_RetrieveLatestUnread(Function.createDelegate(this, this.push_latest_notification_success));
        },
        push_latest_notification_success: function(message)
        {
            if (message)
            {
                this.send_browser_notification(resx.Messenger_NewMessageFrom + ' ' + message.creatorName, message.text, message.primaryKey);
            }
        },
        send_browser_notification: function(title, body, messageId)
        {
            if (this._lastNotifyMessageId != messageId)
            {
                this._lastNotifyMessageId = messageId;
                this._notification = new Notify(title, {
                    body: body,
                    icon: this._themeFolder + "Images/product-" + SpiraContext.ProductType + ".svg",
                    closeOnClick: true
                });
                //Make a beeping sound
                var url = this._themeFolder + 'Audio/message.wav';
                globalFunctions.beep(100, url);

                if (Notify.needsPermission)
                {
                    Notify.requestPermission(Function.createDelegate(this, this.onPermissionGranted), Function.createDelegate(this, this.onPermissionDenied));
                }
                else
                {
                    this._notification.show();
                }
            }
        },
        onPermissionGranted: function()
        {
            console.log('Permission has been granted by the user');
            this._notification.show();
        },
        onPermissionDenied: function()
        {
            console.warn('Permission has been denied by the user');
        }
    };
    var tstucMessageManager = $create(Inflectra.SpiraTest.Web.UserControls.MessageManager);

    //Start the timer to actually check for messages/users
    Sys.Application.add_init(function() { tstucMessageManager.start_polling(); });

</script>