using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CloseEnoughDictionary.Util
{
    /// <summary>
    /// Loads a dictionary from a file as newline separated.
    ///
    /// aspirationally we could load from TEA dictionaries or other file types.
    /// </summary>
    internal class FileLoader
    {
        public static IEnumerable<string> LoadDictionary(string path)
        {
            using (var file = File.OpenRead(path))
            using (var reader = new StreamReader(file))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    var filtered = line.Split('/').FirstOrDefault();
                    filtered = filtered.ToLowerInvariant();
                    filtered = Regex.Replace(filtered, "[^a-z -]", "");

                    yield return filtered;

                    line = reader.ReadLine();
                }
            }
        }
    }
}
