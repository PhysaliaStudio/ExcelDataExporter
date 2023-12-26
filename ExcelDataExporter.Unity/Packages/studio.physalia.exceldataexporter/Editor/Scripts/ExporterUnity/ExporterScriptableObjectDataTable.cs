using System;
using Physalia.ExcelDataExporter.Loader;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class ExporterScriptableObjectDataTable : ExporterScriptableObject
    {
        public static ScriptableObject Export(TypeData typeData, SheetRawData sheetRawData)
        {
            Type tableType = typeData.GetTableType();
            ScriptableObject dataTable = ScriptableObject.CreateInstance(tableType);
            var serializedObject = new SerializedObject(dataTable);

            SerializedProperty property = serializedObject.FindProperty("items");

            Metadata metadata = sheetRawData.Metadata;
            if (metadata.GetSheetLayout() == SheetLayout.Horizontal)
            {
                property.arraySize = sheetRawData.RowCount - Const.DataTableStartLine;
                for (var i = Const.DataTableStartLine; i < sheetRawData.RowCount; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i - Const.DataTableStartLine);
                    ExportDataAsItem(element, typeData, sheetRawData.GetRow(i));
                }
            }
            else
            {
                property.arraySize = sheetRawData.ColumnCount - Const.DataTableStartLine;
                for (var i = Const.DataTableStartLine; i < sheetRawData.ColumnCount; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i - Const.DataTableStartLine);
                    ExportDataAsItem(element, typeData, sheetRawData.GetColumn(i));
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
            return dataTable;
        }
    }
}
