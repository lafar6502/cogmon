Ext.define('CogMon.admui.JobStatusPanel', {
    extend: 'Ext.grid.Panel',
	requires: ['CogMon.admui.ScheduledJobEditorPanel'],
	uses: [],
    alias: 'widget.jobstatuspanel',
    maxHeight: 300,
    initComponent: function() {
        var me = this;
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
            fields: ['Id', 'DataSeriesId', 'Group', {name: 'LastRun', type: 'date'}, {name: 'LastSuccessfulRun', type: 'date'}, 'AgentAddress', 'IsError', 'StatusInfo', 'IntervalSeconds', 'LastExecTimeMs']
        });
        Ext.apply(this, {
            store: st,
            columns: [
                {header: 'Job Id', dataIndex: 'Id', width: 180},
                {header: 'Job group', dataIndex: 'Group'},
                {header: 'Data series', dataIndex: 'DataSeriesId', width: 180},
                {header: 'Last update', dataIndex: 'LastRun', xtype: 'datecolumn', format:'Y-m-d H:i:s', width: 180},
                {header: 'Last successful run', dataIndex: 'LastSuccessfulRun', xtype: 'datecolumn', format:'Y-m-d H:i:s', width: 180},
                {header: 'Last execution time (ms)', dataIndex: 'LastExecTimeMs', width: 100},
                {header: 'Poll interval (s)', dataIndex: 'IntervalSeconds'},
                {header: 'Agent IP', dataIndex: 'AgentAddress'},
                {header: 'Error?', dataIndex: 'IsError', xtype: 'booleancolumn', trueText: 'Y', falseText: 'N'},
                {header: 'Status', dataIndex: 'StatusInfo', flex: 1}
            ],
            viewConfig: {
                getRowClass: function(r, idx, rowParams, store) {
                    if (r.data.IsError) return 'status_row_error';
                    if (r.data.StatusInfo == "Not reported yet" && !r.data.IsError) return 'status_row_inactive';
                    if (r.data.StatusInfo == "OK" && !r.data.IsError) return 'status_row_ok';
                    return null;
                }
            },
            dockedItems: [
                {xtype: 'toolbar',
                    items: [
                        {text: 'Edit job', 
                            handler: function() { 
                                var sj = me.getSelectionModel().getLastSelected();
                                if (Ext.isEmpty(sj)) return;
                                CogMon.admui.ScheduledJobEditorPanel.showJobEditorWindow(sj.data.Id, {});
                            }
                        }
                    ]
                }
            ]
        });
        this.callParent(arguments);
    }
});
