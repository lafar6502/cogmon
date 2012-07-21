using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace CogMon.Lib
{

    /// <summary>
    /// User's account and preferences
    /// </summary>
    public class UserInfo 
    {
        public string Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool NeedsSync { get; set; }
        public string Passwd { get; set; }
        public string ExtId { get; set; }
        public List<string> MemberOf { get; set; }
        public List<string> PinnedPages { get; set; }
        public Dictionary<string, object> Preferences { get; set; }
        public DateTime LastSync { get; set; }

        public UserInfo()
        {
            MemberOf = new List<string>();
            PinnedPages = new List<string>();
            Preferences = new Dictionary<string, object>();
        }

        public IList<string> GetUserACL()
        {
            List<string> ret = new List<string>();
            if (MemberOf != null) ret.AddRange(MemberOf);
            if (!ret.Contains(Id)) ret.Add(Id);
            return ret;
        }

        public bool HasPinnedPage(string pageId)
        {
            return PinnedPages == null ? false : PinnedPages.Contains(pageId);
        }

        public void PinPage(string pageId)
        {
            if (PinnedPages == null) PinnedPages = new List<string>();
            if (!PinnedPages.Contains(pageId)) PinnedPages.Add(pageId);
        }

        public void UnpinPage(string pageId)
        {
            if (PinnedPages == null || !PinnedPages.Contains(pageId)) return;
            PinnedPages.Remove(pageId);
        }
        
    }
}
