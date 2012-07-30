// 'add portlet' panel for creating portlets for RRD graphs
Ext.define('CogMon.ui.AddDataSeriesPortletPanel', {
    extend: 'Ext.panel.Panel',
	requires: [],
	uses: ['Ext.window.Window'],
    statics: {
        showSelectionWindow : function(cfg) {
            var w = Ext.create('Ext.window.Window', {
                modal: true,
                width: 700,
                height: 400,
                title: 'Add graph portlet',
                layout: 'fit',
                autoDestroy: true,
                items: Ext.create('CogMon.ui.AddDataSeriesPortletPanel', {itemId: 'thepnl'}),
                buttons: [
                    {
                        text: 'OK', 
                        handler: function() {
                            var p = w.getComponent('thepnl');
                            var pcfg = p.validateAndGetPortletConfig();
                            if (Ext.isEmpty(pcfg)) return;
                            if (!Ext.isEmpty(cfg.callback)) cfg.callback(pcfg);
                            w.close();
                        }                        
                    },
                    {text: 'Cancel', handler: function() { w.close(); }}
                ]
            });
            w.show();
        }
    },
    validateAndGetPortletConfig: function() {
        var f = this.down('#portletType').getValue();
        if (Ext.isEmpty(f)) {
            alert('missing portlet type');
            return null;
        }
        var g = this.down('#thegrid');
        var sm = g.getSelectionModel();
        var s = sm.getLastSelected();
        if (Ext.isEmpty(s)) return null;
        
        try
        {
            var pcfg = this.createPortletConfig(s.raw, f, {});
            return pcfg;
        }
        catch(s)
        {
            alert('error: ' + s);
        }
    },
    createPortletConfig: function(series, portletClass, portletCfg) {
        if (Ext.isEmpty(portletClass)) throw "portletClass missing";
        if (portletClass == "CogMon.ui.RrdGraphPortlet") {
            if (series.SeriesType != "RRD") throw "This graph cannot handle data series of type " + series.SeriesType;
            return {
                Id: series.Id,
                PortletClass: portletClass,
                Title: series.Name,
                Config: {
                    graphDefinitionId: series.Id.substr(3),
                    height: 300
                }
            };
        }
        else if (portletClass == "CogMon.ui.TimeSeriesGraphPortletGV") {
            return {
                Id: series.Id,
                PortletClass: portletClass,
                Title: series.Name,
                Config: {
                    dataSeriesId: series.Id,
                    height: 300
                }
            };
        }
        else throw "Invalid portlet class";
    },
	initComponent: function() {
		var me = this;
        var gd = [];
        var st = Ext.create('Ext.data.JsonStore', {
            fields: ["Id", "Name", "SeriesType", "Description"], autoLoad: true,
            proxy: {
                type: 'ajax', url: "Data/ListDataSeries", 
                reader: {type: 'json', idProperty: 'Id'}
            }
        });
        var pts = Ext.create('Ext.data.ArrayStore', {
            fields: ['id', 'name'],
            data: [
                ['CogMon.ui.RrdGraphPortlet', 'RRD Graph (image)'],
                ['CogMon.ui.TimeSeriesGraphPortletGV', 'Configurable time series graph']
            ]
        });
		Ext.apply(me, {
			layout: 'fit',
			defaults: {border: false},
			dockedItems: {
                xtype: 'toolbar',
                dock: 'top',
                items: [
                    {
                        xtype: 'textfield', name:'query', itemId: 'query_fld', fieldLabel: 'Search ', width:250,
                        listeners: {
                            buffer: 300,
                            change: function() {
                                var sv = this.getValue().toLowerCase();
                                st.clearFilter();
                                st.filterBy(function(r) {
                                    var s = Ext.encode(r.data).toLowerCase();
                                    return s.indexOf(sv) >= 0;
                                });
                            }
                        }
                    },
                    {xtype: 'tbspacer'},
                    {
                        xtype: 'combobox', name: 'portletType', fieldLabel: 'Graph type', width: 300, flex: 1, store: pts, valueField: 'id', displayField: 'name', allowBlank: false, itemId: 'portletType'
                    }
                ]
            },
            items: {
                xtype: 'gridpanel',
                itemId: 'thegrid',
                columns: [
                    {dataIndex: 'Name', header: 'Title',  flex: 0.5},
                    {dataIndex: 'Id', header: 'Id',  flex: 0},
                    {dataIndex: 'SeriesType', header: 'Type', width: 40, flex: 0},
                    {dataIndex: 'Description', header: 'Description',  flex: 0.5}
                ],
                store: st,
				autoScroll: true
            }
		});
		this.callParent(arguments);
	}
});
