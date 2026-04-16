var functions = {
	InitSDK_js: function(gameKey) {
		
		gameKey = UTF8ToString(gameKey);

		window["SDK_OPTIONS"] = {
			"gameId": gameKey,
			"onEvent": function(event) {
				const eventHandlers = {
					"SDK_GAME_START": () => SendMessage('YG2Instance', 'CloseInterAdv'),
					"SDK_GAME_PAUSE": () => SendMessage('YG2Instance', 'OpenInterAdv'),
					"SDK_ERROR": () => SendMessage('YG2Instance', 'ErrorInterAdv')
				};

				if (eventHandlers[event.name]) {
						eventHandlers[event.name]();
				}
			},
		};
		
		(function(d, s, id) {
			var js, fjs = d.getElementsByTagName(s)[0];
			if (d.getElementById(id)) return;
			js = d.createElement(s);
			js.id = id;
			js.src = 'https://api.gamemonetize.com/sdk.js';
			fjs.parentNode.insertBefore(js, fjs);
		}
		(document, 'script', 'gamemonetize-sdk'));
	},
	
	InterAdvShow_js: function()
	{
		if (typeof sdk !== "undefined")
		{	  
			sdk.showBanner(); 
		}
	}
};

mergeInto(LibraryManager.library, functions);

