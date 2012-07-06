using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Gui
{
   

    public class EventTag
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }
    
    public class ApplicationSettings
    {
        public string Id { get; set; }
        public List<EventTag> EventTags { get; set; }
        public List<IdLabel> Groups { get; set; }

    }
}
