using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Inflectra.SpiraTest.DataModel
{
    public partial class HistoryChangeSetView : Entity
    {
        /// <summary>
        /// The different signed statuses
        /// </summary>
        public enum SignedStatusEnum
        {
            NotSigned = 1,
            Valid = 2,
            Invalid = 3
        }

        /// <summary>
        /// Returns an SHA256 hash
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetHashSha256(string text)
        {
            byte[] bytes = UTF8Encoding.Unicode.GetBytes(text);
            byte[] hash;
            using (SHA256 shaManaged = new SHA256Managed())
            {
                hash = shaManaged.ComputeHash(bytes);
            }
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        /// <summary>
        /// Gets the textual description of whether the change is signed
        /// </summary>
        public string SignedName
        {
            get
            {
                string signedName = null;
                switch ((SignedStatusEnum)Signed)
                {
                    case SignedStatusEnum.NotSigned:
                        break;

                    case SignedStatusEnum.Valid:
                        signedName = "signature_verified";
                        break;

                    case SignedStatusEnum.Invalid:
                        signedName = "signature_failed";
                        break;
                }
                return signedName;
            }
        }

		public string SignedValue
		{
			get
			{
				//See if we have a signature hash first
				if (String.IsNullOrEmpty(SignatureHash))
				{
					return SignedStatusEnum.NotSigned.ToString();
				}

				//See if the hash matches
				string dataToTest = UserId + ":" + ArtifactTypeId + ":" + ArtifactId + ":" + ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);
				string hash = GetHashSha256(dataToTest);
				if (hash == SignatureHash)
				{
					return SignedStatusEnum.Valid.ToString();
				}
				else
				{
					return SignedStatusEnum.Invalid.ToString();
				}
			}
		}

        /// <summary>
        /// Is the change a signed change
        /// </summary>
        public int Signed
        {
            get
            {
                //See if we have a signature hash first
                if (String.IsNullOrEmpty(SignatureHash))
                {
                    return (int)SignedStatusEnum.NotSigned;
                }

                //See if the hash matches
                string dataToTest = UserId + ":" + ArtifactTypeId + ":" + ArtifactId + ":" + ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);
                string hash = GetHashSha256(dataToTest);
                if (hash == SignatureHash)
                {
                    return (int)SignedStatusEnum.Valid;
                }
                else
                {
                    return (int)SignedStatusEnum.Invalid;
                }
            }
        }
    }
}
