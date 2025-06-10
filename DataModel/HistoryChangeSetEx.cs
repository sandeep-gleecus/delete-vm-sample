using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Inflectra.SpiraTest.DataModel
{
    /// <summary>
    /// Extensions on the history changeset object
    /// </summary>
    public partial class HistoryChangeSet : Entity
    {
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
                    return (int)HistoryChangeSetView.SignedStatusEnum.NotSigned;
                }

                //See if the hash matches
                string dataToTest = UserId + ":" + ArtifactTypeId + ":" + ArtifactId + ":" + ChangeDate.ToString(DatabaseExtensions.FORMAT_DATE_TIME_SECONDS_INVARIANT, System.Globalization.CultureInfo.InvariantCulture);
                string hash = HistoryChangeSetView.GetHashSha256(dataToTest);
                if (hash == SignatureHash)
                {
                    return (int)HistoryChangeSetView.SignedStatusEnum.Valid;
                }
                else
                {
                    return (int)HistoryChangeSetView.SignedStatusEnum.Invalid;
                }
            }
        }
    }
}
