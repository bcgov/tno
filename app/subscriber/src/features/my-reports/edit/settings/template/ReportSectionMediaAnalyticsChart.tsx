import { Action } from 'components/action';
import { Colours } from 'components/colours';
import { chartTypeOptions, groupByOptions } from 'features/my-reports/constants';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa6';
import {
  Col,
  datasetOptions,
  datasetValueOptions,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  IOptionItem,
  legendAlignOptions,
  legendPositionOptions,
  mergeChartSettings,
  OptionItem,
  Row,
} from 'tno-core';

export interface IReportSectionMediaAnalyticsChartProps {
  sectionIndex: number;
  chartIndex: number;
  onDisableDrag?: (disable: boolean) => void;
}

export const ReportSectionMediaAnalyticsChart = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsChartProps
>(({ sectionIndex, chartIndex, onDisableDrag, ...rest }, ref) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const [allowedChartTypeOptions] = React.useState(
    chartTypeOptions.filter((o) => chart.settings.chartTypes.includes(o.value)),
  );
  const [allowedDatasetOptions] = React.useState(
    datasetOptions.filter((o) => chart.settings.dataset.includes(o.value)),
  );
  const [allowedDatasetValueOptions] = React.useState(
    datasetValueOptions.filter((o) => chart.settings.datasetValue.includes(o.value)),
  );

  return (
    <Col key={chart.id} className="chart">
      <Row className="chart-header">
        <Col flex="1">
          <b>{chart.name}</b>
        </Col>
        <Col flex="2">{chart.description}</Col>
        <Col>
          <Action
            icon={<FaTrash />}
            onClick={() => {
              let items = [...section.chartTemplates];
              items.splice(chartIndex, 1);
              setFieldValue(`sections.${sectionIndex}.chartTemplates`, items);
            }}
          />
        </Col>
      </Row>
      <Row className="chart-settings" alignItems="flex-start">
        <Col gap="1rem">
          <FormikSelect
            label="Chart Type"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.chartType`}
            value={chartTypeOptions.find((o) => o.value === chart.sectionSettings.chartType) ?? ''}
            options={allowedChartTypeOptions}
            isClearable={false}
            required
          />
          <Row gap="1rem">
            <FormikText
              label="Width"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.width`}
              value={chart.sectionSettings.width ?? 500}
              type="number"
              width="10ch"
            />
            <FormikText
              label="Height"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.height`}
              value={chart.sectionSettings.height ?? 500}
              type="number"
              width="10ch"
            />
          </Row>
        </Col>
        <Col gap="1rem">
          <FormikSelect
            label="Dataset"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataset`}
            value={datasetOptions.find((o) => o.value === chart.sectionSettings.dataset) ?? ''}
            options={allowedDatasetOptions}
            isClearable={false}
            required
          />
          <FormikSelect
            label="Dataset Value"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.datasetValue`}
            value={
              datasetValueOptions.find((o) => o.value === chart.sectionSettings.datasetValue) ?? ''
            }
            options={allowedDatasetValueOptions}
            isClearable={false}
            required
            onChange={(newValue) => {
              var option = newValue as OptionItem;
              var isSentiment = option.value === 'sentiment';
              setFieldValue(
                `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                  datasetValue: (option.value as any) ?? 'count',
                  scaleSuggestedMin: isSentiment ? -5 : undefined,
                  scaleSuggestedMax: isSentiment ? 5 : undefined,
                  scaleTicksStepSize: isSentiment ? 1 : undefined,
                  datasetColors:
                    isSentiment && chart.sectionSettings.dataset === ''
                      ? ['green', 'gold', 'red']
                      : [],
                  minBarLength: isSentiment ? 10 : undefined,
                }),
              );
            }}
          />
          <FormikSelect
            label="Group By"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.groupBy`}
            value={groupByOptions.find((o) => o.value === chart.sectionSettings.groupBy) ?? ''}
            options={groupByOptions.filter((o) => chart.settings.groupBy.includes(o.value))}
            isClearable={false}
            required
          />
        </Col>
        <Col className="frm-in" gap="1rem">
          <Col className="frm-in">
            <label>Layout</label>
            <Col>
              <FormikCheckbox
                label="Stack Dataset"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.stacked`}
                checked={!!chart.sectionSettings.stacked}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      stacked: e.target.checked,
                    }),
                  );
                }}
              />
              <FormikCheckbox
                label="Show values inside chart"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showDataLabels`}
                checked={!!chart.sectionSettings.showDataLabels}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      showDataLabels: e.target.checked,
                    }),
                  );
                }}
              />
              <FormikCheckbox
                label="Flip X and Y axis"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.isHorizontal`}
                checked={!!chart.sectionSettings.isHorizontal}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      isHorizontal: e.target.checked,
                    }),
                  );
                }}
              />
            </Col>
          </Col>
          <Col className="frm-in">
            <label>Scale</label>
            <Col gap="1rem">
              <Row gap="1rem">
                <FormikText
                  label="Min Value"
                  name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleSuggestedMin`}
                  value={chart.sectionSettings.scaleSuggestedMin ?? ''}
                  type="number"
                  width="10ch"
                  onChange={(e) => {
                    const value = parseInt(e.target.value);
                    setFieldValue(
                      `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                      mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                        scaleSuggestedMin: value ? value : undefined,
                      }),
                    );
                  }}
                />
                <FormikText
                  label="Max Value"
                  name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleSuggestedMax`}
                  value={chart.sectionSettings.scaleSuggestedMax ?? ''}
                  type="number"
                  width="10ch"
                  onChange={(e) => {
                    const value = parseInt(e.target.value);
                    setFieldValue(
                      `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                      mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                        scaleSuggestedMax: value ? value : undefined,
                      }),
                    );
                  }}
                />
              </Row>
              <FormikText
                label="Step Size"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleTicksStepSize`}
                value={chart.sectionSettings.scaleTicksStepSize ?? ''}
                type="number"
                width="10ch"
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      scaleTicksStepSize: value ? value : undefined,
                    }),
                  );
                }}
              />
              <FormikText
                name="minBarLength"
                label="Min bar length"
                width="8ch"
                type="number"
                value={chart.sectionSettings.minBarLength ?? ''}
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      minBarLength: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Col>
          </Col>
        </Col>
        <Col className="frm-in">
          <label>Legend Text</label>
          <Row gap="1rem">
            <Col gap="1rem">
              <FormikText
                label="Title"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.title`}
                value={chart.sectionSettings.title ?? ''}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      title: e.target.value,
                    }),
                  );
                }}
              />
              <FormikText
                label="Subtitle"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.subtitle`}
                value={chart.sectionSettings.subtitle ?? ''}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      subtitle: e.target.value,
                    }),
                  );
                }}
              />
            </Col>
            <Col gap="1rem">
              <FormikText
                label="Legend Title"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendTitle`}
                value={chart.sectionSettings.legendTitle ?? ''}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      legendTitle: e.target.value,
                    }),
                  );
                }}
              />
              <FormikText
                label="X Title"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xLegend`}
                value={chart.sectionSettings.xLegend ?? ''}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      xLegend: e.target.value,
                    }),
                  );
                }}
              />
              <FormikText
                label="Y Title"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yLegend`}
                value={chart.sectionSettings.yLegend ?? ''}
                onChange={(e) => {
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      yLegend: e.target.value,
                    }),
                  );
                }}
              />
            </Col>
          </Row>
        </Col>
        <Col gap="1rem">
          <Col className="frm-in">
            <label>Legend Layout</label>
            <FormikCheckbox
              label="Show Legend"
              tooltip="Display the legend information"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showLegend`}
              checked={!!chart.sectionSettings.showLegend}
              onChange={(e) => {
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                    showLegend: e.target.checked,
                  }),
                );
              }}
            />
            <FormikCheckbox
              label="Show axis values"
              tooltip="Display the X and Y axis values"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showAxis`}
              checked={!!chart.sectionSettings.showAxis}
              onChange={(e) => {
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                    showAxis: e.target.checked,
                  }),
                );
              }}
            />
          </Col>
          <Col gap="1rem">
            <FormikSelect
              label="Position"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendPosition`}
              value={
                legendPositionOptions.find(
                  (o) => o.value === chart.sectionSettings.legendPosition,
                ) ?? ''
              }
              options={legendPositionOptions}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                    legendPosition: option ? (option.value as any) : undefined,
                  }),
                );
              }}
            />
            <FormikSelect
              label="Align"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendAlign`}
              value={
                legendAlignOptions.find((o) => o.value === chart.sectionSettings.legendAlign) ?? ''
              }
              options={legendAlignOptions}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                    legendAlign: option ? (option.value as any) : undefined,
                  }),
                );
              }}
            />
          </Col>
        </Col>
        <Col gap="1rem">
          <Colours
            label="Dataset Colours"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.datasetColors`}
            options={['green', 'red', 'gold']}
            values={chart.sectionSettings.datasetColors}
            onOpenPicker={() => {
              onDisableDrag?.(true);
            }}
            onClosePicker={() => {
              onDisableDrag?.(false);
            }}
            onChange={(newValue, values) => {
              setFieldValue(
                `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                  datasetColors: values,
                }),
              );
            }}
          />
          <Colours
            label="Data Label Colours"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelColors`}
            options={['green', 'red', 'gold']}
            values={chart.sectionSettings.dataLabelColors}
            onOpenPicker={() => {
              onDisableDrag?.(true);
            }}
            onClosePicker={() => {
              onDisableDrag?.(false);
            }}
            onChange={(newValue, values) => {
              setFieldValue(
                `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                  dataLabelColors: values,
                }),
              );
            }}
          />
        </Col>
      </Row>
    </Col>
  );
});
