using System.Collections.Generic;
using System.IO;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataExporterDotNet : Exporter
    {
        private readonly CodeGeneratorCSharpData _codeGeneratorJsonData = new CodeGeneratorCSharpData();
        private readonly CodeGeneratorCSharpDataTable _codeGeneratorJsonDataTable = new CodeGeneratorCSharpDataTable();
        private readonly CodeGeneratorCSharpSetting _codeGeneratorJsonSetting = new CodeGeneratorCSharpSetting();

        protected override List<TypeData> GetAdditionalTypesForDefault()
        {
            const string NamespaceName = "Physalia.ExcelDataRuntime";

            return new List<TypeData> {
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Vector2",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "y", typeData = TypeUtility.TypeDataFloat },
                    }
                },
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Vector3",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "y", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "z", typeData = TypeUtility.TypeDataFloat },
                    }
                },
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Vector4",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "y", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "z", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "w", typeData = TypeUtility.TypeDataFloat },
                    }
                },
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Vector2Int",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeUtility.TypeDataInt },
                        new FieldData { name = "y", typeData = TypeUtility.TypeDataInt },
                    }
                },
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Vector3Int",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "x", typeData = TypeUtility.TypeDataInt },
                        new FieldData { name = "y", typeData = TypeUtility.TypeDataInt },
                        new FieldData { name = "z", typeData = TypeUtility.TypeDataInt },
                    }
                },
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Color",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "r", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "g", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "b", typeData = TypeUtility.TypeDataFloat },
                        new FieldData { name = "a", typeData = TypeUtility.TypeDataFloat },
                    }
                },
                new TypeData
                {
                    namespaceName = NamespaceName,
                    name = "Color32",
                    fieldDatas = new List<FieldData>
                    {
                        new FieldData { name = "r", typeData = TypeUtility.TypeDataInt },
                        new FieldData { name = "g", typeData = TypeUtility.TypeDataInt },
                        new FieldData { name = "b", typeData = TypeUtility.TypeDataInt },
                        new FieldData { name = "a", typeData = TypeUtility.TypeDataInt },
                    }
                },
            };
        }

        protected override void GenerateCodeForDataTable(TypeData typeData, SheetRawData sheetRawData)
        {
            {
                string scriptText = _codeGeneratorJsonData.Generate(typeData);
                string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
                string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}.cs");
                CreateScriptFile(fullPath, scriptText);
            }

            {
                string scriptText = _codeGeneratorJsonDataTable.Generate(typeData);
                string relativeDirectory = Path.GetRelativePath(sourceDataDirectory, sheetRawData.ParentDirectory);
                string fullPath = GetExportScriptPath($"{relativeDirectory}/{sheetRawData.Name}Table.cs");
                CreateScriptFile(fullPath, scriptText);
            }
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
