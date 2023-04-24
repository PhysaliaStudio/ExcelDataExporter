using System.Collections.Generic;
using System.IO;

namespace Physalia.ExcelDataExporter
{
    public enum SheetLayout { Horizontal, Vertical }
    public enum SheetType { DataTable, Setting }

    public class Metadata
    {
        private class Item
        {
            public string key;
            public string value;
        }

        private bool export = true;
        private string overrideName;
        private string namespaceName;
        private SheetLayout sheetLayout = SheetLayout.Horizontal;
        private SheetType sheetType = SheetType.DataTable;

        public bool Export => export;
        public string OverrideName => overrideName;
        public string NamespaceName => namespaceName;
        public SheetLayout SheetLayout => sheetLayout;
        public SheetType SheetType => sheetType;

        private Metadata() { }

        public static Metadata Parse(string text)
        {
            var items = new List<Item>();
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                string[] splits = lines[i].Trim().Split('=');
                if (splits.Length == 2)
                {
                    items.Add(new Item { key = splits[0], value = splits[1] });
                }
            }

            var metadata = new Metadata();
            for (var i = 0; i < items.Count; i++)
            {
                HandleItem(metadata, items[i]);
            }

            return metadata;
        }

        private static void HandleItem(Metadata metadata, Item item)
        {
            if (item.key == "name")
            {
                metadata.overrideName = item.value;
            }

            if (item.key == "namespace")
            {
                metadata.namespaceName = item.value;
            }

            if (item.key == "layout")
            {
                if (item.value == "horizontal")
                {
                    metadata.sheetLayout = SheetLayout.Horizontal;
                }
                else if (item.value == "vertical")
                {
                    metadata.sheetLayout = SheetLayout.Vertical;
                }
                else
                {
                    throw new InvalidDataException($"Unknown layout value: {item.value}");
                }
            }

            if (item.key == "type")
            {
                if (item.value == "data-table")
                {
                    metadata.sheetType = SheetType.DataTable;
                }
                else if (item.value == "setting")
                {
                    metadata.sheetType = SheetType.Setting;
                }
                else
                {
                    throw new InvalidDataException($"Unknown type value: {item.value}");
                }
            }

            if (item.key == "export")
            {
                metadata.export = item.value == "false";
            }
        }
    }
}
