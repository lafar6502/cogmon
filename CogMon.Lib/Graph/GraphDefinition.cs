using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib.DataSeries;

namespace CogMon.Lib.Graph
{
    

    public class DEF
    {
        public string Variable { get; set; }
        public string DataSourceId { get; set; }
        public string Field { get; set; }
        public ConsolidationFunction CF { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public ConsolidationFunction? ReduceCF { get; set; }

        public virtual string ToRRDString()
        {
            return string.Format("DEF:{0}={1}:{2}:{3}", Variable, DataSourceId, Field, CF.ToString());
        }

    }

    /// <summary>
    /// CDEF or VDEF
    /// </summary>
    public class CVDEF
    {
        /// <summary>
        /// true - CDEF, false - VDEF
        /// </summary>
        public bool CDEF { get; set; }
        /// <summary>
        /// Variable name
        /// </summary>
        public string Variable { get; set; }
        /// <summary>
        /// RPN expression
        /// </summary>
        public string Expression { get; set; }

        public virtual string ToRRDString()
        {
            return string.Format("{0}:{1}={2}", CDEF ? "CDEF" : "VDEF", Variable, Expression);
        }
    }

    public enum GraphOperation
    {
        LINE1 = 0,
        LINE2 = 1,
        LINE3 = 2,
        PRINT = 3,
        GPRINT = 4,
        COMMENT = 5,
        VRULE = 6,
        HRULE = 7,
        AREA = 8,
        TICK = 9,
        SHIFT = 10,
        TEXTALIGN = 11
    }

    public class GraphElement
    {
        public GraphOperation Op { get; set; }
        public string Value { get; set; }
        public string Color { get; set; }
        public string Legend { get; set; }
        public string Params { get; set; }
        public bool Hide { get; set; }

        public virtual string ToRRDString()
        {
            
            
            switch (Op)
            {
                case GraphOperation.AREA:
                case GraphOperation.LINE1:
                case GraphOperation.LINE2:
                case GraphOperation.LINE3:
                case GraphOperation.HRULE:
                case GraphOperation.VRULE:
                    return string.Format("\"{0}:{1}{2}{3}{4}\"", Op.ToString(), Value, string.IsNullOrEmpty(Color) ? "" : "#" + Color, string.IsNullOrEmpty(Legend) ? "" : ":" + Legend, string.IsNullOrEmpty(Params) ? "" : ":" + Params);
                case GraphOperation.TEXTALIGN:
                    return string.Format("\"{0}:{1}\"", Op.ToString(), Params);
                case GraphOperation.TICK:
                    return string.Format("\"{0}:{1}#{2}{4}{3}\"", Op.ToString(), Value, Color, string.IsNullOrEmpty(Legend) ? "" : ":" + Legend, string.IsNullOrEmpty(Params) ? "" : ":" + Params);
                case GraphOperation.SHIFT:
                    return string.Format("\"{0}:{1}:{2}\"", Op.ToString(), Value, Params);
                case GraphOperation.COMMENT:
                    return string.Format("\"{0}:{1}{2}\"", Op.ToString(), Legend, Params);
                case GraphOperation.PRINT:
                case GraphOperation.GPRINT:
                    return string.Format("\"{0}:{1}:{2}\"", Op.ToString(), Value, Params);
                default: throw new NotImplementedException();
            }
        }
    }

    public class GraphDefinition
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<DEF> Defs { get; set; }
        public List<CVDEF> CVDefs { get; set; }
        public List<GraphElement> Elements { get; set; }
        /// <summary>
        /// Additional command-line parameters.
        /// It's a template.
        /// </summary>
        public string AdditionalCmdParams { get; set; }
        /// <summary>
        /// List of event categories to show on the graph
        /// </summary>
        public List<string> EventCategories { get; set; }

        public string Description { get; set; }
        public List<string> ACL { get; set; }
        public string OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, object> TemplateVariables { get; set; }

        

        public class ResolutionInfo
        {
            /// <summary>
            /// Max graph resolution, in seconds
            /// </summary>
            public int ResSec { get; set; }
            /// <summary>
            /// max period of time covered with this resolution
            /// </summary>
            public int SpanSec { get; set; }
        }
        /// <summary>
        /// resolution info for calculating graph aggregation
        /// steps. Useful when calculating aggregation step size
        /// </summary>
        public List<ResolutionInfo> Resolution { get; set; }
    }

    
}
