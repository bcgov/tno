import React from 'react';
import { FormikText, FormikTextArea } from 'tno-core';

export interface IReportSectionTextProps extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  sectionIndex: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Form is disabled. */
  disabled?: boolean;
}

export const ReportSectionText = React.forwardRef<HTMLDivElement, IReportSectionTextProps>(
  ({ sectionIndex, disabled, ...rest }, ref) => {
    return (
      <>
        <FormikText
          name={`sections.${sectionIndex}.settings.label`}
          label="Section heading:"
          disabled={disabled}
        />
        <FormikTextArea
          name={`sections.${sectionIndex}.description`}
          label="Summary text:"
          disabled={disabled}
        />
      </>
    );
  },
);
