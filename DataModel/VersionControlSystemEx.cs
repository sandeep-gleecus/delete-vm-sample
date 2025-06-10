using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.DataModel
{
	/// <summary>
	/// The extensions to the VersionControlProject entity
	/// </summary>
	/// <remarks>
	/// This makes the table auto-truncate for safety
	/// </remarks>
	public partial class VersionControlSystem : Entity
	{
		/// <summary>
		/// Provides access to the password, handling both encrypted and clear formats seamlessly
		/// </summary>
		public string Password
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
