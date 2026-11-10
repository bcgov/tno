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

import { Col, IReportInstanceContentModel, Row, Show, ToggleButton } from '@/shared';
import { Action } from '@/subscriber/components/action';
import { useReports } from '@/subscriber/store/hooks';

import { useReportEditContext } from '../../ReportEditContext';
import { ReportSectionMediaAnalyticsChartAdvanced } from './ReportSectionMediaAnalyticsChartAdvanced';
import { ReportSectionMediaAnalyticsChartWizard } from './ReportSectionMediaAnalyticsChartWizard';
import { convertToChart, IChartData, isAggregateChart, normalizeChartData } from './utils';

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
    contentLoaded,
    loadContent,
  } = useReportEditContext();

  const [, { getSectionChartData }] = useReports();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const aggregate = isAggregateChart(chart);
  const instanceId = values.instances.length ? values.instances[0].id : undefined;
  const sectionSettings = JSON.stringify(chart.sectionSettings);
  const [showAdvanced, setShowAdvanced] = React.useState(false);
  const [showConfig, setShowConfig] = React.useState(initShowConfig ?? true);
  const [data, setData] = React.useState<IChartData>();
  const [reportContent, setReportContent] = React.useState(
    values.instances.length ? values.instances[0].content : [],
  );
  // Until the stories are loaded, a report with an instance is assumed to have some.
  const hasReportData =
    values.instances.length > 0 && (!contentLoaded || values.instances[0].content.length > 0);
  const [showReportData, setShowReportData] = React.useState(aggregate || hasReportData);
  const [error, setError] = React.useState('');

  const toggleReportData = React.useCallback(() => setShowReportData((show) => !show), []);

  React.useEffect(() => {
    // If there is data, then show it in the charts.
    setShowReportData(aggregate || hasReportData);
  }, [aggregate, hasReportData]);

  // A chart that does not aggregate is drawn from the stories in the browser.
  React.useEffect(() => {
    if (!aggregate && showReportData && !section.linkedReportId && !contentLoaded) loadContent();
  }, [aggregate, contentLoaded, loadContent, section.linkedReportId, showReportData]);

  const convertContent = React.useCallback(() => {
    let sectionContent: IReportInstanceContentModel[] = [];
    if (section.linkedReportId) {
      sectionContent = linkedReportContent[section.name] ?? [];
    } else {
      sectionContent =
        section.filterId || section.folderId
          ? reportContent.filter((rc) => rc.sectionName === section.name)
          : reportContent;
    }
    return convertToChart(
      section,
      chart,
      showReportData ? sectionContent : testData,
      values.sections,
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

  // A chart template that aggregates shows what the report would, without any stories in the browser.
  const fromApi = aggregate && showReportData && !!values.id;

  React.useEffect(() => {
    if (fromApi) return;
    setError('');
    setData(convertContent());
  }, [fromApi, convertContent]);

  React.useEffect(() => {
    if (!fromApi) return;
    let cancelled = false;
    const timer = setTimeout(async () => {
      try {
        const result = await getSectionChartData(values.id, section.id, chart.id, {
          sectionSettings: JSON.parse(sectionSettings),
          useAllContent: !!section.settings.useAllContent,
          filterId: section.filterId,
          folderId: section.folderId,
          linkedReportId: section.linkedReportId,
          instanceId,
        });
        if (cancelled) return;
        setError('');
        if (result) setData(normalizeChartData(result, chart.sectionSettings));
        else setShowReportData(false); // There are no stories to take data from yet.
      } catch (ex: any) {
        if (cancelled) return;
        setError(ex?.response?.data?.error ?? 'Unable to load the chart data.');
        setData(undefined);
      }
    }, 400);
    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
    // Settings are compared by value, as the form creates new objects on every render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    fromApi,
    values.id,
    section.id,
    chart.id,
    sectionSettings,
    section.settings.useAllContent,
    section.filterId,
    section.folderId,
    section.linkedReportId,
    instanceId,
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
        <Show visible={!!error}>
          <p className="error">{error}</p>
        </Show>
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
