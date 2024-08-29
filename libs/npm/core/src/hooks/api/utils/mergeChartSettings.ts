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
    excludeEmptyValues: override.excludeEmptyValues,
    height: override.height,
    width: override.width,
    autoResize: override.autoResize,
    aspectRatio: override.aspectRatio,
    maintainAspectRatio: override.maintainAspectRatio,
    title: override.title,
    titleFontSize: override.titleFontSize,
    subtitle: override.subtitle,
    subtitleFontSize: override.subtitleFontSize,
    showLegend: override.showLegend,
    legendTitle: override.legendTitle,
    legendTitleFontSize: override.legendTitleFontSize,
    showDataLabels: override.showDataLabels,
    legendLabelFontSize: override.legendLabelFontSize,
    legendLabelBoxWidth: override.legendLabelBoxWidth,
    legendPosition: override.legendPosition,
    legendAlign: override.legendAlign,
    xLegend: override.xLegend,
    xLegendFontSize: override.xLegendFontSize,
    xShowAxisLabels: override.xShowAxisLabels,
    yLegend: override.yLegend,
    yLegendFontSize: override.yLegendFontSize,
    yShowAxisLabels: override.yShowAxisLabels,
    isHorizontal: override.isHorizontal ?? true,
    dataLabelColors: override.dataLabelColors,
    applyColorToValue: override.applyColorToValue,
    datasetColors: override.datasetColors,
    datasetBorderColors: override.datasetBorderColors,
    dataLabelFontSize: override.dataLabelFontSize,
    stacked: override.stacked,
    scaleSuggestedMin: override.scaleSuggestedMin,
    scaleSuggestedMax: override.scaleSuggestedMax,
    scaleCalcMax: override.scaleCalcMax,
    scaleTicksStepSize: override.scaleTicksStepSize,
    minBarLength: override.minBarLength,
    options: {
      ...initSettings.options,
      maintainAspectRatio: override.maintainAspectRatio,
      aspectRatio: override.aspectRatio,
      indexAxis: override.isHorizontal || override.isHorizontal === undefined ? 'x' : 'y',
      plugins: {
        ...initSettings.options.plugins,
        colors: {
          ...initSettings.options.plugins?.colors,
        },
        title: {
          ...initSettings.options.plugins?.title,
          display: override.title ? true : false,
          text: override.title,
          font: {
            size: override.titleFontSize,
          },
        },
        subtitle: {
          ...initSettings.options.plugins?.subtitle,
          display: override.subtitle ? true : false,
          text: override.subtitle,
          font: {
            size: override.titleFontSize,
          },
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
            font: {
              size: override.legendTitleFontSize,
            },
          },
          labels: {
            font: {
              size: override.legendLabelFontSize,
            },
            boxWidth: override.legendLabelBoxWidth,
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
              font: {
                size: override.dataLabelFontSize,
              },
            },
          },
        },
      },
      scales: {
        ...(initSettings.options.scales ?? {}),
        x: {
          ...xScales,
          stacked: override.stacked,
          display: override.xShowAxisLabels,
          title: {
            ...xScales.title,
            display: !!override.xLegend,
            text: override.xLegend,
            font: {
              size: override.xLegendFontSize,
            },
          },
          ticks: {
            ...xScales.ticks,
            stepSize: override.scaleTicksStepSize ? override.scaleTicksStepSize : undefined,
            font: {
              size: override.xLegendFontSize,
            },
          },
        },
        y: {
          ...yScales,
          stacked: override.stacked,
          display: override.yShowAxisLabels,
          title: {
            ...yScales.title,
            display: !!override.yLegend,
            text: override.yLegend,
            font: {
              size: override.yLegendFontSize,
            },
          },
          ticks: {
            ...yScales.ticks,
            stepSize: override.scaleTicksStepSize ? override.scaleTicksStepSize : undefined,
            font: {
              size: override.yLegendFontSize,
            },
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
