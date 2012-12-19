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
    public class SortInfo
    {
        public string property { get; set; }
        public string direction { get; set; }
    }
    /// <summary>
    /// API for the admin GUI
    /// </summary>
    public class AdminGUI : IDirectAction
    {
        public MongoDatabase Db { get; set; }
        public IDataSeriesRepository DSRepo { get; set; }
        public IMessageDispatcher EventDispatcher { get; set; }

        [DirectMethod]
        public IList<DataSourceTemplate> GetDataSourceTemplates()
        {
            return Db.GetCollection<DataSourceTemplate>().FindAll().ToList();
        }

        [DirectMethod]
        public void SaveDataSourceTemplate(DataSourceTemplate tpl)
        {
            Db.GetCollection<DataSourceTemplate>().Save(tpl);
        }

        [DirectMethod]
        public IList<GroupInfo> GetUserGroups()
        {
            return Db.GetCollection<GroupInfo>().FindAll().ToList();
        }

        [DirectMethod]
        public void SaveGroup(GroupInfo gi)
        {
            Db.GetCollection<GroupInfo>().Save(gi);
        }

        [DirectMethod]
        public IList<EventCategory> GetEventCategories()
        {
            return Db.GetCollection<EventCategory>().FindAll().ToList();
        }

        [DirectMethod]
        public void SaveEventCategory(EventCategory ec)
        {
            Db.GetCollection<EventCategory>().Save(ec);
        }



        [DirectMethod]
        public DataSeriesInfo CreateDataSeriesFromTemplate(CreateDataSeriesFromTemplate msg)
        {
            return DSRepo.CreateDataSeriesFromTemplate(msg, UserSessionContext.CurrentUserIdentity.Name);
        }

        public class EvalResult
        {
            public bool Error { get; set; }
            public object Result { get; set; }
        }

        public IScriptHost ScriptHost { get; set; }
        
        [DirectMethod]
        public EvalResult EvalScript(string script, bool returnString)
        {
            try
            {
                var sc = ScriptHost.GetScriptInterpreter(UserSessionContext.CurrentUserIdentity.Name);
                var res = sc.Eval(script);
                return new EvalResult { 
                    Error = false, 
                    Result = returnString ? (object) Newtonsoft.Json.JsonConvert.SerializeObject(res, Newtonsoft.Json.Formatting.Indented) : res
                };
            }
            catch (Exception ex)
            {
                return new EvalResult { Error = true, Result = ex.ToString() };
            }
        }

        [DirectMethod]
        public object GetUsersList(int start, int limit, string filter, string sort, string dir)
        {
            var query = string.IsNullOrEmpty(filter) ? Query.Exists("_id", true) : Query.Or(Query.Matches("Login", filter), Query.Matches("Name", filter), Query.Matches("Email", filter));
            var uc = Db.GetCollection<UserInfo>().Find(query).SetSkip(start).SetLimit(limit)
                .SetFields("Id", "Login", "Active", "Email", "Name", "NeedsSync", "ExtId", "MemberOf", "LastSync");
            if (!string.IsNullOrEmpty(sort))
            {
                uc = uc.SetSortOrder(string.Equals("ASC", dir, StringComparison.InvariantCultureIgnoreCase) ? SortBy.Ascending(sort) : SortBy.Descending(sort));
            }
            return new
            {
                Total = uc.Count(),
                Data = uc.ToList()
            };
        }

        [DirectMethod]
        public void SaveUser(UserInfo ui)
        {
            if (string.IsNullOrEmpty(ui.Id))
            {
                Db.GetCollection<UserInfo>().Save(ui);
            }
            else
            {
                Db.GetCollection<UserInfo>().Update(Query.EQ("_id", ui.Id),
                    Update.Set("Name", ui.Name).Set("Email", ui.Email).Set("Login", ui.Login)
                    .Set("NeedsSync", ui.NeedsSync).Set("Active", ui.Active)
                    .Set("MemberOf", MongoDB.Bson.BsonArray.Create(ui.MemberOf)).Set("ExtId", ui.ExtId));
            }
        }
    }
}
