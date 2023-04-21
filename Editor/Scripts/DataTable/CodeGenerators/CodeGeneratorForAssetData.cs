using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class CodeGeneratorForAssetData : CodeGeneratorBase
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
            var namespaces = new List<string>
            {
                "System",
                "Physalia.ExcelDataExporter",
                "UnityEngine",
            };

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

            // Build string
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
                $"[Serializable]{ending}",
                $"public class {typeData.name} : IHasId{ending}",
                $"{{{ending}",
            };

            // Write fields
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string fieldName = fieldData.NameForPrivateField;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}[SerializeField]{ending}");
                codes.Add($"{tab}private {fieldTypeName} {fieldName};{ending}");
            }

            codes.Add(ending);

            // Write properties
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

                // Write property
                string propertyName = fieldData.NameForProperty;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}public {fieldTypeName} {propertyName} {{ get; }}{ending}");
            }

            codes.Add($"}}{ending}");

            return codes;
        }
    }
}
