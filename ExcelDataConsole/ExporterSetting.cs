namespace ExcelDataConsole
{
    public class ExporterSetting
    {
        public string sourceDataDirectory = "";
        public string exportDataDirectory = "";
        public string exportScriptDirectory = "";
        public List<string> filters = new();

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(sourceDataDirectory))
            {
                return false;
            }

            if (string.IsNullOrEmpty(exportDataDirectory))
            {
                return false;
            }

            if (string.IsNullOrEmpty(exportScriptDirectory))
            {
                return false;
            }

            if (filters == null || filters.Count == 0)
            {
                return false;
            }

            return true;
        }
    }
}
