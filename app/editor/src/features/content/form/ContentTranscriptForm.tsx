import React from 'react';
import { useLookup } from 'store/hooks';
import { FormikContentWysiwyg } from 'tno-core';

import { useExtractTags } from './hooks';
import * as styled from './styled';

export interface IContentTranscriptFormProps {
  setParsedTags?: React.Dispatch<React.SetStateAction<string[]>>;
}

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC<IContentTranscriptFormProps> = ({ setParsedTags }) => {
  const [{ tags }] = useLookup();
  const getTags = useExtractTags({ setParsedTags });

  return (
    <styled.ContentTranscriptForm>
      <FormikContentWysiwyg
        className="content-body"
        name="body"
        tags={tags}
        onChange={(text) => getTags('body', text)}
      />
    </styled.ContentTranscriptForm>
  );
};
