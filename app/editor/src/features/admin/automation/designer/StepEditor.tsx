import React from 'react';
import { Checkbox, Col, type IOptionItem, Row, Select, Show, Text, TextArea } from 'tno-core';

import { createOption, findOptionByValue, toNumberOrUndefined } from '../utils';
import { fitSelectWidth, phaseOptions, stepSourceOptions } from './constants';
import { FilterField } from './FilterField';
import { type IAutomationStep } from './interfaces';
import { ScopedNameField } from './ScopedNameField';

export interface IStepEditorProps {
  step: IAutomationStep;
  collectionNames: string[];
  filterOptions: IOptionItem[];
  llmOptions: IOptionItem[];
  onChange: (step: IAutomationStep) => void;
}

/**
 * The step settings form (shown in the step modal), per the design:
 * - Name / Phase / Enabled;
 * - LLM Override (not for init - init only gathers);
 * - Description;
 * - Source and the include/exclude gate filters (process, and complete - a complete step that
 *   declares a source iterates it; leaving the source unset runs it once).
 * The step's analyses and actions are managed from the steps grid, not here.
 */
export const StepEditor: React.FC<IStepEditorProps> = ({
  step,
  collectionNames,
  filterOptions,
  llmOptions,
  onChange,
}) => {
  const set = (values: Partial<IAutomationStep>) => onChange({ ...step, ...values });
  const isInit = step.phase === 'init';
  // Complete steps only iterate when they declare a source; without one they run once.
  const hasSource = step.phase === 'process' || step.source != null;
  const sourceOptions =
    step.phase === 'complete'
      ? [createOption('(none — run once)', ''), ...stepSourceOptions]
      : stepSourceOptions;

  return (
    <Col className="automation-step-editor" gap="0.5rem">
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
          options={phaseOptions}
          value={findOptionByValue(phaseOptions, step.phase)}
          onChange={(newValue) => {
            const option = newValue as IOptionItem;
            const phase = `${option?.value ?? 'process'}` as IAutomationStep['phase'];
            // Phases have different semantics, so switching resets the step to the new phase's
            // defaults - only the name and enabled flag carry over.
            set({
              phase,
              description: undefined,
              llmId: undefined,
              source:
                phase === 'process' ? { from: 'collection', include: [], exclude: [] } : undefined,
              analyses: [],
              actions: [],
            });
          }}
        />
        <Text
          name="step-group"
          label="Group"
          width="10rem"
          placeholder="Optional"
          value={step.group ?? ''}
          onChange={(e) => set({ group: e.target.value || undefined })}
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
            required={step.phase === 'process'}
            width={fitSelectWidth(['(none — run once)', 'collection', 'filter'])}
            isClearable={false}
            options={sourceOptions}
            value={findOptionByValue(
              sourceOptions,
              step.source?.from ?? (step.phase === 'complete' ? '' : 'collection'),
            )}
            onChange={(newValue) => {
              const option = newValue as IOptionItem;
              if (!option?.value) {
                set({ source: undefined });
                return;
              }
              set({
                source: {
                  ...(step.source ?? { include: [], exclude: [] }),
                  from: `${option.value}` as NonNullable<IAutomationStep['source']>['from'],
                },
              });
            }}
          />
          <Show visible={hasSource && (step.source?.from ?? 'collection') === 'collection'}>
            <ScopedNameField
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
          <Show visible={hasSource && step.source?.from === 'filter'}>
            <FilterField
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
        <Show visible={hasSource}>
          <Row gap="1rem" alignItems="flex-end" nowrap>
            <FilterField
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
            <FilterField
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
      </Show>
    </Col>
  );
};
