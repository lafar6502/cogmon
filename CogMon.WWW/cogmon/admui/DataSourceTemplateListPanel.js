Ext.define('CogMon.admui.DataSourceTemplateListPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: ['CogMon.admui.CreateDataSeriesFromTemplatePanel'],
    maxHeight: 300,
    createDataSource: function() {
        var sel = this.getSelectionModel().getLastSelected();
        console.log(sel);
        if (Ext.isEmpty(sel)) return;
        CogMon.admui.CreateDataSeriesFromTemplatePanel.showCreateDataSourceWindow(sel.raw, {});
    },
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Name", "Description"],
			//paramOrder: ['start', 'end'],
            idProperty: 'Id',
            autoLoad: false,
            root: undefined,
            directFn: RPC.AdminGUI.GetDataSourceTemplates,
            autoLoad: true
        });
        Ext.apply(this, {
            dockedItems: [
                {xtype: 'toolbar',items: [
                    {text: 'Create data source', icon: '../Content/img/add.png', handler: function() { me.createDataSource(); }},
                    {text: 'Add template', icon: '../Content/img/add.png'},
                    {text: 'Edit', icon: '../Content/img/edit.png'}
                ]}
            ],
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180},
                {header: 'Name', dataIndex: 'Name'},
                {header: 'Description', dataIndex: 'Description', flex: 1}
            ],
            viewConfig: {
                /*getRowClass: function(r, idx, rowParams, store) {
                    if (r.data.IsError) return 'status_row_error';
                    if (r.data.StatusInfo == "Not reported yet" && !r.data.IsError) return 'status_row_inactive';
                    if (r.data.StatusInfo == "OK" && !r.data.IsError) return 'status_row_ok';
                    return null;
                }*/
            }
        });
        this.callParent(arguments);
    }
});
