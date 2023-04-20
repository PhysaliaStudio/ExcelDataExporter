using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class CodeGeneratorForCustomType : CodeGeneratorBase
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
                    typeBlock[i] = tab + typeBlock[i];
                }
            }

            // Build string
            return BuildScriptText(headerBlock, usingBlock, typeBlock, ending, typeData.namespaceName);
        }

        private static List<string> GenerateUsingBlock(TypeData typeData, string ending)
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

        private static List<string> GenerateTypeBlock(TypeData typeData, string tab, string ending)
        {
            string typeDefine = typeData.define == TypeData.Define.Class ? "class" : "struct";
            var codes = new List<string>
            {
                $"[Serializable]{ending}",
                $"public {typeDefine} {typeData.name}{ending}",
                $"{{{ending}",
            };

            // Write fields
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                string fieldName = fieldData.NameForPublicField;
                string fieldTypeName = fieldData.TypeName;
                codes.Add($"{tab}public {fieldTypeName} {fieldName};{ending}");
            }

            codes.Add($"}}{ending}");

            return codes;
        }
    }
}
