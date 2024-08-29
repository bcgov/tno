import {
  CategoryScale,
  ChartTypeRegistry,
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
import { useModal } from 'tno-core';

import {
  determineBackgroundColor,
  determineBorderColor,
  generateChartOptions,
  getSectionLabel,
  IChartData,
  IChartDataset,
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
    const axisColumnWidth =
      data && chart.sectionSettings.width ? chart.sectionSettings.width / data.labels.length : 0;
    if (data && chart.sectionSettings.autoResize && axisColumnWidth < minAxisColumnWidth) {
      toggleModal();
    }
    // Only update when values change in data or chart.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    chart.sectionSettings.autoResize,
    chart.sectionSettings.width,
    data?.labels.length,
    minAxisColumnWidth,
  ]);

  React.useEffect(() => {
    if (canvasRef.current !== null && data) {
      myChart[uid]?.destroy();
      const widthValue = chart.sectionSettings.width ? chart.sectionSettings.width : 500;
      if (chart.sectionSettings.height) canvasRef.current.height = chart.sectionSettings.height;
      if (chart.sectionSettings.width) canvasRef.current.width = widthValue;

      myChart[uid] = new ChartJS(canvasRef.current, {
        type: chart.sectionSettings.chartType as keyof ChartTypeRegistry,
        data: {
          ...data,
          datasets:
            data?.datasets?.map(
              (dataset: IChartDataset, index: number) =>
                ({
                  ...dataset,
                  label: getSectionLabel(dataset.label, chart.sectionSettings, values.sections),
                  minBarLength: chart.sectionSettings.minBarLength,
                  borderWidth: 2,
                  spanGaps: true,
                  backgroundColor: determineBackgroundColor(
                    index,
                    dataset.label,
                    chart.sectionSettings,
                  ),
                  borderColor: determineBorderColor(index, dataset.label, chart.sectionSettings),
                } as any),
            ) ?? [],
        },
        options: generateChartOptions(data, chart.sectionSettings),
      });
      myChart[uid].resize(widthValue, chart.sectionSettings.height);
    }
  }, [chart.sectionSettings, data, values.sections, uid]);

  return (
    <div>
      <div>
        <canvas ref={canvasRef} id={`my-chart-${uid}`} />
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
            const isHorizontal =
              chart.sectionSettings.isHorizontal ||
              chart.sectionSettings.isHorizontal === undefined;
            const size = data.labels.length * minAxisColumnWidth;
            const height = isHorizontal ? chart.sectionSettings.height : size;
            setFieldValue(`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`, {
              ...chart.sectionSettings,
              width: isHorizontal ? size : chart.sectionSettings.width,
              height: height,
              aspectRatio: height ? undefined : chart.sectionSettings.aspectRatio,
            });
          }
          toggleModal();
        }}
      />
    </div>
  );
};
