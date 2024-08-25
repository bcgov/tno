import { useReportEditContext } from 'features/my-reports/edit/ReportEditContext';
import React from 'react';
import {
  Col,
  datasetOptions,
  datasetValueOptions,
  FormikCheckbox,
  FormikSelect,
  groupByOptions,
  mergeChartSettings,
  Row,
  Show,
  ToggleGroup,
} from 'tno-core';

import { IChartSectionProps } from './IChartSectionProps';

/**
 * Provides a way to configure the dataset for the chart.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ConfigureDataset: React.FC<IChartSectionProps> = ({ sectionIndex, chartIndex }) => {
  const { values, setFieldValue } = useReportEditContext();

  const section = values.sections[sectionIndex];
  const chart = section.chartTemplates[chartIndex];

  const [allowedDatasetOptions] = React.useState(
    datasetOptions.filter((o) => chart.settings.dataset.includes(o.value)),
  );
  const [allowedDatasetValueOptions] = React.useState(
    datasetValueOptions.filter((o) => chart.settings.datasetValue.includes(o.value)),
  );

  return (
    <Row gap="1rem">
      <Col flex="1" gap="0.5rem" className="frm-in">
        <Show visible={!!chart.settings.datasetValue.length}>
          <label>Dataset Type</label>
          <ToggleGroup
            options={allowedDatasetValueOptions.map((o) => ({
              id: o.value,
              label: o.label,
            }))}
            defaultSelected={
              datasetValueOptions.find((o) => o.value === chart.sectionSettings.datasetValue)
                ?.value ?? undefined
            }
            onChange={(newValue: any) => {
              var isSentiment = newValue.id === 'sentiment';
              setFieldValue(
                `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                  datasetValue: newValue.id ?? 'count',
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
          <FormikCheckbox
            label="Exclude Empty Values"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.excludeEmptyValues`}
            checked={!!chart.sectionSettings.excludeEmptyValues}
            onChange={(e) => {
              setFieldValue(
                `sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings`,
                mergeChartSettings(chart.settings.options, chart.sectionSettings, {
                  excludeEmptyValues: e.target.checked,
                }),
              );
            }}
          />
        </Show>
        <Show visible={!!chart.settings.dataset.length}>
          <p>
            A dataset represents an array of values to display on the index axis (default X axis).
            Each dataset provides a way to compare collections of content. Select a property to
            separate the data.
          </p>
          <FormikSelect
            label="Dataset Grouping Property"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.dataset`}
            value={datasetOptions.find((o) => o.value === chart.sectionSettings.dataset) ?? ''}
            options={allowedDatasetOptions}
            isClearable={false}
            required
          />
        </Show>
        <Show visible={!!chart.settings.groupBy.length}>
          <p>
            Select the property to group by and will be plotted on the index axis (default X axes)
            within each dataset.
          </p>
          <FormikSelect
            label="Index Axis Grouping Property"
            name={`sections.${sectionIndex}.chartTemplates.${chartIndex}.sectionSettings.groupBy`}
            value={groupByOptions.find((o) => o.value === chart.sectionSettings.groupBy) ?? ''}
            options={groupByOptions.filter((o) => chart.settings.groupBy.includes(o.value))}
            isClearable={false}
            required
          />
        </Show>
      </Col>
    </Row>
  );
};
