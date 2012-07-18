using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BosonMVC.Services.DirectHandler;
using NLog;
using System.IO;
using System.Security.Principal;
using CogMon.Services.Dao;
using CogMon.Lib.Gui;
using CogMon.Lib.Graph;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using CogMon.Lib;
using CogMon.Lib.DataSeries;

namespace CogMon.Services.Direct
{

    public class treenode
    {
        public string text { get;set;}
        public string cls { get;set;}
        public bool expanded { get;set;}
        public List<treenode> children { get;set;}
        public string id { get; set; }
        public bool leaf { get; set; }
        public string ntype { get; set; }
        

        public treenode()
        {
            
        }
    }
    
    /// <summary>
    /// Ext.Direct implementation of various GUI-related functions
    /// </summary>
    public class UserGui : IDirectAction
    {
        private Logger log = LogManager.GetCurrentClassLogger();

        public MongoDatabase Db { get; set; }
        public IDataSeriesRepository DSRepo { get; set; }

        [DirectMethod]
        public List<treenode> GetUserNavMenu2(string s)
        {
            var user = UserSessionContext.CurrentUserInfo;
            List<treenode> ret = new List<treenode>();
            if (string.IsNullOrEmpty(s) || s == "root")
            {
                ret.Add(new treenode
                {
                    id = "shared",
                    text = "Shared pages",
                    leaf = false,
                    expanded = true,
                    ntype = "folder"
                });
                ret.Add(new treenode
                {
                    id = "my",
                    text = "My pages",
                    leaf = false,
                    expanded = true,
                    ntype = "folder"
                });
                return ret;
            }
            else if (s == "shared")
            {
                var lst = Db.Find<PortalPage>(x => x.ACL.In(user.GetUserACL()) && x.OwnerId != UserSessionContext.CurrentUserRecordId);
                foreach (var pp in lst)
                {
                    ret.Add(new treenode
                    {
                        id = pp.Id,
                        text = pp.Title,
                        leaf = true,
                        ntype = "page"
                    });
                }
                return ret;
                return ret;
            }
            else if (s == "my")
            {
                var lst = Db.Find<PortalPage>(x => x.OwnerId == UserSessionContext.CurrentUserRecordId);
                foreach (var pp in lst)
                {
                    ret.Add(new treenode
                    {
                        id = pp.Id,
                        text = pp.Title,
                        leaf = true,
                        ntype = "page"
                    });
                }
                return ret;
            }
            /*var lst = Db.Find<NavDirectory>(x => x.OwnerId == user.Id || x.OwnerId.In(UserSessionContext.CurrentUserInfo.MemberOf)).ToList();
            foreach (var nd in lst)
            {
                tn.children.Add(ToTreeNode(nd.Root));
            }
            return tn;
             * */
            return ret;
        }

        private treenode ToTreeNode(NavDirectory.Node n)
        {
            var tn = new treenode
            {
                id = n.Id,
                leaf = n.Children == null || n.Children.Count == 0,
                text = n.Label,
                cls = n.CssClass
            };
            if (n.Children != null)
            {
                foreach (var ch in n.Children) tn.children.Add(ToTreeNode(ch));
            }
            return tn;
        }

        [DirectMethod]
        public NavDirectory.Node GetUserNavMenu()
        {
            var nret = new NavDirectory.Node { Id = null, Label = "rroot" };
            
            var user = UserSessionContext.CurrentUserInfo;
            var lst = Db.Find<NavDirectory>(x => x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).ToList();
            if (lst.Count == 0)
            {
                NavDirectory n = new NavDirectory { OwnerId = user.Id, Root = new NavDirectory.Node { Id = "root", Label = "root" } };
                Db.GetCollection<NavDirectory>().Save(n);
                lst.Add(n);
            }
            lst.ForEach(x => {
                if (nret.Children == null) nret.Children = new List<NavDirectory.Node>();
                nret.Children.Add(new NavDirectory.Node
                {
                    Id = x.Id + "/" + x.Root.Id,
                    Label = x.Root.Label,
                    Ref = x.Root.Ref,
                    RefType = x.Root.RefType,
                    Children = x.Root.Children
                });
            });
            return nret;
        }

