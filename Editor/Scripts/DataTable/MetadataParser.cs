namespace Physalia.ExcelDataExporter
{
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
            }

            return metadata;
        }

        private string namespaceName;

        public string NamespaceName => namespaceName;

        private Metadata() { }
    }
}
