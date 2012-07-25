//portlet based on sencha graph and rrd xport
Ext.define('CogMon.ui.TimeSeriesGraphPortlet', {
    extend: 'CogMon.ui.Portlet',
    requires: ['CogMon.ui.ExtGraphTheme'],
    autoRefreshInterval: undefined,
    dataSeriesId: undefined,
    step: 300,
	theme: 'Category1',
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
    loadData: function() {
        Ext.Ajax.request({
            url: 'Data/XPortGraphData', method: 'GET',
            params: {
                definitionId: me.graphDefinitionId,
                startTime: me.getStartTime(),
                endTime: me.getEndTime(),
                step: me.step
            },
            success: function(resp) {
                var rt = Ext.decode(resp.responseText);
                me.loadDataObj(rt);
            },
            failure: function() {
                console.log('fail');
            }
        });
    },
    loadDataObj: function(v) {
        var me = this;
        var flds = [{name: 'ts', type: 'int'}, {name: 'timestamp', type: 'date'}];
        for (var i=0; i<v.Columns.length; i++) {
            flds.push({name: 'c' + i, type: 'float'});
        }
        var dt = [];
        for (var j=0; j<v.Rows.length; j++) {
            var r = v.Rows[j];
            var av = [r.T, new Date(r.T * 1000)];
            dt.push(av.concat(r.V));
        }
        if (Ext.isEmpty(me.gstore)) {
            var gst = Ext.create('Ext.data.ArrayStore', {
                fields: flds,
                data: dt
            });
            var gc = {xtype: 'gridpanel', itemId: 'thegrid',
				emptyText: 'No events in selected date range',
                store: gst,
                columns: [
                    {dataIndex: 'timestamp', header: 'Time', format:"Y-m-d H:i:sO", xtype: "datecolumn"}
                ],
				autoScroll: true
            };
            for (var i=0; i<v.Columns.length; i++) {
                gc.columns.push({dataIndex: 'c' + i, header: v.Columns[i]});
            }
            var grid = Ext.create('Ext.grid.Panel', gc);
            me.removeAll();
            me.add(grid);
            me.gstore = gst;
        }
        else {
            me.gstore.loadData(dt);
        }
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