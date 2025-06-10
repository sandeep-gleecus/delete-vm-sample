namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;

	public class GoogleProvider : Provider
	{
		/* Information needed for Testing:
		 * Client ID:  570197971578-ruq1g4q4ulcukvvhiptpoubge2l74hg7.apps.googleusercontent.com
		 * Secret Key: xBbCsJRsx8BljF6xLUSdWYsZ
		 */

		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("CBA68583-4D3C-4AAA-8472-E225B06C392C"),
			Name = "Google",
			Description = "Allows users to log in with their Google account."
		};
		#endregion

		/// <summary>Configured addresses for this provider.</summary>
		public override ProviderURLs Addresses
		{
            get
            {
                return new ProviderURLs
                {
                    Authorization = new Uri("https://accounts.google.com/o/oauth2/auth"),
                    Profile = new Uri("https://www.googleapis.com/oauth2/v1/userinfo"),
                    Token = new Uri("https://accounts.google.com/o/oauth2/token")
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
					//return "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";
					return "openid profile email";
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
						case "verified_email":
							retVal.Add((int)ClaimTypeEnum.VerifiedEmail, item.Value);
							break;
						case "id":
							retVal.Add((int)ClaimTypeEnum.NameIdentifier, item.Value);
							break;
					}
				}

			return retVal;
		}

	}
}
