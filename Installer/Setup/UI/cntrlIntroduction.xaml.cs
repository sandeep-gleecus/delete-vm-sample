using Inflectra.SpiraTest.Installer.ControlForm;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for UserControl1.xaml</summary>
	public partial class cntrlIntroduction : UserControl, IProcedureComponent
	{
		public cntrlIntroduction()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members
		public string KeyText => "Welcome!";
		public bool IsLinkable => true;
		public string UniqueName => "cntrlIntroduction";

		public bool AllowBack => true;

		public bool AllowNext => true;
	
		/// <summary>The label displayed on the navigation pane.</summary>
		public Label DisplayLabel { get; set; }

		public bool IsAvailable { get { throw new System.NotImplementedException(); } }

		/// <summary>Can we proceed to the next step in the wizard</summary>
		bool IProcedureComponent.IsValid() => true;
		#endregion
	}
}
