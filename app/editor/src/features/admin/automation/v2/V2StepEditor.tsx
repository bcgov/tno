import React from 'react';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import {
  fitSelectWidth,
  v2PhaseOptions,
  v2SourceOptions,
  v2StepSaveModeOptions,
} from './constants';
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
 * The step settings form (shown in the step modal), per the design:
 * - Name / Phase / Enabled;
 * - Save Mode / LLM Override (not for init - init only gathers);
 * - Description;
 * - Source and the include/exclude gate filters (process, and complete - a complete step that
 *   declares a source iterates it; leaving the source unset runs it once).
 * The step's analyses and actions are managed from the steps grid, not here.
 */
export const V2StepEditor: React.FC<IV2StepEditorProps> = ({
  step,
  collectionNames,
  filterOptions,
  llmOptions,
  onChange,
}) => {
  const set = (values: Partial<IV2Step>) => onChange({ ...step, ...values });
  const isInit = step.phase === 'init';

  return (
    <Col className="v2-step-editor" gap="0.5rem">
      <Row gap="1rem" alignItems="flex-end" nowrap>
        <Text
          name="step-name"
          label="Name"
          required
          width="14rem"
          value={step.name}
          onChange={(e) => set({ name: e.target.value })}
        />
        <Select
          name="step-phase"
          label="Phase"
          required
          width={fitSelectWidth(['init', 'process', 'complete'])}
          isClearable={false}
          options={v2PhaseOptions}
          value={findOptionByValue(v2PhaseOptions, step.phase)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const phase = `${option?.value ?? 'process'}` as IV2Step['phase'];
            set({
              phase,
              // Init steps only gather; they never declare a source.
              source: phase === 'init' ? undefined : step.source,
            });
          }}
        />
        <div className="checkbox-inline">
          <Checkbox
            name="step-enabled"
            label="Enabled"
            checked={step.isEnabled}
            onChange={(e) => set({ isEnabled: e.target.checked })}
          />
        </div>
      </Row>
      <Show visible={!isInit}>
        <Row gap="1rem" alignItems="flex-end" nowrap>
          <Select
            name="step-save-mode"
            label="Save Mode"
            width={fitSelectWidth(['profile default', 'end-of-run', 'end-of-step'])}
            isClearable={false}
            options={v2StepSaveModeOptions}
            value={findOptionByValue(v2StepSaveModeOptions, step.saveMode ?? '')}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              set({ saveMode: option?.value ? `${option.value}` : null });
            }}
          />
          <Select
            name="step-llm"
            label="LLM Override"
            width={fitSelectWidth([
              'profile default',
              ...llmOptions.map((option) => `${option.label}`),
            ])}
            isClearable={false}
            options={[createOption('profile default', ''), ...llmOptions]}
            value={findOptionByValue(llmOptions, step.llmId) ?? createOption('profile default', '')}
            onChange={(newValue) => set({ llmId: toNumberOrUndefined(newValue as IOptionItem) })}
          />
        </Row>
      </Show>
      <TextArea
        name="step-description"
        label="Description"
        rows={2}
        placeholder="What this step does"
        value={step.description ?? ''}
        onChange={(e) => set({ description: e.target.value || undefined })}
      />
      <Show visible={!isInit}>
        <Row gap="1rem" alignItems="flex-end" nowrap>
          <Select
            name="step-source-from"
            label="Source"
            required
            width={fitSelectWidth(['collection', 'filter'])}
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
          <Show visible={(step.source?.from ?? 'collection') === 'collection'}>
            <V2ScopedNameField
              name="step-source-collection"
              label="Collection"
              scope="$run"
              placeholder="Pick a collection"
              allowNew={false}
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
          <Show visible={step.source?.from === 'filter'}>
            <V2FilterField
              name="step-source-filter"
              label="Filter"
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
        </Row>
        <Row gap="1rem" alignItems="flex-end" nowrap>
          <V2FilterField
            name="step-include"
            label="Filter (include)"
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
            label="Filter (exclude)"
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
