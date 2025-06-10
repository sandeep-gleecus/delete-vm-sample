namespace Inflectra.OAuth2
{
	using Newtonsoft.Json.Linq;
	using System;
	using System.Collections.Specialized;


	public class AuthorizationToken
	{
		public static AuthorizationToken FromJson(string json)
		{
			var token = JObject.Parse(json);
			var result = new AuthorizationToken();

			// Load in our standard data..
			result.AccessToken = token.Value<string>("access_token");
			result.TokenType = token.Value<string>("token_type");
			result.RefreshToken = token.Value<string>("refresh_token");
			result.Expiration = DateTime.UtcNow.AddMinutes(token.Value<int>("expires_in"));

			// See if we have a M$ ID_TOKEN..
			if (token.GetValue("id_token") != null)
			{
				try
				{
					result.IdToken = token.Value<string>("id_token");
				}
				catch
				{ }
			}

			return result;
		}

		public static AuthorizationToken FromCollection(NameValueCollection values)
		{
			var result = new AuthorizationToken();
			result.AccessToken = values["access_token"];
			result.TokenType = values["token_type"];
			result.RefreshToken = values["refresh_token"];
			var expires = values["expires_in"];
			int expiresInt;
			if (expires != null && Int32.TryParse(expires, out expiresInt))
			{
				result.Expiration = DateTime.UtcNow.AddMinutes(expiresInt);
			}
			else
			{
				expires = values["expires"];
				if (expires != null && Int32.TryParse(expires, out expiresInt))
				{
					result.Expiration = DateTime.UtcNow.AddMinutes(expiresInt);
				}
			}
			return result;
		}

		/// <summary>The access Token.</summary>
		public string AccessToken { get; set; }

		/// <summary>The type of the token we got.</summary>
		public string TokenType { get; set; }

		/// <summary>The date this Token expires.</summary>
		public DateTime Expiration { get; set; }

		/// <summary>The refredsh token. Not needed now, but used to get a new Access Token after this one expired.</summary>
		public string RefreshToken { get; set; }

		/// <summary>Any error message received from the provider.</summary>
		public string Error { get; set; }

		/// <summary>Error details.</summary>
		public string ErrorDetails { get; set; }

		/// <summary>Holds custom information gien by (at least) M$ AzureAD's "id_token" field.</summary>
		/// <remarks>
		/// This is a JWT. See: https://developer.okta.com/blog/2019/06/26/decode-jwt-in-csharp-for-authorization#tldr---how-to-decode-jwts-in-c
		/// </remarks>
		public string IdToken { get; set; }
	}
}
