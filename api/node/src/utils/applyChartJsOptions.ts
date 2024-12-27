export const applyChartJsOptions = (values: any) => {
  const options = { ...values };

  // Ensure default configuration properties exist.
  if (options.plugins === undefined) {
    options.plugins = {};
    if (options.plugins.datalabels === undefined) {
      options.plugins.datalabels = {};
      if (options.plugins.datalabels.labels === undefined) {
        options.plugins.datalabels.labels = {};
        if (options.plugins.datalabels.labels.title === undefined) {
          options.plugins.datalabels.labels.title = {};
        }
      }
    }
  }

  if (options.plugins.datalabels.labels.title.display === undefined) {
    // Default this property.
    options.plugins.datalabels.labels.title.display = false;
  }

  if (options.plugins.datalabels.formatter) {
    // the expectation here is for a lambda type function
    options.plugins.datalabels.formatter = eval(options.plugins.datalabels.formatter);
  }

  return options;
};
