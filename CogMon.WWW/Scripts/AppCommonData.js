﻿Ext.define('CogMon.model.IdName', {
    extend: 'Ext.data.Model',
    fields: ['Id', 'Name'],
	idProperty: 'Id'
});

Ext.define('CogMon.ConstDictionaries', {
	statics: {
		RrdConsolidationFunction: [
			{Id: 0, Name: 'MIN'},
			{Id: 1, Name: 'MAX'},
			{Id: 2, Name: 'AVERAGE'},
			{Id: 3, Name: 'LAST'},
			{Id: 4, Name: 'HWPREDICT'},
			{Id: 5, Name: 'MHWPREDICT'},
			{Id: 6, Name: 'DEVPREDICT'},
			{Id: 7, Name: 'DEVSEASONAL'},
			{Id: 8, Name: 'FAILURES'}
		],
		GraphElementType: [
			{Id: 0, Name: 'LINE1'},
			{Id: 1, Name: 'LINE2'},
			{Id: 2, Name: 'LINE3'},
			{Id: 3, Name: 'PRINT'},
			{Id: 4, Name: 'GPRINT'},
			{Id: 5, Name: 'COMMENT'},
			{Id: 6, Name: 'VRULE'},
			{Id: 7, Name: 'HRULE'},
			{Id: 8, Name: 'AREA'},
			{Id: 9, Name: 'TICK'},
			{Id: 10, Name: 'SHIFT'},
			{Id: 11, Name: 'TEXTALIGN'}
		],
        QueryType: [
            {Id: 0, Name: 'HttpGet'},
            {Id: 1, Name: 'BooScript'},
            {Id: 2, Name: 'SystemCommand'},
            {Id: 3, Name: 'SqlSelect'},
            {Id: 4, Name: 'WinPerf'},
            {Id: 5, Name: 'LogParse'},
            {Id: 6, Name: 'MongoMapReduce'},
            {Id: 7, Name: 'InlineScript'},
            {Id: 8, Name: 'AgentPerfCnt'},
            {Id: 9, Name: 'ServerPerfCnt'},
            {Id: 10, Name: 'SNMP'}
        ],
        RRDFieldType: [
            {Id: 0, Name: 'GAUGE'},
            {Id: 1, Name: 'COUNTER'},
            {Id: 2, Name: 'DERIVE'},
            {Id: 3, Name: 'ABSOLUTE'},
            {Id: 4, Name: 'COMPUTE'}
        ]
	}
});


var scf = Ext.create('Ext.data.Store', {
	storeId: 'rrdConsolidationFunctions', fields: ['Id', 'Name'], idProperty: 'Id',
	data: CogMon.ConstDictionaries.RrdConsolidationFunction, autoDestroy: true
});

var scf = Ext.create('Ext.data.Store', {
	storeId: 'rrdGraphOperations', fields: [{name:'Id', type:'int'}, 'Name'], idProperty: 'Id',
	data: CogMon.ConstDictionaries.GraphElementType
});

var scf = Ext.create('Ext.data.Store', {
	storeId: 'QueryType', fields: [{name:'Id', type:'int'}, 'Name'], idProperty: 'Id',
	data: CogMon.ConstDictionaries.QueryType
});


Ext.define('CogMon.model.RrdGraphElement', {
    extend: 'Ext.data.Model',
    fields: [{name: 'Op', type: 'int', useNull: true}, 'Value', 'Color', 'Legend', 'Params']
});

Ext.define('CogMon.model.RrdGraphDEF', {
    extend: 'Ext.data.Model',
    fields: ['DataSourceId', 'Field', 'Variable', 'Start', 'End', 'CF', 'ReduceCF'],
	idProperty: 'Variable'
});

Ext.define('CogMon.model.RrdGraphCDEF', {
    extend: 'Ext.data.Model',
    fields: [{name:'CDEF', type: 'boolean'}, 'Variable', 'Expression'],
	idProperty: 'Variable'
});

var st = Ext.create('Ext.data.DirectStore', {
	storeId: 'eventCategories', fields: ["Id", "Name", "Color"],
	idProperty: 'Id',
	autoLoad: true,
	root: undefined,
	directFn: RPC.UserGui.GetAllEventCategories
});

var st = Ext.create('Ext.data.DirectStore', {
	storeId: 'userGroups', fields: ["Id", "Name"],
	idProperty: 'Id',
	autoLoad: true,
	root: undefined,
	directFn: RPC.UserGui.GetGroups
});

Ext.define('CogMon.model.RrdGraphListEntry', {
	extend: 'Ext.data.Model',
	fields: ['Id', 'Title', 'OwnerId', 'OwnerName', 'TemplateId', 'IsMine'], idProperty: 'Id'
});

var st = Ext.create('Ext.data.DirectStore', {
	storeId: 'rrdGraphList', model: 'CogMon.model.RrdGraphListEntry',
	directFn: RPC.UserGui.GetRrdGraphsVisibleToMe, autoLoad: true
});
