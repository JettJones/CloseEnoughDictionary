using System;
using System.Collections.Generic;
using System.Text;

using CloseEnoughDictionary.Util;

namespace CloseEnoughDictionary.Data
{
    public class TrieBase
    {
        private List<INode> words;

        internal static void Merge(List<INode> list, INode INode)
        {
            INode parent = list.Find(INode.GetPredicate());

            if (parent == null)
            {
                list.Add(INode);
            }
            else
            {
                parent.Merge(INode);
            }
        }

        internal static void MergeChildren(INode parent, List<INode> list)
        {
            foreach (INode n in list)
            {
                parent.MergeChild(n);
            }
        }

        public TrieBase()
        {
            this.words = new List<INode>();
        }

        public void AddWord(string word)
        {
            //remove whitespace
            string processedWord = ProcessWord(word);

            INode INode = ConvertToLetterNode(processedWord);

            Merge(this.words, INode);
        }

        private INode ConvertToLetterNode(string processedWord)
        {
            INode INode = TerminatorINode.GetInstance();

            //read backwards down the word
            for (int i = processedWord.Length - 1; i >= 0; i--)
            {
                LetterNode tempINode = LetterNode.GetInstance(processedWord[i]);
                tempINode.ChildNodes.Add(INode);
                INode = tempINode;
            }
            return INode;
        }

        private string ProcessWord(string word)
        {
            word = word.ToLowerInvariant();
            word = word.Trim();
            word.Replace(" ", "");
            word.Replace("\t", "");

            return word;
        }
        internal void ShowWords(Action<string> display)
        {
            this.ShowWords(display, this.words);
        }

        internal void ShowWords(Action<string> display, List<INode> words)
        {
            StringBuilder word = new StringBuilder();
            foreach (INode n in words)
            {
                n.ShowWords(display, word);
            }
        }

        internal List<string> GetWords()
        {
            return this.GetWords(this.words);
        }

        internal List<string> GetWords(List<INode> words)
        {
            List<string> strings = new List<string>();
            foreach (INode n in words)
            {
                List<string> INodeStrings = n.GetWords();
                strings.AddRange(INodeStrings);
            }

            return strings;
        }

        internal List<INode> Match(IMatchSet matcher)
        {
            return matcher.Match(this.words);
        }
    }

    public interface INode
    {
        Predicate<INode> GetPredicate();
        bool ShallowMatch(INode n);
        void Merge(INode n);
        void MergeChild(INode n);

        List<INode> MatchDispatch(IMatchSet matcher);

        //debug method
        List<string> GetWords();
        void ShowWords(Action<string> display, StringBuilder word);
    }

    public class LetterNode : INode
    {
        private class CharMatch
        {
            private readonly char letter;
            public Predicate<INode> Predicate { get; private set; }

            public CharMatch(char letter)
            {
                this.letter = letter;
                this.Predicate = new Predicate<INode>(this.MatchChar);
            }

            public bool MatchChar(INode letter)
            {
                LetterNode l = letter as LetterNode;
                return l != null && l.Letter == this.letter;
            }
        }

        private static readonly Dictionary<char, CharMatch> predicates = new
            Dictionary<char, CharMatch>();

        internal List<INode> ChildNodes { get; private set; }
        internal char Letter { get; private set; }

        private LetterNode(char letter)
        {
            this.Letter = letter;
            ChildNodes = new List<INode>(1);
        }

        public static LetterNode GetInstance(char letter)
        {
            return new LetterNode(letter);
        }

        public Predicate<INode> GetPredicate()
        {
            CharMatch match = predicates.GetOrInit(Letter, () => new CharMatch(Letter));
            return match.Predicate;
        }

        #region INode Members

        public bool ShallowMatch(INode n)
        {
            LetterNode letter = n as LetterNode;
            return letter != null && letter.Letter == this.Letter;
        }

        public void Merge(INode n)
        {
            LetterNode letter = n as LetterNode;
            if (n == null)
            {
                throw new ArgumentException("input node must be a letterNode. Use ShallowMatch to enfore this.","n");
            }

            foreach (INode child in letter.ChildNodes)
            {
                TrieBase.Merge(this.ChildNodes, child);
            }
        }

        public void MergeChild(INode n)
        {
            TrieBase.Merge(this.ChildNodes, n);
        }

        public List<INode> MatchDispatch(IMatchSet matcher)
        {
            return matcher.MatchLetter(this);
        }

        public List<string> GetWords()
        {
            List<string> tempList = new List<string>();
            foreach (INode n in this.ChildNodes)
            {
                tempList.AddRange(n.GetWords());
            }
            List<string> finalList = new List<string>();
            foreach (string s in tempList)
            {
                finalList.Add(this.Letter + s);
            }
            return finalList;
        }

        public void ShowWords(Action<string> display, StringBuilder word)
        {
            word.Append(this.Letter);
            foreach (INode n in this.ChildNodes)
            {
                n.ShowWords(display, word);
            }
            word.Remove(word.Length - 1, 1);
        }

        #endregion
    }

    /// <summary>
    /// Marks the end of a word
    /// </summary>
    public class TerminatorINode : INode
    {
        private TerminatorINode()
        {
            this.Predicate = new Predicate<INode>(this.ShallowMatch);
        }
        private static TerminatorINode Only = new TerminatorINode();
        private Predicate<INode> Predicate;
        public static TerminatorINode GetInstance()
        {
            return Only;
        }

        #region INode Members
        public Predicate<INode> GetPredicate()
        {
            return Only.Predicate;
        }
        public bool ShallowMatch(INode n)
        {
            TerminatorINode term = n as TerminatorINode;
            return term != null;
        }

        public void Merge(INode n)
        {
            //two words were identical- no op.
        }

        public void MergeChild(INode n)
        {
            throw new Exception("Didn't expect to add a child to a terminal INode");
        }

        public List<INode> MatchDispatch(IMatchSet matcher)
        {
            return matcher.MatchTerminator(this);
        }

        public List<string> GetWords()
        {
            List<string> list = new List<string>();
            list.Add("");
            return list;
        }

        public void ShowWords(Action<string> display, StringBuilder word)
        {
            display(word.ToString());
        }
        #endregion
    }
}
