using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using Inflectra.SpiraTest.Web.ServerControls.ExtenderBase;

[assembly: System.Web.UI.WebResource("Common.Threading.js", "application/x-javascript")]
[assembly: System.Web.UI.WebResource("Common.Threading.debug.js", "application/x-javascript")]

namespace Inflectra.SpiraTest.Web.ServerControls.CommonScripts
{
    [ClientScriptResource(null, "Common.Threading.js")]
    public static class ThreadingScripts
    {
    }
}
