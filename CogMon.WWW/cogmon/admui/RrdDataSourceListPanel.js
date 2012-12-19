Ext.define('CogMon.admui.RrdDataSourceListPanel', {
    extend: 'Ext.panel.Panel',
	requires: ['Ext.ux.RowExpander','Ext.grid.*', 'CogMon.admui.RrdDataSourceEditPanel'],
	uses: [],
    statics: {
        formatVars: function(v) {
            if (Ext.isEmpty(v)) return "";
            var s = "";
            for (var p in v) 
            {
                if (s.length > 0) s = s + ', ';
                s = s + p + ':' + v[p];
            }
            return s;
        }
    },
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Description", "TemplateId", "CreatedDate", "Variables"],
			paramOrder: ['start', 'limit', 'sort', 'dir', 'filter'],
            idProperty: 'Id',
            autoLoad: true,
            root: undefined,
            directFn: RPC.AdminGUI.GetRRDDataSources
        });
        
        var grid = Ext.create('Ext.grid.Panel', {
            border: false,
            dockedItems: [
                {xtype: 'toolbar',items: [   
                    {text: 'Create new RRD', handler: function() {
                        CogMon.admui.RrdDataSourceEditPanel.openEditorWindow({});
                    }}
                ]},
                {
                    xtype: 'pagingtoolbar', store: st, dock: 'bottom', displayInfo: true
                }
            ],
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180},
                {header: 'CreatedDate', dataIndex: 'CreatedDate'},
                {header: 'Description', dataIndex: 'Description', flex: 1}
            ],
            plugins: [
                {
                    ptype: 'rowexpander',
                    rowBodyTpl : [
                        '<p><b>Variables</b>: {[CogMon.admui.RrdDataSourceListPanel.formatVars(values.Variables)]}</p>'
                    ]
                }
            ]
        });
        
        Ext.apply(this, {
            layout: 'fit', 
            items: grid
        });
        this.callParent(arguments);
    }
});
