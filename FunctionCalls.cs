using System;
using System.Collections.Generic;

namespace CloseEnoughDictionary
{
    /// <summary>
    /// Track how many times functions get called.
    /// </summary>
    class DebugCounts
    {
        private Dictionary<string, int> myCounts;
        private DebugCounts()
        {
            this.myCounts = new Dictionary<string, int>();
        }
        static DebugCounts Instance = new DebugCounts();

        public static void CallFunction(string name)
        {
            Instance.PrivateCallFunction(name);
        }

        private void PrivateCallFunction(string name)
        {
            try
            {
                int count = myCounts[name];
                myCounts[name] = count + 1;
            }
            catch (KeyNotFoundException)
            {
                myCounts.Add(name, 1);
            }
        }

        public static List<string> GetStatistics()
        {
            return Instance.PrivateGetStatistics();
        }

        private List<string> PrivateGetStatistics()
        {
            List<string> strings = new List<string>();
            foreach (string key in myCounts.Keys)
            {
                strings.Add(String.Format("{0} = {1}", key, myCounts[key]));
            }
            return strings;
        }

        internal static void Reset()
        {
            Instance = new DebugCounts();
        }
    }
}
