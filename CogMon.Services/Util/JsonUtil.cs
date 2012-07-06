using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using System.Collections;

namespace CogMon.Services.Util
{
    class JsonUtil
    {
        public static Dictionary<string, object> NormalizeJsonObject(Dictionary<string, object> d)
        {
            if (d == null || d.Count == 0) return d;
            Dictionary<string, object> ret = new Dictionary<string, object>();
            foreach (var kv in d)
            {
                ret[kv.Key] = (kv.Value is JToken) ? Normalize((JToken)kv.Value) : kv.Value; 
            }
            return ret;
        }

        

        public static List<object> Normalize(JArray ja)
        {
            List<object> l = new List<object>();
            foreach (var i in ja)
            {
                l.Add(Normalize(i));
            }
            return l;
        }

        public static Dictionary<string, object> Normalize(JObject job)
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            foreach (var k in job)
            {
                ret[k.Key] = Normalize(k.Value);
            }
            return ret;
        }

        public static object Normalize(JValue jv)
        {
            return jv.Value;
        }

        public static object Normalize(JToken jt)
        {
            if (jt is JArray)
                return Normalize((JArray)jt);
            else if (jt is JObject)
                return Normalize((JObject)jt);
            else if (jt is JValue)
                return Normalize((JValue)jt);
            else
                throw new Exception("Unexpected token: " + jt.ToString());
        }

        public static Dictionary<string, object> NormalizeJsonObject(BsonDocument bd)
        {
            var d = new Dictionary<string, object>();
            foreach (var k in bd)
            {
                d[k.Name] = Normalize(k.Value);
            }
            return d;
        }

        public static List<object> Normalize(BsonArray arr)
        {
            var l = new List<object>();
            foreach (var v in arr)
            {
                l.Add(Normalize(v));
            }
            return l;
        }

        public static object Normalize(BsonValue bv)
        {
            if (bv is BsonArray || bv.BsonType == BsonType.Array) 
                return Normalize((BsonArray)bv);
            if (bv.BsonType == BsonType.Document || bv is BsonDocument)
                return NormalizeJsonObject((BsonDocument)bv);
            return bv.RawValue;
        }

        public static BsonArray ToBson(IEnumerable l)
        {
            BsonArray ba = new BsonArray();
            foreach(var v in l)
            {
                ba.Add(ToBson(v));
            }
            return ba;
        }

        public static BsonValue ToBson(object v)
        {
            if (v is IDictionary<string, object>)
            {
                return ToBson((IDictionary<string, object>) v);
            }
            else if (v is IEnumerable)
            {
                return ToBson((IEnumerable) v);
            }
            else if (v is BsonValue)
            {
                return (BsonValue) v;
            }
            else
            {
                return BsonValue.Create(v);
            }
        }

        public static BsonDocument ToBson(IDictionary<string, object> dic)
        {
            var bd = new BsonDocument();
            foreach (var k in dic.Keys)
            {
                bd[k] = ToBson(dic[k]); 
            }
            return bd;
        }
    }
}
