
var IE9 = false
var _clickBreaker$;
var touchScrollControllers = {};
var _zoomPercent = 100;
var _modConflicts = {};
var _modDeps = {};
var modTabIdx = 50, statusTabIdx = 11;

var mainLocale, modsLocale, statusLocale, settingsLocale;

var _START_VIEW_ID = "Status"

function make$(tag) {
	var items = tag.split(' ')
	var el = document.createElement(items[0])
	for (var i = 1; i < items.length; i++) {
		if (items[i].length) {
			el.classList.add(items[i])
		}
	}
	return $(el)
}

$.fn.onActivate = function(fn) {
	return this.click(fn).keyup(function(e) {
		if (e.key == "Enter" || e.key == "Spacebar" || e.key == " ") {
			fn.apply(this)
			
		}
	}).keydown(function(e) {
		if (e.keyCode === 32 || e.keyCode === 13) {
			e.preventDefault()
		}
	})
}
$.fn.onEnter = function(fn) {
	return this.keyup(function(e) {
		if (e.key == "Enter") {
			fn.apply(this)
		}
	})
}
$.fn.optClass = function(flag, cls) {
	return this[flag ? 'addClass' : 'removeClass'](cls)
};
function strToBool(strval) {
	return strval && (strval == '1' || strval.length == 4)
};

document.oncontextmenu = function(e){
	return false
}

var Dialog = {
	_IsInit: false
	, Init: function() {
		var _self = this
		var touchScrollController = false
		var isShown = false
		var dialog = $('<div class="bb-dialog">').appendTo(document.body)
		var form = $('<div class="bb-dialog-form">').appendTo(dialog)
		var contentWrapper = $('<div class="bb-dialog-contentWrapper">')
		_self.Title$ = $('<div class="bb-dialog-title">')
		_self.Content$ = $('<div class="bb-dialog-content">').appendTo(contentWrapper)
		var close = $('<div class="bb-dialog-closeBtn">').click(function() {
			_self.Hide()
		})

		form.append(_self.Title$, close, contentWrapper)

		_self.SetTitle = function(title) {
			_self.Title$.text(title)
		}
		_self.Show = function(isSmall) {
			isShown = true
			dialog.show()
			form.addClass('small')
			if (!touchScrollController) {
				touchScrollController = addTouchScroll({
					Element: _self.Content$[0]
					, DrawArrows: true
					, DrawScrollBar: true
					, InbarArrows: true
					, Mutable: true
				})
			}
		}
		_self.Hide = function() {
			isShown = false
			dialog.hide()
			form.removeClass('small')
			_self.Content$.empty()
			_self.Title$.empty()
		}
		dialog.hide()
		_self._IsInit = true

		Object.defineProperty(_self, 'IsShown', {
			get: function() { return isShown }
		});
		_self.updateTouchScroll = function() {
			if (isShown && touchScrollController) {
				touchScrollController.TouchScrollCheck()
			}
		}
	}
}

var GalleryDialog = {
	_IsInit: false
	, Init: function() {
		var _self = this
		var touchScrollInit = false
		var curIdx = 0
		var curImgList = []
		var dialog = $('<div class="bb-dialog gallery">').appendTo(document.body)
		var form = $('<div class="bb-dialog-form">').appendTo(dialog)
		var image$ = $('<div class="bb-dialog-image">')
		var close = $('<div class="bb-dialog-closeBtn">').click(function() {
			_self.Hide()
		})
		var prev = $('<div class="bb-dialog-galleryControlBtn prev">').text('<').click(function() {
			_self.Prev()
		})
		var next = $('<div class="bb-dialog-galleryControlBtn next">').text('>').click(function() {
			_self.Next()
		})
		form.append(image$, prev, next, close)

		_self.Show = function(imageList, idx) {
			curIdx = idx || 0
			if (!imageList || curIdx < 0 || curIdx >= imageList.length) {
				return
			}
			curImgList = imageList
			if (curImgList.length > 1) {
				prev.show()
				next.show()
			}
			else {
				prev.hide()
				next.hide()
			}
			updateImg()
			dialog.show()
		}
		_self.Next = function() {
			curIdx++
			if (curIdx == curImgList.length) {
				curIdx = 0
			}
			updateImg()
		}
		_self.Prev = function() {
			if (curIdx == 0) {
				curIdx = curImgList.length
			}
			curIdx--
			updateImg()
		}
		function updateImg() {
			image$.attr('style', 'background-image: url("' + curImgList[curIdx] + '")')
			//image$.attr('src', curImgList[curIdx])
		}
		_self.Hide = function() {
			dialog.hide()
		}
		dialog.hide()
		_self._IsInit = true
	}
}

