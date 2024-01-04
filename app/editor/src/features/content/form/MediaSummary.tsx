import React from 'react';
import { useLookup } from 'store/hooks';
import { FormikContentWysiwyg } from 'tno-core';

import * as styled from './styled';

export interface IMediaSummaryProps {
  setShowExpandModal?: (show: boolean) => void;
  isSummaryRequired: boolean;
  getTags: Function;
}

/**
 * Provides a way to view/edit images/snippets and the summary.
 * @returns the MediaSummary
 */
export const MediaSummary: React.FC<IMediaSummaryProps> = ({
  setShowExpandModal,
  isSummaryRequired,
  getTags,
}) => {
  const [{ tags }] = useLookup();

  return (
    <styled.MediaSummary>
      <FormikContentWysiwyg
        className="quill-summary"
        label="Summary"
        required={isSummaryRequired}
        name="summary"
        expandModal={setShowExpandModal}
        tags={tags}
        onChange={(e) => getTags(e)}
      />
    </styled.MediaSummary>
  );
};
