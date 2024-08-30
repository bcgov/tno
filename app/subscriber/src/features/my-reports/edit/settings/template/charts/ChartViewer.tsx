import {
  CategoryScale,
  ChartTypeRegistry,
  Colors,
  Legend,
  LinearScale,
  LineElement,
  PointElement,
  Title,
  Tooltip,
} from 'chart.js';
import ChartJS from 'chart.js/auto';
import ChartDataLabels from 'chartjs-plugin-datalabels';
import { Modal } from 'components/modal';
import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import { IChartSectionSettingsModel, Show, useModal } from 'tno-core';

import {
  calcAutoSize,
  determineBackgroundColor,
  determineBorderColor,
  generateChartOptions,
  getSectionLabel,
  IChartData,
  IChartDataset,
  shouldResizeChart,
} from '../utils';
import { IChartSectionProps } from './IChartSectionProps';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  ChartDataLabels,
  Colors,
);

let myChart: Record<string, ChartJS> = {};

export interface IChartViewerProps extends IChartSectionProps {
  /** Chart data */
  data?: IChartData;
  minAxisColumnWidth?: number;
}

/**
 * Displays and update the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ChartViewer: React.FC<IChartViewerProps> = ({
  sectionIndex,
  chartIndex,
  data,
  minAxisColumnWidth = 30,
}) => {
  const { values, setFieldValue } = useReportEditContext();
  const { isShowing: showModal, toggle: toggleModal } = useModal();

  const canvasRef = React.useRef<HTMLCanvasElement>(null);

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const uid = `${section.id}_${sectionIndex}`;

  React.useEffect(() => {
    if (
      data &&
      chart.sectionSettings.autoResize &&
      shouldResizeChart(data.labels.length, chart.sectionSettings, minAxisColumnWidth)
    ) {
      // toggleModal();
      // Commented out temporarily until I can present the fact the chart is being resized in a better way.
    }
    // Only update when values change in data or chart.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    chart.sectionSettings.autoResize,
    chart.sectionSettings.width,
    chart.sectionSettings.height,
    data?.labels.length,
    minAxisColumnWidth,
  ]);

  React.useEffect(() => {
    if (canvasRef.current !== null && data) {
      myChart[uid]?.destroy();

      // Auto resize if it's on
      let chartSectionSettings: IChartSectionSettingsModel = {
        ...chart.sectionSettings,
        width: chart.sectionSettings.width ? chart.sectionSettings.width : 500,
      };
      if (
        chart.sectionSettings.autoResize &&
        shouldResizeChart(data.labels.length, chart.sectionSettings, minAxisColumnWidth)
      ) {
        chartSectionSettings = calcAutoSize(
          data.labels.length,
          chart.sectionSettings,
          minAxisColumnWidth,
        );
      }

      if (chartSectionSettings.height) canvasRef.current.height = chartSectionSettings.height;
      if (chartSectionSettings.width) canvasRef.current.width = chartSectionSettings.width;

      myChart[uid] = new ChartJS(canvasRef.current, {
        type: chartSectionSettings.chartType as keyof ChartTypeRegistry,
        data: {
          ...data,
          datasets:
            data?.datasets?.map(
              (dataset: IChartDataset, index: number) =>
                ({
                  ...dataset,
                  label: getSectionLabel(dataset.label, chartSectionSettings, values.sections),
                  minBarLength: chartSectionSettings.minBarLength,
                  borderWidth: 2,
                  spanGaps: true,
                  backgroundColor: determineBackgroundColor(
                    index,
                    dataset.label,
                    chartSectionSettings,
                  ),
                  borderColor: determineBorderColor(index, dataset.label, chartSectionSettings),
                } as any),
            ) ?? [],
        },
        options: generateChartOptions(data, chartSectionSettings),
      });
      myChart[uid].resize(chartSectionSettings.width, chartSectionSettings.height);
    }
  }, [chart.sectionSettings, data, values.sections, uid, minAxisColumnWidth]);

  return (
    <div>
      <div>
        <Show visible={!data?.labels.length}>
          <p>You have no content to display for this section.</p>
          <p>
            Go to the Content tab and regenerate this{' '}
            {section.filterId || section.folderId || section.linkedReportId ? 'section' : 'report'}{' '}
            to pull in content.
          </p>
        </Show>
        <Show visible={!!data?.labels.length}>
          <canvas ref={canvasRef} id={`my-chart-${uid}`} />
        </Show>
      </div>
      <Modal
        isShowing={showModal}
        hide={() => toggleModal()}
        headerText="Chart Width Resized"
        body={
          <div>
            <p>This chart will not display all axis values because it is too small.</p>
            <p>Do you want to automatically apply a size that should fit the data?</p>
          </div>
        }
        type="default"
        confirmText="Automatically Resize"
        onConfirm={() => {
          if (data) {
            // Calculate a new size for the chart based on the number of labels on the axis.
            // Handle the direction of the axis, and aspect ratio.
            const settings = calcAutoSize(
              data.labels.length,
              chart.sectionSettings,
              minAxisColumnWidth,
            );
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              settings,
            );
          }
          toggleModal();
        }}
      />
    </div>
  );
};
