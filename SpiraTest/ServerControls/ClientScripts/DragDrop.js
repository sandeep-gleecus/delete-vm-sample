Inflectra.SpiraTest.Web.ServerControls._DragDropManager = function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager() {
}




    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$add_dragStart(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().addHandler('dragStart', handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$remove_dragStart(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().removeHandler('dragStart', handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$get_events() {
if (arguments.length !== 0) throw Error.parameterCount();
    // todo: doc comments. this one is commented out (two //) due to a bug with the preprocessor.
        // <value type="Sys.EventHandlerList">
        // </value>
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$add_dragStop(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().addHandler('dragStop', handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$remove_dragStop(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().removeHandler('dragStop', handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$_getInstance() {
        if (!this._instance) {
            if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
                this._instance = new Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager();
            }
            else {
                this._instance = new Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager();
            }
            this._instance.initialize();
            this._instance.add_dragStart(Function.createDelegate(this, this._raiseDragStart));
            this._instance.add_dragStop(Function.createDelegate(this, this._raiseDragStop));
        }
        return this._instance;
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$startDragDrop(dragSource, dragVisual, context) {
        this._getInstance().startDragDrop(dragSource, dragVisual, context);
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$registerDropTarget(target) {
        this._getInstance().registerDropTarget(target);
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$unregisterDropTarget(target) {
        this._getInstance().unregisterDropTarget(target);
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$dispose() {
        delete this._events;
        Sys.Application.unregisterDisposableObject(this);
        Sys.Application.removeComponent(this);
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$_raiseDragStart(sender, eventArgs) {
        var handler = this.get_events().getHandler('dragStart');
        if(handler) {
            handler(this, eventArgs);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$_raiseDragStop(sender, eventArgs) {
        var handler = this.get_events().getHandler('dragStop');
        if(handler) {
            handler(this, eventArgs);
        }
    }
Inflectra.SpiraTest.Web.ServerControls._DragDropManager.prototype = {
    _instance: null,
    _events: null,
    
    add_dragStart: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$add_dragStart,
    remove_dragStart: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$remove_dragStart,
    
    get_events: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$get_events,
    
    add_dragStop: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$add_dragStop,
    remove_dragStop: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$remove_dragStop,
    
    _getInstance: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$_getInstance,
    
    startDragDrop: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$startDragDrop,
    
    registerDropTarget: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$registerDropTarget,
    
    unregisterDropTarget: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$unregisterDropTarget,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$dispose,
    
    _raiseDragStart: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$_raiseDragStart,
    
    _raiseDragStop: Inflectra$SpiraTest$Web$ServerControls$_DragDropManager$_raiseDragStop
}
Inflectra.SpiraTest.Web.ServerControls._DragDropManager.registerClass('Inflectra.SpiraTest.Web.ServerControls._DragDropManager');
// CONSIDER: is <dragDropManager> expected in xml script? If so we'll need to turn the class name/instance around.
Inflectra.SpiraTest.Web.ServerControls.DragDropManager = new Inflectra.SpiraTest.Web.ServerControls._DragDropManager();
Inflectra.SpiraTest.Web.ServerControls.DragDropEventArgs = function Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs(dragMode, dragDataType, dragData) {
    this._dragMode = dragMode;
    this._dataType = dragDataType;
    this._data = dragData;
}

    function Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs$get_dragMode() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._dragMode || null;
    }
    function Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs$get_dragDataType() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataType || null;
    }
    function Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs$get_dragData() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._data || null;
    }
Inflectra.SpiraTest.Web.ServerControls.DragDropEventArgs.prototype = {
    get_dragMode: Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs$get_dragMode,
    get_dragDataType: Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs$get_dragDataType,
    get_dragData: Inflectra$SpiraTest$Web$ServerControls$DragDropEventArgs$get_dragData
}
Inflectra.SpiraTest.Web.ServerControls.DragDropEventArgs.registerClass('Inflectra.SpiraTest.Web.ServerControls.DragDropEventArgs');
Inflectra.SpiraTest.Web.ServerControls.IDragSource = function Inflectra$SpiraTest$Web$ServerControls$IDragSource() {
}


    function Inflectra$SpiraTest$Web$ServerControls$IDragSource$get_dragDataType() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDragSource$getDragData() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDragSource$get_dragMode() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDragSource$onDragStart() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDragSource$onDrag() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDragSource$onDragEnd() { throw Error.notImplemented(); }
Inflectra.SpiraTest.Web.ServerControls.IDragSource.prototype = {
    // Type get_dragDataType()
    get_dragDataType: Inflectra$SpiraTest$Web$ServerControls$IDragSource$get_dragDataType,
    // Object getDragData(Context)
    getDragData: Inflectra$SpiraTest$Web$ServerControls$IDragSource$getDragData,
    // DragMode get_dragMode()
    get_dragMode: Inflectra$SpiraTest$Web$ServerControls$IDragSource$get_dragMode,
    // void onDragStart()
    onDragStart: Inflectra$SpiraTest$Web$ServerControls$IDragSource$onDragStart,
    // void onDrag()
    onDrag: Inflectra$SpiraTest$Web$ServerControls$IDragSource$onDrag,
    // void onDragEnd(Cancelled)
    onDragEnd: Inflectra$SpiraTest$Web$ServerControls$IDragSource$onDragEnd
}
Inflectra.SpiraTest.Web.ServerControls.IDragSource.registerInterface('Inflectra.SpiraTest.Web.ServerControls.IDragSource');
///////////////////////////////////////////////////////////////////////////////
// IDropTarget
Inflectra.SpiraTest.Web.ServerControls.IDropTarget = function Inflectra$SpiraTest$Web$ServerControls$IDropTarget() {
}

    function Inflectra$SpiraTest$Web$ServerControls$IDropTarget$get_dropTargetElement() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDropTarget$canDrop() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDropTarget$drop() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDropTarget$onDragEnterTarget() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDropTarget$onDragLeaveTarget() { throw Error.notImplemented(); }

    function Inflectra$SpiraTest$Web$ServerControls$IDropTarget$onDragInTarget() { throw Error.notImplemented(); }
Inflectra.SpiraTest.Web.ServerControls.IDropTarget.prototype = {
    get_dropTargetElement: Inflectra$SpiraTest$Web$ServerControls$IDropTarget$get_dropTargetElement,
    // bool canDrop(DragMode, DataType, Data)
    canDrop: Inflectra$SpiraTest$Web$ServerControls$IDropTarget$canDrop,
    // void drop(DragMode, DataType, Data)
    drop: Inflectra$SpiraTest$Web$ServerControls$IDropTarget$drop,
    // void onDragEnterTarget(DragMode, DataType, Data)
    onDragEnterTarget: Inflectra$SpiraTest$Web$ServerControls$IDropTarget$onDragEnterTarget,
    // void onDragLeaveTarget(DragMode, DataType, Data)
    onDragLeaveTarget: Inflectra$SpiraTest$Web$ServerControls$IDropTarget$onDragLeaveTarget,
    // void onDragInTarget(DragMode, DataType, Data)
    onDragInTarget: Inflectra$SpiraTest$Web$ServerControls$IDropTarget$onDragInTarget
}
Inflectra.SpiraTest.Web.ServerControls.IDropTarget.registerInterface('Inflectra.SpiraTest.Web.ServerControls.IDropTarget');
Inflectra.SpiraTest.Web.ServerControls.DragMode = function Inflectra$SpiraTest$Web$ServerControls$DragMode() {
    throw Error.invalidOperation();
}



Inflectra.SpiraTest.Web.ServerControls.DragMode.prototype = {
    Copy: 0,
    Move: 1
}
Inflectra.SpiraTest.Web.ServerControls.DragMode.registerEnum('Inflectra.SpiraTest.Web.ServerControls.DragMode');
Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager = function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager() {
    Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager.initializeBase(this);
}























    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$add_dragStart(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().addHandler("dragStart", handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$remove_dragStart(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().removeHandler("dragStart", handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$add_dragStop(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().addHandler("dragStop", handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$remove_dragStop(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().removeHandler("dragStop", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager.callBaseMethod(this, 'initialize');
        this._mouseUpHandler = Function.createDelegate(this, this.mouseUpHandler);
        this._documentMouseMoveHandler = Function.createDelegate(this, this.documentMouseMoveHandler);
        this._documentDragOverHandler = Function.createDelegate(this, this.documentDragOverHandler);
        this._dragStartHandler = Function.createDelegate(this, this.dragStartHandler);
        this._mouseMoveHandler = Function.createDelegate(this, this.mouseMoveHandler);
        this._dragEnterHandler = Function.createDelegate(this, this.dragEnterHandler);
        this._dragLeaveHandler = Function.createDelegate(this, this.dragLeaveHandler);
        this._dragOverHandler = Function.createDelegate(this, this.dragOverHandler);
        this._dropHandler = Function.createDelegate(this, this.dropHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dispose() {
        if(this._dropTargets) {
            for (var i = 0; i < this._dropTargets; i++) {
                this.unregisterDropTarget(this._dropTargets[i]);
            }
            this._dropTargets = null;
        }

        Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$startDragDrop(dragSource, dragVisual, context) {
        var ev = window._event;

        // Don't allow drag and drop if there is another active drag operation going on.
        if (this._isDragging) {
            return;
        }
        
        this._underlyingTarget = null;
        this._activeDragSource = dragSource;
        this._activeDragVisual = dragVisual;
        this._activeContext = context;
        
        var mousePosition = { x: ev.clientX, y: ev.clientY };

        // By default we use absolute positioning, unless a different type
        // of positioning is set explicitly.
        dragVisual.originalPosition = dragVisual.style.position;
        dragVisual.style.position = "absolute";
        if (!ev.changedTouches)
        {
            document._lastPosition = mousePosition;
            dragVisual.startingPoint = mousePosition;
            var scrollOffset = this.getScrollOffset(dragVisual, /* recursive */true);
            dragVisual.startingPoint = this.addPoints(dragVisual.startingPoint, scrollOffset);
            if (dragVisual.style.position == "absolute")
            {
                dragVisual.startingPoint = this.subtractPoints(dragVisual.startingPoint, Sys.UI.DomElement.getLocation(dragVisual));
            }
            else
            {
                var left = parseInt(dragVisual.style.left);
                var top = parseInt(dragVisual.style.top);
                if (isNaN(left)) left = "0";
                if (isNaN(top)) top = "0";

                dragVisual.startingPoint = this.subtractPoints(dragVisual.startingPoint, { x: left, y: top });
            }
        }

        // Monitor DOM changes.
        this._prepareForDomChanges();
        dragSource.onDragStart();
        var eventArgs = new Inflectra.SpiraTest.Web.ServerControls.DragDropEventArgs(
            dragSource.get_dragMode(),
            dragSource.get_dragDataType(),
            dragSource.getDragData(context));
        var handler = this.get_events().getHandler('dragStart');
        if(handler) handler(this,eventArgs);
        this._recoverFromDomChanges();

        this._wireEvents();

        this._drag(/* isInitialDrag */ true);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_stopDragDrop(cancelled) {
        var ev = window._event;
        if (this._activeDragSource) {
            this._unwireEvents();

            if (!cancelled) {
                // The drag operation is cancelled if there
                // is no drop target.
                cancelled = (this._underlyingTarget == null);
            }
            
            if (!cancelled && this._underlyingTarget) {
                this._underlyingTarget.drop(this._activeDragSource.get_dragMode(), this._activeDragSource.get_dragDataType(),
                    this._activeDragSource.getDragData(this._activeContext));
            }
            this._activeDragSource.onDragEnd(cancelled);
            var handler = this.get_events().getHandler('dragStop');
            if(handler) handler(this,Sys.EventArgs.Empty);
            
            this._activeDragVisual.style.position = this._activeDragVisual.originalPosition;
        
            this._activeDragSource = null;
            this._activeContext = null;
            this._activeDragVisual = null;
            this._isDragging = false;
            this._potentialTarget = null;
            ev.preventDefault();
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_touchDrag()
    {
        //Get the pos
        var evt = window._event;
        evt.preventDefault();
        var touchPosition = { x: evt.changedTouches[0].pageX, y: evt.changedTouches[0].pageY };
        var position;
        if (this._activeDragVisual.startingPoint)
        {
            position = this.subtractPoints(touchPosition, this._activeDragVisual.startingPoint);
        }
        else
        {
            position = touchPosition;
        }

        // Check if the visual moved at all.
        if (parseInt(this._activeDragVisual.style.left) == position.x && parseInt(this._activeDragVisual.style.top) == position.y)
        {
            return;
        }

        Sys.UI.DomElement.setLocation(this._activeDragVisual, Math.floor(position.x), Math.floor(position.y));

        // Monitor DOM changes.
        this._prepareForDomChanges();
        this._activeDragSource.onDrag();
        this._recoverFromDomChanges();

        // Find a potential target.
        this._potentialTarget = this._findPotentialTarget(this._activeDragSource, this._activeDragVisual);

        var movedToOtherTarget = (this._potentialTarget != this._underlyingTarget || this._potentialTarget == null);
        // Check if we are leaving an underlying target.
        if (movedToOtherTarget && this._underlyingTarget != null)
        {
            this._leaveTarget(this._activeDragSource, this._underlyingTarget);
        }

        if (this._potentialTarget != null)
        {
            // Check if we are entering a new target.
            if (movedToOtherTarget)
            {
                this._underlyingTarget = this._potentialTarget;

                // Enter the new target.
                this._enterTarget(this._activeDragSource, this._underlyingTarget);
            }
            else
            {
                this._moveInTarget(this._activeDragSource, this._underlyingTarget);
            }
        }
        else
        {
            this._underlyingTarget = null;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_drag(isInitialDrag) {
        var ev = window._event;
        var mousePosition = { x: ev.clientX, y: ev.clientY };

        // NOTE: We store the event object to be able to determine the current 
        // mouse position in Mozilla in other event handlers such as keydown.
        document._lastPosition = mousePosition;
        
        var scrollOffset = this.getScrollOffset(this._activeDragVisual, /* recursive */ true);
        var position = this.addPoints(this.subtractPoints(mousePosition, this._activeDragVisual.startingPoint), scrollOffset);
        // Check if the visual moved at all.
        if (!isInitialDrag && parseInt(this._activeDragVisual.style.left) == position.x && parseInt(this._activeDragVisual.style.top) == position.y) {
            return;
        }
        
        Sys.UI.DomElement.setLocation(this._activeDragVisual, position.x, position.y);
        
        // Monitor DOM changes.
        this._prepareForDomChanges();
        this._activeDragSource.onDrag();
        this._recoverFromDomChanges();

        // Find a potential target.
        this._potentialTarget = this._findPotentialTarget(this._activeDragSource, this._activeDragVisual);
        
        var movedToOtherTarget = (this._potentialTarget != this._underlyingTarget || this._potentialTarget == null);
        // Check if we are leaving an underlying target.
        if (movedToOtherTarget && this._underlyingTarget != null) {
            this._leaveTarget(this._activeDragSource, this._underlyingTarget);
        }
        
        if (this._potentialTarget != null) {
            // Check if we are entering a new target.
            if (movedToOtherTarget) {
                this._underlyingTarget = this._potentialTarget;
                
                // Enter the new target.
                this._enterTarget(this._activeDragSource, this._underlyingTarget);
            }
            else {
                this._moveInTarget(this._activeDragSource, this._underlyingTarget);
            }
        }
        else {
            this._underlyingTarget = null;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_wireEvents() {
        Sys.UI.DomEvent.addHandler(document, "mouseup", this._mouseUpHandler);
        Sys.UI.DomEvent.addHandler(document, "mousemove", this._documentMouseMoveHandler);
        Sys.UI.DomEvent.addHandler(document.body, "dragover", this._documentDragOverHandler);        
        Sys.UI.DomEvent.addHandler(this._activeDragVisual, "dragstart", this._dragStartHandler);
        Sys.UI.DomEvent.addHandler(this._activeDragVisual, "dragend", this._mouseUpHandler);
        Sys.UI.DomEvent.addHandler(this._activeDragVisual, "drag", this._mouseMoveHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_unwireEvents() {
        Sys.UI.DomEvent.removeHandler(this._activeDragVisual, "drag", this._mouseMoveHandler);
        Sys.UI.DomEvent.removeHandler(this._activeDragVisual, "dragend", this._mouseUpHandler);
        Sys.UI.DomEvent.removeHandler(this._activeDragVisual, "dragstart", this._dragStartHandler);
        Sys.UI.DomEvent.removeHandler(document.body, "dragover", this._documentDragOverHandler);
        Sys.UI.DomEvent.removeHandler(document, "mousemove", this._documentMouseMoveHandler);
        Sys.UI.DomEvent.removeHandler(document, "mouseup", this._mouseUpHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$registerDropTarget(dropTarget) {
        if (!this._dropTargets) {
            this._dropTargets = [];
        }
        Array.add(this._dropTargets, dropTarget);
        
        this._wireDropTargetEvents(dropTarget);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$unregisterDropTarget(dropTarget) {
        this._unwireDropTargetEvents(dropTarget);
        if(this._dropTargets) {
            Array.remove(this._dropTargets, dropTarget);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_wireDropTargetEvents(dropTarget) {
        var associatedElement = dropTarget.get_dropTargetElement();
        associatedElement._dropTarget = dropTarget;
        Sys.UI.DomEvent.addHandler(associatedElement, "dragenter", this._dragEnterHandler);
        Sys.UI.DomEvent.addHandler(associatedElement, "dragleave", this._dragLeaveHandler);
        Sys.UI.DomEvent.addHandler(associatedElement, "dragover", this._dragOverHandler);
        Sys.UI.DomEvent.addHandler(associatedElement, "drop", this._dropHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_unwireDropTargetEvents(dropTarget) {
        var associatedElement = dropTarget.get_dropTargetElement();

        associatedElement._dropTarget = null;
        Sys.UI.DomEvent.removeHandler(associatedElement, "dragenter", this._dragEnterHandler);
        Sys.UI.DomEvent.removeHandler(associatedElement, "dragleave", this._dragLeaveHandler);
        Sys.UI.DomEvent.removeHandler(associatedElement, "dragover", this._dragOverHandler);
        Sys.UI.DomEvent.removeHandler(associatedElement, "drop", this._dropHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragStartHandler(ev) {
        window._event = ev;
        document.selection.empty();

        var dt = ev.dataTransfer;
        if(!dt) dt = ev.rawEvent.dataTransfer;
        
        var dataType = this._activeDragSource.get_dragDataType().toLowerCase();
        var data = this._activeDragSource.getDragData(this._activeContext);
        
        if (data) {
            // TODO: How do we want to deal with 'non-compatible types'?
            if (dataType != "text" && dataType != "url") {
                dataType = "text";

                if (data.innerHTML != null) {
                    data = data.innerHTML;
                }
            }

            dt.effectAllowed = "move";
            dt.setData(dataType, data.toString());
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$mouseUpHandler(ev) {
        window._event = ev;
        this._stopDragDrop(false);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$documentMouseMoveHandler(ev) {
        window._event = ev;
        this._dragDrop();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$documentDragOverHandler(ev) {
        window._event = ev;
        if(this._potentialTarget) ev.preventDefault();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$mouseMoveHandler(ev) {
        window._event = ev;
        this._drag();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragEnterHandler(ev) {
        window._event = ev;
        if (this._isDragging) {
            ev.preventDefault();
        }
        else {
            // An external object is dragged to the drop target.
            var dataObjects = Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(ev.target));
            for (var i = 0; i < dataObjects.length; i++) {
                this._dropTarget.onDragEnterTarget(Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy, dataObjects[i].type, dataObjects[i].value);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragLeaveHandler(ev) {
        window._event = ev;
        if (this._isDragging) {
            ev.preventDefault();
        }
        else {
            // An external object is dragged to the drop target.
            var dataObjects = Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(ev.target));
            for (var i = 0; i < dataObjects.length; i++) {
                this._dropTarget.onDragLeaveTarget(Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy, dataObjects[i].type, dataObjects[i].value);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragOverHandler(ev) {
        window._event = ev;
        if (this._isDragging) {
            ev.preventDefault();
        }
        else {
            // An external object is dragged over the drop target.
            var dataObjects = Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(ev.target));
            for (var i = 0; i < dataObjects.length; i++) {
                this._dropTarget.onDragInTarget(Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy, dataObjects[i].type, dataObjects[i].value);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dropHandler(ev) {
        window._event = ev;
        if (!this._isDragging) {
            // An external object is dropped on the drop target.
            var dataObjects = Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager._getDataObjectsForDropTarget(this._getDropTarget(ev.target));
            for (var i = 0; i < dataObjects.length; i++) {
                this._dropTarget.drop(Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy, dataObjects[i].type, dataObjects[i].value);
            }
        }
        ev.preventDefault();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_getDropTarget(element) {
        while (element) {
            if (element._dropTarget != null) {
                return element._dropTarget;
            }
            element = element.parentNode;
        }
        return null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_dragDrop() {
        if (this._isDragging) {
            return;
        }
        
        this._isDragging = true;
        this._activeDragVisual.dragDrop();
        document.selection.empty();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_moveInTarget(dragSource, dropTarget) {
        // Monitor DOM changes.
        this._prepareForDomChanges();
        dropTarget.onDragInTarget(dragSource.get_dragMode(), dragSource.get_dragDataType(), dragSource.getDragData(this._activeContext));
        this._recoverFromDomChanges();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_enterTarget(dragSource, dropTarget) {
        // Monitor DOM changes.
        this._prepareForDomChanges();
        dropTarget.onDragEnterTarget(dragSource.get_dragMode(), dragSource.get_dragDataType(), dragSource.getDragData(this._activeContext));
        this._recoverFromDomChanges();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_leaveTarget(dragSource, dropTarget) {
        // Monitor DOM changes.
        this._prepareForDomChanges();
        dropTarget.onDragLeaveTarget(dragSource.get_dragMode(), dragSource.get_dragDataType(), dragSource.getDragData(this._activeContext));
        this._recoverFromDomChanges();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_findPotentialTarget(dragSource, dragVisual) {
        var ev = window._event;

        if (!this._dropTargets) {
            return null;
        }

        var type = dragSource.get_dragDataType();
        var mode = dragSource.get_dragMode();
        var data = dragSource.getDragData(this._activeContext);

        // Get the current cursor location.
        var x;
        var y;
        if (ev.changedTouches)
        {
            x = Math.floor(ev.changedTouches[0].pageX);
            y = Math.floor(ev.changedTouches[0].pageY);
        }
        else
        {
            var scrollOffset = this.getScrollOffset(document.body, /* recursive */true);
            x = ev.clientX + scrollOffset.x;
            y = ev.clientY + scrollOffset.y;
        }
        var cursorRect = { x: x - this._radius, y: y - this._radius, width: this._radius * 2, height: this._radius * 2 };

        // Find any targets near the current cursor location.
        for (var i = 0; i < this._dropTargets.length; i++) {
            var dt = this._dropTargets[i];
            var canDrop = dt.canDrop(mode, type, data);
            if(!canDrop) continue;

            var el = dt.get_dropTargetElement();
            var targetRect = Sys.UI.DomElement.getBounds(el);
            var overlaps = Sys.UI.Control.overlaps(cursorRect, targetRect);
            // document.body always overlaps. Work around for an issue where in FF the body has no offsetHeight if height is set to 100%
            if (overlaps || el === document.body) {
                return dt;
            }
        }        
        return null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_prepareForDomChanges() {
        this._oldOffset = Sys.UI.DomElement.getLocation(this._activeDragVisual);
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_recoverFromDomChanges() {
        var newOffset = Sys.UI.DomElement.getLocation(this._activeDragVisual);
        if (this._oldOffset.x != newOffset.x || this._oldOffset.y != newOffset.y) {
            this._activeDragVisual.startingPoint = this.subtractPoints(this._activeDragVisual.startingPoint, this.subtractPoints(this._oldOffset, newOffset));
            scrollOffset = this.getScrollOffset(this._activeDragVisual, /* recursive */ true);
            var position = this.addPoints(this.subtractPoints(document._lastPosition, this._activeDragVisual.startingPoint), scrollOffset);
            Sys.UI.DomElement.setLocation(this._activeDragVisual, position.x, position.y);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$addPoints(p1, p2) {
        return { x: p1.x + p2.x, y: p1.y + p2.y };
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$subtractPoints(p1, p2)
    {
        if (isNaN(p2.x) || isNaN(p2.y))
        {
            return p1;
        }
        return { x: p1.x - p2.x, y: p1.y - p2.y };
    }


    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$getScrollOffset(element, recursive)
    {
        var left = 0;
        if (element.scrollLeft)
        {
            left = element.scrollLeft;
        }
        var top = 0;
        if (element.scrollTop)
        {
            top = element.scrollTop;
        }
        if (recursive)
        {
            var parent = element.parentNode;
            while (parent != null && parent.scrollLeft != null)
            {
                if (parent.scrollLeft)
                {
                    left += parent.scrollLeft;
                }
                if (parent.scrollTop)
                {
                    top += parent.scrollTop;
                }
                // Don't include anything below the body.
                if (parent == document.body && (left != 0 && top != 0))
                    break;
                parent = parent.parentNode;
            }
        }
        return { x: left, y: top };
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$getBrowserRectangle() {
        var width = window.innerWidth;
        var height = window.innerHeight;
        if (width == null) {
            width = document.body.clientWidth;
        }
        if (height == null) {
            height = document.body.clientHeight;
        }

        return { x: 0, y: 0, width: width, height: height };
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$getNextSibling(item) {
        for (item = item.nextSibling; item != null; item = item.nextSibling) {
            if (item.innerHTML != null) {
                return item;
            }
        }
        return null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$hasParent(element) {
        return (element.parentNode != null && element.parentNode.tagName != null);
    }
Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager.prototype = {
    _dropTargets: null,
    // Radius of the cursor used to determine what drop target we 
    // are hovering. Anything below the cursor's zone may be a 
    // potential drop target.
    _radius: 10,
    _activeDragVisual: null,
    _activeContext: null,
    _activeDragSource: null,
    _underlyingTarget: null,
    _oldOffset: null,
    _potentialTarget: null,
    _isDragging: false,
    _mouseUpHandler: null,
    _documentMouseMoveHandler: null,
    _documentDragOverHandler: null,
    _dragStartHandler: null,
    _mouseMoveHandler: null,
    _dragEnterHandler: null,
    _dragLeaveHandler: null,
    _dragOverHandler: null,
    _dropHandler: null,
    
    add_dragStart: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$add_dragStart,
    remove_dragStart: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$remove_dragStart,
    add_dragStop: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$add_dragStop,
    remove_dragStop: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$remove_dragStop,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$initialize,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dispose,

    startDragDrop: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$startDragDrop,
    
    _stopDragDrop: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_stopDragDrop,

    _drag: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_drag,

    _touchDrag: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_touchDrag,
    
    _wireEvents: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_wireEvents,
    
    _unwireEvents: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_unwireEvents,
    
    registerDropTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$registerDropTarget,
    
    unregisterDropTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$unregisterDropTarget,
    
    _wireDropTargetEvents: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_wireDropTargetEvents,
        
    _unwireDropTargetEvents: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_unwireDropTargetEvents,
    
    dragStartHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragStartHandler,
    
    mouseUpHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$mouseUpHandler,
    
    documentMouseMoveHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$documentMouseMoveHandler,

    documentDragOverHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$documentDragOverHandler,
    
    mouseMoveHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$mouseMoveHandler,
    
    dragEnterHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragEnterHandler,
    
    dragLeaveHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragLeaveHandler,
    
    dragOverHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dragOverHandler,
    
    dropHandler: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$dropHandler,
    
    _getDropTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_getDropTarget,
    
    _dragDrop: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_dragDrop,
    
    _moveInTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_moveInTarget,
    
    _enterTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_enterTarget,
    
    _leaveTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_leaveTarget,
    
    _findPotentialTarget: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_findPotentialTarget,
    
    _prepareForDomChanges: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_prepareForDomChanges,
    
    _recoverFromDomChanges: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_recoverFromDomChanges,
    
    addPoints: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$addPoints,
    
    subtractPoints: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$subtractPoints,
    
    // -- Drag and drop helper methods.
    getScrollOffset: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$getScrollOffset,
    
    getBrowserRectangle: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$getBrowserRectangle,
    
    getNextSibling: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$getNextSibling,
    
    hasParent: Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$hasParent
}
Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager.registerClass('Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager', Sys.Component);

Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager._getDataObjectsForDropTarget = function Inflectra$SpiraTest$Web$ServerControls$IEDragDropManager$_getDataObjectsForDropTarget(dropTarget) {
    if (dropTarget == null) {
        return [];
    }
    var ev = window._event;
    var dataObjects = [];
    var dataTypes = [ "URL", "Text" ];
    var data;
    for (var i = 0; i < dataTypes.length; i++) {
        var dt = ev.dataTransfer;
        if(!dt) dt = ev.rawEvent.dataTransfer;
        data = dt.getData(dataTypes[i]);
        if (dropTarget.canDrop(Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy, dataTypes[i], data)) {
            if (data) {
                Array.add(dataObjects, { type : dataTypes[i], value : data });
            }
        }
    }

    return dataObjects;
}
Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager = function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager() {
    Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.initializeBase(this);
}



















    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.callBaseMethod(this, "initialize");
        
        this._mouseUpHandler = Function.createDelegate(this, this.mouseUpHandler);
        this._mouseMoveHandler = Function.createDelegate(this, this.mouseMoveHandler);
        //Touch
        this._touchMoveHandler = Function.createDelegate(this, this.touchMoveHandler);
        this._touchEndHandler = Function.createDelegate(this, this.touchEndHandler);
        this._keyPressHandler = Function.createDelegate(this, this.keyPressHandler);
        this._scroller = new Inflectra.SpiraTest.Web.AjaxExtensions.Timer();
        this._scroller.set_interval(10);
        this._scroller.add_tick(Function.createDelegate(this,this.scrollerTickHandler));
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$startDragDrop(dragSource, dragVisual, context) {
        //Set the starting position if a touch event
        if (window._event.changedTouches)
        {
            //In the future could calculate the offset from the touch-x/y and the element x/y instead of hard-coding
            dragVisual.startingPoint = { x: 100, y: 50 };
        }
        this._activeDragSource = dragSource;
        this._activeDragVisual = dragVisual;
        this._activeContext = context;
        
        Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.callBaseMethod(this, "startDragDrop", [dragSource, dragVisual, context]);
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_stopDragDrop(cancelled) {
        this._scroller.set_enabled(false);
        
        Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.callBaseMethod(this, "_stopDragDrop", [cancelled]);
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_drag(isInitialDrag) {
        Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.callBaseMethod(this, "_drag", [isInitialDrag]);
        
        this._autoScroll();
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_wireEvents() {
        Sys.UI.DomEvent.addHandler(document, "mouseup", this._mouseUpHandler);
        Sys.UI.DomEvent.addHandler(document, "mousemove", this._mouseMoveHandler);
        Sys.UI.DomEvent.addHandler(document, "keypress", this._keyPressHandler);
        //Touch
        document.addEventListener('touchmove', this._touchMoveHandler, false);
        document.addEventListener('touchend', this._touchEndHandler, false);
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_unwireEvents() {
        Sys.UI.DomEvent.removeHandler(document, "keypress", this._keyPressHandler);
        Sys.UI.DomEvent.removeHandler(document, "mousemove", this._mouseMoveHandler);
        Sys.UI.DomEvent.removeHandler(document, "mouseup", this._mouseUpHandler);
        //Touch
        document.removeEventListener('touchmove', this._touchMoveHandler, false);
        document.removeEventListener('touchend', this._touchEndHandler, false);
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_wireDropTargetEvents(dropTarget) {
        //
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_unwireDropTargetEvents(dropTarget) {
        //
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$mouseUpHandler(e) {
        window._event = e;
        this._stopDragDrop(false);
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$mouseMoveHandler(e) {
        window._event = e;
        this._drag();
    }

    /* Touch Events */
    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$touchEndHandler(e)
    {
        window._event = e;
        this._stopDragDrop(false);
    }
    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$touchMoveHandler(e)
    {
        window._event = e;
        this._touchDrag();
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$keyPressHandler(e) {
        window._event = e;
        // Escape.
        var k = e.keyCode ? e.keyCode : e.rawEvent.keyCode;
        if (k == 27) {
            this._stopDragDrop(/* cancel */ true);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_autoScroll() {
        var ev = window._event;
        var browserRect = this.getBrowserRectangle();
        if (browserRect.width > 0) {
            this._scrollDeltaX = this._scrollDeltaY = 0;
            if (ev.clientX < browserRect.x + this._scrollEdgeConst) this._scrollDeltaX = -this._scrollByConst;
            else if (ev.clientX > browserRect.width - this._scrollEdgeConst) this._scrollDeltaX = this._scrollByConst;
            if (ev.clientY < browserRect.y + this._scrollEdgeConst) this._scrollDeltaY = -this._scrollByConst;
            else if (ev.clientY > browserRect.height - this._scrollEdgeConst) this._scrollDeltaY = this._scrollByConst;
            if (this._scrollDeltaX != 0 || this._scrollDeltaY != 0) {
                this._scroller.set_enabled(true);
            }
            else {
                this._scroller.set_enabled(false);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$scrollerTickHandler() {
        var oldLeft = (document.body.scrollLeft) ? document.body.scrollLeft : document.documentElement.scrollLeft;
        var oldTop = (document.body.scrollTop) ? document.body.scrollTop : document.documentElement.scrollTop;
        window.scrollBy(this._scrollDeltaX, this._scrollDeltaY);
        var newLeft = (document.body.scrollLeft) ? document.body.scrollLeft : document.documentElement.scrollLeft;
        var newTop = (document.body.scrollTop) ? document.body.scrollTop : document.documentElement.scrollTop;
        
        var dragVisual = this._activeDragVisual;
        var position = { x: parseInt(dragVisual.style.left) + (newLeft - oldLeft), y: parseInt(dragVisual.style.top) + (newTop - oldTop) };
        Sys.UI.DomElement.setLocation(dragVisual, position.x, position.y);
    }
Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.prototype = {
    //_dropTargets: null,
    // Radius of the cursor used to determine what drop target we 
    // are hovering. Anything below the cursor's zone may be a 
    // potential drop target.
    _scrollEdgeConst: 40,
    _scrollByConst: 10,
    _scroller: null,
    _scrollDeltaX: null,
    _scrollDeltaY: null,
    _activeDragVisual: null,
    _activeContext: null,
    _activeDragSource: null,
    //_oldOffset: null,
    //_potentialTarget: null,
    _mouseUpHandler: null,
    _mouseMoveHandler: null,
    _keyPressHandler: null,
    _touchMoveHandler: null,
    _touchEndHandler: null,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$initialize,

    startDragDrop: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$startDragDrop,
    
    _stopDragDrop: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_stopDragDrop,
    
    _drag: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_drag,
    
    _wireEvents: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_wireEvents,
    
    _unwireEvents: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_unwireEvents,
    
    _wireDropTargetEvents: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_wireDropTargetEvents,
    
    _unwireDropTargetEvents: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_unwireDropTargetEvents,
    
    mouseUpHandler: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$mouseUpHandler,

    mouseMoveHandler: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$mouseMoveHandler,

    touchMoveHandler: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$touchMoveHandler,

    touchEndHandler: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$touchEndHandler,
    
    keyPressHandler: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$keyPressHandler,
    
    _autoScroll: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$_autoScroll,
    
    scrollerTickHandler: Inflectra$SpiraTest$Web$ServerControls$GenericDragDropManager$scrollerTickHandler
}
Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager.registerClass('Inflectra.SpiraTest.Web.ServerControls.GenericDragDropManager', Inflectra.SpiraTest.Web.ServerControls.IEDragDropManager);

Inflectra.SpiraTest.Web.ServerControls.RepeatDirection = function Inflectra$SpiraTest$Web$ServerControls$RepeatDirection() {
    throw Error.invalidOperation();
}



Inflectra.SpiraTest.Web.ServerControls.RepeatDirection.prototype = {
    Horizontal: 0,
    Vertical: 1
}
Inflectra.SpiraTest.Web.ServerControls.RepeatDirection.registerEnum('Inflectra.SpiraTest.Web.ServerControls.RepeatDirection');
Inflectra.SpiraTest.Web.ServerControls.DragDropList = function Inflectra$SpiraTest$Web$ServerControls$DragDropList(associatedElement) {
    Inflectra.SpiraTest.Web.ServerControls.DragDropList.initializeBase(this, [associatedElement]);
    this._acceptedDataTypes = [];
}





















    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_data() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._data;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_data(value) {
        this._data = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.DragDropList.callBaseMethod(this, 'initialize');
        this.get_element().__dragDropList = this;
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);
    }



    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$startDragDrop(dragObject, context, dragVisual) {
        if (!this._isDragging) {
            this._isDragging = true;
            this._currentContext = context;
            if (!dragVisual) {
                dragVisual = this.createDragVisual(dragObject);
                //DEBUG Sys.Debug.trace("Using default drag visual.");
            }
            else {
                this._dragVisual = dragVisual;
                //DEBUG Sys.Debug.trace("Using user-specified drag visual.");
            }
            Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(this, dragVisual, context);
        }
        else {
            //Sys.Debug.trace("Drag drop rejected by DragDropList: already dragging.");
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$createDragVisual(dragObject) {
        if (this._dragMode === Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy) {
            this._dragVisual = dragObject.cloneNode(true);
        }
        else {
            this._dragVisual = dragObject;
        }

        var oldOffset = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().getScrollOffset(dragObject, true);

        this._dragVisual.style.width = dragObject.offsetWidth + "px";
        this._dragVisual.style.height = dragObject.offsetHeight + "px";

        this._dragVisual.style.opacity = "0.4";
        this._dragVisual.style.filter = "progid:DXImageTransform.Microsoft.BasicImage(opacity=0.4);";
        this._originalZIndex = this._dragVisual.style.zIndex;
        this._dragVisual.style.zIndex = 99999;

        this._originalParent = this._dragVisual.parentNode;
        this._originalNextSibling = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().getNextSibling(this._dragVisual);

        var ddm = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance();
        var currentLocation = Sys.UI.DomElement.getLocation(dragObject);

        // Store the drag object in a temporary container to make it self-contained.
        var dragVisualContainer = this._getFloatContainer();
        Sys.UI.DomElement.setLocation(dragVisualContainer, currentLocation.x, currentLocation.y);
        
        if (Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._dragVisual)) {
            this._dragVisual.parentNode.removeChild(this._dragVisual);
        }
        dragVisualContainer.appendChild(this._dragVisual);

        var newOffset = ddm.getScrollOffset(dragObject, true);
        if (oldOffset.x !== newOffset.x || oldOffset.y !== newOffset.y) {
            var diff = ddm.subtractPoints(oldOffset, newOffset);
            var location = ddm.subtractPoints(currentLocation, diff);
            Sys.UI.DomElement.setLocation(dragVisualContainer, location.x, location.y);
        }

        return dragVisualContainer;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_emptyTemplate() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._emptyTemplate;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_emptyTemplate(value) {
        this._emptyTemplate = value;
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dragDataType() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataType;
    }
    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_dragDataType(value) {
        this._dataType = value;
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$getDragData(context) {
        return context;
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dragMode() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._dragMode;
    }
    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_dragMode(value) {
        this._dragMode = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$dispose() {
        this.get_element().__dragDropList = null;
        Inflectra.SpiraTest.Web.ServerControls.DragDropList.callBaseMethod(this, 'dispose');
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragStart() {
        this._validate();
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDrag() {
        //
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragEnd(cancelled) {
        if (this._floatContainerInstance) {
            if (this._dragMode === Inflectra.SpiraTest.Web.ServerControls.DragMode.Copy) {
                this._floatContainerInstance.removeChild(this._dragVisual);
            }
            else {
                // NOTE: There seems to be a cursor issue in Mozilla when setting the opacity to 1. We
                // can work around this by setting the opacity to anything lower than 1 instead.
                this._dragVisual.style.opacity = "0.999";
                //_dragVisual.style.opacity = "1";
                this._dragVisual.style.filter = "";

                this._dragVisual.style.zIndex = this._originalZIndex ? this._originalZIndex : 0;

                if (cancelled) {
                    // Re-parent the drag visual to its original position.
                    this._dragVisual.parentNode.removeChild(this._dragVisual);
                    if (this._originalNextSibling != null) {
                        this._originalParent.insertBefore(this._dragVisual, this._originalNextSibling);
                    }
                    else {
                        this._originalParent.appendChild(this._dragVisual);
                    }
                }
                else {
                    if (this._dragVisual.parentNode === this._floatContainerInstance) {
                        this._dragVisual.parentNode.removeChild(this._dragVisual);
                    }
                }
            }

            // Remove the container.
            document.body.removeChild(this._floatContainerInstance);
        }
        else {
            this._dragVisual.parentNode.removeChild(this._dragVisual);
        }

        if (!cancelled && this._data && this._dragMode === Inflectra.SpiraTest.Web.ServerControls.DragMode.Move) {
            var data = this.getDragData(this._currentContext);
            if (this._data && data) {
                if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
                    this._data.remove(data);
                }
                else if (this._data instanceof Array) {
                    if(typeof(this._data.remove) === "function") {
                        this._data.remove(data);
                    }
                    else {
                        Array.remove(this._data, data);
                    }
                }
            }
        }

        this._isDragging = false;
        this._validate();
    }



    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_direction() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._direction;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_direction(value) {
        this._direction = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_acceptedDataTypes() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._acceptedDataTypes;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_acceptedDataTypes(value) {
        this._acceptedDataTypes = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dropCueTemplate() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._dropCueTemplate;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_dropCueTemplate(value) {
        this._dropCueTemplate = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dropTargetElement() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element();
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$canDrop(dragMode, dataType, data) {
        for (var i = 0; i < this._acceptedDataTypes.length; i++) {
            if (this._acceptedDataTypes[i] === dataType) {
                return true;
            }
        }

        return false;
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$drop(dragMode, dataType, data) {
        if (dataType === "HTML" && dragMode === Inflectra.SpiraTest.Web.ServerControls.DragMode.Move) {
            // Re-parent the drag visual.
            dragVisual = data;

            var potentialNextSibling = this._findPotentialNextSibling(dragVisual);
            this._setDropCueVisible(false, dragVisual);
            dragVisual.parentNode.removeChild(dragVisual);
            if (potentialNextSibling) {
                this.get_element().insertBefore(dragVisual, potentialNextSibling);
            }
            else {
                this.get_element().appendChild(dragVisual);
            }
        }
        else {
            this._setDropCueVisible(false);
        }

        if (this._data && data) {
            var newRow = data;
            if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.isInstanceOfType(data) && Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.isInstanceOfType(this._data)) {
                var src = data.get_table();
                if (src) {
                    newRow = this._data.createRow(data);
                }
            }
            if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
                this._data.add(newRow);
            }
            else if (this._data instanceof Array) {
                if(typeof(this._data.add) === "function") {
                    this._data.add(newRow);
                }
                else {
                    Array.add(this._data, newRow);
                }
            }
        }
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragEnterTarget(dragMode, dataType, data) {
        if (dataType === "HTML") {
            this._setDropCueVisible(true, data);
            this._validate();
        }
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragLeaveTarget(dragMode, dataType, data) {
        if (dataType === "HTML") {
            this._setDropCueVisible(false);
            this._validate();
        }
    }


    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragInTarget(dragMode, dataType, data) {
        if (dataType === "HTML") {
            this._setDropCueVisible(true, data);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$_setDropCueVisible(visible, dragVisual) {
        if (this._dropCueTemplate) {
            if (visible) {
                if (!this._dropCueTemplateInstance) {
                    var documentContext = document.createDocumentFragment();
                    this._dropCueTemplateInstance = this._dropCueTemplate.createInstance(documentContext).instanceElement;
                }

                var potentialNextSibling = this._findPotentialNextSibling(dragVisual);

                if (!Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._dropCueTemplateInstance)) {
                    // Add drop cue.
                    if (potentialNextSibling) {
                        this.get_element().insertBefore(this._dropCueTemplateInstance, potentialNextSibling);
                    }
                    else {
                        this.get_element().appendChild(this._dropCueTemplateInstance);
                    }

                    this._dropCueTemplateInstance.style.width = dragVisual.offsetWidth + "px";
                    this._dropCueTemplateInstance.style.height = dragVisual.offsetHeight + "px";
                }
                else {
                    // Move drop cue.
                    if (Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().getNextSibling(this._dropCueTemplateInstance) !== potentialNextSibling) {
                        this.get_element().removeChild(this._dropCueTemplateInstance);
                        if (potentialNextSibling) {
                            this.get_element().insertBefore(this._dropCueTemplateInstance, potentialNextSibling);
                        }
                        else {
                            this.get_element().appendChild(this._dropCueTemplateInstance);
                        }
                    }
                }
            }
            else {
                if (this._dropCueTemplateInstance && Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._dropCueTemplateInstance)) {
                    this.get_element().removeChild(this._dropCueTemplateInstance);
                }
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$_findPotentialNextSibling(dragVisual) {
        var dragVisualRect = Sys.UI.DomElement.getBounds(dragVisual);
        var isVertical = (this._direction === Inflectra.SpiraTest.Web.ServerControls.RepeatDirection.Vertical);
        var nodeRect;
        for (var node = this.get_element().firstChild; node !== null; node = node.nextSibling) {
            if (node.innerHTML && node !== this._dropCueTemplateInstance && node !== this._emptyTemplateInstance) {
                nodeRect = Sys.UI.DomElement.getBounds(node);
                if ((!isVertical && dragVisualRect.x <= nodeRect.x) || (isVertical && dragVisualRect.y <= nodeRect.y)) {
                    return node;
                }
            }
        }

        return null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$_validate() {
        var visible = (this._dropCueTemplateInstance == null || !Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._dropCueTemplateInstance));

        // Check if there are draggables left in this host. If not, display a placeholder.
        var count = 0;
        for (var node = this.get_element().firstChild; node !== null; node = node.nextSibling) {
            if (node.innerHTML && node !== this._emptyTemplateInstance && node !== this._dropCueTemplateInstance) {
                count++;
            }
        }

        if (count > 0) {
            visible = false;
        }
        this._setEmptyTemplateVisible(visible);
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$_setEmptyTemplateVisible(visible) {
        if (this._emptyTemplate) {
            if (visible) {
                if (!this._emptyTemplateInstance) {
                    this._emptyTemplateInstance = this._emptyTemplate.createInstance(this.get_element()).instanceElement;
                }
                else if (!Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._emptyTemplateInstance)) {
                    this.get_element().appendChild(this._emptyTemplateInstance);
                }
            }
            else {
                if (this._emptyTemplateInstance && Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._emptyTemplateInstance)) {
                    this.get_element().removeChild(this._emptyTemplateInstance);
                }
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$DragDropList$_getFloatContainer() {
        if (!this._floatContainerInstance) {
            this._floatContainerInstance = document.createElement(this.get_element().tagName);
            var none = "0px 0px 0px 0px";
            this._floatContainerInstance.style.position = "absolute";
            this._floatContainerInstance.style.padding = none;
            this._floatContainerInstance.style.margin = none;
            document.body.appendChild(this._floatContainerInstance);
        }
        else if (!Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(this._floatContainerInstance)) {
            document.body.appendChild(this._floatContainerInstance);
        }

        return this._floatContainerInstance;
    }
Inflectra.SpiraTest.Web.ServerControls.DragDropList.prototype = {
    _isDragging: null,

    _dataType: null,
    _dragMode: null,
    _dragVisual: null,
    _direction: Inflectra.SpiraTest.Web.ServerControls.RepeatDirection.Vertical,

    _emptyTemplate: null,
    _emptyTemplateInstance: null,
    _dropCueTemplate: null,
    _dropCueTemplateInstance: null,
    _floatContainerInstance: null,

    _originalParent: null,
    _originalNextSibling: null,
    _originalZIndex: null,

    _currentContext: null,
    _data: null,

    get_data: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_data,

    set_data: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_data,

    initialize: Inflectra$SpiraTest$Web$ServerControls$DragDropList$initialize,

    // -- IDragSource (related) members --

    startDragDrop: Inflectra$SpiraTest$Web$ServerControls$DragDropList$startDragDrop,

    createDragVisual: Inflectra$SpiraTest$Web$ServerControls$DragDropList$createDragVisual,

    get_emptyTemplate: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_emptyTemplate,

    set_emptyTemplate: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_emptyTemplate,

    // Type get_dragDataType()
    get_dragDataType: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dragDataType,
    set_dragDataType: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_dragDataType,

    // Object getDragData(Context)
    getDragData: Inflectra$SpiraTest$Web$ServerControls$DragDropList$getDragData,

    // DragMode get_dragMode()
    get_dragMode: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dragMode,
    set_dragMode: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_dragMode,

    dispose: Inflectra$SpiraTest$Web$ServerControls$DragDropList$dispose,

    // void onDragStart()
    onDragStart: Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragStart,

    // void onDrag()
    onDrag: Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDrag,

    // void onDragEnd(Cancelled)
    onDragEnd: Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragEnd,

    // -- IDropTarget (related) members --

    get_direction: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_direction,

    set_direction: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_direction,

    get_acceptedDataTypes: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_acceptedDataTypes,

    set_acceptedDataTypes: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_acceptedDataTypes,

    get_dropCueTemplate: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dropCueTemplate,

    set_dropCueTemplate: Inflectra$SpiraTest$Web$ServerControls$DragDropList$set_dropCueTemplate,

    get_dropTargetElement: Inflectra$SpiraTest$Web$ServerControls$DragDropList$get_dropTargetElement,

    // bool canDrop(DragMode, DataType, Data)
    canDrop: Inflectra$SpiraTest$Web$ServerControls$DragDropList$canDrop,

    // void drop(DragMode, DataType, Data)
    drop: Inflectra$SpiraTest$Web$ServerControls$DragDropList$drop,

    // void onDragEnterTarget(DragMode, DataType, Data)
    onDragEnterTarget: Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragEnterTarget,

    // void onDragLeaveTarget(DragMode, DataType, Data)
    onDragLeaveTarget: Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragLeaveTarget,

    // void onDragInTarget(DragMode, DataType, Data)
    onDragInTarget: Inflectra$SpiraTest$Web$ServerControls$DragDropList$onDragInTarget,

    _setDropCueVisible: Inflectra$SpiraTest$Web$ServerControls$DragDropList$_setDropCueVisible,

    _findPotentialNextSibling: Inflectra$SpiraTest$Web$ServerControls$DragDropList$_findPotentialNextSibling,

    _validate: Inflectra$SpiraTest$Web$ServerControls$DragDropList$_validate,

    _setEmptyTemplateVisible: Inflectra$SpiraTest$Web$ServerControls$DragDropList$_setEmptyTemplateVisible,

    _getFloatContainer: Inflectra$SpiraTest$Web$ServerControls$DragDropList$_getFloatContainer
}
Inflectra.SpiraTest.Web.ServerControls.DragDropList.descriptor = {
    properties: [   {name: 'acceptedDataTypes', type: Array},
                    {name: 'data', type: Object},
                    {name: 'dragDataType', type: String},
                    {name: 'emptyTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate},
                    {name: 'dropCueTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate},
                    {name: 'dropTargetElement', type: Object, readOnly: true},
                    {name: 'direction', type: Inflectra.SpiraTest.Web.ServerControls.RepeatDirection},
                    {name: 'dragMode', type: Inflectra.SpiraTest.Web.ServerControls.DragMode} ]
}
Inflectra.SpiraTest.Web.ServerControls.DragDropList.registerClass('Inflectra.SpiraTest.Web.ServerControls.DragDropList', Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDragSource, Inflectra.SpiraTest.Web.ServerControls.IDropTarget, Sys.IDisposable);
Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget = function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget(associatedElement) {
    Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget.initializeBase(this, [associatedElement]);
}







    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_append() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._append;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_append(value) {
        this._append = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_target() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._target;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_target(value) {
        this._target = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_property() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._property;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_property(value) {
        this._property = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_acceptedDataTypes() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._acceptedDataTypes;
    }
    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_acceptedDataTypes(value) {
        this._acceptedDataTypes = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget.callBaseMethod(this, 'initialize');
        this._control = Sys.Application.findComponent(this.get_element().id); // todo: evaluate why this behavior needs the control (see note in drop())
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);
    }

    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_dropTargetElement() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element();
    }


    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$canDrop(dragMode, dataType, data) {
        for (var i = 0; i < this._acceptedDataTypes.length; i++) {
            if (this._acceptedDataTypes[i] === dataType) {
                return true;
            }
        }

        return false;
    }


    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$drop(dragMode, type, data) {
        if (data) {
            var p;
            // note: ValueAdd: this._control is provided by initialize $find(this.get_element().id) since
            //           behaviors are not attached to controls anymore. But that means it may be null here
            //           where before that couldn't happen.
            var target = this._target ? this._target : this._control;
            if (this._append) {
                p = target["get_" + this._property];
                if (p) {
                    var targetData = p.call(target);
                    if (targetData) {
                        if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(targetData)) {
                            targetData.add(data);
                        }
                        else if (targetData instanceof Array) {
                            if(typeof(targetData.add) === "function") {
                                targetData.add(data);
                            }
                            else {
                                Array.add(targetData, data);
                            }
                        }
                    } else {
                        // Set the data.
                        p = target["set_" + this._property];
                        if (p) {
                            p.call(target, data);
                        }
                    }
                }
            }
            else {
                p = target["set_" + this._property];
                if (p) {
                    p.call(target, data);
                }
            }
        }
    }


    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$onDragEnterTarget(dragMode, type, data) {
        //
    }


    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$onDragLeaveTarget(dragMode, type, data) {
        //
    }


    function Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$onDragInTarget(dragMode, type, data) {
        //
    }
Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget.prototype = {
    _control: null,
    _acceptedDataTypes: null,
    _append: true,
    _target: null,
    _property: "data",

    get_append: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_append,

    set_append: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_append,

    get_target: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_target,

    set_target: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_target,

    get_property: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_property,

    set_property: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_property,

    get_acceptedDataTypes: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_acceptedDataTypes,
    set_acceptedDataTypes: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$set_acceptedDataTypes,

    initialize: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$initialize,

    get_dropTargetElement: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$get_dropTargetElement,

    // bool canDrop(DragMode, DataType, Data)
    canDrop: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$canDrop,

    // void drop(DragMode, DataType, Data)
    drop: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$drop,

    // void onDragEnterTarget(DragMode, DataType, Data)
    onDragEnterTarget: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$onDragEnterTarget,

    // void onDragLeaveTarget(DragMode, DataType, Data)
    onDragLeaveTarget: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$onDragLeaveTarget,

    // void onDragInTarget(DragMode, DataType, Data)
    onDragInTarget: Inflectra$SpiraTest$Web$ServerControls$DataSourceDropTarget$onDragInTarget
}
Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget.descriptor = {
    properties: [   {name: 'acceptedDataTypes', type: Array},
                    {name: 'append', type: Boolean},
                    {name: 'dropTargetElement', type: Object, readOnly: true},
                    {name: 'target', type: Object},
                    {name: 'property', type: String} ]
}
Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget.registerClass('Inflectra.SpiraTest.Web.ServerControls.DataSourceDropTarget', Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDropTarget);
Inflectra.SpiraTest.Web.ServerControls.DraggableListItem = function Inflectra$SpiraTest$Web$ServerControls$DraggableListItem(element) {
    Inflectra.SpiraTest.Web.ServerControls.DraggableListItem.initializeBase(this,[element]);
    
    var _data;
    var _handle;
    var _dragVisualTemplate;
    var _dragVisualTemplateInstance;
    
    this.get_data = function this$get_data() {
        if (_data == null) {
            var dragSource = this._findDragSource();
            if (dragSource != null && dragSource.get_dragDataType() == "HTML") {
                return this.get_element();
            }
        }
        
        return _data;
    }
    
    this.set_data = function this$set_data(value) {
        _data = value;
    }
    
    this.get_handle = function this$get_handle() {
        return _handle;
    }
    
    this.set_handle = function this$set_handle(value) {
        if (_handle != null) {
            Sys.UI.DomEvent.removeHandler(_handle, "mousedown", this._handleMouseDown);
            _handle.__draggableBehavior = null;
        }

        if (value.element) {
            value = value.element;
        }
        _handle = value;
        _handle.__draggableBehavior = this;
        
        Sys.UI.DomEvent.addHandler(_handle, "mousedown", this._handleMouseDown);
        _handle.__draggableBehavior = this;
    }
    
    this.get_dragVisualTemplate = function this$get_dragVisualTemplate() {
        return _dragVisualTemplate;
    }
    
    this.set_dragVisualTemplate = function this$set_dragVisualTemplate(value) {
        _dragVisualTemplate = value;
    }
       
    this._handleMouseDown = function this$_handleMouseDown(e) {
        window._event = e;
        _handle.__draggableBehavior._handleMouseDownInternal();
    }
    
    this._handleMouseDownInternal = function this$_handleMouseDownInternal() {
        var ev = window._event;
        if (ev.button <= 1) {
            var dragSource = this._findDragSource();
            if (dragSource != null) {
                var dragVisual = this._createDragVisual();
                dragSource.startDragDrop(this.get_element(), this.get_data(), dragVisual);
                ev.preventDefault();
            }
        }
    }
    
    this._createDragVisual = function this$_createDragVisual() {
        var ev = window._event;
        if (_dragVisualTemplate != null) {
            if (_dragVisualTemplateInstance == null) {
                _dragVisualTemplateInstance = _dragVisualTemplate.createInstance(this.get_element()).instanceElement;
            }
            else if (!Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().hasParent(_dragVisualTemplateInstance)) {
                this.get_element().appendChild(_dragVisualTemplateInstance);
            }
            
            var location = { x: ev.clientX, y: ev.clientY };
            location = Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().addPoints(location, Inflectra.SpiraTest.Web.ServerControls.DragDropManager._getInstance().getScrollOffset(document.body, true));
            Sys.UI.DomElement.setLocation(_dragVisualTemplateInstance, location.x, location.y);
        }
        return _dragVisualTemplateInstance;
    }
    
    this._findDragSource = function this$_findDragSource() {
        var element = this.get_element();
        while (element != null) {
            if (element.__dragDropList != null) {
                return element.__dragDropList;
            }
            element = element.parentNode;
        }
        return null;
    }
}
Inflectra.SpiraTest.Web.ServerControls.DraggableListItem.descriptor = {
    properties: [   {name: 'data', type: Object},
                    {name: 'handle', isDomElement: true},
                    {name: 'dragVisualTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate} ]
}
Inflectra.SpiraTest.Web.ServerControls.DraggableListItem.registerClass('Inflectra.SpiraTest.Web.ServerControls.DraggableListItem', Sys.UI.Behavior);
Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior = function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior(element) {
    Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior.initializeBase(this,[element]);
    this._mouseDownHandler = Function.createDelegate(this, this.mouseDownHandler);
}







    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$add_move(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().addHandler('move', handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$remove_move(handler) {
var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
if (e) throw e;

        this.get_events().removeHandler('move', handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_handle() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._handle;
    }
    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_handle(value) {
        if (this._handle) {
            Sys.UI.DomEvent.removeHandler(this._handle, "mousedown", this._mouseDownHandler);
        }
        this._handle = value;
        Sys.UI.DomEvent.addHandler(this._handle, "mousedown", this._mouseDownHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_profileProperty() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._profileProperty;
    }
    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_profileProperty(value) {
        Sys.Debug.assert(!this.get_isInitialized() || this._profileProperty === value, "You cannot change the profile property after initialization.");
        this._profileProperty = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_profileComponent() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._profileComponent;
    }
    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_profileComponent(value) {
        this._profileComponent = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_location() {
if (arguments.length !== 0) throw Error.parameterCount();
        return this._location;
    }
    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_location(value) {
        if (this._location != value) {
            this._location = value;
            if (this.get_isInitialized()) {
                var numbers = this._location.split(',');
                var location = { x : parseInt(numbers[0]), y : parseInt(numbers[1]) };
                Sys.UI.DomElement.setLocation(this.get_element(), location.x, location.y);
            }
            this.raisePropertyChanged('location');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior.callBaseMethod(this, 'initialize');
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.registerDropTarget(this);

        var el = this.get_element();

        var location;
        if (this._location) {
            var numbers = this._location.split(',');
            location = { x : parseInt(numbers[0]), y : parseInt(numbers[1]) };
        }
        else {
            location = Sys.UI.DomElement.getLocation(el);
        }

        if(el.offsetWidth) {
            el.style.width = el.offsetWidth + "px";
        }
        if(el.offsetHeight) {
            el.style.height = el.offsetHeight + "px";
        }
        el.style.position = "absolute";
        Sys.UI.DomElement.setLocation(el, location.x, location.y);

        var p = this.get_profileProperty();
        if(p) {
            var b = new Inflectra.SpiraTest.Web.AjaxExtensions.Binding();
            b.beginUpdate();
            b.set_target(this);
            b.set_property("location");
            var profile = this.get_profileComponent();
            if(!profile) profile = Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.instance;
            b.set_dataContext(profile);
            b.set_dataPath(p);
            b.set_direction(Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.InOut);

            // we must hook into the loaded event since the profile may be loaded and the location property
            // will be different. But profile doesnt raise a change notificaiton for every property after a load
            var a = new Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction();
            a.beginUpdate();
            a.set_eventSource(profile);
            a.set_eventName("loadComplete");
            a.set_target(b);
            a.set_method("evaluateIn");

            a.endUpdate();
            b.endUpdate();

            this._binding = b;
            this._action = a;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$dispose() {
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.unregisterDropTarget(this);
        if (this._handle && this._mouseDownHandler) {
            Sys.UI.DomEvent.removeHandler(this._handle, "mousedown", this._mouseDownHandler);
        }
        this._mouseDownHandler = null;
        Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$checkCanDrag(element) {
        var undraggableTagNames = ["input", "button", "select", "textarea", "label"];
        var tagName = element.tagName;

        if ((tagName.toLowerCase() == "a") && (element.href != null) && (element.href.length > 0)) {
            return false;
        }
        if (Array.indexOf(undraggableTagNames, tagName.toLowerCase()) > -1) {
            return false;
        }
        return true;
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$mouseDownHandler(ev) {
        window._event = ev;
        var el = this.get_element();

        if (this.checkCanDrag(ev.target)) {
            this._dragStartLocation = Sys.UI.DomElement.getLocation(el);

            ev.preventDefault();
            this.startDragDrop(el);
        }
    }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_dragDataType() {
if (arguments.length !== 0) throw Error.parameterCount();
        return "_floatingObject";
    }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$getDragData(context) {
        return null;
    }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_dragMode() {
if (arguments.length !== 0) throw Error.parameterCount();
        return Inflectra.SpiraTest.Web.ServerControls.DragMode.Move;
    }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragStart() { }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDrag() { }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragEnd(canceled) {
        if (!canceled) {
            var handler = this.get_events().getHandler('move');
            if(handler) {
                var cancelArgs = new Sys.CancelEventArgs();
                handler(this, cancelArgs);
                canceled = cancelArgs.get_cancel();
            }
        }

        var el = this.get_element();
        if (canceled) {
            // Restore the position of the control.
            Sys.UI.DomElement.setLocation(el, this._dragStartLocation.x, this._dragStartLocation.y);
        }
        else {
            var location = Sys.UI.DomElement.getLocation(el);
            this._location = location.x + ',' + location.y;
            this.raisePropertyChanged('location');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$startDragDrop(dragVisual) {
        Inflectra.SpiraTest.Web.ServerControls.DragDropManager.startDragDrop(this, dragVisual, null);
    }

    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_dropTargetElement() {
if (arguments.length !== 0) throw Error.parameterCount();
        return document.body;
    }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$canDrop(dragMode, dataType, data) {
        return (dataType === "_floatingObject");
    }


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$drop(dragMode, dataType, data) {}


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragEnterTarget(dragMode, dataType, data) {}


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragLeaveTarget(dragMode, dataType, data) {}


    function Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragInTarget(dragMode, dataType, data) {}
Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior.prototype = {
    _handle: null,
    _location: null,
    _dragStartLocation: null,
    _profileProperty: null,
    _profileComponent: null,

    add_move: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$add_move,
    remove_move: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$remove_move,

    get_handle: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_handle,
    set_handle: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_handle,

    get_profileProperty: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_profileProperty,
    set_profileProperty: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_profileProperty,

    get_profileComponent: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_profileComponent,
    set_profileComponent: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_profileComponent,

    get_location: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_location,
    set_location: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$set_location,

    initialize: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$initialize,

    dispose: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$dispose,

    checkCanDrag: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$checkCanDrag,

    mouseDownHandler: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$mouseDownHandler,

    // Type get_dataType()
    get_dragDataType: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_dragDataType,

    // Object get_data(Context)
    getDragData: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$getDragData,

    // DragMode get_dragMode()
    get_dragMode: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_dragMode,

    // void onDragStart()
    onDragStart: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragStart,

    // void onDrag()
    onDrag: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDrag,

    // void onDragEnd(Canceled)
    onDragEnd: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragEnd,

    startDragDrop: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$startDragDrop,

    get_dropTargetElement: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$get_dropTargetElement,

    // bool canDrop(DragMode, DataType, Data)
    canDrop: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$canDrop,

    // void drop(DragMode, DataType, Data)
    drop: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$drop,

    // void onDragEnterTarget(DragMode, DataType, Data)
    onDragEnterTarget: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragEnterTarget,

    // void onDragLeaveTarget(DragMode, DataType, Data)
    onDragLeaveTarget: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragLeaveTarget,

    // void onDragInTarget(DragMode, DataType, Data)
    onDragInTarget: Inflectra$SpiraTest$Web$ServerControls$FloatingBehavior$onDragInTarget
}
Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior.descriptor = {
    properties: [   {name: "profileProperty", type: String},
                    {name: "profileComponent", type: Object},
                    {name: "dragData", type: Object, readOnly: true},
                    {name: "dragDataType", type: String, readOnly: true},
                    {name: "dragMode", type: Inflectra.SpiraTest.Web.ServerControls.DragMode, readOnly: true},
                    {name: "dropTargetElement", type: Object, readOnly: true},
                    {name: "handle", isDomElement: true},
                    {name: "location", type: String} ],
    events: [   {name: "move"} ]
}
Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.FloatingBehavior', Sys.UI.Behavior, Inflectra.SpiraTest.Web.ServerControls.IDragSource, Inflectra.SpiraTest.Web.ServerControls.IDropTarget, Sys.IDisposable);

Sys.UI.Control.overlaps = function Sys$UI$Control$overlaps(r1, r2) {
    var xLeft = (r1.x >= r2.x && r1.x <= (r2.x + r2.width));
    var xRight = ((r1.x + r1.width) >= r2.x && (r1.x + r1.width) <= r2.x + r2.width);
    var xComplete = ((r1.x < r2.x) && ((r1.x + r1.height) > (r2.x + r2.height)));
    
    var yLeft = (r1.y >= r2.y && r1.y <= (r2.y + r2.height));
    var yRight = ((r1.y + r1.height) >= r2.y && (r1.y + r1.height) <= r2.y + r2.height);
    var yComplete = ((r1.y < r2.y) && ((r1.y + r1.height) > (r2.y + r2.height)));
    if ((xLeft || xRight || xComplete) && (yLeft || yRight || yComplete)) {
        return true;
    }
   
    return false;
}


if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();
