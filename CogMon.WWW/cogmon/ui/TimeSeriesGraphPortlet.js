//time series graph
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
        this.loadData();
        //if (!suppressNotification) this.fireEvent('daterangechanged', this, start, end);
    },
    setupConfigPropertyGrid: function(gcfg) {
        gcfg = this.callParent(arguments);
        return Ext.apply(gcfg.source, {
            step: this.step,
            dataSeriesId: this.dataSeriesId
        });
    },
    applyUpdatedConfig: function(cfg) {
        this.setHeight(cfg.height);
        this.step = cfg.step;
        this.fireEvent('configchanged', this, cfg);
        this.loadData();
    },
    loadData: function() {
        var me = this;
        Ext.Ajax.request({
            url: 'Data/GetData', method: 'GET',
            params: {
                id: me.dataSeriesId,
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
        var flds = [{name: 'T', type: 'int'}, {name: 'Timestamp', type: 'date'}];
        var dflds = [];
        for (var i=0; i<v.DataColumns.length; i++) {
            flds.push({name: 'c' + i, type: 'float'});
            dflds.push('c' + i);
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
            
            var chart = Ext.create('Ext.chart.Chart', {
                id: 'theChart', xtype: 'chart', animate: false, shadow: false,
                store: gst,
                theme: me.theme,
                legend: {
                    position: 'left'
                },
                axes: [
                {
                    type: 'Numeric', position: 'left', fields: dflds, grid: true, minimum: 0
                }, 
                {
                    type: 'Time', position: 'bottom', fields: 'Timestamp', 
                    dateFormat: 'M d',
                    //groupBy: 'year,month,day',
                    //aggregateOp: 'sum',
                    //constrain: true,
                    //fromDate: new Date('1/1/11'),
                    //toDate: new Date('1/7/11')
                    title: 'Date'
                }],
                series: [{
                    type: 'column',
                    axis: 'left',
                    highlight: true,
                    stacked: Ext.isEmpty(me.stacked) ? false : me.stacked,
                    xField: 'Timestamp',
                    yField: dflds,
                    tips: {
                        /*trackMouse: true,
                        width:170,
                        renderer: function(storeItem, item) {
                            this.setTitle(item.yField + ':' + item.value[1]);
                        }*/
                    }
                }]
            });
            me.removeAll();
            me.add(chart);
            me.gstore = gst;
        }
        else {
            me.gstore.loadData(dt);
        }
    },
    initComponent: function() {
        var me = this;
        
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
            items: []
        });
        if (Ext.isEmpty(me.listeners)) me.listeners = {};        
        me.on('render', function() {
            console.log('activate');
            me.loadData();
        });
        this.addEvents( 'daterangechanged', 'configchanged');
        this.callParent(arguments);
        
    },
    alias: 'widget.timeseriesgraphportlet'
});