using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloseEnoughDictionary.Data
{
    public interface IMatchSet
    {
        List<INode> Match(List<INode> INodes);
        List<INode> MatchLetter(LetterNode l);
        List<INode> MatchTerminator(TerminatorINode t);
    }
}
