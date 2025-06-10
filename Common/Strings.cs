using System.Text.RegularExpressions;
using System.Text;
using System;

namespace Inflectra.SpiraTest.Common
{
	public static class Strings
	{
		public static string EMAIL_LINEBREAK = "\r\n";

        /// <summary>
        /// Provides a case-insensitive string replace function
        /// </summary>
        /// <param name="str">The source string</param>
        /// <param name="oldValue">The old value</param>
        /// <param name="newValue">The replacement value</param>
        /// <param name="comparison">The replacement mode (case sensitive/insensitive)</param>
        /// <returns></returns>
        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        /// <summary>
        /// Returns a safe ToString that handles nulls correctly
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToSafeString(this object obj)
        {
            return (obj ?? string.Empty).ToString();
        }

        /// <summary>
        /// Converts plain text so that it displays nicely as HTML
        /// </summary>
        /// <param name="plainText">The input plain text</param>
        /// <returns>The output html friendly text</returns>
        /// <remarks>Turns newlines into <br /> tags</remarks>
        public static string RenderPlainTextAsHtml(string plainText)
        {
            if (plainText == null)
            {
                return plainText;
            }
            else
            {
                return plainText.Replace("\n", "<br />\n");
            }
        }
		
		/// <summary>Converts the string given by removing all HTML within it.</summary>
		/// <param name="inputString">The string to convert.</param>
		/// <param name="addHrefs">Wether to add links or take them out completely. Default: Yes</param>
		/// <param name="filterWord">Filter word markup as well? Default: YES</param>
		/// <returns>A filtered string.</returns>
        public static string StripHTML(this string inputString, bool addHrefs = true, bool filterWord = true)
		{
			if (string.IsNullOrWhiteSpace(inputString))
				inputString = "";

			string retString = inputString;
			//First see if they want to add Hrefs, and get the links from <a> tags..
			if (addHrefs)
			{
				//Get a list of all <a></a> tags..
				Regex ahrefEdit = new Regex("<[ ]?a.*? href=(\"?)(?<url>(?:ftp|http|https):\\/\\/(?:\\w+:{0,1}\\w*@)?(?:\\S+)(?:\\:[0-9]+)?(?:\\/|\\/(?:[\\w#!:.?+=&%@!\\-\\/]))?)\\1.*?(?:.|\\n)*?</a>");
				MatchCollection ahrefs = ahrefEdit.Matches(inputString);
				//Loop through and replace it..
				foreach (Match aHref in ahrefs)
					retString = retString.Replace(aHref.Groups[0].Value, aHref.Groups[0].Value + " [" + aHref.Groups[2].Value + "]");
			}

			if (filterWord)
				retString = Strings.StripWORD(retString);

			retString = retString.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
			retString = retString.Replace("\t", "");
			retString = Regex.Replace(retString, @"( )+", " ");

			// Remove the header (prepare first by clearing attributes)
			retString = Regex.Replace(retString, "(<( )*head([^>])*>).*(<( )*head([^>])*>)", "", RegexOptions.IgnoreCase);

			// remove all scripts (prepare first by clearing attributes)
			retString = Regex.Replace(retString, @"(<( )*script([^>])*>).*(<( )*(/)( )*script( )*>)", "", RegexOptions.IgnoreCase);

			// remove all styles (prepare first by clearing attributes)
			retString = Regex.Replace(retString, "(<( )*style([^>])*>).*(<( )*(/)( )*style( )*>)", "", RegexOptions.IgnoreCase);

			// insert tabs in spaces of <td> tags
			retString = Regex.Replace(retString, @"<( )*td([^>])*>", "\t", RegexOptions.IgnoreCase);

			// insert line breaks in places of <BR> and <LI> tags
			retString = Regex.Replace(retString, @"<( )*br( )*(/)?>", EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, @"<( )*li( )*>", EMAIL_LINEBREAK + "* ", RegexOptions.IgnoreCase);

			// insert line paragraphs (double line breaks) in place
			// if <P>, <DIV> and <TR> tags
			retString = Regex.Replace(retString, @"<( )*div([^>])*>", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, @"<( )*tr([^>])*>", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, @"<( )*p([^>])*>", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);

			// Remove remaining tags like <a>.
			retString = Regex.Replace(retString, @"<[^>]*>", "", RegexOptions.IgnoreCase);

			//HTML Decode to convert &xxxx; entities back to text
            retString = System.Web.HttpUtility.HtmlDecode(retString);

			//Trim Linebreaks & Spaces
			retString = Regex.Replace(retString, "(\r\n)( )+(\r\n)", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\t)( )+(\t)", "\t\t", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\t)( )+(\r\n)", "\t" + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\r\n)( )+(\t)", EMAIL_LINEBREAK + "\t", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\r\n)(\t)+(\r\n)", EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\r\n)(\t)+", EMAIL_LINEBREAK + "\t", RegexOptions.IgnoreCase);

