
// Format numbers nicely with commas and ₱
function number_format(number, decimals = 0, dec_point = '.', thousands_sep = ',') {
    number = (number + '').replace(',', '').replace(' ', '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = Math.abs(decimals),
        sep = thousands_sep,
        dec = dec_point,
        s = (prec ? n.toFixed(prec) : Math.round(n).toString()).split('.');
    if (s[0].length > 3) {
        s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    }
    return s.join(dec);
}

// Fetch Monthly Sales from AdminController
$.getJSON('/Admin/GetMonthlySales', function (data) {
    console.log("✅ Monthly Sales Data:", data);

    if (!data || !Array.isArray(data)) return;

    // Ensure lowercase keys match backend
    let labels = data.map(x => x.month);
    let values = data.map(x => x.total);

    // pdate monthly earnings card — latest month's total
    let totalMonth = values.length > 0 ? values[values.length - 1] : 0;
    $("#monthlyEarnings").text("₱" + number_format(totalMonth, 0));

    //  Determine chart scaling dynamically
    let maxValue = Math.max(...values);
    let stepSize;

    if (maxValue === 0) {
        maxValue = 20000;   // Default scale when no sales yet
        stepSize = 5000;
    } else {
        stepSize = Math.ceil(maxValue / 5 / 1000) * 1000;
    }

    var ctxElement = document.getElementById("myAreaChart");
    if (!ctxElement) {
        console.error("❌ myAreaChart canvas not found!");
        return;
    }

    var ctx = ctxElement.getContext('2d');

    //  Build Chart.js area chart
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
                lineTension: 0.4,
                pointRadius: 3
            }]
        },
        options: {
            maintainAspectRatio: false,
            scales: {
                xAxes: [{
                    gridLines: { display: false },
                    ticks: {
                        autoSkip: false,
                        maxRotation: 45,
                        minRotation: 45
                    }
                }],
                yAxes: [{
                    ticks: {
                        beginAtZero: true,
                        stepSize: stepSize,
                        max: maxValue,
                        callback: function (value) {
                            return '₱' + number_format(value, 0);
                        }
                    }
                }]
            },
            tooltips: {
                callbacks: {
                    label: function (tooltipItem) {
                        return '₱' + number_format(tooltipItem.yLabel, 0);
                    }
                }
            }
        }
    });
});
