using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using CogMon.Lib.Gui;
using MongoDB.Bson.IO;
using System.IO;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using CogMon.Services;
using Newtonsoft.Json;

namespace SomeTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            NLog.Config.SimpleConfigurator.ConfigureForConsoleLogging(NLog.LogLevel.Debug);
            InitMappings();
            TestDeser();
            return;
            MongoDatabase db = MongoDatabase.Create(System.Configuration.ConfigurationManager.ConnectionStrings["cogmon"].ConnectionString);
            //Test2(db);
            //return;
            var d = new Dictionary<string, object>();
            d["url"] = "lalal://oeoiweo.doi";
            PortalPageColumn c = new PortalPageColumn 
            {
                Config = new Dictionary<string,object>()
            };
            c.Config["ala"] = "ma kota";
            c.Config["kot"] = d;

            StringWriter sw = new StringWriter();
            var w = BsonWriter.Create(sw);
            c.Portlets.Add(new Portlet());
            c.Portlets[0].Config = c.Config;

            BsonSerializer.Serialize(w, c);
            w.Flush();
            BsonSerializer.Deserialize<PortalPageColumn>(BsonReader.Create(sw.ToString()));
            Console.WriteLine(sw.ToString());
            
        }

        static void InitMappings()
        {
            BsonClassMap.RegisterClassMap<PortalPage>(m =>
            {
                m.AutoMap();
                m.SetIdMember(m.GetMemberMap("Id"));
                m.IdMemberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
            });
        }

        public static void Test2(MongoDatabase db)
        {
            var d = new Dictionary<string, object>();
            var pp = new PortalPage { Title = "This is my test", Id = "test1" };
            var pc = new PortalPageColumn();
            pp.Columns.Add(pc);
            pc.Config["width"] = 200;
            d["open"] = "open listener";
            d["close"] = "close listener";
            d["interval"] = 500;
            pc.Config["listeners"] = d;
            
            d = new Dictionary<string, object>();
            d["open"] = "open listener";
            d["close"] = "close listener";
            d["interval"] = 500;
            pc.Config["listeners"] = d;
            
            var pt = new Portlet { Title = "test portlet 1", PortletClass = "GraphImage" };
            pt.Config["width"] = 200;
            pt.Config["height"] = 300;
            pt.Config["listeners"] = d;
            pc.Portlets.Add(pt);

            db.GetCollection<PortalPage>().Save(pp);

            var pp2 = db.GetCollection<PortalPage>().FindOneById(pp.Id);
            Console.WriteLine("p2: {0}", pp2.Title);
        }

        static void TestDeser()
        {
            var d = new Dictionary<string, object>();
            var pp = new PortalPage { Title = "This is my test", Id = "test1" };
            var pc = new PortalPageColumn();
            pp.Columns.Add(pc);
            pc.Config["width"] = 200;
            d["open"] = "open listener";
            d["close"] = "close listener";
            d["interval"] = 500;
            pc.Config["listeners"] = d;

            d = new Dictionary<string, object>();
            d["open"] = "open listener";
            d["close"] = "close listener";
            d["interval"] = 500;
            pc.Config["listeners"] = d;

            var pt = new Portlet { Title = "test portlet 1", PortletClass = "GraphImage" };
            pt.Config["width"] = 200;
            pt.Config["height"] = 300;
            pt.Config["listeners"] = new Dictionary<string, object>(d);
            var l = new List<object>();
            var d2 = new Dictionary<string, object>();
            d2["name"] = "ko";
            d2["value"] = 289;
            l.Add(d2);
            l.Add(new Dictionary<string, object>(d2));
            pt.Config["dataFields"] = l;

            pc.Portlets.Add(pt);
            var sw = new StringWriter();
            var bw = BsonWriter.Create(sw);
            BsonSerializer.Serialize(bw, pp);
            bw.Flush();
            Console.WriteLine("S1: {0}", sw.ToString());
            var p2 = BsonSerializer.Deserialize<PortalPage>(BsonReader.Create(new StringReader(sw.ToString())));

        }
    }
}
