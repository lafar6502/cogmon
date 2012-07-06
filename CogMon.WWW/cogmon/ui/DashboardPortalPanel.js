Ext.define('CogMon.ui.DashboardPortalPanel', {
    extend: 'Ext.app.PortalPanel',
	requires: [],
	uses: ['CogMon.ui.RrdGraphPortlet'],
    alias: 'widget.dashboardportalpanel',
    config: {
        pageDefinition: {},
        pageId: null
    },
    savePage : function() {
    
    },
    //re-loads the portal page from server
    loadPage : function(pageId) {
        RPC.UserGui.GetPortalPageConfig(pageId, {
            success: function(pc, e2) {
                if (e2.status) 
                {
                    me.loadPageConfig(pc);
                }
            },
            failure: function() {
                alert('fail..');
            }
        });
    },
    loadPageConfig : function(pageConfig) {
        var me = this;
		
		
    },
    onPortletRemoved : function(portletId) {
        alert('removed portlet: ' + portletId);
    },
    createPortletComponent: function(portlet) {
        var me = this;
        var id = Ext.isEmpty(portlet.Id) ? Ext.id() : portlet.Id;
        var cfg = {
            title: portlet.Title,
            itemId: id,
            listeners: {
                close: function() {
                    me.onPortletRemoved(id);
                }
            }
        };
        Ext.apply(cfg, portlet.Config);
        return Ext.create(portlet.PortletClass, cfg);
    },
    addPortlet : function(portlet) {
        var p = this.createPortletComponent(portlet);
        var c = this.child('portalcolumn');
        c.add(p);
    },
    
	initComponent: function() {
		var me = this;
		Ext.apply(me, {
		});
        if (!Ext.isEmpty(this.pageConfig)) {
        }
        else if (!Ext.isEmpty(this.pageId)) {
        }
		this.callParent(arguments);
	}
});
