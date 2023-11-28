import { useFormikContext } from 'formik';
import React from 'react';
import { Checkbox, FormikText, FormikTextArea, ReportSectionTypeName, Show } from 'tno-core';

import { IReportForm } from '../../../interfaces';
import { IReportSectionProps } from './ReportSection';
import { ReportSectionCharts } from './ReportSectionCharts';

export const ReportSectionSummary = React.forwardRef<HTMLDivElement, IReportSectionProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useFormikContext<IReportForm>();

    const section = values.sections[index];

    return (
      <>
        <Show visible={section.settings.showCharts}>
          <Checkbox
            name="sectionType"
            label="Summary of stories above"
            checked={section.settings.sectionType === ReportSectionTypeName.Summary}
            value={true}
            onChange={(e) => {
              setFieldValue(
                `sections.${index}.settings.sectionType`,
                e.target.checked ? ReportSectionTypeName.Summary : ReportSectionTypeName.Content,
              );
            }}
          />
        </Show>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikTextArea name={`sections.${index}.description`} label="Summary text:" />
        <Show visible={section.settings.showCharts}>
          <ReportSectionCharts index={index} />
        </Show>
      </>
    );
  },
);
