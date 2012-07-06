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
using CogMon.Lib.Gui;
using Newtonsoft.Json;

namespace CogMon.Services.SCall
{
    public class UtilService
        : IMessageHandlerService<PortletDef>
    {
        public MongoDatabase Db { get; set; }

        public object Handle(PortletDef message)
        {
            Db.GetCollection<PortletDef>().Save(message);
            return message;
        }
    }
}
