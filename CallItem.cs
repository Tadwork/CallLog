using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data;
using ServiceStack.OrmLite.Sqlite;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
namespace FPECallLog
{
    public class CallItem
    {
        [AutoIncrement,Index(Unique = true)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime Time { get; set; }
        
    }
    public class Database
    {
        IDbCommand dbCmd;
        public Database(string path)
        {
            //Use in-memory Sqlite DB instead
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Calls.db");
            var dbFactory = new OrmLiteConnectionFactory(dbPath, false, SqliteOrmLiteDialectProvider.Instance);

            //Non-intrusive: All extension methods hang off System.Data.* interfaces
            IDbConnection dbConn = dbFactory.OpenDbConnection();
            dbCmd = dbConn.CreateCommand();

            //Re-Create all table schemas:
            dbCmd.DropTable<CallItem>();
            dbCmd.CreateTable<CallItem>();
        }
        public IEnumerable<CallItem> TodaysCalls()
        {
            return dbCmd.Select<CallItem>().Where(x => x.Time.Date == DateTime.Now.Date);
        }

        public CallItem QueryCall(int id)
        {
            return (from s in Table<CallItem>()
                    where s.Id == id
                    select s).FirstOrDefault();
        }
        public void AddCall(string name,string phone)
        {
            Insert(new CallItem()
            {
                Name = name,Phone = phone,Time = DateTime.Now
            });
        }
        public void AddCall(CallItem call)
        {
            var s = Insert(call);
            //Console.WriteLine("{0}", s.Id);
        }
   
    }
}
