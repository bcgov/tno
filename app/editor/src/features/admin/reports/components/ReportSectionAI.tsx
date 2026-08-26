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
  const selectedLLMId = values.sections[index].settings.llmId;

  // Derived from the lookup rather than copied into state on mount: the lookup is shared, so it can
  // arrive after this section rendered, and a copy taken too early leaves the field with no options
  // and nothing selected.
  const availableLLMs = React.useMemo(
    () => (isAdmin ? llms : llms.filter((m) => m.isPublic)),
    [isAdmin, llms],
  );
  const llmOptions = React.useMemo(() => getSortableOptions(availableLLMs), [availableLLMs]);
  const llm = React.useMemo(
    () => llms.find((m) => m.id === selectedLLMId) ?? availableLLMs.at(0),
    [availableLLMs, llms, selectedLLMId],
  );

  /**
   * Record the model on the section along with the parameters that belong to it. The values are
   * replaced rather than assigned into: the report often arrives from the cached (frozen) copy, and
   * writing through `values.sections[index].settings` throws '"llmId" is read-only'.
   */
  const applyLLM = React.useCallback(
    (llm?: ILLMModel) => {
      setValues((values) => ({
        ...values,
        sections: values.sections.map((section, i) =>
          i !== index
            ? section
            : {
                ...section,
                settings: {
                  ...section.settings,
                  llmId: llm?.id,
                  temperature: llm?.minTemperature,
                  userPrompt: llm?.userPrompt,
                },
              },
        ),
      }));
    },
    [index, setValues],
  );

  React.useEffect(
    () => {
      if (userInfo && !llms.length) getLLMs().catch(() => {});
    },
    // do not want to trigger on loading change, will cause infinite loop
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [userInfo],
  );

  React.useEffect(() => {
    // Give a section with no model the default one, but only once the lookup has loaded - writing
    // before then clears the saved selection, and the section comes back holding whichever model
    // happens to sort first. A section that already holds this model keeps its own temperature and
    // prompt; they are only reset when the model actually changes.
    if (!llm || selectedLLMId === llm.id) return;
    applyLLM(llm);
  }, [applyLLM, llm, selectedLLMId]);

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
            const option = e as OptionItem;
            applyLLM(availableLLMs.find((m) => m.id === option?.value));
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
        name={`sections.${index}.settings.showErrorDetails`}
        label="Show error details in this section when the AI request fails"
        tooltip="Renders the failure reason, status and provider response into the section body instead of leaving it empty. Turn off once the prompt is working - subscribers see whatever the section holds."
        checked={!!values.sections[index].settings.showErrorDetails}
        onChange={(e) => {
          setFieldValue(`sections.${index}.settings.showErrorDetails`, e.target.checked);
        }}
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
