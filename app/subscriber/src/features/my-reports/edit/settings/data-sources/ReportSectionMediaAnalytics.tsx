import { DataSources } from 'features/my-reports/components';
import React from 'react';
import { Col, FormikCheckbox, Row, Show } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

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
  const { values, setFieldValue } = useReportEditContext();

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
          <Row>
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicates`}
              label="Remove duplicate stories"
            />
            <span className="info">
              Do not include in this section content that already exists in the above sections (does
              not apply to charts that link to other reports)
            </span>
          </Row>
        </Show>
        <Show visible={!!section.folderId || !!section.linkedReportId}>
          <Row>
            <FormikCheckbox
              name={`sections.${index}.settings.overrideExcludeHistorical`}
              label={`Include all content from linked ${
                section.folderId ? 'folder' : 'report'
              } even if in prior report`}
            />
            <span className="info">
              This overrides the report option "Exclude stories that have been sent out in previous
              report" for this section only.
            </span>
          </Row>
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
