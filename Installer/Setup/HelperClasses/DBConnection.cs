namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	public class DBConnection
	{
		#region Constructors
		/// <summary>Creates a new empty class.</summary>
		public DBConnection() { }

		/// <summary>Creates a new class with set values.</summary>
		/// <param name="dbName">The name of the database.</param>
		/// <param name="dbServer">The server name and instance.</param>
		/// <param name="authType">What kind of login we have.</param>
		/// <param name="user">If using SQL Auth, the login to use.</param>
		/// <param name="pass">If using SQL Auth, the password to use for the login.</param>
		public DBConnection(string dbName, string auditdbName, string dbServer, AuthenticationMode authType, string user, string pass, string auditdbServer, AuthenticationMode auditauthType, string audituser, string auditpass)
		{
			AuditDatabaseName = auditdbName;
			DatabaseName = dbName;
			DatabaseServer = dbServer;
			LoginAuthType = authType;
			LoginUser = user;
			LoginPassword = pass;
			AuditDatabaseServer = auditdbServer;
			AuditLoginAuthType = auditauthType;
			AuditLoginUser = audituser;
			AuditLoginPassword = auditpass;
		}
		#endregion

		#region Properties
		/// <summary>The name of the database.</summary>
		public string DatabaseName { get; set; }

		/// <summary>The name of the  audit database.</summary>
		public string AuditDatabaseName { get; set; }

		/// <summary>The SQL Server and instance.</summary>
		public string DatabaseServer { get; set; }

		/// <summary>The Authorization Type of the login.</summary>
		public AuthenticationMode LoginAuthType { get; set; }

		/// <summary>The SQL login to use.</summary>
		public string LoginUser { get; set; }

		/// <summary>The password of the SQL Login to use.</summary>
		public string LoginPassword { get; set; }

		/// <summary>The SQL Server and instance.</summary>
		public string AuditDatabaseServer { get; set; }

		/// <summary>The Authorization Type of the login.</summary>
		public AuthenticationMode AuditLoginAuthType { get; set; }

		/// <summary>The SQL login to use.</summary>
		public string AuditLoginUser { get; set; }

		/// <summary>The password of the SQL Login to use.</summary>
		public string AuditLoginPassword { get; set; }
		#endregion
	}
}
