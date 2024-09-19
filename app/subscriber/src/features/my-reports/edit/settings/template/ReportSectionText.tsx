import React from 'react';
import { FormikCheckbox, FormikText, FormikWysiwyg, Row } from 'tno-core';

export interface IReportSectionTextProps {
  index: number;
}

export const ReportSectionText = React.forwardRef<HTMLDivElement, IReportSectionTextProps>(
  ({ index, ...rest }, ref) => {
    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikWysiwyg name={`sections.${index}.description`} label="Summary text:" />
        <Row>
          <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
          <span className="info">
            When hidden the content is still part of the report, but the stories are not displayed
            in the table of contents, or in their own section.
          </span>
        </Row>
      </>
    );
  },
);
