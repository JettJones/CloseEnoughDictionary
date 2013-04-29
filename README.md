CloseEnoughDictionary
=====================

Trie dictionary in C#, aiming to suggest words even if the pattern to be is incorrect.

Why?
====
Short answer: personal entertainment. 

Fast pattern matching dictionary on windows is well served by TEA (http://www.crosswordman.com/tea.html)
among other options.  Plus searching for Trie-Dictionary will bring up another 30 projects on github alone.
About the only reason I've wanted to roll my own version was the occasional search that didn't fit well
in Tea, and of those the only one that's implemented here is circular search - for crosswords like a
rows garden (http://ariespuzzles.com/category/rows-garden/).

Now that that baseline is out of the way, I may add additional features, but we all know I'll probably
just endlessly refactor things looking for prettier code. *sigh*

What?
=====
Ok, now we're past the not-fit-for-any-purpose stage, what can we do here?

- load a dictionary (line-per word format for now)
- find some matches > awe*me , *;loco, ...pel%
- revel in the non-patterns I was using in 2006. WinForms? apparently I thought that was a good idea.


Dictionaries?
=============
right, I'm havent checked in a dictionary with the tool, as there are plenty to be found online. A fine place to start:

http://www.puzzlers.org/dokuwiki/doku.php?id=solving:wordlists:about:start
