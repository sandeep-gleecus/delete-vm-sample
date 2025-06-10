var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.Messenger = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.Messenger.initializeBase(this, [element]);
    this._cssClass = '';
    this._projectId = -1;
    this._userId = -1;
    this._avatarBaseUrl = '';
    this._errorMessageControlId = '';
    this._dataSource = null;
    this._loadingComplete = false;
    this._autoLoad = true;

    this._clickItems = new Array();
    this._clickHandlers = new Array();
    this._checkboxes = new Array();

    this._table = null;
    this._tbody = null;
    this._textarea = null;
    this._submitButton = null;
    this._messageBox = null;
    this._scrollDiv = null;

    this._artifactId = null;
    this._artifactTypeId = null;
    this._commentControlId = null;
    this._commentProjectId = null;
}
Inflectra.SpiraTest.Web.ServerControls.Messenger.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.Messenger.callBaseMethod(this, 'initialize');

        //Create the shell of the control
        this._createChildControls();

        //Let other controls know we've initialized
        this.raise_init();

        //Now load in the data if autoload set
        if (this._autoLoad)
        {
            this.load_data();
        }
    },
    dispose: function ()
    {
        //Clear the various handlers
        this._clearClickHandlers();
        this._clearOtherHandlers();
        delete this._scrollDiv;
        delete this._table;
        delete this._tbody;
        delete this._textarea;
        delete this._submitButton;
        delete this._clickItems;
        delete this._clickHandlers;
        delete this._messageBox;

        Inflectra.SpiraTest.Web.ServerControls.Messenger.callBaseMethod(this, 'dispose');
    },

    /*  =========================================================
    The properties
    =========================================================  */
    get_element: function ()
    {
        return this._element;
    },
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

    get_userId: function ()
    {
        return this._userId;
    },
    set_userId: function (value)
    {
        this._userId = value;
    },

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

    get_avatarBaseUrl: function ()
    {
        return this._avatarBaseUrl;
    },
    set_avatarBaseUrl: function (value)
    {
        this._avatarBaseUrl = value;
    },

    get_errorMessageControlId: function ()
    {
        return this._errorMessageControlId;
    },

    get_autoLoad: function ()
    {
        return this._autoLoad;
    },
    set_autoLoad: function (value)
    {
        this._autoLoad = value;
    },
    get_loadingComplete: function ()
    {
        return this._loadingComplete;
    },
    clear_loadingComplete: function ()
    {
        this._loadingComplete = false;
    },

    get_dataSource: function ()
    {
        return this._dataSource;
    },
    set_dataSource: function (value)
    {
        this._dataSource = value;
    },

    /*  =========================================================
    The event handler managers
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
    },
    add_close: function (handler) {
        this.get_events().addHandler('close', handler);
    },
    remove_close: function (handler) {
        this.get_events().removeHandler('close', handler);
    },
    raise_close: function () {
        var h = this.get_events().getHandler('close');
        if (h) h();
    },

    /*  =========================================================
    Event Handlers
    =========================================================  */
    onCommentDelete: function (evt, c)
    {
        //Confirm with the user
        if (confirm(resx.CommentList_ConfirmDelete))
        {
            //Delete the item
            var commentId = c.primaryKey;
            c.thisRef.delete_comment(c.primaryKey);
        }
    },
    onTextAreaKeyDown: function (evt)
    {
        //If enter was clicked and shift not held down, fire button submit
        var keynum = evt.keyCode | evt.which;
        if (keynum == 13 && !evt.shiftKey)
        {
            evt.cancelBubble = true;
            evt.stopPropagation();
            evt.preventDefault();
            this.post_message();
        }
        //If ESC pressed, close messenger
        if (keynum == 27)
        {
            evt.cancelBubble = true;
            evt.stopPropagation();
            evt.preventDefault();
            this.raise_close();
        }
    },
    onSubmitClick: function (evt)
    {
        this.post_message();
    },
    onPostCommentsClick: function (evt)
    {
        this.post_artifact_comments();
    },
    onPostCommentCheckboxClick: function()
    {
        this.check_comments_status();
    },

    /*  =========================================================
    Methods/functions
    =========================================================  */
    load_data: function (dontClearMessages)
    {
        //Clear any existing error messages
        if (!dontClearMessages)
        {
            globalFunctions.clear_errors($get(this.get_errorMessageControlId()));
        }

        //Get the list of comments
        Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Comment_Retrieve(this.get_projectId(), this.get_userId(), Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
    },
    retrieve_success: function (dataSource)
    {
        //Set the datasource
        this.set_dataSource(dataSource);

        //Databind
        this.dataBind();

        //Mark as complete
        this._loadingComplete = true;
    },
    delete_comment: function (commentId)
    {
        //Delete the message and then reload the data
        Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Comment_Delete(this.get_projectId(), this.get_userId(), commentId, Function.createDelegate(this, this.delete_comment_success), Function.createDelegate(this, this.operation_failure));
    },
    delete_comment_success: function ()
    {
        this.load_data();
    },

    operation_failure: function (exception)
    {
        //Display validation exceptions in a friendly manner
        globalFunctions.display_error($get(this._errorMessageControlId), exception);
    },

    post_message: function ()
    {
        //Get the user id and message to post
        var message = this._textarea.value;
        var recipientUserId = this._userId;

        //Check text length
        if (message.length > 255)
        {
            message = message.substr(0, 255);
        }

        //Disable the button to avoid double-submissions
        this._submitButton.style.opacity = '0.5'; // IE Only
        this._submitButton.style.mozOpacity = '0.5'; //FireFox only
        this._submitButton.disabled = true;

        //Post the new message
        Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_PostNew(recipientUserId, message, Function.createDelegate(this, this.post_message_success), Function.createDelegate(this, this.post_message_failure));
    },
    post_message_success: function ()
    {
        //Re-enable button
        this._submitButton.style.opacity = ''; // IE Only
        this._submitButton.style.mozOpacity = ''; //FireFox only
        this._submitButton.disabled = false;

        //Blank out the message
        this._textarea.value = '';

        //Reload the message list
        this.load_data();
    },
    post_message_failure: function (exception)
    {
        //Re-enable button
        this._submitButton.style.opacity = ''; // IE Only
        this._submitButton.style.mozOpacity = ''; //FireFox only
        this._submitButton.disabled = false;

        globalFunctions.display_error($get(this._errorMessageControlId), exception);
    },

    //Posts the selected checkboxes as artifact comments
    post_artifact_comments: function ()
    {
        //Get the list of selected checkboxes
        var messageIds = new Array();
        for (var i = 0; i < this._checkboxes.length; i++)
        {
            if (this._checkboxes[i].checked)
            {
                var messageId = this._checkboxes[i].getAttribute('tst:messageid');
                if (messageId && messageId != '')
                {
                    messageIds.push(messageId);
                }
            }
        }

        //Make sure we have at least one checkbox selected
        if (messageIds.length < 1)
        {
            alert(resx.Messenger_NeedToSelectComment);
            return;
        }

        //Post the comments
        var context = {};
        context.commentControlId = this._commentControlId;
        Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_PostArtifactComments(this._commentProjectId, this._artifactTypeId, this._artifactId, messageIds, Function.createDelegate(this, this.post_artifact_comments_success), Function.createDelegate(this, this.post_artifact_comments_failure), context);
    },
    post_artifact_comments_success: function (data, context)
    {
        //Display success
        globalFunctions.display_info_message($get(this._errorMessageControlId), resx.Messenger_PostAsCommentsSuccess);

        //Reload the comments list
        var commentsControl = $find(context.commentControlId);
        if (commentsControl && commentsControl.load_data)
        {
            commentsControl.load_data();
        }
    },
    post_artifact_comments_failure: function (exception)
    {
        globalFunctions.display_error($get(this._errorMessageControlId), exception);
    },

    //Marks all the messages from the other person as read
    mark_as_read: function ()
    {
        var recipientUserId = this._userId;
        Inflectra.SpiraTest.Web.Services.Ajax.MessageService.Message_MarkAllAsRead(recipientUserId, Function.createDelegate(this, this.mark_as_read_success), Function.createDelegate(this, this.mark_as_read_failure));
    },
    mark_as_read_success: function ()
    {
        //Do nothing
    },
    mark_as_read_failure: function (exception)
    {
        //display as alert because dialog will have closed
        globalFunctions.display_error(null, exception);
    },
    check_comments_status: function()
    {
        var n = 0;
        for (var i = 0; i < this._checkboxes.length; i++) {
            if (this._checkboxes[i].checked) {
                n++;
            }
        }
        if (n > 0)
        {
            $('.MessageLink').removeClass('disabled');
        }
        else
        {
            $('.MessageLink').addClass('disabled');
        }
    },

    dataBind: function ()
    {
        //Clear any existing content
        this._clickItems = new Array();
        this._clickHandlers = new Array();
        this._checkboxes = new Array();
        globalFunctions.clearContent(this._tbody);

        var dataSource = this.get_dataSource();
        if (dataSource && dataSource.length > 0)
        {
            //Loop throw and create each comment row
            for (var i = 0; i < dataSource.length; i++)
            {
                //Create the row
                var comment = dataSource[i];
                var tr = d.ce('tr');
                this._tbody.appendChild(tr);

                //Mark message row with the correct classes - if unread, from logged in user or other, if message is from same user as preceding, or last by user in a row
                $(tr).addClass('messagelist-item');

                if (comment.isUnread)
                {
                    $(tr).addClass('Unread');
                }
                if ( (i > 0) && (comment.creatorId === dataSource[i-1].creatorId) )
                {
                    $(tr).addClass('messagelist-item-as-above');
                }
                if (i + 1 < dataSource.length)
                {
                    if (comment.creatorId !== dataSource[i + 1].creatorId)
                    {
                    $(tr).addClass('messagelist-item-last-by-user');
                    }
                }
                if (comment.creatorId == this._userId)
                {
                    $(tr).addClass('messagelist-item-other');
                }
                else
                {
                    $(tr).addClass('messagelist-item-self');
                }

                //Create the td for the row
                var td = d.ce('td');
                td.className = 'messagelist-item-wrapper';
                tr.appendChild(td);

                //Add the timestamp information
                var divDate = d.ce('div');
                divDate.className = "messagelist-item-date";
                td.appendChild(divDate);
                divDate.appendChild(d.createTextNode(comment.creationDateText));

                //Add the avatar cell
                var divAvatar = d.ce('div');
                divAvatar.className = 'Avatar messagelist-item-avatar';
                td.appendChild(divAvatar);
                var img = d.ce('img');
                divAvatar.appendChild(img);
                img.src = this._avatarBaseUrl.replace('{0}', comment.creatorId);

                //Create a wrapper div for the name and message
                divComment = d.ce('div');
                divComment.className = 'messagelist-item-message';
                td.appendChild(divComment);

                //Add the user name information
                var spanUser = d.ce('span');
                spanUser.className = "messagelist-item-username";
                divComment.appendChild(spanUser);
                spanUser.appendChild(d.createTextNode(comment.creatorName));

                //Add the close button cell
                divButton = d.ce('div');
                divButton.className = 'Actions messagelist-item-actions';
                divComment.appendChild(divButton);
                if (comment.deleteable)
                {
                    var closeButton = d.ce('div');
                    closeButton.className = 'Close messagelist-item-close fas fa-times';
                    var handler = Function.createCallback(this.onCommentDelete, { thisRef: this, primaryKey: comment.primaryKey });
                    $addHandler(closeButton, 'click', handler);
                    this._clickItems.push(closeButton);
                    this._clickHandlers.push(handler);
                    divButton.appendChild(closeButton);
                }

                //Add the checkbox, if appropriate
                if (this._artifactTypeId && this._artifactId && this._commentControlId && this._commentProjectId)
                {
                    var checkbox = d.ce('input');
                    checkbox.type = 'checkbox';
                    checkbox.className = "messagelist-item-checkbox";
                    checkbox.id = "item-checkbox" + comment.primaryKey;
                    checkbox.setAttribute('tst:messageid', comment.primaryKey);
                    this._checkboxes.push(checkbox);
                    divButton.appendChild(checkbox);
                    var checkboxLabel = d.ce('label');
                    checkboxLabel.className = "messagelist-item-checkbox-label";
                    checkboxLabel.setAttribute('for', ("item-checkbox" + comment.primaryKey));
                    divButton.appendChild(checkboxLabel);

                    //add a handler to the checkbox labl
                    var handler = Function.createDelegate(this, this.onPostCommentCheckboxClick);
                    $(checkbox).on("change", handler);
                }

                //Add the text of the message
                var divMessage = d.ce('div');
                divMessage.className = 'MessageContainer messagelist-item-content';
                divComment.appendChild(divMessage);

                //The text contains markup so need to use innerHTML and then scrub to prevent XSS
                divMessage.innerHTML = comment.text;
                globalFunctions.cleanHtml(divMessage);
            }
        }

        //Scroll the div to the bottom
        this._scrollDiv.scrollTop = this._scrollDiv.scrollHeight;
    },

    _clearClickHandlers: function ()
    {
        for (var i = 0; i < this._clickItems.length; i++)
        {
            $removeHandler(this._clickItems[i], 'click', this._clickHandlers[i]);
            delete this._clickItems[i];
            delete this._clickHandlers[i];
        }
    },

    _clearOtherHandlers: function ()
    {
        $removeHandler(this._textarea, 'keydown', this._textAreaKeyDownHandler);
        delete this._textAreaKeyDownHandler;
    },

    _createChildControls: function ()
    {
        //Create the message box using jQuery and store its id
        this._errorMessageControlId = this.get_element().id + '_MessageBox';
        this._messageBox = $('<div id="' + this._errorMessageControlId + '" class="alert alert-hidden"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button><span id="' + this._errorMessageControlId + '_text"></span></div>')[0];
        this.get_element().appendChild(this._messageBox);

        //Create the table and tbody (inside a div)
        this._scrollDiv = d.ce('div');
        this._scrollDiv.className = 'MessageListContainer';
        this.get_element().appendChild(this._scrollDiv);
        this._table = d.ce('table');
        this._table.className = 'MessageList';
        this._scrollDiv.appendChild(this._table);
        this._tbody = d.ce('tbody');
        this._table.appendChild(this._tbody);

        //Now we need to add the new message textbox
        this._textarea = d.ce('textarea');
        this._textarea.className = 'text-box message-text-box';
        this._textarea.maxLength = 255;
        this.get_element().appendChild(this._textarea);

        //Add keypress handler (to catch ENTER)
        this._textAreaKeyDownHandler = Function.createDelegate(this, this.onTextAreaKeyDown);
        $addHandler(this._textarea, 'keydown', this._textAreaKeyDownHandler);
        var div = d.ce('div');
        div.style.width = '100%';
        div.style.textAlign = 'right';
        this.get_element().appendChild(div);

        //The Post As Comments
        if (this._artifactTypeId && this._artifactId && this._commentControlId && this._commentProjectId)
        {
            var a = d.ce('a');
            a.href = 'javascript:void(0)';
            a.className = 'MessageLink btn btn-default disabled';
            a.appendChild(d.createTextNode(resx.Messenger_PostAsComments));
            div.appendChild(a);

            //Add handler to button
            var handler = Function.createDelegate(this, this.onPostCommentsClick);
            $addHandler(a, 'click', handler);
            this._clickItems.push(a);
            this._clickHandlers.push(handler);
        }

        //The send message button
        this._submitButton = d.ce('input');
        this._submitButton.type = 'button';
        this._submitButton.className = 'MessageSend btn btn-primary';
        this._submitButton.value = resx.Messenger_SendMessage;
        div.appendChild(this._submitButton);

        //Add handler to button
        var handler = Function.createDelegate(this, this.onSubmitClick);
        $addHandler(this._submitButton, 'click', handler);
        this._clickItems.push(this._submitButton);
        this._clickHandlers.push(handler);
    }
}
Inflectra.SpiraTest.Web.ServerControls.Messenger.registerClass('Inflectra.SpiraTest.Web.ServerControls.Messenger', Sys.UI.Control);
        
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
