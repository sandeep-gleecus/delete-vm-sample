namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Net.Http;
	using System.Threading.Tasks;
	using System.Web;

	/// <summary>This class should ONLY be used for testing Spira's code. It does not always use the same code
	/// that other DLLs use to talk to OAuth Providers, so using this to test common OAuth functions
	/// should be avoided, unless code changed in Providers.cs is copied here.</summary>
	public class DummyProvider : Provider
	{
		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("4B916F61-9AF9-49B9-9347-11B18FA3FE64"),
			Name = "Dummy",
			Description = "Allows users to log in with a dummy OAuth Provider."
		};
		#endregion 

		public override ProviderURLs Addresses
		{
            get
            {
                return new ProviderURLs
                {
                    Authorization = new Uri("https://localhost/notused"),
                    Profile = new Uri("https://localhost/notused"),
                    Token = new Uri("https://localhost/notused")
                };
			}
		}

		/// <summary>The name of this provider!</summary>
		public override string Name => _provider.Name;

		/// <summary>The unique ID of this provider!</summary>
		public override Guid ProviderId => _provider.Identification;

		/// <summary>The unique ID of this provider!</summary>
		public override string Description => _provider.Description;

		/// <summary>The full definition of this provider.</summary>
		public override ProviderType ProviderDefinition => _provider;

		/// <summary>The user-specific Client ID that this client cshould use when calling the provider.</summary>
		public override string ClientId { get; set; }

		/// <summary>The user-specific Secret Key that this client cshould use when calling the provider.</summary>
		public override string ClientSecretKey { get; set; }

		/// <summary>Cannot be set in this provider.</summary>
		public override Uri UrlAuthorization { set { } }

		/// <summary>Cannot be set in this provider.</summary>
		public override Uri UrlToken { set { } }

		/// <summary>Cannot be set in this provider.</summary>
		public override Uri UrlProfile { set { } }

		/// <summary>False, since we have our own hard-coded.</summary>
		public override bool IsUrlsRequired => false;

		/// <summary>True, since we have our own logo.</summary>
		public override bool IsLogoProvided => true;

		/// <summary>Private storage of the default scope.</summary>
		private string _scope;
		public override string Scope
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_scope))
					return "http://localhost/notused";
				else
					return _scope;
			}
			set
			{
				_scope = value;
			}
		}

		/// <summary>Called to start Init mode!</summary>
		public override void BeginInit()
		{
			_IsInInit = true;
		}

		/// <summary>Called to indicate Init mode is over! We need to check to make sure we have all required data.</summary>
		public override void EndInit()
		{
			//Check that we have a ClientId and a Secret Key. That is all that is required.
			if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecretKey))
			{
				throw new Exception("Both ClientId & ClientSecretKey are required to operate!");
			}
			else
			{
				_IsInInit = false;
				_IsInit = true;
			}
		}

		#region Overrides
		public new async Task<AuthorizationToken> GetTokenFromCallbackAsync(AuthorizationContext ctx, NameValueCollection queryString)
		{
			//Throw error if we are not configured!
			if (_IsInInit && !_IsInit)
				throw new NotConfiguredException();

			if (ctx.ProviderId != ProviderId) throw new Exception("Invalid AuthorizationCodeProvider name");

			string error = queryString["error"];
			if (!string.IsNullOrWhiteSpace(error)) return new AuthorizationToken { Error = error };

			string state = queryString["state"];
			if (ctx.State != state) return new AuthorizationToken { Error = "State does not match." };

			string code = queryString["code"];
			if (string.IsNullOrWhiteSpace(code)) return new AuthorizationToken { Error = "Invalid code." };

			List<KeyValuePair<string, string>> postValues = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("code", code),
				new KeyValuePair<string, string>("client_id", ClientId),
				new KeyValuePair<string, string>("client_secret", ClientSecretKey),
				new KeyValuePair<string, string>("redirect_uri", RedirectUrl),
				new KeyValuePair<string, string>("grant_type", "authorization_code")
			};

			return await GetTokenFromCallbackInternalAsync(postValues);
		}

		public new async Task<CallbackResult> ProcessCallbackAsync(AuthorizationContext authCtx, NameValueCollection nameValueCollection)
		{
			//Throw error if we are not configured!
			if (_IsInInit && !_IsInit)
				throw new NotConfiguredException();

			var token = await GetTokenFromCallbackAsync(authCtx, nameValueCollection);
			if (token.Error != null)
			{
				return new CallbackResult
				{
					Error = token.Error,
					ErrorDetails = token.ErrorDetails
				};
			}
			var claims = await GetProfileClaimsAsync(token);
			return new CallbackResult { Claims = claims };
		}

		public override async Task<AuthorizationToken> GetTokenFromCallbackInternalAsync(List<KeyValuePair<string, string>> postValues)
		{
			//Throw error if we are not configured!
			if (_IsInInit && !_IsInit)
				throw new NotConfiguredException();

			HttpClient client = new HttpClient();
			var content = new FormUrlEncodedContent(postValues);
			var result = await client.PostAsync(Addresses.Token, content);
			if (result.IsSuccessStatusCode)
			{
				return await ProcessAuthorizationTokenResponseAsync(result);
			}
			else
			{
				var body = await result.Content.ReadAsStringAsync();
				return new AuthorizationToken
				{
					Error = "Error contacting token endpoint : " + result.ReasonPhrase,
					ErrorDetails = body
				};
			}
		}

		public override Dictionary<int, object> ProviderSpecificClaims(Dictionary<string, object> profile)
		{
			return new Dictionary<int, object>();
		}

		#endregion
		#region Provider.cs Overridden code.
		string RedirectUrl
		{
			get
			{
				//Throw error if we are not configured!
				if (_IsInInit && !_IsInit) throw new NotConfiguredException();

				var ctx = HttpContext.Current; //Require a return URL to be called.

				var origin = OAuth2Client.OAuthCallbackOrigin;

				if (origin == null)
				{
					origin = ctx.Request.Url;
				}

				var app = ctx.Request.ApplicationPath;
				if (!app.EndsWith("/")) app += "/";

				var url = new Uri(origin, app + OAuth2Client.OAuthCallbackUrl);
				return ctx.Request.Url.GetLeftPart(UriPartial.Path);

			}
		}

		#endregion
	}
}
