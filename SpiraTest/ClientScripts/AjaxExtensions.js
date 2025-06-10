// Keep CTP DataContext logic here so it gets copied to descendants.  Only 1.0 Components to extend
// are Behavior and Control; don't need dataContext on Application since it wasn't Component in CTP.
Sys.Component.prototype.get_dataContext = function Sys$Component$get_dataContext() {
	return (this._dataContext || null);
}

Sys.Component.prototype.set_dataContext = function Sys$Component$set_dataContext(value) {
	this._dataContext = value;
}

Sys.UI.Control.prototype.get_dataContext = function Sys$UI$Control$get_dataContext() {
    var dc = Sys.UI.Control.callBaseMethod(this, 'get_dataContext');
    if (!dc) {
        var parent = this.get_parent();
        if (parent) {
            dc = parent.get_dataContext();
        }
    }
    return dc;
}
Sys.UI.Control.prototype.set_dataContext = Sys.Component.prototype.set_dataContext;

Sys.UI.Behavior.prototype.get_dataContext = function Sys$UI$Behavior$get_dataContext() {
    var dc = Sys.UI.Behavior.callBaseMethod(this, 'get_dataContext');
    if (!dc) {
        if (this.control) {
            dc = this.control.get_dataContext();
        }
        else {
			var e = this.get_element();
			if(e) {
				// todo: if no e.control we could always use the parenting logic to find an element
				//		 in the DOM that parents e that does have a control on it, then call get_dataContext on that.
				var c = e.control;
				if(c) dc = c.get_dataContext();
			}
        }
    }
    return dc;
}
Sys.UI.Behavior.prototype.set_dataContext = Sys.Component.prototype.set_dataContext;

// AtlasMozilla.js
// Atlas Framework Core (Mozilla-specific).
//
// This adds APIs when running under Mozilla used by other parts of the
// Atlas framework.
//

function _loadMozillaCompatLayer(w) {

    // innerText is still used by Label.
    w.HTMLElement.prototype.__defineGetter__('innerText', function() {
            return this.textContent;
        });
    w.HTMLElement.prototype.__defineSetter__('innerText', function(v) {
            if (v) {
                this.innerHTML = formatPlainTextAsHtml(v);
            }
            else {
                this.innerHTML = '';
            }
        });

    // Copied from HttpServerUtility.FormatPlainTextAsHtml()
    function formatPlainTextAsHtml(str) {
        var sb = new Sys.StringBuilder();

        var numChars = str.length;
        var prevCh;

        for (var i=0; i < numChars; i++) {
            var ch = str.charAt(i);
            switch (ch) {
                case "<":
                    sb.append("&lt;");
                    break;
                case ">":
                    sb.append("&gt;");
                    break;
                case "\"":
                    sb.append("&quot;");
                    break;
                case "&":
                    sb.append("&amp;");
                    break;
                case " ":
                    if (prevCh == " ") {
                        sb.append("&nbsp;");
                    }
                    else {
                        sb.append(" ");
                    }
                    break;
                case "\r":
                    // Ignore \r, only handle \n
                    break;
                case "\n":
                    // Insert line breaks before and after the <br>, so the HTML looks better.
                    sb.appendLine();
                    sb.appendLine("<br />");
                    break;
                default:
                    sb.append(ch);
                    break;
            }

            prevCh = ch;
        }

        return sb.toString();
    }
    
    function selectNodes(doc, path, contextNode) {
        contextNode = contextNode ? contextNode : doc;
        var xpath = new XPathEvaluator();
        var result = xpath.evaluate(path, contextNode,
                                    doc.createNSResolver(doc.documentElement),
                                    XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);

        var nodeList = new Array(result.snapshotLength);
        for(var i = 0; i < result.snapshotLength; i++) {
            nodeList[i] = result.snapshotItem(i);
        }

        return nodeList;
    }

    function selectSingleNode(doc, path, contextNode) {
        path += '[1]';
        var nodes = selectNodes(doc, path, contextNode);
        if (nodes.length != 0) {
            for (var i = 0; i < nodes.length; i++) {
                if (nodes[i]) {
                    return nodes[i];
                }
            }
        }
        return null;
    }

    w.XMLDocument.prototype.selectNodes = function w$XMLDocument$selectNodes(path, contextNode) {
        return selectNodes(this, path, contextNode);
    }

    w.XMLDocument.prototype.selectSingleNode = function w$XMLDocument$selectSingleNode(path, contextNode) {
        return selectSingleNode(this, path, contextNode);
    }

    w.XMLDocument.prototype.transformNode = function w$XMLDocument$transformNode(xsl) {
        var xslProcessor = new XSLTProcessor();
        xslProcessor.importStylesheet(xsl);

        var ownerDocument = document.implementation.createDocument("", "", null);
        var transformedDoc = xslProcessor.transformToDocument(this);

        return transformedDoc.xml;
    }

    Node.prototype.selectNodes = function Node$selectNodes(path) {
        var doc = this.ownerDocument;
        return doc.selectNodes(path, this);
    }

    Node.prototype.selectSingleNode = function Node$selectSingleNode(path) {
        var doc = this.ownerDocument;
        return doc.selectSingleNode(path, this);
    }

    Node.prototype.__defineGetter__('baseName', function() {
        return this.localName;
    });

    Node.prototype.__defineGetter__('text', function() {
        return this.textContent;
    });
    Node.prototype.__defineSetter__('text', function(value) {
        this.textContent = value;
    });

    Node.prototype.__defineGetter__('xml', function() {
        return (new XMLSerializer()).serializeToString(this);
    });

    DocumentFragment.prototype.getElementById = function DocumentFragment$getElementById(id) {
        var nodeQueue = [];
        var childNodes = this.childNodes;
        var node;
        var c;

        for (c = 0; c < childNodes.length; c++) {
            node = childNodes[c];
            if (node.nodeType == 1) {
                Array.enqueue(nodeQueue, node);
            }
        }

        while (nodeQueue.length) {
            node = Array.dequeue(nodeQueue);
            if (node.id == id) {
                return node;
            }
            childNodes = node.childNodes;
            if (childNodes.length != 0) {
                for (c = 0; c < childNodes.length; c++) {
                    node = childNodes[c];
                    if (node.nodeType == 1) {
                        Array.enqueue(nodeQueue, node);
                    }
                }
            }
        }

        return null;
    }

    DocumentFragment.prototype.createElement = function DocumentFragment$createElement(tagName) {
        return document.createElement(tagName);
    }
}
// AtlasTypeDescriptor.js
// Atlas Framework Core.
// Used by Safari -- it lowercases attribute names,
// so we must store regular and lowercased versions of properties & events.

function _loadTypeDescriptorCompatLayer(w) {
    Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype._addEvent = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addEvent;
    Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype._addProperty = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addProperty;

    Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addEvent = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$addEvent(eventName) {
        this._addEvent(eventName);
        var lcEventName = eventName.toLowerCase();
        if (eventName != lcEventName) {
            this._addEvent(lcEventName);
            this._getEvents()[lcEventName].name = eventName;
        }
    }

    Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addProperty = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$addProperty(propertyName, propertyType, readOnly, isDomElement) {
        var propInfo = this._addProperty.apply(this, arguments);
        var lcPropertyName = propertyName.toLowerCase();
        if (propertyName !== lcPropertyName) {
            var baseArguments = [];
            Array.add(baseArguments, lcPropertyName);
            for (var a = 1; a < arguments.length; a++) {
                Array.add(baseArguments, arguments[a]);
            }
            this._addProperty.apply(this, baseArguments);
            this._getProperties()[lcPropertyName].name = propertyName;
        }
        return propInfo;
    }
}
if (Sys.Browser.agent === Sys.Browser.Firefox) {
    _loadMozillaCompatLayer(window);
}

Type.registerNamespace('Inflectra.SpiraTest.Web.AjaxExtensions');

