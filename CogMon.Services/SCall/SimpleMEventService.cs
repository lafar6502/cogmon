using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGinnBPM.MessageBus;
using CogMon.Lib.DataSeries;
using CogMon.Lib;
using NLog;
using CogMon.Services.Dao;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Lib.Graph;
using Newtonsoft.Json;

namespace CogMon.Services.SCall
{
    public class SimpleMEventService : 
        IMessageHandlerService<MEvent>
    {
        public MongoDatabase Db { get; set; }
        public Database.MongoKeyGen KeyGen { get; set; }

        private Logger log = LogManager.GetCurrentClassLogger();

        public object Handle(MEvent message)
        {
            message.Id = KeyGen.GetNextValue("mevents");
            var r = Db.GetCollection<MEvent>().Insert(message);
            return message.Id;
        }

        
    }

}
