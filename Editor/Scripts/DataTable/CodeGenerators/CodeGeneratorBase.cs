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

        protected static List<string> CollectNamespaces(TypeData typeData, params string[] start)
        {
            var namespaces = new List<string>(start);

            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string namespaceName = fieldData.typeData.namespaceName;
                if (string.IsNullOrEmpty(namespaceName) ||
                    typeData.namespaceName.StartsWith(namespaceName) ||
                    namespaces.Contains(namespaceName))
                {
                    continue;
                }

                namespaces.Add(namespaceName);
            }

            namespaces.Sort((a, b) =>
            {
                bool aIsSystem = a.StartsWith("System");
                bool bIsSystem = b.StartsWith("System");
                if (aIsSystem && !bIsSystem)
                {
                    return -1;
                }
                else if (bIsSystem && !aIsSystem)
                {
                    return 1;
                }
                else
                {
                    return a.CompareTo(b);
                }
            });

            return namespaces;
        }

        protected static List<string> GenerateUsingBlock(List<string> namespaces, string ending)
        {
            if (namespaces.Count == 0)
            {
                return new List<string>();
            }

            var codes = new List<string>(namespaces.Count + 1);
            for (var i = 0; i < namespaces.Count; i++)
            {
                codes.Add($"using {namespaces[i]};{ending}");
            }
            codes.Add(ending);

            return codes;
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
