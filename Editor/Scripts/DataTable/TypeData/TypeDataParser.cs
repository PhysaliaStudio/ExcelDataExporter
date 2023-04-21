using System.Collections.Generic;
using System.IO;

namespace Physalia.ExcelDataExporter
{
    public class TypeDataParser
    {
        private class TypeRawField
        {
            private readonly string fieldName;
            private readonly string typeName;
            private readonly string comment;

            internal TypeRawField(string fieldName, string typeName, string comment)
            {
                this.typeName = typeName;
                this.fieldName = fieldName;
                this.comment = comment;
            }

            internal string FieldName => fieldName;
            internal string TypeName => typeName;
            internal string Comment => comment;
        }

        private readonly CustomTypeTable customTypeTable;

        public TypeDataParser()
        {
            customTypeTable = new CustomTypeTable();
        }

        public TypeDataParser(CustomTypeTable customTypeTable)
        {
            this.customTypeTable = customTypeTable;
        }

        public TypeData ExportTypeData(SheetRawData sheetRawData)
        {
            Metadata metadata = sheetRawData.Metadata;
            var typeData = new TypeData
            {
                isTypeWithId = metadata.SheetType == SheetType.DataTable,
                namespaceName = metadata.NamespaceName,
                name = sheetRawData.Name,
            };

            List<TypeRawField> rawFields;
            if (metadata.SheetLayout == SheetLayout.Horizontal)
            {
                rawFields = FindRawFieldsHorizontally(sheetRawData);
            }
            else
            {
                rawFields = FindRawFieldsVertically(sheetRawData);
            }

            ParseTypeData(typeData, rawFields);
            return typeData;
        }

        private List<TypeRawField> FindRawFieldsHorizontally(SheetRawData sheetRawData)
        {
            string[] fieldCommentRow = sheetRawData.GetRow(Const.DataTableCommentLine);
            string[] fieldNameRow = sheetRawData.GetRow(Const.DataTableNameLine);
            string[] fieldTypeNameRow = sheetRawData.GetRow(Const.DataTableTypeLine);

            var rawFields = new List<TypeRawField>(fieldNameRow.Length);
            for (var i = 0; i < sheetRawData.ColumnCount; i++)
            {
                rawFields.Add(new TypeRawField(fieldNameRow[i], fieldTypeNameRow[i], fieldCommentRow[i]));
            }

            return rawFields;
        }

        private List<TypeRawField> FindRawFieldsVertically(SheetRawData sheetRawData)
        {
            string[] fieldCommentColumn = sheetRawData.GetColumn(Const.DataTableCommentLine);
            string[] fieldNameColumn = sheetRawData.GetColumn(Const.DataTableNameLine);
            string[] fieldTypeNameColumn = sheetRawData.GetColumn(Const.DataTableTypeLine);

            var rawFields = new List<TypeRawField>(fieldNameColumn.Length);
            for (var i = 0; i < sheetRawData.RowCount; i++)
            {
                rawFields.Add(new TypeRawField(fieldNameColumn[i], fieldTypeNameColumn[i], fieldCommentColumn[i]));
            }

            return rawFields;
        }

        private void ParseTypeData(TypeData typeData, List<TypeRawField> rawFields)
        {
            var iterators = new Stack<TypeFieldIterator>();

            for (var i = 0; i < rawFields.Count; i++)
            {
                TypeRawField rawField = rawFields[i];

                // If we are not in a nested type, record the new field and check if we are starting a nested type
                if (iterators.Count == 0)
                {
                    FieldData newFieldData = RecordNewField(rawField);
                    typeData.fieldDatas.Add(newFieldData);

                    // Start new nested type if necessary
                    if (!newFieldData.typeData.IsSystemTypeOrEnum)
                    {
                        var iterator = new TypeFieldIterator(newFieldData);
                        iterators.Push(iterator);
                    }
                }

                // If we are in a nested type, check if current field is legal and check if we are done with the current type
                if (iterators.Count > 0)
                {
                    TypeFieldIterator iterator = iterators.Peek();
                    CheckIfFieldIsLegalWhileNestingType(iterator, rawField);
                    PopIteratorsIfFinished(iterators, rawFields, i);
                }
            }

            if (iterators.Count > 0)
            {
                throw new InvalidDataException("Invalid Data! The nested type does not finish.");
            }
        }

