Ext.define('AppUtil', {
	statics: {
		isGraphDateRelative: function(d) {
			if (!Ext.isString(d)) return false;
			return false;
		},
		parseGraphDate: function(dt, refDate) {
			var isRef = false;
			if (Ext.isDate(dt)) return dt;
			if (Ext.isNumber(dt)) {
				return new Date(dt * 1000);
			}
			if (Ext.isString(dt)) {
				var re = /((s|e|now|start|end)$|(s|e|now|start|end)?(\+|\-)(\d+)(y|year|years|m|month|months|wk|week|weeks|h|hour|hours|d|day|days)$)/;
				var m = dt.match(re);
				if (m) {
					if (!Ext.isEmpty(m[2]))
					{
						if (m[2] === 'now') 
							refDate = new Date();
						else 
							if (!Ext.isDate(refDate)) return null;
						return refDate;
					}
					if (!Ext.isEmpty(m[3]) && m[3] !== 'now') 
					{
						if (!Ext.isDate(refDate)) return null;
					}
					else refDate = new Date();
					var num = (m[4] === '-' ? -1 : 1) * Ext.Number.from(m[5]);
					return AppUtil.moveDateByRange(refDate, num, m[6].charAt(0));
				}
				//alert('parsing date: ' + dt);
				return Ext.Date.parse(dt, 'd/m/Y g:i', false);
			}
			return null;
		},
		parseDateRange: function(start, end) {
			//1. check if relative
			var sd, ed;
			if (Ext.isString(start)) {
				if (start.match(/^s|e|start|end/))
				{
					if (Ext.isString(end) && end.match(/^s|e|start|end/)) return [null, null];
					ed = AppUtil.parseGraphDate(end, null);
					sd = AppUtil.parseGraphDate(start, ed);
					return [sd, ed];
				}
			}
			sd = AppUtil.parseGraphDate(start, null);
			ed = AppUtil.parseGraphDate(end, sd);
			return [sd, ed];
		},
		moveDateByRange: function(dt, num, unit) {
			var intervals = {
				'd' : Ext.Date.DAY,
				'm' : Ext.Date.MONTH,
				'y' : Ext.Date.YEAR,
				'h' : Ext.Date.HOUR,
				'wk' : Ext.Date.DAY
			};
			var u = intervals[unit];
			if (Ext.isEmpty(u)) {
				u = unit;
			} else if (unit === 'wk') {
				num = num * 7;
			}
			return Ext.Date.add(dt, u, num);
		}
	}
});