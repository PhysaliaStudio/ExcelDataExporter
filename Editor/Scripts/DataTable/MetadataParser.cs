using System.IO;

namespace Physalia.ExcelDataExporter
{
    public enum SheetLayout { Horizontal, Vertical }

    public class Metadata
    {
        public static Metadata Parse(string text)
        {
            var metadata = new Metadata();

            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
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
            }

            return metadata;
        }

        private string namespaceName;
        private SheetLayout sheetLayout;

        public string NamespaceName => namespaceName;
        public SheetLayout SheetLayout => sheetLayout;

        private Metadata() { }
    }
}