var progressBar = {
	SetValue: function(value) {
		this._Value = value
		value = Math.floor(this.valSeg * value)
		if (this._IsInit) {
			if (value < 1) {
				this.bar$.hide()
			}
			else {
				this.bar$.show()
				if (value < 8) {
					this.mid.style.width = '0'
					this.start.style.width = '3px'
					this.end.style.width = '4px'
				}
				else if (value < 17) {
					var ev = value % 2
					var half = (value - ev) / 2
					this.mid.style.width = '0'
					this.start.style.width = '' + (half + ev) + 'px'
					this.end.style.width = '' + half + 'px'
				}
				else {
					var ww = value - 16
					if (_zoomPercent != 100) {
						ww--
						//ww = Math.floor(ww / 100 * _zoomPercent)
						//ww = Math.floor(ww / _zoomPercent * 100)
					}
					this.mid.style.width = '' + ww + 'px'
					this.start.style.width = '8px'
					this.end.style.width = '8px'
				}
			}
		}
		else {
			this.InitVal = value
		}
	}
	, Init: function(max) {
		this._MaxValue = max
		this.bar$ = $('.progress .bar')
		this.mid = $('.progress .bar .mid')[0]
		this.start = $('.progress .bar .start')[0]
		this.end = $('.progress .bar .end')[0]

		this._IsInit = true
		this._Value = this.InitVal
		this.UpdateValSeg()
	}
	, UpdateValSeg: function() {
		this._Width = this.bar$.width()
		this.valSeg = this._Width / this._MaxValue
		this.SetValue(this._Value)
	}
	, _IsInit: false
	, InitVal: 0
	, _Value: 0
	, _MaxValue: 0
}
var progressTitle = {
	SetValue: function(value) {
		if (this._IsInit) {
			this.caption$.text(value)
		}
		else {
			this.InitVal = value
		}
	}
	, Init: function() {
		this.caption$ = $('.progress-status .caption')
		this._IsInit = true
		this.SetValue(this.InitVal)
	}
	, _IsInit: false
	, InitVal: ""
}
var launchBtn = {
	Update: function(isReady, isEnabled) {
		this.IsReady = isReady
		this.IsEnabled = isEnabled
		if (this._IsInit) {
			this.element$.optClass(!isEnabled, 'disabled')
			this.caption$.text(mainLocale[isReady ? 'LaunchBtn' : 'UpdateBtn'])
		}
	}
	, Init: function() {
		var jq = this.element$ = $('#LaunchBtn').onActivate(function() {
			if (!this.classList.contains('disabled')) {
				YL.LaunchGame()
			}
		})
		this.caption$ = jq.find('.caption')
		this._IsInit = true
		this.Update(this.IsReady, this.IsEnabled)
	}
	, _IsInit: false
	, IsEnabled: false
	, IsReady: true
}

function onFileActionComboboxSelect() {
	if (!$('#app-content-pages').hasClass("disabled")) {
		var groupidx = parseInt(this.FileElement.attr('groupidx'))
		var fileidx = parseInt(this.FileElement.attr('fileidx'))
		if (this.Value) {
			YL.CheckFile(groupidx, fileidx);
		} else {
			YL.UncheckFile(groupidx, fileidx);
		}
	}
}

function StyledCheckBox(text, isChecked) {
	var containerEl = $('<div class="bbCheckBoxContainer">').bbCode(text)
	var checkBox = $('<div class="bbCheckBox">').prependTo(containerEl)
	var _selfCB = this
	var disabled = false
	_selfCB.Container = containerEl

	this.SetTabIndex = function(tabidx) {
		containerEl.attr('tabindex', tabidx)
	}
	this.Disable = function() {
		containerEl.addClass('disabled')
		disabled = true
	}
	this.SetVal = function(val) {
		containerEl.optClass(_selfCB.IsChecked = val, 'checked')
	}
	this.SetVal(isChecked)

	containerEl.onActivate(function() {
		if (!disabled) {
			containerEl.optClass(_selfCB.IsChecked = !_selfCB.IsChecked, 'checked')
			if (typeof _selfCB.OnChange == 'function') {
				_selfCB.OnChange(_selfCB.IsChecked)
			}
		}
	})
}
function StyledComboBox(selectedIdx, variants) {
	var containerEl = $('<div class="comboBox">');
	var valueDisplay = $('<div class="valueDisplay">').appendTo(containerEl);
	var dropdownContainer = $('<div class="dropdownContainer">').appendTo(containerEl);
	var disabled = false;
	var _selfCB = this
	var lastddv = false
	_selfCB.Container = containerEl
	if (typeof selectedIdx != 'number') {
		var foundIdx = -1
		for (var iv = 0; iv < variants.length; iv++) {
			if (variants[iv].Value == selectedIdx) {
				foundIdx = iv
				break
			}
		}
		selectedIdx = foundIdx
	}
	
	for (var i = 0; i < variants.length; i++) {
		var variant = variants[i];
		var dropdownVariant = $('<div class="dropdownVariant">').appendTo(dropdownContainer).text(variant.Text);
		dropdownVariant[0].__variant = variant
		dropdownVariant[0].__select = function() {
			if (!disabled) {
				if (_selfCB.Value != this.__variant.Value) {
					valueDisplay.empty().text(this.__variant.Text)
					_selfCB.Value = this.__variant.Value
					if (typeof _selfCB.OnSelect == 'function') {
						_selfCB.OnSelect()
					}
					dropdownContainer[0].deferred(function() {
						updateDDPos()
					})
				}
				lastddv = this
				dropdownContainer.toggle()
			}
		}
		dropdownVariant.onActivate(function() {
			this.__select()
		})
		dropdownVariant.on('keypress', function(e) {
			switch (e.key) {
				case 'ArrowUp':
				case 'Up':
					(this.previousSibling || this.parentNode.lastChild || this).focus()
					break;
				case 'ArrowDown':
				case 'Down':
					(this.nextSibling || this.parentNode.firstChild || this).focus()
					break;
			}
		}).on('keydown', function(e) {
			switch (e.keyCode) {
				case 38:
				case 40:
					e.preventDefault()
					break;
			}
		})
		if (i == selectedIdx) {
			dropdownVariant[0].__select()
		}
	}
	if (selectedIdx < 0) {
		valueDisplay.empty().text(" ")
		dropdownContainer.toggle()
	}

	this.SetTabIndex = function(tabidx) {
		valueDisplay.attr('tabindex', tabidx)
	}

	this.Disable = function() {
		containerEl.addClass('disabled')
		disabled = true
	}
	function updateDDPos() {
		var iw = containerEl.innerWidth()
		var style = dropdownContainer[0].style
		style.top = containerEl.innerHeight() + 'px'
		style.minWidth = iw + 'px'
		style.left = (containerEl.outerWidth() - iw) / 2 + 'px'
	}
	dropdownContainer[0].deferred(function() {
		updateDDPos()
	})
	valueDisplay.onActivate(function() {
		if (!disabled) {
			dropdownContainer.toggle()
			dropdownContainer[0].deferred(function() {
				lastddv.focus()
			})
		}
	})
}

