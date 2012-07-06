//
// displays a bar chart with last values of specified data sources
//
Ext.define('CogMon.ui.RrdLastValuePortlet', {
    extend: 'CogMon.ui.Portlet',
    requires: [],
    autoRefreshInterval: 180,
    setDateRange: function(start, end, suppressNotification) {
    },
	setupConfigPropertyGrid: function(gcfg) {
		gcfg = this.callParent(arguments);
		return Ext.apply(gcfg.source, {
			
		});
	},
	applyUpdatedConfig: function(cfg) {
		this.setHeight(cfg.height);
	},
    initComponent: function() {
        var me = this;
		if (Ext.isEmpty(me.listeners)) me.listeners = {};        
        this.callParent(arguments);
        this.addEvents( 'daterangechanged');
    },
    alias: 'widget.rrdlastvalueportlet'
});