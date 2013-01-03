using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        public override string ToString()
        {
            return Name + " " + Phone;
        }        
    }
    public class Database
    {
        IDbCommand dbCmd;
        #region Events
        public event DataUpdatedHandler DataUpdated;
        public EventArgs e = null;
        public delegate void DataUpdatedHandler(CallItem c, EventArgs e); 
        #endregion
        public Database(string path)
        {
            //Use in-memory Sqlite DB instead
            var dbPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Calls.db");
            var dbFactory = new OrmLiteConnectionFactory(dbPath, false, SqliteOrmLiteDialectProvider.Instance);

            //Non-intrusive: All extension methods hang off System.Data.* interfaces
            IDbConnection dbConn = dbFactory.OpenDbConnection();
            dbCmd = dbConn.CreateCommand();

            ////Re-Create all table schemas:
            //dbCmd.DropTable<CallItem>();
            dbCmd.CreateTable<CallItem>();
        }

        #region Read
        public IList<CallItem> TodaysCalls()
        {
            return dbCmd.Select<CallItem>().Where(x => x.Time.Date == DateTime.Now.Date).ToList();
        }

        public CallItem QueryCall(int id)
        {
            return dbCmd.Select<CallItem>().Where(x => x.Id == id).FirstOrDefault();
        } 
        #endregion
        #region Create
        public void AddCall(string name, string phone)
        {

            AddCall(new CallItem()
            {
                Name = name,
                Phone = phone.Remove(phone.IndexOf("@")),
                Time = DateTime.Now
            });
        }
        public void AddCall(CallItem call)
        {
            dbCmd.Insert<CallItem>(call);
            //Console.WriteLine("{0}", s.Id);
        } 
        #endregion
   
    }
}
