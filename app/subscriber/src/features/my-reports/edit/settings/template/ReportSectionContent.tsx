import React from 'react';
import { Col, FormikCheckbox, FormikText } from 'tno-core';

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
    return (
      <Col gap="0.5rem">
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <Col className="frm-in">
          <label>Report Section Options</label>
          <FormikCheckbox
            name={`sections.${index}.settings.hideEmpty`}
            label="Hide this section in the report when empty"
          />
          <FormikCheckbox
            name={`sections.${index}.settings.removeDuplicates`}
            label="Remove duplicate stories"
            tooltip="Remove content from this section that is in above sections"
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
