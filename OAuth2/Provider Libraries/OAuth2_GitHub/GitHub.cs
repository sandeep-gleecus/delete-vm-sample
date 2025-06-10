namespace Inflectra.OAuth2.CLient
{
	using Inflectra.OAuth2;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class GitHubProvider : Provider
	{
		/* Information needed for Testing:
		 * Client ID:  a41e77c02123f4493fb8
		 * Secret Key: f542dbef3a81ba2cc51ef0a77d8b9b180e8bcff9
		 */

		/* Testing Test Set:
		 * Client ID:  73aa941fbcb102123ffb
		 * Secret Key: bd0b090f813f4c1685a5074828e1c8807e1d7608
		 */

		/// <summary>Our scope string.</summary>
		private string _scope;

		#region Static Props/Methods
		/// <summary>Definition of this provider.</summary>
		private readonly static ProviderType _provider = new ProviderType
		{
			Identification = new Guid("C74F8326-D95E-4649-AB92-D450B57F6D77"),
			Name = "GitHub",
			Description = "Allows users to log in with their GitHub account."
		};
		#endregion

		public override ProviderURLs Addresses
		{
			get
			{
				return new ProviderURLs
				{
					Authorization = new Uri("https://github.com/login/oauth/authorize"),
					Profile = new Uri("https://api.github.com/user"),
					Token = new Uri("https://github.com/login/oauth/access_token")
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
				foreach (var item in profile.Where(f => f.Value != null))
				{
					switch (item.Key)
					{
						case "login":
							retVal.Add((int)ClaimTypeEnum.PreferredUsername, item.Value);
							break;
						case "id":
							retVal.Add((int)ClaimTypeEnum.NameIdentifier, item.Value);
							break;
						case "name":
							if (item.Value != null && !string.IsNullOrWhiteSpace(item.Value.ToString()))
							{
								string name = item.Value.ToString().Trim();
								if (name.Contains(" "))
								{
									//Split on spaces.
									string[] names = name.Split(' ');

									//First one is the first name. Last one is the last name. Ignore any middles.
									retVal.Add((int)ClaimTypeEnum.GivenName, names[0]);
									retVal.Add((int)ClaimTypeEnum.SurName, names[names.Length - 1]);
								}
								else
								{
									//One name. Put it in the first name.
									retVal.Add((int)ClaimTypeEnum.GivenName, name);
								}
							}
							break;
						case "avatar_url":
							retVal.Add((int)ClaimTypeEnum.Picture, item.Value);
							break;

					}

				}

			return retVal;
		}

	}
}
