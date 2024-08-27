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
import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';

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
}

/**
 * Displays and update the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ChartViewer: React.FC<IChartViewerProps> = ({ sectionIndex, chartIndex, data }) => {
  const { values } = useReportEditContext();

  const canvasRef = React.useRef<HTMLCanvasElement>(null);

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const uid = `${section.id}_${sectionIndex}`;

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
    </div>
  );
};
