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
}
