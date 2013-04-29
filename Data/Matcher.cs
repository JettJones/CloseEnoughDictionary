using System;
using System.Collections.Generic;
using System.Text;

namespace CloseEnoughDictionary.Data.Legacy
{
    #region Searching patterns
    //match cases
    public interface MatchMaker
    {
        bool MatchTerminator();
        //match one character - true if match succeeds
        bool MatchCharacter(char letter);
        //match complete - true if next character can be used.
        bool IsMatchComplete();
        //is match usable - true if this match can be used again.
        bool IsMatchOpen();

        //clone method
        MatchMaker CopyInstance();
    }

    // match *
    public class MatchAll : MatchMaker
    {
        public MatchAll()
        {
        }

        public bool MatchTerminator()
        {
            return false;
        }

        public bool MatchCharacter(char letter)
        {
            return true;
        }
        public bool IsMatchComplete()
        {
            return true;
        }
        public bool IsMatchOpen()
        {
            return true;
        }

        public MatchMaker CopyInstance()
        {
            return this;
        }
    }

    public abstract class MatcherBase : MatchMaker
    {
        private bool seenLetter;

        public MatcherBase()
        {
            this.seenLetter = false;
        }

        protected MatcherBase(MatcherBase toCopy)
        {
            this.seenLetter = toCopy.seenLetter;
        }

        public bool MatchTerminator()
        {
            return false;
        }

        public bool MatchCharacter(char letter)
        {
            if (seenLetter)
                return false;
            else if (MatchOneLetter(letter))
            {
                this.seenLetter = true;
                return true;
            }
            else
                return false;
        }
        public bool IsMatchComplete()
        {
            return this.seenLetter == true;
        }
        public bool IsMatchOpen()
        {
            return this.seenLetter == false;
        }

        public abstract MatchMaker CopyInstance();
        protected abstract bool MatchOneLetter(char letter);
    }

    public class MatchLetter : MatcherBase
    {
        private char myLetter;

        public MatchLetter(char letter) : base()
        {
            this.myLetter = letter;
        }
        private MatchLetter(MatchLetter toCopy) : base(toCopy)
        {
            this.myLetter = toCopy.myLetter;
        }

        public override MatchMaker CopyInstance()
        {
            MatchLetter matcher = new MatchLetter(this);
            return matcher;
        }

        protected override bool MatchOneLetter(char letter)
        {
            return this.myLetter == letter;
        }
    }

    public class MatchOne : MatcherBase
    {
        public MatchOne() : base()
        {
        }
        private MatchOne(MatchOne toCopy)
            : base(toCopy)
        {
        }

        public override MatchMaker CopyInstance()
        {
            MatchOne matcher = new MatchOne(this);
            return matcher;
        }

        protected override bool MatchOneLetter(char letter)
        {
            return true;
        }
    }

    public class MatchSet : MatcherBase
    {
        private string options;

        public MatchSet(string op) : base()
        {
            this.options = op;
        }

        private MatchSet(MatchSet set)
            : base(set)
        {
            this.options = set.options;
        }

        public override MatchMaker CopyInstance()
        {
            MatchSet matcher = new MatchSet(this);
            return matcher;
        }

        protected override bool MatchOneLetter(char letter)
        {
            return options.Contains(letter.ToString());
        }
    }

    #endregion

    public interface MatchList
    {
        List<MatchList> MatchCharacter(char letter);
        List<MatchList> MatchTerminator();
        MatchList CopyInstance(); // like IClonable with typing.
    }

    public class MatchArray : MatchList
    {
        private List<MatchMaker> innerMatch;
        private int position;

        private int minLength;
        private int maxLength;

        private MatchArray()
        {
            this.innerMatch = new List<MatchMaker>();
            this.position = 0;
            this.minLength = 0;
            this.maxLength = 1000;
        }

        private MatchArray(List<MatchMaker> matches, int position, int min, int max)
        {
            this.innerMatch = matches;
            this.position = position;
            this.minLength = min;
            this.maxLength = max;
        }

        internal void AddMatchMaker(MatchMaker mkr)
        {
            this.innerMatch.Add(mkr);
        }

        public static MatchArray GetInstance()
        {
            return new MatchArray();
        }

        public static MatchArray GetInstance(List<MatchMaker> matches, int position, MatchArray toCopy)
        {
            return new MatchArray(matches, position, toCopy.minLength, toCopy.maxLength);
        }

        public List<MatchList> MatchTerminator()
        {
            List<MatchList> matchSet = new List<MatchList>();
            int tempPosition = this.position;
            while (tempPosition < this.innerMatch.Count)
            {
                MatchMaker matcher = this.innerMatch[tempPosition];
                if (!matcher.IsMatchComplete())
                    break;

                tempPosition++;
            }
            //either we're done, or all remaining matches are satisfied.
            if (tempPosition >= this.innerMatch.Count)
            {
                matchSet.Add(this);
            }
            return matchSet;
        }

        public List<MatchList> MatchCharacter(char letter)
        {
            List<MatchList> matchSet = new List<MatchList>();
            if (this.position >= this.innerMatch.Count)
                return matchSet;

            MatchMaker current = this.innerMatch[this.position];
            if (current.IsMatchComplete())
            {
                int next = this.position + 1;
                matchSet.AddRange(MatchArray.GetInstance(this.CopyMatchers(next), next, this).MatchCharacter(letter));
            }
            if (current.MatchCharacter(letter))
            {
                if (current.IsMatchOpen())
                    matchSet.Add(this);
                else
                    matchSet.Add(MatchArray.GetInstance(this.innerMatch, this.position + 1, this));
            }
            return matchSet;
        }

        public MatchList CopyInstance()
        {
            List<MatchMaker> matchers = CopyMatchers(this.position);

            return MatchArray.GetInstance(matchers, this.position, this);
        }

        private List<MatchMaker> CopyMatchers(int position)
        {
            List<MatchMaker> matchers = new List<MatchMaker>(this.innerMatch.Count);

            //only clone position +;
            int index = 0;
            foreach (MatchMaker match in this.innerMatch)
            {
                if (index < position)
                    matchers.Add(match);
                else
                    matchers.Add(match.CopyInstance());

                index++;
            }
            return matchers;
        }
    }
    
}
