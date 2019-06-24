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
            this.Text = "Why stop, we are having such fun?";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
