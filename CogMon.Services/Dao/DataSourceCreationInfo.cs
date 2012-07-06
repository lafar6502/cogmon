using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Services.Dao
{
    public class DataSourceCreationInfo 
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public string TemplateId { get; set; }

    }
}
