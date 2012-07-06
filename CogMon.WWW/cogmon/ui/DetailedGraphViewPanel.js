// 'add portlet' panel for selecting a portlet from a list
Ext.define('CogMon.ui.DetailedGraphViewPanel', {
    extend: 'Ext.panel.Panel',
	requires: [],
	uses: ['Ext.window.Window', 'CogMon.ui.RrdGraphPortlet', 'Ext.ux.StatusBar'],
    statics: {
        showDetailedGraphWindow : function(cfg) {
			var sbar = Ext.create('Ext.ux.StatusBar', {
				itemId: 'sbar',
				defaultText: '',
				text: 'Status'
			});
            var p = Ext.create('CogMon.ui.RrdGraphPortlet', {
				graphDefinitionId: cfg.graphDefinitionId,
				showGraphTooltip: false,
				startTime: cfg.startTime,
				endTime: cfg.endTime,
				step: cfg.step,
				hideElements: cfg.hideElements,
				border: false,
				padding: {bottom: 0, top: 0},
				draggable: false, header: false,
				region: 'center',
				listeners: {
					mousepositioninfo: function(g, gi) {
						var txt = '<b>Date:</b> ' + gi.dateStr + ', <b>Value:</b> ' + Ext.Number.toFixed(gi.value, 2);
						if (!Ext.isEmpty(gi.event))
						{
							txt = txt + ', <b>Event:</b> ' + gi.event.Text;
						}
						sbar.setText(txt);
					}
				}
			});
			var tls = [];
			if (true) tls.push({
				type: 'gear',
				tooltip: 'Settings',
				handler: function(event, toolEl, panel) {
					p.showConfigEditor();
				}
			});
			var w = Ext.create('Ext.Window', {
				modal: false, layout: 'border',
				width: 800, height: 600, maximizable: true,
				title: cfg.title,
				items: [
					p
				],
				tools: tls,
				bbar: sbar
			});
			w.show();
        }
    },
	initComponent: function() {
		var me = this;
        
		Ext.apply(me, {
			layout: 'fit',
			defaults: {border: false},
			
            items: {
                
            }
		});
		this.callParent(arguments);
	}
});
