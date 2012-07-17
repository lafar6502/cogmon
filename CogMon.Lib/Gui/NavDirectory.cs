using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CogMon.Lib.Gui
{
    public class NavDirectory
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public List<string> ACL { get; set; }
        public Node Root { get;set;}

        public class Node
        {
            public string Id { get; set; }
            public string Label { get; set; }
            public string Ref { get; set; }
            public string RefType { get; set; }
            public List<Node> Children { get; set; }
            public string CssClass { get; set; }
        }
    }
}
