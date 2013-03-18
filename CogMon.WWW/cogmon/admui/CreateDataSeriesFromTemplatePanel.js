Ext.define('CogMon.admui.CreateDataSeriesFromTemplatePanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	uses: [],
    defaults: {padding: 5},
    autoScroll: true,
    frame: false,
    statics: {
        showCreateDataSourceWindow: function(template, cfg) {
            if (Ext.isString(template)) {
                return;
            }
            if (Ext.isObject(template)) {
                var fpnl = Ext.create('CogMon.admui.CreateDataSeriesFromTemplatePanel', {template: template});
                var ed = Ext.create('Ext.window.Window', {
                    modal: true, width: 700, layout: 'fit', title: 'Create data series',
                    items: fpnl,
                    buttons: [
                        {
                            text: 'OK', 
                            handler: function() {
                                var r = fpnl.createDataSource(function(res) {
                                    if (res.success) {
                                        alert('data source created: ' + res.dataSource.Id);
                                        ed.close();
                                    }
                                    else {
                                        alert('error: ' + res.error);
                                    }
                                });
                            }
                        },
                        {text: 'Cancel', handler: function() {ed.close(); }}
                    ]
                });
                ed.show();
            }
            else throw "Invalid template";
        }
    },
    createDataSource: function(callback) {
        if (!this.getForm().isValid()) {
            callback({success: false, error: 'Form data invalid'});
            return;
        }
        var msg = {
            TemplateId: this.template.Id,
            Parameters: this.getForm().getValues()
        };
        RPC.AdminGUI.CreateDataSeriesFromTemplate(msg, {
            success: function(ret, e) {
                if (e.status) {
                    callback({success: true, dataSource: ret});
                }
                else {
                    callback({success: false, error: 'Data source creation error'});
                }
            }
        });
    },
    initComponent: function() {
        if (Ext.isEmpty(this.template)) throw "Data source template missing";
        
        flds = [];
        flds.push(Ext.create('Ext.Component', {
            data: this.template,
            tpl: "<h3>Create data source from template '{Name}'</h3><br/><p>{Description}</p>"
        }));
                
        for (var i=0; i<this.template.Variables.length; i++)
        {
            var v = this.template.Variables[i];
            var lb = Ext.isEmpty(v.Description) ? v.Name : v.Name + ' (' + v.Description + ')';
            var fld = Ext.create('Ext.form.field.Text', {
                name: v.Name, value: v.DefaultValue, allowBlank: !v.Required, fieldLabel: lb, padding: 5, labelAlign: 'top', anchor: '100%'
            });
            flds.push(fld);
        }
        Ext.apply(this, {
            items: flds
        });
        
        this.callParent(arguments);
    }
});
