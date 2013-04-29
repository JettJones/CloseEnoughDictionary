using System;
using System.Collections.Generic;
using System.Windows.Forms;

using CloseEnoughDictionary.Util;

namespace CloseEnoughDictionary.UI
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(DebugForm_FormClosing);

            Constants.DebugAction = this.SafeAppend;
        }

        private void SafeAppend(string debug)
        {
            this.textBox1.InvokeMaybe((c) =>
                ((TextBox)c).AppendText(debug + Environment.NewLine));
        }

        internal void PrintStrings(List<string> strings)
        {
            SafeAppend(String.Join(Environment.NewLine, strings));
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            this.textBox1.InvokeMaybe((c) =>
                ((TextBox)c).Clear());
        }

        private void buttonStats_Click(object sender, EventArgs e)
        {
            this.PrintStrings(DebugCounts.GetStatistics());
        }

        private void buttonReset_Click_1(object sender, EventArgs e)
        {
            DebugCounts.Reset();
        }
    }
}