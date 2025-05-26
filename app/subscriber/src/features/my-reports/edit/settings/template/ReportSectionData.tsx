import React from 'react';
import {
  Checkbox,
  Col,
  FormikCheckbox,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IOptionItem,
  OptionItem,
  ReportSectionDataTypeOptions,
  Row,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionDataProps {
  index: number;
}

export const ReportSectionData = React.forwardRef<HTMLDivElement, IReportSectionDataProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();

    const [dataTypeOptions] = React.useState<IOptionItem[]>(ReportSectionDataTypeOptions);

    React.useEffect(() => {
      setFieldValue(`sections.${index}.settings.preload`, true);
    }, [setFieldValue, index]);

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
            <Row>
              <Col flex="1">
                <FormikText required label="Url" name={`sections.${index}.settings.url`} />
              </Col>
              <FormikSelect
                name={`sections.${index}.settings.dataType`}
                label="Data Type"
                required
                options={dataTypeOptions}
                value={
                  dataTypeOptions.find(
                    (o) => o.value === values.sections[index].settings.dataType,
                  ) ?? ''
                }
                onChange={(newValue) => {
                  const option = newValue as OptionItem;
                  const order = dataTypeOptions.find((f) => f.value === option?.value);
                  if (order) setFieldValue(`sections.${index}.settings.dataType`, order.value);
                }}
              />
            </Row>

            <FormikText
              label="Path to Property"
              name={`sections.${index}.settings.dataProperty`}
              tooltip="XPath to the property in the data you want displayed."
              placeholder="Optional XPath to a single value.  If a path is provided it will be used instead of the template."
            />
            <p>
              The Razor template is a C# syntax that enables the ability to convert the source data
              into HTML.
            </p>
            <FormikTextArea
              label="Razor Template"
              name={`sections.${index}.settings.dataTemplate`}
            />
          </Col>
        </Row>
      </>
    );
  },
);