function showModDetails(modInfo) {
	var descr$ = $("<div class='modDetailsDescription'>")
	var screenshots$ = null
	if (modInfo.DetailedDescription) {
		descr$.bbCode(modInfo.DetailedDescription)
	}
	else {
		descr$.text(modInfo.Description)
	}
	if (modInfo.Screenshots) {
		try {
			var screenshots = JSON.parse(modInfo.Screenshots)
			if (screenshots.length) {
				var sss = [], ssurl = "", i = 0
				for (i = 0; i < screenshots.length; i++) {
					ssurl = screenshots[i]
					if (isValidURL(ssurl)) {
						sss.push(ssurl)
					}
				}
				if (sss.length) {
					screenshots$ = $("<div class='modDetailsScreenshots'>")
					for (i = 0; i < sss.length; i++) {
						$("<div class='modDetailsScreenshotContainer'>").appendTo(screenshots$).append(
							//$("<img class='modDetailsScreenshot'>").attr('src', sss[i])
							$("<div class='modDetailsScreenshot'>").attr('style', 'background-image:url("' + sss[i] + '")').attr('bbimgidx', '' + i).click(function() {
								GalleryDialog.Show(sss, parseInt(this.getAttribute('bbimgidx')))	
							})
						)
					}
				}
			}
		}
		catch (ex) {
			descr$.append($('<div class="modDetailsError">').text(ex.Message))
		}
	}
	Dialog.SetTitle(modInfo.Name)
	Dialog.Content$.append(descr$, screenshots$)
	Dialog.Show()
}

function showGallery(imgList, idx) {

}

var validUrlPattern = new RegExp('^(https?:\\/\\/)?'+ // protocol
	'((([a-z\\d]([a-z\\d-]*[a-z\\d])*)\\.)+[a-z]{2,}|'+ // domain name
	'((\\d{1,3}\\.){3}\\d{1,3}))'+ // OR ip (v4) address
	'(\\:\\d+)?(\\/[-a-z\\d%_.~+]*)*'+ // port and path
	'(\\?[;&a-z\\d%_.~+=-]*)?'+ // query string
	'(\\#[-a-z\\d_]*)?$','i'); // fragment locator

function isValidURL(str) {
	return !!validUrlPattern.test(str)
}

