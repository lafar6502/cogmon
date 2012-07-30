Ext.define('CogMon.ui.DashboardConfigurablePage', {
    extend: 'Ext.panel.Panel',
	requires: [],
	uses: ['CogMon.ui.RrdGraphPortlet', 'Ext.app.PortalPanel', 'CogMon.ui.AddPortletPanel', 'CogMon.ui.DatePickField', 'CogMon.ui.DetailedGraphViewPanel', 'CogMon.ui.AddDataSeriesPortletPanel'],
    alias: 'widget.dashboardconfigurablepage',
    startTime: 'e-1d',
    endTime: 'now',
	pinnedByMe: false,
	editable: false,
    config: {
        pageConfig: {},
        pageId: null,
        startTime: '-1d',
        endTime: 'now'
    },
    savePage : function() {
		RPC.UserGui.SavePage(this.pageConfig, {
			success: function(ret, e) {
				console.log('page saved: ' + e.status);
			}
		});
    },
    //re-loads the portal page from server
    loadPage : function(pageId) {
        RPC.UserGui.GetPortalPageConfig(pageId, {
            success: function(pc, e2) {
                if (e2.status) 
                {
                    me.loadPageConfig(pc);
                }
            },
            failure: function() {
                alert('fail..');
            }
        });
    },
	configPropertyNames: undefined,
	setupConfigPropertyGrid: function(gcfg) {
		var src = {
			columnCount: this.pageConfig.Columns.length,
			title: this.pageConfig.Title
		};
		for (var i=0; i<this.pageConfig.Columns.length;i++) {
			var c = this.pageConfig.Columns[i];
			src['columnWidth_' + i] = Ext.isEmpty(c.Config.columnWidth) ? 0.0 : c.Config.columnWidth;
		}
		
		return Ext.apply(gcfg, {
			"source" : src
		});
	},
	applyUpdatedConfig: function(cfg) {
		var pc = this.pageConfig;
		if (!Ext.isEmpty(cfg.title)) pc.Title = cfg.title;
		if (!Ext.isEmpty(cfg.columnCount)) {
			while(pc.Columns.length < cfg.columnCount) {
				pc.Columns.push({Config: {}, Portlets: []});
			}
			if(pc.Columns.length > cfg.columnCount) {
				pc.Columns.splice(cfg.columnCount, pc.Columns.length - cfg.columnCount);
			}
		};
		for (var i = 0; i<cfg.columnCount; i++) {
			var cw = cfg['columnWidth_' + i];
			if (!Ext.isEmpty(cw) && cw > 0.0) {
				pc.Columns[i].Config.columnWidth = cw;
			}
		}
		if (!Ext.isEmpty(pc.Id) && pc.Editable) {
			this.savePage(pc);
		}
		this.loadPageConfig(pc);
	},
	showConfigEditor: function(wcfg) {
		var me = this;
		var gcfg = {
			width: 300,
			propertyNames: me.configPropertyNames,
			source: {}
		};
		me.setupConfigPropertyGrid(gcfg);
		var propsGrid = Ext.create('Ext.grid.property.Grid', gcfg);
		
		var w = Ext.create('Ext.window.Window', {
			width: 400,
			height: 400,
			layout: 'fit',
			title: 'Settings',
			modal: true,
			items: propsGrid,
			buttons: [
				{
					text: 'OK',
					handler: function() {
						var src = propsGrid.getSource();
						me.applyUpdatedConfig(src);
						w.close();
					}
				},
				{
					text: 'Cancel', handler: function() { w.close(); }
				}
			]
		});
		w.show();
	},
	changeNumberOfColumns: function(pc, cols) {
		if (pc.Columns.length == cols) return pc;
		
	},
    loadPageConfig : function(pc) {
        var me = this;
		me.editable = pc.Editable;
        me.setPageId(pc.Id);
        me.setPageConfig(pc);
		me.setTitle(pc.Title);
        var rendered = !Ext.isEmpty(this.getEl());
        if (rendered)
        {
            me.removeAll(true);
        }
        else
        {
            me.items = [];
        }
        var pcfg = {
            itemId: 'mainp', 
            items: [],
            listeners: {
                drop: function(ev) {
                    var pid = ev.panel.itemId;
                    RPC.UserGui.PortletMovedOnPage(me.getPageId(), pid, ev.columnIndex, ev.position, {
                        success: function(pc, e) {
                            if (e.status) {
                                //me.loadPageConfig(pc);
                            }
                        }
                    });
                }
            }
        };
        me.pageId = pc.Id;
        me.itemId = pc.Id;
        
        if (!Ext.isEmpty(pc.Config))
        {
            Ext.apply(pcfg, pc.Config);
        }
        for (var j=0; j<pc.Columns.length; j++) {
            var pcol = pc.Columns[j];
            var col = {
                itemId: pcol.Id,
                items: []
            };
            if (!Ext.isEmpty(pcol.Portlets))
            {
                for (var k=0; k<pcol.Portlets.length; k++) {
                    var ppor = pcol.Portlets[k];
                    //alert("port: " + Ext.encode(ppor));
                    if (!Ext.isEmpty(ppor.PortletClass)) {
						Ext.apply(ppor.Config, {draggable: pc.Editable, closable: pc.Editable, editable: pc.Editable});
                        col.items.push(this.createPortletComponent(ppor));
                    }
                    else {
                        var portletCfg = {
                            title: ppor.Title,
                            itemId: ppor.Id,
                            startTime: me.getStartTime(),
                            endTime: me.getEndTime(),
                            PortletClass: ppor.PortletClass,
							draggable: pc.Editable,
							closable: pc.Editable,
							editable: pc.Editable
                        };
                        if (!Ext.isEmpty(ppor.Config)) Ext.apply(portletCfg, ppor.Config);
                        col.items.push(portletCfg);
                    }
                }
            }
            if (!Ext.isEmpty(pcol.Config)) Ext.apply(col, pcol.Config);
			console.log('col cfg: ' + Ext.encode(pcol.Config));
            pcfg.items.push(col);
            
        }
        //alert('cfg: ' + Ext.encode(pcfg));
        var cc = Ext.create("Ext.app.PortalPanel", pcfg);
        if (rendered)
            me.add(cc);
        else
            me.items = cc;
		if (Ext.isEmpty(pc.Pinned))
		{
			RPC.UserGui.IsPagePinnedByMe(this.getPageId(), {
				success: function(ret, e) {
					if (e.status) {
						me.setPinnedByMe(ret);
					}
				}
			});
		}
		if (pc.Editable == false)
		{
			Ext.defer(function() {
				me.down('#btn_add').disable();
				me.down('#btn_config').disable();
				},100);
		}
    },
	setPinnedByMe : function(pinned) {
		this.pinnedByMe = pinned;
		var t = this.down('#btn_pin');
		if (!Ext.isEmpty(t)) t.toggle(pinned, true);
	},
    onPortletRemoved : function(portletId) {
		var me = this;
        if (Ext.isEmpty(portletId)) return;
        RPC.UserGui.RemovePortletFromPage(this.pageId, portletId, {
            success: function(ret, e) {
                if (e.status) {
                    me.loadPageConfig(ret);
                }
            }
        });
    },
    createPortletComponent: function(portlet) {
        var me = this;
        var id = Ext.isEmpty(portlet.Id) ? Ext.id() : portlet.Id;
        var cfg = {
            title: portlet.Title,
            itemId: id,
            startTime: me.getStartTime(),
            endTime: me.getEndTime(),
            listeners: {
                close: function() {
                    me.onPortletRemoved(id);
                },
				detailsclicked : function(p, prm) {
					var pc = prm;
					//pc.editable = me.editable;
					CogMon.ui.DetailedGraphViewPanel.showDetailedGraphWindow(pc);
				},
				configchanged : function(p, cfg) {
					console.log('portlet config changed: ' + Ext.encode(cfg));
					var pid = p.itemId;
					if (!Ext.isEmpty(pid)) {
						RPC.UserGui.UpdatePortletConfig(me.pageId, pid, cfg, {});
					}
				}
            }
        };
        Ext.apply(cfg, portlet.Config);
        //alert('creating ' + portlet.PortletClass + ', ' + Ext.encode(cfg));
        return Ext.create(portlet.PortletClass, cfg);
    },
    addPortlet : function(portlet) {
        var me = this;
        RPC.UserGui.AddNewPortletToPage(this.pageId, -1, portlet, {
            success: function(ret, e) {
                if (e.status) {
                    //alert('loading page: ' + Ext.encode(ret));
                    me.loadPageConfig(ret);
                }
                else {
                    alert('faild');
                }
            }
        });
        /*
        var p = this.createPortletComponent(portlet);
        var c = this.getPortalPanel().child('portalcolumn');
        c.add(p);*/
    },    
    getPortalPanel: function() {
        return this.getComponent('mainp');
    },
    setDateRange: function(start, end) {
        this.down('#dateTo').setValue(end);
        this.down('#dateFrom').setValue(start);
        this.fireEvent('timerangechanged', this, {startTime: start, endTime: end});
        var l = Ext.ComponentQuery.query('.portlet', this);
        for (var i=0; i<l.length; i++) {
            l[i].setDateRange(start, end);
        }
    },
    onDateRangeUpdated: function() {
        var end = this.down('#dateTo').getValue();
        var start = this.down('#dateFrom').getValue();
        var l = Ext.ComponentQuery.query('.portlet', this);
        for (var i=0; i<l.length; i++) {
            l[i].setDateRange(start, end);
        }
    },
	initComponent: function() {
		var me = this;
        var pcfg = {
            itemId: 'mainp',
            items: [
                {},
                {},
                {}
            ]
        };
        var pdata = [
            [1, 'e-1h', 'now', 'last hour'],
            [2, 'e-2h', 'now', 'last 2 hours'],
            [3, 'e-6h', 'now', 'last 6 hours'],
            [4, 'e-1d', 'now', 'last day'],
            [5, 'e-2d', 'now', 'last 2 days'],
            [6, 'e-1wk', 'now', 'last week'],
            [7, 'e-2wk', 'now', 'last 2 weeks'],
            [9, 'e-1months', 'now', 'last month'],
            [10, 'e-2months', 'now', 'last 2 months'],
            [11, 'e-6months', 'now', 'last 6 months'],
            [12, 'e-1y', 'now', 'last year']
        ];
        var st = new Ext.data.ArrayStore({
            data: pdata,
            fields: ['id', 'startDate', 'endDate', 'label'],
            idProperty: 'id'
        });
        var mnu = [];
        for(var i=0; i<pdata.length; i++) {
            var di = pdata[i];
            mnu.push({text: di[3], itemId: '' + i, handler: function(itm) {var t = pdata[Ext.Number.from(itm.itemId)]; me.setDateRange(t[1], t[2]);}});
        }
        var mecfg = {
            layout: {
                type: 'fit',
                align: 'stretch'
            },
            dockedItems: [
                {
                    xtype: 'toolbar', itemId: 'toptbar',
                    dock: 'top',
                    collapsible: true,
                    items: [
                        {   
                            xtype: 'button', text: 'Time range ', icon: 'Content/img/calendar.png',
                            menu: mnu
                        },
                        {xtype: 'tbspacer'},
                        {xtype: 'label', text: ' from '},
                        {xtype: 'graphdatefield', name: 'startDate', itemId: 'dateFrom', value: me.getStartTime()},
                        {xtype: 'label', text: ' to '},
                        {xtype: 'graphdatefield', name: 'endDate', itemId: 'dateTo', value: me.getEndTime()},
                        {xtype: 'tbspacer'},
                        {xtype: 'button', icon: 'Content/img/refresh.png', text: 'Refresh', handler: function() {me.onDateRangeUpdated(); }},
                        {xtype: 'tbfill'},
                        {
                            text: 'Add..', icon: 'Content/img/add.png', itemId: 'btn_add',
							menu: [
								{
									text: 'RRD/Time series graph',
									handler: function() {
                                        CogMon.ui.AddDataSeriesPortletPanel.showSelectionWindow({
                                            callback: function(s) {
                                                console.log('sel: ' + Ext.encode(s));
                                                me.addPortlet(s);
                                            }
                                        });
									}
								},
                                {
                                    text: 'Predefined portlets',
                                    handler: function() {
                                        CogMon.ui.AddPortletPanel.runPortletSelection(function(pt) {
                                            me.addPortlet(pt);
                                        });
                                    }
                                },
								{
									text: 'Other',
									menu: [
										{
											text: 'Event list',
											handler: function() {
												var pt = {
													PortletClass: 'CogMon.ui.EventListPortlet',
													Title: 'Event List',
													Config: {
														height: 300
													}
												};
												me.addPortlet(pt);
											}
										}
									]
								}
							]
                        },
						{
							xtype: 'button', enableToggle: true, icon: 'Content/img/pin.png', text: 'Pin', itemId: 'btn_pin',
							toggleHandler: function(b, pin) {
								var pid = me.getPageId();
								if (Ext.isEmpty(pid)) return;
								RPC.UserGui.SetPagePinned(pid, pin, {
									success: function(ret, e) {
										if (e.status) {
											me.pinnedByMe = pin;
										}
									}
								});
							}
						},
						{
							xtype: 'button', text: 'Page config', itemId: 'btn_config',  icon: 'Content/img/config.png',
							handler: function() {
								me.showConfigEditor({});
							}
						},
                        {
                            xtype: 'button', icon: 'Content/img/help.png', text: 'Help'
                        }
                    ]
                }
            ],
            items: []
        };
        Ext.apply(me, mecfg);
		me.on('activate', function(t) {
			console.log('Activate ' + me.getPageId() + ', visible: ' + me.isVisible(true));
			if (me.isVisible(true))
			{
				me.onDateRangeUpdated();
			}
		});
        if (!Ext.isEmpty(this.pageConfig)) {
            this.loadPageConfig(this.pageConfig);
        }
        else if (!Ext.isEmpty(this.pageId)) {
            this.loadPage(this.pageId);
        }
        else {
            Ext.apply(me, {
                items: Ext.create('Ext.app.PortalPanel', pcfg)
            });
        }
        this.addEvents('timerangechanged', 'pinchanged');
		this.callParent(arguments);
	}
});
