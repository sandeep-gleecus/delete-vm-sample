using System;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

namespace Inflectra.SpiraTest.Common
{
	/// <summary>
	/// This contains the information regarding the LDAP configuration
	/// and also methods for accessing a remote LDAP server
	/// </summary>
	public class LdapClient
	{
		protected string ldapHost;
		protected string ldapBaseDn;
		protected string ldapBindDn;
		protected string ldapBindPassword;
		protected string ldapLogin;
		protected string ldapFirstName;
		protected string ldapLastName;
		protected string ldapMiddleInitial;
		protected string ldapEmailAddress;
        protected bool useSSL = false;

        protected const int LDAP_PAGE_SIZE = 500;

		private const string CLASS_NAME = "Inflectra.SpiraTest.Common.LdapClient::";
		
		/// <summary>
		/// Constructor Method
		/// </summary>
		public LdapClient()
		{
		}

		#region Properties

		/// <summary>
		/// The host name and port of the LDAP Server (e.g. myserver, myserver:389)
		/// </summary>
		public string LdapHost
		{
			get
			{
				return this.ldapHost;
			}
			set
			{
				this.ldapHost = value;
			}
		}

        /// <summary>
        /// Should we use SSL or not
        /// </summary>
        public bool UseSSL
        {
            get
            {
                return this.useSSL;
            }
            set
            {
                this.useSSL = value;
            }
        }

		/// <summary>
		/// The base Distinguished Name of the base node in the directory
		/// </summary>
		public string LdapBaseDn
		{
			get
			{
				return this.ldapBaseDn;
			}
			set
			{
				this.ldapBaseDn = value;
			}
		}

		/// <summary>
		/// The Distinguished Name of the user we're binding to the directory as
		/// </summary>
		public string LdapBindDn
		{
			get
			{
				return this.ldapBindDn;
			}
			set
			{
				this.ldapBindDn = value;
			}
		}

		/// <summary>
		/// The password of the user we're binding to the directory as
		/// </summary>
		public string LdapBindPassword
		{
			get
			{
				return this.ldapBindPassword;
			}
			set
			{
				this.ldapBindPassword = value;
			}
		}

		/// <summary>
		/// The user object's attribute that contains the login name
		/// </summary>
		public string LdapLogin
		{
			get
			{
				return this.ldapLogin;
			}
			set
			{
				this.ldapLogin = value;
			}
		}

		/// <summary>
		/// The user object's attribute that contains the first name
		/// </summary
		public string LdapFirstName
		{
			get
			{
				return this.ldapFirstName;
			}
			set
			{
				this.ldapFirstName = value;
			}
		}

		/// <summary>
		/// The user object's attribute that contains the last name
		/// </summary
		public string LdapLastName
		{
			get
			{
				return this.ldapLastName;
			}
			set
			{
				this.ldapLastName = value;
			}
		}

		/// <summary>
		/// The user object's attribute that contains the middle initial
		/// </summary
		public string LdapMiddleInitial
		{
			get
			{
				return this.ldapMiddleInitial;
			}
			set
			{
				this.ldapMiddleInitial = value;
			}
		}

