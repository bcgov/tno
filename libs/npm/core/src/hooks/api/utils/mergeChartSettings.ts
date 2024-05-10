import { IChartSectionSettingsModel } from '..';
import { defaultChartSectionSettings } from '../constants';

/**
 * Generates a new chart settings configuration based on the default, current and overridden values.
 * @param defaultChartOptions Original chart options provided by the template. Only used if 'currentSettings' is undefined.
 * @param currentSettings The current chart settings you want to merge.
 * @param overrideSettings The chart settings to apply and override the current chart settings
 * @returns New chart settings.
 */
export const mergeChartSettings = (
  defaultChartOptions: any,
  currentSettings: IChartSectionSettingsModel | undefined,
  overrideSettings: Partial<IChartSectionSettingsModel>,
): IChartSectionSettingsModel => {
  const initSettings = {
    ...(currentSettings ?? defaultChartSectionSettings),
    options: defaultChartOptions,
  };
  const override = { ...initSettings, ...overrideSettings };
  const { xScales, yScales } = setScales(initSettings, override);

  return {
    ...initSettings,
    chartType: override.chartType,
    dataset: override.dataset,
    datasetValue: override.datasetValue,
    groupBy: override.groupBy,
    height: override.height,
    width: override.width,
    title: override.title,
    subtitle: override.subtitle,
    showLegend: override.showLegend,
    legendTitle: override.legendTitle,
    legendPosition: override.legendPosition,
    legendAlign: override.legendAlign,
    showAxis: override.showAxis,
    xLegend: override.xLegend,
    yLegend: override.yLegend,
    isHorizontal: override.isHorizontal,
    dataLabelColors: override.dataLabelColors,
    datasetColors: override.datasetColors,
    showDataLabels: override.showDataLabels,
    stacked: override.stacked,
    scaleSuggestedMin: override.scaleSuggestedMin,
    scaleSuggestedMax: override.scaleSuggestedMax,
    scaleTicksStepSize: override.scaleTicksStepSize,
    minBarLength: override.minBarLength,
    options: {
      ...initSettings.options,
      indexAxis: override.isHorizontal ? 'y' : 'x',
      plugins: {
        ...initSettings.options.plugins,
        colors: {
          ...initSettings.options.plugins?.colors,
        },
        title: {
          ...initSettings.options.plugins?.title,
          display: override.title ? true : false,
          text: override.title,
        },
        subtitle: {
          ...initSettings.options.plugins?.subtitle,
          display: override.subtitle ? true : false,
          text: override.subtitle,
        },
        legend: {
          ...initSettings.options.plugins?.legend,
          display: override.showLegend,
          align: override.legendAlign ? override.legendAlign : undefined,
          position: override.legendPosition ? override.legendPosition : undefined,
          title: {
            ...initSettings.options.plugins?.legend?.title,
            display: override.legendTitle ? true : false,
            text: override.legendTitle,
          },
        },
        datalabels: {
          ...(initSettings.options.plugins?.datalabels ?? {}),
          labels: {
            ...(initSettings.options.plugins?.datalabels?.labels ?? {}),
            title: {
              ...(initSettings.options.plugins?.datalabels?.labels?.title ?? {}),
              color: override.dataLabelColors,
              display: override.showDataLabels,
            },
          },
        },
      },
      scales: {
        ...(initSettings.options.scales ?? {}),
        x: {
          ...xScales,
          stacked: override.stacked,
          display: override.showAxis,
          title: {
            ...xScales.title,
            display: !!override.xLegend,
            text: override.xLegend,
          },
          ticks: {
            ...xScales.ticks,
            stepSize: override.scaleTicksStepSize ? override.scaleTicksStepSize : undefined,
          },
        },
        y: {
          ...yScales,
          stacked: override.stacked,
          display: override.showAxis,
          title: {
            ...yScales.title,
            display: !!override.yLegend,
            text: override.yLegend,
          },
          ticks: {
            ...yScales.ticks,
            stepSize: override.scaleTicksStepSize ? override.scaleTicksStepSize : undefined,
          },
        },
      },
    },
  };
};

const setScales = (
  defaultSettings: IChartSectionSettingsModel,
  overrideSettings: Partial<IChartSectionSettingsModel>,
) => {
  const {
    suggestedMin: xMin,
    suggestedMax: xMax,
    ...xScalesRest
  } = defaultSettings.options.scales?.x ?? {};
  const {
    suggestedMin: yMin,
    suggestedMax: yMax,
    ...yScalesRest
  } = defaultSettings.options.scales?.y ?? {};

  const xScales =
    overrideSettings.scaleSuggestedMin !== undefined ||
    overrideSettings.scaleSuggestedMax !== undefined
      ? {
          ...defaultSettings.options.scales?.x,
          suggestedMin: overrideSettings.scaleSuggestedMin,
          suggestedMax: overrideSettings.scaleSuggestedMax,
        }
      : { ...xScalesRest };
  const yScales =
    overrideSettings.scaleSuggestedMin !== undefined ||
    overrideSettings.scaleSuggestedMax !== undefined
      ? {
          ...defaultSettings.options.scales?.y,
          suggestedMin: overrideSettings.scaleSuggestedMin,
          suggestedMax: overrideSettings.scaleSuggestedMax,
        }
      : { ...yScalesRest };

  return {
    xScales,
    yScales,
  };
};
