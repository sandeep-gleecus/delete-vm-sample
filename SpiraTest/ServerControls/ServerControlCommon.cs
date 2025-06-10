using System;
using Inflectra.SpiraTest.Web.Services.Ajax.Json;
using Inflectra.SpiraTest.Common;

namespace Inflectra.SpiraTest.Web.ServerControls
{
	/// <summary>
	/// Contains common functions and constants used by all server controls
	/// </summary>
	public class ServerControlCommon
	{
        private const string CLASS_NAME = "Inflectra.SpiraTest.Web.ServerControls.ServerControlCommon::";

		//Script names that are used by more than one control
		internal const string SCRIPT_KEY_PERSISTENT_TOOLTIPS = "Tooltip";

        /// <summary>
        /// Ensures an attribute value ends with a semi-colon
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string EnsureEndWithSemiColon(string value)
        {
            if (value != null)
            {
                int length = value.Length;
                if ((length > 0) && (value[length - 1] != ';'))
                {
                    return (value + ";");
                }
            }
            return value;
        }

        /// <summary>
        /// Merges two inline javascript attributes
        /// </summary>
        /// <param name="firstScript"></param>
        /// <param name="secondScript"></param>
        /// <returns></returns>
        internal static string MergeScript(string firstScript, string secondScript, bool prefixWithJavaScript)
        {
            if (!string.IsNullOrEmpty(firstScript))
            {
                return (firstScript + secondScript);
            }
            if (!prefixWithJavaScript)
            {
                return secondScript;
            }
            else
            {
                if (secondScript.TrimStart(new char[0]).StartsWith("javascript:", StringComparison.Ordinal))
                {
                    return secondScript;
                }
                return ("javascript:" + secondScript);
            }
        }

		/// <summary>
		/// Makes a string safe for use as a Javascript string argument
		/// </summary>
		/// <param name="input">The input string</param>
        /// <param name="containsMarkup">Does the input string contain HTML markup or not</param>
		/// <returns>The string with all single quotes escaped and other characters handled correctly</returns>
		internal static string JSEncode (string input, bool containsMarkup)
		{
            if (input == null)
            {
                return "";
            }
            if (containsMarkup)
            {
                //For input text that has markup we don't add any <BR> tags since they will be in the markup
                string output = input;
                output = output.Replace("\\", "\\\\");
                output = output.Replace("'", @"\'");
                output = output.Replace("\n", "");
                output = output.Replace("\r", "");
                return output;
            }
            else
            {
                //For input text that has no markup we need to convert <BR> tags into newlines
                string output = input;
                output = output.Replace("\\", "\\\\");
                output = output.Replace("'", @"\'");
                output = output.Replace("\n", "<br>");
                output = output.Replace("\r", "");
                return output;
            }
		}
	}
}
