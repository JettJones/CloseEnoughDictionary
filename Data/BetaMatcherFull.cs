using System;
using System.Collections.Generic;
using System.Text;

namespace CloseEnoughDictionary.Data
{
    #region Searching patterns
    //match cases
    public interface MatchMaker
    {
        bool MatchTerminator();
        
        //match one character - true if match succeeds
        bool MatchLetter(LetterNode letter);

        // unmatch one character - true if it was previously matched
        bool UnmatchLetter(LetterNode letter);

        //match complete - true if next character can be used.
        bool IsMatchComplete();
        
        //is match usable - true if this match can be used again.
        bool IsMatchOpen();
    }

    public abstract class MatchBase : MatchMaker
    {
        private List<LetterNode> matches;

        protected MatchBase()
        {
            this.matches = new List<LetterNode>();
        }

        public bool MatchLetter(LetterNode letter)
        {
            if (MatchOneLetter(letter))
            {
                this.matches.Add(letter);
                return true;
            }
            return false;
        }

        public bool UnmatchLetter(LetterNode letter)
        {
            return this.matches.Remove(letter);
        }

        protected int GetMatchedLetterCount()
        {
            return this.matches.Count;
        }

        protected abstract bool MatchOneLetter(LetterNode letter);

        public abstract bool MatchTerminator();

        public abstract bool IsMatchComplete();

        public abstract bool IsMatchOpen();
    }
    // match *
    public class MatchAll : MatchBase
    {
        public MatchAll()
        {
        }

        public override bool MatchTerminator()
        {
            return false;
        }

        protected override bool MatchOneLetter(LetterNode letter)
        {
            return true;
        }
        public override bool IsMatchComplete()
        {
            return true;
        }
        public override bool IsMatchOpen()
        {
            return true;
        }
    }

    public abstract class MatchNLetters : MatchBase
    {
        private int expectedCount;
        public MatchNLetters(int expected)
        {
            this.expectedCount = expected;
        }

        public override bool MatchTerminator()
        {
            return false;
        }

        protected override bool MatchOneLetter(LetterNode letter)
        {
            //already seen letters.
            if (SeenAllLetters())
            {
                return false;
            }
            else
            {
                return MatchOneCharacter(letter.Letter);
            }
        }
        public override bool IsMatchComplete()
        {
            return this.SeenAllLetters();
        }
        public override bool IsMatchOpen()
        {
            return !this.SeenAllLetters();
        }
        private bool SeenAllLetters()
        {
            return base.GetMatchedLetterCount() >= expectedCount;
        }
        protected abstract bool MatchOneCharacter(char letter);
    }

    public class MatchLetter : MatchNLetters
    {
        private char myLetter;

        public MatchLetter(char letter)
            : base(1)
        {
            this.myLetter = letter;
        }

        protected override bool MatchOneCharacter(char letter)
        {
            return this.myLetter == letter;
        }
    }

    public class MatchOneLetter : MatchNLetters
    {
        public MatchOneLetter()
            : base(1)
        {
        }

        protected override bool MatchOneCharacter(char letter)
        {
            return true;
        }
    }

    public class MatchMultiLetter : MatchNLetters
    {
        string options;

        public MatchMultiLetter(string op)
            : base(1)
        {
            this.options = op;
        }

        protected override bool MatchOneCharacter(char letter)
        {
            return options.IndexOf(letter) > -1;
        }
    }
 
    #endregion

    public interface IMatchSet
    {
        List<Node> Match(List<Node> nodes);
        List<Node> MatchLetter(LetterNode l);
        List<Node> MatchTerminator(TerminatorNode t);
    }

    public abstract class MatchSetBase : IMatchSet
    {
        public List<Node> Match(List<Node> nodes)
        {
            List<Node> results = new List<Node>();
            List<Node> temp;
            foreach (Node n in nodes)
            {
                temp = Match(n);
                if (!Constants.IsNullOrEmpty<Node>(temp))
                    results.AddRange(temp);
            }
            return results;
        }

        private List<Node> Match(Node n)
        {
            return n.MatchDispatch(this);
        }

        #region IMatchSet Members

        public virtual List<Node> MatchLetter(LetterNode l)
        {
            List<Node> results = new List<Node>();

            if (AllMatchComplete())
                return results;

            MatchMaker current = GetCurrentMatcher();
            if (current.IsMatchComplete())
            {
                this.Next();
                results.AddRange(MatchLetter(l));
                this.Prev();
            }

            if (current.MatchLetter(l))
            {
                MatchLetterWithCurrent(l, results, current);

                current.UnmatchLetter(l);
            }
            return results;
        }

        public List<Node> MatchTerminator(TerminatorNode t)
        {
            List<Node> results = new List<Node>();
            if (AllMatchComplete())
                results.Add(t);
            return results;
        }

        #endregion

        protected abstract bool AllMatchComplete();
        protected abstract MatchMaker GetCurrentMatcher();
        protected abstract void Next();
        protected abstract void Prev();

