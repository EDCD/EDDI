using System.Data.SQLite;
using Utilities;

namespace EddiDataProviderService
{
    public class SqLiteBaseRepository
    {
        public static string DbFile
        {
            get { return Constants.DATA_DIR + @"\EDDI.sqlite"; }
        }

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection("Data Source=" + DbFile);
        }
    }
}
