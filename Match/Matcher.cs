using System.Collections.Generic;

using CloseEnoughDictionary.Data;

namespace CloseEnoughDictionary.Match
{
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

}
