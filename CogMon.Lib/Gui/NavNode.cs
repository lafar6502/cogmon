using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Gui
{
    
    /// <summary>
    /// navigation tree folder
    /// </summary>
    public class NavNode
    {
        public class Ref
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public string Reftype { get; set; }
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> ACL { get; set; }
        public string OwnerId { get; set; }
        public string NodeClass { get; set; }
        public List<Ref> Items { get; set; }
        public string ParentId { get; set; }
    }
}
