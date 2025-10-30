// Set new default font family and font color to mimic Bootstrap's default styling
Chart.defaults.global.defaultFontFamily = 'Nunito',
    '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';

// Revenue Sources (Pie Chart) - from database
$.getJSON('/Dashboard/GetRevenueSources', function (data) {
    let labels = data.map(x => x.Category);
    let totals = data.map(x => x.Total);

    var ctx = document.getElementById("myPieChart").getContext('2d');
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: totals,
                backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b'],
                hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617'],
                hoverBorderColor: "rgba(234, 236, 244, 1)"
            }]
        },
        options: {
            maintainAspectRatio: false,
            legend: {
                display: true,
                position: 'bottom'
            },
            cutoutPercentage: 70,
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                caretPadding: 10,
                callbacks: {
                    label: function (tooltipItem, data) {
                        let dataset = data.datasets[0];
                        let value = dataset.data[tooltipItem.index];
                        let label = data.labels[tooltipItem.index];
                        return label + ': ₱' + number_format(value);
                    }
                }
            }
        }
    });
});
