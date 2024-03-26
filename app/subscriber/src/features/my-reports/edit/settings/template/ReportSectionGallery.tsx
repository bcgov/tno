import React from 'react';
import { FormikCheckbox, FormikText } from 'tno-core';

export interface IReportSectionGalleryProps {
  index: number;
}

export const ReportSectionGallery = React.forwardRef<HTMLDivElement, IReportSectionGalleryProps>(
  ({ index, ...rest }, ref) => {
    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <FormikCheckbox
          name={`sections.${index}.settings.showImage`}
          label="Show Image"
          tooltip="Display the image for each content item in this section (if there is an image)"
        />
      </>
    );
  },
);
