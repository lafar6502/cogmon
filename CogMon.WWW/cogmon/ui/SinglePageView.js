//panel showing only a single portal page
Ext.define('CogMon.ui.SinglePageView', {
    extend: 'Ext.panel.Panel',
	requires: [],
	uses: ['CogMon.ui.RrdGraphPortlet', 'CogMon.ui.DashboardPortalPanel', 'Ext.app.PortalPanel', 'CogMon.ui.DashboardConfigurablePage'],

    //shows a page with given page Id
    //loads it if not visible, otherwise just makes it active
    showPage : function(pageId) {
		var me = this;
        if (Ext.isEmpty(pageId)) return;
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
        
    },
	removePage: function(pageId) {
		
	},
	addUserPage: function(pc) {
		var me = this;
		if (pc.addCls) { //this is an ext component
			if (Ext.isEmpty(pc.itemId)) throw "itemId";
			me.removeAll();
			me.add(pc);
		}
		else if (pc.Id) {
			var pg = Ext.create('widget.dashboardconfigurablepage', {
				pageConfig: pc,
				title: pc.Title,
				itemId: pc.Id,
				closable: true
			});
			me.removeAll();
			me.add(pg);
		}
		else throw "E1";
	},
	initComponent: function() {
		var me = this;
		Ext.apply(me, {
			layout: 'fit',
			autoDestroy: true,
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
