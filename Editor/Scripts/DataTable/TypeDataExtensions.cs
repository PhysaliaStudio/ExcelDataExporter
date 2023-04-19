namespace Physalia.ExcelDataExporter
{
    public static class TypeDataExtensions
    {
        public static void ParseMetadata(this TypeData typeData, string metadata)
        {
            string[] lines = metadata.Replace("\r\n", "\n").Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.StartsWith("namespace="))
                {
                    typeData.namespaceName = line["namespace=".Length..];
                }
            }
        }
    }
}
