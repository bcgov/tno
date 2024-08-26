import { Action } from 'components/action';
import React from 'react';
import {
  FaAngleDown,
  FaGear,
  FaMinus,
  FaRotate,
  FaToggleOff,
  FaToggleOn,
  FaTrash,
  FaWandMagicSparkles,
} from 'react-icons/fa6';
import { Col, Row, Show, ToggleButton } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import { ReportSectionMediaAnalyticsChartAdvanced } from './ReportSectionMediaAnalyticsChartAdvanced';
import { ReportSectionMediaAnalyticsChartWizard } from './ReportSectionMediaAnalyticsChartWizard';
import { generateChartData, IChartData } from './utils';

export interface IReportSectionMediaAnalyticsChartProps {
  sectionIndex: number;
  chartIndex: number;
  showConfig?: boolean;
  onDisableDrag?: (disable: boolean) => void;
}

/**
 * Provides a way to configure a chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportSectionMediaAnalyticsChart = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsChartProps
>(({ showConfig: initShowConfig, sectionIndex, chartIndex, onDisableDrag, ...rest }, ref) => {
  const { values, setFieldValue, testData, regenerateTestData } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const [showAdvanced, setShowAdvanced] = React.useState(false);
  const [showConfig, setShowConfig] = React.useState(initShowConfig ?? true);
  const [data, setData] = React.useState<IChartData>();
  const [reportContent, setReportContent] = React.useState(
    values.instances ? values.instances[0].content : [],
  );
  const [showReportData, setShowReportData] = React.useState(
    values.instances.length > 0 && values.instances[0].content.length > 0,
  );

  const toggleReportData = React.useCallback(() => setShowReportData((show) => !show), []);

  React.useEffect(() => {
    // If there is data, then show it in the charts.
    setShowReportData(values.instances.length > 0 && values.instances[0].content.length > 0);
  }, [values.instances]);

  React.useEffect(() => {
    setData(generateChartData(chart, showReportData ? reportContent : testData, values.sections));
  }, [chart, reportContent, showReportData, testData, values.sections]);

  React.useEffect(() => {
    setReportContent(values.instances ? values.instances[0].content : []);
  }, [values.instances]);

  return (
    <Col key={chart.id} className="chart">
      <Row className="chart-header">
        <Col flex="1">
          <b>{chart.name}</b>
        </Col>
        <Col flex="2">{chart.description}</Col>
        <Row gap="1.5rem">
          <ToggleButton
            title={showReportData ? 'Using report data' : 'Using test data'}
            on={<FaToggleOn />}
            onLabel="Using report data"
            off={<FaToggleOff />}
            offLabel="Using test data"
            onClick={() => toggleReportData()}
            value={showReportData}
          />
          <Show visible={!showReportData}>
            <Action
              icon={<FaRotate />}
              title="Randomize test data"
              onClick={() => {
                regenerateTestData();
              }}
            />
          </Show>
          <ToggleButton
            title={showAdvanced ? 'Chart Wizard' : 'Advanced Options'}
            on={<FaWandMagicSparkles />}
            off={<FaGear />}
            onClick={() => setShowAdvanced((show) => !show)}
            value={showAdvanced}
          />
          <Action
            icon={<FaTrash />}
            title="Delete"
            onClick={() => {
              let items = [...section.chartTemplates];
              items.splice(chartIndex, 1);
              setFieldValue(`sections.${sectionIndex}.chartTemplates`, items);
            }}
          />
          <ToggleButton
            title={showConfig ? 'Hide' : 'Show'}
            on={<FaMinus />}
            off={<FaAngleDown />}
            onClick={() => setShowConfig((show) => !show)}
            value={showConfig}
          />
        </Row>
      </Row>
      <Show visible={showConfig}>
        <Row className="chart-settings">
          <Show visible={!showAdvanced}>
            <ReportSectionMediaAnalyticsChartWizard
              sectionIndex={sectionIndex}
              chartIndex={chartIndex}
              data={data}
              onDisableDrag={onDisableDrag}
            />
          </Show>
          <Show visible={showAdvanced}>
            <ReportSectionMediaAnalyticsChartAdvanced
              sectionIndex={sectionIndex}
              chartIndex={chartIndex}
              data={data}
              onDisableDrag={onDisableDrag}
            />
          </Show>
        </Row>
      </Show>
    </Col>
  );
});