		/// <summary>
		/// The user object's attribute that contains the email address
		/// </summary
		public string LdapEmailAddress
		{
			get
			{
				return this.ldapEmailAddress;
			}
			set
			{
				this.ldapEmailAddress = value;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Loads in the ldap configuration settings
		/// </summary>
		public void LoadSettings()
		{
            this.ldapHost = ConfigurationSettings.Default.Ldap_Host;
			this.ldapBaseDn = ConfigurationSettings.Default.Ldap_BaseDn;
			this.ldapBindDn = ConfigurationSettings.Default.Ldap_BindDn;
			try
			{
				this.ldapBindPassword = SecureSettings.Default.Ldap_BindPassword;
			}
			catch(Exception ex)
			{
				var msg = ex.Message;
			}
			this.ldapLogin = ConfigurationSettings.Default.Ldap_Login;
			this.ldapFirstName = ConfigurationSettings.Default.Ldap_FirstName;
			this.ldapLastName = ConfigurationSettings.Default.Ldap_LastName;
			this.ldapMiddleInitial = ConfigurationSettings.Default.Ldap_MiddleInitial;
            this.ldapEmailAddress = ConfigurationSettings.Default.Ldap_EmailAddress;
            this.useSSL = ConfigurationSettings.Default.Ldap_UseSSL;
        }

		/// <summary>
		/// Saves in the ldap configuration settings
		/// </summary>
		public void SaveSettings()
		{
            //Normal settings
			ConfigurationSettings.Default.Ldap_Host = this.ldapHost;
			ConfigurationSettings.Default.Ldap_BaseDn = this.ldapBaseDn;
			ConfigurationSettings.Default.Ldap_BindDn = this.ldapBindDn;
			ConfigurationSettings.Default.Ldap_Login = this.ldapLogin;
			ConfigurationSettings.Default.Ldap_FirstName = this.ldapFirstName;
			ConfigurationSettings.Default.Ldap_LastName = this.ldapLastName;
			ConfigurationSettings.Default.Ldap_MiddleInitial = this.ldapMiddleInitial;
            ConfigurationSettings.Default.Ldap_EmailAddress = this.ldapEmailAddress;
            ConfigurationSettings.Default.Ldap_UseSSL = this.useSSL;
            ConfigurationSettings.Default.Save();

            //Encrypted settings
            SecureSettings.Default.Ldap_BindPassword = this.ldapBindPassword;
            SecureSettings.Default.Save();
		}

		/// <summary>
		/// Authenticates a particular user against the LDAP server
		/// </summary>
		/// <param name="userDn">The distinguished name of the user being authenticated</param>
		/// <param name="password">The password for this user</param>
		/// <returns></returns>
		public bool Authenticate(string userDn, string password)
		{
			const string METHOD_NAME = "Authenticate";
			
			try
			{
				//If LDAP is not configured, return false
				if (!Global.Ldap_IsEnabledForAuthentication)
				{
					Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, String.Format(Resources.Main.LdapNotEnabledForAuthentication, userDn));
					return false;
				}

				//Access the specified node at the server using the test user/password
                LdapConnection ldapConnection = new LdapConnection(new LdapDirectoryIdentifier(this.ldapHost));
                ldapConnection.SessionOptions.SecureSocketLayer = this.useSSL;
                ldapConnection.SessionOptions.ProtocolVersion = 3;  //Some servers (e.g. OpenLDAP require this)
                if (this.useSSL)
                {
                    ldapConnection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback(ServerCallback);
                }
                ldapConnection.Credential = new System.Net.NetworkCredential(userDn, password);
                ldapConnection.AuthType = AuthType.Basic;

                //Now bind the connection
                bool matchFound = false; 
                using (ldapConnection)
                {
                    try
                    {
                        ldapConnection.Bind();
                    }
                    catch (DirectoryOperationException exception)
                    {
                        //Log the details of the message and rethrow
                        if (exception.Response == null)
                        {
                            if (exception.InnerException != null)
                            {
                                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to connect to LDAP Server: " + exception.InnerException.Message);
                            }
                            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to connect to LDAP Server: " + exception.Message + " (" + exception.StackTrace + ")");
                            throw;
                        }
                        else
                        {
                            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to connect to LDAP Server: " + exception.Message + " (" + exception.Response.ResultCode + " - " + exception.Response.ErrorMessage + ")");
                            throw;
                        }
                    }

				    //Now try and access the LDAP store by doing a wildcard search on any object
				    //As long as we can retrieve at least one object, then we have authenticated
                    SearchRequest searchRequest = new SearchRequest(
                        userDn,
                        "(objectclass=*)",
                        System.DirectoryServices.Protocols.SearchScope.Base,
                        new string[] { "distinguishedName" });
                    SearchResponse searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                    if (searchResponse != null && searchResponse.Entries.Count > 0)
                    {
                        matchFound = true;
                    }
                }

                return (matchFound);
			}
			catch (Exception exception)
			{
				//Indicate failure if we have an exception
                if (exception.InnerException != null)
                {
                    Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception.InnerException);
                }
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
				return false;
			}
		}

        /// <summary>
        /// Called to verify the certificate
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static bool ServerCallback(LdapConnection connection, X509Certificate certificate)
        {
            //We will trust all certificates - to keep things simple
            return true;
        }

