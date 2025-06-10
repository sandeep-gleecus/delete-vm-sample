namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics;

	public class AzureADProvider : Provider
	{
		/* Information needed for Testing:
		 * https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-sign-user-overview
		 */

		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("D5994FAE-E14F-44C5-9596-8B853ABC0E09"),
			Name = "Microsoft ADFS",
			Description = "Allows users to log in with their Enterprise Active Directory account."
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
		public override string Scope
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_scope))
					return "email openid profile";
				else
					return _scope;
			}
			set
			{
				_scope = value;
			}
		}
		private string _scope;

		/// <summary>Adds the missing info sent to Microsoft.</summary>
		/// <returns></returns>
		public override Dictionary<string, string> Authentication_AdditionalParams()
		{
			//Because M$ does not declare a 'default', we need to tell them we're expecting the info
			//  back as part of the query string.
			Dictionary<string, string> retList = new Dictionary<string, string>
			{
				{"response_mode", "query" },
				{"resource", "urn:microsoft:userinfo" }
			};

			return retList;
		}

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
					Debug.WriteLine("DEBUG  " + item.Key + ": " + item.Value.ToString());
				}

			return retVal;
		}
	}
}
