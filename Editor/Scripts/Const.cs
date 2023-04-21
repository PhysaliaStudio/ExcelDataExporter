namespace Physalia.ExcelDataExporter
{
    public static class Const
    {
        public const string PackageFolderPath = "Packages/studio.physalia.excel-data-exporter/";
        public const string UiAssetFolderPath = PackageFolderPath + "Editor/UiAssets/";

        public const string CustomTypeTableName = "$CustomTypes.xlsx";

        public const int SheetMetaRow = 0;
        public const int SheetMetaColumn = 0;

        public const int DataTableCommentRow = 0;
        public const int DataTableNameRow = 1;
        public const int DataTableTypeRow = 2;
        public const int DataTableStartRow = 3;

        public const int DataTableCommentColumn = 0;
        public const int DataTableNameColumn = 1;
        public const int DataTableTypeColumn = 2;
        public const int DataTableStartColumn = 3;

        public const string NotApplicable = "N/A";
    }
}
