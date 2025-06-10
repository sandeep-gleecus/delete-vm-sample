namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;

	public class AzureADProvider : Provider
	{
		/* To create account, see here: https://docs.microsoft.com/en-us/graph/auth-register-app-v2
		 * Add a permission - Microsoft Graph, "Delegated", "email", "offline_access", "openid", "profile"
		 * https://portal.azure.com

		/* Information needed for Testing:
		 * Client ID:  185c17d0-b1e6-4f90-b56b-32cb32e480c6
		 * Secret Key: v:Q7aZ[KSEp3m-Ll?ahnk1iYhielPFZ4
		 */

		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("1377C5EB-78BB-4AF6-A7A4-86C2281112F6"),
			Name = "Azure AD",
			Description = "Allows users to log in with their Microsoft (Azure, Skype, Bing) accounts."
		};

		//Default URLs.
		private static string URL_Auth = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
		private static string URL_Token = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
		private static string URL_Profile = "https://graph.microsoft.com/oidc/userinfo";
		#endregion

		public AzureADProvider()
		{
			//Set up Address Defaults.
			providerUrls = new ProviderURLs
			{
				Authorization = new Uri(URL_Auth),
				Profile = new Uri(URL_Profile),
				Token = new Uri(URL_Token)
			};
		}

		/// <summary>The addresses for this provider.</summary>
		public override ProviderURLs Addresses
		{
			get
			{
				return providerUrls;
			}
		}
		private ProviderURLs providerUrls;

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

		/// <summary>The URL to direct the user.</summary>
		public override Uri UrlAuthorization
		{
			set
			{
				providerUrls.Authorization = value;
			}
		}

		/// <summaryThe token URL.</summary>
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
				//This is hard-coded in M$ crap.
			}
		}

		/// <summary>False, since we have our own hard-coded.</summary>
		public override bool IsUrlsRequired => true;

		/// <summary>True, since we have our own logo.</summary>
		public override bool IsLogoProvided => true;

		/// <summary>Private storage of the default scope.</summary>
		public override string Scope
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_scope))
					return "openid profile email offline_access";
				else

					return _scope;
			}
			set
			{
				_scope = value;
			}
		}
		private string _scope;

		#region Setup Functions
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
		#endregion

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
						case "nickname":
							retVal.Add((int)ClaimTypeEnum.PreferredUsername, item.Value);
							break;
					}
				}

			return retVal;
		}

	}
}