        [DirectMethod]
        public IEnumerable<string> GetUserPortalPages()
        {
            var usr = Db.GetCollection<UserInfo>().FindOneById(UserSessionContext.CurrentUserInfo.Id);

            var l = Db.Find<PortalPage>(x => x.ACL.In(usr.GetUserACL()) && x._id.In(usr.PinnedPages));
            
            //var l = Db.GetCollection<PortalPage>().Find(Query.EQ("OwnerId", UserSessionContext.CurrentUserId));
            var ret = l.Select(x => x.Id).ToList();
            return ret;
        }

        [DirectMethod]
        public void SetPagePinned(string pageId, bool pin)
        {
            var usr = Db.GetCollection<UserInfo>().FindOneById(UserSessionContext.CurrentUserInfo.Id);
            if (pin)
            {
                usr.PinPage(pageId);
            }
            else
            {
                usr.UnpinPage(pageId);
            }
            Db.GetCollection<UserInfo>().Save(usr);
        }

        [DirectMethod]
        public PortalPage SavePage(PortalPage pp)
        {
            if (!string.IsNullOrEmpty(pp.OwnerId) && pp.OwnerId != UserSessionContext.CurrentUserRecordId)
                throw new Exception("Not allowed to modify this page");
            if (string.IsNullOrEmpty(pp.OwnerId)) pp.OwnerId = UserSessionContext.CurrentUserRecordId;
            if (pp.ACL == null) pp.ACL = new List<string>();
            if (!pp.ACL.Contains(UserSessionContext.CurrentUserRecordId)) pp.ACL.Add(UserSessionContext.CurrentUserRecordId);
            Db.GetCollection<PortalPage>().Save(pp);
            return pp;
        }

        [DirectMethod]
        public bool IsPagePinnedByMe(string pageId)
        {
            var usr = Db.GetCollection<UserInfo>().FindOneById(UserSessionContext.CurrentUserInfo.Id);
            return usr.HasPinnedPage(pageId);
        }

        /// <summary>
        /// Returns a list of user's default portal page Ids
        /// so they can be loaded on startup...
        /// </summary>
        /// <returns></returns>
        [DirectMethod]
        public IEnumerable<string> GetUserDefaultPortalPages()
        {
            var l = Db.Find<PortalPage>(x => x.OwnerId.In(new string[] { UserSessionContext.CurrentUserInfo.Id }));

            //var l = Db.GetCollection<PortalPage>().Find(Query.EQ("OwnerId", UserSessionContext.CurrentUserId));
            return l.Select(x => x.Id).ToList();
        }

        [DirectMethod]
        public PortalPage GetPortalPageConfig(string pageId)
        {
            var p = Db.GetCollection<PortalPage>().FindOneById(pageId);
            return UpdatePageBeforeReturning(p);
        }

        protected PortalPage UpdatePageBeforeReturning(PortalPage p)
        {
            p.Editable = PageEditAllowed(p);
            return p;
        }

        protected bool PageEditAllowed(PortalPage pp)
        {
            return pp.OwnerId == UserSessionContext.CurrentUserRecordId;
        }

        [DirectMethod]
        public PortalPage AddNewUserPortalPage(PortalPage pp)
        {
            pp.Id = null;
            pp.OwnerId = UserSessionContext.CurrentUserRecordId;
            pp.ACL.Add(UserSessionContext.CurrentUserRecordId);
            Db.GetCollection<PortalPage>().Insert(pp);
            return UpdatePageBeforeReturning(pp) ;
        }

        [DirectMethod]
        public PortalPage CreateNewUserPage(string pageTitle, string clonePageId)
        {
            var pp = new PortalPage();
            pp.Title = pageTitle;
            Db.GetCollection<PortalPage>().Insert(pp);
            return UpdatePageBeforeReturning(pp);
        }

        [DirectMethod]
        public void DeleteUserPortalPage(string pageId)
        {
            
            var r = Db.GetCollection<PortalPage>().Remove(Query.And(Query.EQ("_id", pageId), Query.EQ("OwnerId", UserSessionContext.CurrentUserRecordId)));
            
            if (r.DocumentsAffected == 0)
            {
                throw new Exception("Not allowed to delete this page");
                log.Info("Page was not removed: {0}", pageId);
            }
        }

