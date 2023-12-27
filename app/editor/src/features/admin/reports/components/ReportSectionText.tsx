import { Col, FormikText, FormikTextArea } from 'tno-core';

export interface IReportSectionTextProps {
  index: number;
}

export const ReportSectionText = ({ index }: IReportSectionTextProps) => {
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
    </Col>
  );
};
