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
                items: Ext.create('CogMon.admui.RrdDataSourceEditPanel', {
                })
            });
            w.show();
		}
    },
    frame: true,
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
        
		Ext.apply(this, {
			fieldStore: fieldSt,
			rraStore: rraSt,
			hwStore: hwSt,
			defaults: {anchor: '100%', padding: 5},
			items: [
                {xtype: 'textfield',name: 'Description', allowBlank: false, fieldLabel: 'Description'},
                {xtype: 'textfield', name: 'StartTime', allowBlank: true, fieldLabel: 'RRD start time (optional)'},
                {xtype: 'textfield', name: 'Step', allowBlank: false, fieldLabel: 'Step (seconds)'},
                {xtype: 'label', text: 'Data fields'},
                {
                    xtype: 'grid', height: 180,
                    store: fieldSt,
                    columns: [
                        {header: 'Name', dataIndex: 'Name'},
                        {header: 'Description', dataIndex: 'Description', flex: 1},
                        {header: 'Series type', dataIndex: 'SeriesType'},
                        {header: 'Heartbeat (sec)', dataIndex: 'Heartbeat'},
                        {header: 'Min value', dataIndex: 'Min'},
                        {header: 'Max value', dataIndex: 'Max'}
                    ]
                },
                {xtype: 'label', text: 'Data aggregation'},
                {
                    xtype: 'grid', height: 140, store: rraSt,
                    columns: [
                        {header: 'Consolidation function', dataIndex: 'CF'},
                        {header: 'X-files factor', dataIndex: 'XFilesFactor'},
                        {header: 'Steps per row', dataIndex: 'AggregateSteps'},
                        {header: 'Stored aggregated rows', dataIndex: 'StoredRows'}
                    ]
                },
                {xtype: 'label', text: 'Holt-Winters prediction/failure detection'},
                {
                    xtype: 'grid', height: 140, store: rraSt,
                    columns: [
                        {header: 'Operation', dataIndex: 'Op'},
                        {header: 'Rows', dataIndex: 'Rows'},
                        {header: 'Alpha', dataIndex: 'Alpha'},
                        {header: 'Beta', dataIndex: 'Beta'},
                        {header: 'Seasonal period', dataIndex: 'SeasonalPeriod'}
                    ]
                }
			],
            buttons: [
                {text: 'OK'},
                {text: 'Cancel'}
            ]
		});
		this.callParent(arguments);
	}
});
