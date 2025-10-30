// Monthly Earnings (Area Chart)
Chart.defaults.global.defaultFontFamily = 'Nunito',
    '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';

function number_format(number, decimals, dec_point, thousands_sep) {
    number = (number + '').replace(',', '').replace(' ', '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
        sep = thousands_sep || ',',
        dec = dec_point || '.',
        s = (prec ? (Math.round(n * Math.pow(10, prec)) / Math.pow(10, prec)).toString() : '' + Math.round(n)).split('.');
    if (s[0].length > 3) s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    if ((s[1] || '').length < prec) s[1] = (s[1] || '') + new Array(prec - s[1].length + 1).join('0');
    return s.join(dec);
}

$// Monthly Earnings (Area Chart) - from database
$.getJSON('/Dashboard/GetMonthlySales', function (data) {
    let labels = data.map(x => x.Month);
    let values = data.map(x => x.Total);

    // ✅ Update Monthly Earnings Card (last month’s total)
    if (values.length > 0) {
        let lastMonthly = values[values.length - 1];
        $("#monthlyEarnings").text("₱" + number_format(lastMonthly));
    }

    var ctx = document.getElementById("myAreaChart").getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: "Earnings",
                data: values,
                borderColor: "rgba(78, 115, 223, 1)",
                backgroundColor: "rgba(78, 115, 223, 0.05)",
                fill: true,
            }]
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                xAxes: [{ gridLines: { display: false }, ticks: { maxTicksLimit: 12 } }],
                yAxes: [{
                    ticks: {
                        callback: function (value) { return '₱' + number_format(value); }
                    }
                }]
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem) { return '₱' + number_format(tooltipItem.yLabel); }
                }
            }
        }
    });
});
