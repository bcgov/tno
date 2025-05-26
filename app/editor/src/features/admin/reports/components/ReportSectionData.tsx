import { useFormikContext } from 'formik';
import { highlight, languages } from 'prismjs';
import React from 'react';
import Editor from 'react-simple-code-editor';
import {
  Checkbox,
  Col,
  FormikSelect,
  FormikText,
  FormikTextArea,
  IOptionItem,
  IReportModel,
  OptionItem,
  ReportSectionDataTypeOptions,
  Row,
} from 'tno-core';

export interface IReportSectionDataProps {
  index: number;
}

export const ReportSectionData = ({ index }: IReportSectionDataProps) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

  const [dataTypeOptions] = React.useState<IOptionItem[]>(ReportSectionDataTypeOptions);

  React.useEffect(() => {
    setFieldValue(`sections.${index}.settings.preload`, true);
  }, [setFieldValue, index]);

  return (
    <Col gap="1rem" className="section">
      <FormikText
        name={`sections.${index}.settings.label`}
        label="Heading"
        tooltip="A heading label to display at the beginning of the section"
        maxLength={100}
      />
      <FormikTextArea
        name={`sections.${index}.description`}
        label="Summary"
        tooltip="The summary will be displayed at the beginning of the section"
        placeholder="Executive summary for this section or the whole report"
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
      <Col>
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
              dataTypeOptions.find((o) => o.value === values.sections[index].settings.dataType) ??
              ''
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
        <Col className="code">
          <label htmlFor="txa-data-template">Razor Template</label>
          <Col className="editor data-template">
            <Editor
              id="txa-data-template"
              value={values.sections[index].settings.dataTemplate ?? ''}
              onValueChange={(code) =>
                setFieldValue(`sections.${index}.settings.dataTemplate`, code)
              }
              highlight={(code) => {
                return highlight(code, languages.cshtml, 'razor');
              }}
            />
          </Col>
        </Col>
      </Col>
    </Col>
  );
};
