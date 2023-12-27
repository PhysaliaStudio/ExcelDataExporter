using System.Text;
using Newtonsoft.Json;
using Physalia.ExcelDataExporter;

namespace ExcelDataConsole
{
    internal class Program
    {
        private static string _exeDirectory;

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify the command line arguments.");
                Console.WriteLine("    -gencode: Generate C# scripts");
                Console.WriteLine("    -export: Export to Json data");
                return;
            }

            SetupEnvironment();
            ExporterSetting exporterSetting = LoadExporterSetting();
            if (exporterSetting == null)
            {
                return;
            }

            Exporter exporter = CreateExporter(exporterSetting);

            string dataDirectory = Path.GetFullPath(exporter.sourceDataDirectory, exporter.baseDirectory);
            if (!Directory.Exists(dataDirectory))
            {
                Console.WriteLine($"Data directory not found: {dataDirectory}");
                return;
            }

            List<string> filePaths = CollectAllExcelPaths(dataDirectory);

            if (args.Contains("-gencode"))
            {
                GenerateCode(exporter, filePaths);
            }

            if (args.Contains("-export"))
            {
                ExportData(exporter, filePaths);
            }
        }

        private static void SetupEnvironment()
        {
            string exeFilePath = AppDomain.CurrentDomain.BaseDirectory;
            _exeDirectory = Path.GetDirectoryName(exeFilePath);

            // Note: Fix not supported encoding in .NET Core.
            // System.NotSupportedException: No data is available for encoding 1252.
            // Reference: https://github.com/ExcelDataReader/ExcelDataReader#important-note-on-net-core
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private static ExporterSetting LoadExporterSetting()
        {
            string settingPath = Path.Combine(_exeDirectory, "exporter-setting.json");
            if (!File.Exists(settingPath))
            {
                Console.WriteLine($"exporter-setting.json doesn't exist. Expected Path: {settingPath}");
                return null;
            }

            string json = File.ReadAllText(settingPath);
            ExporterSetting setting;
            try
            {
                setting = JsonConvert.DeserializeObject<ExporterSetting>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine($"Failed to parse exporter-setting.json as Json! Path: {settingPath}");
                return null;
            }

            if (!setting.IsValid())
            {
                Console.WriteLine($"Invalid data in exporter-setting! Path: {settingPath}");
                return null;
            }

            Console.WriteLine($"=====Exporter Setting=====");
            Console.WriteLine($"SourceDataDirectory: {setting.sourceDataDirectory}");
            Console.WriteLine($"ExportDataDirectory: {setting.exportDataDirectory}");
            Console.WriteLine($"ExportScriptDirectory: {setting.exportScriptDirectory}");
            Console.WriteLine($"Filters: {string.Join(", ", setting.filters)}");
            Console.WriteLine($"==========================");
            Console.WriteLine();

            return setting;
        }

        private static Exporter CreateExporter(ExporterSetting setting)
        {
            var exporter = new ExcelDataExporterDotNet
            {
                baseDirectory = _exeDirectory,
                sourceDataDirectory = setting.sourceDataDirectory,
                exportDataDirectory = setting.exportDataDirectory,
                exportScriptDirectory = setting.exportScriptDirectory,
                filters = setting.filters,
            };
            return exporter;
        }

        private static List<string> CollectAllExcelPaths(string dataDirectory)
        {
            var directory = new DirectoryInfo(dataDirectory);
            FileInfo[] files = directory.GetFiles("*.xlsx", SearchOption.AllDirectories);
            List<string> filePaths = new List<string>();
            for (var i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                // Skip if the file is a temp file
                if (file.Name.StartsWith("~"))
                {
                    continue;
                }

                // Skip if the file is a special file
                if (file.Name.StartsWith("$"))
                {
                    continue;
                }

                filePaths.Add(files[i].FullName);
            }

            return filePaths;
        }

        private static void GenerateCode(Exporter exporter, List<string> filePaths)
        {
            Console.WriteLine($"======Generate Code=======");
            try
            {
                exporter.GenerateCodeForCustomTypes();
                List<TypeDataValidator.Result> results = exporter.GenerateCodeForTables(filePaths);

                PrintResults(results);
                Console.WriteLine($"Finish generating code for {results.Count} sheets!");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.WriteLine($"Failed to generate code!");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine($"==========================");
                Console.WriteLine();
            }
        }

        private static void ExportData(Exporter exporter, List<string> filePaths)
        {
            Console.WriteLine($"=======Export Data========");
            try
            {
                List<TypeDataValidator.Result> results = exporter.GenerateDataForTables(filePaths);

                PrintResults(results);
                Console.WriteLine($"Finish exporting {results.Count} sheets!");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.WriteLine($"Failed to export data!");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine($"==========================");
                Console.WriteLine();
            }
        }

        private static void PrintResults(List<TypeDataValidator.Result> results)
        {
            for (var i = 0; i < results.Count; i++)
            {
                TypeDataValidator.Result result = results[i];
                if (result.IsValid)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Success: {result.TypeData.name}");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {result.TypeData.name}");
                    if (result.IsIdFieldMissing)
                    {
                        Console.WriteLine("  - Missing Id field");
                    }

                    if (result.HasDuplicatedName)
                    {
                        Console.WriteLine("  - Duplicated field name");
                    }

                    Console.ResetColor();
                }
            }
        }
    }
}
