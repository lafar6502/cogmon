// 'add portlet' panel for selecting a portlet from a list
Ext.define('CogMon.ui.AddPortletPanel', {
    extend: 'Ext.panel.Panel',
	requires: [],
	uses: ['Ext.window.Window'],
    statics: {
        runPortletSelection : function(callback) {
            var w = Ext.create('Ext.window.Window', {
                modal: true,
                width: 500,
                height: 300,
                title: 'Add portlet',
                layout: 'fit',
                autoDestroy: true,
                items: Ext.create('CogMon.ui.AddPortletPanel', {itemId: 'thepnl'}),
                buttons: [
                    {
                        text: 'OK', 
                        handler: function() {
                            var p = w.getComponent('thepnl').getComponent('thegrid');
                            var sm = p.getSelectionModel();
                            var s = sm.getLastSelected();
                            if (Ext.isEmpty(s)) return;
                            if (!Ext.isEmpty(callback)) callback(s.raw);
                            w.close();
                        }                        
                    },
                    {text: 'Cancel', handler: function() { w.close(); }}
                ]
            });
            w.show();
        }
    },
	initComponent: function() {
		var me = this;
        var gd = [];
        var st = Ext.create('Ext.data.DirectStore', {
            //data: [{Id: "a", Title: "be eee", "Config" : {}, PortletClass: "a.b.c"}],
            fields: [
                "Id",
                "Title",
                "Config",
                "PortletClass"
            ],
            idProperty: 'Id',
            autoLoad: true,
            root: undefined,
            directFn: RPC.UserGui.GetGraphPortletList
        });
        
		Ext.apply(me, {
			layout: 'fit',
			defaults: {border: false},
			
            items: {
                xtype: 'gridpanel',
                itemId: 'thegrid',
                columns: [
                    {dataIndex: 'Title', header: 'Title', width: 400, flex: 1.0}
                ],
                store: st,
				autoScroll: true
            }
		});
		this.callParent(arguments);
	}
});
