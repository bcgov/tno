import React from 'react';
import { FormikText, FormikWysiwyg } from 'tno-core';

export interface IReportSectionTextProps {
  index: number;
}

export const ReportSectionText = React.forwardRef<HTMLDivElement, IReportSectionTextProps>(
  ({ index, ...rest }, ref) => {
    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikWysiwyg name={`sections.${index}.description`} label="Summary text:" />
      </>
    );
  },
);
