using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses
{
	/// <summary>The database is the wrong version.</summary>
	public class WrongDBVersionException : Exception
	{
		/// <summary>The current version of the Database.</summary>
		public int CurrentVer;

		/// <summary>The version that this installer upgrades the database to.</summary>
		public int MaximumVer;
	}

	/// <summary>Thrown when the database is lower than the required minimum needed version</summary>
	public class WrongDBVersionMinimumException : WrongDBVersionException
	{
		/// <summary>The required minimum version of the database.</summary>
		public int NeededVer;
	}

	/// <summary>Thrown when the database is equal to or higher than the current </summary>
	public class WrongDBVersionMaximumException : WrongDBVersionException
	{ }
}
