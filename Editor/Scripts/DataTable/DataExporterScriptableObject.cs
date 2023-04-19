using System;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class DataExporterScriptableObject
    {
        private static Type GetTableType(TypeData typeData)
        {
            Type tableType = ReflectionUtility.FindType((Type type) =>
            {
                return type.Namespace == typeData.namespaceName &&
                    type.Name == typeData.name + "Table" &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(DataTable<>);
            });
            return tableType;
        }

        public ScriptableObject Export(TypeData typeData, SheetRawData sheetRawData)
        {
            Type tableType = GetTableType(typeData);
            var dataTable = ScriptableObject.CreateInstance(tableType);

            var serializedObject = new SerializedObject(dataTable);
            SerializedProperty property = serializedObject.FindProperty("items");

            property.arraySize = sheetRawData.RowCount - Const.DataTableStartRow;
            for (var i = Const.DataTableStartRow; i < sheetRawData.RowCount; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i - Const.DataTableStartRow);
                ExportDataAsItem(element, typeData, sheetRawData.GetRow(i));
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
            return dataTable;
        }

        private void ExportDataAsItem(SerializedProperty element, TypeData typeData, string[] dataRow)
        {
            var columnIndex = 0;
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];

                // If the field is system type, write it directly
                if (fieldData.IsSystemType)
                {
                    WritePropertyForSystemType(element, fieldData, dataRow[columnIndex]);
                    columnIndex++;
                    continue;
                }

                // If the field is custom type, write it recursively
                columnIndex = WritePropertyForCustomType(element, fieldData, dataRow, columnIndex);
            }
        }

        private void WritePropertyForSystemType(SerializedProperty element, FieldData fieldData, string dataText)
        {
            SerializedProperty fieldProperty = element.FindPropertyRelative(fieldData.NameForField);
            if (fieldProperty == null)
            {
                Debug.LogError($"Field '{fieldData.NameForField}' not found!");
                return;
            }

            if (fieldData.IsArray)
            {
                string[] splits = dataText.Split(',');

                fieldProperty.arraySize = splits.Length;
                for (var j = 0; j < splits.Length; j++)
                {
                    SerializedProperty item = fieldProperty.GetArrayElementAtIndex(j);
                    WriteValueForSystemType(item, fieldData.BaseTypeName, splits[j]);
                }
            }
            else
            {
                string text = dataText;
                WriteValueForSystemType(fieldProperty, fieldData.BaseTypeName, text);
            }
        }

        private void WriteValueForSystemType(SerializedProperty fieldProperty, string typeName, string text)
        {
            switch (typeName)
            {
                default:
                    break;
                case "string":
                    {
                        fieldProperty.stringValue = text;
                        break;
                    }
                case "int":
                    {
                        bool success = int.TryParse(text, out int result);
                        fieldProperty.intValue = success ? result : default;
                        break;
                    }
                case "long":
                    {
                        bool success = long.TryParse(text, out long result);
                        fieldProperty.longValue = success ? result : default;
                        break;
                    }
                case "bool":
                    {
                        bool success = bool.TryParse(text, out bool result);
                        fieldProperty.boolValue = success ? result : default;
                        break;
                    }
            }
        }

        private int WritePropertyForCustomType(SerializedProperty element, FieldData fieldData, string[] dataRow, int columnIndex)
        {
            SerializedProperty fieldProperty = element.FindPropertyRelative(fieldData.NameForField);
            if (fieldProperty == null)
            {
                Debug.LogError($"Field '{fieldData.NameForField}' not found!");
                return columnIndex;
            }

            SerializedProperty writtenProperty;

            var iterator = new TypeFieldIterator(fieldData);
            if (fieldData.IsArray)
            {
                fieldProperty.arraySize = 1;
                writtenProperty = fieldProperty.GetArrayElementAtIndex(0);
            }
            else
            {
                writtenProperty = fieldProperty;
            }

            while (true)
            {
                FieldData currentMemberFieldData = iterator.CurrentMember;
                if (currentMemberFieldData.IsSystemType)
                {
                    WritePropertyForSystemType(writtenProperty, currentMemberFieldData, dataRow[columnIndex]);
                    columnIndex++;
                }
                else
                {
                    columnIndex = WritePropertyForCustomType(writtenProperty, currentMemberFieldData, dataRow, columnIndex);
                }

                // Increase array index if not finished
                if (!iterator.IsAtLastMember())
                {
                    iterator.IncreaseMemberIndex();
                }
                else if (iterator.IsArray && iterator.ArrayIndex < iterator.FieldData.arraySize - 1)
                {
                    iterator.IncreaseArrayIndex();
                    fieldProperty.arraySize++;
                    writtenProperty = fieldProperty.GetArrayElementAtIndex(fieldProperty.arraySize - 1);
                }
                else
                {
                    break;
                }
            }

            return columnIndex;
        }
    }
}
