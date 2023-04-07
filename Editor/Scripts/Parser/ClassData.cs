using System.Collections.Generic;
using System.Text;

namespace Physalia.ExcelDataExporter
{
    public class ClassData
    {
        public string name;
        public List<FieldData> fieldDatas = new();
    }

    public static class DataTableCodeGenerator
    {
        private const string WARNING_COMMENT =
@"// ###############################################
// #### AUTO GENERATED CODE, DO NOT MODIFIE!! ####
// ###############################################
";

        public static string Generate(string namespaceName, string className, ClassData classData)
        {
            var fieldBuilder = new StringBuilder();

            bool hasNamespace = !string.IsNullOrEmpty(namespaceName);
            string tab = "    ";
            for (var i = 0; i < classData.fieldDatas.Count; i++)
            {
                FieldData fieldData = classData.fieldDatas[i];
                string typeName = fieldData.typeName;
                string fieldName = fieldData.name;

                if (hasNamespace)
                {
                    fieldBuilder.Append(tab);
                    fieldBuilder.Append(tab);
                }
                else
                {
                    fieldBuilder.Append(tab);
                }

                fieldBuilder.Append($"public {typeName} {fieldName};");

                if (i != classData.fieldDatas.Count - 1)
                {
                    fieldBuilder.AppendLine();
                }
            }

            string scriptText;
            if (hasNamespace)
            {
                scriptText =
$@"{WARNING_COMMENT}
using UnityEngine;

namespace {namespaceName}
{{
{tab}public class {className}
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
using UnityEngine;

public class {className}
{{
{fieldBuilder}
}}
";
            }

            return scriptText;
        }
    }
}
