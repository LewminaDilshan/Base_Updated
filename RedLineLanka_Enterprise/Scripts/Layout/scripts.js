function initializeJS() {

    //tool tips
    jQuery('.tooltips').tooltip();

    //popovers
    jQuery('.popovers').popover();
    $sideBar = $("#sidebar");

    //custom scrollbar
    //for html
    jQuery("html").niceScroll({ styler: "fb", cursorcolor: "#007AFF", cursorwidth: '6', cursorborderradius: '10px', background: '#F7F7F7', cursorborder: '', zindex: '1000' });
    //for sidebar
    $sideBar.niceScroll({ styler: "fb", cursorcolor: "#007AFF", cursorwidth: '3', cursorborderradius: '10px', background: '#F7F7F7', cursorborder: '' });
    // for scroll panel
    jQuery(".scroll-panel").niceScroll({ styler: "fb", cursorcolor: "#007AFF", cursorwidth: '3', cursorborderradius: '10px', background: '#F7F7F7', cursorborder: '' });

    //sidebar dropdown menu
    $('li.sub-menu > a', $sideBar).click(function () {

        var menuWidth = Math.round($('ul.sidebar-menu').width());
        //console.log(menuWidth);
        $this = $(this);
        $li = $this.closest('li');
        if ($li.children('ul').length === 0)
            return;

        if ($li.hasClass('cur'))
            $li.removeClass('cur');
        else {
            $li.addClass('cur');
            $li.siblings('li').removeClass('cur');
            $li.siblings('li').find('li.cur').removeClass('cur');
        }

        //var last = jQuery('.sub-menu.open', jQuery('#sidebar'));
        //jQuery('.menu-arrow').removeClass('arrow_carrot-right');
        //jQuery('.sub', last).slideUp(200);
        //var sub = jQuery(this).next();
        //if (sub.is(":visible")) {
        //    jQuery('.menu-arrow').addClass('arrow_carrot-right');
        //    sub.slideUp(200);
        //} else {
        //    jQuery('.menu-arrow').addClass('arrow_carrot-down');
        //    sub.slideDown(200);
        //}
        var o = $this.offset();
        var diff = 200 - o.top;
        if (diff > 0)
            $sideBar.scrollTo("-=" + Math.abs(diff), 500);
        else
            $sideBar.scrollTo("+=" + Math.abs(diff), 500);

        menuWidth = Math.round($('ul.sidebar-menu').width());
        var w = Math.round($("#sidebar").width());
        //console.log(menuWidth + "," + w);
        SideMenuWidth = menuWidth + "px";

        let root = document.documentElement;
        root.style.setProperty('--menu-width', w + "px");
        root.style.setProperty('--menu-width-neg', "-" + w + "px");
        $(window).trigger('resize');
    });

    // sidebar menu toggle
    jQuery(function () {
        function responsiveView() {
            $container = $("#container");
            $mainCont = $('#main-content');

            if ($container.hasClass("sidebar-hidden"))
                return;

            var wSize = jQuery(window).width();
            if (wSize <= 768) {
                $mainCont.css({ 'margin-left': '0px' });
                $sideBar.css({
                    'margin-left': '-' + SideMenuWidth
                });
                jQuery('#sidebar > ul').hide();
                $container.addClass("sidebar-closed");
            }

            if (wSize > 768) {
                $mainCont.css({ 'margin-left': SideMenuWidth });
                jQuery('#sidebar > ul').show();
                $sideBar.css({
                    'margin-left': '0'
                });
                $container.removeClass("sidebar-closed");
            }
            $sideBar.getNiceScroll().resize();
        }
        $().ready(responsiveView);
        $window = $(window);
        $window.on('load', responsiveView);
        $window.on('resize', responsiveView);
    });

    jQuery('.toggle-nav').click(function () {
        if (jQuery('#sidebar > ul').is(":visible") === true) {
            jQuery('#main-content').css({
                'margin-left': '0px'
            });
            jQuery('#sidebar').css({
                'margin-left': '-' + SideMenuWidth
            });
            $('#sidebar').removeClass("show-sidebar-menu").addClass("hide-sidebar-menu");
            jQuery("#container").addClass("sidebar-closed");
            jQuery("#container").addClass("sidebar-hidden");
        } else {
            jQuery('#main-content').css({
                'margin-left': SideMenuWidth
            });
            $('#sidebar').removeClass("hide-sidebar-menu").addClass("show-sidebar-menu");
            jQuery('#sidebar').css({
                'margin-left': '0'
            });
            jQuery("#container").removeClass("sidebar-closed");
            jQuery("#container").removeClass("sidebar-hidden");
        }
    });
}

jQuery(document).ready(function () {
    initializeJS();
});