        /// <summary>
        /// returns a list of rrd graph portlets for current user
        /// </summary>
        /// <returns></returns>
        [DirectMethod]
        public IList<Portlet> GetGraphPortletList()
        {
            List<Portlet> ret = new List<Portlet>();
            var lst = Db.Find<GraphDefinition>(x => x.OwnerId == UserSessionContext.CurrentUserRecordId || x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).ToList();
            foreach (var gd in lst)
            {
                Portlet p = new Portlet();
                p.Id = gd.Id;
                p.Title = gd.Title;
                p.Config = new Dictionary<string, object>();
                p.Config["editable"] = gd.OwnerId == UserSessionContext.CurrentUserRecordId;
                p.Config["graphDefinitionId"] = gd.Id;
                p.Config["height"] = 300;
                p.PortletClass = "CogMon.ui.RrdGraphPortlet";
                ret.Add(p);
            }
            var lst2 = Db.Find<PortletDef>(x => x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).SetSortOrder("Title");
            foreach (var pd in lst2)
            {
                ret.Add(new Portlet { Id = pd.Id, PortletClass = pd.PortletClass, Title = pd.Title, Config = Util.JsonUtil.NormalizeJsonObject(pd.Config) });
            }
            return ret;
        }

        [DirectMethod]
        public IList<Portlet> SearchAvailablePortlets(string query, int? start, int? limit)
        {
            List<Portlet> ret = new List<Portlet>();
            var lst = Db.FindAll<GraphDefinition>().ToList();
            foreach (var gd in lst)
            {
                Portlet p = new Portlet();
                p.Id = gd.Id;
                p.Title = gd.Title;
                p.Config = new Dictionary<string, object>();
                p.Config["graphDefinitionId"] = gd.Id;
                p.Config["height"] = 300;
                p.PortletClass = "CogMon.ui.RrdGraphPortlet";
                ret.Add(p);
            }
            return ret;
        }

        public class PortalPageConfig
        {
            public int? Columns { get;set;}
        }

        [DirectMethod]
        public PortalPage ChangePageConfig(string pageId, PortalPageConfig updatedConfig)
        {
            var p = Db.GetCollection<PortalPage>().FindOneById(pageId);
            if (!PageEditAllowed(p)) throw new Exception("Page not editable");
            if (updatedConfig.Columns.HasValue)
            {
                if (p.Columns.Count < updatedConfig.Columns.Value)
                {

                }
                else if (p.Columns.Count > updatedConfig.Columns.Value)
                {
                }
            }
            return UpdatePageBeforeReturning(p);
        }

        [DirectMethod]
        public PortalPage RemovePortletFromPage(string pageId, string portletId)
        {
            var p = Db.GetCollection<PortalPage>().FindOneById(pageId);
            if (!PageEditAllowed(p)) throw new Exception("Page not editable");
            bool removed = false;
            foreach (var pc in p.Columns)
            {
                if (pc.Portlets != null && pc.Portlets.RemoveAll(x => x.Id == portletId) > 0)
                {
                    removed = true;
                    break;
                }
            }
            if (removed) Db.GetCollection<PortalPage>().Save(p);
            return UpdatePageBeforeReturning(p);
        }

        [DirectMethod]
        public PortalPage UpdatePortletConfig(string pageId, string portletId, Dictionary<string, object> config)
        {
            var p = Db.GetCollection<PortalPage>().FindOneById(pageId);
            foreach (var pc in p.Columns)
            {
                if (pc.Portlets == null) continue;
                int idx = pc.Portlets.FindIndex(x => x.Id == portletId);
                if (idx < 0) continue;
                var por = pc.Portlets[idx];
                por.Config = Util.JsonUtil.NormalizeJsonObject(config);
                Db.GetCollection<PortalPage>().Save(p);
                return p;
            }
            throw new Exception("Portlet not found: " + portletId);
        }

