import React from 'react';
import { Checkbox, Col, FormikCheckbox, FormikText, Row, Show } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionContentProps {
  /** Section index position */
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

    const section = values.sections[index];

    return (
      <Col gap="0.5rem">
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <Col className="frm-in">
          <label>Report Section Options</label>
          <Row>
            <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
            <span className="info">
              When hidden the content is still part of the report, but the stories are not displayed
              in the table of contents, or in their own section.
            </span>
          </Row>
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
          <Row>
            <FormikCheckbox
              name={`sections.${index}.settings.removeDuplicateTitles3Days`}
              label="Remove duplicate titles from the last 3 days"
            />
            <span className="info">
              Keep only the most recent story when duplicate titles occur within the last three
              days.
            </span>
          </Row>
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
            name={`sections.${index}.settings.convertToBase64Image`}
            label="Convert Images to Base64"
            tooltip="This format is not well supported by all email clients and can significantly increase the size of the report.  Use with caution."
          />
          <FormikCheckbox
            name={`sections.${index}.settings.showHeadlines`}
            label="Show additional Table of Content for this section"
            tooltip="Display a Table of Contents at the beginning of this section."
          />
          <Checkbox
            name={`sections.${index}.settings.inTableOfContents`}
            label="Include in Table of Contents"
            checked={
              values.sections[index].settings.inTableOfContents === undefined
                ? true
                : values.sections[index].settings.inTableOfContents
            }
            onChange={(e) => {
              setFieldValue(`sections.${index}.settings.inTableOfContents`, e.target.checked);
            }}
          />
        </Col>
      </Col>
    );
  },
);
