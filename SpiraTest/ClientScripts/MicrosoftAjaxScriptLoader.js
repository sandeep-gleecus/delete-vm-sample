Sys._ScriptLoaderTask = function Sys$_ScriptLoaderTask(scriptElement, completedCallback)
{
    /// <summary locid="M:J#Sys._ScriptLoaderTask.#ctor" />
    /// <param name="scriptElement" domElement="true"></param>
    /// <param name="completedCallback" type="Function"></param>
    var e = Function._validateParams(arguments, [
        { name: "scriptElement", domElement: true },
        { name: "completedCallback", type: Function }
    ]);
    if (e) throw e;
    this._scriptElement = scriptElement;
    this._completedCallback = completedCallback;
}
function Sys$_ScriptLoaderTask$get_scriptElement()
{
    /// <value domElement="true" locid="P:J#Sys._ScriptLoaderTask.scriptElement"></value>
    if (arguments.length !== 0) throw Error.parameterCount();
    return this._scriptElement;
}
function Sys$_ScriptLoaderTask$dispose()
{
    if (this._disposed)
    {
        return;
    }
    this._disposed = true;
    this._removeScriptElementHandlers();
    Sys._ScriptLoaderTask._clearScript(this._scriptElement);
    this._scriptElement = null;
}
function Sys$_ScriptLoaderTask$execute()
{
    /// <summary locid="M:J#Sys._ScriptLoaderTask.execute" />
    if (arguments.length !== 0) throw Error.parameterCount();
    this._addScriptElementHandlers();
    var headElements = document.getElementsByTagName('head');
    if (headElements.length === 0)
    {
        throw new Error.invalidOperation(Sys.Res.scriptLoadFailedNoHead);
    }
    else
    {
        headElements[0].appendChild(this._scriptElement);
    }
}
function Sys$_ScriptLoaderTask$_addScriptElementHandlers()
{
    this._scriptLoadDelegate = Function.createDelegate(this, this._scriptLoadHandler);

    if (Sys.Browser.agent !== Sys.Browser.InternetExplorer)
    {
        this._scriptElement.readyState = 'loaded';
        $addHandler(this._scriptElement, 'load', this._scriptLoadDelegate);
    }
    else
    {
        $addHandler(this._scriptElement, 'readystatechange', this._scriptLoadDelegate);
    }
    if (this._scriptElement.addEventListener)
    {
        this._scriptErrorDelegate = Function.createDelegate(this, this._scriptErrorHandler);
        this._scriptElement.addEventListener('error', this._scriptErrorDelegate, false);
    }
}
function Sys$_ScriptLoaderTask$_removeScriptElementHandlers()
{
    if (this._scriptLoadDelegate)
    {
        var scriptElement = this.get_scriptElement();
        if (Sys.Browser.agent !== Sys.Browser.InternetExplorer)
        {
            $removeHandler(scriptElement, 'load', this._scriptLoadDelegate);
        }
        else
        {
            $removeHandler(scriptElement, 'readystatechange', this._scriptLoadDelegate);
        }
        if (this._scriptErrorDelegate)
        {
            this._scriptElement.removeEventListener('error', this._scriptErrorDelegate, false);
            this._scriptErrorDelegate = null;
        }
        this._scriptLoadDelegate = null;
    }
}
function Sys$_ScriptLoaderTask$_scriptErrorHandler()
{
    if (this._disposed)
    {
        return;
    }

    this._completedCallback(this.get_scriptElement(), false);
}
function Sys$_ScriptLoaderTask$_scriptLoadHandler()
{
    if (this._disposed)
    {
        return;
    }
    var scriptElement = this.get_scriptElement();
    if ((scriptElement.readyState !== 'loaded') &&
            (scriptElement.readyState !== 'complete'))
    {
        return;
    }

    this._completedCallback(scriptElement, true);
}
Sys._ScriptLoaderTask.prototype = {
    get_scriptElement: Sys$_ScriptLoaderTask$get_scriptElement,
    dispose: Sys$_ScriptLoaderTask$dispose,
    execute: Sys$_ScriptLoaderTask$execute,
    _addScriptElementHandlers: Sys$_ScriptLoaderTask$_addScriptElementHandlers,
    _removeScriptElementHandlers: Sys$_ScriptLoaderTask$_removeScriptElementHandlers,
    _scriptErrorHandler: Sys$_ScriptLoaderTask$_scriptErrorHandler,
    _scriptLoadHandler: Sys$_ScriptLoaderTask$_scriptLoadHandler
}
Sys._ScriptLoaderTask.registerClass("Sys._ScriptLoaderTask", null, Sys.IDisposable);
Sys._ScriptLoaderTask._clearScript = function Sys$_ScriptLoaderTask$_clearScript(scriptElement)
{
    if (!Sys.Debug.isDebug)
    {
        scriptElement.parentNode.removeChild(scriptElement);
    }
}