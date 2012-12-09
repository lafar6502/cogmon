Ext.define('CogMon.admui.AdminView', {
    extend: 'Ext.panel.Panel',
	requires: ['Ext.panel.Panel', 'CogMon.admui.AgentStatusPanel', 'CogMon.admui.JobStatusPanel', 'CogMon.admui.DataSeriesStatusPanel', 'CogMon.admui.NavPanel'],
	uses: [],
    alias: 'widget.adminview',
    dummy: false,
    layout: {type: 'border', padding: 5},
    //creates (like ext.create)
    replaceContentPanel: function(itemClass, cfg) {
        this.remove('ContentPanel', true);
        Ext.apply(cfg, {itemId: 'ContentPanel', region: 'center'});
        var cp = Ext.create(itemClass, cfg);
        this.add(cp);
    },
    handleNavCommand: function(cmd, item) {
        if (cmd == "ShowDataSourceTemplates") {
            this.replaceContentPanel('CogMon.admui.DataSourceTemplateListPanel', {});
        }
        else if (cmd == "ShowGraphTemplates") {
            this.replaceContentPanel('CogMon.admui.GraphTemplateListPanel', {});
        }
        else if (cmd == "ShowCurrentStatus") {
            this.replaceContentPanel('CogMon.admui.CogmonStatusPanel', {});
        }
        else {
            alert('unknown command: ' + cmd);
        }
    },
    initComponent: function() {
        var me = this;
        Ext.apply(this, {
            defaults: {},
            items: [
                Ext.create('CogMon.admui.CogmonStatusPanel', {region: 'center', itemId: 'ContentPanel'}),
                Ext.create('CogMon.admui.NavPanel', {
                    region: 'west', width: 250,
                    listeners: {
                        navitemclick: function(np, itemid, item) {
                            me.handleNavCommand(itemid, item);
                        }
                    }
                })
            ]
        });
        this.callParent(arguments);
    }
});
