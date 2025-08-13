import { useFormikContext } from 'formik';
import { Checkbox, Col, FormikCheckbox, FormikText, FormikTextArea, IReportModel } from 'tno-core';

export interface IReportSectionImageProps {
  index: number;
}

export const ReportSectionImage = ({ index }: IReportSectionImageProps) => {
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
      <Col>
        <FormikText label="Url" name={`sections.${index}.settings.url`} required />
      </Col>
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
      <FormikCheckbox
        name={`sections.${index}.settings.cacheData`}
        label="Cache Image"
        tooltip="Save a copy in MMI"
      />
    </Col>
  );
};
