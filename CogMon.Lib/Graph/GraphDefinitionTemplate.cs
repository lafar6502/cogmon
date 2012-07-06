using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Graph
{
    public class GraphDefinitionTemplate
    {
        public string Id { get; set; }

        /// <summary>
        /// Template title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Template description
        /// </summary>
        public string Description { get; set; }
        public List<DataSeries.VariableDef> Variables { get; set; }
        public List<string> ACL { get; set; }
        public string JsonTemplate { get; set; }
    }


    /// <summary>
    /// Message for creating graph definition from a template
    /// </summary>
    public class CreateGraphDefinitionFromTemplate
    {
        public string TemplateId { get; set; }
        public Dictionary<string, object> Variables { get; set; }
    }

    
}
