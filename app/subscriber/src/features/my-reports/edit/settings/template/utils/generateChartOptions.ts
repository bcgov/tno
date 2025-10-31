import { IChartSectionSettingsModel } from 'tno-core';

import { IChartData } from './IChartData';

export const generateChartOptions = (
  data: IChartData,
  chartSectionSettings: IChartSectionSettingsModel,
) => {
  // Expand the maximum past the max value by the scaleCalcMax value.
  const scaleCalcMax = chartSectionSettings.scaleCalcMax
    ? Math.max.apply(Math, data?.datasets?.map((d) => d.data.map((v) => v ?? 0)).flat() ?? []) +
      chartSectionSettings.scaleCalcMax
    : undefined;

  return {
    responsive: false,
    maintainAspectRatio: chartSectionSettings.maintainAspectRatio,
    aspectRatio: chartSectionSettings.aspectRatio,
    indexAxis: (chartSectionSettings.isHorizontal === undefined || chartSectionSettings.isHorizontal
      ? 'x'
      : 'y') as any, // Need this due to some oddity with Typescript and Chart.js
    scales: {
      x: {
        stacked: chartSectionSettings.stacked,
        display: chartSectionSettings.xShowAxisLabels,
        suggestedMin: !chartSectionSettings.isHorizontal
          ? chartSectionSettings.scaleSuggestedMin
          : undefined,
        suggestedMax: !chartSectionSettings.isHorizontal
          ? chartSectionSettings.scaleSuggestedMax ?? scaleCalcMax
          : undefined,
        title: {
          display: !!chartSectionSettings.xLegend,
          text: chartSectionSettings.xLegend,
          font: {
            size: chartSectionSettings.xLegendFontSize,
          },
        },
        ticks: {
          autoSkip: chartSectionSettings.xAutoSkip,
          stepSize: chartSectionSettings.scaleTicksStepSize,
          minRotation: chartSectionSettings.xMinRotation,
          maxRotation: chartSectionSettings.xMaxRotation,
          font: {
            size: chartSectionSettings.xLegendFontSize,
          },
        },
      },
      y: {
        stacked: chartSectionSettings.stacked,
        display: chartSectionSettings.yShowAxisLabels,
        suggestedMin: chartSectionSettings.isHorizontal
          ? chartSectionSettings.scaleSuggestedMin
          : undefined,
        suggestedMax: chartSectionSettings.isHorizontal
          ? chartSectionSettings.scaleSuggestedMax ?? scaleCalcMax
          : undefined,
        title: {
          display: !!chartSectionSettings.yLegend,
          text: chartSectionSettings.yLegend,
          font: {
            size: chartSectionSettings.yLegendFontSize,
          },
        },
        ticks: {
          yAutoSkip: chartSectionSettings.yAutoSkip,
          stepSize: chartSectionSettings.scaleTicksStepSize,
          minRotation: chartSectionSettings.yMinRotation,
          maxRotation: chartSectionSettings.yMaxRotation,
          font: {
            size: chartSectionSettings.yLegendFontSize,
          },
        },
      },
    },
    plugins: {
      title: {
        display: chartSectionSettings.title ? true : false,
        text: chartSectionSettings.title,
        font: {
          size: chartSectionSettings.titleFontSize,
        },
      },
      subtitle: {
        display: chartSectionSettings.subtitle ? true : false,
        text: chartSectionSettings.subtitle,
        font: {
          size: chartSectionSettings.subtitleFontSize,
        },
      },
      legend: {
        display: chartSectionSettings.showLegend,
        align: chartSectionSettings.legendAlign ? chartSectionSettings.legendAlign : undefined,
        position: chartSectionSettings.legendPosition
          ? chartSectionSettings.legendPosition
          : undefined,
        title: {
          display: chartSectionSettings.legendTitle ? true : false,
          text: chartSectionSettings.legendTitle,
          font: {
            size: chartSectionSettings.legendTitleFontSize,
          },
        },
        labels: {
          font: {
            size: chartSectionSettings.legendLabelFontSize,
          },
          boxWidth: chartSectionSettings.legendLabelBoxWidth,
        },
      },
      datalabels: {
        anchor: chartSectionSettings.dataLabelAnchors,
        backgroundColor: chartSectionSettings.dataLabelBackgroundColors,
        align: chartSectionSettings.dataLabelAligns,
        offset: chartSectionSettings.dataLabelOffsets,
        clip: chartSectionSettings.dataLabelClip,
        clamp: chartSectionSettings.dataLabelClamp,
        labels: {
          title: {
            display: chartSectionSettings.showDataLabels,
            color: chartSectionSettings.dataLabelColors,
            font: {
              size: chartSectionSettings.dataLabelFontSize,
            },
          },
        },
      },
    },
  };
};
