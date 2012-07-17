Ext.define('CogMon.ui.RrdGraphListPanel', {
	extend: 'Ext.grid.Panel',
	statics: {
		// mode: select
		// add filtering
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
				dockedItems: {
					xtype: 'toolbar',
					dock: 'top',
					items: [
						{xtype: 'button', text: 'Add..', iconCls: 'btn-add',
							handler: function() {
								Ext.require('CogMon.ui.RrdGraphEditorPanel', function() {
									CogMon.ui.RrdGraphEditorPanel['openEditorWindow']();
								});
							}
						},
						{
							xtype: 'button', text: 'Edit..', iconCls: 'btn-edit',
							handler: function() {
								//console.log('item selected: ' + Ext.encode(grph));
								if (!pnl.getSelectionModel().hasSelection()) return;
								var grph = pnl.getSelectionModel().getLastSelected().getData();
								if (!grph.IsMine) return false;
								CogMon.ui.RrdGraphEditorPanel['openEditorWindow']({graphDefinitionId: grph.Id});
							}
						},
						{xtype: 'tbfill'},
						{
							xtype: 'textfield', name:'query', itemId: 'query_fld', fieldLabel: 'Filter', width:250,
							listeners: {
								buffer: 300,
								change: function() {
									console.log('srch: ' + this.getValue());
									var st = w.down('#theGrid').store;
									console.log('st: ' + st);
									st.clearFilter();
									st.filter({property: 'Title', anyMatch: true, value   : this.getValue()});
								}
							}
						}
					]
				},
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
		{dataIndex: 'OwnerName', header: 'Owner'},
		{dataIndex: 'TemplateId', header: 'TemplateId'},
		{dataIndex: 'IsMine', header: 'Owned by me'}
	],
	initComponent: function() {
		var me = this;
		if (Ext.isEmpty(me.store)) {
			me.store = Ext.create('Ext.data.DirectStore', {model: 'CogMon.model.RrdGraphListEntry',directFn: RPC.UserGui.GetRrdGraphsVisibleToMe, autoLoad: true, autoDestroy: true});
		};
		Ext.apply(me, {
		});
		this.callParent(arguments);
	}
});