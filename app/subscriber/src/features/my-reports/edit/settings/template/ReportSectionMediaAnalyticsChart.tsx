import { Action } from 'components/action';
import { Colours } from 'components/colours';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { FaInfoCircle } from 'react-icons/fa';
import { FaTrash } from 'react-icons/fa6';
import { Tooltip } from 'react-tooltip';
import {
  chartTypeOptions,
  Col,
  datasetOptions,
  datasetValueOptions,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  groupByOptions,
  IOptionItem,
  legendAlignOptions,
  legendPositionOptions,
  mergeChartSettings,
  OptionItem,
  Row,
  Show,
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
      <Row className="chart-settings" justifyContent="space-between">
        <Col gap="1rem">
          <Col>
            <FormikSelect
              label="Chart Type"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.chartType`}
              value={
                chartTypeOptions.find((o) => o.value === chart.sectionSettings.chartType) ?? ''
              }
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
          <Col className="frm-in">
            <label>Scale</label>
            <Row gap="1rem">
              <FormikText
                label="Min value"
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
                label="Max value"
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
            <Row gap="1rem">
              <FormikText
                label="Step size"
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
                label="Auto max"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleCalcMax`}
                value={chart.sectionSettings.scaleCalcMax ?? ''}
                type="number"
                width="10ch"
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      scaleCalcMax: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
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
        <Col className="frm-in">
          <label>Chart Data Settings</label>
          <Show visible={!!chart.settings.dataset.length}>
            <FormikSelect
              label="Dataset"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataset`}
              value={datasetOptions.find((o) => o.value === chart.sectionSettings.dataset) ?? ''}
              options={allowedDatasetOptions}
              isClearable={false}
              required
            />
          </Show>
          <Show visible={!!chart.settings.datasetValue.length}>
            <FormikSelect
              label="Dataset Value"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.datasetValue`}
              value={
                datasetValueOptions.find((o) => o.value === chart.sectionSettings.datasetValue) ??
                ''
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
          </Show>
          <Show visible={!!chart.settings.groupBy.length}>
            <FormikSelect
              label="Group By"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.groupBy`}
              value={groupByOptions.find((o) => o.value === chart.sectionSettings.groupBy) ?? ''}
              options={groupByOptions.filter((o) => chart.settings.groupBy.includes(o.value))}
              isClearable={false}
              required
            />
          </Show>
        </Col>
        <Col>
          <Row className="frm-in">
            <label>Dataset Colours</label>
            <FaInfoCircle data-tooltip-id="dataset-colours" className="info" />
          </Row>
          <Colours
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
          <Tooltip variant="info" id="dataset-colours" float>
            The order of colours will match the order of the dataset values. If you only have one
            dataset, you only need one colour.
          </Tooltip>
        </Col>
        <Col className="frm-in" gap="1rem">
          <Col gap="1rem">
            <Col className="frm-in">
              <label>Axis</label>
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
            <Col className="frm-in">
              <label>Data Labels</label>
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
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelFontSize`}
                label="Font Size"
                disabled={!chart.sectionSettings.showDataLabels}
                value={chart.sectionSettings.dataLabelFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      dataLabelFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
              <Colours
                label="Data Label Colours"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelColors`}
                options={['white', 'black']}
                values={chart.sectionSettings.dataLabelColors}
                disabled={!chart.sectionSettings.showDataLabels}
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
          </Col>
        </Col>
        <Col gap="1rem">
          <Col className="frm-in">
            <label>Legend</label>
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
          </Col>
          <Col>
            <Row gap="1rem">
              <FormikText
                label="Legend Title"
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendTitle`}
                value={chart.sectionSettings.legendTitle ?? ''}
                disabled={!chart.sectionSettings.showLegend}
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
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendTitleFontSize`}
                label="Font size"
                disabled={!chart.sectionSettings.showLegend}
                value={chart.sectionSettings.legendTitleFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      legendTitleFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
            <Row justifyContent="space-between">
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendLabelFontSize`}
                label="Dataset Label font size"
                disabled={!chart.sectionSettings.showLegend}
                value={chart.sectionSettings.legendLabelFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      legendLabelFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendLabelBoxWidth`}
                label="Label box width"
                disabled={!chart.sectionSettings.showLegend}
                value={chart.sectionSettings.legendLabelBoxWidth ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      legendLabelBoxWidth: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
            <FormikSelect
              label="Position"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.legendPosition`}
              value={
                legendPositionOptions.find(
                  (o) => o.value === chart.sectionSettings.legendPosition,
                ) ?? ''
              }
              isDisabled={!chart.sectionSettings.showLegend}
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
              isDisabled={!chart.sectionSettings.showLegend}
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
        <Col className="frm-in">
          <label>Labels</label>
          <Col>
            <Row gap="1rem">
              <FormikText
                label="Chart Title"
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
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.titleFontSize`}
                label="Font size"
                value={chart.sectionSettings.titleFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      titleFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
            <Row gap="1rem">
              <FormikText
                label="Chart Subtitle"
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
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.subtitleFontSize`}
                label="Font size"
                value={chart.sectionSettings.subtitleFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      subtitleFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
            <Row gap="1rem">
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
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xLegendFontSize`}
                label="Font size"
                value={chart.sectionSettings.xLegendFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      xLegendFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
            <Row gap="1rem">
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
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yLegendFontSize`}
                label="Font size"
                value={chart.sectionSettings.yLegendFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      yLegendFontSize: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
          </Col>
        </Col>
      </Row>
    </Col>
  );
});
