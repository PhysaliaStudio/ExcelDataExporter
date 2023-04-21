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
            if (metadata.SheetLayout == SheetLayout.Horizontal)
            {
                ExportDataAsItem(serializedObject, typeData, sheetRawData.GetRow(Const.DataTableStartRow));
            }
            else
            {
                ExportDataAsItem(serializedObject, typeData, sheetRawData.GetColumn(Const.DataTableStartColumn));
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            serializedObject.UpdateIfRequiredOrScript();
            return setting;
        }
    }
}
