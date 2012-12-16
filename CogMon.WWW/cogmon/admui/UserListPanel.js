Ext.define('CogMon.admui.UserListPanel', {
    extend: 'Ext.grid.Panel',
	requires: ['Ext.grid.*', 'Ext.data.*','Ext.ux.RowExpander', 'Ext.grid.feature.RowBody','Ext.grid.feature.RowWrap'],
	uses: [],
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ['Id', 'Login', 'Email', 'Name', 'Active', 'MemberOf', 'ExtId'],
			paramOrder: ['start', 'limit', 'filter', 'sort', 'dir'],
            idProperty: 'Id', totalProperty: 'Total',
            root: 'Data',
            directFn: RPC.AdminGUI.GetUsersList,
            autoLoad: true, remoteSort: true, remoteFilter: true, simpleSortMode: true
        });
        Ext.apply(me, {
            collapsible: true,
            animCollapse: false,
            dockedItems: [
                {
                    xtype: 'toolbar',
                    items: [
                        {xtype: 'button', text: 'Create new'},
                        {xtype: 'button', text: 'Delete'},
                        {xtype: 'button', text: 'Set password'},
                        {xtype: 'button', text: 'Edit'},
                        {xtype: 'tbfill'},
                        {xtype: 'textfield', name:'filter', width: 250},
                        {xtype: 'button', text: 'Search', handler: function() {
                                
                            }
                        }
                    ]
                }
            ],
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180},
                {header: 'Login', dataIndex: 'Login'},
                {header: 'Email', dataIndex: 'Email'},
                {header: 'Name', dataIndex: 'Name'},
                {header: 'Active', dataIndex: 'Active'}
            ],
            plugins: [
                {
                    ptype: 'rowexpander',
                    rowBodyTpl : [
                        '<p>Member of: </p>'
                    ],expandOnRender: true, expandOnDblClick: false
                }
            ]
        });
        this.callParent(arguments);
    }
});
