import React from 'react';
import { FormikText, FormikWysiwyg } from 'tno-core';

export interface IReportSectionTableOfContentsProps
  extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  sectionIndex: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Form is disabled. */
  disabled?: boolean;
}

/**
 * Component provides a way to configure a table of contents section in a report.
 */
export const ReportSectionTableOfContents = React.forwardRef<
  HTMLDivElement,
  IReportSectionTableOfContentsProps
>(({ sectionIndex, disabled, ...rest }, ref) => {
  return (
    <>
      <FormikText
        name={`sections.${sectionIndex}.settings.label`}
        label="Section heading:"
        disabled={disabled}
      />
      <FormikWysiwyg
        name={`sections.${sectionIndex}.description`}
        label="Summary text:"
        disabled={disabled}
      ></FormikWysiwyg>
    </>
  );
});
