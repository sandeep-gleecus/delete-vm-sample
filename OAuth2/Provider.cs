namespace Inflectra.OAuth2
{
	using Inflectra.SpiraTest.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Net;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using System.Threading.Tasks;
	using System.Web;

	public abstract class Provider : ISupportInitialize
	{
		public const int ERROR_CODE = -99;

		#region Abstract Prop/Methods
		/// <summary>The unique ID of the provider. Used in the database. This should be assigned as soon as a new
		/// provider is created, and never, ever changed except in catastrphic Provider changes.</summary>
		public abstract Guid ProviderId { get; }

		/// <summary>The display name of the provider.</summary>
		public abstract string Name { get; }

		/// <summary>A short description of the provider, used in the Admin pages for Admin's to know what it does.</summary>
		public abstract string Description { get; }

		/// <summary>The complete definition of this Provider.</summary>
		public abstract ProviderType ProviderDefinition { get; }

		public abstract string ClientId { get; set; }

		public abstract string ClientSecretKey { get; set; }

		/// <summary>Called to start Initialization.</summary>
		public abstract void BeginInit();

		/// <summary>Called to indicate initialization is over.</summary>
		public abstract void EndInit();

		/// <summary>URls needed for this provider.</summary>
		public abstract ProviderURLs Addresses { get; }

		/// <summary>Contains the Scoope of our request (the areas of the data we're asking for)</summary>
		public abstract string Scope { get; set; }

		/// <summary>The URL to call to have the user Authorize themselves.</summary>
		public abstract Uri UrlAuthorization { set; }

		/// <summary>The URL to get the logged-in token.</summary>
		public abstract Uri UrlToken { set; }

		/// <summary>The URL to get the Profile Claims from.</summary>
		public abstract Uri UrlProfile { set; }

		/// <summary>True if the three URL fields are needed. False if they are not or are fixed.</summary>
		public abstract bool IsUrlsRequired { get; }

		/// <summary>True if the provider DLL provides it's own image URL or data.</summary>
		public abstract bool IsLogoProvided { get; }

		/// <summary>Lets each individual provider library handle any non-standard claims.</summary>
		/// <param name="profile">Profile claims.</param>
		/// <returns>A list of provider-spefici claims.</returns>
		public abstract Dictionary<int, object> ProviderSpecificClaims(Dictionary<string, object> profile);
		#endregion

		#region Virtual Props/Methods
		/// <summary>Authorization passed. Return the call and ask for the authorizaton token!</summary>
		/// <param name="postValues">The querystring/post values on the return call from the provider.</param>
		/// <returns></returns>
		public virtual async Task<AuthorizationToken> GetTokenFromCallbackInternalAsync(List<KeyValuePair<string, string>> postValues)
		{
			if (!_IsConfigured) throw new NotConfiguredException();

			try
			{
				HttpClient client = new HttpClient();

				if (SpiraTest.Common.Properties.Settings.Default.BypassCertCheck)
				{
					//This, in debug mode, will tell the Client to allow ANY certificate. This is useful
					//  when testing against self-signed certificates.
					ServicePointManager.ServerCertificateValidationCallback +=
						(sender, cert, chain, sslPolicyErrors) => true;
				}

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
						Error = "Error contacting token endpoint: " + result.ReasonPhrase,
						ErrorDetails = body
					};
				}
			}
			catch (Exception ex)
			{
				return new AuthorizationToken
				{
					Error = "Error contacting token endpoint.",
					ErrorDetails = Common.DecodeException(ex)
				};
			}
		}

		/// <summary>Reads the responce data for getting the Auth Token from the provider!</summary>
		/// <param name="result">The result received from the Provider</param>
		/// <returns>The token needed to perform query on Profile data.</returns>
		public virtual async Task<AuthorizationToken> ProcessAuthorizationTokenResponseAsync(HttpResponseMessage result)
		{
			if (!_IsConfigured) throw new NotConfiguredException();

			var data = await result.Content.ReadAsStringAsync();
			if (result.Content.Headers.ContentType.MediaType.Equals("application/json"))
			{
				// json from body
				return AuthorizationToken.FromJson(data);
			}
			else
			{
				// form-url-encoded from body
				var values = HttpUtility.ParseQueryString(data);
				return AuthorizationToken.FromCollection(values);
			}
		}

		/// <summary>We are now going to call the provider's URL and get our user's profile information from them.</summary>
		/// <param name="token">The authorization token we got from the provider after they logged in.</param>
		/// <returns>A list of profile data.</returns>
		public virtual async Task<Dictionary<int, object>> GetProfileClaimsAsync(AuthorizationToken token)
		{
			if (!_IsConfigured) throw new NotConfiguredException();

			//Generate the needed querystring.
			NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
			queryString[AccessTokenParameterName] = token.AccessToken;
			//If any additional paramerters are needed, add them..
			if (Profile_AdditionalParams != null && Profile_AdditionalParams.Count > 0)
				foreach (string key in Profile_AdditionalParams)
					queryString[key] = Profile_AdditionalParams[key];

			//Create the new URL.
			//var url = Addresses.Profile.AddQuery(queryString);
			var url = Addresses.Profile;

			//Now we're going to go ask for our information!
			try
			{
				HttpClient client = new HttpClient();
				//var result = await client.GetAsync(url);

				var req = new HttpRequestMessage
				{
					RequestUri = url,
					Method = HttpMethod.Get,
				};
				req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
				req.Headers.UserAgent.Clear();
				req.Headers.UserAgent.Add(new ProductInfoHeaderValue("SpiraTeam", "1.0"));
				var result = await client.SendAsync(req);

				if (result.IsSuccessStatusCode)
				{
					var json = await result.Content.ReadAsStringAsync();
					var profile = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
					return GetClaimsFromProfile(profile);
				}
				else
				{
					//If it's not successful, we need to send the code back.
					string errMsg = "Error getting claims from provider:" +
						Environment.NewLine +
						"HTTP Error: " +
						((int)result.StatusCode).ToString() + " - " +
						result.ReasonPhrase;
					Dictionary<int, object> retValue = new Dictionary<int, object>()
					{
						{ERROR_CODE, errMsg}
					};
					return retValue;
				}
			}
			catch (Exception ex)
			{
				//If we threw an exceptioin here, we need to send it back.
				//If it's not successful, we need to send the code back.
				Dictionary<int, object> retValue = new Dictionary<int, object>()
					{
						{ERROR_CODE, "Error getting claims from provider:" + Environment.NewLine+ Common.DecodeException(ex)}
					};
				return retValue;
			}
		}

		/// <summary>Pulls the data we get back, converting it into an easier-to-access style, converting over any custom data.</summary>
		/// <param name="profile">The claims received from the provider.</param>
		/// <returns>A list of Claim objects.</returns>
		public Dictionary<int, object> GetClaimsFromProfile(Dictionary<string, object> profile)
		{
			CheckConfiguration();

			//Write out the info we got.
			foreach (var kvp in profile)
				Debug.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);

			//Handle all standard OpenID fields here..
			Dictionary<int, object> retVal = new Dictionary<int, object>();

			foreach (var item in profile)
			{
				switch (item.Key)
				{
					case "sub":
						retVal.Add((int)ClaimTypeEnum.NameIdentifier, item.Value);
						break;
					case "email":
						retVal.Add((int)ClaimTypeEnum.Email, item.Value);
						break;
					case "verified_email":
					case "email_verified":
						retVal.Add((int)ClaimTypeEnum.VerifiedEmail, item.Value);
						break;
					case "given_name":
						retVal.Add((int)ClaimTypeEnum.GivenName, item.Value);
						break;
					case "family_name":
						retVal.Add((int)ClaimTypeEnum.SurName, item.Value);
						break;
					case "picture":
						retVal.Add((int)ClaimTypeEnum.Picture, item.Value);
						break;
					case "locale":
						retVal.Add((int)ClaimTypeEnum.Locality, item.Value);
						break;
					default:
						break;
				}
			}

			//Load provider-specific claims.
			var provClaims = ProviderSpecificClaims(profile);
			foreach (var item in provClaims)
			{
				if (retVal.ContainsKey(item.Key))
					retVal[item.Key] = item.Value;
				else
					retVal.Add(item.Key, item.Value);
			}

			return retVal;
		}

		/// <summary>The access token parameter name, if different form the standard. Can be overridden by derived classes.</summary>
		public virtual string AccessTokenParameterName
		{
			get
			{
				return "access_token";
			}
		}

		/// <summary>Any additional parameters needed when redirecting the user to the Provider's URL.</summary>
		public virtual NameValueCollection Profile_AdditionalParams { get; }

		/// <summary>Retrieves any additional POST parameters needed for the OAuth call.</summary>
		public virtual Dictionary<string, string> Authentication_AdditionalParams()
		{
			return null;
		}
		#endregion

		#region Private Properties
		/// <summary>Flag stating whether we are properly init'd or not.</summary>
		protected bool _IsInit;

		/// <summary>Flag stating wiether we are being set up or not.</summary>
		protected bool _IsInInit;
		#endregion

		/// <summary>Creates a new instance of the class!</summary>
		public Provider()
		{ }

		string RedirectUrl
		{
			get
			{
				//Throw error if we are not configured!
				if (!_IsConfigured) throw new NotConfiguredException();

                string baseUrl = ConfigurationSettings.Default.General_WebServerUrl;
                if (String.IsNullOrWhiteSpace(baseUrl))
                {
                    return "";
                }
                string url = baseUrl + ((baseUrl.EndsWith("/")) ? "" : "/") + OAuth2Client.OAuthCallbackUrl;

                //Return the url (keep the case)
                return url;

			}
		}

		/// <summary>Retrievs the URL in which to send the client when they want to log on to their provider.</summary>
		/// <returns></returns>
		public AuthorizationRedirect GetRedirect()
		{
			//Throw error if we are not configured!
			if (!_IsConfigured) throw new NotConfiguredException();

			var client = ClientId;
			var redirect = RedirectUrl;
			var state = Base64Url.Encode(CryptoRandom.CreateRandomKey(10));

			//Generate the URL to call.
			Uri authUrl = new Uri(Addresses.Authorization.ToString(), UriKind.Absolute);

			//Create the parameters.
			Dictionary<string, string> param = new Dictionary<string, string>
			{
				{"client_id", client },
				{"redirect_uri", redirect},
				{"state", state },
				{"response_type", "code" },
				{"scope", Scope }
			};

			//Merge anything we get from the unique provider.
			Dictionary<string, string> param_prov = Authentication_AdditionalParams();
			if (param_prov != null && param_prov.Count > 0)
			{
				//Loop through all the ones we got.
				foreach (var item in param_prov)
				{
					if (param.ContainsKey(item.Key))
						param[item.Key] = item.Value;
					else
						param.Add(item.Key, item.Value);
				}
			}

			//Now add our values to the URL.
			foreach (var item in param)
				authUrl = authUrl.AddQuery(item.Key, item.Value);

			var ctx = new AuthorizationRedirect
			{
				AuthorizationUrl = authUrl.ToString(),
				State = state
			};
			return ctx;
		}

		/// <summary>Returns our token from the retutrn call from the provider.</summary>
		/// <param name="ctx">The Context, so we know which provider we're using.</param>
		/// <param name="queryString">The querystring of the return call.</param>
		/// <returns></returns>
		public async Task<AuthorizationToken> GetTokenFromCallbackAsync(AuthorizationContext ctx, NameValueCollection queryString)
		{
			//Throw error if we are not configured!
			if (!_IsConfigured) throw new NotConfiguredException();

			if (ctx.ProviderId != ProviderId) throw new Exception("Invalid AuthorizationCodeProvider name.");

			string error = queryString["error"];
			if (!string.IsNullOrWhiteSpace(error))
			{
				var res = new AuthorizationToken { Error = error };
				if (!string.IsNullOrWhiteSpace(queryString["error_description"]))
					res.ErrorDetails = queryString["error_description"];

				return res;
			};

			string state = queryString["state"];
			if (ctx.State != state) return new AuthorizationToken
			{
				Error = "State does not match." +
					Environment.NewLine +
					"Query: '" + state +
					"' vs Expected: '" + ctx.State + "'"
			};

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

			//Debug logging.
			if (postValues != null && postValues.Count > 0)
			{
				string msg = "Using values: {";
				foreach (var kvp in postValues)
					msg += "\"" + kvp.Key + "\":\"" + kvp.Value.ToString() + "\",";
				msg += "}";
				Logger.LogTraceEvent("OAuth2.Provider.GetTokenFromCallbackAsync()", msg);
			}

			return await GetTokenFromCallbackInternalAsync(postValues);
		}

		/// <summary>Called when we are posted back to from the Provider. This is halding the call after the user (un-)successfully logged in.</summary>
		/// <param name="authCtx">The context information</param>
		/// <param name="nameValueCollection">Parameters from the Querystring</param>
		/// <returns></returns>
		public async Task<CallbackResult> ProcessCallbackAsync(AuthorizationContext authCtx, NameValueCollection nameValueCollection)
		{
			//Throw error if we are not configured!
			if (!_IsConfigured) throw new NotConfiguredException();

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

		/// <summary>Simple function to tell us if we are init'd.</summary>
		/// <returns>True if functions can be called. False if error or not configured.</returns>
		private bool _IsConfigured { get { return !_IsInInit && _IsInit; } }

		/// <summary>For subclasses to call to make sure they've been configured.</summary>
		protected void CheckConfiguration()
		{
			if (!_IsConfigured)
				throw new NotConfiguredException();
		}

		#region Helper Classes
		/// <summary>The bits of data returned!</summary> 
		public enum ClaimTypeEnum : int
		{
			#region Default Claims
			/// <summary>The URI for a claim that specifies the instant at which an entity was authenticated</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant</remarks> 
			AuthenticationInstant = 1,
			/// <summary>The URI for a claim that specifies a deny-only security identifier (SID) for an entity. A deny-only SID denies the specified entity to a securable object.</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/denyonlysid</remarks> 
			DenyOnlySid = 2,
			/// <summary>The URI for a claim that specifies the email address of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email</remarks> 
			Email = 3,
			/// <summary>The URI for a claim that specifies the gender of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender</remarks> 
			Gender = 4,
			/// <summary>The URI for a claim that specifies the given name of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname</remarks> 
			GivenName = 5,
			/// <summary>The URI for a claim that specifies a hash value</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash</remarks> 
			Hash = 6,
			/// <summary>The URI for a claim that specifies the home phone number of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone</remarks> 
			HomePhone = 7,
			/// <summary>The URI for a claim that specifies the locale in which an entity resides</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality</remarks> 
			Locality = 8,
			/// <summary>The URI for a claim that specifies the mobile phone number of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone</remarks> 
			MobilePhone = 9,
			/// <summary>The URI for a claim that specifies the name of an entiy</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name</remarks> 
			Name = 10,
			/// <summary>The URI for a claim that specifies the Identity Name of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier</remarks> 
			NameIdentifier = 11,
			/// <summary>The URI for a claim that specifies the alternative phone number of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone</remarks> 
			OtherPhone = 12,
			/// <summary>The URI for a claim that specifies the postal code of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode</remarks> 
			PostalCode = 13,
			/// <summary>The URI for a claim that specifies an RSA key</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/rsa</remarks> 
			Rsa = 14,
			/// <summary>The URI for a claim that specifies a security identifier (SID)</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid</remarks> 
			Sid = 15,
			/// <summary>The URI for a claim that specifies a service principal name (SPN) claim</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/spn</remarks> 
			Spn = 16,
			/// <summary>The URI for a claim that specifies the state or province in which an entity resides</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince</remarks> 
			StateOrProvince = 17,
			/// <summary>The URI for a claim that specifies the street address of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress</remarks> 
			StreetAddress = 18,
			/// <summary>The URI for a claim that specifies the surname of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname</remarks> 
			SurName = 19,
			/// <summary>The URI for a claim that identifies the system entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system</remarks> 
			System = 20,
			/// <summary>The URI for a claim that specifies a thumbprint, a thumbprint is a globally unique SHA-1 hash of an X.509 certificate</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/thumbprint</remarks> 
			Thumbprint = 21,
			/// <summary>The URI for a claim that specifies a user principal name (UPN)</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn</remarks> 
			Upn = 22,
			/// <summary>The URI for a claim that specifies a URI</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri</remarks> 
			Uri = 23,
			/// <summary>The URI for a claim that specifies the webpage of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/webpage</remarks> 
			Webpage = 24,
			/// <summary>The URI for a claim that specifies the DNS name associated with the computer name or with the  
			///   alternative name of either the subject or issuer of an X.509 certificate</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dns</remarks> 
			Dns = 25,
			/// <summary>The URI for a claim that specifies the date of birth of an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth</remarks> 
			DateOfBirth = 26,
			/// <summary>The URI for a claim that specifies the country/region in which an entity resides</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country</remarks> 
			Country = 27,
			/// <summary>The URI for a claim that specifies an authorization decision on an entity</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authorizationdecision</remarks> 
			AuthorizationDecision = 28,
			/// <summary>The URI for a claim that specifies the method with which an entity was authenticated</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod</remarks> 
			AuthenticationMethod = 29,
			/// <summary>The URI for a claim that specifies the cookie path</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/cookiepath</remarks> 
			CookiePath = 30,
			/// <summary>The URI for a claim that specifies the deny-only primary SID on an entity.  
			///		A deny-only SID denies the specified entity to a securable object.</summary> 
			///	<remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarysid</remarks> 
			DenyOnlyPrimarySid = 31,
			/// <summary>The URI for a claim that specifies the deny-only primary group SID on an entity. 
			///		A deny-only SID denies the specified entity to a securable object.</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlyprimarygroupsid</remarks> 
			DenyOnlyPrimaryGroupSid = 32,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/denyonlywindowsdevicegroup</remarks> 
			DenyOnlyWindowsDeviceGroup = 33,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/dsa</remarks> 
			Dsa = 34,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/expiration</remarks> 
			Expiration = 35,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/expired</remarks> 
			Expired = 36,
			/// <summary>The URI for a claim that specifies the SID for the group of an entity</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/groupsid</remarks> 
			GroupSid = 37,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/ispersistent</remarks> 
			IsPersistent = 38,
			/// <summary>The URI for a claim that specifies the primary group SID of an entity</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/primarygroupsid</remarks> 
			PromaryGroupsSid = 39,
			/// <summary>The URI for a distinguished name claim of an X.509 certificate. The X.500 standard defines the  
			///		methodology for defining distinguished names that are used by X.509 certificates.</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/x500distinguishedname</remarks> 
			X500DistinguishedName = 40,
			/// <summary>The URI for a claim that specifies the primary SID of an entity</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid</remarks> 
			PrimarySid = 41,
			/// <summary>The URI for a claim that specifies a serial number</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/serialnumber</remarks> 
			SerialNumber = 42,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata</remarks> 
			UserData = 43,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/version</remarks> 
			Version = 44,
			/// <summary>The URI for a claim that specifies the Windows domain account name of an entity</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname</remarks> 
			WindowsAccountName = 45,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsdeviceclaim</remarks> 
			WindowsDeviceClaim = 46,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsdevicegroup</remarks> 
			WindowsDeviceGroup = 47,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsuserclaim</remarks> 
			WindowsUserClaim = 48,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsfqbnversion</remarks> 
			WindowsFqbnVersion = 49,
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/windowssubauthority</remarks> 
			WindowsSubAuthority = 50,
			/// <summary>The URI for a claim that specifies the anonymous user</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous</remarks> 
			Anonymous = 51,
			/// <summary>The URI for a claim that specifies details about whether an identity is authenticated</summary> 
			/// <remarks>http://schemas.xmlsoap.org/ws/2005/05/identity/claims/authenticated</remarks> 
			Authentication = 52,
			/// <summary>The URI for a claim that specifies the role of an entity</summary> 
			/// <remarks>http://schemas.microsoft.com/ws/2008/06/identity/claims/role</remarks> 
			Role = 53,
			/// <remarks>http://schemas.xmlsoap.org/ws/2009/09/identity/claims/actor</remarks> 
			Actor = 54,
			#endregion
			#region Additional Claims
			/// <summary>The URL (or image data?) given by the Provider.</summary>
			/// <remarks>Supported by at least Google & Facebook</remarks>
			Picture = 1000,
			/// <summary>Boolean on whether the email address has been verified or not.</summary>
			VerifiedEmail = 1001,
			/// <summary>Indicatge's a user's chosen username.</summary>
			/// <remarks></remarks>
			PreferredUsername = 1002
			#endregion
		}
		#endregion
	}
}