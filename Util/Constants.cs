using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloseEnoughDictionary.Util
{
    public class Constants
    {
        public static bool DebugEnabled = false;
        public static Action<string> DebugAction;

        public static void Debug(string message)
        {
            if (DebugAction != null && DebugEnabled)
            {
                DebugAction(message);
            }
        }
    }
}