        private FieldData RecordNewField(TypeRawField nextField)
        {
            string fieldName = nextField.FieldName;
            string fieldTypeName = nextField.TypeName;
            bool isArray;

            TypeData fieldTypeData;
            if (fieldTypeName.EndsWith("[]"))  // Expected as array of an system type
            {
                isArray = true;

                fieldTypeName = fieldTypeName[0..^2];
                fieldTypeData = customTypeTable.GetTypeData(fieldTypeName);
                if (fieldTypeData == null || !fieldTypeData.IsSystemTypeOrEnum)
                {
                    throw new InvalidDataException($"Invalid field type cell: {fieldTypeName}");
                }
            }
            else
            {
                fieldTypeData = customTypeTable.GetTypeData(fieldTypeName);
                if (fieldTypeData == null)
                {
                    throw new InvalidDataException($"Invalid field type cell: {fieldTypeName}");
                }

                bool isSystemTypeOrEnum = fieldTypeData.IsSystemTypeOrEnum;

                // If not system type, then it must be a custom type, and the fieldName must have dot in it
                if (!isSystemTypeOrEnum)
                {
                    int indexOfDot = fieldName.IndexOf('.');
                    if (indexOfDot == -1)
                    {
                        throw new InvalidDataException($"Invalid field name cell: {fieldName}");
                    }

                    fieldName = fieldName[0..indexOfDot];
                }

                // The fieldName may have array index in it
                int indexOfArrayChar = fieldName.IndexOf('[');
                isArray = indexOfArrayChar != -1;
                if (isArray)
                {
                    if (isSystemTypeOrEnum)
                    {
                        throw new InvalidDataException($"Invalid array declaration: {fieldName} for {fieldTypeName}");
                    }

                    fieldName = fieldName[0..indexOfArrayChar];
                }
            }

            var fieldData = new FieldData
            {
                name = fieldName.Length > 1 ? char.ToLower(fieldName[0]) + fieldName[1..] : char.ToLower(fieldName[0]).ToString(),
                typeData = fieldTypeData,
                comment = nextField.Comment,
                isArray = isArray,
            };

            return fieldData;
        }

        private void CheckIfFieldIsLegalWhileNestingType(TypeFieldIterator iterator, TypeRawField rawField)
        {
            if (!iterator.IsAtFirstMember() && !string.IsNullOrEmpty(rawField.TypeName))
            {
                throw new InvalidDataException($"Invalid Data! The field type cell should be empty while in nested type, but was {rawField.TypeName}");
            }

            string expectedName = iterator.ExpectedName();
            if (expectedName != rawField.FieldName)
            {
                throw new InvalidDataException($"Invalid Data! The field name cell should be {expectedName}, but was {rawField.FieldName}");
            }
        }

        private void PopIteratorsIfFinished(Stack<TypeFieldIterator> iterators, List<TypeRawField> rawFields, int fieldIndex)
        {
            while (iterators.Count > 0)
            {
                TypeFieldIterator iterator = iterators.Peek();
                if (!iterator.IsAtLastMember())
                {
                    iterator.IncreaseMemberIndex();
                    break;
                }
                else
                {
                    if (iterator.IsArray)
                    {
                        // If we are iterating array type and end, check if there are more elements
                        if (fieldIndex < rawFields.Count - 1 && string.IsNullOrEmpty(rawFields[fieldIndex + 1].TypeName))
                        {
                            iterator.IncreaseArrayIndex();
                            break;
                        }
                        else
                        {
                            iterator.FieldData.arraySize = iterator.ArrayIndex + 1;
                            _ = iterators.Pop();
                        }
                    }
                    else
                    {
                        _ = iterators.Pop();
                    }
                }
            }
        }
    }
}
