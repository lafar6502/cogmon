Ext.define('CogMon.admui.GroupListPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Name", "Description"],
			//paramOrder: ['start', 'end'],
            idProperty: 'Id', autoLoad: true,
            directFn: RPC.AdminGUI.GetUserGroups
        });
        Ext.apply(this, {
            dockedItems: [
                {xtype: 'toolbar',items: [
                    {text: 'Add group', icon: '../Content/img/add.png'},
                    {text: 'Edit', icon: '../Content/img/edit.png'}
                ]}
            ],
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180},
                {header: 'Name', dataIndex: 'Name', editor: 'textfield'},
                {header: 'Description', dataIndex: 'Description', flex: 1, editor: 'textfield'}
            ],
            selType: 'rowmodel',
            plugins: [
                Ext.create('Ext.grid.plugin.RowEditing', {
                    clicksToEdit: 1
                })
            ]
        });
        this.callParent(arguments);
    }
});
