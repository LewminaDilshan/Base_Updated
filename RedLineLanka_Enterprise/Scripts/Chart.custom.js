ChartAxis_Y = function (value, index, values) {
    if (Math.max(...values) < 1e6)
        return value;

    return value / 1e6 + 'M';
};

FilterBudgetedLabels = function (item, chart) {
    return !item.text.includes('Budgeted');
};

function reloadPage() {

    var $mainContent = $("#main-content");
    var $forms = $mainContent.find('form');
    var data = {};
    if ($forms.length > 0) {
        data = $forms.first().serialize();
    }

    $.ajax({
        async: true,
        type: "GET",
        url: location.href,
        data: data,
        done: function (data) {
            var newHtml = $('#main-content', $(data)).html();
            $mainContent.html(newHtml);
            var reponseScript = $(newHtml).find("script");
            $.each(reponseScript, function (idx, val) {
                var fn = window[val.text];
                if (typeof fn === "function") fn();
            });
            DocReadyFunc();
        },
        fail: function () {
            setTimeout(reloadPage, 600000);
        }
    });

}
setTimeout(reloadPage, 600000);