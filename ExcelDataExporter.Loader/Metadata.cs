using System.Collections.Generic;

namespace Physalia.ExcelDataExporter.Loader
{
    public class Metadata
    {
        private const string DefaultKeyName = "name";

        private Dictionary<string, string> _items;

        public string this[string key] => _items.TryGetValue(key, out string value) ? value : "";
        public string OverrideName => _items.TryGetValue(DefaultKeyName, out string name) ? name : "";

        private Metadata() { }

        public static Metadata Parse(string text)
        {
            var items = new Dictionary<string, string>();
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                string[] splits = lines[i].Trim().Split('=');
                if (splits.Length == 2)
                {
                    items.Add(splits[0], splits[1]);
                }
            }

            var metadata = new Metadata { _items = items };
            return metadata;
        }
    }
}
