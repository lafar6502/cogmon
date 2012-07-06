using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.DataSeries
{
    public class CreateDataSeriesFromTemplate
    {
        public string TemplateId { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        /// <summary>
        /// Id of data source to overwrite.
        /// Leave null to create a new data source
        /// </summary>
        public string OverwriteDataSourceId { get; set; }
    }

    public class CreateDataSeriesFromTemplateResponse
    {
        public bool Success { get; set; }
        public string Series { get; set; }
        public string ErrorInfo { get; set; }
    }

    public class ReCreateDataSeries
    {
        public string Id;
    }

}
