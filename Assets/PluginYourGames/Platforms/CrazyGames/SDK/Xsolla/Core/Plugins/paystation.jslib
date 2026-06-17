mergeInto(LibraryManager.library, {

    OpenPayStationWidget: function (token, sandbox, sdkType, engineVersion, sdkVersion, applePayMerchantDomain, appearanceJson) {
        var jsToken = UTF8ToString(token);
        var isSandbox = sandbox > 0 ? true : false;
        var appearance = JSON.parse(UTF8ToString(appearanceJson));

        var options = {
            access_token: jsToken,
            sandbox: isSandbox,
            lightbox: {
                closeByClick: false,
                closeByKeyboard: false,
                width: '740px',
                height: '760px',
                spinner: 'round',
                spinnerColor: '#cccccc'
            },
            queryParams: {
                sdk: UTF8ToString(sdkType),
                sdk_v: UTF8ToString(sdkVersion),
                engine: 'unity',
                engine_v: UTF8ToString(engineVersion),
                browser_type: 'iframe',
                build_platform: 'webglplayer'
            }
        };

        if (appearance.iframeOnly != null) {
            options.iframeOnly = appearance.iframeOnly;
        }

        if (appearance.widgetWidth != null && appearance.widgetWidth !== "") {
            options.lightbox.width = appearance.widgetWidth;
        }

        if (appearance.widgetHeight != null && appearance.widgetHeight !== "") {
            options.lightbox.height = appearance.widgetHeight;
        }

        if (appearance.zIndex != null) {
            options.lightbox.zIndex = appearance.zIndex;
        }

        if (appearance.overlayOpacity != null) {
            options.lightbox.overlayOpacity = appearance.overlayOpacity;
        }

        if (appearance.overlayBackgroundColor != null && appearance.overlayBackgroundColor !== "") {
            options.lightbox.overlayBackground = appearance.overlayBackgroundColor;
        }

        if (appearance.isModal != null) {
            options.lightbox.modal = appearance.isModal;
        }

        if (appearance.closeByClickOverlay != null) {
            options.lightbox.closeByClick = appearance.closeByClickOverlay;
        }

        if (appearance.closeByKeyboardEscape != null) {
            options.lightbox.closeByKeyboard = appearance.closeByKeyboardEscape;
        }

        if (appearance.contentBackgroundColor != null && appearance.contentBackgroundColor !== "") {
            options.lightbox.contentBackground = appearance.contentBackgroundColor;
        }

        if (appearance.contentMargin != null && appearance.contentMargin !== "") {
            options.lightbox.contentMargin = appearance.contentMargin;
        }

        if (appearance.spinnerType != null && appearance.spinnerType !== "") {
            options.lightbox.spinner = appearance.spinnerType;
        }

        if (appearance.spinnerColor != null && appearance.spinnerColor !== "") {
            options.lightbox.spinnerColor = appearance.spinnerColor;
        }

        if (appearance.spinnerUrl != null && appearance.spinnerUrl !== "") {
            options.lightbox.spinnerUrl = appearance.spinnerUrl;
        }

        if (appearance.spinnerRotationPeriod != null) {
            options.lightbox.spinnerRotationPeriod = appearance.spinnerRotationPeriod;
        }

        var jsParentDomain = UTF8ToString(applePayMerchantDomain);
        if (jsParentDomain !== "") {
            options.queryParams.parent_domain = jsParentDomain;
        }

        var s = document.createElement('script');
        s.type = "text/javascript";
        s.async = true;
        s.src = "https://cdn.xsolla.net/payments-bucket-prod/embed/1.5.0/widget.min.js";

        s.addEventListener('load', function (e) {
            XPayStationWidget.on(XPayStationWidget.eventTypes.STATUS, function (event, data) {
                Module.SendMessage('XsollaWebCallbacks', 'PublishPaymentStatusUpdate');

                var status = null;
                if (data && data.paymentInfo && data.paymentInfo.status) {
                    status = data.paymentInfo.status;
                }

                console.log('[Xsolla] Payment status:', status);

                if (status === 'done') {
                    console.log('[Xsolla] Payment completed successfully — closing widget...');
                    if (typeof XPayStationWidget !== 'undefined') {
                        XPayStationWidget.off();
                    }

                    var elements = document.getElementsByClassName('xpaystation-widget-lightbox');
                    for (var i = 0; i < elements.length; i++) {
                        elements[i].style.display = 'none';
                        elements[i].remove();
                    }
                }
            });

            XPayStationWidget.on(XPayStationWidget.eventTypes.CLOSE, function (event, data) {
                if (data === 'undefined') {
                    Module.SendMessage('XsollaWebCallbacks', 'PublishPaymentCancel');
                }
                else {
                    Module.SendMessage('XsollaWebCallbacks', 'PublishPaymentStatusUpdate');
                }
            });

            XPayStationWidget.init(options);
            XPayStationWidget.open();
        }, false);

        var head = document.getElementsByTagName('head')[0];
        head.appendChild(s);
    },

    ClosePayStationWidget: function () {
        if (typeof XPayStationWidget !== 'undefined') {
            XPayStationWidget.off();
        }

        var elements = document.getElementsByClassName('xpaystation-widget-lightbox');
        for (var i = 0; i < elements.length; i++) {
            elements[i].style.display = 'none';
            elements[i].remove();
        }
    },

    ShowPopupAndOpenPayStation: function (url) {
        window.location.href = UTF8ToString(url);
    },

    GetUserAgent: function () {
        var str = navigator.userAgent;
        var lengthBytes = lengthBytesUTF8(str) + 1;
        var stringOnWasmHeap = _malloc(lengthBytes);
        stringToUTF8(str, stringOnWasmHeap, lengthBytes);
        return stringOnWasmHeap;
    },

    GetBrowserLanguage: function () {
        var language = navigator.language || navigator.userLanguage;
        var length = lengthBytesUTF8(language) + 1;
        var buffer = _malloc(length);
        stringToUTF8(language, buffer, length);
        return buffer;
    },
});
