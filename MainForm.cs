using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdenLoginManager
{
    public partial class MainForm : Form
    {
        public EdenClient ec;

        public MainForm()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo("https://github.com/bkesecker/xiloader");
            Process.Start(sInfo);
        }

        private void LaunchLoader()
        {
            string[] args = Environment.GetCommandLineArgs();
            ec = new EdenClient(args);
            ec.config.port = 54231; // set default port
            if (ec.config.server.Length > 0)
            {
                if (ec.Initialize(ec.config.server, ec.config.port))
                {
                    ec.LoadEdenLibrary();
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("Unable to connect to Eden login server. Possibly due to server being down or incorrectly configured.", "Error");
                }
            }
            else
            {                
                MessageBox.Show("Server parameter not specified properly.", "Error");
                Application.Exit();
            }
        }

        private void ProceedButton_Click(object sender, EventArgs e)
        {
            string agreeFile = Utilities.GetEdenAppDataPath() + "\\agreed.txt";
            if (AgreementCB.Checked)
            {
                Hide();
                if (agreeBox.Text == "agree")
                {
                    File.Create(Utilities.GetEdenAppDataPath() + "\\agreed.txt").Dispose();
                }
                LaunchLoader();
            }
            else
            {
                MessageBox.Show("If you would like to proceed, you must agree to downloading and loading the required XILoader plugin.","Info.", MessageBoxButtons.OK);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            string agreeFile = Utilities.GetEdenAppDataPath() + "\\agreed.txt";
            if (File.Exists(agreeFile))
            {
                Hide();
                LaunchLoader();
            }
        }
    }
}
