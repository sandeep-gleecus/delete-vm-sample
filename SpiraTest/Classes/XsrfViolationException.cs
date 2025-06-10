using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Classes
{
    /// <summary>
    /// Thrown if the XSRF token doesn't match
    /// </summary>
    public class XsrfViolationException : InvalidOperationException
    {
        public XsrfViolationException()
        {
        }
        public XsrfViolationException(string message)
			: base(message)
		{
        }
        public XsrfViolationException(string message, Exception inner)
			: base(message, inner)
		{
        }
    }
}