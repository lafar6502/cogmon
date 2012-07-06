// 'add portlet' panel for selecting a portlet from a list
Ext.define('CogMon.ui.AddEventPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	uses: ['Ext.window.Window'],
    statics: {
        showAddEventWindow : function(cfg) {
            var w = Ext.create('Ext.window.Window', {
				modal: true,
				layout: 'fit', 
				width: 400,
				height: 340,
				frame: false,
				title: 'Add event',
				items: Ext.create('CogMon.ui.AddEventPanel', {
					border: false, padding: 10, itemId: 'thepnl'
				}),
				buttons: [
					{
						text: 'OK', 
						handler: function() {
							var p = w.down('#thepnl');
							if (!p.getForm().isValid()) return;
							var v = p.getForm().getFieldValues();
							
							var ei = {
								Timestamp: Ext.Date.add(v.tstamp, Ext.Date.MINUTE, v.tstamp_time.getHours() * 60 + v.tstamp_time.getMinutes()),
								Category: v.category,
								Label: v.text
							};
							RPC.UserGui.AddEvent(ei, {
								success: function(ret, e) {
									if (e.status) {
										w.close();
									}
								}
							});
						}
					}
				]
			});
			w.show();
        }
    },
	initComponent: function() {
		var me = this;
        var gd = [];
		
        
		Ext.apply(me, {
			items: [
				{xtype: 'fieldcontainer', fieldLabel: 'Event date', layout: 'hbox', anchor: '100%',
					items: [
						{xtype: 'datefield', name: 'tstamp', allowBlank: false, value: new Date(), flex: 1},
						{xtype: 'timefield', name: 'tstamp_time', allowBlank: false, flex: 0, format: 'H:i'}
					]
				},
				{xtype: 'combobox', name: 'category', allowBlank: false, fieldLabel: 'Category', store: 'eventCategories', anchor: '100%', valueField: 'Id', displayField: 'Name'},
				{xtype: 'textareafield', name: 'text', grow: true, fieldLabel: 'Description', anchor: '100% -4'}
			]
		});
		this.callParent(arguments);
	}
});
