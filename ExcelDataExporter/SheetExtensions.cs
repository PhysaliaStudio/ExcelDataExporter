using System.IO;
using Physalia.ExcelDataExporter.Loader;

namespace Physalia.ExcelDataExporter
{
    public enum SheetLayout { Horizontal, Vertical }
    public enum SheetType { DataTable, Setting, CustomTypeTable }

    public static class SheetExtensions
    {
        private const string KeyNamespace = "namespace";
        private const string KeyLayout = "layout";
        private const string KeyType = "type";

        public static string GetNamespace(this Metadata metadata)
        {
            return metadata[KeyNamespace];
        }

        public static SheetLayout GetSheetLayout(this Metadata metadata)
        {
            string value = metadata[KeyLayout];
            if (string.IsNullOrEmpty(value))
            {
                SheetType sheetType = metadata.GetSheetType();
                switch (sheetType)
                {
                    case SheetType.DataTable:
                        return SheetLayout.Horizontal;
                    case SheetType.Setting:
                        return SheetLayout.Vertical;
                    case SheetType.CustomTypeTable:
                        return SheetLayout.Horizontal;
                }
            }

            if (value == "horizontal")
            {
                return SheetLayout.Horizontal;
            }
            else if (value == "vertical")
            {
                return SheetLayout.Vertical;
            }
            else
            {
                throw new InvalidDataException($"Unknown layout value: {value}");
            }
        }

        public static SheetType GetSheetType(this Metadata metadata)
        {
            string value = metadata[KeyType];
            if (string.IsNullOrEmpty(value))
            {
                return SheetType.DataTable;
            }

            if (value == "data-table")
            {
                return SheetType.DataTable;
            }
            else if (value == "setting")
            {
                return SheetType.Setting;
            }
            else if (value == "custom-type")
            {
                return SheetType.CustomTypeTable;
            }
            else
            {
                throw new InvalidDataException($"Unknown sheet type value: {value}");
            }
        }
    }
}