        protected void MatchLetterWithCurrent(LetterNode l, List<Node> results, MatchMaker current)
        {
            bool anyMatch = false;
            LetterNode resultLetter = LetterNode.GetInstance(l.Letter);

            if (current.IsMatchComplete())
            {
                this.Next();
                anyMatch = MatchAndMerge(l, resultLetter) || anyMatch;
                this.Prev();
            }

            if (current.IsMatchOpen())
            {
                anyMatch = MatchAndMerge(l, resultLetter) || anyMatch;
            }
            else
            {
                this.Next();
                anyMatch = MatchAndMerge(l, resultLetter) || anyMatch;
                this.Prev();
            }

            if (anyMatch)
                results.Add(resultLetter);
        }

        private bool MatchAndMerge(LetterNode l, LetterNode resultLetter)
        {
            List<Node> temp = this.Match(l.ChildNodes);
            if (!Constants.IsNullOrEmpty<Node>(temp))
            {
                Dictionary.MergeChildren(resultLetter, temp);
                return true;
            }
            return false;
        }
    }

    public class BetaMatchSet : MatchSetBase
    {
        private List<MatchMaker> innerMatch;
        private int position;

        private BetaMatchSet()
        {
            this.innerMatch = new List<MatchMaker>();
            this.position = 0;
        }

        internal void AddMatchMaker(MatchMaker mkr)
        {
            this.innerMatch.Add(mkr);
        }

        public static BetaMatchSet GetInstance()
        {
            return new BetaMatchSet();
        }

        protected override bool AllMatchComplete()
        {
            return this.position >= this.innerMatch.Count;
        }

        protected override MatchMaker GetCurrentMatcher()
        {
            return this.innerMatch[position];
        }

        protected override void Next()
        {
            this.position++;
        }

        protected override void Prev()
        {
            this.position--;
        }
    }

    public class BetaMatchAnagram : MatchSetBase
    {
        private List<MatchMaker> innerMatch;
        private List<MatchMaker> available;
        private int position;
        
        private BetaMatchAnagram() : base()
        {
            this.innerMatch = new List<MatchMaker>();
            this.available = new List<MatchMaker>();
            this.position = 0;
        }

        public static BetaMatchAnagram GetInstance()
        {
            return new BetaMatchAnagram();
        }

        public override List<Node> MatchLetter(LetterNode l)
        {
            List<Node> results = new List<Node>();

            if (InnerMatchComplete())
                return results;

            MatchMaker current = GetCurrentMatcher();
            if (current.IsMatchComplete())
            {
                this.Next();
                results.AddRange(MatchLetter(l));
                this.Prev();
            }

            if (current.MatchLetter(l))
            {
                foreach (MatchMaker aMatch in this.available)
                {
                    if (aMatch.MatchLetter(l))
                    {
                        MatchLetterWithCurrent(l, results, current);
                        aMatch.UnmatchLetter(l);
                    }
                }
                current.UnmatchLetter(l);
            }
            return results;
        }

        internal void AddMatchMaker(MatchMaker mkr)
        {
            this.innerMatch.Add(mkr);
        }

        internal void AddAvailableLetter(MatchMaker mkr)
        {
            this.available.Add(mkr);
        }

        protected override bool AllMatchComplete()
        {
            return this.AvailableComplete() &&
                this.InnerMatchComplete();
        }

        protected override MatchMaker GetCurrentMatcher()
        {
            return this.innerMatch[position];
        }

        protected override void Next()
        {
            this.position++;
        }

        protected override void Prev()
        {
            this.position--;
        }

        private bool AvailableComplete()
        {
            foreach (MatchMaker aMatch in this.available)
            {
                if (!aMatch.IsMatchComplete())
                    return false;
            }
            return true;
        }

        private bool InnerMatchComplete()
        {
            return this.position >= this.innerMatch.Count;
        }
    }

    public class MatchSet : IMatchSet
    {
        private List<MatchMaker> innerMatch;
        private int position;

        private MatchSet()
        {
            this.innerMatch = new List<MatchMaker>();
            this.position = 0;
        }

        internal void AddMatchMaker(MatchMaker mkr)
        {
            this.innerMatch.Add(mkr);
        }

        public static MatchSet GetInstance()
        {
            return new MatchSet();
        }

        public List<Node> Match(List<Node> nodes)
        {
            List<Node> results = new List<Node>();
            List<Node> temp;
            foreach (Node n in nodes)
            {
                temp = Match(n);
                if (!Constants.IsNullOrEmpty<Node>(temp))
                    results.AddRange(temp);
            }
            return results;
        }

        private List<Node> Match(Node n)
        {
            return n.MatchDispatch(this);
        }

        #region IMatchSet Members

