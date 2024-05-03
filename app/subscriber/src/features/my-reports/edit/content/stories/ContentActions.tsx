import { Action } from 'components/action';
import { Button } from 'components/button';
import React from 'react';
import { FaArrowLeft, FaArrowRight, FaSave } from 'react-icons/fa';
import { FaArrowUpRightFromSquare } from 'react-icons/fa6';
import { useApp } from 'store/hooks';
import { IContentModel, ReportStatusName, Show } from 'tno-core';

import { useReportEditContext } from '../../ReportEditContext';
import * as styled from './styled';

export interface IContentActionsProps {
  /** The content being edited */
  content?: IContentModel;
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** Cancel button event. */
  onCancel?: () => void;
  /** Event fires when the update button is clicked and performs an update to the API. */
  onUpdate?: () => void;
  /** Event fires when user clicks previous/next buttons */
  onNavigate?: (action: 'previous' | 'next') => void;
  /** Event fires when content properties are changed. */
  onContentChange?: (content: IContentModel) => void;
}

/**
 * Provides component with action buttons for the content form.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentActions = ({
  content,
  disabled,
  onCancel,
  onUpdate,
  onNavigate,
  onContentChange,
}: IContentActionsProps) => {
  const [{ userInfo }] = useApp();
  const { values, isSubmitting } = useReportEditContext();

  const userId = userInfo?.id ?? 0;
  const instance = values.instances.length ? values.instances[0] : undefined;

  const handleUndo = React.useCallback(
    (content?: IContentModel) => {
      if (!content) return;

      onContentChange?.({
        ...content,
        versions: {
          ...content.versions,
          [userId]: {
            ...content.versions[userId],
            headline: content.headline,
            body: content.body,
          },
        },
      });
    },
    [onContentChange, userId],
  );

  return (
    <styled.ContentActions>
      <Button onClick={() => onCancel?.()} disabled={isSubmitting} variant="secondary">
        Cancel
      </Button>
      <Show visible={!!content}>
        <Button
          onClick={() => {
            window.open(`/view/${content!.id}`, '_blank');
          }}
          variant="secondary"
        >
          View
          <FaArrowUpRightFromSquare />
        </Button>
        <Button
          onClick={() => onUpdate?.()}
          disabled={isSubmitting || disabled || instance?.status === ReportStatusName.Submitted}
        >
          Save story
          <FaSave />
        </Button>
        <Show visible={!content?.isPrivate}>
          <Action variant="undo" title="Revert" onClick={() => handleUndo(content)} />
        </Show>
        <Action
          icon={<FaArrowLeft size="20" />}
          title="Previous"
          disabled={isSubmitting}
          onClick={() => onNavigate?.('previous')}
        />
        <Action
          icon={<FaArrowRight />}
          title="Next"
          disabled={isSubmitting}
          onClick={() => onNavigate?.('next')}
        />
      </Show>
    </styled.ContentActions>
  );
};
