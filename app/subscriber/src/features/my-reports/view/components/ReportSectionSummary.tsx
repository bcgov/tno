import React from 'react';
import { FormikText, FormikTextArea, Show } from 'tno-core';

import { IReportSectionProps } from './ReportSection';

export const ReportSectionSummary = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, showForm, ...rest }, ref) => {
    return (
      <Show visible={showForm}>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
      </Show>
    );
  },
);
