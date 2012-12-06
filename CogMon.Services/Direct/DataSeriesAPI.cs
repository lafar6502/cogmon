using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BosonMVC.Services.DirectHandler;
using NLog;
using System.IO;
using System.Security.Principal;
using CogMon.Services.Dao;
using CogMon.Lib.Gui;
using CogMon.Lib.Graph;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Lib;
using CogMon.Lib.DataSeries;
using CogMon.Services.EventStats;

namespace CogMon.Services.Direct
{
    public class DataSeriesAPI : IDirectAction
    {
        public MongoDatabase Db { get; set; }


        

    }
}
