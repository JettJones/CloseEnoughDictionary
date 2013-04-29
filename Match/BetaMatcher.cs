using System;
using System.Collections.Generic;
using System.Text;
using CloseEnoughDictionary.Data;
using CloseEnoughDictionary.Util;

namespace CloseEnoughDictionary.Match
{
    public class FuzzMatch
    {
        private int fuzzLevel;

        public FuzzMatch(int level)
        {
            this.fuzzLevel = level;
        }

        public bool CanFuzz()
        {
            return this.fuzzLevel > 0;
        }

        public void Fuzz()
        {
            this.fuzzLevel--;
        }

        public void Undo()
        {
            this.fuzzLevel++;
        }
    }

    public abstract class MatchSetBase : IMatchSet
    {
        protected FuzzMatch fuzzer;
        protected MatchSetBase(FuzzMatch fuz)
        {
            this.fuzzer = fuz;
        }
        protected MatchSetBase()
            : this(new FuzzMatch(0))
        {
        }

        public List<INode> Match(List<INode> INodes)
        {
            Constants.Debug("MatchSetBase.Match.Enter");
            DebugCounts.CallFunction("MatchSetBase.Match");

            List<INode> results = new List<INode>();
            List<INode> temp;
            foreach (INode n in INodes)
            {
                temp = Match(n);
                if (!temp.IsNullOrEmpty())
                    results.AddRange(temp);
            }
            Constants.Debug("MatchSetBase.Match.Exit");

            return results;
        }

        private List<INode> Match(INode n)
        {
            return n.MatchDispatch(this);
        }

        #region IMatchSet Members

        public virtual List<INode> MatchLetter(LetterNode l)
        {
            Constants.Debug("MatchSetBase.MatchLetter.Enter");
            DebugCounts.CallFunction("MatchSetBase.MatchLetter");
            List<INode> results = new List<INode>();

            if (!AnyMatchOpen())
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

            Constants.Debug("MatchSetBase.MatchLetter.Exit");
            return results;
        }


        public virtual List<INode> MatchTerminator(TerminatorINode t)
        {
            Constants.Debug("MatchSetBase.MatchTerminator");
            DebugCounts.CallFunction("MatchSetBase.MatchTerminator");
            List<INode> results = new List<INode>();
            if (AllMatchComplete())
            {
                DebugCounts.CallFunction("MatchSetBase.MatchTerminator.Successful");
                results.Add(t);
            }
            return results;
        }

        #endregion

        protected abstract bool AllMatchComplete();
        protected abstract bool AnyMatchOpen();
        protected abstract MatchMaker GetCurrentMatcher();
        protected abstract void Next();
        protected abstract void Prev();

        protected void MatchLetterWithCurrent(LetterNode l, List<INode> results, MatchMaker current)
        {
            Constants.Debug("MatchSetBase.MatchLetterWithCurrent.Enter");
            DebugCounts.CallFunction("MatchSetBase.MatchLetterWithCurrent");

            bool anyMatch = false;
            LetterNode resultLetter = LetterNode.GetInstance(l.Letter);

            if (current.IsMatchComplete())
            {
                this.Next();
                anyMatch = MatchAndMerge(l, resultLetter) || anyMatch;
                this.Prev();
            }
            else
            {
                // we probably should not hit this case-
                // if the current matcher is not complete, but it is still open.
                // ie. it has not matched enough, but it cannot match any more.

                if (!current.IsMatchOpen())
                {
                    this.Next();
                    anyMatch = MatchAndMerge(l, resultLetter) || anyMatch;
                    this.Prev();
                }
            }

            if (current.IsMatchOpen())
            {
                anyMatch = MatchAndMerge(l, resultLetter) || anyMatch;
            }

            if (anyMatch)
            {
                DebugCounts.CallFunction("MatchSetBase.MatchLetterWithCurrent.AnyMatchTrue");
                results.Add(resultLetter);
            }
            Constants.Debug("MatchSetBase.MatchLetterWithCurrent.Exit");
        }

