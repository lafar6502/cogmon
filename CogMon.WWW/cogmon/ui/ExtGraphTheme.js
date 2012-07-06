Ext.define('CogMon.ui.ExtGraphTheme', {
});


Ext.chart.theme.CogMon1 = Ext.extend(Ext.chart.theme.Base, {
constructor: function(config) {
	Ext.chart.theme.Base.prototype.constructor.call(this, Ext.apply({
		colors: ["086CA2", "#245A77", "#024365", "#2689BF", "#3C91BF",
"1826B0", "#2F3781", "#07116E", "#3644C8", "#4B57C8",
"FFBA00", "#BB9632", "#9F7400", "#FFC427", "#FFCD46",
"FF8B00", "#BB7C32", "#9F5700", "#FF9C27", "#FFAB46"]
	}, config));
}
});

Ext.chart.theme.CogMon2 = Ext.extend(Ext.chart.theme.Base, {
constructor: function(config) {
	Ext.chart.theme.Base.prototype.constructor.call(this, Ext.apply({
		colors: [
"1826B0", "#2F3781", "#9F7400", "#FF9C27", "#FFAB46",
"FF8B00", "#BB7C32", "#07116E", "#3644C8", "#4B57C8",
"086CA2", "#245A77", "#024365", "#2689BF", "#3C91BF",
"FFBA00", "#BB9632", "#9F5700","#FFC427", "#FFCD46"]
	}, config));
}
});