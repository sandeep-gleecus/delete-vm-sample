namespace Inflectra.OAuth2
{
	using Newtonsoft.Json;
	using System;
	using System.Collections.Generic;

	public class AuthorizationContext
	{
		/// <summary>The GUID of the provider this references.</summary>
		public Guid ProviderId { get; set; }
		/// <summary>The ReturnUrl that the user should be returned to.</summary>
		public string ReturnUrl { get; set; }
		/// <summary>The unique state key generated before the user was redirected to the OAuth provider.</summary>
		public string State { get; set; }
		/// <summary>The provider's user ID.</summary>
		public string UserId { get; set; }

		/// <summary>Convert our object into a JSON-parseable string.</summary>
		/// <returns></returns>
		public string ToJson()
		{
			string retVal = "";

			if (this != null)
			{
				retVal = Newtonsoft.Json.JsonConvert.SerializeObject(this);
			}
			else
			{
				retVal = JsonConvert.SerializeObject(new AuthorizationContext());
			}

			return retVal;
		}

		/// <summary>Convert the string back into an object.</summary>
		/// <param name="json">The JSON to parse.</param>
		/// <returns>An object, or null if there was an error parsing it.</returns>
		public static AuthorizationContext Parse(string json)
		{
			AuthorizationContext retVal = null;

			try
			{
				retVal = JsonConvert.DeserializeObject<AuthorizationContext>(json);
			}
			catch
			{
				retVal = null;
			}

			return retVal;
		}
	}
}
