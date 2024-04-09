import React from 'react';
import { FormikText, FormikTextArea } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionGalleryProps extends React.AllHTMLAttributes<HTMLDivElement> {
  /** Array index position of section. */
  sectionIndex: number;
  /** Icon to display in header */
  icon?: React.ReactNode;
  /** Form is disabled. */
  disabled?: boolean;
}

/**
 * Component provides a way to configure a section that contains content.
 * Content can be provided by a filter or a folder.
 * A content section can also display charts.
 */
export const ReportSectionGallery: React.FC<IReportSectionGalleryProps> = ({
  sectionIndex,
  disabled,
  ...rest
}) => {
  const { values } = useReportEditContext();

  const instance = values.instances.length ? values.instances[0] : null;

  if (instance == null) return null;

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
};
