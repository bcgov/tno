import React from 'react';
import { Col, FormikCheckbox, FormikText, Row } from 'tno-core';

export interface IReportSectionImageProps {
  index: number;
}

export const ReportSectionImage = React.forwardRef<HTMLDivElement, IReportSectionImageProps>(
  ({ index, ...rest }, ref) => {
    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <Row>
          <Col className="frm-in" flex="1">
            <label>Report Section Options</label>
            <Row>
              <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
            </Row>
            <Col>
              <FormikText name={`sections.${index}.settings.url`} label="URL" />
            </Col>
          </Col>
        </Row>
      </>
    );
  },
);
