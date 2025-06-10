using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Validation.Reports.Windward.Core.Model
{
	[Table("TST_Template")]
	[DataContract]
	public class Template
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key]
		[DataMember]
		public int TemplateId { get; set; }
		[DataMember]
		public String TemplateName { get; set; }
		[DataMember]
		public bool IsCustom { get; set; }
		[DataMember]
		public string TemplateLocation { get; set; }
		[DataMember]
		public string PodLocation { get; set; }
		[DataMember]
		public bool Active { get; set; }
		[DataMember]
		public DateTime CreationDate { get; set; }
		[DataMember]
		public string ReportCategory { get; set; }
		[DataMember]
		public int CategoryGroup { get; set; }
		[DataMember]
		public virtual ICollection<TemplateParameter> Parameters { get; set; }
		[DataMember]
		public virtual ICollection<TemplateOutputType> OutputTypes { get; set; }
		[DataMember]
		public virtual ICollection<TemplateDataSource> DataSources { get; set; }
	}
}