        [DirectMethod]
        public PortalPage PortletMovedOnPage(string pageId, string portletId, int newColumnIndex, int newPosition)
        {
            var p = Db.GetCollection<PortalPage>().FindOneById(pageId);
            if (!PageEditAllowed(p)) throw new Exception("Page not editable");
            bool modified = false;
            foreach (var pc in p.Columns)
            {
                if (pc.Portlets == null) continue;
                int idx = pc.Portlets.FindIndex(x => x.Id == portletId);
                if (idx < 0) continue;
                var por = pc.Portlets[idx];
                pc.Portlets.RemoveAt(idx);
                var ncol = p.Columns[newColumnIndex];
                if (ncol.Portlets == null) ncol.Portlets = new List<Portlet>();
                ncol.Portlets.Insert(newPosition, por);
                modified = true;
            }
            if (modified) Db.GetCollection<PortalPage>().Save(p);
            return UpdatePageBeforeReturning(p);
        }
        

        [DirectMethod]
        public PortalPage AddNewPortletToPage(string pageId, int column, Portlet portlet)
        {
            portlet.Config = Util.JsonUtil.NormalizeJsonObject(portlet.Config);
            var p = Db.GetCollection<PortalPage>().FindOneById(pageId);
            if (!PageEditAllowed(p)) throw new Exception("Page not editable");
            if (p.Columns.Count == 0)
            {
                p.Columns.Add(new PortalPageColumn { });
            }
            PortalPageColumn pc = p.Columns[0];
            if (column == -1)
            {
                foreach (var pcol in p.Columns)
                {
                    if (pcol.Portlets.Count < pc.Portlets.Count)
                    {
                        pc = pcol;
                    }
                }
            }
            else
            {
                pc = p.Columns[column];
            }
            portlet.Id = (p.IdGen++).ToString();
            pc.Portlets.Add(portlet);
            Db.GetCollection<PortalPage>().Save(p);
            return UpdatePageBeforeReturning(p);
        }

        [DirectMethod]
        public UserInfo GetUserInfo()
        {
            var ui = UserSessionContext.CurrentUserInfo;
            ui.Passwd = null;
            return ui;
        }

        [DirectMethod]
        public string AddEvent(EventInfo ei)
        {
            ei.Id = null;
            ei.Owner = new Lib.IdLabel { Id = UserSessionContext.CurrentUserInfo.Id, Name = UserSessionContext.CurrentUserInfo.Name };
            if (ei.Category == null) throw new Exception("Can't save an event without category");
            Db.GetCollection<EventInfo>().Save(ei);
            return ei.Id;
        }

        [DirectMethod]
        public IList<EventCategory> GetAllEventCategories()
        {
            var d = CogmonDbUtil.GetAllEventCategories(this.Db);
            return d.Values.OrderBy(x => x.Name).ToList();
        }

        [DirectMethod]
        public IList<IdLabel> GetGroups()
        {
            var l = new List<IdLabel>();
            foreach (var gi in Db.GetCollection<GroupInfo>().FindAll())
            {
                l.Add(new IdLabel { Id= gi.Id, Name = gi.Name });
            }
            return l;
        }
        

        [DirectMethod]
        public IList<object> GetEventsBetween(string start, string end)
        {
            DateTime d1, d2;
            RRD.RrdUtil.ParseRrdDateRange(start, end, out d1, out d2);
            List<object> ret = new List<object>();
            var cats = CogmonDbUtil.GetAllEventCategories(Db);
            foreach (var ev in Db.Find<EventInfo>(x => x.Timestamp > d1 && x.Timestamp < d2).OrderBy(x => x.Timestamp))
            {
                EventCategory cat;
                cats.TryGetValue(ev.Category, out cat);
                ret.Add(new
                {
                    Id = ev.Id,
                    Text = ev.Label,
                    CategoryId = ev.Category,
                    CategoryName = cat != null ? cat.Name : ev.Category,
                    Timestamp = ev.Timestamp,
                    Color = cat != null ? cat.Color : "E03020A0"
                });
            }
            return ret;
        }

        [DirectMethod]
        public IList<object> GetDataSourcesWithVariables(string query)
        {
            throw new NotImplementedException();
        }

        [DirectMethod]
        public IList<DataSourceRef> GetRrdDataSources()
        {
            return DSRepo.DataSources.ToList();
        }

