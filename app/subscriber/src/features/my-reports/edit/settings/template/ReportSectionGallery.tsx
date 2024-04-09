import React from 'react';
import { Col, FormikCheckbox, FormikSelect, FormikText, OptionItem, Row } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionGalleryProps {
  index: number;
}

export const ReportSectionGallery = React.forwardRef<HTMLDivElement, IReportSectionGalleryProps>(
  ({ index, ...rest }, ref) => {
    const { values } = useReportEditContext();

    const [directionOptions] = React.useState([
      new OptionItem('Horizontal', 'row'),
      new OptionItem('Vertical', 'column'),
    ]);

    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <Row>
          <Col className="frm-in" flex="1">
            <label>Report Section Options</label>
            <FormikCheckbox
              name={`sections.${index}.settings.showImage`}
              label="Show Image"
              tooltip="Display the image for each content item in this section (if there is an image)"
            />
          </Col>
          <Col className="frm-in" flex="1">
            <FormikSelect
              name={`sections.${index}.settings.direction`}
              label="Direction of content"
              options={directionOptions}
              value={
                directionOptions.find(
                  (o) => o.value === values.sections[index].settings.direction,
                ) ?? ''
              }
            />
          </Col>
        </Row>
      </>
    );
  },
);
