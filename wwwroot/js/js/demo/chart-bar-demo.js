// Set new default font family and font color to mimic Bootstrap's default styling
Chart.defaults.global.defaultFontFamily = 'Nunito',
    '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';

function number_format(number, decimals, dec_point, thousands_sep) {
    number = (number + '').replace(',', '').replace(' ', '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
        sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep,
        dec = (typeof dec_point === 'undefined') ? '.' : dec_point,
        s = '',
        toFixedFix = function (n, prec) {
            var k = Math.pow(10, prec);
            return '' + Math.round(n * k) / k;
        };
    s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.');
    if (s[0].length > 3) {
        s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    }
    if ((s[1] || '').length < prec) {
        s[1] = s[1] || '';
        s[1] += new Array(prec - s[1].length + 1).join('0');
    }
    return s.join(dec);
}

/// Annual Earnings (Bar Chart) - from database
$.get('/Dashboard/GetAnnualEarnings', function (data) {
    let labels = data.map(x => x.Year);
    let totals = data.map(x => x.Total);

    // Update Annual Earnings Card 
    if (totals.length > 0) {
        let lastAnnual = totals[totals.length - 1];
        $("#annualEarnings").text("₱" + number_format(lastAnnual));
    } else {
        $("#annualEarnings").text("₱0"); // fallback if no sales yet
    }

    var ctx = document.getElementById("myBarChart");
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: "Revenue",
                backgroundColor: "#4e73df",
                hoverBackgroundColor: "#2e59d9",
                borderColor: "#4e73df",
                data: totals,
            }],
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                xAxes: [{ gridLines: { display: false }, ticks: { maxTicksLimit: 10 } }],
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
