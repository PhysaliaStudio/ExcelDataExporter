using System;
using Physalia.ExcelDataExporter.Loader;
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

            string[] dataLine = metadata.GetSheetLayout() == SheetLayout.Horizontal ?
                sheetRawData.GetRow(Const.SettingTableStartLine) :
                sheetRawData.GetColumn(Const.SettingTableStartLine);
            ExportDataAsItem(serializedObject, typeData, dataLine);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
            return setting;
        }
    }
}
