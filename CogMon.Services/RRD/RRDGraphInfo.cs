using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CogMon.Services.RRD
{
    /// <summary>
    /// Source data reference
    /// </summary>
    public class DataSeriesSource
    {
        public string SeriesId { get; set; }
        /// <summary>
        /// Data series name
        /// </summary>
        public string SeriesName { get; set; }
        /// <summary>
        /// RRD file name
        /// </summary>
        [JsonIgnore]
        public string SeriesFile { get; set; }
        /// <summary>
        /// data field name
        /// </summary>
        public string FieldName { get; set; }
    }

    /// <summary>
    /// Definition of plotted variable
    /// based on RRDTool CDEF expressions
    /// </summary>
    public class GraphVariableDefinition
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string CDEFExpression { get; set; }
    }

    /// <summary>
    /// Graph configuration
    /// </summary>
    public class VariableGraphConfig
    {
        public string VariableName { get; set; }
        public string Color { get; set; }
        public GraphTypes GraphType { get; set; }
        public string Legend { get; set; }
    }

    public enum GraphTypes
    {
        LINE1,
        LINE2,
        AREA,
        STACK
    }

    /// <summary>
    /// Graph configuration
    /// </summary>
    public class RRDGraphInfo
    {
        [JsonIgnore]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<DataSeriesSource> DataSources { get; set; }
        public List<GraphVariableDefinition> GraphVariables { get; set; }
        public List<VariableGraphConfig> Graphs { get; set; }
        public List<string> GraphOptions { get; set; }

        public RRDGraphInfo()
        {
            DataSources = new List<DataSeriesSource>();
            GraphVariables = new List<GraphVariableDefinition>();
            Graphs = new List<VariableGraphConfig>();
        }

    }
}
