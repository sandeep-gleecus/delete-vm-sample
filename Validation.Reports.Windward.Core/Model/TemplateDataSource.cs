using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Validation.Reports.Windward.Core.Model
{
	[Table("TST_TemplateDataSource")]
	[DataContract]
	public class TemplateDataSource
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember]
		public int TemplateDataSourceId { get; set; }
		[DataMember]
		public int TemplateId { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string ProviderClass { get; set; }
		[DataMember]
		public string ConnectionString { get; set; }
	}
}
