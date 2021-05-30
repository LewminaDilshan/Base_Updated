var PopUpSel = function (obj) {
    this.$el = $(obj);
    var $this = this;
    $('.ui-columns-progress', $this.$el).find(".ui-table-progress").width($($this.$el).width());
};

PopUpSel.prototype = {
    LastAjaxReq: null,
    uiSearch: null,
    uiTable: null,
    uiProgress: null,
    uiFooter: null,
    DataUrl: null,
    Filter: null,
    PageSize: 5,
    SortBy: null,
    InReverse: false,
    prevPage: null,
    CurPage: 1,
    PageCount: 0,
    RowCount: 0,
    ParaJson: {},
    Data: null,
    escapeRegExp: function (string) {
        return string.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
    },
    init: function () {
        $this = this;
        $this.uiSearch = $('.ui-columns-search', $this.$el);
        $this.uiTable = $('.ui-columns-table', $this.$el);
        $this.uiProgress = $('.ui-columns-progress', $this.$el);
        $this.uiFooter = $('.ui-columns-footer', $this.$el);

        $this.$el.on('click', '.ui-columns-foot-clear', function () {
            var popObj = $(window.curPopUpSelector);
            if (popObj[0].tagName == "SELECT") {
                popObj.empty().append('<option selected="selected" value="">' + popObj.data("empty-text") + '</option>');
            }
            else {
                popObj.removeData("selected-val");
            }
            popObj.removeData("selected-item");
            closeDialogModal($('#' + popObj.data('dlgid')));
            popObj.trigger("change");
        });
        $this.$el.on('click', '.ui-columns-foot-ok', function () {
            var rows = $('.ui-table-row-selected', $this.$el);

            if (rows.length == 0) {
                AlertIt("A record must be selected.");
                return false;
            }

            var arrKey = [];
            var arrVal = [];

            $('.ui-table thead tr th', $this.$el).each(function () {
                arrKey.push($(this).attr('data-columns-sortby'));
            });
            rows.first().find('td').each(function () {
                arrVal.push($(this).html());
            });

            var popObj = $(window.curPopUpSelector);

            var idx = 0;
            var dspf = popObj.data("dsp-format");
            var vm = popObj.data('value-member');
            var jsObj = {};
            for (var i = 0; i < arrKey.length; i++) {
                if (arrKey[i] == vm) {
                    idx = i;
                }
                jsObj[arrKey[i]] = arrVal[i] == "null" ? null : arrVal[i];

                var re = new RegExp($this.escapeRegExp('{' + arrKey[i] + '}'), 'g');
                dspf = dspf.replace(re, arrVal[i]);

                re = new RegExp($this.escapeRegExp('{' + i + '}'), 'g');
                dspf = dspf.replace(re, arrVal[i]);
            }

            if (popObj[0].tagName == "SELECT") {
                popObj.empty().append('<option selected="selected" value="' + arrVal[idx] + '">' + dspf + '</option>');
            }
            else {
                popObj.data("selected-val", arrVal[idx]);
            }
            
            popObj.data("selected-item", JSON.stringify(jsObj));
            closeDialogModal($('#' + popObj.data('dlgid')));
            popObj.trigger("change");
        });
        $this.$el.on('click', '.ui-columns-foot-cancel', function () {
            var popObj = $(window.curPopUpSelector);
            closeDialogModal($('#' + popObj.data('dlgid')));
        });

        $this.DataUrl = $this.$el.data("url");
        $this.ParaJson = window.curPopUpSelector.data("para-json");
        $this.loadIt();
    },
    loadIt: function () {
        $this = this;
        var para = {};
        if ($this.ParaJson)
        { $.extend(true, para, $this.ParaJson); }
        para.filter = $this.Filter;
        para.sortBy = $this.SortBy;
        para.inReverse = $this.InReverse;
        para.startIndex = ($this.CurPage - 1) * $this.PageSize;
        para.pageSize = $this.PageSize;

        var w = $this.uiTable.parent().width();
        var h = $this.uiTable.height();
        var elm = $this.uiTable;
        while (w <= 0) {
            elm = elm.parent();
            w = elm.width();
            h = 200;
        }

        $this.uiProgress.find(".ui-table-progress").width(w);
        $this.uiProgress.find(".ui-table-progress").height(h);
        $this.uiTable.css("display", "none");
        $this.uiProgress.css("display", "");

        if ($this.LastAjaxReq)
        { $this.LastAjaxReq.abort(); }

        $this.lastAjaxReq = $.ajax({
            type: "POST",
            url: $this.DataUrl,
            data: JSON.stringify(para),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            xhrFields: {
                withCredentials: true
            },
            success: function (jsn, status, jqXHR) {
                $this.prevPage = $this.CurPage;
                $this.prevFilter = $this.Filter;
                $this.RowCount = jsn.RowCount;
                $this.Data = jsn.Data;
                $this.SortBy = jsn.SortBy;
                $this.InReverse = jsn.InReverse;

                if (jqXHR == $this.lastAjaxReq) {
                    $this.setUI();
                    $this.setListeners();
                }
            },
            error: function (data, status, jqXHR) {
                if (status != 'abort') {
                    if (IsJson(data.responseText))
                    { AlertIt("ERROR: " + JSON.parse(data.responseText).Message); }
                    else
                    { AlertIt("ERROR: " + data.statusText); }
                    $this.data = [];
                }
            },
            complete: function (jqXHR, status) {
                dlgOnResize($this.$el.parent());
            },
            global: false
        });
    },
    setUI: function () {
        $this = this;
        var tbl = '<div class="' + ($this.Data.length > 0 ? 'ui-columns-table-data' : 'ui-columns-table-nodata') + '" style="overflow:auto;">' +
            '<table class="ui-table" style="border-bottom:1px solid #ccc;"><thead><tr>';

        $.each($this.Data, function (key, row) {

            //creating thead
            if (key == 0) {
                $.each(row, function (col, val) {

                    var re = new RegExp($this.escapeRegExp('_'), 'g');
                    var re47 = new RegExp($this.escapeRegExp('_47_'), 'g');

                    if ($this.SortBy == col) {
                        if ($this.InReverse) {
                            tbl += '<th class="' + 'ui-table-sort-down' + '" data-columns-sortby="' + col + '">' +
                                col.replace(re47, '/').replace(re, ' ') + ' <span class="fas fa-sort-amount-down"></span></th>';
                        } else {
                            tbl += '<th class="' + 'ui-table-sort-up' + '" data-columns-sortby="' + col + '">' +
                                col.replace(re47, '/').replace(re, ' ') + ' <span class="fas fa-sort-amount-down-alt"></span></th>';
                        }
                    } else {
                        tbl += '<th class="' + 'ui-table-sort' + '" data-columns-sortby="' + col + '">' + col.replace(re47, '/').replace(re, ' ') + '</th>';
                    }
                });
                tbl += '</tr></thead><tbody>';
            }

            //creating tbody
            tbl += (key % 2 == 0) ? '<tr class="ui-table-rows-even">' : '<tr class="ui-table-rows-odd">';

            $.each(row, function (col, val) {
                tbl += '<td>' + val + '</td>';
            });

            tbl += '</tr>';

        });
        tbl += '</tbody></table></div>';
        $this.uiTable.html(tbl);

        $this.PageCount = Math.ceil($this.RowCount / $this.PageSize);
        $this.CurPage = ($this.CurPage <= $this.PageCount ? $this.CurPage : 1);
        $this.CurPage = $this.PageCount == 0 ? 0 : $this.CurPage;

        //setting prev and next
        var navFirst = ($this.CurPage <= 1) ? 0 : 1;
        var navPrev = ($this.CurPage <= 1) ? 0 : $this.CurPage - 1;
        var navNext = ($this.CurPage + 1 <= $this.PageCount) ? $this.CurPage + 1 : 0;
        var navLast = ($this.CurPage + 1 <= $this.PageCount) ? $this.PageCount : 0;

        var start = (($this.CurPage - 1) * ($this.PageSize)) + 1;
        var end = start + $this.PageSize - 1;
        start = $this.RowCount == 0 ? 0 : start;
        end = (end < $this.RowCount) ? end : $this.RowCount;

        var opts = '';
        $.each([5, 10], function (key, val) {
            if (val == $this.PageSize) {
                opts += '<option value="' + val + '" selected="selected">' + val + '</option>';
            } else {
                opts += '<option value="' + val + '">' + val + '</option>';
            }
        });

        var foot =
            '<div class="ui-table-footer">' +
                '<div class="ui-table-results" style="margin:4px 20px 0 0;float:left">' +
                    '<div>' +
                        'Page <strong>' + $this.CurPage + '</strong> of <strong>' + $this.PageCount + '</strong>' +
                    '</div>' +
                '</div>' +
                '<div class="ui-table-show-rows" style="margin-top:4px;float:left">' +
                    '<span>' +
                        'Rows:&nbsp;' +
                        '<select>' + opts + '</select>' +
                    '</span>' +
                '</div>' +
                '<div class="ui-table-controls" style="white-space:nowrap;">' +
                    '<div>' +
                        '<div class="' + (navFirst ? 'ui-table-control-first' : 'ui-table-control-disabled') + '" data-columns-page="' + navFirst + '">' +
                            '<div class="ui-table-control-first-img"></div>' +
                        '</div>' +
                        '<div class="' + (navPrev ? 'ui-table-control-prev' : 'ui-table-control-disabled') + '" data-columns-page="' + navPrev + '">' +
                            '<div class="ui-table-control-prev-img"></div>' +
                        '</div>' +
                        '<span>' +
                            '<input class="ui-table-control-gotopage" type="text" name="page" />' +
                        '</span>' +
                        '<div class="' + (navNext ? "ui-table-control-next" : "ui-table-control-disabled") + '" data-columns-page="' + navNext + '">' +
                            '<div class="ui-table-control-next-img"></div>' +
                        '</div>' +
                        '<div class="' + (navLast ? "ui-table-control-last" : "ui-table-control-disabled") + '" data-columns-page="' + navLast + '">' +
                            '<div class="ui-table-control-last-img"></div>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
                '<div style="clear: both"></div>' +
            '</div>';
        $this.uiFooter.html(foot);

        var tb = $('.ui-table-control-gotopage');
        tb.val($this.CurPage.toString());
        tb.width((tb.val().length * 8) + 'px');

        $('.ui-table tbody tr', $this.$el).click(function (event) {
            $('.ui-table tbody tr', $this.$el).each(function () {
                $(this).removeClass("ui-table-row-selected");
            })
            $(this).addClass("ui-table-row-selected");
        });
        $('.ui-table tbody tr', $this.$el).dblclick(function (event) {
            $('.ui-columns-foot-ok', $this.$el).trigger("click");
        });

        var popObj = $(window.curPopUpSelector);
        var hdnInd = popObj.data("hidden-indices").toString().split(",")
        $.each(hdnInd, function (idx, val) {
            if (!val)
            { return true; }

            $('.ui-table tr > *:nth-child(' + (parseInt(val) + 1) + ')', $this.$el).hide();
        });

        $this.uiTable.css("display", "");
        $this.uiProgress.css("display", "none");
    },
    setNavButtons: function () {
        var navFirst = ($this.CurPage <= 1) ? 0 : 1;
        var navPrev = ($this.CurPage <= 1) ? 0 : $this.CurPage - 1;
        var navNext = ($this.CurPage + 1 <= $this.PageCount) ? $this.CurPage + 1 : 0;
        var navLast = ($this.CurPage + 1 <= $this.PageCount) ? $this.PageCount : 0;

        var start = (($this.CurPage - 1) * ($this.PageSize)) + 1;
        var end = start + $this.PageSize - 1;
        end = (end < $this.RowCount) ? end : $this.RowCount;

        var btns = $this.$el.find('.ui-table-control-first, .ui-table-control-prev, .ui-table-control-next, .ui-table-control-last, .ui-table-control-disabled');
        var objFirst = $(btns[0]);
        var objPrev = $(btns[1]);
        var objNext = $(btns[2]);
        var objLast = $(btns[3]);

        objFirst.attr('class', navFirst ? 'ui-table-control-first' : 'ui-table-control-disabled');
        objPrev.attr('class', navPrev ? 'ui-table-control-prev' : 'ui-table-control-disabled');
        objNext.attr('class', navNext ? 'ui-table-control-next' : 'ui-table-control-disabled');
        objLast.attr('class', navLast ? 'ui-table-control-last' : 'ui-table-control-disabled');

        objFirst.data('columns-page', navFirst);
        objPrev.data('columns-page', navPrev);
        objNext.data('columns-page', navNext);
        objLast.data('columns-page', navLast);

        var res = '<div>Results: <strong>' + start + ' &ndash; ' + end + '</strong> of <strong>' + $this.RowCount + '</strong></div>' +
                  '<div style="margin: 5px 0 5px 0;">' +
                    'Page <strong>' + $this.CurPage + '</strong> of <strong>' + $this.PageCount + '</strong>' +
                  '</div>';

        $this.$el.find('.ui-table-results').html(res);
        $this.$el.find('.ui-table-control-gotopage').val($this.CurPage);
    },
    setListeners: function () {
        $this = this;
        $this.$el.off('click', 'th');
        $this.$el.on('click', 'th', function () {
            if ($this.SortBy == $(this).data('columns-sortby')) {
                $this.InReverse = ($this.InReverse) ? false : true;
            }
            else { $this.InReverse = false; }

            $this.SortBy = $(this).data('columns-sortby');
            $this.CurPage = 1;
            $this.PageCount = 0;
            $this.setNavButtons();

            $this.loadIt();
        });

        $this.$el.off('click', '.ui-table-control-first, .ui-table-control-prev, .ui-table-control-next, .ui-table-control-last');
        $this.$el.on('click', '.ui-table-control-first, .ui-table-control-prev, .ui-table-control-next, .ui-table-control-last', function () {
            $this.CurPage = $(this).data('columns-page');
            $this.setNavButtons();

            $this.loadIt();
        });

        $this.$el.off('keyup', '.ui-table-search');
        $this.$el.on('keyup', '.ui-table-search', function () {
            $this.Filter = $(this).val();
            $this.CurPage = 1;
            $this.PageCount = 0;
            $this.setNavButtons();

            $this.loadIt();
        });

        $this.$el.off('change', '.ui-table-show-rows select');
        $this.$el.on('change', '.ui-table-show-rows select', function () {
            $this.PageSize = parseInt($(this).val());
            $this.CurPage = 1;
            $this.PageCount = 0;
            $this.setNavButtons();

            $this.loadIt();
        });

        $this.$el.off('keyup', '.ui-table-control-gotopage');
        $this.$el.on('keyup', '.ui-table-control-gotopage', function () {
            var val = $(this).val();
            var idx = parseInt(val);
            if ($this.page == idx || !val)
            { return; }

            if (idx > $this.PageCount) {
                idx = $this.PageCount;
            }
            if (idx <= 0) {
                idx = 1;
            }
            $this.CurPage = idx;
            $this.setNavButtons();
            $this.loadIt();
        });
    }
};

function initPopUpSel(id) {
    var pus = new PopUpSel('#' + id);
    pus.init();
};