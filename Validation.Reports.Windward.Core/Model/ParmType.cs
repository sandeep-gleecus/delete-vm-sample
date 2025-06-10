using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Validation.Reports.Windward.Core.Model
{
    public enum ParmType
    {
        Text, DropDown, Date
    }

    [DataContract]
    public class DropDownData
    {
        [Key]
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
    }
}
