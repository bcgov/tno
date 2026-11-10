import { Colours } from 'components/colours';
import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import { Col, FormikCheckbox, FormikText, mergeChartSettings, Row, Text } from 'tno-core';

import { IChartSectionProps } from './IChartSectionProps';

export interface IConfigureLabelsProps extends IChartSectionProps {
  onDisableDrag?: (disable: boolean) => void;
}

/**
 * Provides a way to configure the dataset for the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConfigureLabels: React.FC<IConfigureLabelsProps> = ({
  sectionIndex,
  chartIndex,
  onDisableDrag,
}) => {
  const { values, setFieldValue } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const [dataLabelOffsets, setDataLabelOffsets] = React.useState(
    chart.sectionSettings.dataLabelOffsets?.join(','),
  );

  return (
    <Col className="frm-in" gap="0.15rem">
      <label>Axis Labels</label>
      <FormikCheckbox
        label="Show X axis labels"
        name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xShowAxisLabels`}
        checked={chart.sectionSettings.xShowAxisLabels ?? true}
        onChange={(e) => {
          setFieldValue(
            `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
            mergeChartSettings(chart.settings.options, chart.sectionSettings, {
              xShowAxisLabels: e.target.checked,
            }),
          );
        }}
      />
      <FormikCheckbox
        label="Show Y axis labels"
        name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yShowAxisLabels`}
        checked={chart.sectionSettings.yShowAxisLabels ?? true}
        onChange={(e) => {
          setFieldValue(
            `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
            mergeChartSettings(chart.settings.options, chart.sectionSettings, {
              yShowAxisLabels: e.target.checked,
            }),
          );
        }}
      />
      <label>Data Value Labels</label>
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
      <Row justifyContent="space-between" alignItems="center">
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
        <FormikText
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelAnchors`}
          label="Anchor(s)"
          placeholder="center, start, end"
          disabled={!chart.sectionSettings.showDataLabels}
          value={chart.sectionSettings.dataLabelAnchors?.join(',') ?? ''}
          width="22ch"
          onChange={(e) => {
            const values = e.target.value.split(',').map((v) => v.trim() as any);
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                dataLabelAnchors: values,
              }),
            );
          }}
        />
      </Row>
      <Row justifyContent="space-between" alignItems="center">
        <FormikText
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelAligns`}
          label="Alignment(s)"
          placeholder="center, start, end, right, bottom, left, top"
          disabled={!chart.sectionSettings.showDataLabels}
          value={chart.sectionSettings.dataLabelAligns?.join(',') ?? ''}
          width="22ch"
          onChange={(e) => {
            const values = e.target.value.split(',').map((v) => v.trim() as any);
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                dataLabelAligns: values,
              }),
            );
          }}
        />
        <Text
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelOffsets`}
          label="Offset(s)"
          placeholder="0, 0, 0"
          disabled={!chart.sectionSettings.showDataLabels}
          value={dataLabelOffsets}
          width="22ch"
          onChange={(e) => {
            const values = e.target.value
              .split(',')
              .filter((v) => v.length > 0)
              .map((v) => parseInt(v.trim()) || 0);
            setDataLabelOffsets(e.target.value);
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                dataLabelOffsets: values,
              }),
            );
          }}
        />
      </Row>
      <Row justifyContent="space-between">
        <Colours
          label="Colours"
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
        <Colours
          label="Background Colours"
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataLabelBackgroundColors`}
          options={['white', 'black']}
          values={chart.sectionSettings.dataLabelBackgroundColors}
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
                dataLabelBackgroundColors: values,
              }),
            );
          }}
        />
      </Row>
      <label>Chart Labels</label>
      <Col gap="0.25rem">
        <Row gap="1rem">
          <Col flex="1">
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
          </Col>
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
          <Col flex="1">
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
          </Col>
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
          <Col flex="1">
            <FormikText
              label="X Axis Title"
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
          </Col>
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
          <Col flex="1">
            <FormikText
              label="Y Axis Title"
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
        <FormikCheckbox
          label="Skip X axis labels to prevent overlap"
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xAutoSkip`}
          checked={chart.sectionSettings.xAutoSkip ?? true}
          onChange={(e) => {
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                xAutoSkip: e.target.checked,
              }),
            );
          }}
        />
        <FormikCheckbox
          label="Skip Y axis labels to prevent overlap"
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yAutoSkip`}
          checked={chart.sectionSettings.yAutoSkip ?? true}
          onChange={(e) => {
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                yAutoSkip: e.target.checked,
              }),
            );
          }}
        />
        <Row gap="1rem">
          <Col>
            <label>X Axis Rotation</label>
            <Row gap="1rem">
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xMinRotation`}
                label="Min"
                value={chart.sectionSettings.xMinRotation ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      xMinRotation: value ? value : undefined,
                    }),
                  );
                }}
              />
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.xMaxRotation`}
                label="Max"
                value={chart.sectionSettings.xMaxRotation ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      xMaxRotation: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
          </Col>
          <Col>
            <label>Y Axis Rotation</label>
            <Row gap="1rem">
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yMinRotation`}
                label="Min"
                value={chart.sectionSettings.yMinRotation ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      yMinRotation: value ? value : undefined,
                    }),
                  );
                }}
              />
              <FormikText
                name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.yMaxRotation`}
                label="Max"
                value={chart.sectionSettings.yMaxRotation ?? ''}
                width="10ch"
                type="number"
                onChange={(e) => {
                  const value = parseInt(e.currentTarget.value);
                  setFieldValue(
                    `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                    mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                      yMaxRotation: value ? value : undefined,
                    }),
                  );
                }}
              />
            </Row>
          </Col>
        </Row>
      </Col>
    </Col>
  );
};
