using System.Collections.Generic;
using System.Text;

namespace Physalia.ExcelDataExporter
{
    public abstract class CodeGeneratorBase
    {
        public abstract string Generate(TypeData typeData);

        protected static List<string> GenerateHeaderBlock(string ending)
        {
            return new List<string>()
            {
                $"// ###############################################{ending}",
                $"// #### AUTO GENERATED CODE, DO NOT MODIFY!! #####{ending}",
                $"// ###############################################{ending}"
            };
        }

        protected static string BuildScriptText(List<string> header, List<string> usingBlock, List<string> typeBlock,
            string ending, string namespaceName = null)
        {
            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);

            var sb = new StringBuilder();

            for (var i = 0; i < header.Count; i++)
            {
                sb.Append(header[i]);
            }
            sb.Append(ending);

            for (var i = 0; i < usingBlock.Count; i++)
            {
                sb.Append(usingBlock[i]);
            }

            if (hasNamespace)
            {
                sb.Append($"namespace {namespaceName}{ending}");
                sb.Append($"{{{ending}");
            }

            for (var i = 0; i < typeBlock.Count; i++)
            {
                sb.Append(typeBlock[i]);
            }

            if (hasNamespace)
            {
                sb.Append($"}}{ending}");
            }

            return sb.ToString();
        }
    }
}
