using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.Utils
{
    /// <summary>
    /// Used to denote if a web service data object property is read-only
    /// </summary>
    /// <remarks>
    /// Only used by the documentation generator, does not affect the service itself
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : Attribute
    {
    }

    /// <summary>
    /// Used to denote if a web service data object property is optional
    /// </summary>
    /// <remarks>
    /// Only used by the documentation generator, does not affect the service itself
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class OptionalAttribute : Attribute
    {
    }

}