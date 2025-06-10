using System;

namespace Inflectra.SpiraTest.Installer.HelperClasses.CommandLine
{
	/// <summary>
	/// Applied to a static property that yields a sequence of <see cref="CommandLine.Text.Example"/>,
	/// provides data to render usage section of help screen.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UsageAttribute : Attribute
	{
		/// <summary>
		/// Application name, script or any means that starts current program.
		/// </summary>
		public string ApplicationAlias { get; set; }
	}
}
