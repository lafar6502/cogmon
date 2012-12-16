Ext.define('CogMon.admui.UserListPanel', {
    extend: 'Ext.grid.Panel',
	requires: ['Ext.ux.RowExpander'],
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
        Ext.apply(this, {
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
                },
                {
                    xtype: 'pagingtoolbar',
                    store: st,   
                    dock: 'bottom',
                    displayInfo: true
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
                ]
            }
            ],
            features: [{
                ftype: 'rowbody'
            }],
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
