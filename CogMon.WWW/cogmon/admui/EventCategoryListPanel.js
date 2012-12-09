Ext.define('CogMon.admui.EventCategoryListPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Name", "Color"],
			//paramOrder: ['start', 'end'],
            idProperty: 'Id', autoLoad: true,
            directFn: RPC.AdminGUI.GetEventCategories
        });
        Ext.apply(this, {
            dockedItems: [
                {xtype: 'toolbar',items: [
                    {text: 'Add category', icon: '../Content/img/add.png', handler: function() {
                        st.add({Id: '', Name: '', Color: ''});
                    }},
                    {text: 'Save modified data', icon: '../Content/img/save.png',
                        handler: function() {
                            st.each(function(r) {
                                if (r.dirty)
                                {
                                    RPC.AdminGUI.SaveEventCategory(r.data, {
                                        success: function(e, v) {
                                            r.commit();
                                        }
                                    });
                                    
                                }
                            });
                        }
                    },
                    {text: 'Delete category', icon: '../Content/img/delete.png',
                        handler: function() {
                        }
                    }
                ]}
            ],
            store: st,
            columns: [
                {header: 'Id', dataIndex: 'Id', width: 180, editor: {xtype:'textfield', allowBlank: false}},
                {header: 'Name', dataIndex: 'Name', editor: {xtype:'textfield', allowBlank: false}},
                {header: 'Color', dataIndex: 'Color', flex: 1, editor: {xtype:'textfield', allowBlank: false}}
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
