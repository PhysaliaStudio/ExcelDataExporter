using System;

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

        public static Type GetTableType(this TypeData typeData)
        {
            Type tableType = ReflectionUtility.FindType((Type type) =>
            {
                return type.Namespace == typeData.namespaceName &&
                    type.Name == typeData.name + "Table" &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(DataTable<>);
            });
            return tableType;
        }
    }
}
