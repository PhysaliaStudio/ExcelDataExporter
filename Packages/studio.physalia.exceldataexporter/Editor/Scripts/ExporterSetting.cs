using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public static class ExporterSetting
    {
        public static int FilterFlag = -1;

        private static readonly FilterSetting filterSetting = new();

        public static void SetFilterWords(params string[] words)
        {
            filterSetting.SetWords(words);
        }

        public static bool IsMatchFilterWords(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return true;
            }

            return filterSetting.IsMatch(name);
        }
    }

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
