using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib.Graph;
using CogMon.Lib.DataSeries;
using System.IO;

namespace CogMon.Services.RRD
{

    public class RrdEventInfo
    {
        public int Time { get; set; }
        public string Color { get; set; }
        public string Legend { get; set; }
        public string EventId { get; set; }

        public virtual string ToRRDString()
        {
            return string.Format(string.IsNullOrEmpty(Legend) ? "VRULE:{0}#{1}" : "\"VRULE:{0}#{1}:{2}\"", Time, Color, Legend); 
        }
    }

    /// <summary>
    /// RRDtool interface 
    /// </summary>
    public interface IRrdTool
    {
        /// <summary>
        /// Append data to the RRD file
        /// </summary>
        /// <param name="rrdFile"></param>
        /// <param name="timestamp"></param>
        /// <param name="values"></param>
        void AppendData(string rrdFile, DateTime? timestamp, double[] values);
        void AppendData(string rrdFile, DateTime? timestamp, IDictionary<string, double> values);
        /// <summary>
        /// Create new RRD file
        /// </summary>
        /// <param name="config"></param>
        /// <param name="fileName"></param>
        void CreateRRDFile(RrdCreateInfo config, string fileName);
        /// <summary>
        /// Save graph to specified file
        /// </summary>
        /// <param name="graphConfig"></param>
        /// <param name="outputFile"></param>
        void SaveGraphFile(RRDGraphInfo graphConfig, string outputFile);

        /// <summary>
        /// Read RRD information from a given file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        RrdFileInfo GetRrdInfoFromFile(string fileName);


        RrdImageInfo DrawGraph(GraphDefinition gd, DrawOptions opts, IEnumerable<RrdEventInfo> addEvents, string destinationFile);

        RrdImageInfo DrawGraph(GraphDefinition gd, DrawOptions opts, string destinationFile);


        void DrawGraph(GraphDefinition gd, DrawOptions opts, Stream writeImageTo);

        TimeSeriesData ExportGraphData(GraphDefinition gd, DrawOptions opts);

    }
}
