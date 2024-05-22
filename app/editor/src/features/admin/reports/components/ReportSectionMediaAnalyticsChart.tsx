import { useFormikContext } from 'formik';
import React from 'react';
import { FaTrash } from 'react-icons/fa6';
import {
  Button,
  ButtonVariant,
  chartTypeOptions,
  Col,
  datasetOptions,
  datasetValueOptions,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  groupByOptions,
  IOptionItem,
  IReportModel,
  legendAlignOptions,
  legendPositionOptions,
  mergeChartSettings,
  OptionItem,
  Row,
  Show,
  Text,
} from 'tno-core';

export interface IReportSectionMediaAnalyticsChartProps {
  sectionIndex: number;
  chartIndex: number;
}

export const ReportSectionMediaAnalyticsChart = ({
  sectionIndex,
  chartIndex,
}: IReportSectionMediaAnalyticsChartProps) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];

  const [datasetColors, setDatasetColors] = React.useState(
    chart.sectionSettings.datasetColors?.join(',') ?? '',
  );
  const [dataLabelColors, setDataLabelColors] = React.useState(
    chart.sectionSettings.dataLabelColors?.join(',') ?? '',
  );
  const [datasetAvailableOptions] = React.useState(
    datasetOptions.filter((o) => chart.settings.dataset.includes(o.value)),
  );
  const [datasetValueAvailableOptions] = React.useState(
    datasetValueOptions.filter((o) => chart.settings.datasetValue.includes(o.value)),
  );
  const [groupByAvailableOptions] = React.useState(
    groupByOptions.filter((o) => chart.settings.groupBy.includes(o.value)),
  );

  return (
    <Row key={chart.id}>
      <Col flex="1">
        <Row>
          <Col flex="1">
            <b>{chart.name}</b>
          </Col>
          <Col flex="2">{chart.description}</Col>
          <Col>
            <Button
              variant={ButtonVariant.danger}
              onClick={() => {
                let items = [...section.chartTemplates];
                items.splice(chartIndex, 1);
                setFieldValue(`sections.${sectionIndex}.chartTemplates`, items);
              }}
            >
              <FaTrash />
            </Button>
          </Col>
        </Row>
        <Row className="chart-settings">
          <Col>
            <FormikSelect
              label="Chart Type"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.chartType`}
              value={
                chartTypeOptions.find((o) => o.value === chart.sectionSettings.chartType) ?? ''
              }
              options={chartTypeOptions.filter((o) => chart.settings.chartTypes.includes(o.value))}
              isClearable={false}
            />
            <Row justifyContent="space-around">
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
            <Col>
              <Text
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.datasetColors`}
                label="Dataset Colours"
                placeholder="green,gold,red"
                value={datasetColors}
                onChange={(e) => {
                  const colors = e.target.value
                    .split(',')
                    .map((v) => v.trim())
                    .filter((v) => v);
                  setDatasetColors(e.target.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      datasetColors: colors,
                    }),
                  );
                }}
              />
              <Text
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelColors`}
                label="Data Label Colours"
                placeholder="white,black"
                value={dataLabelColors}
                onChange={(e) => {
                  const colors = e.target.value
                    .split(',')
                    .map((v) => v.trim())
                    .filter((v) => v);
                  setDataLabelColors(e.target.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      dataLabelColors: colors,
                    }),
                  );
                }}
              />
            </Col>
          </Col>
          <Col>
            <Show visible={datasetAvailableOptions.length > 0}>
              <FormikSelect
                label="Dataset"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataset`}
                tooltip="A dataset separates the data into groups based on the selected property."
                value={datasetOptions.find((o) => o.value === chart.sectionSettings.dataset) ?? ''}
                options={datasetAvailableOptions}
                isClearable={false}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      dataset: option.value?.toString() ?? '',
                    }),
                  );
                }}
              />
            </Show>
            <Show visible={datasetValueAvailableOptions.length > 0}>
              <FormikSelect
                label="Dataset Value"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.datasetValue`}
                tooltip="A dataset value is how the totals are calculated for each grouping."
                value={
                  datasetValueOptions.find((o) => o.value === chart.sectionSettings.datasetValue) ??
                  ''
                }
                options={datasetValueAvailableOptions}
                isClearable={false}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  const isSentiment = option.value === 'sentiment';
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      datasetValue: option.value?.toString() ?? 'count',
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
                  setDatasetColors(isSentiment ? 'green,gold,red' : '');
                }}
              />
            </Show>
            <Show visible={groupByAvailableOptions.length > 0}>
              <FormikSelect
                label="Group By"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.groupBy`}
                tooltip="Group the dataset by this property (X axis)."
                value={groupByOptions.find((o) => o.value === chart.sectionSettings.groupBy) ?? ''}
                options={groupByAvailableOptions}
                isClearable={false}
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      groupBy: option.value?.toString() ?? '',
                    }),
                  );
                }}
              />
            </Show>
          </Col>
          <Col gap="1rem">
            <Col>
              <label>Layout</label>
              <FormikCheckbox
                label="Stack Datasets"
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
                label="Show Data Labels"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showDataLabels`}
                checked={chart.sectionSettings.showDataLabels ?? false}
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
            <Col>
              <label>Scale</label>
              <Row>
                <FormikText
                  name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleSuggestedMin`}
                  label="Min Value"
                  width="8ch"
                  type="number"
                  value={chart.sectionSettings.scaleSuggestedMin?.toString() ?? ''}
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
                  name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleSuggestedMax`}
                  label="Max Value"
                  width="8ch"
                  type="number"
                  value={chart.sectionSettings.scaleSuggestedMax?.toString() ?? ''}
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
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleTicksStepSize`}
                label="Step Size"
                width="8ch"
                type="number"
                value={chart.sectionSettings.scaleTicksStepSize?.toString() ?? ''}
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
              <Text
                name="minBarLength"
                label="Min bar length"
                width="8ch"
                type="number"
                value={chart.sectionSettings.minBarLength?.toString() ?? ''}
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
          <Col>
            <label>Legend</label>
            <FormikText
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.title`}
              label="Title"
              value={chart.sectionSettings.title ?? ''}
            />
            <FormikText
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.subtitle`}
              label="Subtitle"
              value={chart.sectionSettings.subtitle ?? ''}
            />
            <FormikText
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendTitle`}
              label="Legend Title"
              value={chart.sectionSettings.legendTitle ?? ''}
            />
            <FormikText
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xLegend`}
              label="X Title"
              value={chart.sectionSettings.xLegend ?? ''}
            />
            <FormikText
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yLegend`}
              label="Y Title"
              value={chart.sectionSettings.yLegend ?? ''}
            />
          </Col>
          <Col>
            <label>Legend Layout</label>
            <FormikCheckbox
              label="Show Legend"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showLegend`}
              checked={chart.sectionSettings.showLegend ?? true}
            />
            <FormikCheckbox
              label="Show axis"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showAxis`}
              checked={!!chart.sectionSettings.showAxis}
              onChange={(e) => {
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  {
                    ...chart.sectionSettings,
                    showAxis: e.target.checked,
                    options: mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      showAxis: e.target.checked,
                    }).options,
                  },
                );
              }}
            />
            <FormikSelect
              label="Position"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendPosition`}
              options={legendPositionOptions}
              value={
                legendPositionOptions.find(
                  (o) => o.value === chart.sectionSettings.legendPosition,
                ) ?? ''
              }
              isClearable
              clearValue={''}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  {
                    ...mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      legendPosition: option ? (option.value as any) : undefined,
                    }),
                  },
                );
              }}
            />
            <FormikSelect
              label="Align"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendAlign`}
              options={legendAlignOptions}
              value={
                legendAlignOptions.find((o) => o.value === chart.sectionSettings.legendAlign) ?? ''
              }
              isClearable
              clearValue={''}
              onChange={(newValue) => {
                const option = newValue as IOptionItem;
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  {
                    ...mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      legendAlign: option ? (option.value as any) : undefined,
                    }),
                  },
                );
              }}
            />
          </Col>
        </Row>
      </Col>
    </Row>
  );
};
