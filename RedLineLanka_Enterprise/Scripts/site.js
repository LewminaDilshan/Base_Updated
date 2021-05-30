function SetDateTimePicker(ctrl, dfmt, mxdt) {
    if (!dfmt) { dfmt = "yy-mm-dd"; }
    ctrl.css("text-transform", "uppercase");
    ctrl.css("position", "relative");
    ctrl.datetimepicker({
        beforeShow: function () { dlg_z_index++; ctrl.css("z-index", dlg_z_index); },
        onClose: function () {
            ctrl.css("z-index", "");
            if (ctrl.is('[data-jump-to-next]')) {
                var v = parseInt(ctrl.attr('data-jump-to-next'));
                var n = isNaN(v) ? 1 : v;
                $(':input:eq(' + ($(':input').index(ctrl) + n) + ')').focus();
            }
        },
        dateFormat: dfmt,
        timeFormat: "hh:mm:ss TT",
        showSecond: false,
        changeMonth: true,
        changeYear: true,
        yearRange: "c-10:c+10",
        maxDate: mxdt,
        showButtonPanel: true
    });
}

function SetDatePicker(ctrl, fmt, mxdt) {
    if (!fmt) { fmt = "yy-mm-dd"; }
    ctrl.css("text-transform", "uppercase");
    ctrl.css("position", "relative");
    ctrl.datepicker({
        beforeShow: function () { dlg_z_index++; ctrl.css("z-index", dlg_z_index); },
        onClose: function () {
            ctrl.css("z-index", "");
            if (ctrl.is('[data-jump-to-next]')) {
                var v = parseInt(ctrl.attr('data-jump-to-next'));
                var n = isNaN(v) ? 1 : v;
                $(':input:eq(' + ($(':input').index(ctrl) + n) + ')').focus();
            }
        },
        dateFormat: fmt,
        changeMonth: true,
        changeYear: true,
        yearRange: "c-10:c+10",
        maxDate: mxdt,
        showButtonPanel: true,
        autoClose: true
    });

    ctrl.blur(function () {
        var v = ctrl.val();
        if (isNaN(parseInt(v)) || !isFinite(v)) { return; }

        if (v.length == 6) {
            y = v.substring(4);
            y = y > 49 ? ("19" + y) : ("20" + y);
            v = y + "-" + v.substring(2, 4) + "-" + v.substring(0, 2);
        }
        else if (v.length == 8) { v = v.substring(4) + "-" + v.substring(2, 4) + "-" + v.substring(0, 2); }

        if (!isNaN(Date.parse(v))) {
            ctrl.val(v);
            ctrl.valid();
        }
    });
}

function SetTimePicker(ctrl, fmt) {
    if (!fmt) { fmt = "HH:mm"; }
    ctrl.css("text-transform", "uppercase");
    ctrl.css("position", "relative");
    ctrl.timepicker({
        beforeShow: function () { dlg_z_index++; ctrl.css("z-index", dlg_z_index); },
        onClose: function () {
            ctrl.css("z-index", "");
            if (ctrl.is('[data-jump-to-next]')) {
                var v = parseInt(ctrl.attr('data-jump-to-next'));
                var n = isNaN(v) ? 1 : v;
                $(':input:eq(' + ($(':input').index(ctrl) + n) + ')').focus();
            }
        },
        timeFormat: fmt,
        showSecond: false,
        showButtonPanel: true,
        autoClose: true
    });
}

function SetMonthPicker(ctrl, fmt, mxMon) {
    ctrl.attr("type", "text");
    if (!fmt) { fmt = "yy-mm"; }
    ctrl.css("text-transform", "uppercase");
    ctrl.css("position", "relative");
    ctrl.MonthPicker({
        OnBeforeMenuOpen: function () { dlg_z_index++; ctrl.css("z-index", dlg_z_index); },
        OnAfterMenuClose: function () {
            ctrl.css("z-index", "");
            if (ctrl.is('[data-jump-to-next]')) {
                var v = parseInt(ctrl.attr('data-jump-to-next'));
                var n = isNaN(v) ? 1 : v;
                $(':input:eq(' + ($(':input').index(ctrl) + n) + ')').focus();
            }
        },
        MonthFormat: fmt,
        MaxMonth: mxMon,
        ShowIcon: false
    });
}

