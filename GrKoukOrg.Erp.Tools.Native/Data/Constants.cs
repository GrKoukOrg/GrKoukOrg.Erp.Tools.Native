namespace GrKoukOrg.Erp.Tools.Native.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "ErpToolsNativeSQLite.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";
    }
}