Inflectra.SpiraTest.Web.AjaxExtensions.IAction = function Inflectra$SpiraTest$Web$AjaxExtensions$IAction() {
    throw Error.notImplemented();
}


    function Inflectra$SpiraTest$Web$AjaxExtensions$IAction$execute() {
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$IAction$setOwner() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.IAction.prototype = {
    
    execute: Inflectra$SpiraTest$Web$AjaxExtensions$IAction$execute,
    
    setOwner: Inflectra$SpiraTest$Web$AjaxExtensions$IAction$setOwner
}
Inflectra.SpiraTest.Web.AjaxExtensions.IAction.registerInterface('Inflectra.SpiraTest.Web.AjaxExtensions.IAction');
//////////////////////////////////////////////////////////////////////////////
// Attributes

Inflectra.SpiraTest.Web.AjaxExtensions.Attributes = new function() {

    this.defineAttribute = function this$defineAttribute(attributeName) {
        this[attributeName] = attributeName;
    }
}
//////////////////////////////////////////////////////////////////////////////
// TypeDescriptor

// REVIEW: We need to review our type descriptor design... eventually I
//         didn't add PropertyDescriptor, EventDesriptor and MethodDescriptor
//         for script size/perf. Instead we have getProperty, setProperty
//         etc. that do the lookup for the descriptor, and the functionality
//         to get/set/invoke etc.
//         But as we build the framework, we find the need to also have
//         the descriptor be a public object (eg. in BindingEventArgs), just
//         like it is in the framework.
//         If we made it a public object, get/set value would go on
//         PropertyDescriptor, add/remove would be on EventDescriptor, and
//         invoke would go into MethodDescriptor.

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor() {
    var _properties = { };
    var _events = { };
    var _methods = { };
    var _attributes = { };

    this._getAttributes = function this$_getAttributes() {
        return _attributes;
    }

    this._getEvents = function this$_getEvents() {
        return _events;
    }

    this._getMethods = function this$_getMethods() {
        return _methods;
    }

    this._getProperties = function this$_getProperties() {
        return _properties;
    }
}
Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor');

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addAttribute = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$addAttribute(attributeName, attributeValue) {
    /// <param name="attributeName" type="String"></param>
    /// <param name="attributeValue" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "attributeName", type: String},
        {name: "attributeValue", type: String}
    ]);
    if (e) throw e;

    this._getAttributes()[attributeName] = attributeValue;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addEvent = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$addEvent(eventName) {
    /// <param name="eventName" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "eventName", type: String}
    ]);
    if (e) throw e;

    return this._getEvents()[eventName] = { name: eventName };
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addMethod = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$addMethod(methodName, associatedParameters) {
    /// <param name="methodName" type="String"></param>
    /// <param name="associatedParameters" type="Array" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "methodName", type: String},
        {name: "associatedParameters", type: Array, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if (associatedParameters) {
        // make sure there are no duplicate parameters or parameters that only vary by casing
        var names = {};
        for (var i=0, l=associatedParameters.length; i < l; i++) {
            var paramInfo = associatedParameters[i];
            var name = paramInfo.name;
            if (!name) {
                throw Error.argument("associatedParameters", "Method '{0}' has an invalid parameter list.");
            }
            var lcName = name.toLowerCase();
            if (names[name]) {
                throw Error.argument("associatedParameters",
                    String.format("Method '{0}' has duplicate parameters.", methodName));
            }
            else if (names[lcName]) {
                throw Error.argument("associatedParameters",
                    String.format("Method '{0}' has parameters that only vary by case.", methodName));
            }
            names[name] = true;
        }
    }
    return this._getMethods()[methodName] = { name: methodName, parameters: associatedParameters };
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.prototype.addProperty = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$addProperty(propertyName, propertyType, readOnly, isDomElement, associatedAttributes) {
    /// <param name="propertyName" type="String"></param>
    /// <param name="propertyType" type="Type" mayBeNull="true"></param>
    /// <param name="readOnly" type="Boolean" optional="true"></param>
    /// <param name="isDomElement" type="Boolean" optional="true"></param>
    /// <param name="associatedAttributes" parameterArray="true" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "propertyName", type: String},
        {name: "propertyType", type: Type, mayBeNull: true},
        {name: "readOnly", type: Boolean, optional: true},
        {name: "isDomElement", type: Boolean, optional: true},
        {name: "associatedAttributes", mayBeNull: true, optional: true, parameterArray: true}
    ]);
    if (e) throw e;

    if (propertyType === Sys.UI.DomElement) {
        // TODO: localize the message. But, the rest of ValueAdd is not localized either.
        throw Error.argumentType("propertyType", Sys.UI.DomElement, Object, "Use isDomElement with a null type for element properties.\ne.g., for descriptors use { name: 'foo', isDomElement: true, type: null }");
    }
    
    readOnly = !!readOnly;
    var attribs;
    if (associatedAttributes) {
        attribs = { };
        for (var i = 4; i < arguments.length; i += 2) {
            var attribute = arguments[i];
            var value = arguments[i + 1];
            attribs[attribute] = value;
        }
    }
    return this._getProperties()[propertyName] = { name: propertyName, type: propertyType, 'readOnly': readOnly, 'isDomElement': isDomElement, attributes: attribs };
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.createParameter = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$createParameter(parameterName, parameterType, isDomElement, isInteger) {
    /// <param name="parameterName" type="String"></param>
    /// <param name="parameterType" type="Type" mayBeNull="true"></param>
    /// <param name="isDomElement" type="Boolean" optional="true"></param>
    /// <param name="isInteger" type="Boolean" optional="true"></param>
    /// <returns type="Object"></returns>
    var e = Function._validateParams(arguments, [
        {name: "parameterName", type: String},
        {name: "parameterType", type: Type, mayBeNull: true},
        {name: "isDomElement", type: Boolean, optional: true},
        {name: "isInteger", type: Boolean, optional: true}
    ]);
    if (e) throw e;

    return { name: parameterName, type: parameterType, 'isDomElement': !!isDomElement, 'isInteger': !!isInteger };
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$getTypeDescriptor(instance) {
    /// <param name="instance" type="Object"></param>
    /// <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object}
    ]);
    if (e) throw e;

    if (instance === null) {
        throw Error.createError('instance is null in TypeDescriptor.getTypeDescriptor');
    }

    var type = Object.getType(instance);
    var td = type._descriptor;
    if (!td && !type._descriptorChecked) {
        if (Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider.isImplementedBy(instance)) {
            td = instance.getDescriptor();
            Sys.Debug.assert(!!td, String.format('Failed to get type descriptor for instance of type "{0}"', type.getName()));
        }
        else {
            // note: allows no type descriptor -- may return null for script types
            td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.generateDescriptor(type);
        }
        type._descriptor = td;
        type._descriptorChecked = true;
    }

    return td;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.generateBaseDescriptor = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$generateBaseDescriptor(instance) {
    /// <param name="instance" type="Object"></param>
    /// <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object}
    ]);
    if (e) throw e;

    var baseType = instance.getBaseType();
    return Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.generateDescriptor(baseType);
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.generateDescriptor = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$generateDescriptor(type) {
    /// <param name="type" type="Sys.Type"></param>
    /// <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Sys.Type}
    ]);
    if (e) throw e;

    var td = null;
    var current = type;
    while(current) {
        if(current.descriptor) {
            if(!td) td = new Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor();
            Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.append(td, current.descriptor);
        }
        current = current.getBaseType();
    }
    return td;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.append = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$append(td, descriptor) {
    /// <param name="td" type="Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor"></param>
    /// <param name="descriptor" type="Object"></param>
    var e = Function._validateParams(arguments, [
        {name: "td", type: Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor},
        {name: "descriptor", type: Object}
    ]);
    if (e) throw e;

    if (descriptor.properties) {
        var length = descriptor.properties.length;
        for (var i = 0; i < length; i++) {
            var property = descriptor.properties[i];
            var propertyName = property.name;
            var associatedAttributes = property.attributes;
            var readOnly = !!(property.readOnly);
            var isDomElement = !!(property.isDomElement);
            var isInteger = !!(property.isInteger);
            if (! td._getProperties()[propertyName]) {
                // create array of arguments
                var args = [propertyName, property.type, readOnly, isDomElement];
                if(typeof(associatedAttributes) === 'array') {
                    for(var j=0, l = associatedAttributes.length; j < l; j++) {
                        var attrib = associatedAttributes[j];
                        args[args.length] = attrib.name;
                        args[args.length] = attrib.value;
                    }
                }
                var propInfo = td.addProperty.apply(td, args);
                //td.addProperty(propertyName, property.type, readOnly, isDomElement, < ...n number of attribute/value pairs... >);
                propInfo.isInteger = isInteger;
            }
        }
    }
    if (descriptor.events) {
        var length = descriptor.events.length;
        for (var i = 0; i < length; i++) {
            var eventName = descriptor.events[i].name
            if (! td._getEvents()[eventName]) {
                td.addEvent(eventName);
            }
        }
    }
    if (descriptor.methods) {
        var length = descriptor.methods.length;
        for (var i = 0; i < length; i++) {
            var methodName = descriptor.methods[i].name;
            if (! td._getMethods()[methodName]) {
                //TODO: what to do w/ descriptor.methods[i].type
                var params = descriptor.methods[i].params;
                if(!params) params = descriptor.methods[i].parameters;
                if (params) {
                    td.addMethod(methodName, params);
                }
                else {
                    td.addMethod(methodName);
                }
            }
        }
    }
    if (descriptor.attributes) {
        var length = descriptor.attributes.length;
        for (var i = 0; i < length; i++) {
            var attributeName = descriptor.attributes[i].name
            if (! td._getAttributes()[attributeName]) {
                td.addAttribute(attributeName, descriptor.attributes[i].value);
            }
        }
    }
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.unload = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$unload() {
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getAttribute = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$getAttribute(instance, attributeName) {
    /// <param name="instance" type="Object"></param>
    /// <param name="attributeName" type="String"></param>
    /// <returns type="Object"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object},
        {name: "attributeName", type: String}
    ]);
    if (e) throw e;

    if (instance === null) {
        throw Error.createError('instance is null in TypeDescriptor.getAttribute');
    }

    var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(instance);
    Sys.Debug.assert(!!td, 'Attributes are only supported for types with a type descriptor');

    return td._getAttributes()[attributeName];
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$getProperty(instance, propertyName, key) {
    /// <param name="instance" type="Object"></param>
    /// <param name="propertyName" type="String" mayBeNull="true"></param>
    /// <param name="key" optional="true" mayBeNull="true"></param>
    /// <returns type="Object"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object},
        {name: "propertyName", type: String, mayBeNull: true},
        {name: "key", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if (instance === null) {
        throw Error.createError('instance is null in TypeDescriptor.getProperty');
    }

    if (Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.isImplementedBy(instance)) {
        return instance.getProperty(propertyName, key);
    }
    Sys.Debug.assert(!!propertyName, "Property name was not specified");

    if ((propertyName === null) || (propertyName.length === 0)) {
        throw Error.createError('propertyName is null');
    }

    var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(instance);
    if (!td) {
        // Plain script object
        var field = instance[propertyName];
        if (field && key) {
            field = key.indexOf('.') === -1 ? (field[key]) : (Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._evaluatePath(field, key));
        }
        return field;
    }

    var propertyInfo = td._getProperties()[propertyName];
    Sys.Debug.assert(!!propertyInfo, String.format('Property "{0}" not found on object of type "{1}"', propertyName, Object.getTypeName(instance)));

    var getter = instance['get_' + propertyInfo.name];
    Sys.Debug.assert(!!getter, String.format('Get accessor was not found for "{0}" property on object of type "{1}"', propertyName, Object.getTypeName(instance)));

    var object = getter.call(instance);
    if (key) {
        object = key.indexOf('.') === -1 ? (object[key]) : (Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._evaluatePath(object, key));
    }

    return object;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.setProperty = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$setProperty(instance, propertyName, value, key) {
    /// <param name="instance" type="Object" mayBeNull="false"></param>
    /// <param name="propertyName" type="String" mayBeNull="true"></param>
    /// <param name="value" mayBeNull="true"></param>
    /// <param name="key" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object},
        {name: "propertyName", type: String, mayBeNull: true},
        {name: "value", mayBeNull: true},
        {name: "key", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if (instance === null) {
        throw Error.createError('instance is null in TypeDescriptor.setProperty');
    }

    if (Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.isImplementedBy(instance)) {
        instance.setProperty(propertyName, value, key);
        return;
    }
    if(!propertyName || propertyName.length === 0) {
        throw Error.invalidOperation("Property name was not specified.");
    }

    var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(instance);
    if (!td) {
        // Plain script object
        if(!key) {
            instance[propertyName] = value;
        }
        else {
            instance = instance[propertyName];
            if(key.indexOf('.') === -1) {
                instance[key] = value;
            }
            else {
                Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._setPath(instance, key, value);
            }
        }
        return;
    }

    var propertyInfo = td._getProperties()[propertyName];
    if(!propertyInfo) {
        throw Error.invalidOperation(String.format('Property "{0}" not found on object of type "{1}"', propertyName, Object.getTypeName(instance)));
    }

    if (key) {
        var getter = instance['get_' + propertyInfo.name];
        if(!getter) {
            throw Error.invalidOperation(String.format('Get accessor was not found for "{0}" property on object of type "{1}"', propertyInfo.name, Object.getTypeName(instance)));
        }
        var object = getter.call(instance);

        if(key.indexOf('.') === -1) {
            object[key] = value;
        }
        else {
            Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._setPath(object, key, value);
        }
    }
    else {
        var setter = instance['set_' + propertyInfo.name];
        if(!setter) {
            throw Error.invalidOperation(String.format('Set accessor was not found for "{0}" property on object of type "{1}"', propertyInfo.name, Object.getTypeName(instance)));
        }

        // convert given value to the appropriate type
        value = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._evaluateValue(propertyInfo.type, propertyInfo.isDomElement, propertyInfo.isInteger, value);

        setter.call(instance, value);
    }
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.invokeMethod = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$invokeMethod(instance, methodName, parameters) {
    /// <param name="instance" type="Object"></param>
    /// <param name="methodName" type="String"></param>
    /// <param name="parameters" type="Object" mayBeNull="true"></param>
    /// <returns type="Object"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object},
        {name: "methodName", type: String},
        {name: "parameters", type: Object, mayBeNull: true}
    ]);
    if (e) throw e;

    if (instance === null) {
        throw Error.createError('instance is null in TypeDescriptor.invokeMethod');
    }

    if (Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.isImplementedBy(instance)) {
        return instance.invokeMethod(methodName, parameters);
    }

    var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(instance);
    if (!td) {
        // Plain script object
        Sys.Debug.assert(!parameters || !parameters.length, 'Parameters are not supported for methods on intrinsic objects');
        return instance[methodName].call(instance);
    }

    var methodInfo = td._getMethods()[methodName];
    Sys.Debug.assert(!!methodInfo, String.format('Method "{0}" not found on object of type "{1}"', methodName, Object.getTypeName(instance)));

    var method = instance[methodInfo.name];

    // NOTE: theres currently no way to specify optional or required parameters.
    // July CTP code attempted to Sys.Debug.assert that the number of parameters given matches the number of parameters the method
    // has, but it didn't work because it used 'parameters.length' -- parameters is a dictionary not an array.
    // Now we just apply as many parameters as we can -- missing params will be passed as undefined.
    if (!parameters || !methodInfo.parameters || !methodInfo.parameters.length) {
        return method.call(instance);
    }
    else {
        var arguments = [];
        for (var i = 0; i < methodInfo.parameters.length; i++) {
            var parameterInfo = methodInfo.parameters[i];
            var value = parameters[parameterInfo.name];
            if (typeof(value) === "undefined") {
                value = parameters[parameterInfo.name.toLowerCase()];
            }

            // convert given value to the appropriate type
            value = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._evaluateValue(parameterInfo.type, parameterInfo.isDomElement, parameterInfo.isInteger, value);

            arguments[i] = value;
        }

        return method.apply(instance, arguments);
    }
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getPropertyType = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$getPropertyType(instance, propertyName, key) {
    /// <param name="instance" type="Object"></param>
    /// <param name="propertyName" type="String"></param>
    /// <param name="key" optional="true" mayBeNull="true"></param>
    /// <returns type="Type"></returns>
    var e = Function._validateParams(arguments, [
        {name: "instance", type: Object},
        {name: "propertyName", type: String},
        {name: "key", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    if (instance === null) {
        throw Error.createError('instance is null in TypeDescriptor.getPropertyType');
    }

    if (Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.isImplementedBy(instance)) {
        return Object;
    }

    if (key) {
        return Object;
    }

    if ((propertyName === null) || (propertyName.length === 0)) {
        throw Error.createError('propertyName is null');
    }

    var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(instance);
    if(!td) return Object;

    var propertyInfo = td._getProperties()[propertyName];
    Sys.Debug.assert(!!propertyInfo, String.format('Property "{0}" not found on object of type "{1}"', propertyName, Object.getTypeName(instance)));

    return propertyInfo.type || null;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._evaluatePath = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$_evaluatePath(instance, path) {
    var part;
    var parts = path.split('.');
    var current = instance;
    for(var i=0, l = parts.length; i < l; i++) {
        part = parts[i];
        current = current[part];
        if(typeof(current) === 'undefined' || current === null) return null;
    }
    return current;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._evaluateValue = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$_evaluateValue(targetType, isDomElement, isInteger, value) {
    // try to coerce the value to the appropriate type, if it makes sense    
    // value is null or undefined, leave it as is.
    // target type is unspecified, leave it as is
    if(!targetType) {
        return value;
    }

    var valueType = typeof(value);
    if(valueType === "undefined" || value === null) {
        return value;
    }

    if(isDomElement) {
        // dom element
        if(valueType === "string") {
            value = Sys.UI.DomElement.getElementById(value);
        }
    }
    else if(targetType === Object || targetType === Sys.Component || targetType.inheritsFrom(Sys.Component)) {
        // reference to another component
        if(valueType === "string") {
            value = Sys.Application.findComponent(value);
        }
    }
    else {
        if(targetType !== String && valueType === "string") {
            // must parse the given string to the correct value
            if(Type.isEnum(targetType)) {
                // use case insensitive parse for enums
                value = targetType.parse(value, true);
            }
            else {
                if(value === "" && targetType === Number) {
                    value = 0;
                }
                else {
                    value = (targetType.parseInvariant || targetType.parse)(value);
                    if (targetType === Number && isInteger) {
                        value = Math.floor(value);
                    }
                }
            }
        }
        else if(targetType === String && valueType !== "string") {
            // must convert the given value to string
            value = value.toString();
        }
        else if(targetType === Number && isInteger) {
            value = Math.floor(value);
        }
    }

    return value;
}

Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor._setPath = function Inflectra$SpiraTest$Web$AjaxExtensions$TypeDescriptor$_setPath(instance, path, value) {
    var current = instance;
    var parts = path.split('.');
    var part;
    for(var i=0; i < parts.length-1; i++) {
        part = parts[i];
        current = current[part];
        if(!current) break;
    }
    if(current) {
        current[parts[parts.length-1]] = value;
    }
}

if(Sys.Browser.agent === Sys.Browser.Safari) {
	_loadTypeDescriptorCompatLayer(window);
}

// MarkupContext.js
//
Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext(document, global, parentContext, dataContext) {
    // <param name="document">Document or fragment of the document.</param>
    // <param name="global" type="Boolean">Whether context is global.</param>
    // <param name="parentContext" type="Object" optional="true" mayBeNull="true">Parent context.</param>
    // <param name="dataContext" type="Object" optional="true" mayBeNull="true">Data context.</param>
    this._document = document;
    this._global = global;
    this._parentContext = parentContext;
    this._dataContext = dataContext || null;
    this._objects = { };
    this._pendingReferences = [];
    this._pendingEndUpdates = [];
}





    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$get_dataContext() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Data context value.</value>
        Sys.Debug.assert(this._opened);

        if (this._dataContextHidden) {
            return null;
        }
        return this._dataContext;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$get_isGlobal() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._global;
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$addComponent(component, noRegisterWithApp) {
        // <param name="component" type="Object"></param>
        // <param name="noRegisterWithApp" type="Boolean" optional="true"></param>
        var id = component.get_id();
        if(id) {
            this._addComponentByID(id, component, noRegisterWithApp);
        }
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$removeComponent(component) {
        // <param name="component" type="Object"></param>
        var id = component.get_id();
        if(id) {
            this._removeComponentByID(id);
        }

        if(this._global && Sys.Component.isInstanceOfType(component)) {
            // document level xml script components are added to application component list so $find can find them
            Sys.Application.removeComponent(object);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$findComponent(id, parent) {
        // <param name="id" type="String">The id of the object to find.</param>
        // <param name="parent" optional="true" mayBeNull="true">
        //   The component or element that contains the object to find.
        //   If not specified or null, the search is made on this Context.
        //   If this Context does not have an object with this ID, the parent
        //   context is searched. If there is no parent context, the search
        //   is made on Application.
        // </param>
        // <returns type="Object" mayBeNull="true">The object, or null if it wasn't found.</returns>
        if(parent) {
            return Sys.Application.findComponent(id, parent);
        }
        else {
            var object = this._objects[id];
            if (!object) {
                parent = this._parentContext || Sys.Application;
                object = parent.findComponent(id);
            }
            return object;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$getComponents() {
        // <returns type="Array" />
        var res = [];
        var objects = this._objects;
        for (var name in objects) {
            res[res.length] = objects[name];
        }
        return res;
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$_addComponentByID(id, object, noRegisterWithApp) {
        Sys.Debug.assert(!this._objects[id], String.format('Duplicate use of id "{0}" for object of type "{1}".', id, Object.getTypeName(object)));
        this._objects[id] = object;
        if(!noRegisterWithApp && this._global && Sys.Component.isInstanceOfType(object)) {
            // document level xml script components are added to application component list so $find can find them
            Sys.Application.addComponent(object);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$addEndUpdate(instance) {
        // <param name="instance">Object instance</param>
        Sys.Debug.assert(this._opened);
        Array.add(this._pendingEndUpdates, instance);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$addReference(instance, propertyInfo, reference) {
        // <param name="instance" type="Object">Object instance</param>
        // <param name="propertyInfo" type="Object">Property information</param>
        // <param name="reference" type="String">Object reference id</param>
        Sys.Debug.assert(this._opened);
        Array.add(this._pendingReferences, { o: instance, p: propertyInfo, r: reference });
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$close() {
        Sys.Debug.assert(this._opened);

        this._opened = false;
        this._dataContext = null;

        // NOTE: The only references that need to get resolved are script
        //       component references.
        //       Element references are resolved as they are encountered.
        //       Hence the exclusive use of findComponent below.
        var i;
        for (i = 0; i < this._pendingReferences.length; i++) {
            var pendingReference = this._pendingReferences[i];

            var instance = pendingReference.o;
            var propertyInfo = pendingReference.p;
            var propertyValue = pendingReference.r;

            var object = this.findComponent(propertyValue);
            Sys.Debug.assert(!!object, String.format('Could not resolve reference to object named "{0}" for "{1}" property on object of type "{2}"', propertyValue, propertyInfo.name, Object.getTypeName(instance)));

            var setter = instance['set_' + propertyInfo.name];
            setter.call(instance, object);
        }
        this._pendingReferences = null;

        for (i = 0; i < this._pendingEndUpdates.length; i++) {
            this._pendingEndUpdates[i].endUpdate();
        }
        this._pendingEndUpdates = null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$dispose() {
        if (!this._global) {
            // Dispose objects that are non-global (eg. scoped within a template instance
            // when the container is disposed), so the objects don't wait for the window's
            // unload event.
            // We don't do this for the global markup context because those objects can
            // be taken care of by the unload event
            for (var o in this._objects) {
                if (Sys.IDisposable.isImplementedBy(this._objects[o])) {
                    this._objects[o].dispose();
                }

                this._objects[o] = null;
            }
        }

        this._document = null;
        this._parentContext = null;
        this._dataContext = null;

        this._objects = null;
        this._pendingReferences = null;
        this._pendingEndUpdates = null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$findElement(id) {
        // <param name="id" type="String">Element id</param>
        // <returns>Element within this context</returns>
        if (this._opened) {
            Sys.Debug.assert(!!this._document);

            var element = Sys.UI.DomElement.getElementById(id, this._document);
            if (!element && this._parentContext) {
                element = Sys.UI.DomElement.getElementById(id, this._parentContext);
            }
            return element;
        }

        return null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$hideDataContext() {
        // <returns type="Boolean">True if data context is hidden, false if not.</returns>
        Sys.Debug.assert(this._opened);

        if (!this._dataContextHidden) {
            this._dataContextHidden = true;
            return true;
        }
        return false;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$open() {
        Sys.Debug.assert(this._opened === false);

        this._pendingReferences = [];
        this._pendingEndUpdates = [];

        this._opened = true;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$restoreDataContext() {
        Sys.Debug.assert(this._opened);

        this._dataContextHidden = false;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext.prototype = {
    _dataContextHidden: false,
    _opened: false,

    get_dataContext: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$get_dataContext,

    get_isGlobal: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$get_isGlobal,

    // Begin IContainer
    addComponent: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$addComponent,
    removeComponent: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$removeComponent,

    findComponent: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$findComponent,

    getComponents: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$getComponents,
    // End IContainer

    _addComponentByID: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$_addComponentByID,

    addEndUpdate: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$addEndUpdate,

    addReference: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$addReference,

    close: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$close,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$dispose,

    findElement: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$findElement,

    hideDataContext: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$hideDataContext,

    open: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$open,

    restoreDataContext: Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$restoreDataContext
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext', null, Sys.IContainer);

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext.createGlobalContext = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$createGlobalContext() {
    /// <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext"></returns>
    if (arguments.length !== 0) throw Error.parameterCount();
    return new Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext(document, true);
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext.createLocalContext = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupContext$createLocalContext(documentFragment, parentContext, dataContext) {
    // <param name="documentFragment" optional="false" mayBeNull="false">Fragment of the document.</param>
    // <param name="parentContext" optional="false" mayBeNull="false">Parent context.</param>
    // <param name="dataContext" optional="true" mayBeNull="true">Data context.</param>
    // <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext">Localized markup context.</returns>
    return new Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext(documentFragment, false, parentContext, dataContext);
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser = new function() {
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._defaultNamespaceURI = 'http://schemas.microsoft.com/xml-script/2005';
Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._cachedNamespaceURILists = {};

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.getNodeName = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$getNodeName(node) {
    return node.localName || node.baseName;
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$initializeObject(instance, node, markupContext) {
    var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(instance);
    if (!td) {
        return null;
    }

    var supportsBatchedUpdates = false;
    if ((instance.beginUpdate && instance.endUpdate && instance !== Sys.Application)) {
        supportsBatchedUpdates = true;
        instance.beginUpdate();
    }

    var i, a;
    var attr, attrName;
    var propertyInfo, propertyName, propertyType, propertyValue;
    var eventInfo, eventValue;
    var setter, getter;
    var nodeName;

    var properties = td._getProperties();
    var events = td._getEvents();

    var attributes = node.attributes;
    if (attributes) {
        for (a = attributes.length - 1; a >= 0; a--) {
            attr = attributes[a];
            attrName = attr.nodeName;
            
            // special case not to set ID on controls
            if(attrName === "id" && Sys.UI.Control.isInstanceOfType(instance)) continue;

            propertyInfo = properties[attrName];
            if (propertyInfo) {
                propertyType = propertyInfo.type;
                propertyValue = attr.nodeValue;

                if (propertyType && (propertyType === Object || propertyType === Sys.Component || propertyType.inheritsFrom(Sys.Component))) {
                    markupContext.addReference(instance, propertyInfo, propertyValue);
                }
                else {
                    if (propertyInfo.isDomElement) {
                        propertyValue = markupContext.findElement(propertyValue);
                    }
                    else {
                        if (propertyType === Array) {
                            propertyValue = Array.parse('[' + propertyValue + ']');
                        }
                        // propertyType might be undefined if the target can take any type
                        else if (propertyType && propertyType !== String) {
                            // if its an enum we want to use case-insensitive parse
                            if(Type.isEnum(propertyType)) {
                                propertyValue = propertyType.parse(propertyValue, true);
                            }
                            else {
                                if(propertyValue === "" && propertyType === Number) {
                                    propertyValue = 0;
                                }
                                else {
                                    propertyValue =
                                        (propertyType.parseInvariant || propertyType.parse)(propertyValue);
                                }
                            }
                        }
                    }

                    propertyName = propertyInfo.name;
                    setter = instance['set_' + propertyName];
                    setter.call(instance, propertyValue);
                }
            }
            else {
                eventInfo = events[attrName];
                if (eventInfo) {
                    var handler = Function.parse(attr.nodeValue);
                    // consider: throw in debug if no function by that name found?
                    if (handler) {
                        eventValue = instance['add_' + eventInfo.name];
                        if (eventValue) {
                            eventValue.apply(instance, [handler]);
                        }
                        else {
                            throw Error.invalidOperation(String.format("The event '{0}' is specified in the type descriptor, but add_{0} was not found.", eventInfo.name));
                        }
                    }
                }
                else {
                    throw Error.invalidOperation(String.format('Unrecognized attribute "{0}" on object of type "{1}"', attrName, Object.getTypeName(instance)));
                }
            }
        }
    }

    // now enumerate over child elements
    var childNodes = node.childNodes;
    if (childNodes && (childNodes.length != 0)) {
        for (i = childNodes.length - 1; i >= 0; i--) {
            var childNode = childNodes[i];
            if (childNode.nodeType != 1) {
                continue;
            }
            
            nodeName = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.getNodeName(childNode);
            propertyInfo = properties[nodeName];

            if (propertyInfo) {
                propertyName = propertyInfo.name;
                propertyType = propertyInfo.type;

                if (propertyInfo.readOnly) {
                    getter = instance['get_' + propertyName];
                    var nestedObject = getter.call(instance);

                    if (propertyType === Array) {
                        if (childNode.childNodes.length) {
                            var items = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNodes(childNode.childNodes, markupContext);
                            for (var itemIndex = 0; itemIndex < items.length; itemIndex++) {
                                var item = items[itemIndex];
                                // the array has its own add method
                                if(typeof(nestedObject.add) === "function") {
                                    nestedObject.add(item);
                                }
                                else {
                                    Array.add(nestedObject, item);
                                    if(typeof(item.setOwner) === "function") {
                                        // the item wants to know who owns it
                                        item.setOwner(instance);
                                    }
                                }
                            }
                        }
                    }
                    else if (propertyType === Object) {
                        // if propertyType is object then treat it like a script object type, setting fields for each attribute
                        attributes = childNode.attributes;
                        for (a = attributes.length - 1; a >= 0; a--) {
                            attr = attributes[a];
                            nestedObject[attr.nodeName] = attr.nodeValue;
                        }
                        // CONSIDER: should we process child elements here? By ignoring them, this scenario doesnt work:
                        // <control ...>
                        //     <element width="100%">
                        //        <style border="1" />
                        // the style child element is ignored.
                        // Possible solution: take out this else case, let initializeObject go recursively on all object types.
                    }
                    else {
                        // if its a class, initialize it recursively
                        Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(nestedObject, childNode, markupContext);
                    }
                }
                else {
                    propertyValue = null;
                    if (propertyType == String) {
                        propertyValue = childNode.text;
                    }
                    else if (childNode.childNodes.length != 0) {
                        var valueNode;
                        for (var childNodeIndex = 0; childNodeIndex < childNode.childNodes.length; childNodeIndex++) {
                            if (childNode.childNodes[childNodeIndex].nodeType != 1) {
                                continue;
                            }
                            valueNode = childNode.childNodes[childNodeIndex];
                            break;
                        }
                        if (valueNode) {
                            propertyValue = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNode(valueNode, markupContext);
                        }
                    }

                    if (propertyValue) {
                        setter = instance['set_' + propertyName];
                        setter.call(instance, propertyValue);
                    }
                }
            }
            else {
                eventInfo = events[nodeName];
                if (eventInfo) {
                    var actions = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNodes(childNode.childNodes, markupContext);
                    if (actions.length) {
                        eventValue = instance["add_" + eventInfo.name];
                        if(eventValue) {
                            for (var e = 0; e < actions.length; e++) {
                                var action = actions[e];
                                // actions add themselves to the event handler list
                                // we just need to tell them the owner and event name
                                action.set_eventName(eventInfo.name);
                                action.set_eventSource(instance);
                            }
                        }
                        else {
                            throw Error.invalidOperation(String.format("The event '{0}' is specified in the type descriptor, but add_{0} was not found.", eventInfo.name));
                        }
                    }
                }
                else {
                    // not a property or an event
                    var type = null;
                    var upperName = nodeName.toUpperCase();
                    if(upperName === 'BINDINGS') {
                        type = Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase;
                    }
                    else if(upperName === 'BEHAVIORS') {
                        type = Sys.UI.Behavior;
                    }
                    if(type) {
                        if (childNode.childNodes.length) {
                            var items = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNodes(childNode.childNodes, markupContext);
                            for (var itemIndex = 0; itemIndex < items.length; itemIndex++) {
                                var item = items[itemIndex];
                                // assert the child item is of the right type
                                Sys.Debug.assert(type.isInstanceOfType(item), String.format("The '{0}' element may only contain child elements of type '{1}'.", nodeName, type.getName()));
                                if(typeof(item.setOwner) === "function") {
                                    item.setOwner(instance);
                                }
                            }
                        }
                    }
                    else {
                        // node is not a property, event, or <bindings> or <behaviors>
                        throw Error.invalidOperation(String.format('Unrecognized child node "{0}" on object of type "{1}"', nodeName, Object.getTypeName(instance)));
                    }
                }
            }
        }
    }

    if (supportsBatchedUpdates) {
        markupContext.addEndUpdate(instance);
    }

    return instance;
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNode = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$parseNode(node, markupContext) {
    var parsedObject = null;

    var tagType = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._getTagType(node);
    if (tagType) {
        var parseMethod = tagType.parseFromMarkup;
        if (!parseMethod) {
            var baseType = tagType.getBaseType();
            while (baseType) {
                parseMethod = baseType.parseFromMarkup;
                if (parseMethod) {
                    break;
                }
                baseType = baseType.getBaseType();
            }
            tagType.parseFromMarkup = parseMethod;
        }

        if (parseMethod) {
            parsedObject = parseMethod.call(null, tagType, node, markupContext);
        }
    }
    return parsedObject;
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNodes = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$parseNodes(nodes, markupContext) {
    var objects = [];

    for (var i = 0; i < nodes.length; i++) {
        var objectNode = nodes[i];
        if (objectNode.nodeType !== 1) {
            // Ignore comments, whitespace nodes etc.
            continue;
        }

        var processedObject = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNode(objectNode, markupContext);
        if (processedObject) {
            Array.add(objects, processedObject);
        }
    }

    return objects;
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.processDocument = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$processDocument(markupContext) {
    Sys.Debug.assert(markupContext.get_isGlobal());

    var scripts = [];
    var scriptElements = document.getElementsByTagName('script');
    for (var e = 0; e < scriptElements.length; e++) {
        if (scriptElements[e].type == 'text/xml-script') {
            var scriptElement = scriptElements[e];

            var scriptMarkup = scriptElement.innerHTML;
            if (scriptMarkup.startsWith('<!--')) {
                var startIndex = scriptMarkup.indexOf('<', 1);
                var endIndex = scriptMarkup.lastIndexOf('>');
                endIndex = scriptMarkup.lastIndexOf('>', endIndex - 1);

                scriptMarkup = scriptMarkup.substring(startIndex, endIndex + 1);
            }

            if (scriptMarkup.length == 0) {
                continue;
            }

            var scriptDOM;
            if (Sys.Net.XMLDOM) {
                scriptDOM = new Sys.Net.XMLDOM(scriptMarkup);
            }
            else {
                // compat with whidbey atlas 1.0
                scriptDOM = new XMLDOM(scriptMarkup);
            }
            var scriptDocumentNode = null;
            var pageElements = scriptDOM.getElementsByTagName("page");
            if (pageElements.length) {
                scriptDocumentNode = pageElements[0];
            }
            
            if (scriptDocumentNode && Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.getNodeName(scriptDocumentNode) === "page") {
                Array.add(scripts, scriptDocumentNode);
            }
            else {
                throw Error.create('Missing page element in xml script block.', scriptMarkup);
            }
        }
    }

    Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.processDocumentScripts(markupContext, scripts);
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.processDocumentScripts = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$processDocumentScripts(markupContext, scripts) {

    markupContext.open();

    for (var s = 0; s < scripts.length; s++) {
        var componentNodes = [];
        var scriptDocumentNode = scripts[s];
        var scriptDocumentItemNodes = scriptDocumentNode.childNodes;

        for (var i = scriptDocumentItemNodes.length - 1; i >= 0; i--) {
            var node = scriptDocumentItemNodes[i];
            if (node.nodeType !== 1) {
                continue;
            }

            var nodeName = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.getNodeName(node);
            if (nodeName) nodeName = nodeName.toLowerCase();
            if (nodeName === 'components') {
                for (var c = 0; c < node.childNodes.length; c++) {
                    var componentNode = node.childNodes[c];
                    if (componentNode.nodeType !== 1) {
                        continue;
                    }

                    Array.add(componentNodes, componentNode);
                }
            }
        } // end for each "script/x" node

        if (componentNodes.length) {
            Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNodes(componentNodes, markupContext);
        }
    } // end for each "script" node

    markupContext.close();
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._getDefaultNamespaces = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$_getDefaultNamespaces() {
    if(!Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._defaultNamespaces) {
        var list = [ Sys, Sys.UI, Sys.Net, Inflectra.SpiraTest.Web.AjaxExtensions, Inflectra.SpiraTest.Web.ServerControls,
                     Inflectra.SpiraTest.Web.AjaxExtensions.Net, Inflectra.SpiraTest.Web.AjaxExtensions.Data, Inflectra.SpiraTest.Web.ServerControls.Data,
                     Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components ];
        if(Inflectra.SpiraTest.Web.ServerControls.Effects) Array.add(list, Inflectra.SpiraTest.Web.ServerControls.Effects);
        Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._defaultNamespaces = list;
    }
    return Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._defaultNamespaces;
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._processNamespaceURI = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$_processNamespaceURI(namespaceURI) {
    if(!namespaceURI || namespaceURI === Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._defaultNamespaceURI) {
        return Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._getDefaultNamespaces();
    }
    
    // remove "javascript:" prefix if any
    var start = namespaceURI.slice(0, 12).toLowerCase(); // "javascript:"
    if(start === "javascript:") {
        namespaceURI = namespaceURI.slice(11);
        if(!namespaceURI.length) {
            return [];
        }
    }
    var nspaceList = namespaceURI.split(',');
    list = [];
    for(var i=0; i < nspaceList.length; i++) {
        var nspaceName = nspaceList[i];
        if(nspaceName.startsWith(' ')) nspaceName = nspaceName.trimStart();
        if(nspaceName.endsWith(' ')) nspaceName = nspaceName.trimEnd();
        if(!nspaceName.length) continue;
        var nspace = null;
        try { nspace = eval(nspaceName) } catch(e) { }
        if (!nspace || !Type.isNamespace(nspace)) {
            throw Error.invalidOperation(String.format("'{0}' is not a valid namespace.", nspaceName));
        }
        if(nspace) {
            Array.add(list, nspace);
        }
    }
    return list;
}

Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._getTagType = function Inflectra$SpiraTest$Web$AjaxExtensions$MarkupParser$_getTagType(node) {
    Sys.Debug.assert(!!node, 'Node should not be null.');
    var tagName = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.getNodeName(node);
    var namespaceURI = node.namespaceURI || Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._defaultNamespaceURI;
    var nspaceList = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._cachedNamespaceURILists[namespaceURI];
    if (typeof(nspaceList) === 'undefined') {
        nspaceList = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._processNamespaceURI(namespaceURI);
        Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser._cachedNamespaceURILists[namespaceURI] = nspaceList;
    }

    var upperTagName = tagName.toUpperCase();
    for(var i=0; i < nspaceList.length; i++) {
        var nspace = nspaceList[i];
        var type = Type.parse(tagName, nspace);
        if(typeof(type) === 'function') {
            return type;
        }
    }
    // special case since application's type is actually _Application
    if(upperTagName === "APPLICATION") {
        return Sys._Application;
    }
    // special case since WebRequestManager's type is actually _WebRequestManager
    if(upperTagName === "WEBREQUESTMANAGER") {
        return Sys.Net._WebRequestManager;
    }
    throw Error.invalidOperation(String.format("Unrecognized tag '{0}'", tagName));
    return null;
}
Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor = function Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor() {
    throw Error.notImplemented();
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor$getProperty() {
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor$setProperty() {
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor$invokeMethod() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.prototype = {
    getProperty: Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor$getProperty,
    
    setProperty: Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor$setProperty,
    
    invokeMethod: Inflectra$SpiraTest$Web$AjaxExtensions$ICustomTypeDescriptor$invokeMethod
}
Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.registerInterface('Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor');

Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider = function Inflectra$SpiraTest$Web$AjaxExtensions$ITypeDescriptorProvider() {
    throw Error.notImplemented();
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$ITypeDescriptorProvider$getDescriptor() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider.prototype = {
    getDescriptor: Inflectra$SpiraTest$Web$AjaxExtensions$ITypeDescriptorProvider$getDescriptor
}
Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider.registerInterface('Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider');
Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged = function Inflectra$SpiraTest$Web$AjaxExtensions$INotifyCollectionChanged() {
    if (arguments.length !== 0) throw Error.parameterCount();
    throw Error.notImplemented();
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$INotifyCollectionChanged$add_collectionChanged() {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        throw Error.notImplemented();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$INotifyCollectionChanged$remove_collectionChanged() {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged.prototype = {
    add_collectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$INotifyCollectionChanged$add_collectionChanged,
    remove_collectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$INotifyCollectionChanged$remove_collectionChanged
}
Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged.registerInterface('Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged');



Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction = function Inflectra$SpiraTest$Web$AjaxExtensions$NotifyCollectionChangedAction() {
    throw Error.invalidOperation();
}




Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.prototype = {
    Add: 0,
    Remove: 1,
    Reset: 2
}
Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.registerEnum('Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction');

Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs = function Inflectra$SpiraTest$Web$AjaxExtensions$CollectionChangedEventArgs(action, changedItem) {
    /// <param name="action" type="Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction"></param>
    /// <param name="changedItem" type="Object" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "action", type: Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction},
        {name: "changedItem", type: Object, mayBeNull: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs.initializeBase(this);
    this._action = action;
    this._changedItem = changedItem;
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$CollectionChangedEventArgs$get_action() {
        /// <value type="Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._action;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$CollectionChangedEventArgs$get_changedItem() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._changedItem;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs.prototype = {
    get_action: Inflectra$SpiraTest$Web$AjaxExtensions$CollectionChangedEventArgs$get_action,
    get_changedItem: Inflectra$SpiraTest$Web$AjaxExtensions$CollectionChangedEventArgs$get_changedItem
}
Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs.descriptor = {
    properties: [   {name: 'action', type: Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction, readOnly: true},
                    {name: 'changedItem', type: Object, readOnly: true} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs', Sys.EventArgs);
Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingDirection() {
    throw Error.invalidOperation();
}




Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.prototype = {
    In: 0,
    Out: 1,
    InOut: 2
}
Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.registerEnum('Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection');
Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs(value, direction, targetPropertyType, transformerArgument) {
    /// <param name="value" mayBeNull="true"></param>
    /// <param name="direction" type="Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection"></param>
    /// <param name="targetPropertyType" type="Type" mayBeNull="true"></param>
    /// <param name="transformerArgument" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "value", mayBeNull: true},
        {name: "direction", type: Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection},
        {name: "targetPropertyType", type: Type, mayBeNull: true},
        {name: "transformerArgument", mayBeNull: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs.initializeBase(this);

    this._value = value;
    this._direction = direction;
    this._targetPropertyType = targetPropertyType;
    this._transformerArgument = transformerArgument;
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_direction() {
        /// <value type="Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._direction;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_targetPropertyType() {
        /// <value type="Type" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._targetPropertyType;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_transformerArgument() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._transformerArgument;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_value() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._value;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$set_value(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._value = value;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs.prototype = {
    get_direction: Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_direction,

    get_targetPropertyType: Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_targetPropertyType,

    get_transformerArgument: Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_transformerArgument,
    
    get_value: Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$get_value,
    set_value: Inflectra$SpiraTest$Web$AjaxExtensions$BindingEventArgs$set_value
}
Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs.descriptor = {
    properties: [   {name: 'direction', type: Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection, readOnly: true},
                    {name: 'targetPropertyType', type: Type, readOnly: true},
                    {name: 'transformerArgument', readOnly: true},
                    {name: 'value'} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs', Sys.CancelEventArgs);

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase(target) {
    /// <param name="target" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "target", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.initializeBase(this);

    if(target) {
        this._target = target;
    }
}












    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_automatic() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._automatic;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_automatic(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        if (!this._source) {
            this._automatic = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_dataContext() {
        /// <value type="Object" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataContext;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_dataContext(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Object, mayBeNull: true}]);
        if (e) throw e;

        if (!this._source) {
            this._dataContext = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_dataPath() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._dataPath;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_dataPath(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if (!this._source) {
            this._dataPath = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_target() {
        /// <value type="Object" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._target;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_target(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Object, mayBeNull: true}]);
        if (e) throw e;

        Sys.Debug.assert(!this.get_isInitialized(), "Binding target cannot be changed after initialization.");
        this._target = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_property() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._property;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_property(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if (!this._source) {
            this._property = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_propertyKey() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._propertyKey;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_propertyKey(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        if (!this._source) {
            this._propertyKey = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_transformerArgument() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._transformerArgument;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_transformerArgument(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._transformerArgument = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$add_transform(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("transform", handler);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$remove_transform(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("transform", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$dispose() {
        this._dataContext = null;
        this._source = null;
        this._target = null;
        Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$evaluate(direction) {
        /// <param name="direction" type="Number"></param>
        var e = Function._validateParams(arguments, [
            {name: "direction", type: Number}
        ]);
        if (e) throw e;

        Sys.Debug.assert((direction === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In) || (direction === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out));

        if (this._bindingExecuting) {
            return;
        }
        this._bindingExecuting = true;
        if (direction === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In) {
            this.evaluateIn();
        }
        else {
            this.evaluateOut();
        }
        this._bindingExecuting = false;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$evaluateIn() {
        var targetPropertyType = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getPropertyType(this._target, this._property, this._propertyKey);
        var value = this._getSourceValue(targetPropertyType);

        var canceled = false;

        var handler = this.get_events().getHandler("transform");
        if (handler) {
            var be = new Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs(value, Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In, targetPropertyType, this._transformerArgument);

            handler(this, be);
            canceled = be.get_cancel();
            value = be.get_value();
        }

        if (!canceled) {
            Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.setProperty(this._target, this._property, value, this._propertyKey);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$evaluateOut() {
        throw Error.createError('evaluateOut is not supported for this binding');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.callBaseMethod(this, 'initialize');

        Sys.Debug.assert(!!this._target, "Binding has no target.");

        this._source = this._dataContext;
        if (!this._source) {
            // Inherit dataContext from the target, i.e., the object with
            // which this binding is associated.
            this._source = this._target.get_dataContext();
        }
        Sys.Debug.assert(!!this._source, String.format('No data context available for binding with ID "{0}" and dataPath "{1}" on object of type "{2}"', this._id, this._dataPath, Object.getTypeName(this._target)));

        if (this._dataPath && this._dataPath.indexOf('.') > 0) {
            this._dataPathParts = this._dataPath.split('.');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_evaluateDataPath() {
        Sys.Debug.assert(!!this._dataPathParts);

        var object = this._source;
        for (var i = 0; i < this._dataPathParts.length - 1; i++) {
            object = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(object, this._dataPathParts[i]);
            if (!object) {
                return null;
            }
        }
        return object;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_get_dataPathParts() {
        return this._dataPathParts;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_getSource() {
        return this._source;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_getSourceValue(targetPropertyType) {
        // get the value of the target that should be assigned to the target
        if (this._dataPath && this._dataPath.length) {
            var propertyObject = this._source;
            var propertyName = this._dataPath;

            if (this._dataPathParts) {
                propertyObject = this._evaluateDataPath();
                if (propertyObject === null) {
                    return null;
                }
                propertyName = this._dataPathParts[this._dataPathParts.length - 1];
            }

            return Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(propertyObject, propertyName);
        }
        if (this._source && Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor.isImplementedBy(this._source)) {
            return this._source.getProperty('');
        }
        return this._source;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_getTargetValue(destinationType) {
        // get the value of the target that should be assigned to the source
        var value = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(this._target, this._property, this._propertyKey);

        var handler = this.get_events().getHandler('transform');
        if(handler) {
            var be = new Inflectra.SpiraTest.Web.AjaxExtensions.BindingEventArgs(value, Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out, destinationType, this._transformerArgument);
            handler(this, be);
            var canceled = be.get_cancel();

            if (!canceled) {
                value = be.get_value();
            }
            else {
                value = null;
            }
        }

        return value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$setOwner(owner) {
        /// <param name="owner"></param>
        var e = Function._validateParams(arguments, [
            {name: "owner"}
        ]);
        if (e) throw e;

        // setOwner is how the binding is connected with its owner when it is declared as
        // a nested element in xml-script.
        this.set_target(owner);
    }
Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.prototype = {
    _target: null,
    _property: null,
    _propertyKey: null,
    _dataContext: null,
    _dataPath: null,
    _dataPathParts: null,
    _transformerArgument: null,
    _automatic: true,
    _bindingExecuting: false,
    _source: null,

    get_automatic: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_automatic,
    set_automatic: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_automatic,

    get_dataContext: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_dataContext,
    set_dataContext: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_dataContext,

    get_dataPath: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_dataPath,
    set_dataPath: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_dataPath,

    get_target: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_target,
    set_target: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_target,

    get_property: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_property,
    set_property: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_property,

    get_propertyKey: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_propertyKey,
    set_propertyKey: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_propertyKey,

    get_transformerArgument: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$get_transformerArgument,
    set_transformerArgument: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$set_transformerArgument,

    add_transform: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$add_transform,
    remove_transform: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$remove_transform,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$dispose,

    evaluate: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$evaluate,

    evaluateIn: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$evaluateIn,

    evaluateOut: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$evaluateOut,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$initialize,

    _evaluateDataPath: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_evaluateDataPath,

    _get_dataPathParts: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_get_dataPathParts,

    _getSource: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_getSource,

    _getSourceValue: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_getSourceValue,

    _getTargetValue: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$_getTargetValue,

    setOwner: Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$setOwner
}
Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.descriptor = {
    properties: [   {name: 'target', type: Object},
                    {name: 'automatic', type: Boolean},
                    {name: 'dataContext', type: Object},
                    {name: 'dataPath', type: String},
                    {name: 'property', type: String},
                    {name: 'propertyKey' },
                    {name: 'transformerArgument', type: String} ],
    methods: [ {name: 'evaluateIn'} ],
    events: [ {name: 'transform'} ]
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase', Sys.Component, Sys.IDisposable);

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.parseFromMarkup = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$parseFromMarkup(type, node, markupContext) {
    /// <param name="type" type="Type"></param>
    /// <param name="node"></param>
    /// <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext"></param>
    /// <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "node"},
        {name: "markupContext", type: Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext}
    ]);
    if (e) throw e;

    var newBinding = new type();

    var builtInTransform;
    var transformAttribute = node.attributes.getNamedItem('transform');
    if (transformAttribute) {
        var transformValue = transformAttribute.nodeValue;
        builtInTransform = Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers[transformValue];
    }
    if (builtInTransform) {
        newBinding.add_transform(builtInTransform);
        node.attributes.removeNamedItem('transform');
    }

    var binding = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(newBinding, node, markupContext);

    if (builtInTransform) {
        node.attributes.setNamedItem(transformAttribute)
    }

    if (binding) {
        markupContext.addComponent(binding);
        return binding;
    }
    else {
        newBinding.dispose();
    }

    return null;
}
Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers = { };

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.Invert = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$Invert(sender, eventArgs) {
    eventArgs.set_value(!eventArgs.get_value());
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.ToString = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$ToString(sender, eventArgs) {
    Sys.Debug.assert(eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);

    var value = eventArgs.get_value();
    var newValue = '';
    var formatString = eventArgs.get_transformerArgument();
    var placeHolder = (formatString && (formatString.length !== 0)) ? formatString.indexOf('{0}') : -1;

    if (placeHolder != -1) {
        newValue = String.format(formatString, value);
    }
    else if (value) {
        newValue = value.toString();
    }
    else {
        newValue = formatString;
    }

    eventArgs.set_value(newValue);
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.ToLocaleString = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$ToLocaleString(sender, eventArgs) {
    Sys.Debug.assert(eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);

    var value = eventArgs.get_value();
    var newValue = '';
    var formatString = eventArgs.get_transformerArgument();
    var placeHolder = (formatString && (formatString.length !== 0)) ? formatString.indexOf('{0}') : -1;

    if (placeHolder !== -1) {
        newValue = String.format(formatString, value.toLocalString ? value.toLocalString() : value.toString());
    }
    else if (value) {
        newValue = value.toLocaleString();
    }
    else {
        newValue = formatString;
    }

    eventArgs.set_value(newValue);
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.Add = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$Add(sender, eventArgs) {
    var value = eventArgs.get_value();
    if (typeof(value) !== 'number') {
        if(value === "") {
            value = 0;
        }
        else {
            value = Number.parseInvariant(value);
        }
    }

    var delta = eventArgs.get_transformerArgument();
    if (!delta) {
        delta = 1;
    }
    if (typeof(delta) !== 'number') {
        if(value === "") {
            delta = 0;
        }
        else {
            delta = Number.parseInvariant(delta);
        }
    }

    if (eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out) {
        delta = -delta;
    }

    var newValue = value + delta;
    if (eventArgs.get_targetPropertyType() !== 'number') {
        newValue = newValue.toString();
    }

    eventArgs.set_value(newValue);
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.Multiply = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$Multiply(sender, eventArgs) {
    var value = eventArgs.get_value();
    if (typeof(value) !== 'number') {
        if(value === "") {
            value = 0;
        }
        else {
            value = Number.parseInvariant(value);
        }
    }

    var factor = eventArgs.get_transformerArgument();
    if (!factor) {
        factor = 1;
    }
    if (typeof(factor) !== 'number') {
        if(factor === "") {
            factor = 0;
        }
        else {
            factor = Number.parseInvariant(factor);
        }
    }

    var newValue;
    if (eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out) {
        newValue = value / factor;
    }
    else {
        newValue = value * factor;
    }

    if (eventArgs.get_targetPropertyType() !== 'number') {
        newValue = newValue.toString();
    }

    eventArgs.set_value(newValue);
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.Compare = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$Compare(sender, eventArgs) {
    Sys.Debug.assert(eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);

    var value = eventArgs.get_value();
    var compareValue = eventArgs.get_transformerArgument();

    if (compareValue === null) {
        value = value ? true : false;
    }
    else {
        value = (value === compareValue);
    }

    eventArgs.set_value(value);
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.CompareInverted = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$CompareInverted(sender, eventArgs) {
    Sys.Debug.assert(eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);

    var value = eventArgs.get_value();
    var compareValue = eventArgs.get_transformerArgument();

    if (compareValue === null) {
        value = value ? false : true;
    }
    else {
        value = (value !== compareValue);
    }

    eventArgs.set_value(value);
}

Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase.Transformers.RSSTransform = function Inflectra$SpiraTest$Web$AjaxExtensions$BindingBase$Transformers$RSSTransform(sender, eventArgs) {
    Sys.Debug.assert(eventArgs.get_direction() === Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);

    function getNodeValue(source, xPath) {
        var node = source.selectSingleNode(xPath);
        if (node) {
            return node.nodeValue;
        }
        return null;
    }

    var xmlNodes = eventArgs.get_value();
    if (!xmlNodes) {
        return;
    }

    // Create DataTable for the return value
    // - All have: defaultValue=null; key=false (except guid); readOnly=true
    // - Although we might seemingly be able to cache the list of columns,
    //   it would be a bit premature to try to optimize that.
    var dataItems = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable([
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('title', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('description', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('link', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('author', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('category', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('comments', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('guid', String, null, true, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('pubDate', String, null, false, true),
        new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn('source', String, null, false, true)
    ]);

    // Convert the XML data into data rows
    for (var i = 0; i < xmlNodes.length; i++) {
        var xmlNode = xmlNodes[i];
        if (!xmlNode || (xmlNode.nodeType != 1)) {
            continue;
        }

        var dataItem = {
            title : getNodeValue(xmlNode, './title/text()'),
            description : getNodeValue(xmlNode, './description/text()'),
            link : getNodeValue(xmlNode, './link/text()'),
            author : getNodeValue(xmlNode, './author/text()'),
            category : getNodeValue(xmlNode, './category/text()'),
            comments : getNodeValue(xmlNode, './comments/text()'),
            guid : getNodeValue(xmlNode, './guid/text()'),
            pubDate : getNodeValue(xmlNode, './pubDate/text()'),
            source : getNodeValue(xmlNode, './source/text()')
        };

        dataItems.add(dataItem);
    }

    eventArgs.set_value(dataItems);
}

Inflectra.SpiraTest.Web.AjaxExtensions.Binding = function Inflectra$SpiraTest$Web$AjaxExtensions$Binding(target) {
    /// <param name="target" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "target", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.AjaxExtensions.Binding.initializeBase(this, [target]);
}





    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$get_direction() {
        /// <value type="Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._direction;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$set_direction(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection}]);
        if (e) throw e;

        if (!this._getSource()) {
            this._direction = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$dispose() {
        var target = this.get_target();
        var source = this._getSource();

        if (this._targetNotificationHandler) {
            target.remove_propertyChanged(this._targetNotificationHandler);
            this._targetNotificationHandler = null;
        }

        if (this._sourceNotificationHandler) {
            source.remove_propertyChanged(this._sourceNotificationHandler);
            this._sourceNotificationHandler = null;
        }

        if (this._targetDisposingHandler) {
            target.remove_disposing(this._targetDisposingHandler);
            this._targetDisposingHandler = null;
        }

        if (this._sourceDisposingHandler) {
            source.remove_disposing(this._sourceDisposingHandler);
            this._sourceDisposingHandler = null;
        }

        Inflectra.SpiraTest.Web.AjaxExtensions.Binding.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$evaluateOut() {
        var propertyObject;
        var propertyName;

        var dataPathParts = this._get_dataPathParts();
        if (dataPathParts) {
            propertyObject = this._evaluateDataPath();
            propertyName = dataPathParts[dataPathParts.length - 1];

            if (!propertyObject) {
                return;
            }
        }
        else {
            propertyObject = this._getSource();
            propertyName = this.get_dataPath();
        }

        Sys.Debug.assert(propertyObject !== null);

        var sourcePropertyType = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getPropertyType(propertyObject, propertyName);

        var value = this._getTargetValue(sourcePropertyType);
        if (value !== null) {
            Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.setProperty(propertyObject, propertyName, value);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.Binding.callBaseMethod(this, 'initialize');

        if (this.get_automatic()) {
            if (this._direction !== Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In) {
                var target = this.get_target();
                if (Sys.INotifyPropertyChange.isImplementedBy(target)) {
                    this._targetNotificationHandler = Function.createDelegate(this, this._onTargetPropertyChanged);
                    target.add_propertyChanged(this._targetNotificationHandler);
                }
                if (Sys.INotifyDisposing.isImplementedBy(target)) {
                    this._targetDisposingHandler = Function.createDelegate(this, this._onDisposing);
                    target.add_disposing(this._targetDisposingHandler);
                }
            }

            if (this._direction !== Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out) {
                var source = this._getSource();
                if (Sys.INotifyPropertyChange.isImplementedBy(source)) {
                    this._sourceNotificationHandler = Function.createDelegate(this, this._onSourcePropertyChanged);
                    source.add_propertyChanged(this._sourceNotificationHandler);
                }
                if (Sys.INotifyDisposing.isImplementedBy(source)) {
                    this._sourceDisposingHandler = Function.createDelegate(this, this._onDisposing);
                    source.add_disposing(this._sourceDisposingHandler);
                }

                this.evaluate(Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$_onSourcePropertyChanged(sender, eventArgs) {
        /// <param name="sender" type="Object"></param>
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "sender", type: Object},
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        Sys.Debug.assert(this._direction !== Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out);

        var compareProperty = this.get_dataPath();
        var dataPathParts = this._get_dataPathParts();
        if (dataPathParts) {
            compareProperty = dataPathParts[0];
        }

        var propertyName = eventArgs.get_propertyName();
        if (!propertyName || (propertyName === compareProperty)) {
            this.evaluate(Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$_onTargetPropertyChanged(sender, eventArgs) {
        /// <param name="sender" type="Object"></param>
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "sender", type: Object},
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        Sys.Debug.assert(this._direction !== Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);

        var propertyName = eventArgs.get_propertyName();
        if (!propertyName || (propertyName === this.get_property())) {
            this.evaluate(Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Binding$_onDisposing(sender, eventArgs) {
        this.dispose();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Binding.prototype = {
    _targetNotificationHandler: null,
    _sourceNotificationHandler: null,
    _direction: Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In,

    get_direction: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$get_direction,
    set_direction: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$set_direction,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$dispose,

    evaluateOut: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$evaluateOut,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$initialize,

    _onSourcePropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$_onSourcePropertyChanged,

    _onTargetPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$_onTargetPropertyChanged,

    _onDisposing: Inflectra$SpiraTest$Web$AjaxExtensions$Binding$_onDisposing
}
Inflectra.SpiraTest.Web.AjaxExtensions.Binding.descriptor = {
    properties: [ {name: 'direction', type: Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection} ],
    methods: [ {name: 'evaluateOut'} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Binding.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Binding', Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase);
Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding = function Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding() {
    Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding.initializeBase(this);
}



    function Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$get_xpath() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._xpath;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$set_xpath(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if (!this._getSource()) {
            this._xpath = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding.callBaseMethod(this, 'initialize');

        if (this.get_automatic()) {
            this.evaluate(Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$_getSourceValue(targetPropertyType) {
        var source = Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding.callBaseMethod(this, '_getSourceValue');
        if (!source) {
            return null;
        }

        if (Array.isInstanceOfType(targetPropertyType)) {
            var nodes = source.selectNodes(this._xpath);

            // We need to convert the node list into an array (in IE, the returned
            // node list looks like an array, but isn't a script Array object, and
            // doesn't have our extensions (IArray, other methods etc.)
            var list = [];
            for (var i = 0; i < nodes.length; i++) {
                var node = nodes[i];
                if (!node || (node.nodeType !== 1)) {
                    continue;
                }
                Array.add(list, node);
            }
            return list;
        }
        else {
            var node = source.selectSingleNode(this._xpath);
            if (node) {
                return node.nodeValue;
            }
            return null;
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding.prototype = {
    _xpath: null,

    get_xpath: Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$get_xpath,
    set_xpath: Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$set_xpath,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$initialize,

    _getSourceValue: Inflectra$SpiraTest$Web$AjaxExtensions$XPathBinding$_getSourceValue
}
Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding.descriptor = {
    properties: [ {name: 'xpath', type: String} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.XPathBinding', Inflectra.SpiraTest.Web.AjaxExtensions.BindingBase);
Inflectra.SpiraTest.Web.AjaxExtensions.Action = function Inflectra$SpiraTest$Web$AjaxExtensions$Action() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Action.initializeBase(this);
}








    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_eventSource() {
        /// <value type="Object" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._eventSource;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$set_eventSource(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Object, mayBeNull: true}]);
        if (e) throw e;

        Sys.Debug.assert(!this.get_isInitialized(), "You cannot change the event source after initialization.");
        if(!this.get_isInitialized()) {
            this._eventSource = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_eventName() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._eventName;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$set_eventName(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        Sys.Debug.assert(!this.get_isInitialized(), "You cannot change the event after initialization.");
        if(!this.get_isInitialized()) {
            this._eventName = value;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_target() {
        /// <value type="Object" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._target;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$set_target(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Object, mayBeNull: true}]);
        if (e) throw e;

        this._target = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_dataContext() {
        /// <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Action"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_eventArgs() {
        /// <value type="Sys.EventArgs"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._eventArgs;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_result() {
        /// <value type="Object" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._result;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_sender() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._eventSource;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_bindings() {
        if (arguments.length !== 0) throw Error.parameterCount();
        if(!this._bindings) {
            this._bindings = Sys.Component.createCollection(this);
            this._bindings.add_collectionChanged(Function.createDelegate(this, this._bindingChanged));
        }
        return this._bindings;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$_bindingChanged(sender, args) {
        if(args.get_action() === Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Add) {
            args.get_changedItem().set_automatic(false);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$dispose() {
        // stop listening to source events
        if(this._sourceHandler) {
            this._eventSource["remove_" + this._eventName](this._sourceHandler);
            this._sourceHandler = null;
        }
        // stop listening to source notify dispose events
        if(this._sourceDisposingHandler) {
            this._eventSource.remove_disposing(this._sourceDisposingHandler);
            this._sourceDisposingHandler = null;
        }
        // stop listening to target notify dispose events
        if(this._targetDisposingHandler) {
            this._target.remove_disposing(this._targetDisposingHandler);
            this._targetDisposingHandler = null;
        }
        
        this._target = null;
        this._eventSource = null;
        Inflectra.SpiraTest.Web.AjaxExtensions.Action.callBaseMethod(this, 'dispose');
        // bindings will dispose themselves since they are listening to INotifyDisposing
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$performAction() {
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$execute(sender, eventArgs) {
        /// <param name="sender" type="Object"></param>
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "sender", type: Object},
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        this._eventArgs = eventArgs;
        
        var bindings = this.get_bindings();
        var binding;
        var bindingType;
        if(bindings) {
            var i;
            for (i = 0; i < bindings.length; i++) {
                binding = bindings[i];
                bindingType = binding ? Object.getType(binding) : null;
                if(bindingType && (bindingType === Inflectra.SpiraTest.Web.AjaxExtensions.Binding || Inflectra.SpiraTest.Web.AjaxExtensions.Binding.inheritsFrom(bindingType))) {
                    if(binding.get_direction() !== Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.Out) {
                        // only execute if direction is In or InOut
                        binding.evaluateIn();
                    }
                }
                else {
                    binding.evaluateIn();
                }
            }
        }
        
        this._result = this.performAction();

        if(bindings) {
            for (i = 0; i < bindings.length; i++) {
                binding = bindings[i];
                bindingType = binding ? Object.getType(binding) : null;
                if(bindingType && (bindingType === Inflectra.SpiraTest.Web.AjaxExtensions.Binding || Inflectra.SpiraTest.Web.AjaxExtensions.Binding.inheritsFrom(bindingType))) {
                    if(binding.get_direction() !== Inflectra.SpiraTest.Web.AjaxExtensions.BindingDirection.In) {
                        // only execute if direction is Out or InOut
                        binding.evaluateOut();
                    }
                }
                else {
                    binding.evaluateOut();
                }
            }
        }
        
        this._eventArgs = null;
        this._result = null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$initialize() {
        if(this._eventSource) {
            // attach this action to the eventsource's event by name.
            // note that we must go through the type descriptor, because the eventInfo.name
            // may not match the case of _eventName, as is the case with Safari.
            // If we just did _eventSource['add_' + name] we may not find it due to the case difference.
            // The type descriptor has the correct case, and via double registration getEvents[name] always works.
            var td = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getTypeDescriptor(this._eventSource);
            if(td) {
                if(Sys.INotifyDisposing.isImplementedBy(this._eventSource)) {
                    this._sourceDisposeHandler = Function.createDelegate(this, this._sourceDisposing);
                    this._eventSource.add_disposing(this._sourceDisposeHandler);
                }
            
                Sys.Debug.assert(!!this.get_eventName(), "You must specify the event name the action will handle.");        
                var eventInfo = td._getEvents()[this.get_eventName()];
                Sys.Debug.assert(!!eventInfo, String.format("The event source does not have a '{0}' event.", this.get_eventName()));
                Sys.Debug.assert(!!this._eventSource["add_" + eventInfo.name], String.format("No add method found for event '{0}'.", eventInfo.name));
                this._sourceHandler = Function.createDelegate(this, this.execute);
                this._eventName = eventInfo.name;
                this._eventSource["add_" + this._eventName](this._sourceHandler);
            }
        }
        
        if(this._target && Sys.INotifyDisposing.isImplementedBy(this._target)) {
            this._targetDisposeHandler = Function.createDelegate(this, this._targetDisposing);
            this._target.add_disposing(this._targetDisposeHandler);
        }
        
        Inflectra.SpiraTest.Web.AjaxExtensions.Action.callBaseMethod(this, 'initialize');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$setOwner(eventSource) {
        /// <param name="eventSource" type="Object"></param>
        var e = Function._validateParams(arguments, [
            {name: "eventSource", type: Object}
        ]);
        if (e) throw e;

        Sys.Debug.assert(!this.get_isInitialized(), "You cannot change the event source after initialization.");
        if(!this.get_isInitialized()) {
            this._eventSource = eventSource;
        }
        // TODO: Is this the right place to do something if target has not
        //       been set? Previously we were doing a check in parseFromMarkup
        //       but now we can't since we're potentially setting references
        //       in a second pass.
        //       Alternatively we would override initialize and do it there.
        //       But regardless, another question is what do we do? Error message?
        //       no-op the action?
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$_sourceDisposing() {
        this.dispose();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Action$_targetDisposing() {
        this.dispose();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Action.prototype = {
    _eventSource: null,
    _eventName: null,
    _eventArgs: null,
    _result: null,
    _target: null,
    _bindings: null,
    
    get_eventSource: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_eventSource,
    set_eventSource: Inflectra$SpiraTest$Web$AjaxExtensions$Action$set_eventSource,
    
    get_eventName: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_eventName,
    set_eventName: Inflectra$SpiraTest$Web$AjaxExtensions$Action$set_eventName,
    
    get_target: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_target,
    set_target: Inflectra$SpiraTest$Web$AjaxExtensions$Action$set_target,
    
    get_dataContext: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_dataContext,
    
    get_eventArgs: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_eventArgs,
    
    get_result: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_result,

    get_sender: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_sender,
    
    get_bindings: Inflectra$SpiraTest$Web$AjaxExtensions$Action$get_bindings,
    
    _bindingChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Action$_bindingChanged,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Action$dispose,
    
    performAction: Inflectra$SpiraTest$Web$AjaxExtensions$Action$performAction,
    
    execute: Inflectra$SpiraTest$Web$AjaxExtensions$Action$execute,
    
    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Action$initialize,
    
    setOwner: Inflectra$SpiraTest$Web$AjaxExtensions$Action$setOwner,
    
    _sourceDisposing: Inflectra$SpiraTest$Web$AjaxExtensions$Action$_sourceDisposing,
    _targetDisposing: Inflectra$SpiraTest$Web$AjaxExtensions$Action$_targetDisposing
}
Inflectra.SpiraTest.Web.AjaxExtensions.Action.descriptor = {
    properties: [   {name: 'eventSource', type: Object},
                    {name: 'eventName', type: String},
                    {name: 'bindings', type: Array, readOnly: true},
                    {name: 'eventArgs', type: Sys.EventArgs, readOnly: true},
                    {name: 'result', type: Object, readOnly: true},
                    {name: 'sender', type: Object, readOnly: true},
                    {name: 'target', type: Object} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Action.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Action', Sys.Component, Inflectra.SpiraTest.Web.AjaxExtensions.IAction);

Inflectra.SpiraTest.Web.AjaxExtensions.Action.parseFromMarkup = function Inflectra$SpiraTest$Web$AjaxExtensions$Action$parseFromMarkup(type, node, markupContext) {
    /// <param name="type" type="Type"></param>
    /// <param name="node"></param>
    /// <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext"></param>
    /// <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.Action"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "node"},
        {name: "markupContext", type: Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext}
    ]);
    if (e) throw e;

    var newAction = new type();
    
    var action = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(newAction, node, markupContext);
    if (action) {
        markupContext.addComponent(action);
        return action;
    }
    else {
        newAction.dispose();
    }

    return null;
}
Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction = function Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction() {
    Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction.initializeBase(this);
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$get_method() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._method;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$set_method(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        this._method = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$get_parameters() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._parameters) {
            this._parameters = { };
        }
        return this._parameters;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$performAction() {
        /// <returns type="Function"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        return Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.invokeMethod(this.get_target(), this._method, this._parameters);
    }
Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction.prototype = {
    _method: null,
    _parameters: null,
    
    get_method: Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$get_method,
    set_method: Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$set_method,
    
    get_parameters: Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$get_parameters,
    
    performAction: Inflectra$SpiraTest$Web$AjaxExtensions$InvokeMethodAction$performAction
}
Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction.descriptor = {
    properties: [ {name: 'method', type: String},
                  {name: 'parameters', type: Object, readOnly: true} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.InvokeMethodAction', Inflectra.SpiraTest.Web.AjaxExtensions.Action);

Inflectra.SpiraTest.Web.AjaxExtensions.SetPropertyAction = function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction() {
    Inflectra.SpiraTest.Web.AjaxExtensions.SetPropertyAction.initializeBase(this);
}





    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$get_property() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._property;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$set_property(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        this._property = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$get_propertyKey() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._propertyKey;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$set_propertyKey(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._propertyKey = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$get_value() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._value;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$set_value(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._value = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$performAction() {
        /// <returns type="Object" mayBeNull="true"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.setProperty(this.get_target(), this._property, this._value, this._propertyKey);
        return null;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.SetPropertyAction.prototype = {
    _property: null,
    _propertyKey: null,
    _value: null,
    
    get_property: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$get_property,
    set_property: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$set_property,
    
    get_propertyKey: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$get_propertyKey,
    set_propertyKey: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$set_propertyKey,
    
    get_value: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$get_value,
    set_value: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$set_value,
    
    performAction: Inflectra$SpiraTest$Web$AjaxExtensions$SetPropertyAction$performAction
}
Inflectra.SpiraTest.Web.AjaxExtensions.SetPropertyAction.descriptor = {
    properties: [   {name: 'property', type: String},
                    {name: 'propertyKey' },
                    {name: 'value', type: String} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.SetPropertyAction.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.SetPropertyAction', Inflectra.SpiraTest.Web.AjaxExtensions.Action);

Inflectra.SpiraTest.Web.AjaxExtensions.PostBackAction = function Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction() {
    Inflectra.SpiraTest.Web.AjaxExtensions.PostBackAction.initializeBase(this);
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$get_target() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._target;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$set_target(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        this._target = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$get_eventArgument() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._eventArgument;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$set_eventArgument(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        this._eventArgument = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$performAction() {
        /// <returns type="Object"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        __doPostBack(this.get_target(), this.get_eventArgument());
        return null;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.PostBackAction.prototype = {
    _eventArgument: null,
    
    // note: redefine get/set target so the xml doc can change the type to String
    get_target: Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$get_target,
    set_target: Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$set_target,
        
    get_eventArgument: Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$get_eventArgument,
    set_eventArgument: Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$set_eventArgument,
    
    performAction: Inflectra$SpiraTest$Web$AjaxExtensions$PostBackAction$performAction
}
Inflectra.SpiraTest.Web.AjaxExtensions.PostBackAction.descriptor = {
    properties: [   {name: 'eventArgument', type: String},
                    {name: 'target', type: String} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.PostBackAction.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.PostBackAction', Inflectra.SpiraTest.Web.AjaxExtensions.Action);
///////////////////////////////////////////////////////////////////////////////
// Inflectra.SpiraTest.Web.AjaxExtensions.Counter
// CONSIDER: An event for when the counter reaches its lower or upper bound would be useful
Inflectra.SpiraTest.Web.AjaxExtensions.Counter = function Inflectra$SpiraTest$Web$AjaxExtensions$Counter() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Counter.initializeBase(this);
}





    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_canDecrement() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return isNaN(this._lowerBound) || (this._value > this._lowerBound);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_canIncrement() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return isNaN(this._upperBound) || (this._value < this._upperBound);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_lowerBound() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._lowerBound;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$set_lowerBound(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        if ((isNaN(value) && isNaN(this._lowerBound)) || (value === this._lowerBound)) return;
        var oldCanDecrement = this.get_canDecrement();
        this._lowerBound = value;
        this.raisePropertyChanged('lowerBound');
        if (oldCanDecrement !== this.get_canDecrement()) {
            this.raisePropertyChanged('canDecrement');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_upperBound() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._upperBound;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$set_upperBound(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        if ((isNaN(value) && isNaN(this._upperBound)) || (value === this._upperBound)) return;
        var oldCanIncrement = this.get_canIncrement();
        this._upperBound = value;
        this.raisePropertyChanged('upperBound');
        if (oldCanIncrement !== this.get_canIncrement()) {
            this.raisePropertyChanged('canIncrement');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_value() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._value;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$set_value(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        if ((isNaN(this._lowerBound) || (value >= this._lowerBound)) &&
            (isNaN(this._upperBound) || (value <= this._upperBound)) && (this._value !== value)) {
            var oldCanDecrement = this.get_canDecrement();
            var oldCanIncrement = this.get_canIncrement();
            this._value = value;
            this.raisePropertyChanged('value');
            if (oldCanDecrement !== this.get_canDecrement()) {
                this.raisePropertyChanged('canDecrement');
            }
            if (oldCanIncrement !== this.get_canIncrement()) {
                this.raisePropertyChanged('canIncrement');
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$decrement() {
        this.set_value(this._value - 1);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Counter$increment() {
        this.set_value(this._value + 1);
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Counter.prototype = {
    _value: 0,
    _lowerBound: Number.NaN,
    _upperBound: Number.NaN,
    
    get_canDecrement: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_canDecrement,
    
    get_canIncrement: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_canIncrement,
    
    get_lowerBound: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_lowerBound,
    set_lowerBound: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$set_lowerBound,
    
    get_upperBound: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_upperBound,
    set_upperBound: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$set_upperBound,
    
    get_value: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$get_value,
    set_value: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$set_value,
    
    decrement: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$decrement,
    
    increment: Inflectra$SpiraTest$Web$AjaxExtensions$Counter$increment
}
Inflectra.SpiraTest.Web.AjaxExtensions.Counter.descriptor = {
    properties: [   {name: 'value', type: Number},
                    {name: 'lowerBound', type: Number},
                    {name: 'upperBound', type: Number},
                    {name: 'canDecrement', type: Boolean, readOnly: true},
                    {name: 'canIncrement', type: Boolean, readOnly: true} ],
    methods: [ {name: 'increment'}, {name: 'decrement'} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Counter.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Counter', Sys.Component);
///////////////////////////////////////////////////////////////////////////////
// Inflectra.SpiraTest.Web.AjaxExtensions.Timer

// REVIEW: 1.0 will have a timer but it wont be a component, we need to have valueadd with its own timer version (this one).
// CONSISER: Timer should be a more efficient (see comment below)
// CONSISER: Create a realtime timer that is based on the clock instead of setTimeout, so you can be at least somewhat sure that
//           a 1 second interval is really executed once a second. A clock based timer would always have a low setTimeout interval
//           and then would diff the clock to know when the fire a tick event.

Inflectra.SpiraTest.Web.AjaxExtensions.Timer = function Inflectra$SpiraTest$Web$AjaxExtensions$Timer() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Timer.initializeBase(this);
    
    this._interval = 1000;
    this._enabled = false;
    this._timer = null;
}


    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$get_interval() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._interval;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$set_interval(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        if (this._interval !== value) {
            this._interval = value;
            this.raisePropertyChanged('interval');
            
            if (!this.get_isUpdating() && (this._timer !== null)) {
                this.restartTimer();
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$get_enabled() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._enabled;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$set_enabled(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        if (value !== this.get_enabled()) {
            this._enabled = value;
            this.raisePropertyChanged('enabled');
            if (!this.get_isUpdating()) {
                if (value) {
                    this._startTimer();
                }
                else {
                    this._stopTimer();
                }
            }
        }
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$add_tick(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("tick", handler);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$remove_tick(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("tick", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$dispose() {
        this.set_enabled(false);
        this._stopTimer();
        
        Inflectra.SpiraTest.Web.AjaxExtensions.Timer.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$updated() {
        Inflectra.SpiraTest.Web.AjaxExtensions.Timer.callBaseMethod(this, 'updated');

        if (this._enabled) {
            this.restartTimer();
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$_timerCallback() {
    // consider: this is not terribly efficient if the timer is set to a low interval.
    // There should be a version of a timer which cannot be changed or ignores changes after
    // it is enabled so it doesn't have to constantly re-retrieve a handler which is expensive.
    // most of the time timers are used internally by a component where they dont expect changes to the
    // timer.
        var handler = this.get_events().getHandler("tick");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$restartTimer() {
        this._stopTimer();
        this._startTimer();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$_startTimer() {
        this._timer = window.setInterval(Function.createDelegate(this, this._timerCallback), this._interval);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Timer$_stopTimer() {
        window.clearInterval(this._timer);
        this._timer = null;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Timer.prototype = {
    get_interval: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$get_interval,
    set_interval: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$set_interval,
    
    get_enabled: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$get_enabled,
    set_enabled: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$set_enabled,

    // events
    add_tick: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$add_tick,
    remove_tick: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$remove_tick,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$dispose,
    
    updated: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$updated,

    _timerCallback: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$_timerCallback,
    
    restartTimer: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$restartTimer,

    _startTimer: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$_startTimer,

    _stopTimer: Inflectra$SpiraTest$Web$AjaxExtensions$Timer$_stopTimer
}

Inflectra.SpiraTest.Web.AjaxExtensions.Timer.descriptor = {
    properties: [   {name: 'interval', type: Number},
                    {name: 'enabled', type: Boolean} ],
    events: [ {name: 'tick'} ]
}

Inflectra.SpiraTest.Web.AjaxExtensions.Timer.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Timer', Sys.Component);
Inflectra.SpiraTest.Web.AjaxExtensions.ITask = function Inflectra$SpiraTest$Web$AjaxExtensions$ITask() {
    throw Error.notImplemented();
}


    function Inflectra$SpiraTest$Web$AjaxExtensions$ITask$execute() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.ITask.prototype = {
    // bool execute();
    execute: Inflectra$SpiraTest$Web$AjaxExtensions$ITask$execute
}
Inflectra.SpiraTest.Web.AjaxExtensions.ITask.registerInterface('Inflectra.SpiraTest.Web.AjaxExtensions.ITask');

Inflectra.SpiraTest.Web.AjaxExtensions.Reference = function Inflectra$SpiraTest$Web$AjaxExtensions$Reference() {
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$Reference$get_component() {
        /// <value type="Sys.Component" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._component;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Reference$set_component(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Sys.Component, mayBeNull: true}]);
        if (e) throw e;

        this._component = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Reference$get_onscriptload() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._onload;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Reference$set_onscriptload(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        this._onload = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Reference$dispose() {
        this._component = null;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Reference.prototype = {
    _component: null,
    _onload: null,
    
    get_component: Inflectra$SpiraTest$Web$AjaxExtensions$Reference$get_component,
    set_component: Inflectra$SpiraTest$Web$AjaxExtensions$Reference$set_component,
    
    get_onscriptload: Inflectra$SpiraTest$Web$AjaxExtensions$Reference$get_onscriptload,
    set_onscriptload: Inflectra$SpiraTest$Web$AjaxExtensions$Reference$set_onscriptload,
    
    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Reference$dispose
}
Inflectra.SpiraTest.Web.AjaxExtensions.Reference.descriptor = {
    properties: [ { name: 'component', type: Object },
                  { name: 'onscriptload', type: String } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Reference.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Reference', null, Sys.IDisposable);

Inflectra.SpiraTest.Web.AjaxExtensions.Reference.parseFromMarkup = function Inflectra$SpiraTest$Web$AjaxExtensions$Reference$parseFromMarkup(type, node, markupContext) {
    var newReference = new Inflectra.SpiraTest.Web.AjaxExtensions.Reference();
    var reference = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(newReference, node, markupContext);
    if (reference) {
        return reference;
    }
    newReference.dispose();
    return null;
}
Inflectra.SpiraTest.Web.AjaxExtensions._TaskManager = function Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager() {
    Sys.Application.registerDisposableObject(this);
    this._tasks = [];
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$addTask(task) {
        /// <param name="task" type="Inflectra.SpiraTest.Web.AjaxExtensions.ITask"></param>
        var e = Function._validateParams(arguments, [
            {name: "task", type: Inflectra.SpiraTest.Web.AjaxExtensions.ITask}
        ]);
        if (e) throw e;

        Sys.Debug.assert(Sys.IDisposable.isImplementedBy(task), 'The task must implement IDisposable');
        Array.enqueue(this._tasks, task);
        this._startTimeout();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$dispose() {
        if (this._timeoutCookie) {
            window.clearTimeout(this._timeoutCookie);
        }

        if (this._tasks && this._tasks.length) {
            for (var i = this._tasks.length - 1; i >= 0; i--) {
                this._tasks[i].dispose();
            }
        }
        this._tasks = null;
        this._timeoutHandler = null;
        Sys.Application.unregisterDisposableObject(this);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$_onTimeout() {
        this._timeoutCookie = 0;

        // Get the next task and execute it
        var task = Array.dequeue(this._tasks);
        if (!task.execute()) {
            // If the task was not yet complete, add it back to the queue
            Array.enqueue(this._tasks, task);
        }

        // If any tasks remain, schedule the next one to run
        if (this._tasks.length) {
            this._startTimeout();
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$_startTimeout() {
        if (!this._timeoutCookie) {
            if (!this._timeoutHandler) {
                this._timeoutHandler = Function.createDelegate(this, this._onTimeout);
            }
            this._timeoutCookie = window.setTimeout(this._timeoutHandler, /* interval */ 0);
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions._TaskManager.prototype = {
    _timeoutCookie: null,
    _timeoutHandler: null,

    addTask: Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$addTask,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$dispose,

    _onTimeout: Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$_onTimeout,

    _startTimeout: Inflectra$SpiraTest$Web$AjaxExtensions$_TaskManager$_startTimeout
}
Inflectra.SpiraTest.Web.AjaxExtensions._TaskManager.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions._TaskManager', null, Sys.IDisposable);
Inflectra.SpiraTest.Web.AjaxExtensions.TaskManager = new Inflectra.SpiraTest.Web.AjaxExtensions._TaskManager();

Type.registerNamespace('Inflectra.SpiraTest.Web.AjaxExtensions.Net');
Inflectra.SpiraTest.Web.AjaxExtensions.Net.ServiceMethodRequest = function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Net.ServiceMethodRequest.initializeBase(this);
}










    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_url() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._url;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_url(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        this._url = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_methodName() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._methodName;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_methodName(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        this._methodName = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_useGet() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._useGet;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_useGet(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        this._useGet = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_parameters() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (this._parameters === null) {
            this._parameters = { };
        }
        return this._parameters;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_result() {
        /// <value type="Object" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._result;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_timeoutInterval() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._timeoutInterval;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_timeoutInterval(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._timeoutInterval = value;
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$add_completed(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("completed", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$remove_completed(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("completed", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$add_timeout(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("timeout", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$remove_timeout(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("timeout", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$add_error(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("error", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$remove_error(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("error", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$invoke(userContext) {
        /// <param name="userContext" mayBeNull="true" optional="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "userContext", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        if (this._request !== null) {
            return false;
        }

        // Added the useGet property to allow you to set the WebMethod's httpVerb.  However, currently
        // there is no way to specify parameters for post requests.
        var params = {parameters: this.get_parameters(), loadMethod: ""};
        this._request = Sys.Net.WebServiceProxy.invoke(this._url, this._methodName, this._useGet, params, onMethodComplete, onMethodError, /*userContext*/this, this._timeoutInterval);

        function onMethodComplete(result, target, methodName) {
            target._request = null;
            target._userContext = userContext;
            target._result = result;
            var handler = target.get_events().getHandler("completed");
            if (handler) {
                handler(target, Sys.EventArgs.Empty);
            }
        }

        function onMethodError(result, target, methodName) {
            target._request = null;
            target._userContext = userContext;
            // CONSIDER: should the error object (result) have its own member _error?
            target._result = result;
            var isTimeout=false;
            if(result.get_errorStatus) isTimeout = (result.get_errorStatus() === 2);
            else if(result.get_timedOut) isTimeout = result.get_timedOut();
            var handler;
            if (isTimeout) {
                handler = target.get_events().getHandler("timeout");
            }
            else {
                handler = target.get_events().getHandler("error");
            }
            if (handler) {
                handler(target, Sys.EventArgs.Empty);
            }
        }
        return true;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Net.ServiceMethodRequest.prototype = {
    _url: null,
    _methodName: null,
    _parameters: null,
    _userContext: null,
    _result: null,
    _request: null,
    _timeoutInterval: 0,
    _useGet: true,
    
    get_url: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_url,
    set_url: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_url,

    get_methodName: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_methodName,
    set_methodName: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_methodName,
    
    get_useGet: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_useGet,
    set_useGet: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_useGet,

    get_parameters: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_parameters,
    
    get_result: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_result,

    get_timeoutInterval: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$get_timeoutInterval,
    
    set_timeoutInterval: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$set_timeoutInterval,
    
    // Events
    add_completed: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$add_completed,
    
    remove_completed: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$remove_completed,

    add_timeout: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$add_timeout,
    
    remove_timeout: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$remove_timeout,
    
    add_error: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$add_error,
    
    remove_error: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$remove_error,

    invoke: Inflectra$SpiraTest$Web$AjaxExtensions$Net$ServiceMethodRequest$invoke
}
// Removed in ValueAdd: response, priority, appUrl, abort (event)
Inflectra.SpiraTest.Web.AjaxExtensions.Net.ServiceMethodRequest.descriptor = {
    properties: [   {name: 'url', type: String},
                    {name: 'methodName', type: String},
                    {name: 'parameters', type: Object, readOnly: true},
                    {name: 'result', type: Object, readOnly: true},
                    {name: 'timeoutInterval', type: Number},
                    {name: 'useGet', type: Boolean} ],
    methods: [ {name: 'invoke', parameters: [ {name: "userContext" } ] } ],
    events: [   {name: 'completed'},
                {name: 'timeout'},
                {name: 'error'} ]
}

Inflectra.SpiraTest.Web.AjaxExtensions.Net.ServiceMethodRequest.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Net.ServiceMethodRequest', Sys.Component);


Sys.Net._WebRequestManager.descriptor = {
    properties: [ { name: 'defaultTimeout', type: Number },
                  { name: 'defaultExecutorType', type: String } ]
}

Sys.Net._WebRequestManager.parseFromMarkup = function Sys$Net$_WebRequestManager$parseFromMarkup(type, node, markupContext) {
    if (!markupContext.get_isGlobal()) {
        return null;
    }
    
    Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(Sys.Net.WebRequestManager, node, markupContext);
    return Sys.Net.WebRequestManager;
}

Type.registerNamespace('Inflectra.SpiraTest.Web.AjaxExtensions.Data');
Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData() {
    throw Error.notImplemented();
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$add() {
        throw Error.notImplemented();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$clear() {
        throw Error.notImplemented();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$get_length() {
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$getRow() {
        throw Error.notImplemented();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$remove() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.prototype = {
    add: Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$add,
    clear: Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$clear,
    get_length: Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$get_length,
    getRow: Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$getRow,
    remove: Inflectra$SpiraTest$Web$AjaxExtensions$Data$IData$remove
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.registerInterface('Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData');
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowState() {
    throw Error.invalidOperation();
}






Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.prototype = {
    Unchanged: 0,
    Added: 1,
    Deleted: 2,
    Detached: 3,
    Modified: 4
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.registerEnum('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState');
Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$SortDirection() {
    throw Error.invalidOperation();
}



Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.prototype = {
    Ascending: 0,
    Descending: 1
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.registerEnum('Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection');

Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$ServiceType() {
    throw Error.invalidOperation();
}



Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType.prototype = {
    DataService: 0,
    Handler: 1
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType.registerEnum('Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType');

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn(columnName, dataType, defaultValue, isKey, isReadOnly) {
    // <param name="columnName" type="String">Column name.</param>
    // <param name="dataType" type="Sys.Type">Data type.</param>
    // <param name="defaultValue" mayBeNull="true">Default value.</param>
    // <param name="isKey" type="Boolean">True if primary key, false if not.</param>
    // <param name="isReadOnly" type="Boolean">True if read only, false if not.</param>
    this._columnName = columnName;
    this._dataType = dataType;
    this._defaultValue = defaultValue;
    this._readOnly = isReadOnly;
    this._key = isKey;
}


    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_columnName() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Column name.</value>
        return this._columnName;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_dataType() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Sys.Type">Data type.</value>
        return this._dataType;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_defaultValue() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Default value.</value>
        return this._defaultValue;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_isKey() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True if primary key, false if not.</value>
        return this._key;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_readOnly() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True if read only, false if not.</value>
        return !!this._readOnly;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$dispose() {
        this._columnName = null;
        this._dataType = null;
        this._defaultValue = null;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn.prototype = {
    get_columnName: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_columnName,
    
    get_dataType: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_dataType,
    
    get_defaultValue: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_defaultValue,
    
    get_isKey: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_isKey,
    
    get_readOnly: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$get_readOnly,
    
    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$dispose
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn.parseFromJson = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataColumn$parseFromJson(json) {
    /// <param name="json" type="Object" optional="false" mayBeNull="false"></param>
    var e = Function._validateParams(arguments, [
        {name: "json", type: Object}
    ]);
    if (e) throw e;

    return new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn(json.name, typeof(json.dataType === 'string') ? eval(json.dataType) : json.dataType, json.defaultValue, json.isKey, json.readOnly);
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn.descriptor = {
    properties: [ { name: 'columnName', type: String, readOnly: true },
                  { name: 'dataType', type: Sys.Type, readOnly: true },
                  { name: 'defaultValue', readOnly: true },
                  { name: 'isKey', type: Boolean, readOnly: true },
                  { name: 'readOnly', type: Boolean, readOnly: true } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn', null, Sys.IDisposable);
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow(objectDataRow, dataTableOwner, index) {
    // <param name="objectDataRow">Data row.</param>
    // <param name="dataTableOwner" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable" mayBeNull="true">Table to which row belongs.</param>
    // <param name="index" type="Number" optional="true" mayBeNull="true">Row index.</param>
    this._owner = dataTableOwner;
    this._row = objectDataRow;
    this._index = index;
}





    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_events() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Sys.EventHandlerList">Collection of event handlers.</value>
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$add_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        if(this._disposed) {
            throw Error.invalidOperation("This instance is disposed.");
            return;
        }
        this.get_events().addHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$remove_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        if(this._disposed) return;
        this.get_events().removeHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_onPropertyChanged(propertyName) {
        // <param name="propertyName" type="String">Property name.</param>
        var handler = this.get_events().getHandler("propertyChanged");
        if (handler) {
            handler(this, new Sys.PropertyChangedEventArgs(propertyName));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_isDirty() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True if data is dirty, false if not.</value>
        return typeof(this._row._original) === "object";
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_index() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Number">Row index.</value>
        return this._index;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_set_index(index) {
        // <param name="index" type="Number">Row index.</param>
        this._index = index;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_rowObject() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value></value>
        return typeof(this._row._rowObject) !== "undefined" ? this._row._rowObject : this._row;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_selected() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True if row is selected, false if not.</value>
        return this._selected;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$set_selected(value) {
        if (this._selected !== value) {
            this._selected = value;
            this._onPropertyChanged("$selected");
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_state() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState">Row state.</value>
        return this._state;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_set_state(value) {
        this._state = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_table() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable" mayBeNull="true">Table to which row belongs.</value>
        return this._owner;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_set_table(value) {
        this._owner = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$dispose() {
        delete this._events;
        this._row = null;
        this._owner = null;
        this._disposed = true;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$getProperty(name, key) {
        // <param name="name" type="String">Property name.</param>
        // <param name="key" optional="true" mayBeNull="true">Property key.</param>
        // <returns>Property value.</returns>
        if (!name) {
            return typeof(this._row._rowObject) !== "undefined" ? this._row._rowObject : this._row;
        }
        switch(name) {
        case "$isDirty":
            return this.get_isDirty();

        case "$index":
            return this._index;

        case "$selected":
            return this.get_selected();
        }

        return Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(this._row, name, key);
//        if (this._row.descriptor || Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider.isImplementedBy(this._row)) {
//            return Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(this._row, name, key);
//        }
//        return this._row[name];
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$setProperty(name, value, key) {
        // <param name="name" type="String">Property name.</param>
        // <param name="value" mayBeNull="true">Property value.</param>
        // <param name="key" optional="true" mayBeNull="true">Property key.</param>
        if (name === "$selected") {
            this.set_selected(value);
            return;
        }
            Sys.Debug.assert(!!this._owner);
            var col = this._owner.getColumn(name);
            Sys.Debug.assert(col.get_readOnly() !== true || this._state === Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Detached, "A read-only property can't be set." );
        if (this._row[name] === value) return;
        var isDirty = this.get_isDirty();
        if (!isDirty && this._owner && (this.get_state() === Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Unchanged)) {
            var original = {};
            for (var columnName in this._row) {
                if ((columnName.charAt(0) !== '_') && (typeof(this._row[columnName]) !== "function")) {
                    original[columnName] = this._row[columnName];
                }
            }
            this._row._original = original;
            this._set_state(Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Modified);
        }

// always use TD because it figures out the logic based on ITypeDescriptorProvider and ICustomTypeDescriptor
        Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.setProperty(this._row, name, value, key);
//        if (this._row.descriptor || Inflectra.SpiraTest.Web.AjaxExtensions.ITypeDescriptorProvider.isImplementedBy(this._row)) {
//            Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.setProperty(this._row, name, value, key);
//        }
//        else {
//            this._row[name] = value;
//        }
        this._onPropertyChanged(name);
        if (!isDirty) {
            this._onPropertyChanged("$isDirty");
        }
        this._owner.raiseRowChanged(this._row);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$invokeMethod(methodName, parameters) {
        // <param name="methodName" type="String" />
        // <param name="parameters" />
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.prototype = {
    _state: Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Unchanged,
    _selected: false,
    _events: null,

    get_events: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_events,

    add_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$add_propertyChanged,

    remove_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$remove_propertyChanged,

    _onPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_onPropertyChanged,

    get_isDirty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_isDirty,

    get_index: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_index,
    _set_index: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_set_index,

    get_rowObject: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_rowObject,

    get_selected: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_selected,
    set_selected: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$set_selected,

    get_state: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_state,
    _set_state: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_set_state,

    get_table: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$get_table,
    _set_table: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$_set_table,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$dispose,

    getProperty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$getProperty,

    setProperty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$setProperty,

    invokeMethod: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRow$invokeMethod
}

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.descriptor = {
    properties: [ { name: '$isDirty', type: Boolean, readOnly: true },
                  { name: '$index', type: Number, readOnly: true },
                  { name: '$selected', type: Boolean } ],
    events: [ { name: 'propertyChanged', readOnly: true } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow', null, Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor, Sys.INotifyPropertyChange, Sys.IDisposable);
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView(dataRow, index) {
    //<param name="dataRow" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Data row.</param>
    //<param name="index" type="Number">Row view index.</param>
    this._row = dataRow;
    this._index = index;
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_events() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Sys.EventHandlerList">Collection of event handlers.</value>
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$add_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        this.get_events().addHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$remove_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_onPropertyChanged(propertyName) {
        var handler = this.get_events().getHandler("propertyChanged");
        if (handler) {
            handler(this, new Sys.PropertyChangedEventArgs(propertyName));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_dataIndex() {
    if (arguments.length !== 0) throw Error.parameterCount();
        //<value type="Number">Row index.</value>
        return this._row.get_index();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_index() {
    if (arguments.length !== 0) throw Error.parameterCount();
        //<value type="Number">Row view index.</value>
        return this._index;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_set_index(value) {
        this._index = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_isDirty() {
    if (arguments.length !== 0) throw Error.parameterCount();
        //<value type="Boolean">True if data is dirty, false if not.</value>
        return this._row.get_isDirty();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_get_row() {
        return this._row;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_rowObject() {
    if (arguments.length !== 0) throw Error.parameterCount();
        //<value>Data row object.</value>
        return this._row.get_rowObject();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_selected() {
    if (arguments.length !== 0) throw Error.parameterCount();
        //<value type="Boolean">True if selected, false if not.</value>
        return this._row.get_selected();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$set_selected(value) {
        this._row.set_selected(value);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_table() {
    if (arguments.length !== 0) throw Error.parameterCount();
        //<value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable">Table that contains the row.</value>
        return this._row.get_table();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$dispose() {
        if (this._row && this._rowPropertyChanged) {
            this._row.remove_propertyChanged(this._rowPropertyChanged);
        }
        delete this._events;
        this._row = null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$initialize() {
        this._rowPropertyChanged = Function.createDelegate(this, this._onRowPropertyChanged);
        this._row.add_propertyChanged(this._rowPropertyChanged);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_onRowPropertyChanged(sender, args) {
        this._onPropertyChanged(args.get_propertyName());
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$getProperty(name, key) {
        // <param name="name" type="String">Property name.</param>
        // <param name="key" optional="true" mayBeNull="true">Property key.</param>
        // <returns>Property value.</returns>
        if (name === "$index") return this._index;
        if (name === "$dataIndex") return this._row.get_index();
        return this._row.getProperty(name, key);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$setProperty(name, value, key) {
        // <param name="name" type="String">Property name.</param>
        // <param name="value" mayBeNull="true">Property value.</param>
        // <param name="key" optional="true" mayBeNull="true">Property key.</param>
        this._row.setProperty(name, value, key);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$invokeMethod(methodName, parameters) {
        // <param name="methodName" type="String" />
        // <param name="parameters" />
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView.prototype = {
    _rowPropertyChanged: null,
    _events: null,
    
    get_events: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_events,
    
    add_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$add_propertyChanged,
    
    remove_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$remove_propertyChanged,
    
    _onPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_onPropertyChanged,

    get_dataIndex: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_dataIndex,

    get_index: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_index,
    _set_index: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_set_index,

    get_isDirty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_isDirty,

    _get_row: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_get_row,

    get_rowObject: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_rowObject,

    get_selected: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_selected,
    set_selected: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$set_selected,

    get_table: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$get_table,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$dispose,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$initialize,

    _onRowPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$_onRowPropertyChanged,

    // REVIEW: why the '$'?
    getProperty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$getProperty,

    setProperty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$setProperty,

    invokeMethod: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowView$invokeMethod
}

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView.descriptor = {
    properties: [ { name: '$dataIndex', type: Number, readOnly: true },
                  { name: '$isDirty', type: Boolean, readOnly: true },
                  { name: '$index', type: Number, readOnly: true },
                  { name: '$selected', type: Boolean } ],
    events: [ { name: 'propertyChanged', readOnly: true } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView', null, Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor, Sys.INotifyPropertyChange, Sys.IDisposable);
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection(dataRowViews, dataTable) {
    // <param name="dataRowViews" type="Array" mayBeNull="true">Data row views.</param>
    // <param name="dataTable">Data table.</param>
    this._rows = dataRowViews;
    this._dataTable = dataTable;
}






    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$get_events() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Sys.EventHandlerList">Collection of event handlers.</value>
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$add_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        this.get_events().addHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$remove_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$_onPropertyChanged(propertyName) {
        // <param name="propertyName" type="String">Property name.</param>
        var handler = this.get_events().getHandler("propertyChanged");
        if (handler) {
            handler(this, new Sys.PropertyChangedEventArgs(propertyName));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$add_collectionChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        this.get_events().addHandler("collectionChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$remove_collectionChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("collectionChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$_onCollectionChanged(action, changedItem) {
        // <param name="action" type="Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction">Change action type.</param>
        // <param name="changedItem" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Row that changed.</param>
        var handler = this.get_events().getHandler("collectionChanged");
        if (handler) {
            handler(this, new Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs(action, changedItem));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$_get_dataTable() {
        // <value>Data table.</value>
        return this._dataTable;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$get_length() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Number">Number of data rows.</value>
        return this._rows.length;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$add(rowObject) {
        // <param name="rowObject">Data row.</param>
        var row = this._dataTable.add(rowObject);
        var rv = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView(row, this._rows.length);
        rv.initialize();
        if(typeof(this._rows.add) === "function") {
            this._rows.add(rv);
        }
        else {
            Array.add(this._rows, rv);
        }
        if (this._indexToRow) {
            this._indexToRow[row.get_dataIndex()] = row;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$clear() {
        this._suspendNotifications = true;
        for (var i = this._rows.length - 1; i >= 0; i--) {
            this._dataTable.remove(this._rows[i]._get_row());
        }
        this._rows = [];
        this._indexToRow = null;
        this._suspendNotifications = false;
        this._onCollectionChanged(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Reset, null);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$getRow(index) {
        // <param name="index" value="Number">Row index.</param>
        // <returns value="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView">Data row view.</returns>
        return this._rows[index];
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$getItem(index) {
        // <param name="index" value="Number">Row index.</param>
        // <returns value="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView">Data row view.</returns>
        return this.getRow(index);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$remove(rowObject) {
        // <param name="rowObject">Data row.</param>
        this._dataTable.remove(rowObject._get_row());
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$dispose() {
        if (this._dataTable && this._tableCollectionChanged) {
            this._dataTable.remove_collectionChanged(this._tableCollectionChanged);
            this._tableCollectionChanged = null;
        }
        delete this._events;
        this._rows = null;
        this._dataTable = null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$initialize() {
        if (this._dataTable.add_collectionChanged) {
            this._tableCollectionChanged = Function.createDelegate(this, this.onTableCollectionChanged);
            this._dataTable.add_collectionChanged(this._tableCollectionChanged);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$ensureLookupTable() {
        if (!this._indexToRow) {
            this._indexToRow = [];
            for (var j = this._rows.length - 1; j >= 0; j--) {
                var row = this._rows[j];
                this._indexToRow[row.get_dataIndex()] = row;
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$onTableCollectionChanged(sender, args) {
        // <param name="sender">Event sender.</param>
        // <param name="args" type="Sys.EventArgs">Event arguments.</param>
        if (this._suspendNotifications) return;
        switch(args.get_action()) {
        case Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Reset:
            this._rows = [];
            this._indexToRow = null;
            this._onCollectionChanged(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Reset, changedItem);
            return;
        case Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Remove:
            var changedItem = args.get_changedItem();
            this.ensureLookupTable();
            var idx = changedItem.get_index();
            if (this._indexToRow[idx]) {
                if(typeof(this._rows.remove) === "function") {
                    this._rows.remove(this._indexToRow[idx]);
                }
                else {
                    Array.remove(this._rows, this._indexToRow[idx]);
                }
                delete this._indexToRow[idx];
                this._onCollectionChanged(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Remove, changedItem);
            }
            return;
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection.prototype = {
    _indexToRow: null,
    _tableCollectionChanged: null,
    _suspendNotifications: false,
    _events: null,

    get_events: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$get_events,

    add_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$add_propertyChanged,

    remove_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$remove_propertyChanged,

    _onPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$_onPropertyChanged,

    add_collectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$add_collectionChanged,

    remove_collectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$remove_collectionChanged,

    _onCollectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$_onCollectionChanged,

    _get_dataTable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$_get_dataTable,

    get_length: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$get_length,

    add: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$add,

    clear: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$clear,

    getRow: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$getRow,

    getItem: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$getItem,

    remove: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$remove,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$dispose,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$initialize,

    ensureLookupTable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$ensureLookupTable,

    onTableCollectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataRowCollection$onTableCollectionChanged
}

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection.descriptor = {
    properties: [ { name: 'length', type: Number, readOnly: true } ],
    methods: [ { name: 'add' },
               { name: 'clear' },
               { name: 'remove' } ],
    events: [ { name: 'collectionChanged', readOnly: true },
              { name: 'propertyChanged', readOnly: true } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection', null, Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData, Sys.INotifyPropertyChange, Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged, Sys.IDisposable);
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable(columns, tableArray) {
    // <param name="columns">Array of columns.</param>
    // <param name="tableArray">Array of row data.</param>
    this._array = Array.isInstanceOfType(tableArray) ? tableArray : [];
    this._columns = Array.isInstanceOfType(columns) ? columns : [];
    this._rows = [];
    this._deletedRows = [];
    this._newRows = [];
    this._updatedRows = [];
    this._columnDictionary = {};
    this._keys = null;
    this._events = null;
}


    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_events() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Sys.EventHandlerList">Collection of event handlers.</value>
        if (!this._events) {
            this._events = new Sys.EventHandlerList();
        }
        return this._events;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$add_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        if(this._disposed) return;
        this.get_events().addHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$remove_propertyChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        if(this._disposed) return;
        this.get_events().removeHandler("propertyChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$_onPropertyChanged(propertyName) {
        // <param name="propertyName" type="String">Property name.</param>
        if(this._disposed) return;
        var handler = this.get_events().getHandler("propertyChanged");
        if (handler) {
            handler(this, new Sys.PropertyChangedEventArgs(propertyName));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$add_collectionChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        if(this._disposed) return;
        this.get_events().addHandler("collectionChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$remove_collectionChanged(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        if(this._disposed) return;
        this.get_events().removeHandler("collectionChanged", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$_onCollectionChanged(action, changedItem) {
        // <param name="action" type="Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction">Change action type.</param>
        // <param name="changedItem" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Row that changed.</param>
        if(this._disposed) return;
        var handler = this.get_events().getHandler("collectionChanged");
        if (handler) {
            handler(this, new Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs(action, changedItem));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_columns() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Array">Array of columns.</value>
        return this._columns;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_keyNames() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Array">Column keys.</value>
        if(this._disposed) return null;
        if (!this._keys) {
            this._keys = [];
            var len = this._columns.length;
            for (var i = 0; i < len; i++) {
                var col = this._columns[i];
                if (col.get_isKey()) {
                    Array.add(this._keys, col.get_columnName());
                }
            }
        }
        return this._keys;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_isDirty() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True if dirty, false otherwise.</value>
        if(this._disposed) return false;
        return (this._deletedRows.length !== 0) || (this._newRows.length !== 0) || (this._updatedRows.length !== 0);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_length() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Number">Number of rows.</value>
        if(this._disposed) return 0;
        return this._array.length;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$add(rowObject) {
        // <param name="rowObject">Data row object.</param>
        // <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Data row.</returns>
        if(this._disposed) return null;
        var row;
        if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.isInstanceOfType(rowObject)) {
            row = rowObject;
            Sys.Debug.assert(((row.get_table() === this) && (row.get_state() === Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Detached)) ||
                         !row.get_table(),
                         'Can\'t add a row that belongs to another table or has been added already.');
            row._set_table(this);
            rowObject = rowObject.get_rowObject();
        }
        else {
            row = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow(rowObject, this);
        }
        var index = this._array.length;
        row._set_index(index);
        var columns = this.get_columns();
        if (columns) {
            for(var i = columns.length - 1; i >= 0; i--) {
                var column = columns[i];
                if (typeof(rowObject[column.get_columnName()]) === "undefined") {
                    rowObject[column.get_columnName()] = column.get_defaultValue();
                }
            }
        }
        var oldIsDirty = this.get_isDirty();
        this._array[index] = rowObject;
        this._rows[index] = row;
        Array.add(this._newRows, rowObject);
        row._set_state(Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Added);
        this._onCollectionChanged(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Add, row);
        this._onPropertyChanged("length");
        if (!oldIsDirty) {
            this._onPropertyChanged("isDirty");
        }
        return row;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$clear() {
        if (this.get_length() > 0) {
            var oldIsDirty = this.get_isDirty();
            for (var i = this._array.length - 1; i >= 0; i--) {
                var row = this._array[i];
                if (row && !Array.contains(this._newRows, row)) {
                    Array.add(this._deletedRows, row);
                    this._rows[i]._set_state(Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Deleted);
                }
            }
            this._rows = [];
            this._array = [];
            this._newRows = [];
            this._updatedRows = [];
            this._onCollectionChanged(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Reset, null);
            this._onPropertyChanged("length");
            if (!oldIsDirty) {
                this._onPropertyChanged("isDirty");
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$createRow(initialData) {
        // <param name="initialData" mayBeNull="true" optional="true">Initial row data.</param>
        // <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Data row.</returns>
        if(this._disposed) return null;
        var obj = {};
        var undef = {};
        for (var i = this._columns.length - 1; i >= 0; i--) {
            var column = this._columns[i];
            var columnName = column.get_columnName();
            var val = undef;
            if (initialData) {
                val = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(initialData, columnName);
            }
            if ((val === undef) || (typeof(val) === "undefined")) {
                val = column.get_defaultValue();
            }
            obj[columnName] = val;
        }
        var row = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow(obj, this, -1);
        row._set_state(Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Detached);
        return row;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getChanges() {
        // <returns>Get row change counts.</returns>
        if(this._disposed) return null;
        return {updated : this._updatedRows, inserted : this._newRows, deleted : this._deletedRows};
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getColumn(name) {
        // <param name="name" type="String">Column name.</param>
        // <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn">Column.</returns>
        if(this._disposed) return null;
        var col = this._columnDictionary[name];
        if (col) {
            return col;
        }
        for (var c = this._columns.length - 1; c >= 0; c--) {
            var column = this._columns[c];
            if (column.get_columnName() === name) {
                this._columnDictionary[name] = column;
                return column;
            }
        }
        Sys.Debug.fail(String.format("Column name {0} doesn't exist.", name));
        return null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getRow(index) {
        // <param name="index" type="Number">Row index.</param>
        // <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Row.</returns>
        if(this._disposed) return null;
        var row = this._rows[index];
        if (!row) {
            var rowObject = this._array[index];
            if (rowObject) {
                row = Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.isInstanceOfType(rowObject) ? rowObject : new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow(rowObject, this, index);
                this._rows[index] = row;
            }
        }
        return row;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getItem(index) {
        // <param name="index" type="Number">Row index.</param>
        // <returns type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Row.</returns>
        return this.getRow(index);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$remove(rowObject) {
        // <param name="rowObject">Row object to remove.</param>
        if(this._disposed) return;
        if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.isInstanceOfType(rowObject)) {
            rowObject = rowObject.get_rowObject();
        }
        var oldIsDirty = this.get_isDirty();
        var index = Array.indexOf(this._array, rowObject);
        var row = this.getItem(index);
        if(typeof(this._array.removeAt) === "function") {
            this._array.removeAt(index);
        }
        else {
            Array.removeAt(this._array, index);
        }
        Array.removeAt(this._rows, index);
        index = Array.indexOf(this._newRows, rowObject);
        if (index !== -1) {
            Array.removeAt(this._newRows, index);
        }
        else {
            Array.add(this._deletedRows, rowObject);
        }
        row._set_state(Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowState.Deleted);
        this._onCollectionChanged(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Remove, row);
        this._onPropertyChanged("length");
        if (oldIsDirty !== this.get_isDirty()) {
            this._onPropertyChanged("isDirty");
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$dispose() {
        delete this._events;
        this._disposed = true;
        var i, row;
        if (this._rows) {
            for (i = this._rows.length - 1; i >= 0; i--) {
                row = this._rows[i];
                if (row) {
                    this._rows[i].dispose();
                }
            }
        }
        if (this._deletedRows) {
            for (i = this._deletedRows.length - 1; i >= 0; i--) {
                row = this._deletedRows[i];
                if (row && row.dispose) {
                    row.dispose();
                }
            }
        }
        if (this._newRows) {
            for (i = this._newRows.length - 1; i >= 0; i--) {
                row = this._newRows[i];
                if (row && row.dispose) {
                    row.dispose();
                }
            }
        }
        if (this._updatedRows) {
            for (i = this._updatedRows.length - 1; i >= 0; i--) {
                row = this._updatedRows[i];
                if (row && row.dispose) {
                    row.dispose();
                }
            }
        }
        this._rows = null;
        this._deletedRows = null;
        this._newRows = null;
        this._updatedRows = null;
        this._columns = null;
        this._array = null;
        this._keys = null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$raiseRowChanged(changedItem) {
        // <param name="changedItem">Row that changed.</param>
        if(this._disposed) return;
        if ((Array.indexOf(this._updatedRows, changedItem) === -1) &&
            (Array.indexOf(this._newRows, changedItem) === -1)) {

            var oldIsDirty = this.get_isDirty();
            Array.add(this._updatedRows, changedItem);
            if (!oldIsDirty) {
                this._onPropertyChanged("isDirty");
            }
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.prototype = {

    get_events: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_events,

    add_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$add_propertyChanged,

    remove_propertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$remove_propertyChanged,

    _onPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$_onPropertyChanged,

    add_collectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$add_collectionChanged,

    remove_collectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$remove_collectionChanged,

    _onCollectionChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$_onCollectionChanged,

    get_columns: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_columns,

    get_keyNames: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_keyNames,

    get_isDirty: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_isDirty,

    get_length: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$get_length,

    add: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$add,

    clear: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$clear,

    createRow: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$createRow,

    getChanges: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getChanges,

    getColumn: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getColumn,

    getRow: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getRow,

    getItem: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$getItem,

    remove: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$remove,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$dispose,

    raiseRowChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$raiseRowChanged
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.parseFromJson = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataTable$parseFromJson(json) {
    /// <param name="json" type="Object" optional="false" mayBeNull="false"></param>
    var e = Function._validateParams(arguments, [
        {name: "json", type: Object}
    ]);
    if (e) throw e;

    var columnArray = null;
    if(json.columns) {
        columnArray = [];
        for(var i=0; i < json.columns.length; i++) {
            Array.add(columnArray, Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataColumn.parseFromJson(json.columns[i]));
        }
    }
    // the rows go in as plain objects. The DataTable converts them to DataRows as needed.
    return new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable(columnArray, json.rows);
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.descriptor = {
    properties: [ { name: 'columns', type: Array, readOnly: true },
                  { name: 'keyNames', type: Array, readOnly: true },
                  { name: 'length', type: Number, readOnly: true },
                  { name: 'isDirty', type: Boolean, readOnly: true } ],
    methods: [ { name: 'add' },
               { name: 'clear' },
               { name: 'remove' } ],
    events: [ { name: 'collectionChanged', readOnly: true },
              { name: 'propertyChanged', readOnly: true } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable', null, Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData, Sys.INotifyPropertyChange, Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged, Sys.IDisposable);

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView() {
    // <summary>
    //   DataView filters its input data through a collection of filters.
    //   It can also paginate and sort data.
    // </summary>
    Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.initializeBase(this);
}





















    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_data() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The data that the view will filter.
        // </summary>
        // <value mayBeNull="true" optional="true">View data.</value>
        return this._data;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_data(data) {
        // <param name="data" mayBeNull="true" optional="true">View data.</param>
        Sys.Debug.assert(!data || Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(data) || Array.isInstanceOfType(data));
        if (!this._dataChangedDelegate) {
            this._dataChangedDelegate = Function.createDelegate(this, this.onDataChanged);
        }
        this._filteredTable = null;
        if (this._data && this._data.remove_collectionChanged) {
            this._data.remove_collectionChanged(this._dataChangedDelegate);
        }
        this._data = data;
        if (this._data && this._data.add_collectionChanged) {
            this._data.add_collectionChanged(this._dataChangedDelegate);
        }
        this.raisePropertyChanged('data');
        this.raisePropertyChanged('filteredData');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_filteredData() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The data after it's been filtered, paginated and sorted by the view.
        // </summary>
        // <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection">Filtered data.</value>
        this.ensureFilteredData();
        return this._filteredTable;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_filters() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The collection of filters for this view.
        // </summary>
        // <value type="Array">Filters.</value>
        if (!this._filters) {
            this._filters = Sys.Component.createCollection(this);
            if (!this._dataChangedDelegate) {
                this._dataChangedDelegate = Function.createDelegate(this, this.onDataChanged);
            }
            this._filters.add_collectionChanged(this._dataChangedDelegate);
        }
        return this._filters;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_hasNextPage() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   True if the view has a next page.
        // </summary>
        // <value type="Boolean">True if next page exists, false if not.</value>
        this.ensureFilteredData();
        return (this.get_pageIndex() < this.get_pageCount() - 1);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_hasPreviousPage() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   True if the view has a previous page.
        // </summary>
        // <value type="Boolean">True if previous page exists, false if not.</value>
        if (!this._data) return false;
        return (this.get_pageIndex() > 0);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_length() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The number of rows in the filtered data.
        // </summary>
        // <value type="Number">Number of rows in filtered data.</value>
        this.ensureFilteredData();
        return this._filteredTable ? (this._filteredTable.length? this._filteredTable.length : this._filteredTable.get_length()) : 0;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_pageCount() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The number of pages in the filtered data.
        // </summary>
        // <value type="Number">Number of pages in filtered data.</value>
        if (this._pageSize === 0) {
            return 1;
        }
        this.ensureFilteredData();
        if (!this._filteredRows) return 1;
        return Math.floor((this._filteredRows.length - 1) / this._pageSize) + 1;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_pageIndex() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The index of the current page.
        // </summary>
        // <value type="Number">Current page index.</value>
        return this._pageIndex;
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_pageIndex(value, dontRaiseFilteredDataChanged) {
        // <param name="value" type="Number">Page index.</param>
        // <param name="dontRaiseFilteredDataChanged" type="Boolean" optional="true" mayBeNull="true">False to raise change event, true to ignore.</param>
        Sys.Debug.assert(value >= 0, "pageIndex should be superior or equal to zero.");
        var count = this.get_pageCount();
        if (value >= count) {
            value = (count > 0 ? count - 1 : 0);
        }
        if (value !== this._pageIndex) {
            var oldState = this.prepareChange();
            this._pageIndex = value;
            this._paginatedRows = null;
            this.triggerChangeEvents(oldState, false);
            if (!dontRaiseFilteredDataChanged) {
                this.raisePropertyChanged('filteredData');
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_pageSize() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The size (number of rows) of a page.
        // </summary>
        // <value type="Number">Number of rows per page.</value>
        return this._pageSize;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_pageSize(value) {
        if (this._pageSize !== value) {
            var oldState = this.prepareChange();
            this._pageSize = value;
            this._paginatedRows = null;
            this.triggerChangeEvents(oldState, true);
            this.raisePropertyChanged('filteredData');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_sortColumn() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The name of the column to sort on.
        // </summary>
        // <value type="String" mayBeNull="true">Sort column.</value>
        return this._sortColumn;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_sortColumn(value) {
        this.sort(value, this._sortDirection);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_sortDirection() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <summary>
        //   The direction of the sort.
        // </summary>
        // <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection">Sort direction.</value>
        return this._sortDirection;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_sortDirection(value) {
        this.sort(this._sortColumn, value);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$dispose() {
        this._disposed = true;
        if (this._filters) {
            this._filters.dispose();
            this._filters = null;
        }
        if (this._data && this._dataChangedDelegate) {
            if(this._data.removeCollectionChanged) this._data.remove_collectionChanged(this._dataChangedDelegate);
            this._dataChangedDelegate = null;
            this._data = null;
        }

        Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$getItem(index) {
        // <summary>
        //   Gets an item in the filtered data by index.
        // </summary>
        // <param name="index" type="Number">The index in the filtered data of the row to return.</param>
        // <returns>Null if the row was not found.</returns>
        return this._filteredTable ? this._filteredTable[index] : null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.callBaseMethod(this, 'initialize');
        if (this._filters) {
            for (var i = 0; i < this._filters.length; i++) {
                this._filters[i].initialize(this);
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$sort(sortColumn, sortDirection) {
        // <summary>
        //   Sets the sort column and direction.
        // </summary>
        // <param name="sortColumn" type="String">The name of the column to sort on.</param>
        // <param name="sortDirection" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection">The direction of the sort.</param>
        var colChanged = (sortColumn !== this._sortColumn);
        var dirChanged = (sortDirection !== this._sortDirection);
        if (colChanged || dirChanged) {
            this._sortColumn = sortColumn;
            this._sortDirection = sortDirection;
            if (colChanged) {
                this.raisePropertyChanged('sortColumn');
            }
            if (dirChanged) {
                this.raisePropertyChanged('sortDirection');
            }
            this._sorted = false;
            this.set_pageIndex(0, true);
            this.raisePropertyChanged('filteredData');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$_raiseFilterChanged(filter) {
        // <param name="filter" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter"></param>
        this._dataChangedDelegate(this, Sys.EventArgs.Empty);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$compareRows(row1, row2) {
        // <param name="row1" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">First row.</param>
        // <param name="row2" type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow">Second row.</param>
        // <returns type="Number">0 if rows are equal, positive if row1 is greater on ascending, negative if row2 is greater on ascending</returns>
        var sortColumn = this.get_sortColumn();
        var sortDirection = this.get_sortDirection();
        if (row1.getProperty(sortColumn) === row2.getProperty(sortColumn)) return 0;
        if (row1.getProperty(sortColumn) < row2.getProperty(sortColumn)) {
            return (sortDirection === Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending) ? -1 : 1;
        }
        return (sortDirection === Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending) ? 1 : -1;
    }


    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$onDataChanged(sender, args) {
        // <param name="sender">Event sender.</param>
        // <param name="args" type="Sys.EventArgs">Event arguments.</param>
        if(this._disposed) return;
        if (args !== Sys.EventArgs.Empty) {
            var item = args.get_changedItem();
            var filters = this.get_filters();
            if (item && !this.isValidAfterFiltering.call(item, filters, filters.length)) {
                // modified item does not pass the filters so this change doesn't affect the view
                return;
            }
            // TODO: sync pagination changes with the UI that's consuming the view.
        }
        this._filteredTable = null;
        this.raisePropertyChanged('filteredData');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$ensureFilteredData() {
        if (this._updating || !this._data) return;
        this._updating = true;
        var oldState = this.prepareChange();
        if ((typeof(this._data.length) === "number") && (this._data.length === 0)) {
            this._filteredRows = [];
            this._paginatedRows = [];
            this._filteredTable = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection([], this._data);
            this._filteredTable.initialize();
            this._sorted = true;
        }
        else {
            if (!this._filteredTable) {
                this._filteredRows = [];
                this._paginatedRows = null;
                this._filteredTable = null;
                var filters = this.get_filters();
                var filterLength = filters.length;
                var dataLength = this._data.get_length ? this._data.get_length() : (typeof(this._data.length) !== 'undefined' ? this._data.length : 0);

                for (var i = 0; i < dataLength; i++) {
                    var item = this._data.getItem? this._data.getItem(i) : this._data[i];
                    if (!Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow.isInstanceOfType(item)) {
                        item = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRow(item, null, i);
                    }

                    if (this.isValidAfterFiltering.call(this, item, filters, filterLength)) {
                        var rv = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowView(item, i);
                        rv.initialize();
                        Array.add(this._filteredRows, rv);
                    }
                }
            }
            if (!this._sorted && this._sortColumn && (this._filteredRows.length !== 0)) {
                if (!this._compareRowsDelegate) {
                    this._compareRowsDelegate = Function.createDelegate(this, this.compareRows);
                }
                this._filteredRows.sort(this._compareRowsDelegate);
                for (var i = this._filteredRows.length - 1; i >= 0; i--) {
                    this._filteredRows[i]._set_index(i);
                }
                this._sorted = true;
                this._paginatedRows = null;
                this._filteredTable = null;
            }
            if ((this._pageSize > 0) && !this._paginatedRows) {
                this._paginatedRows = [];
                this._filteredTable = null;
                var len = this._filteredRows.length;
                var start = this._pageSize * this._pageIndex;
                if (len && (start >= len)) {
                    this._pageIndex = Math.floor(len / this._pageSize) - 1;
                    start = this._pageSize * this._pageIndex;
                }
                var end = start + this._pageSize;
                for(var i = start; (i < end) && (i < len); i++) {
                    this._filteredRows[i]._set_index(i);
                    Array.add(this._paginatedRows, this._filteredRows[i]);
                }
            }
            else {
                this._paginatedRows = this._filteredRows;
            }
            if (!this._filteredTable) {
                this._filteredTable = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataRowCollection(this._paginatedRows, this._data);
                this._filteredTable.initialize();
            }
        }
        this.triggerChangeEvents(oldState, true);
        this._updating = false;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$isValidAfterFiltering(row, filters, filterLength) {
        // <param name="row">Data row.</param>
        // <param name="filters" optional="true" mayBeNull="true">Data filters.</param>
        // <param name="filterLength" optional="true" mayBeNull="true">Number of filters.</param>
        // <returns type="Boolean">True if row passes the filters, false if not.</returns>
        for (var j = 0; j < filterLength; j++) {
            if (!filters[j].filter(row)) {
                return false;
            }
        }
        return true;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$triggerChangeEvents(oldState, lengthCanChange) {
        // <param name="oldState">Last page statistics.</param>
        // <param name="lengthCanChange">True to trigger changes to page and row counts, false to ignore changes.</param>
        var count;
        var pageIndex = this.get_pageIndex();
        if (lengthCanChange) {
            if (this.get_pageCount() !== oldState.pageCount) {
                this.raisePropertyChanged('pageCount');
            }
            if (this.get_length() !== oldState.length) {
                this.raisePropertyChanged('length');
            }
            count = this.get_pageCount();
            if (pageIndex >= count) {
                pageIndex = (count > 0 ? count - 1 : 0);
                this.set_pageIndex(pageIndex);
            }
        }
        else {
            count = oldState.pageCount;
        }
        if (pageIndex !== oldState.pageIndex) {
            this.raisePropertyChanged('pageIndex');
        }
        if ((pageIndex < count - 1) !== oldState.hasNextPage) {
            this.raisePropertyChanged('hasNextPage');
        }
        if ((pageIndex > 0) !== oldState.hasPreviousPage) {
            this.raisePropertyChanged('hasPreviousPage');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$prepareChange() {
        // <returns>View page statistics.</returns>
        return {pageCount: this.get_pageCount(),
                pageIndex: this.get_pageIndex(),
                length: this.get_length(),
                hasNextPage: this.get_hasNextPage(),
                hasPreviousPage: this.get_hasPreviousPage()};
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.prototype = {
    // Data fields
    _data: null,
    _filteredTable: null,
    _filteredRows: null,
    _paginatedRows: null,

    _pageSize: 0,
    _pageIndex: 0,

    _sorted: false,
    _sortColumn: '',
    _sortDirection: Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending,

    _filters: null,

    _dataChangedDelegate: null,
    _compareRowsDelegate: null,

    _updating: false,

    get_data: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_data,
    set_data: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_data,

    get_filteredData: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_filteredData,

    get_filters: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_filters,

    get_hasNextPage: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_hasNextPage,
    get_hasPreviousPage: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_hasPreviousPage,

    get_length: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_length,

    get_pageCount: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_pageCount,

    get_pageIndex: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_pageIndex,
    // TODO: If one needs more than one parameter, one doesn't want a property.
    // The xml doc comments may ended up tripping the validation code in this case anyway, so if you put back the triple slash be sure it works.
    set_pageIndex: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_pageIndex,

    get_pageSize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_pageSize,
    set_pageSize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_pageSize,

    get_sortColumn: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_sortColumn,
    set_sortColumn: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_sortColumn,

    get_sortDirection: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$get_sortDirection,
    set_sortDirection: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$set_sortDirection,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$dispose,

    getItem: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$getItem,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$initialize,

    sort: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$sort,

    _raiseFilterChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$_raiseFilterChanged,

    compareRows: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$compareRows,

    // TODO: handle row change modifications and compare to immediate neighbours to check if re-sorting is necessary.
    onDataChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$onDataChanged,

    ensureFilteredData: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$ensureFilteredData,

    isValidAfterFiltering: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$isValidAfterFiltering,

    triggerChangeEvents: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$triggerChangeEvents,

    prepareChange: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataView$prepareChange
}

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.descriptor = {
    properties: [ { name: 'data', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable },
                  { name: 'filteredData', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable, readOnly: true },
                  { name: 'filters', type: Array, readOnly: true },
                  { name: 'hasNextPage', type: Boolean, readOnly: true},
                  { name: 'hasPreviousPage', type: Boolean, readOnly: true },
                  { name: 'length', type: Number, readOnly: true },
                  { name: 'pageCount', type: Number, readOnly: true },
                  { name: 'pageIndex', type: Number },
                  { name: 'pageSize', type: Number },
                  { name: 'sortColumn', type: String },
                  { name: 'sortDirection', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection } ],
    methods: [ { name: 'sort', params: [ {name: 'sortColumn', type: String},
                                         {name: 'sortDirection', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection} ] } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView', Sys.Component);

Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter.initializeBase(this);
}



    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$filter(value) {
        // <param name="value"></param>
        // <returns type="Boolean"></returns>
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$get_dataContext() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Data context associated with this filter.</value>
        var dc = Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter.callBaseMethod(this, 'get_dataContext');
        if (!dc) {
            if (this.owner) {
                dc = this.owner.get_dataContext();
            }
        }
        return dc;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$dispose() {
        this.owner = null;
        Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$raisePropertyChanged(propertyName) {
        // <param name="propertyName" type="String">Property name.</param>
        Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter.callBaseMethod(this, 'raisePropertyChanged', [propertyName]);
        if (this.owner) {
            this.owner._raiseFilterChanged(this);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$setOwner(owner) {
        // <param name="owner"></param>
        this.owner = owner;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter.prototype = {

    // bool filter(object value)
    filter: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$filter,
    
    get_dataContext: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$get_dataContext,
    
    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$dispose,
    
    raisePropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$raisePropertyChanged,
    
    setOwner: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataFilter$setOwner
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter', Sys.Component);
Inflectra.SpiraTest.Web.AjaxExtensions.Data.PropertyFilter = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Data.PropertyFilter.initializeBase(this);
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$get_property() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String" mayBeNull="true">Property name.</value>
        return this._property;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$set_property(name) {
        this._property = name;
        this.raisePropertyChanged('property');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$get_value() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Property value.</value>
        return this._value;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$set_value(value) {
        this._value = value;
        this.raisePropertyChanged('value');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$filter(item) {
        // <param name="item" />
        // <returns />
         return Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(item, this._property) === this._value;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.PropertyFilter.prototype = {
    _property: null,
    _value: null,
    
    get_property: Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$get_property,
    set_property: Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$set_property,
    
    get_value: Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$get_value,
    set_value: Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$set_value,
    
    filter: Inflectra$SpiraTest$Web$AjaxExtensions$Data$PropertyFilter$filter
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.PropertyFilter.descriptor = {
    properties: [ { name: 'property', type: String },
                  { name: 'value' } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.PropertyFilter.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.PropertyFilter', Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataFilter);
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource() {
    // <summary>
    //   DataSource is a component that communicates with a server-side
    //   data service to expose data to client-side components such as
    //   ListView.
    // </summary>
    Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource.initializeBase(this);
    this._parameters = {};
}












    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$add_dataAvailable(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        this.get_events().addHandler("dataAvailable", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$remove_dataAvailable(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("dataAvailable", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$_onDataAvailable() {
        var handler = this.get_events().getHandler("dataAvailable");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_autoLoad() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>True to auto load, false for manual load.</value>
        return this._autoLoad;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_autoLoad(value) {
        this._autoLoad = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_data() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value />
        return this._data;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_data(data) {
        // <param name="data" />
        if(data && Object.getTypeName(data) === 'Object') {
            data = Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.parseFromJson(data);
        }
        Sys.Debug.assert(!data || Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.isInstanceOfType(data) || (data instanceof Array), "data must be of type Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable or Array.");
        var oldIsDirtyAndReady = this.get_isDirtyAndReady();
        var oldIsReady = this.get_isReady();
        var oldRowCount = this.get_rowCount();
        if (this._data && this._dataChangedDelegate) {
            this._data.remove_propertyChanged(this._dataChangedDelegate);
        }
        if (data instanceof Array) {
            data = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable([], data);
        }
        this._data = data;
        if (this._data) {
            if (!this._dataChangedDelegate) {
                this._dataChangedDelegate = Function.createDelegate(this, this.onDataPropertyChanged);
            }
            this._data.add_propertyChanged(this._dataChangedDelegate);
        }
        this.raisePropertyChanged('data');
        if (oldIsDirtyAndReady !== this.get_isDirtyAndReady()) {
            this.raisePropertyChanged('isDirtyAndReady');
        }
        if (oldIsReady !== this.get_isReady()) {
            this.raisePropertyChanged('isReady');
        }
        if (oldRowCount !== this.get_rowCount()) {
            this.raisePropertyChanged('rowCount');
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_initialData() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value />
        return this._initialData;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_initialData(value) {
        if (!this._data) {
            if (this.get_isInitialized()) {
                var data = null;
                if (value && (value.length)) {
                    data = Sys.Serialization.JavaScriptSerializer.deserialize(value);
                }
                this.set_data(data);
            }
            else {
                this._initialData = value;
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_isDirtyAndReady() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean" />
        return this._isReady && this._data && this._data.get_isDirty();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_isReady() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean" />
        return this._isReady;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$_set_isReady(value) {
        if (this._isReady !== value) {
            var oldDirtyAndReady = this.get_isDirtyAndReady();
            this._isReady = value;
            this.raisePropertyChanged("isReady");
            if (this.get_isDirtyAndReady() !== oldDirtyAndReady) {
                this.raisePropertyChanged("isDirtyAndReady");
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_loadMethod() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Load method.</value>
        return this._loadMethod;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_loadMethod(value) {
        this._loadMethod = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_parameters() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Service input parameters.</value>
        return this._parameters;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_serviceURL() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String" mayBeNull="true">Service URL.</value>
        return this._serviceURL;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_serviceURL(url) {
        this._serviceURL = url;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_serviceType() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType">Service type.</value>
        return this._serviceType;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_serviceType(value) {
        this._serviceType = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_rowCount() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Number">Row count.</value>
        if (this._data) {
            return this._data.get_length();
        }
        return 0;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$dispose() {
        if (this._data) {
            this._data.dispose();
        }
        this._data = null;

        Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource.callBaseMethod(this, 'initialize');
        if (this._autoLoad || this._initialData) {
            this.load();
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$onDataPropertyChanged(sender, args) {
        // <param name="sender">Event sender.</param>
        // <param name="args" type="Sys.EventArgs">Event arguments.</param>
        switch(args.get_propertyName()) {
            case "isDirty":
                this.raisePropertyChanged("isDirtyAndReady");
                break;
            case "length":
                this.raisePropertyChanged("rowCount");
                break;
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$onRequestComplete(response, eventArgs) {
        // <param name="response">WebRequest response.</param>
        // <param name="eventArgs" type="Sys.EventArgs">Event arguments.</param>
        this.onLoadComplete(response.get_object());
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$onLoadComplete(rawData, userContext, methodName) {
        // <param name="rawData" />
        // <param name="userContext" optional="true" mayBeNull="true" />
        // <param name="methodName" optional="true" mayBeNull="true" />
        var oldDirtyAndReady = this.get_isDirtyAndReady();
        this.set_data(eval(rawData));
        this._isReady = true;
        this.raisePropertyChanged("isReady");
        if (this.get_isDirtyAndReady() !== oldDirtyAndReady) {
            this.raisePropertyChanged("isDirtyAndReady");
        }
        this._onDataAvailable();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$ready() {
        this._set_isReady(true);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$load() {
        // <summary>
        //   Load queries the server data service for data if not initial data is available.
        //   This method works asynchronously, which means that it returns immediately even
        //   though the data will only be available when the server returns it. When that happens,
        //   the DataSource fires the dataAvailable event.
        // </summary>
        if (this._initialData) {
            this.set_data(Sys.Serialization.JavaScriptSerializer.deserialize(this._initialData));
            this._initialData = null;
            return;
        }
        this._set_isReady(false);
        if (this._serviceType === Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType.DataService) {
            var method = "GetData";
            var params = {parameters: this._parameters, loadMethod: this._loadMethod};
            var onComplete = Function.createDelegate(this, this.onLoadComplete);
            var onError = Function.createDelegate(this, this.ready); // REVIEW: ready handled timeout in CTP; should this callback handler error better?

            this._request = Sys.Net.WebServiceProxy.invoke(this._serviceURL, method, /*useGet*/false, params, onComplete, onError, /*userContext*/this, this._timeout);
        }
        else {
            var onComplete = Function.createDelegate(this, this.onRequestComplete);
            var onErrorOrTimeout = Function.createDelegate(this, this.ready); // REVIEW: ready handled timeout in CTP; should this callback handler error better?
            var url = Sys.Net.WebRequest._createUrl(this._serviceURL, this._parameters);
            var request = new Sys.Net.WebRequest();
            request.set_url(url);
            request.add_completed(function(response, eventArgs) {
                if (response.get_responseAvailable()) {
                    var statusCode = response.get_statusCode();
                    if (statusCode >= 200 || statusCode < 300) {
                        onComplete(response, eventArgs);
                    }
                    else {
                        onErrorOrTimeout();
                    }
                }
            });
            request.invoke();
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$save() {
        if (this._data && this._data.get_isDirty()) {
            var changes = this._data.getChanges();
            this._set_isReady(false);
            if (this._serviceType === Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType.DataService) {
                var method = "SaveData";
                var params = {changeList: changes, parameters: this._parameters, loadMethod: this._loadMethod};
                var onComplete = Function.createDelegate(this, this.onLoadComplete);
                var onError = Function.createDelegate(this, this.ready); // REVIEW: ready handled timeout in CTP; should this callback handler error better?
                this._request = Sys.Net.WebServiceProxy.invoke(this._serviceURL, method, /*useGet*/false, params, onComplete, onError, /*userContext*/this, this._timeout);
            }
            else {
                throw Error.createError("Save is not supported in Handler mode.");
            }
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource.prototype = {
    _data: null,
    _initialData: null,
    _autoLoad: false,
    _serviceURL: "",
    _loadMethod: "",
    _serviceType: Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType.DataService,
    _isReady: true,
    _dataChangedDelegate: null,
    _request: null,
    _timeout: 0,

    add_dataAvailable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$add_dataAvailable,

    remove_dataAvailable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$remove_dataAvailable,

    _onDataAvailable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$_onDataAvailable,

    get_autoLoad: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_autoLoad,
    set_autoLoad: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_autoLoad,

    get_data: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_data,
    set_data: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_data,

    get_initialData: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_initialData,
    set_initialData: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_initialData,

    get_isDirtyAndReady: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_isDirtyAndReady,

    get_isReady: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_isReady,
    _set_isReady: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$_set_isReady,

    get_loadMethod: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_loadMethod,
    set_loadMethod: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_loadMethod,

    get_parameters: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_parameters,

    get_serviceURL: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_serviceURL,
    set_serviceURL: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_serviceURL,

    get_serviceType: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_serviceType,
    set_serviceType: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$set_serviceType,

    get_rowCount: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$get_rowCount,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$dispose,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$initialize,

    onDataPropertyChanged: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$onDataPropertyChanged,

    onRequestComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$onRequestComplete,

    onLoadComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$onLoadComplete,

    ready: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$ready,

    load: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$load,

    save: Inflectra$SpiraTest$Web$AjaxExtensions$Data$DataSource$save
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource.descriptor = {
    properties: [ { name: 'data', type: Object },
                  { name: 'autoLoad', type: Boolean },
                  { name: 'initialData', type: String },
                  { name: 'isDirtyAndReady', type: Boolean, readOnly: true },
                  { name: 'isReady', type: Boolean, readOnly: true },
                  { name: 'loadMethod', type: String },
                  { name: 'rowCount', type: Number, readOnly: true },
                  { name: 'serviceURL', type: String },
                  { name: 'parameters', type: Object, readOnly: true },
                  { name: 'serviceType', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.ServiceType } ],
    methods: [ { name: 'load' },
               { name: 'save' } ],
    events: [ { name: 'dataAvailable', readOnly: true } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataSource', Sys.Component);

Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource = function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource.initializeBase(this);
}












    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$add_documentAvailable(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to add for the event.</param>
        this.get_events().addHandler("documentAvailable", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$remove_documentAvailable(handler) {
    var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
    if (e) throw e;

        // <param name="handler" type="Function">The handler to remove from the event.</param>
        this.get_events().removeHandler("documentAvailable", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_onDocumentAvailable() {
        var handler = this.get_events().getHandler("documentAvailable");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_autoLoad() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True for auto loading, false otherwise.</value>
        return this._autoLoad;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_autoLoad(value) {
        // <param name="value" type="Boolean">True for auto loading, false otherwise.</param>
        this._autoLoad = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_document() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Document object.</value>
        return this._document;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_data() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Data object.</value>
        return this._data;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_initialDocument() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Initial document string.</value>
        return this._initialDocument;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_initialDocument(value) {
        // <param name="value" type="String">Initial document string.</param>
        if (!this._document) {
            var document;
            if (Sys.Net.XMLDOM) {
                document = new Sys.Net.XMLDOM(value.trim());
            }
            else {
                // compat with whidbey atlas 1.0
                document = new XMLDOM(value.trim());
            }
            if (this.get_isInitialized()) {
                this._setDocument(document);
            }
            else {
                this._initialDocument = document;
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_isReady() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Boolean">True if document is loaded and ready, false otherwise.</value>
        return this._isReady;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_parameters() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Web service input parameters.</value>
        if (this._parameters === null) {
            this._parameters = {};
        }
        return this._parameters;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_serviceURL() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Web service url.</value>
        return this._serviceURL;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_serviceURL(value) {
        // <param name="value" type="String">Web service url.</param>
        this._serviceURL = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_xpath() {
    if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">XML node path.</value>
        return this._xpath;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_xpath(value) {
        // <param name="value" type="String">XML node path.</param>
        if (this._xpath !== value) {
            this._xpath = value;

            if (this._document) {
                this._updateData();
            }
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$dispose() {
        this._document = null;
        this._initialDocument = null;
        this._data = null;

        Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource.callBaseMethod(this, 'initialize');

        if (this._autoLoad) {
            this.load();
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$load() {
        if (this._initialDocument) {
            var document = this._initialDocument;
            this._initialDocument = null;

            this._setDocument(document);
            this._updateReady(true);
        }
        else {
            this._invokeService();
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_invokeService() {
        var onComplete = Function.createDelegate(this, this._serviceCompleted);
        var onErrorOrTimeout = Function.createDelegate(this, this._serviceTimeout);
        var url = Sys.Net.WebRequest._createUrl(this._serviceURL, this.get_parameters());
        var request = new Sys.Net.WebRequest();
        request.set_url(url);
        request.add_completed(function(response, eventArgs) {
            if (response.get_responseAvailable()) {
                var statusCode = response.get_statusCode();
                if (statusCode >= 200 || statusCode < 300) {
                    onComplete(response, eventArgs);
                }
                else {
                    onErrorOrTimeout();
                }
            }
        });
        request.invoke();

        this._updateReady(false);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_serviceCompleted(sender, eventArgs) {
        // <param name="sender">Event sender.</param>
        // <param name="eventArgs" type="Sys.EventArgs">Event arguments.</param>
        if (sender.get_statusCode() === 200) {
            this._setDocument(sender.get_xml());
        }

        this._updateReady(true);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_serviceTimeout(sender, eventArgs) {
        // <param name="sender">Event sender.</param>
        // <param name="eventArgs" type="Sys.EventArgs">Event arguments.</param>
        this._updateReady(true);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_setDocument(document) {
        // <param name="document">Document object.</param>
        this._document = document;
        this._updateData();
        this.raisePropertyChanged('document');
        this._onDocumentAvailable();
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_updateData() {
        var xpath = this._xpath;
        if (!xpath || !xpath.length) {
            // TODO: star-slash-star is breaking AtlasCop;
            xpath = '*/*'; //*/';
        }

        var nodes = this._document.selectNodes(xpath);

        // We need to convert the node list into an array (in IE, the returned
        // node list looks like an array, but isn't a script Array object, and
        // doesn't have our extensions (IArray, other methods etc.)
        var data = [];
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];

            if (!node || (node.nodeType !== 1)) {
                continue;
            }
            Array.add(data, node);
        }

        this._data = data;
        this.raisePropertyChanged('data');
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_updateReady(ready) {
        // <param name="ready" type="Boolean">True if ready, false if not.</param>
        this._isReady = ready;
        this.raisePropertyChanged('isReady');
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource.prototype = {
    _document: null,
    _initialDocument: null,

    _data: null,
    _xpath: '',

    _serviceURL: null,
    _parameters: null,
    _isReady: false,
    _autoLoad: false,

    add_documentAvailable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$add_documentAvailable,

    remove_documentAvailable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$remove_documentAvailable,

    _onDocumentAvailable: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_onDocumentAvailable,

    get_autoLoad: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_autoLoad,
    set_autoLoad: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_autoLoad,

    get_document: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_document,

    get_data: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_data,

    get_initialDocument: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_initialDocument,
    set_initialDocument: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_initialDocument,

    get_isReady: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_isReady,

    get_parameters: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_parameters,

    get_serviceURL: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_serviceURL,
    set_serviceURL: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_serviceURL,

    get_xpath: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$get_xpath,
    set_xpath: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$set_xpath,

    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$dispose,

    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$initialize,

    load: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$load,

    _invokeService: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_invokeService,

    _serviceCompleted: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_serviceCompleted,

    _serviceTimeout: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_serviceTimeout,

    _setDocument: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_setDocument,

    _updateData: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_updateData,

    _updateReady: Inflectra$SpiraTest$Web$AjaxExtensions$Data$XMLDataSource$_updateReady
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource.descriptor = {
    properties: [ { name: 'autoLoad', type: Boolean },
                  { name: 'data', type: Object, readOnly: true },
                  { name: 'document', type: Object, readOnly: true },
                  { name: 'initialDocument', type: String },
                  { name: 'isReady', type: Boolean, readOnly: true },
                  { name: 'parameters', type: Object, readOnly: true },
                  { name: 'serviceURL', type: String },
                  { name: 'xpath', type: String } ],
    events: [ { name: 'documentAvailable', readOnly: true } ],
    methods: [ { name: 'load' } ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Data.XMLDataSource', Sys.Component);


Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls');

Inflectra.SpiraTest.Web.ServerControls.DialogResult = function Inflectra$SpiraTest$Web$ServerControls$DialogResult() {
    throw Error.invalidOperation();
}



Inflectra.SpiraTest.Web.ServerControls.DialogResult.prototype = {
    OK: 0,
    Cancel: 1
}
Inflectra.SpiraTest.Web.ServerControls.DialogResult.registerEnum('Inflectra.SpiraTest.Web.ServerControls.DialogResult');
Inflectra.SpiraTest.Web.ServerControls.Color = function Inflectra$SpiraTest$Web$ServerControls$Color(r, g, b) {
    /// <param name="r" type="Number"></param>
    /// <param name="g" type="Number"></param>
    /// <param name="b" type="Number"></param>
    var e = Function._validateParams(arguments, [
        {name: "r", type: Number},
        {name: "g", type: Number},
        {name: "b", type: Number}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.Color.initializeBase(this);
    
    this._r = r;
    this._g = g;
    this._b = b;
}

    function Inflectra$SpiraTest$Web$ServerControls$Color$get_blue() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._b;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Color$get_green() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._g;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Color$get_red() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._r;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Color$toString() {
        /// <returns type="String"></returns>
        if (arguments.length !== 0) throw Error.parameterCount();
        var red = this._r.toString(16);
        if (this._r < 16) {
            red = '0' + red;
        }
        var green = this._g.toString(16);
        if (this._g < 16) {
            green = '0' + green;
        }
        var blue = this._b.toString(16);
        if (this._b < 16) {
            blue = '0' + blue;
        }
        return "#" + red + green + blue;
    }
Inflectra.SpiraTest.Web.ServerControls.Color.prototype = {
    get_blue: Inflectra$SpiraTest$Web$ServerControls$Color$get_blue,
    
    get_green: Inflectra$SpiraTest$Web$ServerControls$Color$get_green,
    
    get_red: Inflectra$SpiraTest$Web$ServerControls$Color$get_red,
    
    toString: Inflectra$SpiraTest$Web$ServerControls$Color$toString
}
Inflectra.SpiraTest.Web.ServerControls.Color.registerClass('Inflectra.SpiraTest.Web.ServerControls.Color');

Inflectra.SpiraTest.Web.ServerControls.Color.parse = function Inflectra$SpiraTest$Web$ServerControls$Color$parse(value) {
    /// <param name="value" type="String"></param>
    /// <returns type="Inflectra.SpiraTest.Web.ServerControls.Color"></returns>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    if (value && (value.length === 7) && value.startsWith("#")) {
        var red = parseInt('0x' + value.substr(1, 2));
        var green = parseInt('0x' + value.substr(3, 2));
        var blue = parseInt('0x' + value.substr(5, 2));
        
        return new Inflectra.SpiraTest.Web.ServerControls.Color(red, green, blue);
    }
    return null;
}
Inflectra.SpiraTest.Web.AjaxExtensions.Attributes.defineAttribute('ValueProperty');
Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs = function Inflectra$SpiraTest$Web$ServerControls$CommandEventArgs(commandName, argument) {
    /// <param name="commandName" type="String" mayBeNull="true"></param>
    /// <param name="argument" type="String" mayBeNull="true" optional="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "commandName", type: String, mayBeNull: true},
        {name: "argument", type: String, mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs.initializeBase(this);
    
    this._commandName = commandName;
    this._argument = argument;
}

    function Inflectra$SpiraTest$Web$ServerControls$CommandEventArgs$get_argument() {
        /// <value tyep="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._argument;
    }
    function Inflectra$SpiraTest$Web$ServerControls$CommandEventArgs$get_commandName() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._commandName;
    }
Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs.prototype = {
    get_argument: Inflectra$SpiraTest$Web$ServerControls$CommandEventArgs$get_argument,    
    get_commandName: Inflectra$SpiraTest$Web$ServerControls$CommandEventArgs$get_commandName
}
Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs.descriptor = {
    properties: [   {name: 'argument', type: String, readOnly: true},
                    {name: 'commandName', type: String, readOnly: true} ]
}
Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs.registerClass('Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs', Sys.EventArgs);

Inflectra.SpiraTest.Web.ServerControls.IValidationTarget = function Inflectra$SpiraTest$Web$ServerControls$IValidationTarget() {
}





    function Inflectra$SpiraTest$Web$ServerControls$IValidationTarget$get_isInvalid() {
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IValidationTarget$get_validationMessage() {
        if (arguments.length !== 0) throw Error.parameterCount();
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$ServerControls$IValidationTarget$validate() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.ServerControls.IValidationTarget.prototype = {

    validated: null,
    
    get_isInvalid: Inflectra$SpiraTest$Web$ServerControls$IValidationTarget$get_isInvalid,
    
    get_validationMessage: Inflectra$SpiraTest$Web$ServerControls$IValidationTarget$get_validationMessage,
    
    validate: Inflectra$SpiraTest$Web$ServerControls$IValidationTarget$validate
}

Inflectra.SpiraTest.Web.ServerControls.IValidationTarget.registerInterface('Inflectra.SpiraTest.Web.ServerControls.IValidationTarget');

Inflectra.SpiraTest.Web.ServerControls.Validator = function Inflectra$SpiraTest$Web$ServerControls$Validator() {
    Inflectra.SpiraTest.Web.ServerControls.Validator.initializeBase(this);
}






    function Inflectra$SpiraTest$Web$ServerControls$Validator$get_dataContext() {
        if (arguments.length !== 0) throw Error.parameterCount();
        var dc = Sys.Component.callBaseMethod(this, 'get_dataContext');
        if (!dc) {
            if (this.control) {
                dc = this.control.get_dataContext();
            }
        }
        return dc;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$get_errorMessage() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._errorMessage;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$set_errorMessage(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String}]);
        if (e) throw e;

        this._errorMessage = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$get_isInvalid() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._isInvalid;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$dispose() {
        this.control = null;
        Inflectra.SpiraTest.Web.ServerControls.Validator.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$performValidation(value) {
        /// <param name="value" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "value", mayBeNull: true}
        ]);
        if (e) throw e;

        // TODO: Change notification
        this._isInvalid = !this.validate(value);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$setOwner(control) {
        /// <param name="control" type="Sys.UI.Control"></param>
        var e = Function._validateParams(arguments, [
            {name: "control", type: Sys.UI.Control}
        ]);
        if (e) throw e;

        this.control = control;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Validator$validate() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.ServerControls.Validator.prototype = {

    _errorMessage: null,
    _isInvalid: false,
    
    get_dataContext: Inflectra$SpiraTest$Web$ServerControls$Validator$get_dataContext,

    get_errorMessage: Inflectra$SpiraTest$Web$ServerControls$Validator$get_errorMessage,
    
    set_errorMessage: Inflectra$SpiraTest$Web$ServerControls$Validator$set_errorMessage,
    
    get_isInvalid: Inflectra$SpiraTest$Web$ServerControls$Validator$get_isInvalid,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$Validator$dispose,

    performValidation: Inflectra$SpiraTest$Web$ServerControls$Validator$performValidation,
    
    setOwner: Inflectra$SpiraTest$Web$ServerControls$Validator$setOwner,

    validate: Inflectra$SpiraTest$Web$ServerControls$Validator$validate
}
    
Inflectra.SpiraTest.Web.ServerControls.Validator.descriptor = {
    properties: [ { name: 'errorMessage', type: String },
                  { name: 'isInvalid', type: Boolean } ]
}

Inflectra.SpiraTest.Web.ServerControls.Validator.registerClass('Inflectra.SpiraTest.Web.ServerControls.Validator', Sys.Component);
Inflectra.SpiraTest.Web.ServerControls.ValidationGroup = function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup() {
    Inflectra.SpiraTest.Web.ServerControls.ValidationGroup.initializeBase(this);
    this._associatedControls = [];
}





    function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$get_associatedControls() {
        /// <value type="Array"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._associatedControls;
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$get_isValid() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._validated) {
            this.validate();
            this._validated = true;
        }
        return this._valid;
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$dispose() {
        if (this._associatedControls) {
            for (var i = 0; i < this._associatedControls.length; i++) {
                this._associatedControls[i].get_component().remove_validated(this._validatedHandler);
                this._associatedControls[i].dispose();
            }
            this._validatedHandler = null;
            this._associatedControls = null;
        }
        
        Inflectra.SpiraTest.Web.ServerControls.ValidationGroup.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.ValidationGroup.callBaseMethod(this, 'initialize');

        this._validatedHandler = Function.createDelegate(this, this._onControlValidated);
        for (var i = 0; i < this._associatedControls.length; i++) {
            this._associatedControls[i].get_component().add_validated(this._validatedHandler);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$validate() {
        var valid = true;
        
        if (this._associatedControls && this._associatedControls.length) {
            for (var i = 0; i < this._associatedControls.length; i++) {
                if (this._associatedControls[i].get_component().get_isInvalid()) {
                    valid = false;
                    break;
                }
            }
        }
        
        this._valid = valid;
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$_onControlValidated(sender, eventArgs) {
        /// <param name="sender"></param>
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "sender"},
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        var isValid = this._valid;
        this.validate();
        
        if (this._valid !== isValid) {
            this.raisePropertyChanged('isValid');
        }
    }
Inflectra.SpiraTest.Web.ServerControls.ValidationGroup.prototype = {    
    _valid: true,
    _validated: false,
    _validatedHandler: null,
    
    get_associatedControls: Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$get_associatedControls,
    
    get_isValid: Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$get_isValid,

    dispose: Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$dispose,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$initialize,

    validate: Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$validate,
    
    _onControlValidated: Inflectra$SpiraTest$Web$ServerControls$ValidationGroup$_onControlValidated
}
Inflectra.SpiraTest.Web.ServerControls.ValidationGroup.descriptor = {
    properties: [   {name: 'isValid', type: Boolean, readOnly: true},
                    {name: 'associatedControls', type: Array, readOnly: true} ]
}
Inflectra.SpiraTest.Web.ServerControls.ValidationGroup.registerClass('Inflectra.SpiraTest.Web.ServerControls.ValidationGroup', Sys.Component);

Inflectra.SpiraTest.Web.ServerControls.InputControl = function Inflectra$SpiraTest$Web$ServerControls$InputControl(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.InputControl.initializeBase(this, [associatedElement]);
}









    function Inflectra$SpiraTest$Web$ServerControls$InputControl$add_validated(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("validated", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$remove_validated(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        if(this._disposed) return;
        this.get_events().removeHandler("validated", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$_onValidated() {
        var handler = this.get_events().getHandler("validated");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$get_isInvalid() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._validated) {
            this.validate(false);
            this._validated = true;
        }
        return this._invalid;
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$get_validationMessage() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_isInvalid() ? this._validationMessage : '';
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$get_validators() {
        /// <value type="Array"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        if (!this._validators) {
            this._validators = Sys.Component.createCollection(this);
        }
        return this._validators;
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$dispose() {
        if (this._validators) {
            this._validators.dispose();
            this._validators = null;
        }
        this._disposed = true;
        Inflectra.SpiraTest.Web.ServerControls.InputControl.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$raisePropertyChanged(propertyName) {
        /// <param name="propertyName" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyName", type: String}
        ]);
        if (e) throw e;

        if (this._validators && this._validators.length) {
            if (!this._valuePropertyName) {
                this._valuePropertyName = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getAttribute(this, Inflectra.SpiraTest.Web.AjaxExtensions.Attributes.ValueProperty);
            }

            if (this._valuePropertyName === propertyName) {
                if (!this.validate(true)) {
                    return;
                }
            }
        }

        Inflectra.SpiraTest.Web.ServerControls.InputControl.callBaseMethod(this, 'raisePropertyChanged', [propertyName]);
    }

    function Inflectra$SpiraTest$Web$ServerControls$InputControl$validate(raiseEvent) {
        /// <param name="raiseEvent" type="Boolean"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "raiseEvent", type: Boolean}
        ]);
        if (e) throw e;

        if (!this._validators || !this._validators.length) {
            return true;
        }

        if (!this._valuePropertyName) {
            this._valuePropertyName = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getAttribute(this, Inflectra.SpiraTest.Web.AjaxExtensions.Attributes.ValueProperty);
        }
        var value = Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(this, this._valuePropertyName);

        var invalidValidator = null;
        for (var i = 0; i < this._validators.length; i++) {
            var validator = this._validators[i];
            
            validator.performValidation(value);
            if (validator.get_isInvalid()) {
                invalidValidator = validator;
                
                this._validationMessage = invalidValidator.get_errorMessage();
                this._invalid = true;
                break;
            }
        }
        if (!invalidValidator) {
            this._invalid = false;
        }

        if (raiseEvent) {
            this._onValidated();
        }
        return !this._invalid;
    }
Inflectra.SpiraTest.Web.ServerControls.InputControl.prototype = {

    _valuePropertyName: null,
    _validators: null,
    _invalid: false,
    _validated: false,
    _validationMessage: null,

    add_validated: Inflectra$SpiraTest$Web$ServerControls$InputControl$add_validated,
    
    remove_validated: Inflectra$SpiraTest$Web$ServerControls$InputControl$remove_validated,
    
    _onValidated: Inflectra$SpiraTest$Web$ServerControls$InputControl$_onValidated,
    
    get_isInvalid: Inflectra$SpiraTest$Web$ServerControls$InputControl$get_isInvalid,

    get_validationMessage: Inflectra$SpiraTest$Web$ServerControls$InputControl$get_validationMessage,

    get_validators: Inflectra$SpiraTest$Web$ServerControls$InputControl$get_validators,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$InputControl$dispose,

    raisePropertyChanged: Inflectra$SpiraTest$Web$ServerControls$InputControl$raisePropertyChanged,
    
    validate: Inflectra$SpiraTest$Web$ServerControls$InputControl$validate
}

Inflectra.SpiraTest.Web.ServerControls.InputControl.descriptor = {
    properties: [ { name: 'isInvalid', type: Boolean, readOnly: true },
                  { name: 'validationMessage', type: String, readOnly: true },
                  { name: 'validators', type: Array, readOnly: true } ]
}
    
Inflectra.SpiraTest.Web.ServerControls.InputControl.registerClass('Inflectra.SpiraTest.Web.ServerControls.InputControl', Sys.UI.Control, Inflectra.SpiraTest.Web.ServerControls.IValidationTarget);

Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle = function Inflectra$SpiraTest$Web$ServerControls$MessageBoxStyle() {
    throw Error.invalidOperation();
}



Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle.prototype = {
    OK: 0,
    OKCancel: 1
}
Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle.registerEnum('Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle');
Inflectra.SpiraTest.Web.ServerControls.Window = function Inflectra$SpiraTest$Web$ServerControls$Window() {
    throw Error.invalidOperation();
}
Inflectra.SpiraTest.Web.ServerControls.Window.messageBox = function Inflectra$SpiraTest$Web$ServerControls$Window$messageBox(text, style) {
    if (!style) {
        style = Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle.OK;
    }

    var result = Inflectra.SpiraTest.Web.ServerControls.DialogResult.OK;
    switch (style) {
        case Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle.OK:
            window.alert(text);
            break;
        case Inflectra.SpiraTest.Web.ServerControls.MessageBoxStyle.OKCancel:
            if (window.confirm(text) === false) {
                result = Inflectra.SpiraTest.Web.ServerControls.DialogResult.Cancel;
            }
            break;
    }

    return result;
}

Inflectra.SpiraTest.Web.ServerControls.Window.inputBox = function Inflectra$SpiraTest$Web$ServerControls$Window$inputBox(promptText, defaultValue) {
    if (!defaultValue) {
        defaultValue = '';
    }
    return window.prompt(promptText, defaultValue);
}

// consider: make this a component, so it can be used in xml script to cause alerts and confirms
Inflectra.SpiraTest.Web.ServerControls.ITemplate = function Inflectra$SpiraTest$Web$ServerControls$ITemplate() {
    throw Error.notImplemented();
}

    function Inflectra$SpiraTest$Web$ServerControls$ITemplate$createInstance() {
        throw Error.notImplemented();
    }

    function Inflectra$SpiraTest$Web$ServerControls$ITemplate$initialize() {
        throw Error.notImplemented();
    }
Inflectra.SpiraTest.Web.ServerControls.ITemplate.prototype = {
    createInstance: Inflectra$SpiraTest$Web$ServerControls$ITemplate$createInstance,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$ITemplate$initialize
}
Inflectra.SpiraTest.Web.ServerControls.ITemplate.registerInterface('Inflectra.SpiraTest.Web.ServerControls.ITemplate');

Inflectra.SpiraTest.Web.ServerControls.ITemplate.disposeInstance = function Inflectra$SpiraTest$Web$ServerControls$ITemplate$disposeInstance(container) {
    /// <param name="container"></param>
    var e = Function._validateParams(arguments, [
        {name: "container"}
    ]);
    if (e) throw e;

    if (container.markupContext) {
        container.markupContext.dispose();
        container.markupContext = null;
    }
}
Inflectra.SpiraTest.Web.ServerControls.TemplateInstance = function Inflectra$SpiraTest$Web$ServerControls$TemplateInstance() {
    this.instanceElement = null;
    this.callbackResult = null;
}

// Template.js
// Implementation of ITemplate that works against HTML and Script markup.
//

// ValueAdd: renamed from DeclarativeTemplate so it works with <Template> tags in xml script.

Inflectra.SpiraTest.Web.ServerControls.Template = function Inflectra$SpiraTest$Web$ServerControls$Template(layoutElement, scriptNode, parentMarkupContext) {
    // <param name="layoutElement">Layout element.</param>
    // <param name="scriptNode">Script element node.</param>
    // <param name="parentMarkupContext">Parent MarkupContext.</param>
    Inflectra.SpiraTest.Web.ServerControls.Template.initializeBase(this);

    this._layoutElement = layoutElement;
    this._scriptNode = scriptNode;
    this._parentMarkupContext = parentMarkupContext;
}

    function Inflectra$SpiraTest$Web$ServerControls$Template$createInstance(containerElement, dataContext, instanceElementCreatedCallback, callbackContext) {
        // <param name="containerElement">Template container dom element.</param>
        // <param name="dataContext" optional="true" mayBeNull="true">Data context.</param>
        // <param name="instanceElementCreatedCallback" type="Function" optional="true" mayBeNull="true">Post-creation callback.</param>
        // <param name="callbackContext" optional="true" mayBeNull="true">Callback context.</param>
        // <returns type="Inflectra.SpiraTest.Web.ServerControls.TemplateInstance">Template instance.</returns>
        var result = new Inflectra.SpiraTest.Web.ServerControls.TemplateInstance();
        result.instanceElement = this._layoutElement.cloneNode(true);

        var documentFragment = document.createDocumentFragment();
        documentFragment.appendChild(result.instanceElement);

        var markupContext = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext.createLocalContext(documentFragment, this._parentMarkupContext, dataContext);
        markupContext.open();
        Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.parseNodes(this._scriptNode.childNodes, markupContext);

        if (instanceElementCreatedCallback) {
            result.callbackResult = instanceElementCreatedCallback(result.instanceElement, markupContext, callbackContext);
        }

        result.instanceElement.markupContext = markupContext;
        containerElement.appendChild(result.instanceElement);
        markupContext.close();

        return result;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Template$dispose() {
        this._layoutElement = null;
        this._scriptNode = null;
        this._parentMarkupContext = null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Template$initialize() {
        // remove the template from the document
        if (this._layoutElement.parentNode) {
            this._layoutElement.parentNode.removeChild(this._layoutElement);
        }
    }
Inflectra.SpiraTest.Web.ServerControls.Template.prototype = {
    createInstance: Inflectra$SpiraTest$Web$ServerControls$Template$createInstance,

    dispose: Inflectra$SpiraTest$Web$ServerControls$Template$dispose,

    initialize: Inflectra$SpiraTest$Web$ServerControls$Template$initialize
}
Inflectra.SpiraTest.Web.ServerControls.Template.registerClass('Inflectra.SpiraTest.Web.ServerControls.Template', null, Inflectra.SpiraTest.Web.ServerControls.ITemplate, Sys.IDisposable);

Inflectra.SpiraTest.Web.ServerControls.Template.parseFromMarkup = function Inflectra$SpiraTest$Web$ServerControls$Template$parseFromMarkup(type, node, markupContext) {
    // <param name="type">Element node type.</param>
    // <param name="node">Element node.</param>
    // <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext">Associated markup context.</param>
    // <returns type="Inflectra.SpiraTest.Web.ServerControls.Template">Parsed template.</returns>
    var layoutElementAttribute = node.attributes.getNamedItem('layoutElement');
    Sys.Debug.assert(!!(layoutElementAttribute && layoutElementAttribute.nodeValue.length), 'Missing layoutElement attribute on template definition');

    var layoutElementID = layoutElementAttribute.nodeValue;
    var layoutElement = markupContext.findElement(layoutElementID);
    Sys.Debug.assert(!!layoutElement, String.format('Could not find the HTML element with ID "{0}" associated with the template', layoutElementID));

    return new Inflectra.SpiraTest.Web.ServerControls.Template(layoutElement, node, markupContext);
}

Inflectra.SpiraTest.Web.ServerControls.PositioningMode = function Inflectra$SpiraTest$Web$ServerControls$PositioningMode() {
    throw Error.invalidOperation();
}







Inflectra.SpiraTest.Web.ServerControls.PositioningMode.prototype = {
    Absolute: 0,
    Center: 1,
    BottomLeft: 2,
    BottomRight: 3,
    TopLeft: 4,
    TopRight: 5
}
Inflectra.SpiraTest.Web.ServerControls.PositioningMode.registerEnum('Inflectra.SpiraTest.Web.ServerControls.PositioningMode');

Inflectra.SpiraTest.Web.ServerControls.ClickBehavior = function Inflectra$SpiraTest$Web$ServerControls$ClickBehavior(element) {
    /// <param name="element" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "element", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.ClickBehavior.initializeBase(this, [element]);
}





    function Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$add_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('click', handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$remove_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('click', handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$dispose() {
        if (this._clickHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), 'click', this._clickHandler);
        }
        Inflectra.SpiraTest.Web.ServerControls.ClickBehavior.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.ClickBehavior.callBaseMethod(this, 'initialize');

        this._clickHandler = Function.createDelegate(this, this._onClick);
        Sys.UI.DomEvent.addHandler(this.get_element(), 'click', this._clickHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$_onClick() {
        var handler = this.get_events().getHandler('click');
        if(handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }
Inflectra.SpiraTest.Web.ServerControls.ClickBehavior.prototype = {
    
    _clickHandler: null,

    add_click: Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$add_click,
    
    remove_click: Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$remove_click,

    dispose: Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$dispose,

    initialize: Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$initialize,

    _onClick: Inflectra$SpiraTest$Web$ServerControls$ClickBehavior$_onClick
}
Inflectra.SpiraTest.Web.ServerControls.ClickBehavior.descriptor = {
    events: [ {name: 'click'} ]
}
Inflectra.SpiraTest.Web.ServerControls.ClickBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.ClickBehavior', Sys.UI.Behavior);

Inflectra.SpiraTest.Web.ServerControls.Label = function Inflectra$SpiraTest$Web$ServerControls$Label(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.Label.initializeBase(this, [associatedElement]);
}





    function Inflectra$SpiraTest$Web$ServerControls$Label$get_htmlEncode() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._htmlEncode;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Label$set_htmlEncode(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        this._htmlEncode = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Label$get_text() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        var element = this.get_element();
        if (this._htmlEncode) {
            return element.innerText;
        }
        else {
            return element.innerHTML;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Label$set_text(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

        if (!value) value = "";
        var element = this.get_element();
        if (this._htmlEncode) {
            if (element.innerText !== value) {
                element.innerText = value;
                this.raisePropertyChanged('text');
            }
        }
        else {
            if (element.innerHTML !== value) {
                element.innerHTML = value;
                this.raisePropertyChanged('text');
            }
        }
    }

Inflectra.SpiraTest.Web.ServerControls.Label.prototype = {

    _htmlEncode: false,

    get_htmlEncode: Inflectra$SpiraTest$Web$ServerControls$Label$get_htmlEncode,
    
    set_htmlEncode: Inflectra$SpiraTest$Web$ServerControls$Label$set_htmlEncode,

    get_text: Inflectra$SpiraTest$Web$ServerControls$Label$get_text,
    // note: text can be set to number.
    set_text: Inflectra$SpiraTest$Web$ServerControls$Label$set_text
    
}

Inflectra.SpiraTest.Web.ServerControls.Label.descriptor = {
    properties: [ { name: 'htmlEncode', type: Boolean },
                  { name: 'text', type: String } ]
}

Inflectra.SpiraTest.Web.ServerControls.Label.registerClass('Inflectra.SpiraTest.Web.ServerControls.Label', Sys.UI.Control);


Inflectra.SpiraTest.Web.ServerControls.Image = function Inflectra$SpiraTest$Web$ServerControls$Image(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.Image.initializeBase(this, [associatedElement]);
}



    function Inflectra$SpiraTest$Web$ServerControls$Image$get_alternateText() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().alt;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$set_alternateText(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        this.get_element().alt = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$get_height() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().height;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$set_height(value) {
        var e = Function._validateParams(arguments, [{name: "value"}]);
        if (e) throw e;

        this.get_element().height = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$get_imageURL() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().src;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$set_imageURL(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        this.get_element().src = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$get_width() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().width;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Image$set_width(value) {
        var e = Function._validateParams(arguments, [{name: "value"}]);
        if (e) throw e;

        this.get_element().width = value;
    }

Inflectra.SpiraTest.Web.ServerControls.Image.prototype = {
    
    get_alternateText: Inflectra$SpiraTest$Web$ServerControls$Image$get_alternateText,
    
    set_alternateText: Inflectra$SpiraTest$Web$ServerControls$Image$set_alternateText,
    
    get_height: Inflectra$SpiraTest$Web$ServerControls$Image$get_height,
    
    set_height: Inflectra$SpiraTest$Web$ServerControls$Image$set_height,
    
    get_imageURL: Inflectra$SpiraTest$Web$ServerControls$Image$get_imageURL,
    
    set_imageURL: Inflectra$SpiraTest$Web$ServerControls$Image$set_imageURL,
    
    get_width: Inflectra$SpiraTest$Web$ServerControls$Image$get_width,
    
    set_width: Inflectra$SpiraTest$Web$ServerControls$Image$set_width
    
}

Inflectra.SpiraTest.Web.ServerControls.Image.descriptor = {
    properties: [ { name: 'alternateText', type: String },
                  { name: 'height' },
                  { name: 'imageURL', type: String },
                  { name: 'width' } ]
}

Inflectra.SpiraTest.Web.ServerControls.Image.registerClass('Inflectra.SpiraTest.Web.ServerControls.Image', Sys.UI.Control);

if(Sys.Browser.agent === Sys.Browser.Safari) {
    Inflectra.SpiraTest.Web.ServerControls.Image_ = function Inflectra$SpiraTest$Web$ServerControls$Image_(element) { Inflectra.SpiraTest.Web.ServerControls.Image_.initializeBase(this,[element]); }
    Inflectra.SpiraTest.Web.ServerControls.Image_.registerClass('Inflectra.SpiraTest.Web.ServerControls.Image_', Inflectra.SpiraTest.Web.ServerControls.Image);
}

Inflectra.SpiraTest.Web.ServerControls.HyperLink = function Inflectra$SpiraTest$Web$ServerControls$HyperLink(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.HyperLink.initializeBase(this, [associatedElement]);
}






    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$get_navigateURL() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().href;
    }

    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$set_navigateURL(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        this.get_element().href = value? value : ""; // Binding example tries to set to null.
    }

    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.HyperLink.callBaseMethod(this, 'initialize');
        this._clickHandler = Function.createDelegate(this, this._onClick);
        Sys.UI.DomEvent.addHandler(this.get_element(), "click", this._clickHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$dispose() {
        if (this._clickHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "click", this._clickHandler);
        }
        Inflectra.SpiraTest.Web.ServerControls.HyperLink.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$add_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("click", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$remove_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("click", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$HyperLink$_onClick() {
        var handler = this.get_events().getHandler("click");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }
Inflectra.SpiraTest.Web.ServerControls.HyperLink.prototype = {
    
    _clickHandler: null,

    // TODO: navigateURL gets set to null when binding data not available.
    get_navigateURL: Inflectra$SpiraTest$Web$ServerControls$HyperLink$get_navigateURL,
    
    set_navigateURL: Inflectra$SpiraTest$Web$ServerControls$HyperLink$set_navigateURL,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$HyperLink$initialize,

    dispose: Inflectra$SpiraTest$Web$ServerControls$HyperLink$dispose,
    
    add_click: Inflectra$SpiraTest$Web$ServerControls$HyperLink$add_click,

    remove_click: Inflectra$SpiraTest$Web$ServerControls$HyperLink$remove_click,
    
    _onClick: Inflectra$SpiraTest$Web$ServerControls$HyperLink$_onClick
}

Inflectra.SpiraTest.Web.ServerControls.HyperLink.descriptor = {
    properties: [ { name: 'navigateURL', type: String } ],
    events: [ { name: 'click' } ]
}

Inflectra.SpiraTest.Web.ServerControls.HyperLink.registerClass('Inflectra.SpiraTest.Web.ServerControls.HyperLink', Inflectra.SpiraTest.Web.ServerControls.Label);

Inflectra.SpiraTest.Web.ServerControls.Button = function Inflectra$SpiraTest$Web$ServerControls$Button(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.Button.initializeBase(this, [associatedElement]);
}







    function Inflectra$SpiraTest$Web$ServerControls$Button$get_argument() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._arg;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$set_argument(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if (this._arg !== value) {
            this._arg = value;
            this.raisePropertyChanged('argument');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$get_command() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._command;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$set_command(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        if (this._command !== value) {
            this._command = value;
            this.raisePropertyChanged('command');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.Button.callBaseMethod(this, 'initialize');
        this._clickHandler = Function.createDelegate(this, this._onClick);
        Sys.UI.DomEvent.addHandler(this.get_element(), "click", this._clickHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$dispose() {
        if (this._clickHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "click", this._clickHandler);
        }
        Inflectra.SpiraTest.Web.ServerControls.Button.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$add_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("click", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$remove_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("click", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Button$_onClick() {
        var handler = this.get_events().getHandler("click");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }

        if (this._command) {
            this.raiseBubbleEvent(this, new Inflectra.SpiraTest.Web.ServerControls.CommandEventArgs(this._command, this._arg));
        }
    }
Inflectra.SpiraTest.Web.ServerControls.Button.prototype = {

    _command: null,
    _arg: null,
    _clickHandler: null,
    
    get_argument: Inflectra$SpiraTest$Web$ServerControls$Button$get_argument,
    
    set_argument: Inflectra$SpiraTest$Web$ServerControls$Button$set_argument,
    
    get_command: Inflectra$SpiraTest$Web$ServerControls$Button$get_command,
    
    set_command: Inflectra$SpiraTest$Web$ServerControls$Button$set_command,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$Button$initialize,

    dispose: Inflectra$SpiraTest$Web$ServerControls$Button$dispose,
    
    add_click: Inflectra$SpiraTest$Web$ServerControls$Button$add_click,

    remove_click: Inflectra$SpiraTest$Web$ServerControls$Button$remove_click,
    
    _onClick: Inflectra$SpiraTest$Web$ServerControls$Button$_onClick
}
    
Inflectra.SpiraTest.Web.ServerControls.Button.descriptor = {
    properties: [ { name: 'command', type: String },
                  { name: 'argument', type: String } ],
    events: [ { name: 'click' } ]
}

Inflectra.SpiraTest.Web.ServerControls.Button.registerClass('Inflectra.SpiraTest.Web.ServerControls.Button', Sys.UI.Control);

Inflectra.SpiraTest.Web.ServerControls.CheckBox = function Inflectra$SpiraTest$Web$ServerControls$CheckBox(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.CheckBox.initializeBase(this, [associatedElement]);
}





    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$get_checked() {
        /// <value mayBeNull="true" optional="false"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        // note: the type is not Boolean because there are scenarios where a string is bound to this property
        // and it is not easy to setup thte binding to convert the value. It makes sense to be loose.
        return !!(this.get_element().checked);
    }

    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$set_checked(value) {
        var e = Function._validateParams(arguments, [{name: "value", mayBeNull: true}]);
        if (e) throw e;

       value = !!value; // coerce to bool
        if (value !== this.get_checked()) {
            this.get_element().checked = value;
            this.raisePropertyChanged('checked');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.CheckBox.callBaseMethod(this, 'initialize');
        this._clickHandler = Function.createDelegate(this, this._onClick);
        Sys.UI.DomEvent.addHandler(this.get_element(), "click", this._clickHandler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$dispose() {
        if (this._clickHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "click", this._clickHandler);
        }
        Inflectra.SpiraTest.Web.ServerControls.CheckBox.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$add_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("click", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$remove_click(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("click", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$CheckBox$_onClick() {
        this.raisePropertyChanged('checked');
        var handler = this.get_events().getHandler("click");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }
Inflectra.SpiraTest.Web.ServerControls.CheckBox.prototype = {

    _clickHandler: null,
    
    get_checked: Inflectra$SpiraTest$Web$ServerControls$CheckBox$get_checked,
    
    set_checked: Inflectra$SpiraTest$Web$ServerControls$CheckBox$set_checked,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$CheckBox$initialize,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$CheckBox$dispose,
    
    add_click: Inflectra$SpiraTest$Web$ServerControls$CheckBox$add_click,

    remove_click: Inflectra$SpiraTest$Web$ServerControls$CheckBox$remove_click,
    
    _onClick: Inflectra$SpiraTest$Web$ServerControls$CheckBox$_onClick
}

Inflectra.SpiraTest.Web.ServerControls.CheckBox.descriptor = {
    properties: [ { name: 'checked' } ],
    events: [ { name: 'click' } ]
}

Inflectra.SpiraTest.Web.ServerControls.CheckBox.registerClass('Inflectra.SpiraTest.Web.ServerControls.CheckBox', Sys.UI.Control);

Inflectra.SpiraTest.Web.ServerControls.TextBox = function Inflectra$SpiraTest$Web$ServerControls$TextBox(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.TextBox.initializeBase(this, [associatedElement]);
}







    function Inflectra$SpiraTest$Web$ServerControls$TextBox$get_text() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$TextBox$set_text(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        var element = this.get_element();
        if(!value) value = "";
        if (element.value !== value) {
            element.value = value;
            this.raisePropertyChanged('text');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$TextBox$dispose() {
        if (this._changeHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "change", this._changeHandler);
            this._changeHandler = null;
        }
        if (this._keyPressHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "keypress", this._keyPressHandler);
            this._keyPressHandler = null;
        }

        Inflectra.SpiraTest.Web.ServerControls.TextBox.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$TextBox$_onChange() {
        var value = this.get_element().value;
        if (value !== this._text) {
            this._text = value;
            this.raisePropertyChanged('text');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$TextBox$_onKeyPress(e) {
        //var e = window.event;
        var key = e.keyCode ? e.keyCode : e.rawEvent.keyCode;

        if (key === Sys.UI.Key.enter) {
            var value = this.get_element().value;
            if (value !== this._text) {
                this._text = value;
                this.raisePropertyChanged('text');
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$TextBox$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.TextBox.callBaseMethod(this, 'initialize');

        var element = this.get_element();
        this._text = element.value;

        this._changeHandler = Function.createDelegate(this, this._onChange);
        Sys.UI.DomEvent.addHandler(element, "change", this._changeHandler);

        this._keyPressHandler = Function.createDelegate(this, this._onKeyPress);
        Sys.UI.DomEvent.addHandler(element, "keypress", this._keyPressHandler);
    }

Inflectra.SpiraTest.Web.ServerControls.TextBox.prototype = {

    _text: null,
    _changeHandler: null,
    _keyPressHandler: null,

    get_text: Inflectra$SpiraTest$Web$ServerControls$TextBox$get_text,

    set_text: Inflectra$SpiraTest$Web$ServerControls$TextBox$set_text,

    dispose: Inflectra$SpiraTest$Web$ServerControls$TextBox$dispose,

    _onChange: Inflectra$SpiraTest$Web$ServerControls$TextBox$_onChange,

    _onKeyPress: Inflectra$SpiraTest$Web$ServerControls$TextBox$_onKeyPress,

    initialize: Inflectra$SpiraTest$Web$ServerControls$TextBox$initialize

}

Inflectra.SpiraTest.Web.ServerControls.TextBox.descriptor = {
    properties: [ { name: 'text', type: String } ],
    attributes: [ { name: Inflectra.SpiraTest.Web.AjaxExtensions.Attributes.ValueProperty, value: 'text' } ]
}

Inflectra.SpiraTest.Web.ServerControls.TextBox.registerClass('Inflectra.SpiraTest.Web.ServerControls.TextBox', Inflectra.SpiraTest.Web.ServerControls.InputControl);

Inflectra.SpiraTest.Web.ServerControls.Selector = function Inflectra$SpiraTest$Web$ServerControls$Selector(associatedElement) {
    Inflectra.SpiraTest.Web.ServerControls.Selector.initializeBase(this, [associatedElement]);
    this._dataChangedDelegate = Function.createDelegate(this, this.dataBind);
}








    function Inflectra$SpiraTest$Web$ServerControls$Selector$add_selectionChanged(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("selectionChanged", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$remove_selectionChanged(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("selectionChanged", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$_onSelectionChanged() {
        this.raisePropertyChanged('selectedValue');
        var handler = this.get_events().getHandler("selectionChanged");
        if (handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$get_data() {
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._data;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Selector$set_data(value) {
        if (this._data && Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged.isImplementedBy(this._data)) {
            this._data.remove_collectionChanged(this._dataChangedDelegate);
        }
        this._data = value;
        if (this._data) {
            if (!Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.isInstanceOfType(this._data)) {
                if (this._data instanceof Array) {
                    this._data = new Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable([], this._data);
                }
                else if (typeof(this._data) === 'object') {
                    this._data = Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable.parseFromJson(this._data);
                }
                else {
                    throw Error.argumentType("data", Object.getType(value), Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable);
                }
            }
            this._data.add_collectionChanged(this._dataChangedDelegate);
        }
        this.dataBind();
        this.raisePropertyChanged('data');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$get_firstItemText() {
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._firstItemText;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Selector$set_firstItemText(value) {
        if (this._firstItemText != value) {
            this._firstItemText = value;
            this.raisePropertyChanged('firstItemText');
            this.dataBind();
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$get_selectedValue() {
        if (arguments.length !== 0) throw Error.parameterCount();
        return this.get_element().value;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Selector$set_selectedValue(value) {
        this.get_element().value = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$get_textProperty() {
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._textProperty;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Selector$set_textProperty(value) {
        this._textProperty = value;
        this.raisePropertyChanged('textProperty');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$get_valueProperty() {
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._valueProperty;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Selector$set_valueProperty(value) {
        this._valueProperty = value;
        this.raisePropertyChanged('valueProperty');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$dataBind() {
        var options = this.get_element().options;
        var selectedValues = [];
        var i;
        for (i = options.length - 1; i >= 0; i--) {
            if (options[i].selected) {
                Array.add(selectedValues, options[i].value);
            }
            options[i] = null;
        }
        var option;
        if (this._firstItemText && (this._firstItemText.length != 0)) {
            option = new Option(this._firstItemText, "");
            options[this.get_element().length] = option;
        }
        if (this._data) {
            var length = this._data.get_length();
            for (i = 0; i < length; i++) {
                var item = this._data.getItem(i);
                option = new Option(Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(item, this._textProperty),
                    Inflectra.SpiraTest.Web.AjaxExtensions.TypeDescriptor.getProperty(item, this._valueProperty));
                option.selected = Array.contains(selectedValues, option.value);
                options[this.get_element().length] = option;
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$dispose() {
        if (this._selectionChangedHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "change", this._selectionChangedHandler);
            this._selectionChangedHandler = null;
        }

        Inflectra.SpiraTest.Web.ServerControls.Selector.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Selector$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.Selector.callBaseMethod(this, 'initialize');

        this._selectionChangedHandler = Function.createDelegate(this, this._onSelectionChanged);
        Sys.UI.DomEvent.addHandler(this.get_element(), "change", this._selectionChangedHandler);
    }
Inflectra.SpiraTest.Web.ServerControls.Selector.prototype = {

    _selectionChangedHandler: null,
    _data: null,
    _textProperty: null,
    _valueProperty: null,
    _firstItemText: null,

    add_selectionChanged: Inflectra$SpiraTest$Web$ServerControls$Selector$add_selectionChanged,

    remove_selectionChanged: Inflectra$SpiraTest$Web$ServerControls$Selector$remove_selectionChanged,

    _onSelectionChanged: Inflectra$SpiraTest$Web$ServerControls$Selector$_onSelectionChanged,

    get_data: Inflectra$SpiraTest$Web$ServerControls$Selector$get_data,
    set_data: Inflectra$SpiraTest$Web$ServerControls$Selector$set_data,

    get_firstItemText: Inflectra$SpiraTest$Web$ServerControls$Selector$get_firstItemText,
    set_firstItemText: Inflectra$SpiraTest$Web$ServerControls$Selector$set_firstItemText,

    get_selectedValue: Inflectra$SpiraTest$Web$ServerControls$Selector$get_selectedValue,
    set_selectedValue: Inflectra$SpiraTest$Web$ServerControls$Selector$set_selectedValue,

    get_textProperty: Inflectra$SpiraTest$Web$ServerControls$Selector$get_textProperty,
    set_textProperty: Inflectra$SpiraTest$Web$ServerControls$Selector$set_textProperty,

    get_valueProperty: Inflectra$SpiraTest$Web$ServerControls$Selector$get_valueProperty,
    set_valueProperty: Inflectra$SpiraTest$Web$ServerControls$Selector$set_valueProperty,

    dataBind: Inflectra$SpiraTest$Web$ServerControls$Selector$dataBind,

    dispose: Inflectra$SpiraTest$Web$ServerControls$Selector$dispose,

    initialize: Inflectra$SpiraTest$Web$ServerControls$Selector$initialize
}
Inflectra.SpiraTest.Web.ServerControls.Selector.descriptor = {
    properties: [ { name: 'data', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable },
                  { name: 'firstItemText', type: String },
                  { name: 'selectedValue', type: String },
                  { name: 'textProperty', type: String },
                  { name: 'valueProperty', type: String } ],
    events: [ { name: 'selectionChanged', readOnly: true } ]
}
Inflectra.SpiraTest.Web.ServerControls.Selector.registerClass('Inflectra.SpiraTest.Web.ServerControls.Selector', Sys.UI.Control);


Inflectra.SpiraTest.Web.ServerControls.RequiredFieldValidator = function Inflectra$SpiraTest$Web$ServerControls$RequiredFieldValidator() {
    Inflectra.SpiraTest.Web.ServerControls.RequiredFieldValidator.initializeBase(this);
}



    function Inflectra$SpiraTest$Web$ServerControls$RequiredFieldValidator$validate(value) {
        /// <param name="value" mayBeNull="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "value", mayBeNull: true}
        ]);
        if (e) throw e;

        if (!value) {
            return false;
        }
        if (String.isInstanceOfType(value)) {
            if (value.length === 0) {
                return false;
            }
        }
        return true;
    }

Inflectra.SpiraTest.Web.ServerControls.RequiredFieldValidator.prototype = {

    validate: Inflectra$SpiraTest$Web$ServerControls$RequiredFieldValidator$validate
    
}

Inflectra.SpiraTest.Web.ServerControls.RequiredFieldValidator.registerClass('Inflectra.SpiraTest.Web.ServerControls.RequiredFieldValidator', Inflectra.SpiraTest.Web.ServerControls.Validator);
Inflectra.SpiraTest.Web.ServerControls.TypeValidator = function Inflectra$SpiraTest$Web$ServerControls$TypeValidator() {
    Inflectra.SpiraTest.Web.ServerControls.TypeValidator.initializeBase(this);
}





    function Inflectra$SpiraTest$Web$ServerControls$TypeValidator$get_type() {
        /// <value type="Type"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._type;
    }

    function Inflectra$SpiraTest$Web$ServerControls$TypeValidator$set_type(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Type}]);
        if (e) throw e;

        this._type = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$TypeValidator$validate(value) {
        /// <param name="value" mayBeNull="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "value", mayBeNull: true}
        ]);
        if (e) throw e;

        if (typeof(this._type) !== 'function') return false;
        if (this._type === String) return true;
        var parseMethod = this._type.parseLocale || this._type.parseInvariant || this.type.parse;
        if (typeof(parseMethod) !== 'function') return false;
        var valid = true;
        if (value && value.length) {
            try {
                var parsedValue = parseMethod(value);
                if (isNaN(parsedValue) || (parsedValue === null)) {
                    valid = false;
                }
            }
            catch (ex) {
                valid = false;
            }
        }
        return valid;
    }

Inflectra.SpiraTest.Web.ServerControls.TypeValidator.prototype = {

    _type: null,

    get_type: Inflectra$SpiraTest$Web$ServerControls$TypeValidator$get_type,

    set_type: Inflectra$SpiraTest$Web$ServerControls$TypeValidator$set_type,

    validate: Inflectra$SpiraTest$Web$ServerControls$TypeValidator$validate

}

Inflectra.SpiraTest.Web.ServerControls.TypeValidator.descriptor = {
    properties: [ { name: 'type', type: Type } ]
}

Inflectra.SpiraTest.Web.ServerControls.TypeValidator.registerClass('Inflectra.SpiraTest.Web.ServerControls.TypeValidator', Inflectra.SpiraTest.Web.ServerControls.Validator);

Inflectra.SpiraTest.Web.ServerControls.RangeValidator = function Inflectra$SpiraTest$Web$ServerControls$RangeValidator() {
    Inflectra.SpiraTest.Web.ServerControls.RangeValidator.initializeBase(this);
}






    function Inflectra$SpiraTest$Web$ServerControls$RangeValidator$get_lowerBound() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._lowerBound;
    }

    function Inflectra$SpiraTest$Web$ServerControls$RangeValidator$set_lowerBound(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._lowerBound = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$RangeValidator$get_upperBound() {
        /// <value type="Number"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._upperBound;
    }

    function Inflectra$SpiraTest$Web$ServerControls$RangeValidator$set_upperBound(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Number}]);
        if (e) throw e;

        this._upperBound = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$RangeValidator$validate(value) {
        /// <param name="value" mayBeNull="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "value", mayBeNull: true}
        ]);
        if (e) throw e;

        if (value && value.length) {
            return ((value <= this._upperBound) && (value >= this._lowerBound));
        }
        return true;
    }
Inflectra.SpiraTest.Web.ServerControls.RangeValidator.prototype = {
    
    _lowerBound: null,
    _upperBound: null,
    
    get_lowerBound: Inflectra$SpiraTest$Web$ServerControls$RangeValidator$get_lowerBound,
    
    set_lowerBound: Inflectra$SpiraTest$Web$ServerControls$RangeValidator$set_lowerBound,
    
    get_upperBound: Inflectra$SpiraTest$Web$ServerControls$RangeValidator$get_upperBound,
    
    set_upperBound: Inflectra$SpiraTest$Web$ServerControls$RangeValidator$set_upperBound,

    validate: Inflectra$SpiraTest$Web$ServerControls$RangeValidator$validate
}

Inflectra.SpiraTest.Web.ServerControls.RangeValidator.descriptor = {
    properties: [ { name: 'lowerBound', type: Number },
                  { name: 'upperBound', type: Number } ]
}

Inflectra.SpiraTest.Web.ServerControls.RangeValidator.registerClass('Inflectra.SpiraTest.Web.ServerControls.RangeValidator', Inflectra.SpiraTest.Web.ServerControls.Validator);
Inflectra.SpiraTest.Web.ServerControls.RegexValidator = function Inflectra$SpiraTest$Web$ServerControls$RegexValidator() {
    Inflectra.SpiraTest.Web.ServerControls.RegexValidator.initializeBase(this);
}




    function Inflectra$SpiraTest$Web$ServerControls$RegexValidator$get_regex() {
        /// <value></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._regex;
    }

    function Inflectra$SpiraTest$Web$ServerControls$RegexValidator$set_regex(value) {
        var e = Function._validateParams(arguments, [{name: "value"}]);
        if (e) throw e;

        if (typeof(value) === "string") {
            this._regex = new RegExp(value.replace(/^\/|\/$/g, "")); // stripping /'s for backwards compat.
        }
        else {
            this._regex = value;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$RegexValidator$validate(value) {
        /// <param name="value" mayBeNull="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "value", mayBeNull: true}
        ]);
        if (e) throw e;

        if (this._regex && value && value.length) {
            var matches = this._regex.exec(value);
            return (matches && (matches[0] === value));
        }
        return true;
    }
Inflectra.SpiraTest.Web.ServerControls.RegexValidator.prototype = {
    _regex: null,
    
    get_regex: Inflectra$SpiraTest$Web$ServerControls$RegexValidator$get_regex,
    
    set_regex: Inflectra$SpiraTest$Web$ServerControls$RegexValidator$set_regex,

    validate: Inflectra$SpiraTest$Web$ServerControls$RegexValidator$validate
}
Inflectra.SpiraTest.Web.ServerControls.RegexValidator.descriptor = {
    properties: [ {name: 'regex', type: String} ]
}
Inflectra.SpiraTest.Web.ServerControls.RegexValidator.registerClass('Inflectra.SpiraTest.Web.ServerControls.RegexValidator', Inflectra.SpiraTest.Web.ServerControls.Validator);
Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs = function Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs(value) {
    /// <param name="value" type="String"></param>
    var e = Function._validateParams(arguments, [
        {name: "value", type: String}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs.initializeBase(this);
    this._value = value;
}



    function Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs$get_value() {
        /// <value mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs$get_isValid() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._isValid;
    }

    function Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs$set_isValid(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        this._isValid = value;
    }
Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs.prototype = {
    _isValid: true,
       
    get_value: Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs$get_value,
    
    get_isValid: Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs$get_isValid,
    
    set_isValid: Inflectra$SpiraTest$Web$ServerControls$CustomValidationEventArgs$set_isValid
}
Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs.descriptor = {
    properties: [   {name: 'isValid', type: Boolean},
                    {name: 'value', readOnly: true} ]
}

Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs.registerClass('Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs', Sys.EventArgs);

Inflectra.SpiraTest.Web.ServerControls.CustomValidator = function Inflectra$SpiraTest$Web$ServerControls$CustomValidator() {
    Inflectra.SpiraTest.Web.ServerControls.CustomValidator.initializeBase(this);
}


    function Inflectra$SpiraTest$Web$ServerControls$CustomValidator$add_validateValue(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("validateValue", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$CustomValidator$remove_validateValue(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("validateValue", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$CustomValidator$validate(value) {
        /// <param name="value" mayBeNull="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "value", mayBeNull: true}
        ]);
        if (e) throw e;

        if (value && value.length) {
            var cve = new Inflectra.SpiraTest.Web.ServerControls.CustomValidationEventArgs(value);
            var handler = this.get_events().getHandler("validateValue");
            if (handler) {
                handler(this, cve);
            }
            
            return cve.get_isValid();
        }
        return true;
    }
Inflectra.SpiraTest.Web.ServerControls.CustomValidator.prototype = {
    
    add_validateValue: Inflectra$SpiraTest$Web$ServerControls$CustomValidator$add_validateValue,

    remove_validateValue: Inflectra$SpiraTest$Web$ServerControls$CustomValidator$remove_validateValue,

    validate: Inflectra$SpiraTest$Web$ServerControls$CustomValidator$validate
}
Inflectra.SpiraTest.Web.ServerControls.CustomValidator.descriptor = {
    events: [ {name: 'validateValue' } ]
}
Inflectra.SpiraTest.Web.ServerControls.CustomValidator.registerClass('Inflectra.SpiraTest.Web.ServerControls.CustomValidator', Inflectra.SpiraTest.Web.ServerControls.Validator);

Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel = function Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel(associatedElement) {
    /// <param name="associatedElement" domElement="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "associatedElement", domElement: true}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel.initializeBase(this, [associatedElement]);
}






    function Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$get_associatedControl() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._associatedControl;
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$set_associatedControl(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Object}]);
        if (e) throw e;

        if (this._associatedControl && this._validatedHandler) {
            this._associatedControl.remove_validated(this._validatedHandler);
        }
        
        if (Inflectra.SpiraTest.Web.ServerControls.IValidationTarget.isImplementedBy(value)) {
            this._associatedControl = value;
        }
        if (this._associatedControl) {
            if (!this._validatedHandler) {
                this._validatedHandler = Function.createDelegate(this, this._onControlValidated);
            }
            this._associatedControl.add_validated(this._validatedHandler);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$dispose() {
        if (this._associatedControl) {
            if (this._validatedHandler) {
                this._associatedControl.remove_validated(this._validatedHandler);
                this._validatedHandler = null;
            }
            this._associatedControl = null;
        }
        
        Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel.callBaseMethod(this, 'initialize');
        this.set_visible(false);
    }

    function Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$_onControlValidated(sender, eventArgs) {
        /// <param name="sender" type="Object"></param>
        /// <param name="eventArgs" type="Sys.EventArgs"></param>
        var e = Function._validateParams(arguments, [
            {name: "sender", type: Object},
            {name: "eventArgs", type: Sys.EventArgs}
        ]);
        if (e) throw e;

        var isInvalid = this._associatedControl.get_isInvalid();
        var tooltip = '';
        
        if (isInvalid) {
            tooltip = this._associatedControl.get_validationMessage();
        }

        this.set_visible(isInvalid);
        this.get_element().title = tooltip;
    }

Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel.prototype = {
    
    _associatedControl: null,
    _validatedHandler: null,
    
    get_associatedControl: Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$get_associatedControl,
    
    set_associatedControl: Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$set_associatedControl,

    dispose: Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$dispose,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$initialize,
    
    _onControlValidated: Inflectra$SpiraTest$Web$ServerControls$ValidationErrorLabel$_onControlValidated
    
}
    
Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel.descriptor = {
    properties: [ { name: 'associatedControl', type: Object } ]
}

Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel.registerClass('Inflectra.SpiraTest.Web.ServerControls.ValidationErrorLabel', Inflectra.SpiraTest.Web.ServerControls.Label);

Type.registerNamespace('Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components');
Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile = function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile() {
    Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.initializeBase(this);
}




    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$get_autoSave() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._autoSave;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$set_autoSave(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: Boolean}]);
        if (e) throw e;

        this._autoSave = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$get_isDirty() {
        /// <value type="Boolean"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._isDirty;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$get_path() {
        /// <value type="String" mayBeNull="true"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return Sys.Services.ProfileService.get_path();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$set_path(value) {
        var e = Function._validateParams(arguments, [{name: "value", type: String, mayBeNull: true}]);
        if (e) throw e;

        Sys.Services.ProfileService.set_path(value);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$add_loadComplete(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('loadComplete', handler);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$remove_loadComplete(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('loadComplete', handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$add_saveComplete(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler('saveComplete', handler);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$remove_saveComplete(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler('saveComplete', handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$getProperty(name, key) {
        /// <param name="name" type="String" optional="false" mayBeNull="false"></param>
        /// <param name="key" optional="true" mayBeNull="true"></param>
        /// <returns></returns>
        var e = Function._validateParams(arguments, [
            {name: "name", type: String},
            {name: "key", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var props = Sys.Services.ProfileService.properties;
        if(key) {
            var group = props[name];
            return group ? (group[key]||null) : null;
        }
        return props[name] || null;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.callBaseMethod(this, 'initialize');
        
        var empty = true;
        for(var v in Sys.Services.ProfileService.properties) {
            empty = false;
            break;
        }
        if (empty) {
            this.load(); 
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$invokeMethod(methodName, parameters) {
        if(methodName === "save") {
            this.save.apply(this, parameters);
        }
        else if(methodName === "load") {
            this.load.apply(this, parameters);
        }
        else {
            throw Error.invalidOperation(String.format('Method "{0}" not found on object of type "{1}"', methodName, Object.getTypeName(this)));
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$load(propertyNames) {
        /// <param name="propertyNames" type="Array" elementType="String" mayBeNull="true" optional="true" elementMayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyNames", type: Array, mayBeNull: true, optional: true, elementType: String}
        ]);
        if (e) throw e;

        if(!this.loadCallback) this.loadCallback = Function.createDelegate(this,this._loadComplete);
        Sys.Services.ProfileService.load(propertyNames, this.loadCallback);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$save(propertyNames) {
        /// <param name="propertyNames" type="Array" elementType="String" mayBeNull="true" optional="true" elementMayBeNull="false"></param>
        var e = Function._validateParams(arguments, [
            {name: "propertyNames", type: Array, mayBeNull: true, optional: true, elementType: String}
        ]);
        if (e) throw e;

        if(!this.saveCallback) this.saveCallback = Function.createDelegate(this,this._saveComplete);
        Sys.Services.ProfileService.save(propertyNames, this.saveCallback);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$setProperty(name, value, key) {
        /// <param name="name" type="String" optional="false" mayBeNull="false"></param>
        /// <param name="value" optional="false" mayBeNull="true"></param>
        /// <param name="key" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "name", type: String},
            {name: "value", mayBeNull: true},
            {name: "key", mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

        var props = Sys.Services.ProfileService.properties;
        var wasDirty=null;
        var fullName = name;
        if(key) {
            var group = props[fullName];
            if(!group) {
                group = new Sys.Services.ProfileGroup();
                props[fullName] = group;
            }
            fullName = fullName + '.' + key;
            group[key] = value;
            wasDirty = this._isDirty;
            this._isDirty = true;
            this.raisePropertyChanged(fullName);
        }
        else {
            props[fullName] = value;
            wasDirty = this._isDirty;
            this._isDirty = true;
            this.raisePropertyChanged(fullName);
        }
        
        // wasDirty=null means no property was set, we expect not to raise isDirty changed
        if (wasDirty === false) {
            this.raisePropertyChanged('isDirty');
        }
        
        if (this._autoSave && this._isDirty) {
            // isDirty checked again since an event handler may have called save()
            this.save([fullName]);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$_loadComplete() {
        // consider: raise a property changed notification for every property in .properties
        // otherwise bindings won't be notified of updates, they're only notified if someone explicitly sets the property
        this._isDirty = false;
        var handler = this.get_events().getHandler('loadComplete');
        if(handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$_saveComplete() {
        this._isDirty = false;
        this.raisePropertyChanged('isDirty');
        var handler = this.get_events().getHandler('saveComplete');
        if(handler) {
            handler(this, Sys.EventArgs.Empty);
        }
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$_saveIfDirty() {
        if (this._isDirty) {
            this.save();
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.prototype = {
    _isDirty: false,
    _autoSave: false,
    
    get_autoSave: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$get_autoSave,
    set_autoSave: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$set_autoSave,
    
    get_isDirty: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$get_isDirty,
    
    get_path: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$get_path,
    set_path: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$set_path,
    
    add_loadComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$add_loadComplete,
    remove_loadComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$remove_loadComplete,
    
    add_saveComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$add_saveComplete,
    remove_saveComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$remove_saveComplete,
    
    getProperty: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$getProperty,
    
    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$initialize,    
       
    invokeMethod: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$invokeMethod,

    load: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$load,
        
    save: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$save,
        
    setProperty: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$setProperty,

    _loadComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$_loadComplete,

    _saveComplete: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$_saveComplete,

    _saveIfDirty: Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$_saveIfDirty    
}
// note: Because ICustomTypeDescriptor is implemented, and set/getProperty goes to the profile properties collection,
//       you cannot get/set the autoSave or isDirty properties through type descriptor.
//       We could special case these but then you could have a profile property named "autoSave" or "isDirty"
Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.descriptor = {
    properties: [   {name: 'autoSave', type: Boolean},
                    {name: 'path', type: String},
                    {name: 'isDirty', type: Boolean, readOnly: true} ],
    methods: [  {name: 'load'},
                {name: 'save'} ],
    events: [   {name: 'loadComplete'},
                {name: 'saveComplete'} ]
}
Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile', Sys.Component, Inflectra.SpiraTest.Web.AjaxExtensions.ICustomTypeDescriptor);

Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.parseFromMarkup = function Inflectra$SpiraTest$Web$AjaxExtensions$Services$Components$Profile$parseFromMarkup(type, node, markupContext) {
    if (!markupContext.get_isGlobal()) {
        return null;
    }

    var id=null;
    var idAttribute = node.attributes.getNamedItem('id');
    if (idAttribute) {
        id = idAttribute.nodeValue;
        node.attributes.removeNamedItem('id')
    }
    
    Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.instance, node, markupContext);
    
    if (id && id.length) {
        markupContext._addComponentByID(id, Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.instance, true);
        node.attributes.setNamedItem(idAttribute);
    }
    return Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.instance;
}
Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile.instance = new Inflectra.SpiraTest.Web.AjaxExtensions.Services.Components.Profile();

Type.registerNamespace('Inflectra.SpiraTest.Web.ServerControls.Data');

Inflectra.SpiraTest.Web.ServerControls.Data.DataControl = function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl(associatedElement) {
    // <param name="associatedElement">Associated dom element.</param>
    Inflectra.SpiraTest.Web.ServerControls.Data.DataControl.initializeBase(this, [associatedElement]);
    this._dataIndex = 0;//-1;
}






    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$prepareChange() {
        // <returns>Index location statistics.</returns>
        return {dataIndex: this.get_dataIndex(), canMoveNext: this.get_canMoveNext(), canMovePrevious: this.get_canMovePrevious()};
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$triggerChangeEvents(oldState) {
        // <param name="oldState">Last index location statistics.</param>
        var dataIndex = this.get_dataIndex();
        if (oldState.dataIndex !== dataIndex) {
            this.raisePropertyChanged('dataIndex');
            this.raisePropertyChanged('dataItem');
            oldState.dataIndex = dataIndex;
        }
        var canMoveNext = this.get_canMoveNext();
        if (oldState.canMoveNext !== canMoveNext) {
            this.raisePropertyChanged('canMoveNext');
            oldState.canMoveNext = canMoveNext;
        }
        var canMovePrevious = this.get_canMovePrevious();
        if (oldState.canMovePrevious !== canMovePrevious) {
            this.raisePropertyChanged('canMovePrevious');
            oldState.canMovePrevious = canMovePrevious;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_canMoveNext() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value>True if next item exists, false if not.</value>
        if (!this._data) return false;
        return (this._dataIndex < this.get_length() - 1);
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_canMovePrevious() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value>True if previous item exists, false if not.</value>
        if (!this._data) return false;
        return (this._dataIndex > 0);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_data() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Data.</value>
        return this._data;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$set_data(value) {
        // <param name="value" optional="true" mayBeNull="true">Data.</param>
        var oldState = this.prepareChange();
        if (this._data && Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged.isImplementedBy(this._data)) {
            this._data.remove_collectionChanged(this._dataChangedDelegate);
            this._dataChangedDelegate = null;
        }
        this._data = value;
        if (this._data && Inflectra.SpiraTest.Web.AjaxExtensions.INotifyCollectionChanged.isImplementedBy(this._data)) {
            this._dataChangedDelegate = Function.createDelegate(this, this.onDataChanged);
            this._data.add_collectionChanged(this._dataChangedDelegate);
        }
        if (this._dataIndex >= this.get_length()) {
            this.set_dataIndex(0);
        }

        if (!this.get_isUpdating()) {
            this.render();
        }

        this.raisePropertyChanged('data');
        this.triggerChangeEvents(oldState);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_dataContext() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Selected data.</value>
        return this.get_dataItem();
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_dataIndex() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Number">Data index.</value>
        return this._dataIndex;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$set_dataIndex(value) {
        if (this._dataIndex !== value) {
            var oldState = this.prepareChange();
            this._dataIndex = value;
            if (!this._suspendChangeNotifications) {
                this.triggerChangeEvents(oldState);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_dataItem() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Selected data.</value>
        if (this._data && (this._dataIndex >= 0)) {
            if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
                return this._data.getItem(this._dataIndex);
            }
            if (this._data instanceof Array) {
                return this._data[this._dataIndex];
            }
        }
        return null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_length() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Number">Number of data rows.</value>
        if(!this._data) return 0;
        if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
            return this._data.get_length();
        }
        if (this._data instanceof Array) {
            return this._data.length;
        }
        return 0;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$addItem() {
        if (this._data) {
            var oldState = this.prepareChange();
            if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
                this._data.add({});
            }
            else if (this._data instanceof Array) {
                if(typeof(this._data.add) === "function") {
                    this._data.add({});
                }
                else {
                    Array.add(this._data, {});
                }
            }
            this.set_dataIndex(this.get_length() - 1);
            this.triggerChangeEvents(oldState);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$deleteCurrentItem() {
        if (this._data) {
            var oldState = this.prepareChange();
            this._suspendChangeNotifications = true;
            var item = this.get_dataItem();
            if (this.get_dataIndex() === this.get_length() - 1) {
                this.set_dataIndex(Math.max(0, this.get_length() - 2));
            }
            if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
                this._data.remove(item);
            }
            else if (this._data instanceof Array) {
                if(typeof(this._data.remove) === "function") {
                    this._data.remove(item);
                }
                else {
                    Array.remove(this._data, item);
                }
            }
            this._suspendChangeNotifications = false;
            this.triggerChangeEvents(oldState);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$getItem(index) {
        // <param name="index" type="Number">Item index.</param>
        // <returns>Data at the specified index.</returns>
        if (this._data) {
            if (Inflectra.SpiraTest.Web.AjaxExtensions.Data.IData.isImplementedBy(this._data)) {
                return this._data.getItem(index);
            }
            if (this._data instanceof Array) {
                return this._data[index];
            }
        }
        return null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$moveNext() {
        if (this._data) {
            var oldState = this.prepareChange();
            var newIndex = this.get_dataIndex() + 1;
            if (newIndex < this.get_length()) {
                this.set_dataIndex(newIndex);
            }
            this.triggerChangeEvents(oldState);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$movePrevious() {
        if (this._data) {
            var oldState = this.prepareChange();
            var newIndex = this.get_dataIndex() - 1;
            if (newIndex >=0) {
                this.set_dataIndex(newIndex);
            }
            this.triggerChangeEvents(oldState);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$onBubbleEvent(source, args) {
        // <param name="source" type="Object">Event source.</param>
        // <param name="args" type="Sys.EventArgs">Event arguments.</param>
        // <returns type="Boolean">True to bubble event up to the parent, false to stop.</returns>
        if (args.get_commandName() === "select") {
            var arg = args.get_argument();
            if (!arg && arg !== 0) {
                var dataContext = source.get_dataContext();
                if (dataContext) {
                    arg = dataContext.get_index();
                }
            }
            if (arg && String.isInstanceOfType(arg)) {
                arg = Number.parseInvariant(arg);
            }
            if (arg || arg === 0) {
                this.set_dataIndex(arg);
                return true;
            }
        }
        return false;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$onDataChanged(sender, args) {
        // <param name="sender" type="Object">Event source.</param>
        // <param name="args" type="Sys.EventArgs">Event arguments.</param>
        this.render();
    }
Inflectra.SpiraTest.Web.ServerControls.Data.DataControl.prototype = {
    _data: null,
    _suspendChangeNotifications: false,

    _dataChangedDelegate: null,

    prepareChange: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$prepareChange,
    triggerChangeEvents: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$triggerChangeEvents,

    get_canMoveNext: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_canMoveNext,
    get_canMovePrevious: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_canMovePrevious,

    get_data: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_data,
    set_data: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$set_data,

    get_dataContext: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_dataContext,

    get_dataIndex: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_dataIndex,
    set_dataIndex: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$set_dataIndex,

    get_dataItem: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_dataItem,

    get_length: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$get_length,

    addItem: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$addItem,

    deleteCurrentItem: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$deleteCurrentItem,

    getItem: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$getItem,

    moveNext: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$moveNext,

    movePrevious: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$movePrevious,

    onBubbleEvent: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$onBubbleEvent,

    onDataChanged: Inflectra$SpiraTest$Web$ServerControls$Data$DataControl$onDataChanged
}
Inflectra.SpiraTest.Web.ServerControls.Data.DataControl.descriptor = {
    properties: [ { name: 'canMoveNext', type: Boolean, readOnly: true },
                  { name: 'canMovePrevious', type: Boolean, readOnly: true },
                  { name: 'data', type: Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataTable },
                  { name: 'dataIndex', type: Number },
                  { name: 'dataItem', type: Object, readOnly: true },
                  { name: 'length', type: Number, readOnly: true } ],
    methods: [ { name: 'addItem' },
               { name: 'deleteCurrentItem' },
               { name: 'moveNext' },
               { name: 'movePrevious' } ]
}
Inflectra.SpiraTest.Web.ServerControls.Data.DataControl.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.DataControl', Sys.UI.Control);
Inflectra.SpiraTest.Web.ServerControls.Data.DataNavigator = function Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator(associatedElement) {
    // <param name="associatedElement">Associated dom element.</param>
    Inflectra.SpiraTest.Web.ServerControls.Data.DataNavigator.initializeBase(this, [associatedElement]);
}



    function Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$get_dataView() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView">Data view.</value>
        return this._data;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$set_dataView(value) {
        Sys.Debug.assert(Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.isInstanceOfType(value), "dataView must be of type Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.");
        this._data = value;
        this.raisePropertyChanged('dataView');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$get_dataContext() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Data view.</value>
        return this.get_dataView();
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$onBubbleEvent(source, args) {
        // <param name="source" type="Object">Event source.</param>
        // <param name="args" type="Sys.EventArgs">Event arguments.</param>
        // <returns type="Boolean">True to bubble event up to the parent, false to stop.</returns>
        if (!this._data) return false;
        var cmd = args.get_commandName().toLowerCase();
        switch(cmd) {
        case "page":
            var arg = args.get_argument();
            if (arg && String.isInstanceOfType(arg)) {
                arg = Number.parseInvariant(arg);
            }
            if (arg || arg === 0) {
                this._data.set_pageIndex(arg);
                return true;
            }
            break;

        case "nextpage":
            this._data.set_pageIndex(this._data.get_pageIndex() + 1);
            return true;

        case "previouspage":
            var idx = this._data.get_pageIndex() - 1;
            if (idx >= 0) {
                this._data.set_pageIndex(idx);
            }
            return true;

        case "firstpage":
            this._data.set_pageIndex(0);
            return true;

        case "lastpage":
            this._data.set_pageIndex(this._data.get_pageCount() - 1);
            return true;
        }
        return false;
    }
Inflectra.SpiraTest.Web.ServerControls.Data.DataNavigator.prototype = {
    _data: null,

    get_dataView: Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$get_dataView,
    set_dataView: Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$set_dataView,

    get_dataContext: Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$get_dataContext,

    onBubbleEvent: Inflectra$SpiraTest$Web$ServerControls$Data$DataNavigator$onBubbleEvent
}
Inflectra.SpiraTest.Web.ServerControls.Data.DataNavigator.descriptor = {
    properties: [ { name: 'dataView', type: Object } ]
}
Inflectra.SpiraTest.Web.ServerControls.Data.DataNavigator.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.DataNavigator', Sys.UI.Control);

Inflectra.SpiraTest.Web.ServerControls.Data.ItemView = function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView(associatedElement) {
    // <param name="associatedElement">Associated dom element.</param>
    Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.initializeBase(this, [associatedElement]);
}








    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$set_dataIndex(value) {
        // <param name="value" type="Number">Data index</param>
        if (this.get_dataIndex() !== value) {
            Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.callBaseMethod(this, 'set_dataIndex', [value]);
            if (!this.get_isUpdating()) {
                this.render();
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$get_emptyTemplate() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.ServerControls.ITemplate">Empty template.</value>
        return this._emptyTemplate;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$set_emptyTemplate(value) {
        if (this._emptyTemplate) {
            this._emptyTemplate.dispose();
        }
        this._emptyTemplate = value;
        
        if (!this.get_isUpdating()) {
            this.render();
        }
        this.raisePropertyChanged('emptyTemplate');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$get_itemTemplate() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.ServerControls.ITemplate">Item template.</value>
        return this._itemTemplate;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$set_itemTemplate(value) {
        if (this._itemTemplate) {
            this._itemTemplate.dispose();
        }
        this._itemTemplate = value;
        
        if (!this.get_isUpdating()) {
            this.render();
        }
        this.raisePropertyChanged('itemTemplate');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$dispose() {
        var element = this.get_element();
        if (element) {
            if (this._keyDownHandler) {
                Sys.UI.DomEvent.removeHandler(element, "keydown", this._keyDownHandler);
            }
            if (element.childNodes.length) {
                element.markupContext = null;
                Inflectra.SpiraTest.Web.ServerControls.ITemplate.disposeInstance(element.firstChild);
            }
        }
        if (this._itemTemplate) {
            this._itemTemplate.dispose();
            this._itemTemplate = null;
        }
        
        if (this._emptyTemplate) {
            this._emptyTemplate.dispose();
            this._emptyTemplate = null;
        }
        
        this._layoutTemplateElement = null;
        
        Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$initialize() {
        this._keyDownHandler = Function.createDelegate(this, this._onKeyDown);

        Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.callBaseMethod(this, 'initialize');

        Sys.UI.DomEvent.addHandler(this.get_element(), "keydown", this._keyDownHandler);

        if (this._itemTemplate) {
            this._itemTemplate.initialize();
        }
        if (this._emptyTemplate) {
            this._emptyTemplate.initialize();
        }
        
        this.render();
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$_onKeyDown(ev) {
        if (ev.target === this.get_element()) {
            var k = ev.keyCode ? ev.keyCode : ev.rawEvent.keyCode;
            if ((k === Sys.UI.Key.up) || (k === Sys.UI.Key.left)) {
                this.movePrevious();
                ev.preventDefault();
            }
            else if ((k === Sys.UI.Key.down) || (k === Sys.UI.Key.right)) {
                this.moveNext();
                ev.preventDefault();
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$render() {
        var element = this.get_element();
        if (element.childNodes.length) {
            if (this._layoutTemplateElement) {
                Inflectra.SpiraTest.Web.ServerControls.ITemplate.disposeInstance(this._layoutTemplateElement);
            }
        }
        element.innerHTML = '';
        
        var template;
        var data = this.get_data();
        if (data && data.get_length()) {
            template = this._itemTemplate;
        }
        else {
            template = this._emptyTemplate;
        }
        
        if (template) {
            var instance = template.createInstance(element, this.get_dataContext()).instanceElement;
            element.markupContext = instance.markupContext;
            this._layoutTemplateElement = instance;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$findObject(id) {
        // <param name="id" type="String">ID of the object to find.</param>
        // <returns type="Object">The component if one is found, null otherwise.</returns>
        var object;
        var element = this.get_element();
        if (element.markupContext) {
            object = element.markupContext.findComponent(id);
        }
        if (!object) {
            var parent = this.get_parent();
            if (parent) {
                object = parent.findObject(id);
            }
            else {
                object = Sys.Application.findComponent(id);
            }
        }
        return object;
    }
Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.prototype = {
    _itemTemplate: null,
    _emptyTemplate: null,
    
    _keyDownHandler: null,
    
    _layoutTemplateElement: null,

    set_dataIndex: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$set_dataIndex,

    get_emptyTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$get_emptyTemplate,
    set_emptyTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$set_emptyTemplate,
    
    get_itemTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$get_itemTemplate,
    set_itemTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$set_itemTemplate,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$dispose,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$initialize,
    
    _onKeyDown: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$_onKeyDown,
    
    render: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$render,
    
    findObject: Inflectra$SpiraTest$Web$ServerControls$Data$ItemView$findObject
}
Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.descriptor = {
    properties: [ { name: 'itemTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate },
                  { name: 'emptyTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate } ]
}
Inflectra.SpiraTest.Web.ServerControls.Data.ItemView.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.ItemView', Inflectra.SpiraTest.Web.ServerControls.Data.DataControl, Sys.IContainer);
Inflectra.SpiraTest.Web.ServerControls.Data.ListViewRenderTask = function Inflectra$SpiraTest$Web$ServerControls$Data$ListViewRenderTask(listView, data, itemTemplate, itemTemplateParent, separatorTemplate, itemElements, separatorElements, itemClass, alternatingItemClass, separatorClass, itemFocusHandler, itemClickHandler) {
    // <param name="listView" />
    // <param name="data" mayBeNull="true" />
    // <param name="itemTemplate" mayBeNull="true" />
    // <param name="itemTemplateParent" mayBeNull="true" />
    // <param name="separatorTemplate" mayBeNull="true" />
    // <param name="itemElements" mayBeNull="true" />
    // <param name="separatorElements" mayBeNull="true" />
    // <param name="itemClass" mayBeNull="true" />
    // <param name="alternatingItemClass" mayBeNull="true" />
    // <param name="separatorClass" mayBeNull="true" />
    // <param name="itemFocusHandler" />
    // <param name="itemClickHandler" />
    this._listView = listView;
    this._data = data;
    this._itemTemplate = itemTemplate;
    this._itemTemplateParent = itemTemplateParent;
    this._separatorTemplate = separatorTemplate;
    this._itemElements = itemElements;
    this._separatorElements = separatorElements;
    this._itemClass = itemClass;
    this._alternatingItemClass = alternatingItemClass;
    this._separatorClass = separatorClass;
    this._itemFocusHandler = itemFocusHandler;
    this._itemClickHandler = itemClickHandler;
    this._currentIndex = 0;
}




    function Inflectra$SpiraTest$Web$ServerControls$Data$ListViewRenderTask$dispose() {
        this._listView = null;
        this._data = null;
        this._itemTemplate = null;
        this._itemTemplateParent = null;
        this._separatorTemplate = null;
        this._itemElements = null;
        this._separatorElements = null;
        this._itemClass = null;
        this._alternatingItemClass = null;
        this._separatorClass = null;
        this._itemFocusHandler = null;
        this._itemClickHandler = null;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListViewRenderTask$execute() {
        // <returns type="Boolean">True if rendering is complete, false if not.</returns>
        var isArray = Array.isInstanceOfType(this._data);
        var itemLength = isArray? this._data.length : (this._data ? (this._data.get_length ? this._data.get_length() : 0) : 0);
        var lengthm1 = itemLength - 1;

        // Determine the last element to render: either the last item in the list, or the chunk size of the task
        var lastElementToRender = Math.min(itemLength, this._currentIndex + 5);
        for (; this._currentIndex < lastElementToRender; this._currentIndex++) {
            var item = isArray? this._data[this._currentIndex] : this._data.getItem(this._currentIndex);
            if (this._itemTemplate) {
                var element = this._itemTemplate.createInstance(this._itemTemplateParent, item).instanceElement;
                if (this._itemClass) {
                    if ((this._currentIndex % 2 === 1) && (this._alternatingItemClass)) {
                        element.className = this._alternatingItemClass;
                    }
                    else {
                        element.className = this._itemClass;
                    }
                }
                this._itemElements[this._currentIndex] = element;
                element.tabIndex = -1;
                element.dataIndex = this._currentIndex;
                Sys.UI.DomEvent.addHandler(element, "focus", this._itemFocusHandler);
                Sys.UI.DomEvent.addHandler(element, "click", this._itemClickHandler);
                //element.attachEvent("onfocus", this._itemFocusHandler);
                //element.attachEvent("onclick", this._itemClickHandler);
            }
            if (this._separatorTemplate && (this._currentIndex !== lengthm1) && this._itemTemplateParent) {
                var sep = this._separatorTemplate.createInstance(this._itemTemplateParent).instanceElement;
                if (this._separatorClass) {
                    sep.className = this._separatorClass;
                }
                this._separatorElements[this._currentIndex] = sep;
            }
        }

        if (this._currentIndex === itemLength) {






            // tell listview we've rendered
            this._listView._renderTaskComplete(this);
            // We are done rendering, so don't go back into the TaskManager queue
            return true;
        }
        else {
            // We still have more items to render, so tell the TaskManager we need to be called again
            return false;
        }
    }
Inflectra.SpiraTest.Web.ServerControls.Data.ListViewRenderTask.prototype = {
//#define LISTVIEWTIMING


    dispose: Inflectra$SpiraTest$Web$ServerControls$Data$ListViewRenderTask$dispose,

    execute: Inflectra$SpiraTest$Web$ServerControls$Data$ListViewRenderTask$execute
}
Inflectra.SpiraTest.Web.ServerControls.Data.ListViewRenderTask.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.ListViewRenderTask', null, Inflectra.SpiraTest.Web.AjaxExtensions.ITask, Sys.IDisposable);


Inflectra.SpiraTest.Web.ServerControls.Data.ListView = function Inflectra$SpiraTest$Web$ServerControls$Data$ListView(associatedElement) {
    // <param name="associatedElement">Associated dom element.</param>
    Inflectra.SpiraTest.Web.ServerControls.Data.ListView.initializeBase(this, [associatedElement]);
    this._itemElements = [];
    this._separatorElements = [];
}























    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_alternatingItemCssClass() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Alternating item css class.</value>
        return this._alternatingItemClass;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_alternatingItemCssClass(value) {
        if (value !== this._alternatingItemClass) {
            this._alternatingItemClass = value;
            this.render();
            this.raisePropertyChanged('alternatingItemCssClass');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_dataIndex(value) {
        // <param name="value" type="Number">Data index.</param>
        var oldIndex = this.get_dataIndex();
        if (oldIndex !== value) {
            var sel = this.getItemElement(oldIndex);
            if (sel && this._selectedItemClass) {
                Sys.UI.DomElement.removeCssClass(sel, this._selectedItemClass);
            }
            Inflectra.SpiraTest.Web.ServerControls.Data.ListView.callBaseMethod(this, 'set_dataIndex', [value]);
            sel = this.getItemElement(value);
            if (sel && this._selectedItemClass) {
                Sys.UI.DomElement.addCssClass(sel, this._selectedItemClass);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_emptyTemplate() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.ServerControls.ITemplate">Empty template.</value>
        return this._emptyTemplate;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_emptyTemplate(value) {
        if (this._emptyTemplate) {
            this._emptyTemplate.dispose();
        }
        this._emptyTemplate = value;

        if (!this.get_isUpdating()) {
            this.render();
        }
        this.raisePropertyChanged('emptyTemplate');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_itemCssClass() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Item css class.</value>
        return this._itemClass;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_itemCssClass(value) {
        if (value !== this._itemClass) {
            this._itemClass = value;
            this.render();
            this.raisePropertyChanged('itemCssClass');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_itemTemplate() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.ServerControls.ITemplate">Item template.</value>
        return this._itemTemplate;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_itemTemplate(value) {
        if (this._itemTemplate) {
            this._itemTemplate.dispose();
        }
        this._itemTemplate = value;

        if (!this.get_isUpdating()) {
            this.render();
        }
        this.raisePropertyChanged('itemTemplate');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_itemTemplateParentElementId() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Item template parent id.</value>
        return this._itemTemplateParentElementId;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_itemTemplateParentElementId(value) {
        this._itemTemplateParentElementId = value;
        this.raisePropertyChanged('itemTemplateParentElementId');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_layoutTemplate() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.ServerControls.ITemplate">Layout template.</value>
        return this._layoutTemplate;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_layoutTemplate(value) {
        if (this._layoutTemplate) {
            this._layoutTemplate.dispose();
        }
        this._layoutTemplate = value;

        if (!this.get_isUpdating()) {
            this.render();
        }
        this.raisePropertyChanged('layoutTemplate');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_selectedItemCssClass() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Selected item css class.</value>
        return this._selectedItemClass;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_selectedItemCssClass(value) {
        if (value !== this._selectedItemClass) {
            this._selectedItemClass = value;
            this.render();
            this.raisePropertyChanged('selectedItemCssClass');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_separatorCssClass() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Separator css class.</value>
        return this._separatorClass;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_separatorCssClass(value) {
        if (value !== this._separatorClass) {
            this._separatorClass = value;
            this.render();
            this.raisePropertyChanged('separatorCssClass');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_separatorTemplate() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="Inflectra.SpiraTest.Web.ServerControls.ITemplate">Separator template.</value>
        return this._separatorTemplate;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_separatorTemplate(value) {
        if (this._separatorTemplate) {
            this._separatorTemplate.dispose();
        }
        this._separatorTemplate = value;

        if (!this.get_isUpdating()) {
            this.render();
        }
        this.raisePropertyChanged('separatorTemplate');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$getItemElement(index) {
        // <param name="index">Element index.</param>
        // <returns>Item element.</returns>
        return this._itemElements[index];
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$add_renderComplete(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("renderComplete", handler);
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$remove_renderComplete(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("renderComplete", handler);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$initialize() {
        var element = this.get_element();
        this._focusHandler = Function.createDelegate(this, this._onGotFocus);
        this._keyDownHandler = Function.createDelegate(this, this._onKeyDown);
        this._itemFocusHandler = Function.createDelegate(this, this._onItemFocus);
        this._itemClickHandler = Function.createDelegate(this, this._onItemClick);

        Inflectra.SpiraTest.Web.ServerControls.Data.ListView.callBaseMethod(this, 'initialize');

        Sys.UI.DomEvent.addHandler(element, "keydown", this._keyDownHandler);
        Sys.UI.DomEvent.addHandler(element, "focus", this._focusHandler);

        if (this._itemTemplate) {
            this._itemTemplate.initialize();
        }
        if (this._separatorTemplate) {
            this._separatorTemplate.initialize();
        }
        if (this._emptyTemplate) {
            this._emptyTemplate.initialize();
        }
        if (this._layoutTemplate) {
            this._layoutTemplate.initialize();
        }

        if (!element.tabIndex) {
            element.tabIndex = 0;
        }

        this.render();
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$dispose() {
        if(this._disposed) return;
        var element = this.get_element();
        if (element) {
            if (this._focusHandler) {
                Sys.UI.DomEvent.removeHandler(element, "focus", this._focusHandler);
            }
            if (this._keyDownHandler) {
                Sys.UI.DomEvent.removeHandler(element, "keydown", this._keyDownHandler);
            }
        }
        if (this._itemElements) {
            for (var i = this._itemElements.length - 1; i >= 0; i--) {
                if (this._itemFocusHandler) {
                    Sys.UI.DomEvent.removeHandler(this._itemElements[i], "focus", this._itemFocusHandler);
                }
                if (this._itemClickHandler) {
                    Sys.UI.DomEvent.removeHandler(this._itemElements[i], "click", this._itemClickHandler);
                }
            }
        }

        if (this._layoutTemplate) {
            this._layoutTemplate.dispose();
            this._layoutTemplate = null;
        }
        if (this._itemTemplate) {
            this._itemTemplate.dispose();
            this._itemTemplate = null;
        }
        if (this._separatorTemplate) {
            this._separatorTemplate.dispose();
            this._separatorTemplate = null;
        }
        if (this._emptyTemplate) {
            this._emptyTemplate.dispose();
            this._emptyTemplate = null;
        }
        this._itemElements = null;
        this._separatorElements = null;
        this._layoutTemplateElement = null;
        this._disposed = true;

        Inflectra.SpiraTest.Web.ServerControls.Data.ListView.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onGotFocus(ev) {
        if (ev.target === this.get_element()) {
            this.setFocus(this, this.getItemElement(this.get_dataIndex()));
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onKeyDown(ev) {
        if (ev.target === this.getItemElement(this._focusIndex)) {
            var k = ev.keyCode ? ev.keyCode : ev.rawEvent.keyCode;
            if ((k === Sys.UI.Key.up) || (k === Sys.UI.Key.left)) {
                if (this._focusIndex > 0) {
                    this.setFocus(this, this.getItemElement(this._focusIndex - 1));
                    ev.preventDefault();
                }
            }
            else if ((k === Sys.UI.Key.down) || (k === Sys.UI.Key.right)) {
                if (this._focusIndex < (this.get_length() - 1)) {
                    this.setFocus(this, this.getItemElement(this._focusIndex + 1));
                    ev.preventDefault();
                }
            }
            else if ((k === Sys.UI.Key.enter) || (k === Sys.UI.Key.space)) {
                if (this._focusIndex !== -1) {
                    this.set_dataIndex(this._focusIndex);
                    ev.preventDefaut();
                }
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onItemFocus(ev) {
        if (typeof(ev.target.dataIndex) !== "undefined") {
            this._focusIndex = ev.target.dataIndex;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onItemClick(ev) {
        var s = ev.target;
        var srcTag = s.tagName.toUpperCase();
        while (s && (typeof(s.dataIndex) === 'undefined')) {
            s = s.parentNode;
        }
        if (s) {
            var idx = s.dataIndex;
            sel = this.getItemElement(idx);
            if (sel) {
                this.set_dataIndex(idx);
                if ((srcTag !== "INPUT") && (srcTag !== "TEXTAREA") &&
                    (srcTag !== "SELECT") && (srcTag !== "BUTTON") && (srcTag !== "A")) {
                    this.setFocus(this, sel);
                }
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$render() {
        var associatedElement = this.get_element();
        var i, element;
        for (i = this._itemElements.length - 1; i >= 0; i--) {
            element = this._itemElements[i];
            if (element) {
                Inflectra.SpiraTest.Web.ServerControls.ITemplate.disposeInstance(element);
            }
        }
        this._itemElements = [];
        for (i = this._separatorElements.length - 1; i >= 0; i--) {
            element = this._separatorElements[i];
            if (element) {
                Inflectra.SpiraTest.Web.ServerControls.ITemplate.disposeInstance(element);
            }
        }
        this._separatorElements = [];

        if (associatedElement.childNodes.length) {
            if (this._layoutTemplateElement) {
                Inflectra.SpiraTest.Web.ServerControls.ITemplate.disposeInstance(this._layoutTemplateElement);
            }
        }
        associatedElement.innerHTML = '';

        var tasksPending = false;

        var items = this.get_data();
        var itemLength = items ? (items.get_length ? items.get_length() : items.length) : (0);
        //var itemLength = items ? (items.length? items.length : items.get_length()) : 0;
        if (itemLength && itemLength > 0) {
            var template = this.get_layoutTemplate();
            if (template) {
                var itemTemplate = this.get_itemTemplate();
                var separatorTemplate = this.get_separatorTemplate();

                var layoutTemplateInstance = template.createInstance(associatedElement, null, this.findItemTemplateParentCallback, this._itemTemplateParentElementId);
                var itemTemplateParent = layoutTemplateInstance.callbackResult;
                this._layoutTemplateElement = layoutTemplateInstance.instanceElement;

                tasksPending = true;
                this._pendingTasks++;
                var renderTask = new Inflectra.SpiraTest.Web.ServerControls.Data.ListViewRenderTask(this, items, itemTemplate, itemTemplateParent, separatorTemplate, this._itemElements, this._separatorElements, this._itemClass, this._alternatingItemClass, this._separatorClass, this._itemFocusHandler, this._itemClickHandler);
                Inflectra.SpiraTest.Web.AjaxExtensions.TaskManager.addTask(renderTask);
            }
        }
        else {
            var emptyTemplate = this.get_emptyTemplate();
            if (emptyTemplate) {
                emptyTemplate.createInstance(associatedElement);
            }
            var handler = this.get_events().getHandler('renderComplete');
            if(handler) handler(this, Sys.EventArgs.Empty);            
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_renderTaskComplete(renderTask) {
        this._pendingTasks--;
        if(this._pendingTasks <= 0) {
            this._pendingTasks = 0;
            var handler = this.get_events().getHandler('renderComplete');
            if(handler) handler(this, Sys.EventArgs.Empty);
        }  
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$findItemTemplateParentCallback(instanceElement, markupContext, id) {
        // <param name="instanceElement">Instance element.</param>
        // <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext">Markup context.</param>
        // <param name="id" type="String">Element id.</param>
        // <returns>Dom element object.</returns>
        return markupContext.findElement(id);
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$setFocus(owner, element) {
        // <param name="owner">Owner element.</param>
        // <param name="element">Element for focus.</param>
        if (element.focus) {
            for(var i = owner.get_length() - 1; i >= 0; i--) {
                var sel = owner.getItemElement(i);
                if (sel) {
                    sel.tabIndex = -1;
                }
            }
            var ownerElement = owner.get_element();
            var t = ownerElement.tabIndex;
            if (t === -1) {
                t = ownerElement.__tabIndex;
            }
            element.tabIndex = t;
            setTimeout(Function.createCallback(this.focus, element), 0);
            ownerElement.__tabIndex = t;
            ownerElement.tabIndex = -1;
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$ListView$focus(element) {
        // <param name="element">Element for focus.</param>
        try {
            element.focus();
        }
        catch(e) {}
    }
Inflectra.SpiraTest.Web.ServerControls.Data.ListView.prototype = {
    _itemClass: null,
    _alternatingItemClass: null,
    _separatorClass: null,
    _selectedItemClass: null,

    _focusHandler: null,
    _keyDownHandler: null,
    _itemFocusHandler: null,
    _itemClickHandler: null,

    _focusIndex: null,

    // Template fields
    _layoutTemplate: null,
    _itemTemplate: null,
    _separatorTemplate: null,
    _emptyTemplate: null,
    _itemTemplateParentElementId: null,

    _layoutTemplateElement: null,
    _pendingTasks: 0,

    get_alternatingItemCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_alternatingItemCssClass,
    set_alternatingItemCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_alternatingItemCssClass,

    set_dataIndex: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_dataIndex,

    get_emptyTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_emptyTemplate,
    set_emptyTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_emptyTemplate,

    get_itemCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_itemCssClass,
    set_itemCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_itemCssClass,

    get_itemTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_itemTemplate,
    set_itemTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_itemTemplate,

    get_itemTemplateParentElementId: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_itemTemplateParentElementId,
    set_itemTemplateParentElementId: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_itemTemplateParentElementId,

    get_layoutTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_layoutTemplate,
    set_layoutTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_layoutTemplate,

    get_selectedItemCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_selectedItemCssClass,
    set_selectedItemCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_selectedItemCssClass,

    get_separatorCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_separatorCssClass,
    set_separatorCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_separatorCssClass,

    get_separatorTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$get_separatorTemplate,
    set_separatorTemplate: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$set_separatorTemplate,

    getItemElement: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$getItemElement,
    
    add_renderComplete: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$add_renderComplete,
    remove_renderComplete: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$remove_renderComplete,

    initialize: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$initialize,

    dispose: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$dispose,

    _onGotFocus: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onGotFocus,

    _onKeyDown: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onKeyDown,

    _onItemFocus: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onItemFocus,

    _onItemClick: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_onItemClick,

    render: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$render,
    
    _renderTaskComplete: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$_renderTaskComplete,

    findItemTemplateParentCallback: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$findItemTemplateParentCallback,

    setFocus: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$setFocus,

    focus: Inflectra$SpiraTest$Web$ServerControls$Data$ListView$focus
}
Inflectra.SpiraTest.Web.ServerControls.Data.ListView.descriptor = {
    properties: [ { name: 'alternatingItemCssClass', type: String },
                  { name: 'layoutTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate },
                  { name: 'itemCssClass', type: String },
                  { name: 'itemTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate },
                  { name: 'itemTemplateParentElementId', type: String },
                  { name: 'selectedItemCssClass', type: String },
                  { name: 'separatorCssClass', type: String },
                  { name: 'separatorTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate },
                  { name: 'emptyTemplate', type: Inflectra.SpiraTest.Web.ServerControls.ITemplate } ],
    events: [ {name: 'renderComplete'} ]
}
Inflectra.SpiraTest.Web.ServerControls.Data.ListView.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.ListView', Inflectra.SpiraTest.Web.ServerControls.Data.DataControl);

Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior = function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior(element) {
    // <param name="element" domElement="true">The DOM element the behavior is associated with.</param>
    Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior.initializeBase(this,[element]);
}








    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_sortAscendingCssClass() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Sort ascending css class.</value>
        return this._sortAscendingCssClass;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_sortAscendingCssClass(value) {
        this._sortAscendingCssClass = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_sortColumn() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Sort column.</value>
        return this._sortColumn;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_sortColumn(value) {
        if (value !== this._sortColumn) {
            this._sortColumn = value;
            this.raisePropertyChanged('sortColumn');
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_sortDescendingCssClass() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value type="String">Sort descending css class.</value>
        return this._sortDescendingCssClass;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_sortDescendingCssClass(value) {
        this._sortDescendingCssClass = value;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_dataView() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Data view.</value>
        return this._dataView;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_dataView(value) {
        if (!Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.isInstanceOfType(value)) throw Error.createError("dataView must be of type Inflectra.SpiraTest.Web.AjaxExtensions.Data.DataView.");
        if (this._dataView && this._sortChangedDelegate) {
            this._dataView.remove_propertyChanged(this._sortChangedDelegate);
        }
        this._dataView = value;
        if (this.get_isInitialized()) {
            this._dataView.add_propertyChanged(this._sortChangedDelegate);
            this.update();
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$dispose() {
        if (this._dataView && !this._dataView._disposed && this._sortChangedDelegate) {
            this._dataView.remove_propertyChanged(this._sortChangedDelegate);
            this._sortChangedDelegate = null;
        }
        this._dataView = null;
        if (this._clickHandler) {
            Sys.UI.DomEvent.removeHandler(this.get_element(), "click", this._clickHandler);
            this._clickHandler = null;
        }
        Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior.callBaseMethod(this, 'initialize');
        this._clickHandler = Function.createDelegate(this, this.clickHandler);
        Sys.UI.DomEvent.addHandler(this.get_element(), "click", this._clickHandler);
        this._sortChangedDelegate = Function.createDelegate(this, this.sortChanged);
        if (this._dataView) {
            this._dataView.add_propertyChanged(this._sortChangedDelegate);
            this.update();
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$clickHandler() {
        var view = this.get_dataView();
        if (view) {
            if (view.get_sortColumn() === this._sortColumn) {
                view.set_sortDirection(
                    (view.get_sortDirection() === Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending) ?
                    Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Descending :
                    Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending);
            }
            else {
                view.sort(this._sortColumn, Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending);
            }
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$update() {
        var element = this.get_element();
        if (this._dataView && (this._dataView.get_sortColumn() === this._sortColumn)) {
            if (this._dataView.get_sortDirection() === Inflectra.SpiraTest.Web.AjaxExtensions.Data.SortDirection.Ascending) {
                Sys.UI.DomElement.removeCssClass(element, this._sortDescendingCssClass);
                Sys.UI.DomElement.addCssClass(element, this._sortAscendingCssClass);
            }
            else {
                Sys.UI.DomElement.removeCssClass(element, this._sortAscendingCssClass);
                Sys.UI.DomElement.addCssClass(element, this._sortDescendingCssClass);
            }
        }
        else {
            Sys.UI.DomElement.removeCssClass(element, this._sortAscendingCssClass);
            Sys.UI.DomElement.removeCssClass(element, this._sortDescendingCssClass);
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$sortChanged(sender, args) {
        // <param name="sender">Event sender</param>
        // <param name="args">Event arguments</param>
        var pName = args.get_propertyName();
        if ((pName === 'sortColumn') || (pName === 'sortDirection')) {
            this.update();
        }
    }
Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior.prototype = {
    _clickHandler: null,
    _sortChangedDelegate: null,
    _sortColumn: '',
    _sortAscendingCssClass: 'sortAscending',
    _sortDescendingCssClass: 'sortDescending',
    _dataView: null,
    
    get_sortAscendingCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_sortAscendingCssClass,
    set_sortAscendingCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_sortAscendingCssClass,
    
    get_sortColumn: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_sortColumn,
    set_sortColumn: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_sortColumn,
    
    get_sortDescendingCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_sortDescendingCssClass,
    set_sortDescendingCssClass: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_sortDescendingCssClass,
    
    get_dataView: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$get_dataView,
    set_dataView: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$set_dataView,
    
    dispose: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$dispose,

    initialize: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$initialize,
    
    clickHandler: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$clickHandler,
    
    update: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$update,
    
    sortChanged: Inflectra$SpiraTest$Web$ServerControls$Data$SortBehavior$sortChanged
}
Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior.descriptor = {
    properties: [ { name: 'dataView', type: Object },
                  { name: 'sortAscendingCssClass', type: String },
                  { name: 'sortColumn', type: String },
                  { name: 'sortDescendingCssClass', type: String } ]
}
Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.SortBehavior', Sys.UI.Behavior);
Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView = function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView(associatedElement) {
    // <param name="associatedElement">Associated dom element.</param>
    Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView.initializeBase(this, [associatedElement]);
}





    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$get_document() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Document object.</value>
        return this._document;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$set_document(document) {
        // <param name="document" mayBeNull="true">Document object.</param>
        this._document = document;
        if (this.get_isInitialized()) {
            this._render();
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$get_parameters() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value>Parameters object.</value>
        if (!this._parameters) {
            this._parameters = { };
        }
        return this._parameters;
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$get_transform() {
        if (arguments.length !== 0) throw Error.parameterCount();
        // <value mayBeNull="true">Transform object.</value>
        return this._transform;
    }
    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$set_transform(transform) {
        // <param name="transform" mayBeNull="true">Transform object.</param>
        this._transform = transform;
        if (this.get_isInitialized()) {
            this._render();
        }
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$dispose() {
        this._document = null;
        this._transform = null;

        Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView.callBaseMethod(this, 'dispose');
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$initialize() {
        Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView.callBaseMethod(this, 'initialize');
        this._render();
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$update() {
        this._render();
    }

    function Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$_render() {
        var html = '';
        
        if (this._document && this._transform) {
            if (this._parameters) {
                if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
                    this._transform.setProperty('SelectionNamespaces', 'xmlns:xsl="http://www.w3.org/1999/XSL/Transform"');
                }
                for (var paramName in this._parameters) {
                    var paramNode = this._transform.selectSingleNode('//xsl:param[@name="' + paramName + '"]');
                    if (paramNode) {
                        paramNode.text = this._parameters[paramName].toString();
                        paramNode.removeAttribute('select');
                    }
                }
            }
            html = this._document.transformNode(this._transform);
        }
        
        this.get_element().innerHTML = html;
    }
Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView.prototype = {
    _document: null,
    _transform: null,
    _parameters: null,

    get_document: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$get_document,
    set_document: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$set_document,
    
    get_parameters: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$get_parameters,
    
    get_transform: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$get_transform,
    set_transform: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$set_transform,

    dispose: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$dispose,
    
    initialize: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$initialize,
    
    update: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$update,

    _render: Inflectra$SpiraTest$Web$ServerControls$Data$XSLTView$_render    
}

Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView.descriptor = {
    properties: [ { name: 'document', type: Object },
                  { name: 'parameters', type: Object, readOnly: true },
                  { name: 'transform', type: Object } ],
    methods: [ { name: 'update' } ]
}
Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView.registerClass('Inflectra.SpiraTest.Web.ServerControls.Data.XSLTView', Sys.UI.Control);

Sys.Component.descriptor = {
    properties: [ {name: 'dataContext', type: Object},
                  {name: 'id', type: String},
                  {name: 'isInitialized', type: Boolean, readOnly: true},
                  {name: 'isUpdating', type: Boolean, readOnly: true} ],
    events: [ {name: 'propertyChanged'} ]
}

Sys.UI.Control.descriptor = {
    properties: [ {name: 'element', type: Object, readOnly: true},
                  {name: 'role', type: String, readOnly: true},
                  {name: 'parent', type: Object},
                  {name: 'visible', type: Boolean},
                  {name: 'visibilityMode', type: Sys.UI.VisibilityMode} ],
    methods:    [ {name: 'addCssClass', parameters: [ {name: 'className', type: String} ] },
                  {name: 'removeCssClass', parameters: [ {name: 'className', type: String} ] },
                  {name: 'toggleCssClass', parameters: [ {name: 'className', type: String} ] } ]
}

Sys.UI.Behavior.descriptor = {
    properties: [ {name: 'name', type: String} ]
}

Sys.Component.parseFromMarkup = function Sys$Component$parseFromMarkup(type, node, markupContext) {
    // todo: make these triple slash ///. ScriptCop fails on them at the moment, possibly due to an exception in the rules?
    // <param name="type" type="Type">Component type to create.</param>
    // <param name="node">Element node.</param>
    // <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext">Markup context.</param>
    // <returns type="Sys.Component">Parsed result.</returns>
    var newComponent = new type();

    // markupContext contains the dataContext to be used for this component.
    // (for example as specified by a templated control when it instantiates a template representing
    // a given dataItem). This component needs to use it, but nested components within it shouldn't.
    // The nested component should inherit their dataContext from their parent by virtue of their owner
    // relationships.
    // Hence here we hide it (its a no-op if its already hidden), process the node, which in turn
    // processes the child nodes, which as a result do not see a dataContext. Finally, we pass the
    // dataContext to the newly created component here if there was dataContext present.
    var dataContextHidden = false;
    var dataContext = markupContext.get_dataContext();
    if (dataContext) {
        dataContextHidden = markupContext.hideDataContext();
    }

    var component = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(newComponent, node, markupContext);
    if (component) {
        markupContext.addComponent(component);

        if (dataContext) {
            component.set_dataContext(dataContext);
        }
    }
    else {
        newComponent.dispose();
    }

    // Restore the dataContext if we hid it, so that siblings of this component do see the data context
    if (dataContextHidden) {
        markupContext.restoreDataContext();
    }

    return component;
}

Sys.Component.createCollection = function Sys$Component$createCollection(component) {
    var collection = [];
    collection._component = component;

    var _events = null;
    collection.get_events = function collection$get_events() {
        if (!_events) {
            _events = new Sys.EventHandlerList();
        }
        return _events;
    }
    collection.add_collectionChanged = function collection$add_collectionChanged(handler) {
        this.get_events().addHandler("collectionChanged", handler);
    }
    collection.remove_collectionChanged = function collection$remove_collectionChanged(handler) {
        this.get_events().removeHandler("collectionChanged", handler);
    }
    collection._onCollectionChanged = function collection$_onCollectionChanged(args) {
        var handler = this.get_events().getHandler("collectionChanged");
        if (handler) {
            handler(this, args);
        }
    }

    collection.add = function collection$add(item) {
        Array.add(this, item);
        if(typeof(item.setOwner) === "function") {
            item.setOwner(this._component);
        }
        this._onCollectionChanged(new Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Add, item));
    }

    collection.clear = function collection$clear() {
        for (var i = this.length - 1; i >= 0; i--) {
            this[i].dispose();
            this[i] = null;
        }
        Array.clear(this);
        this._onCollectionChanged(new Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Reset, null));
    }

    collection.dispose = function collection$dispose() {
        this.clear();
        delete this._events;
        this._component = null;
        this._disposed = true;
    }

    collection.remove = function collection$remove(item) {
        item.dispose();
        Array.remove(this, item);
        this._onCollectionChanged(new Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Remove, item));
    }

    collection.removeAt = function collection$removeAt(index) {
        var item = this[index];
        item.dispose();
        Array.removeAt(this, index);
        this._onCollectionChanged(new Inflectra.SpiraTest.Web.AjaxExtensions.CollectionChangedEventArgs(Inflectra.SpiraTest.Web.AjaxExtensions.NotifyCollectionChangedAction.Remove, item));
    }

    return collection;
}

Sys.Component.createMultiple = function Sys$Component$createMultiple(elements, type, properties, events, references) {
    /// <param name="elements" type="Array" elementDomElement="true"></param>
    /// <param name="type" type="Type"></param>
    /// <param name="properties" optional="true" mayBeNull="true"></param>
    /// <param name="events" optional="true" mayBeNull="true"></param>
    /// <param name="references" optional="true" mayBeNull="true"></param>
    var e = Function._validateParams(arguments, [
        {name: "elements", type: Array},
        {name: "type", type: Type},
        {name: "properties", mayBeNull: true, optional: true},
        {name: "events", mayBeNull: true, optional: true},
        {name: "references", mayBeNull: true, optional: true}
    ]);
    if (e) throw e;

    var create = Sys.Component.create;
    for (var i = 0, l = elements.length; i < l; i++) {
        create(type, properties, events, references, elements[i]);
    }
}

Sys.UI.Control.parseFromMarkup = function Sys$UI$Control$parseFromMarkup(type, node, markupContext) {
    /// <param name="type" type="Type"></param>
    /// <param name="node"></param>
    /// <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext"></param>
    /// <returns type="Sys.UI.Control"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "node"},
        {name: "markupContext", type: Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext}
    ]);
    if (e) throw e;

    var idAttribute = node.attributes.getNamedItem('id');
    Sys.Debug.assert(!!(idAttribute && idAttribute.nodeValue.length), String.format('No associated HTML element was specified for control of type "{0}"', type.getName()));
    var id = idAttribute.nodeValue;
    var associatedElement = markupContext.findElement(id);
    Sys.Debug.assert(!!associatedElement, String.format('Could not find an HTML element with ID "{0}" for control of type "{1}"', id, type.getName()));

    var dataContextHidden = false;
    var dataContext = markupContext.get_dataContext();
    if (dataContext) {
        dataContextHidden = markupContext.hideDataContext();
    }

    // note: we do not remove the id attribute here, because nested behaviors need it during parseFromMarkup to get
    //       element they attach to. Leaving the id attribute would normally cause set_id to be called on Component, but that
    //       there's a special case for it not to.
    //  node.attributes.removeNamedItem('id');
    var newControl = new type(associatedElement);
    var control = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(newControl, node, markupContext);

    if (control) {
        //if (idAttribute) node.attributes.setNamedItem(idAttribute);
        var id = control.get_id();
        Sys.Debug.assert(!!id && !!(id.length));

        markupContext.addComponent(control);

        if (dataContext) {
            control.set_dataContext(dataContext);
        }
    }
    else {
        newControl.dispose();
    }

    if (dataContextHidden) {
        markupContext.restoreDataContext();
    }

    return control;
}

Sys.UI.Behavior.parseFromMarkup = function Sys$UI$Behavior$parseFromMarkup(type, node, markupContext) {
    /// <param name="type" type="Type"></param>
    /// <param name="node"></param>
    /// <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext"></param>
    /// <returns type="Sys.UI.Behavior"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "node"},
        {name: "markupContext", type: Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext}
    ]);
    if (e) throw e;

    var associatedElement;
    var id;
    var elementAttribute = node.attributes.getNamedItem('elementID');

    if(!elementAttribute) {
        // no elementID specified, but perhaps this node is nested within a control
        var parentNode = node.parentNode;
        if(parentNode) { // behaviors
            parentNode = parentNode.parentNode; // <control>
            if(parentNode && parentNode.attributes) {
                // control requires id attribute
                var idAttribute = parentNode.attributes.getNamedItem('id');
                if(idAttribute) {
                    id = idAttribute.nodeValue;
                    associatedElement = markupContext.findElement(id);
                }
            }
        }
        Sys.Debug.assert(!!associatedElement, String.format('No associated HTML element was specified for behavior of type "{0}"', type.getName()));
    }
    else {
        Sys.Debug.assert(!!elementAttribute.nodeValue.length, String.format('No associated HTML element was specified for behavior of type "{0}"', type.getName()));
        if(elementAttribute.nodeValue.length) {
            id = elementAttribute.nodeValue;
            associatedElement = markupContext.findElement(id);
            Sys.Debug.assert(!!associatedElement, String.format('Could not find an HTML element with ID "{0}" for behavior of type "{1}"', id, type.getName()));
        }
         // remove it so set_elementID is not used
        node.attributes.removeNamedItem('elementID');
    }

    var newBehavior = new type(associatedElement);
    var behavior = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(newBehavior, node, markupContext);

    if (behavior) {
        // put back removed elementID attribute
        if (elementAttribute) {
            node.attributes.setNamedItem(elementAttribute);
        }

        markupContext.addComponent(behavior);
    }
    else {
        newBehavior.dispose();
    }

    return behavior;
}

// contains function for non-IE browsers
Sys.UI.DomElement._contains = function Sys$UI$DomElement$_contains(root, element) {
    while (element) {
        element = element.parentNode;
        if (element === root) return true;
    }
    return false;
}

// Tests a CSS selector term on an element
Sys.UI.DomElement._testTerm = function Sys$UI$DomElement$_testTerm(term, element) {
    return (!term.id || element.id === term.id) &&
        (!term.tagName || element.tagName.toLowerCase() === term.tagName) &&
        ((term.className === '  ') || (' ' + element.className + ' ').indexOf(term.className) !== -1);
}

Sys.UI.DomElement.getElementsByClassName = function Sys$UI$DomElement$getElementsByClassName(className, element) {
    /// <param name="className" type="String"></param>
    /// <param name="element" domElement="true" optional="true"></param>
    /// <returns type="Array" elementDomElement="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "className", type: String},
        {name: "element", domElement: true, optional: true}
    ]);
    if (e) throw e;

    element = element || document;
    className = ' ' + className + ' ';
    var potentials = element.all || element.getElementsByTagName("*");
    var l = potentials.length, results = [], i;
    for (i = 0; i < l; i++) {
        if ((' ' + potentials[i].className + ' ').indexOf(className) !== -1) {
            results[results.length] = potentials[i];
        }
    }
    return results;
}

Sys.UI.DomElement.selectAllElements = function Sys$UI$DomElement$selectAllElements(selector, element) {
    /// <param name="selector" type="String"></param>
    /// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
    /// <returns type="Array" elementDomElement="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "selector", type: String},
        {name: "element", mayBeNull: true, domElement: true, optional: true}
    ]);
    if (e) throw e;


    var cssSelectorExpression = /([^\.#]*)\.?([^#]*)#?(.*)/,
        terms = selector.trim().split(/\s+/),
        root = element || document,
        d = root.body ? root : root.documentElement;
    var l = terms.length;
    if (l === 0) return [];

    // Process the terms to extract tag, class and id information
    for (var i = 0; i < l; i++) {
        terms[i].search(cssSelectorExpression);
        terms[i] = {
            tagName: RegExp.$1.toLowerCase(),
            className: RegExp.$2.toLowerCase(),
            id: RegExp.$3
        };
    }
    // Process the first rule
    var term = terms[0],
        potentials = [],
        nextPotentials = [];
    if (term.id) {
        var elt = d.getElementById(term.id);
        if (elt && 
            ( (root === d) || 
              (root.contains && root.contains(elt)) || 
              this._contains(root, elt) ) ) {
            potentials = [elt];
        }
    }
    else if (term.tagName) {
        potentials = root.getElementsByTagName(term.tagName);
    }
    else if (term.className) {
        potentials = this.getElementsByClassName(term.className, root);
    }
    term.className = ' ' + term.className + ' ';
    // get potential elements for each rule after the first
    for (i = 1; i < l; i++) {
        var m = potentials.length;
        if (m === 0) return [];
        var previousTerm = term;
        term = terms[i];
        term.className = ' ' + term.className + ' ';
        for (var j = 0; j < m; j++) {
            var potential = potentials[j];
            if (!this._testTerm(previousTerm, potential)) continue;
            // Find potential elements for this rule and parent
            if (term.id) {
                elt = d.getElementById(term.id);
                if (elt && 
                    ( (potential === d) ||
                      (potential.contains && potential.contains(elt)) ||
                      this._contains(potential, elt) ) ) {
                    nextPotentials[nextPotentials.length] = elt;
                }
            }
            else if (term.tagName) {
                var elts = potential.getElementsByTagName(term.tagName);
                for (var k = 0, n = elts.length; k < n; k++) {
                    nextPotentials[nextPotentials.length] = elts[k];
                }
            }
            else {
                elts = potential.getElementsByTagName('*');
                n = elts.length;
                for (k = 0; k < n; k++) {
                    elt = elts[k];
                    if ((' ' + elt.className + ' ').indexOf(term.className) !== -1) {
                        nextPotentials[nextPotentials.length] = elt;
                    }
                }
            }
        }
        potentials = nextPotentials;
        nextPotentials = [];
    }
    // Check the last potentials against the last term and remove dupes
    m = potentials.length;
    for (j = 0; j < m; j++) {
        potential = potentials[j];
        if (this._testTerm(term, potential) && !Array.contains(nextPotentials, potential)) {
            nextPotentials[nextPotentials.length] = potential;
        }
    }
    return nextPotentials;
}

Sys.UI.DomElement.selectElement = function Sys$UI$DomElement$selectElement(selector, element) {
    /// <param name="selector" type="String"></param>
    /// <param name="element" domElement="true" optional="true" mayBeNull="true"></param>
    /// <returns domElement="true" mayBeNull="true"></returns>
    var e = Function._validateParams(arguments, [
        {name: "selector", type: String},
        {name: "element", mayBeNull: true, domElement: true, optional: true}
    ]);
    if (e) throw e;

    var potentials = Sys.UI.DomElement.selectAllElements(selector, element);
    return (potentials.length > 0) ? potentials[0] : null;
}

function $object(id, context) {
    return Sys.Application.findComponent(id, context);
}

Sys._Application.descriptor = {
    events: [ {name: 'init'}, {name: 'load'}, {name: 'unload'} ]
}

Sys._Application.parseFromMarkup = function Sys$_Application$parseFromMarkup(type, node, markupContext) {
    /// <param name="type" type="Type"></param>
    /// <param name="node"></param>
    /// <param name="markupContext" type="Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext"></param>
    /// <returns type="Sys._Application"></returns>
    var e = Function._validateParams(arguments, [
        {name: "type", type: Type},
        {name: "node"},
        {name: "markupContext", type: Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext}
    ]);
    if (e) throw e;

    if (!markupContext.get_isGlobal()) {
        return null;
    }

    var id = null;
    var idAttribute = node.attributes.getNamedItem('id');
    if (idAttribute) {
        id = idAttribute.nodeValue;
        node.attributes.removeNamedItem('id');
    }

    Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.initializeObject(Sys.Application, node, markupContext);
    if (idAttribute) {
        node.attributes.setNamedItem(idAttribute);
    }
    if (id && (markupContext.findComponent(id) !== Sys.Application)) {
        markupContext._addComponentByID(id, Sys.Application, true);
    }

    return Sys.Application;
}

Sys.Application.getMarkupContext = function Sys$Application$getMarkupContext() {
    return this._markupContext;
}

Sys.Application.__initHandler = function Sys$Application$__initHandler() {
    var a = Sys.Application;
	a.remove_init(Sys.Application.__initHandler);
	// process xml scripts in the document
	Inflectra.SpiraTest.Web.AjaxExtensions.MarkupParser.processDocument(a._markupContext);
}
Sys.Application.__unloadHandler = function Sys$Application$__unloadHandler() {
    var a = Sys.Application;
    a.remove_unload(a.__unloadHandler);
    if(a._markupContext) {
        a._markupContext.dispose();
        a._markupContext = null;
    }
}

if(!Sys.Application._markupContext) {
    Sys.Application._markupContext = Inflectra.SpiraTest.Web.AjaxExtensions.MarkupContext.createGlobalContext();
    Sys.Application.add_init(Sys.Application.__initHandler);
    Sys.Application.add_unload(Sys.Application.__unloadHandler);
}
// This file contains Orcas changes to the Ajax Library that we need in Futures CTP so that they can run indifferently
// on Ajax 1.0 and Orcas.
// This file should disappear from the Futures when we ship Orcas.

// Using a regex that's defined only in Orcas to only redefine when running on 1.0
if (!Sys.Serialization.JavaScriptSerializer._dateRegEx) {
    Sys.Serialization.JavaScriptSerializer._dateRegEx = new RegExp('(^|[^\\\\])\\"\\\\/Date\\((-?[0-9]+)\\)\\\\/\\"', 'g');
    Sys.Serialization.JavaScriptSerializer._jsonRegEx = new RegExp('[^,:{}\\[\\]0-9.\\-+Eaeflnr-u \\n\\r\\t]', 'g');
    Sys.Serialization.JavaScriptSerializer._jsonStringRegEx = new RegExp('"(\\\\.|[^"\\\\])*"', 'g');
    
    Sys.Serialization.JavaScriptSerializer.deserialize = function Sys$Serialization$JavaScriptSerializer$deserialize(data, secure) {
        /// <param name="data" type="String"></param>
        /// <param name="secure" type="Boolean" optional="true"></param>
        /// <returns></returns>
        var e = Function._validateParams(arguments, [
            {name: "data", type: String},
            {name: "secure", type: Boolean, optional: true}
        ]);
        if (e) throw e;

        
        if (data.length === 0) throw Error.argument('data', Sys.Res.cannotDeserializeEmptyString);
        try {    
            var exp = data.replace(Sys.Serialization.JavaScriptSerializer._dateRegEx, "$1new Date($2)");
            
            if (secure && Sys.Serialization.JavaScriptSerializer._jsonRegEx.test(
                 exp.replace(Sys.Serialization.JavaScriptSerializer._jsonStringRegEx, ''))) throw null;

            return eval('(' + exp + ')');
        }
        catch (e) {
             throw Error.argument('data', Sys.Res.cannotDeserializeInvalidJson);
        }
    }
}

if (!Sys.UI.DomElement.getVisible) {
    Sys.UI.DomElement.getVisible = function Sys$UI$DomElement$getVisible(element) {
        /// <param name="element" domElement="true"></param>
        /// <returns type="Boolean"></returns>
        var e = Function._validateParams(arguments, [
            {name: "element", domElement: true}
        ]);
        if (e) throw e;

        return (element.style.visibility !== 'hidden');
    }
}

if (!Sys.UI.DomElement.setVisible) {
    Sys.UI.DomElement.setVisible = function Sys$UI$DomElement$setVisible(element, value) {
        /// <param name="element" domElement="true"></param>
        /// <param name="value" type="Boolean"></param>
        var e = Function._validateParams(arguments, [
            {name: "element", domElement: true},
            {name: "value", type: Boolean}
        ]);
        if (e) throw e;

        if (value !== Sys.UI.DomElement.getVisible(element)) {
            element.style.visibility = value ? 'visible' : 'hidden';
            if (value) {
                if (element.style.display === "none") {
                    element.style.display = (element._display? element._display : "inline");
                    }
                }
                else {
                element._display = element.style.display;
                element.style.display = "none";
            }
        }
    }
}
Inflectra.SpiraTest.Web.AjaxExtensions.HistoryEventArgs = function Inflectra$SpiraTest$Web$AjaxExtensions$HistoryEventArgs(state) {
    /// <param name="state" type="Object"></param>
    var e = Function._validateParams(arguments, [
        {name: "state", type: Object}
    ]);
    if (e) throw e;

    Inflectra.SpiraTest.Web.AjaxExtensions.HistoryEventArgs.initializeBase(this);
    this._state = state;
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$HistoryEventArgs$get_state() {
        /// <value type="Object"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
        return this._state;
    }
Inflectra.SpiraTest.Web.AjaxExtensions.HistoryEventArgs.prototype = {
    get_state: Inflectra$SpiraTest$Web$AjaxExtensions$HistoryEventArgs$get_state
}
Inflectra.SpiraTest.Web.AjaxExtensions.HistoryEventArgs.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions.HistoryEventArgs', Sys.EventArgs);
// TODO: when this is integrated in the main ASP.NET branch, PageRequestManager
// should be modified to allow for delayed title setting so that the history manager
// can set the hash before the title is changed from the information the server sent back.

Inflectra.SpiraTest.Web.AjaxExtensions._History = function Inflectra$SpiraTest$Web$AjaxExtensions$_History() {
    Inflectra.SpiraTest.Web.AjaxExtensions._History.initializeBase(this);
    
    this._appLoadHandler = null;
    this._beginRequestHandler = null;
    this._clientId = null;
    this._currentEntry = '';
    this._emptyPageUrl = null;
    this._endRequestHandler = null;
    this._history = null;
    this._historyFrame = null;
    this._historyInitialLength = 0;
    this._historyLength = 0;
    this._iframeLoadHandler = null;
    this._ignoreIFrame = false;
    this._ignoreTimer = false;
    this._historyPointIsNew = false;
    this._state = {};
    this._timerCookie = 0;
    this._timerHandler = null;
    this._uniqueId = null;
}

    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$get_stateString() {
        /// <value type="String"></value>
        if (arguments.length !== 0) throw Error.parameterCount();
//        if (Sys.Browser.agent === Sys.Browser.Safari && this._history) Sys.Debug.trace("get state " + this._history[window.history.length - this._historyInitialLength]);
        var hash = (Sys.Browser.agent === Sys.Browser.Safari && this._history) ?
            this._history[window.history.length - this._historyInitialLength]:
            window.location.hash;
        var entry = decodeURIComponent(hash || '');
        if ((entry.length > 0) && (entry.charAt(0) === '#')) {
            entry = entry.substring(1);
        }
        return entry;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$add_navigate(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().addHandler("navigate", handler);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$remove_navigate(handler) {
        var e = Function._validateParams(arguments, [{name: "handler", type: Function}]);
        if (e) throw e;

        this.get_events().removeHandler("navigate", handler);
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$addHistoryPoint(state, title) {
        /// <param name="state" type="Object"></param>
        /// <param name="title" type="String" optional="true" mayBeNull="true"></param>
        var e = Function._validateParams(arguments, [
            {name: "state", type: Object},
            {name: "title", type: String, mayBeNull: true, optional: true}
        ]);
        if (e) throw e;

//        Sys.Debug.traceDump(state, "addHistoryPoint");

        var initialState = this._state;
        for (var key in state) {
            var value = state[key];
            if (value === null) {
                if (typeof(initialState[key]) !== 'undefined') {
                    delete initialState[key];
                }
            }
            else {
                initialState[key] = value;
            }
        }
        var entry = Sys.Serialization.JavaScriptSerializer.serialize(initialState);
        this._ignoreIFrame = true;
        this._historyPointIsNew = true;
        this._setState(entry, title);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$dispose() {
        if (this._appLoadHandler) {
            Sys.Application.remove_load(this._appLoadHandler);
            delete this._appLoadHandler;
        }
        if (this._historyFrame) {
            Sys.UI.DomEvent.removeHandler(this._historyFrame, 'load', this._iframeLoadHandler);
            delete this._iframeLoadHandler;
            delete this._historyFrame;
        }
        if (this._timerCookie) {
            window.clearTimeout(this._timerCookie);
            delete this._timerCookie;
        }
        if (this._endRequestHandler) {
            Sys.WebForms.PageRequestManager.getInstance().remove_endRequest(this._endRequestHandler);
            delete this._endRequestHandler;
        }
        if (this._beginRequestHandler) {
            Sys.WebForms.PageRequestManager.getInstance().remove_beginRequest(this._beginRequestHandler);
            delete this._beginRequestHandler;
        }
        Inflectra.SpiraTest.Web.AjaxExtensions._History.callBaseMethod(this, 'dispose');
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$initialize() {
        Inflectra.SpiraTest.Web.AjaxExtensions._History.callBaseMethod(this, 'initialize');
        
        this._appLoadHandler = Function.createDelegate(this, this._onApplicationLoaded);
        Sys.Application.add_load(this._appLoadHandler);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$setServerId(clientId, uniqueId) {
        /// <param name="clientId" type="String"></param>
        /// <param name="uniqueId" type="String"></param>
        var e = Function._validateParams(arguments, [
            {name: "clientId", type: String},
            {name: "uniqueId", type: String}
        ]);
        if (e) throw e;

        this._clientId = clientId;
        this._uniqueId = uniqueId;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$setServerState(value) {
        this._state.__s = value;
    }

    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_navigate(entry) {
//        Sys.Debug.trace("navigate: " + entry);

        var state = {};
        if (entry) {
            try {
                state = Sys.Serialization.JavaScriptSerializer.deserialize(entry, true);
            } catch(e) {
//                Sys.Debug.trace("Deserialization exception: " + e);
            }
        }
        
        if (this._uniqueId) {
            var oldServerEntry = this._state.__s || '';
            var newServerEntry = state.__s || '';
            if (newServerEntry !== oldServerEntry) {
                __doPostBack(this._uniqueId, newServerEntry);
                this._state = state;
                return;
            }
        }
        this._setState(entry);
        this._state = state;
        this._raiseNavigate();
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onApplicationLoaded(sender, args) {
        Sys.Application.remove_load(this._appLoadHandler);
        delete this._appLoadHandler;

        if (Sys.WebForms) {
            // Create an invisible element fot the history control to work around DevDiv 80942
            // Remove when this code is tied to Orcas
            var elt = document.createElement('DIV');
            elt.id = this._clientId;
            elt.style.display = 'none';
            document.body.appendChild(elt);
            // Subscribe to begin and end request events
            this._beginRequestHandler = Function.createDelegate(this, this._onPageRequestManagerBeginRequest);
            Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(this._beginRequestHandler);
            this._endRequestHandler = Function.createDelegate(this, this._onPageRequestManagerEndRequest);
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(this._endRequestHandler);
        }
        
        if (Sys.Browser.agent === Sys.Browser.InternetExplorer) {
            
            var frameId = '__historyFrame';
            var frame = document.getElementById(frameId);
            if (!frame) throw Error.invalidOperation("For the history feature to work in IE, the page must have an iframe with id '__historyFrame' and src set to point to a page that sets its title to the 'title' querystring parameter when loaded.");
            var src = frame.src;
            this._emptyPageUrl = src + (src.indexOf('?') === -1 ? '?' : '&') + '_state=';
            this._historyFrame = frame;
//            Sys.Debug.trace("Frame ready state: " + frame.readyState);
            // Fixing the issue where the iframe finishes loading after the page initializes
            if (frame.readyState === 'loading') {
                this._ignoreIFrame = true;
            }
//            Sys.Debug.trace("Found frame " + src);
            
            this._iframeLoadHandler = Function.createDelegate(this, this._onIFrameLoad);
            Sys.UI.DomEvent.addHandler(this._historyFrame, 'load', this._iframeLoadHandler);
        }
        if (Sys.Browser.agent === Sys.Browser.Safari) {
            this._history = [window.location.hash];
            this._historyInitialLength = window.history.length;
        }
        
        this._timerHandler = Function.createDelegate(this, this._onIdle);
        this._timerCookie = window.setTimeout(this._timerHandler, 100);
        
        var loadedEntry = this.get_stateString();
        if (loadedEntry !== this._currentEntry) {
            this._navigate(loadedEntry);
        }
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onIdle() {
        delete this._timerCookie;
        
        var entry = this.get_stateString();
        if (entry !== this._currentEntry) {
            if (!this._ignoreTimer) {
//                Sys.Debug.trace("idle change from " + this._currentEntry + " to " + entry + " history length: " + history.length);
                this._historyPointIsNew = false;
                this._navigate(entry);
                this._historyLength = window.history.length;
            }
//            else {
//                Sys.Debug.trace("idle change timer ignored.");
//            }
        }
        else {
            this._ignoreTimer = false;
        }
        this._timerCookie = window.setTimeout(this._timerHandler, 100);
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onIFrameLoad() {
//        Sys.Debug.trace("onIFrameLoad");
        if (!this._ignoreIFrame) {
            var entry = this._historyFrame.contentWindow.location.search;
            var statePos = entry.indexOf('_state=');
            if ((statePos !== -1) && (statePos + 7 < entry.length)) {
                entry = entry.substring(statePos + 7);
                var next = entry.indexOf('&');
                if (next !== -1) {
                    entry = entry.substring(0, next);
                }
            }
            else {
                entry = '';
            }
//            Sys.Debug.trace("iframe load: " + entry);
            this._historyPointIsNew = false;
            this._navigate(entry);
        }
        this._ignoreIFrame = false;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onPageRequestManagerBeginRequest(sender, args) {
        this._ignoreTimer = true;
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onPageRequestManagerEndRequest(sender, args) {
        var dataItem = args.get_dataItems()[this._clientId], title;

//        Sys.Debug.traceDump(dataItem, "end request");

        if (typeof(dataItem) !== 'undefined') {
            var state = dataItem[0];
            title = dataItem[1];
            this.setServerState(state);
            this._historyPointIsNew = true;
        }
        else {
            this._ignoreTimer = false;
        }
        var entry = Sys.Serialization.JavaScriptSerializer.serialize(this._state);
        if (entry === '{}') {
            entry = '';
        }
        if (entry != this._currentEntry) {
//            Sys.Debug.trace("Server changed entry from " + this._currentEntry + " to " + entry);
            this._ignoreTimer = true;
            this._setState(entry, title);
            this._raiseNavigate();
        }
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_raiseNavigate() {
//        Sys.Debug.traceDump(this._state, "raise navigate: " + Sys.Serialization.JavaScriptSerializer.serialize(this._state));

        var h = this.get_events().getHandler("navigate");
        var args = new Inflectra.SpiraTest.Web.AjaxExtensions.HistoryEventArgs(this._state);
        if (h) {
            h(this, args);
        }
        if (window.pageNavigate) {
            window.pageNavigate(this, args);
        }
    }
    function Inflectra$SpiraTest$Web$AjaxExtensions$_History$_setState(entry, title) {
        if (entry !== this._currentEntry) {
            if (this._historyFrame && this._historyPointIsNew) {
                var newFrameUrl = this._emptyPageUrl + entry +
                    '&title=' + encodeURIComponent(title || document.title);
                // TODO: try to write to the document frame instead of creating a new server roundtrip.
                if (this._historyFrame.src != newFrameUrl) {
//                    Sys.Debug.trace("Navigating frame from: " + this._historyFrame.src.substring(this._emptyPageUrl.length) + " to: " + entry);
                    this._ignoreIFrame = true;
                    this._historyFrame.src = newFrameUrl;
                }
                this._historyPointIsNew = false;
            }
            this._ignoreTimer = false;
//            Sys.Debug.trace("Replacing entry from: " + this._currentEntry + " to: " + entry);
            this._currentEntry = entry;
            var currentHash = this.get_stateString();
            if (currentHash === '{}') {
                currentHash = '';
                this._currentEntry = null;
            }
            if (entry !== currentHash) {
//                Sys.Debug.trace("Replacing hash from: " + currentHash + " to: " + entry);
                var encodedEntry = entry ? encodeURIComponent(entry) : '';
                if (Sys.Browser.agent === Sys.Browser.Safari) {
                    // Safari doesn't update the location.hash when the user hits back.
                    // What it does update is history.length (goes down one on back, up
                    // one on forward) so we store all states in an array to work around
                    // the bug and manage our own history stack.
                    // Known issue: navigating away from the page and coming back clears
                    // the history stack.
                    this._history[window.history.length - this._historyInitialLength + 1] = entry;
                    this._historyLength = window.history.length + 1;
                    // In some cases, setting location.hash does not create a new entry in the history
                    // but replaces the current one. Need to "post" a form using the get verb instead.
                    var form = document.createElement('FORM');
                    form.method = 'get';
                    form.action = '#' + encodedEntry;
                    document.appendChild(form);
                    form.submit();
                    document.removeChild(form);
                }
                else {
                    window.location.hash = encodedEntry;
                }
//                Sys.Debug.trace("history length: " + history.length + " current entry: " + this._currentEntry + " state string: " + this.get_stateString());
                if ((typeof(title) !== 'undefined') && (title !== null)) {
                    document.title = title;
                }
            }
        }
    }
Inflectra.SpiraTest.Web.AjaxExtensions._History.prototype = {
    get_stateString: Inflectra$SpiraTest$Web$AjaxExtensions$_History$get_stateString,
    add_navigate: Inflectra$SpiraTest$Web$AjaxExtensions$_History$add_navigate,
    remove_navigate: Inflectra$SpiraTest$Web$AjaxExtensions$_History$remove_navigate,

    addHistoryPoint: Inflectra$SpiraTest$Web$AjaxExtensions$_History$addHistoryPoint,
    dispose: Inflectra$SpiraTest$Web$AjaxExtensions$_History$dispose,
    initialize: Inflectra$SpiraTest$Web$AjaxExtensions$_History$initialize,
    setServerId: Inflectra$SpiraTest$Web$AjaxExtensions$_History$setServerId,
    setServerState: Inflectra$SpiraTest$Web$AjaxExtensions$_History$setServerState,
    
    _navigate: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_navigate,
    _onApplicationLoaded: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onApplicationLoaded,
    _onIdle: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onIdle,
    _onIFrameLoad: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onIFrameLoad,
    _onPageRequestManagerBeginRequest: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onPageRequestManagerBeginRequest,
    _onPageRequestManagerEndRequest: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_onPageRequestManagerEndRequest,
    _raiseNavigate: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_raiseNavigate,
    _setState: Inflectra$SpiraTest$Web$AjaxExtensions$_History$_setState
}
Inflectra.SpiraTest.Web.AjaxExtensions._History.registerClass('Inflectra.SpiraTest.Web.AjaxExtensions._History', Sys.Component);

Sys._Application.prototype.get_history = function Sys$_Application$get_history() {
    var h = this._history;
    if (!h) {
        h = this._history = new Inflectra.SpiraTest.Web.AjaxExtensions._History();
        Sys.Application.registerDisposableObject(h);
        h.initialize();
    }
    return h;
}

//The following allows culture info to be sent from the page to the Ajax web services
Sys.Net.WebServiceProxy.original_invoke = Sys.Net.WebServiceProxy.invoke;
Sys.Net.WebServiceProxy.invoke =
function WebServiceProxy$invoke(servicePath, methodName, useGet, params, onSuccess, onFailure, userContext, timeout, enableJsonp, jsonpCallbackParameter)
{
    /// <summary locid="M:J#Sys.Net.WebServiceProxy.invoke"></summary>
    /// <param name="servicePath" type="String">Path to the webservice</param>
    /// <param name="methodName" type="String" mayBeNull="true" optional="true">Method to invoke</param>
    /// <param name="useGet" type="Boolean" optional="true" mayBeNull="true">Controls whether requests use HttpGet</param>
    /// <param name="params" mayBeNull="true" optional="true">Method args.</param>
    /// <param name="onSuccess" type="Function" mayBeNull="true" optional="true">Success callback</param>
    /// <param name="onFailure" type="Function" mayBeNull="true" optional="true">Failure callback</param>
    /// <param name="userContext" mayBeNull="true" optional="true">Success callback</param>
    /// <param name="timeout" type="Number" optional="true" mayBeNull="true">Timeout in milliseconds</param>
    /// <param name="enableJsonp" type="Boolean" optional="true" mayBeNull="true">Whether to use JSONP if the servicePath is for a different domain (default is true).</param>
    /// <param name="jsonpCallbackParameter" type="String" optional="true" mayBeNull="true">The name of the callback parameter for JSONP request (default is callback).</param>
    /// <returns type="Sys.Net.WebRequest" mayBeNull="true">Returns the request that was sent (null for JSONP requests).</returns>
    var e = Function._validateParams(arguments, [
        { name: "servicePath", type: String },
        { name: "methodName", type: String, mayBeNull: true, optional: true },
        { name: "useGet", type: Boolean, mayBeNull: true, optional: true },
        { name: "params", mayBeNull: true, optional: true },
        { name: "onSuccess", type: Function, mayBeNull: true, optional: true },
        { name: "onFailure", type: Function, mayBeNull: true, optional: true },
        { name: "userContext", mayBeNull: true, optional: true },
        { name: "timeout", type: Number, mayBeNull: true, optional: true },
        { name: "enableJsonp", type: Boolean, mayBeNull: true, optional: true },
        { name: "jsonpCallbackParameter", type: String, mayBeNull: true, optional: true }
    ]);
    if (e) throw e;
    var schemeHost = (enableJsonp !== false) ? Sys.Net.WebServiceProxy._xdomain.exec(servicePath) : null,
        tempCallback, jsonp = schemeHost && (schemeHost.length === 3) &&
            ((schemeHost[1] !== location.protocol) || (schemeHost[2] !== location.host));
    useGet = jsonp || useGet;
    if (jsonp)
    {
        jsonpCallbackParameter = jsonpCallbackParameter || "callback";
        tempCallback = "_jsonp" + Sys._jsonp++;
    }
    if (!params) params = {};
    var urlParams = params;
    if (!useGet || !urlParams) urlParams = {};
    var error, timeoutcookie = null, body = null,
        url = Sys.Net.WebRequest._createUrl(methodName
            ? (servicePath + "/" + encodeURIComponent(methodName))
            : servicePath, urlParams, jsonp ? (jsonpCallbackParameter + "=Sys." + tempCallback) : null);
    if (jsonp)
    {
        function jsonpComplete(data, statusCode)
        {
            if (timeoutcookie !== null)
            {
                window.clearTimeout(timeoutcookie);
                timeoutcookie = null;
            }
            delete Sys[tempCallback];
            tempCallback = null;
            if ((typeof (statusCode) !== "undefined") && (statusCode !== 200))
            {
                if (onFailure)
                {
                    error = new Sys.Net.WebServiceError(false,
                            data.Message || String.format(Sys.Res.webServiceFailedNoMsg, methodName),
                            data.StackTrace || null,
                            data.ExceptionType || null,
                            data);
                    error._statusCode = statusCode;
                    onFailure(error, userContext, methodName);
                }
                else
                {
                    if (data.StackTrace && data.Message)
                    {
                        error = data.StackTrace + "-- " + data.Message;
                    }
                    else
                    {
                        error = data.StackTrace || data.Message;
                    }
                    error = String.format(error ? Sys.Res.webServiceFailed : Sys.Res.webServiceFailedNoMsg, methodName, error);
                    throw Sys.Net.WebServiceProxy._createFailedError(methodName, String.format(Sys.Res.webServiceFailed, methodName, error));
                }
            }
            else if (onSuccess)
            {
                onSuccess(data, userContext, methodName);
            }
        }
        Sys[tempCallback] = jsonpComplete;
        Sys._loadJsonp(url, function ()
        {
            if (tempCallback)
            {
                jsonpComplete({ Message: String.format(Sys.Res.webServiceFailedNoMsg, methodName) }, -1);
            }
        });
        return null;
    }
    var request = new Sys.Net.WebRequest();
    request.set_url(url);
    request.get_headers()['Content-Type'] = 'application/json; charset=utf-8';
    request.get_headers()['X-Culture-Info'] = Sys.CultureInfo.CurrentCulture.name;
    request.get_headers()['X-Timezone-Info'] = SpiraContext.CurrentTimezone;
    request.get_headers()['cache-control'] = 'no-cache';    //Overcomes iOS Safari caching bug
    if (!useGet)
    {
        body = Sys.Serialization.JavaScriptSerializer.serialize(params);
        if (body === "{}") body = "";
    }
    request.set_body(body);
    request.add_completed(onComplete);
    if (timeout > 0) request.set_timeout(timeout);
    request.invoke();

    function onComplete(response, eventArgs)
    {
        if (response.get_responseAvailable())
        {
            var ex, statusCode = response.get_statusCode();
            var result = null;
            var isJson;

            try
            {
                var contentType = response.getResponseHeader("Content-Type");
                isJson = contentType.startsWith("application/json");
                result = isJson ? response.get_object() :
                    (contentType.startsWith("text/xml") ? response.get_xml() : response.get_responseData());
            }
            catch (ex)
            {
            }

            var error = response.getResponseHeader("jsonerror");
            var errorObj = (error === "true");
            if (errorObj)
            {
                if (result)
                {
                    result = new Sys.Net.WebServiceError(false, result.Message, result.StackTrace, result.ExceptionType, result);
                }
            }
            else if (isJson)
            {
                result = (!result || (typeof (result.d) === "undefined")) ? result : result.d;
            }
            if (((statusCode < 200) || (statusCode >= 300)) || errorObj)
            {
                if (onFailure)
                {
                    if (!result || !errorObj)
                    {
                        result = new Sys.Net.WebServiceError(false /*timedout*/, String.format(Sys.Res.webServiceFailedNoMsg, methodName));
                    }
                    result._statusCode = statusCode;
                    onFailure(result, userContext, methodName);
                }
                else
                {
                    if (result && errorObj)
                    {
                        error = result.get_exceptionType() + "-- " + result.get_message();
                    }
                    else
                    {
                        error = response.get_responseData();
                    }
                    throw Sys.Net.WebServiceProxy._createFailedError(methodName, String.format(Sys.Res.webServiceFailed, methodName, error));
                }
            }
            else if (onSuccess)
            {
                onSuccess(result, userContext, methodName);
            }
        }
        else
        {
            var timedOut = response.get_timedOut(),
                msg = String.format((timedOut ? Sys.Res.webServiceTimedOut : Sys.Res.webServiceFailedNoMsg), methodName);
            if (onFailure)
            {
                onFailure(new Sys.Net.WebServiceError(timedOut, msg, "", ""), userContext, methodName);
            }
            else
            {
                throw Sys.Net.WebServiceProxy._createFailedError(methodName, msg);
            }
        }
    }

    return request;
}

if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();

//Added to make accessing Functions object possible in TypeScript
var AspNetAjax$Function = Function;
