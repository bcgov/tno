import React from 'react';
import { Checkbox, Col, FormikCheckbox, FormikSelect, FormikText, OptionItem, Row } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionGalleryProps {
  index: number;
}

export const ReportSectionGallery = React.forwardRef<HTMLDivElement, IReportSectionGalleryProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();

    const section = values.sections[index];

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
            <Row>
              <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
              <span className="info">
                When hidden the content is still part of the report, but the stories are not
                displayed in the table of contents, or in their own section.
              </span>
            </Row>
            <Row>
              <FormikCheckbox
                name={`sections.${index}.settings.removeDuplicates`}
                label="Remove duplicate stories"
              />
              <span className="info">
                Do not include in this section content that already exists in the above sections
                (does not apply to charts that link to other reports)
              </span>
            </Row>
            <Row>
              <FormikCheckbox
                name={`sections.${index}.settings.overrideExcludeHistorical`}
                label={`Include all content from linked ${
                  section.folderId ? 'folder' : 'report'
                } even if in prior report`}
              />
              <span className="info">
                This overrides the report option "Exclude stories that have been sent out in
                previous report" for this section only.
              </span>
            </Row>
            <FormikCheckbox
              name={`sections.${index}.settings.hideEmpty`}
              label="Hide this section in the report when empty"
            />
            <FormikCheckbox
              name={`sections.${index}.settings.showImage`}
              label="Show Image"
              tooltip="Display the image for each content item in this section (if there is an image)"
            />
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
