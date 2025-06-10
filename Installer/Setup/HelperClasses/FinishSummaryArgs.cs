using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>Stores the installation options chosen by the user</summary>
	public class FinishSummaryArgs
	{
		public bool Success;
		public string ExtraMessage;
		public bool UserCancelled;
	}
}
