//editor for RRD data source template
Ext.define('CogMon.admui.UserEditPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	uses: ['Ext.window.Window'],
    statics: {
		openEditorWindow: function(user, cfg) {
			if (Ext.isEmpty(cfg)) cfg = {};
            var w = Ext.create('Ext.window.Window', {
                width: 450, autoHeight: true, title: 'Edit user information',
                layout: 'fit',
                items: Ext.create('CogMon.admui.UserEditPanel', {
                    itemId: 'userpnl', 
                    userData: user
                }),
                buttons: [
                    {text: 'OK', 
                        handler: function() {
                            var p = w.down('#userpnl');
                            if (!p.validate()) return;
                            var usr = p.getCurrentUserData();
                            if (!Ext.isEmpty(cfg.save)) {
                                cfg.save(usr);
                                w.close();
                            }
                            else {
                                RPC.AdminGUI.SaveUser(usr, {
                                    success: function(ret, e) {
                                        w.close();
                                        if (!Ext.isEmpty(cfg.saved)) cfg.saved();
                                    }
                                });
                            }
                        }
                    },
                    {text: 'Cancel', handler: function() { w.close(); }}
                ]
            });
            w.show();
		}
    },
    frame: true,
    userData: undefined,
    loadUser: function(usr) {
        var me = this;
        this.getForm().setValues(usr);
        sel=[];
        this.groupStore.each(function(r) {
            if (Ext.Array.contains(usr.MemberOf, r.data.Id)) sel.push(r);
        });
        me.down('#groups').getSelectionModel().select(sel);
    },
    getCurrentUserData: function() {
        var usr = this.getForm().getValues();
        var rows = this.down('#groups').getSelectionModel().getSelection();
        usr.MemberOf = [];
        Ext.Array.each(rows, function(r) {usr.MemberOf.push(r.data.Id);});
        return usr;
	},
    
	validate: function() {
        return this.getForm().isValid();
	},
	initComponent: function() {
		var me = this;
        var gst = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Name", "Description"],
			idProperty: 'Id', autoLoad: true,
            directFn: RPC.AdminGUI.GetUserGroups
        });
        gst.on('load', function(s, recs, success) {
            if (!success) return;
            if (Ext.isEmpty(me.userData) || Ext.isEmpty(me.userData.MemberOf)) return;
            var gr = me.down('#groups');
            if (Ext.isEmpty(gr)) return;
            var selr = Ext.Array.filter(recs, function(r) {
                return Ext.Array.contains(me.userData.MemberOf, r.data.Id);
            });
            me.down('#groups').getSelectionModel().select(selr);
        });
        
		Ext.apply(this, {
			groupStore: gst,
            defaults: {anchor: '100%'},
			items: [
                {xtype: 'hiddenfield', name: 'Id'},
                {xtype: 'textfield', name: 'Login', allowBlank: false, fieldLabel: 'Login'},
                {xtype: 'textfield', name: 'Email', allowBlank: false, fieldLabel: 'Email'},
                {xtype: 'textfield', name: 'Name', allowBlank: true, fieldLabel: 'User name'},
                {xtype: 'checkbox', name: 'Active', fieldLabel: 'Active', inputValue: true},
                {xtype: 'checkbox', name: 'NeedsSync', fieldLabel: 'Re-sync', inputValue: true},
                {xtype: 'textfield', name: 'ExtId', fieldLabel: 'External ID'},
                {
                    xtype: 'grid', height:200, selType: 'checkboxmodel', multiSelect: true, anchor: '100% -150', itemId: 'groups',
                    store: gst,
                    columns: [
                        {header: 'Group Name', dataIndex: 'Name', flex: 1}
                    ]
                }
            ]
		});
		this.callParent(arguments);
        this.on('afterrender', function() {
            if (!Ext.isEmpty(me.userData)) me.loadUser(me.userData);
        })
	}
});
