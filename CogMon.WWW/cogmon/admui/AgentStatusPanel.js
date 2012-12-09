Ext.define('CogMon.admui.AgentStatusPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
    alias: 'widget.agentstatuspanel',
    initComponent: function() {
        var st = new Ext.data.JsonStore({
            autoDestroy: true,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: 'CurrentAgentStatus',
                reader: {
                    type: 'json', idProperty: 'Id'
                }
            },
            fields: ['Id', 'AgentAddress', 'Groups', 'AgentPID', 'StatusInfo', {name: 'LastSeen', type: 'date'}]
        });
        Ext.apply(this, {
            store: st,
            columns: [
                {header: 'IP', dataIndex: 'AgentAddress'},
                {header: 'Process Id', dataIndex: 'AgentPID'},
                {header: 'Group', dataIndex: 'Groups'},
                {header: 'Last seen', dataIndex: 'LastSeen', xtype: 'datecolumn', format:'Y-m-d H:i:s', width: 200},
                {header: 'Status', dataIndex: 'StatusInfo', flex: 1}
            ]
        });
        this.callParent(arguments);
    }
});
