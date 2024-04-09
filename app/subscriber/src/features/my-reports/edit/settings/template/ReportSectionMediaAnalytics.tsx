import { Action } from 'components/action';
import { Button } from 'components/button';
import { chartTypeOptions, groupByOptions } from 'features/my-reports/constants';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa6';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  getSortableOptions,
  IChartTemplateModel,
  IReportSectionChartTemplateModel,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

export interface IReportSectionMediaAnalyticsProps {
  index: number;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionMediaAnalytics = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsProps
>(({ index, ...rest }, ref) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();

  const [chartOptions] = React.useState(getSortableOptions(values.template?.chartTemplates ?? []));
  const [chart, setChart] = React.useState<IChartTemplateModel>();

  const section = values.sections[index];

  return (
    <Col gap="0.5rem">
      <Col className="section-options">
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
      </Col>
      <Show visible={!values.template?.chartTemplates.length}>
        <p className="error">
          This template does not currently support charts. Update the template and add the charts it
          supports.
        </p>
      </Show>
      <Show visible={!!values.template?.chartTemplates.length}>
        <Col className="section-options" gap="0.5rem">
          <p>Select and order the charts that will be displayed in this section.</p>
          <Row className="add-chart">
            <Select
              name="charts"
              options={chartOptions}
              value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
              width="50ch"
              onChange={(e) => {
                const o = e as OptionItem;
                const chart = values.template?.chartTemplates.find((ct) => ct.id === o?.value);
                setChart(chart);
              }}
            />
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
                      isHorizontal: false,
                      showDataLabels: false,
                      width: 500,
                      height: 500,
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
          </Row>
          <Col className="charts" gap="0.5rem">
            {section.chartTemplates.map((ct, ctIndex) => (
              <Col key={ct.id} className="chart">
                <Row className="chart-header">
                  <Col flex="1">
                    <b>{ct.name}</b>
                  </Col>
                  <Col flex="2">{ct.description}</Col>
                  <Col>
                    <Action
                      icon={<FaTrash />}
                      onClick={() => {
                        let items = [...section.chartTemplates];
                        items.splice(ctIndex, 1);
                        setFieldValue(`sections.${index}.chartTemplates`, items);
                      }}
                    />
                  </Col>
                </Row>
                <Row className="chart-settings" alignItems="center">
                  <FormikSelect
                    label="Chart Type"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.chartType`}
                    value={
                      chartTypeOptions.find((o) => o.value === ct.sectionSettings.chartType) ?? ''
                    }
                    options={chartTypeOptions.filter((o) =>
                      ct.settings.chartTypes.includes(o.value),
                    )}
                    isClearable={false}
                  />
                  <FormikSelect
                    label="Group By"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.groupBy`}
                    value={groupByOptions.find((o) => o.value === ct.sectionSettings.groupBy) ?? ''}
                    options={groupByOptions.filter((o) => ct.settings.groupBy.includes(o.value))}
                    isClearable={false}
                  />
                  <FormikText
                    label="Width"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.width`}
                    value={ct.sectionSettings.width ?? 500}
                    type="number"
                    width="10ch"
                  />
                  <FormikText
                    label="Height"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.height`}
                    value={ct.sectionSettings.height ?? 500}
                    type="number"
                    width="10ch"
                  />
                  <FormikCheckbox
                    label="Is Horizontal"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.isHorizontal`}
                    checked={ct.sectionSettings.isHorizontal ?? true}
                  />
                  <FormikCheckbox
                    label="Show Legend"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.showLegend`}
                    checked={ct.sectionSettings.showLegend ?? true}
                  />
                  <FormikCheckbox
                    label="Show Legend Title"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.showLegendTitle`}
                    checked={ct.sectionSettings.showLegendTitle ?? false}
                  />
                  <FormikCheckbox
                    label="Show Data Labels"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.showDataLabels`}
                    checked={ct.sectionSettings.showDataLabels ?? false}
                  />
                  <FormikCheckbox
                    label="Show Axis"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.showAxis`}
                    checked={ct.sectionSettings.showAxis ?? true}
                  />
                </Row>
              </Col>
            ))}
          </Col>
        </Col>
      </Show>
      <Col className="frm-in">
        <label>Report Section Options</label>
        <FormikCheckbox
          name={`sections.${index}.settings.removeDuplicates`}
          label="Remove duplicate stories"
          tooltip="Remove content from this section that is in above sections"
        />
        <FormikCheckbox
          name={`sections.${index}.settings.hideEmpty`}
          label="Hide this section in the report when empty"
        />
      </Col>
    </Col>
  );
});
