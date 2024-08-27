import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  IOptionItem,
  legendAlignOptions,
  legendPositionOptions,
  mergeChartSettings,
  Row,
} from 'tno-core';

import { IChartSectionProps } from './IChartSectionProps';

/**
 * Provides a way to configure the dataset for the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConfigureLegend: React.FC<IChartSectionProps> = ({ sectionIndex, chartIndex }) => {
  const { values, setFieldValue } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];
  const showLegend =
    chart.sectionSettings.showLegend === undefined ? true : chart.sectionSettings.showLegend;

  return (
    <Col className="frm-in" gap="0.15rem">
      <Col className="frm-in">
        <label>Dataset Legend</label>
        <FormikCheckbox
          label="Show Legend"
          tooltip="Display the legend information"
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.showLegend`}
          checked={showLegend}
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
            disabled={!showLegend}
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
            disabled={!showLegend}
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
            label="Label Font size"
            disabled={!showLegend}
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
            disabled={!showLegend}
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
            legendPositionOptions.find((o) => o.value === chart.sectionSettings.legendPosition) ??
            ''
          }
          isDisabled={!showLegend}
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
          isDisabled={!showLegend}
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
  );
};
