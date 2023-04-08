using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class CustomTypeTable
    {
        private readonly Dictionary<string, ClassData> typeTable = new();

        public int Count => typeTable.Count;
        public IEnumerable<ClassData> CustomTypes => typeTable.Values;

        public static CustomTypeTable Parse(SheetRawData sheetRawData)
        {
            var table = new CustomTypeTable();

            var readyForTypeRow = false;
            ClassData currentClassData = null;

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
                    currentClassData = new ClassData { name = typeName };
                    for (var j = 1; j < row.Length; j++)
                    {
                        string fieldName = row[j];
                        if (string.IsNullOrWhiteSpace(fieldName))
                        {
                            break;
                        }

                        var fieldData = new FieldData { name = fieldName };
                        currentClassData.fieldDatas.Add(fieldData);
                    }

                    readyForTypeRow = true;
                }
                else
                {
                    var failed = false;
                    for (var j = 0; j < currentClassData.fieldDatas.Count; j++)
                    {
                        string typeName = row[j + 1];
                        if (string.IsNullOrWhiteSpace(typeName))
                        {
                            Debug.LogError($"Parse type failed! Invalid format. Type: {currentClassData.name}");
                            failed = true;
                            break;
                        }
                        currentClassData.fieldDatas[j].typeName = typeName;
                    }

                    if (!failed)
                    {
                        table.typeTable.Add(currentClassData.name, currentClassData);
                    }

                    currentClassData = null;
                    readyForTypeRow = false;
                }
            }

            return table;
        }

        public ClassData GetClassData(string typeName)
        {
            bool success = typeTable.TryGetValue(typeName, out ClassData classData);
            if (success)
            {
                return classData;
            }

            return null;
        }
    }
}
