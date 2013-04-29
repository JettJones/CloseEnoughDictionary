using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace CloseEnoughDictionary.Util
{
    public static class Extensions
    {
        public static TValue GetOrInit<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Expression<Func<TValue>> elseCreate)
        {
            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                var compiled = elseCreate.Compile();
                result = compiled();
                dictionary[key] = result;
            }
            return result;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> array)
        {
            if (array == null || array.Count == 0)
                return true;
            return false;
        }

        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
                return true;
            return false;
        }

        [DebuggerStepThrough]
        public static void InvokeMaybe(this Control control, Action<Control> action){
            if (control.InvokeRequired)
            {
                control.Invoke(action, control);
            }
            else
            {
                action(control);
            }
        }
    }
}
