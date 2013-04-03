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
				width: 800,
				title: 'RRD Graph Editor',
				items: pnl,
				layout: 'fit',
				minHeight: 400,
				height: 600,
				buttons: [
					{
						text: 'Clone graph', iconCls: 'btn-clone',
						handler: function() {
							var v = pnl.getCurrentGraphDefinition();
							Ext.apply(v, {Id: null, ACL: [], Title: ''});
							pnl.loadGraphDefinition(v);
							Ext.Msg.alert('Success', 'Graph definition has been cloned but not saved yet');
						}
					},
					{
						text: 'Save',
						handler: function() {
							var v = pnl.getCurrentGraphDefinition();
							console.log("graph: " + Ext.encode(v));
							RPC.UserGui.SaveGraphDefinition(v, {
								success: function(ret, e) {
									if (e.status) {
										pnl.loadGraphDefinition(ret);
										Ext.Msg.alert('Success', 'Graph saved');
									}
									else {
										alert('error');
									}
								}
							});
						}
					},
					{
						text: 'Cancel',
						handler: function() { w.close(); }
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
			//console.log('loading: ' + Ext.encode(gd));
			me.graphDefinitionId = gd.Id;
			me.defStore.loadData(gd.Defs);
			me.cdefStore.loadData(gd.CVDefs);
			me.elementStore.loadData(gd.Elements);
			me.resolutionStore.loadData(Ext.isEmpty(gd.Resolution) ? {} : gd.Resolution);
			me.getForm().setValues(gd);
		}
		else throw "invalid graph definition"
	},
	getCurrentGraphDefinition: function() {
		var v = this.getForm().getValues();
        if (Ext.isEmpty(v.EventCategories)) v.EventCategories = [];
        if (Ext.isEmpty(v.ACL)) v.ACL = [];
		v.Id = this.graphDefinitionId;
		v.Defs = Ext.Array.map(this.defStore.getRange(), function(x) { return x.data; });
		v.CVDefs = Ext.Array.map(this.cdefStore.getRange(), function(x) { return x.data; });
		v.Elements = Ext.Array.map(this.elementStore.getRange(), function(x) { return x.data; });
		v.Resolution = Ext.Array.map(this.resolutionStore.getRange(), function(x) { return x.data; });
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
    runDefaultDSGraphGenerator: function() {
        var me = this;
        var dataSrcSt = Ext.create('Ext.data.DirectStore', {
			directFn: RPC.UserGui.GetRrdDataSources,
			fields: ['Id', 'Description'], idProperty: 'Id', autoDestroy: true, autoLoad: true
		});
        var w = Ext.create('Ext.window.Window', {
            modal: true, layout: 'fit', title: 'Auto-create graph definition for RRD', 
            items: {
                xtype: 'form', frame: false, fieldDefaults: {labelAlign: 'top'}, padding: 5, border: false, itemId: 'theForm',
                items: [
                    {
                        name: 'dsId', xtype: 'combobox', width: 260, store: dataSrcSt, valueField: 'Id', displayField: 'Description', allowBlank: false, queryMode: 'local', typeAhead: true, minChars: 2, fieldLabel: 'Select data source (RRD)'
                    }
                ]
            },
            buttons: [
                {text: 'Ok', handler:function() {
                    var v = w.down('#theForm').getForm().getValues();
                    if (!Ext.isEmpty(v.dsId)) {
                        RPC.UserGui.GetDefaultGraphDefinitionForDataSource(v.dsId, {
                            success: function(ret, e) {
                                if (e.status) {
                                    me.loadGraphDefinition(ret);
                                    Ext.Msg.alert('Success', 'Graph definition updated');
                                }
                                else {
                                    Ext.Msg.alert('Error', 'Graph definition not updated');
                                }
                            }
                        });
                    }
                    w.close();
                }},
                {text: 'Cancel', handler: function() {
                    w.close();
                }}
            ]
        });
        w.show();
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
		var resSt = Ext.create('Ext.data.JsonStore', {
			fields: [{name: 'ResSec', type: 'int'}, {name:'SpanSec', type: 'int'}], autoDestroy: true
		});
		var defCdefSt = Ext.create('Ext.data.ArrayStore', {fields: [{name:'v', type: 'boolean'}, 'name'], data: [[true, 'CDEF'], [false, 'VDEF']], idProperty: 'v'});
		var cfst = Ext.create('Ext.data.Store', {fields:['Id', 'Name'], data: CogMon.ConstDictionaries.RrdConsolidationFunction, autoDestroy: true});
		var cfst2 = Ext.create('Ext.data.Store', {fields:['Id', 'Name'], data: CogMon.ConstDictionaries.RrdConsolidationFunction, autoDestroy: true});
		var gvariableStore = Ext.create('Ext.data.JsonStore', {fields: ['variable', 'type', 'label'], data: [], idProperty: 'variable'});
        var updateVariableStore = function() {
            vars = [];
            defSt.each(function(r) {
                if (Ext.isEmpty(r.data.Variable)) return;
                vars.push({variable: r.data.Variable, type: 'DEF', label: r.data.Variable + ' (DEF)'});
            });
            cdefSt.each(function(r) {
                if (Ext.isEmpty(r.data.Variable)) return;
                vars.push({variable: r.data.Variable, type: r.data.CDEF ? 'CDEF' : 'VDEF', label: r.data.Variable + (r.data.CDEF ? ' (CDEF)' : ' (VDEF)')});
            });
            gvariableStore.loadData(vars, false);
        };
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
				}, 
                blur: function() {
                    dataSrcSt.clearFilter();
                }
			}
		};
		Ext.apply(this, {
			defStore: defSt,
			cdefStore: cdefSt,
			elementStore: elemSt,
			resolutionStore: resSt,
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
											iconCls: 'btn-add',text: 'Add',scope: this, 
											handler: function() {
												me.defStore.add({});
											}
										}, 
										{
											iconCls: 'btn-delete', text: 'Delete', itemId: 'delete',  scope: this,
											handler: function() {
												var sm = me.down('#defsGrid').getSelectionModel();
												if (sm.hasSelection()) {
													me.defStore.remove(sm.getSelection());
												}
											}
										},
										{
											text: 'Clone', itemId: 'clone_btn', scope: this, iconCls: 'btn-clone',
											handler: function() {
												var sm = me.down('#defsGrid').getSelectionModel();
												if (sm.hasSelection()) {
													var v = sm.getSelection();
													me.defStore.add(Ext.decode(Ext.encode(v[0].data)));
												}
											}
										},
                                        {
                                            text: 'Auto-create graph', itemId: 'auto_graph_btn', scope: this, 
                                            handler: function() {
                                                me.runDefaultDSGraphGenerator();
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
						],
                        listeners: {
                            deactivate: function() { 
                                updateVariableStore();
                            }
                        }
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
									iconCls: 'btn-add',text: 'Add',scope: this, 
									handler: function() {
										me.cdefStore.add({});
									}
								}, 
								{
									iconCls: 'btn-delete', text: 'Delete', itemId: 'delete',  scope: this,
									handler: function() {
										var sm = me.down('#cdefsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											me.cdefStore.remove(sm.getSelection());
										}
									}
								},
								{
									text: 'Clone', itemId: 'clone_btn', iconCls: 'btn-clone', scope: this,
									handler: function() {
										var sm = me.down('#cdefsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											var v = sm.getSelection();
											me.cdefStore.add(Ext.decode(Ext.encode(v[0].data)));
										}
									}
								}
							]
						}],
                        listeners: {
                            deactivate: function() { 
                                updateVariableStore();
                            }
                        }
					},
					{
						title: 'Graph elements',  
						xtype: 'gridpanel', itemId: 'elementsGrid', autoScroll: true,
						columns: [
							{
								dataIndex: 'Op', header: 'Element type', sortable: false, 
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
							{
                                dataIndex: 'Value', header: 'Value', 
                                editor: {xtype: 'combobox', store: gvariableStore, allowBlank: false, triggerAction: 'all', queryMode: 'local', valueField: 'variable', displayField: 'label'}, 
                                sortable: false},
							{
								dataIndex: 'Color', header: 'Color', editor: 'textfield', sortable: false,
								renderer: function(v, m) {
									if (Ext.isString(v) && v.length >= 6)
									{
										//console.log('style: ' + Ext.encode(m.style));
										m.style = 'border-bottom: solid 2px #' + v.substring(0,6);
									}
									return v;
								}
							},
							{dataIndex: 'Legend', header: 'Legend', flex: .5, sortable: false, editor: 'textfield'},
							{dataIndex: 'Params', header: 'Parameters', flex: .5, sortable: false, editor: 'textfield'}
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
									iconCls: 'btn-add',text: 'Add',scope: this, 
									handler: function() {
										me.elementStore.add({});
									}
								}, 
								{
									iconCls: 'btn-delete', text: 'Delete', itemId: 'delete',  scope: this,
									handler: function() {
										var sm = me.down('#elementsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											me.elementStore.remove(sm.getSelection());
										}
									}
								},
								{
									text: 'Clone', itemId: 'clone_btn', iconCls: 'btn-clone', scope: this,
									handler: function() {
										var sm = me.down('#elementsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											var v = sm.getSelection();
											me.elementStore.add(Ext.decode(Ext.encode(v[0].data)));
										}
									}
								},
								{
									iconCls: 'btn-up', text: 'Move up', scope: me, 
									handler: function() {
										var sm = me.down('#elementsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											var v = sm.getSelection()[0];
											var rowIndex = me.elementStore.indexOf(v);
											if (rowIndex > 0)
											{
												me.elementStore.removeAt(rowIndex);
												me.elementStore.insert(rowIndex - 1, v);
												sm.select(rowIndex - 1);
											}
											
										}
									}
								},
								{
									iconCls: 'btn-down', text: 'Move down', scope: me, 
									handler: function() {
										var sm = me.down('#elementsGrid').getSelectionModel();
										if (sm.hasSelection()) {
											var v = sm.getSelection()[0];
											var rowIndex = me.elementStore.indexOf(v);
											if (rowIndex < me.elementStore.getCount())
											{
												me.elementStore.removeAt(rowIndex);
												me.elementStore.insert(rowIndex + 1, v);
												sm.select(rowIndex + 1);
											}
											console.log('row index is ' + rowIndex);
										}
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
							{xtype: 'combobox', name: 'ACL', fieldLabel: 'Group permissions', anchor: '100%', store: 'userGroups', valueField: 'Id', displayField: 'Name', multiSelect: true},
							{
								xtype: 'fieldcontainer', fieldLabel: 'Resolution limits', anchor: '100% 100%',
								items: {
									xtype: 'gridpanel', store: resSt, itemId: 'resolutionGrid', 
									columns: [
										{header: 'Resolution (s)', dataIndex: 'ResSec', sortable: false, editor: {xtype: 'numberfield', allowBlank: false}},
										{header: 'Time span (s)', dataIndex: 'SpanSec', sortable: false, editor: {xtype: 'numberfield', allowBlank: false}}
									],
									dockedItems: {
										xtype: 'toolbar',
										items: [
											{
												iconCls: 'btn-add',text: 'Add',scope: this, 
												handler: function() {
													me.resolutionStore.add({});
												}
											}, 
											{
												iconCls: 'btn-delete', text: 'Delete', itemId: 'delete',  scope: this,
												handler: function() {
													var sm = me.down('#resolutionGrid').getSelectionModel();
													if (sm.hasSelection()) {
														me.resolutionStore.remove(sm.getSelection());
													}
												}
											}
										]
									},
									plugins: [
										Ext.create('Ext.grid.plugin.CellEditing', {clicksToEdit: 1})
									]
								}
							}
						]
					},
					{
						title: 'Raw data edit', itemId: 'jsonTab',
						xtype: 'form',
						items: [
							{xtype: 'textareafield', name: 'json', anchor: '100% 100%'}
						],
						dockedItems: {
							xtype: 'toolbar', items: [
								{text: 'Update graph',
									handler: function() {
                                        alert('not implemented');
									}
								}
							]
						},
						listeners: {
							activate: function() {
								this.getForm().reset();
								this.getForm().setValues({'json': Ext.encode(me.getCurrentGraphDefinition())});
							}
						}
						
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
			this.loadGraphDefinition({Defs: [], CVDefs: [], Elements: [], ACL: [], EventCategories: [], Resolution: [], AdditionalCmdParams: '--full-size-mode'});
		}
	}
});
