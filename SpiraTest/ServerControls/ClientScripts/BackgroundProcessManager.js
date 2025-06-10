var resx = Inflectra.SpiraTest.Web.GlobalResources;
var d = document;
d.ce = d.createElement;

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager = function (element)
{
	Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager.initializeBase(this, [element]);
	this._themeFolder = '';
	this._errorMessageControlId = '';
	this._width = 0;
	this._height = 0;
	this._left = 0;
	this._top = 0;
	this._processId = '';
	this._modal = true;
	this._webServiceClass = null;
	this._equalizer = null;
	this._titleBar = null;
	this._innerDiv = null;
	this._messageDiv = null;
	this._background = null;
	this._closeButton = null;
	this._busy = false;
	this._projectId = -1;
	this._parameter1 = null;
	this._parameter2 = null;
	this._autoRedirect = true;
	this._refreshRate = 1;
	this._callSuccessOnError = false;
	this._operation = '';

	//Condition codes
	this._conditionRunning = 1;
	this._conditionCompleted = 2;
	this._conditionError = 3;
	this._conditionWarning = 4;

	//Internal use, whether to call the succeedded function after the user manually clicks close.
	this._callFunction = false;
	this._processStatus = 0;
}

Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager.prototype =
{
	initialize: function ()
	{
		Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager.callBaseMethod(this, 'initialize');

		//Get the pixel width and height of the dialog box
		this._width = this.get_element().style.width.substr(0, this.get_element().style.width.length - 2);
		if (this.get_element().style.height)
		{
			this._height = this.get_element().style.height.substr(0, this.get_element().style.height.length - 2);
		}
		else if (this.get_element().style.minHeight)
		{
			this._height = this.get_element().style.minHeight.substr(0, this.get_element().style.minHeight.length - 2);
		}

		//Create the inner objects

		//The title bar
		this._titleBar = d.ce('div');
		this.get_element().appendChild(this._titleBar);
		this._titleBar.className = 'TitleBar';

		//The body
		this._innerDiv = d.ce('div');
		this.get_element().appendChild(this._innerDiv);
		this._innerDiv.className = 'DialogBody';

		//The messages
		this._messageDiv = d.ce('div');
		this.get_element().appendChild(this._messageDiv);
		this._messageDiv.className = 'DialogBody';

		//The equalizer bar
		this._equalizer = this._createEqualizer(this.get_element(), 0);

		//The Close button
		this._closeButton = d.ce('input');
		this._closeButton.type = 'button';

		this.get_element().appendChild(this._closeButton);
		this._closeButton.className = 'btn btn-primary';
		this._closeButton.value = resx.Global_Close;
		this._closeButton.style.marginTop = '10px';
		this._closeButton.style.marginBottom = '10px';
		this._closeButton.style.marginLeft = 'auto';
		this._closeButton.style.marginRight = 'auto';
		this._closeButton.style.display = 'block';

		//Make button appear inactive
		this._closeButton.style.cursor = 'default';
		this._closeButton.style.filter = 'alpha(opacity=50)'; // IE Only
		this._closeButton.style.opacity = '0.5';  //others
		this._closeButton.style.mozOpacity = '0.5'; //FireFox only

		//Add the event handler to the close button
		this._closeButtonHandler = Function.createDelegate(this, this._onCloseClick);
		$addHandler(this._closeButton, 'click', this._closeButtonHandler);

		//Create a background element if we have the modal flag set
		if (this.get_modal())
		{
			this._background = d.ce('div');
			this.get_element().parentNode.appendChild(this._background);
			this._background.className = 'DialogBoxModalBackground';
			this._background.style.display = 'none';
			this._background.style.position = 'fixed';
			this._background.style.left = '0px';
			this._background.style.top = '0px';
			this._background.style.width = '100%';
			this._background.style.height = '100%';
		}

		//Let everyone know we're ready
		this.raise_init();
	},
	dispose: function ()
	{
		delete this._equalizer;
		delete this._titleBar;
		delete this._innerDiv;
		delete this._messageDiv;

		if (this.get_modal())
		{
			delete this._background;
		}

		//Add custom dispose actions here
		$removeHandler(this._closeButton, 'click', this._closeButtonHandler);
		Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager.callBaseMethod(this, 'dispose');
	},

	/*  =========================================================
	The properties
	=========================================================  */
	get_themeFolder: function ()
	{
		return this._themeFolder;
	},
	set_themeFolder: function (value)
	{
		this._themeFolder = value;
	},

	get_modal: function ()
	{
		return this._modal;
	},
	set_modal: function (value)
	{
		this._modal = value;
	},

	get_projectId: function ()
	{
		return this._projectId;
	},
	set_projectId: function (value)
	{
		this._projectId = value;
	},

	get_left: function ()
	{
		return this._left;
	},
	set_left: function (value)
	{
		this._left = value;
	},

	get_top: function ()
	{
		return this._top;
	},
	set_top: function (value)
	{
		this._top = value;
	},

	get_errorMessageControlId: function ()
	{
		return this._errorMessageControlId;
	},
	set_errorMessageControlId: function (value)
	{
		this._errorMessageControlId = value;
	},

	get_processId: function ()
	{
		return this._processId;
	},
	set_processId: function (value)
	{
		this._processId = value;
	},

	get_webServiceClass: function ()
	{
		return this._webServiceClass;
	},
	set_webServiceClass: function (value)
	{
		this._webServiceClass = value;
	},

	get_parameter1: function ()
	{
		return this._parameter1;
	},
	get_parameter2: function ()
	{
		return this._parameter2;
	},
	get_autoRedirect: function ()
	{
		return this._autoRedirect;
	},
	set_autoRedirect: function (value)
	{
		this._autoRedirect = value;
	},
	get_refreshRate: function ()
	{
		return this._refreshRate;
	},
	set_refreshRate: function (value)
	{
		this._refreshRate = value;
	},
	get_callSuccessOnError: function ()
	{
		return this._callSuccessOnError;
	},
	set_callSuccessOnError: function (value)
	{
		this._callSuccessOnError = value;
	},
	get_operation: function(value)
	{
	    return this._operation;
	},
	/*  =========================================================
	Public methods
	=========================================================  */

	//parameter 1 = integer
	//parameter 2 = array of integers
	//parameter 3 = string
	display: function (projectId, operation, title, description, parameter1, parameter2, parameter3)
	{
		//Mark as busy
		this._busy = true;
		this._closeButton.style.cursor = 'default';
		this._closeButton.style.filter = 'alpha(opacity=50)'; // IE Only
		this._closeButton.style.opacity = '0.5';  //others
		this._closeButton.style.mozOpacity = '0.5'; //FireFox only

		//Store the project id and parameters for use by the calling page
		this._projectId = projectId;
		this._parameter1 = parameter1;
		this._parameter2 = parameter2;
		this._parameter3 = parameter3;

		//Autoposition
		this.auto_position();

		//Set the title
		globalFunctions.clearContent(this._titleBar);
		this._titleBar.appendChild(d.createTextNode(title));

		//Set the body text
		globalFunctions.clearContent(this._innerDiv);
		this._innerDiv.appendChild(d.createTextNode(description));

		//Set the initial message
		globalFunctions.clearContent(this._messageDiv);
		this._messageDiv.appendChild(d.createTextNode(resx.BackgroundProcessManager_StartingProcess));

		//Called if we want to display immediately
		var themeName = this._themeFolder.replace(/App_Themes/, '').replace(/\//g, '');

		//Display the modal background
		if (this.get_modal())
		{
			this._background.style.display = 'block';
		}

		//Make visible and position
		this.get_element().style.left = this._left + 'px';
		this.get_element().style.top = this._top + 'px';
		this.get_element().style.display = 'block';

		//Raise the display event
		this.raise_displayed();

	    //Store the operation name
		this._operation = operation;

		//Now we want to actually initiate the background process
		var webService = this.get_webServiceClass();
		webService.LaunchNewProcess(projectId, operation, parameter1, parameter2, parameter3, Function.createDelegate(this, this.launchProcess_success), Function.createDelegate(this, this.operation_failure));
	},
	launchProcess_success: function (processId)
	{
		//Set the process id
		this.set_processId(processId);

		//Now we need to check the server every second to see if the process is still running
		this.getProcessStatus();
	},

	getProcessStatus: function ()
	{
		//Call the web-service to check on the status of the process
		var webService = this.get_webServiceClass();
		webService.GetProcessStatus(this.get_processId(), Function.createDelegate(this, this.getProcessStatus_success), Function.createDelegate(this, this.operation_failure));
	},
	getProcessStatus_success: function (processStatus)
	{
		//Update progress indicators
		this._updateEqualizer(processStatus.Progress);

		//Update the message if any provided
		if (processStatus.Message && processStatus.Message != '')
		{
			globalFunctions.clearContent(this._messageDiv);
			this._messageDiv.appendChild(d.createTextNode(processStatus.Message));
		}

		//if still running, check back in one second
		if (processStatus.Condition == this._conditionRunning)
		{
			var thisRef = this;
			setTimeout(function () { thisRef.getProcessStatus(); }, (1000 * this._refreshRate));
		}

		if (processStatus.Condition == this._conditionCompleted)
		{
			//If completed successfully, hide after 1 second and then raise success event
			//Make close button active if we're not autoclosing.
			this._busy = false;
			this._processStatus = processStatus.ReturnCode;

			if (this._autoRedirect)
			{
			    //if any information was returned from server in the ReturnMeta field put this in the message to send variable
			    //this message is not used on the client so there are no problems overwriting it
			    //the microsoft code for managing the success function only allows two parameters, so adding Meta as 3rd is not an option
			    var sendReturnMeta = typeof processStatus.ReturnMeta !== "undefined" || processStatus.ReturnMeta !== "",
                    messageToSend = sendReturnMeta ? processStatus.ReturnMeta : resx.BackgroundProcessManager_ProcessCompleted;

			    this.close_delayed(2000);
			    this.raise_succeeded(messageToSend, processStatus.ReturnCode);
			}
			else
			{
				this._closeButton.style.cursor = 'pointer';
				this._closeButton.style.filter = ''; // IE Only
				this._closeButton.style.opacity = '';  //others
				this._closeButton.style.mozOpacity = ''; //FireFox only
			}
			this._callFunction = true;
		}
		else if (processStatus.Condition == this._conditionError)
		{
			//If in error state, leave up and change bar to red, then raise failed event
			//Make close button active
			this._busy = false;
			this._closeButton.style.cursor = 'pointer';
			this._closeButton.style.filter = ''; // IE Only
			this._closeButton.style.opacity = '';  //others
			this._closeButton.style.mozOpacity = ''; //FireFox only
			this._processStatus = processStatus.ReturnCode;

			//Change the bar to red
			var greenBar = this._equalizer.childNodes[0];
			greenBar.className = 'EqualizerRed';
			this.raise_failed(processStatus.Message);
		}
		else if (processStatus.Condition == this._conditionWarning)
		{
			//If completed successfully, hide after 1 second and then raise success event
			//Make close button active
			this._busy = false;
			this._closeButton.style.cursor = 'pointer';
			this._closeButton.style.filter = ''; // IE Only
			this._closeButton.style.opacity = '';  //others
			this._closeButton.style.mozOpacity = ''; //FireFox only
			this._processStatus = processStatus.ReturnCode;

			//Change the bar to red
			var yellBar = this._equalizer.childNodes[0];
			yellBar.className = 'EqualizerYellow';
		}
	},
	close: function ()
	{
		//Close the dialog box
		this.get_element().style.display = 'none';
		if (this.get_modal())
		{
			this._background.style.display = 'none';
		}
	},
	close_delayed: function (delay)
	{
		//Called if we want to close after a short delay
		var thisRef = this;
		this._closeTimeoutId = setTimeout(function () { thisRef.close() }, delay);
	},

	auto_position: function ()
	{
		//Find how wide and high the visible screen is
		var windowWidth = window.innerWidth;
		var windowHeight = window.innerHeight;
		if (document.all)
		{
			//For IE
			windowWidth = globalFunctions.ietruebody().clientWidth;
			windowHeight = globalFunctions.ietruebody().clientHeight;
		}

		//Position at center of visible window and display
		var leftPos = (windowWidth - this._width) / 2;
		var topPos = (windowHeight - this._height) / 2;
		this.set_left(leftPos);
		this.set_top(topPos);
		this.get_element().style.position = 'fixed';
	},
	operation_failure: function (exception)
	{
		//Clear the process id
		this.set_processId('');

		//Populate the error message control if we have one (if not use alert instead)

		//Display validation exceptions in a friendly manner
		var messageBox = document.getElementById(this.get_errorMessageControlId());
		globalFunctions.display_error(messageBox, exception);
	},
	display_error: function (message)
	{
	    globalFunctions.display_error_message($get(this.get_errorMessageControlId()), message);
	},
	display_info: function (message)
	{
	    //If we have a display element, use that otherwise revert to an alert
	    globalFunctions.display_info_message($get(this.get_errorMessageControlId()), message);
	},

	//Create the progress 'equalizer' bar
	_createEqualizer: function (parentElement, progress)
	{
		//Make sure progress is in the expected ranges
		if (progress > 100)
		{
			progress = 100;
		}
		if (progress < 0)
		{
			progress = 0;
		}

		//Populate the equalizer bar
		var div = d.ce('div');
		div.className = 'Equalizer';
		parentElement.appendChild(div);
		span = d.ce('span');
		span.className = 'EqualizerGreen';
		var progressWidth = progress * 3;
		if (typeof parentElement.style.MozUserSelect != 'undefined')
		{
			span.style.paddingLeft = progressWidth + "px";
		}
		else
		{
			span.style.width = progressWidth + "px";
		}
		div.appendChild(span);

		span = d.ce('span');
		span.className = 'EqualizerGray';
		var remainder = 300 - progressWidth;
		if (typeof parentElement.style.MozUserSelect != 'undefined')
		{
			span.style.paddingLeft = remainder + "px";
		}
		else
		{
			span.style.width = remainder + "px";
		}
		div.appendChild(span);

		return div;
	},

	_updateEqualizer: function (progress)
	{
		//Make sure progress is in the expected ranges
		if (progress > 100)
		{
			progress = 100;
		}
		if (progress < 0)
		{
			progress = 0;
		}
		var progressWidth = progress * 3;
		var remainder = 300 - progressWidth;

		//Update the equalizer
		var greenBar = this._equalizer.childNodes[0];
		greenBar.className = 'EqualizerGreen';
		if (typeof greenBar.style.MozUserSelect != 'undefined')
		{
			greenBar.style.paddingLeft = progressWidth + "px";
		}
		else
		{
			greenBar.style.width = progressWidth + "px";
		}
		var grayBar = this._equalizer.childNodes[1];
		if (typeof grayBar.style.MozUserSelect != 'undefined')
		{
			grayBar.style.paddingLeft = remainder + "px";
		}
		else
		{
			grayBar.style.width = remainder + "px";
		}
	},

	/*  =========================================================
	The event handler managers
	=========================================================  */
	add_succeeded: function (handler)
	{
		this.get_events().addHandler('succeeded', handler);
	},
	remove_succeeded: function (handler)
	{
		this.get_events().removeHandler('succeeded', handler);
	},
	raise_succeeded: function (msg, returnCode)
	{
	    var h = this.get_events().getHandler('succeeded');
		if (h) h(msg, returnCode);
	},

	add_failed: function (handler)
	{
		this.get_events().addHandler('failed', handler);
	},
	remove_failed: function (handler)
	{
		this.get_events().removeHandler('failed', handler);
	},
	raise_failed: function (msg)
	{
		var h = this.get_events().getHandler('failed');
		if (h) h(msg);
	},

	add_displayed: function (handler)
	{
		this.get_events().addHandler('displayed', handler);
	},
	remove_displayed: function (handler)
	{
		this.get_events().removeHandler('displayed', handler);
	},
	raise_displayed: function ()
	{
		var h = this.get_events().getHandler('displayed');
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
	Event handlers
	=========================================================  */
	_onCloseClick: function ()
	{
		//Close the dialog box if the status is not pending
		if (!this._busy)
		{
			this.close();
			if (this._callFunction || this._callSuccessOnError)
			{
				this.raise_succeeded(resx.BackgroundProcessManager_ProcessCompleted, this._processStatus);
			}
		}
	}
}
Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager.registerClass('Inflectra.SpiraTest.Web.ServerControls.BackgroundProcessManager', Sys.UI.Control);

//  always end with this goodnight statement        
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();

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
			window.location.href = window.location.protocol + "//" + window.location.host + window.location.pathname.substring(0, window.location.pathname.indexOf('/', 1)) + '/Login.aspx?ReturnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
		}
	}
}  