		/// <summary>
		/// Retrieves a list of all the users in the LDAP server
		/// </summary>
        /// <param name="filters">The filters to apply when searching for users</param>
        /// <param name="sortExpression">The order we want the results sorted in</param>
		public LdapUserCollection GetUsers(Hashtable filters, string sortExpression)
		{
			const string METHOD_NAME = "GetUsers";

            try
            {
                //Access the specified node at the server using the test user/password
                LdapConnection ldapConnection = new LdapConnection(new LdapDirectoryIdentifier(this.ldapHost));
                ldapConnection.SessionOptions.SecureSocketLayer = this.useSSL;
                ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;    //Tell the system to not chase referrals, since that causes DC=X,DC=Y top-level base DNs to fail
                ldapConnection.SessionOptions.ProtocolVersion = 3;  //Needed to do paged queries
                if (this.useSSL)
                {
                    ldapConnection.SessionOptions.VerifyServerCertificate = new VerifyServerCertificateCallback(ServerCallback);
                }
                //See if we need to use anonymous access or not
                if (this.ldapBindDn == "" || this.ldapBindPassword == "")
                {
                    ldapConnection.AuthType = AuthType.Anonymous;
                }
                else
                {
                    ldapConnection.AuthType = AuthType.Basic;
                    ldapConnection.Credential = new System.Net.NetworkCredential(this.ldapBindDn, this.ldapBindPassword);
                }

                //Now bind the connection
                LdapUserCollection ldapUsers = new LdapUserCollection();
                using (ldapConnection)
                {
                    try
                    {
                        ldapConnection.Bind();
                    }
                    catch (DirectoryOperationException exception)
                    {
                        //Log the details of the message and rethrow
                        if (exception.Response == null)
                        {
                            if (exception.InnerException != null)
                            {
                                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to connect to LDAP Server: " + exception.InnerException.Message);
                            }
                            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to connect to LDAP Server: " + exception.Message + " (" + exception.StackTrace + ")");
                            throw;
                        }
                        else
                        {
                            Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, "Unable to connect to LDAP Server: " + exception.Message + " (" + exception.Response.ResultCode + " - " + exception.Response.ErrorMessage + ")");
                            throw;
                        }
                    }

                    //Create a new LDAP search object to query the LDAP store
                    SearchRequest searchRequest = new SearchRequest();
                    searchRequest.DistinguishedName = this.ldapBaseDn;
                    searchRequest.Scope = System.DirectoryServices.Protocols.SearchScope.Subtree;

                    //Specify the pagination information
                    PageResultRequestControl pageResultRequestControl = new PageResultRequestControl(LDAP_PAGE_SIZE);
                    pageResultRequestControl.IsCritical = false;
                    searchRequest.Controls.Add(pageResultRequestControl);

                    //Specify the properties that we want to return, handling optional fields correctly
                    StringCollection propertiesToLoad = new StringCollection();
                    searchRequest.Attributes.Add("cn");
                    searchRequest.Attributes.Add("distinguishedName");
                    searchRequest.Attributes.Add(this.ldapLogin);
                    if (this.ldapFirstName != "")
                    {
                        searchRequest.Attributes.Add(this.ldapFirstName);
                    }
                    if (this.ldapMiddleInitial != "")
                    {
                        searchRequest.Attributes.Add(this.ldapMiddleInitial);
                    }
                    if (this.ldapLastName != "")
                    {
                        searchRequest.Attributes.Add(this.ldapLastName);
                    }
                    if (this.ldapEmailAddress != "")
                    {
                        searchRequest.Attributes.Add(this.ldapEmailAddress);
                    }

                    //We only want entries that have a login set
                    searchRequest.Filter = "(" + this.ldapLogin + "=*)";

                    //Add the filters if we have any
                    if (filters != null && filters.Count > 0)
                    {
                        foreach (DictionaryEntry dictionaryEntry in filters)
                        {
                            string filterName = (string)dictionaryEntry.Key;
                            string filterValue = (string)dictionaryEntry.Value;
                            
                            //Make sure we escape any of the reserved characters
                            filterValue = filterValue.Replace(@"\", @"\5c");
                            filterValue = filterValue.Replace("*", @"\2a");
                            filterValue = filterValue.Replace("(", @"\28");
                            filterValue = filterValue.Replace(")", @"\29");
                            filterValue = filterValue.Replace("/", @"\2f");

                            if (filterName == "CommonName")
                            {
                                searchRequest.Filter += "(cn=" + filterValue + "*)";
                            }
                            if (filterName == "FirstName" && this.ldapFirstName != "")
                            {
                                searchRequest.Filter += "(" + this.ldapFirstName + "=" + filterValue + "*)";
                            }
                            if (filterName == "MiddleInitial" && this.ldapMiddleInitial != "")
                            {
                                searchRequest.Filter += "(" + this.ldapMiddleInitial + "=" + filterValue + "*)";
                            }
                            if (filterName == "LastName" && this.ldapLastName != "")
                            {
                                searchRequest.Filter += "(" + this.ldapLastName + "=" + filterValue + "*)";
                            }
                            if (filterName == "Login")
                            {
                                searchRequest.Filter += "(" + this.ldapLogin + "=" + filterValue + "*)";
                            }
                            if (filterName == "EmailAddress" && this.ldapEmailAddress != "")
                            {
                                searchRequest.Filter += "(" + this.ldapEmailAddress + "=" + filterValue + "*)";
                            }
                        }
                        searchRequest.Filter = "(&" + searchRequest.Filter + ")";    //Tells it to AND the other clauses
                    }
                    Logger.LogTraceEvent(CLASS_NAME + METHOD_NAME, "Using LDAP Search String: " + searchRequest.Filter);

                    //Iterate through, handling the paging correcting
                    while (true)
                    {
                        SearchResponse searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                        //find the returned page response control
                        foreach (DirectoryControl directoryControl in searchResponse.Controls)
                        {
                            if (directoryControl is PageResultResponseControl)
                            {
                                //update the cookie for next set
                                pageResultRequestControl.Cookie = ((PageResultResponseControl)directoryControl).Cookie;
                                break;
                            }
                        }


                        //We now need to put this data into a standardized collection of LDAP Users
                        foreach (SearchResultEntry searchResult in searchResponse.Entries)
                        {
                            string commonName = GetAttributeStringValue(searchResult.Attributes, "cn");
                            string distinguishedName = searchResult.DistinguishedName;
                            string login = GetAttributeStringValue(searchResult.Attributes, this.ldapLogin);
                            string firstName = "";
                            if (this.ldapFirstName != "")
                            {
                                firstName = GetAttributeStringValue(searchResult.Attributes, this.ldapFirstName);
                            }
                            string middleInitial = "";
                            if (this.ldapMiddleInitial != "")
                            {
                                middleInitial = GetAttributeStringValue(searchResult.Attributes, this.ldapMiddleInitial);
                            }
                            string lastName = "";
                            if (this.ldapLastName != "")
                            {
                                lastName = GetAttributeStringValue(searchResult.Attributes, this.ldapLastName);
                            }
                            string emailAddress = "";
                            if (this.ldapEmailAddress != "")
                            {
                                emailAddress = GetAttributeStringValue(searchResult.Attributes, this.ldapEmailAddress);
                            }

                            ldapUsers.Add(new LdapUser(commonName, distinguishedName, login, firstName, middleInitial, lastName, emailAddress));
                        }

                        //our exit condition is when our cookie is empty
                        if (pageResultRequestControl.Cookie.Length == 0)
                        {
                            break;
                        }
                    }
                }

                //Finally apply the sort expression
                if (!string.IsNullOrEmpty(sortExpression))
                {
                    string sortField = sortExpression.Substring(0, sortExpression.IndexOf(" "));
                    string sortDirectionString = sortExpression.Substring(sortExpression.IndexOf(" "), sortExpression.Length - sortExpression.IndexOf(" ")).Trim();
                    SortDirection sortDirection = (sortDirectionString == "ASC") ? SortDirection.Ascending : SortDirection.Descending;
                    ldapUsers.Sort(sortField, sortDirection);
                }
                return ldapUsers;
            }
            catch (DirectoryOperationException exception)
            {
                //Rethrow as a specific exception
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw new LdapClientUnableToAccessServerException("Unable to access LDAP server (" + exception.Message + ")");
            }
            catch (LdapException exception)
            {
                //Rethrow as a specific exception
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw new LdapClientUnableToAccessServerException("Unable to access LDAP server (" + exception.ErrorCode + ": " + exception.Message + ")");
            }
            catch (Exception exception)
            {
                //Rethrow as a specific exception
                Logger.LogErrorEvent(CLASS_NAME + METHOD_NAME, exception);
                throw new LdapClientUnableToAccessServerException("Unable to access LDAP server (" + exception.Message + ")");
            }
		}

