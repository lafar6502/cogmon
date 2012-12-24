//editor for RRD data source template (CreateRRD
Ext.define('CogMon.admui.RrdDataSourceEditPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	graphDefinition: null,
	definitionId: null,
	uses: ['Ext.window.Window'],
    statics: {
		openEditorWindow: function(cfg) {
			if (Ext.isEmpty(cfg)) cfg = {};
            var w = Ext.create('Ext.window.Window', {
                width: 700, layout: 'fit', modal: true, title: 'Create RRD data source',
                items: Ext.create('CogMon.admui.RrdDataSourceEditPanel', {itemId: 'rrdpanel'
                }),
                buttons: [
                    {text: 'Create', handler: function() {
                        var p = w.down('#rrdpanel');
                        if (!p.validate()) return;
                        var rrd = p.getRrdCreateInfo();
                        RPC.AdminGUI.CreateRRDDataSource(rrd, {
                            success: function(ret, e) {
                                if (e.status) {
                                    console.log(ret);
                                    w.close();
                                }
                                else {
                                    Ext.MessageBox.alert('Error', 'Error creating RRD data source');
                                }
                            }
                        });
                    }},
                    {text: 'Cancel', handler: function() {
                        w.close();
                    }}
                ]
            });
            w.show();
		}
    },
    frame: true,
    validate: function() {
        if (!this.getForm().isValid()) return false;
        return true;
    },
    getRrdCreateInfo: function() {
        var v = this.getForm().getValues();
    },
	initComponent: function() {
		var fieldSt = Ext.create('Ext.data.JsonStore', {
            fields: ['Name', 'Description', 'SeriesType', 'Heartbeat', 'Min', 'Max'], idProperty: 'Name'
        });
        var rraSt = Ext.create('Ext.data.JsonStore', {
            fields: ['CF', 'XFilesFactor', 'AggregateSteps', 'StoredRows']
        });
        var hwSt = Ext.create('Ext.data.JsonStore', {
            fields: ['Op', 'Rows', 'Alpha', 'Beta', 'SeasonalPeriod']
        });
        var cfSt = Ext.create('Ext.data.Store', {
            fields: [{name:'Id', type:'int'}, 'Name'], idProperty: 'Id',
            data: Ext.Array.filter(CogMon.ConstDictionaries.RrdConsolidationFunction, function(r) { return r.Id < 4})
        });
        var cfhwSt = Ext.create('Ext.data.Store', {
            fields: [{name:'Id', type:'int'}, 'Name'], idProperty: 'Id',
            data: Ext.Array.filter(CogMon.ConstDictionaries.RrdConsolidationFunction, function(r) { return r.Id >= 4})
        });
        var ftSt = Ext.create('Ext.data.Store', {
            fields: [{name:'Id', type:'int'}, 'Name'], idProperty: 'Id',
            data: CogMon.ConstDictionaries.RRDFieldType
        });
        
		Ext.apply(this, {
			fieldStore: fieldSt,
			rraStore: rraSt,
			hwStore: hwSt,
			defaults: {anchor: '100%', padding: 5},
			items: [
                {xtype: 'textfield',name: 'Description', allowBlank: false, fieldLabel: 'Description'},
                {xtype: 'textfield', name: 'StartTime', allowBlank: true, fieldLabel: 'RRD start time (optional)'},
                {xtype: 'numberfield', name: 'Step', allowBlank: false, fieldLabel: 'Step (seconds)'},
                {
                    xtype: 'grid', height: 200,
                    store: fieldSt,
                    columns: [
                        {header: 'Name', dataIndex: 'Name', editor: {xtype: 'textfield', allowBlank: false}, width: 140},
                        {header: 'Description', dataIndex: 'Description', flex: 1, editor: {xtype: 'textfield', allowBlank: false}, flex: 1},
                        {header: 'Series type', dataIndex: 'SeriesType', editor: {xtype: 'combobox', store: ftSt, valueField: 'Name', displayField: 'Name'}},
                        {header: 'Heartbeat (sec)', dataIndex: 'Heartbeat', editor: {xtype: 'numberfield', allowBlank: false, minValue: 1}},
                        {header: 'Min value', dataIndex: 'Min', editor: {xtype: 'numberfield', allowBlank: true}},
                        {header: 'Max value', dataIndex: 'Max', editor: {xtype: 'numberfield', allowBlank: true}}
                    ],
                    selType: 'cellmodel',
                    plugins: [
                        Ext.create('Ext.grid.plugin.CellEditing', {clicksToEdit: 1})
                    ],
                    dockedItems: [
                        {xtype: 'toolbar', items: [
                            {xtype: 'tbtext', text: 'Data fields'}, {xtype: 'tbfill'},
                            {text: 'Add', icon: '../Content/img/add.png', handler: function() {
                                fieldSt.add({Name:'', Description: '', Heartbeat: 3600, Min: null, Max: null});
                            }},
                            {text: 'Remove', icon: '../Content/img/delete.png', handler: function() {
                            }}
                        ]}
                    ]
                },
                {
                    xtype: 'grid', height: 160, store: rraSt,
                    columns: [
                        {header: 'Consolidation function', dataIndex: 'CF', editor: {xtype: 'combobox', store: cfSt, valueField: 'Name', displayField: 'Name'}, flex: .25},
                        {header: 'X-files factor', dataIndex: 'XFilesFactor', editor: {xtype: 'numberfield', allowBlank: false, minValue: 0, maxValue: 1.0, step: 0.01}, flex: .25},
                        {header: 'Aggregated steps per row', dataIndex: 'AggregateSteps', editor: {xtype: 'numberfield', allowBlank: false, minValue: 1},flex: .25},
                        {header: 'Total aggregated rows', dataIndex: 'StoredRows', editor: {xtype: 'numberfield', allowBlank: false, minValue: 1}, flex: .25}
                    ],
                    selType: 'cellmodel',
                    plugins: [
                        Ext.create('Ext.grid.plugin.CellEditing', {clicksToEdit: 1})
                    ],
                    dockedItems: [
                        {xtype: 'toolbar', items: [
                            {xtype: 'tbtext', text: 'Data aggregation'}, {xtype: 'tbfill'},
                            {text: 'Add', icon: '../Content/img/add.png', handler: function() {
                                rraSt.add({XFilesFactor: 0.1});
                            }},
                            {text: 'Remove', icon: '../Content/img/delete.png', handler: function() {
                            }}
                        ]}
                    ]
                },
                {
                    xtype: 'grid', height: 160, store: hwSt,
                    columns: [
                        {header: 'Operation', dataIndex: 'Op', editor: {xtype: 'combobox', store: cfhwSt, valueField: 'Name', displayField: 'Name'}, flex: .5},
                        {header: 'Rows', dataIndex: 'Rows', editor: {xtype: 'numberfield', allowBlank: false, minValue: 1}, width: 80},
                        {header: 'Alpha', dataIndex: 'Alpha', editor: {xtype: 'numberfield', allowBlank: false, minValue: 0, step: 0.01}, width: 80},
                        {header: 'Beta', dataIndex: 'Beta', editor: {xtype: 'numberfield', allowBlank: false, minValue: 0, step: 0.01}, width: 80},
                        {header: 'Seasonal period', dataIndex: 'SeasonalPeriod', editor: {xtype: 'numberfield', allowBlank: false, minValue: 1}, flex: .5}
                    ],
                    selType: 'cellmodel',
                    plugins: [
                        Ext.create('Ext.grid.plugin.CellEditing', {clicksToEdit: 1})
                    ],
                    dockedItems: [
                        {xtype: 'toolbar', items: [
                            {xtype: 'tbtext', text: 'H/W predict and failure detection'}, {xtype: 'tbfill'},
                            {text: 'Add', icon: '../Content/img/add.png', handler: function() {
                                hwSt.add({});
                            }},
                            {text: 'Remove', icon: '../Content/img/delete.png', handler: function() {
                            }}
                        ]}
                    ]
                }
			]
		});
		this.callParent(arguments);
	}
});
