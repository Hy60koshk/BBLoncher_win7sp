
function __onOk() {
	YL.__runBufFunc('onOk')
}
function __onYes() {
	YL.__runBufFunc('onYes')
}
function __onNo() {
	YL.__runBufFunc('onNo')
}
function __updateModsView(data) {
	YL.__fire('ModsViewUpdate', JSON.parse(data))
}
function __updateStatusView(jsondata) {
	var data = JSON.parse(jsondata);
	YL.__fire('LaunchButtonUpdate', data.LaunchBtn)
	YL.__fire('StatusViewUpdate', data.GameVersion)
}
function __updateProgressBar(progress, labelText) {
	YL.__fire('ProgressBarUpdate', { Progress: progress, Text: labelText})
}
function __updateLaunchBtn(data) {
	YL.__fire('LaunchButtonUpdate', JSON.parse(data))
}

var YL = new (function () {

	this.GetLoc = function (key) {
		if (precachedLocale.hasOwnProperty(key)) {
			return precachedLocale[key]
		}
		return precachedLocale[key] = window.external.GetLoc(key)
	}
	this.GetLocs = function (keys) {
		var res = JSON.parse(window.external.GetLocs(keys))
		for (var key in res) {
			if (res.hasOwnProperty(key) && typeof res[key] == 'string') {
				precachedLocale[key] = res[key]
			}
		}
		return res
	}

	var _self = this
	var precachedLocale = {}
	var _handlers = {}
	var __buffunc;

	this.__fire = function (eventName, data) {
		if (_handlers.hasOwnProperty(eventName)) {
			var hs = _handlers[eventName]
			for (var i = 0; i < hs.length; hs[i++](data));
		}
	}
	this.On = function (eventName, handler) {
		if (!_handlers.hasOwnProperty(eventName)) {
			_handlers[eventName] = []
		}
		_handlers[eventName].push(handler)
	}
	
	function setBufFunc() {
		__buffunc = {}
		for (var i = 0; i < arguments.length; i += 2) {
			__buffunc[arguments[i]] = arguments[i + 1]
		}
	}
	function runBufFunc(funcName) {
		if (__buffunc) {
			if (__buffunc.hasOwnProperty(funcName)) {
				__buffunc[funcName]()
				__buffunc = null
			}
			else {
				_self.Warn('Вызов отсутствующей буферной функции "' + funcName + '". Сообщите об этом происшествии авторам лаунчера.')
			}
		}
		else {
			_self.Warn('Вызов буферной функции при пустом буфере. Сообщите об этом происшествии авторам лаунчера.')
		}
	}
	this.__runBufFunc = function (funcName) {
		runBufFunc(funcName)
	}

	this.Warn = function () {
		switch (arguments.length) {
			case 1:
				window.external.Warn(arguments[0])
				break
			case 2:
				setBufFunc('onOk', arguments[1])
				window.external.Warn(arguments[0], '__onOk')
				break
			case 3:
				setBufFunc('onYes', arguments[1], 'onNo', arguments[2])
				window.external.Warn(arguments[0], '__onYes', '__onNo')
				break
		}
	}
	this.Info = function () {
		switch (arguments.length) {
			case 1:
				window.external.Info(arguments[0])
				break
			case 2:
				setBufFunc('onOk', arguments[1])
				window.external.Info(arguments[0], '__onOk')
				break
		}
	}
	this.ShowMessage = function () {
		switch (arguments.length) {
			case 1:
				window.external.Info(arguments[0])
				break
			case 2:
				setBufFunc('onOk', arguments[1])
				window.external.Info(arguments[0], '__onOk')
				break
			case 3:
				setBufFunc('onYes', arguments[1], 'onNo', arguments[2])
				window.external.Ask(arguments[0], '__onYes', '__onNo')
				break
		}
	}

	this.RetrieveBackground = function () {
		return window.external.RetrieveBackground()
	}
	this.RetrieveStartViewId = function () {
		return window.external.RetrieveStartViewId()
	}
	this.UpdateAppControlsSize = function (width, height) {
		return window.external.UpdateAppControlsSize(width, height)
	}
	this.GetProgressBarMax = function () {
		return window.external.GetProgressBarMax()
	}
	this.GetProgressBarState = function () {
		return JSON.parse(window.external.GetProgressBarState())
	}

	this.CheckFile = function (groupidx, fileidx) {
		window.external.CheckFile(groupidx, fileidx)
	}
	this.UncheckFile = function (groupidx, fileidx) {
		window.external.UncheckFile(groupidx, fileidx)
	}

	this.AppClose = function () {
		window.external.Close()
	}
	this.AppMinimize = function () {
		window.external.Minimize()
	}
	this.AppHelp = function () {
		window.external.Help()
	}
	this.AppSettings = function () {
		window.external.Settings()
	}

	this.LaunchGame = function () {
		window.external.LaunchGame()
	}

	this.ModInstall = function (id, verId) {
		window.external.ModInstall(id, verId)
	}
	this.ModUninstall = function (id) {
		window.external.ModUninstall(id)
	}
	this.ModEnable = function (id) {
		window.external.ModEnable(id)
	}
	this.ModDisable = function (id) {
		window.external.ModDisable(id)
	}
	this.ModNeedsDonation = function (id) {
		//window.external.ModDisable(idx)
		window.onModNeedsDonation()
	}

	this.UpdateModsData = function () {
		return window.external.GetWebModsData()
	}
	this.UpdateStatusData = function () {
		window.external.UpdateStatusWebView()
	}
	this.CheckModUpdates = function () {
		window.external.CheckModUpdates()
	}
	this.CheckModUpdate = function (modId) {
		window.external.CheckModUpdate(modId)
	}

	this.Options = {
		GetCurrentSettings: function () {
			return window.external.OptionsGetCurrentSettings()
		}
		, SetZoom: function (zoomPercent) {
			return window.external.OptionsSetZoom(zoomPercent)
		}
		, SetLoggingLevel: function (loglevel) {
			window.external.OptionsSetLoggingLevel(loglevel)
		}
		, CheckOffline: function (isChecked) {
			return window.external.OptionsCheckOffline(isChecked)
		}
		, CheckLaunchFromGalaxy: function (isChecked) {
			return window.external.OptionsCheckLaunchFromGalaxy(isChecked)
		}
		, CheckCloseOnLaunch: function (isChecked) {
			return window.external.OptionsCheckCloseOnLaunch(isChecked)
		}
		, SelectStartPage: function (pageId) {
			return window.external.OptionsSelectStartPage(pageId)
		}
		, CheckShowHiddenMods: function (pageId) {
			return window.external.OptionsCheckShowHiddenMods(pageId)
		}
		, CheckShowModsCompactMode: function (pageId) {
			return window.external.OptionsCheckModsCompactMode(pageId)
		}
		, BrowseGamePath: function () {
			return window.external.OptionsBrowseGamePath()
		}
		, OpenDataFolder: function () {
			return window.external.OptionsOpenDataFolder()
		}
		, UninstallRussifier: function () {
			window.external.OptionsUninstallRussifier()
		}
		, UninstallLoncher: function () {
			window.external.OptionsUninstallLoncher()
		}
		, CreateShortcut: function () {
			window.external.OptionsCreateShortcut()
		}
		, MakeBackup: function () {
			window.external.OptionsMakeBackup()
		}
	}
})();