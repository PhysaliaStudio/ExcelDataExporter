using System;
using System.IO;
using Physalia.ExcelDataExporter.Loader;
using UnityEditor;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataExporterUnity : Exporter
    {
        private readonly CodeGeneratorAssetData _codeGeneratorAssetData = new();
        private readonly CodeGeneratorAssetDataTable _codeGeneratorAssetDataTable = new();
        private readonly CodeGeneratorAssetSettingTable _codeGeneratorAssetSettingTable = new();

        protected override void GenerateCodeForDataTable(TypeData typeData, SheetRawData sheetRawData)
        {
            {
                string scriptText = _codeGeneratorAssetData.Generate(typeData);
                string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
                string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}.cs");
                CreateScriptFile(fullPath, scriptText);
            }

            {
                string scriptText = _codeGeneratorAssetDataTable.Generate(typeData);
                string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
                string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}Table.cs");
                CreateScriptFile(fullPath, scriptText);
            }
        }

        protected override void GenerateCodeForSettingTable(TypeData typeData, SheetRawData sheetRawData)
        {
            string scriptText = _codeGeneratorAssetSettingTable.Generate(typeData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}.cs");
            CreateScriptFile(fullPath, scriptText);
        }

        protected override void ExportDataForDataTable(TypeData typeData, SheetRawData sheetRawData)
        {
            ScriptableObject scriptableObject = ExporterScriptableObjectDataTable.Export(typeData, sheetRawData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportDataPath($"{relativeDirectory}/{sheetRawData.Name}Table.asset");
            string assetPath = Path.GetRelativePath(baseDirectory, fullPath).Replace('\\', '/');

            // Save asset
            Type tableType = typeData.GetTableType();
            UnityEngine.Object @object = AssetDatabase.LoadAssetAtPath(assetPath, tableType);
            if (@object != null)
            {
                scriptableObject.name = @object.name;  // Unity Rule: The name is need to be same as the original one
                EditorUtility.CopySerialized(scriptableObject, @object);
                EditorUtility.SetDirty(@object);
            }
            else
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                AssetDatabase.CreateAsset(scriptableObject, assetPath);
            }
        }

        protected override void ExportDataForSettingTable(TypeData typeData, SheetRawData sheetRawData)
        {
            ScriptableObject scriptableObject = ExporterScriptableObjectSetting.Export(typeData, sheetRawData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportDataPath($"{relativeDirectory}/{sheetRawData.Name}.asset");
            string assetPath = Path.GetRelativePath(baseDirectory, fullPath).Replace('\\', '/');

            // Save asset
            Type dataType = typeData.GetDataType();
            UnityEngine.Object @object = AssetDatabase.LoadAssetAtPath(assetPath, dataType);
            if (@object != null)
            {
                scriptableObject.name = @object.name;  // Unity Rule: The name is need to be same as the original one
                EditorUtility.CopySerialized(scriptableObject, @object);
                EditorUtility.SetDirty(@object);
            }
            else
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                AssetDatabase.CreateAsset(scriptableObject, assetPath);
            }
        }

        private static string AssetPathToFullPath(string assetPath)
        {
            string relativePath = Path.GetRelativePath("Assets", assetPath);
            string fullPath = Path.GetFullPath(relativePath, Application.dataPath);
            fullPath = fullPath.Replace('\\', '/');
            return fullPath;
        }

        private static string FullPathToAssetPath(string fullPath)
        {
            string relativePath = Path.GetRelativePath(Application.dataPath, fullPath);
            string assetPath = Path.GetFullPath(relativePath, "Assets");
            assetPath = assetPath.Replace('\\', '/');
            return assetPath;
        }
    }
}
