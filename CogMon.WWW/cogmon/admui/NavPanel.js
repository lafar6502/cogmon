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
                    { text: 'Templates', expanded: true, children: [
                        { text: 'Data source templates', leaf: true, itemId: 'ShowDataSourceTemplates'},
                        { text: 'Graph templates', leaf: true, itemId: 'ShowGraphTemplates'}
                    ]},
                    {text: 'Other', children: [
                        {text: 'User groups', leaf: true},
                        {text: 'Event categories', leaf: true},
                        {text: 'Pre-defined GUI portlets', leaf: true}
                    ]}
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