Ext.define('CogMon.view.Viewport', {
    extend: 'Ext.container.Viewport',
	requires: [
		
	],
	uses: ['Ext.app.PortalPanel', 'Ext.app.PortalColumn', 'Ext.app.Portlet', 'CogMon.ui.UserTabView', 'CogMon.ui.GraphList'],
	layout: 'fit',
	initComponent: function() {
		var me = this;
		
		
		Ext.apply(me, {
            items: Ext.create('CogMon.ui.MainGUIPanel', {id: 'viewport'})
        });
		
		this.callParent(arguments);
	}
});