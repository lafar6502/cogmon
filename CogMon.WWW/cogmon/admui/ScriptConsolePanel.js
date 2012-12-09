Ext.define('CogMon.admui.ScriptConsolePanel', {
    extend: 'Ext.panel.Panel',
    requires: [],
    uses: [],
    sendCommand: function() {
        var me = this;
        var ct = this.down('#cmdText').getValue();
        if (Ext.isEmpty(ct)) return;
        //this.down('#console').update('<pre style="color:blue">' + ct + '</pre>');
        this.appendConsoleText(ct, 'blue');
        RPC.AdminGUI.EvalScript(ct, true, {
            success: function(ret, e) {
                if (e.status) {
                    if (ret.Error) {
                        me.appendConsoleText(Ext.isString(ret.Result) ? ret.Result : Ext.encode(ret.Result), 'red');
                    }
                    else {
                        me.appendConsoleText(Ext.isString(ret.Result) ? ret.Result : Ext.encode(ret.Result), 'green');
                    }
                }
                else {
                    me.appendConsoleText("command error", 'red');
                }
            },
            failure: function() {
                log('fail: ' + arguments);
            }
        });
    },
    appendConsoleText: function(txt, color) {
        var el = this.down('#console').body;
        if (Ext.isEmpty(el)) return;
        var nel = Ext.DomHelper.append(el, '<pre style="color:' + color + '">' + txt + '</pre>', true);
        nel.scrollIntoView(el);
    },
    initComponent: function() {
        var me = this;
        Ext.apply(this, {
            layout: 'border',
            items: [
                {xtype: 'panel', itemId: 'console', region: 'center', autoScroll: true},
                {xtype: 'panel', itemId: 'command', region: 'south', height: 200, layout: {type: 'hbox', align: 'stretch'},
                    items: [
                        {xtype: 'textarea', itemId: 'cmdText', flex: 1},
                        {xtype: 'button', text: 'Send', width: 120, handler: function() {me.sendCommand(); }},
                        {xtype: 'button', text: 'Clear', width: 120, handler: function() { me.down('#console').update(''); }}
                    ]
                }
                   
            ]
        });
        this.callParent(arguments);
    }
});