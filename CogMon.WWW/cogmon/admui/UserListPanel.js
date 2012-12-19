Ext.define('CogMon.admui.UserListPanel', {
    extend: 'Ext.panel.Panel',
	requires: ['Ext.grid.*', 'Ext.data.*','Ext.ux.RowExpander', 'Ext.grid.feature.RowBody','Ext.grid.feature.RowWrap', 'CogMon.admui.UserEditPanel'],
	uses: [],
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ['Id', 'Login', 'Email', 'Name', 'Active', 'MemberOf', 'ExtId', 'LastSync', 'NeedsSync'],
			paramOrder: ['start', 'limit', 'filter', 'sort', 'dir'],
            idProperty: 'Id', totalProperty: 'Total',
            root: 'Data',
            directFn: RPC.AdminGUI.GetUsersList,
            autoLoad: true, remoteSort: true, remoteFilter: true, simpleSortMode: true
        });
        var grid = Ext.create('Ext.grid.Panel', {
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180},
                {header: 'Login', dataIndex: 'Login'},
                {header: 'Email', dataIndex: 'Email'},
                {header: 'Name', dataIndex: 'Name'},
                {header: 'Active', dataIndex: 'Active'},
                {header: 'External Id', dataIndex: 'ExtId'},
                {header: 'Needs sync', dataIndex: 'NeedsSync'},
                {header: 'Last sync', dataIndex: 'LastSync'}
            ],
            plugins: [
                {
                    ptype: 'rowexpander',
                    rowBodyTpl : [
                        '<p>Member of: <tpl for="MemberOf">{.}, </tpl></p>'
                    ]
                }
            ]
        });
        
        
        Ext.apply(me, {
            layout: 'fit',
            dockedItems: [
                {
                    xtype: 'toolbar',
                    items: [
                        {xtype: 'button', text: 'Create new', icon: '../Content/img/add.png'},
                        {xtype: 'button', text: 'Delete', icon: '../Content/img/delete.png'},
                        {xtype: 'button', text: 'Set password'},
                        {xtype: 'button', text: 'Edit', icon: '../Content/img/edit.png',
                            handler: function() {
                                var ur = grid.getSelectionModel().getLastSelected();
                                if (Ext.isEmpty(ur)) return;
                                CogMon.admui.UserEditPanel.openEditorWindow(ur.data, {
                                    saved: function() {
                                        st.load();
                                    }
                                });
                            }
                        },
                        {xtype: 'tbfill'},
                        {xtype: 'textfield', name:'filter', width: 250},
                        {xtype: 'button', text: 'Search', icon: '../Content/img/search.png', handler: function() {
                                
                            }
                        }
                    ]
                }
            ],
            items: grid
        });
        this.callParent(arguments);
    }
});
