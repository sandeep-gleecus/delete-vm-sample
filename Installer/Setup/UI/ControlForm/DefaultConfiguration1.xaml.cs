using System;
using System.Windows.Controls;

namespace Inflectra.SpiraTest.Installer.ControlForm
{
	/// <summary>
	/// Interaction logic for DefaultConfiguration1.xaml
	/// </summary>
	public partial class DefaultConfiguration1 : UserControl, IProcedureComponent
	{
		public DefaultConfiguration1()
		{
			InitializeComponent();
		}

		#region IProcedureComponent Members

		public string KeyText => "Configuration 1";

        public bool IsLinkable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

		public string UniqueName { get { throw new NotImplementedException(); } }

		public bool AllowBack { get { throw new NotImplementedException(); } }

		public bool AllowNext { get { throw new NotImplementedException(); } }

		public Label DisplayLabel { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

		public bool IsAvailable { get { throw new NotImplementedException(); } }

		public bool IsValid()
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
