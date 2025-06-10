using System.Web.UI;
using System.Web;
using System.Web.Script.Services;
using System.Globalization;
using Inflectra.SpiraTest.Web.ServerControls.ExtenderBase;

[assembly: System.Web.UI.WebResource("Common.DateTime.js", "application/x-javascript")]
[assembly: System.Web.UI.WebResource("Common.DateTime.debug.js", "application/x-javascript")]

namespace Inflectra.SpiraTest.Web.ServerControls.CommonScripts
{
    [RequiredScript(typeof(CommonToolkitScripts))]
    [ClientScriptResource(null, "Common.DateTime.js")]
    public static class DateTimeScripts
    {
    }
}
