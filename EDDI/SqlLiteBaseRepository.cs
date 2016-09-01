using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDDI
{
    public class SqLiteBaseRepository
    {
        public static string DbFile
        {
            get { return Environment.GetEnvironmentVariable("AppData") + "\\EDDI" + "\\EDDI.sqlite"; }
        }

        public static SQLiteConnection SimpleDbConnection()
        {
            return new SQLiteConnection("Data Source=" + DbFile);
        }
    }
}
