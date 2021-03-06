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
                    {text: 'Add group', icon: '../Content/img/add.png', handler: function() {
                        st.add({Id: '', Name: '', Description: ''});
                    }},
                    {text: 'Save modified data', icon: '../Content/img/save.png',
                        handler: function() {
                            st.each(function(r) {
                                if (r.dirty)
                                {
                                    RPC.AdminGUI.SaveGroup(r.data, {
                                        success: function(e, v) {
                                            r.commit();
                                        }
                                    });
                                    
                                }
                            });
                        }
                    },
                    {text: 'Delete group', icon: '../Content/img/delete.png',
                        handler: function() {
                        }
                    }
                ]}
            ],
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180, editor: {xtype:'textfield', allowBlank: false}},
                {header: 'Name', dataIndex: 'Name', editor: {xtype:'textfield', allowBlank: false}},
                {header: 'Description', dataIndex: 'Description', flex: 1, editor: 'textfield'}
            ],
            selType: 'rowmodel',
            plugins: [
                Ext.create('Ext.grid.plugin.CellEditing', {
                    clicksToEdit: 1,
                    listeners: {
                        beforeedit: function(ed, e) {
                            console.log(e.record);
                            if (e.field == 'Id' && !e.record.phantom) 
                            {
                                e.cancel = true;
                            }
                        }
                    }
                })
            ]
        });
        this.callParent(arguments);
    }
});
