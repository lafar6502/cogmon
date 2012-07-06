Ext.Loader.setConfig({
	enabled:true,
	paths: {
		'Ext.app' : '../app',
		'CogMon': '../cogmon',
		'Ext.ux' : '../ux'
	}
});
	
Ext.onReady(function() {
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
					appFolder: '../cogmon',
					autoCreateViewport: true,
					controllers: ['MainAdmin'],
					launch: function () {
						console.log('app launch');
					}
				});
			}
		}
	});
});

