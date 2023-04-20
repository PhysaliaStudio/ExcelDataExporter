using System;
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
    }
}
