import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import { Col, FormikText, mergeChartSettings, Row } from 'tno-core';

import { IChartSectionProps } from './IChartSectionProps';

/**
 * Provides a way to configure the dataset for the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConfigureScale: React.FC<IChartSectionProps> = ({ sectionIndex, chartIndex }) => {
  const { values, setFieldValue } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];

  return (
    <Col className="frm-in" gap="0.15rem">
      <p>
        Enter the suggested minimum and maximum values on the axis. Or apply a value to add to the
        maximum. This is helpful when displaying sentiment range.
      </p>
      <Row gap="1rem" justifyContent="space-between">
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
        <Row gap="1rem">
          <FormikText
            label="Max value"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleSuggestedMax`}
            value={chart.sectionSettings.scaleSuggestedMax ?? ''}
            type="number"
            width="10ch"
            disabled={!!chart.sectionSettings.scaleCalcMax}
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
          <Col justifyContent="center">or</Col>
          <FormikText
            label="Add to max"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.scaleCalcMax`}
            value={chart.sectionSettings.scaleCalcMax ?? ''}
            disabled={!!chart.sectionSettings.scaleSuggestedMax}
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
      </Row>
      <Row gap="1rem" nowrap>
        <p>Enter the number of units between grid lines.</p>
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
      </Row>
      <Row gap="1rem" nowrap>
        <p>Choose the minimum width in pixels when displaying values (useful to display zero).</p>
        <FormikText
          name="minBarLength"
          label="Min size"
          width="10ch"
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
      </Row>
    </Col>
  );
};
