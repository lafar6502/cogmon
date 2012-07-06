using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace CogMon.Services
{
    public static class MongoUtil
    {

        public static MongoCollection<T> GetCollection<T>(this MongoDatabase database)
        {
            
            string tn = typeof(T).Name.ToLower();
            if (!tn.EndsWith("s")) tn += "s";
            return database.GetCollection<T>(tn);
        }

        public static MongoCursor<T> Find<T>(this MongoDatabase db, Func<dynamic, QueryNode> predicate)
        {
            return db.GetCollection<T>().Find(MongoQueryBuilder.DynQuery(predicate));
        }

        public static MongoCursor<T> FindAll<T>(this MongoDatabase db)
        {
            return db.GetCollection<T>().FindAll();
        }


    }
}
