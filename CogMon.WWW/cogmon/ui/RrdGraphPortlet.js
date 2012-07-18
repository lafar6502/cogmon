Ext.define('CogMon.ui.RrdGraphPortlet', {
    extend: 'CogMon.ui.Portlet',
    requires: [
    ],
    graphDefinitionId: '',
    enableGraphZoom: false,
	showGraphTooltip: true,
	step: 0,
	hideElements: "",
	editable: true,
    setDateRange: function(start, end, suppressNotification) {
        this.setStartTime(start);
        this.setEndTime(end);
        this.setupContentLoader();
		if (!suppressNotification) this.fireEvent('daterangechanged', this, start, end);
    },
	getImageParams : function() {
		var mh = this.getHeight() < 100 ? 100 : this.getHeight();
		var he = this.hideElements;
		if (Ext.isArray(he)) he = he.join();
		var prm = {
			w: this.getWidth() < 100 ? 96 : this.getWidth() - 4,
			h: this.preventHeader ? mh - 4 : mh - 30,
			definitionId: this.graphDefinitionId,
			startTime: this.getStartTime(),
			endTime: this.getEndTime(),
			skipElements: he
		};
		if (!Ext.isEmpty(this.step)) 
			prm.step = this.step;
		return prm;
	},
    setupContentLoader : function() {
		var me = this;
		if (me.reloadScheduled) return;
		var refn = function() {
			me.reloadScheduled = false;
			if (me.isVisible(true))
			{
				var img = me.down('#theImg');
				if (Ext.isEmpty(img)) return;
				var osrc = img.getEl().getAttribute('src');
				var prm = me.getImageParams();
				var nsrc = 'Graph/DrawGraph?' + Ext.urlEncode(prm);
				//if (osrc == nsrc) alert('same src: ' + nsrc);
				img.setSrc(nsrc);
				img.show();
			}
		};
		Ext.defer(refn, 500, me);
		me.reloadScheduled = true;
    },
    refreshRepeat: function() {
        var d = this.getEndTime();
        var ds = this.autoRefreshInterval * 1000;
        var autoRefresh = ds > 0 && (Ext.isEmpty(d) || d == "now" || d == "N");
        //alert('autoRefresh: ' + autoRefresh + ', ds: ' + ds);
        this.setupContentLoader();
        if (autoRefresh) {
            Ext.Function.defer(this.refreshRepeat, ds, this);
        }
    },
	getPortletConfig: function() {
		return {
			height: this.getHeight(),
			graphDefinitionId: this.graphDefinitionId,
			step: this.step,
			hideElements: Ext.isEmpty(this.hideElements) ? "" : this.hideElements
		};
	},
	applyUpdatedConfig: function(cfg) {
		this.setHeight(cfg.height);
		this.step = cfg.step;
		if (Ext.isArray(cfg.hideElements)) {
			cfg.hideElements = cfg.hideElements.join();
		}
		this.hideElements = cfg.hideElements;
		this.fireEvent('configchanged', this, cfg);
		this.setupContentLoader();
	},
	setupConfigPropertyGrid : function(cfg) {
		var st = Ext.create('Ext.data.ArrayStore', {autoDestroy: true, fields: ['id', 'name'], idIndex: 0,
			data: [
				[1, 'Max resolution'],
				[300, '5 minutes'],
				[1800, '30 minutes'],
				[3600, '1 h'],
				[21600, '6 h'],
				[86400, '24 h']
			]
		});
		
		var elst = Ext.create('Ext.data.DirectStore', {
			directFn: RPC.UserGui.GetGraphElements,
			extraParams: {definitionId: this.graphDefinitionId},
			fields: ['Idx', 'Label', 'Color', 'Op'],
			paramOrder: ['definitionId']
		});
		elst.load({params: {definitionId: this.graphDefinitionId}});
		var pcfg = this.getPortletConfig();
		if (Ext.isString(pcfg.hideElements))
		{
			pcfg.hideElements = pcfg.hideElements.split();
		}
		return Ext.apply(cfg, {
			"source" : this.getPortletConfig(),
			customEditors: {
				step: Ext.create('Ext.form.field.ComboBox', {store: st, valueField: 'id', displayField: 'name'}),
				hideElements: Ext.create('Ext.form.field.ComboBox', {store: elst, valueField: 'Idx', displayField: 'Label', multiSelect: true})
			}
		});
	},
	onImageLoaded: function() {
		console.log('onimageloaded ');
		var mh = this.getHeight() < 100 ? 100 : this.getHeight();
		var prm = this.getImageParams();
		var me = this;
		Ext.Ajax.request({
			url: 'Graph/GraphInfo', params: prm, method: 'GET',
			success: function(resp, opts) {
				var ii = resp.status == 200 ? Ext.decode(resp.responseText) : null;
				me.imageInfo = ii;
				me.fireEvent('imageupdated', me, ii);
				if (me.enableGraphZoom)
				{
					me.floater.show();
				}
				//console.log('imageupdated', resp.responseText);
			},
			failure: function(resp, opts) {
				me.fireEvent('imageupdated', me, null);
			}
		});
	},
	//area - rectangle, in image coordinates
	onImageAreaSelected: function(area) {
		var p1 = this.getGraphPointInfo(area.x - this.imageInfo.GraphLeft, area.y - this.imageInfo.GraphTop);
		var p2 = this.getGraphPointInfo(area.x + area.width - this.imageInfo.GraphLeft, area.y + area.height - this.imageInfo.GraphTop);
		//console.log('area: ' + Ext.encode(area));
		
		var b2 = {x: area.x - this.imageInfo.GraphLeft, y: area.y - this.imageInfo.GraphTop, width: area.width, height: area.height};
		if (area.width < 4) return;
		var td = this.imageInfo.GraphEndTime - this.imageInfo.GraphStartTime;
		if (td <= 0) return;
		var nts = this.imageInfo.GraphStartTime + (b2.x / this.imageInfo.GraphWidth) * td;
		var nte = this.imageInfo.GraphStartTime + (b2.x + b2.width) / this.imageInfo.GraphWidth * td;
		//console.log("time: " + Ext.encode({t1: this.imageInfo.GraphStartTime, t2: nts, te: this.imageInfo.GraphEndTime, te2: nte, tp1: p1.unixtime, tp2: p2.unixtime, tp1_d: p1.date, tp2_d: p2.date}));
		this.setDateRange(p1.unixtime, p2.unixtime);
	},
	//x,y - graph area coords
	getGraphPointInfo: function(x,y) {
		if (Ext.isEmpty(this.imageInfo)) return null;
		var td = this.imageInfo.GraphEndTime - this.imageInfo.GraphStartTime;
		var vd = this.imageInfo.MaxValue - this.imageInfo.MinValue;
		if (td <= 0) return null;
		var nt = this.imageInfo.GraphStartTime + (x / this.imageInfo.GraphWidth) * td;
		nt = Math.round(nt);
		var r = {x: x, y: y, date: new Date(nt * 1000), unixtime: nt};
		r.dateStr = Ext.Date.format(r.date, 'd/m/Y H:i');
		r.value = vd > 0 ? this.imageInfo.MinValue + (1.0 - (y / this.imageInfo.GraphHeight)) * vd : 0.0;
		//console.log(Ext.encode(r));
		return r;
	},
	getGraphInfo: function() { 
		return this.imageInfo;
	},
	
    initComponent: function() {
        var me = this;
		var img = Ext.create('Ext.Img', {itemId: 'theImg'});
		var floater = Ext.create('Ext.container.Container', {floating: true, html: '', width: 200, height: 100, style: {border: 'solid 1px red'}, itemId: 'floater', resizable: false});
		me.floater = floater;
        var tip = me.showGraphTooltip ? Ext.create('Ext.tip.ToolTip', {
            anchor: 'top',
			dismissDelay: 0,
            anchorOffset: 85, // center the anchor on the tooltip
			html: '',
			listeners: {
				beforeshow: function() {
					if (Ext.isEmpty(me.imageInfo)) {
						console.log('missing image info');
						me.onImageLoaded();
					}
				}
			}
		}) : null;
		var mup = function(e) {
			console.log('maus ap');
			Ext.getBody().un('mouseup', mup);
			if (me.selecting) {
				var b= floater.getBox(false);
				floater.hide();
				me.selecting = false;
				b.x = b.x - img.getEl().getX();
				b.y = b.y - img.getEl().getY();
				if (e.within(img.getEl()) && b.width > 5) {
					me.onImageAreaSelected(b);
				}
			}
		};
		img.on('render', function(im) { 
			if (me.showGraphTooltip) tip.setTarget(img.getEl());
			img.getEl().on('load', function() { me.onImageLoaded(); });
			img.getEl().on('mousedown', function(e) {
				if (!me.selecting)
				{
					if (Ext.isEmpty(me.imageInfo)) return;
					var x = e.getX() - img.getEl().getX();
					var y = e.getY() - img.getEl().getY();
					console.log('mousedown ' + Ext.encode({X: x, Y: y}));
					if (x < me.imageInfo.GraphLeft) x =  me.imageInfo.GraphLeft;
					if (y < me.imageInfo.GraphTop) y =  me.imageInfo.GraphTop;
					
					//if (x < me.imageInfo.GraphLeft || y < me.imageInfo.GraphTop) return;
					if (x > me.imageInfo.GraphLeft + me.imageInfo.GraphWidth) return;
					if (y > me.imageInfo.GraphTop + me.imageInfo.GraphHeight) return;
					floater.setPosition(x,y);
					floater.setSize(1, 1);
					floater.show();
					Ext.getBody().on('mouseup', mup);
					me.selecting = true;
				}
			});
			
			img.getEl().on('mousemove', function(e) {
				//console.log('mouse move: ' + e.getX() + ', y: ' + e.getY());
				if (me.selecting)
				{
					e.preventDefault();
					var x = e.getX() - img.getEl().getX();
					var y = e.getY() - img.getEl().getY();
					//console.log('mousemove ' + Ext.encode(e));
					if (x > me.imageInfo.GraphLeft + me.imageInfo.GraphWidth) x = me.imageInfo.GraphLeft + me.imageInfo.GraphWidth;
					if (y > me.imageInfo.GraphTop + me.imageInfo.GraphHeight) y = me.imageInfo.GraphTop + me.imageInfo.GraphHeight;
					
					var w = x + img.getEl().getX() - floater.getEl().getX();
					var h = y + img.getEl().getY() - floater.getEl().getY();
					floater.setSize(w, h);
					
					if (!Ext.isEmpty(me.imageInfo))
					{
						var ox = x - me.imageInfo.GraphLeft;
						//var ox = x - me.imageInfo.GraphLeft + w;
						var oy = y - me.imageInfo.GraphTop;
						var gi = me.getGraphPointInfo(ox, oy);
						if (gi != null) floater.update(gi.dateStr);
					}
				}
				else
				{
					if (img.getEl() != null && me.imageInfo != null)
					{
						var ox = e.getX() - img.getEl().getX();
						var oy = e.getY() - img.getEl().getY();
						ox = ox - me.imageInfo.GraphLeft;
						oy = oy - me.imageInfo.GraphTop;
						if (ox < 0 || oy < 0) return;
						if (ox > me.imageInfo.GraphWidth || oy > me.imageInfo.GraphHeight) return;
						var gi = me.getGraphPointInfo(ox, oy);
						if (gi != null)
						{
							if (!Ext.isEmpty(me.imageInfo.Events))
							{
								for (var i=0; i<me.imageInfo.Events.length; i++)
								{
									var ev = me.imageInfo.Events[i];
									if (Math.abs(ev.GraphX - ox) <= 3)
									{
										gi.event = ev;
										break;
									}
									else if (ev.GraphX > ox) break;
								}
							}
							if (me.showGraphTooltip && tip != null && tip.isVisible())
							{
								var txt = '';
								if (Ext.isEmpty(gi.event))
								{
									txt = '<b>Date:</b> ' + gi.dateStr + ', <b>Value:</b> ' + Ext.Number.toFixed(gi.value, 2);
								}
								else
								{
									txt = '<b>Date:</b> ' + gi.dateStr + ', <b>Value:</b> ' + Ext.Number.toFixed(gi.value, 2) + '<br/><b>Event:</b> ' + gi.event.Text;
								}
								tip.update(txt);
							}
							me.fireEvent('mousepositioninfo', me, gi);
						}
					}
				}
			});
			img.getEl().on('dblclick', function(e) {
				console.log('dbl');
			});
		});
		Ext.apply(me, {     
			
            tools: [
                {
                    type:'refresh',
                    tooltip: 'Refresh',
                    handler: function(event, toolEl, panel){
                        me.setupContentLoader();
                    }
                },
                {
                    type:'plus',
                    tooltip: 'More details',
                    handler: function(event, toolEl, panel){
                        me.fireEvent('detailsclicked', me, {graphDefinitionId: me.graphDefinitionId, endTime: me.getEndTime(), startTime: me.getStartTime(), title: me.title, step: me.step, hideElements: me.hideElements});
                    }
                },
				{
                    type:'help',
                    tooltip: 'Graph information',
                    handler: function(event, toolEl, panel){
                    }
                }
            ],
			items: {
				xtype: 'container',
				items: [
					img,
					floater
				]
				
			}
        });
		if (me.editable) me.tools.splice(0,0, {
			type: 'gear',
			tooltip: 'Settings',
			handler: function(event, toolEl, panel) {
				me.showConfigEditor();
			}
		});
        if (Ext.isEmpty(me.listeners)) me.listeners = {};        
        Ext.apply(me.listeners, {
            resize: function(portlet, opts) {
                Ext.callback(me.setupContentLoader, me, [], 800);
            }
        });
    
        this.callParent(arguments);
		this.addEvents('detailsclicked', 'imageupdated', 'mousepositioninfo', 'daterangechanged');
        //draw and start refreshing
        Ext.Function.defer(this.refreshRepeat, 500, this);
    },
    alias: 'widget.rrdgraphportlet'
});