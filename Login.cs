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
    public partial class Login : Form
    {
        public string ReturnVal { get; set; }
        public Login()
        {
            InitializeComponent();
            foreach (string f in System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"*.csv"))
            {
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(f, "keylog_.*_(.*).csv");
                if (match.Success)
                {
                    textBox1.Text = match.Groups[1].Value;
                    break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                this.ReturnVal = textBox1.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
