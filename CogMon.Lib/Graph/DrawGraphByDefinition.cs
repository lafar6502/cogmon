using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Graph
{

    public class DrawOptions
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        /// <summary>
        /// List of graph elements to be skipped
        /// </summary>
        public List<int> SkipElements { get; set; }
        /// <summary>
        /// null - use default event categories from the graph definition
        /// empty list - draw no events
        /// single element list with an empty entry - use default event categories
        /// </summary>
        public List<string> EventCategories { get; set; }
        /// <summary>
        /// Graph resolution, in seconds
        /// Optional, if not specified RRD will select a resolution
        /// </summary>
        public int? Step { get; set; }
    }

    public class DrawGraphByDefinition
    {
        public GraphDefinition Definition { get; set; }
        public string DestinationFile { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// RRD graph image information
    /// </summary>
    public class RrdImageInfo
    {
        public class ImgEventInfo
        {
            public int Time { get; set; }
            public int GraphX { get; set; }
            public string EventId { get; set; }
            public string Text { get; set; }
            public string Category { get; set; }
        }

        public string FileName { get; set; }
        public int GraphStartTime { get; set; }
        public int GraphEndTime { get; set; }
        public int GraphTop { get; set; }
        public int GraphLeft { get; set; }
        public int GraphWidth { get; set; }
        public int GraphHeight { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        /// <summary>
        /// graph data update time, in seconds
        /// </summary>
        public int UpdateSec { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        /// <summary>
        /// List of events with their positions on the graph
        /// </summary>
        public List<ImgEventInfo> Events { get; set; }
    }
}
