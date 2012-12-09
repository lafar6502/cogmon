Ext.define('CogMon.admui.DataSeriesStatusPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
    maxHeight: 400,
    alias: 'widget.dataseriesstatuspanel',
    initComponent: function() {
        var st = new Ext.data.JsonStore({
            autoDestroy: true,
            autoLoad: true,
            proxy: {
                type: 'ajax',
                url: 'CurrentDataSourcesStatus',
                reader: {
                    type: 'json', idProperty: 'SeriesId'
                }
            },
            fields: ['SeriesId', 'Description', {name: 'LastUpdate', type: 'date'}, 'LastUpdateJob']
        });
        Ext.apply(this, {
            store: st,
            columns: [
                {header: 'Series Id', dataIndex: 'SeriesId', width: 180},
                {header: 'Description', dataIndex: 'Description', flex: 1},
                {header: 'Last update', dataIndex: 'LastUpdate', xtype: 'datecolumn', format:'Y-m-d H:i:s', width: 180},
                {header: 'Last update job Id', dataIndex: 'LastUpdateJob', width: 180}
            ]
        });
        this.callParent(arguments);
    }
});
