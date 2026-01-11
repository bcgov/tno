import React from 'react';
import { useAppStore } from 'store/slices';
import {
  Checkbox,
  Claim,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikText,
  FormikWysiwyg,
  Row,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionAIProps {
  index: number;
}

export const ReportSectionAI = React.forwardRef<HTMLDivElement, IReportSectionAIProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue } = useReportEditContext();
    const [{ userInfo }] = useAppStore();

    return (
      <>
        <Row>
          <Col flex="1">
            <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
          </Col>
          <Row>
            {userInfo?.roles.includes(Claim.administrator) && (
              <FormikText
                name={`sections.${index}.settings.deploymentName`}
                label="AI Model:"
                placeholder="gpt-5.1-chat"
                tooltip="The name of the model deployment"
              />
            )}
            <FormikText
              name={`sections.${index}.settings.temperature`}
              label="Temperature:"
              width={FieldSize.Small}
              type="number"
              tooltip="Apply randomness to responses. Depending on the model it may support values 0 - 2. Lower is more deterministic.  Higher is creative."
              onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                let value = e.target.value;
                setFieldValue(
                  `sections.${index}.settings.temperature`,
                  value.trim() === '' ? undefined : +value,
                );
                console.debug(value);
              }}
            />
          </Row>
        </Row>
        <FormikWysiwyg name={`sections.${index}.description`} label="Description:" />
        {userInfo?.roles.includes(Claim.administrator) && (
          <FormikWysiwyg
            name={`sections.${index}.settings.systemPrompt`}
            label="System Prompt:"
            placeholder="You are a report writer.  Review the report data and generate summaries and analysis.  The report data is JSON, each section groups related content and contains an array of story records.  The `content.text` property contains the story information.  The output generated must be in simple HTML that works within Outlook email client.  Place all the output in a <div>\{output\}</div>."
          />
        )}
        <FormikWysiwyg
          name={`sections.${index}.settings.userPrompt`}
          label="Prompt:"
          placeholder="Create an executive summary of the report data."
        />
        <Row>
          <FormikCheckbox name={`sections.${index}.isEnabled`} label="Section is visible" />
        </Row>
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
      </>
    );
  },
);
