namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;

	public class GitLabProvider : Provider
	{
		/* Information needed for Testing:
		 * Client ID:  d086d0b855674c0709ec6a6b13a777ddb44ede4765c16f07ea0640139d99ba8b
		 * Secret Key: 44d5898cdefeef96a2c117a55d2cb35b8c108074af492e1ae419650f17f3001c
		 */

		/// <summary>Our scope string.</summary>
		private string _scope;

		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("9FF81A10-9C65-47D7-9E71-BE1965849740"),
			Name = "GitLab",
			Description = "Allows users to log in with their GitLab registration."
		};
		#endregion

		public override ProviderURLs Addresses
		{
            get
            {
                return new ProviderURLs
                {
                    Authorization = new Uri("https://gitlab.com/oauth/authorize"),
                    Profile = new Uri("https://gitlab.com/oauth/userinfo"),
                    Token = new Uri("https://gitlab.com/oauth/token")
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
		public override string Scope
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_scope))
					return "openid read_user profile email";
				else

					return _scope;
			}
			set
			{
				_scope = value;
			}
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
