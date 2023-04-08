using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public static class TypeUtility
    {
        private static readonly Dictionary<string, TypeData> DefaultTypeTable = new()
        {
            { "string", new TypeData { name = "string" } },
            { "bool", new TypeData { name = "bool" } },
            { "byte", new TypeData { name = "byte" } },
            { "sbyte", new TypeData { name = "sbyte" } },
            { "short", new TypeData { name = "short" } },
            { "ushort", new TypeData { name = "ushort" } },
            { "int", new TypeData { name = "int"} },
            { "uint", new TypeData { name = "uint"} },
            { "long", new TypeData { name = "long" } },
            { "ulong", new TypeData { name = "ulong" } },
            { "float", new TypeData { name = "float" } },
            { "double", new TypeData { name = "double" } },
            { "decimal", new TypeData { name = "decimal" } },

            { "Vector2Int", new TypeData
                {
                    name = "Vector2Int",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeName = "int" },
                        new FieldData { name = "y", typeName = "int" },
                    }
                }
            },
            { "Vector3Int", new TypeData
                {
                    name = "Vector3Int",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeName = "int" },
                        new FieldData { name = "y", typeName = "int" },
                        new FieldData { name = "z", typeName = "int" },
                    }
                }
            },
        };

        public static bool IsDefaultType(string typeName)
        {
            return DefaultTypeTable.ContainsKey(typeName);
        }

        public static TypeData GetDefaultType(string typeName)
        {
            bool success = DefaultTypeTable.TryGetValue(typeName, out TypeData typeData);
            if (success)
            {
                return typeData;
            }

            return null;
        }

        public static bool IsSystemType(string typeName)
        {
            switch (typeName)
            {
                default:
                    return false;
                case "string":
                case "bool":
                case "byte":
                case "sbyte":
                case "short":
                case "ushort":
                case "int":
                case "uint":
                case "long":
                case "ulong":
                case "float":
                case "double":
                case "decimal":
                    return true;
            }
        }

        public static object ParseValueToSystemType(string typeName, string text)
        {
            switch (typeName)
            {
                default:
                    return null;
                case "string":
                    return text;
                case "bool":
                    {
                        bool success = bool.TryParse(text, out bool result);
                        return success ? result : default;
                    }
                case "byte":
                    {
                        bool success = byte.TryParse(text, out byte result);
                        return success ? result : default;
                    }
                case "sbyte":
                    {
                        bool success = sbyte.TryParse(text, out sbyte result);
                        return success ? result : default;
                    }
                case "short":
                    {
                        bool success = short.TryParse(text, out short result);
                        return success ? result : default;
                    }
                case "ushort":
                    {
                        bool success = ushort.TryParse(text, out ushort result);
                        return success ? result : default;
                    }
                case "int":
                    {
                        bool success = int.TryParse(text, out int result);
                        return success ? result : default;
                    }
                case "uint":
                    {
                        bool success = uint.TryParse(text, out uint result);
                        return success ? result : default;
                    }
                case "long":
                    {
                        bool success = long.TryParse(text, out long result);
                        return success ? result : default;
                    }
                case "ulong":
                    {
                        bool success = ulong.TryParse(text, out ulong result);
                        return success ? result : default;
                    }
                case "float":
                    {
                        bool success = float.TryParse(text, out float result);
                        return success ? result : default;
                    }
                case "double":
                    {
                        bool success = double.TryParse(text, out double result);
                        return success ? result : default;
                    }
                case "decimal":
                    {
                        bool success = decimal.TryParse(text, out decimal result);
                        return success ? result : default;
                    }
            }
        }
    }
}
