Ext.define('CogMon.admui.CogmonStatusPanel', {
    extend: 'Ext.panel.Panel',
	requires: [],
	uses: [],
    initComponent: function() {
        Ext.apply(this, {
            layout:'auto', autoScroll:true,
            defaults: {
                layout: 'fit',
                padding: 10,
            },
            items: [{
                    title: 'Current agent status',
                    items: {xtype: 'agentstatuspanel'}
                },{
                    title: 'Current job status',
                    items: {xtype: 'jobstatuspanel'}
                },{
                    title: 'Status of data sources',
                    items: {xtype: 'dataseriesstatuspanel'}
                }
            ]
        });
        this.callParent(arguments);
    }
});
