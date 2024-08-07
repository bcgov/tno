import { IToggleOption, Toggle } from 'components/form';
import { DataSources } from 'features/my-reports/components';
import React from 'react';
import {
  Col,
  FormikCheckbox,
  FormikSelect,
  IOptionItem,
  OptionItem,
  ReportSectionOrderByOptions,
  Row,
  Show,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionContentProps {
  index: number;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionContent = React.forwardRef<HTMLDivElement, IReportSectionContentProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();

    const [orderOptions] = React.useState<IOptionItem[]>(ReportSectionOrderByOptions);
    const directionOptions: IToggleOption<string>[] = [
      { label: 'Ascending', value: 'asc' },
      { label: 'Descending', value: 'desc' },
    ];

    const section = values.sections[index];

    return (
      <Col gap="0.5rem">
        <DataSources index={index} className="section-options" />
        <Row className="frm-in section-options">
          <label>Sort Stories By:</label>
          <Col flex="1">
            <FormikSelect
              name={`sections.${index}.settings.sortBy`}
              options={orderOptions}
              value={orderOptions.find((o) => o.value === section.settings.sortBy) ?? ''}
              onChange={(newValue: any) => {
                const option = newValue as OptionItem;
                const order = orderOptions.find((f) => f.value === option?.value);
                if (order) setFieldValue(`sections.${index}.settings.sortBy`, order.value);
              }}
            />
          </Col>
          <Toggle
            name={`sections.${index}.settings.sortDirection`}
            options={directionOptions}
            value={section.settings.sortDirection ? section.settings.sortDirection : 'asc'}
            onChange={(value) => {
              setFieldValue(`sections.${index}.settings.sortDirection`, value);
            }}
          />
        </Row>
        <Col className="frm-in">
          <label>Report Section Options</label>
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
          <Show visible={!!section.folderId || !!section.linkedReportId}>
            <Row>
              <FormikCheckbox
                name={`sections.${index}.settings.overrideExcludeHistorical`}
                label={`Include all content from linked ${
                  section.folderId ? 'folder' : 'report'
                } even if in prior report`}
              />
              <span className="info">
                This overrides the report option "Exclude stories that have been sent out in
                previous report" for this section only.
              </span>
            </Row>
          </Show>
          <FormikCheckbox
            name={`sections.${index}.settings.hideEmpty`}
            label="Hide this section in the report when empty"
          />
          <FormikCheckbox
            name={`sections.${index}.settings.showFullStory`}
            label="Show Full Story"
            tooltip="Display the full story for each content item in this section"
          />
          <FormikCheckbox
            name={`sections.${index}.settings.showImage`}
            label="Show Image"
            tooltip="Display the image for each content item in this section (if there is an image)"
          />
          <FormikCheckbox
            name={`sections.${index}.settings.showHeadlines`}
            label="Show additional Table of Content for this section"
            tooltip="Display a Table of Contents at the beginning of this section."
          />
        </Col>
      </Col>
    );
  },
);
