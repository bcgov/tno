import { useFormikContext } from 'formik';
import { Checkbox, Col, FormikText, FormikTextArea, IReportModel } from 'tno-core';

export interface IReportSectionTextProps {
  index: number;
}

export const ReportSectionText = ({ index }: IReportSectionTextProps) => {
  const { values, setFieldValue } = useFormikContext<IReportModel>();

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
    </Col>
  );
};
