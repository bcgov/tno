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
    datalabels: {
      anchor: 'center',
      labels: {
        title: null,
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
    datalabels: {
      anchor: 'center',
      labels: {
        title: null,
      },
    },
  },
};
