export const applyChartJsOptions = (values: any) => {
  const options = { ...values };

  // Ensure default configuration properties exist.
  if (options.plugins === undefined) {
    options.plugins = {};
  }
  if (options.plugins.datalabels === undefined) {
    options.plugins.datalabels = {};
  }
  if (options.plugins.datalabels.labels === undefined) {
    options.plugins.datalabels.labels = {};
  }
  if (options.plugins.datalabels.labels.title === undefined) {
    options.plugins.datalabels.labels.title = {};
  }

  if (options.plugins.legend === undefined) {
    options.plugins.legend = {};
  }
  if (options.plugins.legend.labels === undefined) {
    options.plugins.legend.labels = {};
  }
  if (options.plugins.legend.title === undefined) {
    options.plugins.legend.title = {};
  }
  if (options.plugins.title === undefined) {
    options.plugins.title = {};
  }
  if (options.plugins.subtitle === undefined) {
    options.plugins.subtitle = {};
  }

  if (options.plugins.datalabels.labels.title.display === undefined) {
    // Default this property.
    options.plugins.datalabels.labels.title.display = false;
  }
  if (options.plugins.legend.title.display === undefined) {
    // Default this property.
    options.plugins.legend.title.display = false;
  }
  if (options.plugins.title.display === undefined) {
    // Default this property.
    options.plugins.title.display = false;
  }
  if (options.plugins.subtitle.display === undefined) {
    // Default this property.
    options.plugins.subtitle.display = false;
  }

  if (options.plugins.datalabels.formatter) {
    // the expectation here is for a lambda type function
    options.plugins.datalabels.formatter = eval(options.plugins.datalabels.formatter);
  }

  return options;
};
