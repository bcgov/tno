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

export const ContentActions = ({
  disabled,
  onCancel,
  onUpdate,
  onNavigate,
}: IContentActionsProps) => {
  const { isSubmitting } = useReportEditContext();

  return (
    <styled.ContentActions>
      <Button onClick={() => onCancel?.()} disabled={isSubmitting || disabled} variant="secondary">
        Cancel
      </Button>
      <Button onClick={() => onUpdate?.()} disabled={isSubmitting || disabled}>
        Save
        <FaSave />
      </Button>
      <Action
        icon={<FaArrowLeft size="20" />}
        title="Previous"
        disabled={isSubmitting || disabled}
        onClick={() => onNavigate?.('previous')}
      />
      <Action
        icon={<FaArrowRight />}
        title="Next"
        disabled={isSubmitting || disabled}
        onClick={() => onNavigate?.('next')}
      />
    </styled.ContentActions>
  );
};
