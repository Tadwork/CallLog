using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SQLite;

namespace FPECallLog
{
    public class CallItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string Name { get; set; }
        public string Phone { get; set; }
        public DateTime Time { get; set; }
        
    }
    public class Database : SQLiteConnection
    {
        public Database(string path)
            : base(path)
        {
            CreateTable<CallItem>();
        }
        public IEnumerable<CallItem> TodaysCalls()
        {
            return Table<CallItem>().Where(x => x.Time.Date == DateTime.Now.Date);
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
            Console.WriteLine("{0}", s.Id);
        }
   
    }
}
