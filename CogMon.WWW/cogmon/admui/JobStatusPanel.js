Ext.define('CogMon.admui.JobStatusPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
    alias: 'widget.jobstatuspanel',
    initComponent: function() {
        var st = new Ext.data.JsonStore({
            autoDestroy: true,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: 'CurrentJobStatus',
                reader: {
                    type: 'json', idProperty: 'Id'
                }
            },
            fields: ['Id', 'DataSeriesId', 'Group', {name: 'LastRun', type: 'date'}, {name: 'LastSuccessfulRun', type: 'date'}, 'AgentAddress', 'IsError', 'StatusInfo', 'IntervalSeconds']
        });
        Ext.apply(this, {
            store: st,
            columns: [
                {header: 'Job Id', dataIndex: 'Id'},
                {header: 'Job group', dataIndex: 'Group'},
                {header: 'Data series', dataIndex: 'DataSeriesId'},
                {header: 'Last update', dataIndex: 'LastRun', xtype: 'datecolumn'},
                {header: 'Last successful run', dataIndex: 'LastSuccessfulRun', xtype: 'datecolumn'},
                {header: 'Interval (s)', dataIndex: 'IntervalSeconds'},
                {header: 'Agent IP', dataIndex: 'AgentAddress'},
                {header: 'Error?', dataIndex: 'IsError', xtype: 'booleancolumn', trueText: 'Y', falseText: 'N'},
                {header: 'Status', dataIndex: 'StatusInfo'}
            ]
        });
        this.callParent(arguments);
    }
});
