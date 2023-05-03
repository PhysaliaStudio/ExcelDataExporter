using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Physalia.ExcelDataExporter
{
    public static class TypeUtility
    {
        private static readonly TypeData TypeDataString = new() { name = "string" };
        private static readonly TypeData TypeDataBool = new() { name = "bool" };
        private static readonly TypeData TypeDataByte = new() { name = "byte" };
        private static readonly TypeData TypeDataSByte = new() { name = "sbyte" };
        private static readonly TypeData TypeDataShort = new() { name = "short" };
        private static readonly TypeData TypeDataUShort = new() { name = "ushort" };
        private static readonly TypeData TypeDataInt = new() { name = "int" };
        private static readonly TypeData TypeDataUInt = new() { name = "uint" };
        private static readonly TypeData TypeDataLong = new() { name = "long" };
        private static readonly TypeData TypeDataULong = new() { name = "ulong" };
        private static readonly TypeData TypeDataFloat = new() { name = "float" };
        private static readonly TypeData TypeDataDouble = new() { name = "double" };
        private static readonly TypeData TypeDataDecimal = new() { name = "decimal" };

        private static readonly Dictionary<string, TypeData> DefaultTypeTable = new()
        {
            { TypeDataString.name, TypeDataString },
            { TypeDataBool.name, TypeDataBool },
            { TypeDataByte.name, TypeDataByte },
            { TypeDataSByte.name, TypeDataSByte },
            { TypeDataShort.name, TypeDataShort },
            { TypeDataUShort.name, TypeDataUShort },
            { TypeDataInt.name, TypeDataInt },
            { TypeDataUInt.name, TypeDataUInt },
            { TypeDataLong.name, TypeDataLong },
            { TypeDataULong.name, TypeDataULong },
            { TypeDataFloat.name, TypeDataFloat },
            { TypeDataDouble.name, TypeDataDouble },
            { TypeDataDecimal.name, TypeDataDecimal },

            { "Vector2Int", new TypeData
                {
                    namespaceName = "UnityEngine",
                    name = "Vector2Int",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeDataInt },
                        new FieldData { name = "y", typeData = TypeDataInt },
                    }
                }
            },
            { "Vector3Int", new TypeData
                {
                    namespaceName = "UnityEngine",
                    name = "Vector3Int",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeDataInt },
                        new FieldData { name = "y", typeData = TypeDataInt },
                        new FieldData { name = "z", typeData = TypeDataInt },
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

        public static bool IsUnityType(string typeName)
        {
            switch (typeName)
            {
                default:
                    return false;
                case "Vector2":
                case "Vector3":
                case "Vector2Int":
                case "Vector3Int":
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

        public static int ParseInt(string text)
        {
            bool success = int.TryParse(text, out int result);
            return success ? result : default;
        }

        public static int ParseIntWithBinary(string text)
        {
            if (text.StartsWith("0b"))
            {
                text = text[2..];
            }
            return Convert.ToInt32(text, 2);
        }

        public static bool ParseBool(string text)
        {
            bool success = bool.TryParse(text, out bool result);
            if (success)
            {
                return result;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (text == "0")
            {
                return false;
            }

            return true;
        }

        public static int ParseFilterCell(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return -1;
            }

            MatchCollection matches = Regex.Matches(text, @"\d+");
            string valueText = matches[^1].Value;
            int filter = ParseIntWithBinary(valueText);
            return filter;
        }
    }
}
