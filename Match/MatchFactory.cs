using CloseEnoughDictionary.Data;

namespace CloseEnoughDictionary.Match
{
    public class MatchFactory
    {
        public static MatchFactory GetInstance()
        {
            return new MatchFactory();
        }

        public IMatchSet GetMatcher(string word)
        {
            if (word.IndexOf(';') >= 0)
                return GetBetaMatchAnagram(word);
            else if (word.IndexOf('%') >= 0)
                return GetBetaMatchCircle(word);
            else if (word.Equals("PUZZEL"))
                return GetBetaMatchState(word);
            else
                return GetBetaMatchSet(word);
        }

        private IMatchSet GetBetaMatchSet(string word)
        {
            BetaMatchSet matcher = BetaMatchSet.GetInstance();
            //check for multiple stars in a row.
            bool multiStar = false;
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (c == '*')
                {
                    if (multiStar) continue;

                    multiStar = true;
                    matcher.AddMatchMaker(new MatchAll());
                }
                else
                {
                    multiStar = false;

                    if (c == '?' || c == '.')
                        matcher.AddMatchMaker(new MatchOneLetter());
                    else if (c == '[')
                    {
                        int end = word.IndexOf(']', i);
                        if (end == -1) //thow exception?
                        {
                            matcher.AddMatchMaker(new MatchLetter(c));
                        }
                        else
                        {
                            string inner = word.Substring(i + 1, end - i - 1);
                            if (inner.Length > 0)
                                matcher.AddMatchMaker(new MatchMultiLetter(inner));
                            i = end;
                        }
                    }
                    else
                        matcher.AddMatchMaker(new MatchLetter(c));
                }
            }
            return matcher;
        }

        private IMatchSet GetBetaMatchAnagram(string word)
        {
            BetaMatchAnagram matcher = BetaMatchAnagram.GetInstance();
            bool anagramState = false;
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];

                if (c == ';')
                    anagramState = true;
                else if (c == '?' || c == '.')
                    AddLetterToBetaAnagram(matcher, anagramState, new MatchOneLetter());
                else if (c == '*')
                    AddLetterToBetaAnagram(matcher, anagramState, new MatchAll());
                else if (c == '[')
                {
                    int end = word.IndexOf(']', i);
                    if (end == -1) //thow exception?
                    {
                        AddLetterToBetaAnagram(matcher, anagramState, new MatchLetter(c));
                    }
                    else
                    {
                        string inner = word.Substring(i + 1, end - i - 1);
                        if (inner.Length > 0)
                            AddLetterToBetaAnagram(matcher, anagramState, new MatchMultiLetter(inner));
                        i = end;
                    }
                }
                else
                    AddLetterToBetaAnagram(matcher, anagramState, new MatchLetter(c));
            }
            return matcher;
        }

        private void AddLetterToBetaAnagram(BetaMatchAnagram matcher, bool anagramState, MatchMaker Matcher)
        {
            if (anagramState == true)
                matcher.AddAvailableLetter(Matcher);
            else
                matcher.AddMatchMaker(Matcher);
        }

        /// <summary>
        /// Match a word in a circle - unknown starting and ending points
        /// </summary>
        private IMatchSet GetBetaMatchCircle(string word)
        {
            BetaMatchCircle matcher = BetaMatchCircle.GetInstance();
            //check for multiple stars in a row.
            bool multiStar = false;
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (c == '*')
                {
                    if (multiStar) continue;

                    multiStar = true;
                    matcher.AddMatchMaker(new MatchAll());
                }
                else
                {
                    multiStar = false;

                    if (c == '?' || c == '.')
                        matcher.AddMatchMaker(new MatchOneLetter());
                    else if (c == '[')
                    {
                        int end = word.IndexOf(']', i);
                        if (end == -1) //thow exception?
                        {
                            matcher.AddMatchMaker(new MatchLetter(c));
                        }
                        else
                        {
                            string inner = word.Substring(i + 1, end - i - 1);
                            if (inner.Length > 0)
                                matcher.AddMatchMaker(new MatchMultiLetter(inner));
                            i = end;
                        }
                    }
                    else if (c == '%')
                    {
                        // no op
                    }
                    else
                        matcher.AddMatchMaker(new MatchLetter(c));
                }
            }
            return matcher;
        }

        private IMatchSet GetBetaMatchState(string word)
        {
            return new BetaMatchStates();
        }

    }
}
