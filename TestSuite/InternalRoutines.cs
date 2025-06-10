using Inflectra.SpiraTest.Web.Classes.HttpSimulator;
using System;
using System.Configuration;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace Inflectra.SpiraTest.TestSuite
{
    /// <summary>
    /// Contains helper functions used internally by the test fixtures.
    /// </summary>
    public static class InternalRoutines
	{
        const string ENTITY_CONNECTION_STRING = "SpiraTestEntities";

		public const int SPIRATEST_INTERNAL_PROJECT_ID = Common.Global.PROJECT_ID_SPIRA;
        public const int SPIRATEST_INTERNAL_CURRENT_RELEASE_ID = Common.Global.RELEASE_ID_SPIRA;
        public const string SPIRATEST_INTERNAL_URL = "https://spira.inflectra.com";
		public const string SPIRATEST_INTERNAL_LOGIN= "nunitrunner";
		public const string SPIRATEST_INTERNAL_PASSWORD = "auto7821";
        public const string VERSION_CONTROL_FOLDERPATH = @"C:\\Git\\SpiraTeam\\SpiraTest\\SpiraCache\\VersionControlCache";
        public const string APP_LOCATION = @"C:\Git\SpiraTeam\SpiraTest";

		public const int INCIDENT_STATUS_NEW = 1;
		public const int INCIDENT_STATUS_OPEN = 2;
		public const int INCIDENT_STATUS_ASSIGNED = 3;
		public const int INCIDENT_STATUS_RESOLVED = 4;
		public const int INCIDENT_STATUS_CLOSED = 5;
		public const int INCIDENT_STATUS_NOT_REPRODUCIBLE = 6;
		public const int INCIDENT_STATUS_DUPLICATE = 7;
		public const int INCIDENT_STATUS_REOPEN = 8;

		public const int PROJECT_ROLE_PROJECT_OWNER = 1;
		public const int PROJECT_ROLE_MANAGER = 2;
		public const int PROJECT_ROLE_DEVELOPER = 3;
		public const int PROJECT_ROLE_TESTER = 4;
		public const int PROJECT_ROLE_OBSERVER = 5;
		public const int PROJECT_ROLE_INCIDENT_USER = 6;

        public const double UTC_OFFSET = 0;    //Universal Coordinated Time (UTC)

        /// <summary>
        /// Executes a database query, often used to clean up data
        /// </summary>
        /// <param name="sql">The query to execute</param>
        public static void ExecuteNonQuery(string sql)
        {
            //First get the Entity connection string
            ConnectionStringSettings inflectraEntities = System.Configuration.ConfigurationManager.ConnectionStrings[ENTITY_CONNECTION_STRING];
            if (inflectraEntities == null)
            {
                throw new ArgumentException("Specified named connection string '" + ENTITY_CONNECTION_STRING + "' was not found in the configuration file.");
            }

            //Now extract the actual database provider connection string
            EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder(inflectraEntities.ConnectionString);
            string providerConnection = ecsb.ProviderConnectionString;
            using (SqlConnection sqlConnection = new SqlConnection(providerConnection))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = sql;
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    sqlCommand.ExecuteNonQuery();
                }

                sqlConnection.Close();
            }
        }

        /// <summary>
        /// Executes a database query and gets back the first string value
        /// </summary>
        /// <param name="sql">The query to execute</param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql)
        {
            object result;
            //First get the Entity connection string
            ConnectionStringSettings inflectraEntities = System.Configuration.ConfigurationManager.ConnectionStrings[ENTITY_CONNECTION_STRING];
            if (inflectraEntities == null)
            {
                throw new ArgumentException("Specified named connection string '" + ENTITY_CONNECTION_STRING + "' was not found in the configuration file.");
            }

            //Now extract the actual database provider connection string
            EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder(inflectraEntities.ConnectionString);
            string providerConnection = ecsb.ProviderConnectionString;
            using (SqlConnection sqlConnection = new SqlConnection(providerConnection))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    sqlCommand.CommandText = sql;
                    sqlCommand.CommandType = System.Data.CommandType.Text;
                    result = sqlCommand.ExecuteScalar();
                }

                sqlConnection.Close();
            }
            return result;
        }

		/// <summary>
		/// Makes the test runner wait for a specified period of time
		/// </summary>
		/// <param name="seconds">The number of seconds to wait</param>
		internal static void Wait (int seconds)
		{
			DateTime startTime = DateTime.Now;
			while (DateTime.Now < startTime.AddSeconds(seconds));
		}

        /// <summary>
        /// Sets up a simulated HTTP context if needed in the tests
        /// </summary>
        /// <param name="host"></param>
        /// <param name="application"></param>
        public static void SetHttpContextWithSimulatedRequest(string host, string application)
        {
            string appVirtualDir = "/";
            string appPhysicalDir = @"C:\\Subversion\\Projects\\SpiraTeam\\Trunk\\SpiraTest";
            string page = application.Replace("/", string.Empty) + "/Default.aspx";
            string query = string.Empty;
            TextWriter output = null;

            SimulatedHttpRequest workerRequest = new SimulatedHttpRequest(appVirtualDir, appPhysicalDir, Path.Combine(appPhysicalDir, page), page, query, output, host, 80, "GET");
            HttpContext.Current = new HttpContext(workerRequest);
        }

        /// <summary>
        /// Helper method that makes switching from DataSets > Entities much easier in terms of null handling
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(this int? obj)
        {
            return !obj.HasValue;
        }

        /// <summary>
        /// Helper method that makes switching from DataSets > Entities much easier in terms of null handling
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(this DateTime? obj)
        {
            return !obj.HasValue;
        }

        /// <summary>
        /// Helper method that makes switching from DataSets > Entities much easier in terms of null handling
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(this string obj)
        {
            return String.IsNullOrEmpty(obj);
        }

		/// <summary>
		/// Generates a repeating string from the provided fragment
		/// </summary>
		/// <param name="fragment">The string fragment to repeat</param>
		/// <param name="repeat"></param>
		internal static string RepeatString(string fragment, int repeat)
		{
			string concat = "";

			//Iterate the number of times and create the string
			for (int i = 0; i < repeat; i++)
			{
				concat = concat + fragment;
			}
			return concat;
		}
	}
}