function SetGridSortIconAndFirstCol() {
    var tbl = $('.table');
    $('tr th a', tbl).each(function () {
        var a = $(this);
        var hdr = a.html();
        if (hdr.indexOf(' ▲', hdr.length - 2) != -1) {
            a.html(hdr.substring(0, hdr.length - 2) + ' <span class="bi bi-sort-down-alt"></span>');
        }
        if (hdr.indexOf(' ▼', hdr.length - 2) != -1) {
            a.html(hdr.substring(0, hdr.length - 2) + ' <span class="bi bi-sort-down"></span>');
        }
    });

    var fth = $("thead tr th", tbl).eq(0);
    if (tbl.find('tbody > tr').length == 0) {
        if (!$.trim(fth.html())) { fth.hide(); }
    }
    else { fth.show(); }
}

function SetComboReadonly(obj, val) {
    if (val) {
        obj.attr("readonly", "readonly");
        obj.on("focus.ro mousedown.ro mouseup.ro click.ro change.ro dblclick.ro", function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
        });
    }
    else {
        obj.removeAttr("readonly");
        obj.off(".ro");
    }
};

function SetCheckBoxReadonly(obj, val) {
    if (val) {
        obj.attr("readonly", "readonly");
        obj.on("focus.ro mousedown.ro mouseup.ro click.ro change.ro dblclick.ro", function (e) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
        });

        //var par = obj.parent();

        //if (par.data("chkbox-wrap"))
        //{ par.css('background', '#FF0000'); }
        //else
        //{ obj.wrap("<span style='background:#FF0000' data-chkbox-wrap='1'></span>"); }
    }
    else {
        obj.removeAttr("readonly");
        obj.off(".ro");

        //var par = obj.parent();

        //if (par.data("chkbox-wrap"))
        //{ par.unwrap(); }
    }
};

function IsJson(str) {
    try {
        $.parseJSON(str);
        return true;
    } catch (e) {
        return false;
    }
}

function dlgOnResize(dlg, w) {
    var h = dlg.dialog("option", "height");
    if (!w) {
        if (window.curPopUpSelector && dlg.attr('id') == window.curPopUpSelector.data("dlgid")) { w = window.curPopUpSelector.data("popup-width"); }
        if (!w) { w = 500; }
    }

    var mh = $(window).height() - 200;
    var mw = $(window).innerWidth() - 50;

    dlg.dialog("option", "maxHeight", mh);
    dlg.dialog("option", "maxWidth", mw);

    dlg.dialog("option", "height", h > mh ? mh : "auto");
    dlg.dialog("option", "width", w > mw ? mw : w);
    dlg.dialog("option", "position", { my: "center", at: "center", of: window });
}

function closeDialogModal(obj, FireCloseEvent) {
    if (!obj) { return; }

    var dlg;
    if (obj instanceof $) { dlg = obj; }
    else { dlg = $(obj).parents(".popDlgMdlCont"); }

    if (IsDilogOpen(dlg)) {
        if (!FireCloseEvent) { dlg.off("dialogclose"); }
        dlg.dialog("close");
    }
    return false;
}

function AlertIt(msg) {
    var dlgId = $(window).alertDialogId;
    if (!dlgId) {
        dlgId = "ID" + Math.random().toString().replace(".", "_").replace("-", "_");
        $(window).alertDialogId = dlgId;
        $(document.body).append('<div id="' + dlgId + '" class="popDlgMdlCont" style="display:none;"></div>');
    }
    var dlg = $('#' + dlgId);

    function winResFunc() { dlgOnResize(dlg, 300); }

    dlg.dialog({
        height: "auto",
        show: "clip",
        modal: true,
        autoOpen: false,
        open: function (event, ui) {
            var dlgParent = dlg.closest('.ui-dialog');
            var closeBtn = $('.ui-dialog-titlebar-close', dlgParent);
            closeBtn.addClass("btn btn-outline-danger btn-close");
            dlg_z_index++;
            dlgParent.css("z-index", dlg_z_index.toString());
            dlg.dialog("option", "position", { my: "center", at: "center", of: window });

            $("body").css({
                overflow: 'hidden'
            });
            $(".ui-dialog", dlg.parent().parent()).find(".ui-widget-header").css("background", "#e9e9e9");
            $(".ui-dialog", dlg.parent().parent()).find(".ui-widget-header").css("color", "#333333");

            dlgOnResize(dlg, 300);
            window.addEventListener("resize", winResFunc);
        },
        beforeClose: function (event, ui) {
            $("body").css({ overflow: 'inherit' })
            window.removeEventListener("resize", winResFunc);
        }
    });

    dlg.html(
        '<div class="modal-body" style="width:auto">' +
        '<div class="form-horizontal">' +
        '<div class="row-fluid">' +
        '<div class="control-group">' +
        '<label>' + msg + '</label>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '<div style="padding-top:10px;text-align:right;border-top: 1px solid #e5e5e5;">' +
        '<input type="button" class="btn btn-outline-secondary" style="min-width:75px" value="Ok" />' +
        '</div>');

    $('input[type="button"][value="Ok"]', dlg).click(function () {
        closeDialogModal(dlg);
    });

    if (objProg.length > 0) { objProg.hide(); }

    dlg.dialog("option", "title", "Alert");
    dlgOnResize(dlg, 300);
    dlg.dialog("open");
}

