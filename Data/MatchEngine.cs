using System;
using System.Collections.Generic;
using System.Text;

namespace CloseEnoughDictionary.Data.Legacy
{
    //
    // How to match 
    // Match array- match each letter and unmatch afterward.
    // Match anagrams- match letter, and unmatch in the engine. 
    //
    public class MatchEngine
    {
        private MatchEngine()
        {
        }

        public static MatchEngine GetInstance()
        {
            return new MatchEngine();
        }

        public List<Node> Match(MatchList matcher, List<Node> words)
        {
            List<Node> results = new List<Node>();
            foreach (Node node in words)
            {
                MatchList matchClone = matcher.CopyInstance();
                Node[] temp = Match(matchClone, node);
                if (!Constants.IsNullOrEmpty<Node>(temp))
                    results.AddRange(temp);
            }
            return results;
        }

        public Node[] Match(MatchList matcher, Node word)
        {
            return word.MatchDispatch(this, matcher);
        }

        public Node[] MatchLetter(MatchList matcher, LetterNode word)
        {
            char letter = word.Letter;
            List<MatchList> matches = matcher.MatchCharacter(letter);
            if (Constants.IsNullOrEmpty<MatchList>(matches))
                return new Node[] { };

            bool anyMatches = false;
            LetterNode result = LetterNode.GetInstance(letter);
            foreach (MatchList match in matches)
            {
                List<Node> nodes = Match(match, word.ChildNodes);
                if (Constants.IsNullOrEmpty<Node>(nodes))
                    continue;

                anyMatches = true;
                Dictionary.MergeChildren(result, nodes);
                //result.ChildNodes.AddRange(results);
            }

            if (anyMatches)
                return new Node[] { result };
            else
                return new Node[] { };
        }

        public Node[] MatchTerminator(MatchList matcher, TerminatorNode word)
        {
            List<MatchList> matches = matcher.MatchTerminator();
            if (Constants.IsNullOrEmpty<MatchList>(matches))
                return new Node[] { };
            return new Node[] { word };
        }
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

    public class MatchFactory
    {
        public static MatchFactory GetInstance()
        {
            return new MatchFactory();
        }

        public MatchArray GetMatchArray(string word)
        {
            MatchArray matcher = MatchArray.GetInstance();
            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (c == '*')
                    matcher.AddMatchMaker(new MatchAll());
                else if (c == '?' || c == '.')
                    matcher.AddMatchMaker(new MatchOne());
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
                            matcher.AddMatchMaker(new MatchSet(inner));
                        i = end;
                    }
                }
                else
                    matcher.AddMatchMaker(new MatchLetter(c));
            }

            return matcher;
        }
    }
}
