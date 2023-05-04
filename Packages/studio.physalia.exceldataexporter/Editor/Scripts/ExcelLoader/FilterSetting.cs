using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    internal class FilterSetting
    {
        private readonly List<string> filterWords = new();

        internal void SetWords(params string[] words)
        {
            filterWords.Clear();
            filterWords.AddRange(words);
        }

        internal void Clear()
        {
            filterWords.Clear();
        }

        internal bool IsMatch(string name)
        {
            // Note: Special case for empty filter words.
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            for (var i = 0; i < filterWords.Count; i++)
            {
                if (filterWords[i].Contains(name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
