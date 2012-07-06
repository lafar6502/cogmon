Ext.define('CogMon.controller.Main', {
    extend: 'Ext.app.Controller',
	requires: [
		
	],
	refs: [
        {ref: 'navPnl', selector: 'navpanel'},
        {ref: 'contPnl',    selector: 'contentpanel'}
    ],
	models: [
		
	],
	init: function() {
		var me = this;
		this.control('navpanel', {
			navactionselected : function(pn, actName, act) {
				console.log("NAV Action: " + actName);
				console.log("loading View: " + act.cfg);
				me.getContPnl().loadView(act.cfg.viewName, {});
			}
		});
	},
	onLaunch: function() {
		console.log("launch");
		console.log("navPnl: " + this.getNavPnl());
		console.log("contPnl: " + this.contPnl);
	}
});