function ConfirmIt(msg, title, posButText, negButText, showCancel, funcSuccess, funcCancel, hideNegBut) {
    if (!msg) { msg = "Please click ok to confirm."; }
    if (!title) { title = "Please confirm"; }
    if (!posButText) { posButText = "Ok"; }
    if (!negButText) { negButText = "Cancel"; }

    var dlgId = $(window).confirmDialogId;
    if (!dlgId) {
        dlgId = "ID" + Math.random().toString().replace(".", "_").replace("-", "_");
        $(window).confirmDialogId = dlgId;
        $(document.body).append('<div id="' + dlgId + '" class="popDlgMdlCont" style="display:none;"></div>');
    }
    var dlg = $('#' + dlgId);
    dlg.data("is-pos", 0);

    function winResFunc() { dlgOnResize(dlg, 300); }

    dlg.dialog({
        height: "auto",
        show: "clip",
        modal: true,
        autoOpen: false,
        open: function (event, ui) {
            var dlgParent = dlg.closest('.ui-dialog');
            var closeBtn = $('.ui-dialog-titlebar-close', dlgParent);
            closeBtn.addClass("btn btn-outline-danger btn-close");
            dlg_z_index++;
            dlgParent.css("z-index", dlg_z_index.toString());
            dlg.dialog("option", "position", { my: "center", at: "center", of: window });

            $("body").css({
                overflow: 'hidden'
            });
            $(".ui-dialog", dlg.parent().parent()).find(".ui-widget-header").css("background", "#e9e9e9");
            $(".ui-dialog", dlg.parent().parent()).find(".ui-widget-header").css("color", "#333333");

            dlgOnResize(dlg, 300);
            window.addEventListener("resize", winResFunc);
        },
        beforeClose: function (event, ui) {
            $("body").css({ overflow: 'inherit' })
            window.removeEventListener("resize", winResFunc);
        },
        close: function (event, ui) {
            if (dlg.data("is-pos") == 1) {
                if (funcSuccess) { funcSuccess(); }
            }
            else {
                if (funcCancel) { funcCancel(); }
            }
        }
    });

    dlg.html(
        '<div class="modal-body" style="width:auto">' +
        '<div class="form-horizontal">' +
        '<div class="row-fluid">' +
        '<div class="control-group">' +
        '<label>' + msg + '</label>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '</div>' +
        '<div class="d-grid gap-2 d-md-block" style="padding-top:10px;text-align:center;border-top: 1px solid #e5e5e5;">' +
        '<input id="' + dlgId + '_btnPositive" type="button" class="btn btn-primary" style="min-width:75px" value="' + posButText + '" />' +
        (hideNegBut ? '' : '<input id="' + dlgId + '_btnNegative" type="button" class="btn btn-default" style="min-width:75px" value="' + negButText + '" />') +
        (showCancel ? '<input id="' + dlgId + '_btnCancel" type="button" class="btn btn-secondary" style="min-width:75px" value="Cancel" />' : '') +
        '</div>');

    $('#' + dlgId + '_btnPositive', dlg).click(function () {
        dlg.data("is-pos", 1);
        closeDialogModal(dlg);
    });

    $('#' + dlgId + '_btnNegative, #' + dlgId + '_btnCancel', dlg).click(function () {
        closeDialogModal(dlg);
    });

    if (objProg.length > 0) { objProg.hide(); }

    dlg.dialog("option", "title", title);
    dlgOnResize(dlg, 300);
    dlg.dialog("open");
}