			//Reduce anything more than four linebreaks to three.
			string limitRet = EMAIL_LINEBREAK + EMAIL_LINEBREAK + EMAIL_LINEBREAK + EMAIL_LINEBREAK;
			string maxRet = EMAIL_LINEBREAK + EMAIL_LINEBREAK + EMAIL_LINEBREAK ;
			while (retString.Contains(limitRet))
				retString = retString.Replace(limitRet, maxRet);
			//Reduce any more than 5 tabs to 5 tabs.
			string limitTab = "\t\t\t\t\t\t";
			string maxTab = "\t\t\t\t\t";
			while (retString.Contains(limitTab))
				retString = retString.Replace(limitTab, maxTab);

			return retString;
		}

		/// <summary>Converts the object given by removing all HTML within it, after trying to convert it to a string.</summary>
		/// <param name="inputString">The string to convert.</param>
		/// <param name="addHrefs">Wether to add links or take them out completely. Default: Yes</param>
		/// <param name="filterWord">Filter word markup as well? Default: YES</param>
		/// <returns>A filtered string.</returns>
		public static string StripHTML(this object inputObject, bool addHrefs = true, bool filterWord = true)
		{
			string inputString="";

			if (inputObject.GetType() != typeof(DBNull) && inputObject != null)
			{
				inputString = inputObject.ToString();
				if (string.IsNullOrWhiteSpace(inputString))
					inputString = "";
			}

			string retString = inputString;
			//First see if they want to add Hrefs, and get the links from <a> tags..
			if (addHrefs)
			{
				//Get a list of all <a></a> tags..
				Regex ahrefEdit = new Regex("<[ ]?a.*? href=(\"?)(?<url>(?:ftp|http|https):\\/\\/(?:\\w+:{0,1}\\w*@)?(?:\\S+)(?:\\:[0-9]+)?(?:\\/|\\/(?:[\\w#!:.?+=&%@!\\-\\/]))?)\\1.*?(?:.|\\n)*?</a>");
				MatchCollection ahrefs = ahrefEdit.Matches(inputString);
				//Loop through and replace it..
				foreach (Match aHref in ahrefs)
					retString = retString.Replace(aHref.Groups[0].Value, aHref.Groups[0].Value + " [" + aHref.Groups[2].Value + "]");
			}

			if (filterWord)
				retString = Strings.StripWORD(retString);

			retString = retString.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
			retString = retString.Replace("\t", "");
			retString = Regex.Replace(retString, @"( )+", " ");

			// Remove the header (prepare first by clearing attributes)
			retString = Regex.Replace(retString, "(<( )*head([^>])*>).*(<( )*head([^>])*>)", "", RegexOptions.IgnoreCase);

			// remove all scripts (prepare first by clearing attributes)
			retString = Regex.Replace(retString, @"(<( )*script([^>])*>).*(<( )*(/)( )*script( )*>)", "", RegexOptions.IgnoreCase);

			// remove all styles (prepare first by clearing attributes)
			retString = Regex.Replace(retString, "(<( )*style([^>])*>).*(<( )*(/)( )*style( )*>)", "", RegexOptions.IgnoreCase);

			// insert tabs in spaces of <td> tags
			retString = Regex.Replace(retString, @"<( )*td([^>])*>", "\t", RegexOptions.IgnoreCase);

			// insert line breaks in places of <BR> and <LI> tags
			retString = Regex.Replace(retString, @"<( )*br( )*(/)?>", EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, @"<( )*li( )*>", EMAIL_LINEBREAK + "* ", RegexOptions.IgnoreCase);

			// insert line paragraphs (double line breaks) in place
			// if <P>, <DIV> and <TR> tags
			retString = Regex.Replace(retString, @"<( )*div([^>])*>", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, @"<( )*tr([^>])*>", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, @"<( )*p([^>])*>", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);

			// Remove remaining tags like <a>.
			retString = Regex.Replace(retString, @"<[^>]*>", "", RegexOptions.IgnoreCase);

			// replace special characters:
			retString = Regex.Replace(retString, "&nbsp;", " ", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&bull;", " * ", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&lsaquo;", "<", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&rsaquo;", ">", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&laquo;", "«", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&raquo;", "»", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&trade;", "(tm)", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&copy;", "(C)", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&frasl;", "/", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&lt;", "<", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&gt;", ">", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&reg;", "(R)", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&deg;", "°", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&cent;", "¢", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&pound;", "£", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&curren;", "¤", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&yen;", "¥", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&brvbar;", "|", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&(n|m)dash;", "-", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&euro;", "€", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&empty;", "∅", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "&sim;", "~", RegexOptions.IgnoreCase);
			//Very last, amps..
			retString = Regex.Replace(retString, "&amp;", "&", RegexOptions.IgnoreCase);
			//Clear out what's left.
			retString = Regex.Replace(retString, @"&(.{2,6});", "", RegexOptions.IgnoreCase);

			//Trim Linebreaks & Spaces
			retString = Regex.Replace(retString, "(\r\n)( )+(\r\n)", EMAIL_LINEBREAK + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\t)( )+(\t)", "\t\t", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\t)( )+(\r\n)", "\t" + EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\r\n)( )+(\t)", EMAIL_LINEBREAK + "\t", RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\r\n)(\t)+(\r\n)", EMAIL_LINEBREAK, RegexOptions.IgnoreCase);
			retString = Regex.Replace(retString, "(\r\n)(\t)+", EMAIL_LINEBREAK + "\t", RegexOptions.IgnoreCase);

			//Reduce anything more than four linebreaks to three.
			string limitRet = EMAIL_LINEBREAK + EMAIL_LINEBREAK + EMAIL_LINEBREAK + EMAIL_LINEBREAK;
			string maxRet = EMAIL_LINEBREAK + EMAIL_LINEBREAK + EMAIL_LINEBREAK;
			while (retString.Contains(limitRet))
				retString = retString.Replace(limitRet, maxRet);
			//Reduce any more than 5 tabs to 5 tabs.
			string limitTab = "\t\t\t\t\t\t";
			string maxTab = "\t\t\t\t\t";
			while (retString.Contains(limitTab))
				retString = retString.Replace(limitTab, maxTab);

			return retString;
		}

		/// <summary>Strips MSWord tags form an input string.</summary>
		/// <param name="inputString">The string to strip HTML tags from.</param>
		/// <returns>A clean string.</returns>
		public static string StripWORD(this string inputString)
		{
			if (inputString != null)
			{
				inputString = Regex.Replace(inputString, "(?is)<!--\\[if gte mso \\d{0,3}\\]>.*?(<!\\[endif\\]-->)", ""); //Removes MSW IF/ENDIF format.
				inputString = Regex.Replace(inputString, "(?is)<!--\\[if gte vml \\d{0,3}\\]>.*?(<!\\[endif\\]-->)", ""); //Removes MSW VML objects.
				inputString = Regex.Replace(inputString, "(?is)<!--\\[if \\!vml\\]>.*?(<!\\[endif\\]-->)", ""); //Removes MSW non-VML images.
				inputString = Regex.Replace(inputString, "(?is)<meta[^>]*?>", ""); // Removes meta tags.
				inputString = Regex.Replace(inputString, "(?is)<link[^>]*?>", ""); // Removes link tags.
				inputString = Regex.Replace(inputString, "(?is)<o:([^>]*)>.*?</o:\\1>", ""); // Removes Office-Specific tags.
				inputString = Regex.Replace(inputString, "(?is)<!--.*?-->", ""); //Remove any last HMTL comment tags.
			}

			return inputString;
		}

        /// <summary>
        /// Truncates a string to be no longer that a certain length (used to prevent DB field overflow)
        /// </summary>
        /// <param name="str">The string</param>
        /// <param name="maxLength">The max number of character</param>
        /// <returns></returns>
        public static string MaxLength(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Length <= maxLength)
            {
                return str;
            }
            return str.Substring(0, maxLength);
        }

		/// <summary>Returns the specified substring safely. (Null if there's no string within the given boundaries.</summary>
		/// <param name="str1">The string to get the substring for.</param>
		/// <param name="start">The start position.</param>
		/// <param name="length">The length of the string.</param>
		/// <returns>The specified a string, or an empty string.</returns>
		public static string SafeSubstring(this string str1, int start = 0, int length = -1)
		{
			string retString = null;

            if (str1 == null)
            {
                return null;
            }

			//Reset the length if it wasn't specified or specified too long.
			if (length == -1 || length > str1.Substring(start).Length)
				length = str1.Substring(start).Length;

			try
			{
				retString = str1.Substring(start, length);
			}
			catch { }

			return retString;
		}

        /// <summary>
        /// Determines if a string is integer or not
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static bool IsInteger(this string inputString)
        {
            int tryInt = 0;
            if (int.TryParse(inputString, out tryInt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the integer value of a numeric string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static int? AttemptGetInt(this string inputString)
        {
            //If the string is an invalid nuimber we return null.
            int? retInt = null;

            int tryInt = 0;
            if (int.TryParse(inputString, out tryInt))
            {
                retInt = tryInt;
            }

            return retInt;
        }

	}
}
