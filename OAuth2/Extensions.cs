using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace Inflectra.OAuth2
{
	public static class Extensions
	{
		/// <summary>Add the variable to the URL's querystring.</summary>
		/// <param name="uri">The URI to add the name/value to.</param>
		/// <param name="name">The name of the value/key.</param>
		/// <param name="value">The value of variable to encode into the querystring.</param>
		/// <returns>A URI object with the value added.</returns>
		public static Uri AddQuery(this Uri uri, string name, string value)
		{
			var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

			httpValueCollection.Remove(name);
			httpValueCollection.Add(name, value);

			var ub = new UriBuilder(uri);

			// this code block is taken from httpValueCollection.ToString() method
			// and modified so it encodes strings with HttpUtility.UrlEncode
			if (httpValueCollection.Count == 0)
				ub.Query = string.Empty;
			else
			{
				var sb = new StringBuilder();

				for (int i = 0; i < httpValueCollection.Count; i++)
				{
					string text = httpValueCollection.GetKey(i);
					{
						text = HttpUtility.UrlEncode(text);

						string val = (text != null) ? (text + "=") : string.Empty;
						string[] vals = httpValueCollection.GetValues(i);

						if (sb.Length > 0)
							sb.Append('&');

						if (vals == null || vals.Length == 0)
							sb.Append(val);
						else
						{
							if (vals.Length == 1)
							{
								sb.Append(val);
								sb.Append(HttpUtility.UrlEncode(vals[0]));
							}
							else
							{
								for (int j = 0; j < vals.Length; j++)
								{
									if (j > 0)
										sb.Append('&');

									sb.Append(val);
									sb.Append(HttpUtility.UrlEncode(vals[j]));
								}
							}
						}
					}
				}

				ub.Query = sb.ToString();
			}

			return ub.Uri;
		}

		/// <summary>Add tghe collection of variables to the URL's querystring.</summary>
		/// <param name="uri">The URI to add the name/value to.</param>
		/// <param name="values">A NameValue collection of all the variables & values to add.</param>
		/// <returns>A URI object with the values added.</returns>
		public static Uri AddQuery(this Uri uri, NameValueCollection values)
		{
			//Loop through the values in our collection, and call the other function.
			if (values != null && values.Count > 0)
				foreach (string key in values.Keys)
					uri = uri.AddQuery(key, values[key]);

			return uri;
		}
	}
}
