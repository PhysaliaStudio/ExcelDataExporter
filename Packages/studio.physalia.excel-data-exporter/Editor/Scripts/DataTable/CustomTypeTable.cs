using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class CustomTypeTable
    {
        private readonly Dictionary<string, TypeData> typeTable = new();

        public int Count => typeTable.Count;
        public IEnumerable<TypeData> CustomTypes => typeTable.Values;

        public static CustomTypeTable Parse(params SheetRawData[] sheetRawDatas)
        {
            var table = new CustomTypeTable();
            for (var i = 0; i < sheetRawDatas.Length; i++)
            {
                ParseSheet(table, sheetRawDatas[i]);
            }

            return table;
        }

        private static void ParseSheet(CustomTypeTable table, SheetRawData sheetRawData)
        {
            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                bool success = TryParseType(sheetRawData, i, out TypeData typeData);
                if (success)
                {
                    Metadata metadata = sheetRawData.Metadata;
                    typeData.namespaceName = metadata.NamespaceName;
                    table.typeTable.Add(typeData.name, typeData);
                    i += 2;
                }
            }
        }

        private static bool TryParseType(SheetRawData sheetRawData, int startIndex, out TypeData typeData)
        {
            typeData = null;

            string[] startRow = sheetRawData.GetRow(startIndex);
            if (string.IsNullOrWhiteSpace(startRow[0]))
            {
                return false;
            }

            TypeData typeDataToContinued = StartTypeData(sheetRawData, startIndex);
            if (typeDataToContinued == null)  // Encounter invalid data
            {
                return false;
            }

            if (!ParseCommentRow(typeDataToContinued, sheetRawData.GetRow(startIndex)))
            {
                return false;
            }

            if (!ParseNameRow(typeDataToContinued, sheetRawData.GetRow(startIndex + 1)))
            {
                return false;
            }

            if (typeDataToContinued.define != TypeData.Define.Enum)
            {
                if (!ParseTypeRow(typeDataToContinued, sheetRawData.GetRow(startIndex + 2)))
                {
                    return false;
                }
            }
            else
            {
                if (!ParseEnumValueRow(typeDataToContinued, sheetRawData.GetRow(startIndex + 2)))
                {
                    return false;
                }
            }

            typeData = typeDataToContinued;
            return true;
        }

        private static TypeData StartTypeData(SheetRawData sheetRawData, int startIndex)
        {
            // If row count is not enough, then it must failed.
            if (startIndex + 2 >= sheetRawData.RowCount)
            {
                return null;
            }

            string[] startRow = sheetRawData.GetRow(startIndex);
            string startCell = startRow[0];
            string[] splits = startCell.Split(' ');
            if (splits.Length != 2)
            {
                return null;
            }

            // Type define and type name
            TypeData typeData;
            string typeDefine = splits[0];
            string typeName = splits[1];
            switch (typeDefine)
            {
                default:
                    return null;
                case "class":
                    typeData = new TypeData { name = typeName, define = TypeData.Define.Class };
                    break;
                case "struct":
                    typeData = new TypeData { name = typeName, define = TypeData.Define.Struct };
                    break;
                case "enum":
                    typeData = new TypeData { name = typeName, define = TypeData.Define.Enum };
                    break;
            }

            // Field count
            string[] nameRow = sheetRawData.GetRow(startIndex + 1);
            for (var i = 1; i < nameRow.Length; i++)
            {
                string fieldName = nameRow[i];
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    break;
                }

                typeData.fieldDatas.Add(new FieldData());
            }

            return typeData;
        }

        private static bool ParseCommentRow(TypeData typeDataToContinued, string[] row)
        {
            for (var i = 0; i < typeDataToContinued.fieldDatas.Count; i++)
            {
                string comment = row[i + 1];
                typeDataToContinued.fieldDatas[i].comment = comment;
            }

            return true;
        }

        private static bool ParseNameRow(TypeData typeDataToContinued, string[] row)
        {
            for (var i = 0; i < typeDataToContinued.fieldDatas.Count; i++)
            {
                string name = row[i + 1];
                typeDataToContinued.fieldDatas[i].name = name;
            }

            return true;
        }

        private static bool ParseTypeRow(TypeData typeDataToContinued, string[] row)
        {
            for (var i = 0; i < typeDataToContinued.fieldDatas.Count; i++)
            {
                string typeName = row[i + 1];
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    Debug.LogError($"Parse type failed! Invalid format. Type: {typeDataToContinued.name}");
                    return false;
                }

                typeDataToContinued.fieldDatas[i].typeData = TypeUtility.GetDefaultType(typeName);
            }

            return true;
        }

        private static bool ParseEnumValueRow(TypeData typeDataToContinued, string[] row)
        {
            for (var i = 0; i < typeDataToContinued.fieldDatas.Count; i++)
            {
                string value = row[i + 1];
                if (string.IsNullOrWhiteSpace(value))
                {
                    Debug.LogError($"Parse enum failed! Empty value. Type: {typeDataToContinued.name}");
                    return false;
                }

                bool success = int.TryParse(value, out int enumValue);
                if (!success)
                {
                    Debug.LogError($"Parse enum failed! Not int value. Type: {typeDataToContinued.name}");
                    return false;
                }

                typeDataToContinued.fieldDatas[i].enumValue = enumValue;
            }

            return true;
        }

        public TypeData GetTypeData(string typeName)
        {
            bool success = typeTable.TryGetValue(typeName, out TypeData typeData);
            if (success)
            {
                return typeData;
            }

            typeData = TypeUtility.GetDefaultType(typeName);
            if (typeData != null)
            {
                return typeData;
            }

            return null;
        }
    }
}
