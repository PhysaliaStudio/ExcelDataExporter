using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class CodeGeneratorForJsonSetting : CodeGeneratorBase
    {
        public override string Generate(TypeData typeData)
        {
            bool hasNamespace = !string.IsNullOrEmpty(typeData.namespaceName);
            string tab = "    ";
            string ending = "\r\n";

            List<string> headerBlock = GenerateHeaderBlock(ending);
            List<string> usingBlock = GenerateUsingBlock(typeData, ending);
            List<string> typeBlock = GenerateTypeBlock(typeData, tab, ending);
            if (hasNamespace)
            {
                for (var i = 0; i < typeBlock.Count; i++)
                {
                    if (typeBlock[i] != ending)
                    {
                        typeBlock[i] = tab + typeBlock[i];
                    }
                }
            }

            // Build string
            return BuildScriptText(headerBlock, usingBlock, typeBlock, ending, typeData.namespaceName);
        }

        private static List<string> GenerateUsingBlock(TypeData typeData, string ending)
        {
            // Collect namespaces
            List<string> namespaces = CollectNamespaces(typeData);

            // Build string
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

        private static List<string> GenerateTypeBlock(TypeData typeData, string tab, string ending)
        {
            var codes = new List<string>
            {
                $"public class {typeData.name}{ending}",
                $"{{{ending}",
            };

            // Write fields
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];

                // Write comment as summary
                string comment = fieldData.Comment;
                if (!string.IsNullOrEmpty(comment))
                {
                    if (i != 0)
                    {
                        codes.Add(ending);
                    }

                    codes.Add($"{tab}/// <summary>{ending}");
                    codes.Add($"{tab}/// {comment}{ending}");
                    codes.Add($"{tab}/// </summary>{ending}");
                }

                // Write field
                string fieldName = fieldData.NameForPublicField;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}public {fieldTypeName} {fieldName};{ending}");
            }

            codes.Add($"}}{ending}");

            return codes;
        }
    }
}
