Ext.define('CogMon.admui.AdminView', {
    extend: 'Ext.panel.Panel',
	requires: ['Ext.panel.Panel', 'CogMon.admui.AgentStatusPanel', 'CogMon.admui.JobStatusPanel'],
	uses: [],
    alias: 'widget.adminview',
    dummy: false,
    layout: {type: 'border', padding: 5},
    initComponent: function() {
        
        Ext.apply(this, {
            items: [
                Ext.create('Ext.panel.Panel', {region: 'center',
                    layout:'auto',
                    autoScroll:true,
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
                        }]
                }),
                Ext.create('Ext.panel.Panel', {html: 'west', region: 'west', width: 200})
            ]
        });
        this.callParent(arguments);
    }
});
