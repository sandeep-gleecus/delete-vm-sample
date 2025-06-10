using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Inflectra.InternalTools.DatabasePatcher;

namespace Inflectra.InternalTools.DatabasePatcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <remarks>
        /// If we're passed the /silent switch through the command-line we apply the patch without any popup messages
        /// </remarks>
        [STAThread]
        static void Main(string[] args)
        {
			Console.Write("Apply Patch");
            //See if we have the command-line switch provided
            if (args.Length > 0 && args[0].ToLowerInvariant() == "/silent")
            {
				Console.Write("Apply Patch1");
				string result = Patcher.ApplyPatch();
                if (String.IsNullOrEmpty(result))
                {
                    Console.WriteLine("Successfully patched database.");
                }
                else
                {
                    Console.WriteLine(result);
                }
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
