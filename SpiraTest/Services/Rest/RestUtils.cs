using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Inflectra.SpiraTest.Common;
using System.ServiceModel.Web;
using System.Globalization;

namespace Inflectra.SpiraTest.Web.Services.Rest
{
    /// <summary>
    /// Contains some static utility functions useful when working with REST web services
    /// </summary>
    public static class RestUtils
    {
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.Services.Rest.RestUtils";

        public const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fff";

        public static bool IsEquivalentToWithoutQueryString (this UriTemplate uriTemplate1, UriTemplate uriTemplate2)
        {
            if (uriTemplate1 == null || uriTemplate2 == null)
            {
                return false;
            }

            string template1 = uriTemplate1.ToString().ToLowerInvariant();
            string template2 = uriTemplate2.ToString().ToLowerInvariant();

            //Remove path
            string template1path = template1.Split('?')[0];
            string template2path = template2.Split('?')[0];
            
            return (template1path == template2path);
        }

        /// <summary>
        /// Converts a string that contains a Guid to a Guid. It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The Guid value</returns>
        public static Guid ConvertToGuid(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToGuid";

            Guid returnValue;
            if (Guid.TryParse(parameterValue, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be a valid GUID");
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be a valid GUID", System.Net.HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Converts a string that contains an integer to an integer. It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The integer value</returns>
        public static int ConvertToInt32(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToInt32";

            int returnValue;
            if (Int32.TryParse(parameterValue, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be an integer");
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be an integer", System.Net.HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Converts a string that contains an integer to an integer. It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The nullable integer value</returns>
        public static int? ConvertToInt32Nullable(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToInt32";

            if (String.IsNullOrWhiteSpace(parameterValue) || parameterValue.ToLowerInvariant().Equals("null"))
            {
                return null;
            }

            int returnValue;
            if (Int32.TryParse(parameterValue, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be an integer");
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be an integer", System.Net.HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Converts a string that contains a datetime to DateTime object. It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The DateTime value</returns>
        public static DateTime ConvertToDateTime(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToDateTime";

            DateTime returnValue;
            if (DateTime.TryParseExact(parameterValue, DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None , out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be a DateTime in the format: " + DATETIME_FORMAT);
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be a DateTime in the format: " + DATETIME_FORMAT, System.Net.HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Converts a string that contains a datetime to nullable DateTime object. It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The DateTime value</returns>
        public static DateTime? ConvertToDateTimeNullable(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToDateTimeNullable";

            //Handle null case first
            if (string.IsNullOrEmpty(parameterValue))
            {
                return null;
            }

            DateTime returnValue;
            if (DateTime.TryParseExact(parameterValue, DATETIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be a DateTime in the format: " + DATETIME_FORMAT);
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be a DateTime in the format: " + DATETIME_FORMAT, System.Net.HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Converts a string that contains an integer to an integer. It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The integer value</returns>
        public static bool ConvertToBoolean(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToBoolean";

            bool returnValue;
            if (Boolean.TryParse(parameterValue, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be either 'true' or 'false'");
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be either 'true' or 'false'", System.Net.HttpStatusCode.NotAcceptable);
            }
        }

        /// <summary>
        /// Converts a string that contains a comma-separated list of integers to an MultiValueFilter object.
        /// It it cannot be converted, it throws a meaningful exception
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="parameterValue">The string parameter</param>
        /// <returns>The integer value</returns>
        public static Common.MultiValueFilter ConvertToMultiValueFilter(string parameterValue, string parameterName)
        {
            const string METHOD_NAME = "ConvertToMultiValueFilter";

            Common.MultiValueFilter returnValue;
            if (Common.MultiValueFilter.TryParse(parameterValue, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Logger.LogWarningEvent(CLASS_NAME + METHOD_NAME, "The passed in '" + parameterName + "' needs to be a comma-separated list of integers");
                Logger.Flush();
                throw new WebFaultException<string>("The passed in '" + parameterName + "' needs to be a comma-separated list of integers", System.Net.HttpStatusCode.NotAcceptable);
            }
        }

    }
}