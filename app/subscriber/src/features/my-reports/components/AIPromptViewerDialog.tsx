import React from 'react';
import { IReportAISectionPromptModel } from 'store/hooks/subscriber/interfaces';

import { AIPromptDialog } from './AIPromptDialog';
import { AIPromptTranscript } from './AIPromptTranscript';

export interface IAIPromptViewerDialogProps {
  open: boolean;
  title?: string;
  onClose: () => void;
  /** The described prompt; undefined while loading. */
  prompt?: IReportAISectionPromptModel;
  isLoading?: boolean;
  /** Why the prompt could not be fetched. */
  error?: string;
  /** Controls after the heading, such as a section picker. */
  header?: React.ReactNode;
}

/**
 * Shows the planned prompt of an AI section in a dialog.
 */
export const AIPromptViewerDialog: React.FC<IAIPromptViewerDialogProps> = ({
  open,
  title = 'AI section prompt',
  onClose,
  prompt,
  isLoading,
  error,
  header,
}) => (
  <AIPromptDialog
    open={open}
    title={prompt?.sectionLabel ? `${title}: ${prompt.sectionLabel}` : title}
    onClose={onClose}
    header={header}
  >
    {prompt?.llmName && (
      <p>
        Model: <strong>{prompt.llmName}</strong>
      </p>
    )}
    <AIPromptTranscript
      exchanges={prompt?.exchanges ?? []}
      isPlanned={prompt?.isPlanned ?? true}
      error={error ?? prompt?.error}
      isLoading={isLoading}
      fileName={`${(prompt?.sectionLabel || 'ai-section').replace(/[^\w-]+/g, '-')}-prompt.txt`}
    />
  </AIPromptDialog>
);
