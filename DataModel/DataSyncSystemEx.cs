using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Adds extensions to the data sync system entity
    /// </summary>
    public partial class DataSyncSystem : Entity
    {
        #region Enumerations

        //Data Sync Statuses
        public enum DataSyncStatusEnum
        {
            Success = 1,
            Failure = 2,
            Warning = 3,
            NotRun = 4
        }

        #endregion

        /// <summary>
        /// Display the caption if available, otherwise the actual plugin DLL name
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (String.IsNullOrEmpty(this.Caption))
                {
                    return this.Name;
                }
                else
                {
                    return this.Caption;
                }
            }
        }

        /// <summary>
        /// The name of the status this system is in
        /// </summary>
        public string DataSyncStatusName
        {
            get
            {
                if (this.Status != null)
                {
                    return this.Status.Name;
                }
                return "";
            }
        }

        /// <summary>
        /// Provides access to the password, handling both encrypted and clear formats seamlessly
        /// </summary>
        public string ExternalPassword
        {
            get
            {
                //See if the stored password is encrypted and return directly from the DB or decrypt first depending
                if (this.IsEncrypted && this.EncryptedPassword != null)
                {
                    string clearPassword = new EncryptionHelper().DecryptString(this.EncryptedPassword);
                    return clearPassword;
                }
                else
                {
                    return this.EncryptedPassword;
                }
            }
            set
            {
                //We force the flag to be encrypted
                this.IsEncrypted = true;
                if (value == null)
                {
                    this.EncryptedPassword = null;
                }
                else
                {
                    this.EncryptedPassword = new EncryptionHelper().EncryptToString(value);
                }
            }
        }
    }
}
