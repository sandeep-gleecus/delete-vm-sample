using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Inflectra.SpiraTest.ApiTestSuite.Rest_Code_Generation.SpiraRestService60
{
    /// <summary>
    /// Adds any additional classes that are not automatically serialized
    /// </summary>
    public partial class SpiraRestClient60
    {
        public const string DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fff";

        /// <summary>
        /// Default constructor
        /// </summary>
        public SpiraRestClient60()
        {

        }

        /// <summary>
        /// Constructor when you want to specify the endpoint base URL
        /// </summary>
        /// <param name="baseUrl">The base url of the REST endpoint</param>
        public SpiraRestClient60(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        /// <summary>
        /// Adds a parameter to the request (nulls become empty string)
        /// </summary>
        /// <param name="request">The REST request</param>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="parameterValue">The parameter value</param>
        public void AddRequestParameter(RestRequest request, string parameterName, object parameterValue)
        {
            if (!String.IsNullOrEmpty(parameterName))
            {
                if (parameterValue == null)
                {
                    RestParameter restParameter = new RestParameter();
                    restParameter.Name = parameterName;
                    restParameter.Value = "null";
                    request.Parameters.Add(restParameter);
                }
                else
                {
                    RestParameter restParameter = new RestParameter();
                    restParameter.Name = parameterName;
                    //Serialize dates correctly
                    if (parameterValue is DateTime)
                    {
                        restParameter.Value = ((DateTime)parameterValue).ToString(DATETIME_FORMAT);
                    }
                    else if (parameterValue is DateTime?)
                    {
                        restParameter.Value = ((DateTime?)parameterValue).Value.ToString(DATETIME_FORMAT);
                    }
                    else
                    {
                        restParameter.Value = parameterValue.ToString();
                    }
                    request.Parameters.Add(restParameter);
                }
            }
        }

        /// <summary>
        /// Authenticates against the server. Need to call before using other methods.
        /// This overload allows you to use the same API Key / RSS Token as the REST Service
        /// </summary>
        /// <param name="userName">The username of the user</param>
        /// <param name="apiKey">The user's API Key / RSS Token</param>
        /// <param name="plugInName">The name of the plug-in</param>
        /// <returns>The credentials object if successful, NULL if not</returns>
        /// <remarks>Also checks to make sure they have enough connection licenses</remarks>
        public RemoteCredentials Connection_Authenticate(string userName, string apiKey, string plugInName)
        {
            //Create the request
            RestRequest request = new RestRequest("Connection_Authenticate2");
            request.Credential = new System.Net.NetworkCredential();
            request.Credential.UserName = userName;
            request.Credential.Password = apiKey;
            request.Method = "GET";
            request.Url = baseUrl + "/" + "users";
            request.Headers.Add(new RestHeader() { Name = StandardHeaders.Content_Type, Value = "application/json" });

            //Authenticate against the web service by calling a simple method
            RestClient restClient = new RestClient(request);
            RestResponse response = restClient.SendRequest();
            if (!response.IsErrorStatus && response.Headers.FirstOrDefault(h => h.Name == Properties.Resources.Rest_StatusCode).Value == "200 OK" && !String.IsNullOrEmpty(response.Body))
            {
                //Deserialize the user
                RemoteUser remoteUser = JsonConvert.DeserializeObject<RemoteUser>(response.Body);

                RemoteCredentials remoteCredentials = new RemoteCredentials();
                remoteCredentials.UserId = remoteUser.UserId.Value;
                remoteCredentials.UserName = userName;
                remoteCredentials.PlugInName = plugInName;
                remoteCredentials.IsSystemAdmin = remoteUser.Admin;
                remoteCredentials.ApiKey = apiKey;

                return remoteCredentials;
            }
            
            //Failure
            return null;
        }
    }

    #region Filter/Sort objects

    public class RemoteSort
    {
        /// <summary>
        /// The name of the field that we want to sort on
        /// </summary>
        public string PropertyName = "";

        /// <summary>
        /// Set true to sort ascending and false to sort descending
        /// </summary>
        public bool SortAscending = true;
    }

    public class MultiValueFilter
    {
        /// <summary>
        /// Contains the list of specified values to filter on
        /// </summary>
        public int[] Values { get; set; }

        /// <summary>
        /// Do we have the special case of a filter for (None) - i.e. all records that have no value set
        /// </summary>
        public bool IsNone { get; set; }
    }

    /// <summary>
    /// Represents a date-range that is used in filters, etc.
    /// </summary>
    public class DateRange
    {
        /// <summary>
        /// Do we want to consider times when filtering
        /// </summary>
        /// <remarks>This is not used interally by the class, just tracked for the client's benefit</remarks>
        public bool ConsiderTimes { get; set; }

        /// <summary>
        /// The starting date of the date-range
        /// </summary>
        public Nullable<DateTime> StartDate { get; set; }

        /// <summary>
        /// The ending date of the date-range
        /// </summary>
        public Nullable<DateTime> EndDate { get; set; }
    }

    #endregion

    #region Data Objects that are not serialized

    /// <summary>
    /// Represents the credentials used to connect to the SOAP API
    /// </summary>
    /// <remarks>
    /// The login is always populated. Either the password or API is populated
    /// </remarks>
    public class RemoteCredentials
    {
        /// <summary>
        /// The ID of the user. This is read-only and not needed for authentication, it is just returned by the API
        /// when you use the connection option as a reference
        /// </summary>
        public int UserId;

        /// <summary>
        /// A valid login for connecting to the system
        /// </summary>
        public string UserName;

        /// <summary>
        /// The API Key (RSS Token) for the user nsme
        /// </summary>
        /// <remarks>REST calls only support API Keys, not passwords</remarks>
        public string ApiKey;

        /// <summary>
        /// The name of the client/plugin calling the API Key (optional)
        /// </summary>
        public string PlugInName;

        /// <summary>
        /// Is the user a system admin. This is read-only and returned by the API.
        /// </summary>
        public bool IsSystemAdmin;
    }

    /// <summary>
    /// Represents a single project custom property option entry.
    /// Examples of options include: maximum length, minimum value, etc.
    /// </summary>
    public class RemoteCustomPropertyOption
    {
        /// <summary>
        /// The id of the custom property option. Allowed values are:
        ///     AllowEmpty = 1,
        ///     MaxLength = 2,
        ///     MinLength = 3,
        ///     RichText = 4,
        ///     Default = 5,
        ///     MaxValue = 6,
        ///     MinValue = 7,
        ///     Precision = 8
        /// </summary>
        public int CustomPropertyOptionId;

        /// <summary>
        /// The value of the custom property option
        /// </summary>
        public string Value;
    }

    /// <summary>
    /// Represents a single source code revision association with a SpiraTeam build
    /// </summary>
    public class RemoteBuildSourceCode
    {
        /// <summary>
        /// The id of the build this revision is associated with
        /// </summary>
        public int BuildId;

        /// <summary>
        /// The key that uniquely identifies the revision
        /// </summary>
        public string RevisionKey;

        /// <summary>
        /// The date/time that the revision was associated with the build
        /// </summary>
        /// <remarks>
        /// Pass null to use the current date/time on the server
        /// </remarks>
        public Nullable<DateTime> CreationDate;
    }

    /// <summary>
    /// Represents one test configuration entry
    /// </summary>
    public class RemoteTestConfigurationEntry
    {
        /// <summary>
        /// The id of the entry
        /// </summary>
        public int TestConfigurationEntryId;

        /// <summary>
        /// The list of test case parameter name/values provided by the entry
        /// </summary>
        public List<RemoteTestConfigurationParameterValue> ParameterValues;
    }

    /// <summary>
    /// Represents a test set parameter (used when you have a test set pass parameters values to all of the containes test cases)
    /// </summary>
    public class RemoteTestConfigurationParameterValue
    {
        /// <summary>
        /// The id of the test case parameter
        /// </summary>
        public int TestCaseParameterId;

        /// <summary>
        /// The name of the test case parameter
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the parameter to be passed from the test configuratiin entry to the test cases
        /// </summary>
        public string Value;
    }

    /// <summary>
    /// Represents a single role's permission (e.g. create + requirement)
    /// </summary>
    public class RemoteRolePermission
    {
        /// <summary>
        /// The artifact type
        /// </summary>
        public int ArtifactTypeId;

        /// <summary>
        /// The permission id:
        /// None = -1,
        /// ProjectAdmin = -2,
        /// SystemAdmin = -3,
        /// ProjectGroupAdmin = -4,
        /// Create = 1,
        /// Modify = 2,
        /// Delete = 3,
        /// View = 4,
        /// LimitedModify = 5
        /// </summary>
        public int PermissionId;

        /// <summary>
        /// The role
        /// </summary>
        public int ProjectRoleId;
    }

    /// <summary>
    /// This object represents a single step in a test run in the system
    /// </summary>
    public class RemoteTestRunStep
    {
        /// <summary>
        /// The id of the test run step
        /// </summary>
        public Nullable<int> TestRunStepId;

        /// <summary>
        /// The is of the parent test run
        /// </summary>
        public int TestRunId;

        /// <summary>
        /// The id of the test step the test run step is based on
        /// </summary>
        public Nullable<int> TestStepId;

        /// <summary>
        /// The id of the test case that the test run step is based on
        /// </summary>
        /// <remarks>
        /// May be different from the TestRun.TestCaseId in the case of Linked Test Cases
        /// </remarks>
        public Nullable<int> TestCaseId;

        /// <summary>
        /// The id of the execution status of the test run step result
        /// </summary>
        /// <remarks>
        /// Failed = 1;
        /// Passed = 2;
        /// NotRun = 3;
        /// NotApplicable = 4;
        /// Blocked = 5;
        /// Caution = 6;
        /// </remarks>
        public int ExecutionStatusId;

        /// <summary>
        /// The positional order of the test run step in the test run
        /// </summary>
        public int Position;

        /// <summary>
        /// The description of what the tester should do when executing the step
        /// </summary>
        public string Description;

        /// <summary>
        /// The expected result that should oocur when the tester executes the step
        /// </summary>
        public string ExpectedResult;

        /// <summary>
        /// The sample data that should be used by the tester
        /// </summary>
        public string SampleData;

        /// <summary>
        /// The actual result that occurs when the tester executes the step
        /// </summary>
        public string ActualResult;

        /// <summary>
        /// The actual duration of the test run step
        /// </summary>
        public int? ActualDuration;

        /// <summary>
        /// The start date/time of the test run step
        /// </summary>
        public DateTime? StartDate;

        /// <summary>
        /// The end date/time of the test run step
        /// </summary>
        public DateTime? EndDate;
    }

    /// <summary>
    /// Represents a single custom property instance associated with an artifact
    /// </summary>
    public class RemoteArtifactCustomProperty
    {
        /// <summary>
        /// The number of the custom property field
        /// </summary>
        public int PropertyNumber;

        /// <summary>
        /// The value of a string custom property
        /// </summary>
        public string StringValue;

        /// <summary>
        /// The value of an integer custom property
        /// </summary>
        public int? IntegerValue;

        /// <summary>
        /// The value of a boolean custom property
        /// </summary>
        public bool? BooleanValue;

        /// <summary>
        /// The value of a date-time custom property
        /// </summary>
        public DateTime? DateTimeValue;

        /// <summary>
        /// The value of a decimal custom property
        /// </summary>
        public Decimal? DecimalValue;

        /// <summary>
        /// The value of a multi-list custom property
        /// </summary>
        public List<int> IntegerListValue;

        /// <summary>
        /// The associated custom property definition (read-only)
        /// </summary>
        public RemoteCustomProperty Definition;
    }

    /// <summary>
    /// Represents a single column in the data table
    /// </summary>
    public class RemoteTableColumn
    {
        /// <summary>
        /// The system name of the column
        /// </summary>
        public string Name;

        /// <summary>
        /// The caption of the column (optional)
        /// </summary>
        public string Caption;

        /// <summary>
        /// The position of the column (starting at 1)
        /// </summary>
        public int Position;

        /// <summary>
        /// The type of data this column holds (short name)
        /// </summary>
        public string Type;

        /// <summary>
        /// The type of data this column holds (long name)
        /// </summary>
        public string TypeNameSpace;
    }

    /// <summary>
    /// Represents a single row in the data table
    /// </summary>
    public class RemoteTableRow
    {
        /// <summary>
        /// The number of the row in the table
        /// </summary>
        public int RowNumber;

        /// <summary>
        /// The list of items in the row
        /// </summary>
        public List<RemoteTableRowItem> Items;
    }

    /// <summary>
    /// Represents a single item in a row in the table. The intersection of a specific row and column
    /// </summary>
    public class RemoteTableRowItem
    {
        /// <summary>
        /// The system name of the column
        /// </summary>
        public string Name;

        /// <summary>
        /// Link to the column it references
        /// </summary>
        public RemoteTableColumn Column;

        /// <summary>
        /// The value of the item (dynamically typed)
        /// </summary>
        public object Value;
    }

    #endregion
}
