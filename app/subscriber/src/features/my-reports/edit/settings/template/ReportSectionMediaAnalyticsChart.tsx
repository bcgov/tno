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
import { Col, IReportInstanceContentModel, Row, Show, ToggleButton } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import { ReportSectionMediaAnalyticsChartAdvanced } from './ReportSectionMediaAnalyticsChartAdvanced';
import { ReportSectionMediaAnalyticsChartWizard } from './ReportSectionMediaAnalyticsChartWizard';
import { convertToChart, IChartData } from './utils';

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
  const {
    values,
    setFieldValue,
    testData,
    regenerateTestData,
    linkedReportContent,
    setLinkedReportContent,
  } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const [showAdvanced, setShowAdvanced] = React.useState(false);
  const [showConfig, setShowConfig] = React.useState(initShowConfig ?? true);
  const [data, setData] = React.useState<IChartData>();
  const [reportContent, setReportContent] = React.useState(
    values.instances.length ? values.instances[0].content : [],
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
    let sectionContent: IReportInstanceContentModel[] = [];
    if (section.linkedReportId) {
      sectionContent = linkedReportContent[section.name] ?? [];
    } else {
      sectionContent =
        section.filterId || section.folderId
          ? reportContent.filter((rc) => rc.sectionName === section.name)
          : reportContent;
    }
    setData(
      convertToChart(section, chart, showReportData ? sectionContent : testData, values.sections),
    );
  }, [
    section,
    chart,
    reportContent,
    showReportData,
    testData,
    values.sections,
    linkedReportContent,
  ]);

  React.useEffect(() => {
    setReportContent(
      values.instances.length && values.instances[0] ? values.instances[0].content : [],
    );
  }, [values.instances]);

  return (
    <Col key={chart.id} className="chart">
      <Row className="chart-header">
        <Col flex="1" onClick={() => setShowConfig((show) => !show)}>
          <b>{chart.name}</b>
        </Col>
        <Col flex="2" onClick={() => setShowConfig((show) => !show)}>
          {chart.description}
        </Col>
        <Row gap="1.5rem">
          <ToggleButton
            title={'Use test data'}
            label="Use test data"
            on={<FaToggleOn />}
            off={<FaToggleOff />}
            onClick={() => toggleReportData()}
            value={!showReportData}
          />
          <Action
            icon={<FaRotate />}
            title={!showReportData ? 'Randomize test data' : 'Refresh data'}
            onClick={() => {
              if (!showReportData) regenerateTestData();
              else setLinkedReportContent((data) => ({ ...data, [section.name]: [] }));
            }}
          />
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