        public List<Node> MatchLetter(LetterNode l)
        {
            List<Node> results = new List<Node>();
            List<Node> temp;
            bool anyMatch = false;

            if (this.position >= this.innerMatch.Count)
                return results;

            MatchMaker current = this.innerMatch[this.position];
            if (current.IsMatchComplete())
            {
                this.position++;
                results.AddRange(MatchLetter(l));
                this.position--;
            }
            if (current.MatchLetter(l))
            {
                LetterNode resultLetter = LetterNode.GetInstance(l.Letter);
                // can keep matching, use for child matches
                if (current.IsMatchOpen())
                {
                    temp = this.Match(l.ChildNodes);
                    if (!Constants.IsNullOrEmpty<Node>(temp))
                    {
                        Dictionary.MergeChildren(resultLetter, temp);
                        anyMatch = true;
                    }
                }
                //match is done, increment position and continue
                else
                {
                    this.position++;
                    temp = this.Match(l.ChildNodes);
                    if (!Constants.IsNullOrEmpty<Node>(temp))
                    {
                        Dictionary.MergeChildren(resultLetter, temp);
                        anyMatch = true;
                    }
                    this.position--;
                }
                if (anyMatch)
                    results.Add(resultLetter);

                current.UnmatchLetter(l);
            }

            return results;
        }

        public List<Node> MatchTerminator(TerminatorNode t)
        {
            List<Node> results = new List<Node>();
            int tempPosition = this.position;
            while (tempPosition < this.innerMatch.Count)
            {
                MatchMaker matcher = this.innerMatch[tempPosition];
                if (!matcher.IsMatchComplete())
                    break;

                tempPosition++;
            }
            if (tempPosition >= this.innerMatch.Count)
                results.Add(t);

            return results;
        }

        #endregion
    }

    public class MatchAnagram : IMatchSet
    {
        private List<MatchMaker> innerMatch;
        private List<MatchMaker> available;
        private int position;

        private MatchAnagram()
        {
            this.innerMatch = new List<MatchMaker>();
            this.available = new List<MatchMaker>();
            this.position = 0;
        }

        internal void AddMatchMaker(MatchMaker mkr)
        {
            this.innerMatch.Add(mkr);
        }

        internal void AddAvailableLetter(MatchMaker mkr)
        {
            this.available.Add(mkr);
        }

        public static MatchAnagram GetInstance()
        {
            return new MatchAnagram();
        }

        public List<Node> Match(List<Node> nodes)
        {
            List<Node> results = new List<Node>();
            List<Node> temp;
            foreach (Node n in nodes)
            {
                temp = Match(n);
                if (!Constants.IsNullOrEmpty<Node>(temp))
                    results.AddRange(temp);
            }
            return results;
        }

        private List<Node> Match(Node n)
        {
            return n.MatchDispatch(this);
        }

        #region IMatchSet Members

        public List<Node> MatchLetter(LetterNode l)
        {
            List<Node> results = new List<Node>();
            List<Node> temp;
            bool anyMatch = false;

            if (this.position >= this.innerMatch.Count)
                return results;

            MatchMaker current = this.innerMatch[this.position];
            if (current.IsMatchComplete())
            {
                this.position++;
                results.AddRange(MatchLetter(l));
                this.position--;
            }
            if (current.MatchLetter(l))
            {
                foreach (MatchMaker aMatch in this.available)
                {
                    if (aMatch.MatchLetter(l))
                    {
                        LetterNode resultLetter = LetterNode.GetInstance(l.Letter);
                        // can keep matching, use for child matches
                        if (current.IsMatchOpen())
                        {
                            temp = this.Match(l.ChildNodes);
                            if (!Constants.IsNullOrEmpty<Node>(temp))
                            {
                                Dictionary.MergeChildren(resultLetter, temp);
                                anyMatch = true;
                            }
                        }
                        //match is done, increment position and continue
                        else
                        {
                            this.position++;
                            temp = this.Match(l.ChildNodes);
                            if (!Constants.IsNullOrEmpty<Node>(temp))
                            {
                                Dictionary.MergeChildren(resultLetter, temp);
                                anyMatch = true;
                            }
                            this.position--;
                        }
                        if (anyMatch)
                            results.Add(resultLetter);

                        aMatch.UnmatchLetter(l);
                    }
                }
                current.UnmatchLetter(l);
            }

            return results;
        }

        public List<Node> MatchTerminator(TerminatorNode t)
        {
            List<Node> results = new List<Node>();
            if (InnerMatchComplete() && AvailableComplete())
            {
                results.Add(t);
            }

            return results;
        }

        private bool AvailableComplete()
        {
            foreach (MatchMaker aMatch in this.available)
            {
                if (!aMatch.IsMatchComplete())
                    return false;
            }
            return true;
        }

        private bool InnerMatchComplete()
        {
            for(int tpos = this.position; tpos < this.innerMatch.Count; tpos++)
            {
                MatchMaker matcher = this.innerMatch[tpos];
                if (!matcher.IsMatchComplete())
                    return false;
            }
            return true;
        }

        #endregion
    }

    public class Constants
    {
        public static bool IsNullOrEmpty<T>(T[] array)
        {
            if (array == null || array.Length == 0)
                return true;
            return false;
        }

        public static bool IsNullOrEmpty<T>(List<T> array)
        {
            if (array == null || array.Count == 0)
                return true;
            return false;
        }
    }
}