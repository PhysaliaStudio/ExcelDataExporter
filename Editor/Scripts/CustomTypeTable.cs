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

            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                string[] row = sheetRawData.GetRow(i);
                if (!readyForTypeRow)
                {
                    if (string.IsNullOrWhiteSpace(row[0]))
                    {
                        continue;
                    }

                    string typeName = row[0];
                    currentTypeData = new TypeData { name = typeName };
                    for (var j = 1; j < row.Length; j++)
                    {
                        string fieldName = row[j];
                        if (string.IsNullOrWhiteSpace(fieldName))
                        {
                            break;
                        }

                        var fieldData = new FieldData { name = fieldName };
                        currentTypeData.fieldDatas.Add(fieldData);
                    }

                    readyForTypeRow = true;
                }
                else
                {
                    var failed = false;
                    for (var j = 0; j < currentTypeData.fieldDatas.Count; j++)
                    {
                        string typeName = row[j + 1];
                        if (string.IsNullOrWhiteSpace(typeName))
                        {
                            Debug.LogError($"Parse type failed! Invalid format. Type: {currentTypeData.name}");
                            failed = true;
                            break;
                        }

                        currentTypeData.fieldDatas[j].typeData = TypeUtility.GetDefaultType(typeName);
                    }

                    if (!failed)
                    {
                        table.typeTable.Add(currentTypeData.name, currentTypeData);
                    }

                    currentTypeData = null;
                    readyForTypeRow = false;
                }
            }

            return table;
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
