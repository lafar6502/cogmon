Ext.define('CogMon.ui.UserTabView', {
    extend: 'Ext.tab.Panel',
	requires: [],
	uses: ['CogMon.ui.RrdGraphPortlet', 'CogMon.ui.DashboardPortalPanel', 'Ext.app.PortalPanel', 'CogMon.ui.DashboardConfigurablePage', 'CogMon.ui.DashboardConfigurablePageWithEvents'],

    //shows a page with given page Id
    //loads it if not visible, otherwise just makes it active
    showPage : function(pageId) {
        
        if (Ext.isEmpty(pageId)) return;
        var fnd = false;
        var me = this;
        this.items.each(function(c) {
            if (!Ext.isEmpty(c.itemId) && c.itemId == pageId) {
                fnd = true;
                me.setActiveTab(c);
                return false;
            }
            return true;
        });
        if (fnd) return;
        RPC.UserGui.GetPortalPageConfig(pageId, {
            success: function(pc, e2) {
                if (e2.status) {
                    me.addUserPage(pc);
                }
            }, 
            failure : function() {
                alert('fail');
            }
        });
    },
    getActivePageId : function() {
        return this.getActiveTab().itemId;
    },
	removePage: function(pageId) {
		this.remove(pageId);
	},
	addUserPage: function(pc) {
		var me = this;
		if (pc.addCls) { //this is an ext component
			if (Ext.isEmpty(pc.itemId)) throw "itemId";
			me.add(pc);
			me.setActiveTab(pc);
		}
		else if (pc.Id) {
			var pg = Ext.create('widget.dashboardconfigurablepagewevents', {
				pageConfig: pc,
				title: pc.Title,
				itemId: pc.Id,
				closable: true
			});
			me.add(pg);
			me.setActiveTab(pg);
		}
		else throw "E1";
	},
	initComponent: function() {
		var me = this;
		Ext.apply(me, {
            frame: false,
            border: false,
            bodyBorder: false,
			items: [
			]
		});
		RPC.UserGui.GetUserPortalPages({
			success: function(ret, e) {
                
				if (e.status) {
					if (ret.length == 0) 
					{
						var pc = {
							Title: 'Nowa strona',
							Config: { },
							Columns: [
								{
									Config: {},
									Portlets: []
								},
                                {
									Config: {},
									Portlets: []
								},
                                {
									Config: {},
									Portlets: []
								}
							]
						};
						//me.addUserPage(pc);
					}
					else 
					{
						for (var i=0;i<ret.length; i++) {
							RPC.UserGui.GetPortalPageConfig(ret[i], {
								success: function(pc, e2) {
									if (e2.status) 
									{
										me.addUserPage(pc);
									}
								}
							});
						}
					}
				}
				else {
					alert('error');
				}
			}
			
		});
		
		this.callParent(arguments);
	}
});
