Ext.define('CogMon.admui.NavPanel', {
    extend: 'Ext.tree.Panel',
	requires: [],
	uses: [],
    initComponent: function() {
        var me = this;
        var st = Ext.create('Ext.data.TreeStore', {
            root: {
                expanded: true,
                children: [
                    { text: 'Current status', leaf: true, itemId: 'ShowCurrentStatus'},
                    { text: 'Data sources', expanded: true, children: [
                        { text: 'Data source templates', leaf: true, itemId: 'ShowDataSourceTemplates'},
                        { text: 'RRD data sources', leaf: true, itemId: 'ShowDataSources'}
                    ]},
                    {text: 'Other', expanded: true, children: [
                        {text: 'Users', leaf: true, itemId: 'ShowUsers', command: {}},
                        {text: 'User groups', leaf: true, itemId: 'ShowGroups', command: {name: 'ShowContent', viewClass: 'CogMon.admui.GroupListPanel', cfg: {}}},
                        {text: 'Event categories', leaf: true, itemId: 'ShowEventCategories', command: {name: 'ShowContent', viewClass: 'CogMon.admui.EventCategoryListPanel', cfg: {}}},
                        {text: 'Pre-defined GUI portlets', leaf: true},
                        {text: 'Script console', leaf: true, itemId: 'ShowConsole', command: {name: 'ShowContent', viewClass: 'CogMon.admui.ScriptConsolePanel', cfg: {}}}
       
                    ]},
                    {text: 'Return to reports', leaf: true, href: '..'}
                ]
            }
        });
        Ext.apply(this, {
            store: st,
            rootVisible: false
        });
        if (Ext.isEmpty(this.listeners)) this.listeners= {};
        Ext.apply(this.listeners, {
            itemclick: function(v, tn, el) {
                if (!Ext.isEmpty(tn) && !Ext.isEmpty(tn.raw) && !Ext.isEmpty(tn.raw.itemId))
                {
                    me.fireEvent('navitemclick', me, tn.raw.itemId, tn.raw);
                    console.log(tn.raw);
                }
            }
        });
        this.addEvents('navitemclick');
        this.callParent(arguments);
    }
});