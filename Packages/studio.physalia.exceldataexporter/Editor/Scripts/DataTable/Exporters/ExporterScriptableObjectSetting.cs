using System;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class ExporterScriptableObjectSetting : ExporterScriptableObject
    {
        public static ScriptableObject Export(TypeData typeData, SheetRawData sheetRawData)
        {
            Type dataType = typeData.GetDataType();
            ScriptableObject setting = ScriptableObject.CreateInstance(dataType);
            var serializedObject = new SerializedObject(setting);

            Metadata metadata = sheetRawData.Metadata;

            string[] dataLine = metadata.SheetLayout == SheetLayout.Horizontal ?
                sheetRawData.GetRow(Const.DataTableStartLine) :
                sheetRawData.GetColumn(Const.DataTableStartLine);
            ExportDataAsItem(serializedObject, typeData, dataLine);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
            return setting;
        }
    }
}
