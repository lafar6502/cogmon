Ext.define('CogMon.ui.EventCategorySelectGridPanel', {
    extend: 'Ext.grid.Panel',
    requires: [],
    initComponent: function() {
		var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Name", "Color"],
			//paramOrder: ['start', 'end'],
            idProperty: 'Id', autoLoad: true,
            directFn: RPC.AdminGUI.GetEventCategories
        });
        Ext.apply(this, {
            store: st,            
            columns: [
                {header: 'Color', dataIndex: 'Color', editor: {xtype:'textfield', allowBlank: false},
                    renderer: function(v, m) {
                        if (Ext.isString(v) && v.length >= 6)
                        {
                            m.style = 'border-bottom: solid 2px #' + v.substring(0,6);
                        }
                        return v;
                    }
                },
                {header: 'Name', flex: 1, dataIndex: 'Name', editor: {xtype:'textfield', allowBlank: false}}
            ],
            selType: 'checkboxmodel'
        });
        this.callParent(arguments);
    },
    alias: 'widget.eventcategoryselectgrid'
});