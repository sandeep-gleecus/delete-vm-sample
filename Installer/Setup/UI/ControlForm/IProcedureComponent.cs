using System.Windows.Controls;

namespace Inflectra.SpiraTest.Installer.ControlForm
{
	public interface IProcedureComponent
	{
		/// <summary>The text to display in the top of the wizard</summary>
		string KeyText
		{
			get;
		}

		/// <summary>
		/// Returned by the component if it's valid and can be moved to the next step in the wizard
		/// </summary>
		/// <returns>True if ready to move on</returns>
		bool IsValid();

		/// <summary>Whether the page can be jumped to directly, bypassing next/back buttons.</summary>
		bool IsLinkable
		{
			get;
		}

		/// <summary>A unique string identifying this panel from others, to allow direct jumping.</summary>
		string UniqueName
		{
			get;
		}

		/// <summary>Whether to allow the user to go 'back' a screen from this one.</summary>
		bool AllowBack { get; }

		/// <summary>Whether to allow the user to go to the 'next' screen from this one.</summary>
		bool AllowNext { get; }

		/// <summary>The label that is displayed in the navigation panel.</summary>
		Label DisplayLabel { get; set; }

		/// <summary>Returns whether the screen is available given the current settings.</summary>
		bool IsAvailable { get; }
	}
}
