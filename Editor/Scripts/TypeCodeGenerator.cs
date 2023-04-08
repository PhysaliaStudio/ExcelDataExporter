using System.Text;

namespace Physalia.ExcelDataExporter
{
    public static class TypeCodeGenerator
    {
        private const string WARNING_COMMENT =
@"// ###############################################
// #### AUTO GENERATED CODE, DO NOT MODIFIE!! ####
// ###############################################";

        public static string Generate(string namespaceName, TypeData typeData)
        {
            string usingBlock = CreateUsingBlock(typeData);

            var fieldBuilder = new StringBuilder();

            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);
            string tab = "    ";
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

{usingBlock}public class {typeData.name}
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
    }
}
