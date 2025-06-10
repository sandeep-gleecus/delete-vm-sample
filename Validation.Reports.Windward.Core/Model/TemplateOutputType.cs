using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Validation.Reports.Windward.Core.Model
{
	[Table("TST_TemplateOutputType")]
	[DataContract]
	public class TemplateOutputType
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember]
		public int OutputTypeId { get; set; }
		[DataMember]
		public int TemplateId { get; set; }
		[DataMember]
		public string TypeDescription { get; set; }
	}
}
