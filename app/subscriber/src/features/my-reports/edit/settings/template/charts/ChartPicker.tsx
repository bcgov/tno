import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import {
  chartTypeOptions,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  mergeChartSettings,
  Row,
} from 'tno-core';

import { IChartSectionProps } from './IChartSectionProps';

/**
 * Provides a way to pick the chart type and configure the canvas size.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ChartPicker: React.FC<IChartSectionProps> = ({ sectionIndex, chartIndex }) => {
  const { values, setFieldValue } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];

  const [allowedChartTypeOptions] = React.useState(
    chartTypeOptions.filter((o) => chart.settings.chartTypes.includes(o.value)),
  );

  return (
    <Row gap="1rem">
      <Col flex="1" gap="0.5rem">
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
            value={chart.sectionSettings.width ?? ''}
            onChange={(e) => {
              const value = e.target.value.trim();
              setFieldValue(
                `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.width`,
                value ? +value : undefined,
              );
            }}
            type="number"
            width="10ch"
          />
          <Row gap="0.5rem">
            <FormikText
              label="Aspect Ratio"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.aspectRatio`}
              value={chart.sectionSettings.aspectRatio ?? ''}
              onChange={(e) => {
                const value = e.target.value.trim();
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.aspectRatio`,
                  value ? +value : undefined,
                );
              }}
              type="number"
              width="10ch"
              disabled={!!chart.sectionSettings.height}
            />
            <Col justifyContent="center">or</Col>
            <FormikText
              label="Height"
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.height`}
              value={chart.sectionSettings.height ?? ''}
              onChange={(e) => {
                const value = e.target.value.trim();
                setFieldValue(
                  `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                  {
                    ...chart.sectionSettings,
                    height: value ? +value : undefined,
                    aspectRatio: value ? undefined : 1,
                  },
                );
              }}
              type="number"
              width="10ch"
              disabled={!!chart.sectionSettings.aspectRatio}
            />
          </Row>
        </Row>
        <FormikCheckbox
          label="Maintain Canvas Aspect Ratio when Resizing"
          name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.maintainAspectRatio`}
          checked={chart.sectionSettings.maintainAspectRatio ?? false}
        />

        <Col className="frm-in">
          <label>Axis</label>
          <p>Choose how your chart axis is displayed.</p>
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
    </Row>
  );
};
