
var IE9 = false

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
$.fn.optClass = function (flag, cls) {
	return this[flag ? 'addClass' : 'removeClass'](cls)
};

document.oncontextmenu = function(e){
	return false
}

var Dialog = {
	_IsInit: false
	, Init: function() {
		var _self = this
		var touchScrollInit = false
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
		_self.Show = function() {
			dialog.show()
			if (!touchScrollInit) {
				addTouchScroll({
					Element: _self.Content$[0]
					, DrawArrows: true
					, DrawScrollBar: true
					, InbarArrows: true
					, Mutable: true
				})
				touchScrollInit = true
			}
		}
		_self.Hide = function() {
			dialog.hide()
			_self.Content$.empty()
			_self.Title$.empty()
		}
		dialog.hide()
		_self._IsInit = true
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
					this.mid.style.width = '' + (value - 16) + 'px'
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
		this.bar$ = $('.progress .bar')
		this.valSeg = this.bar$.width() / max
		this.mid = $('.progress .bar .mid')[0]
		this.start = $('.progress .bar .start')[0]
		this.end = $('.progress .bar .end')[0]

		this._IsInit = true
		this.SetValue(this.InitVal)
	}
	, _IsInit: false
	, InitVal: 0
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

function StyledComboBox(selectedIdx, variants) {
	var containerEl = $('<div class="comboBox">');
	var valueDisplay = $('<div class="valueDisplay">').appendTo(containerEl);
	var dropdownContainer = $('<div class="dropdownContainer">').appendTo(containerEl);
	var disabled = false;
	var _selfCB = this
	var lastddv = false
	_selfCB.Container = containerEl
	
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
			//$(lastddv).trigger('focus')
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

	Dialog.Init()
	GalleryDialog.Init()

	YL.UpdateAppControlsSize("130", "26")
	var bg = YL.RetrieveBackground()
	document.body.style.backgroundImage = 'url("' + bg + '")'

	var startViewId = YL.RetrieveStartViewId()
	if (startViewId && startViewId.length > 2) {
		_START_VIEW_ID = startViewId
	}

	$(document.body).keyup(function(e) {
		if (e.key == 'Alt') {
			var target = $(e.target)
			if (target.is('.app-control-btn')) {
				launchBtn.element$.focus()
			}
			else if (target.is('.app-btn')) {
				$('.app-controls .help').focus()
			}
			//else if (target.is('.app-btn')) {
			//	$('.app-controls .help').focus()
			//}
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
				$('#' + lastActive + 'View').addClass('active').show() //[0].removeAttribute('disabled')
			}
		}
	})
	var appBtnsIds = ['Status', 'Mods', 'Changelog', 'Links', 'FAQ']
	var appBtnsMutable = [false, false, true, false, false]

	for (var i = 0; i < appBtnsIds.length; i++) {
		var btnId = appBtnsIds[i]
		views.Reg(btnId)
		var btn = $('<div class="app-btn">').appendTo(appBtns)
			.attr('title', mainLocale[btnId + 'Tooltip']).attr('id', btnId + 'Btn').attr('tabindex', i + 3)
			.append(
				$('<div class="caption">').text(mainLocale[btnId + 'Btn'])
			).onActivate(function() {
				views.Show(this.viewId)
			})
		btn[0].viewId = btnId
	}
	$('#SettingsBtn').attr('title', mainLocale['SettingsTooltip']).onActivate(function() {
		YL.AppSettings()
	})
	$('#SettingsBtn .caption').text(mainLocale['SettingsBtn'])


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
				content.html(changelogSections[item.attr('acticleId')].Content)
				item.addClass('active')
				selectedItem = item
			}
		}
	})();

	// FAQ Init
	$('#FAQView .article-content').html(faqraw)

	YL.UpdateStatusData()
	YL.UpdateModsData()

	for (var i = 0; i < appBtnsIds.length; i++) {
		(function(btnid) {
			var viewsel = '#' + btnid + 'View'
			var el = $(viewsel + ' .article-content')
			if (el.length) {
				addTouchScroll({
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
});

$.fn.bbCode = function (text) {
	if (!this.length) {
		return this
	}
	if (typeof text !== 'string') {
		return this.text(text)
	}
	this.contents().remove()

	var bbRoot = this
	var bbCurrentParent = this
	var parseStart = 0
	var cursor = 0
	var bracketEnd = 0
	var tagLen = 0
	var bbTagName = ""
	var classLine = ""
	var eqidx = 0
	var bbvalue = ""

	while (true) {
		cursor = text.indexOf('[', cursor)
		if (cursor < 0) {
			addText(text.substring(parseStart))
			break;
		}
		bracketEnd = text.indexOf(']', cursor)
		if (bracketEnd < 0) {
			addText(text.substring(parseStart))
			break;
		}
		tagLen = bracketEnd - cursor - 1
		if (tagLen == 1) {
			bbTagName = text.charAt(cursor + 1).toUpperCase()
			switch (bbTagName) {
				case 'B':
				case 'I':
				case 'S':
				case 'U':
					putLastText()
					bbCurrentParent = $('<' + bbTagName + '>').appendTo(bbCurrentParent)
					break;
				case 'N':
					putLastText()
					$('<br>').appendTo(bbCurrentParent)
					break;
			}
		}
		else if (tagLen == 2) {
			bbTagName = text.substring(cursor + 1, bracketEnd).toUpperCase()
			switch (bbTagName) {
				case 'UL':
				case 'LI':
					putLastText()
					bbCurrentParent = $('<' + bbTagName + '>').appendTo(bbCurrentParent)
					break;
				case '/B':
				case '/I':
				case '/S':
				case '/U':
					closeTag(bbTagName.charAt(1))
					break;
			}
		}
		else if (tagLen == 3) {
			bbTagName = text.substring(cursor + 1, bracketEnd).toUpperCase()
			switch (bbTagName) {
				case '/UL':
				case '/LI':
					closeTag(bbTagName.substring(1))
					break;
				case '/HL':
					closeTag('SPAN')
					break;
			}
		}
		else {
			classLine = text.substring(cursor + 1, bracketEnd)
			eqidx = classLine.indexOf('=')
			if (eqidx > -1) {
				bbTagName = classLine.substring(0, eqidx).toUpperCase()
				bbvalue = classLine.substring(eqidx + 1)
				switch (bbTagName) {
					case 'COLOR':
						if (/^#[a-fA-F0-9]{6}$/.test(bbvalue)) {
							putLastText()
							bbCurrentParent = $('<span class="bb-coloredText" style="color: ' + bbvalue + '">').appendTo(bbCurrentParent)
						}
						break;
					case 'HL':
						if (/^#[a-fA-F0-9]{6}$/.test(bbvalue)) {
							putLastText()
							bbCurrentParent = $('<span class="bb-highlight" style="background-color: ' + bbvalue + '">').appendTo(bbCurrentParent)
						}
						break;
					case 'URL':
						if (isValidURL(bbvalue)) {
							putLastText()
							bbCurrentParent = $('<a class="bb-url" href="' + bbvalue + '">').appendTo(bbCurrentParent)
						}
						break;
				}
			}
			else {
				bbTagName = classLine.toUpperCase()
				switch (bbTagName) {
					case '/COLOR':
						closeTag('SPAN')
						break;
					case '/URL':
						closeTag('A')
						break;
				}
			}
		}
		cursor = bracketEnd + 1
	}

	function closeTag(tagName) {
		var target = bbCurrentParent[0]
		var root = bbRoot[0]
		while (target != root && target.tagName != tagName) {
			target = target.parentNode
		}
		if (target != root) {
			putLastText()
			bbCurrentParent = $(target.parentNode)
		}
	}
	function addText(val) {
		if (val.length > 3) {
			val = val.split(/<br>|<br\/>|\r\n|\n/)
			bbCurrentParent.append(document.createTextNode(val[0]))
			for (var i = 1; i < val.length; i++) {
				bbCurrentParent.append($('<br>'), document.createTextNode(val[i]))
			}
		}
		else {
			bbCurrentParent.append(document.createTextNode(val))
		}
	}
	function putLastText() {
		var line = text.substring(parseStart, cursor)
		if (line.length) {
			addText(line)
		}
		parseStart = bracketEnd + 1
	}

	return this
};