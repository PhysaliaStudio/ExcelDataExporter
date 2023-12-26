using System.IO;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataExporterDotNet : Exporter
    {
        private readonly CodeGeneratorCSharpData _codeGeneratorJsonData = new CodeGeneratorCSharpData();
        private readonly CodeGeneratorCSharpSetting _codeGeneratorJsonSetting = new CodeGeneratorCSharpSetting();

        protected override void GenerateCodeForDataTable(TypeData typeData, SheetRawData sheetRawData)
        {
            string scriptText = _codeGeneratorJsonData.Generate(typeData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}.cs");
            CreateScriptFile(fullPath, scriptText);
        }

        protected override void GenerateCodeForSettingTable(TypeData typeData, SheetRawData sheetRawData)
        {
            string scriptText = _codeGeneratorJsonSetting.Generate(typeData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}.cs");
            CreateScriptFile(fullPath, scriptText);
        }

        protected override void ExportDataForDataTable(TypeData typeData, SheetRawData sheetRawData)
        {
            string json = ExporterJson.Export(typeData, sheetRawData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportDataPath($"{relativeDirectory}/{sheetRawData.Name}.json");
            SaveFile(fullPath, json);
        }

        protected override void ExportDataForSettingTable(TypeData typeData, SheetRawData sheetRawData)
        {
            string json = ExporterJson.Export(typeData, sheetRawData);
            string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
            string fullPath = GetExportDataPath($"{relativeDirectory}/{sheetRawData.Name}.json");
            SaveFile(fullPath, json);
        }
    }
}
