using System.IO;
using Microsoft.Maui.Storage;
namespace GrKoukOrg.Erp.Tools.Native.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "ErpToolsNativeSQLite.db3";

        public static string DatabasePath
        {
            get
            {
                var databasePath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DatabaseFilename);
                Console.WriteLine($"*******Database full pathname {databasePath}");
                //var databasePath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename); 
                return $"Data Source={databasePath}";
            }
        }

          
    }
}