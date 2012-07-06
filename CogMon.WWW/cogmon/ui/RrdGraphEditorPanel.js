// 'add portlet' panel for selecting a portlet from a list
Ext.define('CogMon.ui.RrdGraphEditorPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	graphDefinition: null,
	definitionId: null,
	uses: ['Ext.window.Window'],
    statics: {
		openEditorWindow: function(cfg) {
			if (Ext.isEmpty(cfg)) cfg = {};
			var pcfg = {
				itemId: 'thePnl', border: false, graphDefinitionId: cfg.graphDefinitionId
			};
			var pnl = Ext.create('CogMon.ui.RrdGraphEditorPanel', pcfg);
			var w = Ext.create('Ext.window.Window', {
				modal: true,
				width: 750,
				title: 'RRD Graph Editor',
				items: pnl,
				layout: 'fit',
				minHeight: 400,
				height: 500,
				buttons: [
					{
						text: 'Cancel',
						handler: function() { w.close(); }
					},
					{
						text: 'Ok',
						handler: function() {
							var v = pnl.getCurrentGraphDefinition();
							console.log("graph: " + Ext.encode(v));
							RPC.UserGui.SaveGraphDefinition(v, {
								success: function(ret, e) {
									if (e.status) {
										pnl.loadGraphDefinition(ret);
									}
									else {
										alert('error');
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
	loadGraphDefinition: function(gd) {
		var me = this;
		if (Ext.isString(gd)) {
			RPC.UserGui.GetGraphDefinition(gd, {
				success: function(ret,e) {
					if (e.status)
					{
						me.loadGraphDefinition(ret);
					}
				}
			});
			return;
		}
		else if (Ext.isObject(gd)) {
			console.log('loading: ' + Ext.encode(gd));
			me.graphDefinitionId = gd.Id;
			me.defStore.loadData(gd.Defs);
			me.cdefStore.loadData(gd.CVDefs);
			me.elementStore.loadData(gd.Elements);
			me.getForm().setValues(gd);
		}
		else throw "invalid graph definition"
	},
	getCurrentGraphDefinition: function() {
		var v = this.getForm().getValues();
		v.Id = this.graphDefinitionId;
		v.Defs = this.defStore.getRange().map(function(x) { return x.data; });
		v.CVDefs = this.cdefStore.getRange().map(function(x) { return x.data; });
		v.Elements = this.elementStore.getRange().map(function(x) { return x.data; });
		return v;
	},
	validate: function() {
	},
	showPreview: function() {
		var v = this.getCurrentGraphDefinition();
		var tb = this.down('#previewTab');
		Ext.Ajax.request({
			url: 'Graph/DrawByDef?w=' + tb.getWidth() + '&h=' + tb.getHeight(), method: 'POST',
			params: Ext.encode(v),
			callback: function(opts, success, resp) {
				if (success) {
					console.log('success: ' + resp.responseText);
					var s = Ext.String.format('<img src="Graph/DrawByDefImage/?id={0}"></img>', resp.responseText);
					tb.update(s);
				}
				else {
					tb.update(resp.responseText);
				}
			}
		});
	},
	initComponent: function() {
		var me = this;
		var defSt = Ext.create('Ext.data.JsonStore', {
			model: 'CogMon.model.RrdGraphDEF', autoDestroy: true
		});
		var cdefSt = Ext.create('Ext.data.JsonStore', {
			model: 'CogMon.model.RrdGraphCDEF', autoDestroy: true
		});
		var elemSt = Ext.create('Ext.data.JsonStore', {
			model: 'CogMon.model.RrdGraphElement', autoDestroy: true
		});
		var dataSrcSt = Ext.create('Ext.data.DirectStore', {
			directFn: RPC.UserGui.GetRrdDataSources,
			fields: ['Id', 'Description'], idProperty: 'Id', autoDestroy: true, autoLoad: true
		});
		var dataFieldSt = Ext.create('Ext.data.DirectStore', {
			directFn: RPC.UserGui.GetRrdDataSourceFields,
			fields: ['Id', 'Name'], idProperty: 'Id',
			paramOrder: ['dataSourceId'], autoDestroy: true
		});
		var defCdefSt = Ext.create('Ext.data.ArrayStore', {fields: [{name:'v', type: 'boolean'}, 'name'], data: [[true, 'CDEF'], [false, 'VDEF']], idProperty: 'v'});
		var cfst = Ext.StoreManager.get('rrdConsolidationFunctions');
		var cfst2 = Ext.create('Ext.data.Store', {fields:['Id', 'Name'], data: CogMon.ConstDictionaries.RrdConsolidationFunction, autoDestroy: true});
		
		var dsedit = {
			xtype: 'combobox', store: dataSrcSt, valueField: 'Id', displayField: 'Description', allowBlank: false, queryMode: 'local', typeAhead: true, minChars: 2,
			listeners: {
				buffer: 50,
				change: function() {
				  var store = this.store;
				  store.clearFilter();
				  store.filter({
					  property: 'Description',
					  anyMatch: true,
					  value   : this.getValue()
				  });
				}
			}
		};
		Ext.apply(this, {
			defStore: defSt,
			cdefStore: cdefSt,
			elementStore: elemSt,
			layout: 'fit',
			items: {
				xtype: 'tabpanel', defaults: {xtype: 'panel', border: false}, border: false,
				listeners: {
					tabchange: function(p, nt, ot) {
						if (nt.itemId == 'previewTab') {
							me.showPreview();
						}
					}
				},
				items: [
					{
						title: 'Data sources', xtype:'panel', layout: {type: 'vbox', align: 'stretch'},
						items: [
							{
								xtype: 'fieldset', layout: 'anchor', margin: 2,
								items: [
									{xtype: 'textfield', name: 'Title', fieldLabel: 'Graph Title', anchor: '100%', allowBlank: false},
									{xtype: 'textfield', name: 'Description', fieldLabel: 'Description', anchor: '100%', allowBlank: true}
								]
							},
							
							{
								xtype: 'gridpanel', itemId: 'defsGrid', autoScroll: true, flex: 1,
								columns: [
									{dataIndex: 'DataSourceId', header: 'Data source', editor: dsedit, flex: 1},
									{dataIndex: 'Field', header: 'DS Field', editor: {xtype: 'combobox', store: dataFieldSt, valueField: 'Id', displayField: 'Name', allowBlank: false}},
									{
										dataIndex: 'CF', header: 'Consolidation fn', editor: {xtype: 'combobox', store: cfst, valueField: 'Id', displayField: 'Name', allowBlank: false},
										renderer: function(v, m) {
											var v2 = v;
											Ext.Array.each(CogMon.ConstDictionaries.RrdConsolidationFunction, function(r) {
												if (r.Id == v) v2 = r.Name;
												if (r.Id >= v) return false;
											});
											return v2;
										}
									},
									{dataIndex: 'Variable', header: 'Variable name', editor: {xtype: 'textfield', allowBlank: false}},
									{dataIndex: 'Start', header: 'Start', editor: {xtype: 'textfield', allowBlank: true}, width: 90},
									{dataIndex: 'End', header: 'End', editor: {xtype: 'textfield', allowBlank: true}, width: 90},
									{
										dataIndex: 'ReduceCF', header: 'Reduce fn', editor: {xtype: 'combobox', store: cfst2, valueField: 'Id', displayField: 'Name', allowBlank: true}, width: 90,
										renderer: function(v, m) {
											var v2 = v;
											Ext.Array.each(CogMon.ConstDictionaries.RrdConsolidationFunction, function(r) {
												if (r.Id == v) v2 = r.Name;
												if (r.Id >= v) return false;
											});
											return v2;
										}
									}
								],
								dockedItems: [{
									xtype: 'toolbar',
									items: [
										{
											iconCls: 'icon-add',text: 'Add',scope: this, 
											handler: function() {
												me.defStore.add({});
											}
										}, 
										{
											iconCls: 'icon-delete', text: 'Delete', itemId: 'delete',  scope: this,
											handler: function() {
												var sm = me.down('#defsGrid').getSelectionModel();
												if (sm.hasSelection()) {
													me.defStore.remove(sm.getSelection());
												}
											}
										}
									]
								}],
								store: defSt,
								selType: 'cellmodel',
								plugins: [
									Ext.create('Ext.grid.plugin.CellEditing', {
										clicksToEdit: 1,
										listeners: {
											beforeedit: function(c, e) {
												console.log('field: ' + e.field);
												if (e.field == 'Field')
												{
													dataFieldSt.load({params: {dataSourceId: e.record.get('DataSourceId')}});
												}
											}
										}
									})
								]
							}
						]
					},
					{
						title: 'CDEF/VDEF',  
						xtype: 'gridpanel', itemId: 'cdefsGrid', autoScroll: true, flex: 1,
						columns: [
							{xtype: 'booleancolumn', dataIndex: 'CDEF', header: 'CDEF', editor: {xtype: 'combobox', store: defCdefSt, valueField: 'v', displayField: 'name', allowBlank: false}},
							//{xtype: 'checkcolumn', dataIndex: 'CDEF', header: 'CDEF'},
							{dataIndex: 'Variable', header: 'Variable', editor: {xtype: 'textfield', allowBlank: false}},
							{dataIndex: 'Expression', header: 'RPN Expression', editor: {xtype: 'textfield', allowBlank: false}, flex: 1}
						],
						store: cdefSt,
						plugins: [
							Ext.create('Ext.grid.plugin.CellEditing', {
								clicksToEdit: 1,
								listeners: {
									beforeedit: function(c, e) {
									}
								}
							})
						],
						dockedItems: [{
							xtype: 'toolbar',
							items: [
								{
									iconCls: 'icon-add',text: 'Add',scope: this, 
									handler: function() {
										me.cdefStore.add({});
									}
								}, 
								{
									iconCls: 'icon-delete', text: 'Delete', itemId: 'delete',  scope: this,
									handler: function() {
										var sm = me.down('#cdefsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											me.cdefStore.remove(sm.getSelection());
										}
									}
								}
							]
						}]
					},
					{
						title: 'Graph elements',  
						xtype: 'gridpanel', itemId: 'elementsGrid', autoScroll: true,
						columns: [
							{
								dataIndex: 'Op', header: 'Element type', 
								editor: {xtype: 'combobox', store: 'rrdGraphOperations', valueField: 'Id', displayField: 'Name', allowBlank: false},
								renderer: function(v, m) {
									var v2 = v;
									Ext.Array.each(CogMon.ConstDictionaries.GraphElementType, function(r) {
										if (r.Id == v) v2 = r.Name;
										if (r.Id >= v) return false;
									});
									return v2;
								}
							},
							{dataIndex: 'Value', header: 'Value', editor: 'textfield'},
							{
								dataIndex: 'Color', header: 'Color', editor: 'textfield',
								renderer: function(v, m) {
									if (Ext.isString(v) && v.length >= 6)
									{
										//console.log('style: ' + Ext.encode(m.style));
										m.style = 'border-bottom: solid 2px #' + v.substring(0,6);
									}
									return v;
								}
							},
							{dataIndex: 'Legend', header: 'Legend', flex: .5, editor: 'textfield'},
							{dataIndex: 'Params', header: 'Parameters', flex: .5, editor: 'textfield'}
						],
						store: elemSt,
						plugins: [
							Ext.create('Ext.grid.plugin.CellEditing', {
								clicksToEdit: 1,
								listeners: {
									beforeedit: function(c, e) {
									}
								}
							})
						],
						viewConfig: {
							plugins: {
								ptype: 'gridviewdragdrop', dragGroup: 'elemsGridDD', dropGroup: 'elemsGridDD'
							}
						},
						dockedItems: [{
							xtype: 'toolbar',
							items: [
								{
									iconCls: 'icon-add',text: 'Add',scope: this, 
									handler: function() {
										me.elementStore.add({});
									}
								}, 
								{
									iconCls: 'icon-delete', text: 'Delete', itemId: 'delete',  scope: this,
									handler: function() {
										var sm = me.down('#elementsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											me.elementStore.remove(sm.getSelection());
										}
									}
								},
								{
									iconCls: 'icon-up', text: 'Move up', scope: me, 
									handler: function() {
									}
								},
								{
									iconCls: 'icon-down', text: 'Move down', scope: me, 
									handler: function() {
									}
								}
							]
						}]
					},
					{
						title: 'Permissions, Events', layout: 'form', padding: 3,
						items: [
							{xtype: 'textfield', name: 'AdditionalCmdParams', fieldLabel: 'Additional rrdtool cmdline', anchor: '100%'},
							{xtype: 'combobox', name: 'EventCategories', fieldLabel: 'Event categories', anchor: '100%', store: 'eventCategories', valueField: 'Id', displayField: 'Name', multiSelect: true},
							{xtype: 'combobox', name: 'ACL', fieldLabel: 'Group permissions', anchor: '100%', store: 'userGroups', valueField: 'Id', displayField: 'Name', multiSelect: true}
						]
					},
					{
						title: 'Preview', itemId: 'previewTab',
						html: '', autoScroll: true
					}
				]
			}
		});
		this.callParent(arguments);
		if (!Ext.isEmpty(this.graphDefinitionId)) {
			this.loadGraphDefinition(this.graphDefinitionId);
		} else {
			this.loadGraphDefinition({Defs: [], CVDefs: [], Elements: [], ACL: [], EventCategories: []});
		}
	}
});
