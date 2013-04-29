using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using CloseEnoughDictionary.Data;
using CloseEnoughDictionary.Match;
using CloseEnoughDictionary.UI;
using CloseEnoughDictionary.Util;

namespace CloseEnoughDictionary
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //must be called before initialization
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //Initialize components, give them references.
            CedController programController = new CedController();
            MainWindow window = new MainWindow(programController);
            programController.Window = window;

            Application.Run(window);
        }
    }

    /// <summary>
    /// Program flow, connects the UI to data layers
    /// </summary>
    public class CedController
    {
        private TrieBase myDictionary;
        private MainWindow myWindow;
        private DebugForm myDebug;
        internal MainWindow Window
        {
            set
            {
                this.myWindow = value;
            }
        }
        public CedController()
        {
            this.myDictionary = new TrieBase();
            this.myWindow = null;
            this.myDebug = new DebugForm();
            this.myDebug.Hide();
        }

        internal void AddText(string p)
        {
            DisplayStatus("Adding Text '{0}'", p);

            ThreadPool.QueueUserWorkItem(AddTextDelegate, p);
        }
        private void AddTextDelegate(object param)
        {
            myDictionary.AddWord((string)param);

            List<string> allWords = this.myDictionary.GetWords();
            this.myWindow.DisplayWords(allWords);
        }

        internal void FindText(string p)
        {
            DisplayStatus("Finding Text '{0}'", p);

            ThreadPool.QueueUserWorkItem(FindTextDelegate, p);
        }

        private void FindTextDelegate(object param)
        {
            MatchFactory factory = MatchFactory.GetInstance();
            IMatchSet matcher = factory.GetMatcher((string)param);

            List<INode> INodes = this.myDictionary.Match(matcher);

            DisplayStatus("Formatting...");
            var str = new List<string>();
            this.myDictionary.ShowWords(str.Add, INodes);
            this.myWindow.DisplayWords(str);
        }

        private void DisplayStatus(string p, params object[] args)
        {
            myWindow.StatusText = String.Format(p, args);
        }

        // debug method to display all loaded words
        internal void ShowAll()
        {
            var str = new List<string>();
            DisplayStatus("Formatting...");
            this.myDictionary.ShowWords(str.Add);
            this.myWindow.DisplayWords(str);
        }

        internal void LoadDictionary(string path)
        {
            myWindow.StatusText = "Opening " + path;

            ThreadPool.QueueUserWorkItem(LoadDictionaryDelegate, path);
        }

        private void LoadDictionaryDelegate(object p)
        {
            string path = (string)p;

            int count = 0;
            foreach (var word in FileLoader.LoadDictionary(path))
            {
                if (!String.IsNullOrWhiteSpace(word))
                {
                    count++;
                    myDictionary.AddWord(word);

                    if (count % 50 == 0)
                    {
                        count = 0;
                        myWindow.StatusText = String.Format("Loaded {0}", word);
                    }
                }
            }
        }

        internal void ResetFunctionCount()
        {
            DebugCounts.Reset();
        }

        internal void DebugShow()
        {
            this.myDebug.Show();
        }
    }
}