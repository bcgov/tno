import React from 'react';

import {
  IFilterAIGenerateRequestModel,
  IFilterAIGenerateResultModel,
  IFilterSettingsModel,
} from '../../hooks';
import { Modal } from '../modal';
import { FilterAIButton, FilterAIButtonComponent } from './FilterAIButton';
import { filterPromptNeedsGeneration, getFilterAIErrorMessage } from './utils';

type Action = 'search' | 'save';
interface IPendingAction {
  action: Action;
  prompt: string;
  settings: IFilterSettingsModel;
  proceed: (settings: IFilterSettingsModel) => void | Promise<void>;
}

/** Ask before searching or saving criteria that have not been generated from the current prompt. */
export const useFilterAIGenerationGuard = (
  prompt: string,
  settings: IFilterSettingsModel,
  onGenerate: (request: IFilterAIGenerateRequestModel) => Promise<IFilterAIGenerateResultModel>,
  onGenerated: (result: IFilterAIGenerateResultModel, prompt: string) => void,
  ButtonComponent: FilterAIButtonComponent = FilterAIButton,
) => {
  const [pending, setPending] = React.useState<IPendingAction>();
  const [busy, setBusy] = React.useState(false);
  const [error, setError] = React.useState<string>();
  const running = React.useRef(false);

  const confirmAction = (action: Action, proceed: IPendingAction['proceed']) => {
    if (pending || running.current) return;
    if (filterPromptNeedsGeneration(prompt, settings.generatedPrompt)) {
      setError(undefined);
      setPending({ action, prompt, settings, proceed });
    } else void proceed(settings);
  };

  const generateAndProceed = async () => {
    if (!pending || running.current) return;
    running.current = true;
    setBusy(true);
    setError(undefined);
    try {
      const generated = await onGenerate({ prompt: pending.prompt, settings: pending.settings });
      const result = {
        ...generated,
        settings: { ...generated.settings, generatedPrompt: pending.prompt },
      };
      onGenerated(result, pending.prompt);
      // Pass the new settings directly; React's state update has not necessarily rendered yet.
      await pending.proceed(result.settings);
      setPending(undefined);
    } catch (ex) {
      setError(getFilterAIErrorMessage(ex));
    } finally {
      running.current = false;
      setBusy(false);
    }
  };

  const confirmation = (
    <Modal
      isShowing={!!pending}
      type="custom"
      headerText={`Generate the filter before ${
        pending?.action === 'save' ? 'saving' : 'searching'
      }?`}
      component={
        <div>
          <p>
            The prompt is new or has changed since the filter was generated. Generate it first to
            use the current prompt's criteria.
          </p>
          {error && <p role="alert">{error}</p>}
          {busy && <p role="status">Generating the filter…</p>}
        </div>
      }
      customButtons={
        <>
          <ButtonComponent variant="primary" disabled={busy} onClick={generateAndProceed}>
            Generate and {pending?.action}
          </ButtonComponent>
          <ButtonComponent
            variant="secondary"
            disabled={busy}
            onClick={() => {
              if (!pending || running.current) return;
              setPending(undefined);
              void pending.proceed(pending.settings);
            }}
          >
            {pending?.action === 'save' ? 'Save' : 'Search'} without generating
          </ButtonComponent>
          <ButtonComponent
            variant="secondary"
            disabled={busy}
            onClick={() => setPending(undefined)}
          >
            Cancel
          </ButtonComponent>
        </>
      }
    />
  );
  return { confirmAction, confirmation };
};
