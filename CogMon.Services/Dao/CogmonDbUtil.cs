using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using CogMon.Lib;
using CogMon.Lib.DataSeries;

namespace CogMon.Services.Dao
{
    public class CogmonDbUtil
    {
        private static Dictionary<string, EventCategory> _cats;
        public static Dictionary<string, EventCategory> GetAllEventCategories(MongoDatabase db)
        {
            var c = _cats;
            if (c != null) return c;
            c = new Dictionary<string, EventCategory>();
            foreach (var ec in db.GetCollection<EventCategory>().FindAll())
            {
                c[ec.Id] = ec;
            }
            _cats = c;
            return c;
        }
    }
}
