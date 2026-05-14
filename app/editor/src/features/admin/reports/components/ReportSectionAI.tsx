import { useFormikContext } from 'formik';
import React from 'react';
import { FaPaste } from 'react-icons/fa';
import { useLookup } from 'store/hooks';
import { useAppStore } from 'store/slices';
import {
  Button,
  ButtonVariant,
  Checkbox,
  Claim,
  Col,
  FieldSize,
  FormikText,
  FormikTextArea,
  FormikWysiwyg,
  getSortableOptions,
  ILLMModel,
  IOptionItem,
  type IReportModel,
  OptionItem,
  Row,
  Select,
  Show,
} from 'tno-core';

export interface IReportSectionAIProps {
  index: number;
}

export const ReportSectionAI = ({ index }: IReportSectionAIProps) => {
  const { values, setFieldValue, setValues } = useFormikContext<IReportModel>();
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
    setLLM(defaultLLM);
  }, [defaultLLMId, llms]);

  React.useEffect(() => {
    const newValues = { ...values };
    newValues.sections[index].settings.llmId = llm?.id;
    newValues.sections[index].settings.temperature = llm?.minTemperature;
    newValues.sections[index].settings.userPrompt = llm?.userPrompt;

    setValues(newValues);
    // Do not execute every time the values is updates otherwise it will be an infinite loop.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [llm, setValues]);

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
        <Select
          name={`sections.${index}.settings.deploymentName`}
          label="AI Model:"
          tooltip="The name of the deployed AI model"
          options={llmOptions}
          value={llmOptions.find((c) => c.value === llm?.id) ?? ''}
          onChange={(e) => {
            const o = e as OptionItem;
            const llm = llms.find((m) => m.id === o?.value);
            setLLM(llm);
          }}
        ></Select>
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
        <FormikText
          name={`sections.${index}.settings.choiceQty`}
          label="Choices"
          tooltip="Number of responses you want provided to compare.  Some models do not have this feature."
          type="number"
          width={FieldSize.Tiny}
          placeholder="1"
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            const value = e.target.value;
            setFieldValue(
              `sections.${index}.settings.choiceQty`,
              value.trim() === '' ? undefined : +value,
            );
            console.debug(value);
          }}
        />
      </Row>
      <Row>
        <Col flex="1">
          <FormikWysiwyg
            name={`sections.${index}.settings.userPrompt`}
            label="Prompt:"
            placeholder={llm?.userPrompt}
          />
        </Col>
        <Col alignContent="center" justifyContent="center">
          <Button
            variant={ButtonVariant.link}
            title="Use default user prompt"
            onClick={() => setFieldValue(`sections.${index}.settings.userPrompt`, llm?.userPrompt)}
          >
            <FaPaste />
          </Button>
        </Col>
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
      />{' '}
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
    </Col>
  );
};
