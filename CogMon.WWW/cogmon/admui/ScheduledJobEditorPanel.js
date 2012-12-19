Ext.define('CogMon.admui.ScheduledJobEditorPanel', {
    extend: 'Ext.form.Panel',
	requires: ['Ext.form.field.*'],
	uses: [],
    
    autoScroll: true,
    frame: true,
    statics: {
        showJobEditorWindow: function(job, cfg) {
            var w = Ext.create('Ext.window.Window', {
                layout: 'fit', width: 600, title: 'Edit job configuration', 
                items: Ext.create('CogMon.admui.ScheduledJobEditorPanel', {
                    job: job, itemId: 'jobpanel'
                    }),
                buttons: [
                    {text: 'Save', handler: function() {
                        var p = w.down('#jobpanel');
                        if (!p.getForm().isValid()) return;
                        var jd = p.getJobData();
                        RPC.AdminGUI.UpdateJob(jd, {
                            success: function(ret, e) {
                                if (e.status) {
                                    w.close();
                                    if (!Ext.isEmpty(cfg.saved)) {
                                        cfg.saved(jd);
                                    }
                                }
                                else {
                                    Ext.MessageBox.alert('Error', 'Error saving job data');
                                }
                            }
                        });
                    }},
                    {text: 'Exit', handler: function() {
                        w.close();
                    }}
                ]
            });
            w.show();
        }
    },
    loadJobData: function(job) {
        var me = this;
        if (Ext.isString(job)) {
            RPC.AdminGUI.GetJobDetails(job, {
                success: function(jd, e) {
                    if (e.status) {
                        me.loadJobData(jd);
                    }
                    else {
                        Ext.MessageBox.alert('Error', 'Error getting job data');
                    }
                }
            });
            return;
        }
        else if (Ext.isObject(job)) {
            me.getForm().setValues(job);
            var vg = me.down('#variableGrid');
            var og = me.down('#optionsGrid');
            if (!Ext.isEmpty(job.VariableNames)) {
                for(var i=0; i<job.VariableNames.length; i++) {
                    vg.store.add({Variable: job.VariableNames[i], VariableRegex: Ext.isEmpty(job.VariableRetrieveRegex) ? '' : job.VariableRetrieveRegex[i]});
                }
            }
            if (!Ext.isEmpty(job.Options)) {
                og.setSource(job.Options);
            }
        }
        else throw "argument";
    },
    getJobData: function() {
        var jd = this.getForm().getValues();
        jd.Options = this.down('#optionsGrid').getSource();
        jd.VariableNames = [];
        jd.VariableRetrieveRegex = [];
        var anyRegex = false;
        this.down('#variableGrid').store.each(function(r) {
            if (!Ext.isEmpty(r.data.Variable)) jd.VariableNames.push(r.data.Variable);
            jd.VariableRetrieveRegex.push(r.data.VariableRegex);
            if (!Ext.isEmpty(r.data.VariableRegex)) anyRegex = true;
        });
        if (!anyRegex) jd.VariableRetrieveRegex = null;
        return jd;
    },
    initComponent: function() {
        var me = this;
        var scf = Ext.create('Ext.data.Store', {
            fields: [{name:'Id', type:'int'}, 'Name'], idProperty: 'Id',
            data: CogMon.ConstDictionaries.QueryType
        });
        var vs = Ext.create('Ext.data.JsonStore', {
            fields:['Variable', 'VariableRegex'], idProperty: 'Variable'
        });
        
        var itmz = [
            {xtype: 'hiddenfield', name: 'Id'},
            {xtype: 'hiddenfield', name: 'TemplateId'},
            {xtype: 'textfield', fieldLabel: 'Description', name: 'Description'},
            {xtype: 'combobox', store: scf, name: 'QueryMethod', valueField: 'Id', displayField: 'Name', fieldLabel: 'Query method'},
            {xtype: 'textfield', fieldLabel: 'Poll interval (s)', name: 'IntervalSeconds', allowBlank: false},
            {xtype: 'textfield', fieldLabel: 'Data source Id', name: 'DataSource', allowBlank: true},
            {xtype: 'textfield', fieldLabel: 'Script name', name: 'ScriptName', allowBlank: true},
            {xtype: 'textfield', fieldLabel: 'Arguments', name: 'Arguments', allowBlank: true},
            {xtype: 'textfield', fieldLabel: 'Job group', name: 'Group', allowBlank: true},
            {xtype: 'checkbox', name: 'Active', fieldLabel: 'Active', inputValue: true},
            {xtype: 'label', text: 'Data source variables'},
            {
                xtype: 'grid', store: vs, itemId: 'variableGrid', height: 200,
                selType: 'cellmodel',
                plugins: [
                    Ext.create('Ext.grid.plugin.CellEditing', {clicksToEdit: 1})
                ],
                columns: [
                    {header: 'Variable', dataIndex: 'Variable', editor: {xtype: 'textfield', allowBlank: false}},
                    {header: 'Value expr/regex', dataIndex: 'VariableRegex', flex:1, editor: {xtype: 'textfield', allowBlank: false}}
                ],
                dockedItems: [
                    {xtype: 'toolbar', items: [
                        {text: 'Add', handler: function() {
                            var gr = me.down('#variableGrid');
                            gr.store.add({Variable: '', VariableRegex: ''});
                        }},
                        {text: 'Remove', handler: function() {
                            var gr = me.down('#variableGrid');
                            var s = gr.getSelectionModel().getLastSelected();
                            if (!Ext.isEmpty(s)) gr.store.remove(s);
                        }}
                    ]}
                ]
            },
            {xtype: 'label', text: 'Options'},
            {
                xtype: 'propertygrid', height: 180, itemId: 'optionsGrid', 
                dockedItems: [
                    {xtype: 'toolbar', items: [
                        {text: 'Add', handler: function() {
                            var gr = me.down('#optionsGrid');
                            Ext.MessageBox.prompt("Add option", "Please specify option name", function(b, v) {
                                if (b != "ok") return;
                                gr.store.add({name: v, value: ''});
                            });
                        }},
                        {text: 'Remove', handler: function() {
                            var gr = me.down('#optionsGrid');
                            var s = gr.getSelectionModel().getLastSelected();
                            if (!Ext.isEmpty(s)) gr.removeProperty(s.data.name);
                        }}
                    ]}
                ],
                source: {}
            }
        ];
        
        Ext.apply(this, {
            defaults: {padding: 5, anchor: '100%'},
            varStore: vs,
            items: itmz
        });
        
        this.callParent(arguments);
        this.on('afterrender', function() {
            if (!Ext.isEmpty(me.job)) me.loadJobData(me.job);
        });
    }
}); 