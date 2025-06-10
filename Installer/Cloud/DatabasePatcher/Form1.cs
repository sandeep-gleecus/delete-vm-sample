using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.EntityClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Data.SqlClient;
using Inflectra.InternalTools.DatabasePatcher;

namespace Inflectra.InternalTools.DatabasePatcher
{
    /// <summary>
    /// Applies the database portion of the patch
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //Close the application
            this.Close();
        }

        /// <summary>
        /// Applies the patch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnApply_Click(object sender, EventArgs e)
        {
            string result = Patcher.ApplyPatch();
            if (String.IsNullOrEmpty(result))
            {
                //Success
                MessageBox.Show ("Successfully Applied Patch.", "Patching Database Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            else
            {
                MessageBox.Show(result, "Patching Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Display the version number
            this.lblVersion.Text = "v" + Properties.Settings.Default.Version;
        }
    }
}
