using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// List of CORS constants
    /// </summary>
    public static class CorsConstants
    {
        internal const string Origin = "Origin";
        internal const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";
        internal const string AccessControlRequestMethod = "Access-Control-Request-Method";
        internal const string AccessControlRequestHeaders = "Access-Control-Request-Headers";
        internal const string AccessControlAllowMethods = "Access-Control-Allow-Methods";
        internal const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        internal const string PreflightSuffix = "_preflight_";

        internal const string AllowOriginAll = "*"; //Denotes all all (not recommended)
	}
}
