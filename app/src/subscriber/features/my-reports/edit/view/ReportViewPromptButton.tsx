import { Action } from 'components/action';
import React from 'react';
import { FaCode } from 'react-icons/fa6';
import { useReportAI } from 'store/hooks';
import { IReportAISectionPromptModel } from 'store/hooks/subscriber/interfaces';
import { ReportSectionTypeName } from 'tno-core';

import { AIPromptViewerDialog } from '../../components';
import { useReportEditContext } from '../ReportEditContext';

export interface IReportViewPromptButtonProps {
  /** The instance whose content the prompts are built from. */
  instanceId?: number;
}

/**
 * Opens a dialog showing the prompt each AI section sends to its model for the previewed instance,
 * built from the saved section settings without calling the model.
 */
export const ReportViewPromptButton: React.FC<IReportViewPromptButtonProps> = ({ instanceId }) => {
  const { values } = useReportEditContext();
  const { describeInstanceSection } = useReportAI();
  const [open, setOpen] = React.useState(false);
  const [sectionId, setSectionId] = React.useState<number>();
  const [prompt, setPrompt] = React.useState<IReportAISectionPromptModel>();
  const [isLoading, setIsLoading] = React.useState(false);
  const [error, setError] = React.useState<string>();

  const aiSections = React.useMemo(
    () =>
      (values.sections ?? []).filter(
        (s) => s.sectionType === ReportSectionTypeName.AI && s.isEnabled && !!s.id,
      ),
    [values.sections],
  );

  const load = React.useCallback(
    async (id: number) => {
      if (!instanceId) return;
      setIsLoading(true);
      setError(undefined);
      setPrompt(undefined);
      try {
        setPrompt(await describeInstanceSection(instanceId, id));
      } catch (ex: any) {
        setError(ex?.response?.data?.error ?? 'The prompt could not be fetched.');
      } finally {
        setIsLoading(false);
      }
    },
    [describeInstanceSection, instanceId],
  );

  if (!aiSections.length || !instanceId) return null;

  const show = () => {
    const id =
      sectionId && aiSections.some((s) => s.id === sectionId) ? sectionId : aiSections[0].id;
    setSectionId(id);
    setOpen(true);
    load(id);
  };

  return (
    <>
      <Action
        icon={<FaCode />}
        label="AI prompts"
        title="View the prompt each AI section sends to its model"
        aria-label="View AI prompts"
        role="button"
        onClick={show}
      />
      <AIPromptViewerDialog
        open={open}
        title="AI section prompt"
        onClose={() => setOpen(false)}
        prompt={prompt}
        isLoading={isLoading}
        error={error}
        header={
          aiSections.length > 1 ? (
            <label>
              Section{' '}
              <select
                aria-label="AI section"
                value={sectionId ?? ''}
                onChange={(e) => {
                  const id = +e.target.value;
                  setSectionId(id);
                  load(id);
                }}
              >
                {aiSections.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.settings.label || s.name}
                  </option>
                ))}
              </select>
            </label>
          ) : undefined
        }
      />
    </>
  );
};
