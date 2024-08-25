import React from 'react';
import { TextArea } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import * as styled from './styled';
import { generateChartOptions, IChartData } from './utils';

export interface IReportSectionMediaAnalyticsChartProps {
  sectionIndex: number;
  chartIndex: number;
  data?: IChartData;
  onDisableDrag?: (disable: boolean) => void;
}

export const ReportSectionMediaAnalyticsChartAdvanced = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsChartProps
>(({ sectionIndex, chartIndex, data, onDisableDrag, ...rest }, ref) => {
  const { values } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];

  return (
    <styled.ChartAdvanced>
      <TextArea
        name="chart-options"
        value={
          data ? JSON.stringify(generateChartOptions(data, chart.sectionSettings), null, 2) : ''
        }
        disabled
      />
    </styled.ChartAdvanced>
  );
});
