using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

using CloseEnoughDictionary.Util;

namespace CloseEnoughDictionary.UI
{
    public partial class MainWindow : Form
    {
        //delegates for thread safe operation
        delegate void DisplayWordsCallback(List<string> allWords);
        delegate void AppendWordCallback(string word);

        CedController controller;
        public MainWindow(CedController control)
        {
            controller = control;

            //setup window components.
            InitializeComponent();
        }

        internal string StatusText
        {
            set
            {
                this.statusText.Text = value;
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            this.controller.AddText(this.TextInput.Text);
        }

        private void ButtonFind_Click(object sender, EventArgs e)
        {
            this.controller.FindText(this.TextInput.Text);
        }

        private void ButtonShow_Click(object sender, EventArgs e)
        {
            this.controller.ShowAll();
        }

        internal void DisplayWords(List<string> allWords)
        {
            //Make sure to call this from the initializing thread.
            this.TextOutput.InvokeMaybe( (tb) =>
            {
                if (allWords.Any())
                {
                    StringBuilder builder = new StringBuilder();
                    foreach (string str in allWords)
                    {
                        builder.Append(str);
                        builder.Append(Environment.NewLine);
                    }
                    this.TextOutput.Text = builder.ToString();
                }
                else
                {
                    var notFoundMessage = String.Join(Environment.NewLine,
                    "<No words found>",
                    "",
                    "Type a letter pattern -",
                    "Use . or ? for any character",
                    "   [abc] for a letter set",
                    "   *;pattern for an anagram",
                    "   %pattern for a circular match");

                    this.TextOutput.Text = notFoundMessage;
                }

                StatusText = "Displaying " + allWords.Count + " words";

            });
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            OpenFileDialog dialog = sender as OpenFileDialog;
            if (dialog == null)
            {
                StatusText = "Open file message recieved from unknown source.";
                return;
            }
            string path = dialog.FileName;

            this.controller.LoadDictionary(path);
        }

        private void TextInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.ButtonFind_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.controller.ResetFunctionCount();
        }

        private void displayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.controller.DebugShow();
        }

        private void debugEnabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Constants.DebugEnabled = !Constants.DebugEnabled;
            this.debugEnabledToolStripMenuItem.Checked = Constants.DebugEnabled;
        }
    }
}