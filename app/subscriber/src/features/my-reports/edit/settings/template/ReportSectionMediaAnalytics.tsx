import { Button } from 'components/button';
import React from 'react';
import { useReports } from 'store/hooks';
import {
  Checkbox,
  Col,
  FormikCheckbox,
  FormikText,
  getSortableOptions,
  IChartTemplateModel,
  IReportSectionChartTemplateModel,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import { ReportSectionMediaAnalyticsChart } from './ReportSectionMediaAnalyticsChart';

export interface IReportSectionMediaAnalyticsProps {
  index: number;
  onDisableDrag?: (disable: boolean) => void;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionMediaAnalytics = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsProps
>(({ index, onDisableDrag, ...rest }, ref) => {
  const { values, setFieldValue, linkedReportContent, setLinkedReportContent } =
    useReportEditContext();
  const [, { getReport }] = useReports();

  const [chartOptions] = React.useState(getSortableOptions(values.template?.chartTemplates ?? []));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  const section = values.sections[index];

  React.useEffect(() => {
    if (
      section.linkedReportId &&
      (!linkedReportContent[section.name] || !linkedReportContent[section.name].length)
    ) {
      // Fetch linked report content.
      getReport(section.linkedReportId, true)
        .then((report) => {
          if (report) {
            let sectionContent = report.instances.length ? report.instances[0].content : [];
            // change the section names to the labels for the chart.
            sectionContent = sectionContent.map((sc) => {
              const section = report.sections.find((s) => s.name === sc.sectionName);
              return { ...sc, sectionName: section?.settings.label ?? sc.sectionName };
            });
            setLinkedReportContent((data) => ({ ...data, [section.name]: sectionContent }));
          }
        })
        .catch(() => {});
    }
    // Only get linked report content when required.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [linkedReportContent, section.linkedReportId, section.name]);

  return (
    <Col gap="0.5rem">
      <Col className="section-options">
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
      </Col>
      <Show visible={!values.template?.chartTemplates.length}>
        <p className="error">
          This template does not currently support charts. Update the template and add the charts it
          supports. Save your report first to apply the template.
        </p>
      </Show>
      <Show visible={!!values.template?.chartTemplates.length}>
        <Col className="section-options" gap="0.5rem">
          <Row className="add-chart">
            <Col>
              <p>Select and order the charts that will be displayed in this section.</p>
              <Select
                name="charts"
                options={chartOptions}
                value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
                onChange={(e) => {
                  const o = e as OptionItem;
                  const chart = values.template?.chartTemplates.find((ct) => ct.id === o?.value);
                  setChart(chart);
                }}
              >
                <Button
                  onClick={() => {
                    if (chart) {
                      const newChart: IReportSectionChartTemplateModel = {
                        ...chart,
                        sectionSettings: {
                          ...chart.sectionSettings,
                          title: '',
                          chartType: chart.settings?.chartTypes.length
                            ? chart.settings.chartTypes[0]
                            : '',
                          groupBy: chart.settings?.groupBy.length ? chart.settings.groupBy[0] : '',
                          groupByOrder: 'asc',
                          dataset: chart.settings?.dataset.length ? chart.settings.dataset[0] : '',
                          datasetOrder: 'asc',
                          datasetValue: chart.settings?.datasetValue.length
                            ? chart.settings.datasetValue[0]
                            : '',
                          excludeEmptyValues: false,
                          isHorizontal: true,
                          showDataLabels: false,
                          width: 500,
                          options: { ...chart.settings?.options },
                        },
                      };
                      const charts = [...section.chartTemplates, newChart].map((ct, i) => {
                        return { ...ct, sortOrder: i };
                      });
                      setFieldValue(`sections.${index}.chartTemplates`, charts);
                      setChart(undefined);
                    }
                  }}
                  disabled={!chart || section.chartTemplates.some((ct) => ct.id === chart.id)}
                >
                  Add Chart
                </Button>
              </Select>
            </Col>
            <Col className="frm-in">
              <label>Report Section Options</label>
              <Row>
                <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
                <span className="info">
                  When hidden the content is still part of the report, but the stories are not
                  displayed in the table of contents, or in their own section.
                </span>
              </Row>
              <Show visible={!section.settings.useAllContent}>
                <Row>
                  <FormikCheckbox
                    name={`sections.${index}.settings.removeDuplicates`}
                    label="Remove duplicate stories"
                  />
                  <span className="info">
                    Do not include in this section content that already exists in the above sections
                    (does not apply to charts that link to other reports)
                  </span>
                </Row>
              </Show>
              <Row>
                <FormikCheckbox
                  name={`sections.${index}.settings.overrideExcludeHistorical`}
                  label={`Include all content from linked ${
                    section.folderId ? 'folder' : 'report'
                  } even if in prior report`}
                />
                <span className="info">
                  This overrides the report option "Exclude stories that have been sent out in
                  previous report" for this section only.
                </span>
              </Row>
              <FormikCheckbox
                name={`sections.${index}.settings.direction`}
                label="Horizontally align the next media analytic chart"
                tooltip="This controls the placement of the next media analytics chart"
                checked={section.settings.direction === 'row'}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${index}.settings.direction`,
                    e.target.checked ? 'row' : 'column',
                  );
                }}
              />
              <Checkbox
                name={`sections.${index}.settings.inTableOfContents`}
                label="Include in Table of Contents"
                checked={
                  values.sections[index].settings.inTableOfContents === undefined
                    ? true
                    : values.sections[index].settings.inTableOfContents
                }
                onChange={(e) => {
                  setFieldValue(`sections.${index}.settings.inTableOfContents`, e.target.checked);
                }}
              />
            </Col>
          </Row>
          <Col className="charts" gap="0.5rem">
            {section.chartTemplates.map((ct, ctIndex) => (
              <ReportSectionMediaAnalyticsChart
                key={ct.id}
                sectionIndex={index}
                chartIndex={ctIndex}
                onDisableDrag={onDisableDrag}
              />
            ))}
          </Col>
        </Col>
      </Show>
    </Col>
  );
});
