using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Validation.Reports.Windward.Core.Model
{
	[Table("TST_TemplateParameter")]
	[DataContract]
	public class TemplateParameter
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember]
		public int ParameterId { get; set; }
		[DataMember]
		public int TemplateId { get; set; }
		[DataMember]
		public string ParameterLabel { get; set; }
		[DataMember]
		public string ParameterType { get; set; }
		[NotMapped]
		public string DefaultValue { set; get; }
	}
}
