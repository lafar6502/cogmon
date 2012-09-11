//Ext js graph. TODO: remove it
Ext.define('CogMon.ui.EventStatGraphPortlet', {
    extend: 'CogMon.ui.Portlet',
    requires: ['CogMon.ui.ExtGraphTheme'],
    autoRefreshInterval: 180,
    seriesId: '',
    step: null,
	theme: 'Category1',
	dataFields: [],
	idField: 'Id',
	labelField: 'Label',
	xField: 'Timestamp',
	stacked: false,
	setDateRange: function(start, end, suppressNotification) {
        this.setStartTime(start);
        this.setEndTime(end);
		Ext.apply(this.gstore.getProxy().extraParams, {
			start: start,
			end: end
		});
		this.gstore.load();
		//if (!suppressNotification) this.fireEvent('daterangechanged', this, start, end);
    },
	setupConfigPropertyGrid: function(gcfg) {
		gcfg = this.callParent(arguments);
		return Ext.apply(gcfg.source, {
			step: 'D'
		});
	},
	applyUpdatedConfig: function(cfg) {
		this.setHeight(cfg.height);
	},
    initComponent: function() {
        var me = this;
		var flds = ['Id', {name: 'Timestamp', type: 'date'}, 'Label'];
		var series = [];
		for (var i=0; i<me.dataFields.length; i++)
		{
			flds.push(me.dataFields[i].name);
			series.push(me.dataFields[i].name);
		}
		var st = Ext.create('Ext.data.JsonStore', {
			fields: flds,
			proxy: {
				type: 'ajax',
				url: 'EventStat/GetData',
				extraParams: {
					series: me.seriesId,
					start: me.getStartTime(),
					end: me.getEndTime(),
					step: me.step
				},
				reader: {
					type: 'json',
					idProperty: 'Id'
				}
			},
			autoLoad: true
		});
		me.gstore = st;
		var grph = Ext.create('Ext.chart.Chart', {
			id: 'chartCmp',
			xtype: 'chart',
			animate: false,
			shadow: false,
			store: st,
			theme: me.theme,
			legend: {
				position: 'bottom'
			},
			axes: [{
				type: 'Numeric',
				position: 'left',
				fields: series,
				grid: true,
				minimum: 0
			}, {
				type: 'Category',
				position: 'bottom',
				fields: ['Label'],
				dateFormat: 'M d',
				groupBy: 'year,month,day',
				aggregateOp: 'sum'
			}],
			series: [{
				type: 'column',
				axis: 'left',
				highlight: true,
				stacked: Ext.isEmpty(me.stacked) ? false : me.stacked,
				xField: 'Timestamp',
				yField: series,
				tips: {
                    trackMouse: true,
					width:170,
                    renderer: function(storeItem, item) {
						
                        this.setTitle(item.yField + ':' + item.value[1]);
                    }
                }
			}]
		});
		Ext.apply(me, {     
            layout: 'fit',
			tools: [
                {
                    type: 'gear',
                    tooltip: 'Settings',
                    handler: function(event, toolEl, panel) {
						me.showConfigEditor();
                    }
                },
                {
                    type:'refresh',
                    tooltip: 'Refresh',
                    handler: function(event, toolEl, panel){
						me.gstore.load();
                    }
                }
            ],
			items: grph
        });
        if (Ext.isEmpty(me.listeners)) me.listeners = {};        
        this.callParent(arguments);
		this.addEvents( 'daterangechanged');
    },
    alias: 'widget.eventstatgraphportlet'
});