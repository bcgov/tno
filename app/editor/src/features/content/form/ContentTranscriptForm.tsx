import { Wysiwyg } from 'components/form';
import { Tags } from 'components/form/tags';

import * as styled from './styled';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC = () => {
  return (
    <styled.ContentTranscriptForm>
      <Wysiwyg fieldName="body" />
      <Tags fieldName="body" />
    </styled.ContentTranscriptForm>
  );
};
