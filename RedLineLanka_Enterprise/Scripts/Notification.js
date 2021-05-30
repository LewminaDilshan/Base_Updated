$.ajaxSetup({ cache: false });

(function ($) {

    // jQuery on an empty object, we are going to use this as our Queue
    var ajaxQueue = $({});

    $.ajaxQueue = function (ajaxOpts, beforeFunc) {
        var jqXHR,
            dfd = $.Deferred(),
            promise = dfd.promise();

        // queue our ajax request
        ajaxQueue.queue(doRequest);

        // add the abort method
        promise.abort = function (statusText) {

            // proxy abort to the jqXHR if it is active
            if (jqXHR) {
                return jqXHR.abort(statusText);
            }

            // if there wasn't already a jqXHR we need to remove from queue
            var queue = ajaxQueue.queue(),
                index = $.inArray(doRequest, queue);

            if (index > -1) {
                queue.splice(index, 1);
            }

            // and then reject the deferred
            dfd.rejectWith(ajaxOpts.context || ajaxOpts, [promise, statusText, ""]);
            return promise;
        };

        // run the actual query
        function doRequest(next) {
            if (beforeFunc) {
                beforeFunc();
            }

            jqXHR = $.ajax(ajaxOpts)
                .done(dfd.resolve)
                .fail(dfd.reject)
                .then(next, next);
        }

        return promise;
    };

})(jQuery);

$(function () {

    $('.tile a[data-in-new-tab]').click(function () {
        event.preventDefault();
        var win = window.open(this.href, '_blank');
        if (win) {
            win.focus();
        }
    });

    var contTiles = $(".tiles-container");
    var contTilesBot = $(".tiles-container-bottom");
    var lsTiles = localStorage.getItem('tiles');
    var lsTilesBot = localStorage.getItem('tiles-bottom');
    if (lsTiles && IsJson(lsTiles)) {
        var arr = JSON.parse(lsTiles);
        for (var i = 0; i < arr.length; i++) {
            contTiles.append($('div[data-id="' + arr[i] + '"]', contTiles));
        }
    }
    if (lsTilesBot && IsJson(lsTilesBot)) {
        var arr = JSON.parse(lsTilesBot);
        for (var i = 0; i < arr.length; i++) {
            contTilesBot.append($('div[data-id="' + arr[i] + '"]', contTilesBot));
        }
    }

    function loadNotifications() {

        $('.tile').each(function () {
            var $this = $(this);

            $.ajaxQueue({
                url: $this.data("url"),
                async: true,
                success: function (jsn) {
                    $('.tileLabelNo', $this).text(jsn.count);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    var aa = "";
                },
                complete: function () {
                    showProgress = true;
                }
            }, function () { showProgress = false; });
        });

        setTimeout(function () { loadNotifications(); }, 30000);
    };

    function displayError(jqX) {
        var bdy = $('.container.body-content');
        var alr = $('.alert', bdy);
        if (alr.length == 0) {
            bdy.prepend('<div class="alert alert-danger alert-dismissable" style="margin-top:10px;"></div')
            alr = $('.alert', bdy);
        }
        var errMsg;

        if (IsJson(jqX.responseText)) {
            errMsg = "ERROR: " + JSON.parse(jqX.responseText).Message;
        }
        else {
            errMsg = "ERROR: " + jqX.statusText;
        }
        alr.html('<button class="close" aria-hidden="true" type="button" data-dismiss="alert">×</button>' + errMsg);
    }

    loadNotifications();

    contTiles.sortable({
        stop: function (event, ui) {
            var arr = [];
            $('div[data-id]', contTiles).each(function () {
                arr.push($(this).data("id"));
            });

            localStorage.setItem('tiles', JSON.stringify(arr));
        }
    });

    contTilesBot.sortable({
        stop: function (event, ui) {
            var arr = [];
            $('div[data-id]', contTilesBot).each(function () {
                arr.push($(this).data("id"));
            });

            localStorage.setItem('tiles-bottom', JSON.stringify(arr));
        }
    });
});