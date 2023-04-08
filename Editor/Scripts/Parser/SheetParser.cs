using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Physalia.ExcelDataExporter
{
    public class SheetParser
    {
        public ClassData ExportClassData(string className, SheetRawData sheetRawData)
        {
            string[] nameRow = sheetRawData.GetRow(0);
            string[] typeNameRow = sheetRawData.GetRow(1);

            var classData = new ClassData { name = className };
            for (var i = 0; i < sheetRawData.ColumnCount; i++)
            {
                string name = nameRow[i];
                string typeName = typeNameRow[i];

                var fieldData = new FieldData
                {
                    name = name.Length > 1 ? char.ToLower(name[0]) + name[1..] : char.ToLower(name[0]).ToString(),
                    typeName = typeName,
                };
                classData.fieldDatas.Add(fieldData);
            }

            return classData;
        }

        public string ExportDataTableAsJson(SheetRawData sheetRawData)
        {
            var sb = new StringBuilder();

            ClassData classData = ExportClassData("", sheetRawData);  // TODO: class name
            for (var i = 2; i < sheetRawData.RowCount; i++)
            {
                string json = ExportDataRowAsJson(classData, sheetRawData.GetRow(i));
                sb.Append(json);
                sb.Append('\n');
            }

            return sb.ToString();
        }

        private string ExportDataRowAsJson(ClassData classData, string[] dataRow)
        {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            for (var i = 0; i < dataRow.Length; i++)
            {
                FieldData fieldData = classData.fieldDatas[i];

                writer.WritePropertyName(fieldData.name);
                if (fieldData.IsArray)
                {
                    writer.WriteStartArray();

                    string[] splits = dataRow[i].Split(',');
                    for (var j = 0; j < splits.Length; j++)
                    {
                        WriteForBuiltInTypes(writer, fieldData.BaseTypeName, splits[j]);
                    }

                    writer.WriteEndArray();
                }
                else
                {
                    string text = dataRow[i];
                    WriteForBuiltInTypes(writer, fieldData.BaseTypeName, text);
                }
            }

            writer.WriteEndObject();
            return sb.ToString();
        }

        private void WriteForBuiltInTypes(JsonTextWriter writer, string typeName, string text)
        {
            object value = ParseBuiltInValue(typeName, text);
            writer.WriteValue(value);
        }

        public static object ParseBuiltInValue(string typeName, string text)
        {
            switch (typeName)
            {
                default:
                    return null;
                case "string":
                    return text;
                case "bool":
                    {
                        bool success = bool.TryParse(text, out bool result);
                        return success ? result : default;
                    }
                case "byte":
                    {
                        bool success = byte.TryParse(text, out byte result);
                        return success ? result : default;
                    }
                case "sbyte":
                    {
                        bool success = sbyte.TryParse(text, out sbyte result);
                        return success ? result : default;
                    }
                case "short":
                    {
                        bool success = short.TryParse(text, out short result);
                        return success ? result : default;
                    }
                case "ushort":
                    {
                        bool success = ushort.TryParse(text, out ushort result);
                        return success ? result : default;
                    }
                case "int":
                    {
                        bool success = int.TryParse(text, out int result);
                        return success ? result : default;
                    }
                case "uint":
                    {
                        bool success = uint.TryParse(text, out uint result);
                        return success ? result : default;
                    }
                case "long":
                    {
                        bool success = long.TryParse(text, out long result);
                        return success ? result : default;
                    }
                case "ulong":
                    {
                        bool success = ulong.TryParse(text, out ulong result);
                        return success ? result : default;
                    }
                case "float":
                    {
                        bool success = float.TryParse(text, out float result);
                        return success ? result : default;
                    }
                case "double":
                    {
                        bool success = double.TryParse(text, out double result);
                        return success ? result : default;
                    }
                case "decimal":
                    {
                        bool success = decimal.TryParse(text, out decimal result);
                        return success ? result : default;
                    }
            }
        }
    }
}