        private bool MatchAndMerge(LetterNode l, LetterNode resultLetter)
        {
            Constants.Debug("MatchSetBase.MatchAndMerge");
            List<INode> temp = this.Match(l.ChildNodes);
            if (!temp.IsNullOrEmpty())
            {
                TrieBase.MergeChildren(resultLetter, temp);
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

        protected override bool AnyMatchOpen()
        {
            for (int i = this.position; i < this.innerMatch.Count; i++)
            {
                MatchMaker mm = this.innerMatch[i];
                if (mm.IsMatchOpen())
                    return true;
            }
            return false;
        }

        protected override bool AllMatchComplete()
        {
            for (int i = this.position; i < this.innerMatch.Count; i++)
            {
                MatchMaker mm = this.innerMatch[i];
                if (!mm.IsMatchComplete())
                    return false;
            }
            return true;
        }

        protected override MatchMaker GetCurrentMatcher()
        {
            return this.innerMatch[position];
        }

        protected override void Next()
        {
            this.position++;
            Constants.Debug("BetaMatchSet.Next : " + this.position);
        }

        protected override void Prev()
        {
            this.position--;
            Constants.Debug("BetaMatchSet.Prev : " + this.position);
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

        public override List<INode> MatchLetter(LetterNode l)
        {
            List<INode> results = new List<INode>();

            if (!this.InnerMatchOpen())
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

        protected override bool AnyMatchOpen()
        {
            return this.InnerMatchOpen() && this.AvailableOpen();
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

        private bool AvailableOpen()
        {
            foreach (MatchMaker aMatch in this.available)
            {
                if (aMatch.IsMatchOpen())
                    return true;
            }

            return false;
        }

        private bool InnerMatchOpen()
        {
            for (int i = this.position; i < this.innerMatch.Count; i++)
            {
                MatchMaker mm = this.innerMatch[i];
                if (mm.IsMatchOpen())
                    return true;
            }
            return false;
        }
    }

    public class BetaMatchCircle : IMatchSet
    {
        private List<MatchMaker> innerMatch;
        private List<BetaMatchSet> innerMatcher;

        private BetaMatchCircle()
        {
            this.innerMatch = new List<MatchMaker>();
            this.innerMatcher = null;
        }

        public static BetaMatchCircle GetInstance()
        {
            return new BetaMatchCircle();
        }

        internal void AddMatchMaker(MatchMaker mkr)
        {
            this.innerMatch.Add(mkr);
        }

        private void PrepareMatchers()
        {
            if (this.innerMatcher == null)
            {
                //List<List<MatchMaker>> matchers;
                this.innerMatcher = new List<BetaMatchSet>();

                // all variants of our current matchers
                List<MatchMaker> temp = new List<MatchMaker>();
                temp.AddRange(this.innerMatch);

                for (int i = 0; i < this.innerMatch.Count; i++)
                {
                    List<MatchMaker> fwd = new List<MatchMaker>(temp);
                    List<MatchMaker> rev = new List<MatchMaker>(temp);
                    rev.Reverse();
                    this.CreateMatchSet(fwd);
                    this.CreateMatchSet(rev);

                    // rotate the list by one.
                    temp.Add(temp[0]);
                    temp.RemoveAt(0);
                }
            }
        }

        public List<INode> Match(List<INode> INodes)
        {
            Constants.Debug("BetaMatchCircle.Match.Enter");
            DebugCounts.CallFunction("BetaMatchCircle.Match");

            PrepareMatchers();

            List<INode> results = new List<INode>();
            List<INode> temp;
            foreach (INode n in INodes)
            {
                temp = Match(n);
                if (!temp.IsNullOrEmpty())
                    results.AddRange(temp);
            }

            Constants.Debug("BetaMatchCircle.Match.Exit");

            return results;
        }

        private List<INode> Match(INode n)
        {
            return n.MatchDispatch(this);
        }

        private void CreateMatchSet(List<MatchMaker> fwd)
        {
            BetaMatchSet ms = BetaMatchSet.GetInstance();
            fwd.ForEach(act => ms.AddMatchMaker(act));

            this.innerMatcher.Add(ms);
        }

        public List<INode> MatchLetter(LetterNode l)
        {
            List<INode> result = new List<INode>();
            foreach (BetaMatchSet ms in this.innerMatcher)
            {
                result.AddRange(ms.MatchLetter(l));
            }

            return result;
        }

        public List<INode> MatchTerminator(TerminatorINode t)
        {
            Constants.Debug("BetaMatchCircle.MatchTerminator");
            DebugCounts.CallFunction("BetaMatchCircle.MatchTerminator");
            List<INode> results = new List<INode>();

            foreach (BetaMatchSet ms in this.innerMatcher)
            {
                results.AddRange(ms.MatchTerminator(t));
            }

            return results;
        }
    }

    // todo - remove puzzle specific.
    public class BetaMatchStates : MatchSetBase
    {
        class LetterClass
        {
            public static Dictionary<char, LetterClass> Classes;

            static LetterClass()
            {
                //Classes = SetupForNavalFlags();
                Classes = SetupForBraille();
            }

            private static Dictionary<char, LetterClass> SetupForNavalFlags()
            {
                Dictionary<char, LetterClass> classes = new Dictionary<char, LetterClass>();

                List<string> categories = new List<string>();
                categories.Add("acfhjmnpstuvwx"); // white
                categories.Add("acdegjkmnpstwxz"); // blue
                categories.Add("bcefhortuvwyz"); // red
                categories.Add("dgikloqryz"); // yellow
                categories.Add("ilz"); // black
                categories.Add("lnprsuwx"); // cube
                categories.Add("fmovyz"); // triangle
                categories.Add("psw"); // donut
                categories.Add("aghkt"); // vertical bar
                categories.Add("cdejt"); // horizontal 

                for (char i = 'a'; i <= 'z'; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string set in categories)
                    {
                        if (set.Contains(i.ToString()))
                        {
                            sb.Append(set.Replace(i.ToString(), String.Empty));
                        }
                    }

                    classes.Add(i, new LetterClass(sb.ToString()));
                }

                return classes;
            }

            private static Dictionary<char, LetterClass> SetupForBraille()
            {
                Dictionary<char, LetterClass> classes = new Dictionary<char, LetterClass>();

                classes.Add('a', new LetterClass("is"));
                classes.Add('b', new LetterClass("acdeijkmnstuxyz"));
                classes.Add('c', new LetterClass("abehijklorstuvwz"));
                classes.Add('d', new LetterClass("cefhjmoprtwxz"));
                classes.Add('e', new LetterClass("abcfjklmptuvwx"));

                classes.Add('f', new LetterClass("bcdhijlmnrstvwxy"));
                classes.Add('g', new LetterClass("dfhjnprtvy"));
                classes.Add('h', new LetterClass("bdefjlnoptvwyz"));
                classes.Add('i', new LetterClass("bcdhlmnrvxy"));
                classes.Add('j', new LetterClass("dfhinprsy"));

                classes.Add('k', new LetterClass("abcdefghst"));
                classes.Add('l', new LetterClass("bfghstuxyz"));
                classes.Add('m', new LetterClass("cdfgrstuvz"));
                classes.Add('n', new LetterClass("dgmoprtx"));
                classes.Add('o', new LetterClass("deghklmptuvx"));

                classes.Add('p', new LetterClass("fglmnrstvxy"));
                classes.Add('q', new LetterClass("gnpy"));
                classes.Add('r', new LetterClass("ghlnoptvyz"));
                classes.Add('s', new LetterClass("fgijlmnrvwxy"));
                classes.Add('t', new LetterClass("gjnprswy"));

                classes.Add('u', new LetterClass("klmnopqr"));
                classes.Add('v', new LetterClass("lpqruxyz"));
                classes.Add('w', new LetterClass("gjqty"));
                classes.Add('x', new LetterClass("mnpquvz"));
                classes.Add('y', new LetterClass("nqxz"));
                classes.Add('z', new LetterClass("noqruvx"));

                return classes;
            }

            string next;
            public LetterClass(string n)
            {
                next = n;
            }

            public bool Allow(char c)
            {
                return next.Contains(c.ToString());
            }
        }

        class SetState : MatchMaker
        {
            List<char> matchedLetters;

            public SetState()
            {
                matchedLetters = new List<char>();
            }

            #region MatchMaker Members

            public bool MatchTerminator()
            {
                return true;
            }

            public bool MatchLetter(LetterNode letter)
            {
                if (matchedLetters.Count > 0)
                {
                    char last = matchedLetters[matchedLetters.Count - 1];
                    LetterClass current = LetterClass.Classes[last];
                    if (current.Allow(letter.Letter) && !matchedLetters.Contains(letter.Letter))
                    {
                        matchedLetters.Add(letter.Letter);
                        return true;
                    }
                    return false;
                }
                else if (LetterClass.Classes.ContainsKey(letter.Letter))
                {
                    matchedLetters.Add(letter.Letter);
                    return true;
                }

                return false;
            }

            public bool UnmatchLetter(LetterNode letter)
            {
                if (matchedLetters.Count > 0)
                {
                    char last = matchedLetters[matchedLetters.Count - 1];
                    if (last == letter.Letter)
                    {
                        matchedLetters.RemoveAt(matchedLetters.Count - 1);
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            public bool IsMatchComplete()
            {
                return false;
            }

            public bool IsMatchOpen()
            {
                return true;
            }

            #endregion
        }

        private SetState state = new SetState();

        protected override bool AllMatchComplete()
        {
            return true;
        }

        protected override bool AnyMatchOpen()
        {
            return true;
        }

        protected override MatchMaker GetCurrentMatcher()
        {
            return state;
        }

        protected override void Next()
        {
            // no op.
        }

        protected override void Prev()
        {
            // no op (handled by unmatch);
        }
    }
}