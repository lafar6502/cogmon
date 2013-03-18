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
                {
                    header: 'Name', flex: 1, dataIndex: 'Name', 
                    renderer: function(v, m, r) {
                        var c = r.data.Color;
                        if (Ext.isString(c) && c.length >= 6)
                        {
                            m.style = 'border-bottom: solid 2px #' + c.substring(0,6);
                        }
                        return v;
                    }
                }
            ],
            selType: 'checkboxmodel'
        });
        
        this.callParent(arguments);
    },
    alias: 'widget.eventcategoryselectgrid'
});