$(function() {
	body$ = $(document.body)

	if (!document.body.classList) {
		IE9 = true
		Object.defineProperty(HTMLElement.prototype, "classList", {
			get: function () {
				if (!this.__classList) {
					var classes = this.getAttribute("class") || ""
					if (classes) {
						this.__classList = classes.split(' ')
					}
					else {
						this.__classList = []
					}
					this.__classList.__element = this
					this.__classList.update = function() {
						this.__element.className = this.join(' ')
					}
					this.__classList.add = function() {
						for (var i = 0; i < arguments.length; i++) {
							if (this.indexOf(arguments[i]) < 0) {
								this.push(arguments[i])
							}
						}
						this.update()
					}
					this.__classList.remove = function() {
						for (var i = 0; i < arguments.length; i++) {
							var idx = this.indexOf(arguments[i])
							if (idx > -1) {
								this.splice(idx, 1)
							}
						}
						this.update()
					}
					this.__classList.contains = function() {
						for (var i = 0; i < arguments.length; i++) {
							var idx = this.indexOf(arguments[i])
							if (idx > -1) {
								return true
							}
						}
					}
				}
				return this.__classList
			}
		})
	}

	function updateBodySizeClasses() {
		if (window.outerWidth > 1079) {
			body$.addClass('wide')
		}
		else {
			body$.removeClass('wide')
		}
	}

	var scrollableElements = []
	var resizeTimeout = null
	window.addEventListener("resize", function() {
		if (resizeTimeout === null) {
			resizeTimeout = setTimeout(onResize, 10)
		}
	}, false)

	function onResize() {
		clearTimeout(resizeTimeout)
		resizeTimeout = null
		var screl;
		progressBar.UpdateValSeg()
		updateBodySizeClasses()

		for (var i = 0; i < scrollableElements.length; i++) {
			screl = scrollableElements[i]
			if ($(screl).is(':visible')) {
				screl.TouchScrollCheck()
			}
			else {
				screl.__markedToCheck = true
			}
		}
		Dialog.updateTouchScroll()
	}

	_clickBreaker$ = spawn(document.body, 'div', 'touchScrollClickBreaker')
	_clickBreaker$.setAttribute('style', 'left: 0px; top: 0px; height: 100%; width: 100%; position: absolute; z-index: 9001; display: none;')

	Dialog.Init()
	GalleryDialog.Init()

	YL.UpdateAppControlsSize("130", "30")
	var bg = YL.RetrieveBackground()
	document.body.style.backgroundImage = 'url("' + bg + '")'

	var startViewId = YL.RetrieveStartViewId()
	if (startViewId && startViewId.length > 2) {
		_START_VIEW_ID = startViewId
	}

	body$.keyup(function(e) {
		if (e.key == 'Alt') {
			var target = $(e.target)
			if (target.is('.app-control-btn')) {
				launchBtn.element$.focus()
			}
			else if (target.is('.app-btn')) {
				$('.app-controls .help').focus()
			}
		}
	})

	var doReset = true
	$(document).on('focus', '.app-control-btn, .app-btn, .modControlButton, .valueDisplay, .dropdownVariant, .customLink', function (e) {
		$(e.target).addClass('tabbed')
		doReset = true
	});
	$(document).on('keydown mousedown mouseup', function (e) {
		if (doReset) {
			$('.tabbed').removeClass('tabbed')
			doReset = false
		}
	});

	var progressBarState = YL.GetProgressBarState()
	progressTitle.SetValue(progressBarState.Caption)
	progressBar.Init(progressBarState.MaxValue)
	progressBar.SetValue(progressBarState.Progress)
	progressTitle.Init()
	launchBtn.Init()

	var appBtns = $('.app-buttons')
	var views = new (function () {
		var views_ = []
		var lastActive = false
		this.Reg = function(viewId) {
			views_.push({
				Id: viewId
				, Container: $('#' + btnId + 'View')
			})
		}
		this.Show = function(viewId) {
			if (lastActive != viewId) {
				if (lastActive) {
					$('#' + lastActive + 'Btn').removeClass('active')
					$('#' + lastActive + 'View').removeClass('active').hide() //[0].setAttribute('disabled', '1')
				}
				lastActive = viewId
				$('#' + viewId + 'Btn').addClass('active')
				var newViewContent = $('#' + lastActive + 'View').addClass('active').show().find('.article-content')[0] //[0].removeAttribute('disabled')

				if (newViewContent.__markedToCheck) {
					newViewContent.TouchScrollCheck()
				}
			}
		}
	})
	var appBtnsIds = ['Status', 'Mods', 'Changelog', 'Links', 'FAQ', 'Donate', 'Settings']
	var appBtnsMutable = [false, true, true, false, false, false]

	for (var i = 0; i < appBtnsIds.length; i++) {
		var btnId = appBtnsIds[i]
		views.Reg(btnId)
		var btn = $('<div class="app-btn">').appendTo(appBtns)
			.attr('title', mainLocale[btnId + 'Tooltip']).attr('id', btnId + 'Btn').attr('tabindex', i + 3)
			.append(
				$('<div class="caption">').text(mainLocale[btnId + 'Btn'])
			).onActivate(function() {
				views.Show(this.viewId)
				if (this.viewId == 'Mods') {
					touchScrollControllers['Mods'].TouchScrollCheck()
				}
			})
		btn[0].viewId = btnId
	}

	window.onModNeedsDonation = function() {
		views.Show('Donate')
	}

	;(function() {
		// Changelog Init

		var changelogSections = []

		var indesplit = changelograw.split('=====|=|=|=====');
		var changelogReadyCountdown = indesplit.length

		for (var i = 0; i < indesplit.length; i++) {
			var pair = indesplit[i].split('=====Content=====');
			changelogSections.push({ Title: pair[0], Content: pair[1] });
		}

		var items = []
		var selectedItem = null

		var menu = $('#ChangelogView .kb-side-menu')
		var content = $('#ChangelogView .article-content')

		for (var i = 0; i < changelogSections.length; i++) {
			items.push($('<div class="kb-side-item">').attr('acticleId', i).appendTo(menu).text(changelogSections[i].Title).click(function() {
				selectItem($(this))
			}))
		}

		selectItem(items[0])

		function selectItem(item) {
			if (!item.hasClass('active')) {
				if (selectedItem) {
					selectedItem.removeClass('active')
				}
				content[0].innerHTML = changelogSections[item.attr('acticleId')].Content//.html(changelogSections[item.attr('acticleId')].Content)
				item.addClass('active')
				selectedItem = item
			}
		}
	})();

	// FAQ Init
	$('#FAQView .article-content').html(faqraw)

	YL.UpdateStatusData()
	var modCofsAndDeps = null //YL.UpdateModsData()
	if (modCofsAndDeps) {
		try {
			modCofsAndDeps = JSON.parse(modCofsAndDeps)
			_modConflicts = modCofsAndDeps.Conflicts
			_modDeps = modCofsAndDeps.Dependencies
		}
		catch (ex) {
			Dialog.SetTitle('Ошибка')
			Dialog.Content$.append(
				$('<p>').text(ex.message)
				, $('<p>').bbCode(ex.stack)
			)
			Dialog.Show()
		}
	}

	for (var i = 0; i < appBtnsIds.length; i++) {
		(function(btnid) {
			var viewsel = '#' + btnid + 'View'
			var el = $(viewsel + ' .article-content')
			if (el.length) {
				scrollableElements.push(el[0])
				touchScrollControllers[btnid] = addTouchScroll({
					Element: el[0]
					, DrawArrows: true
					, DrawScrollBar: true
					, InbarArrows: true
					, Mutable: appBtnsMutable[i]
					, OnReady: function() {
						if (btnid == _START_VIEW_ID) {
							views.Show(btnid)
							$('.preloader').hide()
						}
						else {
							$(viewsel).hide()
						}
					}
				})
			}
		})(appBtnsIds[i]);
	}

	;(function() {
		// Settings Init

		var content = $('#SettingsView .article-content')
		content.append($('<h2>').text(settingsLocale.SettingsTitle))
		try {
			var lsstr = YL.Options.GetCurrentSettings()
			var launchSettings = JSON.parse(lsstr)

			var loncherStartPanelCB = new StyledComboBox('' + launchSettings.StartPage, [
				{ Text: settingsLocale.SettingsOpeningPanelChangelog, Value: "0" }
				, { Text: settingsLocale.SettingsOpeningPanelStatus, Value: "1" }
				, { Text: settingsLocale.SettingsOpeningPanelMods, Value: "3" }
				//, { Text: "Links", Value: "2" }
				//, { Text: "FAQ", Value: "4" }
			])
			loncherStartPanelCB.OnSelect = function() {
				YL.Options.SelectStartPage(parseInt(this.Value))
			}
			loncherStartPanelCB.SetTabIndex(10)

			//alert(lsstr)

			var loncherLoggingLevelCB = new StyledComboBox('' + launchSettings.LoggingLevel, [
				{ Text: "Отключить логирование", Value: "-1" }
				, { Text: "Основная информация", Value: "0" }
				, { Text: "Расширенная информация", Value: "1" }
				, { Text: "Тайминги запуска", Value: "2" }
				, { Text: "Вообще всё", Value: "3" }
			])
			loncherLoggingLevelCB.OnSelect = function() {
				YL.Options.SetLoggingLevel(parseInt(this.Value))
			}
			loncherLoggingLevelCB.SetTabIndex(11)

			var gameDirInput = $('<div class="gameDirInput">').text(launchSettings.GameDir)
			$('<p>').appendTo(content).append(
				$('<div class="label">').text(settingsLocale.SettingsGamePath)
				, $('<div class="bbButton browse">').text(settingsLocale.Browse).onActivate(function() {
					var newpath = YL.Options.BrowseGamePath()
					if (newpath) {
						gameDirInput.text(newpath)
					}
				})
				, gameDirInput
			)

			loncherStartPanelCB.Container.addClass('loncherStartPanelCB')
			$('<p>').appendTo(content).append(
				$('<div class="label">').text(settingsLocale.SettingsOpeningPanel)
				, loncherStartPanelCB.Container
			)
			
			var cb = new StyledCheckBox(settingsLocale.SettingsGogGalaxy, strToBool(launchSettings.LaunchFromGalaxy))
			cb.OnChange = function(isChecked) {
				YL.Options.CheckLaunchFromGalaxy(isChecked)
			}
			cb.Container.appendTo($('<p>').appendTo(content))

			cb = new StyledCheckBox(settingsLocale.SettingsCloseOnLaunch, strToBool(launchSettings.CloseOnLaunch))
			cb.OnChange = function(isChecked) {
				YL.Options.CheckCloseOnLaunch(isChecked)
			}
			cb.Container.appendTo($('<p>').appendTo(content))

			cb = new StyledCheckBox(settingsLocale.SettingsOfflineMode, strToBool(launchSettings.StartOffline))
			cb.OnChange = function(isChecked) {
				YL.Options.CheckOffline(isChecked)
			}
			cb.Container.appendTo($('<p>').appendTo(content))

			cb = new StyledCheckBox(settingsLocale.SettingsHiddenMods, strToBool(launchSettings.ShowHiddenMods))
			cb.OnChange = function(isChecked) {
				YL.Options.CheckShowHiddenMods(isChecked)
			}
			cb.Container.appendTo($('<p>').appendTo(content))

			cb = new StyledCheckBox(settingsLocale.SettingsModsCompactMode, strToBool(launchSettings.ModsCompactMode))
			cb.OnChange = function(isChecked) {
				YL.Options.CheckShowModsCompactMode(isChecked)
			}
			cb.Container.appendTo($('<p>').appendTo(content))

			_zoomPercent = parseInt(launchSettings.ZoomPercent)
			var zoomInput = $('<input class="zoomInput">').val(_zoomPercent).onEnter(function() {
				var vvv = parseInt(zoomInput.val())
				if (!isNaN(vvv)) {
					_zoomPercent = YL.Options.SetZoom(vvv)
					zoomInput.val(_zoomPercent)
				}
			})

			content.append(
				$('<p>').append(
					$('<div class="label">').text("Масштаб интерфейса, %")
					, $('<div class="bbButton zoomBtn">').text('-').onActivate(function() {
						_zoomPercent = YL.Options.SetZoom(_zoomPercent - 5)
						zoomInput.val(_zoomPercent)
					})
					, zoomInput
					, $('<div class="bbButton zoomBtn">').text('+').onActivate(function() {
						_zoomPercent = YL.Options.SetZoom(_zoomPercent + 5)
						zoomInput.val(_zoomPercent)
					})
				)
			)

			loncherLoggingLevelCB.Container.addClass('loncherLoggingLevelCB')
			$('<p>').appendTo(content).append(
				$('<div class="label">').text("Логирование (в файл \\loncherData\\log.txt)")
				, loncherLoggingLevelCB.Container
			)

			content.append(
				$('<p>').append(
					$('<div class="bbButton">').text(settingsLocale.SettingsOpenDataFolder).onActivate(function() {
						YL.Options.OpenDataFolder()
					})
				)
				, $('<p>').append(
					$('<div class="bbButton">').text(settingsLocale.SettingsMakeBackup).onActivate(function() {
						YL.Options.MakeBackup()
					})
				)
				, $('<p>').append(
					$('<div class="bbButton">').text(settingsLocale.SettingsCreateShortcut).onActivate(function() {
						YL.Options.CreateShortcut()
					})
				)
				, $('<p>').append(
					$('<div class="bbButton uninstall">').text(settingsLocale.SettingsUninstallLoncher).onActivate(function() {
						YL.Options.UninstallLoncher()
					})
				)
			)
		}
		catch (ex) {
			content.append(
				$('<p>').text(ex.message)
				, $('<p>').bbCode(ex.stack)
			)
		}
	})();

	setTimeout(function() {
		YL.CheckModUpdates()
		updateBodySizeClasses()
	}, 10)
});

