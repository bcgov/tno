import React from 'react';
import { FormikText, FormikTextArea, Show } from 'tno-core';

import { IReportSectionProps } from './ReportSection';

/**
 * Component provides a way to configure a table of contents section in a report.
 */
export const ReportSectionTableOfContents = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, showForm, ...rest }, ref) => {
    return (
      <Show visible={showForm}>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
      </Show>
    );
  },
);
