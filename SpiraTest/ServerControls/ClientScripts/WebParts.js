Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls.WebParts');
Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart = function Inflectra$SpiraTest$Web$ServerControls$WebParts$WebPart(associatedElement)
{
    Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart.initializeBase(this, [associatedElement]);

    //Member variables
    var _titleElement;
    var _menuElement;
    var _menuLabelElement;
    var _zone;
    var _zoneIndex;
    var _allowZoneChange = true;
    var _popup;

    //Delegates
    var _mouseDownHandler = null;
    var _Menu_OnClick = null;
    var _Menu_OnKeyPress = null;
    var _Menu_OnMouseEnter = null;
    var _Menu_OnMouseLeave = null;

    //Properties
    this.get_allowZoneChange = function this$get_allowZoneChange()
    {
        return _allowZoneChange;
    }
    this.set_allowZoneChange = function this$set_allowZoneChange(value)
    {
        _allowZoneChange = value;
    }

    this.get_titleElement = function this$get_titleElement()
    {
        return _titleElement;
    }
    this.set_titleElement = function this$set_titleElement(value)
    {
        _titleElement = value;
    }

    this.get_menuLabelElement = function this$get_menuLabelElement()
    {
        return _menuLabelElement;
    }
    this.set_menuLabelElement = function this$set_menuLabelElement(value)
    {
        _menuLabelElement = value;
    }

    this.get_zone = function this$get_zone()
    {
        return _zone;
    }

    this.set_zone = function this$set_zone(value)
    {
        //When we set the zones, also add it to the web parts collection stored in the zone
        _zone = value;
        _zone.add_webPart(this);
    }

    this.get_zoneIndex = function this$get_zoneIndex()
    {
        return _zoneIndex;
    }

    this.set_zoneIndex = function this$set_zoneIndex(value)
    {
        _zoneIndex = value;
    }

    this.dispose = function this$dispose()
    {
        Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart.callBaseMethod(this, "dispose");

        //Remove event handlers
        if (_titleElement && _zone)
        {
            if (_zone.get_webPartManager().get_allowPageDesign() && _zone.get_allowLayoutChange())
            {
                if (_menuLabelElement)
                {
                    $removeHandler(_menuLabelElement, 'click', _Menu_OnClick);
                    $removeHandler(_menuLabelElement, 'keypress', _Menu_OnKeyPress);
                }
                $removeHandler(_titleElement, "mousedown", _mouseDownHandler);

                _menuLabelElement = null;
                _menuDropDownElement = null;
                _menuElement = null;
            }
        }
    }

    this.initialize = function this$initialize()
    {
        Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart.callBaseMethod(this, "initialize");

        var element = this.get_element();

        // _titleElement will be null if a WebPart has EffectiveChromeType of None or BorderOnly.
        if (_titleElement && _zone.get_webPartManager().get_allowPageDesign() && _zone.get_allowLayoutChange())
        {
            _menuLabelElement = document.getElementById(element.id + 'verbs');
            _menuDropDownElement = document.getElementById(element.id + 'verbsPopup');
            _menuElement = document.getElementById(element.id + 'verbsMenu');

            // Attach event handlers to title element
            _titleElement.style.cursor = "move";
            _mouseDownHandler = Function.createDelegate(this, this.mouseDownHandler);
            $addHandler(_titleElement, "mousedown", _mouseDownHandler);

            //Attach event handlers to verbs menu
            if (_menuLabelElement)
            {
                _Menu_OnClick = Function.createDelegate(this, this.Menu_OnClick);
                _Menu_OnKeyPress = Function.createDelegate(this, this.Menu_OnKeyPress);

                $addHandler(_menuLabelElement, 'click', _Menu_OnClick);
                $addHandler(_menuLabelElement, 'keypress', _Menu_OnKeyPress);
            }
        }
    }
    this.Menu_Show = function this$Menu_Show()
    {
        var webPartManager = _zone.get_webPartManager();
        if ((typeof (webPartManager.menu) != "undefined") && (webPartManager.menu != null))
        {
            //If we clicked on the one that's already open then don't re-display it
            if (webPartManager.menu == this)
            {
                webPartManager.menu.Menu_Hide();
                return;
            }
            else
            {
                webPartManager.menu.Menu_Hide();
            }
        }

        _popup = document.createElement('div');
        _popup.style.position = 'absolute';
        _popup.className = _zone.get_menuPopupCssClass();
        webPartManager.menu = this;
        _popup.innerHTML = _menuElement.innerHTML;
        _popup.style.display = 'block';
        _menuLabelElement.appendChild(_popup);
    }
    this.Menu_Hide = function this$Menu_Hide()
    {
        var webPartManager = _zone.get_webPartManager();

        if (webPartManager.menu == this)
        {
            webPartManager.menu = null;
            if ((typeof (_popup) != "undefined") && (_popup != null))
            {
                _menuLabelElement.removeChild(_popup);
                _popup = null;
            }
        }
    }
    this.Menu_OnClick = function this$Menu_OnClick(currentEvent)
    {
        var srcElement = currentEvent.srcElement ? currentEvent.srcElement : currentEvent.target;
        currentEvent.returnValue = false;
        currentEvent.cancelBubble = true;
        this.Menu_Show();
    }
    this.Menu_OnKeyPress = function this$Menu_OnKeyPress(currentEvent)
    {
        //Enter pressed
        if (currentEvent.keyCode == 13)
        {
            currentEvent.returnValue = false;
            currentEvent.cancelBubble = true;
            this.Menu_Show();
        }

        //Escape pressed
        if (currentEvent.keyCode == 27)
        {
            currentEvent.returnValue = false;
            currentEvent.cancelBubble = true;
            this.Menu_Hide();
        }
    }

    this.UpdatePosition = function this$UpdatePosition()
    {
        var webPartElement = this.get_element();
        var location = _zone.translateOffset(0, 0, webPartElement, null, false);
        this.middleX = location.x + webPartElement.offsetWidth / 2;
        this.middleY = location.y + webPartElement.offsetHeight / 2;
    }

    this.mouseDownHandler = function this$mouseDownHandler(domEvent)
    {
        var webPartManager = _zone.get_webPartManager();
        window._event = domEvent;
        //Close the open menu
        if ((typeof (webPartManager.menu) != "undefined") && (webPartManager.menu != null))
        {
            webPartManager.menu.Menu_Hide();
        }

        //Start the drag
        _zone.startDragDrop(this);
        // Prevents browser from selecting the document while dragging
        domEvent.preventDefault();
    }
}
Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart.descriptor =
{
    properties: [ {name: 'titleElement', isDomElement: true},
                  {name: 'zone', type: Object},
                  {name: 'zoneIndex', type: Number},
                  {name: 'allowZoneChange', type: Boolean} ]
}

Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart.registerClass("Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart", Sys.UI.Control);

Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager = function Inflectra$SpiraTest$Web$ServerControls$WebParts$WebPartManager(associatedElement)
{
    Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager.initializeBase(this, [associatedElement]);

    //Member variables
    var _allowPageDesign;
    var _zones = new Array();

    //Needed to make the old WebParts.js code work
    __wpm = this;

    //Properties
    this.get_zones = function this$get_zones()
    {
        return _zones;
    }
    this.add_zone = function this$add_zone(zone)
    {
        _zones.push(zone);
    }
    this.get_allowPageDesign = function this$get_allowPageDesign()
    {
        return _allowPageDesign;
    }

    this.set_allowPageDesign = function this$set_allowPageDesign(value)
    {
        _allowPageDesign = value;
    }

    //Methods
    this.initialize = function this$initialize()
    {
        Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager.callBaseMethod(this, "initialize");
    }

    this.SubmitPage = function this$SubmitPage(eventTarget, eventArgument)
    {
        if ((typeof (this.menu) != "undefined") && (this.menu != null))
        {
            this.menu.Menu_Hide();
        }
        __doPostBack(eventTarget, eventArgument);
    }

    this.UpdatePositions = function this$UpdatePositions()
    {
        var zones = this.get_zones();
        for (var i = 0; i < zones.length; i++)
        {
            zones[i].UpdatePosition();
        }
    }

    this.ShowHelp = function this$ShowHelp(helpUrl, helpMode)
    {
        if (helpMode == 0 && !window.showModalDialog)
        {
            helpMode = 1;
        }

        if ((typeof (this.menu) != "undefined") && (this.menu != null))
        {
            this.menu.Menu_Hide();
        }
        if (helpMode == 0 || helpMode == 1)
        {
            if (helpMode == 0)
            {
                var dialogInfo = "edge: Sunken; center: yes; help: no; resizable: yes; status: no";
                window.showModalDialog(helpUrl, null, dialogInfo);
            }
            else
            {
                window.open(helpUrl, null, "scrollbars=yes,resizable=yes,status=no,toolbar=no,menubar=no,location=no");
            }
        }
        else if (helpMode == 2)
        {
            window.location = helpUrl;
        }
    }
}

Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager.descriptor = {
    properties: [ {name: 'allowPageDesign', type: Boolean} ]
}
Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager.registerClass("Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartManager", Sys.UI.Control);

Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone = function Inflectra$SpiraTest$Web$ServerControls$WebParts$WebPartZone(associatedElement) {
    Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone.initializeBase(this, [associatedElement]);

    // Constants
    var _dataType = "WebPart";

    //Member variables
    var _allowLayoutChange = true;
    var _uniqueId;
    var _webPartManager;
    var _webPartTable;
    var _webPartTableLeft;
    var _webPartTableTop;
    var _webPartTableRight;
    var _webPartTableBottom;
    var _dropIndex = -1;
    var _dropTargetRegistered = false;
    var _floatContainer;
    var _webParts = new Array();
    var _isVertical;
    var _highlightColor;
    var _dragZoneColor;
    var _dropCueElements = new Array();
    var _menuPopupCssClass = '';
       
    //Properties
    this.get_webParts = function this$get_webParts()
    {
        return _webParts;
    }
    this.add_webPart = function this$add_webParts(webPart)
    {
        _webParts.push(webPart);
    }
    this.get_webPartTable = function this$get_webPartTable()
    {
        return _webPartTable;
    }

    this.get_allowLayoutChange = function this$get_allowLayoutChange()
    {
        return _allowLayoutChange;
    }
    this.set_allowLayoutChange = function this$set_allowLayoutChange(value)
    {
        _allowLayoutChange = value;
    }
    
    this.get_menuPopupCssClass = function this$get_menuPopupCssClass()
    {
        return _menuPopupCssClass;
    }
    this.set_menuPopupCssClass = function this$set_menuPopupCssClass(value)
    {
        _menuPopupCssClass = value;
    }
    
    this.get_highlightColor = function this$get_highlightColor()
    {
        return _highlightColor;
    }
    this.set_highlightColor = function this$set_highlightColor(value)
    {
        _highlightColor = value;
    }

    this.get_dragZoneColor = function this$get_dragZoneColor()
    {
        return _dragZoneColor;
    }
    this.set_dragZoneColor = function this$set_dragZoneColor(value)
    {
        _dragZoneColor = value;
    }


    this.get_isVertical = function this$get_isVertical()
    {
        return _isVertical;
    }
    this.set_isVertical = function this$set_isVertical(value)
    {
        _isVertical = value;
    }


    this.get_uniqueId = function this$get_uniqueId()
    {
        return _uniqueId;
    }

    this.set_uniqueId = function this$set_uniqueId(value)
    {
        _uniqueId = value;
    }

    this.get_webPartManager = function this$get_webPartManager()
    {
        return _webPartManager;
    }

    this.set_webPartManager = function this$set_webPartManager(value)
    {
        //When we set the manager, also add it to the manager collection
        _webPartManager = value;
        _webPartManager.add_zone(this);
    }
    
    function Point(x, y)
    {
        this.x = x;
        this.y = y;
    }
    this.translateOffset = function this$translateOffset(x, y, offsetElement, relativeToElement, includeScroll)
    {
        while ((typeof(offsetElement) != "undefined") && (offsetElement != null) && (offsetElement != relativeToElement))
        {
            x += offsetElement.offsetLeft;
            y += offsetElement.offsetTop;
            var tagName = offsetElement.tagName;
            if (tagName != "TABLE" && tagName != "BODY" && offsetElement.clientLeft && offsetElement.clientTop)
            {
                x += offsetElement.clientLeft;
                y += offsetElement.clientTop;
            }
            if (includeScroll && (tagName != "BODY"))
            {
                x -= offsetElement.scrollLeft;
                y -= offsetElement.scrollTop;
            }
            offsetElement = offsetElement.offsetParent;
        }
        return new Point(x, y);
    }

    this.getPageEventLocation = function this$getPageEventLocation(event, includeScroll)
    {
        if ((typeof(event) == "undefined") || (event == null))
        {
            event = window.event;
        }
        var srcElement = event.srcElement ? event.srcElement : event.target;
        return this.translateOffset(event.offsetX, event.offsetY, srcElement, null, includeScroll);
    }

    function createFloatContainer(webPart)
    {
        // Mozilla does not allow setting styles in string used to create element
        var floatContainer = document.createElement("div");
        floatContainer.style.filter = "progid:DXImageTransform.Microsoft.BasicImage(opacity=0.75);";
        floatContainer.style.opacity = '0.75';  //Others
        floatContainer.style.mozOpacity = '0.75';   //Firefox
        floatContainer.style.position = "absolute";
        floatContainer.style.zIndex = 32000;

        var webPartElement = webPart.get_element();
        var currentLocation = Sys.UI.DomElement.getLocation(webPartElement);
        Sys.UI.DomElement.setLocation(floatContainer, currentLocation.x, currentLocation.y);
        floatContainer.style.display = "block";

        // Mozilla needs "px" in the width and height
        floatContainer.style.width = webPartElement.offsetWidth + "px";
        floatContainer.style.height = webPartElement.offsetHeight + "px";
        floatContainer.appendChild(webPartElement.cloneNode(true));

        return floatContainer;
    }
    
    this.dispose = function this$dispose()
    {
        Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone.callBaseMethod(this, "dispose");
        if (_dropTargetRegistered)
        {
            Inflectra.SpiraTest.Web.ServerControls.DragDropManager.unregisterDropTarget(this);
        }
    }

    this.initialize = function this$initialize()
    {
        Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone.callBaseMethod(this, "initialize");
        var element = this.get_element();
               
        //Need to set _webPartTable
        var webPartTableContainer;
        if (element.rows.length == 1)
        {
            webPartTableContainer = element.rows[0].cells[0];
        }
        else
        {
            webPartTableContainer = element.rows[1].cells[0];
        }
        var i;
        for (i = 0; i < webPartTableContainer.childNodes.length; i++)
        {
            var node = webPartTableContainer.childNodes[i];
            if (node.tagName == "TABLE")
            {
                _webPartTable = node;
                break;
            }
        }
            
        if (_webPartTable != null)
        {
            _webPartTable.style.borderColor = _dragZoneColor;
            if (_isVertical)
            {
                for (i = 0; i < _webPartTable.rows.length; i += 2)
                {
                    _dropCueElements[i / 2] = _webPartTable.rows[i].cells[0].childNodes[0];
                }
            }
            else
            {
                for (i = 0; i < _webPartTable.rows[0].cells.length; i += 2)
                {
                    _dropCueElements[i / 2] = _webPartTable.rows[0].cells[i].childNodes[0];
                }
            }
        }
        
        //Register the table with the drag and drop handler
        if (_webPartManager.get_allowPageDesign() && _allowLayoutChange)
        {
            Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);
            _dropTargetRegistered = true;
        }
    }
    
    this.startDragDrop = function this$startDragDrop(webPart)
    {
        // Need to update the positions of the Zones and WebParts, so the drop indexes will be correct
        _webPartManager.UpdatePositions();

        _floatContainer = createFloatContainer(webPart);
        document.body.appendChild(_floatContainer);
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(this, _floatContainer, webPart);
    }

    // IDragSource implementation
    // Type get_dataType()
    this.get_dragDataType = function this$get_dragDataType()
    {
        return _dataType;
    }

    // Object get_data(Context)
    this.getDragData = function this$getDragData(context)
    {
        return context;
    }

    // DragMode get_dragMode()
    this.get_dragMode = function this$get_dragMode()
    {
        return Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy;
    }

    // void onDragStart()
    this.onDragStart = function this$onDragStart()
    {
    }

    // void onDrag()
    this.onDrag = function this$onDrag()
    {
    }

    // void onDragEnd(Cancelled)
    this.onDragEnd = function this$onDragEnd(cancelled)
    {
        // Assumes that the floatContainer is currently parented to document.body.
        // Stated another way, this method assumes that startDragDrop() was called
        // before this method.
        //Sys.Debug.assert(_floatContainer != null, "_floatContainer is null");
        //Sys.Debug.assert(_floatContainer.parentNode == document.body, "_floatContainer is not parented to document.body");

        document.body.removeChild(_floatContainer);
    }

    // IDragTarget implementation
    this.get_dropTargetElement = function this$get_dropTargetElement()
    {
        return this.get_webPartTable();
    }

    // bool canDrop(DragMode, DataType, Data)
    this.canDrop = function this$canDrop(dragMode, dataType, data)
    {
        var webPart = data;
        // In IEDragDropManager, the default radius is 10px, which means the zone will be selected
        // as a drop target if the mouse is 10px away from the webPartTable.  We only want to allow
        // dropping if the mouse is actually inside the webPartTable.

        return ((dragMode == Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy) &&
                (dataType == _dataType) &&
                (Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPart.isInstanceOfType(webPart)) &&
                (webPart.get_allowZoneChange() || (webPart.get_zone() == this)) &&
                (getDropIndex(this) != -1));
    }

    // void drop(DragMode, DataType, Data)
    this.drop = function this$drop(dragMode, dataType, data)
    {
        // IE has a bug, where window.event.offsetX/Y changes by 1px between the call to canDrop()
        // and the call to drop().  Because of this bug, drop() cannot calculate the dropIndex
        // itself -- it must rely on WebPartZone._dropIndex being set before it is called.
        //Sys.Debug.assert(_dropIndex != -1);

        // drop() should hide the drop cues, so they are not visible while the browser
        // is posting back, and also if the WebPart has not moved.
        this.ToggleDropCues(false, _dropIndex, false);

        var webPart = data;
        if (webPartMoved(webPart, this, _dropIndex))
        {
            var eventTarget = _uniqueId;
            var eventArgument = "Drag:" + webPart.get_id() + ":" + _dropIndex;
            __doPostBack(eventTarget, eventArgument);
        }
    }

    function webPartMoved(webPart, dropZone, dropIndex)
    {
        if (dropZone != webPart.get_zone())
        {
            return true;
        }

        // If the dropCue is immediately before or after the current WebPart, it is
        // not considered to have moved.
        if (dropIndex == webPart.get_zoneIndex() || dropIndex == (webPart.get_zoneIndex() + 1))
        {
            return false;
        }

        return true;
    }

    // void onDragEnterTarget(DragMode, DataType, Data)
    this.onDragEnterTarget = function this$onDragEnterTarget(dragMode, dataType, data)
    {
        var dropIndex = getDropIndex(this);
        this.ToggleDropCues(true, dropIndex, false);
        _dropIndex = dropIndex;
    }

    // void onDragLeaveTarget(DragMode, DataType, Data)
    this.onDragLeaveTarget = function this$onDragLeaveTarget(dragMode, dataType, data)
    {
        this.ToggleDropCues(false, _dropIndex, false);
    }

    // void onDragInTarget(DragMode, DataType, Data)
    this.onDragInTarget = function this$onDragInTarget()
    {
        var dropIndex = getDropIndex(this);
        if (dropIndex != _dropIndex)
        {
            this.ToggleDropCues(false, _dropIndex, true);
            this.ToggleDropCues(true, dropIndex, true);
            _dropIndex = dropIndex;
        }
    }
    
    this.ToggleDropCues = function this$ToggleDropCues (show, index, ignoreOutline)
    {
        var webPartTable = this.get_webPartTable();
        if (ignoreOutline == false)
        {
            webPartTable.style.borderColor = (show ? _highlightColor : _dragZoneColor);
        }
        if (index == -1)
        {
            return;
        }
        var dropCue = _dropCueElements[index];
        if (dropCue && dropCue.style)
        {
            if (dropCue.style.height == "100%" && !dropCue.webPartZoneHorizontalCueResized)
            {
                var oldParentHeight = dropCue.parentElement.clientHeight;
                var realHeight = oldParentHeight - 10;
                dropCue.style.height = realHeight + "px";
                var dropCueVerticalBar = dropCue.getElementsByTagName("DIV")[0];
                if (dropCueVerticalBar && dropCueVerticalBar.style)
                {
                    dropCueVerticalBar.style.height = dropCue.style.height;
                    var heightDiff = (dropCue.parentElement.clientHeight - oldParentHeight);
                    if (heightDiff)
                    {
                        dropCue.style.height = (realHeight - heightDiff) + "px";
                        dropCueVerticalBar.style.height = dropCue.style.height;
                    }
                }
                dropCue.webPartZoneHorizontalCueResized = true;
            }
            dropCue.style.visibility = (show ? "visible" : "hidden");
        }
    }

    this.GetWebPartIndex = function this$GetWebPartIndex(location)
    {
        var x = location.x;
        var y = location.y;
        if ((x < _webPartTableLeft) || (x > _webPartTableRight) ||
            (y < _webPartTableTop) || (y > _webPartTableBottom))
        {
            return -1;
        }
        var vertical = _isVertical;
        var webParts = this.get_webParts();
        var webPartsCount = webParts.length;
        for (var i = 0; i < webPartsCount; i++)
        {
            var webPart = webParts[i];
            if (vertical)
            {
                if (y < webPart.middleY)
                {
                    return i;
                }
            }
            else
            {
                if (x < webPart.middleX)
                {
                    return i;
                }
            }
        }

        return webPartsCount;
    }
    
    this.UpdatePosition = function this$UpdatePosition()
    {
        var topLeft = this.translateOffset(0, 0, _webPartTable, null, false);
        _webPartTableLeft = topLeft.x;
        _webPartTableTop = topLeft.y;
        _webPartTableRight = (_webPartTable != null) ? topLeft.x + _webPartTable.offsetWidth : topLeft.x;
        _webPartTableBottom = (_webPartTable != null) ? topLeft.y + _webPartTable.offsetHeight : topLeft.y;
        var webParts = this.get_webParts();
        for (var i = 0; i < webParts.length; i++)
        {
            webParts[i].UpdatePosition();
        }
    }

    function getDropIndex(thisRef)
    {
        var pageLocation = thisRef.getPageEventLocation(window._event, false);
        return thisRef.GetWebPartIndex(pageLocation);
    }

}
Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone.descriptor =
{
    properties: [ {name: 'uniqueId', type: String},
                  {name: 'webPartManager', type: Object},
                  {name: 'allowLayoutChange', type: Boolean} ]
}

Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone.registerClass("Inflectra.SpiraTest.Web.ServerControls.WebParts.WebPartZone",
    Sys.UI.Control, Inflectra.SpiraTest.Web.ServerControls.IDragSource, Inflectra.SpiraTest.Web.ServerControls.IDropTarget);

if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();

