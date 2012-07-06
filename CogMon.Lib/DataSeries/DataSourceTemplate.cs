using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib.Scheduling;


namespace CogMon.Lib.DataSeries
{
    public enum TemplateVariableType
    {
        StringVar,
        NumberVar,
        DataSource,
        DataSourceAndVariable
    }
    /// <summary>
    /// All template variables are strings...
    /// </summary>
    public class VariableDef
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
        public TemplateVariableType VariableType { get; set; }
        /// <summary>
        /// Filter depends on variable type, for example for DataSource variable the filter
        /// will be the data source template Id, and for string variables it will be a regex
        /// </summary>
        public string Filter { get; set; }
    }

    public class SeriesTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Heartbeat { get; set; }
        public string SeriesType { get; set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string Expression { get; set; }
    }



    public class RRATemplate
    {
        public string ConsolidationFunction { get; set; }
        public string XFilesFactor { get; set; }
        public string AggregateSteps { get; set; }
        public string StoredRows { get; set; }
    }

    public class DataSourceTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<VariableDef> Variables { get; set; }

        public string StartTime { get; set; }
        public string Step { get; set; }
        public List<SeriesTemplate> Series { get; set; }
        public List<RRATemplate> Aggregates { get; set; }

        public bool CreateSchedule { get; set; }
        public string IntervalSecs { get; set; }
        public QueryType Query { get; set; }
        public string ScriptOrUrl { get; set; }
        public string Arguments { get; set; }
        public List<string> VariableValueRegex { get; set; }
    }

    public class GetDataSourceTemplateNames
    {
    }

    public class GetDataSourceTemplateNamesResponse
    {
        public class TplInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public List<TplInfo> Templates { get; set; }
    }

}
