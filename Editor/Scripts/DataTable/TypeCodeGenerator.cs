using System.Collections.Generic;
using System.Text;

namespace Physalia.ExcelDataExporter
{
    public static class TypeCodeGenerator
    {
        private const string WARNING_COMMENT =
@"// ###############################################
// #### AUTO GENERATED CODE, DO NOT MODIFY!! #####
// ###############################################";

        public static string Generate(string namespaceName, TypeData typeData)
        {
            string usingBlock = CreateUsingBlock(typeData);

            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);
            string tab = "    ";

            // Data class
            var fieldBuilder = new StringBuilder();
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string fieldName = fieldData.name;
                string fieldTypeName = fieldData.typeData.name;
                if (fieldData.IsArray)
                {
                    fieldTypeName += "[]";
                }

                if (hasNamespace)
                {
                    fieldBuilder.Append(tab);
                    fieldBuilder.Append(tab);
                }
                else
                {
                    fieldBuilder.Append(tab);
                }

                fieldBuilder.Append($"public {fieldTypeName} {fieldName};");

                if (i != typeData.fieldDatas.Count - 1)
                {
                    fieldBuilder.AppendLine();
                }
            }

            string scriptText;
            if (hasNamespace)
            {
                scriptText =
$@"{WARNING_COMMENT}

{usingBlock}namespace {namespaceName}
{{
{tab}[Serializable]
{tab}public class {typeData.name}
{tab}{{
{fieldBuilder}
{tab}}}
}}
";
            }
            else
            {
                scriptText =
$@"{WARNING_COMMENT}

{usingBlock}[Serializable]
public class {typeData.name}
{{
{fieldBuilder}
}}
";
            }

            return scriptText;
        }

        private static string CreateUsingBlock(TypeData typeData)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");

            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string typeName = fieldData.typeData.name;
                if (TypeUtility.IsUnityType(typeName))
                {
                    sb.AppendLine("using UnityEngine;");
                    break;
                }
            }

            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static string GenerateCodesOfTypeTable(string namespaceName, TypeData typeData)
        {
            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);
            string tab = "    ";
            string ending = "\r\n";

            List<string> codesOfUsingBlock = GenerateCodeOfUsingBlockOfTableClass(ending);

            // Table class
            List<string> codesOfClass = GenerateCodesOfTableClass(typeData, tab, ending);
            if (hasNamespace)
            {
                for (var i = 0; i < codesOfClass.Count; i++)
                {
                    codesOfClass[i] = tab + codesOfClass[i];
                }
            }

            // Build string
            var sb = new StringBuilder();

            sb.Append(WARNING_COMMENT);
            sb.Append(ending);
            sb.Append(ending);

            for (var i = 0; i < codesOfUsingBlock.Count; i++)
            {
                sb.Append(codesOfUsingBlock[i]);
            }

            if (hasNamespace)
            {
                sb.Append($"namespace {namespaceName}{ending}");
                sb.Append($"{{{ending}");
            }

            for (var i = 0; i < codesOfClass.Count; i++)
            {
                sb.Append(codesOfClass[i]);
            }

            if (hasNamespace)
            {
                sb.Append($"}}{ending}");
            }

            return sb.ToString();
        }

        private static List<string> GenerateCodeOfUsingBlockOfTableClass(string ending)
        {
            var codes = new List<string>
            {
                $"using Physalia.ExcelDataExporter;{ending}",
                $"{ending}",
            };
            return codes;
        }

        private static List<string> GenerateCodesOfTableClass(TypeData typeData, string tab, string ending)
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