function IsDilogOpen(dlg) {
    return typeof dlg.dialog("instance") != "undefined" && dlg.dialog("isOpen") == true;
}

var dlg_z_index = 9999;
var dlg_lastAjaxReq;
var dlg_seq = 0;
var objProg;
var showProgress = true;

function GetDialogObj(obj) {
    var dlgId = obj.data("dlgid");
    if (!dlgId) {
        dlg_seq = dlg_seq + 1;
        dlgId = "dlgID" + dlg_seq.toString();
        obj.data("dlgid", dlgId);
        $(document.body).append('<div id="' + dlgId + '" class="popDlgMdlCont" style="display:none;"></div>');
    }
    return $('#' + dlgId);
}

$(document).ready(function () {
    SetProgressPosition();
    DocReadyFunc();
});

function DocReadyFunc() {
    SetGridSortIconAndFirstCol();
    $(":input[type='JQ-datetime']:not([readonly])").each(function () { SetDateTimePicker($(this)); });
    $(":input[type='JQ-date']:not([readonly])").each(function () { SetDatePicker($(this)); });
    $(":input[type='JQ-time']:not([readonly])").each(function () { SetTimePicker($(this)); });
    $(":input[data-type='JQ-month']:not([readonly])").each(function () { SetMonthPicker($(this)); });
    $("select[readonly]").each(function () { SetComboReadonly($(this), true); });
    $("input[type='checkbox'][readonly]").each(function () { SetCheckBoxReadonly($(this), true); });
    //For Three State Checkbox
    $("input[type='checkbox']").filter("[data-hf-name]").each(function () {
        var el = $(this);
        var trueVal = el.data('true-val').toString();
        var falseVal = el.data('false-val').toString();
        var indetVal = el.data('indet-val').toString();

        var el_hf = $(this).next('#' + el.data("hf-name"));
        el_hf.change(function () {
            switch (el_hf.val()) {
                case trueVal:
                    el.prop('checked', true);
                    el.prop('indeterminate', false);
                    break;
                case falseVal:
                    el.prop('checked', false);
                    el.prop('indeterminate', false);
                    break;
                default:
                    el.prop('checked', true);
                    el.prop('indeterminate', true);
                    break;
            }
        });
        el.click(function () {
            var ind = el_hf.val();
            el_hf.val(ind == trueVal ? falseVal : ind == falseVal ? indetVal : trueVal);
            el_hf.trigger('change');
        });
        el_hf.trigger('change');
    });
    $("input[readonly]").each(function () { $(this).attr('tabindex', '-1'); });

    $('.dlgConfirmSubmit').each(function () {
        var $this = $(this);
        var dlg = GetDialogObj($this);

        function winResFunc() { dlgOnResize(dlg); }

        dlg.dialog({
            height: "auto",
            show: "clip",
            modal: true,
            autoOpen: false,
            open: function (event, ui) {
                var dlgParent = dlg.closest('.ui-dialog');
                var closeBtn = $('.ui-dialog-titlebar-close', dlgParent);
                closeBtn.addClass("btn btn-outline-danger btn-close");
                dlg_z_index++;
                dlgParent.css("z-index", dlg_z_index.toString());
                dlg.dialog("option", "position", { my: "center", at: "center", of: window });

                $("body").css({
                    overflow: 'hidden'
                });

                $(".ui-widget-overlay").bind('click', function () { dlg.dialog("close"); });

                dlgOnResize(dlg);
                window.addEventListener("resize", winResFunc);
            },
            beforeClose: function (event, ui) {
                $("body").css({ overflow: 'inherit' })
                window.removeEventListener("resize", winResFunc);
            }
        });

        $this.off(".ConfirmSubmit");
        $this.on("click.ConfirmSubmit", function (e) {

            e.preventDefault();

            var msg = $this.data('message');
            if (!msg) { msg = "Are you sure you want to continue?"; }

            var title = $this.data('title');
            if (!title) { title = "Please confirm"; }

            var btnTxt = $this.data('button-text');
            if (!btnTxt) { btnTxt = "Yes"; }

            var btnClass = $this.data('button-class');
            if (!btnClass) { btnClass = "btn btn-danger"; }

            var submitAction = $this.data('submit-action');
            var jsFunction = $this.data('js-function');

            var contentSelector = $this.data('content-selector');

            if (contentSelector) {
                var contentObj = $('' + contentSelector);
                dlg.html(contentObj.html());
            }
            else {
                dlg.html(
                    '<div class="modal-body" style="width:auto">' +
                    '<div class="form-horizontal">' +
                    '<div class="row-fluid">' +
                    '<div class="control-group">' +
                    '<label>' + msg + '</label>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '</div>' +
                    '<div class="modal-footer">' +
                    '<a href="#" class="btn ' + btnClass + '" style="min-width:100px">' + btnTxt + '</a>' +
                    '<input type="button" class="btn btn-secondary" style="min-width:100px" value="Cancel" />' +
                    '</div>');
            }

            $('.modal-footer a', dlg).click(function () {
                if (jsFunction) {
                    closeDialogModal(dlg);
                    window.curSubmitter = $this;
                    var fn = window[jsFunction];
                    if (typeof fn === "function") fn();
                }
                else {
                    var frm = $this.closest("form");
                    if (dlg.find('form').length > 0) {
                        frm = $(this).closest("form");
                    }
                    if (submitAction) {
                        var act = frm.attr('action');
                        act = act.substring(0, act.lastIndexOf("/") + 1) + submitAction + (act.indexOf("?") == -1 ? "" : act.substring(act.indexOf("?")));
                        frm.attr('action', act);
                    }
                    frm.submit();
                    closeDialogModal(dlg);
                }
            });

            $('.modal-footer input[type="button"]', dlg).click(function () {
                closeDialogModal(dlg);
            });

            dlg.dialog("option", "title", title);
            dlgOnResize(dlg);
            dlg.dialog("open");
        });
    });

    $('.dlgPopUpSelector').each(function (index, element) {
        var $this = $(this);
        var dlg = GetDialogObj($this);

        function winResFunc() { dlgOnResize(dlg, $this.data("popup-width")); }

        dlg.dialog({
            height: "auto",
            show: "clip",
            modal: true,
            autoOpen: false,
            open: function (event, ui) {
                var dlgParent = dlg.closest('.ui-dialog');
                var closeBtn = $('.ui-dialog-titlebar-close', dlgParent);
                closeBtn.addClass("btn btn-outline-danger btn-close");
                dlg_z_index++;
                dlgParent.css("z-index", dlg_z_index.toString());
                dlg.dialog("option", "position", { my: "center", at: "center", of: window });

                $("body").css({
                    overflow: 'hidden'
                });

                $(".ui-widget-overlay").bind('click', function () { dlg.dialog("close"); });

                winResFunc();
                window.curPopUpSelector = $this;
                window.addEventListener("resize", winResFunc);
            },
            beforeClose: function (event, ui) {
                $("body").css({ overflow: 'inherit' })
                window.removeEventListener("resize", winResFunc);
            }
        });

        $this.off(".PopUpSelector");
        function onclick(e) {

            e.preventDefault();
            e.stopPropagation();

            if ($this.prop('readOnly') || $this.prop('disabled') || $this.is('[readOnly]') || $this.is('[disabled]')) { return; }

            var url = $this.data("url") + "&dlgID=" + $this.data("dlgid");
            var title = $this.data("title");

            dlg.dialog("option", "title", title);
            winResFunc(dlg);

            $.get(url, function (data) {
                window.curPopUpSelector = $this;
                dlg.html(data);
                winResFunc(dlg);
                dlg.dialog("open");
            });
        }

        $this.on("mousedown.PopUpSelector", onclick);
        $this.on("keydown.PopUpSelector", function (e) {
            if (e.keyCode) {
                if (e.altKey || e.ctrlKey || e.metaKey) { return; }

                if ((e.keyCode < 48 || e.keyCode > 90) && //Pass numbers and characters
                    (e.keyCode < 96 || e.keyCode > 111) && //Pass numpad numbers
                    ($.inArray(e.keyCode, [115, 13, 32, 8, 37, 38, 39, 40]) === -1)) { //Pass f4,enter,space,backspace,ArrowKeys
                    return;
                }
            }

            onclick(e);
        });
    });

    $('.dlgChangePassword').each(function () {
        var $this = $(this);
        var dlg = GetDialogObj($this);

        function winResFunc() { dlgOnResize(dlg); }

        dlg.dialog({
            height: "auto",
            show: "clip",
            modal: true,
            autoOpen: false,
            open: function (event, ui) {
                var dlgParent = dlg.closest('.ui-dialog');
                var closeBtn = $('.ui-dialog-titlebar-close', dlgParent);
                closeBtn.addClass("btn btn-outline-danger btn-close");
                dlg_z_index++;
                dlgParent.css("z-index", dlg_z_index.toString());
                dlg.dialog("option", "position", { my: "center", at: "center", of: window });

                $("body").css({
                    overflow: 'hidden'
                });

                $(".ui-widget-overlay").bind('click', function () { dlg.dialog("close"); });

                dlgOnResize(dlg);
                window.addEventListener("resize", winResFunc);
            },
            beforeClose: function (event, ui) {
                $("body").css({ overflow: 'inherit' })
                window.removeEventListener("resize", winResFunc);
            }
        });

        $this.off(".ChangePassword");
        $this.on("click.ChangePassword", function (e) {

            dlg.dialog("option", "title", "Change password");
            winResFunc(dlg);

            function bindDlgEvents() {
                $('input[type="submit"][value="Save"]', dlg).closest("form").submit(function () {
                    $.ajax({
                        url: this.action,
                        type: this.method,
                        data: $(this).serialize(),
                        success: function (result) {
                            if (result.success) {
                                closeDialogModal(dlg);
                                AlertIt("Password changed successfully. You have to use the new password next time you signin.");
                            } else {
                                dlg.html(result);
                                bindDlgEvents();
                            }
                        }
                    });
                    return false;
                });

                $('input[type="button"][value="Cancel"]', dlg).click(function () {
                    closeDialogModal(dlg);
                });

                winResFunc(dlg);
            }

            $.get(AppRoot + "Base/Home/ChangePassword", function (data) {
                window.curPopUpSelector = $this;
                dlg.html(data);
                bindDlgEvents();
                dlg.dialog("open");
            });
        });
    });

    $("select#PageSize.custom-select").change(function () {
        var $this = $(this);
        var prevVal = $this.data("prev-val");

        if ($this.val() == prevVal) { return; }

        var frm = $this.closest("form");
        if ($this.val() == "0" && $this.is("[data-warn-show-all]")) {
            ConfirmIt("It may take some time to render all the data. Are you sure you want to show all?",
                "Confirm Show All", "Yes", "No", true,
                function () {
                    frm.submit();
                },
                function () {
                    $this.val(prevVal);
                });
        }
        else {
            frm.submit();
        }
    });
}

