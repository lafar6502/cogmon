//time series graph
Ext.define('CogMon.ui.TimeSeriesGraphPortletGV', {
    extend: 'CogMon.ui.Portlet',
    requires: ['CogMon.ui.ExtGraphTheme'],
    autoRefreshInterval: undefined,
    dataSeriesId: undefined,
    step: 300,
    theme: 'Category1',
    stacked: false,
    graphType: 'line',
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
        var dT = new google.visualization.DataTable();
        dT.addColumn('date', 'Timestamp');
        for (var i=0; i<v.DataColumns.length; i++) {
            dT.addColumn('number', v.DataColumns[i]);
        }
        var dt = [];
        for (var j=0; j<v.Rows.length; j++) {
            var r = v.Rows[j];
            var av = [new Date(r.T * 1000)];
            dt.push(av.concat(r.V));
        }
        console.log('DATA: ' + Ext.encode(dt));
        dT.addRows(dt);
        console.log('my id is ' + me.getId());
        var bid = me.getId() + '-body';
        var wrp = new google.visualization.ChartWrapper({
            chartType: 'LineChart',
            dataTable: dT,
            options: {'title': 'Test'},
            containerId: bid
        });
        wrp.draw();
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
    alias: 'widget.timeseriesgraphportletgv'
});