//editor for RRD data source template
Ext.define('CogMon.admui.UserEditPanel', {
    extend: 'Ext.form.Panel',
	requires: [],
	uses: ['Ext.window.Window'],
    statics: {
		openEditorWindow: function(user, cfg) {
			if (Ext.isEmpty(cfg)) cfg = {};
            var w = Ext.create('Ext.window.Window', {
                width: 400, autoHeight: true,
                layout: 'fit',
                items: Ext.create('CogMon.admui.UserEditPanel', {
                    userData: user
                }),
                buttons: [
                    {text: 'OK'},
                    {text: 'Cancel', handler: function() { w.close(); }}
                ]
            });
            w.show();
		}
    },
    frame: true,
    userData: undefined,
    loadUser: function(usr) {
        this.getForm().setValues(usr);
    },
    getCurrentUserData: function() {
	},
	validate: function() {
	},
	initComponent: function() {
		var me = this;
        var gst = Ext.create('Ext.data.DirectStore', {
            fields: ["Id", "Name", "Description"],
			idProperty: 'Id', autoLoad: true,
            directFn: RPC.AdminGUI.GetUserGroups
        });
        
		Ext.apply(this, {
			groupStore: gst,
			items: [
                {xtype: 'textfield', name: 'Login', allowBlank: false, fieldLabel: 'Login'},
                {xtype: 'textfield', name: 'Email', allowBlank: false, fieldLabel: 'Email'},
                {xtype: 'textfield', name: 'Name', allowBlank: true, fieldLabel: 'User name'},
                {xtype: 'checkbox', name: 'Active', fieldLabel: 'Active'},
                {xtype: 'checkbox', name: 'NeedsSync', fieldLabel: 'Re-sync'},
                {xtype: 'textfield', name: 'ExtId', fieldLabel: 'External ID'},
                {
                    xtype: 'grid', height:100, selType: 'checkboxmodel', multiSelect: true, anchor: '100% -150',
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
