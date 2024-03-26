import { Action } from 'components/action';
import { Button } from 'components/button';
import { FaArrowLeft, FaArrowRight, FaSave } from 'react-icons/fa';

import { useReportEditContext } from '../../ReportEditContext';
import * as styled from './styled';

export interface IContentActionsProps {
  /** Whether edit functionality is disabled. */
  disabled?: boolean;
  /** Cancel button event. */
  onCancel?: () => void;
  /** Event fires when the update button is clicked and performs an update to the API. */
  onUpdate?: () => void;
  /** Event fires when user clicks previous/next buttons */
  onNavigate?: (action: 'previous' | 'next') => void;
}

/**
 * Provides component with action buttons for the content form.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ContentActions = ({
  disabled,
  onCancel,
  onUpdate,
  onNavigate,
}: IContentActionsProps) => {
  const { isSubmitting } = useReportEditContext();

  return (
    <styled.ContentActions>
      <Button onClick={() => onCancel?.()} disabled={isSubmitting} variant="secondary">
        Cancel
      </Button>
      <Button onClick={() => onUpdate?.()} disabled={isSubmitting || disabled}>
        Save story
        <FaSave />
      </Button>
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
    </styled.ContentActions>
  );
};
