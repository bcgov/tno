import React from 'react';
import { useLookup } from 'store/hooks';
import { FormikContentWysiwyg } from 'tno-core';

import * as styled from './styled';

export interface IMediaSummaryProps {
  isSummaryRequired: boolean;
  getTags: Function;
}

/**
 * Provides a way to view/edit images/snippets and the summary.
 * @returns the MediaSummary
 */
export const MediaSummary: React.FC<IMediaSummaryProps> = ({ isSummaryRequired, getTags }) => {
  const [{ tags }] = useLookup();

  return (
    <styled.MediaSummary>
      <FormikContentWysiwyg
        className="quill-summary"
        label="Summary"
        required={isSummaryRequired}
        name="summary"
        tags={tags}
        onChange={(e) => getTags('summary', e)}
      />
    </styled.MediaSummary>
  );
};
