using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;
using CogMon.Lib.DataSeries;
using CogMon.Lib.Graph;
using System.IO;
using CogMon.Services.RRD;

namespace CogMon.Services
{
    public class DataSourceRef
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Rrd repository
    /// </summary>
    public interface IDataSeriesRepository
    {
        IEnumerable<DataSourceRef> DataSources { get; }
        DataSeriesInfo GetDataSeries(string id, bool withCurrentFileInfo);
        void AppendData(DataRecord dr);
        void AddEvent(string id, double? value);
        DataSeriesInfo CreateDataSeriesFromTemplate(CreateDataSeriesFromTemplate request, string userId);
        DataSeriesInfo ReCreateDataSeries(string seriesId, string userId);
        RrdImageInfo CreateGraph(GraphDefinition gd, DrawOptions opts, string destinationFile);
        RrdImageInfo CreateGraph(GraphDefinition gd, DrawOptions options, Stream output);
        RrdImageInfo CreateGraph(string definitionId, DrawOptions options, Stream output);
        /// <summary>
        /// Retrieve most recent values of specified data fields
        /// </summary>
        /// <param name="dataSeries"></param>
        /// <returns></returns>
        IList<double> GetLastDataValues(IEnumerable<KeyValuePair<string, string>> dataSeries);
        RrdDataXport ExportGraphData(string graphDefinitionId, DrawOptions opts);
    }
}
