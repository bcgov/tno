import React from 'react';
import { FaPaste } from 'react-icons/fa6';
import { useLookup } from 'store/hooks';
import { useAppStore } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Claim,
  Col,
  FieldSize,
  FormikCheckbox,
  FormikText,
  FormikWysiwyg,
  getSortableOptions,
  ILLMModel,
  IOptionItem,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';

export interface IReportSectionAIProps {
  index: number;
}

export const ReportSectionAI = React.forwardRef<HTMLDivElement, IReportSectionAIProps>(
  ({ index, ...rest }, ref) => {
    const { values, setFieldValue, setValues } = useReportEditContext();
    const [{ userInfo }] = useAppStore();
    const [{ llms }, { getLLMs }] = useLookup();

    const isAdmin = userInfo?.roles.includes(Claim.administrator);
    const defaultLLMId = values.sections[index].settings.llmId;

    const [llm, setLLM] = React.useState<ILLMModel>();
    const [llmOptions, setLLMOptions] = React.useState<IOptionItem[]>(
      getSortableOptions(isAdmin ? llms : llms.filter((m) => m.isPublic)),
    );

    React.useEffect(
      () => {
        if (userInfo && !llms.length) {
          getLLMs().then((llms) => {
            const llmValues = isAdmin ? llms : llms.filter((m) => m.isPublic);
            setLLMOptions(getSortableOptions(llmValues));
          });
        }
      },
      // do not want to trigger on loading change, will cause infinite loop
      // eslint-disable-next-line react-hooks/exhaustive-deps
      [userInfo],
    );

    React.useEffect(() => {
      const defaultLLM =
        llms.find((m) => m.id === defaultLLMId) ?? (llms.length > 0 ? llms[0] : undefined);
      // Compare by ID so a new array reference from Redux doesn't re-trigger effect 2.
      setLLM((prev) => (prev?.id === defaultLLM?.id ? prev : defaultLLM));
    }, [defaultLLMId, llms]);

    React.useEffect(() => {
      if (llm === undefined) return;
      const newValues = { ...values };
      console.error('What does this do?', llm);
      if (llm.id && llm.id !== newValues.sections[index].settings.llmId) {
        newValues.sections[index].settings.llmId = llm.id;
        newValues.sections[index].settings.temperature = llm.minTemperature;
        newValues.sections[index].settings.userPrompt = llm.userPrompt;
      }
      setValues(newValues);
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [llm, setValues]);

    return (
      <>
        <Row>
          <Col flex="1">
            <FormikText name={`sections.${index}.settings.label`} label="Section heading:" />
          </Col>
          <Row>
            <Select
              name={`sections.${index}.settings.deploymentName`}
              label="AI Model:"
              tooltip="The name of the deployed AI model"
              isClearable={false}
              options={llmOptions}
              value={llmOptions.find((c) => c.value === llm?.id) ?? ''}
              onChange={(e) => {
                const o = e as OptionItem;
                const llm = llms.find((m) => m.id === o?.value);
                setLLM(llm);
              }}
            ></Select>
            {userInfo?.roles.includes(Claim.administrator) && (
              <Show visible={llm?.minTemperature !== llm?.maxTemperature}>
                <FormikText
                  name={`sections.${index}.settings.temperature`}
                  label="Temp:"
                  width={FieldSize.Tiny}
                  type="number"
                  tooltip={`Apply randomness to responses. Depending on the model it may support values ${llm?.minTemperature} - ${llm?.maxTemperature}. Lower is more deterministic.  Higher is creative.`}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
                    let value = +e.target.value;
                    if (e.target.value === '')
                      setFieldValue(`sections.${index}.settings.temperature`, undefined);
                    else if (
                      llm?.minTemperature !== undefined &&
                      value >= llm.minTemperature &&
                      llm?.maxTemperature !== undefined &&
                      value <= llm.maxTemperature
                    ) {
                      setFieldValue(`sections.${index}.settings.temperature`, value);
                    }
                  }}
                />
              </Show>
            )}
          </Row>
        </Row>
        <FormikWysiwyg name={`sections.${index}.description`} label="Description:" />
        <Row>
          <Col flex="1">
            <FormikWysiwyg
              name={`sections.${index}.settings.userPrompt`}
              label="Prompt:"
              placeholder="Create a concise summary within each section."
            />
          </Col>
          <Col justifyContent="center">
            <Button
              variant={ButtonVariant.link}
              title="Use default user prompt"
              onClick={() =>
                setFieldValue(`sections.${index}.settings.userPrompt`, llm?.userPrompt)
              }
            >
              <FaPaste />
            </Button>
          </Col>
        </Row>
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
        <FormikText
          name={`sections.${index}.settings.includePreviousReports`}
          label="Number of prior reports included:"
          tooltip="Specify the number of previous reports that should be included in this section."
          width={FieldSize.Tiny}
          type="number"
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            let value = +e.target.value;
            if (e.target.value === '')
              setFieldValue(`sections.${index}.settings.includePreviousReports`, undefined);
            else setFieldValue(`sections.${index}.settings.includePreviousReports`, value);
          }}
        />
      </>
    );
  },
);
