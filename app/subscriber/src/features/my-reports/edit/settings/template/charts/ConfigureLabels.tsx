import { Colours } from 'components/colours';
import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import { Col, FormikCheckbox, FormikText, mergeChartSettings, Row } from 'tno-core';

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
      <Row justifyContent="space-between" alignItems="center">
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
      </Row>
      <Colours
        label="Data Value Label Colours"
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
      <label>Chart Labels</label>
      <Col>
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
      </Col>
    </Col>
  );
};
