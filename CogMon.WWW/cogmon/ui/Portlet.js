Ext.define('CogMon.ui.Portlet', {
    extend: 'Ext.app.Portlet',
    requires: [],
    autoRefreshInterval: 180,
    startTime: 'e-1d',
    endTime: 'now',
    eventCategories: "",
    title: 'Graph',
	step: null,
	editable: true,
	getStartTime: function() { return this.startTime; },
    setStartTime: function(v) { this.startTime = v; },
    getEndTime: function() { return this.endTime; },
    setEndTime: function(v) { this.endTime = v; },
    setEventCategories: function(v) {this.eventCategories = v; },
    getEventCategories: function() { return this.eventCategories; },
    setDateRange: function(start, end, suppressNotification) {
        this.setStartTime(start);
        this.setEndTime(end);
		if (!suppressNotification) this.fireEvent('daterangechanged', this, start, end);
    },
    setGraphParams: function(start, end, eventCategories) {
        this.setStartTime(start);
        this.setEndTime(end);
        this.setEventCategories(eventCategories);
    },
	setupConfigPropertyGrid: function(gcfg) {
		return Ext.apply(gcfg, {
			"source" : this.getPortletConfig()
		});
	},
	getPortletConfig: function() {
		return {
			height: this.getHeight(),
			step: this.step
		};
	},
	applyUpdatedConfig: function(cfg) {
		this.setHeight(cfg.height);
		this.fireEvent('configchanged', this, cfg);
	},
	configPropertyNames: {
	},
	showConfigEditor: function() {
		var me = this;
		var gcfg = {
			width: 300,
			propertyNames: me.configPropertyNames,
			source: {}
		};
		me.setupConfigPropertyGrid(gcfg);
		var propsGrid = Ext.create('Ext.grid.property.Grid', gcfg);
		
		var w = Ext.create('Ext.window.Window', {
			width: 400,
			height: 400,
			layout: 'fit',
			title: 'Settings',
			modal: true,
			items: propsGrid,
			buttons: [
				{
					text: 'OK',
					handler: function() {
						var src = propsGrid.getSource();
						me.applyUpdatedConfig(src);
						w.close();
					}
				},
				{
					text: 'Cancel', handler: function() { w.close(); }
				}
			]
		});
		w.show();
	},
	initComponent: function() {
		this.addEvents('configchanged');
		this.callParent(arguments);
	}
});