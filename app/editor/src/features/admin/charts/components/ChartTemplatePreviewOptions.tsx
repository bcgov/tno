import React from 'react';
import { FaAngleDown, FaMinus } from 'react-icons/fa6';
import {
  chartTypeOptions,
  Checkbox,
  Col,
  datasetOptions,
  datasetValueOptions,
  groupByOptions,
  legendAlignOptions,
  legendPositionOptions,
  mergeChartSettings,
  OptionItem,
  Row,
  Select,
  Text,
  ToggleButton,
} from 'tno-core';

import { useChartTemplateContext } from '../ChartTemplateContext';
import * as styled from './styled';

export const ChartTemplatePreviewOptions = () => {
  const { values, chartRequestForm, setChartRequestForm } = useChartTemplateContext();

  const [chartTypes] = React.useState(
    chartTypeOptions.filter((o) => values.settings?.chartTypes?.some((v) => v === o.value)) ?? [],
  );
  const [datasets] = React.useState(
    datasetOptions.filter((o) => values.settings?.dataset?.some((v) => v === o.value)) ?? [],
  );
  const [datasetValues] = React.useState(
    datasetValueOptions.filter((o) => values.settings?.datasetValue?.some((v) => v === o.value)) ??
      [],
  );
  const [groupBys] = React.useState(
    groupByOptions.filter((o) => values.settings?.groupBy?.some((v) => v === o.value)) ?? [],
  );
  const [datasetColors, setDatasetColors] = React.useState('');
  const [datasetBorderColors, setDatasetBorderColors] = React.useState('');
  const [dataLabelColors, setDataLabelColors] = React.useState('');
  const [show, setShow] = React.useState(false);

  return (
    <styled.ChartTemplatePreviewOptions>
      <Row justifyContent="space-between">
        <h3>Chart Options</h3>
        <span>Temporarily override default configuration to preview.</span>
        <ToggleButton
          on={<FaMinus />}
          off={<FaAngleDown />}
          onClick={() => setShow(!show)}
          value={show}
        />
      </Row>
      {show && (
        <Row justifyContent="space-between" gap="1rem">
          <Col>
            <Select
              label="Chart Type"
              name="chartType"
              options={chartTypes}
              value={
                chartTypeOptions.find((o) => o.value === chartRequestForm.settings.chartType) ?? ''
              }
              isClearable={false}
              required
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    chartType: option.value?.toString() ?? 'bar',
                  }),
                });
              }}
            />
            <Row justifyContent="space-around">
              <Text
                name="width"
                label="Width"
                value={chartRequestForm.settings.width ?? ''}
                type="number"
                width="10ch"
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        width: parseInt(e.target.value),
                      },
                    ),
                  })
                }
              />
              <Text
                name="height"
                label="Height"
                value={chartRequestForm.settings.height ?? ''}
                type="number"
                width="10ch"
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        height: parseInt(e.target.value),
                      },
                    ),
                  })
                }
              />
            </Row>
            <Col>
              <Checkbox
                name="applyColorToValue"
                label="Apply Colour to Group"
                checked={!!chartRequestForm.settings.applyColorToValue}
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        applyColorToValue: e.target.checked,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="datasetColors"
                label={`${
                  chartRequestForm.settings.applyColorToValue ? 'Group' : 'Dataset'
                } Colours`}
                placeholder="green,gold,red"
                value={datasetColors}
                onChange={(e) => {
                  const colors = e.target.value
                    .split(',')
                    .map((v) => v.trim())
                    .filter((v) => v);
                  setDatasetColors(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        datasetColors: colors,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="datasetBorderColors"
                label="Border Colours"
                placeholder="white"
                value={datasetBorderColors}
                onChange={(e) => {
                  const colors = e.target.value
                    .split(',')
                    .map((v) => v.trim())
                    .filter((v) => v);
                  setDatasetBorderColors(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        datasetBorderColors: colors,
                      },
                    ),
                  });
                }}
              />
            </Col>
          </Col>
          <Col>
            <Select
              label="Dataset"
              name="dataset"
              tooltip="A dataset separates the data into groups based on the selected property."
              options={datasets}
              value={
                datasetOptions.find((o) => o.value === chartRequestForm.settings.dataset) ?? ''
              }
              width="200px"
              isClearable={false}
              required
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    dataset: option.value?.toString() ?? '',
                  }),
                });
              }}
            />
            <Select
              label="Dataset Value"
              name="datasetValue"
              tooltip="A dataset value is how the totals are calculated for each grouping."
              options={datasetValues}
              value={
                datasetValueOptions.find(
                  (o) => o.value === chartRequestForm.settings.datasetValue,
                ) ?? ''
              }
              width="200px"
              isClearable={false}
              required
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                const isSentiment = option.value === 'sentiment';
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    datasetValue: option.value?.toString() ?? 'count',
                    scaleSuggestedMin: isSentiment ? -5 : undefined,
                    scaleSuggestedMax: isSentiment ? 5 : undefined,
                    scaleTicksStepSize: isSentiment ? 1 : undefined,
                    datasetColors:
                      isSentiment && chartRequestForm.settings.dataset === ''
                        ? ['green', 'gold', 'red']
                        : [],
                    minBarLength: isSentiment ? 10 : undefined,
                  }),
                });
                setDatasetColors(isSentiment ? 'green,gold,red' : '');
              }}
            />
            <Select
              label="Group By"
              name="groupBy"
              tooltip="Group the dataset by this property (X axis)."
              options={groupBys}
              value={
                groupByOptions.find((o) => o.value === chartRequestForm.settings.groupBy) ?? ''
              }
              isClearable
              clearValue={''}
              width="200px"
              required
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: {
                    ...chartRequestForm.settings,
                    groupBy: option.value?.toString() ?? '',
                  },
                });
              }}
            />
            <Checkbox
              name="excludeEmptyValues"
              label="Exclude empty values"
              checked={!!chartRequestForm.settings.excludeEmptyValues}
              onChange={(e) => {
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    excludeEmptyValues: e.target.checked,
                  }),
                });
              }}
            />
          </Col>
          <Col gap="1rem">
            <Col>
              <label>Axis</label>
              <Checkbox
                name="stacked"
                label="Stack Datasets"
                checked={!!chartRequestForm.settings.stacked}
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        stacked: e.target.checked,
                      },
                    ),
                  });
                }}
              />
              <Checkbox
                name="isHorizontal"
                label="Flip X and Y axis"
                checked={!!chartRequestForm.settings.isHorizontal}
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        isHorizontal: e.target.checked,
                      },
                    ),
                  });
                }}
              />
              <Checkbox
                name="xShowAxisLabels"
                label="Show X axis labels"
                checked={!!chartRequestForm.settings.xShowAxisLabels}
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        xShowAxisLabels: e.target.checked,
                      },
                    ),
                  });
                }}
              />
              <Checkbox
                name="yShowAxisLabels"
                label="Show Y axis labels"
                checked={!!chartRequestForm.settings.yShowAxisLabels}
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        yShowAxisLabels: e.target.checked,
                      },
                    ),
                  });
                }}
              />
            </Col>
            <Col>
              <label>Data Labels</label>
              <Checkbox
                name="showDataLabels"
                label="Show Data Labels"
                checked={!!chartRequestForm.settings.showDataLabels}
                onChange={(e) => {
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        showDataLabels: e.target.checked,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="data-label-font-size"
                label="Font Size"
                disabled={!chartRequestForm.settings.showDataLabels}
                value={chartRequestForm.settings.dataLabelFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        dataLabelFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="dataLabelColors"
                label="Data Label Colours"
                placeholder="white,black"
                value={dataLabelColors}
                disabled={!chartRequestForm.settings.showDataLabels}
                onChange={(e) => {
                  const colors = e.target.value
                    .split(',')
                    .map((v) => v.trim())
                    .filter((v) => v);
                  setDataLabelColors(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        dataLabelColors: colors,
                      },
                    ),
                  });
                }}
              />
            </Col>
          </Col>
          <Col>
            <label>Scale</label>
            <Row>
              <Text
                name="scaleSuggestedMin"
                label="Min value"
                width="8ch"
                type="number"
                value={chartRequestForm.settings.scaleSuggestedMin?.toString() ?? ''}
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        scaleSuggestedMin: value,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="scaleSuggestedMax"
                label="Max value"
                width="8ch"
                type="number"
                value={chartRequestForm.settings.scaleSuggestedMax?.toString() ?? ''}
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        scaleSuggestedMax: value,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Row>
              <Text
                name="scaleCalcMax"
                label="Auto max"
                width="8ch"
                type="number"
                value={chartRequestForm.settings.scaleCalcMax?.toString() ?? ''}
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        scaleCalcMax: value,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="scaleTicksStepSize"
                label="Step size"
                width="8ch"
                type="number"
                value={chartRequestForm.settings.scaleTicksStepSize?.toString() ?? ''}
                onChange={(e) => {
                  const value = parseInt(e.target.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        scaleTicksStepSize: value,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Text
              name="minBarLength"
              label="Min bar length"
              width="8ch"
              type="number"
              value={chartRequestForm.settings.minBarLength?.toString() ?? ''}
              onChange={(e) => {
                const value = parseInt(e.target.value);
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    minBarLength: value,
                  }),
                });
              }}
            />
          </Col>
          <Col>
            <label>Legend</label>
            <Checkbox
              name="showLegend"
              label="Show Legend"
              checked={!!chartRequestForm.settings.showLegend}
              value={true}
              onChange={(e) => {
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    showLegend: e.target.checked,
                  }),
                });
              }}
            />
            <Row>
              <Text
                name="legendTitle"
                label="Legend Title"
                disabled={!chartRequestForm.settings.showLegend}
                value={chartRequestForm.settings.legendTitle ?? ''}
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        legendTitle: e.target.value,
                      },
                    ),
                  })
                }
              />
              <Text
                name="legendTitle-font-size"
                label="Font Size"
                disabled={!chartRequestForm.settings.showLegend}
                value={chartRequestForm.settings.legendTitleFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        legendTitleFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Row>
              <Text
                name="legend-label-font-size"
                label="Dataset Label Font Size"
                value={chartRequestForm.settings.legendLabelFontSize ?? ''}
                width="10ch"
                type="number"
                disabled={!chartRequestForm.settings.showLegend}
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        legendLabelFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
              <Text
                name="legend-label-box-width"
                label="Label Box Width"
                value={chartRequestForm.settings.legendLabelBoxWidth ?? ''}
                width="10ch"
                type="number"
                disabled={!chartRequestForm.settings.showLegend}
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        legendLabelBoxWidth: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Select
              label="Position"
              name="legendPosition"
              options={legendPositionOptions}
              value={
                legendPositionOptions.find(
                  (o) => o.value === chartRequestForm.settings.legendPosition,
                ) ?? ''
              }
              isClearable
              clearValue={''}
              isDisabled={!chartRequestForm.settings.showLegend}
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    legendPosition: (option.value?.toString() ?? '') as any,
                  }),
                });
              }}
            />
            <Select
              label="Align"
              name="legendAlign"
              options={legendAlignOptions}
              value={
                legendAlignOptions.find((o) => o.value === chartRequestForm.settings.legendAlign) ??
                ''
              }
              isClearable
              clearValue={''}
              isDisabled={!chartRequestForm.settings.showLegend}
              onChange={(newValue) => {
                const option = newValue as OptionItem;
                setChartRequestForm({
                  ...chartRequestForm,
                  settings: mergeChartSettings(values.settings.options, chartRequestForm.settings, {
                    legendAlign: (option.value?.toString() ?? '') as any,
                  }),
                });
              }}
            />
          </Col>
          <Col>
            <label>Labels</label>
            <Row>
              <Text
                name="title"
                label="Chart title"
                value={chartRequestForm.settings.title ?? ''}
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        title: e.target.value,
                      },
                    ),
                  })
                }
              />
              <Text
                name="title-font-size"
                label="Font Size"
                value={chartRequestForm.settings.titleFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        titleFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Row>
              <Text
                name="subtitle"
                label="Chart subtitle"
                value={chartRequestForm.settings.subtitle ?? ''}
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        subtitle: e.target.value,
                      },
                    ),
                  })
                }
              />
              <Text
                name="subtitle-font-size"
                label="Font Size"
                value={chartRequestForm.settings.subtitleFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        subtitleFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Row>
              <Text
                name="xLegend"
                label="X Title"
                value={chartRequestForm.settings.xLegend ?? ''}
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        xLegend: e.target.value,
                      },
                    ),
                  })
                }
              />
              <Text
                name="xLegend-font-size"
                label="Font Size"
                value={chartRequestForm.settings.xLegendFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        xLegendFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
            </Row>
            <Row>
              <Text
                name="yLegend"
                label="Y Title"
                value={chartRequestForm.settings.yLegend ?? ''}
                onChange={(e) =>
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        yLegend: e.target.value,
                      },
                    ),
                  })
                }
              />
              <Text
                name="yLegend-font-size"
                label="Font Size"
                value={chartRequestForm.settings.yLegendFontSize ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setChartRequestForm({
                    ...chartRequestForm,
                    settings: mergeChartSettings(
                      values.settings.options,
                      chartRequestForm.settings,
                      {
                        yLegendFontSize: value ? value : undefined,
                      },
                    ),
                  });
                }}
              />
            </Row>
          </Col>
        </Row>
      )}
    </styled.ChartTemplatePreviewOptions>
  );
};
