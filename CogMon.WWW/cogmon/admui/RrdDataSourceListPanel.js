Ext.define('CogMon.admui.RrdDataSourceListPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
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
        
        Ext.apply(this, {
            dockedItems: [
                {xtype: 'toolbar',items: [
                    
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
            ]
        });
        this.callParent(arguments);
    }
});
