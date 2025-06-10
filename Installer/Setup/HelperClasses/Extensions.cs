using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	static class Extenstions
    {
		/// <summary>Fixes Newlines in Resource files from not getting translated properly.</summary>
		/// <param name="str">The string to translate.</param>
		/// <returns>The translated string.</returns>
		public static string FixNewLineResource(this string str)
		{
			if (str.Contains("\\r\\n"))
				str = str.Replace("\\r\\n", Environment.NewLine);

			if (str.Contains("\\n"))
				str = str.Replace("\\n", Environment.NewLine);

			if (str.Contains("&#x0a;&#x0d;"))
				str = str.Replace("&#x0a;&#x0d;", Environment.NewLine);

			if (str.Contains("&#x0a;"))
				str = str.Replace("&#x0a;", Environment.NewLine);

			if (str.Contains("&#x0d;"))
				str = str.Replace("&#x0d;", Environment.NewLine);

			if (str.Contains("&#10;&#13;"))
				str = str.Replace("&#10;&#13;", Environment.NewLine);

			if (str.Contains("&#10;"))
				str = str.Replace("&#10;", Environment.NewLine);

			if (str.Contains("&#13;"))
				str = str.Replace("&#13;", Environment.NewLine);

			if (str.Contains("<br />"))
				str = str.Replace("<br />", Environment.NewLine);

			if (str.Contains("<LineBreak />"))
				str = str.Replace("<LineBreak />", Environment.NewLine);

			return str;
		}
    }
}