        [DirectMethod]
        public IList<object> GetRrdGraphsVisibleToMe()
        {
            var ret = new List<object>();
            var lst = Db.Find<GraphDefinition>(x => x.OwnerId == UserSessionContext.CurrentUserRecordId || x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).ToList();
            foreach (var gd in lst)
            {
                ret.Add(new
                {
                    Id = gd.Id,
                    Title = gd.Title,
                    OwnerId = gd.OwnerId,
                    OwnerName = gd.OwnerName,
                    TemplateId = gd.TemplateId,
                    IsMine = gd.OwnerId == UserSessionContext.CurrentUserRecordId
                });
            }
            return ret;
        }

        [DirectMethod]
        public IList<IdLabel> GetRrdDataSourceFields(string dataSourceId)
        {
            var ds = DSRepo.GetDataSeries(dataSourceId, false);
            return ds.Fields.Select(x => new IdLabel { Id = x.Name, Name = string.IsNullOrEmpty(x.Description) ? x.Name : x.Description }).ToList();
        }

        [DirectMethod]
        public IList<object> GetGraphElements(string definitionId)
        {
            if (string.IsNullOrEmpty(definitionId)) throw new Exception("definitionId missing");
            var gd = Db.GetCollection<GraphDefinition>().FindOneById(definitionId);
            if (gd == null) throw new Exception("Graph not found: " + definitionId);
            List<object> ret = new List<object>();
            for (int i = 0; i < gd.Elements.Count; i++)
            {
                var ge = gd.Elements[i];
                ret.Add(new
                {
                    Idx = i,
                    Color = ge.Color,
                    Op = ge.Op.ToString(),
                    Label = string.Format("{0}:{1} {2} {3}", i, ge.Op, ge.Value, ge.Legend)
                });
            }
            return ret;
        }

        [DirectMethod]
        public GraphDefinition GetGraphDefinition(string definitionId)
        {
            return Db.GetCollection<GraphDefinition>().FindOneById(definitionId);
        }

        [DirectMethod]
        public GraphDefinition SaveGraphDefinition(GraphDefinition gd)
        {
            if (string.IsNullOrEmpty(gd.Id))
            {
                gd.OwnerId = UserSessionContext.CurrentUserRecordId;
                Db.GetCollection<GraphDefinition>().Save(gd);
            }
            else
            {
                var gd2 = Db.GetCollection<GraphDefinition>().FindOneById(gd.Id);
                if (gd2.OwnerId != UserSessionContext.CurrentUserRecordId) throw new Exception("You can't modify this graph");
                gd.OwnerId = UserSessionContext.CurrentUserRecordId;
                Db.GetCollection<GraphDefinition>().Save(gd);
            }
            return gd;
        }

        [DirectMethod]
        public void UpdateUserPreferences(Dictionary<string, object> d)
        {
            var usr = Db.GetCollection<UserInfo>().FindOneById(UserSessionContext.CurrentUserInfo.Id);
            if (usr.Preferences == null) usr.Preferences = new Dictionary<string, object>();
            foreach (var k in d.Keys)
            {
                usr.Preferences.Remove(k);
                usr.Preferences[k] = d[k];
            }
            Db.GetCollection<UserInfo>().Save(usr);
        }

        protected treenode ToTreeNode(NavNode nn)
        {
            var tn = new treenode
            {
                id = nn.Id,
                text = nn.Name,
                cls = nn.NodeClass,
                children = new List<treenode>(),
                leaf = false,
                ntype = "folder",
                expanded = true
            };
            if (nn.Items != null && nn.Items.Count > 0)
            {
                tn.leaf = false;
                foreach(var r in nn.Items)
                {
                    tn.children.Add(new treenode
                    {
                        leaf = true,
                        id = r.Id,
                        text = r.Label,
                        ntype = r.Reftype
                    });
                }
            }
            return tn;
        }

