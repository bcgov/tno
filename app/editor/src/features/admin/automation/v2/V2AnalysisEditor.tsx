import React from 'react';
import { FaPlus, FaTrash } from 'react-icons/fa';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import { fitSelectWidth } from './constants';
import { type IV2Analysis } from './interfaces';
import { V2ComboBox } from './V2ComboBox';

export interface IV2AnalysisEditorProps {
  analysis: IV2Analysis;
  /** Names of analyses declared earlier in the step (valid chain targets). */
  earlierNames: string[];
  promptNames: string[];
  llmOptions: IOptionItem[];
  onChange: (analysis: IV2Analysis) => void;
}

/** Common return type specs offered by the Type autocomplete; freeform (e.g. 'int(-5..5)')
 * stays allowed. */
const RETURN_TYPES = ['string', 'string?', 'string[]', 'bool', 'int', 'int(-5..5)'];

/**
 * Editor for one named analysis: its prompt (library reference plus override, or inline text),
 * its declared result shape, optional chaining, and raw mode. One analysis covering several
 * properties shares a single LLM call; one per property keeps a complex prompt isolated.
 */
export const V2AnalysisEditor: React.FC<IV2AnalysisEditorProps> = ({
  analysis,
  earlierNames,
  promptNames,
  llmOptions,
  onChange,
}) => {
  const promptOptions = [
    createOption('(inline text)', ''),
    ...promptNames.map((name) => createOption(name, name)),
  ];
  const chainOptions = [
    createOption('(none)', ''),
    ...earlierNames.map((name) => createOption(name, name)),
  ];
  const llmSelectOptions = [createOption('step default', ''), ...llmOptions];
  const returnEntries = Object.entries(analysis.returns ?? {});

  const set = (values: Partial<IV2Analysis>) => onChange({ ...analysis, ...values });

  return (
    <Col className="v2-analysis-editor" gap="0.5rem">
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Text
          name="analysis-name"
          label="Name"
          required
          width="12rem"
          value={analysis.name}
          onChange={(e) => set({ name: e.target.value })}
        />
        <Select
          name="analysis-prompt-ref"
          label="Prompt"
          width={fitSelectWidth(['(inline text)', ...promptNames])}
          isClearable={false}
          options={promptOptions}
          value={findOptionByValue(promptOptions, analysis.prompt?.ref ?? '')}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const ref = option?.value ? `${option.value}` : null;
            set({ prompt: { ...analysis.prompt, ref, text: ref ? null : analysis.prompt?.text } });
          }}
        />
        <Select
          name="analysis-chain"
          label="Chain"
          width={fitSelectWidth(['(none)', ...earlierNames])}
          isClearable={false}
          options={chainOptions}
          value={findOptionByValue(chainOptions, analysis.chain ?? '')}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            set({ chain: option?.value ? `${option.value}` : null });
          }}
        />
      </Row>
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Select
          name="analysis-llm"
          label="LLM Override"
          width={fitSelectWidth(['step default', ...llmOptions.map((option) => `${option.label}`)])}
          isClearable={false}
          options={llmSelectOptions}
          value={findOptionByValue(llmOptions, analysis.llmId) ?? createOption('step default', '')}
          onChange={(newValue) => set({ llmId: toNumberOrUndefined(newValue as IOptionItem) })}
        />
        <div className="checkbox-inline">
          <Checkbox
            name="analysis-raw"
            label="Raw response"
            tooltip="Keep the response as plain text; actions gate on it with confirmation statements (how migrated v1 actions work)."
            checked={analysis.raw ?? false}
            onChange={(e) => set({ raw: e.target.checked })}
          />
        </div>
      </Row>
      <Show visible={!analysis.prompt?.ref}>
        <TextArea
          name="analysis-prompt-text"
          label="Prompt Text"
          rows={4}
          placeholder="The prompt sent for this analysis"
          value={analysis.prompt?.text ?? ''}
          onChange={(e) => set({ prompt: { ...analysis.prompt, text: e.target.value } })}
        />
      </Show>
      <Show visible={!!analysis.prompt?.ref}>
        <TextArea
          name="analysis-prompt-override"
          label="Prompt Override"
          rows={4}
          placeholder="Text layered onto the shared prompt for this step only"
          value={analysis.prompt?.override ?? ''}
          onChange={(e) =>
            set({ prompt: { ...analysis.prompt, override: e.target.value || null } })
          }
        />
      </Show>
      <Show visible={!analysis.raw}>
        <Col gap="0.25rem">
          <label>Returns</label>
          <div className="v2-returns">
            <div className="v2-returns-head">
              <span>Key</span>
              <span>Type</span>
              <button
                type="button"
                className="v2-returns-add"
                title="Add a return key"
                onClick={() => set({ returns: { ...(analysis.returns ?? {}), '': 'string' } })}
              >
                <FaPlus />
              </button>
            </div>
            {returnEntries.map(([key, type], index) => (
              <div key={index} className="v2-returns-row">
                <Text
                  name={`analysis-return-key-${index}`}
                  placeholder="key"
                  width="100%"
                  value={key}
                  onChange={(e) => {
                    const updated = returnEntries.map(([k, t], i) =>
                      i === index ? ([e.target.value, t] as const) : ([k, t] as const),
                    );
                    set({ returns: Object.fromEntries(updated) });
                  }}
                />
                <V2ComboBox
                  name={`analysis-return-type-${index}`}
                  placeholder="type"
                  suggestions={RETURN_TYPES}
                  value={type}
                  onChange={(next) => {
                    const updated = returnEntries.map(([k, t], i) =>
                      i === index ? ([k, next] as const) : ([k, t] as const),
                    );
                    set({ returns: Object.fromEntries(updated) });
                  }}
                />
                <button
                  type="button"
                  className="v2-returns-del"
                  title="Remove this return key"
                  onClick={() =>
                    set({
                      returns: Object.fromEntries(returnEntries.filter((_, i) => i !== index)),
                    })
                  }
                >
                  <FaTrash />
                </button>
              </div>
            ))}
          </div>
        </Col>
      </Show>
    </Col>
  );
};
