import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
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

import { chartTypeOptions, groupByOptions } from '../../../constants';
import { IReportForm } from '../../../interfaces';
import { IReportSectionProps } from './ReportSection';

export const ReportSectionCharts = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();

    const [chartOptions] = React.useState(
      getSortableOptions(values.template?.chartTemplates ?? []),
    );
    const [chart, setChart] = React.useState<IChartTemplateModel>();

    const section = values.sections[index];

    return (
      <Col className="frm-in">
        <label>Charts</label>
        <Show visible={!values.template?.chartTemplates.length}>
          <p className="error">The template for this report does not have any available charts.</p>
        </Show>
        <Show visible={!!values.template?.chartTemplates.length}>
          <p>Select and order the charts that will be displayed in this section.</p>
          <Row className="add-chart">
            <Col flex="1">
              <Select
                name="charts"
                options={chartOptions.filter(
                  (c) => !section.chartTemplates.some((t) => t.id === c.value),
                )}
                value={chartOptions.find((c) => c.value === chart?.id) ?? ''}
                onChange={(e) => {
                  const o = e as OptionItem;
                  const chart = values.template?.chartTemplates.find((ct) => ct.id === o?.value);
                  setChart(chart);
                }}
              />
            </Col>
            <Button
              variant={ButtonVariant.secondary}
              onClick={() => {
                if (chart) {
                  const newChart: IReportSectionChartTemplateModel = {
                    ...chart,
                    sectionSettings: {
                      ...chart.sectionSettings,
                      title: '',
                      chartType: chart.settings?.chartTypes.length
                        ? chart.settings?.chartTypes[0]
                        : '',
                      groupBy: chart.settings?.groupBy.length ? chart.settings.groupBy[0] : '',
                      isHorizontal: false,
                      showDataValues: false,
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

          <Col className="charts">
            {section.chartTemplates.map((ct, ctIndex) => (
              <Row key={ct.id}>
                <Col flex="1">
                  <Row>
                    <Col flex="1">
                      <b>{ct.name}</b>
                    </Col>
                    <Col flex="2">{ct.description}</Col>
                    <Col>
                      <Button
                        variant={ButtonVariant.danger}
                        onClick={() => {
                          let items = [...section.chartTemplates];
                          items.splice(ctIndex, 1);
                          setFieldValue(`sections.${index}.chartTemplates`, items);
                        }}
                      >
                        <FaTrash />
                      </Button>
                    </Col>
                  </Row>
                  <Row className="chart-settings">
                    {/* <FormikText
                    label="Title"
                    name={`sections.${index}.chartTemplates.${ctIndex}.sectionSettings.title`}
                    value={ct.sectionSettings.title}
                    UPDATE VALUE IN OPTIONS
                  /> */}
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
                      value={
                        groupByOptions.find((o) => o.value === ct.sectionSettings.groupBy) ?? ''
                      }
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
                  </Row>
                </Col>
              </Row>
            ))}
          </Col>
        </Show>
      </Col>
    );
  },
);
