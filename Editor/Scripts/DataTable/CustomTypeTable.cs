using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class CustomTypeTable
    {
        private readonly Dictionary<string, TypeData> typeTable = new();

        public int Count => typeTable.Count;
        public IEnumerable<TypeData> CustomTypes => typeTable.Values;

        public static CustomTypeTable Parse(SheetRawData sheetRawData)
        {
            var table = new CustomTypeTable();

            var readyForTypeRow = false;
            TypeData currentTypeData = null;
            string metadata = sheetRawData.Get(Const.SheetMetaRow, Const.SheetMetaColumn);

            for (var i = Const.SheetMetaRow + 1; i < sheetRawData.RowCount; i++)
            {
                string[] row = sheetRawData.GetRow(i);
                if (!readyForTypeRow)
                {
                    currentTypeData = ParseNameRow(row);
                    if (currentTypeData == null)
                    {
                        continue;
                    }

                    readyForTypeRow = true;
                }
                else
                {
                    bool success;
                    if (currentTypeData.define != TypeData.Define.Enum)
                    {
                        success = ParseTypeRow(currentTypeData, row);
                    }
                    else
                    {
                        success = ParseEnumValueRow(currentTypeData, row);
                    }

                    if (success)
                    {
                        currentTypeData.ParseMetadata(metadata);
                        table.typeTable.Add(currentTypeData.name, currentTypeData);
                    }

                    currentTypeData = null;
                    readyForTypeRow = false;
                }
            }

            return table;
        }

        private static TypeData ParseNameRow(string[] row)
        {
            if (string.IsNullOrWhiteSpace(row[0]))
            {
                return null;
            }

            TypeData typeData = StartTypeData(row[0]);
            if (typeData == null)  // Encounter invalid data
            {
                return null;
            }

            for (var i = 1; i < row.Length; i++)
            {
                string fieldName = row[i];
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    break;
                }

                var fieldData = new FieldData { name = fieldName };
                typeData.fieldDatas.Add(fieldData);
            }

            return typeData;
        }

        private static TypeData StartTypeData(string cell)
        {
            string[] splits = cell.Split(' ');
            if (splits.Length != 2)
            {
                return null;
            }

            string typeDefine = splits[0];
            string typeName = splits[1];
            switch (typeDefine)
            {
                default:
                    return null;
                case "class":
                    return new TypeData { name = typeName, define = TypeData.Define.Class };
                case "struct":
                    return new TypeData { name = typeName, define = TypeData.Define.Struct };
                case "enum":
                    return new TypeData { name = typeName, define = TypeData.Define.Enum };
            }
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
