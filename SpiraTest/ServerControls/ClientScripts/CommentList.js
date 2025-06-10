var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.CommentList = function (element)
{
    Inflectra.SpiraTest.Web.ServerControls.CommentList.initializeBase(this, [element]);
    this._webServiceClass = '';
    this._cssClass = '';
    this._projectId = -1;
    this._artifactId = -1;
    this._artifactTypeId = null;
    this._avatarBaseUrl = '';
    this._errorMessageControlId = '';
    this._dataSource = null;
    this._loadingComplete = false;
    this._autoLoad = true;

    this._clickItems = new Array();
    this._clickHandlers = new Array();
    this._sortNewestFirst = null;
    this._sortNewestFirstHandler = null;
    this._sortOldestFirst = null;
    this._sortOldestFirstHandler = null;

    this._sortDirectionAscending = 1;
    this._sortDirectionDescending = 2;

    this._table = null;
    this._tbody = null;
}
Inflectra.SpiraTest.Web.ServerControls.CommentList.prototype =
{
    initialize: function ()
    {
        Inflectra.SpiraTest.Web.ServerControls.CommentList.callBaseMethod(this, 'initialize');

        //Create the shell of the control
        this._createChildControls();

        //Let other controls know we've initialized
        this.raise_init();

        //Now load in the data if autoload set
        if (this._autoLoad)
        {
            this.load_data();
        }

        //If we have an instant messenger manager, register the artifact type/id
        if (typeof (tstucMessageManager) != 'undefined' && tstucMessageManager)
        {
            tstucMessageManager.set_commentProjectId(this._projectId);
            tstucMessageManager.set_artifactId(this._artifactId);
            tstucMessageManager.set_artifactTypeId(this._artifactTypeId);
            tstucMessageManager.set_commentControlId(this.get_element().id);
        }
    },
    dispose: function ()
    {
        //Clear the various handlers
        this._clearClickHandlers();
        $removeHandler(this._sortNewestFirst, 'click', this._sortNewestFirstHandler);
        $removeHandler(this._sortOldestFirst, 'click', this._sortOldestFirstHandler);
        delete this._table;
        delete this._tbody;
        delete this._clickItems;
        delete this._clickHandlers;

        delete this._sortNewestFirst;
        delete this._sortNewestFirstHandler;
        delete this._sortOldestFirst;
        delete this._sortOldestFirstHandler;

        Inflectra.SpiraTest.Web.ServerControls.CommentList.callBaseMethod(this, 'dispose');
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
    set_errorMessageControlId: function (value)
    {
        this._errorMessageControlId = value;
    },

    get_webServiceClass: function ()
    {
        return this._webServiceClass;
    },
    set_webServiceClass: function (value)
    {
        this._webServiceClass = value;
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

    /*  =========================================================
    Event Handlers
    =========================================================  */
    _onSortNewestFirst: function ()
    {
        //Update the direction and then reload the data
        this.get_webServiceClass().Comment_UpdateSortDirection(this.get_projectId(), this._sortDirectionDescending, Function.createDelegate(this, this.update_sort_success), Function.createDelegate(this, this.operation_failure));
    },
    _onSortOldestFirst: function ()
    {
        //Update the direction and then reload the data
        this.get_webServiceClass().Comment_UpdateSortDirection(this.get_projectId(), this._sortDirectionAscending, Function.createDelegate(this, this.update_sort_success), Function.createDelegate(this, this.operation_failure));
    },
    update_sort_success: function ()
    {
        //Reload the data
        this.load_data();
    },
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
        globalFunctions.display_spinner();
        this.get_webServiceClass().Comment_Retrieve(this.get_projectId(), this.get_artifactId(), Function.createDelegate(this, this.retrieve_success), Function.createDelegate(this, this.operation_failure));
    },
    retrieve_success: function (dataSource)
    {
        //Set the datasource
        this.set_dataSource(dataSource);

        //Databind
        this.dataBind();

        //Mark as complete
        this._loadingComplete = true;
        globalFunctions.hide_spinner();
    },
    delete_comment: function (commentId)
    {
        //Delete the commend and then reload the data
        globalFunctions.display_spinner();
        this.get_webServiceClass().Comment_Delete(this.get_projectId(), this.get_artifactId(), commentId, Function.createDelegate(this, this.delete_comment_success), Function.createDelegate(this, this.operation_failure));
    },
    delete_comment_success: function ()
    {
        globalFunctions.hide_spinner();
        this.load_data();
    },

    add_comment: function (textBoxId)
    {
        //The new comment will be in a CKEDITOR instance
        var editor = CKEDITOR.instances[textBoxId];
        var newComment = '';
        if (editor)
        {
            newComment = editor.getData();
        }
        if (newComment && newComment != '')
        {
            globalFunctions.display_spinner();
            var args = { 'textBoxId': textBoxId };
            this.get_webServiceClass().Comment_Add(this.get_projectId(), this.get_artifactId(), newComment, Function.createDelegate(this, this.add_comment_success), Function.createDelegate(this, this.operation_failure), args);
        }
        else
        {
            alert(resx.CommentList_PleaseEnterComment);
        }
    },
    add_comment_success: function (commentId, args)
    {
        globalFunctions.hide_spinner();
        //Remove the old comment
        var editor = CKEDITOR.instances[args.textBoxId];
        if (editor)
        {
            editor.setData('');
        }
        this.load_data();
    },

    operation_failure: function (exception)
    {
        //Populate the error message control if we have one (if not use alert instead)
        globalFunctions.hide_spinner();

        //Display validation exceptions in a friendly manner
        globalFunctions.display_error($get(this.get_errorMessageControlId()), exception);
    },

    dataBind: function ()
    {
        //Clear any existing content
        this._clickItems = new Array();
        this._clickHandlers = new Array();
        globalFunctions.clearContent(this._tbody);

        var dataSource = this.get_dataSource();
        if (dataSource && dataSource.length > 0)
        {
            //Update the sort direction legend
            if (dataSource[0].sortDirection == this._sortDirectionAscending)
            {
                this._sortNewestFirst.className = 'btn btn-default';
                this._sortOldestFirst.className = 'btn btn-default active';
            }
            if (dataSource[0].sortDirection == this._sortDirectionDescending)
            {
                this._sortNewestFirst.className = 'btn btn-default active';
                this._sortOldestFirst.className = 'btn btn-default';
            }

            //Loop throw and create each comment row
            for (var i = 0; i < dataSource.length; i++)
            {
                //Create the row
                var comment = dataSource[i];
                var tr = d.ce('tr');
                $(tr).addClass('commentlist-item');
                this._tbody.appendChild(tr);

                //Create the td for the row
                var td = d.ce('td');
                td.className = 'commentlist-item-wrapper';
                tr.appendChild(td);

                //Add the timestamp information
                var divDate = d.ce('div');
                divDate.className = "commentlist-item-date";
                td.appendChild(divDate);
                divDate.appendChild(d.createTextNode(comment.creationDateText));

                //Add the avatar cell
                var divAvatar = d.ce('div');
                divAvatar.className = 'Avatar commentlist-item-avatar';
                td.appendChild(divAvatar);
                var img = d.ce('img');
                divAvatar.appendChild(img);
                img.src = this._avatarBaseUrl.replace('{0}', comment.creatorId);

                //Create a wrapper div for the name and message
                divComment = d.ce('div');
                divComment.className = 'commentlist-item-message';
                td.appendChild(divComment);

                //Add the user name information
                var spanUser = d.ce('span');
                spanUser.className = "commentlist-item-username";
                divComment.appendChild(spanUser);
                spanUser.appendChild(d.createTextNode(comment.creatorName));

                //Add the close button cell
                divButton = d.ce('div');
                divButton.className = 'Actions commentlist-item-actions';
                divComment.appendChild(divButton);
                if (comment.deleteable) {
                    var closeButton = d.ce('div');
                    closeButton.className = 'Close commentlist-item-close fas fa-times';
                    var handler = Function.createCallback(this.onCommentDelete, { thisRef: this, primaryKey: comment.primaryKey });
                    $addHandler(closeButton, 'click', handler);
                    this._clickItems.push(closeButton);
                    this._clickHandlers.push(handler);
                    divButton.appendChild(closeButton);
                }

                //Add the text of the message
                var divMessage = d.ce('div');
                divMessage.className = 'CommentContainer commentlist-item-content';
                divComment.appendChild(divMessage);

                //The text contains markup so need to use innerHTML and then scrub to prevent XSS
                divMessage.innerHTML = comment.text;
                globalFunctions.cleanHtml(divMessage);

            }
        }
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

    _createChildControls: function ()
    {
        //Create the tbody, since everything else gets generated during databind
        var table = this.get_element();
        this._tbody = d.ce('tbody');
        table.appendChild(this._tbody);

        //Add the sorting legend above the table
        var p = d.ce('div');
        p.className = "Legend";
        table.parentNode.insertBefore(p, table);
        var span = d.ce('span');
        p.appendChild(span);
        span.appendChild(d.createTextNode(resx.Global_Displaying));
        var btnGroup = d.ce('div');
        btnGroup.className = "btn-group ml3";
        p.appendChild(btnGroup);
        this._sortNewestFirst = d.ce('a');
        btnGroup.appendChild(this._sortNewestFirst);
        this._sortNewestFirst.href = 'javascript:void(0)';
        this._sortNewestFirst.appendChild(d.createTextNode(resx.CommentList_NewestFirst));
        this._sortNewestFirst.className = "btn btn-default"
        this._sortOldestFirst = d.ce('a');
        btnGroup.appendChild(this._sortOldestFirst);
        this._sortOldestFirst.href = 'javascript:void(0)';
        this._sortOldestFirst.appendChild(d.createTextNode(resx.CommentList_OldestFirst));
        this._sortOldestFirst.className = "btn btn-default";

        //Add handlers
        this._sortNewestFirstHandler = Function.createDelegate(this, this._onSortNewestFirst);
        $addHandler(this._sortNewestFirst, 'click', this._sortNewestFirstHandler);
        this._sortOldestFirstHandler = Function.createDelegate(this, this._onSortOldestFirst);
        $addHandler(this._sortOldestFirst, 'click', this._sortOldestFirstHandler);
    }
}
Inflectra.SpiraTest.Web.ServerControls.CommentList.registerClass('Inflectra.SpiraTest.Web.ServerControls.CommentList', Sys.UI.Control);
        
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