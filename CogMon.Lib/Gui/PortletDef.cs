using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson;

namespace CogMon.Lib.Gui
{
    public class PortletDef 
    {
        public string Id { get; set; }
        public string PortletClass { get; set; }
        public string Title { get; set; }
        //[BsonDictionaryOptions(DictionaryRepresentation.Dynamic)]
        //public Dictionary<string, object> Config { get; set; }
        public BsonDocument Config { get; set; }
        public PortletDef() 
        {
            //Config = new Dictionary<string, object>();
            Config = new BsonDocument();
            ACL = new List<string>();
        }

        public List<string> ACL { get; set; }
    }
}