$.widget("ui.dialog", $.extend({}, $.ui.dialog.prototype, {
    _title: function (title) {
        if (!this.options.title) {
            title.html("&#160;");
        } else {
            title.html(this.options.title);
        }
    }
}));

function SetProgressPosition() {
    objProg = $('div#dspProgress');
    var objProgBody = $('div#dspProgressMessage', objProg);

    $('div#dspProgressBackgroundFilter', objProg).addClass('progressBackground');
    objProgBody.addClass('progressBody');

    var winW = $(window).width() / 2;
    var winH = $(window).height() / 2;
    var divW = objProgBody.width() / 2;
    var divH = objProgBody.height() / 2;

    var w = parseInt(winW - divW);
    var h = parseInt(winH - divH);

    objProgBody.css('left', w);
    objProgBody.css('top', h);

    objProg.hide();
    $(document).ajaxStart(function () { if (showProgress) objProg.show(); });
    $(document).ajaxStop(function () { objProg.hide(); });

    $("form").each(function () {
        $(this).submit(function (eve) {
            if (typeof $(this).valid != "function" || $(this).valid()) {
                objProg.show();
            }
        });
    });

    var progImgUrl = AppRoot + "Content/Images/Progress/Progress-";
    var timeOut;
    objProg.on('show', function () {
        clearTimeout(timeOut);
        setTimeout(function () {
            window.timer_on = 1;
            set(1);
        }, 350);
    });
    objProg.on('hide', function () {
        clearTimeout(timeOut);
        window.timer_on = 0;
    });

    function set(i) {
        if (i > 5) i = 1;
        if (window.timer_on === 0) return;

        $('img', objProgBody).attr('src', progImgUrl + '0' + i + '.png?v=1');

        timeOut = setTimeout(function () {
            set(++i);
        }, 300);
    }
}

$(document).ready(function () {
    $(window).bind('resize', function () { SetProgressPosition(); });
});

(function ($) {
    $.each(['show', 'hide'], function (i, ev) {
        var el = $.fn[ev];
        $.fn[ev] = function () {
            this.trigger(ev);
            return el.apply(this, arguments);
        };
    });
})(jQuery);