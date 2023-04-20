using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Physalia.ExcelDataExporter
{
    public class DataExporterJson
    {
        public string Export(TypeData typeData, SheetRawData sheetRawData)
        {
            var sb = new StringBuilder();
            for (var i = Const.DataTableStartRow; i < sheetRawData.RowCount; i++)
            {
                string json = ExportDataRowAsJson(typeData, sheetRawData.GetRow(i));
                sb.Append(json);
                sb.Append('\n');
            }

            return sb.ToString();
        }

        private string ExportDataRowAsJson(TypeData typeData, string[] dataRow)
        {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            var columnIndex = 0;
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];

                // If the field is system type, write it directly
                if (fieldData.IsSystemType)
                {
                    WritePropertyForSystemType(writer, fieldData, dataRow[columnIndex]);
                    columnIndex++;
                    continue;
                }

                // If the field is custom type, write it recursively
                columnIndex = WritePropertyForCustomType(writer, fieldData, dataRow, columnIndex);
            }

            writer.WriteEndObject();
            return sb.ToString();
        }

        private int WritePropertyForCustomType(JsonTextWriter writer, FieldData fieldData, string[] dataRow, int columnIndex)
        {
            writer.WritePropertyName(fieldData.NameForPublicField);

            var iterator = new TypeFieldIterator(fieldData);
            if (fieldData.IsArray)
            {
                writer.WriteStartArray();
            }

            while (true)
            {
                bool isCurrentCellApplicable = dataRow[columnIndex] != Const.NotApplicable;

                if (iterator.IsAtFirstMember())
                {
                    if (isCurrentCellApplicable)
                    {
                        writer.WriteStartObject();
                    }
                }

                FieldData currentMemberFieldData = iterator.CurrentMember;
                if (currentMemberFieldData.IsSystemType)
                {
                    if (isCurrentCellApplicable)
                    {
                        WritePropertyForSystemType(writer, currentMemberFieldData, dataRow[columnIndex]);
                    }
                    columnIndex++;
                }
                else
                {
                    if (isCurrentCellApplicable)
                    {
                        columnIndex = WritePropertyForCustomType(writer, currentMemberFieldData, dataRow, columnIndex);
                    }
                    else
                    {
                        columnIndex += currentMemberFieldData.EvaluateColumnCount();
                    }
                }

                // Increase array index if not finished
                if (!iterator.IsAtLastMember())
                {
                    iterator.IncreaseMemberIndex();
                }
                else if (iterator.IsArray && iterator.ArrayIndex < iterator.FieldData.arraySize - 1)
                {
                    if (isCurrentCellApplicable)
                    {
                        writer.WriteEndObject();
                    }
                    iterator.IncreaseArrayIndex();
                }
                else
                {
                    if (isCurrentCellApplicable)
                    {
                        writer.WriteEndObject();
                    }
                    break;
                }
            }

            if (fieldData.IsArray)
            {
                writer.WriteEndArray();
            }

            return columnIndex;
        }

        private void WritePropertyForSystemType(JsonTextWriter writer, FieldData fieldData, string dataText)
        {
            writer.WritePropertyName(fieldData.NameForPublicField);
            if (fieldData.IsArray)
            {
                writer.WriteStartArray();

                string[] splits = dataText.Split(',');
                for (var j = 0; j < splits.Length; j++)
                {
                    WriteValueForSystemType(writer, fieldData.BaseTypeName, splits[j]);
                }

                writer.WriteEndArray();
            }
            else
            {
                string text = dataText;
                WriteValueForSystemType(writer, fieldData.BaseTypeName, text);
            }
        }

        private void WriteValueForSystemType(JsonTextWriter writer, string typeName, string text)
        {
            object value = TypeUtility.ParseValueToSystemType(typeName, text);
            writer.WriteValue(value);
        }
    }
}