        [DirectMethod]
        public List<treenode> GetUserNavigationMenu()
        {
            var nns = Db.Find<NavNode>(x => x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).OrderBy(x => x.Name);
            Dictionary<string, treenode> d = new Dictionary<string, treenode>();
            Dictionary<string, List<treenode>> childs = new Dictionary<string, List<treenode>>();
            string rootid = ".root";
            ///now build a tree from flat node list
            foreach (var nn in nns)
            {
                var tn = ToTreeNode(nn);
                d[tn.id] = tn;
                string rid = string.IsNullOrEmpty(nn.ParentId) ? rootid : nn.ParentId;
                List<treenode> chl;
                if (!childs.TryGetValue(rid, out chl)) 
                {
                    chl = new List<treenode>();
                    childs[rid] = chl;
                }
                chl.Add(tn);
            }
            List<string> rmv = new List<string>();
            foreach (var k in childs.Keys)
            {
                if (k == rootid) continue;
                treenode tn;
                if (!d.TryGetValue(k, out tn))
                {
                    rmv.Add(k);
                }
                else
                {
                    tn.children.AddRange(childs[k]);
                    tn.leaf = false;
                }
            }
            var rtl = childs.ContainsKey(rootid) ? childs[rootid] : new List<treenode>();
            if (rtl.Count > 0)
            {
                var pl = Db.Find<PortalPage>(x => x.ACL.In(UserSessionContext.CurrentUserInfo.GetUserACL())).SetFields("_id", "Title", "OwnerId", "FolderId");
                foreach (var pp in pl)
                {
                    treenode tn;
                    if (string.IsNullOrEmpty(pp.FolderId))
                        tn = rtl.Last();
                    else 
                        if (!d.TryGetValue(pp.FolderId, out tn)) continue;
                    tn.children.Add(new treenode
                    {
                        id = pp.Id,
                        text = pp.Title,
                        leaf = true,
                        ntype = "page"
                    });
                }
            }
            return rtl;
        }

        /// <summary>
        /// Create navigation folder assigned to current user
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        [DirectMethod]
        public void CreateNavigationFolder(string parentId, string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name cannot be empty");
            if (!string.IsNullOrEmpty(parentId))
            {
                var pn = Db.GetCollection<NavNode>().FindOneById(parentId);
                if (pn == null) throw new Exception("Invalid parent Id");
                if (pn.OwnerId != UserSessionContext.CurrentUserRecordId) throw new Exception("You have no permission to create folders here");
            }
            var nn = new NavNode();
            nn.ParentId = parentId;
            nn.Name = name;
            nn.OwnerId = UserSessionContext.CurrentUserRecordId;
            nn.ACL = new List<string>();
            nn.ACL.Add("ALL");
            nn.Items = new List<NavNode.Ref>();
            nn.NodeClass = "folder";
            Db.GetCollection<NavNode>().Save(nn);
        }

        [DirectMethod]
        public void DeleteNavigationFolder(string id)
        {
            var nn = Db.GetCollection<NavNode>().FindOneById(id);
            if (nn == null) throw new ArgumentException("folder not found");
            if (nn.OwnerId != UserSessionContext.CurrentUserRecordId) throw new Exception("Not allowed to delete this folder");
            if (Db.Find<NavNode>(x => x.ParentId == id).Count() > 0 ||
                Db.Find<PortalPage>(x => x.FolderId == id).Count() > 0)
                throw new Exception("This folder contains child folders or portal pages and cannot be deleted");
            Db.GetCollection<NavNode>().Remove(Query.EQ("_id", id));
        }

        /// <summary>
        /// Moves a folder or a folder item
        /// TODO: add permission control
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="parentFolderId"></param>
        /// <param name="newParentFolderId"></param>
        [DirectMethod]
        public void MoveNavigationItem(string itemId, string itemType, string parentFolderId, string newParentFolderId)
        {
            var df = Db.GetCollection<NavNode>().FindOneById(newParentFolderId);
            if (df == null) throw new Exception("Destination folder not found");
            
            if (itemType == "folder")
            {
                var nn = Db.GetCollection<NavNode>().FindOneById(itemId);
                if (nn == null) throw new Exception("Folder not found");
                if (nn.OwnerId != UserSessionContext.CurrentUserRecordId) throw new Exception("Not allowed");
                nn.ParentId = df.Id;
                Db.GetCollection<NavNode>().Save(nn);
            }
            else if (itemType == "page")
            {
                var pp = Db.GetCollection<PortalPage>().FindOneById(itemId);
                if (pp == null) throw new Exception("Page not found");
                if (pp.OwnerId != UserSessionContext.CurrentUserRecordId) throw new Exception("Not allowed");
                pp.FolderId = df.Id;
                Db.GetCollection<PortalPage>().Save(pp);
            }
            else throw new Exception("item type");
        }

        
    }
}
