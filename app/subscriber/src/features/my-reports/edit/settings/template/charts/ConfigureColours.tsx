import { Action } from 'components/action';
import { Colours } from 'components/colours';
import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import { FaInfoCircle, FaPaintBrush } from 'react-icons/fa';
import { FaEraser } from 'react-icons/fa6';
import { Tooltip } from 'react-tooltip';
import { Col, FormikCheckbox, mergeChartSettings, Row } from 'tno-core';

import { IChartSectionProps } from './IChartSectionProps';

const defaultColors = [
  '#36A2EB',
  '#FF6384',
  '#4BC0C0',
  '#FF9F40',
  '#9966FF',
  '#FFCD56',
  '#C9CBCF',
  '#AA0DFE',
  '#3283FE',
  '#85660D',
  '#782AB6',
  '#565656',
  '#1C8356',
  '#16FF32',
  '#F7E1A0',
  '#1CBE4F',
  '#C4451C',
  '#DEA0FD',
  '#FE00FA',
  '#325A9B',
  '#FEAF16',
  '#F8A19F',
  '#90AD1C',
  '#F6222E',
  '#1CFFCE',
  '#2ED9FF',
  '#FBE426',
];

export interface IConfigureColoursProps extends IChartSectionProps {
  onDisableDrag?: (disable: boolean) => void;
}

/**
 * Provides a way to configure the dataset for the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConfigureColours: React.FC<IConfigureColoursProps> = ({
  sectionIndex,
  chartIndex,
  onDisableDrag,
}) => {
  const { values, setFieldValue } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];

  return (
    <Col className="frm-in" gap="0.25rem">
      <label>Predefined Colours</label>
      <Row className="chart-config-action-bar" gap="1rem" justifyContent="space-between">
        <Action
          icon={<FaPaintBrush />}
          onClick={() => {
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                datasetColors: defaultColors,
              }),
            );
          }}
        >
          Default
        </Action>
        <Action
          icon={<FaPaintBrush />}
          onClick={() => {
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                datasetColors: ['green', 'gold', 'red'],
              }),
            );
          }}
        >
          Sentiment
        </Action>
        <Action
          icon={<FaEraser />}
          onClick={() => {
            setFieldValue(
              `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
              mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                datasetColors: undefined,
                datasetBorderColors: undefined,
              }),
            );
          }}
        >
          Reset
        </Action>
      </Row>
      <p>
        If you want each colour to be applied in the specified order to the values in each dataset.
      </p>
      <FormikCheckbox
        label="Apply Colour to Index Axis Grouping Property"
        name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.applyColorToValue`}
        checked={chart.sectionSettings.applyColorToValue ?? true}
        onChange={(e) => {
          setFieldValue(
            `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
            mergeChartSettings(chart.settings.options, chart.sectionSettings, {
              applyColorToValue: e.target.checked,
            }),
          );
        }}
      />
      <Row gap="1rem">
        <Col gap="1rem" flex="1">
          <Row className="frm-in">
            <label>{`${
              chart.sectionSettings.applyColorToValue ? 'Group' : 'Dataset'
            } Colours`}</label>
            <FaInfoCircle data-tooltip-id="dataset-colours" className="info" />
          </Row>
          <Col className="chart-config-window">
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
          </Col>
        </Col>
        <Col gap="1rem" flex="1">
          <Row className="frm-in">
            <label>Border Colours</label>
            <FaInfoCircle data-tooltip-id="dataset-colours" className="info" />
          </Row>
          <Col className="chart-config-window">
            <Colours
              name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.datasetBorderColors`}
              options={['white', 'black']}
              values={chart.sectionSettings.datasetBorderColors}
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
                    datasetBorderColors: values,
                  }),
                );
              }}
            />
          </Col>
        </Col>
      </Row>
      <Tooltip variant="info" id="dataset-colours" float>
        The order of colours will match the order of the dataset values. If you only have one
        dataset, you only need one colour.
      </Tooltip>
    </Col>
  );
};
