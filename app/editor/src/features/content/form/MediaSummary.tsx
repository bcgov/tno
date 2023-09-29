import { Wysiwyg } from 'components/wysiwyg';
import React from 'react';

import * as styled from './styled';

export interface IMediaSummaryProps {
  setShowExpandModal?: (show: boolean) => void;
  isSummaryRequired: boolean;
  setCreateAfterPublish?: (state: boolean) => void;
}

/**
 * Provides a way to view/edit images/snippets and the summary.
 * @returns the MediaSummary
 */
export const MediaSummary: React.FC<IMediaSummaryProps> = ({
  setShowExpandModal,
  isSummaryRequired,
  setCreateAfterPublish,
}) => {
  return (
    <styled.MediaSummary>
      <Wysiwyg
        className="summary"
        label="Summary"
        required={isSummaryRequired}
        fieldName="summary"
        expandModal={setShowExpandModal}
      />
    </styled.MediaSummary>
  );
};
