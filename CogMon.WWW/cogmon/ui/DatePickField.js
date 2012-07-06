// 'add portlet' panel for selecting a portlet from a list
Ext.define('CogMon.ui.DatePickField', {
    extend: 'Ext.form.field.Picker',
    alias: 'widget.graphdatefield',
	requires: [],
	uses: ['Ext.picker.Date'],
    format: 'm/d/Y H:i',
    createPicker: function() {
        var me = this,
        format = Ext.String.format;

        return Ext.create('Ext.picker.Date', {
            pickerField: me,
            ownerCt: me.ownerCt,
            renderTo: Ext.getBody(),
            floating: true,
            focusOnShow: true,
            listeners: {
                select: function(m, d) {
                    var v = Ext.isEmpty(d) ? '' : (d == 'now' ? d : Ext.Date.dateFormat(d, me.format));
                    me.setValue(v);
                    me.fireEvent('select', me, v);
                    me.collapse();
                }                
            },
            selectToday : function(){
                var mt = this;
                mt.setValue(Ext.Date.clearTime(new Date()));
                mt.fireEvent('select', mt, 'now');
                if (mt.handler) {
                    mt.handler.call(mt.scope || mt, mt, 'now');
                }
                mt.onSelect();
                return mt;
            },
            keyNavConfig: {
                esc: function() {
                    me.collapse();
                }
            }
        });
    },
    initComponent: function() {
		var me = this;
        Ext.apply(me, {
            matchFieldWidth: false
        });
		this.callParent(arguments);
	}
});
