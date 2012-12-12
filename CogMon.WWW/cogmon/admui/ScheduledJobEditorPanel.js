Ext.define('CogMon.admui.ScheduledJobEditorPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	uses: [],
    defaults: {padding: 5},
    autoScroll: true,
    frame: true,
    statics: {
        showJobEditorWindow: function(job, cfg) {
            var w = Ext.create('Ext.window.Window', {
                layout: 'fit', 
                items: Ext.create('CogMon.admui.ScheduledJobEditorPanel', {
                    job: job
                    })
            });
            w.show();
        }
    },
    loadJobData: function(job) {
        if (Ext.isString(job)) {
        }
        else if (Ext.isObject(job0)) {
        }
        else throw "argument";
    },
    initComponent: function() {
        var me = this;
        
        var itmz = [
            {xtype: 'textfield', fieldLabel: 'Description', name: 'Description'},
            {xtype: 'checkbox', fieldLabel: 'Active', name: 'Active'},
            {xtype: 'combobox', fieldLabel: 'Job type', name: 'QueryMethod', store: 'QueryType', valueField: 'Id', displayField: 'Name'},
            {xtype: 'textfield', fieldLabel: 'Poll interval (s)', name: 'IntervalSeconds', allowBlank: false},
            {xtype: 'textfield', fieldLabel: 'Script name', name: 'ScriptName', allowBlank: true},
            {xtype: 'textfield', fieldLabel: 'Arguments', name: 'Arguments', allowBlank: true}
        ];
        
        Ext.apply(this, {
            defaults: {labelAlign: 'top', padding: 5},
            items: itmz
        });
        
        this.callParent(arguments);
    }
});