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
    /// <summary>
    /// Remote call handler for various rrd management requests.
    /// </summary>
    public class DataSourceService : 
        IMessageHandlerService<GetDataSeriesInfo>,
        IMessageHandlerService<GetDataSeriesList>,
        IMessageHandlerService<GetDataSourceTemplateNames>,
        IMessageHandlerService<SaveDataSourceTemplate>,
        IMessageHandlerService<CreateDataSeriesFromTemplate>,
        IMessageHandlerService<UpdateData>,
        IMessageHandlerService<DataRecord>,
        IMessageHandlerService<DrawGraphByDefinition>,
        IMessageHandlerService<ReCreateDataSeries>,
        IMessageHandlerService<CreateGraphDefinitionFromTemplate>
    {
        public IDataSeriesRepository DSRepo { get; set; }
        public IEventAggregator EventAggregator { get; set; }
        public MongoDatabase Db { get; set; }

        public object Handle(GetDataSeriesInfo message)
        {
            var dsi = DSRepo.GetDataSeries(message.Name, true);
            return dsi;
        }

        public object Handle(Lib.DataSeries.GetDataSeriesList message)
        {
            return DSRepo.DataSources;
        }

        public object Handle(GetDataSourceTemplateNames message)
        {
            var ret = new GetDataSourceTemplateNamesResponse
            {
                Templates = new List<GetDataSourceTemplateNamesResponse.TplInfo>()
            };
            foreach (var tpl in Db.GetCollection<DataSourceTemplate>().FindAll())
            {
                ret.Templates.Add(new GetDataSourceTemplateNamesResponse.TplInfo
                {
                    Id = tpl.Id,
                    Name = tpl.Name,
                    Description = tpl.Description
                });
            }
            return ret;

            throw new NotImplementedException();
        }

        public object Handle(SaveDataSourceTemplate message)
        {
            Db.GetCollection<DataSourceTemplate>().Save(message.Template);
            return message.Template;
        }

        public object Handle(CreateDataSeriesFromTemplate message)
        {
            return DSRepo.CreateDataSeriesFromTemplate(message, "system");
        }

        public object Handle(UpdateData message)
        {
            Handle(message.Data);
            return "OK";
        }

        public object Handle(DataRecord message)
        {
            if (message.Series.StartsWith("#"))
            {
                message.Series = message.Series.Substring(1);
                EventAggregator.AddEvent(message);
            }
            else
            {
                DSRepo.AppendData(message);
            }
            return "OK";
        }

        public object Handle(DrawGraphByDefinition message)
        {
            DSRepo.CreateGraph(message.Definition, new DrawOptions(), message.DestinationFile);
            return message.DestinationFile;
        }

        public object Handle(ReCreateDataSeries message)
        {
            return DSRepo.ReCreateDataSeries(message.Id, "system");
        }

        public object Handle(CreateGraphDefinitionFromTemplate message)
        {
            var tpl = Db.GetCollection<GraphDefinitionTemplate>().FindOneById(message.TemplateId);
            if (tpl == null) throw new Exception("Template not found: " + message.TemplateId);
            Dictionary<string, object> vals = new Dictionary<string, object>(message.Variables);
            foreach (var vd in tpl.Variables)
            {
                if (!vals.ContainsKey(vd.Name))
                {
                    if (vd.DefaultValue != null)
                    {
                        vals[vd.Name] = vd.DefaultValue;
                    }
                    else if (vd.Required) throw new Exception("Missing required variable: " + vd.Name);
                }
            }
            string json = Utils.SubstValue(tpl.JsonTemplate, vals);

            var gd = JsonConvert.DeserializeObject<GraphDefinition>(json);
            gd.TemplateId = tpl.Id;
            gd.TemplateVariables = vals;
            gd.ACL = tpl.ACL;
            if (string.IsNullOrEmpty(gd.Description)) gd.Description = tpl.Description;
            Db.GetCollection<GraphDefinition>().Save(gd);
            return gd;
        }
    }
}
