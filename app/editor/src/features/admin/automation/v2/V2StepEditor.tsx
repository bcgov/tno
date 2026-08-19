import React from 'react';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { findOptionByValue, toNumberOrUndefined } from '../utils';
import { v2PhaseOptions, v2SourceOptions, v2StepSaveModeOptions } from './constants';
import { type IV2Step } from './interfaces';
import { V2FilterField } from './V2FilterField';
import { V2ScopedNameField } from './V2ScopedNameField';

export interface IV2StepEditorProps {
  step: IV2Step;
  collectionNames: string[];
  filterOptions: IOptionItem[];
  llmOptions: IOptionItem[];
  onChange: (step: IV2Step) => void;
}

/**
 * The step settings form (shown in the step modal): phase, content source with gate filters
 * (process steps), flush override, and LLM override. The step's analyses and actions are managed
 * from the steps grid, not here.
 */
export const V2StepEditor: React.FC<IV2StepEditorProps> = ({
  step,
  collectionNames,
  filterOptions,
  llmOptions,
  onChange,
}) => {
  const set = (values: Partial<IV2Step>) => onChange({ ...step, ...values });

  return (
    <Col className="v2-step-editor" gap="0.5rem">
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        <Text
          name="step-name"
          label="Step name"
          width="16rem"
          value={step.name}
          onChange={(e) => set({ name: e.target.value })}
        />
        <Select
          name="step-phase"
          label="Phase"
          width="18rem"
          isClearable={false}
          options={v2PhaseOptions}
          value={findOptionByValue(v2PhaseOptions, step.phase)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const phase = `${option?.value ?? 'process'}` as IV2Step['phase'];
            set({
              phase,
              // Once-phases run with no subject and cannot declare a source.
              source:
                phase === 'process'
                  ? step.source ?? { from: 'collection', include: [], exclude: [] }
                  : undefined,
            });
          }}
        />
        <Checkbox
          name="step-enabled"
          label="Enabled"
          checked={step.isEnabled}
          onChange={(e) => set({ isEnabled: e.target.checked })}
        />
      </Row>
      <Row gap="0.5rem" alignItems="flex-end" nowrap>
        {/* Init steps only gather: no content changes to save, no analyses for an LLM to run. */}
        <Show visible={step.phase === 'process'}>
          <Select
            name="step-save-mode"
            label="Save changes"
            width="16rem"
            isClearable={false}
            options={v2StepSaveModeOptions}
            value={findOptionByValue(v2StepSaveModeOptions, step.saveMode ?? '')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ saveMode: option?.value ? `${option.value}` : null });
            }}
          />
        </Show>
        <Show visible={step.phase !== 'init'}>
          <Select
            name="step-llm"
            label="LLM override"
            width="14rem"
            options={llmOptions}
            value={findOptionByValue(llmOptions, step.llmId) ?? ''}
            onChange={(newValue) => set({ llmId: toNumberOrUndefined(newValue as IOptionItem) })}
          />
        </Show>
      </Row>
      <TextArea
        name="step-description"
        label="Description"
        rows={2}
        value={step.description ?? ''}
        onChange={(e) => set({ description: e.target.value || undefined })}
      />

      <Show visible={step.phase === 'process'}>
        <Row gap="0.5rem" alignItems="flex-end" nowrap>
          <Select
            name="step-source-from"
            label="Content source"
            width="18rem"
            isClearable={false}
            options={v2SourceOptions}
            value={findOptionByValue(v2SourceOptions, step.source?.from ?? 'collection')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({
                source: {
                  ...(step.source ?? { include: [], exclude: [] }),
                  from: `${option?.value ?? 'collection'}` as NonNullable<
                    IV2Step['source']
                  >['from'],
                },
              });
            }}
          />
          <Show visible={step.source?.from === 'filter'}>
            <V2FilterField
              name="step-source-filter"
              value={step.source?.filter}
              options={filterOptions}
              onChange={(filterId) =>
                set({
                  source: {
                    ...(step.source ?? { from: 'filter' }),
                    from: 'filter',
                    filter: filterId,
                  },
                })
              }
            />
          </Show>
          <Show visible={step.source?.from === 'collection'}>
            <V2ScopedNameField
              name="step-source-collection"
              label="Collection"
              scope="$run"
              value={step.source?.collection}
              knownNames={collectionNames}
              onChange={(next) =>
                set({
                  source: {
                    ...(step.source ?? { from: 'collection' }),
                    from: 'collection',
                    collection: next,
                  },
                })
              }
            />
          </Show>
          <Text
            name="step-source-max"
            label="Max items"
            width="8rem"
            type="number"
            value={step.source?.max ?? ''}
            onChange={(e) =>
              set({
                source: {
                  ...(step.source ?? { from: 'collection' }),
                  max: e.target.value === '' ? null : Number(e.target.value),
                },
              })
            }
          />
        </Row>
        <Row gap="1rem" alignItems="flex-end" nowrap>
          <V2FilterField
            name="step-include"
            label="Only items matching filter (include)"
            value={step.source?.include?.[0]}
            options={filterOptions}
            onChange={(filterId) =>
              set({
                source: {
                  ...(step.source ?? { from: 'collection' }),
                  include: filterId ? [filterId] : [],
                },
              })
            }
          />
          <V2FilterField
            name="step-exclude"
            label="Skip items matching filter (exclude)"
            value={step.source?.exclude?.[0]}
            options={filterOptions}
            onChange={(filterId) =>
              set({
                source: {
                  ...(step.source ?? { from: 'collection' }),
                  exclude: filterId ? [filterId] : [],
                },
              })
            }
          />
        </Row>
      </Show>
    </Col>
  );
};