function YLExtInit() {
	mainLocale = YL.GetLocs(
		'LaunchBtn, UpdateBtn, Close, Minimize, Help, About, ChangelogBtn, LinksBtn, DonateBtn, StatusBtn, SettingsBtn, FAQBtn, ModsBtn'
		+ ', ChangelogTooltip, StatusTooltip, LinksTooltip, DonateTooltip, SettingsTooltip, ModsTooltip, FAQTooltip'
	);

	modsLocale = YL.GetLocs(
		'ModInstallationInProgress, InstallMod, EnableMod, DisableMod, UninstallMod, NeedsDonationMod'
		+ ', NoModsForThisVersion, ModDetailedInfo, ModConflictsHintTitle, ModConflictsHintTextSingle'
		+ ', ModConflictsHintTextPlural, ModDependenciesHintTitle, ModDependenciesHintTextSingle'
		+ ', ModDependenciesHintTextPlural, ModDependenciesHintOR, ModNotesHintTitle'
	);

	statusLocale = YL.GetLocs(
		'StatusListDownloadedFile, StatusListDownloadedFileTooltip, StatusListRecommendedFile, StatusListRecommendedFileTooltip,'
		+ ', StatusListOptionalFile, StatusListOptionalFileTooltip, StatusListRequiredFile, StatusListOptionalFileTooltip'
		+ ', StatusComboboxDownload, StatusComboboxDownloadForced, StatusComboboxNoDownload, StatusComboboxUpdate'
		+ ', StatusComboboxUpdateForced, StatusComboboxNoUpdate'
	);

	settingsLocale = YL.GetLocs(
		'SettingsTitle, SettingsGamePath, Browse, SettingsOpeningPanel, SettingsOpeningPanelChangelog,'
		+ ', SettingsOpeningPanelStatus, SettingsOpeningPanelLinks, SettingsOpeningPanelMods, SettingsCloseOnLaunch'
		+ ', SettingsGogGalaxy, SettingsOfflineMode, SettingsHiddenMods, SettingsModsCompactMode, SettingsCreateShortcut'
		+ ', SettingsOpenDataFolder, SettingsMakeBackup, SettingsUninstallLoncher'
	);

	YL.On('ProgressBarUpdate', function(event) {
		progressBar.SetValue(event.Progress)
		if (typeof event.Text == 'string') {
			progressTitle.SetValue(event.Text)
		}
	})

	YL.On('LaunchButtonUpdate', function(event) {
		launchBtn.Update(event.IsReady, event.Enabled)
	})

	YL.On('ModsViewUpdate', function(modsData) {
		var modsContent$ = $('#ModsView .article-content') //touchScrollControllers['Mods']
		modsContent$.empty()
		if (!modsData) {
			$("<div class='noMods'>").appendTo(modsContent$).text("No ModsData provided")
			return
		}
		if (!(modsData.ModList instanceof Array)) {
			$("<div class='noMods'>").appendTo(modsContent$).text("ModsData contains no valid ModList")
			return
		}
		if (modsData.ModList.length) {
			modTabIdx = 50
			if (modsData.ModsCompactMode) {
				modsContent$.addClass('compact')
			}
			else {
				modsContent$.removeClass('compact')
			}
			var depi = 0
			var namelessGroup$ = $("<div class='modsGroup'>").appendTo(modsContent$)
			var groups = {}
			for (var i = 0; i < modsData.ModList.length; i++) {
				;(function(modInfo) {
					var modEntry$ = make$("div modEntry")
					if (modsData.ModsCompactMode) {
						modEntry$.addClass('compact')
					}

					if (modInfo.Group) {
						var group = groups[modInfo.Group.Id]
						if (group) {
							modEntry$.appendTo(group)
						}
						else {
							groups[modInfo.Group.Id] = make$("div modsGroup").appendTo(modsContent$).append(
								make$("div modsGroupTitle").text(modInfo.Group.Name)
								, modEntry$
							)
						}
					}
					else {
						modEntry$.appendTo(namelessGroup$)
					}
					
					var modControls$ = make$("div modControls")
					var modWarnings$ = make$("div modWarnings")
					var modDetails$ = make$("div modDesc").bbCode(modInfo.Description)
					modEntry$[0].modId = modInfo.Id
					modEntry$.append(
						make$("div modTitle").text(modInfo.Name)
						, modDetails$
						, modControls$
						, modWarnings$
					)

					if (modInfo.Screenshots || modInfo.DetailedDescription) {
						make$("div modControlButton details").text(modsLocale["ModDetailedInfo"]).onActivate(function() {
							showModDetails(this.__modInfo)
						}).attr('tabindex', modTabIdx++).appendTo(modDetails$)[0].__modInfo = modInfo
					}

					if (modInfo.DlInProgress) {
						modEntry$.addClass('loading')
						make$("div modLoadingLabel").text(modsLocale["ModInstallationInProgress"]).appendTo(modControls$)
					}
					else if (modInfo.NeedsDonation) {
						createModBtn(modControls$, 'NeedsDonation')
					}
					else if (!modInfo.Installed) {
						//createModBtn(modControls$, 'Install')

						make$("div modControlButton install").attr({
							tabindex: modTabIdx++
							, title: modsLocale["InstallMod"]
						}).text(modsLocale["InstallMod"]).appendTo(modControls$).onActivate(function() {
							var id = modInfo.Id
							var this_el = this
							switch (modInfo.Versions.length) {
								case 0:
									$(this_el).remove()
									break
								case 1:
									YL.ModInstall(id, modInfo.Versions[0].Id)
									break
								default: {
									Dialog.SetTitle("Выберите версию мода")
									var versionList$ = make$("div modVersionList")
									for (var vi = 0; vi < modInfo.Versions.length; vi++) {
										;(function(version) {
											var verModEntry$ = make$("div modEntry").appendTo(versionList$)
											var vmodControls$ = make$("div modControls")
											
											make$("div modControlButton install").attr({
												tabindex: modTabIdx++
												, title: modsLocale["InstallMod"]
											}).text(modsLocale["InstallMod"]).appendTo(vmodControls$).onActivate(function() {
												YL.ModInstall(id, version.Id)
												Dialog.Hide()
											})
											
											verModEntry$.append(
												make$("div modTitle").text(version.Name)
												, make$("div modDesc").bbCode(version.Description)
												, vmodControls$
												, make$("div modWarnings")
											)
										})(modInfo.Versions[vi])
									}
									Dialog.Content$.append(versionList$)
									Dialog.Show()
									break
								}
							}
						})
					}
					else {
						if (modInfo.Active) {
							modEntry$.addClass("active")
							createModBtn(modControls$, 'Disable')
						}
						else {
							modEntry$.addClass("inactive")
							createModBtn(modControls$, 'Enable')
						}
						modEntry$.addClass("installed")
						createModBtn(modControls$, 'Uninstall')
					}

					if (!modInfo.DlInProgress && modInfo.IsUpdateAvailable) {
						addModWarning('needsUpdate', modWarnings$, modsLocale["ModNeedsUpdateHintTitle"], function() {
							
						})
					}
					if (modInfo.ImportantNotes) {
						addModWarning('notesWarning', modWarnings$, modsLocale["ModNotesHintTitle"], modInfo.ImportantNotes)
					}
					if (modInfo.Conflicts && modInfo.Conflicts.length) {
						var conflictText = modsLocale[modInfo.Conflicts.length > 1 ? "ModConflictsHintTextPlural" : "ModConflictsHintTextSingle"]
						addModWarning('conflictWarning', modWarnings$, modsLocale["ModConflictsHintTitle"], conflictText + modInfo.Conflicts.join('[n]'))
					}
					if (modInfo.Dependencies && modInfo.Dependencies.length) {
						var depstr = modsLocale[modInfo.Dependencies.length > 1 ? "ModDependenciesHintTextPlural" : "ModDependenciesHintTextSingle"]
						var orstr = ' ' + modsLocale["ModDependenciesHintOR"] + ' '
						var depmodlist = ""
						for (depi = 0; depi < modInfo.Dependencies.length; depi++) {
							depmodlist += modInfo.Dependencies[depi].join(orstr) + '[n]'
						}
						addModWarning('dependencyWarning', modWarnings$, modsLocale["ModDependenciesHintTitle"], depstr + depmodlist)
					}
				})(modsData.ModList[i])
			}

			function createModBtn(modControls$, key) {
				var caption = modsLocale[key + "Mod"];
				make$("div modControlButton " + key.toLowerCase()).attr({
					tabindex: modTabIdx++
					, title: caption
				}).text(caption).appendTo(modControls$).onActivate(function() {
					var id = $(this).parents('.modEntry')[0].modId
					YL['Mod' + key](id)
				})
			}
		}
		else {
			make$("div noMods").appendTo(modsContent$).text(modsLocale["NoModsForThisVersion"])
		}

		requestAnimationFrame(function() {
			touchScrollControllers["Mods"].TouchScrollCheck()
		})
	});

	function addModWarning(wclass, parent, wheader, wtext) {
		$('<div>').attr('title', wheader).addClass(wclass).appendTo(parent).click(function() {
			if (typeof wtext == 'function') {
				wtext()
			}
			else {
				Dialog.SetTitle(wheader)
				Dialog.Content$.bbCode(wtext)
				Dialog.Show(true)
			}
		})
	}

	YL.On('StatusViewUpdate', function(gameVersion) {
		var statusContent = $('#StatusView .article-content')
		statusContent.empty()
		if (!gameVersion) {
			$("<div class='noMods'>").appendTo(statusContent).text("Нет данных о версии игры. Скорее всего, русификатор недоступен для вашей версии игры.")
		}
		else {// if (gameVersion.length) {
			function appendFiles(container$, files, groupIdx) {
				for (var i = 0; i < files.length; i++) {
					var fi = files[i]
					var tooltip = fi.Tooltip
					var fileElement = $("<div class='fileEntry' fileidx='" + i + "'>").appendTo(container$)
					if (fi.IsOK) {
						fileElement.addClass("fileok")
						statusText = statusLocale["StatusListDownloadedFile"]
						if (!tooltip) {
							tooltip = statusLocale["StatusListDownloadedFileTooltip"]
						}
						$("<div class='statusIndicator'>").text(statusText).appendTo(fileElement)
					}
					else {
						var options;
						if (!tooltip) {
							if (fi.Importance === 1 && fi.IsPresent) {
								tooltip = statusLocale["StatusListRecommendedFileTooltip"]
							}
							else if (fi.Importance > 1) {
								tooltip = statusLocale["StatusListOptionalFileTooltip"]
							}
							else {
								tooltip = statusLocale["StatusListRequiredFileTooltip"]
							}
						}
						if (fi.IsPresent) {
							if (fi.Importance > 0) {
								options = [{ Text: statusLocale['StatusComboboxUpdate'], Value: true }, { Text: statusLocale['StatusComboboxNoUpdate'], Value: false }]
							}
							else {
								options = [{ Text: statusLocale['StatusComboboxUpdateForced'], Value: true }]
							}
						}
						else {
							if (fi.Importance > 1) {
								options = [{ Text: statusLocale['StatusComboboxDownload'], Value: true }, { Text: statusLocale['StatusComboboxNoDownload'], Value: false }]
							}
							else {
								options = [{ Text: statusLocale['StatusComboboxDownloadForced'], Value: true }]
							}
						}
						var fileActionCombobox = $("<div class='fileActionCombobox' fileidx='" + i + "' groupidx='" + groupIdx + "'>").appendTo(fileElement)

						var cb = new StyledComboBox(fi.IsCheckedToDl ? 0 : 1, options)
						cb.Container.appendTo(fileActionCombobox)
						if (options.length < 2) {
							cb.Disable()
						} else {
							cb.FileElement = fileActionCombobox
							cb.OnSelect = onFileActionComboboxSelect
							cb.SetTabIndex(statusTabIdx++)
						}
					}
					fileElement.attr('title', tooltip).append(
						$("<div class='fileName'>").text(fi.Description || fi.Path)
					);
				}
			}
			try {
				statusTabIdx = 11
				for (var i = 0; i < gameVersion.FileGroups.length; i++) {
					var fg = gameVersion.FileGroups[i]
					var spoiler = $("<div class='group-spoiler-button'>").appendTo(statusContent).click(function() {
						var tsc = touchScrollControllers['Status']
						if (this._expanded = !this._expanded) {
							this._spoilerContent.slideDown(150, function() { tsc.TouchScrollCheck() })
							this._spoilerIndicator.html('-&nbsp;')
						} else {
							this._spoilerContent.slideUp(150, function() { tsc.TouchScrollCheck() })
							this._spoilerIndicator.html('+&nbsp;')
						}
					})
					spoiler[0]._spoilerIndicator = $("<span class='spoilersym'>-&nbsp;</span>").appendTo(spoiler)
					$("<span class='group-name'>").text(fg.Name).appendTo(spoiler)

					spoiler[0]._spoilerContent = $("<div class='group-spoiler'>").appendTo(statusContent)
					spoiler[0]._expanded = true
					appendFiles(spoiler[0]._spoilerContent, fg.Files, i);
					$("<div class='spoilerdash'>").appendTo(statusContent)
				}
				if (gameVersion.Files.length) {
					appendFiles($("<div class='version-files'>").appendTo(statusContent), gameVersion.Files, -1);
				}
			}
			catch (ex) {
				$("<div class='noMods'>").appendTo(statusContent).text(ex)
			}
		}
	});
}
