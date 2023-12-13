import React from 'react';
import { FormikText, FormikTextArea, Show } from 'tno-core';

export interface IReportSectionTableOfContentsProps
  extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  index: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Enable toggling the form values */
  showForm?: boolean;
  /** Form is disabled. */
  disabled?: boolean;
}

/**
 * Component provides a way to configure a table of contents section in a report.
 */
export const ReportSectionTableOfContents = React.forwardRef<
  HTMLDivElement,
  IReportSectionTableOfContentsProps
>(({ index, showForm, disabled, ...rest }, ref) => {
  return (
    <Show visible={showForm}>
      <FormikText
        name={`sections.${index}.settings.label`}
        label="Section heading:"
        disabled={disabled}
      />
      <FormikTextArea
        name={`sections.${index}.description`}
        label="Summary text:"
        disabled={disabled}
      />
    </Show>
  );
});
