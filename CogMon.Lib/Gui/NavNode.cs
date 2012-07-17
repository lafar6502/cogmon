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
        public string Id { get; set; }
        public string Name { get; set; }
        public List<NavNode> Children { get; set; }
        public List<string> ACL { get; set; }
        public string OwnerId { get; set; }
        public string Ref { get; set; }
        public string NodeClass { get; set; }

        public void AddChild(NavNode nn)
        {
            if (string.IsNullOrEmpty(nn.Id))
            {
                throw new Exception("Id required");
            }
            if (Children.Any(x => x.Id == nn.Id)) throw new Exception("Already there");
            if (Children.Contains(nn)) throw new Exception("Already there");
            if (nn.IsDescendantOfMe(this)) throw new Exception("aaa");
            Children.Add(nn);
        }

        public bool IsDescendantOfMe(NavNode nn)
        {
            if (nn == this) return true;
            if (nn.Id == this.Id) return true;
            if (Children == null) return false;
            foreach (var cn in Children)
            {
                if (cn.IsDescendantOfMe(nn)) return true;
            }
            return false;
        }
    }
}
