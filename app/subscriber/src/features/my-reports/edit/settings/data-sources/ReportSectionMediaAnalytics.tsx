import { DataSources } from 'features/my-reports/components';
import { IReportForm } from 'features/my-reports/interfaces';
import { useFormikContext } from 'formik';
import React from 'react';
import { Col, FormikCheckbox, Show } from 'tno-core';

export interface IReportSectionMediaAnalyticsProps {
  index: number;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionMediaAnalytics = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsProps
>(({ index, ...rest }, ref) => {
  const { values, setFieldValue } = useFormikContext<IReportForm>();

  const section = values.sections[index];

  return (
    <Col gap="0.5rem">
      <DataSources
        index={index}
        className="section-options"
        none={{ value: 'none', label: 'Content In This Report' }}
        onChange={(value) => {
          if (value === 'none') {
            setFieldValue(`sections.${index}.settings.useAllContent`, true);
          } else {
            setFieldValue(`sections.${index}.settings.useAllContent`, false);
          }
        }}
      />
      <Col className="frm-in">
        <label>Report Section Options</label>
        <Show visible={!section.settings.useAllContent}>
          <FormikCheckbox
            name={`sections.${index}.settings.removeDuplicates`}
            label="Remove duplicate stories"
            tooltip="Remove content from this section that is in above sections"
          />
        </Show>
        <FormikCheckbox
          name={`sections.${index}.settings.hideEmpty`}
          label="Hide this section in the report when empty"
        />
        <FormikCheckbox
          name={`sections.${index}.settings.direction`}
          label="Horizontally align the next media analytic chart"
          tooltip="This controls the placement of the next media analytics chart"
          checked={section.settings.direction === 'row'}
          onChange={(e) => {
            setFieldValue(
              `sections.${index}.settings.direction`,
              e.target.checked ? 'row' : 'column',
            );
          }}
        />
      </Col>
    </Col>
  );
});
