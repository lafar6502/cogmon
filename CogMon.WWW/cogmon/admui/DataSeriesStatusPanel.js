Ext.define('CogMon.admui.DataSeriesStatusPanel', {
    extend: 'Ext.grid.Panel',
	requires: [],
	uses: [],
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
                {header: 'Series Id', dataIndex: 'SeriesId'},
                {header: 'Description', dataIndex: 'Description'},
                {header: 'Last update', dataIndex: 'LastUpdate', xtype: 'datecolumn'},
                {header: 'Last update job Id', dataIndex: 'LastUpdateJob'}
            ]
        });
        this.callParent(arguments);
    }
});
