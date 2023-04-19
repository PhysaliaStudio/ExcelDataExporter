using System.Collections.Generic;
using System.Text;

namespace Physalia.ExcelDataExporter
{
    public static class TypeCodeGenerator
    {
        public static string GeneratePoco(TypeData typeData)
        {
            bool hasNamespace = !string.IsNullOrEmpty(typeData.namespaceName);
            string tab = "    ";
            string ending = "\r\n";

            List<string> headerBlock = GenerateHeaderBlock(ending);
            List<string> usingBlock = GenerateUsingBlockOfPoco(typeData, ending);
            List<string> typeBlock = GenerateTypeBlockOfPoco(typeData, tab, ending);
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

        public static string GenerateTypeClass(TypeData typeData)
        {
            bool hasNamespace = !string.IsNullOrEmpty(typeData.namespaceName);
            string tab = "    ";
            string ending = "\r\n";

            List<string> headerBlock = GenerateHeaderBlock(ending);
            List<string> usingBlock = GenerateUsingBlockOfClass(typeData, ending);

            // Data class
            List<string> typeBlock = GenerateTypeBlockOfClass(typeData, tab, ending);
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

        private static List<string> GenerateHeaderBlock(string ending)
        {
            return new List<string>()
            {
                $"// ###############################################{ending}",
                $"// #### AUTO GENERATED CODE, DO NOT MODIFY!! #####{ending}",
                $"// ###############################################{ending}"
            };
        }

        private static List<string> GenerateUsingBlockOfPoco(TypeData typeData, string ending)
        {
            var codes = new List<string> {
                $"using System;{ending}",
            };

            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string typeName = fieldData.typeData.name;
                if (TypeUtility.IsUnityType(typeName))
                {
                    codes.Add($"using UnityEngine;{ending}");
                    break;
                }
            }

            codes.Add(ending);

            return codes;
        }

        private static List<string> GenerateUsingBlockOfClass(TypeData typeData, string ending)
        {
            var codes = new List<string> {
                $"using System;{ending}",
                $"using Physalia.ExcelDataExporter;{ending}"
            };

            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string typeName = fieldData.typeData.name;
                if (TypeUtility.IsUnityType(typeName))
                {
                    codes.Add($"using UnityEngine;{ending}");
                    break;
                }
            }

            codes.Add(ending);

            return codes;
        }

        private static List<string> GenerateTypeBlockOfPoco(TypeData typeData, string tab, string ending)
        {
            var codes = new List<string>
            {
                $"[Serializable]{ending}",
                $"public class {typeData.name}{ending}",
                $"{{{ending}",
            };

            // Write fields
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string fieldName = fieldData.NameForField;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}public {fieldTypeName} {fieldName};{ending}");
            }

            codes.Add($"}}{ending}");

            return codes;
        }

        private static List<string> GenerateTypeBlockOfClass(TypeData typeData, string tab, string ending)
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
                string fieldName = fieldData.NameForField;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}public {fieldTypeName} {fieldName};{ending}");
            }

            codes.Add(ending);

            // Write properties
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string propertyName = fieldData.NameForProperty;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}public {fieldTypeName} {propertyName} {{ get; }}{ending}");
            }

            codes.Add($"}}{ending}");

            return codes;
        }

        public static string GenerateTypeTableClass(TypeData typeData)
        {
            bool hasNamespace = !string.IsNullOrEmpty(typeData.namespaceName);
            string tab = "    ";
            string ending = "\r\n";

            List<string> headerBlock = GenerateHeaderBlock(ending);
            List<string> usingBlock = GenerateUsingBlockOfTableClass(ending);

            // Table class
            List<string> typeBlock = GenerateTypeBlockOfTableClass(typeData, tab, ending);
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

        private static List<string> GenerateUsingBlockOfTableClass(string ending)
        {
            var codes = new List<string>
            {
                $"using Physalia.ExcelDataExporter;{ending}",
                $"{ending}",
            };
            return codes;
        }

        private static List<string> GenerateTypeBlockOfTableClass(TypeData typeData, string tab, string ending)
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

        private static string BuildScriptText(List<string> header, List<string> usingBlock, List<string> typeBlock,
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
