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
    /// <summary>
    /// API for the admin GUI
    /// </summary>
    public class AdminGUI : IDirectAction
    {
        public MongoDatabase Db { get; set; }
        public IDataSeriesRepository DSRepo { get; set; }
        public IMessageDispatcher EventDispatcher { get; set; }

        [DirectMethod]
        IList<DataSourceTemplate> GetDataSourceTemplates()
        {
            return Db.GetCollection<DataSourceTemplate>().FindAll().ToList();
        }

        [DirectMethod]
        void SaveDataSourceTemplate(DataSourceTemplate tpl)
        {
            Db.GetCollection<DataSourceTemplate>().Save(tpl);
        }

        [DirectMethod]
        IList<GroupInfo> GetUserGroups()
        {
            return Db.GetCollection<GroupInfo>().FindAll().ToList();
        }

        [DirectMethod]
        void SaveGroup(GroupInfo gi)
        {
            Db.GetCollection<GroupInfo>().Save(gi);
        }

        [DirectMethod]
        IList<EventCategory> GetEventCategories()
        {
            return Db.GetCollection<EventCategory>().FindAll().ToList();
        }

        [DirectMethod]
        void SaveEventCategory(EventCategory ec)
        {
            Db.GetCollection<EventCategory>().Save(ec);
        }
    }
}
