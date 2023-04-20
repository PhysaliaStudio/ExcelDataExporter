using System;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class DataExporterScriptableObject
    {
        public ScriptableObject Export(TypeData typeData, SheetRawData sheetRawData)
        {
            Type tableType = typeData.GetTableType();
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
                if (fieldData.IsSystemType)  // If the field is system type, write it directly
                {
                    WritePropertyForSystemType(element, fieldData, dataRow[columnIndex], true);
                    columnIndex++;
                }
                else  // If the field is custom type, write it recursively
                {
                    columnIndex = WritePropertyForCustomType(element, fieldData, dataRow, columnIndex, true);
                }
            }
        }

        private void WritePropertyForSystemType(SerializedProperty element, FieldData fieldData, string dataText, bool isRoot)
        {
            string propertyName = isRoot ? fieldData.NameForPrivateField : fieldData.NameForPublicField;
            SerializedProperty fieldProperty = element.FindPropertyRelative(propertyName);
            if (fieldProperty == null)
            {
                Debug.LogError($"Field '{propertyName}' not found!");
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

        private int WritePropertyForCustomType(SerializedProperty element, FieldData fieldData, string[] dataRow, int columnIndex, bool isRoot)
        {
            string propertyName = isRoot ? fieldData.NameForPrivateField : fieldData.NameForPublicField;
            SerializedProperty fieldProperty = element.FindPropertyRelative(propertyName);
            if (fieldProperty == null)
            {
                Debug.LogError($"Field '{propertyName}' not found!");
                return columnIndex;
            }

            var iterator = new TypeFieldIterator(fieldData);
            SerializedProperty writtenProperty = fieldProperty;

            while (true)
            {
                bool isCurrentCellApplicable = !fieldData.IsArray || dataRow[columnIndex] != Const.NotApplicable;

                if (iterator.IsAtFirstMember())
                {
                    if (isCurrentCellApplicable)
                    {
                        if (fieldData.IsArray)
                        {
                            fieldProperty.arraySize++;
                            writtenProperty = fieldProperty.GetArrayElementAtIndex(fieldProperty.arraySize - 1);
                        }
                    }
                }

                FieldData currentMemberFieldData = iterator.CurrentMember;
                if (currentMemberFieldData.IsSystemType)
                {
                    if (isCurrentCellApplicable)
                    {
                        WritePropertyForSystemType(writtenProperty, currentMemberFieldData, dataRow[columnIndex], false);
                    }
                    columnIndex++;
                }
                else
                {
                    if (isCurrentCellApplicable)
                    {
                        columnIndex = WritePropertyForCustomType(writtenProperty, currentMemberFieldData, dataRow, columnIndex, false);
                    }
                    else
                    {
                        columnIndex += currentMemberFieldData.EvaluateColumnCount();
                    }
                }

                // Increase array index if not finished
                if (!iterator.IsAtLastMember())
                {
                    iterator.IncreaseMemberIndex();
                }
                else if (iterator.IsArray && iterator.ArrayIndex < iterator.FieldData.arraySize - 1)
                {
                    iterator.IncreaseArrayIndex();
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
