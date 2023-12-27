using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public static class TypeUtility
    {
        public static readonly TypeData TypeDataString = new TypeData { name = "string" };
        public static readonly TypeData TypeDataBool = new TypeData { name = "bool" };
        public static readonly TypeData TypeDataByte = new TypeData { name = "byte" };
        public static readonly TypeData TypeDataSByte = new TypeData { name = "sbyte" };
        public static readonly TypeData TypeDataShort = new TypeData { name = "short" };
        public static readonly TypeData TypeDataUShort = new TypeData { name = "ushort" };
        public static readonly TypeData TypeDataInt = new TypeData { name = "int" };
        public static readonly TypeData TypeDataUInt = new TypeData { name = "uint" };
        public static readonly TypeData TypeDataLong = new TypeData { name = "long" };
        public static readonly TypeData TypeDataULong = new TypeData { name = "ulong" };
        public static readonly TypeData TypeDataFloat = new TypeData { name = "float" };
        public static readonly TypeData TypeDataDouble = new TypeData { name = "double" };
        public static readonly TypeData TypeDataDecimal = new TypeData { name = "decimal" };

        private static readonly Dictionary<string, TypeData> DefaultTypeTable = new Dictionary<string, TypeData>
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

        public static object ParseValueToSystemType(string typeName, string text)
        {
            switch (typeName)
            {
                default:
                    return null;
                case "string":
                    return text;
                case "bool":
                    return ParseBool(text);
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
    }
}
