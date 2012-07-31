//time series graph
Ext.define('CogMon.ui.TimeSeriesGraphPortletGV', {
    extend: 'CogMon.ui.Portlet',
    requires: ['CogMon.ui.ExtGraphTheme'],
    autoRefreshInterval: undefined,
    dataSeriesId: undefined,
    step: 300,
    chartType: 'ColumnChart',
    chartConfig: {},
    setDateRange: function(start, end, suppressNotification) {
        this.setStartTime(start);
        this.setEndTime(end);
        this.loadData();
        //if (!suppressNotification) this.fireEvent('daterangechanged', this, start, end);
    },
    setupConfigPropertyGrid: function(gcfg) {
        gcfg = this.callParent(arguments);
        var st = Ext.create('Ext.data.ArrayStore', {autoDestroy: true, fields: ['id', 'name'], idIndex: 0,
			data: [
				[0, 'Auto select'],
				[1, 'Max resolution'],
				[300, '5 minutes'],
				[900, '15 minutes'],
				[1800, '30 minutes'],
				[3600, '1 h'],
				[21600, '6 h'],
				[86400, '24 h']
			]
		});
        Ext.apply(gcfg, {
            customEditors: {
				step: Ext.create('Ext.form.field.ComboBox', {store: st, valueField: 'id', displayField: 'name'})
			}
        });
        return Ext.apply(gcfg.source, {
            step: this.step,
            dataSeriesId: this.dataSeriesId
        });
    },
    applyUpdatedConfig: function(cfg) {
        this.setHeight(cfg.height);
        this.step = cfg.step;
        if (Ext.isEmpty(this.chartWrapper)) return;
        Ext.apply(cfg, {
            chartConfig: this.chartWrapper.getOptions(),
            chartType: this.chartWrapper.getChartType()
        });
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
			if (j == 0 || j == v.Rows.length - 1) 
			{
				var ae = true;
				for (var k = 0; k<r.V.length; k++) 
				{
					if (Ext.isNumber(r.V[k]))
					{
						ae = false;
						break;
					}
				}
				if (ae) continue;
			}
            dt.push(av.concat(r.V));
        }
        //console.log('loading DATA: ' + Ext.encode(dt));
        dT.addRows(dt);
        console.log('my id is ' + me.getId());
        var bid = me.getId() + '-body';
        var wrp = new google.visualization.ChartWrapper({
            chartType: me.chartType,
            dataTable: dT,
            options: me.chartConfig,
            containerId: bid
        });
        this.chartWrapper = wrp;
        wrp.draw();
        return wrp;
    },
    refresh: function() {
        if (Ext.isEmpty(this.chartWrapper)) {
            this.loadData();
        }
        else {
            //this.chartWrapper.draw();
        }
    },
    showChartEditor: function() {
        if (Ext.isEmpty(this.chartWrapper)) {
            console.log('chartWrapper missing');
            return;
        };
        var me = this;
        var ce = new google.visualization.ChartEditor();
        google.visualization.events.addListener(ce, 'ok', function() {
              var cw = ce.getChartWrapper();  
              console.log('current chart: ' + Ext.encode(cw.getOptions()));
              var cfg = {
                chartType: cw.getChartType(),
                chartConfig: cw.getOptions(),
                step: me.step,
                dataSeriesId: me.dataSeriesId,
                height: me.getHeight()
              };
              me.fireEvent('configchanged', me, cfg);
              me.chartWrapper = cw;
              me.chartWrapper.draw();
              //me.chartWrapper.draw();
        });
        ce.openDialog(me.chartWrapper, {});
    },
    initComponent: function() {
        var me = this;
        var mnu = Ext.create('Ext.menu.Menu', {
            items: [
                {text: 'Portlet configuration', handler: me.showConfigEditor, scope: me},
                {text: 'Chart configuration', handler: me.showChartEditor, scope: me}
            ]
        });
        
        Ext.apply(me, {     
            layout: 'fit',
            tools: [
                {
                    type: 'gear',
                    tooltip: 'Settings',
                    handler: function(event, toolEl, panel) {
                        mnu.showBy(toolEl);
                    }
                },
                {
                    type:'refresh',
                    tooltip: 'Refresh',
                    handler: function(event, toolEl, panel){
                        
                        
                    }
                }
            ],
            items: []
        });
        if (Ext.isEmpty(me.listeners)) me.listeners = {};        
        me.on('render', function() {
            console.log('activate');
            me.refresh();
        });
        me.on('resize', function() {
            me.refresh();
        });
        this.addEvents( 'daterangechanged', 'configchanged');
        this.callParent(arguments);
        
    },
    alias: 'widget.timeseriesgraphportletgv'
});