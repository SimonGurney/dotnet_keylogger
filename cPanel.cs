using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dotnet_keylogger
{
    public partial class cPanel : Form
    {
        public control control;
        public cPanel(control control)
        {
            this.control = control;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.control.state == true) { this.control.state = false; button1.Text = "Resume"; } else { this.control.state = true; button1.Text = "Pause"; }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            label1.Text = "Hi, stop clicking random stuff";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (CloseCancel() == false)
            {
                e.Cancel = true;
            };
        }
        public static bool CloseCancel()
        {
            const string message = "Are you sure that you would like to quit your session?";
            const string caption = "Close me down";
            var result = MessageBox.Show(message, caption,
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                return true;
            else
                return false;
        }

    }
}
