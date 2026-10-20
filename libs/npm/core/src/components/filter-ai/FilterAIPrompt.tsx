import React from 'react';
import { FaToolbox, FaWandMagicSparkles } from 'react-icons/fa6';

import {
  IFilterAIChatRequestModel,
  IFilterAIChatResultModel,
  IFilterAIGenerateRequestModel,
  IFilterAIGenerateResultModel,
  IFilterSettingsModel,
} from '../../hooks';
import { Spinner } from '../spinners';
import { FilterAIButton, FilterAIButtonComponent } from './FilterAIButton';
import { FilterAIPromptEditor, FilterAIPromptEditorType } from './FilterAIPromptEditor';
import { FilterAIWorkbench } from './FilterAIWorkbench';
import * as styled from './styled';
import { getFilterAIErrorMessage, htmlToPromptText } from './utils';

export interface IFilterAIPromptProps {
  /** The prompt (HTML, or plain text with the 'textarea' editor). */
  prompt?: string;
  /** The editor the prompt is written in (default: wysiwyg). */
  editor?: FilterAIPromptEditorType;
  /** The filter's current settings. */
  settings: IFilterSettingsModel;
  /** The label above the prompt editor. */
  label?: string;
  /** The editor's placeholder text. */
  placeholder?: string;
  /** The prompt changed. */
  onPromptChange: (prompt: string) => void;
  /** Generate filter settings from a prompt. */
  onGenerate: (request: IFilterAIGenerateRequestModel) => Promise<IFilterAIGenerateResultModel>;
  /** Ask how to write the prompt. */
  onChat: (request: IFilterAIChatRequestModel) => Promise<IFilterAIChatResultModel>;
  /** Apply the generated filter and the prompt it came from. */
  onApply: (result: IFilterAIGenerateResultModel, prompt: string) => void;
  /** The button to render, so the app's own button styling is used (default: tno-core Button). */
  buttonComponent?: FilterAIButtonComponent;
}

/**
 * A prompt that generates a filter: describe the stories, generate the settings, or open the
 * workbench to iterate on the prompt with the AI.
 */
export const FilterAIPrompt: React.FC<IFilterAIPromptProps> = ({
  prompt,
  editor = 'wysiwyg',
  settings,
  label = 'Describe the filter',
  placeholder = 'i.e. CBC and Global radio stories about wildfires in the last 7 days',
  onPromptChange,
  onGenerate,
  onChat,
  onApply,
  buttonComponent: ButtonComponent = FilterAIButton,
}) => {
  const [isGenerating, setIsGenerating] = React.useState(false);
  const [error, setError] = React.useState<string>();
  const [explanation, setExplanation] = React.useState<string>();
  const [showWorkbench, setShowWorkbench] = React.useState(false);

  const apply = (result: IFilterAIGenerateResultModel, text: string) => {
    setExplanation(result.explanation);
    setError(undefined);
    onApply(result, text);
  };

  const generate = async () => {
    if (isGenerating || !prompt) return;
    setIsGenerating(true);
    setError(undefined);
    try {
      const result = await onGenerate({ prompt, settings });
      apply(result, prompt);
    } catch (ex) {
      setError(getFilterAIErrorMessage(ex));
    } finally {
      setIsGenerating(false);
    }
  };

  return (
    <styled.FilterAIPrompt className="filter-ai-prompt">
      <FilterAIPromptEditor
        name="filterAIPrompt"
        editor={editor}
        label={label}
        placeholder={placeholder}
        value={editor === 'textarea' ? htmlToPromptText(prompt) : prompt ?? ''}
        onChange={onPromptChange}
      />
      <div className="filter-ai-actions">
        <ButtonComponent
          variant="primary"
          onClick={generate}
          disabled={isGenerating || !prompt}
          title="Replace the filter settings with the filter this prompt describes"
        >
          <FaWandMagicSparkles /> Generate filter
        </ButtonComponent>
        <ButtonComponent
          variant="secondary"
          onClick={() => setShowWorkbench(true)}
          title="Iterate on the prompt and ask the AI how to describe the filter"
        >
          <FaToolbox /> Workbench
        </ButtonComponent>
        {isGenerating && (
          <span className="filter-ai-status" role="status">
            <Spinner size="1rem" /> Generating the filter…
          </span>
        )}
      </div>
      {error && (
        <div className="filter-ai-error" role="alert">
          {error}
        </div>
      )}
      {explanation && <div className="filter-ai-explanation">{explanation}</div>}
      <FilterAIWorkbench
        open={showWorkbench}
        prompt={prompt ?? ''}
        editor={editor}
        settings={settings}
        explanation={explanation}
        onGenerate={onGenerate}
        onChat={onChat}
        onApply={apply}
        onClose={() => setShowWorkbench(false)}
        buttonComponent={ButtonComponent}
      />
    </styled.FilterAIPrompt>
  );
};
