Ext.define('CogMon.ui.RrdGraphListPanel', {
	extend: 'Ext.grid.Panel',
	statics: {
		openGraphListWindow: function(cfg) {
			var pcfg = { 
				itemId: 'theGrid',
				listeners: {
					itemdblclick: function(v, r, itm, idx) {
						if (!Ext.isEmpty(cfg.itemselected)) {
							if (cfg.itemselected(this, r.getData())) {
								w.close();
							}
						}
					}
				}
			};
			var pnl = Ext.create('CogMon.ui.RrdGraphListPanel', pcfg);
			var w = Ext.create('Ext.window.Window', {
				modal: true, width: 700, height: 500, layout: 'fit',
				items: pnl,
				buttons: [
					{text: 'Cancel', handler: function() { w.close(); }},
					{
						text: 'Ok',
						handler: function() { 
							if (Ext.isEmpty(cfg.itemselected)) w.close();
							var sm = pnl.getSelectionModel();
							if (!sm.hasSelection()) return;
							if (cfg.itemselected(this, sm.getLastSelected().getData())) {
								w.close();
							}
						}
					}
				]
			});
			w.show();
		}
	},
	columns: [
		{dataIndex: 'Id', header: 'Id'},
		{dataIndex: 'Title', header: 'Graph title', flex: 1},
		{dataIndex: 'OwnerId', header: 'Owner'},
		{dataIndex: 'TemplateId', header: 'TemplateId'},
		{dataIndex: 'IsMine', header: 'Owned by me'}
	],
	initComponent: function() {
		var me = this;
		Ext.apply(me, {
			store: 'rrdGraphList'
		});
		this.callParent(arguments);
	}
});