using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    public partial class User : Artifact
    {
        public const string ARTIFACT_PREFIX = "US";

        public const int UserInternal = 0; //Always has no navigation data, used for viewing all expanded
        public const int UserSystemAdministrator = 1; // The build in system administrator

        /// <summary>
        /// Returns the type of artifact
        /// </summary>
        public override ArtifactTypeEnum ArtifactType
        {
            get
            {
                return ArtifactTypeEnum.None;
            }
        }

        /// <summary>
        /// Returns the artifact prefix
        /// </summary>
        public override string ArtifactPrefix
        {
            get
            {
                return ARTIFACT_PREFIX;
            }
        }

        /// <summary>
        /// The session id of the user
        /// </summary>
        /// <remarks>This is used by Global.asax and is NOT persisted in the database</remarks>
        public string SessionId
        {
            get;
            set;
        }

        /// <summary>
        /// The plugin name connecting as this user
        /// </summary>
        /// <remarks>This is used by Global.asax and is NOT persisted in the database</remarks>
        public string PlugInName
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the user's full name (from their profile)
        /// </summary>
        public string FullName
        {
            get
            {
                if (Profile != null)
                {
                    return Profile.FullName;
                }
                else
                {
                    return null;
                }
            }
        }

		/// <summary>Translates any columns in the sort expression that need to be translated</summary>
		/// <param name="sortExpression">The input sort expression</param>
		/// <returns>The translated sort expression</returns>
		public static string TranslateSortExpression(string sortExpression)
		{
			string retString = sortExpression;

            if (sortExpression.Trim() == "FullName ASC")
            {
                retString = "Profile.FirstName ASC, Profile.LastName ASC";
            }
            else if (sortExpression.Trim() == "FullName DESC")
            {
                retString = "Profile.FirstName DESC, Profile.LastName DESC";
            }

			return retString;
		}

		/// <summary>The Artifact's token.</summary>
		public override string ArtifactToken
		{
			get
			{
				return this.ArtifactPrefix + ":" + this.UserId;
			}
		}

        /// <summary>The Artifact's ID</summary>
        public override int ArtifactId
        {
            get
            {
                return this.UserId;
            }
        }

		/// <summary>
		/// The MFA secret token (unencrypyted)
		/// </summary>
		public string MfaToken
		{
			get
			{
				if (String.IsNullOrEmpty(this.MfaTokenEncrypted))
				{
					return null;
				}
				else
				{
					//Decrypt the password
					EncryptionHelper encryptionHelper = new EncryptionHelper();
					return encryptionHelper.DecryptString(this.MfaTokenEncrypted);
				}
			}
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					this.MfaTokenEncrypted = null;
				}
				else
				{
					//Encrypt the password
					EncryptionHelper encryptionHelper = new EncryptionHelper();
					this.MfaTokenEncrypted = encryptionHelper.EncryptToString(value);
				}
			}
		}
	}
}
