using System.IO;

namespace Physalia.ExcelDataExporter
{
    public enum SheetLayout { Horizontal, Vertical }
    public enum SheetType { DataTable, Setting }

    public class Metadata
    {
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
            var metadata = new Metadata();

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.StartsWith("export="))
                {
                    metadata.export = line["export=".Length..] != "false";
                }

                if (line.StartsWith("name="))
                {
                    metadata.overrideName = line["name=".Length..];
                }

                if (line.StartsWith("namespace="))
                {
                    metadata.namespaceName = line["namespace=".Length..];
                }

                if (line.StartsWith("layout="))
                {
                    string value = line["layout=".Length..];
                    if (value == "horizontal")
                    {
                        metadata.sheetLayout = SheetLayout.Horizontal;
                    }
                    else if (value == "vertical")
                    {
                        metadata.sheetLayout = SheetLayout.Vertical;
                    }
                    else
                    {
                        throw new InvalidDataException($"Unknown layout value: {value}");
                    }
                }

                if (line.StartsWith("type="))
                {
                    string value = line["type=".Length..];
                    if (value == "data-table")
                    {
                        metadata.sheetType = SheetType.DataTable;
                    }
                    else if (value == "setting")
                    {
                        metadata.sheetType = SheetType.Setting;
                    }
                    else
                    {
                        throw new InvalidDataException($"Unknown type value: {value}");
                    }
                }
            }

            return metadata;
        }
    }
}
