using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Inflectra.SpiraTest.Installer.UI
{
	/// <summary>Interaction logic for ItemProgress.xaml</summary>
	public partial class ItemProgress : UserControl
	{
		private ProcessStatusEnum _CurProcess = ProcessStatusEnum.None;

		public ItemProgress()
		{
			InitializeComponent();
		}

		/// <summary>The status image of the item.</summary>
		public enum ProcessStatusEnum
		{
			/// <summary>None - No image, leaves it as a 'to do'.</summary>
			None = 0,
			/// <summary>Processing - gives the item a highlightd blue arrow.</summary>
			Processing = 1,
			/// <summary>Error - An error ocured during processing. Gives the item a red X.</summary>
			Error = 2,
			/// <summary>Success - Successful processing, move on to the next item.</summary>
			Success = 3,
			Failure = 4
		}

		/// <summary>Sets the action text.</summary>
		public string SetActionName
		{
			get
			{
				return txtName.Text;
			}
			set
			{
				txtName.Text = value;
			}
		}

		/// <summary>Sets the error message, if needed,</summary>
		public string SetErrorString
		{
			get
			{
				return txtError.Text;
			}
			set
			{
				txtError.Text = value;
				txtError.Visibility = ((string.IsNullOrEmpty(value)) ? Visibility.Collapsed : Visibility.Visible);
			}
		}

		/// <summary>Sets the satatus of the item.</summary>
		public ProcessStatusEnum SetActionStatus
		{
			get
			{
				return _CurProcess;
			}
			set
			{
				_CurProcess = value;
				imgItem.Visibility = ((value == ProcessStatusEnum.None) ? Visibility.Hidden : Visibility.Visible);
				string src = null;
				string lead = "/" + Assembly.GetEntryAssembly().GetName().Name + ";component";
				switch (value)
				{
					case ProcessStatusEnum.Error:
						src = lead + @"/Themes/Inflectra/Error.png";
						break;
					case ProcessStatusEnum.Processing:
						src = lead + @"/Themes/Inflectra/Processing.png";
						break;
					case ProcessStatusEnum.Success:
						src = lead + @"/Themes/Inflectra/Success.png";
						break;
				}
				if (src != null)
					imgItem.Source = new BitmapImage(new Uri(src, UriKind.Relative));
			}
		}
	}
}
