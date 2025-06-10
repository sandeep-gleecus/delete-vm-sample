using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Inflectra.SpiraTest.Web.Services.v6_0.DataObjects
{
	/// <summary>
	/// Stores the source code information associated with a Spira project
	/// </summary>
	public class RemoteSourceCodeConnection
	{
		/// <summary>
		/// The provider name (GitProvider SubversionProvider) unless TaraVault is being used, in which
		/// case it will return either SVN or Git
		/// </summary>
		public string ProviderName;

		/// <summary>
		/// The connection string (typically a url) for this source code repository
		/// </summary>
		public string Connection;

		/// <summary>
		/// The login to the source code provider (TaraVault only)
		/// </summary>
		public string Login;

		/// <summary>
		/// The password / API key to the source code provider (TaraVault only)
		/// </summary>
		public string Password;
	}
}
