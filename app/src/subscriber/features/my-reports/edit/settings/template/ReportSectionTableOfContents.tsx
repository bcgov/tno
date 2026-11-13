import React from 'react';

import { FormikText } from '@/shared';

export interface IReportSectionTableOfContentsProps {
  index: number;
}

/**
 * Component provides a way to configure a table of contents section in a report.
 */
export const ReportSectionTableOfContents = React.forwardRef<
  HTMLDivElement,
  IReportSectionTableOfContentsProps
>(({ index, ...rest }, ref) => {
  return (
    <>
      <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
    </>
  );
});
