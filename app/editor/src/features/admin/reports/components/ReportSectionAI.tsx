import { useFormikContext } from 'formik';
import { Checkbox, Col, FieldSize, FormikText, FormikTextArea, IReportModel, Row } from 'tno-core';

export interface IReportSectionAIProps {
  index: number;
}

export const ReportSectionAI = ({ index }: IReportSectionAIProps) => {
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
        label="Description"
        tooltip="Describe the purpose of this section"
        placeholder="An AI summary of the report"
      />
      <Row>
        <FormikText
          name={`sections.${index}.settings.deploymentName`}
          label="Model Deployment Name"
          tooltip="The name of the model deployment"
          maxLength={100}
          placeholder="gpt-5.1-chat"
        />
        <FormikText
          name={`sections.${index}.settings.temperature`}
          label="Temperature"
          tooltip="Apply randomness to responses. Depending on the model it may support values 0 - 2. Lower is more deterministic.  Higher is creative."
          type="number"
          width={FieldSize.Tiny}
          placeholder="0"
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            let value = e.target.value;
            setFieldValue(
              `sections.${index}.settings.temperature`,
              value.trim() === '' ? undefined : +value,
            );
            console.debug(value);
          }}
        />
        <FormikText
          name={`sections.${index}.settings.choiceQty`}
          label="Choices"
          tooltip="Number of responses you want provided to compare.  Some models do not have this feature."
          type="number"
          width={FieldSize.Tiny}
          placeholder="1"
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            let value = e.target.value;
            setFieldValue(
              `sections.${index}.settings.choiceQty`,
              value.trim() === '' ? undefined : +value,
            );
            console.debug(value);
          }}
        />
      </Row>
      <FormikTextArea
        name={`sections.${index}.settings.systemPrompt`}
        label="System Prompt"
        tooltip="Sets the global instructions and behavior for the model."
        placeholder="You are a report writer.  Review the report data and generate summaries and analysis.  The report data is JSON, each section groups related content and contains an array of story records. The `content.text` property contains the story information.  The output generated must be in simple HTML that works within Outlook email client.  Place all the output in a <div>\{output\}</div>."
      />
      <FormikTextArea
        name={`sections.${index}.settings.userPrompt`}
        label="User Prompt"
        tooltip="Represents end-user input — questions, instructions, data, or prompts."
        placeholder="Create an executive summary of the report data."
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
