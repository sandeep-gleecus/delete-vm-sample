namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;

	public class OKTAProvider : Provider
	{
		/* Information needed for Testing:
		 * Client ID:  0oauxxsuqatA2enlp356
		 * Secret Key: SHlEeMYFUVtG6JO8hVosyxOB0vUeT1LM3DvMEvWz
		 * 
		 * Auth URL: https://dev-937609.okta.com/oauth2/default/v1/authorize
		 * Token URL: https://dev-937609.okta.com/oauth2/default/v1/token
		 * Profile URL: https://dev-937609.okta.com/oauth2/default/v1/userinfo
		 * 
		 * dev-937609.okta.com
		 */

		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("22FD65ED-CC98-4487-A99E-179C98ABDF3B"),
			Name = "Okta",
			Description = "Allows users to log in with an OKTA account."
		};
		#endregion

		/// <summary>Configured addresses for this provider.</summary> 
		public override ProviderURLs Addresses
		{
			get
            {
                return providerUrls;
            }
		}
		private ProviderURLs providerUrls = new ProviderURLs();

		/// <summary>The name of this provider!</summary>
		public override string Name => _provider.Name;

		/// <summary>The unique ID of this provider!</summary>
		public override Guid ProviderId => _provider.Identification;

		/// <summary>The unique ID of this provider!</summary>
		public override string Description => _provider.Description;

		/// <summary>The full definition of this provider.</summary>
		public override ProviderType ProviderDefinition => _provider;

		/// <summary>The user-specific Client ID that this client should use when calling the provider.</summary>
		public override string ClientId { get; set; }

		/// <summary>The user-specific Secret Key that this client should use when calling the provider.</summary>
		public override string ClientSecretKey { get; set; }

		/// <summary>Cannot be set in this provider.</summary>
		public override Uri UrlAuthorization
		{
			set
			{
				providerUrls.Authorization = value;
			}
		}

		/// <summary>Cannot be set in this provider.</summary>
		public override Uri UrlToken
		{
			set
			{
				providerUrls.Token = value;
			}
		}

		/// <summary>Cannot be set in this provider.</summary>
		public override Uri UrlProfile
		{
			set
			{
				providerUrls.Profile = value;
			}
		}

		/// <summary>False, since we have our own hard-coded.</summary>
		public override bool IsUrlsRequired => true;

		/// <summary>True, since we have our own logo.</summary>
		public override bool IsLogoProvided => true;

		/// <summary>Private storage of the default scope.</summary>
		private string _scope;
		public override string Scope
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_scope))
					return "openid profile email address phone";
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
			else if (providerUrls.Authorization == null || providerUrls.Profile == null || providerUrls.Token == null)
			{
				throw new Exception("Login URLs are required to operate!");
			}
			else
			{
				_IsInInit = false;
				_IsInit = true;
			}
		}

		/// <summary>Lets each individual provider library handle any non-standard claims.</summary>
		/// <param name="profile">Profile claims.</param>
		/// <returns>A list of provider-spefici claims.</returns>
		public override Dictionary<int, object> ProviderSpecificClaims(Dictionary<string, object> profile)
		{
			Dictionary<int, object> retVal = new Dictionary<int, object>();

			if (profile != null)
				foreach (var item in profile)
				{
					switch (item.Key)
					{
						case "preferred_username":
							retVal.Add((int)ClaimTypeEnum.PreferredUsername, item.Value);
							break;
					}
				}

			return retVal;
		}
	}
}
