using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Inflectra.SpiraTest.Business;

namespace Inflectra.SpiraTest.Web.Services.Utils
{
    /// <summary>
    /// Ensures that the names of exceptions don't change to API clients
    /// </summary>
    public static class StandardExceptions
    {
        public const string EXCEPTION_TYPE_CONCURRENCY = "OptimisticConcurrencyException";

        /// <summary>
        /// Returns the standard name of an exception for consumption by API clients
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>The standardized 'type' name</returns>
        public static string GetExceptionType(Exception exception)
        {
            if (exception is OptimisticConcurrencyException || exception is OptimisticConcurrencyException)
            {
                return EXCEPTION_TYPE_CONCURRENCY;
            }
            else
            {
                return exception.GetType().Name;
            }
        }
    }
}