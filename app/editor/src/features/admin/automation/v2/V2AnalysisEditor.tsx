import React from 'react';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import { type IV2Analysis } from './interfaces';

export interface IV2AnalysisEditorProps {
  analysis: IV2Analysis;
  /** Names of analyses declared earlier in the step (valid chain targets). */
  earlierNames: string[];
  promptNames: string[];
  llmOptions: IOptionItem[];
  onChange: (analysis: IV2Analysis) => void;
}

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
  const returnEntries = Object.entries(analysis.returns ?? {});

  const set = (values: Partial<IV2Analysis>) => onChange({ ...analysis, ...values });

  return (
    <Col className="v2-analysis-editor" gap="0.5rem">
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Text
          name="analysis-name"
          label="Analysis name"
          width="14rem"
          value={analysis.name}
          onChange={(e) => set({ name: e.target.value })}
        />
        <Select
          name="analysis-prompt-ref"
          label="Prompt (library entry)"
          width="18rem"
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
          label="Continue (chain)"
          width="14rem"
          isClearable={false}
          options={chainOptions}
          value={findOptionByValue(chainOptions, analysis.chain ?? '')}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            set({ chain: option?.value ? `${option.value}` : null });
          }}
        />
        <Select
          name="analysis-llm"
          label="LLM override"
          width="14rem"
          options={llmOptions}
          value={findOptionByValue(llmOptions, analysis.llmId) ?? ''}
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
          label="Prompt text"
          rows={4}
          value={analysis.prompt?.text ?? ''}
          onChange={(e) => set({ prompt: { ...analysis.prompt, text: e.target.value } })}
        />
      </Show>
      <Show visible={!!analysis.prompt?.ref}>
        <TextArea
          name="analysis-prompt-override"
          label="Override (layered onto the library entry)"
          rows={3}
          value={analysis.prompt?.override ?? ''}
          onChange={(e) =>
            set({ prompt: { ...analysis.prompt, override: e.target.value || null } })
          }
        />
      </Show>
      <Show visible={!analysis.raw}>
        <Col gap="0.25rem">
          <label>
            Returns (key and type — 'string', 'string?', 'string[]', 'bool', 'int', 'int(-5..5)')
          </label>
          {returnEntries.map(([key, type], index) => (
            <Row key={index} gap="0.5rem" alignItems="center" nowrap>
              <Text
                name={`analysis-return-key-${index}`}
                placeholder="key"
                width="12rem"
                value={key}
                onChange={(e) => {
                  const updated = returnEntries.map(([k, t], i) =>
                    i === index ? ([e.target.value, t] as const) : ([k, t] as const),
                  );
                  set({ returns: Object.fromEntries(updated) });
                }}
              />
              <Text
                name={`analysis-return-type-${index}`}
                placeholder="type"
                width="10rem"
                value={type}
                onChange={(e) => {
                  const updated = returnEntries.map(([k, t], i) =>
                    i === index ? ([k, e.target.value] as const) : ([k, t] as const),
                  );
                  set({ returns: Object.fromEntries(updated) });
                }}
              />
              <button
                type="button"
                className="rule-icon-button delete"
                title="Remove key"
                onClick={() =>
                  set({
                    returns: Object.fromEntries(returnEntries.filter((_, i) => i !== index)),
                  })
                }
              >
                ×
              </button>
            </Row>
          ))}
          <Row>
            <button
              type="button"
              className="v2-link-button"
              onClick={() => set({ returns: { ...(analysis.returns ?? {}), '': 'string' } })}
            >
              + return key
            </button>
          </Row>
        </Col>
      </Show>
    </Col>
  );
};
