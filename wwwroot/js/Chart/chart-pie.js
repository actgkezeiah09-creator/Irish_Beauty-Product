// Revenue Sources (Pie Chart)
Chart.defaults.global.defaultFontFamily = 'Nunito',
    '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
Chart.defaults.global.defaultFontColor = '#858796';

$.get('/Admin/GetRevenueSources', function (data) {
    console.log("Revenue Sources:", data); 

    let labels = data.map(x => x.category); 
    let totals = data.map(x => x.total);   

    var ctx = document.getElementById("myPieChart");
    if (!ctx) return;
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: totals,
                backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b'],
                hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617'],
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }],
        },
        options: {
            maintainAspectRatio: false,
            legend: { position: 'bottom' },
            cutoutPercentage: 70,
            tooltips: {
                callbacks: {
                    label: function (tooltipItem, chart) {
                        let dataset = chart.datasets[0];
                        let value = dataset.data[tooltipItem.index] || 0;
                        return chart.labels[tooltipItem.index] + ': ₱' + number_format(value);
                    }
                }
            }
        }
    });
});
