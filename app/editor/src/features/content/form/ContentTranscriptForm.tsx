import { Wysiwyg } from 'components/form';
import { useFormikContext } from 'formik';
import { useCombinedView } from 'hooks';
import { FieldSize, FormikText } from 'tno-core';

import { IContentForm } from './interfaces';
import * as styled from './styled';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC = () => {
  const { values } = useFormikContext<IContentForm>();
  const combined = useCombinedView();

  return (
    <styled.ContentTranscriptForm>
      <Wysiwyg fieldName="body" />
      <FormikText
        name="tags"
        label="Tags"
        disabled
        width={combined ? FieldSize.Big : FieldSize.Large}
        value={values.tags.map((t) => t.id).join(', ')}
      />
    </styled.ContentTranscriptForm>
  );
};
