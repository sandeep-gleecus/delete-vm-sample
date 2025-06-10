using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.OAuth2
{
	static class Common
	{
		/// <summary>Decode the given Exception into a nice pretty string.</summary>
		/// <param name="ex">The exception to decode.</param>
		/// <returns>String used for logging.</returns>
		public static string DecodeException(Exception ex)
		{
			//Constants
			const string indent = "   ";
			string crlf = Environment.NewLine;

			string retString = "";
			if (ex != null)
			{
				//Get the stack trace, and initialize out other variables.
				string strStackTrace = ex.StackTrace;
				List<string> lstSQLErrors = new List<string>();
				string strFusion = "";
				List<string> lstMsgs = new List<string>();

				//Loop through each exception.
				for (Exception curEx = ex; curEx != null; curEx = curEx.InnerException)
				{
					//Get fusion log (if needed).
					if (string.IsNullOrWhiteSpace(strFusion) &&
						curEx.GetType().GetProperty("FusionLog") != null &&
						((dynamic)curEx).FusionLog != null)
					{
						strFusion = ((dynamic)curEx).FusionLog;
					}

					//Get any SQL errors (if needed).
					if (curEx is SqlException && lstSQLErrors.Count < 1)
					{
						List<string> listSQLErrors = new List<string>();
						int counter = 1;
						foreach (SqlError error in ((SqlException)curEx).Errors)
						{
							listSQLErrors.Add(counter.ToString() + ": " + error.Message);
							counter++;
						}
					}

					//Now get the type and message.
					lstMsgs.Add(curEx.Message + " [" + curEx.GetType().ToString() + "]");
				}

				//Now combine them all to be pretty!
				retString = "Messages:" + crlf;
				retString += indent + string.Join(crlf + indent, lstMsgs).Trim();
				retString += crlf + crlf;
				if (lstSQLErrors.Count > 0)
				{
					retString += "SQL Messages:" + crlf;
					retString += indent + string.Join(crlf + indent, lstSQLErrors).Trim();
					retString += crlf + crlf;
				}
				if (!string.IsNullOrWhiteSpace(strFusion))
				{
					retString += "Fusion Log:" + crlf;
					retString += strFusion;
					retString += crlf + crlf;
				}
				retString += "Stack Trace:" + crlf;
				retString += strStackTrace;

				//Trim off any extras..
				retString = retString.Trim();
			}

			//Send it back!
			return retString;
		}
	}
}
