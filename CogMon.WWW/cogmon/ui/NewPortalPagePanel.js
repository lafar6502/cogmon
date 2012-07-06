// 'new page' panel for adding new page to the dashboard
Ext.define('CogMon.ui.NewPortalPagePanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	uses: ['Ext.window.Window'],
	
    statics: {
        showNewPageWindow : function(callback) {
            var w = Ext.create('Ext.window.Window', {
                modal: true,
                width: 500,
                height: 300,
                title: 'Add new page',
                layout: 'fit',
                autoDestroy: true,
                items: Ext.create('CogMon.ui.NewPortalPagePanel', {itemId: 'thepnl'}),
                buttons: [
                    {
                        text: 'OK', 
                        handler: function() {
							var p = w.getComponent('thepnl');
							if (p.getForm().isValid())
							{
								var pc = p.getNewPageConfig();
								if (!Ext.isEmpty(pc))
								{
									callback(pc);
									w.close();
								}
							}
                        }                        
                    },
                    {text: 'Cancel', handler: function() { w.close(); }}
                ]
            });
            w.show();
        }
    },
	getNewPageConfig: function() {
		if (!this.getForm().isValid()) return null;
		var v = this.getForm().getValues();
		var pc = {
			Title: v.pageTitle,
			Config: {},
			Columns: [
				{
					Portlets: []
				},
				{
					Portlets: []
				},
				{
					Portlets: []
				}
			]
		};
		return pc;
	},
	initComponent: function() {
		var me = this;
        var st = Ext.create('Ext.data.ArrayStore', {
			data: [
				['CogMon.ui.DashboardConfigurablePage', 'Portal page']
			],
			fields: ['className', 'displayName'],
			idProperty: 'className'
		});
        
		Ext.apply(me, {
			padding: 10,
			frame: true,
			defaults: {anchor: '100%'},
            items: [
                {xtype: 'textfield', name: 'pageTitle', fieldLabel: 'Page title', allowBlank: false},
                {xtype: 'combo', name: 'pageType', fieldLabel: 'Select page type', store: st, displayField:'displayName', valueField: 'className', allowBlank: false}
            ]
		});
		this.callParent(arguments);
	}
});
