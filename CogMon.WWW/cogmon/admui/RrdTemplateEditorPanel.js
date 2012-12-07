//editor for RRD data source template
Ext.define('CogMon.ui.RrdTemplateEditorPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	graphDefinition: null,
	definitionId: null,
	uses: ['Ext.window.Window'],
    statics: {
		openEditorWindow: function(cfg) {
			if (Ext.isEmpty(cfg)) cfg = {};
		}
    },
    loadTemplate: function(tp) {
        if (Ext.isString(gd)) {
			RPC.UserGui.GetGraphDefinition(gd, {
				success: function(ret,e) {
					if (e.status)
					{
						me.loadGraphDefinition(ret);
					}
				}
			});
			return;
		}
		else if (Ext.isObject(gd)) {
		}
    },
    getCurrentTemplateDefinition: function() {
    	var v = this.getForm().getValues();
		v.Id = this.graphDefinitionId;
		v.Defs = Ext.Array.map(this.defStore.getRange(), function(x) { return x.data; });
		v.CVDefs = Ext.Array.map(this.cdefStore.getRange(), function(x) { return x.data; });
		v.Elements = Ext.Array.map(this.elementStore.getRange(), function(x) { return x.data; });
		v.Resolution = Ext.Array.map(this.resolutionStore.getRange(), function(x) { return x.data; });
		return v;
	},
	validate: function() {
	},
	initComponent: function() {
		var me = this;
		var defSt = Ext.create('Ext.data.JsonStore', {
			model: 'CogMon.model.RrdGraphDEF', autoDestroy: true
		});
		var cdefSt = Ext.create('Ext.data.JsonStore', {
			model: 'CogMon.model.RrdGraphCDEF', autoDestroy: true
		});
		var elemSt = Ext.create('Ext.data.JsonStore', {
			model: 'CogMon.model.RrdGraphElement', autoDestroy: true
		});
		var dataSrcSt = Ext.create('Ext.data.DirectStore', {
			directFn: RPC.UserGui.GetRrdDataSources,
			fields: ['Id', 'Description'], idProperty: 'Id', autoDestroy: true, autoLoad: true
		});
		var dataFieldSt = Ext.create('Ext.data.DirectStore', {
			directFn: RPC.UserGui.GetRrdDataSourceFields,
			fields: ['Id', 'Name'], idProperty: 'Id',
			paramOrder: ['dataSourceId'], autoDestroy: true
		});
		var resSt = Ext.create('Ext.data.JsonStore', {
			fields: [{name: 'ResSec', type: 'int'}, {name:'SpanSec', type: 'int'}], autoDestroy: true
		});
		var defCdefSt = Ext.create('Ext.data.ArrayStore', {fields: [{name:'v', type: 'boolean'}, 'name'], data: [[true, 'CDEF'], [false, 'VDEF']], idProperty: 'v'});
		var cfst = Ext.create('Ext.data.Store', {fields:['Id', 'Name'], data: CogMon.ConstDictionaries.RrdConsolidationFunction, autoDestroy: true});
		var cfst2 = Ext.create('Ext.data.Store', {fields:['Id', 'Name'], data: CogMon.ConstDictionaries.RrdConsolidationFunction, autoDestroy: true});
		
		var dsedit = {
			xtype: 'combobox', store: dataSrcSt, valueField: 'Id', displayField: 'Description', allowBlank: false, queryMode: 'local', typeAhead: true, minChars: 2,
			listeners: {
				buffer: 50,
				change: function() {
				  var store = this.store;
				  store.clearFilter();
				  store.filter({
					  property: 'Description',
					  anyMatch: true,
					  value   : this.getValue()
				  });
				}
			}
		};
		Ext.apply(this, {
			defStore: defSt,
			cdefStore: cdefSt,
			elementStore: elemSt,
			resolutionStore: resSt,
			layout: 'fit',
			items: {
				xtype: 'tabpanel', defaults: {xtype: 'panel', border: false}, border: false,
				listeners: {
					tabchange: function(p, nt, ot) {
						if (nt.itemId == 'previewTab') {
							me.showPreview();
						}
					}
				},
				items: [
                    {html: 'a la ma kota'}
				]
			}
		});
		this.callParent(arguments);
	}
});
