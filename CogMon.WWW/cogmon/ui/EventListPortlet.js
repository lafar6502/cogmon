Ext.define('CogMon.ui.EventListPortlet', {
    extend: 'CogMon.ui.Portlet',
    requires: [
    ],
    autoRefreshInterval: 60,
    title: 'Events',
	setDateRange: function(start, end, suppressNotification) {
        this.setStartTime(start);
        this.setEndTime(end);
		this.refreshData();
		//if (!suppressNotification) this.fireEvent('daterangechanged', this, start, end);
    },
	getDataQueryParams: function() {
		return {
			start: this.getStartTime(),
			end: this.getEndTime()
		};
	},
	refreshData : function() {
		this.eventStore.load({params: this.getDataQueryParams()});
	},
	initComponent: function() {
		var me = this;
        var st = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Text", "CategoryId", "CategoryName", {name: "Timestamp", type: 'date'}, "Color"],
			paramOrder: ['start', 'end'],
            idProperty: 'Id',
            autoLoad: false,
            root: undefined,
            directFn: RPC.UserGui.GetEventsBetween
        });
		
		Ext.apply(me, {     
            tools: [
                {
                    type:'refresh',
                    tooltip: 'Refresh',
                    handler: function(event, toolEl, panel){
                        me.refreshData();
                    }
                },
				{
                    type:'help',
                    tooltip: 'Graph information',
                    handler: function(event, toolEl, panel){
                    }
                }
            ],
			eventStore: st,
			items: {
                xtype: 'gridpanel',
                itemId: 'thegrid',
				emptyText: 'No events in selected date range',
                columns: [
                    {dataIndex: 'Timestamp', header: 'Time', xtype: 'datecolumn', format: 'm/d/Y G:i'},
					{dataIndex: 'CategoryName', header: 'Category'},
                    {
						dataIndex: 'Text', header: 'Description', flex: 1.0,
						renderer: function(v, m, r) {
							return Ext.String.format('<span style="color:#{1};!important;">{0}</span>', v, r.data.Color.slice(0, 6));
						}
					}
                ],
                store: st,
				autoScroll: true
            }
        });
        this.callParent(arguments);
    },
    alias: 'widget.eventlistportlet'
});