Ext.onReady(function() {
	Ext.Loader.setConfig({enabled:true});
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

