using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Newtonsoft.Json;

namespace CogMon.Lib.Gui
{
    public class PortalPage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string OwnerId { get; set; }
        public List<string> ACL { get; set; }

        public string PageClass { get; set; }
        
        public Dictionary<string, object> Config { get;set;}

        
        public List<PortalPageColumn> Columns { get; set; }
        public int IdGen { get; set; }
        public bool Editable { get; set; }

        public PortalPage()
        {
            Config = new Dictionary<string, object>();
            Columns = new List<PortalPageColumn>();
            ACL = new List<string>();
        }
    }


    public class PortalPageColumn
    {
        public Dictionary<string, object> Config { get; set; }
        public List<Portlet> Portlets { get; set; }

        public PortalPageColumn()
        {
            Config = new Dictionary<string, object>();
            Portlets = new List<Portlet>();
        }
    }

    public class Portlet
    {
        public string Id { get; set; }
        public string PortletClass { get; set; }
        public string Title { get; set; }
        
        
        public Dictionary<string, object> Config { get; set; }
        
        
        public Portlet()
        {
            Config = new Dictionary<string, object>();
        }
    }
}
