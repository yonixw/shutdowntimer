using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimerShutdownTimer.Properties;

namespace TimerShutdownTimer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool disableClose = true;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (disableClose)
                e.Cancel = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            disableClose = false;
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
        }


        

        public int CurrentLeftSec = 60 * 60;
        public string random_code = randomCode();
        private void Form1_Load(object sender, EventArgs e)
        {
            ProcessTerminationProtection.ProcessProtect.ProtectCurrentProcess();
            btnClose.Enabled = Settings.Default.CancelEnabled;
            CurrentLeftSec = Settings.Default.TimerTimeMIN * 60;
            tbTime.Maximum = CurrentLeftSec;
            tbTime.Value = tbTime.Maximum;

            btnAddTime.Text += Settings.Default.AddOneTimeMIN;
            txtCode.Text =  string.Join(" - ", random_code.ToCharArray());

            tmrSub.Interval = 1 * 1000;
            tmrSub.Enabled = true;
        }

        private void OnBoom()
        {
            if(!Settings.Default.DryRun)
            {
                var psi = new ProcessStartInfo("shutdown", "/s /f /t 0");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi);
            }
            else
            {
                MessageBox.Show("Dryrun Boom!");
            }
        }

        bool isTimerEnabled = true;
        private void tmrSub_Tick(object sender, EventArgs e)
        {
            if (isTimerEnabled)
            {
                CurrentLeftSec = Math.Max(0, CurrentLeftSec - (tmrSub.Interval / 1000));
                if (CurrentLeftSec % 20 == 0 )
                {
                    // Recalculate code
                    random_code = randomCode();
                    txtCode.Text = string.Join(" - ", random_code.ToCharArray());
                }
                if (CurrentLeftSec <= 2)
                {
                    isTimerEnabled = false;
                    tbTime.Enabled = false;
                    OnBoom();
                }
                else
                {
                    tbTime.Value = CurrentLeftSec;
                    lblTimeLeft.Text = TimeSpan.FromSeconds(CurrentLeftSec).ToString();
                }
            }
        }

        public static string ComputeSHA256Hash(string text)
        {
            using (var sha256 = new SHA256Managed())
            {
                return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "");
            }
        }

        DateTime lastTry = new DateTime();
        bool isPasswordOK(string realpass)
        {
            DateTime now = DateTime.Now;
            if ((now - lastTry).TotalSeconds >= 3)
            {
                lastTry = new DateTime();
                if (realpass.Equals(txtPassword.Text))
                {
                    return true;
                }
                else if (ComputeSHA256Hash(random_code + realpass).ToLower().Equals(txtPassword.Text))
                {
                    return true;
                }
            }
            return false;
        }


        bool added1Time = false;
        private void btnAddTime_Click(object sender, EventArgs e)
        {
            if (!added1Time)
            {
                if (isPasswordOK(Settings.Default.AddOnPassword))
                {
                    tbTime.Maximum += Settings.Default.AddOneTimeMIN * 60;
                    CurrentLeftSec += Settings.Default.AddOneTimeMIN * 60;
                    added1Time = true;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (isPasswordOK(Settings.Default.CancelPassword))
            {
                tmrSub.Enabled = false;
                btnCancel.Enabled = false;
                btnClose.Enabled = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            OnBoom();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }


        const string code_source = "ABCDEFGHJKLMNPQRSTVWXYZ23456789";
        static string randomCode(int length = 6)
        {
            Random random = new Random();
            return new string (Enumerable.Repeat(code_source, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(random_code);
        }
    }
}
