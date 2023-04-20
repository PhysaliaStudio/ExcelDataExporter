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
            ScriptableObject dataTable = ScriptableObject.CreateInstance(tableType);
            var serializedObject = new SerializedObject(dataTable);

            string metadataText = sheetRawData.Get(Const.SheetMetaRow, Const.SheetMetaColumn);
            Metadata metadata = Metadata.Parse(metadataText);

            SerializedProperty property = serializedObject.FindProperty("items");
            if (metadata.SheetLayout == SheetLayout.Horizontal)
            {
                property.arraySize = sheetRawData.RowCount - Const.DataTableStartRow;
                for (var i = Const.DataTableStartRow; i < sheetRawData.RowCount; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i - Const.DataTableStartRow);
                    ExportDataAsItem(element, typeData, sheetRawData.GetRow(i));
                }
            }
            else
            {
                property.arraySize = sheetRawData.ColumnCount - Const.DataTableStartColumn;
                for (var i = Const.DataTableStartColumn; i < sheetRawData.ColumnCount; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i - Const.DataTableStartColumn);
                    ExportDataAsItem(element, typeData, sheetRawData.GetColumn(i)[1..]);
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
            return dataTable;
        }

        private void ExportDataAsItem(SerializedObject @object, TypeData typeData, string[] dataRow)
        {
            var columnIndex = 0;
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                if (fieldData.IsSystemType)  // If the field is system type, write it directly
                {
                    WriteForSystemType(@object, fieldData, dataRow[columnIndex], true);
                    columnIndex++;
                }
                else  // If the field is custom type, write it recursively
                {
                    columnIndex = WriteForCustomType(@object, fieldData, dataRow, columnIndex, true);
                }
            }
        }

        private void ExportDataAsItem(SerializedProperty property, TypeData typeData, string[] dataRow)
        {
            var columnIndex = 0;
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                if (fieldData.IsSystemType)  // If the field is system type, write it directly
                {
                    WriteForSystemType(property, fieldData, dataRow[columnIndex], true);
                    columnIndex++;
                }
                else  // If the field is custom type, write it recursively
                {
                    columnIndex = WriteForCustomType(property, fieldData, dataRow, columnIndex, true);
                }
            }
        }

        private void WriteForSystemType(SerializedObject @object, FieldData fieldData, string dataText, bool isRoot)
        {
            SerializedProperty fieldProperty = FindFieldProperty(@object, fieldData, isRoot);
            if (fieldProperty == null)
            {
                return;
            }

            WritePropertyForSystemType(fieldProperty, fieldData, dataText);
        }

        private void WriteForSystemType(SerializedProperty property, FieldData fieldData, string dataText, bool isRoot)
        {
            SerializedProperty fieldProperty = FindFieldProperty(property, fieldData, isRoot);
            if (fieldProperty == null)
            {
                return;
            }

            WritePropertyForSystemType(fieldProperty, fieldData, dataText);
        }

        private int WriteForCustomType(SerializedObject @object, FieldData fieldData, string[] dataRow, int columnIndex, bool isRoot)
        {
            SerializedProperty fieldProperty = FindFieldProperty(@object, fieldData, isRoot);
            if (fieldProperty == null)
            {
                return columnIndex;
            }

            return WritePropertyForCustomType(fieldProperty, fieldData, dataRow, columnIndex);
        }

        private int WriteForCustomType(SerializedProperty property, FieldData fieldData, string[] dataRow, int columnIndex, bool isRoot)
        {
            SerializedProperty fieldProperty = FindFieldProperty(property, fieldData, isRoot);
            if (fieldProperty == null)
            {
                return columnIndex;
            }

            return WritePropertyForCustomType(fieldProperty, fieldData, dataRow, columnIndex);
        }

        private static SerializedProperty FindFieldProperty(SerializedObject @object, FieldData fieldData, bool isRoot)
        {
            string propertyName = isRoot ? fieldData.NameForPrivateField : fieldData.NameForPublicField;
            SerializedProperty fieldProperty = @object.FindProperty(propertyName);
            if (fieldProperty == null)
            {
                Debug.LogError($"Field '{propertyName}' not found!");
            }

            return fieldProperty;
        }

        private static SerializedProperty FindFieldProperty(SerializedProperty property, FieldData fieldData, bool isRoot)
        {
            string propertyName = isRoot ? fieldData.NameForPrivateField : fieldData.NameForPublicField;
            SerializedProperty fieldProperty = property.FindPropertyRelative(propertyName);
            if (fieldProperty == null)
            {
                Debug.LogError($"Field '{propertyName}' not found!");
            }

            return fieldProperty;
        }

        private void WritePropertyForSystemType(SerializedProperty fieldProperty, FieldData fieldData, string dataText)
        {
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

        private int WritePropertyForCustomType(SerializedProperty fieldProperty, FieldData fieldData, string[] dataRow, int columnIndex)
        {
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
                        WriteForSystemType(writtenProperty, currentMemberFieldData, dataRow[columnIndex], false);
                    }
                    columnIndex++;
                }
                else
                {
                    if (isCurrentCellApplicable)
                    {
                        columnIndex = WriteForCustomType(writtenProperty, currentMemberFieldData, dataRow, columnIndex, false);
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
    }
}
