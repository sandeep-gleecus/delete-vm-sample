using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Inflectra.SpiraTest.Installer.UI
{
	public class OpenFileDialog : FileDialog
	{
		public bool? ShowDialog(Window owner)
		{
			NativeMethods.OpenFileName ofn = ToOfn(owner);
			if (NativeMethods.GetOpenFileName(ofn))
			{
				FromOfn(ofn);
				return true;
			}
			else
			{
				FreeOfn(ofn);
				return false;
			}
		}
	}
}
