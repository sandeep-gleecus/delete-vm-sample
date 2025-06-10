using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.Common
{
	public class RecentProjectDetails
	{
		public int ProjectId { get; set; }
		public string ProjectName { get; set; }
		public string ProjectGroup { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime LastActivityDate { get; set; }
	}
}