        // helper method to convert a byte array to a hex string.
        // used by one of the AttributeSearch methods.
        protected static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {
                '0', '1', '2', '3', '4', '5', '6', '7',
                '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }


        /// <summary>
        /// Extracts the specified attribute from the directory object
        /// </summary>
        /// <param name="directoryAttributes">The collection of attributes</param>
        /// <param name="attributeName">The name of the attribute to retrieve a value for</param>
        /// <returns>The value of the attribute or an empty string if no value set</returns>
        protected string GetAttributeStringValue(SearchResultAttributeCollection directoryAttributes, string attributeName)
        {
            if (directoryAttributes.Contains(attributeName))
            {
                DirectoryAttribute directoryAttribute = directoryAttributes[attributeName];

                //Make sure we have a value
                if (directoryAttribute.Count > 0)
                {
                    //Sometimes they are returned as strings, other times as byte arrays
                    string attributeValue;
                    if (directoryAttribute[0] is string)
                    {
                        attributeValue = (string)directoryAttribute[0];
                    }
                    else if (directoryAttribute[0] is byte[])
                    {
                        byte[] attributeBytes = (byte[])directoryAttribute[0];
                        attributeValue = ToHexString(attributeBytes);
                    }
                    else
                    {
                        //Just return a null string
                        attributeValue = "";
                    }

                    return attributeValue;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

		#endregion
	}

	/// <summary>
	/// Contains a collection of LDAP users
	/// </summary>
	public class LdapUserCollection : CollectionBase
	{
		/// <summary>
		/// Class constructor
		/// </summary>
		public LdapUserCollection()
		{
		}

		/// <summary>
		/// Indexer property for the collection that returns and sets an item
		/// </summary>
		public LdapUser this[int index]
		{
			get
			{
				return (LdapUser) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Adds a new ldap user to the collection
		/// </summary>
		/// <param name="item">The ldap user item to add</param>
		public void Add(LdapUser item) 
		{
			this.List.Add(item);
		}

		/// <summary>
		/// Inserts a ldap user at a specific point in the collection
		/// </summary>
		/// <param name="index">The index of the location to insert at</param>
		/// <param name="item">The ldap user to insert</param>
		public void Insert(int index, LdapUser item) 
		{
			this.List.Insert(index, item);
		}
		
		/// <summary>
		/// Removes a ldap user from the collection
		/// </summary>
		/// <param name="item">The ldap user to remove</param>
		public void Remove(LdapUser item) 
		{
			List.Remove(item);
		}

		/// <summary>
		/// Determines if a specific ldap user is already in the collection
		/// </summary>
		/// <param name="item">The ldap user to search for</param>
		/// <returns>True if the user is already in the collection</returns>
		public bool Contains(LdapUser item) 
		{
			return this.List.Contains(item);
		}

		/// <summary>
		/// Collection IndexOf method 
		/// </summary>
		/// <param name="item">The ldap user to get the index of</param>
		/// <returns>The index that the user resides at in the collection</returns>
		public int IndexOf(LdapUser item) 
		{ 
			return List.IndexOf(item); 
		} 

		/// <summary>
		/// Copues an array of ldap users into the collection at the specified location
		/// </summary>
		/// <param name="array">The array of ldap users to copy</param>
		/// <param name="index">The location in the collection to copy them to</param>
		public void CopyTo(LdapUser[] array, int index) 
		{ 
			List.CopyTo(array, index); 
		}

        /// <summary>
        /// Sorts the collection by the appropriate field
        /// </summary>
        /// <param name="fieldName">The field to sort by</param>
        public void Sort(string fieldName, SortDirection sortDirection)
        {
            IComparer ldapUserSorter = new LdapUserSorter(fieldName, sortDirection);
            InnerList.Sort(ldapUserSorter);
        }

        /// <summary>
        /// Class that implements sorting for this collection
        /// </summary>
        private class LdapUserSorter : IComparer
        {
            private string fieldName;
            private SortDirection sortDirection;

            /// <summary>
            /// Constructor method
            /// </summary>
            /// <param name="fieldName">The field to sort by</param>
            /// <param name="sortDirection">The direction to sort</param>
            public LdapUserSorter(string fieldName, SortDirection sortDirection)
            {
                this.fieldName = fieldName;
                this.sortDirection = sortDirection;
            }

            public int Compare(Object x, Object y)
            {
                LdapUser ldapUser1 = (LdapUser)x;
                LdapUser ldapUser2 = (LdapUser)y;

                //Compare the appropriate fields
                IComparable ic1;
                IComparable ic2;
                if (this.fieldName == "Login")
                {
                    ic1 = (IComparable)ldapUser1.Login;
                    ic2 = (IComparable)ldapUser2.Login;
                }
                else if (this.fieldName == "FirstName")
                {
                    ic1 = (IComparable)ldapUser1.FirstName;
                    ic2 = (IComparable)ldapUser2.FirstName;
                }
                else if (this.fieldName == "MiddleInitial")
                {
                    ic1 = (IComparable)ldapUser1.MiddleInitial;
                    ic2 = (IComparable)ldapUser2.MiddleInitial;
                }
                else if (this.fieldName == "LastName")
                {
                    ic1 = (IComparable)ldapUser1.LastName;
                    ic2 = (IComparable)ldapUser2.LastName;
                }
                else if (this.fieldName == "CommonName")
                {
                    ic1 = (IComparable)ldapUser1.CommonName;
                    ic2 = (IComparable)ldapUser2.CommonName;
                }
                else if (this.fieldName == "EmailAddress")
                {
                    ic1 = (IComparable)ldapUser1.EmailAddress;
                    ic2 = (IComparable)ldapUser2.EmailAddress;
                }
                else if (this.fieldName == "DistinguishedName")
                {
                    ic1 = (IComparable)ldapUser1.DistinguishedName;
                    ic2 = (IComparable)ldapUser2.DistinguishedName;
                }
                else
                {
                    ic1 = (IComparable)ldapUser1.CommonName;
                    ic2 = (IComparable)ldapUser2.CommonName;
                }

                //Apply the CompareTo function depending on the direction of sorting
                if (sortDirection == SortDirection.Ascending)
                {
                    return ic1.CompareTo(ic2);
                }
                else
                {
                    return ic2.CompareTo(ic1);
                }
            }
        }
	}

	/// <summary>
	/// Contains a single LDAP user record
	/// </summary>
	public class LdapUser
	{
		protected string login;
		protected string firstName;
		protected string middleInitial;
		protected string lastName;
		protected string commonName;
		protected string distinguishedName;
		protected string emailAddress;

		/// <summary>
		/// Constructor for class
		/// </summary>
		/// <param name="commonName">The user's common name</param>
		/// <param name="distinguishedName">The user's distinguished name</param>
		/// <param name="login">The user's login</param>
		/// <param name="firstName">The user's first name</param>
		/// <param name="middleInitial">The user's middle initial</param>
		/// <param name="emailAddress">The user's email address</param>
		/// <param name="lastName">The user's last name</param>
		public LdapUser (string commonName, string distinguishedName, string login, string firstName, string middleInitial, string lastName, string emailAddress)
		{
			this.commonName = commonName;
			this.distinguishedName = distinguishedName;
			this.login = login;
			this.firstName = firstName;
			this.middleInitial = middleInitial;
			this.lastName = lastName;
			this.emailAddress = emailAddress;
		}

		public string EmailAddress
		{
			get
			{
				return this.emailAddress;
			}
		}

		public string CommonName
		{
			get
			{
				return this.commonName;
			}
		}
		
		public string DistinguishedName
		{
			get
			{
				return this.distinguishedName;
			}
		}

		public string Login
		{
			get
			{
				return this.login;
			}
		}

		public string FirstName
		{
			get
			{
				return this.firstName;
			}
		}

		public string MiddleInitial
		{
			get
			{
				return this.middleInitial;
			}
		}

		public string LastName
		{
			get
			{
				return this.lastName;
			}
		}
	}

	/// <summary>
	/// This exception is thrown when the client cannot access the LDAP store for some reason
	/// </summary>
	public class LdapClientUnableToAccessServerException: ApplicationException
	{
		public LdapClientUnableToAccessServerException()
		{
		}
		public LdapClientUnableToAccessServerException(string message)
			: base(message)
		{
		}
		public LdapClientUnableToAccessServerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
