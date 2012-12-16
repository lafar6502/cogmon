if (typeof console === "undefined") {console = { log: function() {} };};
Ext.Loader.setConfig({
	enabled:true,
	paths: {
		'Ext.app' : 'app',
		'CogMon': 'cogmon',
		'Ext.ux' : 'ux'
	}
});
google.load('visualization', '1.0', {'packages':['corechart', 'charteditor']});
google.setOnLoadCallback(function() {
    console.log('google vis api loaded');
});
Ext.onReady(function() {
	Ext.tip.QuickTipManager.init();
	RPC.UserGui.GetUserInfo({
		success: function(ret, e) {
			if (e.status)
			{
				console.log('Starting app');
				Ext.ns('CogMon');
				CogMon.User = ret;
				console.log('Current user is ' + Ext.encode(CogMon.User));
				
				
				
				
				Ext.application({
					name: 'CogMon',
					appFolder: 'cogmon',
					autoCreateViewport: true,
					controllers: ['Main'],
					launch: function () {
						
					}
				});
			}
		}
	});
});

