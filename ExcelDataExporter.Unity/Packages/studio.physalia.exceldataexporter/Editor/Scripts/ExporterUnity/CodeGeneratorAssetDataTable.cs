using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class CodeGeneratorAssetDataTable : CodeGenerator
    {
        public override string Generate(TypeData typeData)
        {
            bool hasNamespace = !string.IsNullOrEmpty(typeData.namespaceName);
            string tab = "    ";
            string ending = "\r\n";

            List<string> headerBlock = GenerateHeaderBlock(ending);
            List<string> usingBlock = GenerateUsingBlock(ending);
            List<string> typeBlock = GenerateTypeBlock(typeData, tab, ending);
            if (hasNamespace)
            {
                for (var i = 0; i < typeBlock.Count; i++)
                {
                    typeBlock[i] = tab + typeBlock[i];
                }
            }

            // Build string
            return BuildScriptText(headerBlock, usingBlock, typeBlock, ending, typeData.namespaceName);
        }

        private static List<string> GenerateUsingBlock(string ending)
        {
            var codes = new List<string>
            {
                $"using Physalia.ExcelDataRuntime;{ending}",
                $"{ending}",
            };
            return codes;
        }

        private static List<string> GenerateTypeBlock(TypeData typeData, string tab, string ending)
        {
            var codes = new List<string>
            {
                $"public class {typeData.name}Table : DataTable<{typeData.name}>{ending}",
                $"{{{ending}",
                $"{tab}{ending}",
                $"}}{ending}"
            };
            return codes;
        }
    }
}
