using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inflectra.SpiraTest.Business
{
	public class TestCaseApproverDataObject
	{
		public int ProjectId { get; set; }

		public int WorkflowTransitionId { get; set; }

		public int? OrderId { get; set; }

		public int UserId { get; set; }

		public string UserName { get; set; }

		public string FullName { get; set; }

		public string Department { get; set; }

		public int ProjectSignatureId { get; set; }


	}
}
