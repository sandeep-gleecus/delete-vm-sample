using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Runtime.Serialization;

namespace Inflectra.SpiraTest.Web.Services.Ajax.DataObjects
{
    [DataContract(Namespace = "tst.dataObjects")]
    public class PlanningDataItem : DataItem
    {
        [DataMember(Name = "expanded", EmitDefaultValue = false)]
        public bool? Expanded = null;

		[DataMember(Name = "ownerIconInitials", EmitDefaultValue = false)]
		public string OwnerIconInitials = null;

		[DataMember(Name = "rank", EmitDefaultValue = false)]
        public int? Rank = null;

        [DataMember(Name = "childTasks", EmitDefaultValue = false)]
        public List<DataItem> ChildTasks = null;

        [DataMember(Name = "childTestCases", EmitDefaultValue = false)]
        public List<DataItem> ChildTestCases = null;
    }
}
