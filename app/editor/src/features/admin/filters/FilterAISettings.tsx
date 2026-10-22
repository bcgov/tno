import React from 'react';
import { toast } from 'react-toastify';
import { useLLMs, useSettings } from 'store/hooks/admin';
import { type ILLMModel, type ISettingModel, OptionItem, Row, Select } from 'tno-core';

import { FilterAILlmSettingName } from './constants';

/**
 * Chooses the AI model that generates filters from prompts, in the editor and the subscriber app.
 * Agent-backed models are left out; filters need a deployment-based model.
 */
export const FilterAISettings: React.FC = () => {
  const [, { findAllLLMs }] = useLLMs();
  const [, { findAllSettings, addSetting, updateSetting }] = useSettings();

  const [llms, setLLMs] = React.useState<ILLMModel[]>([]);
  const [setting, setSetting] = React.useState<ISettingModel>();
  const [isSaving, setIsSaving] = React.useState(false);

  React.useEffect(() => {
    Promise.all([findAllLLMs(), findAllSettings()])
      .then(([models, settings]) => {
        setLLMs(models.filter((m) => m.isEnabled && !m.agentName));
        setSetting(settings.find((s) => s.name === FilterAILlmSettingName));
      })
      .catch(() => {});
    // Only load once.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const options = llms.map((m) => new OptionItem(m.name, m.id));
  const selectedId = setting?.value ? +setting.value : undefined;

  const save = async (llmId?: number) => {
    setIsSaving(true);
    try {
      const value = llmId ? `${llmId}` : '';
      const result = setting
        ? await updateSetting({ ...setting, value })
        : await addSetting({
            id: 0,
            name: FilterAILlmSettingName,
            description: 'The AI model that generates filters from prompts.',
            value,
            isEnabled: true,
            sortOrder: 0,
          });
      setSetting(result);
      toast.success(
        llmId
          ? `Filters are generated with ${llms.find((m) => m.id === llmId)?.name}.`
          : 'Filters can no longer be generated from prompts.',
      );
    } catch {
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Row className="filter-ai-settings" alignItems="center" gap="1rem">
      <Select
        name="filterAILlmId"
        label="AI model for filter prompts"
        tooltip="The model that generates filters from prompts and answers questions in the filter workbench, here and in the subscriber app's advanced search."
        width="40ch"
        options={options}
        value={options.find((o) => o.value === selectedId) ?? ''}
        isDisabled={isSaving}
        onChange={(o) => save((o as OptionItem)?.value as number | undefined)}
      />
      {!selectedId && <span>Choose a model to let users generate filters from a prompt.</span>}
    </Row>
  );
};
