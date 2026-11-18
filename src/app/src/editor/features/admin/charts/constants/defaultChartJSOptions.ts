export const defaultChartJSOptions = {
  scales: {
    x: {
      ticks: {
        stepSize: 1,
      },
    },
    y: {
      ticks: {
        stepSize: 1,
      },
    },
  },
  plugins: {
    legend: {
      title: {
        display: true,
        text: '',
      },
    },
    datalabels: {
      anchor: 'center',
      labels: {
        title: {
          font: {},
          display: false,
        },
      },
    },
  },
};

export const defaultChartJSStackedOptions = {
  scales: {
    x: {
      stacked: true,
      ticks: {
        stepSize: 1,
      },
    },
    y: {
      stacked: true,
      ticks: {
        stepSize: 1,
      },
    },
  },
  plugins: {
    legend: {
      title: {
        display: true,
        text: '',
      },
    },
    datalabels: {
      anchor: 'center',
      labels: {
        title: null,
      },
    },
  },
};
