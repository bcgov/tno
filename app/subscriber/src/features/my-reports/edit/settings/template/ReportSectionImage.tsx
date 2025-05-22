import React from 'react';
import { Checkbox, Col, FormikCheckbox, FormikText, Row } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionImageProps {
  index: number;
}

export const ReportSectionImage = React.forwardRef<HTMLDivElement, IReportSectionImageProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();

    return (
      <>
        <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
        <Row>
          <Col className="frm-in" flex="1">
            <label>Report Section Options</label>
            <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
            <Checkbox
              name={`sections.${index}.settings.inTableOfContents`}
              label="Include in Table of Contents"
              checked={
                values.sections[index].settings.inTableOfContents === undefined
                  ? true
                  : values.sections[index].settings.inTableOfContents
              }
              onChange={(e) => {
                setFieldValue(`sections.${index}.settings.inTableOfContents`, e.target.checked);
              }}
            />
            <Col>
              <FormikText name={`sections.${index}.settings.url`} label="URL" required />
            </Col>
          </Col>
        </Row>
      </>
    );
  },
);
